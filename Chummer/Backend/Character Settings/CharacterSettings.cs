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
using System.Threading;
using System.Threading.Tasks;
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

    public sealed class CharacterSettings : INotifyMultiplePropertiesChangedAsync, IHasName, IHasLockObject
    {
        private Guid _guiSourceId = Guid.Empty;
        private string _strFileName = string.Empty;
        private string _strName = "Standard";
        private bool _blnDoingCopy;

        // Settings.

        private bool _blnAllowBiowareSuites;
        private bool _blnAllowCyberwareESSDiscounts;
        private bool _blnAllowEditPartOfBaseWeapon;
        private bool _blnAllowHigherStackedFoci;
        private bool _blnAllowInitiationInCreateMode;
        private bool _blnDontUseCyberlimbCalculation;
        private bool _blnAllowSkillRegrouping = true;
        private bool _blnSpecializationsBreakSkillGroups = true;
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
        private bool _blnExceedNegativeQualitiesNoBonus;
        private bool _blnExceedPositiveQualities;
        private bool _blnExceedPositiveQualitiesCostDoubled;
        private bool _blnExtendAnyDetectionSpell;
        private bool _blnDroneArmorMultiplierEnabled;
        private bool _blnFreeSpiritPowerPointsMAG;
        private bool _blnNoArmorEncumbrance;
        private bool _blnUncappedArmorAccessoryBonuses;
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
        private string _strRegisteredSpriteExpression = "{CHA}";
        private string _strEssenceModifierPostExpression = "{Modifier}";
        private string _strLiftLimitExpression = "{STR} * 15";
        private string _strCarryLimitExpression = "{STR} * 10";
        private string _strEncumbranceIntervalExpression = "15";
        private bool _blnDoEncumbrancePenaltyPhysicalLimit = true;
        private bool _blnDoEncumbrancePenaltyMovementSpeed;
        private bool _blnDoEncumbrancePenaltyAgility;
        private bool _blnDoEncumbrancePenaltyReaction;
        private bool _blnDoEncumbrancePenaltyWoundModifier;
        private int _intEncumbrancePenaltyPhysicalLimit = 1;
        private int _intEncumbrancePenaltyMovementSpeed = 1;
        private int _intEncumbrancePenaltyAgility = 1;
        private int _intEncumbrancePenaltyReaction = 1;
        private int _intEncumbrancePenaltyWoundModifier = 1;
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
        private string _strWeightFormat = "#,0.###";
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
        private int _intMaxNumberMaxAttributesCreate = 1;
        private int _intMaxSkillRatingCreate = 6;
        private int _intMaxKnowledgeSkillRatingCreate = 6;
        private int _intMaxSkillRating = 12;
        private int _intMaxKnowledgeSkillRating = 12;
        private HashSet<string> _setBannedWareGrades = Utils.StringHashSetPool.Get();
        private HashSet<string> _setRedlinerExcludes = Utils.StringHashSetPool.Get();

        // Initiative variables
        private int _intMinInitiativeDice = 1;

        private int _intMaxInitiativeDice = 5;
        private int _intMinAstralInitiativeDice = 3;
        private int _intMaxAstralInitiativeDice = 5;
        private int _intMinColdSimInitiativeDice = 3;
        private int _intMaxColdSimInitiativeDice = 5;
        private int _intMinHotSimInitiativeDice = 4;
        private int _intMaxHotSimInitiativeDice = 5;

        // Karma variables.
        private int _intKarmaAttribute = 5;
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

        private decimal _decKarmaMAGInitiationGroupPercent = 0.1m;
        private decimal _decKarmaRESInitiationGroupPercent = 0.1m;
        private decimal _decKarmaMAGInitiationOrdealPercent = 0.1m;
        private decimal _decKarmaRESInitiationOrdealPercent = 0.2m;
        private decimal _decKarmaMAGInitiationSchoolingPercent = 0.1m;
        private decimal _decKarmaRESInitiationSchoolingPercent = 0.1m;

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
        private int _intMaxMartialArts = 1;
        private int _intMaxMartialTechniques = 5;
        private decimal _decNuyenCarryover = 5000;
        private int _intKarmaCarryover = 7;

        // Dictionary of id (or names) of custom data directories, ordered by load order with the second value element being whether it's enabled
        private readonly LockingTypedOrderedDictionary<string, bool> _dicCustomDataDirectoryKeys;

        // Cached lists that should be updated every time _dicCustomDataDirectoryKeys is updated
        private readonly OrderedSet<CustomDataDirectoryInfo> _setEnabledCustomDataDirectories = new OrderedSet<CustomDataDirectoryInfo>();

        private readonly HashSet<Guid> _setEnabledCustomDataDirectoryGuids = new HashSet<Guid>();

        private readonly List<string> _lstEnabledCustomDataDirectoryPaths = new List<string>();

        // Sourcebook list.
        private HashSet<string> _setBooks = Utils.StringHashSetPool.Get();

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentHashSet<PropertyChangedAsyncEventHandler> _setPropertyChangedAsync =
            new ConcurrentHashSet<PropertyChangedAsyncEventHandler>();

        public event PropertyChangedAsyncEventHandler PropertyChangedAsync
        {
            add => _setPropertyChangedAsync.TryAdd(value);
            remove => _setPropertyChangedAsync.Remove(value);
        }

        public event MultiplePropertiesChangedEventHandler MultiplePropertiesChanged;

        private readonly ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler> _setMultiplePropertiesChangedAsync =
            new ConcurrentHashSet<MultiplePropertiesChangedAsyncEventHandler>();

        public event MultiplePropertiesChangedAsyncEventHandler MultiplePropertiesChangedAsync
        {
            add => _setMultiplePropertiesChangedAsync.TryAdd(value);
            remove => _setMultiplePropertiesChangedAsync.Remove(value);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            this.OnMultiplePropertyChanged(strPropertyName);
        }

        public Task OnPropertyChangedAsync(string strPropertyName, CancellationToken token = default)
        {
            return this.OnMultiplePropertyChangedAsync(token, strPropertyName);
        }

        public void OnMultiplePropertiesChanged(IReadOnlyCollection<string> lstPropertyNames)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (_blnDoingCopy)
                    return;
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = s_CharacterSettingsDependencyGraph.GetWithAllDependents(this, strPropertyName, true);
                        else
                        {
                            foreach (string strLoopChangedProperty in s_CharacterSettingsDependencyGraph
                                         .GetWithAllDependentsEnumerable(this, strPropertyName))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    using (LockObject.EnterWriteLock())
                    {
                        if (setNamesOfChangedProperties.Contains(nameof(MaxNuyenDecimals)))
                            _intCachedMaxNuyenDecimals = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(MinNuyenDecimals)))
                            _intCachedMinNuyenDecimals = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(EssenceDecimals)))
                            _intCachedEssenceDecimals = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(WeightDecimals)))
                            _intCachedWeightDecimals = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(Books)))
                            RecalculateBookXPath();
                        if (setNamesOfChangedProperties.Contains(nameof(CustomDataDirectoryKeys)))
                            RecalculateEnabledCustomDataDirectories();
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Func<Task>> lstFuncs = new List<Func<Task>>(_setMultiplePropertiesChangedAsync.Count);
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstFuncs.Add(() => objEvent.Invoke(this, objArgs));
                        }

                        Utils.RunWithoutThreadLock(lstFuncs);
                        if (MultiplePropertiesChanged != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                MultiplePropertiesChanged?.Invoke(this, objArgs);
                            });
                        }
                    }
                    else if (MultiplePropertiesChanged != null)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        Utils.RunOnMainThread(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        });
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties.Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Func<Task>> lstFuncs = new List<Func<Task>>(lstArgsList.Count * _setPropertyChangedAsync.Count);
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                                lstFuncs.Add(() => objEvent.Invoke(this, objArg));
                        }

                        Utils.RunWithoutThreadLock(lstFuncs);
                        if (PropertyChanged != null)
                        {
                            Utils.RunOnMainThread(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                    {
                                        PropertyChanged.Invoke(this, objArgs);
                                    }
                                }
                            });
                        }
                    }
                    else if (PropertyChanged != null)
                    {
                        Utils.RunOnMainThread(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        });
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
        }

        public async Task OnMultiplePropertiesChangedAsync(IReadOnlyCollection<string> lstPropertyNames,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoingCopy)
                    return;
                HashSet<string> setNamesOfChangedProperties = null;
                try
                {
                    foreach (string strPropertyName in lstPropertyNames)
                    {
                        if (setNamesOfChangedProperties == null)
                            setNamesOfChangedProperties
                                = await s_CharacterSettingsDependencyGraph.GetWithAllDependentsAsync(this, strPropertyName, true, token).ConfigureAwait(false);
                        else
                        {
                            foreach (string strLoopChangedProperty in await s_CharacterSettingsDependencyGraph
                                         .GetWithAllDependentsEnumerableAsync(this, strPropertyName, token).ConfigureAwait(false))
                                setNamesOfChangedProperties.Add(strLoopChangedProperty);
                        }
                    }

                    if (setNamesOfChangedProperties == null || setNamesOfChangedProperties.Count == 0)
                        return;

                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (setNamesOfChangedProperties.Contains(nameof(MaxNuyenDecimals)))
                            _intCachedMaxNuyenDecimals = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(MinNuyenDecimals)))
                            _intCachedMinNuyenDecimals = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(EssenceDecimals)))
                            _intCachedEssenceDecimals = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(WeightDecimals)))
                            _intCachedWeightDecimals = int.MinValue;
                        if (setNamesOfChangedProperties.Contains(nameof(Books)))
                            await RecalculateBookXPathAsync(token).ConfigureAwait(false);
                        if (setNamesOfChangedProperties.Contains(nameof(CustomDataDirectoryKeys)))
                            await RecalculateEnabledCustomDataDirectoriesAsync(token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    if (_setMultiplePropertiesChangedAsync.Count > 0)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (MultiplePropertiesChangedAsyncEventHandler objEvent in _setMultiplePropertiesChangedAsync)
                        {
                            lstTasks.Add(objEvent.Invoke(this, objArgs, token));
                            if (++i < Utils.MaxParallelBatchSize)
                                continue;
                            await Task.WhenAll(lstTasks).ConfigureAwait(false);
                            lstTasks.Clear();
                            i = 0;
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);
                        if (MultiplePropertiesChanged != null)
                        {
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                MultiplePropertiesChanged?.Invoke(this, objArgs);
                            }, token: token).ConfigureAwait(false);
                        }
                    }
                    else if (MultiplePropertiesChanged != null)
                    {
                        MultiplePropertiesChangedEventArgs objArgs =
                            new MultiplePropertiesChangedEventArgs(setNamesOfChangedProperties.ToArray());
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure
                            MultiplePropertiesChanged?.Invoke(this, objArgs);
                        }, token: token).ConfigureAwait(false);
                    }

                    if (_setPropertyChangedAsync.Count > 0)
                    {
                        List<PropertyChangedEventArgs> lstArgsList = setNamesOfChangedProperties
                            .Select(x => new PropertyChangedEventArgs(x)).ToList();
                        List<Task> lstTasks = new List<Task>(Utils.MaxParallelBatchSize);
                        int i = 0;
                        foreach (PropertyChangedAsyncEventHandler objEvent in _setPropertyChangedAsync)
                        {
                            foreach (PropertyChangedEventArgs objArg in lstArgsList)
                            {
                                lstTasks.Add(objEvent.Invoke(this, objArg, token));
                                if (++i < Utils.MaxParallelBatchSize)
                                    continue;
                                await Task.WhenAll(lstTasks).ConfigureAwait(false);
                                lstTasks.Clear();
                                i = 0;
                            }
                        }

                        await Task.WhenAll(lstTasks).ConfigureAwait(false);

                        if (PropertyChanged != null)
                        {
                            await Utils.RunOnMainThreadAsync(() =>
                            {
                                if (PropertyChanged != null)
                                {
                                    // ReSharper disable once AccessToModifiedClosure
                                    foreach (PropertyChangedEventArgs objArgs in lstArgsList)
                                    {
                                        PropertyChanged.Invoke(this, objArgs);
                                    }
                                }
                            }, token).ConfigureAwait(false);
                        }
                    }
                    else if (PropertyChanged != null)
                    {
                        await Utils.RunOnMainThreadAsync(() =>
                        {
                            if (PropertyChanged != null)
                            {
                                // ReSharper disable once AccessToModifiedClosure
                                foreach (string strPropertyToChange in setNamesOfChangedProperties)
                                {
                                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
                                }
                            }
                        }, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    if (setNamesOfChangedProperties != null)
                        Utils.StringHashSetPool.Return(ref setNamesOfChangedProperties);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly PropertyDependencyGraph<CharacterSettings> s_CharacterSettingsDependencyGraph =
            new PropertyDependencyGraph<CharacterSettings>(
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
                new DependencyGraphNode<string, CharacterSettings>(nameof(WeightDecimals),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(WeightFormat))
                ),
                new DependencyGraphNode<string, CharacterSettings>(nameof(BuiltInOption),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(SourceIdString))
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
                new DependencyGraphNode<string, CharacterSettings>(nameof(CurrentDisplayName),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(Name)),
                    new DependencyGraphNode<string, CharacterSettings>(nameof(SourceIdString))
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

        public CharacterSettings(CharacterSettings objOther = null, bool blnCopySourceId = true, string strOverrideFileName = "")
        {
            _dicCustomDataDirectoryKeys = new LockingTypedOrderedDictionary<string, bool>(LockObject);
            _setBannedWareGrades.Add("Betaware");
            _setBannedWareGrades.Add("Deltaware");
            _setBannedWareGrades.Add("Gammaware");
            _setRedlinerExcludes.Add("skull");
            _setRedlinerExcludes.Add("torso");
            if (objOther != null)
                CopyValues(objOther, blnCopySourceId, strOverrideFileName);
        }

        public void CopyValues(CharacterSettings objOther, bool blnCopySourceId = true, string strOverrideFileName = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objOther == null || objOther == this)
                return;
            using (LockObject.EnterWriteLock(token))
            {
                bool blnOldDoingCopy = _blnDoingCopy;
                _blnDoingCopy = true;
                List<string> lstPropertiesToUpdate;
                try
                {
                    PropertyInfo[] aobjProperties = typeof(CharacterSettings).GetProperties();
                    lstPropertiesToUpdate = new List<string>(aobjProperties.Length);
                    using (objOther.LockObject.EnterReadLock(token))
                    {
                        if (blnCopySourceId && !_guiSourceId.Equals(objOther._guiSourceId))
                        {
                            lstPropertiesToUpdate.Add(nameof(SourceIdString));
                            _guiSourceId = objOther._guiSourceId;
                        }

                        if (string.IsNullOrEmpty(strOverrideFileName))
                        {
                            if (!_strFileName.Equals(objOther._strFileName))
                            {
                                lstPropertiesToUpdate.Add(nameof(FileName));
                                _strFileName = objOther._strFileName;
                            }
                        }
                        else if (!_strFileName.Equals(strOverrideFileName))
                        {
                            lstPropertiesToUpdate.Add(nameof(FileName));
                            _strFileName = strOverrideFileName;
                        }

                        token.ThrowIfCancellationRequested();

                        // Copy over via properties in order to trigger OnPropertyChanged as appropriate
                        foreach (PropertyInfo objProperty in aobjProperties.Where(x => x.CanRead && x.CanWrite))
                        {
                            token.ThrowIfCancellationRequested();
                            object objMyValue = objProperty.GetValue(this);
                            object objOtherValue = objProperty.GetValue(objOther);
                            if (objMyValue.Equals(objOtherValue))
                                continue;
                            lstPropertiesToUpdate.Add(objProperty.Name);
                            objProperty.SetValue(this, objOtherValue);
                        }

                        bool blnDoRebuildDirectoryKeys = _dicCustomDataDirectoryKeys.Count != objOther._dicCustomDataDirectoryKeys.Count;

                        if (blnDoRebuildDirectoryKeys)
                        {
                            lstPropertiesToUpdate.Add(nameof(CustomDataDirectoryKeys));
                            using (_dicCustomDataDirectoryKeys.LockObject.EnterWriteLock(token))
                            {
                                _dicCustomDataDirectoryKeys.Clear();
                                objOther.CustomDataDirectoryKeys.ForEach(
                                    kvpOther => _dicCustomDataDirectoryKeys.Add(kvpOther.Key, kvpOther.Value),
                                    token);
                            }
                        }
                        else
                        {
                            using (objOther._dicCustomDataDirectoryKeys.LockObject.EnterReadLock(token))
                            using (_dicCustomDataDirectoryKeys.LockObject.EnterUpgradeableReadLock(token))
                            {
                                int intMyCount = _dicCustomDataDirectoryKeys.Count;
                                blnDoRebuildDirectoryKeys = intMyCount != objOther._dicCustomDataDirectoryKeys.Count;
                                if (!blnDoRebuildDirectoryKeys)
                                {
                                    for (int i = 0; i < intMyCount; ++i)
                                    {
                                        token.ThrowIfCancellationRequested();
                                        KeyValuePair<string, bool> kvpMine = _dicCustomDataDirectoryKeys[i];
                                        KeyValuePair<string, bool> kvpOther = objOther._dicCustomDataDirectoryKeys[i];
                                        if (!string.Equals(kvpMine.Key, kvpOther.Key,
                                                StringComparison.OrdinalIgnoreCase)
                                            || kvpMine.Value != kvpOther.Value)
                                        {
                                            blnDoRebuildDirectoryKeys = true;
                                            break;
                                        }
                                    }
                                }

                                if (blnDoRebuildDirectoryKeys)
                                {
                                    lstPropertiesToUpdate.Add(nameof(CustomDataDirectoryKeys));
                                    using (_dicCustomDataDirectoryKeys.LockObject.EnterWriteLock(token))
                                    {
                                        _dicCustomDataDirectoryKeys.Clear();
                                        objOther.CustomDataDirectoryKeys.ForEach(
                                            kvpOther => _dicCustomDataDirectoryKeys.Add(kvpOther.Key, kvpOther.Value),
                                            token);
                                    }
                                }
                            }
                        }

                        token.ThrowIfCancellationRequested();

                        if (!_setBooks.SetEquals(objOther._setBooks))
                        {
                            lstPropertiesToUpdate.Add(nameof(Books));
                            _setBooks.Clear();
                            foreach (string strBook in objOther._setBooks)
                            {
                                _setBooks.Add(strBook);
                            }
                        }

                        token.ThrowIfCancellationRequested();

                        if (!_setBannedWareGrades.SetEquals(objOther._setBannedWareGrades))
                        {
                            lstPropertiesToUpdate.Add(nameof(BannedWareGrades));
                            _setBannedWareGrades.Clear();
                            foreach (string strGrade in objOther._setBannedWareGrades)
                            {
                                _setBannedWareGrades.Add(strGrade);
                            }
                        }
                    }

                    // RedlinerExcludes handled through the four RedlinerExcludes[Limb] properties
                }
                finally
                {
                    _blnDoingCopy = blnOldDoingCopy;
                }

                OnMultiplePropertiesChanged(lstPropertiesToUpdate);
            }
        }

        public async Task CopyValuesAsync(CharacterSettings objOther, bool blnCopySourceId = true, string strOverrideFileName = "", CancellationToken token = default)
        {
            if (objOther == null || objOther == this)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnOldDoingCopy = _blnDoingCopy;
                _blnDoingCopy = true;
                List<string> lstPropertiesToUpdate;
                try
                {
                    PropertyInfo[] aobjProperties = typeof(CharacterSettings).GetProperties();
                    lstPropertiesToUpdate = new List<string>(aobjProperties.Length);
                    IAsyncDisposable objLocker2 =
                        await objOther.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (blnCopySourceId && !_guiSourceId.Equals(objOther._guiSourceId))
                        {
                            lstPropertiesToUpdate.Add(nameof(SourceIdString));
                            _guiSourceId = objOther._guiSourceId;
                        }

                        if (string.IsNullOrEmpty(strOverrideFileName))
                        {
                            if (!_strFileName.Equals(objOther._strFileName))
                            {
                                lstPropertiesToUpdate.Add(nameof(FileName));
                                _strFileName = objOther._strFileName;
                            }
                        }
                        else if (!_strFileName.Equals(strOverrideFileName))
                        {
                            lstPropertiesToUpdate.Add(nameof(FileName));
                            _strFileName = strOverrideFileName;
                        }

                        // Copy over via properties in order to trigger OnPropertyChanged as appropriate
                        foreach (PropertyInfo objProperty in aobjProperties.Where(x => x.CanRead && x.CanWrite))
                        {
                            object objMyValue = objProperty.GetValue(this);
                            object objOtherValue = objProperty.GetValue(objOther);
                            if (objMyValue.Equals(objOtherValue))
                                continue;
                            lstPropertiesToUpdate.Add(objProperty.Name);
                            objProperty.SetValue(this, objOtherValue);
                        }

                        IAsyncDisposable objLocker3 = await objOther._dicCustomDataDirectoryKeys.LockObject.EnterReadLockAsync(token)
                            .ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            IAsyncDisposable objLocker4 = await _dicCustomDataDirectoryKeys.LockObject
                                .EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                            try
                            {
                                token.ThrowIfCancellationRequested();
                                int intMyCount = await _dicCustomDataDirectoryKeys.GetCountAsync(token)
                                    .ConfigureAwait(false);
                                bool blnDoRebuildDirectoryKeys = intMyCount != await objOther
                                    ._dicCustomDataDirectoryKeys
                                    .GetCountAsync(token).ConfigureAwait(false);
                                if (!blnDoRebuildDirectoryKeys)
                                {
                                    for (int i = 0; i < intMyCount; ++i)
                                    {
                                        KeyValuePair<string, bool> kvpMine = await _dicCustomDataDirectoryKeys
                                            .GetValueAtAsync(i, token)
                                            .ConfigureAwait(false);
                                        KeyValuePair<string, bool> kvpOther = await objOther._dicCustomDataDirectoryKeys
                                            .GetValueAtAsync(i, token).ConfigureAwait(false);
                                        if (!string.Equals(kvpMine.Key, kvpOther.Key,
                                                StringComparison.OrdinalIgnoreCase)
                                            || kvpMine.Value != kvpOther.Value)
                                        {
                                            blnDoRebuildDirectoryKeys = true;
                                            break;
                                        }
                                    }
                                }

                                if (blnDoRebuildDirectoryKeys)
                                {
                                    lstPropertiesToUpdate.Add(nameof(CustomDataDirectoryKeys));
                                    IAsyncDisposable objLocker5 = await _dicCustomDataDirectoryKeys.LockObject
                                        .EnterWriteLockAsync(token).ConfigureAwait(false);
                                    try
                                    {
                                        token.ThrowIfCancellationRequested();
                                        await _dicCustomDataDirectoryKeys.ClearAsync(token).ConfigureAwait(false);
                                        await objOther._dicCustomDataDirectoryKeys
                                            .ForEachAsync(
                                                kvpOther => _dicCustomDataDirectoryKeys.AddAsync(kvpOther.Key,
                                                    kvpOther.Value, token), token).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        await objLocker5.DisposeAsync().ConfigureAwait(false);
                                    }
                                }
                            }
                            finally
                            {
                                await objLocker4.DisposeAsync().ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            await objLocker3.DisposeAsync().ConfigureAwait(false);
                        }

                        if (!_setBooks.SetEquals(objOther._setBooks))
                        {
                            lstPropertiesToUpdate.Add(nameof(Books));
                            _setBooks.Clear();
                            foreach (string strBook in objOther._setBooks)
                            {
                                _setBooks.Add(strBook);
                            }
                        }

                        if (!_setBannedWareGrades.SetEquals(objOther._setBannedWareGrades))
                        {
                            lstPropertiesToUpdate.Add(nameof(BannedWareGrades));
                            _setBannedWareGrades.Clear();
                            foreach (string strGrade in objOther._setBannedWareGrades)
                            {
                                _setBannedWareGrades.Add(strGrade);
                            }
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }

                    // RedlinerExcludes handled through the four RedlinerExcludes[Limb] properties
                }
                finally
                {
                    _blnDoingCopy = blnOldDoingCopy;
                }

                await OnMultiplePropertiesChangedAsync(lstPropertiesToUpdate, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public IEnumerable<string> GetDifferingPropertyNames(CharacterSettings objOther, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            PropertyInfo[] aobjProperties = typeof(CharacterSettings).GetProperties();
            if (objOther == null)
            {
                yield return nameof(SourceIdString);
                yield return nameof(FileName);
                foreach (PropertyInfo objProperty in aobjProperties.Where(x => x.CanRead && x.CanWrite))
                    yield return objProperty.Name;
                yield return nameof(CustomDataDirectoryKeys);
                yield return nameof(Books);
                yield return nameof(BannedWareGrades);
                yield break;
            }

            if (objOther == this)
                yield break;

            using (objOther.LockObject.EnterReadLock(token))
            using (LockObject.EnterReadLock(token))
            {
                if (!_guiSourceId.Equals(objOther.SourceId))
                {
                    yield return nameof(SourceIdString);
                }

                token.ThrowIfCancellationRequested();

                if (!_strFileName.Equals(objOther.FileName))
                {
                    yield return nameof(FileName);
                }

                token.ThrowIfCancellationRequested();

                // Copy over via properties in order to trigger OnPropertyChanged as appropriate
                foreach (PropertyInfo objProperty in aobjProperties.Where(x => x.CanRead && x.CanWrite))
                {
                    token.ThrowIfCancellationRequested();
                    object objMyValue = objProperty.GetValue(this);
                    object objOtherValue = objProperty.GetValue(objOther);
                    if (objMyValue.Equals(objOtherValue))
                        continue;
                    yield return objProperty.Name;
                }

                if (!_dicCustomDataDirectoryKeys.SequenceEqual(objOther.CustomDataDirectoryKeys))
                {
                    yield return nameof(CustomDataDirectoryKeys);
                }

                token.ThrowIfCancellationRequested();

                if (!_setBooks.SetEquals(objOther.BooksWritable))
                {
                    yield return nameof(Books);
                }

                token.ThrowIfCancellationRequested();

                if (!_setBannedWareGrades.SetEquals(objOther.BannedWareGrades))
                {
                    yield return nameof(BannedWareGrades);
                }
            }
        }

        public async Task<List<string>> GetDifferingPropertyNamesAsync(CharacterSettings objOther, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            PropertyInfo[] aobjProperties = typeof(CharacterSettings).GetProperties();
            List<string> lstReturn = new List<string>(aobjProperties.Length);
            if (objOther == null)
            {
                lstReturn.Add(nameof(SourceIdString));
                lstReturn.Add(nameof(FileName));
                foreach (PropertyInfo objProperty in aobjProperties.Where(x => x.CanRead && x.CanWrite))
                    lstReturn.Add(objProperty.Name);
                lstReturn.Add(nameof(CustomDataDirectoryKeys));
                lstReturn.Add(nameof(Books));
                lstReturn.Add(nameof(BannedWareGrades));
                return lstReturn;
            }

            if (objOther == this)
                return lstReturn;

            IAsyncDisposable objLocker =
                await objOther.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 =
                    await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (!_guiSourceId.Equals(objOther._guiSourceId))
                    {
                        lstReturn.Add(nameof(SourceIdString));
                    }

                    token.ThrowIfCancellationRequested();

                    if (!_strFileName.Equals(objOther._strFileName))
                    {
                        lstReturn.Add(nameof(FileName));
                    }

                    token.ThrowIfCancellationRequested();

                    // Copy over via properties in order to trigger OnPropertyChanged as appropriate
                    foreach (PropertyInfo objProperty in aobjProperties.Where(x => x.CanRead && x.CanWrite))
                    {
                        token.ThrowIfCancellationRequested();
                        object objMyValue = objProperty.GetValue(this);
                        object objOtherValue = objProperty.GetValue(objOther);
                        if (objMyValue.Equals(objOtherValue))
                            continue;
                        lstReturn.Add(objProperty.Name);
                    }

                    if (!await _dicCustomDataDirectoryKeys.SequenceEqualAsync(await objOther.GetCustomDataDirectoryKeysAsync(token).ConfigureAwait(false), token).ConfigureAwait(false))
                    {
                        lstReturn.Add(nameof(CustomDataDirectoryKeys));
                    }

                    token.ThrowIfCancellationRequested();

                    if (!_setBooks.SetEquals(await objOther.GetBooksWritableAsync(token).ConfigureAwait(false)))
                    {
                        lstReturn.Add(nameof(Books));
                    }

                    token.ThrowIfCancellationRequested();

                    if (!_setBannedWareGrades.SetEquals(await objOther.GetBannedWareGradesAsync(token).ConfigureAwait(false)))
                    {
                        lstReturn.Add(nameof(BannedWareGrades));
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return lstReturn;
        }

        public bool HasIdenticalSettings(CharacterSettings objOther, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objOther == null)
                return false;
            using (objOther.LockObject.EnterReadLock(token))
            using (LockObject.EnterReadLock(token))
            {
                if (_guiSourceId != objOther._guiSourceId)
                    return false;
                if (_strFileName != objOther._strFileName)
                    return false;
                if (GetEquatableHashCode(token) != objOther.GetEquatableHashCode(token))
                    return false;

                PropertyInfo[] aobjProperties = typeof(CharacterSettings).GetProperties();
                foreach (PropertyInfo objProperty in aobjProperties.Where(x => x.PropertyType.IsValueType && x.CanRead))
                {
                    token.ThrowIfCancellationRequested();
                    object objMyValue = objProperty.GetValue(this);
                    object objOtherValue = objProperty.GetValue(objOther);
                    if (!objMyValue.Equals(objOtherValue))
                        return false;
                }

                using (objOther._dicCustomDataDirectoryKeys.LockObject.EnterReadLock(token))
                using (_dicCustomDataDirectoryKeys.LockObject.EnterReadLock(token))
                {
                    int intMyCount = _dicCustomDataDirectoryKeys.Count;
                    if (intMyCount != objOther._dicCustomDataDirectoryKeys.Count)
                        return false;
                    for (int i = 0; i < intMyCount; ++i)
                    {
                        KeyValuePair<string, bool> kvpMine = _dicCustomDataDirectoryKeys.GetValueAt(i, token);
                        KeyValuePair<string, bool> kvpOther = objOther._dicCustomDataDirectoryKeys.GetValueAt(i, token);
                        if (!string.Equals(kvpMine.Key, kvpOther.Key, StringComparison.OrdinalIgnoreCase)
                            || kvpMine.Value != kvpOther.Value)
                        {
                            return false;
                        }
                    }
                }

                // RedlinerExcludes handled through the four RedlinerExcludes[Limb] properties

                return _setBooks.SetEquals(objOther._setBooks) && _setBannedWareGrades.SetEquals(objOther._setBannedWareGrades);
            }
        }

        public async Task<bool> HasIdenticalSettingsAsync(CharacterSettings objOther, CancellationToken token = default)
        {
            if (objOther == null)
                return false;
            IAsyncDisposable objLocker =
                await objOther.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                IAsyncDisposable objLocker2 =
                    await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (_guiSourceId != objOther._guiSourceId)
                        return false;
                    if (_strFileName != objOther._strFileName)
                        return false;
                    if (await GetEquatableHashCodeAsync(token).ConfigureAwait(false) !=
                        await objOther.GetEquatableHashCodeAsync(token).ConfigureAwait(false))
                        return false;

                    PropertyInfo[] aobjProperties = typeof(CharacterSettings).GetProperties();
                    foreach (PropertyInfo objProperty in aobjProperties.Where(x =>
                                 x.PropertyType.IsValueType && x.CanRead))
                    {
                        token.ThrowIfCancellationRequested();
                        object objMyValue = objProperty.GetValue(this);
                        object objOtherValue = objProperty.GetValue(objOther);
                        if (!objMyValue.Equals(objOtherValue))
                            return false;
                    }

                    IAsyncDisposable objLocker3 =
                        await objOther._dicCustomDataDirectoryKeys.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        IAsyncDisposable objLocker4 =
                            await _dicCustomDataDirectoryKeys.LockObject.EnterReadLockAsync(token)
                                .ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            int intMyCount = await _dicCustomDataDirectoryKeys.GetCountAsync(token)
                                .ConfigureAwait(false);
                            if (intMyCount != await objOther._dicCustomDataDirectoryKeys.GetCountAsync(token)
                                    .ConfigureAwait(false))
                                return false;
                            for (int i = 0; i < intMyCount; ++i)
                            {
                                KeyValuePair<string, bool> kvpMine = await _dicCustomDataDirectoryKeys
                                    .GetValueAtAsync(i, token)
                                    .ConfigureAwait(false);
                                KeyValuePair<string, bool> kvpOther = await objOther._dicCustomDataDirectoryKeys
                                    .GetValueAtAsync(i, token)
                                    .ConfigureAwait(false);
                                if (!string.Equals(kvpMine.Key, kvpOther.Key, StringComparison.OrdinalIgnoreCase)
                                    || kvpMine.Value != kvpOther.Value)
                                {
                                    return false;
                                }
                            }
                        }
                        finally
                        {
                            await objLocker4.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objLocker3.DisposeAsync().ConfigureAwait(false);
                    }

                    // RedlinerExcludes handled through the four RedlinerExcludes[Limb] properties

                    return _setBooks.SetEquals(objOther._setBooks) &&
                           _setBannedWareGrades.SetEquals(objOther._setBannedWareGrades);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Needed because it's not a strict replacement for GetHashCode().
        /// Gets a number based on every single private property of the setting.
        /// If two settings have unequal Hash Codes, they will never actually be equal.
        /// </summary>
        public int GetEquatableHashCode(CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
                return GetEquatableHashCodeCommon();
        }

        /// <summary>
        /// Needed because it's not a strict replacement for GetHashCode().
        /// Gets a number based on every single private property of the setting.
        /// If two settings have unequal Hash Codes, they will never actually be equal.
        /// </summary>
        public async Task<int> GetEquatableHashCodeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return GetEquatableHashCodeCommon();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int GetEquatableHashCodeCommon()
        {
            unchecked
            {
                int hashCode = _guiSourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (_strFileName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _blnAllowBiowareSuites.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowCyberwareESSDiscounts.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowEditPartOfBaseWeapon.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowHigherStackedFoci.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowInitiationInCreateMode.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDontUseCyberlimbCalculation.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnAllowSkillRegrouping.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnSpecializationsBreakSkillGroups.GetHashCode();
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
                hashCode = (hashCode * 397) ^ _blnExceedNegativeQualitiesNoBonus.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnExceedPositiveQualities.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnExceedPositiveQualitiesCostDoubled.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnExtendAnyDetectionSpell.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDroneArmorMultiplierEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnFreeSpiritPowerPointsMAG.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnNoArmorEncumbrance.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnUncappedArmorAccessoryBonuses.GetHashCode();
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
                hashCode = (hashCode * 397) ^ (_strEssenceModifierPostExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strLiftLimitExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strCarryLimitExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strEncumbranceIntervalExpression?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _blnDoEncumbrancePenaltyPhysicalLimit.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDoEncumbrancePenaltyMovementSpeed.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDoEncumbrancePenaltyAgility.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDoEncumbrancePenaltyReaction.GetHashCode();
                hashCode = (hashCode * 397) ^ _blnDoEncumbrancePenaltyWoundModifier.GetHashCode();
                hashCode = (hashCode * 397) ^ _intEncumbrancePenaltyPhysicalLimit.GetHashCode();
                hashCode = (hashCode * 397) ^ _intEncumbrancePenaltyMovementSpeed.GetHashCode();
                hashCode = (hashCode * 397) ^ _intEncumbrancePenaltyAgility.GetHashCode();
                hashCode = (hashCode * 397) ^ _intEncumbrancePenaltyReaction.GetHashCode();
                hashCode = (hashCode * 397) ^ _intEncumbrancePenaltyWoundModifier.GetHashCode();
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
                hashCode = (hashCode * 397) ^ (_strWeightFormat?.GetHashCode() ?? 0);
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
                hashCode = (hashCode * 397) ^ _intMaxNumberMaxAttributesCreate;
                hashCode = (hashCode * 397) ^ _intMaxSkillRatingCreate;
                hashCode = (hashCode * 397) ^ _intMaxKnowledgeSkillRatingCreate;
                hashCode = (hashCode * 397) ^ _intMaxSkillRating;
                hashCode = (hashCode * 397) ^ _intMaxKnowledgeSkillRating;
                hashCode = (hashCode * 397) ^ _intMinInitiativeDice;
                hashCode = (hashCode * 397) ^ _intMaxInitiativeDice;
                hashCode = (hashCode * 397) ^ _intMinAstralInitiativeDice;
                hashCode = (hashCode * 397) ^ _intMaxAstralInitiativeDice;
                hashCode = (hashCode * 397) ^ _intMinColdSimInitiativeDice;
                hashCode = (hashCode * 397) ^ _intMaxColdSimInitiativeDice;
                hashCode = (hashCode * 397) ^ _intMinHotSimInitiativeDice;
                hashCode = (hashCode * 397) ^ _intMaxHotSimInitiativeDice;
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
                hashCode = (hashCode * 397) ^ (int)_eBuildMethod;
                hashCode = (hashCode * 397) ^ _intBuildPoints;
                hashCode = (hashCode * 397) ^ _intQualityKarmaLimit;
                hashCode = (hashCode * 397) ^ (_strPriorityArray?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_strPriorityTable?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _intSumtoTen;
                hashCode = (hashCode * 397) ^ _decNuyenMaximumBP.GetHashCode();
                hashCode = (hashCode * 397) ^ _intAvailability;
                hashCode = (hashCode * 397) ^ _intMaxMartialArts;
                hashCode = (hashCode * 397) ^ _intMaxMartialTechniques;
                hashCode = (hashCode * 397) ^ _decNuyenCarryover.GetHashCode();
                hashCode = (hashCode * 397) ^ (_dicCustomDataDirectoryKeys?.GetEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_setEnabledCustomDataDirectories?.GetEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397)
                           ^ (_setEnabledCustomDataDirectoryGuids?.GetOrderInvariantEnsembleHashCodeSmart() ?? 0);
                hashCode = (hashCode * 397) ^ (_lstEnabledCustomDataDirectoryPaths?.GetEnsembleHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (_setBooks?.GetOrderInvariantEnsembleHashCodeSmart() ?? 0);
                hashCode = (hashCode * 397) ^ (_setBannedWareGrades?.GetOrderInvariantEnsembleHashCodeSmart() ?? 0);
                hashCode = (hashCode * 397) ^ (_setRedlinerExcludes?.GetOrderInvariantEnsembleHashCodeSmart() ?? 0);
                hashCode = (hashCode * 397) ^ _decKarmaMAGInitiationGroupPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ _decKarmaRESInitiationGroupPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ _decKarmaMAGInitiationOrdealPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ _decKarmaRESInitiationOrdealPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ _decKarmaMAGInitiationSchoolingPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ _decKarmaRESInitiationSchoolingPercent.GetHashCode();
                hashCode = (hashCode * 397) ^ SpecializationBonus;
                hashCode = (hashCode * 397) ^ ExpertiseBonus;
                return hashCode;
            }
        }

        /// <summary>
        /// Save the current settings to the settings file.
        /// </summary>
        /// <param name="strNewFileName">New file name to use. If empty, uses the existing, built-in file name.</param>
        /// <param name="blnClearSourceGuid">Whether to clear SourceId after a successful save or not. Used to turn built-in options into custom ones.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public bool Save(string strNewFileName = "", bool blnClearSourceGuid = false, CancellationToken token = default)
        {
            // Create the settings directory if it does not exist.
            if (!Directory.Exists(Utils.GetSettingsFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(Utils.GetSettingsFolderPath);
                }
                catch (UnauthorizedAccessException)
                {
                    Program.ShowScrollableMessageBox(LanguageManager.GetString("Message_Insufficient_Permissions_Warning", token: token));
                    return false;
                }
            }

            using (LockObject.EnterReadLock(token))
            {
                if (!string.IsNullOrEmpty(strNewFileName))
                    _strFileName = strNewFileName;
                string strFilePath = Path.Combine(Utils.GetSettingsFolderPath, _strFileName);
                using (FileStream objStream
                       = new FileStream(strFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                    {
                        token.ThrowIfCancellationRequested();
                        objWriter.WriteStartDocument();

                        token.ThrowIfCancellationRequested();
                        // <settings>
                        objWriter.WriteStartElement("settings");

                        // <id />
                        objWriter.WriteElementString(
                            "id",
                            blnClearSourceGuid ? Utils.GuidEmptyString : _guiSourceId.ToString("D", GlobalSettings.InvariantCultureInfo));
                        // <name />
                        objWriter.WriteElementString("name", _strName);

                        // <licenserestricted />
                        objWriter.WriteElementString("licenserestricted",
                                                     _blnLicenseRestrictedItems.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <morelethalgameplay />
                        objWriter.WriteElementString("morelethalgameplay",
                                                     _blnMoreLethalGameplay.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <spiritforcebasedontotalmag />
                        objWriter.WriteElementString("spiritforcebasedontotalmag",
                                                     _blnSpiritForceBasedOnTotalMAG.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <nuyenperbpwftm />
                        objWriter.WriteElementString("nuyenperbpwftm",
                                                     _decNuyenPerBPWftM.ToString(GlobalSettings.InvariantCultureInfo));
                        // <nuyenperbpwftp />
                        objWriter.WriteElementString("nuyenperbpwftp",
                                                     _decNuyenPerBPWftP.ToString(GlobalSettings.InvariantCultureInfo));
                        // <UnarmedImprovementsApplyToWeapons />
                        objWriter.WriteElementString("unarmedimprovementsapplytoweapons",
                                                     _blnUnarmedImprovementsApplyToWeapons.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <allowinitiationincreatemode />
                        objWriter.WriteElementString("allowinitiationincreatemode",
                                                     _blnAllowInitiationInCreateMode.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <usepointsonbrokengroups />
                        objWriter.WriteElementString("usepointsonbrokengroups",
                                                     _blnUsePointsOnBrokenGroups.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <dontdoublequalities />
                        objWriter.WriteElementString("dontdoublequalities",
                                                     _blnDontDoubleQualityPurchaseCost.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <dontdoublequalities />
                        objWriter.WriteElementString("dontdoublequalityrefunds",
                                                     _blnDontDoubleQualityRefundCost.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <ignoreart />
                        objWriter.WriteElementString("ignoreart",
                                                     _blnIgnoreArt.ToString(GlobalSettings.InvariantCultureInfo));
                        // <cyberlegmovement />
                        objWriter.WriteElementString("cyberlegmovement",
                                                     _blnCyberlegMovement.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <contactpointsexpression />
                        objWriter.WriteElementString("contactpointsexpression", _strContactPointsExpression);
                        // <knowledgepointsexpression />
                        objWriter.WriteElementString("knowledgepointsexpression", _strKnowledgePointsExpression);
                        // <chargenkarmatonuyenexpression />
                        objWriter.WriteElementString("chargenkarmatonuyenexpression",
                                                     _strChargenKarmaToNuyenExpression);
                        // <boundspiritexpression />
                        objWriter.WriteElementString("boundspiritexpression", _strBoundSpiritExpression);
                        // <compiledspriteexpression />
                        objWriter.WriteElementString("compiledspriteexpression", _strRegisteredSpriteExpression);
                        // <essencemodifierpostexpression />
                        objWriter.WriteElementString("essencemodifierpostexpression", _strEssenceModifierPostExpression);
                        // <liftlimitexpression />
                        objWriter.WriteElementString("liftlimitexpression", _strLiftLimitExpression);
                        // <carrylimitexpression />
                        objWriter.WriteElementString("carrylimitexpression", _strCarryLimitExpression);
                        // <encumbranceintervalexpression />
                        objWriter.WriteElementString("encumbranceintervalexpression",
                                                     _strEncumbranceIntervalExpression);
                        // <doencumbrancepenaltyphysicallimit />
                        objWriter.WriteElementString("doencumbrancepenaltyphysicallimit",
                                                     _blnDoEncumbrancePenaltyPhysicalLimit.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <doencumbrancepenaltymovementspeed />
                        objWriter.WriteElementString("doencumbrancepenaltymovementspeed",
                                                     _blnDoEncumbrancePenaltyMovementSpeed.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <doencumbrancepenaltyagility />
                        objWriter.WriteElementString("doencumbrancepenaltyagility",
                                                     _blnDoEncumbrancePenaltyAgility.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <doencumbrancepenaltyreaction />
                        objWriter.WriteElementString("doencumbrancepenaltyreaction",
                                                     _blnDoEncumbrancePenaltyReaction.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <doencumbrancepenaltywoundmodifier />
                        objWriter.WriteElementString("doencumbrancepenaltywoundmodifier",
                                                     _blnDoEncumbrancePenaltyWoundModifier.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <encumbrancepenaltyphysicallimit />
                        objWriter.WriteElementString("encumbrancepenaltyphysicallimit",
                                                     _intEncumbrancePenaltyPhysicalLimit.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <encumbrancepenaltymovementspeed />
                        objWriter.WriteElementString("encumbrancepenaltymovementspeed",
                                                     _intEncumbrancePenaltyMovementSpeed.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <encumbrancepenaltyagility />
                        objWriter.WriteElementString("encumbrancepenaltyagility",
                                                     _intEncumbrancePenaltyAgility.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <encumbrancepenaltyreaction />
                        objWriter.WriteElementString("encumbrancepenaltyreaction",
                                                     _intEncumbrancePenaltyReaction.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <encumbrancepenaltywoundmodifier />
                        objWriter.WriteElementString("encumbrancepenaltywoundmodifier",
                                                     _intEncumbrancePenaltyWoundModifier.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <dronearmormultiplierenabled />
                        objWriter.WriteElementString("dronearmormultiplierenabled",
                                                     _blnDroneArmorMultiplierEnabled.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <dronearmorflatnumber />
                        objWriter.WriteElementString("dronearmorflatnumber",
                                                     _intDroneArmorMultiplier.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <nosinglearmorencumbrance />
                        objWriter.WriteElementString("nosinglearmorencumbrance",
                                                     _blnNoSingleArmorEncumbrance.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <ignorecomplexformlimit />
                        objWriter.WriteElementString("ignorecomplexformlimit",
                                                     _blnIgnoreComplexFormLimit.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <noarmorencumbrance />
                        objWriter.WriteElementString("noarmorencumbrance",
                                                     _blnNoArmorEncumbrance.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <uncappedarmoraccessorybonuses />
                        objWriter.WriteElementString("uncappedarmoraccessorybonuses",
                                                     _blnUncappedArmorAccessoryBonuses.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <esslossreducesmaximumonly />
                        objWriter.WriteElementString("esslossreducesmaximumonly",
                                                     _blnESSLossReducesMaximumOnly.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <allowskillregrouping />
                        objWriter.WriteElementString("allowskillregrouping",
                                                     _blnAllowSkillRegrouping.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <specializationsbreakskillgroups />
                        objWriter.WriteElementString("specializationsbreakskillgroups",
                                                     _blnSpecializationsBreakSkillGroups.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <metatypecostskarma />
                        objWriter.WriteElementString("metatypecostskarma",
                                                     _blnMetatypeCostsKarma.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <metatypecostskarmamultiplier />
                        objWriter.WriteElementString("metatypecostskarmamultiplier",
                                                     _intMetatypeCostMultiplier.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <limbcount />
                        objWriter.WriteElementString("limbcount",
                                                     _intLimbCount.ToString(GlobalSettings.InvariantCultureInfo));
                        // <excludelimbslot />
                        objWriter.WriteElementString("excludelimbslot", _strExcludeLimbSlot);
                        // <allowcyberwareessdiscounts />
                        objWriter.WriteElementString("allowcyberwareessdiscounts",
                                                     _blnAllowCyberwareESSDiscounts.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maximumarmormodifications />
                        objWriter.WriteElementString("maximumarmormodifications",
                                                     _blnMaximumArmorModifications.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <armordegredation />
                        objWriter.WriteElementString("armordegredation",
                                                     _blnArmorDegradation.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <specialkarmacostbasedonshownvalue />
                        objWriter.WriteElementString("specialkarmacostbasedonshownvalue",
                                                     _blnSpecialKarmaCostBasedOnShownValue.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <exceedpositivequalities />
                        objWriter.WriteElementString("exceedpositivequalities",
                                                     _blnExceedPositiveQualities.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <exceedpositivequalitiescostdoubled />
                        objWriter.WriteElementString("exceedpositivequalitiescostdoubled",
                                                     _blnExceedPositiveQualitiesCostDoubled.ToString(
                                                         GlobalSettings.InvariantCultureInfo));

                        objWriter.WriteElementString("mysaddppcareer",
                            _blnMysAdeptAllowPpCareer.ToString(
                                GlobalSettings.InvariantCultureInfo));

                        // <mysadeptsecondmagattribute />
                        objWriter.WriteElementString("mysadeptsecondmagattribute",
                                                     _blnMysAdeptSecondMAGAttribute.ToString(
                                                         GlobalSettings.InvariantCultureInfo));

                        // <exceednegativequalities />
                        objWriter.WriteElementString("exceednegativequalities",
                                                     _blnExceedNegativeQualities.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <exceednegativequalitiesnobonus />
                        objWriter.WriteElementString("exceednegativequalitiesnobonus",
                                                     _blnExceedNegativeQualitiesNoBonus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <multiplyrestrictedcost />
                        objWriter.WriteElementString("multiplyrestrictedcost",
                                                     _blnMultiplyRestrictedCost.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <multiplyforbiddencost />
                        objWriter.WriteElementString("multiplyforbiddencost",
                                                     _blnMultiplyForbiddenCost.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <restrictedcostmultiplier />
                        objWriter.WriteElementString("restrictedcostmultiplier",
                                                     _intRestrictedCostMultiplier.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <forbiddencostmultiplier />
                        objWriter.WriteElementString("forbiddencostmultiplier",
                                                     _intForbiddenCostMultiplier.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <donotroundessenceinternally />
                        objWriter.WriteElementString("donotroundessenceinternally",
                                                     _blnDoNotRoundEssenceInternally.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <enableenemytracking />
                        objWriter.WriteElementString("enableenemytracking",
                                                     _blnEnableEnemyTracking.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <enemykarmaqualitylimit />
                        objWriter.WriteElementString("enemykarmaqualitylimit",
                                                     _blnEnemyKarmaQualityLimit.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <nuyenformat />
                        objWriter.WriteElementString("nuyenformat", _strNuyenFormat);
                        // <weightformat />
                        objWriter.WriteElementString("weightformat", _strWeightFormat);
                        // <essencedecimals />
                        objWriter.WriteElementString("essenceformat", _strEssenceFormat);
                        // <enforcecapacity />
                        objWriter.WriteElementString("enforcecapacity",
                                                     _blnEnforceCapacity.ToString(GlobalSettings.InvariantCultureInfo));
                        // <restrictrecoil />
                        objWriter.WriteElementString("restrictrecoil",
                                                     _blnRestrictRecoil.ToString(GlobalSettings.InvariantCultureInfo));
                        // <unrestrictednuyen />
                        objWriter.WriteElementString("unrestrictednuyen",
                                                     _blnUnrestrictedNuyen.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <allowhigherstackedfoci />
                        objWriter.WriteElementString("allowhigherstackedfoci",
                                                     _blnAllowHigherStackedFoci.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <alloweditpartofbaseweapon />
                        objWriter.WriteElementString("alloweditpartofbaseweapon",
                                                     _blnAllowEditPartOfBaseWeapon.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <breakskillgroupsincreatemode />
                        objWriter.WriteElementString("breakskillgroupsincreatemode",
                                                     _blnStrictSkillGroupsInCreateMode.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <allowpointbuyspecializationsonkarmaskills />
                        objWriter.WriteElementString("allowpointbuyspecializationsonkarmaskills",
                                                     _blnAllowPointBuySpecializationsOnKarmaSkills.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <extendanydetectionspell />
                        objWriter.WriteElementString("extendanydetectionspell",
                                                     _blnExtendAnyDetectionSpell.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        //<dontusecyberlimbcalculation />
                        objWriter.WriteElementString("dontusecyberlimbcalculation",
                                                     _blnDontUseCyberlimbCalculation.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <alternatemetatypeattributekarma />
                        objWriter.WriteElementString("alternatemetatypeattributekarma",
                                                     _blnAlternateMetatypeAttributeKarma.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <reversekarmapriorityorder />
                        objWriter.WriteElementString("reverseattributepriorityorder",
                            _blnReverseAttributePriorityOrder.ToString(
                                GlobalSettings.InvariantCultureInfo));
                        // <allowbiowaresuites />
                        objWriter.WriteElementString("allowbiowaresuites",
                                                     _blnAllowBiowareSuites.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <freespiritpowerpointsmag />
                        objWriter.WriteElementString("freespiritpowerpointsmag",
                                                     _blnFreeSpiritPowerPointsMAG.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <compensateskillgroupkarmadifference />
                        objWriter.WriteElementString("compensateskillgroupkarmadifference",
                                                     _blnCompensateSkillGroupKarmaDifference.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <autobackstory />
                        objWriter.WriteElementString("autobackstory",
                                                     _blnAutomaticBackstory.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <freemartialartspecialization />
                        objWriter.WriteElementString("freemartialartspecialization",
                                                     _blnFreeMartialArtSpecialization.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <priorityspellsasadeptpowers />
                        objWriter.WriteElementString("priorityspellsasadeptpowers",
                                                     _blnPrioritySpellsAsAdeptPowers.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <usecalculatedpublicawareness />
                        objWriter.WriteElementString("usecalculatedpublicawareness",
                                                     _blnUseCalculatedPublicAwareness.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <increasedimprovedabilitymodifier />
                        objWriter.WriteElementString("increasedimprovedabilitymodifier",
                                                     _blnIncreasedImprovedAbilityMultiplier.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <allowfreegrids />
                        objWriter.WriteElementString("allowfreegrids",
                                                     _blnAllowFreeGrids.ToString(GlobalSettings.InvariantCultureInfo));
                        // <allowtechnomancerschooling />
                        objWriter.WriteElementString("allowtechnomancerschooling",
                                                     _blnAllowTechnomancerSchooling.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <cyberlimbattributebonuscapoverride />
                        objWriter.WriteElementString("cyberlimbattributebonuscapoverride",
                                                     _blnCyberlimbAttributeBonusCapOverride.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <cyberlimbattributebonuscap />
                        objWriter.WriteElementString("cyberlimbattributebonuscap",
                                                     _intCyberlimbAttributeBonusCap.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <unclampattributeminimum />
                        objWriter.WriteElementString("unclampattributeminimum",
                                                     _blnUnclampAttributeMinimum.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <dronemods />
                        objWriter.WriteElementString("dronemods",
                                                     _blnDroneMods.ToString(GlobalSettings.InvariantCultureInfo));
                        // <dronemodsmaximumpilot />
                        objWriter.WriteElementString("dronemodsmaximumpilot",
                                                     _blnDroneModsMaximumPilot.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maxnumbermaxattributescreate />
                        objWriter.WriteElementString("maxnumbermaxattributescreate",
                                                     _intMaxNumberMaxAttributesCreate.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maxskillratingcreate />
                        objWriter.WriteElementString("maxskillratingcreate",
                                                     _intMaxSkillRatingCreate.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maxknowledgeskillratingcreate />
                        objWriter.WriteElementString("maxknowledgeskillratingcreate",
                                                     _intMaxKnowledgeSkillRatingCreate.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maxskillrating />
                        objWriter.WriteElementString("maxskillrating",
                                                     _intMaxSkillRating.ToString(GlobalSettings.InvariantCultureInfo));
                        // <maxknowledgeskillrating />
                        objWriter.WriteElementString("maxknowledgeskillrating",
                                                     _intMaxKnowledgeSkillRating.ToString(
                                                         GlobalSettings.InvariantCultureInfo));

                        // <dicepenaltysustaining />
                        objWriter.WriteElementString("dicepenaltysustaining",
                                                     _intDicePenaltySustaining.ToString(
                                                         GlobalSettings.InvariantCultureInfo));

                        // <mininitiativedice />
                        objWriter.WriteElementString("mininitiativedice",
                                                     _intMinInitiativeDice.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maxinitiativedice />
                        objWriter.WriteElementString("maxinitiativedice",
                                                     _intMaxInitiativeDice.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <minastralinitiativedice />
                        objWriter.WriteElementString("minastralinitiativedice",
                                                     _intMinAstralInitiativeDice.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maxastralinitiativedice />
                        objWriter.WriteElementString("maxastralinitiativedice",
                                                     _intMaxAstralInitiativeDice.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <mincoldsiminitiativedice />
                        objWriter.WriteElementString("mincoldsiminitiativedice",
                                                     _intMinColdSimInitiativeDice.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maxcoldsiminitiativedice />
                        objWriter.WriteElementString("maxcoldsiminitiativedice",
                                                     _intMaxColdSimInitiativeDice.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <minhotsiminitiativedice />
                        objWriter.WriteElementString("minhotsiminitiativedice",
                                                     _intMinHotSimInitiativeDice.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <maxhotsiminitiativedice />
                        objWriter.WriteElementString("maxhotsiminitiativedice",
                                                     _intMaxHotSimInitiativeDice.ToString(
                                                         GlobalSettings.InvariantCultureInfo));

                        token.ThrowIfCancellationRequested();

                        // <karmacost>
                        objWriter.WriteStartElement("karmacost");
                        // <karmaattribute />
                        objWriter.WriteElementString("karmaattribute",
                                                     _intKarmaAttribute.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaquality />
                        objWriter.WriteElementString("karmaquality",
                                                     _intKarmaQuality.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaspecialization />
                        objWriter.WriteElementString("karmaspecialization",
                                                     _intKarmaSpecialization.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaknospecialization />
                        objWriter.WriteElementString("karmaknospecialization",
                                                     _intKarmaKnoSpecialization.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmanewknowledgeskill />
                        objWriter.WriteElementString("karmanewknowledgeskill",
                                                     _intKarmaNewKnowledgeSkill.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmanewactiveskill />
                        objWriter.WriteElementString("karmanewactiveskill",
                                                     _intKarmaNewActiveSkill.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmanewskillgroup />
                        objWriter.WriteElementString("karmanewskillgroup",
                                                     _intKarmaNewSkillGroup.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaimproveknowledgeskill />
                        objWriter.WriteElementString("karmaimproveknowledgeskill",
                                                     _intKarmaImproveKnowledgeSkill.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaimproveactiveskill />
                        objWriter.WriteElementString("karmaimproveactiveskill",
                                                     _intKarmaImproveActiveSkill.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaimproveskillgroup />
                        objWriter.WriteElementString("karmaimproveskillgroup",
                                                     _intKarmaImproveSkillGroup.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaspell />
                        objWriter.WriteElementString("karmaspell",
                                                     _intKarmaSpell.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaenhancement />
                        objWriter.WriteElementString("karmaenhancement",
                                                     _intKarmaEnhancement.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmanewcomplexform />
                        objWriter.WriteElementString("karmanewcomplexform",
                                                     _intKarmaNewComplexForm.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmanewaiprogram />
                        objWriter.WriteElementString("karmanewaiprogram",
                                                     _intKarmaNewAIProgram.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmanewaiadvancedprogram />
                        objWriter.WriteElementString("karmanewaiadvancedprogram",
                                                     _intKarmaNewAIAdvancedProgram.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmacontact />
                        objWriter.WriteElementString("karmacontact",
                                                     _intKarmaContact.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaenemy />
                        objWriter.WriteElementString("karmaenemy",
                                                     _intKarmaEnemy.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmacarryover />
                        objWriter.WriteElementString("karmacarryover",
                                                     _intKarmaCarryover.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaspirit />
                        objWriter.WriteElementString("karmaspirit",
                                                     _intKarmaSpirit.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmamaneuver />
                        objWriter.WriteElementString("karmatechnique",
                                                     _intKarmaTechnique.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmainitiation />
                        objWriter.WriteElementString("karmainitiation",
                                                     _intKarmaInitiation.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmainitiationflat />
                        objWriter.WriteElementString("karmainitiationflat",
                                                     _intKarmaInitiationFlat.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmametamagic />
                        objWriter.WriteElementString("karmametamagic",
                                                     _intKarmaMetamagic.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmajoingroup />
                        objWriter.WriteElementString("karmajoingroup",
                                                     _intKarmaJoinGroup.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaleavegroup />
                        objWriter.WriteElementString("karmaleavegroup",
                                                     _intKarmaLeaveGroup.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaalchemicalfocus />
                        objWriter.WriteElementString("karmaalchemicalfocus",
                                                     _intKarmaAlchemicalFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmabanishingfocus />
                        objWriter.WriteElementString("karmabanishingfocus",
                                                     _intKarmaBanishingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmabindingfocus />
                        objWriter.WriteElementString("karmabindingfocus",
                                                     _intKarmaBindingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmacenteringfocus />
                        objWriter.WriteElementString("karmacenteringfocus",
                                                     _intKarmaCenteringFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmacounterspellingfocus />
                        objWriter.WriteElementString("karmacounterspellingfocus",
                                                     _intKarmaCounterspellingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmadisenchantingfocus />
                        objWriter.WriteElementString("karmadisenchantingfocus",
                                                     _intKarmaDisenchantingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaflexiblesignaturefocus />
                        objWriter.WriteElementString("karmaflexiblesignaturefocus",
                                                     _intKarmaFlexibleSignatureFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmamaskingfocus />
                        objWriter.WriteElementString("karmamaskingfocus",
                                                     _intKarmaMaskingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmapowerfocus />
                        objWriter.WriteElementString("karmapowerfocus",
                                                     _intKarmaPowerFocus.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaqifocus />
                        objWriter.WriteElementString("karmaqifocus",
                                                     _intKarmaQiFocus.ToString(GlobalSettings.InvariantCultureInfo));
                        // <karmaritualspellcastingfocus />
                        objWriter.WriteElementString("karmaritualspellcastingfocus",
                                                     _intKarmaRitualSpellcastingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaspellcastingfocus />
                        objWriter.WriteElementString("karmaspellcastingfocus",
                                                     _intKarmaSpellcastingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaspellshapingfocus />
                        objWriter.WriteElementString("karmaspellshapingfocus",
                                                     _intKarmaSpellShapingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmasummoningfocus />
                        objWriter.WriteElementString("karmasummoningfocus",
                                                     _intKarmaSummoningFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmasustainingfocus />
                        objWriter.WriteElementString("karmasustainingfocus",
                                                     _intKarmaSustainingFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaweaponfocus />
                        objWriter.WriteElementString("karmaweaponfocus",
                                                     _intKarmaWeaponFocus.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaweaponfocus />
                        objWriter.WriteElementString("karmamysadpp",
                                                     _intKarmaMysticAdeptPowerPoint.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <karmaspiritfettering />
                        objWriter.WriteElementString("karmaspiritfettering",
                                                     _intKarmaSpiritFettering.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // </karmacost>
                        objWriter.WriteEndElement();

                        XPathNodeIterator lstAllowedBooksCodes = XmlManager
                                                                 .LoadXPath("books.xml",
                                                                            EnabledCustomDataDirectoryPaths, token: token)
                                                                 .SelectAndCacheExpression(
                                                                     "/chummer/books/book[not(hide)]/code", token);
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                        out HashSet<string> setAllowedBooks))
                        {
                            foreach (XPathNavigator objAllowedBook in lstAllowedBooksCodes)
                            {
                                token.ThrowIfCancellationRequested();
                                if (_setBooks.Contains(objAllowedBook.Value))
                                    setAllowedBooks.Add(objAllowedBook.Value);
                            }

                            token.ThrowIfCancellationRequested();

                            // <books>
                            objWriter.WriteStartElement("books");
                            foreach (string strBook in setAllowedBooks)
                                objWriter.WriteElementString("book", strBook);
                            // </books>
                        }

                        objWriter.WriteEndElement();

                        token.ThrowIfCancellationRequested();

                        string strCustomDataRootPath = Path.Combine(Utils.GetStartupPath, "customdata");

                        // <customdatadirectorynames>
                        objWriter.WriteStartElement("customdatadirectorynames");
                        int i = -1;
                        _dicCustomDataDirectoryKeys.ForEach(kvpDirectoryInfo =>
                        {
                            string strDirectoryName = kvpDirectoryInfo.Key;
                            bool blnDirectoryIsEnabled = kvpDirectoryInfo.Value;
                            if (!blnDirectoryIsEnabled && GlobalSettings.CustomDataDirectoryInfos.Any(
                                    x => x.DirectoryPath.StartsWith(strCustomDataRootPath, StringComparison.Ordinal)
                                         && x.CharacterSettingsSaveKey.Equals(
                                             strDirectoryName, StringComparison.OrdinalIgnoreCase)))
                                return; // Do not save disabled custom data directories that are in the customdata folder and would be auto-populated anyway
                            // ReSharper disable AccessToDisposedClosure
                            objWriter.WriteStartElement("customdatadirectoryname");
                            objWriter.WriteElementString("directoryname", strDirectoryName);
                            objWriter.WriteElementString("order", Interlocked.Increment(ref i).ToString(GlobalSettings.InvariantCultureInfo));
                            objWriter.WriteElementString(
                                "enabled", blnDirectoryIsEnabled.ToString(GlobalSettings.InvariantCultureInfo));
                            objWriter.WriteEndElement();
                            // ReSharper restore AccessToDisposedClosure
                        }, token);

                        // </customdatadirectorynames>
                        objWriter.WriteEndElement();

                        // <buildmethod />
                        objWriter.WriteElementString("buildmethod", _eBuildMethod.ToString());
                        // <buildpoints />
                        objWriter.WriteElementString("buildpoints",
                                                     _intBuildPoints.ToString(GlobalSettings.InvariantCultureInfo));
                        // <qualitykarmalimit />
                        objWriter.WriteElementString("qualitykarmalimit",
                                                     _intQualityKarmaLimit.ToString(
                                                         GlobalSettings.InvariantCultureInfo));
                        // <priorityarray />
                        objWriter.WriteElementString("priorityarray", _strPriorityArray);
                        // <prioritytable />
                        objWriter.WriteElementString("prioritytable", _strPriorityTable);
                        // <sumtoten />
                        objWriter.WriteElementString(
                            "sumtoten", _intSumtoTen.ToString(GlobalSettings.InvariantCultureInfo));
                        // <availability />
                        objWriter.WriteElementString("availability",
                                                     _intAvailability.ToString(GlobalSettings.InvariantCultureInfo));
                        // <maxmartialarts />
                        objWriter.WriteElementString("maxmartialarts",
                            _intMaxMartialArts.ToString(GlobalSettings.InvariantCultureInfo));
                        // <maxmartialtechniques />
                        objWriter.WriteElementString("maxmartialtechniques",
                            _intMaxMartialTechniques.ToString(GlobalSettings.InvariantCultureInfo));
                        // <nuyencarryover />
                        objWriter.WriteElementString("nuyencarryover",
                            _decNuyenCarryover.ToString(GlobalSettings.InvariantCultureInfo));
                        // <nuyenmaxbp />
                        objWriter.WriteElementString("nuyenmaxbp",
                                                     _decNuyenMaximumBP.ToString(GlobalSettings.InvariantCultureInfo));

                        token.ThrowIfCancellationRequested();

                        // <bannedwaregrades>
                        objWriter.WriteStartElement("bannedwaregrades");
                        foreach (string strGrade in _setBannedWareGrades)
                        {
                            objWriter.WriteElementString("grade", strGrade);
                        }

                        // </bannedwaregrades>
                        objWriter.WriteEndElement();

                        token.ThrowIfCancellationRequested();

                        // <redlinerexclusion>
                        objWriter.WriteStartElement("redlinerexclusion");
                        foreach (string strLimb in _setRedlinerExcludes)
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
        }

        /// <summary>
        /// Save the current settings to the settings file.
        /// </summary>
        /// <param name="strNewFileName">New file name to use. If empty, uses the existing, built-in file name.</param>
        /// <param name="blnClearSourceGuid">Whether to clear SourceId after a successful save or not. Used to turn built-in options into custom ones.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<bool> SaveAsync(string strNewFileName = "", bool blnClearSourceGuid = false,
                                               CancellationToken token = default)
        {
            // Create the settings directory if it does not exist.
            if (!Directory.Exists(Utils.GetSettingsFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(Utils.GetSettingsFolderPath);
                }
                catch (UnauthorizedAccessException)
                {
                    await Program.ShowScrollableMessageBoxAsync(await LanguageManager
                        .GetStringAsync(
                            "Message_Insufficient_Permissions_Warning", token: token)
                        .ConfigureAwait(false), token: token).ConfigureAwait(false);
                    return false;
                }
            }

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(strNewFileName))
                    _strFileName = strNewFileName;
                string strFilePath = Path.Combine(Utils.GetSettingsFolderPath, _strFileName);
                using (FileStream objStream
                       = new FileStream(strFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (XmlWriter objWriter = Utils.GetStandardXmlWriter(objStream))
                    {
                        await objWriter.WriteStartDocumentAsync().ConfigureAwait(false);

                        // <settings>
                        await objWriter.WriteStartElementAsync("settings", token: token).ConfigureAwait(false);

                        // <id />
                        await objWriter.WriteElementStringAsync(
                            "id",
                            blnClearSourceGuid ? Utils.GuidEmptyString : _guiSourceId.ToString("D", GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        // <name />
                        await objWriter.WriteElementStringAsync("name", _strName, token: token).ConfigureAwait(false);

                        // <licenserestricted />
                        await objWriter.WriteElementStringAsync("licenserestricted",
                                _blnLicenseRestrictedItems.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <morelethalgameplay />
                        await objWriter.WriteElementStringAsync("morelethalgameplay",
                                _blnMoreLethalGameplay.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <spiritforcebasedontotalmag />
                        await objWriter.WriteElementStringAsync("spiritforcebasedontotalmag",
                                _blnSpiritForceBasedOnTotalMAG.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <nuyenperbpwftm />
                        await objWriter.WriteElementStringAsync("nuyenperbpwftm",
                                _decNuyenPerBPWftM.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <nuyenperbpwftp />
                        await objWriter.WriteElementStringAsync("nuyenperbpwftp",
                                _decNuyenPerBPWftP.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <UnarmedImprovementsApplyToWeapons />
                        await objWriter.WriteElementStringAsync("unarmedimprovementsapplytoweapons",
                                _blnUnarmedImprovementsApplyToWeapons.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <allowinitiationincreatemode />
                        await objWriter.WriteElementStringAsync("allowinitiationincreatemode",
                                _blnAllowInitiationInCreateMode.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <usepointsonbrokengroups />
                        await objWriter.WriteElementStringAsync("usepointsonbrokengroups",
                                _blnUsePointsOnBrokenGroups.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <dontdoublequalities />
                        await objWriter.WriteElementStringAsync("dontdoublequalities",
                                _blnDontDoubleQualityPurchaseCost.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <dontdoublequalities />
                        await objWriter.WriteElementStringAsync("dontdoublequalityrefunds",
                                _blnDontDoubleQualityRefundCost.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <ignoreart />
                        await objWriter.WriteElementStringAsync("ignoreart",
                                _blnIgnoreArt.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <cyberlegmovement />
                        await objWriter.WriteElementStringAsync("cyberlegmovement",
                                _blnCyberlegMovement.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <contactpointsexpression />
                        await objWriter
                            .WriteElementStringAsync("contactpointsexpression", _strContactPointsExpression,
                                token: token).ConfigureAwait(false);
                        // <knowledgepointsexpression />
                        await objWriter
                            .WriteElementStringAsync("knowledgepointsexpression", _strKnowledgePointsExpression,
                                token: token).ConfigureAwait(false);
                        // <chargenkarmatonuyenexpression />
                        await objWriter.WriteElementStringAsync("chargenkarmatonuyenexpression",
                                _strChargenKarmaToNuyenExpression, token: token)
                            .ConfigureAwait(false);
                        // <boundspiritexpression />
                        await objWriter
                            .WriteElementStringAsync("boundspiritexpression", _strBoundSpiritExpression, token: token)
                            .ConfigureAwait(false);
                        // <compiledspriteexpression />
                        await objWriter
                            .WriteElementStringAsync("compiledspriteexpression", _strRegisteredSpriteExpression,
                                token: token).ConfigureAwait(false);
                        // <essencemodifierpostexpression />
                        await objWriter
                            .WriteElementStringAsync("essencemodifierpostexpression",
                                _strEssenceModifierPostExpression, token: token)
                            .ConfigureAwait(false);
                        // <liftlimitexpression />
                        await objWriter
                            .WriteElementStringAsync("liftlimitexpression", _strLiftLimitExpression, token: token)
                            .ConfigureAwait(false);
                        // <carrylimitexpression />
                        await objWriter
                            .WriteElementStringAsync("carrylimitexpression", _strCarryLimitExpression, token: token)
                            .ConfigureAwait(false);
                        // <encumbranceintervalexpression />
                        await objWriter.WriteElementStringAsync("encumbranceintervalexpression",
                                _strEncumbranceIntervalExpression, token: token)
                            .ConfigureAwait(false);
                        // <doencumbrancepenaltyphysicallimit />
                        await objWriter.WriteElementStringAsync("doencumbrancepenaltyphysicallimit",
                                _blnDoEncumbrancePenaltyPhysicalLimit.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <doencumbrancepenaltymovementspeed />
                        await objWriter.WriteElementStringAsync("doencumbrancepenaltymovementspeed",
                                _blnDoEncumbrancePenaltyMovementSpeed.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <doencumbrancepenaltyagility />
                        await objWriter.WriteElementStringAsync("doencumbrancepenaltyagility",
                                _blnDoEncumbrancePenaltyAgility.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <doencumbrancepenaltyreaction />
                        await objWriter.WriteElementStringAsync("doencumbrancepenaltyreaction",
                                _blnDoEncumbrancePenaltyReaction.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <doencumbrancepenaltywoundmodifier />
                        await objWriter.WriteElementStringAsync("doencumbrancepenaltywoundmodifier",
                                _blnDoEncumbrancePenaltyWoundModifier.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <encumbrancepenaltyphysicallimit />
                        await objWriter.WriteElementStringAsync("encumbrancepenaltyphysicallimit",
                                _intEncumbrancePenaltyPhysicalLimit.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <encumbrancepenaltymovementspeed />
                        await objWriter.WriteElementStringAsync("encumbrancepenaltymovementspeed",
                                _intEncumbrancePenaltyMovementSpeed.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <encumbrancepenaltyagility />
                        await objWriter.WriteElementStringAsync("encumbrancepenaltyagility",
                                _intEncumbrancePenaltyAgility.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <encumbrancepenaltyreaction />
                        await objWriter.WriteElementStringAsync("encumbrancepenaltyreaction",
                                _intEncumbrancePenaltyReaction.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <encumbrancepenaltywoundmodifier />
                        await objWriter.WriteElementStringAsync("encumbrancepenaltywoundmodifier",
                                _intEncumbrancePenaltyWoundModifier.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <dronearmormultiplierenabled />
                        await objWriter.WriteElementStringAsync("dronearmormultiplierenabled",
                                _blnDroneArmorMultiplierEnabled.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <dronearmorflatnumber />
                        await objWriter.WriteElementStringAsync("dronearmorflatnumber",
                                _intDroneArmorMultiplier.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <nosinglearmorencumbrance />
                        await objWriter.WriteElementStringAsync("nosinglearmorencumbrance",
                                _blnNoSingleArmorEncumbrance.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <ignorecomplexformlimit />
                        await objWriter.WriteElementStringAsync("ignorecomplexformlimit",
                                _blnIgnoreComplexFormLimit.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <noarmorencumbrance />
                        await objWriter.WriteElementStringAsync("noarmorencumbrance",
                                _blnNoArmorEncumbrance.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <uncappedarmoraccessorybonuses />
                        await objWriter.WriteElementStringAsync("uncappedarmoraccessorybonuses",
                                _blnUncappedArmorAccessoryBonuses.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <esslossreducesmaximumonly />
                        await objWriter.WriteElementStringAsync("esslossreducesmaximumonly",
                                _blnESSLossReducesMaximumOnly.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <allowskillregrouping />
                        await objWriter.WriteElementStringAsync("allowskillregrouping",
                                _blnAllowSkillRegrouping.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <specializationsbreakskillgroups />
                        await objWriter.WriteElementStringAsync("specializationsbreakskillgroups",
                                _blnSpecializationsBreakSkillGroups.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <metatypecostskarma />
                        await objWriter.WriteElementStringAsync("metatypecostskarma",
                                _blnMetatypeCostsKarma.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <metatypecostskarmamultiplier />
                        await objWriter.WriteElementStringAsync("metatypecostskarmamultiplier",
                                _intMetatypeCostMultiplier.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <limbcount />
                        await objWriter.WriteElementStringAsync("limbcount",
                                _intLimbCount.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <excludelimbslot />
                        await objWriter.WriteElementStringAsync("excludelimbslot", _strExcludeLimbSlot, token: token)
                            .ConfigureAwait(false);
                        // <allowcyberwareessdiscounts />
                        await objWriter.WriteElementStringAsync("allowcyberwareessdiscounts",
                                _blnAllowCyberwareESSDiscounts.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maximumarmormodifications />
                        await objWriter.WriteElementStringAsync("maximumarmormodifications",
                                _blnMaximumArmorModifications.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <armordegredation />
                        await objWriter.WriteElementStringAsync("armordegredation",
                                _blnArmorDegradation.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <specialkarmacostbasedonshownvalue />
                        await objWriter.WriteElementStringAsync("specialkarmacostbasedonshownvalue",
                                _blnSpecialKarmaCostBasedOnShownValue.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <exceedpositivequalities />
                        await objWriter.WriteElementStringAsync("exceedpositivequalities",
                                _blnExceedPositiveQualities.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <exceedpositivequalitiescostdoubled />
                        await objWriter.WriteElementStringAsync("exceedpositivequalitiescostdoubled",
                                _blnExceedPositiveQualitiesCostDoubled.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);

                        await objWriter.WriteElementStringAsync("mysaddppcareer",
                                _blnMysAdeptAllowPpCareer.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);

                        // <mysadeptsecondmagattribute />
                        await objWriter.WriteElementStringAsync("mysadeptsecondmagattribute",
                                _blnMysAdeptSecondMAGAttribute.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);

                        // <exceednegativequalities />
                        await objWriter.WriteElementStringAsync("exceednegativequalities",
                                _blnExceedNegativeQualities.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <exceednegativequalitiesnobonus />
                        await objWriter.WriteElementStringAsync("exceednegativequalitiesnobonus",
                                _blnExceedNegativeQualitiesNoBonus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <multiplyrestrictedcost />
                        await objWriter.WriteElementStringAsync("multiplyrestrictedcost",
                                _blnMultiplyRestrictedCost.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <multiplyforbiddencost />
                        await objWriter.WriteElementStringAsync("multiplyforbiddencost",
                                _blnMultiplyForbiddenCost.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <restrictedcostmultiplier />
                        await objWriter.WriteElementStringAsync("restrictedcostmultiplier",
                                _intRestrictedCostMultiplier.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <forbiddencostmultiplier />
                        await objWriter.WriteElementStringAsync("forbiddencostmultiplier",
                                _intForbiddenCostMultiplier.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <donotroundessenceinternally />
                        await objWriter.WriteElementStringAsync("donotroundessenceinternally",
                                _blnDoNotRoundEssenceInternally.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <enableenemytracking />
                        await objWriter.WriteElementStringAsync("enableenemytracking",
                                _blnEnableEnemyTracking.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <enemykarmaqualitylimit />
                        await objWriter.WriteElementStringAsync("enemykarmaqualitylimit",
                                _blnEnemyKarmaQualityLimit.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <nuyenformat />
                        await objWriter.WriteElementStringAsync("nuyenformat", _strNuyenFormat, token: token)
                            .ConfigureAwait(false);
                        // <weightformat />
                        await objWriter.WriteElementStringAsync("weightformat", _strWeightFormat, token: token)
                            .ConfigureAwait(false);
                        // <essencedecimals />
                        await objWriter.WriteElementStringAsync("essenceformat", _strEssenceFormat, token: token)
                            .ConfigureAwait(false);
                        // <enforcecapacity />
                        await objWriter.WriteElementStringAsync("enforcecapacity",
                                _blnEnforceCapacity.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <restrictrecoil />
                        await objWriter.WriteElementStringAsync("restrictrecoil",
                                _blnRestrictRecoil.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <unrestrictednuyen />
                        await objWriter.WriteElementStringAsync("unrestrictednuyen",
                                _blnUnrestrictedNuyen.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <allowhigherstackedfoci />
                        await objWriter.WriteElementStringAsync("allowhigherstackedfoci",
                                _blnAllowHigherStackedFoci.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <alloweditpartofbaseweapon />
                        await objWriter.WriteElementStringAsync("alloweditpartofbaseweapon",
                                _blnAllowEditPartOfBaseWeapon.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <breakskillgroupsincreatemode />
                        await objWriter.WriteElementStringAsync("breakskillgroupsincreatemode",
                                _blnStrictSkillGroupsInCreateMode.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <allowpointbuyspecializationsonkarmaskills />
                        await objWriter.WriteElementStringAsync("allowpointbuyspecializationsonkarmaskills",
                                _blnAllowPointBuySpecializationsOnKarmaSkills.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <extendanydetectionspell />
                        await objWriter.WriteElementStringAsync("extendanydetectionspell",
                                _blnExtendAnyDetectionSpell.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        //<dontusecyberlimbcalculation />
                        await objWriter.WriteElementStringAsync("dontusecyberlimbcalculation",
                                _blnDontUseCyberlimbCalculation.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <alternatemetatypeattributekarma />
                        await objWriter.WriteElementStringAsync("alternatemetatypeattributekarma",
                                _blnAlternateMetatypeAttributeKarma.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <reversekarmapriorityorder />
                        await objWriter.WriteElementStringAsync("reverseattributepriorityorder",
                                _blnReverseAttributePriorityOrder.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <allowbiowaresuites />
                        await objWriter.WriteElementStringAsync("allowbiowaresuites",
                                _blnAllowBiowareSuites.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <freespiritpowerpointsmag />
                        await objWriter.WriteElementStringAsync("freespiritpowerpointsmag",
                                _blnFreeSpiritPowerPointsMAG.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <compensateskillgroupkarmadifference />
                        await objWriter.WriteElementStringAsync("compensateskillgroupkarmadifference",
                                _blnCompensateSkillGroupKarmaDifference.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <autobackstory />
                        await objWriter.WriteElementStringAsync("autobackstory",
                                _blnAutomaticBackstory.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <freemartialartspecialization />
                        await objWriter.WriteElementStringAsync("freemartialartspecialization",
                                _blnFreeMartialArtSpecialization.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <priorityspellsasadeptpowers />
                        await objWriter.WriteElementStringAsync("priorityspellsasadeptpowers",
                                _blnPrioritySpellsAsAdeptPowers.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <usecalculatedpublicawareness />
                        await objWriter.WriteElementStringAsync("usecalculatedpublicawareness",
                                _blnUseCalculatedPublicAwareness.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <increasedimprovedabilitymodifier />
                        await objWriter.WriteElementStringAsync("increasedimprovedabilitymodifier",
                                _blnIncreasedImprovedAbilityMultiplier.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <allowfreegrids />
                        await objWriter.WriteElementStringAsync("allowfreegrids",
                                _blnAllowFreeGrids.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <allowtechnomancerschooling />
                        await objWriter.WriteElementStringAsync("allowtechnomancerschooling",
                                _blnAllowTechnomancerSchooling.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <cyberlimbattributebonuscapoverride />
                        await objWriter.WriteElementStringAsync("cyberlimbattributebonuscapoverride",
                                _blnCyberlimbAttributeBonusCapOverride.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <cyberlimbattributebonuscap />
                        await objWriter.WriteElementStringAsync("cyberlimbattributebonuscap",
                                _intCyberlimbAttributeBonusCap.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <unclampattributeminimum />
                        await objWriter.WriteElementStringAsync("unclampattributeminimum",
                                _blnUnclampAttributeMinimum.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <dronemods />
                        await objWriter.WriteElementStringAsync("dronemods",
                                _blnDroneMods.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <dronemodsmaximumpilot />
                        await objWriter.WriteElementStringAsync("dronemodsmaximumpilot",
                                _blnDroneModsMaximumPilot.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxnumbermaxattributescreate />
                        await objWriter.WriteElementStringAsync("maxnumbermaxattributescreate",
                                _intMaxNumberMaxAttributesCreate.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxskillratingcreate />
                        await objWriter.WriteElementStringAsync("maxskillratingcreate",
                                _intMaxSkillRatingCreate.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxknowledgeskillratingcreate />
                        await objWriter.WriteElementStringAsync("maxknowledgeskillratingcreate",
                                _intMaxKnowledgeSkillRatingCreate.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxskillrating />
                        await objWriter.WriteElementStringAsync("maxskillrating",
                                _intMaxSkillRating.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxknowledgeskillrating />
                        await objWriter.WriteElementStringAsync("maxknowledgeskillrating",
                                _intMaxKnowledgeSkillRating.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);

                        // <dicepenaltysustaining />
                        await objWriter.WriteElementStringAsync("dicepenaltysustaining",
                                _intDicePenaltySustaining.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);

                        // <mininitiativedice />
                        await objWriter.WriteElementStringAsync("mininitiativedice",
                                _intMinInitiativeDice.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxinitiativedice />
                        await objWriter.WriteElementStringAsync("maxinitiativedice",
                                _intMaxInitiativeDice.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <minastralinitiativedice />
                        await objWriter.WriteElementStringAsync("minastralinitiativedice",
                                _intMinAstralInitiativeDice.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxastralinitiativedice />
                        await objWriter.WriteElementStringAsync("maxastralinitiativedice",
                                _intMaxAstralInitiativeDice.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <mincoldsiminitiativedice />
                        await objWriter.WriteElementStringAsync("mincoldsiminitiativedice",
                                _intMinColdSimInitiativeDice.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxcoldsiminitiativedice />
                        await objWriter.WriteElementStringAsync("maxcoldsiminitiativedice",
                                _intMaxColdSimInitiativeDice.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <minhotsiminitiativedice />
                        await objWriter.WriteElementStringAsync("minhotsiminitiativedice",
                                _intMinHotSimInitiativeDice.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxhotsiminitiativedice />
                        await objWriter.WriteElementStringAsync("maxhotsiminitiativedice",
                                _intMaxHotSimInitiativeDice.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);

                        // <karmacost>
                        await objWriter.WriteStartElementAsync("karmacost", token: token).ConfigureAwait(false);
                        // <karmaattribute />
                        await objWriter.WriteElementStringAsync("karmaattribute",
                                _intKarmaAttribute.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaquality />
                        await objWriter.WriteElementStringAsync("karmaquality",
                                _intKarmaQuality.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaspecialization />
                        await objWriter.WriteElementStringAsync("karmaspecialization",
                                _intKarmaSpecialization.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaknospecialization />
                        await objWriter.WriteElementStringAsync("karmaknospecialization",
                                _intKarmaKnoSpecialization.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmanewknowledgeskill />
                        await objWriter.WriteElementStringAsync("karmanewknowledgeskill",
                                _intKarmaNewKnowledgeSkill.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmanewactiveskill />
                        await objWriter.WriteElementStringAsync("karmanewactiveskill",
                                _intKarmaNewActiveSkill.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmanewskillgroup />
                        await objWriter.WriteElementStringAsync("karmanewskillgroup",
                                _intKarmaNewSkillGroup.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaimproveknowledgeskill />
                        await objWriter.WriteElementStringAsync("karmaimproveknowledgeskill",
                                _intKarmaImproveKnowledgeSkill.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaimproveactiveskill />
                        await objWriter.WriteElementStringAsync("karmaimproveactiveskill",
                                _intKarmaImproveActiveSkill.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaimproveskillgroup />
                        await objWriter.WriteElementStringAsync("karmaimproveskillgroup",
                                _intKarmaImproveSkillGroup.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaspell />
                        await objWriter.WriteElementStringAsync("karmaspell",
                                _intKarmaSpell.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaenhancement />
                        await objWriter.WriteElementStringAsync("karmaenhancement",
                                _intKarmaEnhancement.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmanewcomplexform />
                        await objWriter.WriteElementStringAsync("karmanewcomplexform",
                                _intKarmaNewComplexForm.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmanewaiprogram />
                        await objWriter.WriteElementStringAsync("karmanewaiprogram",
                                _intKarmaNewAIProgram.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmanewaiadvancedprogram />
                        await objWriter.WriteElementStringAsync("karmanewaiadvancedprogram",
                                _intKarmaNewAIAdvancedProgram.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmacontact />
                        await objWriter.WriteElementStringAsync("karmacontact",
                                _intKarmaContact.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaenemy />
                        await objWriter.WriteElementStringAsync("karmaenemy",
                                _intKarmaEnemy.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmacarryover />
                        await objWriter.WriteElementStringAsync("karmacarryover",
                                _intKarmaCarryover.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaspirit />
                        await objWriter.WriteElementStringAsync("karmaspirit",
                                _intKarmaSpirit.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmamaneuver />
                        await objWriter.WriteElementStringAsync("karmatechnique",
                                _intKarmaTechnique.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmainitiation />
                        await objWriter.WriteElementStringAsync("karmainitiation",
                                _intKarmaInitiation.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmainitiationflat />
                        await objWriter.WriteElementStringAsync("karmainitiationflat",
                                _intKarmaInitiationFlat.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmametamagic />
                        await objWriter.WriteElementStringAsync("karmametamagic",
                                _intKarmaMetamagic.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmajoingroup />
                        await objWriter.WriteElementStringAsync("karmajoingroup",
                                _intKarmaJoinGroup.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaleavegroup />
                        await objWriter.WriteElementStringAsync("karmaleavegroup",
                                _intKarmaLeaveGroup.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaalchemicalfocus />
                        await objWriter.WriteElementStringAsync("karmaalchemicalfocus",
                                _intKarmaAlchemicalFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmabanishingfocus />
                        await objWriter.WriteElementStringAsync("karmabanishingfocus",
                                _intKarmaBanishingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmabindingfocus />
                        await objWriter.WriteElementStringAsync("karmabindingfocus",
                                _intKarmaBindingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmacenteringfocus />
                        await objWriter.WriteElementStringAsync("karmacenteringfocus",
                                _intKarmaCenteringFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmacounterspellingfocus />
                        await objWriter.WriteElementStringAsync("karmacounterspellingfocus",
                                _intKarmaCounterspellingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmadisenchantingfocus />
                        await objWriter.WriteElementStringAsync("karmadisenchantingfocus",
                                _intKarmaDisenchantingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaflexiblesignaturefocus />
                        await objWriter.WriteElementStringAsync("karmaflexiblesignaturefocus",
                                _intKarmaFlexibleSignatureFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmamaskingfocus />
                        await objWriter.WriteElementStringAsync("karmamaskingfocus",
                                _intKarmaMaskingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmapowerfocus />
                        await objWriter.WriteElementStringAsync("karmapowerfocus",
                                _intKarmaPowerFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaqifocus />
                        await objWriter.WriteElementStringAsync("karmaqifocus",
                                _intKarmaQiFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaritualspellcastingfocus />
                        await objWriter.WriteElementStringAsync("karmaritualspellcastingfocus",
                                _intKarmaRitualSpellcastingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaspellcastingfocus />
                        await objWriter.WriteElementStringAsync("karmaspellcastingfocus",
                                _intKarmaSpellcastingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaspellshapingfocus />
                        await objWriter.WriteElementStringAsync("karmaspellshapingfocus",
                                _intKarmaSpellShapingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmasummoningfocus />
                        await objWriter.WriteElementStringAsync("karmasummoningfocus",
                                _intKarmaSummoningFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmasustainingfocus />
                        await objWriter.WriteElementStringAsync("karmasustainingfocus",
                                _intKarmaSustainingFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaweaponfocus />
                        await objWriter.WriteElementStringAsync("karmaweaponfocus",
                                _intKarmaWeaponFocus.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaweaponfocus />
                        await objWriter.WriteElementStringAsync("karmamysadpp",
                                _intKarmaMysticAdeptPowerPoint.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <karmaspiritfettering />
                        await objWriter.WriteElementStringAsync("karmaspiritfettering",
                                _intKarmaSpiritFettering.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // </karmacost>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                        XPathNodeIterator lstAllowedBooksCodes = (await XmlManager
                                .LoadXPathAsync("books.xml",
                                    await GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false),
                                    token: token).ConfigureAwait(false))
                            .SelectAndCacheExpression(
                                "/chummer/books/book[not(hide)]/code", token);
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                   out HashSet<string> setAllowedBooks))
                        {
                            foreach (XPathNavigator objAllowedBook in lstAllowedBooksCodes)
                            {
                                if (_setBooks.Contains(objAllowedBook.Value))
                                    setAllowedBooks.Add(objAllowedBook.Value);
                            }

                            // <books>
                            await objWriter.WriteStartElementAsync("books", token: token).ConfigureAwait(false);
                            foreach (string strBook in setAllowedBooks)
                                await objWriter.WriteElementStringAsync("book", strBook, token: token)
                                    .ConfigureAwait(false);
                            // </books>
                        }

                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                        string strCustomDataRootPath = Path.Combine(Utils.GetStartupPath, "customdata");

                        // <customdatadirectorynames>
                        await objWriter.WriteStartElementAsync("customdatadirectorynames", token: token)
                            .ConfigureAwait(false);
                        int i = -1;
                        await _dicCustomDataDirectoryKeys.ForEachAsync(async kvpDirectoryInfo =>
                        {
                            string strDirectoryName = kvpDirectoryInfo.Key;
                            bool blnDirectoryIsEnabled = kvpDirectoryInfo.Value;
                            if (!blnDirectoryIsEnabled && GlobalSettings.CustomDataDirectoryInfos.Any(
                                    x => x.DirectoryPath.StartsWith(strCustomDataRootPath, StringComparison.Ordinal)
                                         && x.CharacterSettingsSaveKey.Equals(
                                             strDirectoryName, StringComparison.OrdinalIgnoreCase)))
                                return; // Do not save disabled custom data directories that are in the customdata folder and would be auto-populated anyway
                            // ReSharper disable AccessToDisposedClosure
                            await objWriter.WriteStartElementAsync("customdatadirectoryname", token: token)
                                .ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync("directoryname", strDirectoryName, token: token)
                                .ConfigureAwait(false);
                            await objWriter
                                .WriteElementStringAsync("order",
                                    Interlocked.Increment(ref i).ToString(GlobalSettings.InvariantCultureInfo),
                                    token: token).ConfigureAwait(false);
                            await objWriter.WriteElementStringAsync(
                                "enabled", blnDirectoryIsEnabled.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token).ConfigureAwait(false);
                            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
                            // ReSharper restore AccessToDisposedClosure
                        }, token).ConfigureAwait(false);

                        // </customdatadirectorynames>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                        // <buildmethod />
                        await objWriter.WriteElementStringAsync("buildmethod", _eBuildMethod.ToString(), token: token)
                            .ConfigureAwait(false);
                        // <buildpoints />
                        await objWriter.WriteElementStringAsync("buildpoints",
                                _intBuildPoints.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <qualitykarmalimit />
                        await objWriter.WriteElementStringAsync("qualitykarmalimit",
                                _intQualityKarmaLimit.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <priorityarray />
                        await objWriter.WriteElementStringAsync("priorityarray", _strPriorityArray, token: token)
                            .ConfigureAwait(false);
                        // <prioritytable />
                        await objWriter.WriteElementStringAsync("prioritytable", _strPriorityTable, token: token)
                            .ConfigureAwait(false);
                        // <sumtoten />
                        await objWriter.WriteElementStringAsync(
                                "sumtoten", _intSumtoTen.ToString(GlobalSettings.InvariantCultureInfo),
                                token: token)
                            .ConfigureAwait(false);
                        // <availability />
                        await objWriter.WriteElementStringAsync("availability",
                                _intAvailability.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxmartialarts />
                        await objWriter.WriteElementStringAsync("maxmartialarts",
                                _intMaxMartialArts.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <maxmartialtechniques />
                        await objWriter.WriteElementStringAsync("maxmartialtechniques",
                                _intMaxMartialTechniques.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <nuyencarryover />
                        await objWriter.WriteElementStringAsync("nuyencarryover",
                                _decNuyenCarryover.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);
                        // <nuyenmaxbp />
                        await objWriter.WriteElementStringAsync("nuyenmaxbp",
                                _decNuyenMaximumBP.ToString(
                                    GlobalSettings.InvariantCultureInfo), token: token)
                            .ConfigureAwait(false);

                        // <bannedwaregrades>
                        await objWriter.WriteStartElementAsync("bannedwaregrades", token: token).ConfigureAwait(false);
                        foreach (string strGrade in _setBannedWareGrades)
                        {
                            await objWriter.WriteElementStringAsync("grade", strGrade, token: token)
                                .ConfigureAwait(false);
                        }

                        // </bannedwaregrades>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                        // <redlinerexclusion>
                        await objWriter.WriteStartElementAsync("redlinerexclusion", token: token).ConfigureAwait(false);
                        foreach (string strLimb in _setRedlinerExcludes)
                        {
                            await objWriter.WriteElementStringAsync("limb", strLimb, token: token)
                                .ConfigureAwait(false);
                        }

                        // </redlinerexclusion>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                        // </settings>
                        await objWriter.WriteEndElementAsync().ConfigureAwait(false);

                        await objWriter.WriteEndDocumentAsync().ConfigureAwait(false);
                    }
                }

                if (blnClearSourceGuid)
                    _guiSourceId = Guid.Empty;
                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Load the settings from the settings file.
        /// </summary>
        /// <param name="strFileName">Settings file to load from.</param>
        /// <param name="blnShowDialogs">Whether to show message boxes on failures to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public bool Load(string strFileName, bool blnShowDialogs = true, CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                _strFileName = strFileName;
                string strFilePath = Path.Combine(Utils.GetSettingsFolderPath, _strFileName);
                XPathDocument objXmlDocument;
                // Make sure the settings file exists. If not, ask the user if they would like to use the default settings file instead. A character cannot be loaded without a settings file.
                if (File.Exists(strFilePath))
                {
                    try
                    {
                        objXmlDocument = XPathDocumentExtensions.LoadStandardFromFile(strFilePath, token: token);
                    }
                    catch (IOException)
                    {
                        if (blnShowDialogs)
                            Program.ShowScrollableMessageBox(
                                LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter", token: token),
                                LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter", token: token),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        return false;
                    }
                    catch (XmlException)
                    {
                        if (blnShowDialogs)
                            Program.ShowScrollableMessageBox(
                                LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter", token: token),
                                LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter", token: token),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    if (blnShowDialogs)
                        Program.ShowScrollableMessageBox(
                            LanguageManager.GetString("Message_CharacterOptions_CannotLoadCharacter", token: token),
                            LanguageManager.GetString("MessageText_CharacterOptions_CannotLoadCharacter", token: token),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    return false;
                }

                return Load(objXmlDocument.CreateNavigator().SelectSingleNodeAndCacheExpression(".//settings", token), token);
            }
        }

        /// <summary>
        /// Load the settings from a settings node.
        /// </summary>
        /// <param name="objXmlNode">Settings node to load from.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public bool Load(XPathNavigator objXmlNode, CancellationToken token = default)
        {
            if (objXmlNode == null)
                return false;
            string strTemp = string.Empty;
            // Setting id.
            string strId = string.Empty;
            using (LockObject.EnterWriteLock(token))
            {
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
                objXmlNode.TryGetBoolFieldQuickly("unarmedimprovementsapplytoweapons",
                                                  ref _blnUnarmedImprovementsApplyToWeapons);
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
                    _strContactPointsExpression
                        = strTemp + " * " + intTemp.ToString(GlobalSettings.InvariantCultureInfo);
                }

                // XPath expression for knowledge points
                if (!objXmlNode.TryGetStringFieldQuickly("knowledgepointsexpression",
                                                         ref _strKnowledgePointsExpression))
                {
                    // Legacy shim
                    int intTemp = 2;
                    bool blnTemp = false;
                    strTemp = "({INTUnaug} + {LOGUnaug})";
                    if (objXmlNode.TryGetBoolFieldQuickly("usetotalvalueforknowledge", ref blnTemp) && blnTemp)
                        strTemp = "({INT} + {LOG})";
                    if (objXmlNode.TryGetBoolFieldQuickly("freekarmaknowledgemultiplierenabled", ref blnTemp)
                        && blnTemp)
                        objXmlNode.TryGetInt32FieldQuickly("freekarmaknowledgemultiplier", ref intTemp);
                    _strKnowledgePointsExpression
                        = strTemp + " * " + intTemp.ToString(GlobalSettings.InvariantCultureInfo);
                }

                // XPath expression for nuyen at chargen
                if (!objXmlNode.TryGetStringFieldQuickly("chargenkarmatonuyenexpression",
                                                         ref _strChargenKarmaToNuyenExpression))
                {
                    // Legacy shim
                    _strChargenKarmaToNuyenExpression = "{Karma} * "
                                                        + _decNuyenPerBPWftM.ToString(
                                                            GlobalSettings.InvariantCultureInfo) + " + {PriorityNuyen}";
                }
                // A very hacky legacy shim, but also works as a bit of a sanity check
                else if (!_strChargenKarmaToNuyenExpression.Contains("{PriorityNuyen}"))
                {
                    _strChargenKarmaToNuyenExpression = '(' + _strChargenKarmaToNuyenExpression + ") + {PriorityNuyen}";
                }

                // Various expressions used to determine certain character stats
                objXmlNode.TryGetStringFieldQuickly("compiledspriteexpression", ref _strRegisteredSpriteExpression);
                objXmlNode.TryGetStringFieldQuickly("boundspiritexpression", ref _strBoundSpiritExpression);
                objXmlNode.TryGetStringFieldQuickly("essencemodifierpostexpression", ref _strEssenceModifierPostExpression);
                objXmlNode.TryGetStringFieldQuickly("liftlimitexpression", ref _strLiftLimitExpression);
                objXmlNode.TryGetStringFieldQuickly("carrylimitexpression", ref _strCarryLimitExpression);
                objXmlNode.TryGetStringFieldQuickly("encumbranceintervalexpression",
                                                    ref _strEncumbranceIntervalExpression);
                // Whether to apply certain penalties to encumbrance and, if so, how much per tick
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltyphysicallimit",
                                                  ref _blnDoEncumbrancePenaltyPhysicalLimit);
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltymovementspeed",
                                                  ref _blnDoEncumbrancePenaltyMovementSpeed);
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltyagility", ref _blnDoEncumbrancePenaltyAgility);
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltyreaction", ref _blnDoEncumbrancePenaltyReaction);
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltywoundmodifier",
                                                  ref _blnDoEncumbrancePenaltyWoundModifier);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltyphysicallimit",
                                                   ref _intEncumbrancePenaltyPhysicalLimit);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltymovementspeed",
                                                   ref _intEncumbrancePenaltyMovementSpeed);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltyagility", ref _intEncumbrancePenaltyAgility);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltyreaction", ref _intEncumbrancePenaltyReaction);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltywoundmodifier",
                                                   ref _intEncumbrancePenaltyWoundModifier);
                // Drone Armor Multiplier Enabled
                objXmlNode.TryGetBoolFieldQuickly("dronearmormultiplierenabled", ref _blnDroneArmorMultiplierEnabled);
                // Drone Armor Multiplier Value
                objXmlNode.TryGetInt32FieldQuickly("dronearmorflatnumber", ref _intDroneArmorMultiplier);
                // No Single Armor Encumbrance
                objXmlNode.TryGetBoolFieldQuickly("nosinglearmorencumbrance", ref _blnNoSingleArmorEncumbrance);
                // Ignore Armor Encumbrance
                objXmlNode.TryGetBoolFieldQuickly("noarmorencumbrance", ref _blnNoArmorEncumbrance);
                // Do not cap armor bonuses from accessories
                objXmlNode.TryGetBoolFieldQuickly("uncappedarmoraccessorybonuses",
                                                  ref _blnUncappedArmorAccessoryBonuses);
                // Ignore Complex Form Limit
                objXmlNode.TryGetBoolFieldQuickly("ignorecomplexformlimit", ref _blnIgnoreComplexFormLimit);
                // Essence Loss Reduces Maximum Only.
                objXmlNode.TryGetBoolFieldQuickly("esslossreducesmaximumonly", ref _blnESSLossReducesMaximumOnly);
                // Allow Skill Regrouping.
                objXmlNode.TryGetBoolFieldQuickly("allowskillregrouping", ref _blnAllowSkillRegrouping);
                // Whether skill specializations break skill groups.
                objXmlNode.TryGetBoolFieldQuickly("specializationsbreakskillgroups",
                                                  ref _blnSpecializationsBreakSkillGroups);
                // Metatype Costs Karma.
                objXmlNode.TryGetBoolFieldQuickly("metatypecostskarma", ref _blnMetatypeCostsKarma);
                // Allow characters to spend karma before attribute points.
                objXmlNode.TryGetBoolFieldQuickly("reverseattributepriorityorder",
                                                  ref _blnReverseAttributePriorityOrder);
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
                // Whether Karma costs for increasing Special Attributes is based on the shown value instead of actual value.
                objXmlNode.TryGetBoolFieldQuickly("specialkarmacostbasedonshownvalue",
                                                  ref _blnSpecialKarmaCostBasedOnShownValue);
                // Allow more than 35 BP in Positive Qualities.
                objXmlNode.TryGetBoolFieldQuickly("exceedpositivequalities", ref _blnExceedPositiveQualities);
                // Double all positive qualities in excess of the limit
                objXmlNode.TryGetBoolFieldQuickly("exceedpositivequalitiescostdoubled",
                                                  ref _blnExceedPositiveQualitiesCostDoubled);

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
                if (!objXmlNode.TryGetBoolFieldQuickly("exceednegativequalitiesnobonus", ref _blnExceedNegativeQualitiesNoBonus))
                    objXmlNode.TryGetBoolFieldQuickly("exceednegativequalitieslimit", ref _blnExceedNegativeQualitiesNoBonus);
                // Whether Restricted items have their cost multiplied.
                objXmlNode.TryGetBoolFieldQuickly("multiplyrestrictedcost", ref _blnMultiplyRestrictedCost);
                // Whether Forbidden items have their cost multiplied.
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
                // Format in which weight values are displayed
                if (objXmlNode.TryGetStringFieldQuickly("weightformat", ref _strWeightFormat))
                {
                    int intDecimalPlaces = _strWeightFormat.IndexOf('.');
                    if (intDecimalPlaces == -1)
                        _strWeightFormat += ".###";
                }

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
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdZeros))
                            {
                                for (int i = _strEssenceFormat.Length - 1 - intDecimalPlaces; i < intDecimalPlaces; ++i)
                                    sbdZeros.Append('0');
                                _strEssenceFormat += sbdZeros.ToString();
                            }
                        }
                    }
                }

                // Whether Capacity limits should be enforced.
                objXmlNode.TryGetBoolFieldQuickly("enforcecapacity", ref _blnEnforceCapacity);
                // Whether Recoil modifiers are restricted (AR 148).
                objXmlNode.TryGetBoolFieldQuickly("restrictrecoil", ref _blnRestrictRecoil);
                // Whether character are not restricted to the number of points they can invest in Nuyen.
                objXmlNode.TryGetBoolFieldQuickly("unrestrictednuyen", ref _blnUnrestrictedNuyen);
                // Whether Stacked Foci can go a combined Force higher than 6.
                objXmlNode.TryGetBoolFieldQuickly("allowhigherstackedfoci", ref _blnAllowHigherStackedFoci);
                // Whether the user can change the status of a Weapon Mod or Accessory being part of the base Weapon.
                objXmlNode.TryGetBoolFieldQuickly("alloweditpartofbaseweapon", ref _blnAllowEditPartOfBaseWeapon);
                // Whether the user can break Skill Groups while in Create Mode.
                objXmlNode.TryGetBoolFieldQuickly("breakskillgroupsincreatemode",
                                                  ref _blnStrictSkillGroupsInCreateMode);
                // Whether the user is allowed to buy specializations with skill points for skills only bought with karma.
                objXmlNode.TryGetBoolFieldQuickly("allowpointbuyspecializationsonkarmaskills",
                                                  ref _blnAllowPointBuySpecializationsOnKarmaSkills);
                // Whether any Detection Spell can be taken as Extended range version.
                objXmlNode.TryGetBoolFieldQuickly("extendanydetectionspell", ref _blnExtendAnyDetectionSpell);
                // Whether cyberlimbs are used for augmented attribute calculation.
                objXmlNode.TryGetBoolFieldQuickly("dontusecyberlimbcalculation", ref _blnDontUseCyberlimbCalculation);
                // House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
                objXmlNode.TryGetBoolFieldQuickly("alternatemetatypeattributekarma",
                                                  ref _blnAlternateMetatypeAttributeKarma);
                // Whether Bioware Suites can be created and added.
                objXmlNode.TryGetBoolFieldQuickly("allowbiowaresuites", ref _blnAllowBiowareSuites);
                // House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
                objXmlNode.TryGetBoolFieldQuickly("freespiritpowerpointsmag", ref _blnFreeSpiritPowerPointsMAG);
                // House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
                objXmlNode.TryGetBoolFieldQuickly("compensateskillgroupkarmadifference",
                                                  ref _blnCompensateSkillGroupKarmaDifference);
                // Optional Rule: Whether Life Modules should automatically create a character back story.
                objXmlNode.TryGetBoolFieldQuickly("autobackstory", ref _blnAutomaticBackstory);
                // House Rule: Whether Public Awareness should be a calculated attribute based on Street Cred and Notoriety.
                objXmlNode.TryGetBoolFieldQuickly("usecalculatedpublicawareness", ref _blnUseCalculatedPublicAwareness);
                // House Rule: Whether Improved Ability should be capped at 0.5 (false) or 1.5 (true) of the target skill's Learned Rating.
                objXmlNode.TryGetBoolFieldQuickly("increasedimprovedabilitymodifier",
                                                  ref _blnIncreasedImprovedAbilityMultiplier);
                // House Rule: Whether lifestyles will give free grid subscriptions found in HT to players.
                objXmlNode.TryGetBoolFieldQuickly("allowfreegrids", ref _blnAllowFreeGrids);
                // House Rule: Whether Technomancers should be allowed to receive Schooling discounts in the same manner as Awakened.
                objXmlNode.TryGetBoolFieldQuickly("allowtechnomancerschooling", ref _blnAllowTechnomancerSchooling);
                // House Rule: Maximum value that cyberlimbs can have as a bonus on top of their Customization.
                objXmlNode.TryGetInt32FieldQuickly("cyberlimbattributebonuscap", ref _intCyberlimbAttributeBonusCap);
                if (!objXmlNode.TryGetBoolFieldQuickly("cyberlimbattributebonuscapoverride",
                                                       ref _blnCyberlimbAttributeBonusCapOverride))
                    _blnCyberlimbAttributeBonusCapOverride = _intCyberlimbAttributeBonusCap == 4;
                // House/Optional Rule: Attribute values are allowed to go below 0 due to Essence Loss.
                objXmlNode.TryGetBoolFieldQuickly("unclampattributeminimum", ref _blnUnclampAttributeMinimum);
                // Following two settings used to be stored in global options, so they are fetched from the registry if they are not present
                // Use Rigger 5.0 drone mods
                if (!objXmlNode.TryGetBoolFieldQuickly("dronemods", ref _blnDroneMods))
                    GlobalSettings.LoadBoolFromRegistry(ref _blnDroneMods, "dronemods", string.Empty, true);
                // Apply maximum drone attribute improvement rule to Pilot, too
                if (!objXmlNode.TryGetBoolFieldQuickly("dronemodsmaximumpilot", ref _blnDroneModsMaximumPilot))
                    GlobalSettings.LoadBoolFromRegistry(ref _blnDroneModsMaximumPilot, "dronemodsPilot", string.Empty,
                                                        true);

                // Maximum number of attributes at metatype maximum in character creation
                if (!objXmlNode.TryGetInt32FieldQuickly("maxnumbermaxattributescreate",
                                                        ref _intMaxNumberMaxAttributesCreate))
                {
                    // Legacy shim
                    bool blnTemp = false;
                    if (objXmlNode.TryGetBoolFieldQuickly("allow2ndmaxattribute", ref blnTemp) && blnTemp)
                        _intMaxNumberMaxAttributesCreate = 2;
                }

                // Maximum skill rating in character creation
                objXmlNode.TryGetInt32FieldQuickly("maxskillratingcreate", ref _intMaxSkillRatingCreate);
                // Maximum knowledge skill rating in character creation
                objXmlNode.TryGetInt32FieldQuickly("maxknowledgeskillratingcreate",
                                                   ref _intMaxKnowledgeSkillRatingCreate);
                // Maximum skill rating
                if (objXmlNode.TryGetInt32FieldQuickly("maxskillrating", ref _intMaxSkillRating)
                    && _intMaxSkillRatingCreate > _intMaxSkillRating)
                    _intMaxSkillRatingCreate = _intMaxSkillRating;
                // Maximum knowledge skill rating
                if (objXmlNode.TryGetInt32FieldQuickly("maxknowledgeskillrating", ref _intMaxKnowledgeSkillRating)
                    && _intMaxKnowledgeSkillRatingCreate > _intMaxKnowledgeSkillRating)
                    _intMaxKnowledgeSkillRatingCreate = _intMaxKnowledgeSkillRating;

                //House Rule: The DicePenalty per sustained spell or form
                objXmlNode.TryGetInt32FieldQuickly("dicepenaltysustaining", ref _intDicePenaltySustaining);

                // Initiative dice
                objXmlNode.TryGetInt32FieldQuickly("mininitiativedice", ref _intMinInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("maxinitiativedice", ref _intMaxInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("minastralinitiativedice", ref _intMinAstralInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("maxastralinitiativedice", ref _intMaxAstralInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("mincoldsiminitiativedice", ref _intMinColdSimInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("maxcoldsiminitiativedice", ref _intMaxColdSimInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("minhotsiminitiativedice", ref _intMinHotSimInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("maxhotsiminitiativedice", ref _intMaxHotSimInitiativeDice);

                XPathNavigator xmlKarmaCostNode = objXmlNode.SelectSingleNodeAndCacheExpression("karmacost", token);
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
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveknowledgeskill",
                                                             ref _intKarmaImproveKnowledgeSkill);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveactiveskill",
                                                             ref _intKarmaImproveActiveSkill);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveskillgroup", ref _intKarmaImproveSkillGroup);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspell", ref _intKarmaSpell);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewcomplexform", ref _intKarmaNewComplexForm);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewaiprogram", ref _intKarmaNewAIProgram);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewaiadvancedprogram",
                                                             ref _intKarmaNewAIAdvancedProgram);
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
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmacounterspellingfocus",
                                                             ref _intKarmaCounterspellingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmadisenchantingfocus",
                                                             ref _intKarmaDisenchantingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaflexiblesignaturefocus",
                                                             ref _intKarmaFlexibleSignatureFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmamaskingfocus", ref _intKarmaMaskingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmapowerfocus", ref _intKarmaPowerFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaqifocus", ref _intKarmaQiFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaritualspellcastingfocus",
                                                             ref _intKarmaRitualSpellcastingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspellcastingfocus", ref _intKarmaSpellcastingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspellshapingfocus", ref _intKarmaSpellShapingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmasummoningfocus", ref _intKarmaSummoningFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmasustainingfocus", ref _intKarmaSustainingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaweaponfocus", ref _intKarmaWeaponFocus);
                }

                XPathNavigator xmlLegacyCharacterNavigator = null;
                // Legacy sweep by looking at MRU
                if (!BuiltInOption && objXmlNode.SelectSingleNodeAndCacheExpression("books/book", token) == null
                                   && objXmlNode.SelectSingleNodeAndCacheExpression(
                                       "customdatadirectorynames/directoryname", token) == null)
                {
                    foreach (string strMruCharacterFile in GlobalSettings.MostRecentlyUsedCharacters)
                    {
                        token.ThrowIfCancellationRequested();
                        XPathDocument objXmlDocument;
                        if (!File.Exists(strMruCharacterFile))
                            continue;
                        try
                        {
                            objXmlDocument = XPathDocumentExtensions.LoadStandardFromFilePatient(strMruCharacterFile, token: token);
                        }
                        catch (XmlException)
                        {
                            continue;
                        }

                        xmlLegacyCharacterNavigator = objXmlDocument.CreateNavigator()
                                                                    .SelectSingleNodeAndCacheExpression("/character", token);

                        if (xmlLegacyCharacterNavigator == null)
                            continue;

                        string strLoopSettingsFile = xmlLegacyCharacterNavigator
                                                     .SelectSingleNodeAndCacheExpression("settings", token)?.Value;
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
                                objXmlDocument = XPathDocumentExtensions.LoadStandardFromFilePatient(strMruCharacterFile, token: token);
                            }
                            catch (XmlException)
                            {
                                continue;
                            }

                            xmlLegacyCharacterNavigator = objXmlDocument.CreateNavigator()
                                                                        .SelectSingleNodeAndCacheExpression(
                                                                            "/character", token);

                            if (xmlLegacyCharacterNavigator == null)
                                continue;

                            string strLoopSettingsFile = xmlLegacyCharacterNavigator
                                                         .SelectSingleNodeAndCacheExpression("settings", token)?.Value;
                            if (strLoopSettingsFile == _strFileName)
                                break;
                            xmlLegacyCharacterNavigator = null;
                        }
                    }
                }

                // Load Books.
                _setBooks.Clear();
                foreach (XPathNavigator xmlBook in objXmlNode.SelectAndCacheExpression("books/book", token))
                    _setBooks.Add(xmlBook.Value);
                // Legacy sweep for sourcebooks
                if (xmlLegacyCharacterNavigator != null)
                {
                    foreach (XPathNavigator xmlBook in xmlLegacyCharacterNavigator.SelectAndCacheExpression(
                                 "sources/source", token))
                    {
                        if (!string.IsNullOrEmpty(xmlBook.Value))
                            _setBooks.Add(xmlBook.Value);
                    }
                }

                // Load Custom Data Directory names.
                int intTopMostOrder = 0;
                int intBottomMostOrder = 0;
                Dictionary<int, Tuple<string, bool>> dicLoadingCustomDataDirectories =
                    new Dictionary<int, Tuple<string, bool>>(GlobalSettings.CustomDataDirectoryInfos.Count);
                bool blnNeedToProcessInfosWithoutLoadOrder = false;
                foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression(
                             "customdatadirectorynames/customdatadirectoryname", token))
                {
                    string strDirectoryKey
                        = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("directoryname", token)?.Value;
                    if (string.IsNullOrEmpty(strDirectoryKey))
                        continue;
                    string strLoopId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                    // Only load in directories that are either present in our GlobalSettings or are enabled
                    bool blnLoopEnabled = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("enabled", token)?.Value
                                          == bool.TrueString;
                    if (blnLoopEnabled || (string.IsNullOrEmpty(strLoopId)
                            ? GlobalSettings.CustomDataDirectoryInfos.Any(
                                x => x.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                            : GlobalSettings.CustomDataDirectoryInfos.Any(
                                x => x.InternalId.Equals(strLoopId, StringComparison.OrdinalIgnoreCase))))
                    {
                        string strOrder = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("order", token)?.Value;
                        if (!string.IsNullOrEmpty(strOrder)
                            && int.TryParse(strOrder, NumberStyles.Integer, GlobalSettings.InvariantCultureInfo,
                                            out int intOrder))
                        {
                            while (dicLoadingCustomDataDirectories.ContainsKey(intOrder))
                                ++intOrder;
                            intTopMostOrder = Math.Max(intOrder, intTopMostOrder);
                            intBottomMostOrder = Math.Min(intOrder, intBottomMostOrder);
                            dicLoadingCustomDataDirectories.Add(intOrder,
                                                                new Tuple<string, bool>(
                                                                    strDirectoryKey, blnLoopEnabled));
                        }
                        else
                            blnNeedToProcessInfosWithoutLoadOrder = true;
                    }
                }

                using (_dicCustomDataDirectoryKeys.LockObject.EnterWriteLock(token))
                {
                    _dicCustomDataDirectoryKeys.Clear();
                    for (int i = intBottomMostOrder; i <= intTopMostOrder; ++i)
                    {
                        if (!dicLoadingCustomDataDirectories.TryGetValue(i, out Tuple<string, bool> tupLoop))
                            continue;
                        string strDirectoryKey = tupLoop.Item1;
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

                        _dicCustomDataDirectoryKeys.TryAdd(strDirectoryKey, tupLoop.Item2, token);
                    }

                    // Legacy sweep for custom data directories
                    if (xmlLegacyCharacterNavigator != null)
                    {
                        foreach (XPathNavigator xmlCustomDataDirectoryName in xmlLegacyCharacterNavigator
                                     .SelectAndCacheExpression("customdatadirectorynames/directoryname", token))
                        {
                            string strDirectoryKey = xmlCustomDataDirectoryName.Value;
                            if (string.IsNullOrEmpty(strDirectoryKey))
                                continue;
                            string strLoopId
                                = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
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

                            _dicCustomDataDirectoryKeys.TryAdd(strDirectoryKey, true, token);
                        }
                    }

                    // Add in the stragglers that didn't have any load order info
                    if (blnNeedToProcessInfosWithoutLoadOrder)
                    {
                        foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression(
                                     "customdatadirectorynames/customdatadirectoryname", token))
                        {
                            string strDirectoryKey = objXmlDirectoryName
                                                     .SelectSingleNodeAndCacheExpression("directoryname", token)
                                                     ?.Value;
                            if (string.IsNullOrEmpty(strDirectoryKey))
                                continue;
                            string strLoopId
                                = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                            string strOrder = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("order", token)?.Value;
                            if (!string.IsNullOrEmpty(strOrder) && int.TryParse(strOrder, NumberStyles.Integer,
                                    GlobalSettings.InvariantCultureInfo,
                                    out int _))
                                continue;
                            // Only load in directories that are either present in our GlobalSettings or are enabled
                            bool blnLoopEnabled
                                = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("enabled", token)?.Value
                                  == bool.TrueString;
                            if (blnLoopEnabled || (string.IsNullOrEmpty(strLoopId)
                                    ? GlobalSettings.CustomDataDirectoryInfos.Any(
                                        x => x.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                                    : GlobalSettings.CustomDataDirectoryInfos.Any(
                                        x => x.InternalId.Equals(strLoopId, StringComparison.OrdinalIgnoreCase))))
                            {
                                if (string.IsNullOrEmpty(strLoopId))
                                {
                                    CustomDataDirectoryInfo objExistingInfo = null;
                                    foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings
                                                 .CustomDataDirectoryInfos)
                                    {
                                        if (!objLoopInfo.Name.Equals(strDirectoryKey,
                                                                     StringComparison.OrdinalIgnoreCase))
                                            continue;
                                        if (objExistingInfo == null
                                            || objLoopInfo.MyVersion > objExistingInfo.MyVersion)
                                            objExistingInfo = objLoopInfo;
                                    }

                                    if (objExistingInfo != null)
                                        strDirectoryKey = objExistingInfo.InternalId;
                                }

                                _dicCustomDataDirectoryKeys.TryAdd(strDirectoryKey, blnLoopEnabled, token);
                            }
                        }
                    }

                    if (_dicCustomDataDirectoryKeys.Count == 0)
                    {
                        foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression(
                                     "customdatadirectorynames/directoryname", token))
                        {
                            string strDirectoryKey = objXmlDirectoryName.Value;
                            if (string.IsNullOrEmpty(strDirectoryKey))
                                continue;
                            string strLoopId
                                = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
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

                            _dicCustomDataDirectoryKeys.TryAdd(strDirectoryKey, true, token);
                        }
                    }

                    // Add in any directories that are in GlobalSettings but are not present in the settings so that we may enable them if we want to
                    foreach (string strCharacterSettingsSaveKey in GlobalSettings.CustomDataDirectoryInfos.Select(
                                 x => x.CharacterSettingsSaveKey))
                    {
                        _dicCustomDataDirectoryKeys.TryAdd(strCharacterSettingsSaveKey, false, token);
                    }
                }

                RecalculateEnabledCustomDataDirectories(token);

                foreach (XPathNavigator xmlBook in XmlManager.LoadXPath("books.xml", EnabledCustomDataDirectoryPaths, token: token)
                                                             .SelectAndCacheExpression(
                                                                 "/chummer/books/book[permanent]/code", token))
                {
                    if (!string.IsNullOrEmpty(xmlBook.Value))
                        _setBooks.Add(xmlBook.Value);
                }

                RecalculateBookXPath(token);

                // Used to legacy sweep build settings.
                XPathNavigator xmlDefaultBuildNode = objXmlNode.SelectSingleNodeAndCacheExpression("defaultbuild", token);
                if (objXmlNode.TryGetStringFieldQuickly("buildmethod", ref strTemp)
                    && Enum.TryParse(strTemp, true, out CharacterBuildMethod eBuildMethod)
                    || xmlDefaultBuildNode?.TryGetStringFieldQuickly("buildmethod", ref strTemp) == true
                    && Enum.TryParse(strTemp, true, out eBuildMethod))
                    _eBuildMethod = eBuildMethod;
                if (!objXmlNode.TryGetInt32FieldQuickly("buildpoints", ref _intBuildPoints))
                    xmlDefaultBuildNode?.TryGetInt32FieldQuickly("buildpoints", ref _intBuildPoints);
                if (!objXmlNode.TryGetInt32FieldQuickly("qualitykarmalimit", ref _intQualityKarmaLimit)
                    && BuildMethodUsesPriorityTables)
                    _intQualityKarmaLimit = _intBuildPoints;
                objXmlNode.TryGetStringFieldQuickly("priorityarray", ref _strPriorityArray);
                objXmlNode.TryGetStringFieldQuickly("prioritytable", ref _strPriorityTable);
                objXmlNode.TryGetInt32FieldQuickly("sumtoten", ref _intSumtoTen);
                if (!objXmlNode.TryGetInt32FieldQuickly("availability", ref _intAvailability))
                    xmlDefaultBuildNode?.TryGetInt32FieldQuickly("availability", ref _intAvailability);
                if (!objXmlNode.TryGetInt32FieldQuickly("maxmartialarts", ref _intMaxMartialArts))
                    xmlDefaultBuildNode?.TryGetInt32FieldQuickly("maxmartialarts", ref _intMaxMartialArts);
                if (!objXmlNode.TryGetInt32FieldQuickly("maxmartialtechniques", ref _intMaxMartialTechniques))
                    xmlDefaultBuildNode?.TryGetInt32FieldQuickly("maxmartialtechniques", ref _intMaxMartialTechniques);
                objXmlNode.TryGetDecFieldQuickly("nuyencarryover", ref _decNuyenCarryover);
                objXmlNode.TryGetDecFieldQuickly("nuyenmaxbp", ref _decNuyenMaximumBP);

                _setBannedWareGrades.Clear();
                foreach (XPathNavigator xmlGrade in objXmlNode.SelectAndCacheExpression("bannedwaregrades/grade", token))
                    _setBannedWareGrades.Add(xmlGrade.Value);

                _setRedlinerExcludes.Clear();
                foreach (XPathNavigator xmlLimb in objXmlNode.SelectAndCacheExpression("redlinerexclusion/limb", token))
                    _setRedlinerExcludes.Add(xmlLimb.Value);

                return true;
            }
        }

        /// <summary>
        /// Load the settings from the settings file.
        /// </summary>
        /// <param name="strFileName">Settings file to load from.</param>
        /// <param name="blnShowDialogs">Whether to show message boxes on failures to load.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<bool> LoadAsync(string strFileName, bool blnShowDialogs = true, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _strFileName = strFileName;
                string strFilePath = Path.Combine(Utils.GetSettingsFolderPath, _strFileName);
                XPathDocument objXmlDocument;
                // Make sure the settings file exists. If not, ask the user if they would like to use the default settings file instead. A character cannot be loaded without a settings file.
                if (File.Exists(strFilePath))
                {
                    try
                    {
                        objXmlDocument
                            = await XPathDocumentExtensions.LoadStandardFromFileAsync(strFilePath, token: token).ConfigureAwait(false);
                    }
                    catch (IOException)
                    {
                        if (blnShowDialogs)
                            await Program.ShowScrollableMessageBoxAsync(
                                await LanguageManager.GetStringAsync("Message_CharacterOptions_CannotLoadCharacter", token: token).ConfigureAwait(false),
                                await LanguageManager.GetStringAsync("MessageText_CharacterOptions_CannotLoadCharacter", token: token).ConfigureAwait(false),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                        return false;
                    }
                    catch (XmlException)
                    {
                        if (blnShowDialogs)
                            await Program.ShowScrollableMessageBoxAsync(
                                await LanguageManager.GetStringAsync("Message_CharacterOptions_CannotLoadCharacter", token: token).ConfigureAwait(false),
                                await LanguageManager.GetStringAsync("MessageText_CharacterOptions_CannotLoadCharacter", token: token).ConfigureAwait(false),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                        return false;
                    }
                }
                else
                {
                    if (blnShowDialogs)
                        await Program.ShowScrollableMessageBoxAsync(
                            await LanguageManager.GetStringAsync("Message_CharacterOptions_CannotLoadCharacter", token: token).ConfigureAwait(false),
                            await LanguageManager.GetStringAsync("MessageText_CharacterOptions_CannotLoadCharacter", token: token).ConfigureAwait(false),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error, token: token).ConfigureAwait(false);
                    return false;
                }

                if (objXmlDocument == null)
                    return false;

                return await LoadAsync(objXmlDocument.CreateNavigator().SelectSingleNodeAndCacheExpression(".//settings", token), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Load the settings from a settings node.
        /// </summary>
        /// <param name="objXmlNode">Settings node to load from.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<bool> LoadAsync(XPathNavigator objXmlNode, CancellationToken token = default)
        {
            if (objXmlNode == null)
                return false;
            string strTemp = string.Empty;
            // Setting id.
            string strId = string.Empty;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
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
                objXmlNode.TryGetBoolFieldQuickly("unarmedimprovementsapplytoweapons",
                                                  ref _blnUnarmedImprovementsApplyToWeapons);
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
                    _strContactPointsExpression
                        = strTemp + " * " + intTemp.ToString(GlobalSettings.InvariantCultureInfo);
                }

                // XPath expression for knowledge points
                if (!objXmlNode.TryGetStringFieldQuickly("knowledgepointsexpression",
                                                         ref _strKnowledgePointsExpression))
                {
                    // Legacy shim
                    int intTemp = 2;
                    bool blnTemp = false;
                    strTemp = "({INTUnaug} + {LOGUnaug})";
                    if (objXmlNode.TryGetBoolFieldQuickly("usetotalvalueforknowledge", ref blnTemp) && blnTemp)
                        strTemp = "({INT} + {LOG})";
                    if (objXmlNode.TryGetBoolFieldQuickly("freekarmaknowledgemultiplierenabled", ref blnTemp)
                        && blnTemp)
                        objXmlNode.TryGetInt32FieldQuickly("freekarmaknowledgemultiplier", ref intTemp);
                    _strKnowledgePointsExpression
                        = strTemp + " * " + intTemp.ToString(GlobalSettings.InvariantCultureInfo);
                }

                // XPath expression for nuyen at chargen
                if (!objXmlNode.TryGetStringFieldQuickly("chargenkarmatonuyenexpression",
                                                         ref _strChargenKarmaToNuyenExpression))
                {
                    // Legacy shim
                    _strChargenKarmaToNuyenExpression = "{Karma} * "
                                                        + _decNuyenPerBPWftM.ToString(
                                                            GlobalSettings.InvariantCultureInfo) + " + {PriorityNuyen}";
                }
                // A very hacky legacy shim, but also works as a bit of a sanity check
                else if (!_strChargenKarmaToNuyenExpression.Contains("{PriorityNuyen}"))
                {
                    _strChargenKarmaToNuyenExpression = '(' + _strChargenKarmaToNuyenExpression + ") + {PriorityNuyen}";
                }

                // Various expressions used to determine certain character stats
                objXmlNode.TryGetStringFieldQuickly("compiledspriteexpression", ref _strRegisteredSpriteExpression);
                objXmlNode.TryGetStringFieldQuickly("boundspiritexpression", ref _strBoundSpiritExpression);
                objXmlNode.TryGetStringFieldQuickly("essencemodifierpostexpression", ref _strEssenceModifierPostExpression);
                objXmlNode.TryGetStringFieldQuickly("liftlimitexpression", ref _strLiftLimitExpression);
                objXmlNode.TryGetStringFieldQuickly("carrylimitexpression", ref _strCarryLimitExpression);
                objXmlNode.TryGetStringFieldQuickly("encumbranceintervalexpression",
                                                    ref _strEncumbranceIntervalExpression);
                // Whether to apply certain penalties to encumbrance and, if so, how much per tick
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltyphysicallimit",
                                                  ref _blnDoEncumbrancePenaltyPhysicalLimit);
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltymovementspeed",
                                                  ref _blnDoEncumbrancePenaltyMovementSpeed);
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltyagility", ref _blnDoEncumbrancePenaltyAgility);
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltyreaction", ref _blnDoEncumbrancePenaltyReaction);
                objXmlNode.TryGetBoolFieldQuickly("doencumbrancepenaltywoundmodifier",
                                                  ref _blnDoEncumbrancePenaltyWoundModifier);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltyphysicallimit",
                                                   ref _intEncumbrancePenaltyPhysicalLimit);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltymovementspeed",
                                                   ref _intEncumbrancePenaltyMovementSpeed);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltyagility", ref _intEncumbrancePenaltyAgility);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltyreaction", ref _intEncumbrancePenaltyReaction);
                objXmlNode.TryGetInt32FieldQuickly("encumbrancepenaltywoundmodifier",
                                                   ref _intEncumbrancePenaltyWoundModifier);
                // Drone Armor Multiplier Enabled
                objXmlNode.TryGetBoolFieldQuickly("dronearmormultiplierenabled", ref _blnDroneArmorMultiplierEnabled);
                // Drone Armor Multiplier Value
                objXmlNode.TryGetInt32FieldQuickly("dronearmorflatnumber", ref _intDroneArmorMultiplier);
                // No Single Armor Encumbrance
                objXmlNode.TryGetBoolFieldQuickly("nosinglearmorencumbrance", ref _blnNoSingleArmorEncumbrance);
                // Ignore Armor Encumbrance
                objXmlNode.TryGetBoolFieldQuickly("noarmorencumbrance", ref _blnNoArmorEncumbrance);
                // Do not cap armor bonuses from accessories
                objXmlNode.TryGetBoolFieldQuickly("uncappedarmoraccessorybonuses",
                                                  ref _blnUncappedArmorAccessoryBonuses);
                // Ignore Complex Form Limit
                objXmlNode.TryGetBoolFieldQuickly("ignorecomplexformlimit", ref _blnIgnoreComplexFormLimit);
                // Essence Loss Reduces Maximum Only.
                objXmlNode.TryGetBoolFieldQuickly("esslossreducesmaximumonly", ref _blnESSLossReducesMaximumOnly);
                // Allow Skill Regrouping.
                objXmlNode.TryGetBoolFieldQuickly("allowskillregrouping", ref _blnAllowSkillRegrouping);
                // Whether skill specializations break skill groups.
                objXmlNode.TryGetBoolFieldQuickly("specializationsbreakskillgroups",
                                                  ref _blnSpecializationsBreakSkillGroups);
                // Metatype Costs Karma.
                objXmlNode.TryGetBoolFieldQuickly("metatypecostskarma", ref _blnMetatypeCostsKarma);
                // Allow characters to spend karma before attribute points.
                objXmlNode.TryGetBoolFieldQuickly("reverseattributepriorityorder",
                                                  ref _blnReverseAttributePriorityOrder);
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
                // Whether Karma costs for increasing Special Attributes is based on the shown value instead of actual value.
                objXmlNode.TryGetBoolFieldQuickly("specialkarmacostbasedonshownvalue",
                                                  ref _blnSpecialKarmaCostBasedOnShownValue);
                // Allow more than 35 BP in Positive Qualities.
                objXmlNode.TryGetBoolFieldQuickly("exceedpositivequalities", ref _blnExceedPositiveQualities);
                // Double all positive qualities in excess of the limit
                objXmlNode.TryGetBoolFieldQuickly("exceedpositivequalitiescostdoubled",
                                                  ref _blnExceedPositiveQualitiesCostDoubled);

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
                if (!objXmlNode.TryGetBoolFieldQuickly("exceednegativequalitiesnobonus",
                                                       ref _blnExceedNegativeQualitiesNoBonus))
                    objXmlNode.TryGetBoolFieldQuickly("exceednegativequalitieslimit",
                                                      ref _blnExceedNegativeQualitiesNoBonus);
                // Whether Restricted items have their cost multiplied.
                objXmlNode.TryGetBoolFieldQuickly("multiplyrestrictedcost", ref _blnMultiplyRestrictedCost);
                // Whether Forbidden items have their cost multiplied.
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
                // Format in which weight values are displayed
                if (objXmlNode.TryGetStringFieldQuickly("weightformat", ref _strWeightFormat))
                {
                    int intDecimalPlaces = _strWeightFormat.IndexOf('.');
                    if (intDecimalPlaces == -1)
                        _strWeightFormat += ".###";
                }

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
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdZeros))
                            {
                                for (int i = _strEssenceFormat.Length - 1 - intDecimalPlaces; i < intDecimalPlaces; ++i)
                                    sbdZeros.Append('0');
                                _strEssenceFormat += sbdZeros.ToString();
                            }
                        }
                    }
                }

                // Whether Capacity limits should be enforced.
                objXmlNode.TryGetBoolFieldQuickly("enforcecapacity", ref _blnEnforceCapacity);
                // Whether Recoil modifiers are restricted (AR 148).
                objXmlNode.TryGetBoolFieldQuickly("restrictrecoil", ref _blnRestrictRecoil);
                // Whether character are not restricted to the number of points they can invest in Nuyen.
                objXmlNode.TryGetBoolFieldQuickly("unrestrictednuyen", ref _blnUnrestrictedNuyen);
                // Whether Stacked Foci can go a combined Force higher than 6.
                objXmlNode.TryGetBoolFieldQuickly("allowhigherstackedfoci", ref _blnAllowHigherStackedFoci);
                // Whether the user can change the status of a Weapon Mod or Accessory being part of the base Weapon.
                objXmlNode.TryGetBoolFieldQuickly("alloweditpartofbaseweapon", ref _blnAllowEditPartOfBaseWeapon);
                // Whether the user can break Skill Groups while in Create Mode.
                objXmlNode.TryGetBoolFieldQuickly("breakskillgroupsincreatemode",
                                                  ref _blnStrictSkillGroupsInCreateMode);
                // Whether the user is allowed to buy specializations with skill points for skills only bought with karma.
                objXmlNode.TryGetBoolFieldQuickly("allowpointbuyspecializationsonkarmaskills",
                                                  ref _blnAllowPointBuySpecializationsOnKarmaSkills);
                // Whether any Detection Spell can be taken as Extended range version.
                objXmlNode.TryGetBoolFieldQuickly("extendanydetectionspell", ref _blnExtendAnyDetectionSpell);
                // Whether cyberlimbs are used for augmented attribute calculation.
                objXmlNode.TryGetBoolFieldQuickly("dontusecyberlimbcalculation", ref _blnDontUseCyberlimbCalculation);
                // House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
                objXmlNode.TryGetBoolFieldQuickly("alternatemetatypeattributekarma",
                                                  ref _blnAlternateMetatypeAttributeKarma);
                // Whether Bioware Suites can be created and added.
                objXmlNode.TryGetBoolFieldQuickly("allowbiowaresuites", ref _blnAllowBiowareSuites);
                // House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
                objXmlNode.TryGetBoolFieldQuickly("freespiritpowerpointsmag", ref _blnFreeSpiritPowerPointsMAG);
                // House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
                objXmlNode.TryGetBoolFieldQuickly("compensateskillgroupkarmadifference",
                                                  ref _blnCompensateSkillGroupKarmaDifference);
                // Optional Rule: Whether Life Modules should automatically create a character back story.
                objXmlNode.TryGetBoolFieldQuickly("autobackstory", ref _blnAutomaticBackstory);
                // House Rule: Whether Public Awareness should be a calculated attribute based on Street Cred and Notoriety.
                objXmlNode.TryGetBoolFieldQuickly("usecalculatedpublicawareness", ref _blnUseCalculatedPublicAwareness);
                // House Rule: Whether Improved Ability should be capped at 0.5 (false) or 1.5 (true) of the target skill's Learned Rating.
                objXmlNode.TryGetBoolFieldQuickly("increasedimprovedabilitymodifier",
                                                  ref _blnIncreasedImprovedAbilityMultiplier);
                // House Rule: Whether lifestyles will give free grid subscriptions found in HT to players.
                objXmlNode.TryGetBoolFieldQuickly("allowfreegrids", ref _blnAllowFreeGrids);
                // House Rule: Whether Technomancers should be allowed to receive Schooling discounts in the same manner as Awakened.
                objXmlNode.TryGetBoolFieldQuickly("allowtechnomancerschooling", ref _blnAllowTechnomancerSchooling);
                // House Rule: Maximum value that cyberlimbs can have as a bonus on top of their Customization.
                objXmlNode.TryGetInt32FieldQuickly("cyberlimbattributebonuscap", ref _intCyberlimbAttributeBonusCap);
                if (!objXmlNode.TryGetBoolFieldQuickly("cyberlimbattributebonuscapoverride",
                                                       ref _blnCyberlimbAttributeBonusCapOverride))
                    _blnCyberlimbAttributeBonusCapOverride = _intCyberlimbAttributeBonusCap == 4;
                // House/Optional Rule: Attribute values are allowed to go below 0 due to Essence Loss.
                objXmlNode.TryGetBoolFieldQuickly("unclampattributeminimum", ref _blnUnclampAttributeMinimum);
                // Following two settings used to be stored in global options, so they are fetched from the registry if they are not present
                // Use Rigger 5.0 drone mods
                if (!objXmlNode.TryGetBoolFieldQuickly("dronemods", ref _blnDroneMods))
                    GlobalSettings.LoadBoolFromRegistry(ref _blnDroneMods, "dronemods", string.Empty, true);
                // Apply maximum drone attribute improvement rule to Pilot, too
                if (!objXmlNode.TryGetBoolFieldQuickly("dronemodsmaximumpilot", ref _blnDroneModsMaximumPilot))
                    GlobalSettings.LoadBoolFromRegistry(ref _blnDroneModsMaximumPilot, "dronemodsPilot", string.Empty,
                                                        true);

                // Maximum number of attributes at metatype maximum in character creation
                if (!objXmlNode.TryGetInt32FieldQuickly("maxnumbermaxattributescreate",
                                                        ref _intMaxNumberMaxAttributesCreate))
                {
                    // Legacy shim
                    bool blnTemp = false;
                    if (objXmlNode.TryGetBoolFieldQuickly("allow2ndmaxattribute", ref blnTemp) && blnTemp)
                        _intMaxNumberMaxAttributesCreate = 2;
                }

                // Maximum skill rating in character creation
                objXmlNode.TryGetInt32FieldQuickly("maxskillratingcreate", ref _intMaxSkillRatingCreate);
                // Maximum knowledge skill rating in character creation
                objXmlNode.TryGetInt32FieldQuickly("maxknowledgeskillratingcreate",
                                                   ref _intMaxKnowledgeSkillRatingCreate);
                // Maximum skill rating
                if (objXmlNode.TryGetInt32FieldQuickly("maxskillrating", ref _intMaxSkillRating)
                    && _intMaxSkillRatingCreate > _intMaxSkillRating)
                    _intMaxSkillRatingCreate = _intMaxSkillRating;
                // Maximum knowledge skill rating
                if (objXmlNode.TryGetInt32FieldQuickly("maxknowledgeskillrating", ref _intMaxKnowledgeSkillRating)
                    && _intMaxKnowledgeSkillRatingCreate > _intMaxKnowledgeSkillRating)
                    _intMaxKnowledgeSkillRatingCreate = _intMaxKnowledgeSkillRating;

                //House Rule: The DicePenalty per sustained spell or form
                objXmlNode.TryGetInt32FieldQuickly("dicepenaltysustaining", ref _intDicePenaltySustaining);

                // Initiative dice
                objXmlNode.TryGetInt32FieldQuickly("mininitiativedice", ref _intMinInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("maxinitiativedice", ref _intMaxInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("minastralinitiativedice", ref _intMinAstralInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("maxastralinitiativedice", ref _intMaxAstralInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("mincoldsiminitiativedice", ref _intMinColdSimInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("maxcoldsiminitiativedice", ref _intMaxColdSimInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("minhotsiminitiativedice", ref _intMinHotSimInitiativeDice);
                objXmlNode.TryGetInt32FieldQuickly("maxhotsiminitiativedice", ref _intMaxHotSimInitiativeDice);

                XPathNavigator xmlKarmaCostNode = objXmlNode.SelectSingleNodeAndCacheExpression("karmacost", token);
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
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveknowledgeskill",
                                                             ref _intKarmaImproveKnowledgeSkill);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveactiveskill",
                                                             ref _intKarmaImproveActiveSkill);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaimproveskillgroup", ref _intKarmaImproveSkillGroup);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspell", ref _intKarmaSpell);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewcomplexform", ref _intKarmaNewComplexForm);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewaiprogram", ref _intKarmaNewAIProgram);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmanewaiadvancedprogram",
                                                             ref _intKarmaNewAIAdvancedProgram);
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
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmacounterspellingfocus",
                                                             ref _intKarmaCounterspellingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmadisenchantingfocus",
                                                             ref _intKarmaDisenchantingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaflexiblesignaturefocus",
                                                             ref _intKarmaFlexibleSignatureFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmamaskingfocus", ref _intKarmaMaskingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmapowerfocus", ref _intKarmaPowerFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaqifocus", ref _intKarmaQiFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaritualspellcastingfocus",
                                                             ref _intKarmaRitualSpellcastingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspellcastingfocus", ref _intKarmaSpellcastingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaspellshapingfocus", ref _intKarmaSpellShapingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmasummoningfocus", ref _intKarmaSummoningFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmasustainingfocus", ref _intKarmaSustainingFocus);
                    xmlKarmaCostNode.TryGetInt32FieldQuickly("karmaweaponfocus", ref _intKarmaWeaponFocus);
                }

                XPathNavigator xmlLegacyCharacterNavigator = null;
                // Legacy sweep by looking at MRU
                if (!await GetBuiltInOptionAsync(token).ConfigureAwait(false)
                    && objXmlNode.SelectSingleNodeAndCacheExpression("books/book", token) == null
                    && objXmlNode.SelectSingleNodeAndCacheExpression("customdatadirectorynames/directoryname", token) == null)
                {
                    foreach (string strMruCharacterFile in GlobalSettings.MostRecentlyUsedCharacters)
                    {
                        XPathDocument objXmlDocument;
                        if (!File.Exists(strMruCharacterFile))
                            continue;
                        try
                        {
                            objXmlDocument = await XPathDocumentExtensions.LoadStandardFromFilePatientAsync(strMruCharacterFile, token: token).ConfigureAwait(false);
                        }
                        catch (XmlException)
                        {
                            continue;
                        }

                        xmlLegacyCharacterNavigator = objXmlDocument.CreateNavigator()
                                                                          .SelectSingleNodeAndCacheExpression("/character", token);

                        if (xmlLegacyCharacterNavigator == null)
                            continue;

                        string strLoopSettingsFile = xmlLegacyCharacterNavigator
                            .SelectSingleNodeAndCacheExpression("settings", token)?.Value;
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
                                objXmlDocument
                                    = await XPathDocumentExtensions.LoadStandardFromFilePatientAsync(
                                        strMruCharacterFile, token: token).ConfigureAwait(false);
                            }
                            catch (XmlException)
                            {
                                continue;
                            }

                            xmlLegacyCharacterNavigator = objXmlDocument.CreateNavigator()
                                                                              .SelectSingleNodeAndCacheExpression(
                                                                                  "/character", token);

                            if (xmlLegacyCharacterNavigator == null)
                                continue;

                            string strLoopSettingsFile = xmlLegacyCharacterNavigator
                                .SelectSingleNodeAndCacheExpression("settings", token)?.Value;
                            if (strLoopSettingsFile == _strFileName)
                                break;
                            xmlLegacyCharacterNavigator = null;
                        }
                    }
                }

                // Load Books.
                _setBooks.Clear();
                foreach (XPathNavigator xmlBook in objXmlNode.SelectAndCacheExpression("books/book", token))
                    _setBooks.Add(xmlBook.Value);
                // Legacy sweep for sourcebooks
                if (xmlLegacyCharacterNavigator != null)
                {
                    foreach (XPathNavigator xmlBook in xmlLegacyCharacterNavigator.SelectAndCacheExpression(
                                 "sources/source", token))
                    {
                        if (!string.IsNullOrEmpty(xmlBook.Value))
                            _setBooks.Add(xmlBook.Value);
                    }
                }

                // Load Custom Data Directory names.
                int intTopMostOrder = 0;
                int intBottomMostOrder = 0;
                Dictionary<int, Tuple<string, bool>> dicLoadingCustomDataDirectories =
                    new Dictionary<int, Tuple<string, bool>>(GlobalSettings.CustomDataDirectoryInfos.Count);
                bool blnNeedToProcessInfosWithoutLoadOrder = false;
                foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression(
                             "customdatadirectorynames/customdatadirectoryname", token))
                {
                    string strDirectoryKey
                        = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("directoryname", token)?.Value;
                    if (string.IsNullOrEmpty(strDirectoryKey))
                        continue;
                    string strLoopId = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                    // Only load in directories that are either present in our GlobalSettings or are enabled
                    bool blnLoopEnabled = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("enabled", token)?.Value
                                          == bool.TrueString;
                    if (blnLoopEnabled || (string.IsNullOrEmpty(strLoopId)
                            ? GlobalSettings.CustomDataDirectoryInfos.Any(
                                x => x.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                            : GlobalSettings.CustomDataDirectoryInfos.Any(
                                x => x.InternalId.Equals(strLoopId, StringComparison.OrdinalIgnoreCase))))
                    {
                        string strOrder = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("order", token)?.Value;
                        if (!string.IsNullOrEmpty(strOrder)
                            && int.TryParse(strOrder, NumberStyles.Integer, GlobalSettings.InvariantCultureInfo,
                                            out int intOrder))
                        {
                            while (dicLoadingCustomDataDirectories.ContainsKey(intOrder))
                                ++intOrder;
                            intTopMostOrder = Math.Max(intOrder, intTopMostOrder);
                            intBottomMostOrder = Math.Min(intOrder, intBottomMostOrder);
                            dicLoadingCustomDataDirectories.Add(intOrder,
                                                                new Tuple<string, bool>(
                                                                    strDirectoryKey, blnLoopEnabled));
                        }
                        else
                            blnNeedToProcessInfosWithoutLoadOrder = true;
                    }
                }

                IAsyncDisposable objLocker2 = await _dicCustomDataDirectoryKeys.LockObject.EnterWriteLockAsync(token)
                                                                               .ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await _dicCustomDataDirectoryKeys.ClearAsync(token).ConfigureAwait(false);
                    for (int i = intBottomMostOrder; i <= intTopMostOrder; ++i)
                    {
                        if (!dicLoadingCustomDataDirectories.TryGetValue(i, out Tuple<string, bool> tupLoop))
                            continue;
                        string strDirectoryKey = tupLoop.Item1;
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

                        await _dicCustomDataDirectoryKeys
                              .TryAddAsync(strDirectoryKey, tupLoop.Item2, token)
                              .ConfigureAwait(false);
                    }

                    // Legacy sweep for custom data directories
                    if (xmlLegacyCharacterNavigator != null)
                    {
                        foreach (XPathNavigator xmlCustomDataDirectoryName in xmlLegacyCharacterNavigator
                                     .SelectAndCacheExpression("customdatadirectorynames/directoryname", token))
                        {
                            string strDirectoryKey = xmlCustomDataDirectoryName.Value;
                            if (string.IsNullOrEmpty(strDirectoryKey))
                                continue;
                            string strLoopId
                                = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
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

                            await _dicCustomDataDirectoryKeys.TryAddAsync(strDirectoryKey, true, token)
                                                             .ConfigureAwait(false);
                        }
                    }

                    // Add in the stragglers that didn't have any load order info
                    if (blnNeedToProcessInfosWithoutLoadOrder)
                    {
                        foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression(
                                     "customdatadirectorynames/customdatadirectoryname", token))
                        {
                            string strDirectoryKey = objXmlDirectoryName
                                .SelectSingleNodeAndCacheExpression(
                                    "directoryname", token)
                                ?.Value;
                            if (string.IsNullOrEmpty(strDirectoryKey))
                                continue;
                            string strLoopId
                                = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
                            string strOrder = objXmlDirectoryName
                                .SelectSingleNodeAndCacheExpression("order", token)?.Value;
                            if (!string.IsNullOrEmpty(strOrder) && int.TryParse(strOrder, NumberStyles.Integer,
                                    GlobalSettings.InvariantCultureInfo,
                                    out int _))
                                continue;
                            // Only load in directories that are either present in our GlobalSettings or are enabled
                            bool blnLoopEnabled
                                = objXmlDirectoryName.SelectSingleNodeAndCacheExpression("enabled", token)?.Value
                                  == bool.TrueString;
                            if (blnLoopEnabled || (string.IsNullOrEmpty(strLoopId)
                                    ? GlobalSettings.CustomDataDirectoryInfos.Any(
                                        x => x.Name.Equals(strDirectoryKey, StringComparison.OrdinalIgnoreCase))
                                    : GlobalSettings.CustomDataDirectoryInfos.Any(
                                        x => x.InternalId.Equals(strLoopId, StringComparison.OrdinalIgnoreCase))))
                            {
                                if (string.IsNullOrEmpty(strLoopId))
                                {
                                    CustomDataDirectoryInfo objExistingInfo = null;
                                    foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings
                                                 .CustomDataDirectoryInfos)
                                    {
                                        if (!objLoopInfo.Name.Equals(strDirectoryKey,
                                                                     StringComparison.OrdinalIgnoreCase))
                                            continue;
                                        if (objExistingInfo == null
                                            || objLoopInfo.MyVersion > objExistingInfo.MyVersion)
                                            objExistingInfo = objLoopInfo;
                                    }

                                    if (objExistingInfo != null)
                                        strDirectoryKey = objExistingInfo.InternalId;
                                }

                                await _dicCustomDataDirectoryKeys.TryAddAsync(strDirectoryKey, blnLoopEnabled, token)
                                                                 .ConfigureAwait(false);
                            }
                        }
                    }

                    if (await _dicCustomDataDirectoryKeys.GetCountAsync(token).ConfigureAwait(false) == 0)
                    {
                        foreach (XPathNavigator objXmlDirectoryName in objXmlNode.SelectAndCacheExpression(
                                     "customdatadirectorynames/directoryname", token))
                        {
                            string strDirectoryKey = objXmlDirectoryName.Value;
                            if (string.IsNullOrEmpty(strDirectoryKey))
                                continue;
                            string strLoopId
                                = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(strDirectoryKey);
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

                            await _dicCustomDataDirectoryKeys.TryAddAsync(strDirectoryKey, true, token)
                                                             .ConfigureAwait(false);
                        }
                    }

                    // Add in any directories that are in GlobalSettings but are not present in the settings so that we may enable them if we want to
                    foreach (string strCharacterSettingsSaveKey in GlobalSettings.CustomDataDirectoryInfos.Select(
                                 x => x.CharacterSettingsSaveKey))
                    {
                        await _dicCustomDataDirectoryKeys.TryAddAsync(strCharacterSettingsSaveKey, false, token)
                                                         .ConfigureAwait(false);
                    }
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await RecalculateEnabledCustomDataDirectoriesAsync(token).ConfigureAwait(false);

                foreach (XPathNavigator xmlBook in (await XmlManager.LoadXPathAsync("books.xml", await GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                                                         .SelectAndCacheExpression(
                                                             "/chummer/books/book[permanent]/code", token))
                {
                    if (!string.IsNullOrEmpty(xmlBook.Value))
                        _setBooks.Add(xmlBook.Value);
                }

                await RecalculateBookXPathAsync(token).ConfigureAwait(false);

                // Used to legacy sweep build settings.
                XPathNavigator xmlDefaultBuildNode = objXmlNode.SelectSingleNodeAndCacheExpression("defaultbuild", token);
                if (objXmlNode.TryGetStringFieldQuickly("buildmethod", ref strTemp)
                    && Enum.TryParse(strTemp, true, out CharacterBuildMethod eBuildMethod)
                    || xmlDefaultBuildNode?.TryGetStringFieldQuickly("buildmethod", ref strTemp) == true
                    && Enum.TryParse(strTemp, true, out eBuildMethod))
                    _eBuildMethod = eBuildMethod;
                if (!objXmlNode.TryGetInt32FieldQuickly("buildpoints", ref _intBuildPoints))
                    xmlDefaultBuildNode?.TryGetInt32FieldQuickly("buildpoints", ref _intBuildPoints);
                if (!objXmlNode.TryGetInt32FieldQuickly("qualitykarmalimit", ref _intQualityKarmaLimit)
                    && BuildMethodUsesPriorityTables)
                    _intQualityKarmaLimit = _intBuildPoints;
                objXmlNode.TryGetStringFieldQuickly("priorityarray", ref _strPriorityArray);
                objXmlNode.TryGetStringFieldQuickly("prioritytable", ref _strPriorityTable);
                objXmlNode.TryGetInt32FieldQuickly("sumtoten", ref _intSumtoTen);
                if (!objXmlNode.TryGetInt32FieldQuickly("availability", ref _intAvailability))
                    xmlDefaultBuildNode?.TryGetInt32FieldQuickly("availability", ref _intAvailability);
                if (!objXmlNode.TryGetInt32FieldQuickly("maxmartialarts", ref _intMaxMartialArts))
                    xmlDefaultBuildNode?.TryGetInt32FieldQuickly("maxmartialarts", ref _intMaxMartialArts);
                if (!objXmlNode.TryGetInt32FieldQuickly("maxmartialtechniques", ref _intMaxMartialTechniques))
                    xmlDefaultBuildNode?.TryGetInt32FieldQuickly("maxmartialtechniques", ref _intMaxMartialTechniques);
                objXmlNode.TryGetDecFieldQuickly("nuyencarryover", ref _decNuyenCarryover);
                objXmlNode.TryGetDecFieldQuickly("nuyenmaxbp", ref _decNuyenMaximumBP);

                _setBannedWareGrades.Clear();
                foreach (XPathNavigator xmlGrade in objXmlNode.SelectAndCacheExpression("bannedwaregrades/grade", token))
                    _setBannedWareGrades.Add(xmlGrade.Value);

                _setRedlinerExcludes.Clear();
                foreach (XPathNavigator xmlLimb in objXmlNode.SelectAndCacheExpression("redlinerexclusion/limb", token))
                    _setRedlinerExcludes.Add(xmlLimb.Value);

                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Initialization, Save, and Load Methods

        #region Build Properties

        /// <summary>
        /// Method being used to build the character.
        /// </summary>
        public CharacterBuildMethod BuildMethod
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eBuildMethod;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (InterlockedExtensions.Exchange(ref _eBuildMethod, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Method being used to build the character.
        /// </summary>
        public async Task<CharacterBuildMethod> GetBuildMethodAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eBuildMethod;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Method being used to build the character.
        /// </summary>
        public async Task SetBuildMethodAsync(CharacterBuildMethod value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (InterlockedExtensions.Exchange(ref _eBuildMethod, value) != value)
                    await OnPropertyChangedAsync(nameof(BuildMethod), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool BuildMethodUsesPriorityTables
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eBuildMethod.UsesPriorityTables();
            }
        }

        public async Task<bool> GetBuildMethodUsesPriorityTablesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eBuildMethod.UsesPriorityTables();
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool BuildMethodIsPriority => BuildMethod == CharacterBuildMethod.Priority;

        public async Task<bool> GetBuildMethodIsPriorityAsync(CancellationToken token = default) =>
            await GetBuildMethodAsync(token).ConfigureAwait(false) == CharacterBuildMethod.Priority;

        public bool BuildMethodIsSumtoTen => BuildMethod == CharacterBuildMethod.SumtoTen;

        public async Task<bool> GetBuildMethodIsSumtoTenAsync(CancellationToken token = default) =>
            await GetBuildMethodAsync(token).ConfigureAwait(false) == CharacterBuildMethod.SumtoTen;

        public bool BuildMethodIsLifeModule => BuildMethod == CharacterBuildMethod.LifeModule;

        public async Task<bool> GetBuildMethodIsLifeModuleAsync(CancellationToken token = default) =>
            await GetBuildMethodAsync(token).ConfigureAwait(false) == CharacterBuildMethod.LifeModule;

        /// <summary>
        /// The priority configuration used in Priority mode.
        /// </summary>
        public string PriorityArray
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPriorityArray;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strPriorityArray, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The priority configuration used in Priority mode.
        /// </summary>
        public async Task<string> GetPriorityArrayAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strPriorityArray;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The priority configuration used in Priority mode.
        /// </summary>
        public async Task SetPriorityArrayAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strPriorityArray, value) != value)
                    await OnPropertyChangedAsync(nameof(PriorityArray), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The priority table used in Priority or Sum-to-Ten mode.
        /// </summary>
        public string PriorityTable
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPriorityTable;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strPriorityTable, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The total value of priorities used in Sum-to-Ten mode.
        /// </summary>
        public int SumtoTen
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSumtoTen;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intSumtoTen, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The total value of priorities used in Sum-to-Ten mode.
        /// </summary>
        public async Task<int> GetSumtoTenAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intSumtoTen;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The total value of priorities used in Sum-to-Ten mode.
        /// </summary>
        public async Task SetSumtoTenAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intSumtoTen == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intSumtoTen, value) != value)
                    await OnPropertyChangedAsync(nameof(SumtoTen), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of Karma that is used to create the character.
        /// </summary>
        public int BuildKarma
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intBuildPoints;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intBuildPoints, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Amount of Karma that is used to create the character.
        /// </summary>
        public async Task<int> GetBuildKarmaAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intBuildPoints;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of Karma that is used to create the character.
        /// </summary>
        public async Task SetBuildKarmaAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intBuildPoints == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intBuildPoints, value) != value)
                    await OnPropertyChangedAsync(nameof(BuildKarma), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Limit on the amount of karma that can be spent at creation on qualities
        /// </summary>
        public int QualityKarmaLimit
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intQualityKarmaLimit;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intQualityKarmaLimit, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Limit on the amount of karma that can be spent at creation on qualities
        /// </summary>
        public async Task<int> GetQualityKarmaLimitAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intQualityKarmaLimit;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Limit on the amount of karma that can be spent at creation on qualities
        /// </summary>
        public async Task SetQualityKarmaLimitAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intQualityKarmaLimit == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intQualityKarmaLimit, value) != value)
                    await OnPropertyChangedAsync(nameof(QualityKarmaLimit), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum item Availability for new characters.
        /// </summary>
        public int MaximumAvailability
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intAvailability;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intAvailability, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Limit on the amount of karma that can be spent at creation on qualities
        /// </summary>
        public async Task<int> GetMaximumAvailabilityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intAvailability;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Limit on the amount of karma that can be spent at creation on qualities
        /// </summary>
        public async Task SetMaximumAvailabilityAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intAvailability == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intAvailability, value) != value)
                    await OnPropertyChangedAsync(nameof(MaximumAvailability), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of martial arts that for new characters.
        /// </summary>
        public int MaximumMartialArts
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxMartialArts;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxMartialArts, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum number of martial arts that for new characters.
        /// </summary>
        public async Task<int> GetMaximumMartialArtsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxMartialArts;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of martial arts that for new characters.
        /// </summary>
        public async Task SetMaximumMartialArtsAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxMartialArts == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxMartialArts, value) != value)
                    await OnPropertyChangedAsync(nameof(MaximumAvailability), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of martial techniques on a martial art for new characters.
        /// </summary>
        public int MaximumMartialTechniques
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxMartialTechniques;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxMartialTechniques, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum number of martial arts that for new characters.
        /// </summary>
        public async Task<int> GetMaximumMartialTechniquesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxMartialTechniques;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of martial arts that for new characters.
        /// </summary>
        public async Task SetMaximumMartialTechniquesAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxMartialTechniques == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxMartialTechniques, value) != value)
                    await OnPropertyChangedAsync(nameof(MaximumAvailability), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of Build Points that can be spent on Nuyen.
        /// </summary>
        public decimal NuyenMaximumBP
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decNuyenMaximumBP;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decNuyenMaximumBP == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decNuyenMaximumBP = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Maximum number of Build Points that can be spent on Nuyen.
        /// </summary>
        public async Task<decimal> GetNuyenMaximumBPAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decNuyenMaximumBP;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum amount of remaining Nuyen that is carried over to the character once they are created.
        /// </summary>
        public async Task SetNuyenMaximumBPAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decNuyenMaximumBP == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decNuyenMaximumBP == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decNuyenMaximumBP = value;
                    await OnPropertyChangedAsync(nameof(NuyenMaximumBP), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Blocked grades of cyber/bioware in Create mode.
        /// </summary>
        public HashSet<string> BannedWareGrades
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setBannedWareGrades;
            }
        }

        /// <summary>
        /// Blocked grades of cyber/bioware in Create mode.
        /// </summary>
        public async Task<HashSet<string>> GetBannedWareGradesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setBannedWareGrades;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Limb types excluded by redliner.
        /// </summary>
        public HashSet<string> RedlinerExcludes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setRedlinerExcludes;
            }
        }

        /// <summary>
        /// Limb types excluded by redliner.
        /// </summary>
        public async Task<HashSet<string>> GetRedlinerExcludesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setRedlinerExcludes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool RedlinerExcludesSkull
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setRedlinerExcludes.Contains("skull");
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (value == _setRedlinerExcludes.Contains("skull"))
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (value)
                    {
                        if (_setRedlinerExcludes.Contains("skull"))
                            return;
                        using (LockObject.EnterWriteLock())
                        {
                            if (!_setRedlinerExcludes.Add("skull"))
                                return;
                            this.OnMultiplePropertyChanged(nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes));
                        }
                    }
                    else
                    {
                        if (!_setRedlinerExcludes.Contains("skull"))
                            return;
                        using (LockObject.EnterWriteLock())
                        {
                            if (!_setRedlinerExcludes.Remove("skull"))
                                return;
                            this.OnMultiplePropertyChanged(nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes));
                        }
                    }
                }
            }
        }

        public async Task<bool> GetRedlinerExcludesSkullAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setRedlinerExcludes.Contains("skull");
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetRedlinerExcludesSkullAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value == _setRedlinerExcludes.Contains("skull"))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value)
                {
                    if (_setRedlinerExcludes.Contains("skull"))
                        return;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_setRedlinerExcludes.Add("skull"))
                            return;
                        await this.OnMultiplePropertyChangedAsync(token, nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    if (!_setRedlinerExcludes.Contains("skull"))
                        return;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_setRedlinerExcludes.Remove("skull"))
                            return;
                        await this.OnMultiplePropertyChangedAsync(token, nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool RedlinerExcludesTorso
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setRedlinerExcludes.Contains("torso");
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (value == _setRedlinerExcludes.Contains("torso"))
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (value)
                    {
                        if (_setRedlinerExcludes.Contains("torso"))
                            return;
                        using (LockObject.EnterWriteLock())
                        {
                            if (!_setRedlinerExcludes.Add("torso"))
                                return;
                            this.OnMultiplePropertyChanged(nameof(RedlinerExcludesTorso), nameof(RedlinerExcludes));
                        }
                    }
                    else
                    {
                        if (!_setRedlinerExcludes.Contains("torso"))
                            return;
                        using (LockObject.EnterWriteLock())
                        {
                            if (!_setRedlinerExcludes.Remove("torso"))
                                return;
                            this.OnMultiplePropertyChanged(nameof(RedlinerExcludesTorso), nameof(RedlinerExcludes));
                        }
                    }
                }
            }
        }

        public async Task<bool> GetRedlinerExcludesTorsoAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setRedlinerExcludes.Contains("torso");
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetRedlinerExcludesTorsoAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value == _setRedlinerExcludes.Contains("torso"))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value)
                {
                    if (_setRedlinerExcludes.Contains("torso"))
                        return;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_setRedlinerExcludes.Add("torso"))
                            return;
                        await this.OnMultiplePropertyChangedAsync(token, nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    if (!_setRedlinerExcludes.Contains("torso"))
                        return;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_setRedlinerExcludes.Remove("torso"))
                            return;
                        await this.OnMultiplePropertyChangedAsync(token, nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool RedlinerExcludesArms
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setRedlinerExcludes.Contains("arm");
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (value == _setRedlinerExcludes.Contains("arm"))
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (value)
                    {
                        if (_setRedlinerExcludes.Contains("arm"))
                            return;
                        using (LockObject.EnterWriteLock())
                        {
                            if (!_setRedlinerExcludes.Add("arm"))
                                return;
                            this.OnMultiplePropertyChanged(nameof(RedlinerExcludesArms), nameof(RedlinerExcludes));
                        }
                    }
                    else
                    {
                        if (!_setRedlinerExcludes.Contains("arm"))
                            return;
                        using (LockObject.EnterWriteLock())
                        {
                            if (!_setRedlinerExcludes.Remove("arm"))
                                return;
                            this.OnMultiplePropertyChanged(nameof(RedlinerExcludesArms), nameof(RedlinerExcludes));
                        }
                    }
                }
            }
        }

        public async Task<bool> GetRedlinerExcludesArmsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setRedlinerExcludes.Contains("arm");
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetRedlinerExcludesArmsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value == _setRedlinerExcludes.Contains("arm"))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value)
                {
                    if (_setRedlinerExcludes.Contains("arm"))
                        return;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_setRedlinerExcludes.Add("arm"))
                            return;
                        await this.OnMultiplePropertyChangedAsync(token, nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    if (!_setRedlinerExcludes.Contains("arm"))
                        return;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_setRedlinerExcludes.Remove("arm"))
                            return;
                        await this.OnMultiplePropertyChangedAsync(token, nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool RedlinerExcludesLegs
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setRedlinerExcludes.Contains("leg");
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (value == _setRedlinerExcludes.Contains("leg"))
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (value)
                    {
                        if (_setRedlinerExcludes.Contains("leg"))
                            return;
                        using (LockObject.EnterWriteLock())
                        {
                            if (!_setRedlinerExcludes.Add("leg"))
                                return;
                            this.OnMultiplePropertyChanged(nameof(RedlinerExcludesLegs), nameof(RedlinerExcludes));
                        }
                    }
                    else
                    {
                        if (!_setRedlinerExcludes.Contains("leg"))
                            return;
                        using (LockObject.EnterWriteLock())
                        {
                            if (!_setRedlinerExcludes.Remove("leg"))
                                return;
                            this.OnMultiplePropertyChanged(nameof(RedlinerExcludesLegs), nameof(RedlinerExcludes));
                        }
                    }
                }
            }
        }

        public async Task<bool> GetRedlinerExcludesLegsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setRedlinerExcludes.Contains("leg");
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task SetRedlinerExcludesLegsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value == _setRedlinerExcludes.Contains("leg"))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (value)
                {
                    if (_setRedlinerExcludes.Contains("leg"))
                        return;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_setRedlinerExcludes.Add("leg"))
                            return;
                        await this.OnMultiplePropertyChangedAsync(token, nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    if (!_setRedlinerExcludes.Contains("leg"))
                        return;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!_setRedlinerExcludes.Remove("leg"))
                            return;
                        await this.OnMultiplePropertyChangedAsync(token, nameof(RedlinerExcludesSkull), nameof(RedlinerExcludes)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string DictionaryKey => BuiltInOption ? SourceIdString : FileName;

        public async Task<string> GetDictionaryKeyAsync(CancellationToken token = default)
        {
            return await GetBuiltInOptionAsync(token).ConfigureAwait(false)
                ? await GetSourceIdAsync(token).ConfigureAwait(false)
                : await GetFileNameAsync(token).ConfigureAwait(false);
        }

        #endregion Build Properties

        #region Properties and Methods

        /// <summary>
        /// Determine whether a given book is in use.
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public bool BookEnabled(string strCode, CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
                return _setBooks.Contains(strCode);
        }

        /// <summary>
        /// Determine whether a given book is in use.
        /// </summary>
        /// <param name="strCode">Book code to search for.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<bool> BookEnabledAsync(string strCode, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setBooks.Contains(strCode);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books and optional rules.
        /// </summary>
        public string BookXPath(bool excludeHidden = true, CancellationToken token = default)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdPath))
            {
                if (excludeHidden)
                    sbdPath.Append("not(hide)");
                using (LockObject.EnterReadLock(token))
                {
                    if (string.IsNullOrWhiteSpace(_strBookXPath) && _setBooks.Count > 0)
                    {
                        RecalculateBookXPath(token);
                    }

                    if (!string.IsNullOrEmpty(_strBookXPath))
                    {
                        if (sbdPath.Length != 0)
                            sbdPath.Append(" and ");
                        sbdPath.Append("(ignoresourcedisabled or ").Append(_strBookXPath).Append(')');
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
                }

                if (sbdPath.Length > 1)
                    return '(' + sbdPath.ToString() + ')';
            }

            // We have only the opening parentheses; return an empty string
            // The above should not happen, so break if we're debugging, as there's something weird going on with the character's setup
            Utils.BreakIfDebug();
            return string.Empty;
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books and optional rules.
        /// </summary>
        public async Task<string> BookXPathAsync(bool excludeHidden = true, CancellationToken token = default)
        {
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdPath))
            {
                if (excludeHidden)
                    sbdPath.Append("not(hide)");
                IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(_strBookXPath) && _setBooks.Count > 0)
                    {
                        await RecalculateBookXPathAsync(token).ConfigureAwait(false);
                    }

                    token.ThrowIfCancellationRequested();

                    if (!string.IsNullOrEmpty(_strBookXPath))
                    {
                        if (sbdPath.Length != 0)
                            sbdPath.Append(" and ");
                        sbdPath.Append("(ignoresourcedisabled or ").Append(_strBookXPath).Append(')');
                    }
                    else
                    {
                        // Should not ever have a situation where BookXPath remains empty after recalculation, but it's here just in case
                        Utils.BreakIfDebug();
                    }

                    if (!await GetDroneModsAsync(token).ConfigureAwait(false))
                    {
                        if (sbdPath.Length != 0)
                            sbdPath.Append(" and ");
                        sbdPath.Append("not(optionaldrone)");
                    }
                }
                finally
                {
                    await objLocker.DisposeAsync().ConfigureAwait(false);
                }

                if (sbdPath.Length > 1)
                    return '(' + sbdPath.ToString() + ')';
            }

            // We have only the opening parentheses; return an empty string
            // The above should not happen, so break if we're debugging, as there's something weird going on with the character's setup
            Utils.BreakIfDebug();
            return string.Empty;
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books.
        /// </summary>
        public void RecalculateBookXPath(CancellationToken token = default)
        {
            using (LockObject.EnterReadLock(token))
            {
                _strBookXPath = string.Empty;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdBookXPath))
                {
                    sbdBookXPath.Append('(');
                    foreach (string strBook in _setBooks)
                    {
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrWhiteSpace(strBook))
                        {
                            sbdBookXPath.Append("source = ").Append(strBook.CleanXPath()).Append(" or ");
                        }
                    }

                    if (sbdBookXPath.Length >= 4)
                    {
                        sbdBookXPath.Length -= 4;
                        sbdBookXPath.Append(')');
                        Interlocked.CompareExchange(ref _strBookXPath, sbdBookXPath.ToString(), string.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// XPath query used to filter items based on the user's selected source books.
        /// </summary>
        public async Task RecalculateBookXPathAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _strBookXPath = string.Empty;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdBookXPath))
                {
                    sbdBookXPath.Append('(');
                    foreach (string strBook in _setBooks)
                    {
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrWhiteSpace(strBook))
                        {
                            sbdBookXPath.Append("source = ").Append(strBook.CleanXPath()).Append(" or ");
                        }
                    }

                    if (sbdBookXPath.Length >= 4)
                    {
                        sbdBookXPath.Length -= 4;
                        sbdBookXPath.Append(')');
                        Interlocked.CompareExchange(ref _strBookXPath, sbdBookXPath.ToString(), string.Empty);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public LockingTypedOrderedDictionary<string, bool> CustomDataDirectoryKeys
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _dicCustomDataDirectoryKeys;
            }
        }

        public async Task<LockingTypedOrderedDictionary<string, bool>> GetCustomDataDirectoryKeysAsync(
            CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _dicCustomDataDirectoryKeys;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public IReadOnlyList<string> EnabledCustomDataDirectoryPaths
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstEnabledCustomDataDirectoryPaths;
            }
        }

        public async Task<IReadOnlyList<string>> GetEnabledCustomDataDirectoryPathsAsync(
            CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _lstEnabledCustomDataDirectoryPaths;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public IReadOnlyList<CustomDataDirectoryInfo> EnabledCustomDataDirectoryInfos
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setEnabledCustomDataDirectories;
            }
        }

        public async Task<IReadOnlyList<CustomDataDirectoryInfo>> GetEnabledCustomDataDirectoryInfosAsync(
            CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setEnabledCustomDataDirectories;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// A HashSet that can be used for fast queries, which content is (and should) always identical to the IReadOnlyList EnabledCustomDataDirectoryInfos
        /// </summary>
        public IReadOnlyCollection<Guid> EnabledCustomDataDirectoryInfoGuids
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setEnabledCustomDataDirectoryGuids;
            }
        }

        /// <summary>
        /// A HashSet that can be used for fast queries, which content is (and should) always identical to the IReadOnlyList EnabledCustomDataDirectoryInfos
        /// </summary>
        public async Task<IReadOnlyCollection<Guid>> GetEnabledCustomDataDirectoryInfoGuidsAsync(
            CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setEnabledCustomDataDirectoryGuids;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public void RecalculateEnabledCustomDataDirectories(CancellationToken token = default)
        {
            using (LockObject.EnterWriteLock(token))
            {
                _setEnabledCustomDataDirectoryGuids.Clear();
                _setEnabledCustomDataDirectories.Clear();
                _lstEnabledCustomDataDirectoryPaths.Clear();
                _dicCustomDataDirectoryKeys.ForEach(kvpCustomDataDirectoryName =>
                {
                    if (!kvpCustomDataDirectoryName.Value)
                        return;
                    string strKey = kvpCustomDataDirectoryName.Key;
                    string strId
                        = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(
                            strKey, out ValueVersion objPreferredVersion);
                    CustomDataDirectoryInfo objInfoToAdd = null;
                    if (string.IsNullOrEmpty(strId))
                    {
                        foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings.CustomDataDirectoryInfos)
                        {
                            token.ThrowIfCancellationRequested();
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
                            token.ThrowIfCancellationRequested();
                            if (!objLoopInfo.InternalId.Equals(strId, StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (objInfoToAdd == null || VersionMatchScore(objLoopInfo.MyVersion)
                                > VersionMatchScore(objInfoToAdd.MyVersion))
                                objInfoToAdd = objLoopInfo;
                        }

                        int VersionMatchScore(ValueVersion objVersion)
                        {
                            int intReturn = int.MaxValue;
                            intReturn -= (objPreferredVersion.Build - objVersion.Build).Pow(2)
                                         * 16777216;
                            intReturn -= (objPreferredVersion.Major - objVersion.Major).Pow(2)
                                         * 65536;
                            intReturn -= (objPreferredVersion.Minor - objVersion.Minor).Pow(2)
                                         * 256;
                            intReturn -= (objPreferredVersion.Revision - objVersion.Revision).Pow(2);
                            return intReturn;
                        }
                    }

                    if (objInfoToAdd != null)
                    {
                        _setEnabledCustomDataDirectoryGuids.Add(objInfoToAdd.Guid);
                        _setEnabledCustomDataDirectories.Add(objInfoToAdd);
                        _lstEnabledCustomDataDirectoryPaths.Add(objInfoToAdd.DirectoryPath);
                    }
                    else
                        Utils.BreakIfDebug();
                }, token);
            }
        }

        public async Task RecalculateEnabledCustomDataDirectoriesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _setEnabledCustomDataDirectoryGuids.Clear();
                _setEnabledCustomDataDirectories.Clear();
                _lstEnabledCustomDataDirectoryPaths.Clear();
                await _dicCustomDataDirectoryKeys.ForEachAsync(kvpCustomDataDirectoryName =>
                {
                    if (!kvpCustomDataDirectoryName.Value)
                        return;
                    string strKey = kvpCustomDataDirectoryName.Key;
                    string strId
                        = CustomDataDirectoryInfo.GetIdFromCharacterSettingsSaveKey(
                            strKey, out ValueVersion objPreferredVersion);
                    CustomDataDirectoryInfo objInfoToAdd = null;
                    if (string.IsNullOrEmpty(strId))
                    {
                        foreach (CustomDataDirectoryInfo objLoopInfo in GlobalSettings.CustomDataDirectoryInfos)
                        {
                            token.ThrowIfCancellationRequested();
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
                            token.ThrowIfCancellationRequested();
                            if (!objLoopInfo.InternalId.Equals(strId, StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (objInfoToAdd == null || VersionMatchScore(objLoopInfo.MyVersion)
                                > VersionMatchScore(objInfoToAdd.MyVersion))
                                objInfoToAdd = objLoopInfo;
                        }

                        int VersionMatchScore(ValueVersion objVersion)
                        {
                            int intReturn = int.MaxValue;
                            intReturn -= (objPreferredVersion.Build - objVersion.Build).Pow(2)
                                         * 16777216;
                            intReturn -= (objPreferredVersion.Major - objVersion.Major).Pow(2)
                                         * 65536;
                            intReturn -= (objPreferredVersion.Minor - objVersion.Minor).Pow(2)
                                         * 256;
                            intReturn -= (objPreferredVersion.Revision - objVersion.Revision).Pow(2);
                            return intReturn;
                        }
                    }

                    if (objInfoToAdd != null)
                    {
                        _setEnabledCustomDataDirectoryGuids.Add(objInfoToAdd.Guid);
                        _setEnabledCustomDataDirectories.Add(objInfoToAdd);
                        _lstEnabledCustomDataDirectoryPaths.Add(objInfoToAdd.DirectoryPath);
                    }
                    else
                        Utils.BreakIfDebug();
                }, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Syntactic sugar for XmlManager.LoadXPath() where we use the current enabled custom data directory list from our options file.
        /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
        /// Returns a new XPathNavigator associated with the XPathDocument so that multiple threads each get their own navigator if they're called on the same file
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XPathNavigator LoadDataXPath(string strFileName, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            IReadOnlyList<string> lstCustomPaths = EnabledCustomDataDirectoryPaths;
            if (strFileName == "packs.xml")
            {
                List<string> lstCustomPacksPaths = new List<string>(lstCustomPaths);
                lstCustomPacksPaths.Add(Utils.GetPacksFolderPath);
                return XmlManager.LoadXPath(strFileName, lstCustomPacksPaths, strLanguage, blnLoadFile, token);
            }
            return XmlManager.LoadXPath(strFileName, lstCustomPaths, strLanguage, blnLoadFile, token);
        }

        /// <summary>
        /// Syntactic sugar for XmlManager.LoadXPathAsync() where we use the current enabled custom data directory list from our options file.
        /// XPathDocuments are usually faster than XmlDocuments, but are read-only and take longer to load if live custom data is enabled
        /// Returns a new XPathNavigator associated with the XPathDocument so that multiple threads each get their own navigator if they're called on the same file
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<XPathNavigator> LoadDataXPathAsync(string strFileName, string strLanguage = "",
            bool blnLoadFile = false, CancellationToken token = default)
        {
            IReadOnlyList<string> lstCustomPaths = await GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false);
            if (strFileName == "packs.xml")
            {
                List<string> lstCustomPacksPaths = new List<string>(lstCustomPaths);
                lstCustomPacksPaths.Add(Utils.GetPacksFolderPath);
                return await XmlManager.LoadXPathAsync(strFileName, lstCustomPacksPaths, strLanguage, blnLoadFile, token).ConfigureAwait(false);
            }
            return await XmlManager.LoadXPathAsync(strFileName, lstCustomPaths, strLanguage, blnLoadFile, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Syntactic sugar for XmlManager.Load() where we use the current enabled custom data directory list from our options file.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        /// <param name="token">Cancellation token to use.</param>
        [NotNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public XmlDocument LoadData(string strFileName, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            IReadOnlyList<string> lstCustomPaths = EnabledCustomDataDirectoryPaths;
            if (strFileName == "packs.xml")
            {
                List<string> lstCustomPacksPaths = new List<string>(lstCustomPaths);
                lstCustomPacksPaths.Add(Utils.GetPacksFolderPath);
                return XmlManager.Load(strFileName, lstCustomPacksPaths, strLanguage, blnLoadFile, token);
            }
            return XmlManager.Load(strFileName, lstCustomPaths, strLanguage, blnLoadFile, token);
        }

        /// <summary>
        /// Syntactic sugar for XmlManager.LoadAsync() where we use the current enabled custom data directory list from our options file.
        /// </summary>
        /// <param name="strFileName">Name of the XML file to load.</param>
        /// <param name="strLanguage">Language in which to load the data document.</param>
        /// <param name="blnLoadFile">Whether to force reloading content even if the file already exists.</param>
        /// <param name="token">Cancellation token to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public async Task<XmlDocument> LoadDataAsync(string strFileName, string strLanguage = "", bool blnLoadFile = false, CancellationToken token = default)
        {
            IReadOnlyList<string> lstCustomPaths = await GetEnabledCustomDataDirectoryPathsAsync(token).ConfigureAwait(false);
            if (strFileName == "packs.xml")
            {
                List<string> lstCustomPacksPaths = new List<string>(lstCustomPaths);
                lstCustomPacksPaths.Add(Utils.GetPacksFolderPath);
                return await XmlManager.LoadAsync(strFileName, lstCustomPacksPaths, strLanguage, blnLoadFile, token).ConfigureAwait(false);
            }
            return await XmlManager.LoadAsync(strFileName, lstCustomPaths, strLanguage, blnLoadFile, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Takes a semicolon-separated list of book codes and returns a formatted string with displaynames.
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="strLanguage">Language to fetch</param>
        public string TranslatedBookList(string strInput, string strLanguage = "")
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            List<string> lstBooks = new List<string>(strInput.Count(x => x == ';'));
            // Load the Sourcebook information.
            XPathNavigator objXmlDocument = LoadDataXPath("books.xml", strLanguage);

            foreach (string strBook in strInput.TrimEndOnce(';')
                                               .SplitNoAlloc(';', StringSplitOptions.RemoveEmptyEntries))
            {
                XPathNavigator objXmlBook
                    = objXmlDocument.SelectSingleNodeAndCacheExpression("/chummer/books/book[code = " + strBook.CleanXPath() + ']');
                if (objXmlBook != null)
                {
                    string strToAppend = objXmlBook.SelectSingleNodeAndCacheExpression("translate")?.Value;
                    if (!string.IsNullOrEmpty(strToAppend))
                        lstBooks.Add(strToAppend);
                    else
                    {
                        strToAppend = objXmlBook.SelectSingleNodeAndCacheExpression("name")?.Value;
                        if (!string.IsNullOrEmpty(strToAppend))
                            lstBooks.Add(strToAppend);
                        else
                        {
                            strToAppend = objXmlBook.SelectSingleNodeAndCacheExpression("altcode")?.Value ?? strBook;
                            lstBooks.Add(LanguageManager.GetString("String_Unknown", strLanguage)
                                         + LanguageManager.GetString("String_Space", strLanguage) + '('
                                         + strToAppend + ')');
                        }
                    }
                }
                else
                {
                    lstBooks.Add(LanguageManager.GetString("String_Unknown", strLanguage)
                                 + LanguageManager.GetString("String_Space", strLanguage) + strBook);
                }
            }

            lstBooks.Sort();
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                foreach (string strToAppend in lstBooks)
                    sbdReturn.AppendLine(strToAppend);
                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Takes a semicolon-separated list of book codes and returns a formatted string with displaynames.
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="strLanguage">Language to fetch</param>
        /// <param name="token">Cancellation token to use.</param>
        public async Task<string> TranslatedBookListAsync(string strInput, string strLanguage = "",
                                                          CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            List<string> lstBooks = new List<string>(strInput.Count(x => x == ';'));
            // Load the Sourcebook information.
            XPathNavigator objXmlDocument
                = await LoadDataXPathAsync("books.xml", strLanguage, token: token).ConfigureAwait(false);

            foreach (string strBook in strInput.TrimEndOnce(';')
                                               .SplitNoAlloc(';', StringSplitOptions.RemoveEmptyEntries))
            {
                XPathNavigator objXmlBook
                    = objXmlDocument.SelectSingleNodeAndCacheExpression("/chummer/books/book[code = " + strBook.CleanXPath() + ']', token);
                if (objXmlBook != null)
                {
                    string strToAppend = objXmlBook.SelectSingleNodeAndCacheExpression("translate", token)?.Value;
                    if (!string.IsNullOrEmpty(strToAppend))
                        lstBooks.Add(strToAppend);
                    else
                    {
                        strToAppend = objXmlBook.SelectSingleNodeAndCacheExpression("name", token)?.Value;
                        if (!string.IsNullOrEmpty(strToAppend))
                            lstBooks.Add(strToAppend);
                        else
                        {
                            strToAppend = objXmlBook.SelectSingleNodeAndCacheExpression("altcode", token)?.Value ?? strBook;
                            lstBooks.Add(await LanguageManager
                                               .GetStringAsync("String_Unknown", strLanguage, token: token)
                                               .ConfigureAwait(false)
                                         + await LanguageManager
                                                 .GetStringAsync("String_Space", strLanguage, token: token)
                                                 .ConfigureAwait(false) + '('
                                         + strToAppend + ')');
                        }
                    }
                }
                else
                {
                    lstBooks.Add(await LanguageManager.GetStringAsync("String_Unknown", strLanguage, token: token)
                                                      .ConfigureAwait(false)
                                 + await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                                                        .ConfigureAwait(false) + strBook);
                }
            }

            lstBooks.Sort();
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                foreach (string strToAppend in lstBooks)
                    sbdReturn.AppendLine(strToAppend);
                return sbdReturn.ToString();
            }
        }

        public Guid SourceId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceId;
            }
        }

        public string SourceIdString
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceId.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        public async Task<string> GetSourceIdAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _guiSourceId.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool BuiltInOption
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceId != Guid.Empty;
            }
        }

        public async Task<bool> GetBuiltInOptionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _guiSourceId != Guid.Empty;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the More Lethal Gameplay optional rule is enabled.
        /// </summary>
        public bool MoreLethalGameplay
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnMoreLethalGameplay;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnMoreLethalGameplay == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnMoreLethalGameplay = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the More Lethal Gameplay optional rule is enabled.
        /// </summary>
        public async Task<bool> GetMoreLethalGameplayAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnMoreLethalGameplay;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the More Lethal Gameplay optional rule is enabled.
        /// </summary>
        public async Task SetMoreLethalGameplayAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnMoreLethalGameplay == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnMoreLethalGameplay == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnMoreLethalGameplay = value;
                    await OnPropertyChangedAsync(nameof(MoreLethalGameplay), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to require licensing restricted items.
        /// </summary>
        public bool LicenseRestricted
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnLicenseRestrictedItems;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnLicenseRestrictedItems == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnLicenseRestrictedItems = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether to require licensing restricted items.
        /// </summary>
        public async Task<bool> GetLicenseRestrictedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnLicenseRestrictedItems;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to require licensing restricted items.
        /// </summary>
        public async Task SetLicenseRestrictedAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnLicenseRestrictedItems == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnLicenseRestrictedItems == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnLicenseRestrictedItems = value;
                    await OnPropertyChangedAsync(nameof(LicenseRestricted), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether a Spirit's Maximum Force is based on the character's total MAG.
        /// </summary>
        public bool SpiritForceBasedOnTotalMAG
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnSpiritForceBasedOnTotalMAG;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnSpiritForceBasedOnTotalMAG == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnSpiritForceBasedOnTotalMAG = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether a Spirit's Maximum Force is based on the character's total MAG.
        /// </summary>
        public async Task<bool> GetSpiritForceBasedOnTotalMAGAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnSpiritForceBasedOnTotalMAG;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of Nuyen gained per BP spent when Working for the Man.
        /// </summary>
        public decimal NuyenPerBPWftM
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decNuyenPerBPWftM;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decNuyenPerBPWftM == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decNuyenPerBPWftM = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Amount of Nuyen gained per BP spent when Working for the Man.
        /// </summary>
        public async Task<decimal> GetNuyenPerBPWftMAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decNuyenPerBPWftM;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of Nuyen gained per BP spent when Working for the Man.
        /// </summary>
        public async Task SetNuyenPerBPWftMAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decNuyenPerBPWftM == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decNuyenPerBPWftM == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decNuyenPerBPWftM = value;
                    await OnPropertyChangedAsync(nameof(NuyenPerBPWftM), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of Nuyen spent per BP gained when Working for the People.
        /// </summary>
        public decimal NuyenPerBPWftP
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decNuyenPerBPWftP;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decNuyenPerBPWftP == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decNuyenPerBPWftP = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Amount of Nuyen gained per BP spent when Working for the People.
        /// </summary>
        public async Task<decimal> GetNuyenPerBPWftPAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decNuyenPerBPWftP;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Amount of Nuyen gained per BP spent when Working for the People.
        /// </summary>
        public async Task SetNuyenPerBPWftPAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decNuyenPerBPWftP == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decNuyenPerBPWftP == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decNuyenPerBPWftP = value;
                    await OnPropertyChangedAsync(nameof(NuyenPerBPWftP), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether UnarmedAP, UnarmedReach and UnarmedDV Improvements apply to weapons that use the Unarmed Combat skill.
        /// </summary>
        public bool UnarmedImprovementsApplyToWeapons
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnUnarmedImprovementsApplyToWeapons;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnUnarmedImprovementsApplyToWeapons == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnUnarmedImprovementsApplyToWeapons = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether UnarmedAP, UnarmedReach and UnarmedDV Improvements apply to weapons that use the Unarmed Combat skill.
        /// </summary>
        public async Task<bool> GetUnarmedImprovementsApplyToWeaponsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnUnarmedImprovementsApplyToWeapons;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether UnarmedAP, UnarmedReach and UnarmedDV Improvements apply to weapons that use the Unarmed Combat skill.
        /// </summary>
        public async Task SetUnarmedImprovementsApplyToWeaponsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUnarmedImprovementsApplyToWeapons == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUnarmedImprovementsApplyToWeapons == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnUnarmedImprovementsApplyToWeapons = value;
                    await OnPropertyChangedAsync(nameof(UnarmedImprovementsApplyToWeapons), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters may use Initiation/Submersion in Create mode.
        /// </summary>
        public bool AllowInitiationInCreateMode
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowInitiationInCreateMode;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowInitiationInCreateMode == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowInitiationInCreateMode = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether characters may use Initiation/Submersion in Create mode.
        /// </summary>
        public async Task<bool> GetAllowInitiationInCreateModeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAllowInitiationInCreateMode;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters may use Initiation/Submersion in Create mode.
        /// </summary>
        public async Task SetAllowInitiationInCreateModeAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowInitiationInCreateMode == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAllowInitiationInCreateMode = value;
                    await OnPropertyChangedAsync(nameof(AllowInitiationInCreateMode), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters can spend skill points on broken groups.
        /// </summary>
        public bool UsePointsOnBrokenGroups
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnUsePointsOnBrokenGroups;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnUsePointsOnBrokenGroups == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnUsePointsOnBrokenGroups = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether characters can spend skill points on broken groups.
        /// </summary>
        public async Task<bool> GetUsePointsOnBrokenGroupsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnUsePointsOnBrokenGroups;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters can spend skill points on broken groups.
        /// </summary>
        public async Task SetUsePointsOnBrokenGroupsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUsePointsOnBrokenGroups == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUsePointsOnBrokenGroups == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnUsePointsOnBrokenGroups = value;
                    await OnPropertyChangedAsync(nameof(UsePointsOnBrokenGroups), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters in Career Mode should pay double for qualities.
        /// </summary>
        public bool DontDoubleQualityPurchases
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDontDoubleQualityPurchaseCost;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDontDoubleQualityPurchaseCost == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDontDoubleQualityPurchaseCost = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether characters in Career Mode should pay double for qualities.
        /// </summary>
        public async Task<bool> GetDontDoubleQualityPurchasesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDontDoubleQualityPurchaseCost;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters in Career Mode should pay double for qualities.
        /// </summary>
        public async Task SetDontDoubleQualityPurchasesAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDontDoubleQualityPurchaseCost == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDontDoubleQualityPurchaseCost == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDontDoubleQualityPurchaseCost = value;
                    await OnPropertyChangedAsync(nameof(DontDoubleQualityPurchases), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters in Career Mode should pay double for removing Negative Qualities.
        /// </summary>
        public bool DontDoubleQualityRefunds
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDontDoubleQualityRefundCost;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDontDoubleQualityRefundCost == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDontDoubleQualityRefundCost = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether characters in Career Mode should pay double for removing Negative Qualities.
        /// </summary>
        public async Task<bool> GetDontDoubleQualityRefundsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDontDoubleQualityRefundCost;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters in Career Mode should pay double for removing Negative Qualities.
        /// </summary>
        public async Task SetDontDoubleQualityRefundsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDontDoubleQualityRefundCost == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDontDoubleQualityRefundCost == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDontDoubleQualityRefundCost = value;
                    await OnPropertyChangedAsync(nameof(DontDoubleQualityRefunds), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to ignore the art requirements from street grimoire.
        /// </summary>
        public bool IgnoreArt
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIgnoreArt;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnIgnoreArt == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnIgnoreArt = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether to ignore the art requirements from street grimoire.
        /// </summary>
        public async Task<bool> GetIgnoreArtAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnIgnoreArt;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to ignore the art requirements from street grimoire.
        /// </summary>
        public async Task SetIgnoreArtAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIgnoreArt == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIgnoreArt == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnIgnoreArt = value;
                    await OnPropertyChangedAsync(nameof(IgnoreArt), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to ignore the limit on Complex Forms in Career mode.
        /// </summary>
        public bool IgnoreComplexFormLimit
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIgnoreComplexFormLimit;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnIgnoreComplexFormLimit == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnIgnoreComplexFormLimit = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether to ignore the limit on Complex Forms in Career mode.
        /// </summary>
        public async Task<bool> GetIgnoreComplexFormLimitAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnIgnoreComplexFormLimit;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to ignore the limit on Complex Forms in Career mode.
        /// </summary>
        public async Task SetIgnoreComplexFormLimitAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIgnoreComplexFormLimit == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIgnoreComplexFormLimit == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnIgnoreComplexFormLimit = value;
                    await OnPropertyChangedAsync(nameof(IgnoreComplexFormLimit), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to use stats from Cyberlegs when calculating movement rates
        /// </summary>
        public bool CyberlegMovement
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnCyberlegMovement;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnCyberlegMovement == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnCyberlegMovement = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether to use stats from Cyberlegs when calculating movement rates
        /// </summary>
        public async Task<bool> GetCyberlegMovementAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnCyberlegMovement;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to use stats from Cyberlegs when calculating movement rates
        /// </summary>
        public async Task SetCyberlegMovementAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnCyberlegMovement == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnCyberlegMovement == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnCyberlegMovement = value;
                    await OnPropertyChangedAsync(nameof(CyberlegMovement), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Allow Mystic Adepts to increase their power points during career mode
        /// </summary>
        public bool MysAdeptAllowPpCareer
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnMysAdeptAllowPpCareer;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnMysAdeptAllowPpCareer == value)
                        return;
                    if (value)
                    {
                        bool blnTemp = false;
                        using (LockObject.EnterWriteLock())
                        {
                            _blnMysAdeptAllowPpCareer = true;
                            if (MysAdeptSecondMAGAttribute)
                            {
                                _blnMysAdeptSecondMAGAttribute = false;
                                blnTemp = true;
                            }
                            if (blnTemp)
                                this.OnMultiplePropertyChanged(nameof(MysAdeptAllowPpCareer), nameof(MysAdeptSecondMAGAttribute));
                            else
                                OnPropertyChanged();
                        }
                    }
                    else
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnMysAdeptAllowPpCareer = false;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Allow Mystic Adepts to increase their power points during career mode
        /// </summary>
        public async Task<bool> GetMysAdeptAllowPpCareerAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnMysAdeptAllowPpCareer;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Allow Mystic Adepts to increase their power points during career mode
        /// </summary>
        public async Task SetMysAdeptAllowPpCareerAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnMysAdeptAllowPpCareer == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnMysAdeptAllowPpCareer == value)
                    return;
                if (value)
                {
                    bool blnTemp = false;
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnPrioritySpellsAsAdeptPowers = true;
                        if (await GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false))
                        {
                            _blnMysAdeptSecondMAGAttribute = false;
                            blnTemp = true;
                        }
                        if (blnTemp)
                            await this.OnMultiplePropertyChangedAsync(token, nameof(MysAdeptAllowPpCareer),
                                nameof(MysAdeptSecondMAGAttribute)).ConfigureAwait(false);
                        else
                            await OnPropertyChangedAsync(nameof(MysAdeptAllowPpCareer), token)
                                .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnMysAdeptAllowPpCareer = true;
                        await OnPropertyChangedAsync(nameof(MysAdeptAllowPpCareer), token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Split MAG for Mystic Adepts so that they have a separate MAG rating for Adept Powers instead of using the special PP rules for mystic adepts
        /// </summary>
        public bool MysAdeptSecondMAGAttribute
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnMysAdeptSecondMAGAttribute;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnMysAdeptSecondMAGAttribute == value)
                        return;
                    if (value)
                    {
                        using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                   out HashSet<string> setProperties))
                        {
                            setProperties.Add(nameof(MysAdeptSecondMAGAttribute));
                            using (LockObject.EnterWriteLock())
                            {
                                _blnMysAdeptSecondMAGAttribute = true;
                                if (PrioritySpellsAsAdeptPowers)
                                {
                                    _blnPrioritySpellsAsAdeptPowers = false;
                                    setProperties.Add(nameof(PrioritySpellsAsAdeptPowers));
                                }
                                if (MysAdeptAllowPpCareer)
                                {
                                    _blnMysAdeptAllowPpCareer = false;
                                    setProperties.Add(nameof(MysAdeptAllowPpCareer));
                                }
                                OnMultiplePropertiesChanged(setProperties);
                            }
                        }
                    }
                    else
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnMysAdeptSecondMAGAttribute = false;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Split MAG for Mystic Adepts so that they have a separate MAG rating for Adept Powers instead of using the special PP rules for mystic adepts
        /// </summary>
        public async Task<bool> GetMysAdeptSecondMAGAttributeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnMysAdeptSecondMAGAttribute;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Split MAG for Mystic Adepts so that they have a separate MAG rating for Adept Powers instead of using the special PP rules for mystic adepts
        /// </summary>
        public async Task SetMysAdeptSecondMAGAttributeAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnMysAdeptSecondMAGAttribute == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnMysAdeptSecondMAGAttribute == value)
                    return;
                if (value)
                {
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                               out HashSet<string> setProperties))
                    {
                        setProperties.Add(nameof(MysAdeptSecondMAGAttribute));
                        IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            _blnMysAdeptSecondMAGAttribute = true;
                            if (await GetPrioritySpellsAsAdeptPowersAsync(token).ConfigureAwait(false))
                            {
                                _blnPrioritySpellsAsAdeptPowers = false;
                                setProperties.Add(nameof(PrioritySpellsAsAdeptPowers));
                            }

                            if (await GetMysAdeptAllowPpCareerAsync(token).ConfigureAwait(false))
                            {
                                _blnMysAdeptAllowPpCareer = false;
                                setProperties.Add(nameof(MysAdeptAllowPpCareer));
                            }
                            await OnMultiplePropertiesChangedAsync(setProperties, token).ConfigureAwait(false);
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnMysAdeptSecondMAGAttribute = true;
                        await OnPropertyChangedAsync(nameof(MysAdeptSecondMAGAttribute), token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool MysAdeptSecondMAGAttributeEnabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return !PrioritySpellsAsAdeptPowers && !MysAdeptAllowPpCareer;
            }
        }

        public async Task<bool> GetMysAdeptSecondMAGAttributeEnabledAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !await GetPrioritySpellsAsAdeptPowersAsync(token).ConfigureAwait(false) && !await GetMysAdeptAllowPpCareerAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many contact points the character has
        /// </summary>
        public string ContactPointsExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strContactPointsExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strContactPointsExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many contact points the character has
        /// </summary>
        public async Task<string> GetContactPointsExpressionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strContactPointsExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many contact points the character has
        /// </summary>
        public async Task SetContactPointsExpressionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = value.CleanXPath().Trim('\"');
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strContactPointsExpression, value) != value)
                    await OnPropertyChangedAsync(nameof(ContactPointsExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many knowledge points the character has
        /// </summary>
        public string KnowledgePointsExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strKnowledgePointsExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strKnowledgePointsExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many knowledge points the character has
        /// </summary>
        public async Task<string> GetKnowledgePointsExpressionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strKnowledgePointsExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many knowledge points the character has
        /// </summary>
        public async Task SetKnowledgePointsExpressionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = value.CleanXPath().Trim('\"');
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strKnowledgePointsExpression, value) != value)
                    await OnPropertyChangedAsync(nameof(KnowledgePointsExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how much nuyen the character gets at character creation
        /// </summary>
        public string ChargenKarmaToNuyenExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strChargenKarmaToNuyenExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                // A safety check to make sure that we always still account for Priority-given Nuyen
                if (SettingsManager.LoadedCharacterSettings.ContainsKey(DictionaryKey)
                    && !value.Contains("{PriorityNuyen}"))
                {
                    value = '(' + value + ") + {PriorityNuyen}";
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strChargenKarmaToNuyenExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how much nuyen the character gets at character creation
        /// </summary>
        public async Task<string> GetChargenKarmaToNuyenExpressionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strChargenKarmaToNuyenExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how much nuyen the character gets at character creation
        /// </summary>
        public async Task SetChargenKarmaToNuyenExpressionAsync(string value, CancellationToken token = default)
        {
            value = value.CleanXPath().Trim('\"');
            // A safety check to make sure that we always still account for Priority-given Nuyen
            if ((await SettingsManager.GetLoadedCharacterSettingsAsync(token).ConfigureAwait(false))
                .ContainsKey(await GetDictionaryKeyAsync(token).ConfigureAwait(false))
                && !value.Contains("{PriorityNuyen}"))
            {
                value
                    = '(' + value + ") + {PriorityNuyen}";
            }

            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strChargenKarmaToNuyenExpression, value) == value)
                    return;
                await OnPropertyChangedAsync(nameof(ChargenKarmaToNuyenExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many spirits a character can bind
        /// </summary>
        public string BoundSpiritExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strBoundSpiritExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strBoundSpiritExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many spirits a character can bind
        /// </summary>
        public async Task<string> GetBoundSpiritExpressionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strBoundSpiritExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many spirits a character can bind
        /// </summary>
        public async Task SetBoundSpiritExpressionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = value.CleanXPath().Trim('\"');
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strBoundSpiritExpression, value) != value)
                    await OnPropertyChangedAsync(nameof(BoundSpiritExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many sprites a character can register
        /// </summary>
        public string RegisteredSpriteExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strRegisteredSpriteExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strRegisteredSpriteExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many sprites a character can register
        /// </summary>
        public async Task<string> GetRegisteredSpriteExpressionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strRegisteredSpriteExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine how many sprites a character can register
        /// </summary>
        public async Task SetRegisteredSpriteExpressionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = value.CleanXPath().Trim('\"');
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strRegisteredSpriteExpression, value) != value)
                    await OnPropertyChangedAsync(nameof(RegisteredSpriteExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use (if any) to modify Essence modifiers after they have all been collected
        /// </summary>
        public string EssenceModifierPostExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strEssenceModifierPostExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strEssenceModifierPostExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use (if any) to modify Essence modifiers after they have all been collected
        /// </summary>
        public async Task<string> GetEssenceModifierPostExpressionAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strEssenceModifierPostExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use (if any) to modify Essence modifiers after they have all been collected
        /// </summary>
        public async Task SetEssenceModifierPostExpressionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = value.CleanXPath().Trim('\"');
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strEssenceModifierPostExpression, value) != value)
                    await OnPropertyChangedAsync(nameof(EssenceModifierPostExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Drone Body multiplier for maximal Armor
        /// </summary>
        public int DroneArmorMultiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intDroneArmorMultiplier;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intDroneArmorMultiplier, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The Drone Body multiplier for maximal Armor
        /// </summary>
        public async Task<int> GetDroneArmorMultiplierAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intDroneArmorMultiplier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Drone Body multiplier for maximal Armor
        /// </summary>
        public async Task SetDroneArmorMultiplierAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intDroneArmorMultiplier == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intDroneArmorMultiplier, value) != value)
                    await OnPropertyChangedAsync(nameof(DroneArmorMultiplier), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Armor
        /// </summary>
        public bool DroneArmorMultiplierEnabled
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDroneArmorMultiplierEnabled;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDroneArmorMultiplierEnabled == value)
                        return;
                    if (!value)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnDroneArmorMultiplierEnabled = false;
                            if (Interlocked.Exchange(ref _intDroneArmorMultiplier, 2) != 2)
                                this.OnMultiplePropertyChanged(nameof(DroneArmorMultiplierEnabled), nameof(DroneArmorMultiplier));
                            else
                                OnPropertyChanged();
                        }
                    }
                    else
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnDroneArmorMultiplierEnabled = true;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Whether Armor
        /// </summary>
        public async Task<bool> GetDroneArmorMultiplierEnabledAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDroneArmorMultiplierEnabled;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Armor
        /// </summary>
        public async Task SetDroneArmorMultiplierEnabledAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDroneArmorMultiplierEnabled == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDroneArmorMultiplierEnabled == value)
                    return;
                if (!value)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnDroneArmorMultiplierEnabled = false;
                        if (Interlocked.Exchange(ref _intDroneArmorMultiplier, 2) != 2)
                            await this.OnMultiplePropertyChangedAsync(token, nameof(DroneArmorMultiplierEnabled), nameof(DroneArmorMultiplier)).ConfigureAwait(false);
                        else
                            await OnPropertyChangedAsync(nameof(DroneArmorMultiplierEnabled), token)
                                .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnDroneArmorMultiplierEnabled = true;
                        await OnPropertyChangedAsync(nameof(DroneArmorMultiplierEnabled), token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House Rule: Ignore Armor Encumbrance entirely.
        /// </summary>
        public bool NoArmorEncumbrance
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnNoArmorEncumbrance;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnNoArmorEncumbrance == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnNoArmorEncumbrance = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// House Rule: Ignore Armor Encumbrance entirely.
        /// </summary>
        public async Task<bool> GetNoArmorEncumbranceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnNoArmorEncumbrance;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House Rule: Ignore Armor Encumbrance entirely.
        /// </summary>
        public async Task SetNoArmorEncumbranceAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnNoArmorEncumbrance == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnNoArmorEncumbrance == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnNoArmorEncumbrance = value;
                    await OnPropertyChangedAsync(nameof(NoArmorEncumbrance), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House Rule: Do not cap armor bonuses from accessories.
        /// </summary>
        public bool UncappedArmorAccessoryBonuses
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnUncappedArmorAccessoryBonuses;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnUncappedArmorAccessoryBonuses == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnUncappedArmorAccessoryBonuses = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// House Rule: Do not cap armor bonuses from accessories.
        /// </summary>
        public async Task<bool> GetUncappedArmorAccessoryBonusesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnUncappedArmorAccessoryBonuses;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House Rule: Do not cap armor bonuses from accessories.
        /// </summary>
        public async Task SetUncappedArmorAccessoryBonusesAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUncappedArmorAccessoryBonuses == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUncappedArmorAccessoryBonuses == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnUncappedArmorAccessoryBonuses = value;
                    await OnPropertyChangedAsync(nameof(UncappedArmorAccessoryBonuses), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Essence loss only reduces MAG/RES maximum value, not the current value.
        /// </summary>
        public bool ESSLossReducesMaximumOnly
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnESSLossReducesMaximumOnly;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnESSLossReducesMaximumOnly == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnESSLossReducesMaximumOnly = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether Essence loss only reduces MAG/RES maximum value, not the current value.
        /// </summary>
        public async Task<bool> GetESSLossReducesMaximumOnlyAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnESSLossReducesMaximumOnly;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Essence loss only reduces MAG/RES maximum value, not the current value.
        /// </summary>
        public async Task SetESSLossReducesMaximumOnlyAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnESSLossReducesMaximumOnly == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnESSLossReducesMaximumOnly == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnESSLossReducesMaximumOnly = value;
                    await OnPropertyChangedAsync(nameof(ESSLossReducesMaximumOnly), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters are allowed to put points into a Skill Group again once it is broken and all Ratings are the same.
        /// </summary>
        public bool AllowSkillRegrouping
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowSkillRegrouping;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowSkillRegrouping == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowSkillRegrouping = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether characters are allowed to put points into a Skill Group again once it is broken and all Ratings are the same.
        /// </summary>
        public async Task<bool> GetAllowSkillRegroupingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAllowSkillRegrouping;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters are allowed to put points into a Skill Group again once it is broken and all Ratings are the same.
        /// </summary>
        public async Task SetAllowSkillRegroupingAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowSkillRegrouping == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowSkillRegrouping == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAllowSkillRegrouping = value;
                    await OnPropertyChangedAsync(nameof(AllowSkillRegrouping), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether specializations in an active skill (permanently) break a skill group.
        /// </summary>
        public bool SpecializationsBreakSkillGroups
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnSpecializationsBreakSkillGroups;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnSpecializationsBreakSkillGroups == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnSpecializationsBreakSkillGroups = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether specializations in an active skill (permanently) break a skill group.
        /// </summary>
        public async Task<bool> GetSpecializationsBreakSkillGroupsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnSpecializationsBreakSkillGroups;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether specializations in an active skill (permanently) break a skill group.
        /// </summary>
        public async Task SetSpecializationsBreakSkillGroupsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSpecializationsBreakSkillGroups == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSpecializationsBreakSkillGroups == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnSpecializationsBreakSkillGroups = value;
                    await OnPropertyChangedAsync(nameof(SpecializationsBreakSkillGroups), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sourcebooks.
        /// </summary>
        public HashSet<string> BooksWritable
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setBooks;
            }
        }

        /// <summary>
        /// Sourcebooks.
        /// </summary>
        public IReadOnlyCollection<string> Books
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _setBooks;
            }
        }

        /// <summary>
        /// Sourcebooks.
        /// </summary>
        public async Task<HashSet<string>> GetBooksWritableAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _setBooks;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sourcebooks.
        /// </summary>
        public async Task<IReadOnlyCollection<string>> GetBooksAsync(CancellationToken token = default)
        {
            return await GetBooksWritableAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// File name of the option (if it is not a built-in one).
        /// </summary>
        public string FileName
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFileName;
            }
        }

        /// <summary>
        /// File name of the option (if it is not a built-in one).
        /// </summary>
        public async Task<string> GetFileNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strFileName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Setting name.
        /// </summary>
        public string Name
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strName;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strName, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Setting name.
        /// </summary>
        public async Task<string> GetNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strName;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Setting name.
        /// </summary>
        public async Task SetNameAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strName == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strName, value) != value)
                    await OnPropertyChangedAsync(nameof(Name), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Setting name to display in the UI.
        /// </summary>
        public string CurrentDisplayName
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    string strReturn = Name;
                    if (BuiltInOption)
                    {
                        if (GlobalSettings.Language == GlobalSettings.DefaultLanguage)
                            return strReturn;
                        strReturn = XmlManager.LoadXPath("settings.xml")
                                              .TryGetNodeById("/chummer/settings/setting", SourceId)
                                              ?.SelectSingleNodeAndCacheExpression("translate")?.Value
                                    ?? strReturn;
                    }
                    else
                    {
                        strReturn += LanguageManager.GetString("String_Space") + '[' + FileName + ']';
                    }

                    return strReturn;
                }
            }
        }

        /// <summary>
        /// Setting name to display in the UI.
        /// </summary>
        public async Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = await GetNameAsync(token).ConfigureAwait(false);
                if (await GetBuiltInOptionAsync(token).ConfigureAwait(false))
                {
                    strReturn = (await XmlManager.LoadXPathAsync("settings.xml", token: token).ConfigureAwait(false))
                                .SelectSingleNode(
                                    "/chummer/settings/setting[id = '"
                                    + await GetSourceIdAsync(token).ConfigureAwait(false) + "']/translate")?.Value
                                ?? strReturn;
                }
                else
                {
                    strReturn += await LanguageManager.GetStringAsync("String_Space", token: token)
                                                      .ConfigureAwait(false) + '['
                                                                             + await GetFileNameAsync(token)
                                                                                 .ConfigureAwait(false) + ']';
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Metatypes cost Karma equal to their BP when creating a character with Karma.
        /// </summary>
        public bool MetatypeCostsKarma
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnMetatypeCostsKarma;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnMetatypeCostsKarma == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnMetatypeCostsKarma = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Multiplier for Metatype Karma Costs when converting from BP to Karma.
        /// </summary>
        public int MetatypeCostsKarmaMultiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMetatypeCostMultiplier;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMetatypeCostMultiplier, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Multiplier for Metatype Karma Costs when converting from BP to Karma.
        /// </summary>
        public async Task<int> GetMetatypeCostsKarmaMultiplierAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMetatypeCostMultiplier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Multiplier for Metatype Karma Costs when converting from BP to Karma.
        /// </summary>
        public async Task SetMetatypeCostsKarmaMultiplierAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMetatypeCostMultiplier == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMetatypeCostMultiplier, value) != value)
                    await OnPropertyChangedAsync(nameof(MetatypeCostsKarmaMultiplier), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of Limbs a standard character has.
        /// </summary>
        public int LimbCount
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intLimbCount;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intLimbCount, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Number of Limbs a standard character has.
        /// </summary>
        public async Task<int> GetLimbCountAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intLimbCount;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of Limbs a standard character has.
        /// </summary>
        public async Task SetLimbCountAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intLimbCount == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intLimbCount, value) != value)
                    await OnPropertyChangedAsync(nameof(LimbCount), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Exclude a particular Limb Slot from count towards the Limb Count.
        /// </summary>
        public string ExcludeLimbSlot
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strExcludeLimbSlot;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strExcludeLimbSlot, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Exclude a particular Limb Slot from count towards the Limb Count.
        /// </summary>
        public async Task<string> GetExcludeLimbSlotAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strExcludeLimbSlot;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Exclude a particular Limb Slot from count towards the Limb Count.
        /// </summary>
        public async Task SetExcludeLimbSlotAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strExcludeLimbSlot == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strExcludeLimbSlot, value) != value)
                    await OnPropertyChangedAsync(nameof(ExcludeLimbSlot), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Allow Cyberware Essence cost discounts.
        /// </summary>
        public bool AllowCyberwareESSDiscounts
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowCyberwareESSDiscounts;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowCyberwareESSDiscounts == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowCyberwareESSDiscounts = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Allow Cyberware Essence cost discounts.
        /// </summary>
        public async Task<bool> GetAllowCyberwareESSDiscountsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAllowCyberwareESSDiscounts;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Allow Cyberware Essence cost discounts.
        /// </summary>
        public async Task SetAllowCyberwareESSDiscountsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowCyberwareESSDiscounts == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowCyberwareESSDiscounts == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAllowCyberwareESSDiscounts = value;
                    await OnPropertyChangedAsync(nameof(AllowCyberwareESSDiscounts), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Armor Degradation is allowed.
        /// </summary>
        public bool ArmorDegradation
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnArmorDegradation;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnArmorDegradation == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnArmorDegradation = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether Armor Degradation is allowed.
        /// </summary>
        public async Task<bool> GetArmorDegradationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnArmorDegradation;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Armor Degradation is allowed.
        /// </summary>
        public async Task SetArmorDegradationAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnArmorDegradation == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnArmorDegradation == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnArmorDegradation = value;
                    await OnPropertyChangedAsync(nameof(ArmorDegradation), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// If true, karma costs will not decrease from reductions due to essence loss. Effectively, essence loss becomes an augmented modifier, not one that alters minima and maxima.
        /// </summary>
        public bool SpecialKarmaCostBasedOnShownValue
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnSpecialKarmaCostBasedOnShownValue;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnSpecialKarmaCostBasedOnShownValue == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnSpecialKarmaCostBasedOnShownValue = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// If true, karma costs will not decrease from reductions due to essence loss. Effectively, essence loss becomes an augmented modifier, not one that alters minima and maxima.
        /// </summary>
        public async Task<bool> GetSpecialKarmaCostBasedOnShownValueAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnSpecialKarmaCostBasedOnShownValue;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// If true, karma costs will not decrease from reductions due to essence loss. Effectively, essence loss becomes an augmented modifier, not one that alters minima and maxima.
        /// </summary>
        public async Task SetSpecialKarmaCostBasedOnShownValueAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSpecialKarmaCostBasedOnShownValue == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnSpecialKarmaCostBasedOnShownValue == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnSpecialKarmaCostBasedOnShownValue = value;
                    await OnPropertyChangedAsync(nameof(SpecialKarmaCostBasedOnShownValue), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters can have more than 25 BP in Positive Qualities.
        /// </summary>
        public bool ExceedPositiveQualities
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnExceedPositiveQualities;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnExceedPositiveQualities == value)
                        return;
                    if (!value)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnExceedPositiveQualities = false;
                            if (ExceedPositiveQualitiesCostDoubled)
                            {
                                _blnExceedPositiveQualitiesCostDoubled = false;
                                this.OnMultiplePropertyChanged(nameof(ExceedPositiveQualities), nameof(ExceedPositiveQualitiesCostDoubled));
                            }
                            else
                                OnPropertyChanged();
                        }
                    }
                    else
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnExceedPositiveQualities = true;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Whether characters can have more than 25 BP in Positive Qualities.
        /// </summary>
        public async Task<bool> GetExceedPositiveQualitiesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnExceedPositiveQualities;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters can have more than 25 BP in Positive Qualities.
        /// </summary>
        public async Task SetExceedPositiveQualitiesAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExceedPositiveQualities == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExceedPositiveQualities == value)
                    return;
                if (!value)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnExceedPositiveQualities = false;
                        if (await GetExceedPositiveQualitiesCostDoubledAsync(token).ConfigureAwait(false))
                        {
                            _blnExceedPositiveQualitiesCostDoubled = false;
                            await this.OnMultiplePropertyChangedAsync(token, nameof(ExceedPositiveQualities), nameof(ExceedPositiveQualitiesCostDoubled)).ConfigureAwait(false);
                        }
                        else
                            await OnPropertyChangedAsync(nameof(ExceedPositiveQualities), token)
                                .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnExceedPositiveQualities = true;
                        await OnPropertyChangedAsync(nameof(ExceedPositiveQualities), token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// If true, the karma cost of qualities is doubled after the initial 25.
        /// </summary>
        public bool ExceedPositiveQualitiesCostDoubled
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnExceedPositiveQualitiesCostDoubled;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnExceedPositiveQualitiesCostDoubled == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnExceedPositiveQualitiesCostDoubled = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// If true, the karma cost of qualities is doubled after the initial 25.
        /// </summary>
        public async Task<bool> GetExceedPositiveQualitiesCostDoubledAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnExceedPositiveQualitiesCostDoubled;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// If true, the karma cost of qualities is doubled after the initial 25.
        /// </summary>
        public async Task SetExceedPositiveQualitiesCostDoubledAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExceedPositiveQualitiesCostDoubled == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExceedPositiveQualitiesCostDoubled == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnExceedPositiveQualitiesCostDoubled = value;
                    await OnPropertyChangedAsync(nameof(ExceedPositiveQualitiesCostDoubled), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters can have more than 25 BP in Negative Qualities.
        /// </summary>
        public bool ExceedNegativeQualities
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnExceedNegativeQualities;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnExceedNegativeQualities == value)
                        return;
                    if (!value)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnExceedNegativeQualities = false;
                            if (ExceedNegativeQualitiesNoBonus)
                            {
                                _blnExceedNegativeQualitiesNoBonus = false;
                                this.OnMultiplePropertyChanged(nameof(ExceedNegativeQualities), nameof(ExceedNegativeQualitiesNoBonus));
                            }
                            else
                                OnPropertyChanged();
                        }
                    }
                    else
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnExceedNegativeQualities = true;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Whether characters can have more than 25 BP in Negative Qualities.
        /// </summary>
        public async Task<bool> GetExceedNegativeQualitiesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnExceedNegativeQualities;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters can have more than 25 BP in Negative Qualities.
        /// </summary>
        public async Task SetExceedNegativeQualitiesAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExceedNegativeQualities == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExceedNegativeQualities == value)
                    return;
                if (!value)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnExceedNegativeQualities = false;
                        if (await GetExceedNegativeQualitiesNoBonusAsync(token).ConfigureAwait(false))
                        {
                            _blnExceedNegativeQualitiesNoBonus = false;
                            await this.OnMultiplePropertyChangedAsync(token, nameof(ExceedNegativeQualities),
                                nameof(ExceedNegativeQualitiesNoBonus)).ConfigureAwait(false);
                        }
                        else
                            await OnPropertyChangedAsync(nameof(ExceedNegativeQualities), token)
                                .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnExceedNegativeQualities = true;
                        await OnPropertyChangedAsync(nameof(ExceedNegativeQualities), token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// If true, the character will not receive additional BP from Negative Qualities past the initial 25
        /// </summary>
        public bool ExceedNegativeQualitiesNoBonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnExceedNegativeQualitiesNoBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnExceedNegativeQualitiesNoBonus == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnExceedNegativeQualitiesNoBonus = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// If true, the character will not receive additional BP from Negative Qualities past the initial 25
        /// </summary>
        public async Task<bool> GetExceedNegativeQualitiesNoBonusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnExceedNegativeQualitiesNoBonus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// If true, the character will not receive additional BP from Negative Qualities past the initial 25
        /// </summary>
        public async Task SetExceedNegativeQualitiesNoBonusAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExceedNegativeQualitiesNoBonus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExceedNegativeQualitiesNoBonus == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnExceedNegativeQualitiesNoBonus = value;
                    await OnPropertyChangedAsync(nameof(ExceedNegativeQualitiesNoBonus), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Restricted items have their cost multiplied.
        /// </summary>
        public bool MultiplyRestrictedCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnMultiplyRestrictedCost;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnMultiplyRestrictedCost == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnMultiplyRestrictedCost = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether Forbidden items have their cost multiplied.
        /// </summary>
        public bool MultiplyForbiddenCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnMultiplyForbiddenCost;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnMultiplyForbiddenCost == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnMultiplyForbiddenCost = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Cost multiplier for Restricted items.
        /// </summary>
        public int RestrictedCostMultiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intRestrictedCostMultiplier;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intRestrictedCostMultiplier, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Cost multiplier for Forbidden items.
        /// </summary>
        public int ForbiddenCostMultiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intForbiddenCostMultiplier;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intForbiddenCostMultiplier, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        private int _intCachedMaxNuyenDecimals = int.MinValue;

        /// <summary>
        /// Maximum number of decimal places to round to when displaying nuyen values.
        /// </summary>
        public int MaxNuyenDecimals
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedMaxNuyenDecimals >= 0)
                        return _intCachedMaxNuyenDecimals;
                    string strNuyenFormat = NuyenFormat;
                    int intDecimalPlaces = strNuyenFormat.IndexOf('.');
                    if (intDecimalPlaces == -1)
                        intDecimalPlaces = 0;
                    else
                        intDecimalPlaces = strNuyenFormat.Length - intDecimalPlaces - 1;
                    Interlocked.CompareExchange(ref _intCachedMaxNuyenDecimals, intDecimalPlaces, int.MinValue);
                    return _intCachedMaxNuyenDecimals = intDecimalPlaces;
                }
            }
            set
            {
                int intNewNuyenDecimals = Math.Max(value, 0);
                using (LockObject.EnterWriteLock())
                {
                    if (MinNuyenDecimals > intNewNuyenDecimals)
                        MinNuyenDecimals = intNewNuyenDecimals;
                    if (intNewNuyenDecimals == 0)
                        return; // Already taken care of by MinNuyenDecimals
                    int intCurrentNuyenDecimals = MaxNuyenDecimals;
                    if (intNewNuyenDecimals < intCurrentNuyenDecimals)
                    {
                        NuyenFormat = NuyenFormat.Substring(
                            0, NuyenFormat.Length - (intNewNuyenDecimals - intCurrentNuyenDecimals));
                    }
                    else if (intNewNuyenDecimals > intCurrentNuyenDecimals)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdNuyenFormat))
                        {
                            sbdNuyenFormat.Append(string.IsNullOrEmpty(NuyenFormat) ? "#,0" : NuyenFormat);
                            if (intCurrentNuyenDecimals == 0)
                            {
                                sbdNuyenFormat.Append('.');
                            }

                            for (int i = intCurrentNuyenDecimals; i < intNewNuyenDecimals; ++i)
                            {
                                sbdNuyenFormat.Append('#');
                            }
                            NuyenFormat = sbdNuyenFormat.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Maximum number of decimal places to round to when displaying nuyen values.
        /// </summary>
        public async Task<int> GetMaxNuyenDecimalsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedMaxNuyenDecimals >= 0)
                    return _intCachedMaxNuyenDecimals;
                string strNuyenFormat = await GetNuyenFormatAsync(token).ConfigureAwait(false);
                int intDecimalPlaces = strNuyenFormat.IndexOf('.');
                if (intDecimalPlaces == -1)
                    intDecimalPlaces = 0;
                else
                    intDecimalPlaces = strNuyenFormat.Length - intDecimalPlaces - 1;
                Interlocked.CompareExchange(ref _intCachedMaxNuyenDecimals, intDecimalPlaces, int.MinValue);
                return _intCachedMaxNuyenDecimals = intDecimalPlaces;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of decimal places to round to when displaying nuyen values.
        /// </summary>
        public async Task SetMaxNuyenDecimalsAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intNewNuyenDecimals = Math.Max(value, 0);
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetMinNuyenDecimalsAsync(token).ConfigureAwait(false) > intNewNuyenDecimals)
                    await SetMinNuyenDecimalsAsync(intNewNuyenDecimals, token).ConfigureAwait(false);
                if (intNewNuyenDecimals == 0)
                    return; // Already taken care of by MinNuyenDecimals
                int intCurrentNuyenDecimals = await GetMaxNuyenDecimalsAsync(token).ConfigureAwait(false);
                if (intNewNuyenDecimals < intCurrentNuyenDecimals)
                {
                    string strNuyenFormat = await GetNuyenFormatAsync(token).ConfigureAwait(false);
                    await SetNuyenFormatAsync(
                        strNuyenFormat.Substring(0,
                            strNuyenFormat.Length - (intNewNuyenDecimals - intCurrentNuyenDecimals)), token).ConfigureAwait(false);
                }
                else if (intNewNuyenDecimals > intCurrentNuyenDecimals)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdNuyenFormat))
                    {
                        string strNuyenFormat = await GetNuyenFormatAsync(token).ConfigureAwait(false);
                        sbdNuyenFormat.Append(string.IsNullOrEmpty(strNuyenFormat) ? "#,0" : strNuyenFormat);
                        if (intCurrentNuyenDecimals == 0)
                        {
                            sbdNuyenFormat.Append('.');
                        }

                        for (int i = intCurrentNuyenDecimals; i < intNewNuyenDecimals; ++i)
                        {
                            sbdNuyenFormat.Append('#');
                        }

                        await SetNuyenFormatAsync(sbdNuyenFormat.ToString(), token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedMinNuyenDecimals = int.MinValue;

        /// <summary>
        /// Minimum number of decimal places to round to when displaying nuyen values.
        /// </summary>
        public int MinNuyenDecimals
        {
            get
            {
                using (LockObject.EnterReadLock())
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
                    Interlocked.CompareExchange(ref _intCachedMinNuyenDecimals, intDecimalPlaces, int.MinValue);
                    return _intCachedMinNuyenDecimals;
                }
            }
            set
            {
                int intNewNuyenDecimals = Math.Max(value, 0);
                using (LockObject.EnterWriteLock())
                {
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
        }

        /// <summary>
        /// Minimum number of decimal places to round to when displaying nuyen values.
        /// </summary>
        public async Task<int> GetMinNuyenDecimalsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedMinNuyenDecimals >= 0)
                    return _intCachedMinNuyenDecimals;
                string strNuyenFormat = await GetNuyenFormatAsync(token).ConfigureAwait(false);
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
                Interlocked.CompareExchange(ref _intCachedMinNuyenDecimals, intDecimalPlaces, int.MinValue);
                return _intCachedMinNuyenDecimals;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum number of decimal places to round to when displaying nuyen values.
        /// </summary>
        public async Task SetMinNuyenDecimalsAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intNewNuyenDecimals = Math.Max(value, 0);
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetMaxNuyenDecimalsAsync(token).ConfigureAwait(false) < intNewNuyenDecimals)
                    await SetMaxNuyenDecimalsAsync(intNewNuyenDecimals, token).ConfigureAwait(false);
                string strNuyenFormat = await GetNuyenFormatAsync(token).ConfigureAwait(false);
                int intDecimalPlaces = strNuyenFormat.IndexOf('.');
                if (intNewNuyenDecimals == 0)
                {
                    if (intDecimalPlaces != -1)
                        await SetNuyenFormatAsync(strNuyenFormat.Substring(0, intDecimalPlaces), token).ConfigureAwait(false);
                    return;
                }

                int intCurrentNuyenDecimals = await GetMinNuyenDecimalsAsync(token).ConfigureAwait(false);
                if (intNewNuyenDecimals < intCurrentNuyenDecimals)
                {
                    char[] achrNuyenFormat = strNuyenFormat.ToCharArray();
                    for (int i = intDecimalPlaces + 1 + intNewNuyenDecimals; i < achrNuyenFormat.Length; ++i)
                        achrNuyenFormat[i] = '0';
                    await SetNuyenFormatAsync(new string(achrNuyenFormat), token).ConfigureAwait(false);
                }
                else if (intNewNuyenDecimals > intCurrentNuyenDecimals)
                {
                    char[] achrNuyenFormat = strNuyenFormat.ToCharArray();
                    for (int i = 1; i < intNewNuyenDecimals; ++i)
                        achrNuyenFormat[intDecimalPlaces + i] = '0';
                    await SetNuyenFormatAsync(new string(achrNuyenFormat), token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Format in which nuyen values should be displayed (does not include nuyen symbol).
        /// </summary>
        public string NuyenFormat
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strNuyenFormat;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strNuyenFormat, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Format in which nuyen values should be displayed (does not include nuyen symbol).
        /// </summary>
        public async Task<string> GetNuyenFormatAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strNuyenFormat;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Format in which nuyen values should be displayed (does not include nuyen symbol).
        /// </summary>
        public async Task SetNuyenFormatAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strNuyenFormat == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strNuyenFormat, value) != value)
                    await OnPropertyChangedAsync(nameof(NuyenFormat), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Format in which weight values should be displayed (does not include kg units).
        /// </summary>
        public string WeightFormat
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strWeightFormat;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strWeightFormat, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Format in which weight values should be displayed (does not include kg units).
        /// </summary>
        public async Task<string> GetWeightFormatAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strWeightFormat;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Format in which weight values should be displayed (does not include kg units).
        /// </summary>
        public async Task SetWeightFormatAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strWeightFormat == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strWeightFormat, value) != value)
                    await OnPropertyChangedAsync(nameof(WeightFormat), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedWeightDecimals = int.MinValue;

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        public int WeightDecimals
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedWeightDecimals >= 0)
                        return _intCachedWeightDecimals;
                    string strWeightFormat = WeightFormat;
                    int intDecimalPlaces = strWeightFormat.IndexOf('.');
                    intDecimalPlaces = strWeightFormat.Length - intDecimalPlaces - 1;
                    Interlocked.CompareExchange(ref _intCachedWeightDecimals, intDecimalPlaces, int.MinValue);
                    return _intCachedWeightDecimals;
                }
            }
            set
            {
                int intNewWeightDecimals = Math.Max(value, 0);
                if (intNewWeightDecimals == WeightDecimals)
                    return;
                using (LockObject.EnterUpgradeableReadLock())
                {
                    int intCurrentWeightDecimals = WeightDecimals;
                    if (intNewWeightDecimals < intCurrentWeightDecimals)
                    {
                        WeightFormat
                                = WeightFormat.Substring(
                                    0, WeightFormat.Length - (intCurrentWeightDecimals - intNewWeightDecimals));
                    }
                    else if (intNewWeightDecimals > intCurrentWeightDecimals)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdWeightFormat))
                        {
                            sbdWeightFormat.Append(string.IsNullOrEmpty(WeightFormat) ? "#,0" : WeightFormat);
                            if (intCurrentWeightDecimals == 0)
                            {
                                sbdWeightFormat.Append('.');
                            }

                            for (int i = intCurrentWeightDecimals; i < intNewWeightDecimals; ++i)
                            {
                                sbdWeightFormat.Append('#');
                            }

                            WeightFormat = sbdWeightFormat.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        public async Task<int> GetWeightDecimalsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedWeightDecimals >= 0)
                    return _intCachedWeightDecimals;
                string strWeightFormat = await GetWeightFormatAsync(token).ConfigureAwait(false);
                int intDecimalPlaces = strWeightFormat.IndexOf('.');
                intDecimalPlaces = strWeightFormat.Length - intDecimalPlaces - 1;
                Interlocked.CompareExchange(ref _intCachedWeightDecimals, intDecimalPlaces, int.MinValue);
                return _intCachedWeightDecimals;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        public async Task SetWeightDecimalsAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intNewWeightDecimals = Math.Max(value, 0);
            if (intNewWeightDecimals == await GetWeightDecimalsAsync(token).ConfigureAwait(false))
                return;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCurrentWeightDecimals = await GetWeightDecimalsAsync(token).ConfigureAwait(false);
                if (intNewWeightDecimals < intCurrentWeightDecimals)
                {
                    string strWeightFormat = await GetWeightFormatAsync(token).ConfigureAwait(false);
                    await SetWeightFormatAsync(strWeightFormat.Substring(
                        0, strWeightFormat.Length - (intCurrentWeightDecimals - intNewWeightDecimals)), token).ConfigureAwait(false);
                }
                else if (intNewWeightDecimals > intCurrentWeightDecimals)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdWeightFormat))
                    {
                        string strWeightFormat = await GetWeightFormatAsync(token).ConfigureAwait(false);
                        sbdWeightFormat.Append(string.IsNullOrEmpty(strWeightFormat) ? "#,0" : strWeightFormat);
                        if (intCurrentWeightDecimals == 0)
                        {
                            sbdWeightFormat.Append('.');
                        }

                        for (int i = intCurrentWeightDecimals; i < intNewWeightDecimals; ++i)
                        {
                            sbdWeightFormat.Append('#');
                        }

                        await SetWeightFormatAsync(sbdWeightFormat.ToString(), token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the maximum weight the character can lift in kg
        /// </summary>
        public string LiftLimitExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strLiftLimitExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strLiftLimitExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the maximum weight the character can lift in kg
        /// </summary>
        public async Task<string> GetLiftLimitExpressionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strLiftLimitExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the maximum weight the character can lift in kg
        /// </summary>
        public async Task SetLiftLimitExpressionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = value.CleanXPath().Trim('\"');
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strLiftLimitExpression, value) != value)
                    await OnPropertyChangedAsync(nameof(LiftLimitExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the maximum weight the character can carry in kg
        /// </summary>
        public string CarryLimitExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCarryLimitExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strCarryLimitExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the maximum weight the character can carry in kg
        /// </summary>
        public async Task<string> GetCarryLimitExpressionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strCarryLimitExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the maximum weight the character can carry in kg
        /// </summary>
        public async Task SetCarryLimitExpressionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = value.CleanXPath().Trim('\"');
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strCarryLimitExpression, value) != value)
                    await OnPropertyChangedAsync(nameof(CarryLimitExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the amount of weight necessary to increase encumbrance penalties by one tick
        /// </summary>
        public string EncumbranceIntervalExpression
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strEncumbranceIntervalExpression;
            }
            set
            {
                value = value.CleanXPath().Trim('\"');
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strEncumbranceIntervalExpression, value) == value)
                        return;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the amount of weight necessary to increase encumbrance penalties by one tick
        /// </summary>
        public async Task<string> GetEncumbranceIntervalExpressionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strEncumbranceIntervalExpression;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The XPath expression to use to determine the amount of weight necessary to increase encumbrance penalties by one tick
        /// </summary>
        public async Task SetEncumbranceIntervalExpressionAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = value.CleanXPath().Trim('\"');
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strEncumbranceIntervalExpression, value) != value)
                    await OnPropertyChangedAsync(nameof(EncumbranceIntervalExpression), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Physical Limit from encumbrance?
        /// </summary>
        public bool DoEncumbrancePenaltyPhysicalLimit
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDoEncumbrancePenaltyPhysicalLimit;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDoEncumbrancePenaltyPhysicalLimit == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDoEncumbrancePenaltyPhysicalLimit = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Should we apply a penalty to Physical Limit from encumbrance?
        /// </summary>
        public async Task<bool> GetDoEncumbrancePenaltyPhysicalLimitAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDoEncumbrancePenaltyPhysicalLimit;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Physical Limit from encumbrance?
        /// </summary>
        public async Task SetDoEncumbrancePenaltyPhysicalLimitAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyPhysicalLimit == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyPhysicalLimit == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDoEncumbrancePenaltyPhysicalLimit = value;
                    await OnPropertyChangedAsync(nameof(DoEncumbrancePenaltyPhysicalLimit), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Physical Limit that should come from one encumbrance tick
        /// </summary>
        public int EncumbrancePenaltyPhysicalLimit
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intEncumbrancePenaltyPhysicalLimit;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intEncumbrancePenaltyPhysicalLimit, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The penalty to Physical Limit that should come from one encumbrance tick
        /// </summary>
        public async Task<int> GetEncumbrancePenaltyPhysicalLimitAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intEncumbrancePenaltyPhysicalLimit;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Physical Limit that should come from one encumbrance tick
        /// </summary>
        public async Task SetEncumbrancePenaltyPhysicalLimitAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intEncumbrancePenaltyPhysicalLimit == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intEncumbrancePenaltyPhysicalLimit, value) != value)
                    await OnPropertyChangedAsync(nameof(EncumbrancePenaltyPhysicalLimit), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Movement Speeds from encumbrance?
        /// </summary>
        public bool DoEncumbrancePenaltyMovementSpeed
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDoEncumbrancePenaltyMovementSpeed;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDoEncumbrancePenaltyMovementSpeed == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDoEncumbrancePenaltyMovementSpeed = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Should we apply a penalty to Movement Speeds from encumbrance?
        /// </summary>
        public async Task<bool> GetDoEncumbrancePenaltyMovementSpeedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDoEncumbrancePenaltyMovementSpeed;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Movement Speeds from encumbrance?
        /// </summary>
        public async Task SetDoEncumbrancePenaltyMovementSpeedAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyMovementSpeed == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyMovementSpeed == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDoEncumbrancePenaltyMovementSpeed = value;
                    await OnPropertyChangedAsync(nameof(DoEncumbrancePenaltyMovementSpeed), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Movement Speeds that should come from one encumbrance tick
        /// </summary>
        public int EncumbrancePenaltyMovementSpeed
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intEncumbrancePenaltyMovementSpeed;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intEncumbrancePenaltyMovementSpeed, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The penalty to Movement Speeds that should come from one encumbrance tick
        /// </summary>
        public async Task<int> GetEncumbrancePenaltyMovementSpeedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intEncumbrancePenaltyMovementSpeed;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Movement Speeds that should come from one encumbrance tick
        /// </summary>
        public async Task SetEncumbrancePenaltyMovementSpeedAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intEncumbrancePenaltyMovementSpeed == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intEncumbrancePenaltyMovementSpeed, value) != value)
                    await OnPropertyChangedAsync(nameof(EncumbrancePenaltyMovementSpeed), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Agility from encumbrance?
        /// </summary>
        public bool DoEncumbrancePenaltyAgility
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDoEncumbrancePenaltyAgility;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDoEncumbrancePenaltyAgility == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDoEncumbrancePenaltyAgility = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Should we apply a penalty to Agility from encumbrance?
        /// </summary>
        public async Task<bool> GetDoEncumbrancePenaltyAgilityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDoEncumbrancePenaltyAgility;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Agility from encumbrance?
        /// </summary>
        public async Task SetDoEncumbrancePenaltyAgilityAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyAgility == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyAgility == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDoEncumbrancePenaltyAgility = value;
                    await OnPropertyChangedAsync(nameof(DoEncumbrancePenaltyAgility), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Agility that should come from one encumbrance tick
        /// </summary>
        public int EncumbrancePenaltyAgility
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intEncumbrancePenaltyAgility;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intEncumbrancePenaltyAgility, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The penalty to Agility that should come from one encumbrance tick
        /// </summary>
        public async Task<int> GetEncumbrancePenaltyAgilityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intEncumbrancePenaltyAgility;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Agility that should come from one encumbrance tick
        /// </summary>
        public async Task SetEncumbrancePenaltyAgilityAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intEncumbrancePenaltyAgility == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intEncumbrancePenaltyAgility, value) != value)
                    await OnPropertyChangedAsync(nameof(EncumbrancePenaltyAgility), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Reaction from encumbrance?
        /// </summary>
        public bool DoEncumbrancePenaltyReaction
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDoEncumbrancePenaltyReaction;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDoEncumbrancePenaltyReaction == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDoEncumbrancePenaltyReaction = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Should we apply a penalty to Reaction from encumbrance?
        /// </summary>
        public async Task<bool> GetDoEncumbrancePenaltyReactionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDoEncumbrancePenaltyReaction;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Reaction from encumbrance?
        /// </summary>
        public async Task SetDoEncumbrancePenaltyReactionAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyReaction == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyReaction == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDoEncumbrancePenaltyReaction = value;
                    await OnPropertyChangedAsync(nameof(DoEncumbrancePenaltyAgility), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Reaction that should come from one encumbrance tick
        /// </summary>
        public int EncumbrancePenaltyReaction
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intEncumbrancePenaltyReaction;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intEncumbrancePenaltyReaction, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The penalty to Reaction that should come from one encumbrance tick
        /// </summary>
        public async Task<int> GetEncumbrancePenaltyReactionAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intEncumbrancePenaltyReaction;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Reaction that should come from one encumbrance tick
        /// </summary>
        public async Task SetEncumbrancePenaltyReactionAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intEncumbrancePenaltyReaction == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intEncumbrancePenaltyReaction, value) != value)
                    await OnPropertyChangedAsync(nameof(EncumbrancePenaltyReaction), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Physical Active and Weapon skills from encumbrance?
        /// </summary>
        public bool DoEncumbrancePenaltyWoundModifier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDoEncumbrancePenaltyWoundModifier;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDoEncumbrancePenaltyWoundModifier == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDoEncumbrancePenaltyWoundModifier = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Should we apply a penalty to Physical Active and Weapon skills from encumbrance?
        /// </summary>
        public async Task<bool> GetDoEncumbrancePenaltyWoundModifierAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDoEncumbrancePenaltyWoundModifier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Should we apply a penalty to Physical Active and Weapon skills from encumbrance?
        /// </summary>
        public async Task SetDoEncumbrancePenaltyWoundModifierAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyWoundModifier == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoEncumbrancePenaltyWoundModifier == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDoEncumbrancePenaltyWoundModifier = value;
                    await OnPropertyChangedAsync(nameof(DoEncumbrancePenaltyWoundModifier), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Physical Active and Weapon skills that should come from one encumbrance tick
        /// </summary>
        public int EncumbrancePenaltyWoundModifier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intEncumbrancePenaltyWoundModifier;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intEncumbrancePenaltyWoundModifier, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The penalty to Physical Active and Weapon skills that should come from one encumbrance tick
        /// </summary>
        public async Task<int> GetEncumbrancePenaltyWoundModifierAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intEncumbrancePenaltyWoundModifier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The penalty to Physical Active and Weapon skills that should come from one encumbrance tick
        /// </summary>
        public async Task SetEncumbrancePenaltyWoundModifierAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intEncumbrancePenaltyWoundModifier == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intEncumbrancePenaltyWoundModifier, value) != value)
                    await OnPropertyChangedAsync(nameof(EncumbrancePenaltyWoundModifier), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intCachedEssenceDecimals = int.MinValue;

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        public int EssenceDecimals
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intCachedEssenceDecimals >= 0)
                        return _intCachedEssenceDecimals;
                    string strEssenceFormat = EssenceFormat;
                    int intDecimalPlaces = strEssenceFormat.IndexOf('.');
                    intDecimalPlaces = strEssenceFormat.Length - intDecimalPlaces - 1;
                    Interlocked.CompareExchange(ref _intCachedEssenceDecimals, intDecimalPlaces, int.MinValue);
                    return _intCachedEssenceDecimals;
                }
            }
            set
            {
                int intNewEssenceDecimals = Math.Max(value, 2);
                if (intNewEssenceDecimals == EssenceDecimals)
                    return;
                using (LockObject.EnterUpgradeableReadLock())
                {
                    int intCurrentEssenceDecimals = EssenceDecimals;
                    if (intNewEssenceDecimals < intCurrentEssenceDecimals)
                    {
                        EssenceFormat
                            = EssenceFormat.Substring(
                                0, EssenceFormat.Length - (intCurrentEssenceDecimals - intNewEssenceDecimals));
                    }
                    else if (intNewEssenceDecimals > intCurrentEssenceDecimals)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdEssenceFormat))
                        {
                            sbdEssenceFormat.Append(string.IsNullOrEmpty(EssenceFormat) ? "#,0" : EssenceFormat);
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
            }
        }

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        public async Task<int> GetEssenceDecimalsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCachedEssenceDecimals >= 0)
                    return _intCachedEssenceDecimals;
                string strEssenceFormat = await GetEssenceFormatAsync(token).ConfigureAwait(false);
                int intDecimalPlaces = strEssenceFormat.IndexOf('.');
                intDecimalPlaces = strEssenceFormat.Length - intDecimalPlaces - 1;
                Interlocked.CompareExchange(ref _intCachedEssenceDecimals, intDecimalPlaces, int.MinValue);
                return _intCachedEssenceDecimals;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Number of decimal places to round to when calculating Essence.
        /// </summary>
        public async Task SetEssenceDecimalsAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intNewEssenceDecimals = Math.Max(value, 2);
            if (intNewEssenceDecimals == await GetEssenceDecimalsAsync(token).ConfigureAwait(false))
                return;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCurrentEssenceDecimals = await GetEssenceDecimalsAsync(token).ConfigureAwait(false);
                if (intNewEssenceDecimals < intCurrentEssenceDecimals)
                {
                    string strEssenceFormat = await GetEssenceFormatAsync(token).ConfigureAwait(false);
                    await SetEssenceFormatAsync(strEssenceFormat.Substring(
                        0, strEssenceFormat.Length - (intCurrentEssenceDecimals - intNewEssenceDecimals)), token).ConfigureAwait(false);
                }
                else if (intNewEssenceDecimals > intCurrentEssenceDecimals)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdEssenceFormat))
                    {
                        string strEssenceFormat = await GetEssenceFormatAsync(token).ConfigureAwait(false);
                        sbdEssenceFormat.Append(string.IsNullOrEmpty(strEssenceFormat) ? "#,0" : strEssenceFormat);
                        if (intCurrentEssenceDecimals == 0)
                        {
                            sbdEssenceFormat.Append('.');
                        }

                        for (int i = intCurrentEssenceDecimals; i < intNewEssenceDecimals; ++i)
                        {
                            sbdEssenceFormat.Append('0');
                        }

                        await SetEssenceFormatAsync(sbdEssenceFormat.ToString(), token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Display format for Essence.
        /// </summary>
        public string EssenceFormat
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strEssenceFormat;
            }
            set
            {
                int intDecimalPlaces = value.IndexOf('.');
                if (intDecimalPlaces < 2)
                {
                    if (intDecimalPlaces == -1)
                        value += ".00";
                    else
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdZeros))
                        {
                            sbdZeros.Append(value);
                            for (int i = value.Length - 1 - intDecimalPlaces; i < intDecimalPlaces; ++i)
                                sbdZeros.Append('0');
                            value = sbdZeros.ToString();
                        }
                    }
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strEssenceFormat, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Display format for Essence.
        /// </summary>
        public async Task<string> GetEssenceFormatAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strEssenceFormat;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Display format for Essence.
        /// </summary>
        public async Task SetEssenceFormatAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strEssenceFormat == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _strEssenceFormat, value) != value)
                    await OnPropertyChangedAsync(nameof(EssenceFormat), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Only round essence when its value is displayed
        /// </summary>
        public bool DontRoundEssenceInternally
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDoNotRoundEssenceInternally;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDoNotRoundEssenceInternally == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDoNotRoundEssenceInternally = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Only round essence when its value is displayed
        /// </summary>
        public async Task<bool> GetDontRoundEssenceInternallyAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDoNotRoundEssenceInternally;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Only round essence when its value is displayed
        /// </summary>
        public async Task SetDontRoundEssenceInternallyAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoNotRoundEssenceInternally == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDoNotRoundEssenceInternally == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDoNotRoundEssenceInternally = value;
                    await OnPropertyChangedAsync(nameof(DontRoundEssenceInternally), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Allow Enemies to be bought and tracked like in 4e?
        /// </summary>
        public bool EnableEnemyTracking
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnEnableEnemyTracking;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnEnableEnemyTracking == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnEnableEnemyTracking = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Allow Enemies to be bought and tracked like in 4e?
        /// </summary>
        public async Task<bool> GetEnableEnemyTrackingAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnEnableEnemyTracking;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Allow Enemies to be bought and tracked like in 4e?
        /// </summary>
        public async Task SetEnableEnemyTrackingAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnEnableEnemyTracking == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnEnableEnemyTracking == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnEnableEnemyTracking = value;
                    await OnPropertyChangedAsync(nameof(EnableEnemyTracking), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Do Enemies count towards Negative Quality Karma limit in create mode?
        /// </summary>
        public bool EnemyKarmaQualityLimit
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnEnemyKarmaQualityLimit;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnEnemyKarmaQualityLimit == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnEnemyKarmaQualityLimit = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Do Enemies count towards Negative Quality Karma limit in create mode?
        /// </summary>
        public async Task<bool> GetEnemyKarmaQualityLimitAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnEnemyKarmaQualityLimit;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Do Enemies count towards Negative Quality Karma limit in create mode?
        /// </summary>
        public async Task SetEnemyKarmaQualityLimitAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnEnemyKarmaQualityLimit == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnEnemyKarmaQualityLimit == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnEnemyKarmaQualityLimit = value;
                    await OnPropertyChangedAsync(nameof(EnemyKarmaQualityLimit), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Capacity limits should be enforced.
        /// </summary>
        public bool EnforceCapacity
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnEnforceCapacity;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnEnforceCapacity == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnEnforceCapacity = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether Capacity limits should be enforced.
        /// </summary>
        public async Task<bool> GetEnforceCapacityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnEnforceCapacity;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Capacity limits should be enforced.
        /// </summary>
        public async Task SetEnforceCapacityAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnEnforceCapacity == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnEnforceCapacity == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnEnforceCapacity = value;
                    await OnPropertyChangedAsync(nameof(EnforceCapacity), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Recoil modifiers are restricted (AR 148).
        /// </summary>
        public bool RestrictRecoil
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnRestrictRecoil;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnRestrictRecoil == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnRestrictRecoil = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether Recoil modifiers are restricted (AR 148).
        /// </summary>
        public async Task<bool> GetRestrictRecoilAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnRestrictRecoil;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Recoil modifiers are restricted (AR 148).
        /// </summary>
        public async Task SetRestrictRecoilAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnRestrictRecoil == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnRestrictRecoil == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnRestrictRecoil = value;
                    await OnPropertyChangedAsync(nameof(RestrictRecoil), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters are unrestricted in the number of points they can invest in Nuyen.
        /// </summary>
        public bool UnrestrictedNuyen
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnUnrestrictedNuyen;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnUnrestrictedNuyen == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnUnrestrictedNuyen = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether characters are unrestricted in the number of points they can invest in Nuyen.
        /// </summary>
        public async Task<bool> GetUnrestrictedNuyenAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnUnrestrictedNuyen;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether characters are unrestricted in the number of points they can invest in Nuyen.
        /// </summary>
        public async Task SetUnrestrictedNuyenAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUnrestrictedNuyen == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUnrestrictedNuyen == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnUnrestrictedNuyen = value;
                    await OnPropertyChangedAsync(nameof(UnrestrictedNuyen), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Stacked Foci can have a combined Force higher than 6.
        /// </summary>
        public bool AllowHigherStackedFoci
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowHigherStackedFoci;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowHigherStackedFoci == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowHigherStackedFoci = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the user can change the Part of Base Weapon flag for a Weapon Accessory or Mod.
        /// </summary>
        public bool AllowEditPartOfBaseWeapon
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowEditPartOfBaseWeapon;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowEditPartOfBaseWeapon == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowEditPartOfBaseWeapon = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the user is allowed to break Skill Groups while in Create Mode.
        /// </summary>
        public bool StrictSkillGroupsInCreateMode
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnStrictSkillGroupsInCreateMode;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnStrictSkillGroupsInCreateMode == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnStrictSkillGroupsInCreateMode = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the user is allowed to break Skill Groups while in Create Mode.
        /// </summary>
        public async Task<bool> GetStrictSkillGroupsInCreateModeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnStrictSkillGroupsInCreateMode;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the user is allowed to break Skill Groups while in Create Mode.
        /// </summary>
        public async Task SetStrictSkillGroupsInCreateModeAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnStrictSkillGroupsInCreateMode == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnStrictSkillGroupsInCreateMode == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnStrictSkillGroupsInCreateMode = value;
                    await OnPropertyChangedAsync(nameof(StrictSkillGroupsInCreateMode), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the user is allowed to buy specializations with skill points for skills only bought with karma.
        /// </summary>
        public bool AllowPointBuySpecializationsOnKarmaSkills
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowPointBuySpecializationsOnKarmaSkills;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowPointBuySpecializationsOnKarmaSkills == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowPointBuySpecializationsOnKarmaSkills = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the user is allowed to buy specializations with skill points for skills only bought with karma.
        /// </summary>
        public async Task<bool> GetAllowPointBuySpecializationsOnKarmaSkillsAsync(
            CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAllowPointBuySpecializationsOnKarmaSkills;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the user is allowed to buy specializations with skill points for skills only bought with karma.
        /// </summary>
        public async Task SetAllowPointBuySpecializationsOnKarmaSkillsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowPointBuySpecializationsOnKarmaSkills == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowPointBuySpecializationsOnKarmaSkills == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAllowPointBuySpecializationsOnKarmaSkills = value;
                    await OnPropertyChangedAsync(nameof(AllowPointBuySpecializationsOnKarmaSkills), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether any Detection Spell can be taken as Extended range version.
        /// </summary>
        public bool ExtendAnyDetectionSpell
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnExtendAnyDetectionSpell;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnExtendAnyDetectionSpell == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnExtendAnyDetectionSpell = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether any Detection Spell can be taken as Extended range version.
        /// </summary>
        public async Task<bool> GetExtendAnyDetectionSpellAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnExtendAnyDetectionSpell;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether any Detection Spell can be taken as Extended range version.
        /// </summary>
        public async Task SetExtendAnyDetectionSpellAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExtendAnyDetectionSpell == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnExtendAnyDetectionSpell == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnExtendAnyDetectionSpell = value;
                    await OnPropertyChangedAsync(nameof(ExtendAnyDetectionSpell), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether cyberlimbs stats are used in attribute calculation
        /// </summary>
        public bool DontUseCyberlimbCalculation
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDontUseCyberlimbCalculation;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDontUseCyberlimbCalculation == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDontUseCyberlimbCalculation = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether cyberlimbs stats are used in attribute calculation
        /// </summary>
        public async Task<bool> GetDontUseCyberlimbCalculationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDontUseCyberlimbCalculation;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether cyberlimbs stats are used in attribute calculation
        /// </summary>
        public async Task SetDontUseCyberlimbCalculationAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDontUseCyberlimbCalculation == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDontUseCyberlimbCalculation == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDontUseCyberlimbCalculation = value;
                    await OnPropertyChangedAsync(nameof(DontUseCyberlimbCalculation), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
        /// </summary>
        public bool AlternateMetatypeAttributeKarma
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAlternateMetatypeAttributeKarma;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAlternateMetatypeAttributeKarma == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAlternateMetatypeAttributeKarma = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
        /// </summary>
        public async Task<bool> GetAlternateMetatypeAttributeKarmaAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAlternateMetatypeAttributeKarma;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House rule: Treat the Metatype Attribute Minimum as 1 for the purpose of calculating Karma costs.
        /// </summary>
        public async Task SetAlternateMetatypeAttributeKarmaAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAlternateMetatypeAttributeKarma == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAlternateMetatypeAttributeKarma == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAlternateMetatypeAttributeKarma = value;
                    await OnPropertyChangedAsync(nameof(AlternateMetatypeAttributeKarma), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
        /// </summary>
        public bool CompensateSkillGroupKarmaDifference
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnCompensateSkillGroupKarmaDifference;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnCompensateSkillGroupKarmaDifference == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnCompensateSkillGroupKarmaDifference = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
        /// </summary>
        public async Task<bool> GetCompensateSkillGroupKarmaDifferenceAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnCompensateSkillGroupKarmaDifference;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House rule: Whether to compensate for the karma cost difference between raising skill ratings and skill groups when increasing the rating of the last skill in the group
        /// </summary>
        public async Task SetCompensateSkillGroupKarmaDifferenceAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnCompensateSkillGroupKarmaDifference == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnCompensateSkillGroupKarmaDifference == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnCompensateSkillGroupKarmaDifference = value;
                    await OnPropertyChangedAsync(nameof(CompensateSkillGroupKarmaDifference), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Bioware Suites can be added and created.
        /// </summary>
        public bool AllowBiowareSuites
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowBiowareSuites;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowBiowareSuites == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowBiowareSuites = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
        /// </summary>
        public bool FreeSpiritPowerPointsMAG
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnFreeSpiritPowerPointsMAG;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnFreeSpiritPowerPointsMAG == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnFreeSpiritPowerPointsMAG = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
        /// </summary>
        public async Task<bool> GetFreeSpiritPowerPointsMAGAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnFreeSpiritPowerPointsMAG;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House rule: Free Spirits calculate their Power Points based on their MAG instead of EDG.
        /// </summary>
        public async Task SetFreeSpiritPowerPointsMAGAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFreeSpiritPowerPointsMAG == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFreeSpiritPowerPointsMAG == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnFreeSpiritPowerPointsMAG = value;
                    await OnPropertyChangedAsync(nameof(FreeSpiritPowerPointsMAG), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// House rule: Attribute values are clamped to 0 or are allowed to go below 0 due to Essence Loss.
        /// </summary>
        public bool UnclampAttributeMinimum
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnUnclampAttributeMinimum;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnUnclampAttributeMinimum == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnUnclampAttributeMinimum = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Use Rigger 5.0 drone modding rules
        /// </summary>
        public bool DroneMods
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDroneMods;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDroneMods == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDroneMods = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Use Rigger 5.0 drone modding rules
        /// </summary>
        public async Task<bool> GetDroneModsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDroneMods;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Use Rigger 5.0 drone modding rules
        /// </summary>
        public async Task SetDroneModsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDroneMods == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDroneMods == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDroneMods = value;
                    await OnPropertyChangedAsync(nameof(DroneMods), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Apply drone mod attribute maximum rule to Pilot, too
        /// </summary>
        public bool DroneModsMaximumPilot
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDroneModsMaximumPilot;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDroneModsMaximumPilot == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnDroneModsMaximumPilot = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Apply drone mod attribute maximum rule to Pilot, too
        /// </summary>
        public async Task<bool> GetDroneModsMaximumPilotAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDroneModsMaximumPilot;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Apply drone mod attribute maximum rule to Pilot, too
        /// </summary>
        public async Task SetDroneModsMaximumPilotAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDroneModsMaximumPilot == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDroneModsMaximumPilot == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDroneModsMaximumPilot = value;
                    await OnPropertyChangedAsync(nameof(DroneModsMaximumPilot), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of attributes at metatype maximum in character creation
        /// </summary>
        public int MaxNumberMaxAttributesCreate
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxNumberMaxAttributesCreate;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxNumberMaxAttributesCreate, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum number of attributes at metatype maximum in character creation
        /// </summary>
        public async Task<int> GetMaxNumberMaxAttributesCreateAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxNumberMaxAttributesCreate;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of attributes at metatype maximum in character creation
        /// </summary>
        public async Task SetMaxNumberMaxAttributesCreateAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxNumberMaxAttributesCreate == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxNumberMaxAttributesCreate, value) != value)
                    await OnPropertyChangedAsync(nameof(MaxNumberMaxAttributesCreate), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum skill rating in character creation
        /// </summary>
        public int MaxSkillRatingCreate
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxSkillRatingCreate;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxSkillRatingCreate, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum skill rating in character creation
        /// </summary>
        public async Task<int> GetMaxSkillRatingCreateAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxSkillRatingCreate;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum skill rating in character creation
        /// </summary>
        public async Task SetMaxSkillRatingCreateAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxSkillRatingCreate == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxSkillRatingCreate, value) != value)
                    await OnPropertyChangedAsync(nameof(MaxSkillRatingCreate), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum knowledge skill rating in character creation
        /// </summary>
        public int MaxKnowledgeSkillRatingCreate
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxKnowledgeSkillRatingCreate;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxKnowledgeSkillRatingCreate, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum knowledge skill rating in character creation
        /// </summary>
        public async Task<int> GetMaxKnowledgeSkillRatingCreateAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxKnowledgeSkillRatingCreate;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum knowledge skill rating in character creation
        /// </summary>
        public async Task SetMaxKnowledgeSkillRatingCreateAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxKnowledgeSkillRatingCreate == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxKnowledgeSkillRatingCreate, value) != value)
                    await OnPropertyChangedAsync(nameof(MaxKnowledgeSkillRatingCreate), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum skill rating
        /// </summary>
        public int MaxSkillRating
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxSkillRating;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxSkillRating, value) == value)
                        return;
                    if (MaxSkillRatingCreate > value)
                    {
                        if (Interlocked.Exchange(ref _intMaxSkillRatingCreate, value) != value)
                            this.OnMultiplePropertyChanged(nameof(MaxSkillRating), nameof(MaxSkillRatingCreate));
                        else
                            OnPropertyChanged();
                    }
                    else
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum skill rating
        /// </summary>
        public async Task<int> GetMaxSkillRatingAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxSkillRating;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum skill rating
        /// </summary>
        public async Task SetMaxSkillRatingAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxSkillRating == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxSkillRating, value) == value)
                    return;
                if (await GetMaxSkillRatingCreateAsync(token).ConfigureAwait(false) > value)
                {
                    if (Interlocked.Exchange(ref _intMaxSkillRatingCreate, value) != value)
                        await this.OnMultiplePropertyChangedAsync(token, nameof(MaxSkillRating), nameof(MaxSkillRatingCreate)).ConfigureAwait(false);
                    else
                        await OnPropertyChangedAsync(nameof(MaxSkillRating), token).ConfigureAwait(false);
                }
                else
                    await OnPropertyChangedAsync(nameof(MaxSkillRating), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum knowledge skill rating
        /// </summary>
        public int MaxKnowledgeSkillRating
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxKnowledgeSkillRating;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxKnowledgeSkillRating, value) == value)
                        return;
                    if (MaxKnowledgeSkillRatingCreate > value)
                    {
                        if (Interlocked.Exchange(ref _intMaxKnowledgeSkillRatingCreate, value) != value)
                            this.OnMultiplePropertyChanged(nameof(MaxKnowledgeSkillRating), nameof(MaxKnowledgeSkillRatingCreate));
                        else
                            OnPropertyChanged();
                    }
                    else
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum knowledge skill rating
        /// </summary>
        public async Task<int> GetMaxKnowledgeSkillRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxKnowledgeSkillRating;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum knowledge skill rating
        /// </summary>
        public async Task SetMaxKnowledgeSkillRatingAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxKnowledgeSkillRating == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxKnowledgeSkillRating, value) == value)
                    return;
                if (await GetMaxSkillRatingCreateAsync(token).ConfigureAwait(false) > value)
                {
                    if (Interlocked.Exchange(ref _intMaxKnowledgeSkillRatingCreate, value) != value)
                        await this.OnMultiplePropertyChangedAsync(token, nameof(MaxKnowledgeSkillRating), nameof(MaxKnowledgeSkillRatingCreate)).ConfigureAwait(false);
                    else
                        await OnPropertyChangedAsync(nameof(MaxKnowledgeSkillRating), token).ConfigureAwait(false);
                }
                else
                    await OnPropertyChangedAsync(nameof(MaxKnowledgeSkillRating), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Life Modules should automatically generate a character background.
        /// </summary>
        public bool AutomaticBackstory
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAutomaticBackstory;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAutomaticBackstory == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAutomaticBackstory = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether Life Modules should automatically generate a character background.
        /// </summary>
        public async Task<bool> GetAutomaticBackstoryAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAutomaticBackstory;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to use the rules from SR4 to calculate Public Awareness.
        /// </summary>
        public bool UseCalculatedPublicAwareness
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnUseCalculatedPublicAwareness;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnUseCalculatedPublicAwareness == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnUseCalculatedPublicAwareness = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether to use the rules from SR4 to calculate Public Awareness.
        /// </summary>
        public async Task<bool> GetUseCalculatedPublicAwarenessAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnUseCalculatedPublicAwareness;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether to use the rules from SR4 to calculate Public Awareness.
        /// </summary>
        public async Task SetUseCalculatedPublicAwarenessAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUseCalculatedPublicAwareness == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnUseCalculatedPublicAwareness == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnUseCalculatedPublicAwareness = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(UseCalculatedPublicAwareness), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Martial Arts grant a free specialization in a skill.
        /// </summary>
        public bool FreeMartialArtSpecialization
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnFreeMartialArtSpecialization;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnFreeMartialArtSpecialization == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnFreeMartialArtSpecialization = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether Martial Arts grant a free specialization in a skill.
        /// </summary>
        public async Task<bool> GetFreeMartialArtSpecializationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnFreeMartialArtSpecialization;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Martial Arts grant a free specialization in a skill.
        /// </summary>
        public async Task SetFreeMartialArtSpecializationAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFreeMartialArtSpecialization == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnFreeMartialArtSpecialization == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnFreeMartialArtSpecialization = value;
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                await OnPropertyChangedAsync(nameof(FreeMartialArtSpecialization), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Spells from Magic Priority can also be spent on power points.
        /// </summary>
        public bool PrioritySpellsAsAdeptPowers
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnPrioritySpellsAsAdeptPowers;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnPrioritySpellsAsAdeptPowers == value)
                        return;
                    if (value)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnPrioritySpellsAsAdeptPowers = true;
                            if (MysAdeptSecondMAGAttribute)
                            {
                                _blnMysAdeptSecondMAGAttribute = false;
                                this.OnMultiplePropertyChanged(nameof(PrioritySpellsAsAdeptPowers), nameof(MysAdeptSecondMAGAttribute));
                            }
                            else
                                OnPropertyChanged();
                        }
                    }
                    else
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            _blnPrioritySpellsAsAdeptPowers = false;
                            OnPropertyChanged();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Whether Spells from Magic Priority can also be spent on power points.
        /// </summary>
        public async Task<bool> GetPrioritySpellsAsAdeptPowersAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnPrioritySpellsAsAdeptPowers;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Spells from Magic Priority can also be spent on power points.
        /// </summary>
        public async Task SetPrioritySpellsAsAdeptPowersAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnPrioritySpellsAsAdeptPowers == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnPrioritySpellsAsAdeptPowers == value)
                    return;
                if (value)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnPrioritySpellsAsAdeptPowers = true;
                        if (await GetMysAdeptSecondMAGAttributeAsync(token).ConfigureAwait(false))
                        {
                            _blnMysAdeptSecondMAGAttribute = false;
                            await this.OnMultiplePropertyChangedAsync(token, nameof(PrioritySpellsAsAdeptPowers), nameof(MysAdeptSecondMAGAttribute)).ConfigureAwait(false);
                        }
                        else
                            await OnPropertyChangedAsync(nameof(PrioritySpellsAsAdeptPowers), token)
                                .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
                else
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnPrioritySpellsAsAdeptPowers = true;
                        await OnPropertyChangedAsync(nameof(PrioritySpellsAsAdeptPowers), token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Allows characters to spend their Karma before Priority Points.
        /// </summary>
        public bool ReverseAttributePriorityOrder
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnReverseAttributePriorityOrder;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnReverseAttributePriorityOrder == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnReverseAttributePriorityOrder = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Allows characters to spend their Karma before Priority Points.
        /// </summary>
        public async Task<bool> GetReverseAttributePriorityOrderAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnReverseAttributePriorityOrder;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Allows characters to spend their Karma before Priority Points.
        /// </summary>
        public async Task SetReverseAttributePriorityOrderAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnReverseAttributePriorityOrder == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnReverseAttributePriorityOrder == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnReverseAttributePriorityOrder = value;
                    await OnPropertyChangedAsync(nameof(ReverseAttributePriorityOrder), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Improved Ability power (SR5 309) should be capped at 0.5 of current Rating or 1.5 of current Rating.
        /// </summary>
        public bool IncreasedImprovedAbilityMultiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIncreasedImprovedAbilityMultiplier;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnIncreasedImprovedAbilityMultiplier == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnIncreasedImprovedAbilityMultiplier = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether the Improved Ability power (SR5 309) should be capped at 0.5 of current Rating or 1.5 of current Rating.
        /// </summary>
        public async Task<bool> GetIncreasedImprovedAbilityMultiplierAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnIncreasedImprovedAbilityMultiplier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Improved Ability power (SR5 309) should be capped at 0.5 of current Rating or 1.5 of current Rating.
        /// </summary>
        public async Task SetIncreasedImprovedAbilityMultiplierAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIncreasedImprovedAbilityMultiplier == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnIncreasedImprovedAbilityMultiplier == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnIncreasedImprovedAbilityMultiplier = value;
                    await OnPropertyChangedAsync(nameof(IncreasedImprovedAbilityMultiplier), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether lifestyles will automatically give free grid subscriptions found in (HT)
        /// </summary>
        public bool AllowFreeGrids
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowFreeGrids;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowFreeGrids == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowFreeGrids = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether lifestyles will automatically give free grid subscriptions found in (HT)
        /// </summary>
        public async Task<bool> GetAllowFreeGridsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAllowFreeGrids;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether lifestyles will automatically give free grid subscriptions found in (HT)
        /// </summary>
        public async Task SetAllowFreeGridsAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowFreeGrids == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowFreeGrids == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAllowFreeGrids = value;
                    await OnPropertyChangedAsync(nameof(AllowFreeGrids), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Technomancers are allowed to use the Schooling discount on their initiations in the same manner as awakened.
        /// </summary>
        public bool AllowTechnomancerSchooling
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAllowTechnomancerSchooling;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAllowTechnomancerSchooling == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAllowTechnomancerSchooling = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Whether Technomancers are allowed to use the Schooling discount on their initiations in the same manner as awakened.
        /// </summary>
        public async Task<bool> GetAllowTechnomancerSchoolingAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAllowTechnomancerSchooling;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether Technomancers are allowed to use the Schooling discount on their initiations in the same manner as awakened.
        /// </summary>
        public async Task SetAllowTechnomancerSchoolingAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowTechnomancerSchooling == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAllowTechnomancerSchooling == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAllowTechnomancerSchooling = value;
                    await OnPropertyChangedAsync(nameof(AllowTechnomancerSchooling), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Override the maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public bool CyberlimbAttributeBonusCapOverride
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnCyberlimbAttributeBonusCapOverride;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnCyberlimbAttributeBonusCapOverride == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnCyberlimbAttributeBonusCapOverride = value;
                        if (value || Interlocked.Exchange(ref _intCyberlimbAttributeBonusCap, 4) == 4)
                            OnPropertyChanged();
                        else
                            this.OnMultiplePropertyChanged(nameof(CyberlimbAttributeBonusCapOverride),
                                nameof(CyberlimbAttributeBonusCap));
                    }
                }
            }
        }

        /// <summary>
        /// Override the maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public async Task<bool> GetCyberlimbAttributeBonusCapOverrideAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnCyberlimbAttributeBonusCapOverride;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Override the maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public async Task SetCyberlimbAttributeBonusCapOverrideAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnCyberlimbAttributeBonusCapOverride == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnCyberlimbAttributeBonusCapOverride == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnCyberlimbAttributeBonusCapOverride = value;
                    if (value || Interlocked.Exchange(ref _intCyberlimbAttributeBonusCap, 4) == 4)
                        await OnPropertyChangedAsync(nameof(CyberlimbAttributeBonusCapOverride), token).ConfigureAwait(false);
                    else
                        await this.OnMultiplePropertyChangedAsync(token, nameof(CyberlimbAttributeBonusCapOverride), nameof(CyberlimbAttributeBonusCap)).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public int CyberlimbAttributeBonusCap
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intCyberlimbAttributeBonusCap;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intCyberlimbAttributeBonusCap, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public async Task<int> GetCyberlimbAttributeBonusCapAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intCyberlimbAttributeBonusCap;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum value of bonuses that can affect cyberlimbs.
        /// </summary>
        public async Task SetCyberlimbAttributeBonusCapAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intCyberlimbAttributeBonusCap == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intCyberlimbAttributeBonusCap, value) != value)
                    await OnPropertyChangedAsync(nameof(CyberlimbAttributeBonusCap), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Dice Penalty per Spell
        /// </summary>
        public int DicePenaltySustaining
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intDicePenaltySustaining;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intDicePenaltySustaining, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        #endregion Properties and Methods

        #region Initiative Dice Properties

        /// <summary>
        /// Minimum number of initiative dice
        /// </summary>
        public int MinInitiativeDice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMinInitiativeDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMinInitiativeDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Minimum number of initiative dice
        /// </summary>
        public async Task<int> GetMinInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMinInitiativeDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum number of initiative dice
        /// </summary>
        public async Task SetMinInitiativeDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMinInitiativeDice == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMinInitiativeDice, value) != value)
                    await OnPropertyChangedAsync(nameof(MinInitiativeDice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of initiative dice
        /// </summary>
        public int MaxInitiativeDice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxInitiativeDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxInitiativeDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum number of initiative dice
        /// </summary>
        public async Task<int> GetMaxInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxInitiativeDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of initiative dice
        /// </summary>
        public async Task SetMaxInitiativeDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxInitiativeDice == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxInitiativeDice, value) != value)
                    await OnPropertyChangedAsync(nameof(MaxInitiativeDice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in Astral
        /// </summary>
        public int MinAstralInitiativeDice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMinAstralInitiativeDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMinAstralInitiativeDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in Astral
        /// </summary>
        public async Task<int> GetMinAstralInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMinAstralInitiativeDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in Astral
        /// </summary>
        public async Task SetMinAstralInitiativeDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMinAstralInitiativeDice == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMinAstralInitiativeDice, value) != value)
                    await OnPropertyChangedAsync(nameof(MinAstralInitiativeDice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in Astral
        /// </summary>
        public int MaxAstralInitiativeDice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxAstralInitiativeDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxAstralInitiativeDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in Astral
        /// </summary>
        public async Task<int> GetMaxAstralInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxAstralInitiativeDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in Astral
        /// </summary>
        public async Task SetMaxAstralInitiativeDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxAstralInitiativeDice == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxAstralInitiativeDice, value) != value)
                    await OnPropertyChangedAsync(nameof(MaxAstralInitiativeDice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in cold sim VR
        /// </summary>
        public int MinColdSimInitiativeDice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMinColdSimInitiativeDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMinColdSimInitiativeDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in cold sim VR
        /// </summary>
        public async Task<int> GetMinColdSimInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMinColdSimInitiativeDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in cold sim VR
        /// </summary>
        public async Task SetMinColdSimInitiativeDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMinColdSimInitiativeDice == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMinColdSimInitiativeDice, value) != value)
                    await OnPropertyChangedAsync(nameof(MinColdSimInitiativeDice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in cold sim VR
        /// </summary>
        public int MaxColdSimInitiativeDice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxColdSimInitiativeDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxColdSimInitiativeDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in cold sim VR
        /// </summary>
        public async Task<int> GetMaxColdSimInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxColdSimInitiativeDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in cold sim VR
        /// </summary>
        public async Task SetMaxColdSimInitiativeDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxColdSimInitiativeDice == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxColdSimInitiativeDice, value) != value)
                    await OnPropertyChangedAsync(nameof(MaxColdSimInitiativeDice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in hot sim VR
        /// </summary>
        public int MinHotSimInitiativeDice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMinHotSimInitiativeDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMinHotSimInitiativeDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in hot sim VR
        /// </summary>
        public async Task<int> GetMinHotSimInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMinHotSimInitiativeDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Minimum number of initiative dice in hot sim VR
        /// </summary>
        public async Task SetMinHotSimInitiativeDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMinHotSimInitiativeDice == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMinHotSimInitiativeDice, value) != value)
                    await OnPropertyChangedAsync(nameof(MinHotSimInitiativeDice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in hot sim VR
        /// </summary>
        public int MaxHotSimInitiativeDice
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMaxHotSimInitiativeDice;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intMaxHotSimInitiativeDice, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in hot sim VR
        /// </summary>
        public async Task<int> GetMaxHotSimInitiativeDiceAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intMaxHotSimInitiativeDice;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum number of initiative dice in hot sim VR
        /// </summary>
        public async Task SetMaxHotSimInitiativeDiceAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intMaxHotSimInitiativeDice == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intMaxHotSimInitiativeDice, value) != value)
                    await OnPropertyChangedAsync(nameof(MaxHotSimInitiativeDice), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Initiative Dice Properties

        #region Karma

        /// <summary>
        /// Karma cost to improve an Attribute = New Rating X this value.
        /// </summary>
        public int KarmaAttribute
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaAttribute;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaAttribute, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to improve an Attribute = New Rating X this value.
        /// </summary>
        public async Task<int> GetKarmaAttributeAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaAttribute;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to improve an Attribute = New Rating X this value.
        /// </summary>
        public async Task SetKarmaAttributeAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaAttribute == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaAttribute, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaAttribute), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a Quality = BP Cost x this value.
        /// </summary>
        public int KarmaQuality
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaQuality;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaQuality, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a Quality = BP Cost x this value.
        /// </summary>
        public async Task<int> GetKarmaQualityAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaQuality;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a Quality = BP Cost x this value.
        /// </summary>
        public async Task SetKarmaQualityAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaQuality == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaQuality, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaQuality), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for an active skill = this value.
        /// </summary>
        public int KarmaSpecialization
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaSpecialization;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaSpecialization, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for an active skill = this value.
        /// </summary>
        public async Task<int> GetKarmaSpecializationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaSpecialization;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for an active skill = this value.
        /// </summary>
        public async Task SetKarmaSpecializationAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaSpecialization == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaSpecialization, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaSpecialization), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for a knowledge skill = this value.
        /// </summary>
        public int KarmaKnowledgeSpecialization
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaKnoSpecialization;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaKnoSpecialization, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for a knowledge skill = this value.
        /// </summary>
        public async Task<int> GetKarmaKnowledgeSpecializationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaKnoSpecialization;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a Specialization for a knowledge skill = this value.
        /// </summary>
        public async Task SetKarmaKnowledgeSpecializationAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaKnoSpecialization == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaKnoSpecialization, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaKnowledgeSpecialization), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Knowledge Skill = this value.
        /// </summary>
        public int KarmaNewKnowledgeSkill
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaNewKnowledgeSkill;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaNewKnowledgeSkill, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Knowledge Skill = this value.
        /// </summary>
        public async Task<int> GetKarmaNewKnowledgeSkillAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaNewKnowledgeSkill;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Knowledge Skill = this value.
        /// </summary>
        public async Task SetKarmaNewKnowledgeSkillAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaNewKnowledgeSkill == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaNewKnowledgeSkill, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaNewKnowledgeSkill), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Active Skill = this value.
        /// </summary>
        public int KarmaNewActiveSkill
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaNewActiveSkill;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaNewActiveSkill, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Active Skill = this value.
        /// </summary>
        public async Task<int> GetKarmaNewActiveSkillAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaNewActiveSkill;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Active Skill = this value.
        /// </summary>
        public async Task SetKarmaNewActiveSkillAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaNewActiveSkill == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaNewActiveSkill, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaNewActiveSkill), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Skill Group = this value.
        /// </summary>
        public int KarmaNewSkillGroup
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaNewSkillGroup;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaNewSkillGroup, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Skill Group = this value.
        /// </summary>
        public async Task<int> GetKarmaNewSkillGroupAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaNewSkillGroup;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to purchase a new Skill Group = this value.
        /// </summary>
        public async Task SetKarmaNewSkillGroupAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaNewSkillGroup == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaNewSkillGroup, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaNewSkillGroup), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to improve a Knowledge Skill = New Rating x this value.
        /// </summary>
        public int KarmaImproveKnowledgeSkill
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaImproveKnowledgeSkill;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaImproveKnowledgeSkill, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to improve a Knowledge Skill = New Rating x this value.
        /// </summary>
        public async Task<int> GetKarmaImproveKnowledgeSkillAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaImproveKnowledgeSkill;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to improve a Knowledge Skill = New Rating x this value.
        /// </summary>
        public async Task SetKarmaImproveKnowledgeSkillAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaImproveKnowledgeSkill == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaImproveKnowledgeSkill, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaImproveKnowledgeSkill), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to improve an Active Skill = New Rating x this value.
        /// </summary>
        public int KarmaImproveActiveSkill
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaImproveActiveSkill;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaImproveActiveSkill, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to improve an Active Skill = New Rating x this value.
        /// </summary>
        public async Task<int> GetKarmaImproveActiveSkillAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaImproveActiveSkill;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to improve an Active Skill = New Rating x this value.
        /// </summary>
        public async Task SetKarmaImproveActiveSkillAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaImproveActiveSkill == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaImproveActiveSkill, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaImproveActiveSkill), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to improve a Skill Group = New Rating x this value.
        /// </summary>
        public int KarmaImproveSkillGroup
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaImproveSkillGroup;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaImproveSkillGroup, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to improve a Skill Group = New Rating x this value.
        /// </summary>
        public async Task<int> GetKarmaImproveSkillGroupAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaImproveSkillGroup;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to improve a Skill Group = New Rating x this value.
        /// </summary>
        public async Task SetKarmaImproveSkillGroupAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaImproveSkillGroup == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaImproveSkillGroup, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaImproveActiveSkill), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for each Spell = this value.
        /// </summary>
        public int KarmaSpell
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaSpell;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaSpell, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for each Spell = this value.
        /// </summary>
        public async Task<int> GetKarmaSpellAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaSpell;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for each Spell = this value.
        /// </summary>
        public async Task SetKarmaSpellAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaSpell == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaSpell, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaSpell), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for each Enhancement = this value.
        /// </summary>
        public int KarmaEnhancement
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaEnhancement;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaEnhancement, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for each Enhancement = this value.
        /// </summary>
        public async Task<int> GetKarmaEnhancementAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaEnhancement;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for each Enhancement = this value.
        /// </summary>
        public async Task SetKarmaEnhancementAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaEnhancement == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaEnhancement, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaEnhancement), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a new Complex Form = this value.
        /// </summary>
        public int KarmaNewComplexForm
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaNewComplexForm;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaNewComplexForm, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a new Complex Form = this value.
        /// </summary>
        public async Task<int> GetKarmaNewComplexFormAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaNewComplexForm;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a new Complex Form = this value.
        /// </summary>
        public async Task SetKarmaNewComplexFormAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaNewComplexForm == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaNewComplexForm, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaNewComplexForm), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a new AI Program
        /// </summary>
        public int KarmaNewAIProgram
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaNewAIProgram;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaNewAIProgram, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a new AI Program
        /// </summary>
        public async Task<int> GetKarmaNewAIProgramAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaNewAIProgram;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a new AI Program
        /// </summary>
        public async Task SetKarmaNewAIProgramAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaNewAIProgram == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaNewAIProgram, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaNewAIProgram), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a new AI Advanced Program
        /// </summary>
        public int KarmaNewAIAdvancedProgram
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaNewAIAdvancedProgram;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaNewAIAdvancedProgram, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a new AI Advanced Program
        /// </summary>
        public async Task<int> GetKarmaNewAIAdvancedProgramAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaNewAIAdvancedProgram;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a new AI Advanced Program
        /// </summary>
        public async Task SetKarmaNewAIAdvancedProgramAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaNewAIAdvancedProgram == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaNewAIAdvancedProgram, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaNewAIAdvancedProgram), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Contact = (Connection + Loyalty) x this value.
        /// </summary>
        public int KarmaContact
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaContact;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaContact, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a Contact = (Connection + Loyalty) x this value.
        /// </summary>
        public async Task<int> GetKarmaContactAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaContact;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Contact = (Connection + Loyalty) x this value.
        /// </summary>
        public async Task SetKarmaContactAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaContact == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaContact, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaContact), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for an Enemy = (Connection + Loyalty) x this value.
        /// </summary>
        public int KarmaEnemy
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaEnemy;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaEnemy, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for an Enemy = (Connection + Loyalty) x this value.
        /// </summary>
        public async Task<int> GetKarmaEnemyAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaEnemy;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Enemy = (Connection + Loyalty) x this value.
        /// </summary>
        public async Task SetKarmaEnemyAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaEnemy == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaEnemy, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaEnemy), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum amount of remaining Karma that is carried over to the character once they are created.
        /// </summary>
        public int KarmaCarryover
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaCarryover;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaCarryover, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Maximum amount of remaining Karma that is carried over to the character once they are created.
        /// </summary>
        public async Task<int> GetKarmaCarryoverAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaCarryover;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum amount of remaining Karma that is carried over to the character once they are created.
        /// </summary>
        public async Task SetKarmaCarryoverAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaCarryover == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaCarryover, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaCarryover), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum amount of remaining Nuyen that is carried over to the character once they are created.
        /// </summary>
        public decimal NuyenCarryover
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decNuyenCarryover;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decNuyenCarryover == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decNuyenCarryover = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Maximum amount of remaining Nuyen that is carried over to the character once they are created.
        /// </summary>
        public async Task<decimal> GetNuyenCarryoverAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decNuyenCarryover;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum amount of remaining Nuyen that is carried over to the character once they are created.
        /// </summary>
        public async Task SetNuyenCarryoverAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decNuyenCarryover == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decNuyenCarryover == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decNuyenCarryover = value;
                    await OnPropertyChangedAsync(nameof(NuyenCarryover), token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Spirit = this value.
        /// </summary>
        public int KarmaSpirit
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaSpirit;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaSpirit, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a Spirit = this value.
        /// </summary>
        public async Task<int> GetKarmaSpiritAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaSpirit;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Spirit = this value.
        /// </summary>
        public async Task SetKarmaSpiritAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaSpirit == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaSpirit, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaSpirit), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Martial Arts Technique = this value.
        /// </summary>
        public int KarmaTechnique
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaTechnique;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaTechnique, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a Martial Arts Technique = this value.
        /// </summary>
        public async Task<int> GetKarmaTechniqueAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaTechnique;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Martial Arts Technique = this value.
        /// </summary>
        public async Task SetKarmaTechniqueAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaTechnique == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaTechnique, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaTechnique), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for an Initiation = KarmaInitiationFlat + (New Rating x this value).
        /// </summary>
        public int KarmaInitiation
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaInitiation;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaInitiation, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for an Initiation = KarmaInitiationFlat + (New Rating x this value).
        /// </summary>
        public async Task<int> GetKarmaInitiationAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaInitiation;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for an Initiation = KarmaInitiationFlat + (New Rating x this value).
        /// </summary>
        public async Task SetKarmaInitiationAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaInitiation == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaInitiation, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaInitiation), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for an Initiation = this value + (New Rating x KarmaInitiation).
        /// </summary>
        public int KarmaInitiationFlat
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaInitiationFlat;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaInitiationFlat, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for an Initiation = this value + (New Rating x KarmaInitiation).
        /// </summary>
        public async Task<int> GetKarmaInitiationFlatAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaInitiationFlat;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for an Initiation = this value + (New Rating x KarmaInitiation).
        /// </summary>
        public async Task SetKarmaInitiationFlatAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaInitiationFlat == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaInitiationFlat, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaInitiationFlat), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Metamagic = this value.
        /// </summary>
        public int KarmaMetamagic
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaMetamagic;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaMetamagic, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for a Metamagic = this value.
        /// </summary>
        public async Task<int> GetKarmaMetamagicAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaMetamagic;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for a Metamagic = this value.
        /// </summary>
        public async Task SetKarmaMetamagicAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaMetamagic == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaMetamagic, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaMetamagic), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to join a Group = this value.
        /// </summary>
        public int KarmaJoinGroup
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaJoinGroup;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaJoinGroup, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to join a Group = this value.
        /// </summary>
        public async Task<int> GetKarmaJoinGroupAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaJoinGroup;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to join a Group = this value.
        /// </summary>
        public async Task SetKarmaJoinGroupAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaJoinGroup == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaJoinGroup, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaJoinGroup), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to leave a Group = this value.
        /// </summary>
        public int KarmaLeaveGroup
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaLeaveGroup;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaLeaveGroup, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost to leave a Group = this value.
        /// </summary>
        public async Task<int> GetKarmaLeaveGroupAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaLeaveGroup;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost to leave a Group = this value.
        /// </summary>
        public async Task SetKarmaLeaveGroupAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaLeaveGroup == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaLeaveGroup, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaLeaveGroup), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Alchemical Foci.
        /// </summary>
        public int KarmaAlchemicalFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaAlchemicalFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaAlchemicalFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Alchemical Foci.
        /// </summary>
        public async Task<int> GetKarmaAlchemicalFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaAlchemicalFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Alchemical Foci.
        /// </summary>
        public async Task SetKarmaAlchemicalFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaAlchemicalFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaAlchemicalFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaAlchemicalFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Banishing Foci.
        /// </summary>
        public int KarmaBanishingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaBanishingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaBanishingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Banishing Foci.
        /// </summary>
        public async Task<int> GetKarmaBanishingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaBanishingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Banishing Foci.
        /// </summary>
        public async Task SetKarmaBanishingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaBanishingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaBanishingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaBanishingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Binding Foci.
        /// </summary>
        public int KarmaBindingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaBindingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaBindingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Binding Foci.
        /// </summary>
        public async Task<int> GetKarmaBindingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaBindingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Binding Foci.
        /// </summary>
        public async Task SetKarmaBindingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaBindingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaBindingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaBindingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Centering Foci.
        /// </summary>
        public int KarmaCenteringFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaCenteringFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaCenteringFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Centering Foci.
        /// </summary>
        public async Task<int> GetKarmaCenteringFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaCenteringFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Centering Foci.
        /// </summary>
        public async Task SetKarmaCenteringFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaCenteringFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaCenteringFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaCenteringFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Counterspelling Foci.
        /// </summary>
        public int KarmaCounterspellingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaCounterspellingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaCounterspellingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Counterspelling Foci.
        /// </summary>
        public async Task<int> GetKarmaCounterspellingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaCounterspellingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Counterspelling Foci.
        /// </summary>
        public async Task SetKarmaCounterspellingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaCounterspellingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaCounterspellingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaCounterspellingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Disenchanting Foci.
        /// </summary>
        public int KarmaDisenchantingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaDisenchantingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaDisenchantingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Disenchanting Foci.
        /// </summary>
        public async Task<int> GetKarmaDisenchantingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaDisenchantingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Disenchanting Foci.
        /// </summary>
        public async Task SetKarmaDisenchantingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaDisenchantingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaDisenchantingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaDisenchantingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Flexible Signature Foci.
        /// </summary>
        public int KarmaFlexibleSignatureFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaFlexibleSignatureFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaFlexibleSignatureFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Flexible Signature Foci.
        /// </summary>
        public async Task<int> GetKarmaFlexibleSignatureFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaFlexibleSignatureFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Flexible Signature Foci.
        /// </summary>
        public async Task SetKarmaFlexibleSignatureFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaFlexibleSignatureFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaFlexibleSignatureFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaFlexibleSignatureFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Masking Foci.
        /// </summary>
        public int KarmaMaskingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaMaskingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaMaskingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Masking Foci.
        /// </summary>
        public async Task<int> GetKarmaMaskingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaMaskingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Masking Foci.
        /// </summary>
        public async Task SetKarmaMaskingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaMaskingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaMaskingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaMaskingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Power Foci.
        /// </summary>
        public int KarmaPowerFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaPowerFocus;
            }
            set
            {
                if (Interlocked.Exchange(ref _intKarmaPowerFocus, value) != value)
                    OnPropertyChanged();
            }
        }

        /// <summary>
        /// Karma cost for Power Foci.
        /// </summary>
        public async Task<int> GetKarmaPowerFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaPowerFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Power Foci.
        /// </summary>
        public async Task SetKarmaPowerFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaPowerFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaPowerFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaPowerFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Qi Foci.
        /// </summary>
        public int KarmaQiFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaQiFocus;
            }
            set
            {
                if (Interlocked.Exchange(ref _intKarmaQiFocus, value) != value)
                    OnPropertyChanged();
            }
        }

        /// <summary>
        /// Karma cost for Qi Foci.
        /// </summary>
        public async Task<int> GetKarmaQiFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaQiFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Power Foci.
        /// </summary>
        public async Task SetKarmaQiFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaQiFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaQiFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaQiFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Ritual Spellcasting Foci.
        /// </summary>
        public int KarmaRitualSpellcastingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaRitualSpellcastingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaRitualSpellcastingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Ritual Spellcasting Foci.
        /// </summary>
        public async Task<int> GetKarmaRitualSpellcastingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaRitualSpellcastingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Ritual Spellcasting Foci.
        /// </summary>
        public async Task SetKarmaRitualSpellcastingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaRitualSpellcastingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaRitualSpellcastingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaRitualSpellcastingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Spellcasting Foci.
        /// </summary>
        public int KarmaSpellcastingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaSpellcastingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaSpellcastingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Spellcasting Foci.
        /// </summary>
        public async Task<int> GetKarmaSpellcastingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaSpellcastingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Spellcasting Foci.
        /// </summary>
        public async Task SetKarmaSpellcastingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaSpellcastingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaSpellcastingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaSpellcastingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Spell Shaping Foci.
        /// </summary>
        public int KarmaSpellShapingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaSpellShapingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaSpellShapingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Spell Shaping Foci.
        /// </summary>
        public async Task<int> GetKarmaSpellShapingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaSpellShapingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Spell Shaping Foci.
        /// </summary>
        public async Task SetKarmaSpellShapingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaSpellShapingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaSpellShapingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaSpellShapingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Summoning Foci.
        /// </summary>
        public int KarmaSummoningFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaSummoningFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaSummoningFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Summoning Foci.
        /// </summary>
        public async Task<int> GetKarmaSummoningFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaSummoningFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Summoning Foci.
        /// </summary>
        public async Task SetKarmaSummoningFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaSummoningFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaSummoningFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaSummoningFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Sustaining Foci.
        /// </summary>
        public int KarmaSustainingFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaSustainingFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaSustainingFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Sustaining Foci.
        /// </summary>
        public async Task<int> GetKarmaSustainingFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaSustainingFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Sustaining Foci.
        /// </summary>
        public async Task SetKarmaSustainingFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaSustainingFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaSustainingFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaSustainingFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Weapon Foci.
        /// </summary>
        public int KarmaWeaponFocus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaWeaponFocus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaWeaponFocus, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for Weapon Foci.
        /// </summary>
        public async Task<int> GetKarmaWeaponFocusAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaWeaponFocus;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for Weapon Foci.
        /// </summary>
        public async Task SetKarmaWeaponFocusAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaWeaponFocus == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaWeaponFocus, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaWeaponFocus), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How much Karma a single Power Point costs for a Mystic Adept.
        /// </summary>
        public int KarmaMysticAdeptPowerPoint
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaMysticAdeptPowerPoint;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaMysticAdeptPowerPoint, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How much Karma a single Power Point costs for a Mystic Adept.
        /// </summary>
        public async Task<int> GetKarmaMysticAdeptPowerPointAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaMysticAdeptPowerPoint;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How much Karma a single Power Point costs for a Mystic Adept.
        /// </summary>
        public async Task SetKarmaMysticAdeptPowerPointAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaMysticAdeptPowerPoint == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaMysticAdeptPowerPoint, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaMysticAdeptPowerPoint), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for fetting a spirit (gets multiplied by Force).
        /// </summary>
        public int KarmaSpiritFettering
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intKarmaSpiritFettering;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intKarmaSpiritFettering, value) != value)
                        OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Karma cost for fetting a spirit (gets multiplied by Force).
        /// </summary>
        public async Task<int> GetKarmaSpiritFetteringAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intKarmaSpiritFettering;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Karma cost for fetting a spirit (gets multiplied by Force).
        /// </summary>
        public async Task SetKarmaSpiritFetteringAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intKarmaSpiritFettering == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            token.ThrowIfCancellationRequested();
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intKarmaSpiritFettering, value) != value)
                    await OnPropertyChangedAsync(nameof(KarmaSpiritFettering), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Karma

        #region Default Build

        /// <summary>
        /// Percentage by which adding an Initiate Grade to an Awakened is discounted if a member of a Group.
        /// </summary>
        public decimal KarmaMAGInitiationGroupPercent
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decKarmaMAGInitiationGroupPercent;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decKarmaMAGInitiationGroupPercent == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decKarmaMAGInitiationGroupPercent = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Percentage by which adding a Submersion Grade to a Technomancer is discounted if a member of a Group.
        /// </summary>
        public decimal KarmaRESInitiationGroupPercent
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decKarmaRESInitiationGroupPercent;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decKarmaRESInitiationGroupPercent == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decKarmaRESInitiationGroupPercent = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Percentage by which adding an Initiate Grade to an Awakened is discounted if performing an Ordeal.
        /// </summary>
        public decimal KarmaMAGInitiationOrdealPercent
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decKarmaMAGInitiationOrdealPercent;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decKarmaMAGInitiationOrdealPercent == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decKarmaMAGInitiationOrdealPercent = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Percentage by which adding a Submersion Grade to a Technomancer is discounted if performing an Ordeal.
        /// </summary>
        public decimal KarmaRESInitiationOrdealPercent
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decKarmaRESInitiationOrdealPercent;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decKarmaRESInitiationOrdealPercent == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decKarmaRESInitiationOrdealPercent = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Percentage by which adding an Initiate Grade to an Awakened is discounted if performing an Ordeal.
        /// </summary>
        public decimal KarmaMAGInitiationSchoolingPercent
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decKarmaMAGInitiationSchoolingPercent;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decKarmaMAGInitiationSchoolingPercent == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decKarmaMAGInitiationSchoolingPercent = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        /// <summary>
        /// Percentage by which adding a Submersion Grade to a Technomancer is discounted if performing an Ordeal.
        /// </summary>
        public decimal KarmaRESInitiationSchoolingPercent
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decKarmaRESInitiationSchoolingPercent;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decKarmaRESInitiationSchoolingPercent == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decKarmaRESInitiationSchoolingPercent = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        #endregion Default Build

        #region Constant Values

        /// <summary>
        /// The value by which Specializations add to dicepool.
        /// </summary>
        public int SpecializationBonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return 2;
            }
        }

        /// <summary>
        /// The value by which Expertise Specializations add to dicepool (does not stack with SpecializationBonus).
        /// </summary>
        public int ExpertiseBonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return 3;
            }
        }

        #endregion Constant Values

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                Utils.StringHashSetPool.Return(ref _setBooks);
                Utils.StringHashSetPool.Return(ref _setBannedWareGrades);
                Utils.StringHashSetPool.Return(ref _setRedlinerExcludes);
                _dicCustomDataDirectoryKeys.Dispose();
            }

            LockObject.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                Utils.StringHashSetPool.Return(ref _setBooks);
                Utils.StringHashSetPool.Return(ref _setBannedWareGrades);
                Utils.StringHashSetPool.Return(ref _setRedlinerExcludes);
                await _dicCustomDataDirectoryKeys.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            await LockObject.DisposeAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; } = new AsyncFriendlyReaderWriterLock();
    }
}
