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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Microsoft.VisualStudio.Threading;
using NLog;
using IAsyncDisposable = System.IAsyncDisposable;
using Version = System.Version;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A piece of Cyberware.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage)}")]
    public sealed class Cyberware : ICanPaste, IHasChildrenAndCost<Cyberware>, IHasGear, IHasName, IHasInternalId,
        IHasSourceId, IHasXmlDataNode,
        IHasMatrixAttributes, IHasNotes, ICanSell, IHasRating, IHasSource, ICanSort, IHasStolenProperty,
        IHasWirelessBonus, ICanBlackMarketDiscount, IHasLockObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strLimbSlot = string.Empty;
        private string _strLimbSlotCount = "1";
        private bool _blnInheritAttributes;
        private string _strESS = string.Empty;
        private decimal _decExtraESSAdditiveMultiplier;
        private decimal _decExtraESSMultiplicativeMultiplier = 1.0m;
        private string _strCapacity = string.Empty;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strWeight = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private int _intMatrixCMFilled;
        private int _intRating;
        private int _intMinStrength = 3;
        private int _intMinAgility = 3;
        private string _strMinRating = string.Empty;
        private string _strMaxRating = string.Empty;
        private string _strRatingLabel = "String_Rating";
        private string _strAllowSubsystems = string.Empty;
        private bool _blnSuite;
        private bool _blnStolen;
        private string _strLocation = string.Empty;
        private string _strExtra = string.Empty;
        private Guid _guiWeaponID = Guid.Empty;
        private Guid _guiWeaponAccessoryID = Guid.Empty;
        private Guid _guiVehicleID = Guid.Empty;
        private Grade _objGrade;

        private readonly TaggedObservableCollection<Cyberware> _lstChildren;
        private readonly TaggedObservableCollection<Gear> _lstGear;
        private XmlNode _nodBonus;
        private XmlNode _nodPairBonus;
        private XmlNode _nodWirelessBonus;
        private XmlNode _nodWirelessPairBonus;
        private HashSet<string> _lstIncludeInPairBonus = Utils.StringHashSetPool.Get();
        private HashSet<string> _lstIncludeInWirelessPairBonus = Utils.StringHashSetPool.Get();
        private bool _blnWirelessOn = true;
        private XmlNode _nodAllowGear;
        private Improvement.ImprovementSource _eImprovementSource = Improvement.ImprovementSource.Cyberware;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private int _intEssenceDiscount;
        private string _strForceGrade = string.Empty;
        private bool _blnDiscountCost;
        private Vehicle _objParentVehicle;
        private bool _blnPrototypeTranshuman;
        private Cyberware _objParent;
        private bool _blnAddToParentESS;
        private bool _blnAddToParentCapacity;
        private string _strParentID = string.Empty;
        private string _strHasModularMount = string.Empty;
        private string _strPlugsIntoModularMount = string.Empty;
        private string _strBlocksMounts = string.Empty;
        private string _strForced = string.Empty;
        private bool _blnIsGeneware;

        private string _strDeviceRating = string.Empty;
        private string _strAttack = string.Empty;
        private string _strSleaze = string.Empty;
        private string _strDataProcessing = string.Empty;
        private string _strFirewall = string.Empty;
        private string _strAttributeArray = string.Empty;
        private string _strModAttack = string.Empty;
        private string _strModSleaze = string.Empty;
        private string _strModDataProcessing = string.Empty;
        private string _strModFirewall = string.Empty;
        private string _strModAttributeArray = string.Empty;
        private string _strProgramLimit = string.Empty;
        private string _strOverclocked = "None";
        private string _strCanFormPersona = string.Empty;
        private int _intMatrixCMBonus;
        private bool _blnCanSwapAttributes;
        private int _intSortOrder;

        private readonly Character _objCharacter;
        private static readonly char[] s_MathOperators = "\"*/+-".ToCharArray();

        private bool _blnDoPropertyChangedInCollectionChanged = true;

        // I don't like this, but it's easier than making it a specific property of the cyberware.
        // We're using IReadOnlyCollection and IReadOnlyDictionary to make extra-sure that none of these will get changed

        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>>
            s_AttributeCustomizationCyberwares
                = new Dictionary<string, IReadOnlyCollection<string>>
                {
                    { "AGI", new HashSet<string> { "Customized Agility", "Cyberlimb Customization, Agility (2050)" } },
                    { "STR", new HashSet<string> { "Customized Strength", "Cyberlimb Customization, Strength (2050)" } }
                };

        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>>
            s_AttributeEnhancementCyberwares
                = new Dictionary<string, IReadOnlyCollection<string>>
                {
                    { "AGI", new HashSet<string> { "Enhanced Agility", "Cyberlimb Augmentation, Agility (2050)" } },
                    { "STR", new HashSet<string> { "Enhanced Strength", "Cyberlimb Augmentation, Strength (2050)" } }
                };

        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> s_AttributeAffectingCyberwares
            = new Dictionary<string, IReadOnlyCollection<string>>
            {
                {
                    "AGI",
                    new HashSet<string>(s_AttributeCustomizationCyberwares["AGI"]
                        .Concat(s_AttributeEnhancementCyberwares["AGI"]))
                },
                {
                    "STR",
                    new HashSet<string>(s_AttributeCustomizationCyberwares["STR"]
                        .Concat(s_AttributeEnhancementCyberwares["STR"]))
                }
            };

        public static IReadOnlyCollection<string> CyberlimbAttributeAbbrevs { get; } =
            new HashSet<string>(s_AttributeAffectingCyberwares.Keys);

        #region Helper Methods

        /// <summary>
        /// Convert a string to a Grade.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        /// <param name="objSource">Source representing whether this is a cyberware or bioware grade.</param>
        /// <param name="objCharacter">Character from which to fetch a grade list</param>
        public static Grade ConvertToCyberwareGrade(string strValue, Improvement.ImprovementSource objSource,
            Character objCharacter)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            Grade objStandardGrade = null;
            foreach (Grade objGrade in objCharacter.GetGrades(objSource, true))
            {
                if (objGrade.Name == strValue)
                    return objGrade;
                if (objGrade.Name == "Standard")
                    objStandardGrade = objGrade;
            }

            return objStandardGrade;
        }

        /// <summary>
        /// Returns the limb type associated with a particular mount.
        /// </summary>
        /// <param name="strMount">Mount to check.</param>
        /// <returns>Limb associated with <paramref name="strMount"/>. If there is none, returns an empty string.</returns>
        public static string MountToLimbType(string strMount)
        {
            switch (strMount)
            {
                case "wrist":
                case "elbow":
                case "shoulder":
                    return "arm";

                case "ankle":
                case "knee":
                case "hip":
                    return "leg";
            }

            return string.Empty;
        }

        #endregion Helper Methods

        #region Constructor, Create, Save, Load, and Print Methods

        public Cyberware(Character objCharacter)
        {
            // Create the GUID for the new piece of Cyberware.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            LockObject = objCharacter.LockObject;
            _lstChildren = new TaggedObservableCollection<Cyberware>(LockObject);
            _lstChildren.AddTaggedCollectionChanged(this, CyberwareChildrenOnCollectionChanged);
            _lstGear = new TaggedObservableCollection<Gear>(LockObject);
            _lstGear.AddTaggedCollectionChanged(this, GearChildrenOnCollectionChanged);
        }

        private async Task CyberwareChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // If we are loading, only manage parent setting, don't do property updating
                if (!_blnDoPropertyChangedInCollectionChanged)
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            List<Cyberware> lstNewItems = e.NewItems?.OfType<Cyberware>().ToList() ?? new List<Cyberware>();
                            foreach (Cyberware objNewItem in lstNewItems)
                                await objNewItem.SetParentAsync(this, token).ConfigureAwait(false);
                            break;
                        }

                        case NotifyCollectionChangedAction.Remove:
                        {
                            List<Cyberware> lstOldItems = e.OldItems?.OfType<Cyberware>().ToList() ?? new List<Cyberware>();
                            foreach (Cyberware objOldItem in lstOldItems)
                            {
                                try
                                {
                                    IAsyncDisposable objLocker2 = await objOldItem.LockObject
                                        .EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                                    try
                                    {
                                        token.ThrowIfCancellationRequested();
                                        if (await objOldItem.GetParentAsync(token).ConfigureAwait(false) == this)
                                            await objOldItem.SetParentAsync(null, token).ConfigureAwait(false);
                                    }
                                    finally
                                    {
                                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                                    }
                                }
                                catch (ObjectDisposedException)
                                {
                                    //swallow this
                                }
                            }

                            break;
                        }

                        case NotifyCollectionChangedAction.Replace:
                        {
                            HashSet<Cyberware> setNewItems = e.NewItems.OfType<Cyberware>().ToHashSet();
                            List<Cyberware> lstOldItems = e.OldItems?.OfType<Cyberware>().ToList() ?? new List<Cyberware>();
                            foreach (Cyberware objOldItem in lstOldItems)
                            {
                                if (!setNewItems.Contains(objOldItem))
                                {
                                    try
                                    {
                                        IAsyncDisposable objLocker2 = await objOldItem.LockObject
                                            .EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
                                        try
                                        {
                                            token.ThrowIfCancellationRequested();
                                            if (await objOldItem.GetParentAsync(token).ConfigureAwait(false) == this)
                                                await objOldItem.SetParentAsync(null, token).ConfigureAwait(false);
                                        }
                                        finally
                                        {
                                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                                        }
                                    }
                                    catch (ObjectDisposedException)
                                    {
                                        //swallow this
                                    }
                                }
                            }

                            foreach (Cyberware objNewItem in setNewItems)
                                await objNewItem.SetParentAsync(this, token).ConfigureAwait(false);
                            break;
                        }

                        case NotifyCollectionChangedAction.Move:
                            return;

                        case NotifyCollectionChangedAction.Reset:
                            break;
                    }

                    await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
                    return;
                }

                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                           out HashSet<string> setAttributesToRefresh))
                {
                    bool blnDoEssenceImprovementsRefresh = false;
                    bool blnDoRedlinerRefresh = false;
                    bool blnEverDoEncumbranceRefresh =
                        await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false) && await GetParentVehicleAsync(token).ConfigureAwait(false) == null;
                    bool blnDoEncumbranceRefresh = false;
                    List<Cyberware> lstImprovementSourcesToProcess = new List<Cyberware>(e.NewItems?.Count ?? 0);
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            List<Cyberware> lstNewItems = e.NewItems?.OfType<Cyberware>().ToList() ?? new List<Cyberware>();
                            foreach (Cyberware objNewItem in lstNewItems)
                            {
                                await objNewItem.SetParentAsync(this, token).ConfigureAwait(false);
                                    if (await objNewItem.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                                {
                                    if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh
                                                                    && (!string.IsNullOrEmpty(Weight)
                                                                        || !string.IsNullOrEmpty(objNewItem.Weight)
                                                                        || objNewItem.GearChildren.DeepAny(
                                                                            x => x.Children,
                                                                            x => !string.IsNullOrEmpty(x.Weight))
                                                                        || objNewItem.Children.DeepAny(
                                                                            x => x.Children,
                                                                            y => !string.IsNullOrEmpty(y.Weight)
                                                                                || y.GearChildren.DeepAny(
                                                                                    x => x.Children,
                                                                                    x => !string
                                                                                        .IsNullOrEmpty(x.Weight)))))
                                        blnDoEncumbranceRefresh = true;
                                    lstImprovementSourcesToProcess.Add(objNewItem);
                                }

                                if (setAttributesToRefresh.Count < CyberlimbAttributeAbbrevs.Count
                                    && Parent?.InheritAttributes != false && await GetParentVehicleAsync(token).ConfigureAwait(false) == null &&
                                    !await _objCharacter.Settings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false) && await GetIsLimbAsync(token).ConfigureAwait(false) &&
                                    !(await _objCharacter.Settings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(await GetLimbSlotAsync(token).ConfigureAwait(false)))
                                {
                                    if (InheritAttributes)
                                    {
                                        setAttributesToRefresh.AddRange(CyberlimbAttributeAbbrevs);
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in
                                                 s_AttributeAffectingCyberwares)
                                        {
                                            if (!setAttributesToRefresh.Contains(kvpToCheck.Key)
                                                && kvpToCheck.Value.Contains(objNewItem.Name))
                                                setAttributesToRefresh.Add(kvpToCheck.Key);
                                        }
                                    }
                                }

                                if (!blnDoEssenceImprovementsRefresh && (GetParentAsync(token) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) &&
                                    string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) && await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                                    blnDoEssenceImprovementsRefresh = true;
                            }

                            await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
                            blnDoRedlinerRefresh = true;
                            break;
                        }

                        case NotifyCollectionChangedAction.Remove:
                        {
                            List<Cyberware> lstOldItems = e.OldItems?.OfType<Cyberware>().ToList() ?? new List<Cyberware>();
                            foreach (Cyberware objOldItem in lstOldItems)
                            {
                                if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh
                                                                && await objOldItem
                                                                    .GetIsModularCurrentlyEquippedAsync(token)
                                                                    .ConfigureAwait(false)
                                                                && (!string.IsNullOrEmpty(Weight)
                                                                    || !string.IsNullOrEmpty(objOldItem.Weight)
                                                                    || objOldItem.GearChildren.DeepAny(
                                                                        x => x.Children,
                                                                        x => !string.IsNullOrEmpty(x.Weight))
                                                                    || objOldItem.Children.DeepAny(
                                                                        x => x.Children,
                                                                        y => !string.IsNullOrEmpty(y.Weight)
                                                                             || y.GearChildren.DeepAny(
                                                                                 x => x.Children,
                                                                                 x => !string
                                                                                     .IsNullOrEmpty(x.Weight)))))
                                {
                                    blnDoEncumbranceRefresh = true;
                                }

                                if (await objOldItem.GetParentAsync(token).ConfigureAwait(false) == this)
                                    await objOldItem.SetParentAsync(null, token).ConfigureAwait(false);

                                if (setAttributesToRefresh.Count < CyberlimbAttributeAbbrevs.Count
                                    && Parent?.InheritAttributes != false && await GetParentVehicleAsync(token).ConfigureAwait(false) == null &&
                                    !await _objCharacter.Settings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false) && await GetIsLimbAsync(token).ConfigureAwait(false) &&
                                    !(await _objCharacter.Settings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(await GetLimbSlotAsync(token).ConfigureAwait(false)))
                                {
                                    if (InheritAttributes)
                                    {
                                        setAttributesToRefresh.AddRange(CyberlimbAttributeAbbrevs);
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in
                                                 s_AttributeAffectingCyberwares)
                                        {
                                            if (!setAttributesToRefresh.Contains(kvpToCheck.Key)
                                                && kvpToCheck.Value.Contains(objOldItem.Name))
                                                setAttributesToRefresh.Add(kvpToCheck.Key);
                                        }
                                    }
                                }

                                if (!blnDoEssenceImprovementsRefresh && (GetParentAsync(token) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) &&
                                    string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) && await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                                    blnDoEssenceImprovementsRefresh = true;
                            }

                            await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
                            blnDoRedlinerRefresh = true;
                            break;
                        }

                        case NotifyCollectionChangedAction.Replace:
                        {
                            // ReSharper disable once AssignNullToNotNullAttribute
                            HashSet<Cyberware> setNewItems = e.NewItems.OfType<Cyberware>().ToHashSet();
                            List<Cyberware> lstOldItems = e.OldItems?.OfType<Cyberware>().ToList() ?? new List<Cyberware>();
                            foreach (Cyberware objOldItem in lstOldItems)
                            {
                                if (setNewItems.Contains(objOldItem))
                                    continue;
                                if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh
                                                                && await objOldItem
                                                                    .GetIsModularCurrentlyEquippedAsync(token)
                                                                    .ConfigureAwait(false)
                                                                && (!string.IsNullOrEmpty(Weight)
                                                                    || !string.IsNullOrEmpty(objOldItem.Weight)
                                                                    || objOldItem.GearChildren.DeepAny(
                                                                        x => x.Children,
                                                                        x => !string.IsNullOrEmpty(x.Weight))
                                                                    || objOldItem.Children.DeepAny(
                                                                        x => x.Children,
                                                                        y => !string.IsNullOrEmpty(y.Weight)
                                                                             || y.GearChildren.DeepAny(
                                                                                 x => x.Children,
                                                                                 x => !string
                                                                                     .IsNullOrEmpty(x.Weight)))))
                                {
                                    blnDoEncumbranceRefresh = true;
                                }

                                if (await objOldItem.GetParentAsync(token).ConfigureAwait(false) == this)
                                    await objOldItem.SetParentAsync(null, token).ConfigureAwait(false);

                                if (setAttributesToRefresh.Count < CyberlimbAttributeAbbrevs.Count
                                    && Parent?.InheritAttributes != false && await GetParentVehicleAsync(token).ConfigureAwait(false) == null &&
                                    !await _objCharacter.Settings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false) && await GetIsLimbAsync(token).ConfigureAwait(false) &&
                                    !(await _objCharacter.Settings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(await GetLimbSlotAsync(token).ConfigureAwait(false)))
                                {
                                    if (await GetInheritAttributesAsync(token).ConfigureAwait(false))
                                    {
                                        setAttributesToRefresh.AddRange(CyberlimbAttributeAbbrevs);
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in
                                                 s_AttributeAffectingCyberwares)
                                        {
                                            if (!setAttributesToRefresh.Contains(kvpToCheck.Key)
                                                && kvpToCheck.Value.Contains(objOldItem.Name))
                                                setAttributesToRefresh.Add(kvpToCheck.Key);
                                        }
                                    }
                                }

                                if (!blnDoEssenceImprovementsRefresh && (GetParentAsync(token) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) &&
                                    string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) && await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                                    blnDoEssenceImprovementsRefresh = true;
                            }

                            foreach (Cyberware objNewItem in setNewItems)
                            {
                                await objNewItem.SetParentAsync(this, token).ConfigureAwait(false);
                                if (await objNewItem.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                                {
                                    if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh
                                                                    && (!string.IsNullOrEmpty(Weight)
                                                                        || !string.IsNullOrEmpty(objNewItem.Weight)
                                                                        || objNewItem.GearChildren.DeepAny(
                                                                            x => x.Children,
                                                                            x => !string.IsNullOrEmpty(x.Weight))
                                                                        || objNewItem.Children.DeepAny(
                                                                            x => x.Children,
                                                                            y => !string.IsNullOrEmpty(y.Weight)
                                                                                || y.GearChildren.DeepAny(
                                                                                    x => x.Children,
                                                                                    x => !string
                                                                                        .IsNullOrEmpty(x.Weight)))))
                                        blnDoEncumbranceRefresh = true;
                                    lstImprovementSourcesToProcess.Add(objNewItem);
                                }

                                if (setAttributesToRefresh.Count < CyberlimbAttributeAbbrevs.Count
                                    && Parent?.InheritAttributes != false && await GetParentVehicleAsync(token).ConfigureAwait(false) == null &&
                                    !await _objCharacter.Settings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false) && await GetIsLimbAsync(token).ConfigureAwait(false) &&
                                    !(await _objCharacter.Settings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(await GetLimbSlotAsync(token).ConfigureAwait(false)))
                                {
                                    if (await GetInheritAttributesAsync(token).ConfigureAwait(false))
                                    {
                                        setAttributesToRefresh.AddRange(CyberlimbAttributeAbbrevs);
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in
                                                 s_AttributeAffectingCyberwares)
                                        {
                                            if (!setAttributesToRefresh.Contains(kvpToCheck.Key)
                                                && kvpToCheck.Value.Contains(objNewItem.Name))
                                                setAttributesToRefresh.Add(kvpToCheck.Key);
                                        }
                                    }
                                }

                                if (!blnDoEssenceImprovementsRefresh && (GetParentAsync(token) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) &&
                                    string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) && await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                                    blnDoEssenceImprovementsRefresh = true;
                            }

                            await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
                            blnDoRedlinerRefresh = true;
                            break;
                        }

                        case NotifyCollectionChangedAction.Reset:
                            blnDoEssenceImprovementsRefresh = true;
                            blnDoEncumbranceRefresh = blnEverDoEncumbranceRefresh;
                            if (Parent?.InheritAttributes != false && await GetParentVehicleAsync(token).ConfigureAwait(false) == null &&
                                !await _objCharacter.Settings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false) && await GetIsLimbAsync(token).ConfigureAwait(false) &&
                                !(await _objCharacter.Settings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(await GetLimbSlotAsync(token).ConfigureAwait(false)))
                            {
                                setAttributesToRefresh.AddRange(CyberlimbAttributeAbbrevs);
                            }

                            await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
                            blnDoRedlinerRefresh = true;
                            break;
                    }

                    using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>>(
                               Utils.DictionaryForMultiplePropertyChangedPool,
                               out Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>
                                   dicChangedProperties))
                    {
                        try
                        {
                            // Note: Movement is always handled whenever AGI or STR is changed, regardless of whether we use cyberleg movement
                            foreach (string strAbbrev in setAttributesToRefresh)
                            {
                                foreach (CharacterAttrib objCharacterAttrib in
                                         _objCharacter.GetAllAttributes(strAbbrev, token: token))
                                {
                                    if (objCharacterAttrib == null)
                                        continue;
                                    if (!dicChangedProperties.TryGetValue(objCharacterAttrib,
                                            out HashSet<string> setChangedProperties))
                                    {
                                        setChangedProperties = Utils.StringHashSetPool.Get();
                                        dicChangedProperties.Add(objCharacterAttrib, setChangedProperties);
                                    }

                                    setChangedProperties.Add(nameof(CharacterAttrib.TotalValue));
                                }
                            }

                            if (!_objCharacter.IsLoading)
                            {
                                if (blnDoRedlinerRefresh)
                                {
                                    if (!dicChangedProperties.TryGetValue(_objCharacter,
                                            out HashSet<string> setChangedProperties))
                                    {
                                        setChangedProperties = Utils.StringHashSetPool.Get();
                                        dicChangedProperties.Add(_objCharacter, setChangedProperties);
                                    }

                                    setChangedProperties.Add(nameof(Character.RedlinerBonus));
                                }

                                if (blnDoEssenceImprovementsRefresh)
                                {
                                    if (!dicChangedProperties.TryGetValue(_objCharacter,
                                            out HashSet<string> setChangedProperties))
                                    {
                                        setChangedProperties = Utils.StringHashSetPool.Get();
                                        dicChangedProperties.Add(_objCharacter, setChangedProperties);
                                    }

                                    setChangedProperties.Add(EssencePropertyName);
                                }

                                if (blnDoEncumbranceRefresh)
                                {
                                    if (!dicChangedProperties.TryGetValue(_objCharacter,
                                            out HashSet<string> setChangedProperties))
                                    {
                                        setChangedProperties = Utils.StringHashSetPool.Get();
                                        dicChangedProperties.Add(_objCharacter, setChangedProperties);
                                    }

                                    setChangedProperties.Add(nameof(Character.TotalCarriedWeight));
                                }

                                if (lstImprovementSourcesToProcess.Count > 0)
                                {
                                    foreach (Cyberware objItem in lstImprovementSourcesToProcess)
                                    {
                                        // Needed in order to properly process named sources where
                                        // the tooltip was built before the object was added to the character
                                        await _objCharacter.Improvements.ForEachAsync(objImprovement =>
                                        {
                                            if (objImprovement.SourceName.TrimEndOnce("Pair").TrimEndOnce("Wireless")
                                                != objItem.InternalId || !objImprovement.Enabled)
                                                return;
                                            foreach ((INotifyMultiplePropertiesChangedAsync objItemToUpdate,
                                                         string strPropertyToUpdate) in objImprovement
                                                         .GetRelevantPropertyChangers())
                                            {
                                                if (!dicChangedProperties.TryGetValue(
                                                        objItemToUpdate, out HashSet<string> setChangedProperties))
                                                {
                                                    setChangedProperties = Utils.StringHashSetPool.Get();
                                                    dicChangedProperties.Add(objItemToUpdate, setChangedProperties);
                                                }

                                                setChangedProperties.Add(strPropertyToUpdate);
                                            }
                                        }, token: token).ConfigureAwait(false);
                                    }
                                }
                            }

                            foreach (KeyValuePair<INotifyMultiplePropertiesChangedAsync, HashSet<string>> kvpToProcess in
                                     dicChangedProperties)
                            {
                                await kvpToProcess.Key
                                    .OnMultiplePropertiesChangedAsync(kvpToProcess.Value.ToList(), token)
                                    .ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            List<HashSet<string>> lstToReturn = dicChangedProperties.Values.ToList();
                            for (int i = lstToReturn.Count - 1; i >= 0; --i)
                            {
                                HashSet<string> setLoop = lstToReturn[i];
                                Utils.StringHashSetPool.Return(ref setLoop);
                            }
                        }
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private async Task GearChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            bool blnDoEquipped = _objCharacter?.IsLoading == false;
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                blnDoEquipped = blnDoEquipped &&
                                await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false) &&
                                ParentVehicle == null;
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (Gear objNewItem in e.NewItems)
                        {
                            await objNewItem.SetParentAsync(this, token).ConfigureAwait(false);
                            if (blnDoEquipped)
                                await objNewItem.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (Gear objOldItem in e.OldItems)
                        {
                            await objOldItem.SetParentAsync(null, token).ConfigureAwait(false);
                            if (blnDoEquipped)
                                await objOldItem.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                        }

                        break;

                    case NotifyCollectionChangedAction.Replace:
                        foreach (Gear objOldItem in e.OldItems)
                        {
                            await objOldItem.SetParentAsync(null, token).ConfigureAwait(false);
                            if (blnDoEquipped)
                                await objOldItem.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                        }

                        foreach (Gear objNewItem in e.NewItems)
                        {
                            await objNewItem.SetParentAsync(this, token).ConfigureAwait(false);
                            if (blnDoEquipped)
                                await objNewItem.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                        }

                        break;

                    case NotifyCollectionChangedAction.Reset:
                        if (blnDoEquipped)
                            await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token)
                                .ConfigureAwait(false);
                        break;
                }

                await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Create a Cyberware from an XmlNode.
        /// </summary>
        /// <param name="objXmlCyberware">XmlNode to create the object from.</param>
        /// <param name="objGrade">Grade of the selected piece.</param>
        /// <param name="objSource">Source of the piece.</param>
        /// <param name="intRating">Selected Rating of the piece of Cyberware.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="lstVehicles">List of Vehicles that should be added to the Character.</param>
        /// <param name="blnCreateImprovements">Whether Improvements should be created.</param>
        /// <param name="blnCreateChildren">Whether child items should be created.</param>
        /// <param name="strForced">Force a particular value to be selected by an Improvement prompts.</param>
        /// <param name="objParent">Cyberware to which this new cyberware should be added (needed in creation method for selecting a side).</param>
        /// <param name="objParentVehicle">Vehicle to which this new cyberware will be added (needed in creation method for selecting a side and improvements).</param>
        /// <param name="blnSkipSelectForms">Whether to skip forms that are created for bonuses. Use only when creating Gear for previews in selection forms.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlCyberware, Grade objGrade, Improvement.ImprovementSource objSource,
            int intRating, IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles, bool blnCreateImprovements = true,
            bool blnCreateChildren = true, string strForced = "", Cyberware objParent = null,
            Vehicle objParentVehicle = null, bool blnSkipSelectForms = false, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlCyberware, objGrade, objSource, intRating,
                lstWeapons, lstVehicles,
                blnCreateImprovements, blnCreateChildren, strForced, objParent, objParentVehicle, blnSkipSelectForms,
                token), token);
        }

        /// <summary>
        /// Create a Cyberware from an XmlNode.
        /// </summary>
        /// <param name="objXmlCyberware">XmlNode to create the object from.</param>
        /// <param name="objGrade">Grade of the selected piece.</param>
        /// <param name="objSource">Source of the piece.</param>
        /// <param name="intRating">Selected Rating of the piece of Cyberware.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="lstVehicles">List of Vehicles that should be added to the Character.</param>
        /// <param name="blnCreateImprovements">Whether Improvements should be created.</param>
        /// <param name="blnCreateChildren">Whether child items should be created.</param>
        /// <param name="strForced">Force a particular value to be selected by an Improvement prompts.</param>
        /// <param name="objParent">Cyberware to which this new cyberware should be added (needed in creation method for selecting a side).</param>
        /// <param name="objParentVehicle">Vehicle to which this new cyberware will be added (needed in creation method for selecting a side and improvements).</param>
        /// <param name="blnSkipSelectForms">Whether to skip forms that are created for bonuses. Use only when creating Gear for previews in selection forms.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task CreateAsync(XmlNode objXmlCyberware, Grade objGrade, Improvement.ImprovementSource objSource,
            int intRating, IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles, bool blnCreateImprovements = true,
            bool blnCreateChildren = true, string strForced = "", Cyberware objParent = null,
            Vehicle objParentVehicle = null, bool blnSkipSelectForms = false, CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlCyberware, objGrade, objSource, intRating, lstWeapons, lstVehicles,
                blnCreateImprovements, blnCreateChildren, strForced, objParent, objParentVehicle, blnSkipSelectForms,
                token);
        }

        private async Task CreateCoreAsync(bool blnSync, XmlNode objXmlCyberware, Grade objGrade, Improvement.ImprovementSource objSource,
            int intRating, IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles, bool blnCreateImprovements = true,
            bool blnCreateChildren = true, string strForced = "", Cyberware objParent = null,
            Vehicle objParentVehicle = null, bool blnSkipSelectForms = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objSyncLocker = null;
            IAsyncDisposable objAsyncLocker = null;
            if (blnSync)
                objSyncLocker = LockObject.EnterWriteLock(token);
            else
                objAsyncLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _blnDoPropertyChangedInCollectionChanged = false;
                try
                {
                    if (blnSync)
                        Parent = objParent;
                    else
                        await SetParentAsync(objParent, token).ConfigureAwait(false);
                    _strForced = strForced;
                    _objParentVehicle = objParentVehicle;

                    if (!objXmlCyberware.TryGetField("id", Guid.TryParse, out _guiSourceID))
                    {
                        Log.Warn(new object[] { "Missing id field for cyberware xmlnode", objXmlCyberware });
                        Utils.BreakIfDebug();
                    }
                    else
                    {
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }

                    objXmlCyberware.TryGetStringFieldQuickly("name", ref _strName);
                    objXmlCyberware.TryGetStringFieldQuickly("category", ref _strCategory);
                    objXmlCyberware.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
                    objXmlCyberware.TryGetStringFieldQuickly("limbslotcount", ref _strLimbSlotCount);
                    if (!objXmlCyberware.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                        objXmlCyberware.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

                    string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                    objXmlCyberware.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                    _colNotes = ColorTranslator.FromHtml(sNotesColor);

                    _blnInheritAttributes = objXmlCyberware["inheritattributes"] != null;
                    _objGrade = objGrade;
                    objXmlCyberware.TryGetStringFieldQuickly("ess", ref _strESS);
                    objXmlCyberware.TryGetStringFieldQuickly("capacity", ref _strCapacity);
                    objXmlCyberware.TryGetStringFieldQuickly("avail", ref _strAvail);
                    objXmlCyberware.TryGetStringFieldQuickly("source", ref _strSource);
                    objXmlCyberware.TryGetStringFieldQuickly("page", ref _strPage);
                    if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                    {
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverload
                            Notes = CommonFunctions.GetBookNotes(objXmlCyberware, Name, CurrentDisplayName, Source,
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                Page, DisplayPage(GlobalSettings.Language), _objCharacter, token);
                        else
                            Notes = await CommonFunctions.GetBookNotesAsync(objXmlCyberware, Name,
                                await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                                await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter, token).ConfigureAwait(false);
                    }

                    _blnAddToParentESS = objXmlCyberware["addtoparentess"] != null
                                         && objXmlCyberware["addtoparentess"].InnerText != bool.FalseString;
                    _blnAddToParentCapacity = objXmlCyberware["addtoparentcapacity"] != null
                                              && objXmlCyberware["addtoparentcapacity"].InnerText != bool.FalseString;
                    _blnIsGeneware = objXmlCyberware["isgeneware"] != null
                                     && objXmlCyberware["isgeneware"].InnerText != bool.FalseString;
                    _nodBonus = objXmlCyberware["bonus"];
                    _nodPairBonus = objXmlCyberware["pairbonus"];
                    _nodWirelessBonus = objXmlCyberware["wirelessbonus"];
                    _nodWirelessPairBonus = objXmlCyberware["wirelesspairbonus"];
                    _blnWirelessOn = _nodWirelessPairBonus != null;
                    _nodAllowGear = objXmlCyberware["allowgear"];
                    objXmlCyberware.TryGetStringFieldQuickly("mountsto", ref _strPlugsIntoModularMount);
                    objXmlCyberware.TryGetStringFieldQuickly("modularmount", ref _strHasModularMount);
                    objXmlCyberware.TryGetStringFieldQuickly("blocksmounts", ref _strBlocksMounts);

                    _eImprovementSource = objSource;
                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    objXmlCyberware.TryGetStringFieldQuickly("rating", ref _strMaxRating);
                    objXmlCyberware.TryGetStringFieldQuickly("minrating", ref _strMinRating);
                    objXmlCyberware.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);

                    objXmlCyberware.TryGetInt32FieldQuickly("minagility", ref _intMinAgility);
                    objXmlCyberware.TryGetInt32FieldQuickly("minstrength", ref _intMinStrength);

                    _intRating = blnSync
                        ? Math.Min(Math.Max(intRating, MinRating), MaxRating)
                        : Math.Min(Math.Max(intRating, await GetMinRatingAsync(token).ConfigureAwait(false)), await GetMaxRatingAsync(token).ConfigureAwait(false));

                    objXmlCyberware.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
                    objXmlCyberware.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
                    if (!objXmlCyberware.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
                    {
                        objXmlCyberware.TryGetStringFieldQuickly("attack", ref _strAttack);
                        objXmlCyberware.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                        objXmlCyberware.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                        objXmlCyberware.TryGetStringFieldQuickly("firewall", ref _strFirewall);
                    }
                    else
                    {
                        _blnCanSwapAttributes = true;
                        string[] strArray = _strAttributeArray.Split(',');
                        _strAttack = strArray[0];
                        _strSleaze = strArray[1];
                        _strDataProcessing = strArray[2];
                        _strFirewall = strArray[3];
                    }

                    objXmlCyberware.TryGetStringFieldQuickly("modattack", ref _strModAttack);
                    objXmlCyberware.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
                    objXmlCyberware.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
                    objXmlCyberware.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
                    objXmlCyberware.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

                    objXmlCyberware.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
                    objXmlCyberware.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);

                    objXmlCyberware.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);

                    // Add Subsytem information if applicable.
                    XPathNavigator xmlCyberwareNavigator = objXmlCyberware.CreateNavigator();
                    XPathNavigator xmlAllowSubsystems =
                        xmlCyberwareNavigator.SelectSingleNodeAndCacheExpression("allowsubsystems", token);
                    if (xmlAllowSubsystems != null)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                   out StringBuilder sbdSubsystem))
                        {
                            foreach (XPathNavigator xmlSubsystem in xmlAllowSubsystems.SelectAndCacheExpression(
                                         "category", token))
                            {
                                sbdSubsystem.Append(xmlSubsystem.Value).Append(',');
                            }

                            if (sbdSubsystem.Length > 0)
                                --sbdSubsystem.Length;
                            _strAllowSubsystems = sbdSubsystem.ToString();
                        }
                    }

                    XPathNavigator xmlPairInclude =
                        xmlCyberwareNavigator.SelectSingleNodeAndCacheExpression("pairinclude", token);
                    if (xmlPairInclude != null)
                    {
                        if (xmlPairInclude.SelectSingleNodeAndCacheExpression("@includeself", token)?.Value !=
                            bool.FalseString)
                        {
                            _lstIncludeInPairBonus.Add(Name);
                        }

                        foreach (XPathNavigator objPairNameNode in xmlPairInclude.SelectAndCacheExpression("name", token))
                        {
                            _lstIncludeInPairBonus.Add(objPairNameNode.Value);
                        }
                    }
                    else
                        _lstIncludeInPairBonus.Add(Name);

                    xmlPairInclude = xmlCyberwareNavigator.SelectSingleNodeAndCacheExpression("wirelesspairinclude", token);
                    if (xmlPairInclude != null)
                    {
                        if (xmlPairInclude.SelectSingleNodeAndCacheExpression("@includeself", token)?.Value !=
                            bool.FalseString)
                        {
                            _lstIncludeInWirelessPairBonus.Add(Name);
                        }

                        foreach (XPathNavigator objPairNameNode in xmlPairInclude.SelectAndCacheExpression("name", token))
                        {
                            _lstIncludeInPairBonus.Add(objPairNameNode.Value);
                        }
                    }
                    else
                        _lstIncludeInWirelessPairBonus.Add(Name);

                    if (!objXmlCyberware.TryGetStringFieldQuickly("cost", ref _strCost))
                        _strCost = "0";
                    objXmlCyberware.TryGetStringFieldQuickly("weight", ref _strWeight);

                    // Check for a Variable Cost.
                    if (_strCost.StartsWith("Variable(", StringComparison.Ordinal))
                    {
                        string strFirstHalf = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                        string strSecondHalf = string.Empty;
                        int intHyphenIndex = strFirstHalf.IndexOf('-');
                        if (intHyphenIndex != -1)
                        {
                            if (intHyphenIndex + 1 < strFirstHalf.Length)
                                strSecondHalf = strFirstHalf.Substring(intHyphenIndex + 1);
                            strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
                        }

                        if (!blnSkipSelectForms)
                        {
                            decimal decMin;
                            decimal decMax = decimal.MaxValue;
                            if (intHyphenIndex != -1)
                            {
                                decMin = Convert.ToDecimal(strFirstHalf, GlobalSettings.InvariantCultureInfo);
                                decMax = Convert.ToDecimal(strSecondHalf, GlobalSettings.InvariantCultureInfo);
                            }
                            else
                                decMin = Convert.ToDecimal(strFirstHalf.FastEscape('+'),
                                    GlobalSettings.InvariantCultureInfo);

                            if (decMin != decimal.MinValue || decMax != decimal.MaxValue)
                            {
                                if (decMax > 1000000)
                                    decMax = 1000000;

                                if (blnSync)
                                {
                                    using (ThreadSafeForm<SelectNumber> frmPickNumber
                                           // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                           = ThreadSafeForm<SelectNumber>.Get(
                                               () => new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
                                               {
                                                   Minimum = decMin,
                                                   Maximum = decMax,
                                                   Description = string.Format(
                                                       GlobalSettings.CultureInfo,
                                                       LanguageManager.GetString("String_SelectVariableCost", token: token),
                                                       CurrentDisplayNameShort),
                                                   AllowCancel = false
                                               }))
                                    {
                                        // ReSharper disable once MethodHasAsyncOverload
                                        if (frmPickNumber.ShowDialogSafe(_objCharacter, token) == DialogResult.Cancel)
                                        {
                                            _guiID = Guid.Empty;
                                            return;
                                        }

                                        _strCost = frmPickNumber.MyForm.SelectedValue.ToString(
                                            GlobalSettings.InvariantCultureInfo);
                                    }
                                }
                                else
                                {
                                    string strDescription = string.Format(
                                        GlobalSettings.CultureInfo,
                                        await LanguageManager.GetStringAsync("String_SelectVariableCost", token: token).ConfigureAwait(false),
                                        await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false));
                                    using (ThreadSafeForm<SelectNumber> frmPickNumber
                                           = await ThreadSafeForm<SelectNumber>.GetAsync(
                                               () => new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
                                               {
                                                   Minimum = decMin,
                                                   Maximum = decMax,
                                                   Description = strDescription,
                                                   AllowCancel = false
                                               }, token).ConfigureAwait(false))
                                    {
                                        if (await frmPickNumber.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) == DialogResult.Cancel)
                                        {
                                            _guiID = Guid.Empty;
                                            return;
                                        }

                                        _strCost = frmPickNumber.MyForm.SelectedValue.ToString(
                                            GlobalSettings.InvariantCultureInfo);
                                    }
                                }
                            }
                            else
                                _strCost = strFirstHalf;
                        }
                        else
                            _strCost = strFirstHalf;
                    }

                    if (blnCreateChildren)
                    {
                        // Add Cyberweapons if applicable.
                        XmlDocument objXmlWeaponDocument = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? _objCharacter.LoadData("weapons.xml", token: token)
                            : await _objCharacter.LoadDataAsync("weapons.xml", token: token).ConfigureAwait(false);

                        // More than one Weapon can be added, so loop through all occurrences.
                        foreach (XmlNode objXmlAddWeapon in objXmlCyberware.SelectNodes("addweapon"))
                        {
                            XmlNode objXmlWeapon = objXmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon",
                                objXmlAddWeapon.InnerText);

                            if (objXmlWeapon != null)
                            {
                                Weapon objGearWeapon = new Weapon(_objCharacter);
                                if (blnSync)
                                    objGearWeapon.ParentVehicle = ParentVehicle;
                                else
                                    await objGearWeapon.SetParentVehicleAsync(await GetParentVehicleAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                                int intAddWeaponRating = 0;
                                string strLoopRating = objXmlAddWeapon.Attributes["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strLoopRating))
                                {
                                    strLoopRating = blnSync
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        ? strLoopRating.CheapReplace("{Rating}",
                                            () => Rating.ToString(
                                                GlobalSettings
                                                    .InvariantCultureInfo))
                                        : await strLoopRating.CheapReplaceAsync("{Rating}",
                                            async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(
                                                GlobalSettings
                                                    .InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    int.TryParse(strLoopRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                        out intAddWeaponRating);
                                }

                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objGearWeapon.Create(objXmlWeapon, lstWeapons, blnCreateChildren,
                                        blnCreateImprovements,
                                        blnSkipSelectForms, intAddWeaponRating, token: token);
                                else
                                    await objGearWeapon.CreateAsync(objXmlWeapon, lstWeapons, blnCreateChildren,
                                        blnCreateImprovements,
                                        blnSkipSelectForms, intAddWeaponRating, token: token).ConfigureAwait(false);
                                objGearWeapon.ParentID = InternalId;
                                objGearWeapon.Cost = "0";

                                if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID))
                                    lstWeapons.Add(objGearWeapon);
                            }
                        }

                        string strWeaponId = blnSync ? Parent?.WeaponID : (await GetParentAsync(token).ConfigureAwait(false))?.WeaponID;
                        if (!string.IsNullOrEmpty(strWeaponId) && !strWeaponId.IsEmptyGuid())
                        {
                            Weapon objWeapon = ParentVehicle != null
                                ? ParentVehicle.Weapons.FindById(strWeaponId)
                                : _objCharacter.Weapons.FindById(strWeaponId);

                            if (objWeapon != null)
                            {
                                foreach (XmlNode objXml in objXmlCyberware.SelectNodes("addparentweaponaccessory"))
                                {
                                    XmlNode objXmlAccessory = objXmlWeaponDocument.TryGetNodeByNameOrId(
                                        "/chummer/accessories/accessory",
                                        objXml.InnerText);

                                    if (objXmlAccessory == null) continue;
                                    WeaponAccessory objGearWeapon = new WeaponAccessory(_objCharacter);
                                    int intAddWeaponRating = 0;
                                    string strLoopRating = objXml.Attributes["rating"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strLoopRating))
                                    {
                                        strLoopRating = blnSync
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            ? strLoopRating.CheapReplace("{Rating}",
                                                () => Rating.ToString(
                                                    GlobalSettings
                                                        .InvariantCultureInfo))
                                            : await strLoopRating.CheapReplaceAsync("{Rating}",
                                                async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(
                                                    GlobalSettings
                                                        .InvariantCultureInfo), token: token).ConfigureAwait(false);
                                        int.TryParse(strLoopRating, NumberStyles.Any,
                                            GlobalSettings.InvariantCultureInfo,
                                            out intAddWeaponRating);
                                    }

                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverload
                                        objGearWeapon.Create(objXmlAccessory,
                                            new Tuple<string, string>(string.Empty, string.Empty),
                                            intAddWeaponRating,
                                            blnSkipSelectForms, true, blnCreateImprovements, token);
                                    else
                                        await objGearWeapon.CreateAsync(objXmlAccessory,
                                            new Tuple<string, string>(string.Empty, string.Empty),
                                            intAddWeaponRating,
                                            blnSkipSelectForms, true, blnCreateImprovements, token).ConfigureAwait(false);
                                    objGearWeapon.Cost = "0";
                                    objGearWeapon.ParentID = InternalId;
                                    objGearWeapon.Parent = objWeapon;

                                    if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponAccessoryID))
                                    {
                                        if (blnSync)
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            objWeapon.WeaponAccessories.Add(objGearWeapon);
                                        else
                                            await objWeapon.WeaponAccessories.AddAsync(objGearWeapon, token).ConfigureAwait(false);
                                    }
                                }
                            }
                        }

                        // Add Drone Bodyparts if applicable.
                        XmlDocument objXmlVehicleDocument = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? _objCharacter.LoadData("vehicles.xml", token: token)
                            : await _objCharacter.LoadDataAsync("vehicles.xml", token: token).ConfigureAwait(false);

                        // More than one Weapon can be added, so loop through all occurrences.
                        foreach (XmlNode xmlAddVehicle in objXmlCyberware.SelectNodes("addvehicle"))
                        {
                            string strLoopID = xmlAddVehicle.InnerText;
                            XmlNode xmlVehicle =
                                objXmlVehicleDocument.TryGetNodeByNameOrId("/chummer/vehicles/vehicle", strLoopID);

                            if (xmlVehicle != null)
                            {
                                Vehicle objVehicle = new Vehicle(_objCharacter);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objVehicle.Create(xmlVehicle, blnSkipSelectForms, true, blnCreateImprovements,
                                        blnSkipSelectForms, token: token);
                                else
                                    await objVehicle.CreateAsync(xmlVehicle, blnSkipSelectForms, true,
                                        blnCreateImprovements,
                                        blnSkipSelectForms, token: token).ConfigureAwait(false);
                                objVehicle.ParentID = InternalId;

                                if (Guid.TryParse(objVehicle.InternalId, out _guiVehicleID))
                                    lstVehicles.Add(objVehicle);
                            }
                        }
                    }

                    /*
                     * This needs to be handled separately from usual bonus nodes because:
                     * - Children must always inherit the side of their parent(s)
                     * - In case of numerical limits, we must be able to apply them separately to each side
                     * - Modular cyberlimbs need a constant side regardless of their equip status
                     * - In cases where modular mounts might get blocked, we must force the 'ware to the unblocked side
                     */
                    if (objXmlCyberware["selectside"] != null)
                    {
                        string strParentSide = blnSync ? Parent?.Location : (await GetParentAsync(token).ConfigureAwait(false))?.Location;
                        if (!string.IsNullOrEmpty(strParentSide))
                        {
                            _strLocation = strParentSide;
                        }
                        else if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            if (!GetValidLimbSlot(objXmlCyberware.CreateNavigator()))
                                return;
                        }
                        else if (!await GetValidLimbSlotAsync(objXmlCyberware.CreateNavigator(), token).ConfigureAwait(false))
                            return;
                    }

                    // If the piece grants a bonus, pass the information to the Improvement Manager.
                    // Modular cyberlimbs only get their bonuses applied when they are equipped onto a limb, so we're skipping those here
                    if ((Bonus != null || PairBonus != null) && !blnSkipSelectForms)
                    {
                        if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                            ImprovementManager.SetForcedValue(_strForced, _objCharacter);

                        if (Bonus != null)
                        {
                            if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                if (!ImprovementManager.CreateImprovements(_objCharacter, objSource,
                                        _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, Rating,
                                        CurrentDisplayNameShort, blnCreateImprovements, token))
                                {
                                    _guiID = Guid.Empty;
                                    return;
                                }
                            }
                            else if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, objSource,
                                         _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, await GetRatingAsync(token).ConfigureAwait(false),
                                         await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), blnCreateImprovements, token).ConfigureAwait(false))
                            {
                                _guiID = Guid.Empty;
                                return;
                            }
                        }

                        string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                            _strExtra = strSelectedValue;

                        if (PairBonus != null)
                        {
                            // This cyberware should not be included in the count to make things easier.
                            List<Cyberware> lstPairableCyberwares = blnSync
                                ? _objCharacter.Cyberware.DeepWhere(x => x.Children,
                                    x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                         x.IsModularCurrentlyEquipped, token).ToList()
                                : await _objCharacter.Cyberware.DeepWhereAsync(x => x.Children,
                                    async x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                               await x.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                            int intCount = lstPairableCyberwares.Count;
                            // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                            if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                            {
                                intCount = 0;
                                foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                                {
                                    if (objPairableCyberware.Location != Location)
                                        // We have found a cyberware with which this one could be paired, so increase count by 1
                                        ++intCount;
                                    else
                                        // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                        --intCount;
                                }

                                // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                                intCount = (intCount > 0).ToInt32();
                            }

                            if ((intCount & 1) == 1)
                            {
                                if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                    ImprovementManager.SetForcedValue(_strForced, _objCharacter);
                                else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                    ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverload
                                    if (!ImprovementManager.CreateImprovements(_objCharacter, objSource,
                                            _guiID.ToString(
                                                "D", GlobalSettings.InvariantCultureInfo)
                                            + "Pair", PairBonus,
                                            Rating,
                                            CurrentDisplayNameShort,
                                            blnCreateImprovements, token))
                                    {
                                        _guiID = Guid.Empty;
                                        return;
                                    }
                                }
                                else if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, objSource,
                                             _guiID.ToString(
                                                 "D", GlobalSettings.InvariantCultureInfo)
                                             + "Pair", PairBonus,
                                             await GetRatingAsync(token).ConfigureAwait(false),
                                             await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                             blnCreateImprovements, token).ConfigureAwait(false))
                                {
                                    _guiID = Guid.Empty;
                                    return;
                                }
                            }
                        }
                    }

                    if (blnSync)
                    {
                        // Retrieve the Bioware or Cyberware ESS Cost Multiplier. Bioware Modifiers do not apply to Genetech.
                        if (_objCharacter.Created)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            SaveNonRetroactiveEssenceModifiers();

                        if (blnCreateChildren)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            CreateChildren(objXmlCyberware, objGrade, lstWeapons, lstVehicles, blnCreateImprovements);

                        if (!string.IsNullOrEmpty(_strPlugsIntoModularMount))
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            ChangeModularEquip(false);

                        if (blnCreateImprovements)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            RefreshWirelessBonuses();
                    }
                    else
                    {
                        // Retrieve the Bioware or Cyberware ESS Cost Multiplier. Bioware Modifiers do not apply to Genetech.
                        if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                            await SaveNonRetroactiveEssenceModifiersAsync(token).ConfigureAwait(false);

                        if (blnCreateChildren)
                            await CreateChildrenAsync(objXmlCyberware, objGrade, lstWeapons, lstVehicles, blnCreateImprovements, token).ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(_strPlugsIntoModularMount))
                            await ChangeModularEquipAsync(false, token: token).ConfigureAwait(false);

                        if (blnCreateImprovements)
                            await RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _blnDoPropertyChangedInCollectionChanged = true;
                    if (blnSync)
                    {
                        if (Children.Count > 0)
                            Utils.SafelyRunSynchronously(() => CyberwareChildrenOnCollectionChanged(
                                this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children),
                                token), token);
                    }
                    else if (await Children.GetCountAsync(token).ConfigureAwait(false) > 0)
                        await CyberwareChildrenOnCollectionChanged(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children), token).ConfigureAwait(false);
                }
            }
            finally
            {
                objSyncLocker?.Dispose();
                if (objAsyncLocker != null)
                    await objAsyncLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool GetValidLimbSlot(XPathNavigator xpnCyberware)
        {
            string strForcedSide = string.Empty;
            using (LockObject.EnterReadLock())
            {
                if (_strForced == "Right" || _strForced == "Left")
                    strForcedSide = _strForced;
                if (string.IsNullOrEmpty(strForcedSide) && ParentVehicle == null)
                {
                    ThreadSafeObservableCollection<Cyberware> lstCyberwareToCheck =
                        Parent == null ? _objCharacter.Cyberware : Parent.Children;
                    Dictionary<string, int> dicNumLeftMountBlockers = new Dictionary<string, int>(6);
                    Dictionary<string, int> dicNumRightMountBlockers = new Dictionary<string, int>(6);
                    lstCyberwareToCheck.ForEach(objCheckCyberware =>
                    {
                        if (string.IsNullOrEmpty(objCheckCyberware.BlocksMounts))
                            return;
                        Dictionary<string, int> dicToUse;
                        switch (objCheckCyberware.Location)
                        {
                            case "Left":
                                dicToUse = dicNumLeftMountBlockers;
                                break;

                            case "Right":
                                dicToUse = dicNumRightMountBlockers;
                                break;

                            default:
                                return;
                        }

                        foreach (string strBlockMount in objCheckCyberware.BlocksMounts.SplitNoAlloc(',',
                                     StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (dicToUse.TryGetValue(strBlockMount, out int intExistingLimbCount))
                                dicToUse[strBlockMount] = intExistingLimbCount + objCheckCyberware.LimbSlotCount;
                            else
                                dicToUse.Add(strBlockMount, objCheckCyberware.LimbSlotCount);
                        }
                    });

                    bool blnAllowLeft = true;
                    bool blnAllowRight = true;
                    // Potentially expensive checks that can (and therefore should) be parallelized. Normally, this would just be a Parallel.Invoke,
                    // but we want to allow UI messages to happen, just in case this is called on the Main Thread and another thread wants to show a message box.
                    // Not using async-await because this is trivial code and I do not want to infect everything that calls this with async as well.
                    Utils.RunWithoutThreadLock(
                        () =>
                        {
                            blnAllowLeft = xpnCyberware.RequirementsMet(_objCharacter, Parent, strLocation: "Left");
                            if (!blnAllowLeft)
                                return;
                            if (!string.IsNullOrEmpty(HasModularMount)
                                && dicNumLeftMountBlockers.TryGetValue(HasModularMount, out int intNumBlockers))
                            {
                                string strLimbTypeOfMount = MountToLimbType(HasModularMount);
                                blnAllowLeft = !string.IsNullOrEmpty(strLimbTypeOfMount)
                                               && _objCharacter.LimbCount(strLimbTypeOfMount) / 2 >= intNumBlockers;
                                if (!blnAllowLeft)
                                    return;
                            }

                            if (string.IsNullOrEmpty(BlocksMounts) || lstCyberwareToCheck.Count == 0)
                                return;
                            using (new FetchSafelyFromPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setBlocksMounts))
                            {
                                setBlocksMounts.AddRange(BlocksMounts
                                    .SplitNoAlloc(
                                        ',', StringSplitOptions.RemoveEmptyEntries));
                                blnAllowLeft = !lstCyberwareToCheck.Any(x =>
                                {
                                    if (string.IsNullOrEmpty(x.HasModularMount))
                                        return false;
                                    if (x.Location != "Left")
                                        return false;
                                    if (!setBlocksMounts.Contains(x.HasModularMount))
                                        return false;
                                    string strLimbTypeOfMount = MountToLimbType(x.HasModularMount);
                                    if (string.IsNullOrEmpty(strLimbTypeOfMount))
                                        return true;

                                    int intLimbSlotCount = LimbSlotCount;
                                    if (dicNumLeftMountBlockers.TryGetValue(x.HasModularMount, out intNumBlockers))
                                        intLimbSlotCount += intNumBlockers;
                                    return _objCharacter.LimbCount(strLimbTypeOfMount) / 2 < intLimbSlotCount;
                                });
                            }
                        },
                        () =>
                        {
                            blnAllowRight = xpnCyberware.RequirementsMet(
                                _objCharacter, Parent, strLocation: "Right");
                            if (!blnAllowRight)
                                return;
                            if (!string.IsNullOrEmpty(HasModularMount)
                                && dicNumRightMountBlockers.TryGetValue(HasModularMount, out int intNumBlockers))
                            {
                                string strLimbTypeOfMount = MountToLimbType(HasModularMount);
                                blnAllowRight = !string.IsNullOrEmpty(strLimbTypeOfMount)
                                                && _objCharacter.LimbCount(strLimbTypeOfMount) / 2 >= intNumBlockers;
                                if (!blnAllowRight)
                                    return;
                            }

                            if (string.IsNullOrEmpty(BlocksMounts) || lstCyberwareToCheck.Count == 0)
                                return;
                            using (new FetchSafelyFromPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setBlocksMounts))
                            {
                                setBlocksMounts.AddRange(BlocksMounts
                                    .SplitNoAlloc(
                                        ',', StringSplitOptions.RemoveEmptyEntries));
                                blnAllowRight = !lstCyberwareToCheck.Any(x =>
                                {
                                    if (string.IsNullOrEmpty(x.HasModularMount))
                                        return false;
                                    if (x.Location != "Right")
                                        return false;
                                    if (!setBlocksMounts.Contains(x.HasModularMount))
                                        return false;
                                    string strLimbTypeOfMount = MountToLimbType(x.HasModularMount);
                                    if (string.IsNullOrEmpty(strLimbTypeOfMount))
                                        return true;

                                    int intLimbSlotCount = LimbSlotCount;
                                    if (dicNumRightMountBlockers.TryGetValue(x.HasModularMount, out intNumBlockers))
                                        intLimbSlotCount += intNumBlockers;
                                    return _objCharacter.LimbCount(strLimbTypeOfMount) / 2 < intLimbSlotCount;
                                });
                            }
                        });
                    // Only one side is allowed.
                    if (blnAllowLeft != blnAllowRight)
                        strForcedSide = blnAllowLeft ? "Left" : "Right";
#if DEBUG
                    // Should never happen, so break immediately if we have a debugger attached
                    else if (!blnAllowLeft && !blnAllowRight)
                        Utils.BreakIfDebug();
#endif
                }

                if (!string.IsNullOrEmpty(strForcedSide))
                {
                    _strLocation = strForcedSide;
                }
                else
                {
                    using (ThreadSafeForm<SelectSide> frmPickSide = ThreadSafeForm<SelectSide>.Get(() => new SelectSide
                           {
                               Description =
                                   string.Format(GlobalSettings.CultureInfo,
                                       LanguageManager.GetString("Label_SelectSide"),
                                       CurrentDisplayNameShort)
                           }))
                    {
                        // Make sure the dialogue window was not canceled.
                        if (frmPickSide.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                        {
                            _guiID = Guid.Empty;
                            return false;
                        }

                        _strLocation = frmPickSide.MyForm.SelectedSide;
                    }
                }

                return true;
            }
        }

        public async Task<bool> GetValidLimbSlotAsync(XPathNavigator xpnCyberware, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strForcedSide = string.Empty;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strForced == "Right" || _strForced == "Left")
                    strForcedSide = _strForced;
                if (string.IsNullOrEmpty(strForcedSide) && await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                {
                    Cyberware objParent = await GetParentAsync(token).ConfigureAwait(false);
                    ThreadSafeObservableCollection<Cyberware> lstCyberwareToCheck =
                        objParent == null
                            ? await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)
                            : objParent.Children;
                    string strHasModularMount = await GetHasModularMountAsync(token).ConfigureAwait(false);
                    string strBlocksMounts = await GetBlocksMountsAsync(token).ConfigureAwait(false);
                    Dictionary<string, int> dicNumLeftMountBlockers = new Dictionary<string, int>(6);
                    Dictionary<string, int> dicNumRightMountBlockers = new Dictionary<string, int>(6);
                    await lstCyberwareToCheck.ForEachAsync(async objCheckCyberware =>
                    {
                        string strLoopBlocksMounts = await objCheckCyberware.GetBlocksMountsAsync(token).ConfigureAwait(false);
                        if (string.IsNullOrEmpty(strLoopBlocksMounts))
                            return;
                        Dictionary<string, int> dicToUse;
                        switch (await objCheckCyberware.GetLocationAsync(token).ConfigureAwait(false))
                        {
                            case "Left":
                                dicToUse = dicNumLeftMountBlockers;
                                break;

                            case "Right":
                                dicToUse = dicNumRightMountBlockers;
                                break;

                            default:
                                return;
                        }

                        foreach (string strBlockMount in strLoopBlocksMounts.SplitNoAlloc(',',
                                     StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (dicToUse.TryGetValue(strBlockMount, out int intExistingLimbCount))
                                dicToUse[strBlockMount] = intExistingLimbCount +
                                                          await objCheckCyberware.GetLimbSlotCountAsync(token)
                                                              .ConfigureAwait(false);
                            else
                                dicToUse.Add(strBlockMount,
                                    await objCheckCyberware.GetLimbSlotCountAsync(token).ConfigureAwait(false));
                        }
                    }, token).ConfigureAwait(false);

                    bool blnAllowLeft = true;
                    bool blnAllowRight = true;
                    // Potentially expensive checks that can (and therefore should) be parallelized. Normally, this would just be a Parallel.Invoke,
                    // but we want to allow UI messages to happen, just in case this is called on the Main Thread and another thread wants to show a message box.
                    // Not using async-await because this is trivial code and I do not want to infect everything that calls this with async as well.
                    await Task.WhenAll(
                        Task.Run(async () =>
                        {
                            blnAllowLeft = await xpnCyberware.RequirementsMetAsync(_objCharacter, objParent, strLocation: "Left", token: token).ConfigureAwait(false);
                            if (!blnAllowLeft)
                                return;
                            if (!string.IsNullOrEmpty(strHasModularMount)
                                && dicNumLeftMountBlockers.TryGetValue(strHasModularMount, out int intNumBlockers))
                            {
                                string strLimbTypeOfMount = MountToLimbType(strHasModularMount);
                                blnAllowLeft = !string.IsNullOrEmpty(strLimbTypeOfMount)
                                               && await _objCharacter.LimbCountAsync(strLimbTypeOfMount, token)
                                                   .ConfigureAwait(false) / 2 >= intNumBlockers;
                                if (!blnAllowLeft)
                                    return;
                            }

                            if (string.IsNullOrEmpty(strBlocksMounts) || lstCyberwareToCheck.Count == 0)
                                return;
                            using (new FetchSafelyFromPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setBlocksMounts))
                            {
                                setBlocksMounts.AddRange(strBlocksMounts
                                    .SplitNoAlloc(
                                        ',', StringSplitOptions.RemoveEmptyEntries));
                                blnAllowLeft = !await lstCyberwareToCheck.AnyAsync(async x =>
                                {
                                    string strLoopHasModularMount = await x.GetHasModularMountAsync(token).ConfigureAwait(false);
                                    if (string.IsNullOrEmpty(strLoopHasModularMount))
                                        return false;
                                    if (await x.GetLocationAsync(token).ConfigureAwait(false) != "Left")
                                        return false;
                                    if (!setBlocksMounts.Contains(strLoopHasModularMount))
                                        return false;
                                    string strLimbTypeOfMount = MountToLimbType(strLoopHasModularMount);
                                    if (string.IsNullOrEmpty(strLimbTypeOfMount))
                                        return true;

                                    int intLimbSlotCount = await GetLimbSlotCountAsync(token).ConfigureAwait(false);
                                    if (dicNumLeftMountBlockers.TryGetValue(strLoopHasModularMount, out intNumBlockers))
                                        intLimbSlotCount += intNumBlockers;
                                    return await _objCharacter.LimbCountAsync(strLimbTypeOfMount, token)
                                        .ConfigureAwait(false) / 2 < intLimbSlotCount;
                                }, token).ConfigureAwait(false);
                            }
                        }, token),
                        Task.Run(async () =>
                        {
                            blnAllowRight = await xpnCyberware.RequirementsMetAsync(
                                _objCharacter, objParent, strLocation: "Right", token: token).ConfigureAwait(false);
                            if (!blnAllowRight)
                                return;
                            if (!string.IsNullOrEmpty(strHasModularMount)
                                && dicNumRightMountBlockers.TryGetValue(strHasModularMount, out int intNumBlockers))
                            {
                                string strLimbTypeOfMount = MountToLimbType(strHasModularMount);
                                blnAllowRight = !string.IsNullOrEmpty(strLimbTypeOfMount)
                                                && await _objCharacter.LimbCountAsync(strLimbTypeOfMount, token)
                                                    .ConfigureAwait(false) / 2 >= intNumBlockers;
                                if (!blnAllowRight)
                                    return;
                            }

                            if (string.IsNullOrEmpty(BlocksMounts) || lstCyberwareToCheck.Count == 0)
                                return;
                            using (new FetchSafelyFromPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setBlocksMounts))
                            {
                                setBlocksMounts.AddRange(BlocksMounts
                                    .SplitNoAlloc(
                                        ',', StringSplitOptions.RemoveEmptyEntries));
                                blnAllowRight = !await lstCyberwareToCheck.AnyAsync(async x =>
                                {
                                    string strLoopHasModularMount = await x.GetHasModularMountAsync(token).ConfigureAwait(false);
                                    if (string.IsNullOrEmpty(strLoopHasModularMount))
                                        return false;
                                    if (await x.GetLocationAsync(token).ConfigureAwait(false) != "Right")
                                        return false;
                                    if (!setBlocksMounts.Contains(strLoopHasModularMount))
                                        return false;
                                    string strLimbTypeOfMount = MountToLimbType(strLoopHasModularMount);
                                    if (string.IsNullOrEmpty(strLimbTypeOfMount))
                                        return true;

                                    int intLimbSlotCount = await GetLimbSlotCountAsync(token).ConfigureAwait(false);
                                    if (dicNumRightMountBlockers.TryGetValue(strLoopHasModularMount, out intNumBlockers))
                                        intLimbSlotCount += intNumBlockers;
                                    return await _objCharacter.LimbCountAsync(strLimbTypeOfMount, token)
                                        .ConfigureAwait(false) / 2 < intLimbSlotCount;
                                }, token).ConfigureAwait(false);
                            }
                        }, token)).ConfigureAwait(false);
                    // Only one side is allowed.
                    if (blnAllowLeft != blnAllowRight)
                        strForcedSide = blnAllowLeft ? "Left" : "Right";
#if DEBUG
                    // Should never happen, so break immediately if we have a debugger attached
                    else if (!blnAllowLeft && !blnAllowRight)
                        Utils.BreakIfDebug();
#endif
                }

                if (!string.IsNullOrEmpty(strForcedSide))
                {
                    _strLocation = strForcedSide;
                }
                else
                {
                    string strDescription = string.Format(GlobalSettings.CultureInfo,
                        await LanguageManager.GetStringAsync("Label_SelectSide", token: token).ConfigureAwait(false),
                        await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false));
                    using (ThreadSafeForm<SelectSide> frmPickSide = await ThreadSafeForm<SelectSide>.GetAsync(() =>
                               new SelectSide
                               {
                                   Description = strDescription
                               }, token).ConfigureAwait(false))
                    {
                        // Make sure the dialogue window was not canceled.
                        if (await frmPickSide.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false) ==
                            DialogResult.Cancel)
                        {
                            _guiID = Guid.Empty;
                            return false;
                        }

                        _strLocation = frmPickSide.MyForm.SelectedSide;
                    }
                }

                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private void CreateChildren(XmlNode objParentNode, Grade objGrade, IList<Weapon> lstWeapons,
            IList<Vehicle> objVehicles, bool blnCreateImprovements = true)
        {
            // If we've just added a new base item, see if there are any subsystems that should automatically be added.
            XmlElement xmlSubsystemsNode = objParentNode["subsystems"];
            if (xmlSubsystemsNode != null)
            {
                // Load Cyberware subsystems first
                using (XmlNodeList objXmlSubSystemNameList = xmlSubsystemsNode.SelectNodes("cyberware"))
                {
                    if (objXmlSubSystemNameList?.Count > 0)
                    {
                        XmlDocument objXmlDocument = _objCharacter.LoadData("cyberware.xml");
                        foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                        {
                            string strName = objXmlSubsystemNode["name"]?.InnerText;
                            if (string.IsNullOrEmpty(strName))
                                continue;
                            XmlNode objXmlSubsystem =
                                objXmlDocument.TryGetNodeByNameOrId("/chummer/cyberwares/cyberware", strName);

                            if (objXmlSubsystem != null)
                            {
                                Cyberware objSubsystem = new Cyberware(_objCharacter);
                                int.TryParse(objXmlSubsystemNode["rating"]?.InnerText, NumberStyles.Any,
                                    GlobalSettings.InvariantCultureInfo, out int intSubSystemRating);
                                objSubsystem.Create(objXmlSubsystem, objGrade, Improvement.ImprovementSource.Cyberware,
                                    intSubSystemRating, lstWeapons, objVehicles, blnCreateImprovements, true,
                                    objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty, this);
                                objSubsystem.ParentID = InternalId;
                                objSubsystem.Cost = "0";
                                // If the <subsystem> tag itself contains extra children, add those, too
                                objSubsystem.CreateChildren(objXmlSubsystemNode, objGrade, lstWeapons, objVehicles,
                                    blnCreateImprovements);

                                _lstChildren.Add(objSubsystem);
                            }
                        }
                    }
                }

                // Load bioware subsystems next
                using (XmlNodeList objXmlSubSystemNameList = xmlSubsystemsNode.SelectNodes("bioware"))
                {
                    if (objXmlSubSystemNameList?.Count > 0)
                    {
                        XmlDocument objXmlDocument = _objCharacter.LoadData("bioware.xml");
                        foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                        {
                            string strName = objXmlSubsystemNode["name"]?.InnerText;
                            if (string.IsNullOrEmpty(strName))
                                continue;
                            XmlNode objXmlSubsystem =
                                objXmlDocument.TryGetNodeByNameOrId("/chummer/biowares/bioware", strName);

                            if (objXmlSubsystem != null)
                            {
                                Cyberware objSubsystem = new Cyberware(_objCharacter);
                                int.TryParse(objXmlSubsystemNode["rating"]?.InnerText, NumberStyles.Any,
                                    GlobalSettings.InvariantCultureInfo, out int intSubSystemRating);
                                objSubsystem.Create(objXmlSubsystem, objGrade, Improvement.ImprovementSource.Bioware,
                                    intSubSystemRating, lstWeapons, objVehicles, blnCreateImprovements, true,
                                    objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty, this);
                                objSubsystem.ParentID = InternalId;
                                objSubsystem.Cost = "0";
                                // If the <subsystem> tag itself contains extra children, add those, too
                                objSubsystem.CreateChildren(objXmlSubsystemNode, objGrade, lstWeapons, objVehicles,
                                    blnCreateImprovements);

                                _lstChildren.Add(objSubsystem);
                            }
                        }
                    }
                }
            }

            // Check to see if there are any child elements.
            if (objParentNode["gears"] != null)
            {
                using (XmlNodeList objXmlGearList = objParentNode["gears"].SelectNodes("usegear"))
                {
                    if (objXmlGearList?.Count > 0)
                    {
                        XmlDocument objXmlGearDocument = _objCharacter.LoadData("gear.xml");
                        List<Weapon> lstChildWeapons = new List<Weapon>(1);
                        foreach (XmlNode objXmlVehicleGear in objXmlGearList)
                        {
                            Gear objGear = new Gear(_objCharacter);
                            if (!objGear.CreateFromNode(objXmlGearDocument, objXmlVehicleGear, lstChildWeapons,
                                    blnCreateImprovements))
                                continue;
                            foreach (Weapon objWeapon in lstChildWeapons)
                            {
                                objWeapon.ParentID = InternalId;
                            }
                            objGear.Parent = this;
                            objGear.ParentID = InternalId;
                            GearChildren.Add(objGear);
                            lstChildWeapons.AddRange(lstWeapons);
                        }

                        lstWeapons.AddRange(lstChildWeapons);
                    }
                }
            }
        }

        private async Task CreateChildrenAsync(XmlNode objParentNode, Grade objGrade, IList<Weapon> lstWeapons,
            IList<Vehicle> objVehicles, bool blnCreateImprovements = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // If we've just added a new base item, see if there are any subsystems that should automatically be added.
            XmlElement xmlSubsystemsNode = objParentNode["subsystems"];
            if (xmlSubsystemsNode != null)
            {
                // Load Cyberware subsystems first
                using (XmlNodeList objXmlSubSystemNameList = xmlSubsystemsNode.SelectNodes("cyberware"))
                {
                    if (objXmlSubSystemNameList?.Count > 0)
                    {
                        XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("cyberware.xml", token: token).ConfigureAwait(false);
                        foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                        {
                            string strName = objXmlSubsystemNode["name"]?.InnerText;
                            if (string.IsNullOrEmpty(strName))
                                continue;
                            XmlNode objXmlSubsystem =
                                objXmlDocument.TryGetNodeByNameOrId("/chummer/cyberwares/cyberware", strName);

                            if (objXmlSubsystem != null)
                            {
                                Cyberware objSubsystem = new Cyberware(_objCharacter);
                                int.TryParse(objXmlSubsystemNode["rating"]?.InnerText, NumberStyles.Any,
                                    GlobalSettings.InvariantCultureInfo, out int intSubSystemRating);
                                await objSubsystem.CreateAsync(objXmlSubsystem, objGrade, Improvement.ImprovementSource.Cyberware,
                                    intSubSystemRating, lstWeapons, objVehicles, blnCreateImprovements, true,
                                    objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty, this, token: token).ConfigureAwait(false);
                                objSubsystem.ParentID = InternalId;
                                objSubsystem.Cost = "0";
                                // If the <subsystem> tag itself contains extra children, add those, too
                                await objSubsystem.CreateChildrenAsync(objXmlSubsystemNode, objGrade, lstWeapons, objVehicles,
                                    blnCreateImprovements, token).ConfigureAwait(false);

                                await _lstChildren.AddAsync(objSubsystem, token).ConfigureAwait(false);
                            }
                        }
                    }
                }

                // Load bioware subsystems next
                using (XmlNodeList objXmlSubSystemNameList = xmlSubsystemsNode.SelectNodes("bioware"))
                {
                    if (objXmlSubSystemNameList?.Count > 0)
                    {
                        XmlDocument objXmlDocument = await _objCharacter.LoadDataAsync("bioware.xml", token: token).ConfigureAwait(false);
                        foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                        {
                            string strName = objXmlSubsystemNode["name"]?.InnerText;
                            if (string.IsNullOrEmpty(strName))
                                continue;
                            XmlNode objXmlSubsystem =
                                objXmlDocument.TryGetNodeByNameOrId("/chummer/biowares/bioware", strName);

                            if (objXmlSubsystem != null)
                            {
                                Cyberware objSubsystem = new Cyberware(_objCharacter);
                                int.TryParse(objXmlSubsystemNode["rating"]?.InnerText, NumberStyles.Any,
                                    GlobalSettings.InvariantCultureInfo, out int intSubSystemRating);
                                await objSubsystem.CreateAsync(objXmlSubsystem, objGrade, Improvement.ImprovementSource.Bioware,
                                    intSubSystemRating, lstWeapons, objVehicles, blnCreateImprovements, true,
                                    objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty, this, token: token).ConfigureAwait(false);
                                objSubsystem.ParentID = InternalId;
                                objSubsystem.Cost = "0";
                                // If the <subsystem> tag itself contains extra children, add those, too
                                await objSubsystem.CreateChildrenAsync(objXmlSubsystemNode, objGrade, lstWeapons, objVehicles,
                                    blnCreateImprovements, token).ConfigureAwait(false);

                                await _lstChildren.AddAsync(objSubsystem, token).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            // Check to see if there are any child elements.
            if (objParentNode["gears"] != null)
            {
                using (XmlNodeList objXmlGearList = objParentNode["gears"].SelectNodes("usegear"))
                {
                    if (objXmlGearList?.Count > 0)
                    {
                        XmlDocument objXmlGearDocument = await _objCharacter.LoadDataAsync("gear.xml", token: token).ConfigureAwait(false);
                        List<Weapon> lstChildWeapons = new List<Weapon>(1);
                        foreach (XmlNode objXmlVehicleGear in objXmlGearList)
                        {
                            Gear objGear = new Gear(_objCharacter);
                            if (!await objGear.CreateFromNodeAsync(objXmlGearDocument, objXmlVehicleGear, lstChildWeapons,
                                    blnCreateImprovements, token: token).ConfigureAwait(false))
                                continue;
                            foreach (Weapon objWeapon in lstChildWeapons)
                            {
                                objWeapon.ParentID = InternalId;
                            }

                            await objGear.SetParentAsync(this, token).ConfigureAwait(false);
                            objGear.ParentID = InternalId;
                            await GearChildren.AddAsync(objGear, token).ConfigureAwait(false);
                            lstChildWeapons.AddRange(lstWeapons);
                        }

                        lstWeapons.AddRange(lstChildWeapons);
                    }
                }
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            using (LockObject.EnterReadLock())
            {
                objWriter.WriteStartElement("cyberware");
                objWriter.WriteElementString("guid", InternalId);
                objWriter.WriteElementString("sourceid", SourceIDString);
                objWriter.WriteElementString("name", _strName);
                objWriter.WriteElementString("category", _strCategory);
                objWriter.WriteElementString("limbslot", _strLimbSlot);
                objWriter.WriteElementString("limbslotcount", _strLimbSlotCount);
                objWriter.WriteElementString("inheritattributes",
                    _blnInheritAttributes.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("ess", _strESS);
                objWriter.WriteElementString("capacity", _strCapacity);
                objWriter.WriteElementString("avail", _strAvail);
                objWriter.WriteElementString("cost", _strCost);
                objWriter.WriteElementString("weight", _strWeight);
                objWriter.WriteElementString("source", _strSource);
                objWriter.WriteElementString("page", _strPage);
                objWriter.WriteElementString("parentid", _strParentID);
                objWriter.WriteElementString("hasmodularmount", _strHasModularMount);
                objWriter.WriteElementString("plugsintomodularmount", _strPlugsIntoModularMount);
                objWriter.WriteElementString("blocksmounts", _strBlocksMounts);
                objWriter.WriteElementString("forced", _strForced);
                objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("minagility",
                    _intMinAgility.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("minstrength",
                    _intMinStrength.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("minrating", _strMinRating);
                objWriter.WriteElementString("maxrating", _strMaxRating);
                objWriter.WriteElementString("ratinglabel", _strRatingLabel);
                objWriter.WriteElementString("subsystems", _strAllowSubsystems);
                objWriter.WriteElementString("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("grade", _objGrade.Name);
                objWriter.WriteElementString("location", _strLocation);
                objWriter.WriteElementString("extra", _strExtra);
                objWriter.WriteElementString("suite", _blnSuite.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("essdiscount",
                    _intEssenceDiscount.ToString(GlobalSettings.InvariantCultureInfo));
                if (_objCharacter.Created)
                {
                    objWriter.WriteElementString("extraessadditivemultiplier",
                        _decExtraESSAdditiveMultiplier.ToString(
                            GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("extraessmultiplicativemultiplier",
                        _decExtraESSMultiplicativeMultiplier.ToString(
                            GlobalSettings.InvariantCultureInfo));
                }
                else
                {
                    decimal decTemp1 = 0;
                    decimal decTemp2 = 1;
                    switch (_eImprovementSource)
                    {
                        // Apply the character's Cyberware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Cyberware:
                        {
                            List<Improvement> lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.CyberwareEssCostNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decimal decMultiplier = 1;
                                decMultiplier = lstUsedImprovements.Aggregate(decMultiplier,
                                    (current, objImprovement) =>
                                        current - (1m - objImprovement.Value
                                            / 100m));
                                decTemp1 -= 1.0m - decMultiplier;
                            }

                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decTemp2 *=
                                        objImprovement.Value / 100m;
                                }
                            }

                            break;
                        }
                        // Apply the character's Bioware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Bioware:
                        {
                            List<Improvement> lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.BiowareEssCostNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decimal decMultiplier = 1;
                                decMultiplier = lstUsedImprovements.Aggregate(decMultiplier,
                                    (current, objImprovement) =>
                                        current - (1m - objImprovement.Value
                                            / 100m));
                                decTemp1 -= 1.0m - decMultiplier;
                            }

                            lstUsedImprovements
                                = ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    decTemp2 *=
                                        objImprovement.Value / 100m;
                                }
                            }

                            break;
                        }
                    }

                    objWriter.WriteElementString("extraessadditivemultiplier",
                        decTemp1.ToString(GlobalSettings.InvariantCultureInfo));
                    objWriter.WriteElementString("extraessmultiplicativemultiplier",
                        decTemp2.ToString(GlobalSettings.InvariantCultureInfo));
                }

                objWriter.WriteElementString("forcegrade", _strForceGrade);
                objWriter.WriteElementString("matrixcmfilled",
                    _intMatrixCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("matrixcmbonus",
                    _intMatrixCMBonus.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("prototypetranshuman",
                    (_blnPrototypeTranshuman && _objCharacter.IsPrototypeTranshuman).ToString(
                        GlobalSettings.InvariantCultureInfo));
                if (_nodBonus != null)
                    objWriter.WriteRaw(_nodBonus.OuterXml);
                else
                    objWriter.WriteElementString("bonus", string.Empty);
                if (_nodPairBonus != null)
                    objWriter.WriteRaw(_nodPairBonus.OuterXml);
                else
                    objWriter.WriteElementString("pairbonus", string.Empty);
                if (_nodWirelessBonus != null)
                    objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
                else
                    objWriter.WriteElementString("wirelessbonus", string.Empty);
                if (_nodWirelessPairBonus != null)
                    objWriter.WriteRaw(_nodWirelessPairBonus.OuterXml);
                else
                    objWriter.WriteElementString("wirelesspairbonus", string.Empty);
                if (_nodAllowGear != null)
                    objWriter.WriteRaw(_nodAllowGear.OuterXml);
                objWriter.WriteElementString("improvementsource", _eImprovementSource.ToString());
                if (_guiWeaponID != Guid.Empty)
                    objWriter.WriteElementString("weaponguid",
                        _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo));
                if (_guiVehicleID != Guid.Empty)
                    objWriter.WriteElementString("vehicleguid",
                        _guiVehicleID.ToString("D", GlobalSettings.InvariantCultureInfo));

                #region PairInclude

                objWriter.WriteStartElement("pairinclude");
                foreach (string strName in _lstIncludeInPairBonus)
                    objWriter.WriteElementString("name", strName);
                objWriter.WriteEndElement();

                #endregion PairInclude

                #region WirelessPairInclude

                objWriter.WriteStartElement("wirelesspairinclude");
                foreach (string strName in _lstIncludeInWirelessPairBonus)
                    objWriter.WriteElementString("name", strName);
                objWriter.WriteEndElement();

                #endregion WirelessPairInclude

                #region Children

                if (_lstChildren.Count > 0)
                {
                    objWriter.WriteStartElement("children");
                    _lstChildren.ForEach(x => x.Save(objWriter));
                    objWriter.WriteEndElement();
                }

                #endregion Children

                #region Gear

                if (_lstGear.Count > 0)
                {
                    objWriter.WriteStartElement("gears");
                    _lstGear.ForEach(x => x.Save(objWriter));
                    objWriter.WriteEndElement();
                }

                #endregion Gear

                objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
                objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
                objWriter.WriteElementString("discountedcost",
                    _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("addtoparentess",
                    _blnAddToParentESS.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("addtoparentcapacity",
                    _blnAddToParentCapacity.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("isgeneware",
                    _blnIsGeneware.ToString(GlobalSettings.InvariantCultureInfo));

                objWriter.WriteElementString(
                    "active", this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString(
                    "homenode", this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("devicerating", _strDeviceRating);
                objWriter.WriteElementString("programlimit", _strProgramLimit);
                objWriter.WriteElementString("overclocked", _strOverclocked);
                objWriter.WriteElementString("canformpersona", _strCanFormPersona);
                objWriter.WriteElementString("attack", _strAttack);
                objWriter.WriteElementString("sleaze", _strSleaze);
                objWriter.WriteElementString("dataprocessing", _strDataProcessing);
                objWriter.WriteElementString("firewall", _strFirewall);
                objWriter.WriteElementString("attributearray", _strAttributeArray);
                objWriter.WriteElementString("modattack", _strModAttack);
                objWriter.WriteElementString("modsleaze", _strModSleaze);
                objWriter.WriteElementString("moddataprocessing", _strModDataProcessing);
                objWriter.WriteElementString("modfirewall", _strModFirewall);
                objWriter.WriteElementString("modattributearray", _strModAttributeArray);
                objWriter.WriteElementString("canswapattributes",
                    _blnCanSwapAttributes.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
                objWriter.WriteEndElement();
            }
        }

        /// <summary>
        /// Load the cyberware/bioware from the XmlNode synchronously.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether this is a copy of an existing cyberware/bioware being loaded.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Load(XmlNode objNode, bool blnCopy = false, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objNode, blnCopy, token), token);
        }

        /// <summary>
        /// Load the cyberware/bioware from the XmlNode asynchronously.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether this is a copy of an existing cyberware/bioware being loaded.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task LoadAsync(XmlNode objNode, bool blnCopy = false, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objNode, blnCopy, token);
        }

        /// <summary>
        /// Load the cyberware/bioware from the XmlNode.
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag hack for whether this method is being called synchronously or asynchronously</param>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether this is a copy of an existing cyberware/bioware being loaded.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async Task LoadCoreAsync(bool blnSync, XmlNode objNode, bool blnCopy = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterWriteLock(token);
            else
                objLockerAsync = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                _blnDoPropertyChangedInCollectionChanged = false;
                try
                {
                    objNode.TryGetStringFieldQuickly("name", ref _strName);
                    if (objNode["improvementsource"] != null)
                    {
                        _eImprovementSource =
                            Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
                    }

                    _objCachedMyXmlNode = null;
                    _objCachedMyXPathNode = null;
                    Lazy<XmlNode> objMyNode = new Lazy<XmlNode>(() => this.GetNode());
                    if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
                    {
                        objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                    }

                    if (blnCopy)
                        _guiID = Guid.NewGuid();
                    else
                        objNode.TryGetField("guid", Guid.TryParse, out _guiID);

                    objNode.TryGetStringFieldQuickly("category", ref _strCategory);

                    // Legacy shim for mis-formatted name of Reflex Recorder
                    if (_strName == "Reflex Recorder (Skill)"
                        && _objCharacter.LastSavedVersion <= new Version(5, 198, 31))
                    {
                        // This step is needed in case there's a custom data file that has the name "Reflex Recorder (Skill)", in which case we wouldn't want to rename the 'ware
                        XPathNavigator xmlReflexRecorderNode
                            = _eImprovementSource == Improvement.ImprovementSource.Bioware
                                ? (blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? _objCharacter.LoadDataXPath("bioware.xml", token: token)
                                    : await _objCharacter.LoadDataXPathAsync("bioware.xml", token: token).ConfigureAwait(false))
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/biowares/bioware[name = \"Reflex Recorder (Skill)\"]", token)
                                : (blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? _objCharacter.LoadDataXPath("cyberware.xml", token: token)
                                    : await _objCharacter.LoadDataXPathAsync("bioware.xml", token: token).ConfigureAwait(false))
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/cyberwares/cyberware[name = \"Reflex Recorder (Skill)\"]", token);
                        if (xmlReflexRecorderNode == null)
                            _strName = "Reflex Recorder";
                    }

                    objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
                    objNode.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
                    objNode.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
                    objNode.TryGetStringFieldQuickly("limbslotcount", ref _strLimbSlotCount);
                    objNode.TryGetBoolFieldQuickly("inheritattributes", ref _blnInheritAttributes);
                    objNode.TryGetStringFieldQuickly("ess", ref _strESS);
                    objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
                    objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
                    objNode.TryGetStringFieldQuickly("cost", ref _strCost);
                    if (!objNode.TryGetStringFieldQuickly("weight", ref _strWeight))
                        objMyNode.Value?.TryGetStringFieldQuickly("weight", ref _strWeight);
                    objNode.TryGetStringFieldQuickly("source", ref _strSource);
                    objNode.TryGetStringFieldQuickly("page", ref _strPage);
                    objNode.TryGetStringFieldQuickly("parentid", ref _strParentID);
                    if (!objNode.TryGetStringFieldQuickly("hasmodularmount", ref _strHasModularMount))
                        _strHasModularMount = objMyNode.Value?["hasmodularmount"]?.InnerText ?? string.Empty;
                    if (!objNode.TryGetStringFieldQuickly("plugsintomodularmount", ref _strPlugsIntoModularMount))
                        _strPlugsIntoModularMount
                            = objMyNode.Value?["plugsintomodularmount"]?.InnerText ?? string.Empty;
                    if (!objNode.TryGetStringFieldQuickly("blocksmounts", ref _strBlocksMounts))
                        _strBlocksMounts = objMyNode.Value?["blocksmounts"]?.InnerText ?? string.Empty;
                    objNode.TryGetStringFieldQuickly("forced", ref _strForced);
                    objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
                    objNode.TryGetInt32FieldQuickly("minstrength", ref _intMinStrength);
                    objNode.TryGetInt32FieldQuickly("minagility", ref _intMinAgility);
                    objNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
                    objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
                    objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
                    objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
                    // Legacy shim for old-form customized attribute
                    if (s_AttributeAffectingCyberwares.Values.Any(x => x.Contains(Name)) &&
                        int.TryParse(MaxRatingString, out int _))
                    {
                        XmlNode objMyXmlNode = objMyNode.Value;
                        if (objMyXmlNode != null)
                        {
                            objMyXmlNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
                            objMyXmlNode.TryGetStringFieldQuickly("rating", ref _strMaxRating);
                            objMyXmlNode.TryGetStringFieldQuickly("avail", ref _strAvail);
                            objMyXmlNode.TryGetStringFieldQuickly("cost", ref _strCost);
                        }
                    }
                    // Legacy shim for older style of requesting the maximum or minimum of an attribute
                    if (_objCharacter.LastSavedVersion < new Version(5, 225, 443) && (_strMinRating.Contains("imum") || _strMaxRating.Contains("imum")))
                    {
                        _strMinRating = _strMinRating.Replace("MinimumSTR", "{STRMinimum}")
                            .Replace("MaximumSTR", "{STRMaximum}")
                            .Replace("MinimumAGI", "{AGIMinimum}")
                            .Replace("MaximumAGI", "{AGIMaximum}");
                        _strMaxRating = _strMaxRating.Replace("MinimumSTR", "{STRMinimum}")
                            .Replace("MaximumSTR", "{STRMaximum}")
                            .Replace("MinimumAGI", "{AGIMinimum}")
                            .Replace("MaximumAGI", "{AGIMaximum}");
                    }

                    objNode.TryGetStringFieldQuickly("subsystems", ref _strAllowSubsystems);
                    if (objNode["grade"] != null)
                        _objGrade = blnSync
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            ? Grade.ConvertToCyberwareGrade(objNode["grade"].InnerText, _eImprovementSource,
                                _objCharacter)
                            : await Grade.ConvertToCyberwareGradeAsync(objNode["grade"].InnerText, _eImprovementSource,
                                _objCharacter, token).ConfigureAwait(false);
                    objNode.TryGetStringFieldQuickly("location", ref _strLocation);
                    if (!objNode.TryGetStringFieldQuickly("extra", ref _strExtra) && _strLocation != "Left" &&
                        _strLocation != "Right")
                    {
                        _strExtra = _strLocation;
                        _strLocation = string.Empty;
                    }

                    objNode.TryGetBoolFieldQuickly("suite", ref _blnSuite);
                    objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
                    objNode.TryGetInt32FieldQuickly("essdiscount", ref _intEssenceDiscount);
                    if (blnSync ? _objCharacter.Created : await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                    {
                        objNode.TryGetDecFieldQuickly("extraessadditivemultiplier", ref _decExtraESSAdditiveMultiplier);
                        objNode.TryGetDecFieldQuickly("extraessmultiplicativemultiplier",
                            ref _decExtraESSMultiplicativeMultiplier);
                    }

                    objNode.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);
                    if ((blnSync
                            ? _objCharacter.IsPrototypeTranshuman
                            : await _objCharacter.GetIsPrototypeTranshumanAsync(token).ConfigureAwait(false)) &&
                        SourceType == Improvement.ImprovementSource.Bioware)
                        objNode.TryGetBoolFieldQuickly("prototypetranshuman", ref _blnPrototypeTranshuman);

                    _nodBonus = objNode["bonus"] ?? objMyNode.Value?["bonus"];
                    _nodPairBonus = objNode["pairbonus"] ?? objMyNode.Value?["pairbonus"];
                    XmlElement xmlPairIncludeNode = objNode["pairinclude"];
                    if (xmlPairIncludeNode == null)
                    {
                        xmlPairIncludeNode = objMyNode.Value?["pairinclude"];
                        _lstIncludeInPairBonus.Add(Name);
                    }

                    if (xmlPairIncludeNode != null)
                    {
                        using (XmlNodeList xmlNameList = xmlPairIncludeNode.SelectNodes("name"))
                        {
                            if (xmlNameList?.Count > 0)
                            {
                                foreach (XmlNode xmlNameNode in xmlNameList)
                                    _lstIncludeInPairBonus.Add(xmlNameNode.InnerText);
                            }
                        }
                    }

                    _nodWirelessPairBonus = objNode["wirelesspairbonus"] ?? objMyNode.Value?["wirelesspairbonus"];
                    xmlPairIncludeNode = objNode["wirelesspairinclude"];
                    if (xmlPairIncludeNode == null)
                    {
                        xmlPairIncludeNode = objMyNode.Value?["wirelesspairinclude"];
                        _lstIncludeInWirelessPairBonus.Add(Name);
                    }

                    if (xmlPairIncludeNode != null)
                    {
                        using (XmlNodeList xmlNameList = xmlPairIncludeNode.SelectNodes("name"))
                        {
                            if (xmlNameList?.Count > 0)
                            {
                                foreach (XmlNode xmlNameNode in xmlNameList)
                                    _lstIncludeInWirelessPairBonus.Add(xmlNameNode.InnerText);
                            }
                        }
                    }

                    _nodWirelessBonus = objNode["wirelessbonus"] ?? objMyNode.Value?["wirelessbonus"];
                    if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                    {
                        _blnWirelessOn = false;
                    }

                    _nodAllowGear = objNode["allowgear"];
                    // Legacy Sweep
                    if (_strForceGrade != "None" && IsGeneware)
                    {
                        _strForceGrade = objMyNode.Value?["forcegrade"]?.InnerText;
                        if (!string.IsNullOrEmpty(_strForceGrade))
                            _objGrade = blnSync
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                ? Grade.ConvertToCyberwareGrade(_strForceGrade, _eImprovementSource,
                                    _objCharacter)
                                : await Grade.ConvertToCyberwareGradeAsync(_strForceGrade, _eImprovementSource,
                                    _objCharacter, token).ConfigureAwait(false);
                    }

                    if (objNode["weaponguid"] != null
                        && !Guid.TryParse(objNode["weaponguid"].InnerText, out _guiWeaponID))
                    {
                        _guiWeaponID = Guid.Empty;
                    }

                    if (objNode["vehicleguid"] != null &&
                        !Guid.TryParse(objNode["vehicleguid"].InnerText, out _guiVehicleID))
                    {
                        _guiVehicleID = Guid.Empty;
                    }

                    if (objNode.InnerXml.Contains("<children>") || objNode.InnerXml.Contains("<cyberware>"))
                    {
                        XmlNodeList nodChildren = objNode.SelectNodes("children/cyberware");
                        foreach (XmlNode nodChild in nodChildren)
                        {
                            Cyberware objChild = new Cyberware(_objCharacter);
                            if (blnSync)
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                objChild.Load(nodChild, blnCopy, token);
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                _lstChildren.Add(objChild);
                            }
                            else
                            {
                                await objChild.LoadAsync(nodChild, blnCopy, token).ConfigureAwait(false);
                                await _lstChildren.AddAsync(objChild, token).ConfigureAwait(false);
                            }
                        }
                    }

                    if (objNode.InnerXml.Contains("<gears>"))
                    {
                        XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
                        foreach (XmlNode nodChild in nodChildren)
                        {
                            Gear objGear = new Gear(_objCharacter);
                            objGear.Load(nodChild, blnCopy);
                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                _lstGear.Add(objGear);
                            else
                                await _lstGear.AddAsync(objGear, token).ConfigureAwait(false);
                        }
                    }

                    objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

                    string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                    objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                    _colNotes = ColorTranslator.FromHtml(sNotesColor);

                    objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
                    if (objNode["addtoparentess"] != null)
                    {
                        if (bool.TryParse(objNode["addtoparentess"].InnerText, out bool blnTmp))
                        {
                            _blnAddToParentESS = blnTmp;
                        }
                    }
                    else
                        _blnAddToParentESS = objMyNode.Value?["addtoparentess"] != null;

                    if (objNode["addtoparentcapacity"] != null)
                    {
                        if (bool.TryParse(objNode["addtoparentcapacity"].InnerText, out bool blnTmp))
                        {
                            _blnAddToParentCapacity = blnTmp;
                        }
                    }
                    else
                        _blnAddToParentCapacity = objMyNode.Value?["addtoparentcapacity"] != null;

                    if (objNode["geneware"] != null)
                    {
                        if (bool.TryParse(objNode["geneware"].InnerText, out bool blnTmp))
                        {
                            _blnIsGeneware = blnTmp;
                        }
                    }
                    else
                    {
                        _blnIsGeneware = objMyNode.Value?["geneware"] != null;
                        _strCategory = objMyNode.Value?["category"]?.InnerText ?? _strCategory;
                    }

                    bool blnIsActive = false;
                    if (objNode.TryGetBoolFieldQuickly("active", ref blnIsActive) && blnIsActive)
                    {
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            this.SetActiveCommlink(_objCharacter, true);
                        else
                            await this.SetActiveCommlinkAsync(_objCharacter, true, token).ConfigureAwait(false);
                    }

                    if (blnCopy)
                    {
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            this.SetHomeNode(_objCharacter, false);
                        else
                            await this.SetHomeNodeAsync(_objCharacter, false, token).ConfigureAwait(false);
                    }
                    else
                    {
                        bool blnIsHomeNode = false;
                        if (objNode.TryGetBoolFieldQuickly("homenode", ref blnIsHomeNode) && blnIsHomeNode)
                        {
                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                this.SetHomeNode(_objCharacter, true);
                            else
                                await this.SetHomeNodeAsync(_objCharacter, true, token).ConfigureAwait(false);
                        }
                    }

                    if (!objNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating))
                        objMyNode.Value?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
                    if (!objNode.TryGetStringFieldQuickly("programlimit", ref _strProgramLimit))
                        objMyNode.Value?.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
                    objNode.TryGetStringFieldQuickly("overclocked", ref _strOverclocked);
                    if (!objNode.TryGetStringFieldQuickly("attack", ref _strAttack))
                        objMyNode.Value?.TryGetStringFieldQuickly("attack", ref _strAttack);
                    if (!objNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze))
                        objMyNode.Value?.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                    if (!objNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing))
                        objMyNode.Value?.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                    if (!objNode.TryGetStringFieldQuickly("firewall", ref _strFirewall))
                        objMyNode.Value?.TryGetStringFieldQuickly("firewall", ref _strFirewall);
                    if (!objNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
                        objMyNode.Value?.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray);
                    if (!objNode.TryGetStringFieldQuickly("modattack", ref _strModAttack))
                        objMyNode.Value?.TryGetStringFieldQuickly("modattack", ref _strModAttack);
                    if (!objNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze))
                        objMyNode.Value?.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
                    if (!objNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing))
                        objMyNode.Value?.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
                    if (!objNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall))
                        objMyNode.Value?.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
                    if (!objNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray))
                        objMyNode.Value?.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);
                    objNode.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);

                    if (blnCopy)
                    {
                        if (Bonus != null || WirelessBonus != null || PairBonus != null || WirelessPairBonus != null)
                        {
                            if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                ImprovementManager.SetForcedValue(_strForced, _objCharacter);

                            if (Bonus != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ImprovementManager.CreateImprovements(_objCharacter, _eImprovementSource,
                                        _guiID.ToString(
                                            "D", GlobalSettings.InvariantCultureInfo),
                                        Bonus, Rating,
                                        CurrentDisplayNameShort, token: token);
                                else
                                    await ImprovementManager.CreateImprovementsAsync(_objCharacter, _eImprovementSource,
                                        _guiID.ToString(
                                            "D", GlobalSettings.InvariantCultureInfo),
                                        Bonus, await GetRatingAsync(token).ConfigureAwait(false),
                                        await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                            }

                            string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                            if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                                _strExtra = strSelectedValue;

                            if (PairBonus != null)
                            {
                                // This cyberware should not be included in the count to make things easier.
                                List<Cyberware> lstPairableCyberwares;
                                if (blnSync)
                                    lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(
                                        x => x.Children,
                                        x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                             x.IsModularCurrentlyEquipped, token).ToList();
                                else
                                {
                                    lstPairableCyberwares = await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepWhereAsync(
                                        x => x.Children,
                                        async x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                                   await x.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                                }
                                int intCount = lstPairableCyberwares.Count;
                                // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                                if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                                {
                                    intCount = 0;
                                    foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                                    {
                                        if (objPairableCyberware.Location != Location)
                                            // We have found a cyberware with which this one could be paired, so increase count by 1
                                            ++intCount;
                                        else
                                            // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                            --intCount;
                                    }

                                    // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                                    intCount = (intCount > 0).ToInt32();
                                }

                                if ((intCount & 1) == 1)
                                {
                                    if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left"
                                                                          && _strForced != "Right")
                                        ImprovementManager.SetForcedValue(_strForced, _objCharacter);
                                    else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                        ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverload
                                        ImprovementManager.CreateImprovements(
                                            _objCharacter, SourceType, InternalId + "Pair",
                                            PairBonus, Rating, CurrentDisplayNameShort, token: token);
                                    else
                                        await ImprovementManager.CreateImprovementsAsync(
                                            _objCharacter, await GetSourceTypeAsync(token).ConfigureAwait(false), InternalId + "Pair",
                                            PairBonus, await GetRatingAsync(token).ConfigureAwait(false), await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                                }
                            }

                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                RefreshWirelessBonuses();
                            else
                                await RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
                        }

                        if (blnSync)
                        {
                            if (!IsModularCurrentlyEquipped)
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                ChangeModularEquip(false);
                        }
                        else if (!await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false))
                            await ChangeModularEquipAsync(false, token: token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    _blnDoPropertyChangedInCollectionChanged = true;
                    if (blnSync)
                    {
                        if (Children.Count > 0)
                            Utils.SafelyRunSynchronously(() => CyberwareChildrenOnCollectionChanged(
                                this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children), token), token);
                    }
                    else if (await Children.GetCountAsync(token).ConfigureAwait(false) > 0)
                        await CyberwareChildrenOnCollectionChanged(
                            this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children), token).ConfigureAwait(false);
                }
            }
            finally
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    objLocker.Dispose();
                else
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>obv
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint,
            CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // <cyberware>
                XmlElementWriteHelper objBaseElement
                    = await objWriter.StartElementAsync("cyberware", token: token).ConfigureAwait(false);
                try
                {
                    if (!await GetIsLimbAsync(token).ConfigureAwait(false) || CyberlimbAttributeAbbrevs.Count == 0)
                    {
                        await objWriter
                            .WriteElementStringAsync(
                                "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false),
                                token: token).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("name_english", Name, token: token)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        string strSpace = await LanguageManager
                            .GetStringAsync("String_Space", strLanguageToPrint, token: token)
                            .ConfigureAwait(false);
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                   out StringBuilder sbdName))
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                   out StringBuilder sbdNameEnglish))
                        {
                            sbdName.Append(await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false))
                                .Append(strSpace).Append('(');
                            sbdNameEnglish.Append(Name).Append(strSpace).Append('(');
                            bool blnFirst = true;
                            foreach (string strAbbrev in CyberlimbAttributeAbbrevs)
                            {
                                int intTotalValue = await GetAttributeTotalValueAsync(strAbbrev, token)
                                    .ConfigureAwait(false);
                                if (!blnFirst)
                                {
                                    sbdName.Append(',').Append(strSpace);
                                    sbdNameEnglish.Append(',').Append(strSpace);
                                }
                                else
                                    blnFirst = false;

                                CharacterAttrib objLoopAttribute = await _objCharacter
                                    .GetAttributeAsync(strAbbrev, token: token)
                                    .ConfigureAwait(false);
                                if (objLoopAttribute != null)
                                    sbdName.Append(await objLoopAttribute
                                        .GetDisplayAbbrevAsync(strLanguageToPrint, token)
                                        .ConfigureAwait(false));
                                else
                                    sbdName.Append(strAbbrev);
                                sbdName.Append(strSpace).Append(intTotalValue.ToString(objCulture));
                                sbdNameEnglish.Append(strAbbrev).Append(strSpace)
                                    .Append(intTotalValue.ToString(GlobalSettings.InvariantCultureInfo));
                            }

                            sbdName.Append(')');
                            sbdNameEnglish.Append(')');
                            await objWriter.WriteElementStringAsync("name", sbdName.ToString(), token: token)
                                .ConfigureAwait(false);
                            await objWriter
                                .WriteElementStringAsync("name_english", sbdNameEnglish.ToString(), token: token)
                                .ConfigureAwait(false);
                        }
                    }

                    await objWriter
                        .WriteElementStringAsync(
                            "category", await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("category_english", Category, token: token)
                        .ConfigureAwait(false);

                    await objWriter.WriteElementStringAsync("ess",
                        (await GetCalculatedESSAsync(token).ConfigureAwait(false))
                        .ToString(_objCharacter.Settings.EssenceFormat, objCulture),
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("capacity", Capacity, token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("avail",
                            await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false),
                            token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "cost", TotalCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "owncost",
                            (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(
                                _objCharacter.Settings.NuyenFormat, objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "weight", TotalWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("ownweight",
                            OwnWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "source",
                            await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                                .ConfigureAwait(false), token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "rating", (await GetRatingAsync(token).ConfigureAwait(false)).ToString(objCulture),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("minrating",
                            (await GetMinRatingAsync(token).ConfigureAwait(false)).ToString(
                                objCulture), token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("maxrating",
                            (await GetMaxRatingAsync(token).ConfigureAwait(false)).ToString(
                                objCulture), token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("ratinglabel", RatingLabel, token: token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("allowsubsystems", AllowedSubsystems, token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("wirelesson",
                            WirelessOn.ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "grade", await (await GetGradeAsync(token).ConfigureAwait(false)).DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false),
                            token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("location", Location, token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "extra",
                            await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token)
                                .ConfigureAwait(false), token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("improvementsource", SourceType.ToString(), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("isgeneware",
                            IsGeneware.ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);

                    await objWriter
                        .WriteElementStringAsync(
                            "attack", (await this.GetTotalMatrixAttributeAsync("Attack", token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "sleaze", (await this.GetTotalMatrixAttributeAsync("Sleaze", token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("dataprocessing",
                            (await this.GetTotalMatrixAttributeAsync("Data Processing", token).ConfigureAwait(false)).ToString(objCulture),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "firewall", (await this.GetTotalMatrixAttributeAsync("Firewall", token).ConfigureAwait(false)).ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("devicerating",
                            (await this.GetTotalMatrixAttributeAsync("Device Rating", token).ConfigureAwait(false)).ToString(objCulture),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("programlimit",
                            (await this.GetTotalMatrixAttributeAsync("Program Limit", token).ConfigureAwait(false)).ToString(objCulture),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("iscommlink",
                            (await GetIsCommlinkAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "active",
                            (await this.IsActiveCommlinkAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync(
                            "homenode", (await this.IsHomeNodeAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("conditionmonitor", MatrixCM.ToString(objCulture), token: token)
                        .ConfigureAwait(false);
                    await objWriter
                        .WriteElementStringAsync("matrixcmfilled", MatrixCMFilled.ToString(objCulture), token: token)
                        .ConfigureAwait(false);

                    if (GearChildren.Count > 0)
                    {
                        // <gears>
                        XmlElementWriteHelper objGearsElement
                            = await objWriter.StartElementAsync("gears", token: token).ConfigureAwait(false);
                        try
                        {
                            foreach (Gear objGear in GearChildren)
                            {
                                await objGear.Print(objWriter, objCulture, strLanguageToPrint, token)
                                    .ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            // </gears>
                            await objGearsElement.DisposeAsync().ConfigureAwait(false);
                        }
                    }

                    // <children>
                    XmlElementWriteHelper objChildrenElement
                        = await objWriter.StartElementAsync("children", token: token).ConfigureAwait(false);
                    try
                    {
                        foreach (Cyberware objChild in Children)
                        {
                            await objChild.Print(objWriter, objCulture, strLanguageToPrint, token)
                                .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        // </children>
                        await objChildrenElement.DisposeAsync().ConfigureAwait(false);
                    }

                    if (GlobalSettings.PrintNotes)
                        await objWriter.WriteElementStringAsync("notes", Notes, token: token).ConfigureAwait(false);
                }
                finally
                {
                    // </cyberware>
                    await objBaseElement.DisposeAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Cyberware in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Guid of a Cyberware Weapon.
        /// </summary>
        public string WeaponID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                {
                    using (LockObject.EnterUpgradeableReadLock())
                        _guiWeaponID = guiTemp;
                }
            }
        }

        /// <summary>
        /// Guid of a Cyberware Weapon Accessory.
        /// </summary>
        public string WeaponAccessoryID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiWeaponAccessoryID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                {
                    using (LockObject.EnterUpgradeableReadLock())
                        _guiWeaponAccessoryID = guiTemp;
                }
            }
        }

        /// <summary>
        /// Guid of a Cyberware Drone/Vehicle.
        /// </summary>
        public string VehicleID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiVehicleID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                {
                    using (LockObject.EnterUpgradeableReadLock())
                        _guiVehicleID = guiTemp;
                }
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _nodBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _nodBonus = value;
            }
        }

        /// <summary>
        /// Bonus node from the XML file that only activates for each pair of 'ware.
        /// </summary>
        public XmlNode PairBonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _nodPairBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _nodPairBonus = value;
            }
        }

        /// <summary>
        /// Bonus node from the XML file that only activates for each pair of 'ware.
        /// </summary>
        public XmlNode WirelessPairBonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _nodWirelessPairBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _nodWirelessPairBonus = value;
            }
        }

        /// <summary>
        /// Wireless bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _nodWirelessBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _nodWirelessBonus = value;
            }
        }

        /// <summary>
        /// Whether the Cyberware's Wireless is enabled
        /// </summary>
        public bool WirelessOn
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnWirelessOn;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnWirelessOn == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnWirelessOn = value;
                        RefreshWirelessBonuses();
                    }
                }
            }
        }

        /// <summary>
        /// AllowGear node from the XML file.
        /// </summary>
        public XmlNode AllowGear
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _nodAllowGear;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _nodAllowGear = value;
            }
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public Improvement.ImprovementSource SourceType
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _eImprovementSource;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (InterlockedExtensions.Exchange(ref _eImprovementSource, value) == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _objCachedMyXmlNode = null;
                        _objCachedMyXPathNode = null;
                    }
                }
            }
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public async Task<Improvement.ImprovementSource> GetSourceTypeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _eImprovementSource;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cyberware name.
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
                    string strOldValue = Interlocked.Exchange(ref _strName, value);
                    if (strOldValue == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _lstIncludeInPairBonus.Remove(strOldValue);
                        _lstIncludeInPairBonus.Add(value);
                        _lstIncludeInWirelessPairBonus.Remove(strOldValue);
                        _lstIncludeInWirelessPairBonus.Add(value);
                    }

                    if (_objParent?.IsLimb != true
                        || _objParent.Parent?.InheritAttributes == false || _objParent.ParentVehicle != null
                        || _objCharacter.Settings.DontUseCyberlimbCalculation
                        || _objCharacter.Settings.ExcludeLimbSlot.Contains(_objParent.LimbSlot))
                        return;
                    // Note: Movement is always handled whenever AGI or STR is changed, regardless of whether we use cyberleg movement
                    foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in
                             s_AttributeAffectingCyberwares)
                    {
                        if (kvpToCheck.Value.Contains(value) || kvpToCheck.Value.Contains(strOldValue))
                        {
                            foreach (CharacterAttrib objCharacterAttrib in
                                     _objCharacter.GetAllAttributes(kvpToCheck.Key))
                            {
                                objCharacterAttrib?.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }
                    }
                }
            }
        }

        public bool InheritAttributes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnInheritAttributes;
            }
        }

        public async Task<bool> GetInheritAttributesAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnInheritAttributes;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceID;
            }
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public async Task<Guid> GetSourceIDAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _guiSourceID;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public async Task<string> GetSourceIDStringAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            using (LockObject.EnterReadLock())
                return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                return objNode != null
                    ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name
                    : Name;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        public const string EssenceHoleGuidString = "b57eadaa-7c3b-4b80-8d79-cbbd922c1196";
        public static Guid EssenceHoleGUID { get; } = new Guid(EssenceHoleGuidString);

        public const string EssenceAntiHoleGuidString = "961eac53-0c43-4b19-8741-2872177a3a4c";
        public static Guid EssenceAntiHoleGUID { get; } = new Guid(EssenceAntiHoleGuidString);

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            using (LockObject.EnterReadLock())
            {
                string strReturn = DisplayNameShort(strLanguage);
                string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                int intRating = Rating;
                if (intRating > 0 && SourceID != EssenceHoleGUID && SourceID != EssenceAntiHoleGUID)
                {
                    strReturn += strSpace + '(' + LanguageManager.GetString(RatingLabel, strLanguage) + strSpace
                                 + intRating.ToString(objCulture) + ')';
                }

                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += strSpace + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
                }

                if (!string.IsNullOrEmpty(Location))
                {
                    string strSide = string.Empty;
                    switch (Location)
                    {
                        case "Left":
                            strSide = LanguageManager.GetString("String_Improvement_SideLeft", strLanguage);
                            break;

                        case "Right":
                            strSide = LanguageManager.GetString("String_Improvement_SideRight", strLanguage);
                            break;
                    }

                    if (!string.IsNullOrEmpty(strSide))
                        strReturn += strSpace + '(' + strSide + ')';
                }

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
                string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                    .ConfigureAwait(false);
                int intRating = await GetRatingAsync(token).ConfigureAwait(false);
                if (intRating > 0)
                {
                    Guid guidSourceId = await GetSourceIDAsync(token).ConfigureAwait(false);
                    if (guidSourceId != EssenceHoleGUID && guidSourceId != EssenceAntiHoleGUID)
                    {
                        strReturn += strSpace + '('
                                              + await LanguageManager.GetStringAsync(RatingLabel, strLanguage, token: token)
                                                  .ConfigureAwait(false) + strSpace
                                              + intRating.ToString(objCulture) + ')';
                    }
                }

                if (!string.IsNullOrEmpty(Extra))
                {
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += strSpace + '(' + await _objCharacter
                        .TranslateExtraAsync(Extra, strLanguage, token: token)
                        .ConfigureAwait(false) + ')';
                }

                if (!string.IsNullOrEmpty(Location))
                {
                    string strSide = string.Empty;
                    switch (Location)
                    {
                        case "Left":
                            strSide = await LanguageManager
                                .GetStringAsync("String_Improvement_SideLeft", strLanguage, token: token)
                                .ConfigureAwait(false);
                            break;

                        case "Right":
                            strSide = await LanguageManager
                                .GetStringAsync("String_Improvement_SideRight", strLanguage, token: token)
                                .ConfigureAwait(false);
                            break;
                    }

                    if (!string.IsNullOrEmpty(strSide))
                        strReturn += strSpace + '(' + strSide + ')';
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            using (LockObject.EnterReadLock())
            {
                return _objCharacter
                    .LoadDataXPath(
                        SourceType == Improvement.ImprovementSource.Cyberware ? "cyberware.xml" : "bioware.xml",
                        strLanguage)
                    .SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " + Category.CleanXPath() +
                                                        "]/@translate")
                    ?.Value ?? Category;
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public async Task<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (await _objCharacter
                        .LoadDataXPathAsync(
                            SourceType == Improvement.ImprovementSource.Cyberware
                                ? "cyberware.xml"
                                : "bioware.xml", strLanguage, token: token).ConfigureAwait(false))
                    .SelectSingleNodeAndCacheExpression(
                        "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate", token)
                    ?.Value ?? Category;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cyberware category.
        /// </summary>
        public string Category
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCategory;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    string strOldValue = Interlocked.Exchange(ref _strCategory, value);
                    if (strOldValue != value && IsLimb && Parent?.InheritAttributes != false && ParentVehicle == null
                        && !_objCharacter.Settings.DontUseCyberlimbCalculation
                        && !_objCharacter.Settings.ExcludeLimbSlot.Contains(LimbSlot))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                        {
                            if (CyberlimbAttributeAbbrevs.Contains(objCharacterAttrib.Abbrev))
                            {
                                objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }

                        if (_objCharacter.Settings.CyberlegMovement && LimbSlot == "leg")
                            _objCharacter.OnPropertyChanged(nameof(Character.GetMovement));
                    }
                }
            }
        }

        /// <summary>
        /// The type of body "slot" a Cyberlimb occupies.
        /// </summary>
        public string LimbSlot
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strLimbSlot;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    string strOldValue = Interlocked.Exchange(ref _strLimbSlot, value);
                    if (
                        strOldValue != value &&
                        (
                            Parent?.InheritAttributes != false &&
                            ParentVehicle == null &&
                            !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                            !string.IsNullOrWhiteSpace(value) &&
                            !_objCharacter.Settings.ExcludeLimbSlot.Contains(value) ||
                            (
                                !string.IsNullOrWhiteSpace(strOldValue) &&
                                !_objCharacter.Settings.ExcludeLimbSlot.Contains(strOldValue)
                            )
                        )
                    )
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                        {
                            if (CyberlimbAttributeAbbrevs.Contains(objCharacterAttrib.Abbrev))
                            {
                                objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }

                        if (_objCharacter.Settings.CyberlegMovement && (value == "leg" || strOldValue == "leg"))
                            _objCharacter.OnPropertyChanged(nameof(Character.GetMovement));
                    }
                }
            }
        }

        /// <summary>
        /// The type of body "slot" a Cyberlimb occupies.
        /// </summary>
        public async Task<string> GetLimbSlotAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strLimbSlot;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The amount of body "slots" a Cyberlimb occupies.
        /// </summary>
        public int LimbSlotCount
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (string.Equals(_strLimbSlotCount, "all", StringComparison.OrdinalIgnoreCase))
                    {
                        return _objCharacter.LimbCount(LimbSlot);
                    }

                    int.TryParse(_strLimbSlotCount, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                        out int intReturn);
                    return intReturn;
                }
            }
            set
            {
                string strNewValue = value.ToString(GlobalSettings.InvariantCultureInfo);
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _strLimbSlotCount, strNewValue) != strNewValue && IsLimb
                        && Parent?.InheritAttributes != false && ParentVehicle == null
                        && !_objCharacter.Settings.DontUseCyberlimbCalculation
                        && !_objCharacter.Settings.ExcludeLimbSlot.Contains(LimbSlot))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                        {
                            if (CyberlimbAttributeAbbrevs.Contains(objCharacterAttrib.Abbrev))
                            {
                                objCharacterAttrib.OnPropertyChanged(nameof(CharacterAttrib.TotalValue));
                            }
                        }

                        if (_objCharacter.Settings.CyberlegMovement && LimbSlot == "leg")
                            _objCharacter.OnPropertyChanged(nameof(Character.GetMovement));
                    }
                }
            }
        }

        /// <summary>
        /// The amount of body "slots" a Cyberlimb occupies.
        /// </summary>
        public async Task<int> GetLimbSlotCountAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (string.Equals(_strLimbSlotCount, "all", StringComparison.OrdinalIgnoreCase))
                {
                    return await _objCharacter.LimbCountAsync(await GetLimbSlotAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                }

                int.TryParse(_strLimbSlotCount, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                    out int intReturn);
                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// How many limbs does this cyberware have?
        /// </summary>
        public int GetCyberlimbCount(ICollection<string> lstExcludeLimbs = null)
        {
            using (LockObject.EnterReadLock())
            {
                int intCount = 0;
                if (!string.IsNullOrEmpty(LimbSlot) && lstExcludeLimbs?.All(l => l != LimbSlot) != false)
                {
                    intCount += LimbSlotCount;
                }
                else
                {
                    intCount += Children.Sum(x => x.GetCyberlimbCount(lstExcludeLimbs));
                }

                return intCount;
            }
        }

        /// <summary>
        /// How many limbs does this cyberware have?
        /// </summary>
        public async Task<int> GetCyberlimbCountAsync(ICollection<string> lstExcludeLimbs = null, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intCount = 0;
                if (!string.IsNullOrEmpty(LimbSlot) && lstExcludeLimbs?.All(l => l != LimbSlot) != false)
                {
                    intCount += await GetLimbSlotCountAsync(token).ConfigureAwait(false);
                }
                else
                {
                    intCount += await Children.SumAsync(x => x.GetCyberlimbCountAsync(lstExcludeLimbs, token), token).ConfigureAwait(false);
                }

                return intCount;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The location of a Cyberlimb (Left or Right).
        /// </summary>
        public string Location
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strLocation;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_strLocation == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strLocation = value;
                }
            }
        }

        /// <summary>
        /// The location of a Cyberlimb (Left or Right).
        /// </summary>
        public async Task<string> GetLocationAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strLocation;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The location of a Cyberlimb (Left or Right).
        /// </summary>
        public async Task SetLocationAsync(string value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_strLocation == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _strLocation = value;
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
        /// Original Forced Extra string associated with the 'ware.
        /// </summary>
        public string Forced
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strForced;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strForced = value;
            }
        }

        /// <summary>
        /// Extra string associated with the 'ware.
        /// </summary>
        public string Extra
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strExtra;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strExtra = value;
            }
        }

        /// <summary>
        /// Essence cost of the Cyberware.
        /// </summary>
        public string ESS
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strESS;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strESS = value;
            }
        }

        /// <summary>
        /// Cyberware capacity.
        /// </summary>
        public string Capacity
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCapacity;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strCapacity = value;
            }
        }

        /// <summary>
        /// Availability.
        /// </summary>
        public string Avail
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strAvail;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strAvail = value;
            }
        }

        /// <summary>
        /// Cost.
        /// </summary>
        public string Cost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCost;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strCost = value;
            }
        }

        /// <summary>
        /// Weight.
        /// </summary>
        public string Weight
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strWeight;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strWeight = value;
            }
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strSource;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strSource = value;
            }
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPage;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strPage = value;
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
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            using (LockObject.EnterReadLock())
            {
                string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
                return !string.IsNullOrWhiteSpace(s) ? s : Page;
            }
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async Task<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                string strReturn = objNode?.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? Page;
                return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return _objCachedSourceDetail == default
                        ? _objCachedSourceDetail = SourceString.GetSourceString(Source,
                            DisplayPage(GlobalSettings.Language),
                            GlobalSettings.Language,
                            GlobalSettings.CultureInfo,
                            _objCharacter)
                        : _objCachedSourceDetail;
                }
            }
        }

        public async Task<SourceString> GetSourceDetailAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objCachedSourceDetail == default
                    ? _objCachedSourceDetail = await SourceString.GetSourceStringAsync(Source,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false),
                        GlobalSettings.Language,
                        GlobalSettings.CultureInfo,
                        _objCharacter, token).ConfigureAwait(false)
                    : _objCachedSourceDetail;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ID of the object that added this cyberware (if any).
        /// </summary>
        public string ParentID
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strParentID;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strParentID = value;
            }
        }

        /// <summary>
        /// The modular mount this cyberware contains. Returns string.Empty if it contains no mount.
        /// </summary>
        public string HasModularMount
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strHasModularMount;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strHasModularMount = value;
            }
        }

        /// <summary>
        /// The modular mount this cyberware contains. Returns string.Empty if it contains no mount.
        /// </summary>
        public async Task<string> GetHasModularMountAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strHasModularMount;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// What modular mount this cyberware plugs into. Returns string.Empty if it doesn't plug into a modular mount.
        /// </summary>
        public string PlugsIntoModularMount
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strPlugsIntoModularMount;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strPlugsIntoModularMount = value;
            }
        }

        /// <summary>
        /// What modular mount this cyberware plugs into. Returns string.Empty if it doesn't plug into a modular mount.
        /// </summary>
        public async Task<string> GetPlugsIntoModularMountAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strPlugsIntoModularMount;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns whether the 'ware is currently equipped (with improvements applied) or not.
        /// </summary>
        public bool IsModularCurrentlyEquipped
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // Cyberware always equipped if it's not a modular one
                    bool blnReturn = string.IsNullOrEmpty(PlugsIntoModularMount);
                    Cyberware objCurrentParent = Parent;
                    // If top-level parent is one that has a modular mount but also does not plug into another modular mount itself, then return true, otherwise return false
                    while (objCurrentParent != null)
                    {
                        using (objCurrentParent.LockObject.EnterReadLock())
                        {
                            if (!string.IsNullOrEmpty(objCurrentParent.HasModularMount))
                                blnReturn = true;
                            if (!string.IsNullOrEmpty(objCurrentParent.PlugsIntoModularMount))
                                blnReturn = false;
                            objCurrentParent = objCurrentParent.Parent;
                        }
                    }

                    return blnReturn;
                }
            }
        }

        /// <summary>
        /// Returns whether the 'ware is currently equipped (with improvements applied) or not.
        /// </summary>
        public async Task<bool> GetIsModularCurrentlyEquippedAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Cyberware always equipped if it's not a modular one
                bool blnReturn = string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false));
                Cyberware objCurrentParent = await GetParentAsync(token).ConfigureAwait(false);
                // If top-level parent is one that has a modular mount but also does not plug into another modular mount itself, then return true, otherwise return false
                while (objCurrentParent != null)
                {
                    IAsyncDisposable objLocker2 = await objCurrentParent.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (!string.IsNullOrEmpty(await objCurrentParent.GetHasModularMountAsync(token).ConfigureAwait(false)))
                            blnReturn = true;
                        if (!string.IsNullOrEmpty(await objCurrentParent.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)))
                            blnReturn = false;
                        objCurrentParent = await objCurrentParent.GetParentAsync(token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return blnReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool Stolen
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnStolen;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnStolen = value;
            }
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this weapon accessory.
        /// </summary>
        public void RefreshWirelessBonuses()
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (!string.IsNullOrEmpty(WirelessBonus?.InnerText)
                    || !string.IsNullOrEmpty(WirelessPairBonus?.InnerText))
                {
                    if (WirelessOn && ParentVehicle == null && IsModularCurrentlyEquipped
                        && Parent?.WirelessOn != false)
                    {
                        if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
                        {
                            if (WirelessBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                            {
                                ImprovementManager.DisableImprovements(_objCharacter,
                                                                       _objCharacter.Improvements
                                                                           .Where(x => x.ImproveSource == SourceType
                                                                               && x.SourceName == InternalId));
                            }

                            string strSourceNameToUse = InternalId + "Wireless";
                            ImprovementManager.RemoveImprovements(_objCharacter,
                                                                  _objCharacter.Improvements
                                                                               .Where(x => x.ImproveSource == SourceType
                                                                                   && x.SourceName
                                                                                   == strSourceNameToUse)
                                                                               .ToList());
                            ImprovementManager.CreateImprovements(_objCharacter, _eImprovementSource,
                                                                  strSourceNameToUse, WirelessBonus, Rating,
                                                                  CurrentDisplayNameShort);

                            string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                            if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                                _strExtra = strSelectedValue;
                        }

                        if (!string.IsNullOrEmpty(WirelessPairBonus?.InnerText))
                        {
                            // This cyberware should not be included in the count to make things easier.
                            List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                                x => x != this && IncludeWirelessPair.Contains(x.Name) && x.Extra == Extra &&
                                     x.IsModularCurrentlyEquipped && x.WirelessOn).ToList();
                            int intCount = lstPairableCyberwares.Count;
                            // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                            if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                            {
                                intCount = 0;
                                foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                                {
                                    if (objPairableCyberware.Location != Location)
                                        // We have found a cyberware with which this one could be paired, so increase count by 1
                                        ++intCount;
                                    else
                                        // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                        --intCount;
                                }

                                // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                                intCount = (intCount > 0).ToInt32();
                            }

                            if (intCount % 2 == 1)
                            {
                                if (WirelessPairBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                                {
                                    ImprovementManager.DisableImprovements(_objCharacter,
                                                                           _objCharacter.Improvements
                                                                               .Where(x => x.ImproveSource == SourceType
                                                                                   && x.SourceName == InternalId)
                                                                               .ToList());
                                }

                                string strSourceNameToUse = InternalId + "WirelessPair";
                                ImprovementManager.RemoveImprovements(_objCharacter,
                                                                      _objCharacter.Improvements
                                                                          .Where(x => x.ImproveSource == SourceType
                                                                              && x.SourceName == strSourceNameToUse)
                                                                          .ToList());
                                ImprovementManager.CreateImprovements(_objCharacter, SourceType,
                                                                      strSourceNameToUse, WirelessPairBonus,
                                                                      Rating, CurrentDisplayNameShort);
                            }

                            foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                            {
                                ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                                      objLoopCyberware.InternalId + "WirelessPair");
                                if (objLoopCyberware.WirelessPairBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                                {
                                    ImprovementManager.DisableImprovements(_objCharacter,
                                                                           _objCharacter.Improvements
                                                                               .Where(
                                                                                   x => x.ImproveSource
                                                                                       == objLoopCyberware.SourceType
                                                                                       && x.SourceName
                                                                                       == objLoopCyberware.InternalId)
                                                                               .ToList());
                                }

                                // Go down the list and create pair bonuses for every second item
                                if (intCount > 0 && intCount % 2 == 1)
                                {
                                    ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                                          objLoopCyberware.InternalId + "WirelessPair",
                                                                          objLoopCyberware.WirelessPairBonus,
                                                                          objLoopCyberware.Rating,
                                                                          objLoopCyberware.CurrentDisplayNameShort);
                                }

                                --intCount;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
                        {
                            if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                            {
                                ImprovementManager.EnableImprovements(_objCharacter,
                                                                      _objCharacter.Improvements
                                                                          .Where(x => x.ImproveSource == SourceType
                                                                              && x.SourceName == InternalId));
                            }

                            string strSourceNameToRemove = InternalId + "Wireless";
                            ImprovementManager.RemoveImprovements(_objCharacter,
                                                                  _objCharacter.Improvements
                                                                               .Where(x => x.ImproveSource == SourceType
                                                                                   && x.SourceName
                                                                                   == strSourceNameToRemove)
                                                                               .ToList());
                        }

                        if (!string.IsNullOrEmpty(WirelessPairBonus?.InnerText))
                        {
                            ImprovementManager.RemoveImprovements(_objCharacter, SourceType,
                                                                  InternalId + "WirelessPair");
                            // This cyberware should not be included in the count to make things easier (we want to get the same number regardless of whether we call this before or after the actual equipping).
                            List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                                x => x != this && IncludeWirelessPair.Contains(x.Name) && x.Extra == Extra &&
                                     x.IsModularCurrentlyEquipped && WirelessOn).ToList();
                            int intCount = lstPairableCyberwares.Count;
                            // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                            if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                            {
                                int intMatchLocationCount = 0;
                                int intNotMatchLocationCount = 0;
                                foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                                {
                                    if (objPairableCyberware.Location != Location)
                                        ++intNotMatchLocationCount;
                                    else
                                        ++intMatchLocationCount;
                                }

                                // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                                intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                            }

                            if (WirelessPairBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                            {
                                ImprovementManager.EnableImprovements(_objCharacter,
                                                                      _objCharacter.Improvements
                                                                          .Where(x => x.ImproveSource == SourceType
                                                                              && x.SourceName == InternalId));
                            }

                            foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                            {
                                ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                                      objLoopCyberware.InternalId + "WirelessPair");
                                // Go down the list and create pair bonuses for every second item
                                if (intCount > 0 && intCount % 2 == 0)
                                {
                                    ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                                          objLoopCyberware.InternalId + "WirelessPair",
                                                                          objLoopCyberware.WirelessPairBonus,
                                                                          objLoopCyberware.Rating,
                                                                          objLoopCyberware.CurrentDisplayNameShort);
                                }

                                --intCount;
                            }
                        }
                    }
                }

                foreach (Cyberware objCyberware in Children.AsEnumerableWithSideEffects())
                    objCyberware.RefreshWirelessBonuses();
                foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                    objGear.RefreshWirelessBonuses();
            }
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this weapon accessory.
        /// </summary>
        public async Task RefreshWirelessBonusesAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(WirelessBonus?.InnerText)
                    || !string.IsNullOrEmpty(WirelessPairBonus?.InnerText))
                {
                    if (WirelessOn && ParentVehicle == null && await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false)
                        && Parent?.WirelessOn != false)
                    {
                        if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
                        {
                            if (WirelessBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value ==
                                "replace")
                            {
                                await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                    await _objCharacter.Improvements.ToListAsync(
                                        x => x.ImproveSource == SourceType
                                             && x.SourceName == InternalId,
                                        token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                            }

                            string strSourceNameToUse = InternalId + "Wireless";
                            await ImprovementManager.RemoveImprovementsAsync(
                                _objCharacter,
                                await _objCharacter.Improvements.ToListAsync(
                                    x => x.ImproveSource == SourceType && x.SourceName == strSourceNameToUse,
                                    token: token).ConfigureAwait(false),
                                token: token).ConfigureAwait(false);
                            await ImprovementManager.CreateImprovementsAsync(
                                    _objCharacter, _eImprovementSource, strSourceNameToUse,
                                    WirelessBonus,
                                    await GetRatingAsync(token).ConfigureAwait(false),
                                    await GetCurrentDisplayNameShortAsync(token)
                                        .ConfigureAwait(false), token: token)
                                .ConfigureAwait(false);

                            string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                            if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                                _strExtra = strSelectedValue;
                        }

                        if (!string.IsNullOrEmpty(WirelessPairBonus?.InnerText))
                        {
                            // This cyberware should not be included in the count to make things easier.
                            List<Cyberware> lstPairableCyberwares
                                = await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false))
                                    .DeepWhereAsync(
                                        x => x.Children,
                                        async x => !ReferenceEquals(x, this) && IncludeWirelessPair.Contains(x.Name)
                                                                             && x.Extra == Extra &&
                                                                             await x.GetIsModularCurrentlyEquippedAsync(
                                                                                     token)
                                                                                 .ConfigureAwait(false) && x.WirelessOn,
                                        token).ConfigureAwait(false);
                            int intCount = lstPairableCyberwares.Count;
                            // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                            if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                            {
                                intCount = 0;
                                foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                                {
                                    if (objPairableCyberware.Location != Location)
                                        // We have found a cyberware with which this one could be paired, so increase count by 1
                                        ++intCount;
                                    else
                                        // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                        --intCount;
                                }

                                // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                                intCount = (intCount > 0).ToInt32();
                            }

                            if (intCount % 2 == 1)
                            {
                                if (WirelessPairBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)
                                        ?.Value == "replace")
                                {
                                    await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                        await _objCharacter.Improvements.ToListAsync(
                                            x => x.ImproveSource == SourceType
                                                 && x.SourceName == InternalId,
                                            token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                                }

                                string strSourceNameToUse = InternalId + "WirelessPair";
                                await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                        await _objCharacter.Improvements
                                            .ToListAsync(
                                                x => x.ImproveSource == SourceType
                                                     && x.SourceName
                                                     == strSourceNameToUse,
                                                token: token).ConfigureAwait(false),
                                        token: token)
                                    .ConfigureAwait(false);
                                await ImprovementManager.CreateImprovementsAsync(
                                        _objCharacter, SourceType, strSourceNameToUse,
                                        WirelessPairBonus,
                                        await GetRatingAsync(token).ConfigureAwait(false),
                                        await GetCurrentDisplayNameShortAsync(token)
                                            .ConfigureAwait(false), token: token)
                                    .ConfigureAwait(false);
                            }

                            foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                            {
                                await ImprovementManager.RemoveImprovementsAsync(
                                    _objCharacter, objLoopCyberware.SourceType,
                                    objLoopCyberware.InternalId
                                    + "WirelessPair", token).ConfigureAwait(false);
                                if (WirelessPairBonus != null &&
                                    objLoopCyberware.WirelessPairBonus
                                        .SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value ==
                                    "replace")
                                {
                                    await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                        await _objCharacter.Improvements.ToListAsync(
                                            x => x.ImproveSource
                                                 == objLoopCyberware.SourceType
                                                 && x.SourceName
                                                 == objLoopCyberware.InternalId,
                                            token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                                }

                                // Go down the list and create pair bonuses for every second item
                                if (intCount > 0 && intCount % 2 == 1)
                                {
                                    await ImprovementManager.CreateImprovementsAsync(
                                            _objCharacter, objLoopCyberware.SourceType,
                                            objLoopCyberware.InternalId + "WirelessPair",
                                            objLoopCyberware.WirelessPairBonus,
                                            await objLoopCyberware.GetRatingAsync(token)
                                                .ConfigureAwait(false),
                                            await objLoopCyberware
                                                .GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false), token: token)
                                        .ConfigureAwait(false);
                                }

                                --intCount;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
                        {
                            if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value ==
                                "replace")
                            {
                                await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                                    await _objCharacter.Improvements.ToListAsync(
                                        x => x.ImproveSource == SourceType
                                             && x.SourceName == InternalId,
                                        token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                            }

                            string strSourceNameToRemove = InternalId + "Wireless";
                            await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                    await _objCharacter.Improvements
                                        .ToListAsync(
                                            x => x.ImproveSource == SourceType
                                                 && x.SourceName
                                                 == strSourceNameToRemove,
                                            token: token)
                                        .ConfigureAwait(false), token: token)
                                .ConfigureAwait(false);
                        }

                        if (!string.IsNullOrEmpty(WirelessPairBonus?.InnerText))
                        {
                            await ImprovementManager.RemoveImprovementsAsync(
                                _objCharacter, SourceType, InternalId + "WirelessPair", token).ConfigureAwait(false);
                            // This cyberware should not be included in the count to make things easier (we want to get the same number regardless of whether we call this before or after the actual equipping).
                            List<Cyberware> lstPairableCyberwares
                                = await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false))
                                    .DeepWhereAsync(
                                        x => x.Children,
                                        async x => !ReferenceEquals(x, this) && IncludeWirelessPair.Contains(x.Name)
                                                                             && x.Extra == Extra &&
                                                                             await x.GetIsModularCurrentlyEquippedAsync(
                                                                                     token)
                                                                                 .ConfigureAwait(false) && x.WirelessOn,
                                        token).ConfigureAwait(false);
                            int intCount = lstPairableCyberwares.Count;
                            // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                            if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                            {
                                int intMatchLocationCount = 0;
                                int intNotMatchLocationCount = 0;
                                foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                                {
                                    if (objPairableCyberware.Location != Location)
                                        ++intNotMatchLocationCount;
                                    else
                                        ++intMatchLocationCount;
                                }

                                // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                                intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                            }

                            if (WirelessPairBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)
                                    ?.Value == "replace")
                            {
                                await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                                        await _objCharacter.Improvements.ToListAsync(
                                                x => x.ImproveSource == SourceType
                                                     && x.SourceName == InternalId, token: token)
                                            .ConfigureAwait(false), token)
                                    .ConfigureAwait(false);
                            }

                            foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                            {
                                await ImprovementManager.RemoveImprovementsAsync(
                                    _objCharacter, objLoopCyberware.SourceType,
                                    objLoopCyberware.InternalId
                                    + "WirelessPair", token).ConfigureAwait(false);
                                // Go down the list and create pair bonuses for every second item
                                if (intCount > 0 && intCount % 2 == 0)
                                {
                                    await ImprovementManager.CreateImprovementsAsync(
                                            _objCharacter, objLoopCyberware.SourceType,
                                            objLoopCyberware.InternalId + "WirelessPair",
                                            objLoopCyberware.WirelessPairBonus,
                                            await objLoopCyberware.GetRatingAsync(token)
                                                .ConfigureAwait(false),
                                            await objLoopCyberware
                                                .GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false), token: token)
                                        .ConfigureAwait(false);
                                }

                                --intCount;
                            }
                        }
                    }
                }

                await Children.ForEachWithSideEffectsAsync(x => x.RefreshWirelessBonusesAsync(token), token).ConfigureAwait(false);
                await GearChildren.ForEachWithSideEffectsAsync(x => x.RefreshWirelessBonusesAsync(token), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intSortOrder;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _intSortOrder = value;
            }
        }

        /// <summary>
        /// Equips a piece of modular cyberware, activating the improvements of it and its children. Call after attaching onto objCharacter.Cyberware or a parent
        /// </summary>
        public void ChangeModularEquip(bool blnEquip, bool blnSkipEncumbranceOnPropertyChanged = false)
        {
            using (LockObject.EnterWriteLock())
            {
                if (blnEquip)
                {
                    ImprovementManager.EnableImprovements(_objCharacter,
                                                          _objCharacter.Improvements.Where(
                                                              x => x.ImproveSource == SourceType
                                                                   && x.SourceName == InternalId));

                    /*
                    // If the piece grants a bonus, pass the information to the Improvement Manager.
                    if (Bonus != null || WirelessBonus != null || PairBonus != null)
                    {
                        if (!string.IsNullOrEmpty(_strForced) && _strForced != "Right" && _strForced != "Left")
                            ImprovementManager.ForcedValue = _strForced;

                        if (Bonus != null)
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, Bonus, false, Rating, CurrentDisplayNameShort);
                        string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                            _strExtra = strSelectedValue;

                        if (WirelessBonus != null && WirelessOn)
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, WirelessBonus, false, Rating, CurrentDisplayNameShort);
                        string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                            _strExtra = strSelectedValue;
                    }
                    */

                    if (PairBonus != null)
                    {
                        // This cyberware should not be included in the count to make things easier.
                        List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                            x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                 x.IsModularCurrentlyEquipped).ToList();
                        int intCount = lstPairableCyberwares.Count;
                        // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                        if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                        {
                            intCount = 0;
                            foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                            {
                                if (objPairableCyberware.Location != Location)
                                    // We have found a cyberware with which this one could be paired, so increase count by 1
                                    ++intCount;
                                else
                                    // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                    --intCount;
                            }

                            // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                            intCount = (intCount > 0).ToInt32();
                        }

                        if ((intCount & 1) == 1)
                        {
                            if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                ImprovementManager.SetForcedValue(_strForced, _objCharacter);
                            else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId + "Pair",
                                                                  PairBonus,
                                                                  Rating, CurrentDisplayNameShort);
                        }
                    }
                }
                else
                {
                    ImprovementManager.DisableImprovements(_objCharacter,
                                                           _objCharacter.Improvements.Where(
                                                               x => x.ImproveSource == SourceType
                                                                    && x.SourceName == InternalId));

                    if (PairBonus != null)
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "Pair");
                        // This cyberware should not be included in the count to make things easier (we want to get the same number regardless of whether we call this before or after the actual equipping).
                        List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                            x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                 x.IsModularCurrentlyEquipped).ToList();
                        int intCount = lstPairableCyberwares.Count;
                        // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                        if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                        {
                            int intMatchLocationCount = 0;
                            int intNotMatchLocationCount = 0;
                            foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                            {
                                if (objPairableCyberware.Location != Location)
                                    ++intNotMatchLocationCount;
                                else
                                    ++intMatchLocationCount;
                            }

                            // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                            intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                        }

                        foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                        {
                            ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                                  objLoopCyberware.InternalId + "Pair");
                            // Go down the list and create pair bonuses for every second item
                            if (intCount > 0 && (intCount & 1) == 0)
                            {
                                if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                    ImprovementManager.SetForcedValue(_strForced, _objCharacter);
                                else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                    ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                                ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                                      objLoopCyberware.InternalId + "Pair",
                                                                      objLoopCyberware.PairBonus,
                                                                      objLoopCyberware.Rating,
                                                                      objLoopCyberware.CurrentDisplayNameShort);
                            }

                            --intCount;
                        }
                    }
                }

                foreach (Gear objChildGear in GearChildren.AsEnumerableWithSideEffects())
                    objChildGear.ChangeEquippedStatus(blnEquip, true);

                foreach (Cyberware objChild in Children.AsEnumerableWithSideEffects())
                    objChild.ChangeModularEquip(blnEquip, true);

                RefreshWirelessBonuses();

                if (!blnSkipEncumbranceOnPropertyChanged && ParentVehicle == null && _objCharacter?.IsLoading == false
                    && (!string.IsNullOrEmpty(Weight)
                        || GearChildren.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                x => x.Equipped && !string.IsNullOrEmpty(x.Weight))
                        || Children.DeepAny(x => x.Children,
                                            y => !string.IsNullOrEmpty(y.Weight)
                                                 || y.GearChildren.DeepAny(
                                                     x => x.Children.Where(z => z.Equipped),
                                                     x => x.Equipped && !string.IsNullOrEmpty(x.Weight)))))
                    _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
            }
        }

        /// <summary>
        /// Equips a piece of modular cyberware, activating the improvements of it and its children. Call after attaching onto objCharacter.Cyberware or a parent
        /// </summary>
        public async Task ChangeModularEquipAsync(bool blnEquip, bool blnSkipEncumbranceOnPropertyChanged = false, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (blnEquip)
                {
                    await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                                                                     await (await _objCharacter
                                                                               .GetImprovementsAsync(token)
                                                                               .ConfigureAwait(false))
                                                                           .ToListAsync(
                                                                               x => x.ImproveSource == SourceType
                                                                                   && x.SourceName == InternalId,
                                                                               token: token).ConfigureAwait(false),
                                                                     token).ConfigureAwait(false);

                    /*
                    // If the piece grants a bonus, pass the information to the Improvement Manager.
                    if (Bonus != null || WirelessBonus != null || PairBonus != null)
                    {
                        if (!string.IsNullOrEmpty(_strForced) && _strForced != "Right" && _strForced != "Left")
                            ImprovementManager.ForcedValue = _strForced;

                        if (Bonus != null)
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, Bonus, false, Rating, CurrentDisplayNameShort);
                        string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                            _strExtra = strSelectedValue;

                        if (WirelessBonus != null && WirelessOn)
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, WirelessBonus, false, Rating, CurrentDisplayNameShort);
                        string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                        if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                            _strExtra = strSelectedValue;
                    }
                    */

                    if (PairBonus != null)
                    {
                        // This cyberware should not be included in the count to make things easier.
                        List<Cyberware> lstPairableCyberwares
                            = await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepWhereAsync(
                                x => x.Children,
                                async x => !ReferenceEquals(x, this) && IncludePair.Contains(x.Name)
                                                                     && x.Extra == Extra &&
                                                                     await x.GetIsModularCurrentlyEquippedAsync(token)
                                                                            .ConfigureAwait(false),
                                token).ConfigureAwait(false);
                        int intCount = lstPairableCyberwares.Count;
                        // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                        if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                        {
                            intCount = 0;
                            foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                            {
                                if (objPairableCyberware.Location != Location)
                                    // We have found a cyberware with which this one could be paired, so increase count by 1
                                    ++intCount;
                                else
                                    // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                    --intCount;
                            }

                            // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                            intCount = (intCount > 0).ToInt32();
                        }

                        if ((intCount & 1) == 1)
                        {
                            if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                ImprovementManager.SetForcedValue(_strForced, _objCharacter);
                            else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                            await ImprovementManager.CreateImprovementsAsync(
                                _objCharacter, SourceType, InternalId + "Pair", PairBonus,
                                await GetRatingAsync(token).ConfigureAwait(false), await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                token: token).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                                                      await (await _objCharacter
                                                                          .GetImprovementsAsync(token)
                                                                          .ConfigureAwait(false)).ToListAsync(
                                                                          x => x.ImproveSource == SourceType
                                                                               && x.SourceName == InternalId,
                                                                          token: token).ConfigureAwait(false), token)
                                            .ConfigureAwait(false);

                    if (PairBonus != null)
                    {
                        await ImprovementManager
                              .RemoveImprovementsAsync(_objCharacter, SourceType, InternalId + "Pair", token)
                              .ConfigureAwait(false);
                        // This cyberware should not be included in the count to make things easier (we want to get the same number regardless of whether we call this before or after the actual equipping).
                        List<Cyberware> lstPairableCyberwares
                            = await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepWhereAsync(
                                x => x.Children,
                                async x => !ReferenceEquals(x, this) && IncludePair.Contains(x.Name)
                                                                     && x.Extra == Extra &&
                                                                     await x.GetIsModularCurrentlyEquippedAsync(token)
                                                                            .ConfigureAwait(false),
                                token).ConfigureAwait(false);
                        int intCount = lstPairableCyberwares.Count;
                        // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                        if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                        {
                            int intMatchLocationCount = 0;
                            int intNotMatchLocationCount = 0;
                            foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                            {
                                if (objPairableCyberware.Location != Location)
                                    ++intNotMatchLocationCount;
                                else
                                    ++intMatchLocationCount;
                            }

                            // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                            intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                        }

                        foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                        {
                            await ImprovementManager.RemoveImprovementsAsync(_objCharacter, objLoopCyberware.SourceType,
                                                                             objLoopCyberware.InternalId + "Pair",
                                                                             token).ConfigureAwait(false);
                            // Go down the list and create pair bonuses for every second item
                            if (intCount > 0 && (intCount & 1) == 0)
                            {
                                if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                    ImprovementManager.SetForcedValue(_strForced, _objCharacter);
                                else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                    ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                                await ImprovementManager.CreateImprovementsAsync(
                                    _objCharacter, objLoopCyberware.SourceType,
                                    objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus,
                                    await objLoopCyberware.GetRatingAsync(token).ConfigureAwait(false),
                                    await objLoopCyberware.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                    token: token).ConfigureAwait(false);
                            }

                            --intCount;
                        }
                    }
                }

                await GearChildren.ForEachWithSideEffectsAsync(x => x.ChangeEquippedStatusAsync(blnEquip, true, token),
                    token).ConfigureAwait(false);
                await Children.ForEachWithSideEffectsAsync(x => x.ChangeModularEquipAsync(blnEquip, true, token),
                    token).ConfigureAwait(false);

                await RefreshWirelessBonusesAsync(token).ConfigureAwait(false);

                if (!blnSkipEncumbranceOnPropertyChanged && ParentVehicle == null && _objCharacter?.IsLoading == false
                    && (!string.IsNullOrEmpty(Weight)
                        || GearChildren.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                x => x.Equipped && !string.IsNullOrEmpty(x.Weight))
                        || await Children.DeepAnyAsync(x => x.Children,
                                                       y => !string.IsNullOrEmpty(y.Weight)
                                                            || y.GearChildren.DeepAny(
                                                                x => x.Children.Where(z => z.Equipped),
                                                                x => x.Equipped && !string.IsNullOrEmpty(x.Weight)),
                                                       token)
                                         .ConfigureAwait(false)))
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool CanRemoveThroughImprovements
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    Cyberware objGrandparent = Parent;
                    bool blnNoParentIsModular = string.IsNullOrEmpty(PlugsIntoModularMount);
                    while (objGrandparent != null && blnNoParentIsModular)
                    {
                        using (objGrandparent.LockObject.EnterReadLock())
                        {
                            Cyberware objParent = objGrandparent;
                            objGrandparent = objGrandparent.Parent;
                            blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                        }
                    }

                    return blnNoParentIsModular;
                }
            }
        }

        public async Task<bool> GetCanRemoveThroughImprovementsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Cyberware objGrandparent = await GetParentAsync(token).ConfigureAwait(false);
                bool blnNoParentIsModular = string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false));
                while (objGrandparent != null && blnNoParentIsModular)
                {
                    token.ThrowIfCancellationRequested();
                    IAsyncDisposable objLocker2 = await objGrandparent.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        Cyberware objParent = objGrandparent;
                        objGrandparent = await objGrandparent.GetParentAsync(token).ConfigureAwait(false);
                        blnNoParentIsModular = string.IsNullOrEmpty(await objParent.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false));
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return blnNoParentIsModular;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Comma-separated list of mount locations with which this 'ware is mutually exclusive.
        /// </summary>
        public string BlocksMounts
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strBlocksMounts;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strBlocksMounts = value;
            }
        }

        /// <summary>
        /// Comma-separated list of mount locations with which this 'ware is mutually exclusive.
        /// </summary>
        public async Task<string> GetBlocksMountsAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strBlocksMounts;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return Math.Max(Math.Min(_intRating, MaxRating), MinRating);
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    value = Math.Max(Math.Min(value, MaxRating), MinRating);
                    if (Interlocked.Exchange(ref _intRating, value) == value)
                        return;
                    if (GearChildren.Count > 0)
                    {
                        foreach (Gear objChild in GearChildren.Where(x =>
                                                                         x.MaxRating.Contains("Parent")
                                                                         || x.MinRating.Contains("Parent")))
                        {
                            // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                            int intCurrentRating = objChild.Rating;
                            objChild.Rating = intCurrentRating;
                        }
                    }

                    DoPropertyChanges(true, false);
                }
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public async Task<int> GetRatingAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Math.Max(Math.Min(_intRating, await GetMaxRatingAsync(token).ConfigureAwait(false)),
                                await GetMinRatingAsync(token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public async Task SetRatingAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                value = Math.Max(Math.Min(value, await GetMaxRatingAsync(token).ConfigureAwait(false)), await GetMinRatingAsync(token).ConfigureAwait(false));
                if (Interlocked.Exchange(ref _intRating, value) == value)
                    return;
                if (await GearChildren.GetCountAsync(token).ConfigureAwait(false) > 0)
                {
                    await GearChildren.ForEachAsync(async objChild =>
                    {
                        if (objChild.MaxRating.Contains("Parent") || objChild.MinRating.Contains("Parent"))
                        {
                            // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                            await objChild.SetRatingAsync(await objChild.GetRatingAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                        }
                    }, token).ConfigureAwait(false);
                }

                await DoPropertyChangesAsync(true, false, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private int _intProcessPropertyChanges = 1;

        private void DoPropertyChanges(bool blnDoRating, bool blnDoGrade)
        {
            using (LockObject.EnterReadLock())
            {
                // Do not do property changes if we're not directly equipped to a character
                if (_intProcessPropertyChanges == 0
                    || (ParentVehicle != null && string.IsNullOrEmpty(PlugsIntoModularMount)))
                    return;
            }

            using (LockObject.EnterUpgradeableReadLock())
            {
                // Do not do property changes if we're not directly equipped to a character
                if (_intProcessPropertyChanges == 0
                    || (ParentVehicle != null && string.IsNullOrEmpty(PlugsIntoModularMount)))
                    return;
                using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>>(
                           Utils.DictionaryForMultiplePropertyChangedPool,
                           out Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>> dicChangedProperties))
                {
                    try
                    {
                        if ((blnDoGrade || (blnDoRating && ESS.ContainsAny("Rating", "FixedValues"))) &&
                            (Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount))
                        {
                            if (!dicChangedProperties.TryGetValue(_objCharacter,
                                                                  out HashSet<string> setChangedProperties))
                            {
                                setChangedProperties = Utils.StringHashSetPool.Get();
                                dicChangedProperties.Add(_objCharacter, setChangedProperties);
                            }

                            setChangedProperties.Add(EssencePropertyName);
                        }

                        if (blnDoRating)
                        {
                            if (IsModularCurrentlyEquipped && ParentVehicle == null)
                            {
                                if (_objParent?.IsLimb == true
                                    && _objParent.Parent?.InheritAttributes != false
                                    && _objParent.ParentVehicle == null
                                    && !_objCharacter.Settings.DontUseCyberlimbCalculation
                                    && !_objCharacter.Settings.ExcludeLimbSlot.Contains(_objParent.LimbSlot))
                                {
                                    foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in
                                             s_AttributeAffectingCyberwares)
                                    {
                                        if (!kvpToCheck.Value.Contains(Name))
                                            continue;
                                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes(
                                                     kvpToCheck.Key))
                                        {
                                            if (!dicChangedProperties.TryGetValue(
                                                    objCharacterAttrib, out HashSet<string> setChangedProperties))
                                            {
                                                setChangedProperties = Utils.StringHashSetPool.Get();
                                                dicChangedProperties.Add(objCharacterAttrib, setChangedProperties);
                                            }

                                            setChangedProperties.Add(nameof(CharacterAttrib.TotalValue));
                                        }
                                    }
                                }

                                if (Weight.ContainsAny("Rating", "FixedValues")
                                    || GearChildren.Any(x => x.Equipped
                                                             && x.Weight.Contains(
                                                                 "Parent Rating", StringComparison.OrdinalIgnoreCase))
                                    || Children.Any(x => x.IsModularCurrentlyEquipped
                                                         && x.Weight.Contains(
                                                             "Parent Rating", StringComparison.OrdinalIgnoreCase)))
                                {
                                    if (!dicChangedProperties.TryGetValue(_objCharacter,
                                                                          out HashSet<string> setChangedProperties))
                                    {
                                        setChangedProperties = Utils.StringHashSetPool.Get();
                                        dicChangedProperties.Add(_objCharacter, setChangedProperties);
                                    }

                                    setChangedProperties.Add(nameof(Character.TotalCarriedWeight));
                                }
                            }

                            // Needed in order to properly process named sources where
                            // the tooltip was built before the object was added to the character
                            if (Bonus?.InnerText.Contains("Rating") == true
                                || PairBonus?.InnerText.Contains("Rating") == true
                                || (WirelessOn && (WirelessBonus?.InnerText.Contains("Rating") == true
                                                   || WirelessPairBonus?.InnerText.Contains("Rating") == true)))
                            {
                                if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                    ImprovementManager.SetForcedValue(_strForced, _objCharacter);

                                if (Bonus != null)
                                {
                                    if (PairBonus != null)
                                        ImprovementManager.RemoveImprovements(_objCharacter, SourceType, new[] { InternalId, InternalId + "Pair"});
                                    else
                                        ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId);
                                    ImprovementManager.CreateImprovements(_objCharacter, SourceType,
                                                                          InternalId, Bonus, Rating,
                                                                          CurrentDisplayNameShort);
                                }

                                string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                                if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                                    _strExtra = strSelectedValue;

                                if (PairBonus != null)
                                {
                                    if (Bonus == null)
                                        ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "Pair");
                                    // This cyberware should not be included in the count to make things easier.
                                    List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(
                                        x => x.Children,
                                        x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                             x.IsModularCurrentlyEquipped).ToList();
                                    int intCount = lstPairableCyberwares.Count;
                                    // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                                    if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                                    {
                                        intCount = 0;
                                        foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                                        {
                                            if (objPairableCyberware.Location != Location)
                                                // We have found a cyberware with which this one could be paired, so increase count by 1
                                                ++intCount;
                                            else
                                                // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                                --intCount;
                                        }

                                        // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                                        intCount = (intCount > 0).ToInt32();
                                    }

                                    if ((intCount & 1) == 1)
                                    {
                                        if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left"
                                                                              && _strForced != "Right")
                                            ImprovementManager.SetForcedValue(_strForced, _objCharacter);
                                        else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                            ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                                        ImprovementManager.CreateImprovements(
                                            _objCharacter, SourceType, InternalId + "Pair",
                                            PairBonus, Rating, CurrentDisplayNameShort);
                                    }
                                }

                                if (!IsModularCurrentlyEquipped || ParentVehicle != null)
                                    ChangeModularEquip(false);
                                else
                                    RefreshWirelessBonuses();
                            }
                        }

                        foreach (KeyValuePair<INotifyMultiplePropertiesChangedAsync, HashSet<string>> kvpToProcess in
                                 dicChangedProperties)
                        {
                            kvpToProcess.Key.OnMultiplePropertiesChanged(kvpToProcess.Value.ToList());
                        }
                    }
                    finally
                    {
                        List<HashSet<string>> lstToReturn = dicChangedProperties.Values.ToList();
                        for (int i = lstToReturn.Count - 1; i >= 0; --i)
                        {
                            HashSet<string> setLoop = lstToReturn[i];
                            Utils.StringHashSetPool.Return(ref setLoop);
                        }
                    }
                }
            }
        }

        private async Task DoPropertyChangesAsync(bool blnDoRating, bool blnDoGrade, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Do not do property changes if we're not directly equipped to a character
                if (_intProcessPropertyChanges == 0
                    || (ParentVehicle != null && string.IsNullOrEmpty(PlugsIntoModularMount)))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Do not do property changes if we're not directly equipped to a character
                if (_intProcessPropertyChanges == 0
                    || (await GetParentVehicleAsync(token).ConfigureAwait(false) != null && string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false))))
                    return;
                using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>>(
                           Utils.DictionaryForMultiplePropertyChangedPool,
                           out Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>> dicChangedProperties))
                {
                    try
                    {
                        if ((blnDoGrade || (blnDoRating && ESS.ContainsAny("Rating", "FixedValues"))) &&
                            (await GetParentAsync(token).ConfigureAwait(false) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) && string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)))
                        {
                            if (!dicChangedProperties.TryGetValue(_objCharacter,
                                    out HashSet<string> setChangedProperties))
                            {
                                setChangedProperties = Utils.StringHashSetPool.Get();
                                dicChangedProperties.Add(_objCharacter, setChangedProperties);
                            }

                            setChangedProperties.Add(await GetEssencePropertyNameAsync(token).ConfigureAwait(false));
                        }

                        if (blnDoRating)
                        {
                            if (await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false) &&
                                await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                            {
                                if (_objParent != null && await _objParent.GetIsLimbAsync(token).ConfigureAwait(false)
                                                       && (await _objParent.GetParentAsync(token).ConfigureAwait(false) == null || await
                                                           (await _objParent.GetParentAsync(token).ConfigureAwait(false))
                                                           .GetInheritAttributesAsync(token).ConfigureAwait(false))
                                                       && await _objParent.GetParentVehicleAsync(token).ConfigureAwait(false) == null
                                                       && !await _objCharacter.Settings.GetDontUseCyberlimbCalculationAsync(token).ConfigureAwait(false)
                                                       && !(await _objCharacter.Settings.GetExcludeLimbSlotAsync(token).ConfigureAwait(false)).Contains(await _objParent
                                                           .GetLimbSlotAsync(token).ConfigureAwait(false)))
                                {
                                    foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in
                                             s_AttributeAffectingCyberwares)
                                    {
                                        if (!kvpToCheck.Value.Contains(Name))
                                            continue;
                                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes(
                                                     kvpToCheck.Key, token: token))
                                        {
                                            if (!dicChangedProperties.TryGetValue(
                                                    objCharacterAttrib, out HashSet<string> setChangedProperties))
                                            {
                                                setChangedProperties = Utils.StringHashSetPool.Get();
                                                dicChangedProperties.Add(objCharacterAttrib, setChangedProperties);
                                            }

                                            setChangedProperties.Add(nameof(CharacterAttrib.TotalValue));
                                        }
                                    }
                                }

                                if (Weight.ContainsAny("Rating", "FixedValues")
                                    || await GearChildren.AnyAsync(x => x.Equipped
                                                                        && x.Weight.Contains(
                                                                            "Parent Rating",
                                                                            StringComparison.OrdinalIgnoreCase),
                                        token: token).ConfigureAwait(false)
                                    || await Children.AnyAsync(async x =>
                                            await x.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false)
                                            && x.Weight.Contains(
                                                "Parent Rating", StringComparison.OrdinalIgnoreCase), token: token)
                                        .ConfigureAwait(false))
                                {
                                    if (!dicChangedProperties.TryGetValue(_objCharacter,
                                            out HashSet<string> setChangedProperties))
                                    {
                                        setChangedProperties = Utils.StringHashSetPool.Get();
                                        dicChangedProperties.Add(_objCharacter, setChangedProperties);
                                    }

                                    setChangedProperties.Add(nameof(Character.TotalCarriedWeight));
                                }
                            }

                            // Needed in order to properly process named sources where
                            // the tooltip was built before the object was added to the character
                            if (Bonus?.InnerText.Contains("Rating") == true
                                || PairBonus?.InnerText.Contains("Rating") == true
                                || (WirelessOn && (WirelessBonus?.InnerText.Contains("Rating") == true
                                                   || WirelessPairBonus?.InnerText.Contains("Rating") == true)))
                            {
                                if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                    ImprovementManager.SetForcedValue(_strForced, _objCharacter);

                                if (Bonus != null)
                                {
                                    if (PairBonus != null)
                                        await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                            await GetSourceTypeAsync(token).ConfigureAwait(false),
                                            new[] { InternalId, InternalId + "Pair" }, token).ConfigureAwait(false);
                                    else
                                        await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                                await GetSourceTypeAsync(token).ConfigureAwait(false), InternalId,
                                                token)
                                            .ConfigureAwait(false);
                                    await ImprovementManager.CreateImprovementsAsync(_objCharacter,
                                        await GetSourceTypeAsync(token).ConfigureAwait(false),
                                        InternalId, Bonus, await GetRatingAsync(token).ConfigureAwait(false),
                                        await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                        token: token).ConfigureAwait(false);
                                }

                                string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                                if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                                    _strExtra = strSelectedValue;

                                if (PairBonus != null)
                                {
                                    if (Bonus == null)
                                        await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                            await GetSourceTypeAsync(token).ConfigureAwait(false),
                                            InternalId + "Pair", token).ConfigureAwait(false);
                                    // This cyberware should not be included in the count to make things easier.
                                    List<Cyberware> lstPairableCyberwares = await _objCharacter.Cyberware
                                        .DeepWhereAsync(
                                            x => x.Children,
                                            async x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
                                                       await x.GetIsModularCurrentlyEquippedAsync(token)
                                                           .ConfigureAwait(false), token).ConfigureAwait(false);
                                    int intCount = lstPairableCyberwares.Count;
                                    // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                                    if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                                    {
                                        intCount = 0;
                                        foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                                        {
                                            if (objPairableCyberware.Location != Location)
                                                // We have found a cyberware with which this one could be paired, so increase count by 1
                                                ++intCount;
                                            else
                                                // We have found a cyberware that would serve as a pair to another cyberware instead of this one, so decrease count by 1
                                                --intCount;
                                        }

                                        // If we have at least one cyberware with which we could pair, set count to 1 so that it passes the modulus to add the PairBonus. Otherwise, set to 0 so it doesn't pass.
                                        intCount = (intCount > 0).ToInt32();
                                    }

                                    if ((intCount & 1) == 1)
                                    {
                                        if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left"
                                                                              && _strForced != "Right")
                                            ImprovementManager.SetForcedValue(_strForced, _objCharacter);
                                        else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                            ImprovementManager.SetForcedValue(_strExtra, _objCharacter);
                                        await ImprovementManager.CreateImprovementsAsync(
                                            _objCharacter, await GetSourceTypeAsync(token).ConfigureAwait(false),
                                            InternalId + "Pair",
                                            PairBonus, await GetRatingAsync(token).ConfigureAwait(false),
                                            await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                            token: token).ConfigureAwait(false);
                                    }
                                }

                                if (!await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false) ||
                                    ParentVehicle != null)
                                    await ChangeModularEquipAsync(false, token: token).ConfigureAwait(false);
                                else
                                    await RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
                            }
                        }

                        foreach (KeyValuePair<INotifyMultiplePropertiesChangedAsync, HashSet<string>> kvpToProcess in
                                 dicChangedProperties)
                        {
                            await kvpToProcess.Key.OnMultiplePropertiesChangedAsync(kvpToProcess.Value.ToList(), token)
                                .ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        List<HashSet<string>> lstToReturn = dicChangedProperties.Values.ToList();
                        for (int i = lstToReturn.Count - 1; i >= 0; --i)
                        {
                            HashSet<string> setLoop = lstToReturn[i];
                            Utils.StringHashSetPool.Return(ref setLoop);
                        }
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Minimum Rating.
        /// </summary>
        public int MinRating
        {
            get
            {
                int intReturn = 0;
                using (LockObject.EnterReadLock())
                {
                    string strRating = MinRatingString;

                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strRating)
                        && !int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intReturn))
                    {
                        Dictionary<string, int> dicVehicleValues = null;
                        if (strRating.Contains("imum}"))
                        {
                            if (ParentVehicle != null)
                            {
                                int intVehicleBody = ParentVehicle.TotalBody;
                                int intVehiclePilot = ParentVehicle.Pilot;
                                dicVehicleValues = new Dictionary<string, int>(4)
                                {
                                    { "STRMaximum", Math.Max(1, intVehicleBody * 2) },
                                    { "AGIMaximum", Math.Max(1, intVehiclePilot * 2) },
                                    { "STRMinimum", Math.Max(1, intVehicleBody) },
                                    { "AGIMinimum", Math.Max(1, intVehiclePilot) }
                                };
                            }
                            else if (Category == "Cyberlimb" || IsLimb)
                            {
                                dicVehicleValues = new Dictionary<string, int>(2)
                                {
                                    { "STRMinimum", MinStrength },
                                    { "AGIMinimum", MinAgility }
                                };
                            }
                            else
                            {
                                Cyberware objLoop = Parent;
                                while (objLoop != null && objLoop.Category != "Cyberlimb" && !objLoop.IsLimb)
                                    objLoop = objLoop.Parent;
                                if (objLoop != null)
                                {
                                    dicVehicleValues = new Dictionary<string, int>(2)
                                    {
                                        { "STRMinimum", objLoop.MinStrength },
                                        { "AGIMinimum", objLoop.MinAgility }
                                    };
                                }
                            }
                        }

                        strRating = _objCharacter.AttributeSection.ProcessAttributesInXPath(strRating, dicVehicleValues);

                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strRating);
                        if (blnIsSuccess)
                            intReturn = ((double) objProcess).StandardRound();
                    }
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Total Minimum Rating.
        /// </summary>
        public async Task<int> GetMinRatingAsync(CancellationToken token = default)
        {
            int intReturn = 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strRating = MinRatingString;

                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strRating)
                    && !int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intReturn))
                {
                    Dictionary<string, int> dicVehicleValues = null;
                    if (strRating.Contains("imum}"))
                    {
                        Vehicle objParentVehicle = await GetParentVehicleAsync(token).ConfigureAwait(false);
                        if (objParentVehicle != null)
                        {
                            int intVehicleBody = await objParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                            int intVehiclePilot = await objParentVehicle.GetPilotAsync(token).ConfigureAwait(false);
                            dicVehicleValues = new Dictionary<string, int>(4)
                            {
                                { "STRMaximum", Math.Max(1, intVehicleBody * 2) },
                                { "AGIMaximum", Math.Max(1, intVehiclePilot * 2) },
                                { "STRMinimum", Math.Max(1, intVehicleBody) },
                                { "AGIMinimum", Math.Max(1, intVehiclePilot) }
                            };
                        }
                        else
                        {
                            if (Category == "Cyberlimb" || await GetIsLimbAsync(token).ConfigureAwait(false))
                            {
                                dicVehicleValues = new Dictionary<string, int>(2)
                                {
                                    { "STRMinimum", await GetMinStrengthAsync(token).ConfigureAwait(false) },
                                    { "AGIMinimum", await GetMinAgilityAsync(token).ConfigureAwait(false) }
                                };
                            }
                            else
                            {
                                Cyberware objLoop = await GetParentAsync(token).ConfigureAwait(false);
                                while (objLoop != null && objLoop.Category != "Cyberlimb" && !await objLoop.GetIsLimbAsync(token).ConfigureAwait(false))
                                    objLoop = await objLoop.GetParentAsync(token).ConfigureAwait(false);
                                if (objLoop != null)
                                {
                                    dicVehicleValues = new Dictionary<string, int>(2)
                                    {
                                        { "STRMinimum", await objLoop.GetMinStrengthAsync(token).ConfigureAwait(false) },
                                        { "AGIMinimum", await objLoop.GetMinAgilityAsync(token).ConfigureAwait(false) }
                                    };
                                }
                            }
                        }
                    }

                    strRating = await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(strRating, dicVehicleValues, token).ConfigureAwait(false);

                    (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                   .EvaluateInvariantXPathAsync(strRating, token)
                                                                   .ConfigureAwait(false);
                    if (blnIsSuccess)
                        intReturn = ((double) objProcess).StandardRound();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return intReturn;
        }

        /// <summary>
        /// String representing minimum rating before it would be computed.
        /// </summary>
        public string MinRatingString
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strMinRating;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strMinRating = value;
            }
        }

        /// <summary>
        /// Total Maximum Rating.
        /// </summary>
        public int MaxRating
        {
            get
            {
                int intReturn = 0;
                using (LockObject.EnterReadLock())
                {
                    string strRating = MaxRatingString;

                    // Not a simple integer, so we need to start mucking around with strings
                    if (!string.IsNullOrEmpty(strRating)
                        && !int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                         out intReturn))
                    {
                        Dictionary<string, int> dicVehicleValues = null;
                        if (strRating.Contains("imum}"))
                        {
                            if (ParentVehicle != null)
                            {
                                int intVehicleBody = ParentVehicle.TotalBody;
                                int intVehiclePilot = ParentVehicle.Pilot;
                                dicVehicleValues = new Dictionary<string, int>(4)
                                {
                                    { "STRMaximum", Math.Max(1, intVehicleBody * 2) },
                                    { "AGIMaximum", Math.Max(1, intVehiclePilot * 2) },
                                    { "STRMinimum", Math.Max(1, intVehicleBody) },
                                    { "AGIMinimum", Math.Max(1, intVehiclePilot) }
                                };
                            }
                            else if (Category == "Cyberlimb" || IsLimb)
                            {
                                dicVehicleValues = new Dictionary<string, int>(2)
                                {
                                    { "STRMinimum", MinStrength },
                                    { "AGIMinimum", MinAgility }
                                };
                            }
                            else
                            {
                                Cyberware objLoop = Parent;
                                while (objLoop != null && objLoop.Category != "Cyberlimb" && !objLoop.IsLimb)
                                    objLoop = objLoop.Parent;
                                if (objLoop != null)
                                {
                                    dicVehicleValues = new Dictionary<string, int>(2)
                                    {
                                        { "STRMinimum", objLoop.MinStrength },
                                        { "AGIMinimum", objLoop.MinAgility }
                                    };
                                }
                            }
                        }

                        strRating = _objCharacter.AttributeSection.ProcessAttributesInXPath(strRating, dicVehicleValues);

                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strRating);
                        if (blnIsSuccess)
                            intReturn = ((double) objProcess).StandardRound();
                    }
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Total Maximum Rating.
        /// </summary>
        public async Task<int> GetMaxRatingAsync(CancellationToken token = default)
        {
            int intReturn = 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strRating = MaxRatingString;

                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strRating)
                    && !int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intReturn))
                {
                    Dictionary<string, int> dicVehicleValues = null;
                    if (strRating.Contains("imum}"))
                    {
                        Vehicle objParentVehicle = await GetParentVehicleAsync(token).ConfigureAwait(false);
                        if (objParentVehicle != null)
                        {
                            int intVehicleBody = await objParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                            int intVehiclePilot = await objParentVehicle.GetPilotAsync(token).ConfigureAwait(false);
                            dicVehicleValues = new Dictionary<string, int>(4)
                            {
                                { "STRMaximum", Math.Max(1, intVehicleBody * 2) },
                                { "AGIMaximum", Math.Max(1, intVehiclePilot * 2) },
                                { "STRMinimum", Math.Max(1, intVehicleBody) },
                                { "AGIMinimum", Math.Max(1, intVehiclePilot) }
                            };
                        }
                        else
                        {
                            if (Category == "Cyberlimb" || await GetIsLimbAsync(token).ConfigureAwait(false))
                            {
                                dicVehicleValues = new Dictionary<string, int>(2)
                                {
                                    { "STRMinimum", await GetMinStrengthAsync(token).ConfigureAwait(false) },
                                    { "AGIMinimum", await GetMinAgilityAsync(token).ConfigureAwait(false) }
                                };
                            }
                            else
                            {
                                Cyberware objLoop = await GetParentAsync(token).ConfigureAwait(false);
                                while (objLoop != null && objLoop.Category != "Cyberlimb" && !await objLoop.GetIsLimbAsync(token).ConfigureAwait(false))
                                    objLoop = await objLoop.GetParentAsync(token).ConfigureAwait(false);
                                if (objLoop != null)
                                {
                                    dicVehicleValues = new Dictionary<string, int>(2)
                                    {
                                        { "STRMinimum", await objLoop.GetMinStrengthAsync(token).ConfigureAwait(false) },
                                        { "AGIMinimum", await objLoop.GetMinAgilityAsync(token).ConfigureAwait(false) }
                                    };
                                }
                            }
                        }
                    }

                    strRating = await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(strRating, dicVehicleValues, token).ConfigureAwait(false);

                    (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                                   .EvaluateInvariantXPathAsync(strRating, token)
                                                                   .ConfigureAwait(false);
                    if (blnIsSuccess)
                        intReturn = ((double) objProcess).StandardRound();
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return intReturn;
        }

        /// <summary>
        /// String representing maximum rating before it would be computed.
        /// </summary>
        public string MaxRatingString
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strMaxRating;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strMaxRating = value;
            }
        }

        public string RatingLabel
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strRatingLabel;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strRatingLabel = value;
            }
        }

        /// <summary>
        /// Grade level of the Cyberware.
        /// </summary>
        public Grade Grade
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (!string.IsNullOrWhiteSpace(ForceGrade) && ForceGrade != _objGrade.Name)
                    {
                        return Grade.ConvertToCyberwareGrade(ForceGrade, SourceType, _objCharacter);
                    }

                    return _objGrade;
                }
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    Grade objOldGrade = Interlocked.Exchange(ref _objGrade, value);
                    if (objOldGrade == value)
                        return;
                    bool blnGradeEssenceChanged = value == null || objOldGrade.Essence != value.Essence;
                    // Run through all of the child pieces and make sure their Grade matches.
                    foreach (Cyberware objChild in Children.AsEnumerableWithSideEffects())
                    {
                        //Ignore child pieces that have a forcegrade specified.
                        //Generally expected to be items with <forcegrade>None</forcegrade>
                        //TODO: This might need a handler for deeper-nested children
                        if (!string.IsNullOrWhiteSpace(objChild.ForceGrade)) continue;
                        int intMyProcessPropertyChanges = _intProcessPropertyChanges;
                        int intOldChildProcessPropertyChanges
                            = Interlocked.Exchange(ref objChild._intProcessPropertyChanges, _intProcessPropertyChanges);
                        try
                        {
                            objChild.Grade = value;
                        }
                        finally
                        {
                            Interlocked.CompareExchange(ref objChild._intProcessPropertyChanges,
                                                        intOldChildProcessPropertyChanges, intMyProcessPropertyChanges);
                        }
                    }

                    if (blnGradeEssenceChanged)
                        DoPropertyChanges(false, true);
                }
            }
        }

        /// <summary>
        /// Grade level of the Cyberware.
        /// </summary>
        public async Task<Grade> GetGradeAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrWhiteSpace(ForceGrade) && ForceGrade != _objGrade.Name)
                {
                    return await Grade.ConvertToCyberwareGradeAsync(ForceGrade, await GetSourceTypeAsync(token).ConfigureAwait(false), _objCharacter, token).ConfigureAwait(false);
                }

                return _objGrade;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Grade level of the Cyberware.
        /// </summary>
        public async Task SetGradeAsync(Grade value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Grade objOldGrade = Interlocked.Exchange(ref _objGrade, value);
                if (objOldGrade == value)
                    return;
                bool blnGradeEssenceChanged = value == null || objOldGrade.Essence != value.Essence;
                // Run through all of the child pieces and make sure their Grade matches.
                await Children.ForEachWithSideEffectsAsync(async objChild =>
                {
                    //Ignore child pieces that have a forcegrade specified.
                    //Generally expected to be items with <forcegrade>None</forcegrade>
                    //TODO: This might need a handler for deeper-nested children
                    if (!string.IsNullOrWhiteSpace(objChild.ForceGrade))
                        return;
                    int intMyProcessPropertyChanges = _intProcessPropertyChanges;
                    int intOldChildProcessPropertyChanges
                        = Interlocked.Exchange(ref objChild._intProcessPropertyChanges, _intProcessPropertyChanges);
                    try
                    {
                        await objChild.SetGradeAsync(value, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.CompareExchange(ref objChild._intProcessPropertyChanges,
                            intOldChildProcessPropertyChanges, intMyProcessPropertyChanges);
                    }
                }, token).ConfigureAwait(false);

                if (blnGradeEssenceChanged)
                    await DoPropertyChangesAsync(false, true, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The Categories of allowable Subsystems.
        /// </summary>
        public string AllowedSubsystems
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strAllowSubsystems;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strAllowSubsystems = value;
            }
        }

        /// <summary>
        /// Whether the piece of Cyberware is part of a Cyberware Suite.
        /// </summary>
        public bool Suite
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnSuite;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnSuite = value;
            }
        }

        /// <summary>
        /// Essence cost discount.
        /// </summary>
        public int ESSDiscount
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intEssenceDiscount;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_intEssenceDiscount == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (Interlocked.Exchange(ref _intEssenceDiscount, value) != value
                        && (Parent == null || AddToParentESS)
                        && string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Essence cost discount.
        /// </summary>
        public async Task<int> GetESSDiscountAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _intEssenceDiscount;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Essence cost discount.
        /// </summary>
        public async Task SetESSDiscountAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_intEssenceDiscount == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Interlocked.Exchange(ref _intEssenceDiscount, value) != value
                    && (await GetParentAsync(token).ConfigureAwait(false) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false))
                    && string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) && await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                    await _objCharacter.OnPropertyChangedAsync(await GetEssencePropertyNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (additively stacking, starts at 0).
        /// </summary>
        public decimal ExtraESSAdditiveMultiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decExtraESSAdditiveMultiplier;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decExtraESSAdditiveMultiplier == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decExtraESSAdditiveMultiplier == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decExtraESSAdditiveMultiplier = value;
                        if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                            ParentVehicle == null)
                            _objCharacter.OnPropertyChanged(EssencePropertyName);
                    }
                }
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (additively stacking, starts at 0).
        /// </summary>
        public async Task<decimal> GetExtraESSAdditiveMultiplierAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decExtraESSAdditiveMultiplier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (additively stacking, starts at 0).
        /// </summary>
        public async Task SetExtraESSAdditiveMultiplierAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decExtraESSAdditiveMultiplier == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decExtraESSAdditiveMultiplier == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    _decExtraESSAdditiveMultiplier = value;
                    if ((await GetParentAsync(token).ConfigureAwait(false) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) && string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                        await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                        await _objCharacter.OnPropertyChangedAsync(await GetEssencePropertyNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
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
        /// Extra Essence cost multiplier (multiplicative stacking, starts at 1).
        /// </summary>
        public decimal ExtraESSMultiplicativeMultiplier
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _decExtraESSMultiplicativeMultiplier;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_decExtraESSMultiplicativeMultiplier == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_decExtraESSMultiplicativeMultiplier == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _decExtraESSMultiplicativeMultiplier = value;
                        if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                            ParentVehicle == null)
                            _objCharacter.OnPropertyChanged(EssencePropertyName);
                    }
                }
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (multiplicative stacking, starts at 1).
        /// </summary>
        public async Task<decimal> GetExtraESSMultiplicativeMultiplierAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _decExtraESSMultiplicativeMultiplier;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (multiplicative stacking, starts at 1).
        /// </summary>
        public async Task SetExtraESSMultiplicativeMultiplierAsync(decimal value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decExtraESSMultiplicativeMultiplier == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_decExtraESSMultiplicativeMultiplier == value)
                    return;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    _decExtraESSMultiplicativeMultiplier = value;
                    if ((await GetParentAsync(token).ConfigureAwait(false) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) && string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                        await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                        await _objCharacter.OnPropertyChangedAsync(await GetEssencePropertyNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
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

        public void SaveNonRetroactiveEssenceModifiers()
        {
            using (LockObject.EnterReadLock())
            {
                if (this.GetNodeXPath()?.SelectSingleNodeAndCacheExpression("forcegrade")?.Value == "None")
                    return;
            }

            using (LockObject.EnterUpgradeableReadLock())
            {
                if (this.GetNodeXPath()?.SelectSingleNodeAndCacheExpression("forcegrade")?.Value == "None")
                    return;
                decimal decOldExtraESSAdditiveMultiplier = _decExtraESSAdditiveMultiplier;
                decimal decOldExtraESSMultiplicativeMultiplier = _decExtraESSMultiplicativeMultiplier;
                using (LockObject.EnterWriteLock())
                {
                    _decExtraESSAdditiveMultiplier = 0;
                    _decExtraESSMultiplicativeMultiplier = 1;
                    switch (_eImprovementSource)
                    {
                        // Apply the character's Cyberware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Cyberware:
                        {
                            List<Improvement> lstUsedImprovements =
                                ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.CyberwareEssCostNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decimal decMultiplier = 1;
                                decMultiplier = lstUsedImprovements.Aggregate(decMultiplier,
                                    (current, objImprovement) =>
                                        current - (1m - objImprovement.Value
                                            / 100m));
                                _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                            }

                            List<Improvement> lstUsedImprovements2 =
                                ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive);
                            if (lstUsedImprovements2.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements2)
                                {
                                    _decExtraESSMultiplicativeMultiplier *=
                                        objImprovement.Value / 100m;
                                }
                            }

                            break;
                        }
                        // Apply the character's Bioware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Bioware:
                        {
                            List<Improvement> lstUsedImprovements =
                                ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.BiowareEssCostNonRetroactive);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decimal decMultiplier = 1;
                                decMultiplier = lstUsedImprovements.Aggregate(decMultiplier,
                                    (current, objImprovement) =>
                                        current - (1m - objImprovement.Value
                                            / 100m));
                                _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                            }

                            List<Improvement> lstUsedImprovements2 =
                                ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive);
                            if (lstUsedImprovements2.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements2)
                                {
                                    _decExtraESSMultiplicativeMultiplier *=
                                        objImprovement.Value / 100m;
                                }
                            }

                            break;
                        }
                    }
                    if (decOldExtraESSAdditiveMultiplier != _decExtraESSAdditiveMultiplier
                        || decOldExtraESSMultiplicativeMultiplier != _decExtraESSMultiplicativeMultiplier)
                    {
                        if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                            _objCharacter.OnPropertyChanged(EssencePropertyName);
                    }
                }
            }
        }

        public async Task SaveNonRetroactiveEssenceModifiersAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                if (objNode?.SelectSingleNodeAndCacheExpression("forcegrade", token)?.Value == "None")
                {
                    return;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objNode = await this.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                if (objNode?.SelectSingleNodeAndCacheExpression("forcegrade", token)?.Value == "None")
                {
                    return;
                }

                decimal decOldExtraESSAdditiveMultiplier = _decExtraESSAdditiveMultiplier;
                decimal decOldExtraESSMultiplicativeMultiplier = _decExtraESSMultiplicativeMultiplier;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _decExtraESSAdditiveMultiplier = 0;
                    _decExtraESSMultiplicativeMultiplier = 1;
                    switch (_eImprovementSource)
                    {
                        // Apply the character's Cyberware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Cyberware:
                        {
                            List<Improvement> lstUsedImprovements =
                                await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                                        Improvement.ImprovementType
                                            .CyberwareEssCostNonRetroactive,
                                        token: token)
                                    .ConfigureAwait(false);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decimal decMultiplier = 1;
                                decMultiplier = lstUsedImprovements.Aggregate(decMultiplier,
                                    (current, objImprovement) =>
                                        current - (1m - objImprovement.Value
                                            / 100m));
                                _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                            }

                            List<Improvement> lstUsedImprovements2 =
                                await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                                        Improvement.ImprovementType
                                            .CyberwareTotalEssMultiplierNonRetroactive,
                                        token: token)
                                    .ConfigureAwait(false);
                            if (lstUsedImprovements2.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements2)
                                {
                                    _decExtraESSMultiplicativeMultiplier *=
                                        objImprovement.Value / 100m;
                                }
                            }

                            break;
                        }
                        // Apply the character's Bioware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Bioware:
                        {
                            List<Improvement> lstUsedImprovements =
                                await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                                        Improvement.ImprovementType
                                            .BiowareEssCostNonRetroactive,
                                        token: token)
                                    .ConfigureAwait(false);
                            if (lstUsedImprovements.Count != 0)
                            {
                                decimal decMultiplier = 1;
                                decMultiplier = lstUsedImprovements.Aggregate(decMultiplier,
                                    (current, objImprovement) =>
                                        current - (1m - objImprovement.Value
                                            / 100m));
                                _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                            }

                            List<Improvement> lstUsedImprovements2 =
                                await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                                        Improvement.ImprovementType
                                            .BiowareTotalEssMultiplierNonRetroactive,
                                        token: token)
                                    .ConfigureAwait(false);
                            if (lstUsedImprovements2.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements2)
                                {
                                    _decExtraESSMultiplicativeMultiplier *=
                                        objImprovement.Value / 100m;
                                }
                            }

                            break;
                        }
                    }

                    if (decOldExtraESSAdditiveMultiplier != _decExtraESSAdditiveMultiplier
                        || decOldExtraESSMultiplicativeMultiplier != _decExtraESSMultiplicativeMultiplier)
                    {
                        if ((await GetParentAsync(token).ConfigureAwait(false) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false))
                            && string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                            await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                            await _objCharacter.OnPropertyChangedAsync(await GetEssencePropertyNameAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
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
        }

        /// <summary>
        /// Base Physical Boxes. 12 for vehicles, 6 for Drones.
        /// </summary>
        public int BaseMatrixBoxes => 8;

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return BaseMatrixBoxes + (this.GetTotalMatrixAttribute("Device Rating") + 1) / 2 +
                           TotalBonusMatrixBoxes;
                }
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes filled.
        /// </summary>
        public int MatrixCMFilled
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMatrixCMFilled;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _intMatrixCMFilled = value;
            }
        }

        /// <summary>
        /// A List of child pieces of Cyberware.
        /// </summary>
        public TaggedObservableCollection<Cyberware> Children
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstChildren;
            }
        }

        /// <summary>
        /// A List of the Gear attached to the Cyberware.
        /// </summary>
        public TaggedObservableCollection<Gear> GearChildren
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstGear;
            }
        }

        /// <summary>
        /// List of names to include in pair bonus
        /// </summary>
        public HashSet<string> IncludePair
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstIncludeInPairBonus;
            }
        }

        /// <summary>
        /// List of names to include in pair bonus
        /// </summary>
        public HashSet<string> IncludeWirelessPair
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _lstIncludeInWirelessPairBonus;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_strNotes == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_strNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _strNotes = value;
                }
            }
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _colNotes;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_colNotes == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_colNotes == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _colNotes = value;
                }
            }
        }

        /// <summary>
        /// Whether the Cyberware's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnDiscountCost;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnDiscountCost == value)
                        return;
                }

                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnDiscountCost == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnDiscountCost = value;
                }
            }
        }

        /// <summary>
        /// Whether the Cyberware's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public async Task<bool> GetDiscountCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnDiscountCost;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Cyberware's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public async Task SetDiscountCostAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDiscountCost == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnDiscountCost == value)
                    return;

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnDiscountCost = value;
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
        /// Whether the Cyberware's ESS cost increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public bool AddToParentESS
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAddToParentESS;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnAddToParentESS == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    bool blnOldValue = _blnAddToParentESS;
                    if (blnOldValue == value)
                        return;
                    using (LockObject.EnterWriteLock())
                    {
                        _blnAddToParentESS = value;
                        if ((Parent == null || AddToParentESS || blnOldValue) &&
                            string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                            _objCharacter.OnPropertyChanged(EssencePropertyName);
                    }
                }
            }
        }

        /// <summary>
        /// Whether the Cyberware's ESS cost increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public async Task<bool> GetAddToParentESSAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAddToParentESS;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the Cyberware's ESS cost increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public async Task SetAddToParentESSAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAddToParentESS == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAddToParentESS == value)
                    return;

                string strOldEssencePropertyName = await GetEssencePropertyNameAsync(token).ConfigureAwait(false);
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAddToParentESS = value;
                    if ((await GetParentAsync(token).ConfigureAwait(false) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) &&
                        string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                        await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                        await _objCharacter.OnMultiplePropertyChangedAsync(token, strOldEssencePropertyName,
                            await GetEssencePropertyNameAsync(token).ConfigureAwait(false)).ConfigureAwait(false);
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
        /// Whether the capacity increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public bool AddToParentCapacity
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnAddToParentCapacity;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (_blnAddToParentCapacity == value)
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnAddToParentCapacity == value)
                        return;
                    using (LockObject.EnterWriteLock())
                        _blnAddToParentCapacity = value;
                }
            }
        }

        /// <summary>
        /// Whether the capacity increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public async Task<bool> GetAddToParentCapacityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnAddToParentCapacity;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether the capacity increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public async Task SetAddToParentCapacityAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAddToParentCapacity == value)
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnAddToParentCapacity == value)
                    return;

                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    _blnAddToParentCapacity = value;
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
        /// Parent Cyberware.
        /// </summary>
        public Cyberware Parent
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objParent;
            }
            set
            {
                using (LockObject.EnterReadLock())
                {
                    if (ReferenceEquals(_objParent, value))
                        return;
                }
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (ReferenceEquals(_objParent, value))
                        return;
                    bool blnOldEquipped = IsModularCurrentlyEquipped;
                    using (LockObject.EnterReadLock())
                    {
                        if (ReferenceEquals(Interlocked.Exchange(ref _objParent, value), value))
                            return;
                        ParentVehicle = value?.ParentVehicle;
                        if (IsModularCurrentlyEquipped == blnOldEquipped)
                            return;
                        foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                        {
                            if (blnOldEquipped)
                                objGear.ChangeEquippedStatus(false);
                            else if (objGear.Equipped)
                                objGear.ChangeEquippedStatus(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Parent Cyberware.
        /// </summary>
        public async Task<Cyberware> GetParentAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objParent;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Parent Cyberware.
        /// </summary>
        public async Task SetParentAsync(Cyberware value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (ReferenceEquals(_objParent, value))
                    return;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (ReferenceEquals(_objParent, value))
                    return;
                bool blnOldEquipped = await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false);
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    if (ReferenceEquals(Interlocked.Exchange(ref _objParent, value), value))
                        return;
                    await SetParentVehicleAsync(value != null ? await value.GetParentVehicleAsync(token).ConfigureAwait(false) : null, token).ConfigureAwait(false);
                    if (await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false) == blnOldEquipped)
                        return;
                    await GearChildren.ForEachWithSideEffectsAsync(async objGear =>
                    {
                        if (blnOldEquipped)
                            await objGear.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                        else if (objGear.Equipped)
                            await objGear.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
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
        /// Topmost Parent Cyberware.
        /// </summary>
        public Cyberware TopMostParent
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    Cyberware objReturn = this;
                    Cyberware objParent = Parent;
                    while (objParent != null)
                    {
                        using (objParent.LockObject.EnterReadLock())
                        {
                            objReturn = objParent;
                            objParent = objParent.Parent;
                        }
                    }

                    return objReturn;
                }
            }
        }

        /// <summary>
        /// Topmost Parent Cyberware.
        /// </summary>
        public async Task<Cyberware> GetTopMostParentAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Cyberware objReturn = this;
                Cyberware objParent = await GetParentAsync(token).ConfigureAwait(false);
                while (objParent != null)
                {
                    IAsyncDisposable objLocker2 = await objParent.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        objReturn = objParent;
                        objParent = await objParent.GetParentAsync(token).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }

                return objReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Vehicle to which this cyberware is attached (if any)
        /// </summary>
        public Vehicle ParentVehicle
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objParentVehicle;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_objParentVehicle != value)
                    {
                        using (LockObject.EnterWriteLock())
                        {
                            if (Interlocked.Exchange(ref _objParentVehicle, value) != value)
                            {
                                bool blnEquipped = IsModularCurrentlyEquipped;
                                foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                                {
                                    if (value != null)
                                        objGear.ChangeEquippedStatus(false);
                                    else if (objGear.Equipped && blnEquipped)
                                        objGear.ChangeEquippedStatus(true);
                                }
                            }
                        }
                    }

                    foreach (Cyberware objChild in Children.AsEnumerableWithSideEffects())
                        objChild.ParentVehicle = value;
                }
            }
        }

        /// <summary>
        /// Vehicle to which this cyberware is attached (if any)
        /// </summary>
        public async Task<Vehicle> GetParentVehicleAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _objParentVehicle;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Vehicle to which this cyberware is attached (if any)
        /// </summary>
        public async Task SetParentVehicleAsync(Vehicle value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_objParentVehicle != value)
                {
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        if (Interlocked.Exchange(ref _objParentVehicle, value) != value)
                        {
                            bool blnEquipped = await GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false);
                            await GearChildren.ForEachWithSideEffectsAsync(async objGear =>
                            {
                                if (value != null)
                                    await objGear.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                                else if (objGear.Equipped && blnEquipped)
                                    await objGear.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                            }, token).ConfigureAwait(false);
                        }
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }

                await Children.ForEachWithSideEffectsAsync(x => x.SetParentVehicleAsync(value, token), token)
                    .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Grade that the Cyberware should be forced to use, if applicable.
        /// </summary>
        public string ForceGrade
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strForceGrade;
            }
        }

        /// <summary>
        /// Is this cyberware/bioware geneware?
        /// </summary>
        public bool IsGeneware
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnIsGeneware;
            }
        }

        /// <summary>
        /// Is the Bioware's cost affected by Prototype Transhuman?
        /// </summary>
        public bool PrototypeTranshuman
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnPrototypeTranshuman;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                {
                    if (_blnPrototypeTranshuman != value)
                    {
                        string strOldEssencePropertyName = EssencePropertyName;
                        using (LockObject.EnterWriteLock())
                        {
                            _blnPrototypeTranshuman = value;
                            if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                                ParentVehicle == null)
                                _objCharacter.OnMultiplePropertyChanged(strOldEssencePropertyName, EssencePropertyName);
                        }
                    }

                    foreach (Cyberware objCyberware in Children.AsEnumerableWithSideEffects())
                        objCyberware.PrototypeTranshuman = value;
                }
            }
        }

        /// <summary>
        /// Is the Bioware's cost affected by Prototype Transhuman?
        /// </summary>
        public async Task<bool> GetPrototypeTranshumanAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _blnPrototypeTranshuman;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Is the Bioware's cost affected by Prototype Transhuman?
        /// </summary>
        public async Task SetPrototypeTranshumanAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_blnPrototypeTranshuman != value)
                {
                    string strOldEssencePropertyName = await GetEssencePropertyNameAsync(token).ConfigureAwait(false);
                    IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        _blnPrototypeTranshuman = value;
                        if ((await GetParentAsync(token).ConfigureAwait(false) == null || await GetAddToParentESSAsync(token).ConfigureAwait(false)) && string.IsNullOrEmpty(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)) &&
                            await GetParentVehicleAsync(token).ConfigureAwait(false) == null)
                            await _objCharacter.OnMultiplePropertyChangedAsync(token, strOldEssencePropertyName,
                                await GetEssencePropertyNameAsync(token).ConfigureAwait(false)).ConfigureAwait(false);
                    }
                    finally
                    {
                        await objLocker2.DisposeAsync().ConfigureAwait(false);
                    }
                }

                await Children.ForEachWithSideEffectsAsync(x => x.SetPrototypeTranshumanAsync(value, token), token)
                    .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public string EssencePropertyName
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (_objCharacter.IsPrototypeTranshuman && PrototypeTranshuman)
                        return nameof(Character.PrototypeTranshumanEssenceUsed);
                    if (SourceID.Equals(EssenceHoleGUID) || SourceID.Equals(EssenceAntiHoleGUID))
                        return nameof(Character.EssenceHole);
                    switch (SourceType)
                    {
                        case Improvement.ImprovementSource.Bioware:
                            return nameof(Character.BiowareEssence);

                        case Improvement.ImprovementSource.Cyberware:
                            return nameof(Character.CyberwareEssence);

                        default:
                            return nameof(Character.Essence);
                    }
                }
            }
        }

        public async Task<string> GetEssencePropertyNameAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await _objCharacter.GetIsPrototypeTranshumanAsync(token).ConfigureAwait(false) && await GetPrototypeTranshumanAsync(token).ConfigureAwait(false))
                    return nameof(Character.PrototypeTranshumanEssenceUsed);
                Guid guidSourceId = await GetSourceIDAsync(token).ConfigureAwait(false);
                if (guidSourceId.Equals(EssenceHoleGUID) || guidSourceId.Equals(EssenceAntiHoleGUID))
                    return nameof(Character.EssenceHole);
                switch (SourceType)
                {
                    case Improvement.ImprovementSource.Bioware:
                        return nameof(Character.BiowareEssence);

                    case Improvement.ImprovementSource.Cyberware:
                        return nameof(Character.CyberwareEssence);

                    default:
                        return nameof(Character.Essence);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XmlNode objReturn = _objCachedMyXmlNode;
                if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                string strDoc = "cyberware.xml";
                string strPath = "/chummer/cyberwares/cyberware";
                if (SourceType == Improvement.ImprovementSource.Bioware)
                {
                    strDoc = "bioware.xml";
                    strPath = "/chummer/biowares/bioware";
                }

                XmlDocument objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData(strDoc, strLanguage, token: token)
                    : await _objCharacter.LoadDataAsync(strDoc, strLanguage, token: token).ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById(strPath, SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId(strPath, Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                _objCachedMyXmlNode = objReturn;
                _strCachedXmlNodeLanguage = strLanguage;
                return objReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                // ReSharper disable once MethodHasAsyncOverload
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                XPathNavigator objReturn = _objCachedMyXPathNode;
                if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                      && !GlobalSettings.LiveCustomData)
                    return objReturn;
                string strDoc = "cyberware.xml";
                string strPath = "/chummer/cyberwares/cyberware";
                if (SourceType == Improvement.ImprovementSource.Bioware)
                {
                    strDoc = "bioware.xml";
                    strPath = "/chummer/biowares/bioware";
                }

                XPathNavigator objDoc = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadDataXPath(strDoc, strLanguage, token: token)
                    : await _objCharacter.LoadDataXPathAsync(strDoc, strLanguage, token: token).ConfigureAwait(false);
                if (SourceID != Guid.Empty)
                    objReturn = objDoc.TryGetNodeById(strPath, SourceID);
                if (objReturn == null)
                {
                    objReturn = objDoc.TryGetNodeByNameOrId(strPath, Name);
                    objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                }

                _objCachedMyXPathNode = objReturn;
                _strCachedXPathNodeLanguage = strLanguage;
                return objReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns true if this ware could modify AGI or STR
        /// </summary>
        public bool IsLimb
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return !string.IsNullOrWhiteSpace(LimbSlot)
                           || !string.IsNullOrEmpty(MountToLimbType(PlugsIntoModularMount))
                           || (InheritAttributes && Children.Any(objChild => objChild.IsLimb));
                }
            }
        }

        /// <summary>
        /// Returns true if this ware could modify AGI or STR
        /// </summary>
        public async Task<bool> GetIsLimbAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return !string.IsNullOrWhiteSpace(await GetLimbSlotAsync(token).ConfigureAwait(false))
                       || !string.IsNullOrEmpty(MountToLimbType(await GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)))
                       || (await GetInheritAttributesAsync(token).ConfigureAwait(false) && await Children
                           .AnyAsync(objChild => objChild.GetIsLimbAsync(token), token).ConfigureAwait(false));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public Task<string> GetDisplayTotalAvailAsync(CancellationToken token = default) => TotalAvailAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Total Availability of the Cyberware and its plugins.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage) => TotalAvailTuple().ToString(objCulture, strLanguage);

        /// <summary>
        /// Calculated Availability of the Vehicle.
        /// </summary>
        public async Task<string> TotalAvailAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return await (await TotalAvailTupleAsync(token: token).ConfigureAwait(false)).ToStringAsync(objCulture, strLanguage, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple(bool blnCheckChildren = true)
        {
            using (LockObject.EnterReadLock())
            {
                bool blnModifyParentAvail = false;
                string strAvail = Avail;
                char chrLastAvailChar = ' ';
                int intAvail = Grade.Avail;
                bool blnOrGear = false;
                if (strAvail.Length > 0)
                {
                    if (strAvail.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                     .Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                    }

                    blnOrGear = strAvail.EndsWith(" or Gear", StringComparison.Ordinal);
                    if (blnOrGear)
                        strAvail = strAvail.TrimEndOnce(" or Gear", true);

                    chrLastAvailChar = strAvail[strAvail.Length - 1];
                    if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                    {
                        strAvail = strAvail.Substring(0, strAvail.Length - 1);
                    }

                    blnModifyParentAvail = strAvail.StartsWith('+', '-');
                    if (blnModifyParentAvail)
                        intAvail = 0;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));
                        sbdAvail.CheapReplace(strAvail, "MinRating",
                                              () => MinRating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdAvail.CheapReplace(strAvail, "Rating",
                                              () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                        _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdAvail, strAvail);
                        (bool blnIsSuccess, object objProcess)
                            = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString());
                        if (blnIsSuccess)
                            intAvail += ((double) objProcess).StandardRound();
                    }
                }

                if (blnCheckChildren)
                {
                    // Run through cyberware children and increase the Avail by any installed Mod whose Avail starts with "+" or "-".
                    foreach (Cyberware objChild in Children)
                    {
                        if (objChild.ParentID == InternalId ||
                            !objChild.IsModularCurrentlyEquipped &&
                            !string.IsNullOrEmpty(objChild.PlugsIntoModularMount))
                            continue;
                        AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }

                int intLoopAvail = 0;
                // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Gear objChild in GearChildren)
                {
                    if (objChild.ParentID != InternalId)
                    {
                        AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                        if (!objLoopAvailTuple.AddToParent)
                            intLoopAvail = Math.Max(intLoopAvail, objLoopAvailTuple.Value);
                        if (blnCheckChildren)
                        {
                            if (objLoopAvailTuple.AddToParent)
                                intAvail += objLoopAvailTuple.Value;
                            if (objLoopAvailTuple.Suffix == 'F')
                                chrLastAvailChar = 'F';
                            else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                                chrLastAvailChar = 'R';
                        }
                        else if (blnOrGear)
                        {
                            if (objLoopAvailTuple.Suffix == 'F')
                                chrLastAvailChar = 'F';
                            else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                                chrLastAvailChar = 'R';
                        }
                    }
                }

                // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
                if (intAvail < 0)
                    intAvail = 0;

                if (blnOrGear && intLoopAvail > intAvail)
                    intAvail = intLoopAvail;

                return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
            }
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async Task<AvailabilityValue> TotalAvailTupleAsync(bool blnCheckChildren = true, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                bool blnModifyParentAvail = false;
                string strAvail = Avail;
                char chrLastAvailChar = ' ';
                int intAvail = (await GetGradeAsync(token).ConfigureAwait(false)).Avail;
                bool blnOrGear = false;
                if (strAvail.Length > 0)
                {
                    if (strAvail.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                     .Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strAvail = strValues[Math.Max(Math.Min(await GetRatingAsync(token).ConfigureAwait(false), strValues.Length) - 1, 0)];
                    }

                    blnOrGear = strAvail.EndsWith(" or Gear", StringComparison.Ordinal);
                    if (blnOrGear)
                        strAvail = strAvail.TrimEndOnce(" or Gear", true);

                    chrLastAvailChar = strAvail[strAvail.Length - 1];
                    if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                    {
                        strAvail = strAvail.Substring(0, strAvail.Length - 1);
                    }

                    blnModifyParentAvail = strAvail.StartsWith('+', '-');
                    if (blnModifyParentAvail)
                        intAvail = 0;
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));
                        await sbdAvail.CheapReplaceAsync(strAvail, "MinRating",
                                                         async () => (await GetMinRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        await sbdAvail.CheapReplaceAsync(strAvail, "Rating",
                                                         async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdAvail, strAvail, token: token).ConfigureAwait(false);
                        (bool blnIsSuccess, object objProcess)
                            = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                        if (blnIsSuccess)
                            intAvail += ((double)objProcess).StandardRound();
                    }
                }

                if (blnCheckChildren)
                {
                    // Run through cyberware children and increase the Avail by any installed Mod whose Avail starts with "+" or "-".
                    intAvail += await Children.SumAsync(async objChild =>
                    {
                        if (objChild.ParentID == InternalId ||
                            !await objChild.GetIsModularCurrentlyEquippedAsync(token).ConfigureAwait(false) &&
                            !string.IsNullOrEmpty(await objChild.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)))
                            return 0;
                        AvailabilityValue objLoopAvailTuple = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                        return objLoopAvailTuple.AddToParent ? objLoopAvailTuple.Value : 0;
                    }, token).ConfigureAwait(false);
                }

                int intLoopAvail = 0;
                // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                intAvail += await GearChildren.SumAsync(async objChild =>
                {
                    if (objChild.ParentID == InternalId)
                        return 0;
                    AvailabilityValue objLoopAvailTuple =
                        await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (!objLoopAvailTuple.AddToParent)
                        intLoopAvail = Math.Max(intLoopAvail, objLoopAvailTuple.Value);
                    if (blnCheckChildren)
                    {
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                        return objLoopAvailTuple.AddToParent ? objLoopAvailTuple.Value : 0;
                    }

                    if (blnOrGear)
                    {
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }

                    return 0;
                }, token).ConfigureAwait(false);

                // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
                if (intAvail < 0)
                    intAvail = 0;

                if (blnOrGear && intLoopAvail > intAvail)
                    intAvail = intLoopAvail;

                return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calculated Capacity of the Cyberware.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    string strCapacity = Capacity;
                    if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                    {
                        string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                        strCapacity = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                    }

                    if (string.IsNullOrEmpty(strCapacity))
                        return 0.0m.ToString("#,0.##", GlobalSettings.CultureInfo);
                    if (strCapacity == "[*]")
                        return "*";
                    string strReturn;
                    int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                    if (intPos != -1)
                    {
                        string strFirstHalf = strCapacity.Substring(0, intPos);
                        string strSecondHalf = strCapacity.Substring(intPos + 1, strCapacity.Length - intPos - 1);
                        bool blnSquareBrackets = strFirstHalf.StartsWith('[');

                        if (blnSquareBrackets && strFirstHalf.Length > 2)
                            strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                        try
                        {
                            (bool blnIsSuccess, object objProcess) =
                                CommonFunctions.EvaluateInvariantXPath(
                                    strFirstHalf.Replace(
                                        "Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)));
                            strReturn = blnIsSuccess
                                ? ((double) objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                                : strFirstHalf;
                        }
                        catch (OverflowException) // Result is text and not a double
                        {
                            strReturn = strFirstHalf;
                        }
                        catch (InvalidCastException) // Result is text and not a double
                        {
                            strReturn = strFirstHalf;
                        }

                        if (blnSquareBrackets)
                            strReturn = '[' + strCapacity + ']';

                        strSecondHalf = strSecondHalf.Trim('[', ']');
                        if (Children.Any(x => x.AddToParentCapacity))
                        {
                            // Run through its Children and deduct the Capacity costs.
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                          out StringBuilder sbdSecondHalf))
                            {
                                foreach (Cyberware objChildCyberware in Children.Where(
                                             objChild => objChild.AddToParentCapacity))
                                {
                                    if (objChildCyberware.ParentID == InternalId)
                                    {
                                        continue;
                                    }

                                    string strLoopCapacity = objChildCyberware.CalculatedCapacity;
                                    int intLoopPos = strLoopCapacity.IndexOf("/[", StringComparison.Ordinal);
                                    if (intLoopPos != -1)
                                        strLoopCapacity = strLoopCapacity.Substring(intLoopPos + 2,
                                            strLoopCapacity.LastIndexOf(']') - intLoopPos - 2);
                                    else if (strLoopCapacity.StartsWith('['))
                                        strLoopCapacity = strLoopCapacity.Substring(1, strLoopCapacity.Length - 2);
                                    if (strLoopCapacity == "*")
                                        strLoopCapacity = "0";
                                    sbdSecondHalf.Append("+(").Append(strLoopCapacity).Append(')');
                                }

                                strSecondHalf += sbdSecondHalf.ToString();
                            }
                        }

                        try
                        {
                            (bool blnIsSuccess, object objProcess) =
                                CommonFunctions.EvaluateInvariantXPath(
                                    strSecondHalf.Replace(
                                        "Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)));
                            strSecondHalf =
                                '[' + (blnIsSuccess
                                    ? ((double) objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                                    : strSecondHalf) + ']';
                        }
                        catch (OverflowException) // Result is text and not a double
                        {
                            strSecondHalf = '[' + strSecondHalf + ']';
                        }
                        catch (InvalidCastException) // Result is text and not a double
                        {
                            strSecondHalf = '[' + strSecondHalf + ']';
                        }

                        strReturn += '/' + strSecondHalf;
                    }
                    else if (strCapacity.Contains("Rating") ||
                             (strCapacity.StartsWith('[') && Children.Any(x => x.AddToParentCapacity)))
                    {
                        // If the Capacity is determined by the Rating, evaluate the expression.
                        // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                        bool blnSquareBrackets = strCapacity.StartsWith('[');
                        if (blnSquareBrackets)
                        {
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                            if (Children.Any(x => x.AddToParentCapacity))
                            {
                                // Run through its Children and deduct the Capacity costs.
                                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                              out StringBuilder sbdCapacity))
                                {
                                    foreach (Cyberware objChildCyberware in Children.Where(objChild =>
                                                 objChild.AddToParentCapacity))
                                    {
                                        if (objChildCyberware.ParentID == InternalId)
                                        {
                                            continue;
                                        }

                                        string strLoopCapacity = objChildCyberware.CalculatedCapacity;
                                        int intLoopPos = strLoopCapacity.IndexOf("/[", StringComparison.Ordinal);
                                        if (intLoopPos != -1)
                                            strLoopCapacity = strLoopCapacity.Substring(intLoopPos + 2,
                                                strLoopCapacity.LastIndexOf(']') - intLoopPos - 2);
                                        else if (strLoopCapacity.StartsWith('['))
                                            strLoopCapacity = strLoopCapacity.Substring(1, strLoopCapacity.Length - 2);
                                        if (strLoopCapacity == "*")
                                            strLoopCapacity = "0";
                                        sbdCapacity.Append("+(").Append(strLoopCapacity).Append(')');
                                    }

                                    strCapacity += sbdCapacity.ToString();
                                }
                            }
                        }

                        (bool blnIsSuccess, object objProcess) =
                            CommonFunctions.EvaluateInvariantXPath(
                                strCapacity.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)));
                        strReturn = blnIsSuccess
                            ? ((double) objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                            : strCapacity;
                        if (blnSquareBrackets)
                            strReturn = '[' + strReturn + ']';
                    }
                    else if (decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                              out decimal decReturn))
                        return decReturn.ToString("#,0.##", GlobalSettings.CultureInfo);
                    else
                    {
                        // Just a straight Capacity, so return the value.
                        return strCapacity;
                    }

                    return strReturn;
                }
            }
        }

        /// <summary>
        /// Calculated Capacity of the Cyberware.
        /// </summary>
        public async Task<string> GetCalculatedCapacityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strCapacity = Capacity;
                if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCapacity = strValues[Math.Max(Math.Min(await GetRatingAsync(token).ConfigureAwait(false), strValues.Length) - 1, 0)];
                }

                if (string.IsNullOrEmpty(strCapacity))
                    return 0.0m.ToString("#,0.##", GlobalSettings.CultureInfo);
                if (strCapacity == "[*]")
                    return "*";
                string strReturn;
                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    string strFirstHalf = strCapacity.Substring(0, intPos);
                    string strSecondHalf = strCapacity.Substring(intPos + 1, strCapacity.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.StartsWith('[');

                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                    try
                    {
                        (bool blnIsSuccess, object objProcess) =
                            await CommonFunctions.EvaluateInvariantXPathAsync(
                                strFirstHalf.Replace(
                                    "Rating", (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                        strReturn = blnIsSuccess
                            ? ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                            : strFirstHalf;
                    }
                    catch (OverflowException) // Result is text and not a double
                    {
                        strReturn = strFirstHalf;
                    }
                    catch (InvalidCastException) // Result is text and not a double
                    {
                        strReturn = strFirstHalf;
                    }

                    if (blnSquareBrackets)
                        strReturn = '[' + strCapacity + ']';

                    strSecondHalf = strSecondHalf.Trim('[', ']');
                    if (await Children.AnyAsync(x => x.GetAddToParentCapacityAsync(token), token: token).ConfigureAwait(false))
                    {
                        // Run through its Children and deduct the Capacity costs.
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                   out StringBuilder sbdSecondHalf))
                        {
                            await Children.ForEachAsync(async objChildCyberware =>
                            {
                                if (!await objChildCyberware.GetAddToParentCapacityAsync(token).ConfigureAwait(false))
                                    return;
                                if (objChildCyberware.ParentID == InternalId)
                                    return;

                                string strLoopCapacity = await objChildCyberware.GetCalculatedCapacityAsync(token).ConfigureAwait(false);
                                int intLoopPos = strLoopCapacity.IndexOf("/[", StringComparison.Ordinal);
                                if (intLoopPos != -1)
                                    strLoopCapacity = strLoopCapacity.Substring(intLoopPos + 2,
                                        strLoopCapacity.LastIndexOf(']') - intLoopPos - 2);
                                else if (strLoopCapacity.StartsWith('['))
                                    strLoopCapacity = strLoopCapacity.Substring(1, strLoopCapacity.Length - 2);
                                if (strLoopCapacity == "*")
                                    strLoopCapacity = "0";
                                sbdSecondHalf.Append("+(").Append(strLoopCapacity).Append(')');
                            }, token).ConfigureAwait(false);
                            strSecondHalf += sbdSecondHalf.ToString();
                        }
                    }

                    try
                    {
                        (bool blnIsSuccess, object objProcess) =
                            await CommonFunctions.EvaluateInvariantXPathAsync(
                                strSecondHalf.Replace(
                                    "Rating", (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                        strSecondHalf =
                            '[' + (blnIsSuccess
                                ? ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                                : strSecondHalf) + ']';
                    }
                    catch (OverflowException) // Result is text and not a double
                    {
                        strSecondHalf = '[' + strSecondHalf + ']';
                    }
                    catch (InvalidCastException) // Result is text and not a double
                    {
                        strSecondHalf = '[' + strSecondHalf + ']';
                    }

                    strReturn += '/' + strSecondHalf;
                }
                else if (strCapacity.Contains("Rating") ||
                         (strCapacity.StartsWith('[') && await Children.AnyAsync(x => x.GetAddToParentCapacityAsync(token), token: token).ConfigureAwait(false)))
                {
                    // If the Capacity is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strCapacity.StartsWith('[');
                    if (blnSquareBrackets)
                    {
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (await Children.AnyAsync(x => x.GetAddToParentCapacityAsync(token), token: token).ConfigureAwait(false))
                        {
                            // Run through its Children and deduct the Capacity costs.
                            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                       out StringBuilder sbdCapacity))
                            {
                                await Children.ForEachAsync(async objChildCyberware =>
                                {
                                    if (!await objChildCyberware.GetAddToParentCapacityAsync(token).ConfigureAwait(false))
                                        return;
                                    if (objChildCyberware.ParentID == InternalId)
                                        return;

                                    string strLoopCapacity = await objChildCyberware.GetCalculatedCapacityAsync(token).ConfigureAwait(false);
                                    int intLoopPos = strLoopCapacity.IndexOf("/[", StringComparison.Ordinal);
                                    if (intLoopPos != -1)
                                        strLoopCapacity = strLoopCapacity.Substring(intLoopPos + 2,
                                            strLoopCapacity.LastIndexOf(']') - intLoopPos - 2);
                                    else if (strLoopCapacity.StartsWith('['))
                                        strLoopCapacity = strLoopCapacity.Substring(1, strLoopCapacity.Length - 2);
                                    if (strLoopCapacity == "*") strLoopCapacity = "0";
                                    sbdCapacity.Append("+(").Append(strLoopCapacity).Append(')');
                                }, token).ConfigureAwait(false);

                                strCapacity += sbdCapacity.ToString();
                            }
                        }
                    }

                    (bool blnIsSuccess, object objProcess) =
                        await CommonFunctions.EvaluateInvariantXPathAsync(
                            strCapacity.Replace("Rating", (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                    strReturn = blnIsSuccess
                        ? ((double)objProcess).ToString("#,0.##", GlobalSettings.CultureInfo)
                        : strCapacity;
                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                }
                else if (decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                             out decimal decReturn))
                    return decReturn.ToString("#,0.##", GlobalSettings.CultureInfo);
                else
                {
                    // Just a straight Capacity, so return the value.
                    return strCapacity;
                }

                return strReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware.
        /// </summary>
        public decimal CalculatedESS
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return _objCharacter.IsPrototypeTranshuman && PrototypeTranshuman
                        ? 0
                        : CalculatedESSPrototypeInvariant;
                }
            }
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware.
        /// </summary>
        public async Task<decimal> GetCalculatedESSAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await _objCharacter.GetIsPrototypeTranshumanAsync(token).ConfigureAwait(false)
                       && await GetPrototypeTranshumanAsync(token).ConfigureAwait(false)
                    ? 0
                    : await GetCalculatedESSPrototypeInvariantAsync(token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware if Prototype Transhuman is ignored.
        /// </summary>
        public decimal CalculatedESSPrototypeInvariant
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return GetCalculatedESSPrototypeInvariant(Rating, Grade);
            }
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware if Prototype Transhuman is ignored.
        /// </summary>
        public async Task<decimal> GetCalculatedESSPrototypeInvariantAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetCalculatedESSPrototypeInvariantAsync(await GetRatingAsync(token).ConfigureAwait(false),
                    await GetGradeAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public decimal GetCalculatedESSPrototypeInvariant(int intRating, Grade objGrade)
        {
            return Utils.SafelyRunSynchronously(() => GetCalculatedESSPrototypeInvariantCoreAsync(true, intRating, objGrade));
        }

        public Task<decimal> GetCalculatedESSPrototypeInvariantAsync(int intRating, Grade objGrade, CancellationToken token = default)
        {
            return GetCalculatedESSPrototypeInvariantCoreAsync(false, intRating, objGrade, token);
        }

        private async Task<decimal> GetCalculatedESSPrototypeInvariantCoreAsync(bool blnSync, int intRating, Grade objGrade, CancellationToken token = default)
        {
            IDisposable objLocker = null;
            IAsyncDisposable objLockerAsync = null;
            if (blnSync)
                objLocker = LockObject.EnterReadLock(token);
            else
                objLockerAsync = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if ((blnSync ? Parent : await GetParentAsync(token).ConfigureAwait(false)) != null && !(blnSync ? AddToParentESS : await GetAddToParentESSAsync(token).ConfigureAwait(false)))
                    return 0;

                Guid guidSourceId = blnSync ? SourceID : await GetSourceIDAsync(token).ConfigureAwait(false);
                if (guidSourceId == EssenceHoleGUID) // Essence hole
                {
                    return intRating / 100m;
                }

                if (guidSourceId == EssenceAntiHoleGUID) // Essence anti-hole
                {
                    return intRating / -100m;
                }

                decimal decReturn;

                string strESS = ESS;
                if (strESS.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string strSuffix = string.Empty;
                    if (!strESS.EndsWith(')'))
                    {
                        strSuffix = strESS.Substring(strESS.LastIndexOf(')') + 1);
                        strESS = strESS.TrimEndOnce(strSuffix);
                    }

                    string[] strValues = strESS.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strESS = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                    strESS += strSuffix;
                }

                if (strESS.Contains("Rating") || strESS.IndexOfAny(s_MathOperators) >= 0)
                {
                    // If the cost is determined by the Rating or there's a math operation in play, evaluate the expression.
                    (bool blnIsSuccess, object objProcess) = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? CommonFunctions.EvaluateInvariantXPath(
                            strESS.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo)), token)
                        : await CommonFunctions.EvaluateInvariantXPathAsync(
                                strESS.Replace(
                                    "Rating",
                                    intRating.ToString(GlobalSettings.InvariantCultureInfo)), token)
                            .ConfigureAwait(false);
                    decReturn = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) : 0;
                }
                else
                {
                    // Just a straight cost, so return the value.
                    decimal.TryParse(strESS, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decReturn);
                }

                // Factor in the Essence multiplier of the selected CyberwareGrade.
                // We apply grade-based Essence modifiers to the additive stack because of Wakshaani's post about Biocompatibility:
                // https://forums.shadowruntabletop.com/index.php?topic=21061.msg381782#msg381782
                decimal decESSMultiplier = objGrade.Essence + (blnSync ? ExtraESSAdditiveMultiplier : await GetExtraESSAdditiveMultiplierAsync(token).ConfigureAwait(false));
                decimal decTotalESSMultiplier = blnSync ? ExtraESSMultiplicativeMultiplier : await GetExtraESSMultiplicativeMultiplierAsync(token).ConfigureAwait(false);

                if (Suite)
                    decESSMultiplier -= 0.1m;

                decimal decEssDiscount = blnSync ? ESSDiscount : await GetESSDiscountAsync(token).ConfigureAwait(false);
                if (decEssDiscount != 0)
                {
                    decimal decDiscount = decEssDiscount * 0.01m;
                    decTotalESSMultiplier *= 1.0m - decDiscount;
                }

                // Retrieve the Bioware, Geneware or Cyberware ESS Cost Multiplier.
                if (ForceGrade != "None" || IsGeneware)
                {
                    switch (SourceType)
                    {
                        // Apply the character's Cyberware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Cyberware when !IsGeneware:
                            if (blnSync)
                            {
                                UpdateMultipliers(Improvement.ImprovementType.CyberwareEssCost,
                                    Improvement.ImprovementType.CyberwareTotalEssMultiplier);
                                if (!_objCharacter.Created)
                                    UpdateMultipliers(Improvement.ImprovementType.CyberwareEssCostNonRetroactive,
                                        Improvement.ImprovementType
                                            .CyberwareTotalEssMultiplierNonRetroactive);
                            }
                            else
                            {
                                await UpdateMultipliersAsync(Improvement.ImprovementType.CyberwareEssCost,
                                        Improvement.ImprovementType.CyberwareTotalEssMultiplier)
                                    .ConfigureAwait(false);
                                if (!_objCharacter.Created)
                                    await UpdateMultipliersAsync(
                                            Improvement.ImprovementType.CyberwareEssCostNonRetroactive,
                                            Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive)
                                        .ConfigureAwait(false);
                            }

                            break;
                        // Apply the character's Bioware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Bioware when !IsGeneware:
                            if (blnSync)
                            {
                                UpdateMultipliers(Improvement.ImprovementType.BiowareEssCost,
                                    Improvement.ImprovementType.BiowareTotalEssMultiplier);
                                if (!_objCharacter.Created)
                                    UpdateMultipliers(Improvement.ImprovementType.BiowareEssCostNonRetroactive,
                                        Improvement.ImprovementType
                                            .BiowareTotalEssMultiplierNonRetroactive);
                            }
                            else
                            {
                                await UpdateMultipliersAsync(Improvement.ImprovementType.BiowareEssCost,
                                        Improvement.ImprovementType.BiowareTotalEssMultiplier)
                                    .ConfigureAwait(false);
                                if (!_objCharacter.Created)
                                    await UpdateMultipliersAsync(
                                            Improvement.ImprovementType.BiowareEssCostNonRetroactive,
                                            Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive)
                                        .ConfigureAwait(false);
                            }

                            // Apply the character's Basic Bioware Essence cost multiplier if applicable.
                            if (Category == "Basic")
                            {
                                List<Improvement> lstUsedImprovements = blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                        Improvement.ImprovementType.BasicBiowareEssCost, token: token)
                                    : await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                                            Improvement.ImprovementType.BasicBiowareEssCost,
                                            token: token)
                                        .ConfigureAwait(false);
                                if (lstUsedImprovements.Count != 0)
                                {
                                    foreach (Improvement objImprovement in lstUsedImprovements)
                                        decESSMultiplier -= 1m - objImprovement.Value / 100m;
                                }
                            }

                            break;
                        // Apply the character's Geneware Essence cost multiplier if applicable. Since Geneware does not use Grades, we only check the genetechessmultiplier improvement.
                        case Improvement.ImprovementSource.Cyberware when IsGeneware:
                            if (blnSync)
                                UpdateMultipliers(Improvement.ImprovementType.GenetechEssMultiplier,
                                    Improvement.ImprovementType.None);
                            else
                                await UpdateMultipliersAsync(Improvement.ImprovementType.GenetechEssMultiplier,
                                    Improvement.ImprovementType.None).ConfigureAwait(false);
                            break;
                        // Apply the character's Geneware Essence cost multiplier if applicable. Since Geneware does not use Grades, we only check the genetechessmultiplier improvement.
                        case Improvement.ImprovementSource.Bioware when IsGeneware:
                            if (blnSync)
                                UpdateMultipliers(Improvement.ImprovementType.GenetechEssMultiplier,
                                    Improvement.ImprovementType.None);
                            else
                                await UpdateMultipliersAsync(Improvement.ImprovementType.GenetechEssMultiplier,
                                    Improvement.ImprovementType.None).ConfigureAwait(false);
                            break;
                    }

                    void UpdateMultipliers(Improvement.ImprovementType eBaseMultiplier,
                        Improvement.ImprovementType eTotalMultiplier)
                    {
                        if (eBaseMultiplier != Improvement.ImprovementType.None)
                        {
                            List<Improvement> lstUsedImprovements =
                                ImprovementManager.GetCachedImprovementListForValueOf(
                                    _objCharacter, eBaseMultiplier, token: token);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                    decESSMultiplier -= 1m - objImprovement.Value / 100m;
                            }
                        }

                        if (eTotalMultiplier != Improvement.ImprovementType.None)
                        {
                            List<Improvement> lstUsedImprovements =
                                ImprovementManager.GetCachedImprovementListForValueOf(
                                    _objCharacter, eTotalMultiplier, token: token);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                    decTotalESSMultiplier *= objImprovement.Value / 100m;
                            }
                        }
                    }

                    async ValueTask UpdateMultipliersAsync(Improvement.ImprovementType eBaseMultiplier,
                        Improvement.ImprovementType eTotalMultiplier)
                    {
                        if (eBaseMultiplier != Improvement.ImprovementType.None)
                        {
                            List<Improvement> lstUsedImprovements =
                                await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        _objCharacter, eBaseMultiplier, token: token).ConfigureAwait(false);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                    // ReSharper disable once AccessToModifiedClosure
                                    decESSMultiplier -= 1m - objImprovement.Value / 100m;
                            }
                        }

                        if (eTotalMultiplier != Improvement.ImprovementType.None)
                        {
                            List<Improvement> lstUsedImprovements =
                                await ImprovementManager
                                    .GetCachedImprovementListForValueOfAsync(
                                        _objCharacter, eTotalMultiplier, token: token).ConfigureAwait(false);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                    decTotalESSMultiplier *= objImprovement.Value / 100m;
                            }
                        }
                    }
                }

                decimal decTotalModifier = Math.Max(0, decESSMultiplier * decTotalESSMultiplier);
                if (_objCharacter != null)
                {
                    string strPostModifierExpression
                        = (blnSync
                            ? _objCharacter.Settings
                            : await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                        .EssenceModifierPostExpression;
                    if (!string.IsNullOrEmpty(strPostModifierExpression) && strPostModifierExpression != "{Modifier}")
                    {
                        (bool blnIsSuccess, object objProcess) = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? CommonFunctions.EvaluateInvariantXPath(
                                strPostModifierExpression.Replace("{Modifier}",
                                    decTotalModifier.ToString(
                                        GlobalSettings.InvariantCultureInfo)), token)
                            : await CommonFunctions.EvaluateInvariantXPathAsync(
                                    strPostModifierExpression.Replace("{Modifier}",
                                        decTotalModifier.ToString(
                                            GlobalSettings.InvariantCultureInfo)), token)
                                .ConfigureAwait(false);
                        if (blnIsSuccess)
                            decTotalModifier = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                    }
                }

                decReturn *= decTotalModifier;

                if (_objCharacter?.Settings.DontRoundEssenceInternally == false)
                    decReturn = decimal.Round(decReturn, _objCharacter.Settings.EssenceDecimals,
                        MidpointRounding.AwayFromZero);

                if (blnSync)
                {
                    if (_objCharacter?.IsPrototypeTranshuman == true)
                        decReturn += Children.Sum(objChild => objChild.AddToParentESS && !objChild.PrototypeTranshuman,
                            objChild =>
                                objChild.GetCalculatedESSPrototypeInvariant(
                                    objChild.Rating, objGrade), token);
                    else
                        decReturn += Children.Sum(objChild => objChild.AddToParentESS,
                            objChild =>
                                objChild.GetCalculatedESSPrototypeInvariant(
                                    objChild.Rating, objGrade), token);
                }
                else if (_objCharacter != null && await _objCharacter.GetIsPrototypeTranshumanAsync(token).ConfigureAwait(false))
                    decReturn += await Children.SumAsync(
                        async objChild => await objChild.GetAddToParentESSAsync(token).ConfigureAwait(false) && !await objChild.GetPrototypeTranshumanAsync(token).ConfigureAwait(false),
                        async objChild =>
                            await objChild.GetCalculatedESSPrototypeInvariantAsync(
                                await objChild.GetRatingAsync(token).ConfigureAwait(false), objGrade, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                else
                    decReturn += await Children.SumAsync(objChild => objChild.GetAddToParentESSAsync(token),
                            async objChild =>
                                await objChild.GetCalculatedESSPrototypeInvariantAsync(
                                    await objChild.GetRatingAsync(token).ConfigureAwait(false), objGrade, token).ConfigureAwait(false), token: token)
                        .ConfigureAwait(false);

                return decReturn;
            }
            finally
            {
                objLocker?.Dispose();
                if (objLockerAsync != null)
                    await objLockerAsync.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            using (LockObject.EnterReadLock())
            {
                string strExpression = this.GetMatrixAttributeString(strAttributeName);
                if (string.IsNullOrEmpty(strExpression))
                {
                    switch (strAttributeName)
                    {
                        case "Device Rating":
                            return _objGrade.DeviceRating;

                        case "Program Limit":
                            if (IsCommlink)
                            {
                                strExpression = this.GetMatrixAttributeString("Device Rating");
                                if (string.IsNullOrEmpty(strExpression))
                                    return _objGrade.DeviceRating;
                            }
                            else
                                return _objGrade.DeviceRating;

                            break;

                        case "Data Processing":
                        case "Firewall":
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                return _objGrade.DeviceRating;
                            break;

                        default:
                            return 0;
                    }
                }

                if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                      .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strExpression = strValues[Math.Max(0, Math.Min(Rating, strValues.Length) - 1)].Trim('[', ']');
                }

                if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        sbdValue.Replace("{Rating}", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                        foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                        {
                            sbdValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + '}',
                                                  () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0)
                                                      .ToString(
                                                          GlobalSettings
                                                              .InvariantCultureInfo));
                            sbdValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + '}',
                                                  () => Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0");
                            if (Children.Count + GearChildren.Count > 0 &&
                                strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                            {
                                int intTotalChildrenValue =
                                    Children.Sum(x => x.IsModularCurrentlyEquipped,
                                        x => x.GetBaseMatrixAttribute(strMatrixAttribute)) +
                                    GearChildren.Sum(x => x.Equipped,
                                        x => x.GetBaseMatrixAttribute(strMatrixAttribute));

                                sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                                 intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                            }
                        }

                        _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);

                        // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                        (bool blnIsSuccess, object objProcess)
                            = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                        return blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
                    }
                }

                int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
                return intReturn;
            }
        }

        public async Task<int> GetBaseMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strExpression = this.GetMatrixAttributeString(strAttributeName);
                if (string.IsNullOrEmpty(strExpression))
                {
                    switch (strAttributeName)
                    {
                        case "Device Rating":
                            return _objGrade.DeviceRating;

                        case "Program Limit":
                            if (await GetIsCommlinkAsync(token).ConfigureAwait(false))
                            {
                                strExpression = this.GetMatrixAttributeString("Device Rating");
                                if (string.IsNullOrEmpty(strExpression))
                                    return _objGrade.DeviceRating;
                            }
                            else
                                return _objGrade.DeviceRating;

                            break;

                        case "Data Processing":
                        case "Firewall":
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                return _objGrade.DeviceRating;
                            break;

                        default:
                            return 0;
                    }
                }

                if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strExpression = strValues[Math.Max(0, Math.Min(await GetRatingAsync(token).ConfigureAwait(false), strValues.Length) - 1)].Trim('[', ']');
                }

                if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        sbdValue.Replace("{Rating}", (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo));
                        foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                        {
                            await sbdValue.CheapReplaceAsync(strExpression, "{Gear " + strMatrixAttribute + '}',
                                () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0)
                                    .ToString(
                                        GlobalSettings
                                            .InvariantCultureInfo), token: token).ConfigureAwait(false);
                            await sbdValue.CheapReplaceAsync(strExpression, "{Parent " + strMatrixAttribute + '}',
                                    () => Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0", token: token)
                                .ConfigureAwait(false);
                            if (await Children.GetCountAsync(token).ConfigureAwait(false) +
                                await GearChildren.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                                strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                            {
                                int intTotalChildrenValue = await Children
                                    .SumAsync(x => x.GetIsModularCurrentlyEquippedAsync(token),
                                        x => x.GetBaseMatrixAttributeAsync(strMatrixAttribute, token), token: token)
                                    .ConfigureAwait(false);

                                intTotalChildrenValue += await GearChildren
                                    .SumAsync(x => x.Equipped,
                                        x => x.GetBaseMatrixAttributeAsync(strMatrixAttribute, token), token: token)
                                    .ConfigureAwait(false);

                                sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                    intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                            }
                        }

                        await _objCharacter.AttributeSection
                            .ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);

                        // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                        (bool blnIsSuccess, object objProcess)
                            = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token)
                                .ConfigureAwait(false);
                        return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                    }
                }

                int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            using (LockObject.EnterReadLock())
            {
                int intReturn = Overclocked == strAttributeName ? 1 : 0;

                if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                    strAttributeName = "Mod " + strAttributeName;

                intReturn += Children.Sum(x => x.IsModularCurrentlyEquipped,
                    x => x.GetTotalMatrixAttribute(strAttributeName));

                intReturn += GearChildren.Sum(x => x.Equipped,
                    x => x.GetTotalMatrixAttribute(strAttributeName));

                return intReturn;
            }
        }

        public async Task<int> GetBonusMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;

            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intReturn = await GetOverclockedAsync(token).ConfigureAwait(false) == strAttributeName ? 1 : 0;

                if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                    strAttributeName = "Mod " + strAttributeName;

                intReturn += await Children.SumAsync(x => x.GetIsModularCurrentlyEquippedAsync(token),
                    x => x.GetTotalMatrixAttributeAsync(strAttributeName, token), token: token).ConfigureAwait(false);

                intReturn += await GearChildren.SumAsync(x => x.Equipped,
                    x => x.GetTotalMatrixAttributeAsync(strAttributeName, token), token).ConfigureAwait(false);

                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total cost of the just the Cyberware itself before we factor in any multipliers.
        /// </summary>
        public decimal CalculatedOwnCostPreMultipliers(int intRating, Grade objGrade)
        {
            using (LockObject.EnterReadLock())
            {
                string strCostExpression = Cost;

                if (strCostExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string strSuffix = string.Empty;
                    if (!strCostExpression.EndsWith(')'))
                    {
                        strSuffix = strCostExpression.Substring(strCostExpression.LastIndexOf(')') + 1);
                        strCostExpression = strCostExpression.TrimEndOnce(strSuffix);
                    }

                    string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                          .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCostExpression = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)]
                        .Trim('[', ']');
                    strCostExpression += strSuffix;
                }

                string strParentCost = "0";
                decimal decTotalParentGearCost = 0;
                if (_objParent != null)
                {
                    if (strCostExpression.Contains("Parent Cost"))
                        strParentCost = _objParent.Cost;
                    if (strCostExpression.Contains("Parent Gear Cost"))
                        decTotalParentGearCost += _objParent.GearChildren.Sum(loopGear => loopGear.CalculatedCost);
                }

                decimal decTotalGearCost = 0;
                if (GearChildren.Count > 0 && strCostExpression.Contains("Gear Cost"))
                {
                    decTotalGearCost += GearChildren.Sum(loopGear => loopGear.CalculatedCost);
                }

                decimal decTotalChildrenCost = 0;
                if (Children.Count > 0 && strCostExpression.Contains("Children Cost"))
                {
                    decTotalChildrenCost
                        += Children.Sum(loopWare => loopWare.CalculatedTotalCost(loopWare.Rating, objGrade));
                }

                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCostExpression.TrimStart('+'));
                    sbdCost.Replace("Parent Cost", strParentCost);
                    sbdCost.Replace("Parent Gear Cost",
                                    decTotalParentGearCost.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.Replace("Gear Cost", decTotalGearCost.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.Replace("Children Cost",
                                    decTotalChildrenCost.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.CheapReplace(strCostExpression, "MinRating",
                                         () => MinRating.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdCost, strCostExpression);
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// Total cost of the Cyberware and its plugins.
        /// </summary>
        public decimal CalculatedTotalCost(int intRating, Grade objGrade)
        {
            using (LockObject.EnterReadLock())
            {
                decimal decReturn = CalculatedTotalCostWithoutModifiers(intRating, objGrade);

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// Identical to TotalCost, but without the Improvement and Suite multipliers which would otherwise be doubled.
        /// </summary>
        private decimal CalculatedTotalCostWithoutModifiers(int intRating, Grade objGrade)
        {
            using (LockObject.EnterReadLock())
            {
                decimal decCost = CalculatedOwnCostPreMultipliers(intRating, objGrade);
                decimal decReturn = decCost;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= objGrade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Genetech Cost multiplier.
                if (IsGeneware)
                {
                    List<Improvement> lstUsedImprovements
                        = ImprovementManager.GetCachedImprovementListForValueOf(
                            _objCharacter, Improvement.ImprovementType.GenetechCostMultiplier);
                    if (lstUsedImprovements.Count != 0)
                    {
                        decimal decMultiplier = lstUsedImprovements.Aggregate(
                            1.0m, (current, objImprovement) => current - (1.0m - objImprovement.Value / 100.0m));

                        decReturn *= decMultiplier;
                    }
                }

                // Add in the cost of all child components.
                foreach (Cyberware objChild in Children)
                {
                    if (objChild.Capacity == "[*]")
                        continue;
                    // If the child cost starts with "*", multiply the item's base cost.
                    if (objChild.Cost.StartsWith('*'))
                    {
                        decimal decPluginCost =
                            decCost * (Convert.ToDecimal(objChild.Cost.TrimStart('*'),
                                                         GlobalSettings.InvariantCultureInfo) - 1);

                        if (objChild.DiscountCost)
                            decPluginCost *= 0.9m;

                        decReturn += decPluginCost;
                    }
                    else
                        decReturn += objChild.CalculatedTotalCostWithoutModifiers(objChild.Rating, objGrade)
                                     * ChildCostMultiplier;
                }

                // Add in the cost of all Gear plugins.
                decReturn += GearChildren.Sum(x => x.TotalCost);

                return decReturn;
            }
        }

        /// <summary>
        /// Total cost of the just the Cyberware itself before we factor in any multipliers.
        /// </summary>
        public async Task<decimal> CalculatedOwnCostPreMultipliersAsync(int intRating, Grade objGrade, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strCostExpression = Cost;

                if (strCostExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string strSuffix = string.Empty;
                    if (!strCostExpression.EndsWith(')'))
                    {
                        strSuffix = strCostExpression.Substring(strCostExpression.LastIndexOf(')') + 1);
                        strCostExpression = strCostExpression.TrimEndOnce(strSuffix);
                    }

                    string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                          .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCostExpression = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)]
                        .Trim('[', ']');
                    strCostExpression += strSuffix;
                }

                string strParentCost = "0";
                decimal decTotalParentGearCost = 0;
                if (_objParent != null)
                {
                    if (strCostExpression.Contains("Parent Cost"))
                        strParentCost = _objParent.Cost;
                    if (strCostExpression.Contains("Parent Gear Cost"))
                        decTotalParentGearCost += await _objParent.GearChildren.SumAsync(loopGear => loopGear.GetCalculatedCostAsync(token), token).ConfigureAwait(false);
                }

                decimal decTotalGearCost = 0;
                if (strCostExpression.Contains("Gear Cost"))
                {
                    decTotalGearCost += await GearChildren.SumAsync(loopGear => loopGear.GetCalculatedCostAsync(token), token).ConfigureAwait(false);
                }

                decimal decTotalChildrenCost = 0;
                if (strCostExpression.Contains("Children Cost"))
                {
                    decTotalChildrenCost += await Children
                        .SumAsync(
                            async x => await x.CalculatedTotalCostAsync(await x.GetRatingAsync(token).ConfigureAwait(false), objGrade, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                }

                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCostExpression.TrimStart('+'));
                    sbdCost.Replace("Parent Cost", strParentCost);
                    sbdCost.Replace("Parent Gear Cost",
                                    decTotalParentGearCost.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.Replace("Gear Cost", decTotalGearCost.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.Replace("Children Cost",
                                    decTotalChildrenCost.ToString(GlobalSettings.InvariantCultureInfo));
                    await sbdCost.CheapReplaceAsync(strCostExpression, "MinRating",
                                                    async () => (await GetMinRatingAsync(token).ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo),
                                                    token: token).ConfigureAwait(false);
                    sbdCost.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                    await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdCost, strCostExpression, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(sbdCost.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                return decReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total cost of the Cyberware and its plugins.
        /// </summary>
        public async Task<decimal> CalculatedTotalCostAsync(int intRating, Grade objGrade, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                decimal decReturn = await CalculatedTotalCostWithoutModifiersAsync(intRating, objGrade, token).ConfigureAwait(false);

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Identical to TotalCost, but without the Improvement and Suite multipliers which would otherwise be doubled.
        /// </summary>
        private async Task<decimal> CalculatedTotalCostWithoutModifiersAsync(int intRating, Grade objGrade, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                decimal decCost = await CalculatedOwnCostPreMultipliersAsync(intRating, objGrade, token).ConfigureAwait(false);
                decimal decReturn = decCost;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= objGrade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Genetech Cost multiplier.
                if (IsGeneware)
                {
                    List<Improvement> lstUsedImprovements = await ImprovementManager
                                                                  .GetCachedImprovementListForValueOfAsync(
                                                                      _objCharacter,
                                                                      Improvement.ImprovementType
                                                                          .GenetechCostMultiplier,
                                                                      token: token).ConfigureAwait(false);
                    if (lstUsedImprovements.Count != 0)
                    {
                        decimal decMultiplier = lstUsedImprovements.Aggregate(
                            1.0m, (current, objImprovement) => current - (1.0m - objImprovement.Value / 100.0m));

                        decReturn *= decMultiplier;
                    }
                }

                // Add in the cost of all child components.
                decReturn += await Children.SumAsync(objChild => objChild.Capacity != "[*]", async objChild =>
                {
                    // If the child cost starts with "*", multiply the item's base cost.
                    if (objChild.Cost.StartsWith('*'))
                    {
                        decimal decPluginCost =
                            decCost * (Convert.ToDecimal(objChild.Cost.TrimStart('*'),
                                                         GlobalSettings.InvariantCultureInfo) - 1);

                        if (objChild.DiscountCost)
                            decPluginCost *= 0.9m;

                        return decPluginCost;
                    }

                    return await objChild.CalculatedTotalCostWithoutModifiersAsync(
                               await objChild.GetRatingAsync(token).ConfigureAwait(false), objGrade, token).ConfigureAwait(false)
                           * ChildCostMultiplier;
                }, token).ConfigureAwait(false);

                // Add in the cost of all Gear plugins.
                decReturn += await GearChildren.SumAsync(x => x.GetTotalCostAsync(token), token).ConfigureAwait(false);

                return decReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public decimal StolenTotalCost => CalculatedStolenTotalCost(true);

        public decimal NonStolenTotalCost => CalculatedStolenTotalCost(false);

        public decimal CalculatedStolenTotalCost(bool blnStolen)
        {
            using (LockObject.EnterReadLock())
                return CalculatedStolenTotalCost(Rating, Grade, blnStolen);
        }

        public decimal CalculatedStolenTotalCost(int intRating, Grade objGrade, bool blnStolen)
        {
            using (LockObject.EnterReadLock())
            {
                Lazy<decimal> decCost = new Lazy<decimal>(() => CalculatedOwnCostPreMultipliers(intRating, objGrade));
                decimal decReturn = Stolen == blnStolen ? decCost.Value : 0;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= objGrade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Add in the cost of all child components.
                foreach (Cyberware objChild in Children)
                {
                    if (objChild.Capacity == "[*]")
                        continue;
                    // If the child cost starts with "*", multiply the item's base cost.
                    if (objChild.Cost.StartsWith('*'))
                    {
                        if (objChild.Stolen == blnStolen)
                        {
                            decimal decPluginCost =
                                decCost.Value * (Convert.ToDecimal(objChild.Cost.TrimStart('*'),
                                                                   GlobalSettings.InvariantCultureInfo) - 1);

                            if (objChild.DiscountCost)
                                decPluginCost *= 0.9m;

                            decReturn += decPluginCost;
                        }
                    }
                    else
                        decReturn += objChild.CalculatedStolenTotalCost(objChild.Rating, objGrade, blnStolen)
                                     * ChildCostMultiplier;
                }

                // Add in the cost of all Gear plugins.
                decReturn += GearChildren.Sum(objGear => objGear.CalculatedStolenTotalCost(blnStolen));

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        public Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(true, token);

        public Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(false, token);

        public async Task<decimal> CalculatedStolenTotalCostAsync(bool blnStolen, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await CalculatedStolenTotalCostAsync(await GetRatingAsync(token).ConfigureAwait(false), await GetGradeAsync(token).ConfigureAwait(false), blnStolen, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<decimal> CalculatedStolenTotalCostAsync(int intRating, Grade objGrade, bool blnStolen, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                AsyncLazy<decimal> decCost = new AsyncLazy<decimal>(
                    () => CalculatedOwnCostPreMultipliersAsync(intRating, objGrade, token),
                    Utils.JoinableTaskFactory);
                decimal decReturn = Stolen == blnStolen ? await decCost.GetValueAsync(token).ConfigureAwait(false) : 0;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= objGrade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Add in the cost of all child components.
                decReturn += await Children.SumAsync(objChild => objChild.Capacity != "[*]", async objChild =>
                {
                    // If the child cost starts with "*", multiply the item's base cost.
                    if (objChild.Cost.StartsWith('*'))
                    {
                        if (objChild.Stolen != blnStolen)
                            return 0;
                        decimal decPluginCost =
                            await decCost.GetValueAsync(token).ConfigureAwait(false) * (Convert.ToDecimal(
                                objChild.Cost.TrimStart('*'),
                                GlobalSettings.InvariantCultureInfo) - 1);

                        if (objChild.DiscountCost)
                            decPluginCost *= 0.9m;

                        return decPluginCost;
                    }

                    return await objChild.CalculatedStolenTotalCostAsync(
                               await objChild.GetRatingAsync(token).ConfigureAwait(false), objGrade, blnStolen, token).ConfigureAwait(false)
                           * ChildCostMultiplier;
                }, token).ConfigureAwait(false);

                // Add in the cost of all Gear plugins.
                decReturn += GearChildren.Sum(objGear => objGear.CalculatedStolenTotalCost(blnStolen));

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Cost of just the Cyberware itself.
        /// </summary>
        public decimal CalculatedOwnCost(int intRating, Grade objGrade)
        {
            using (LockObject.EnterReadLock())
            {
                decimal decReturn = CalculatedOwnCostPreMultipliers(intRating, objGrade);

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= objGrade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// Cost of just the Cyberware itself.
        /// </summary>
        public async Task<decimal> CalculatedOwnCostAsync(int intRating, Grade objGrade, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                decimal decReturn = await CalculatedOwnCostPreMultipliersAsync(intRating, objGrade, token).ConfigureAwait(false);

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= objGrade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                if (_blnSuite)
                    decReturn *= 0.9m;

                return decReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public decimal TotalCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return CalculatedTotalCost(Rating, Grade);
            }
        }

        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await CalculatedTotalCostAsync(await GetRatingAsync(token).ConfigureAwait(false), await GetGradeAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public decimal OwnCost
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return CalculatedOwnCost(Rating, Grade);
            }
        }

        public async Task<decimal> GetOwnCostAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await CalculatedOwnCostAsync(await GetRatingAsync(token).ConfigureAwait(false), await GetGradeAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total weight of the just the Cyberware itself before we factor in any multipliers.
        /// </summary>
        public decimal CalculatedOwnWeight(int intRating, Grade objGrade)
        {
            using (LockObject.EnterReadLock())
            {
                if (!string.IsNullOrEmpty(ParentID))
                    return 0;
                string strWeightExpression = Weight;
                if (string.IsNullOrEmpty(strWeightExpression))
                    return 0;

                if (strWeightExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string strSuffix = string.Empty;
                    if (!strWeightExpression.EndsWith(')'))
                    {
                        strSuffix = strWeightExpression.Substring(strWeightExpression.LastIndexOf(')') + 1);
                        strWeightExpression = strWeightExpression.TrimEndOnce(strSuffix);
                    }

                    string[] strValues = strWeightExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')')
                                                            .Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strWeightExpression = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)]
                        .Trim('[', ']');
                    strWeightExpression += strSuffix;
                }

                string strParentWeight = "0";
                decimal decTotalParentGearWeight = 0;
                if (_objParent != null)
                {
                    if (strWeightExpression.Contains("Parent Weight"))
                        strParentWeight = _objParent.Weight;
                    if (strWeightExpression.Contains("Parent Gear Weight"))
                        decTotalParentGearWeight
                            += _objParent.GearChildren.Sum(loopGear => loopGear.OwnWeight * loopGear.Quantity);
                }

                decimal decTotalGearWeight = 0;
                if (GearChildren.Count > 0 && strWeightExpression.Contains("Gear Weight"))
                {
                    decTotalGearWeight += GearChildren.Sum(loopGear => loopGear.OwnWeight * loopGear.Quantity);
                }

                decimal decTotalChildrenWeight = 0;
                if (Children.Count > 0 && strWeightExpression.Contains("Children Weight"))
                {
                    decTotalChildrenWeight
                        += Children.Sum(loopWare => loopWare.CalculatedTotalWeight(loopWare.Rating, objGrade));
                }

                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdWeight))
                {
                    sbdWeight.Append(strWeightExpression.TrimStart('+'));
                    sbdWeight.Replace("Parent Weight", strParentWeight);
                    sbdWeight.Replace("Parent Gear Weight",
                                      decTotalParentGearWeight.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdWeight.Replace("Gear Weight", decTotalGearWeight.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdWeight.Replace("Children Weight",
                                      decTotalChildrenWeight.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdWeight.CheapReplace(strWeightExpression, "MinRating",
                                           () => MinRating.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdWeight.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdWeight, strWeightExpression);
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdWeight.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// Total weight of the Cyberware and its plugins.
        /// </summary>
        public decimal CalculatedTotalWeight(int intRating, Grade objGrade)
        {
            using (LockObject.EnterReadLock())
            {
                decimal decWeight = CalculatedOwnWeight(intRating, objGrade);
                decimal decReturn = decWeight;

                // Add in the weight of all child components.
                foreach (Cyberware objChild in Children.Where(x => x.IsModularCurrentlyEquipped))
                {
                    if (objChild.Capacity == "[*]")
                        continue;
                    // If the child cost starts with "*", multiply the item's base cost.
                    if (objChild.Weight.StartsWith('*'))
                    {
                        decimal decPluginWeight =
                            decWeight * (Convert.ToDecimal(objChild.Cost.TrimStart('*'),
                                                           GlobalSettings.InvariantCultureInfo) - 1);
                        decReturn += decPluginWeight;
                    }
                    else
                        decReturn += objChild.CalculatedTotalWeight(objChild.Rating, objGrade);
                }

                // Add in the weight of all Gear plugins.
                decReturn += GearChildren.Sum(x => x.Equipped, objGear => objGear.TotalWeight);

                return decReturn;
            }
        }

        public decimal TotalWeight
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return CalculatedTotalWeight(Rating, Grade);
            }
        }

        public decimal OwnWeight
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return CalculatedOwnWeight(Rating, Grade);
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                decimal decCapacity = 0;
                using (LockObject.EnterReadLock())
                {
                    if (Capacity.Contains("/["))
                    {
                        // Get the Cyberware base Capacity.
                        string strBaseCapacity = CalculatedCapacity;
                        strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                        decCapacity = Convert.ToDecimal(strBaseCapacity, GlobalSettings.CultureInfo)
                                      // Run through its Children and deduct the Capacity costs.
                                      - Children.Sum(objChildCyberware =>
                                      {
                                          // Children that are built into the parent
                                          if (objChildCyberware.PlugsIntoModularMount == HasModularMount &&
                                              !string.IsNullOrWhiteSpace(HasModularMount) ||
                                              objChildCyberware.ParentID == InternalId)
                                              return 0;
                                          string strCapacity = objChildCyberware.CalculatedCapacity;
                                          int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                          if (intPos != -1)
                                              strCapacity = strCapacity.Substring(intPos + 2,
                                                  strCapacity.LastIndexOf(']') - intPos - 2);
                                          else if (strCapacity.StartsWith('['))
                                              strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                          if (strCapacity == "*")
                                              strCapacity = "0";
                                          return Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                                      })
                                      // Run through its Children and deduct the Capacity costs.
                                      - GearChildren.Sum(objChildGear =>
                                      {
                                          if (objChildGear.IncludedInParent)
                                              return 0;

                                          string strCapacity = objChildGear.CalculatedCapacity;
                                          int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                          if (intPos != -1)
                                              strCapacity = strCapacity.Substring(
                                                  intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                                          else if (strCapacity.StartsWith('['))
                                              strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                          if (strCapacity == "*")
                                              strCapacity = "0";
                                          return Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                                      });
                    }
                    else if (!Capacity.Contains('['))
                    {
                        // Get the Cyberware base Capacity.
                        decCapacity = Convert.ToDecimal(CalculatedCapacity, GlobalSettings.CultureInfo)
                                      // Run through its Children and deduct the Capacity costs.
                                      - Children.Sum(objChildCyberware =>
                                      {
                                          if (objChildCyberware.PlugsIntoModularMount == HasModularMount &&
                                              !string.IsNullOrWhiteSpace(HasModularMount) ||
                                              objChildCyberware.ParentID == InternalId)
                                              return 0;
                                          string strCapacity = objChildCyberware.CalculatedCapacity;
                                          int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                          if (intPos != -1)
                                              strCapacity = strCapacity.Substring(
                                                  intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                                          else if (strCapacity.StartsWith('['))
                                              strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                          if (strCapacity == "*")
                                              strCapacity = "0";
                                          return Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                                      })
                                      // Run through its Children and deduct the Capacity costs.
                                      - GearChildren.Sum(objChildGear =>
                                      {
                                          if (objChildGear.IncludedInParent)
                                              return 0;

                                          string strCapacity = objChildGear.CalculatedCapacity;
                                          int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                          if (intPos != -1)
                                              strCapacity = strCapacity.Substring(
                                                  intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                                          else if (strCapacity.StartsWith('['))
                                              strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                          if (strCapacity == "*")
                                              strCapacity = "0";
                                          return Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                                      });
                    }
                }

                return decCapacity;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public async Task<decimal> GetCapacityRemainingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decCapacity = 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Capacity.Contains("/["))
                {
                    string strHasModularMount = await GetHasModularMountAsync(token).ConfigureAwait(false);
                    // Get the Cyberware base Capacity.
                    string strBaseCapacity = await GetCalculatedCapacityAsync(token).ConfigureAwait(false);
                    strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                    decCapacity = Convert.ToDecimal(strBaseCapacity, GlobalSettings.CultureInfo)
                                  // Run through its Children and deduct the Capacity costs.
                                  - await Children.SumAsync(async objChildCyberware =>
                                  {
                                      // Children that are built into the parent
                                      if (await objChildCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false) == strHasModularMount &&
                                          !string.IsNullOrWhiteSpace(strHasModularMount) ||
                                          objChildCyberware.ParentID == InternalId)
                                          return 0;
                                      string strCapacity = await objChildCyberware.GetCalculatedCapacityAsync(token).ConfigureAwait(false);
                                      int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                      if (intPos != -1)
                                          strCapacity = strCapacity.Substring(intPos + 2,
                                              strCapacity.LastIndexOf(']') - intPos - 2);
                                      else if (strCapacity.StartsWith('['))
                                          strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                      if (strCapacity == "*")
                                          strCapacity = "0";
                                      return Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                                  }, token).ConfigureAwait(false)
                                  // Run through its Children and deduct the Capacity costs.
                                  - await GearChildren.SumAsync(async objChildGear =>
                                  {
                                      if (objChildGear.IncludedInParent)
                                          return 0;

                                      string strCapacity = await objChildGear.GetCalculatedArmorCapacityAsync(token).ConfigureAwait(false);
                                      int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                      if (intPos != -1)
                                          strCapacity = strCapacity.Substring(
                                              intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                                      else if (strCapacity.StartsWith('['))
                                          strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                      if (strCapacity == "*")
                                          strCapacity = "0";
                                      return Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                                  }, token).ConfigureAwait(false);
                }
                else if (!Capacity.Contains('['))
                {
                    string strHasModularMount = await GetHasModularMountAsync(token).ConfigureAwait(false);
                    // Get the Cyberware base Capacity.
                    decCapacity = Convert.ToDecimal(await GetCalculatedCapacityAsync(token).ConfigureAwait(false), GlobalSettings.CultureInfo)
                                  // Run through its Children and deduct the Capacity costs.
                                  - await Children.SumAsync(async objChildCyberware =>
                                  {
                                      if (await objChildCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false) == strHasModularMount &&
                                          !string.IsNullOrWhiteSpace(strHasModularMount) ||
                                          objChildCyberware.ParentID == InternalId)
                                          return 0;
                                      string strCapacity = await objChildCyberware.GetCalculatedCapacityAsync(token).ConfigureAwait(false);
                                      int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                      if (intPos != -1)
                                          strCapacity = strCapacity.Substring(
                                              intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                                      else if (strCapacity.StartsWith('['))
                                          strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                      if (strCapacity == "*")
                                          strCapacity = "0";
                                      return Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                                  }, token).ConfigureAwait(false)
                                  // Run through its Children and deduct the Capacity costs.
                                  - await GearChildren.SumAsync(async objChildGear =>
                                  {
                                      if (objChildGear.IncludedInParent)
                                          return 0;

                                      string strCapacity = await objChildGear.GetCalculatedCapacityAsync(token).ConfigureAwait(false);
                                      int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                      if (intPos != -1)
                                          strCapacity = strCapacity.Substring(
                                              intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                                      else if (strCapacity.StartsWith('['))
                                          strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                      if (strCapacity == "*")
                                          strCapacity = "0";
                                      return Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                                  }, token).ConfigureAwait(false);
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return decCapacity;
        }

        public string DisplayCapacity
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (Capacity.Contains('[') && !Capacity.Contains("/["))
                        return CalculatedCapacity;
                    return string.Format(GlobalSettings.CultureInfo,
                                         LanguageManager.GetString("String_CapacityRemaining"),
                                         CalculatedCapacity,
                                         CapacityRemaining.ToString("#,0.##", GlobalSettings.CultureInfo));
                }
            }
        }

        public async Task<string> GetDisplayCapacityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Capacity.Contains('[') && !Capacity.Contains("/["))
                    return await GetCalculatedCapacityAsync(token).ConfigureAwait(false);
                return string.Format(GlobalSettings.CultureInfo,
                    await LanguageManager.GetStringAsync("String_CapacityRemaining", token: token).ConfigureAwait(false),
                    await GetCalculatedCapacityAsync(token).ConfigureAwait(false),
                    (await GetCapacityRemainingAsync(token).ConfigureAwait(false)).ToString("#,0.##", GlobalSettings.CultureInfo));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int MinStrength
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // Base Strength for any limb is 3.
                    return Category == "Cyberlimb" || IsLimb ? ParentVehicle != null ? Math.Max(ParentVehicle.TotalBody, 0) : _intMinStrength : 0;
                }
            }
        }

        public async Task<int> GetMinStrengthAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Base Agility for any limb is 3.
                return Category == "Cyberlimb" || await GetIsLimbAsync(token).ConfigureAwait(false)
                    ? ParentVehicle != null ? Math.Max(await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false), 0) : _intMinStrength
                    : 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public int MinAgility
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    // Base Agility for any limb is 3.
                    return Category == "Cyberlimb" || IsLimb ? ParentVehicle != null ? Math.Max(ParentVehicle.Pilot, 0) : _intMinAgility : 0;
                }
            }
        }

        public async Task<int> GetMinAgilityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Base Agility for any limb is 3.
                return Category == "Cyberlimb" || await GetIsLimbAsync(token).ConfigureAwait(false)
                    ? ParentVehicle != null ? Math.Max(await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false), 0) : _intMinAgility
                    : 0;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Base Cyberlimb attribute value (before modifiers and customization).
        /// </summary>
        public int GetAttributeBaseValue(string strAbbrev)
        {
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
            using (LockObject.EnterReadLock())
            {
                if (Category != "Cyberlimb" && !IsLimb)
                    return 0;
                switch (strAbbrev)
                {
                    case "STR":
                        return MinStrength;

                    case "AGI":
                        return MinAgility;

                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Base Cyberlimb attribute value (before modifiers and customization).
        /// </summary>
        public async Task<int> GetAttributeBaseValueAsync(string strAbbrev, CancellationToken token = default)
        {
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Category != "Cyberlimb" && !await GetIsLimbAsync(token).ConfigureAwait(false))
                    return 0;
                switch (strAbbrev)
                {
                    case "STR":
                        return await GetMinStrengthAsync(token).ConfigureAwait(false);

                    case "AGI":
                        return await GetMinAgilityAsync(token).ConfigureAwait(false);

                    default:
                        return 0;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Unaugmented Cyberlimb attribute value (before modifiers).
        /// </summary>
        public int GetAttributeValue(string strAbbrev)
        {
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
            using (LockObject.EnterReadLock())
            {
                int intValue = GetAttributeBaseValue(strAbbrev);
                if (Children.Count > 0
                    && s_AttributeCustomizationCyberwares.TryGetValue(
                        strAbbrev, out IReadOnlyCollection<string> setNamesToCheck))
                {
                    List<Cyberware> lstCustomizationWare = new List<Cyberware>(Children.Count);
                    Children.ForEach(objChild =>
                    {
                        if (setNamesToCheck.Contains(objChild.Name))
                            lstCustomizationWare.Add(objChild);
                    });

                    if (lstCustomizationWare.Count > 0)
                    {
                        intValue = lstCustomizationWare.Count > 1
                            ? lstCustomizationWare.Max(s => s.Rating)
                            : lstCustomizationWare[0].Rating;
                    }
                }

                return ParentVehicle == null
                    ? Math.Min(intValue, _objCharacter.GetAttribute(strAbbrev)?.TotalMaximum ?? 0)
                    : Math.Min(intValue, Math.Max(ParentVehicle.TotalBody * 2, 1));
            }
        }

        /// <summary>
        /// Unaugmented Cyberlimb attribute value (before modifiers).
        /// </summary>
        public async Task<int> GetAttributeValueAsync(string strAbbrev, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intValue = await GetAttributeBaseValueAsync(strAbbrev, token).ConfigureAwait(false);
                if (await Children.GetCountAsync(token).ConfigureAwait(false) > 0
                    && s_AttributeCustomizationCyberwares.TryGetValue(
                        strAbbrev, out IReadOnlyCollection<string> setNamesToCheck))
                {
                    List<Cyberware> lstCustomizationWare
                        = new List<Cyberware>(await Children.GetCountAsync(token).ConfigureAwait(false));
                    await Children.ForEachAsync(objChild =>
                    {
                        if (setNamesToCheck.Contains(objChild.Name))
                            lstCustomizationWare.Add(objChild);
                    }, token).ConfigureAwait(false);

                    if (lstCustomizationWare.Count > 0)
                    {
                        if (lstCustomizationWare.Count > 1)
                        {
                            foreach (Cyberware objCyberware in lstCustomizationWare)
                            {
                                intValue = Math.Max(intValue, await objCyberware.GetRatingAsync(token).ConfigureAwait(false));
                            }
                        }
                        else
                            intValue = await lstCustomizationWare[0].GetRatingAsync(token).ConfigureAwait(false);
                    }
                }

                if (ParentVehicle == null)
                {
                    CharacterAttrib objAttribute = await _objCharacter.GetAttributeAsync(strAbbrev, token: token)
                                                                      .ConfigureAwait(false);
                    return Math.Min(
                        intValue,
                        objAttribute != null
                            ? await objAttribute.GetTotalMaximumAsync(token).ConfigureAwait(false)
                            : 0);
                }

                return Math.Min(
                    intValue, Math.Max(await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false) * 2, 1));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total value for an attribute on a cyberlimb.
        /// </summary>
        public int GetAttributeTotalValue(string strAbbrev)
        {
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
            using (LockObject.EnterReadLock())
            {
                if (InheritAttributes)
                {
                    int intAverageAttribute = 0;
                    int intCyberlimbChildrenNumber = 0;
                    foreach (int intChildTotalValue in Children.Select(x => x.GetAttributeTotalValue(strAbbrev)))
                    {
                        if (intChildTotalValue <= 0)
                            continue;
                        ++intCyberlimbChildrenNumber;
                        intAverageAttribute += intChildTotalValue;
                    }

                    if (intCyberlimbChildrenNumber == 0)
                        intCyberlimbChildrenNumber = 1;

                    return intAverageAttribute / intCyberlimbChildrenNumber;
                }

                if (Category != "Cyberlimb" && !IsLimb)
                    return 0;

                int intBonus = 0;

                if (Children.Count > 0
                    && s_AttributeEnhancementCyberwares.TryGetValue(
                        strAbbrev, out IReadOnlyCollection<string> setNamesToCheck))
                {
                    List<Cyberware> lstEnhancementWare = new List<Cyberware>(Children.Count);
                    Children.ForEach(objChild =>
                    {
                        if (setNamesToCheck.Contains(objChild.Name))
                            lstEnhancementWare.Add(objChild);
                    });

                    if (lstEnhancementWare.Count > 0)
                    {
                        intBonus = lstEnhancementWare.Count > 1
                            ? lstEnhancementWare.Max(s => s.Rating)
                            : lstEnhancementWare[0].Rating;
                    }
                }

                if (ParentVehicle == null)
                {
                    intBonus += _objCharacter.RedlinerBonus;
                }

                intBonus = Math.Min(intBonus, _objCharacter.Settings.CyberlimbAttributeBonusCap);

                int intReturn = GetAttributeValue(strAbbrev) + intBonus;
                return ParentVehicle == null
                    ? Math.Min(intReturn, _objCharacter.GetAttribute(strAbbrev)?.TotalAugmentedMaximum ?? 0)
                    : Math.Min(intReturn, Math.Max(ParentVehicle.TotalBody * 2, 1));
            }
        }

        /// <summary>
        /// Total value for an attribute on a cyberlimb.
        /// </summary>
        public async Task<int> GetAttributeTotalValueAsync(string strAbbrev, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (await GetInheritAttributesAsync(token).ConfigureAwait(false))
                {
                    int intAverageAttribute = 0;
                    int intCyberlimbChildrenNumber = await Children.SumAsync(async objChild =>
                    {
                        int intChildTotalValue = await objChild.GetAttributeTotalValueAsync(strAbbrev, token)
                                                               .ConfigureAwait(false);
                        if (intChildTotalValue <= 0)
                            return 0;
                        intAverageAttribute += intChildTotalValue;
                        return 1;
                    }, token: token).ConfigureAwait(false);

                    if (intCyberlimbChildrenNumber == 0)
                        intCyberlimbChildrenNumber = 1;

                    return intAverageAttribute / intCyberlimbChildrenNumber;
                }

                if (Category != "Cyberlimb" && !await GetIsLimbAsync(token).ConfigureAwait(false))
                    return 0;

                int intBonus = 0;

                if (await Children.GetCountAsync(token).ConfigureAwait(false) > 0
                    && s_AttributeEnhancementCyberwares.TryGetValue(
                        strAbbrev, out IReadOnlyCollection<string> setNamesToCheck))
                {
                    List<Cyberware> lstEnhancementWare
                        = new List<Cyberware>(await Children.GetCountAsync(token).ConfigureAwait(false));
                    await Children.ForEachAsync(objChild =>
                    {
                        if (setNamesToCheck.Contains(objChild.Name))
                            lstEnhancementWare.Add(objChild);
                    }, token: token).ConfigureAwait(false);
                    if (lstEnhancementWare.Count > 0)
                    {
                        intBonus = await lstEnhancementWare[0].GetRatingAsync(token).ConfigureAwait(false);
                        for (int i = 1; i < lstEnhancementWare.Count; ++i)
                            intBonus = Math.Max(intBonus, await lstEnhancementWare[i].GetRatingAsync(token).ConfigureAwait(false));
                    }
                }

                if (ParentVehicle == null)
                {
                    intBonus += await _objCharacter.GetRedlinerBonusAsync(token).ConfigureAwait(false);
                }

                intBonus = Math.Min(intBonus, await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetCyberlimbAttributeBonusCapAsync(token).ConfigureAwait(false));

                int intReturn = await GetAttributeValueAsync(strAbbrev, token).ConfigureAwait(false) + intBonus;
                if (ParentVehicle == null)
                {
                    CharacterAttrib objAttribute = await _objCharacter.GetAttributeAsync(strAbbrev, token: token)
                                                                      .ConfigureAwait(false);
                    return Math.Min(
                        intReturn,
                        objAttribute != null
                            ? await objAttribute.GetTotalAugmentedMaximumAsync(token).ConfigureAwait(false)
                            : 0);
                }

                return Math.Min(
                    intReturn, Math.Max(await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false) * 2, 1));
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public bool IsProgram => false;

        /// <summary>
        /// Device rating string for Cyberware. If it's empty, then GetBaseMatrixAttribute for Device Rating will fetch the grade's DR.
        /// </summary>
        public string DeviceRating
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDeviceRating;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strDeviceRating = value;
            }
        }

        /// <summary>
        /// Attack string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Attack
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strAttack;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strAttack = value;
            }
        }

        /// <summary>
        /// Sleaze string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Sleaze
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strSleaze;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strSleaze = value;
            }
        }

        /// <summary>
        /// Data Processing string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string DataProcessing
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strDataProcessing;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strDataProcessing = value;
            }
        }

        /// <summary>
        /// Firewall string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Firewall
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strFirewall;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strFirewall = value;
            }
        }

        /// <summary>
        /// Modify Parent's Attack by this.
        /// </summary>
        public string ModAttack
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strModAttack;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strModAttack = value;
            }
        }

        /// <summary>
        /// Modify Parent's Sleaze by this.
        /// </summary>
        public string ModSleaze
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strModSleaze;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strModSleaze = value;
            }
        }

        /// <summary>
        /// Modify Parent's Data Processing by this.
        /// </summary>
        public string ModDataProcessing
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strModDataProcessing;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strModDataProcessing = value;
            }
        }

        /// <summary>
        /// Modify Parent's Firewall by this.
        /// </summary>
        public string ModFirewall
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strModFirewall;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strModFirewall = value;
            }
        }

        /// <summary>
        /// Cyberdeck's Attribute Array string.
        /// </summary>
        public string AttributeArray
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strAttributeArray;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strAttributeArray = value;
            }
        }

        /// <summary>
        /// Modify Parent's Attribute Array by this.
        /// </summary>
        public string ModAttributeArray
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strModAttributeArray;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strModAttributeArray = value;
            }
        }

        /// <inheritdoc />
        public string Overclocked
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _objCharacter.Overclocker ? _strOverclocked : string.Empty;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strOverclocked = value;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetOverclockedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await _objCharacter.GetOverclockerAsync(token).ConfigureAwait(false) ? _strOverclocked : string.Empty;
        }

        /// <inheritdoc />
        public string CanFormPersona
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strCanFormPersona;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strCanFormPersona = value;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetCanFormPersonaAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return _strCanFormPersona;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public bool IsCommlink
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    return CanFormPersona.Contains("Self")
                           || (ChildrenWithMatrixAttributes.Any(x => x.CanFormPersona.Contains("Parent")) &&
                               this.GetTotalMatrixAttribute("Device Rating") > 0);
                }
            }
        }

        /// <inheritdoc />
        public async Task<bool> GetIsCommlinkAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return (await GetCanFormPersonaAsync(token).ConfigureAwait(false)).Contains("Self")
                       || (await ChildrenWithMatrixAttributes
                               .AnyAsync(
                                   async x =>
                                       (await x.GetCanFormPersonaAsync(token).ConfigureAwait(false)).Contains("Parent"),
                                   token: token).ConfigureAwait(false) &&
                           await this.GetTotalMatrixAttributeAsync("Device Rating", token).ConfigureAwait(false) > 0);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Bonus Matrix Boxes.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _intMatrixCMBonus;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _intMatrixCMBonus = value;
            }
        }

        public int TotalBonusMatrixBoxes
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    int intBonusBoxes = Children.Sum(x => x.TotalBonusMatrixBoxes)
                                        + GearChildren.Sum(x => x.Equipped, x => x.TotalBonusMatrixBoxes);
                    return intBonusBoxes;
                }
            }
        }

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _strProgramLimit;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _strProgramLimit = value;
            }
        }

        /// <summary>
        /// Returns true if this is a Cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return _blnCanSwapAttributes;
            }
            set
            {
                using (LockObject.EnterUpgradeableReadLock())
                    _blnCanSwapAttributes = value;
            }
        }

        public IEnumerable<IHasMatrixAttributes> ChildrenWithMatrixAttributes
        {
            get
            {
                using (LockObject.EnterReadLock())
                    return GearChildren.Concat<IHasMatrixAttributes>(Children);
            }
        }

        #endregion Complex Properties

        #region Methods

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteCyberware(bool blnDoRemoval = true, bool blnIncreaseEssenceHole = true, bool blnDoDisposal = true)
        {
            decimal decReturn = 0;
            using (LockObject.EnterWriteLock())
            {
                // Unequip all modular children first so that we don't delete them
                Cyberware objModularChild
                    = Children.DeepFirstOrDefault(x => x.Children, x => !string.IsNullOrEmpty(x.PlugsIntoModularMount));
                while (objModularChild != null)
                {
                    Children.Remove(objModularChild);
                    _objCharacter.Cyberware.Add(objModularChild);
                    objModularChild.ChangeModularEquip(false);
                    objModularChild
                        = Children.DeepFirstOrDefault(x => x.Children,
                                                      x => !string.IsNullOrEmpty(x.PlugsIntoModularMount));
                }

                // Remove the cyberware from the actual parent
                if (blnDoRemoval)
                {
                    if (Parent != null)
                    {
                        Parent.Children.Remove(this);
                    }
                    else if (ParentVehicle != null)
                    {
                        _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == InternalId,
                                                                    out VehicleMod objMod);
                        objMod.Cyberware.Remove(this);
                    }
                    else if (_objCharacter.Cyberware.Contains(this))
                    {
                        if (blnIncreaseEssenceHole && _objCharacter.Created && SourceID != EssenceAntiHoleGUID
                            && SourceID != EssenceHoleGUID)
                        {
                            // Add essence hole.
                            decimal decEssenceHoleToAdd = CalculatedESS;
                            _objCharacter.Cyberware.Remove(this);
                            _objCharacter.IncreaseEssenceHole(decEssenceHoleToAdd);
                        }
                        else
                            _objCharacter.Cyberware.Remove(this);
                    }
                }

                // Remove any children the Gear may have.
                decReturn += Children.AsEnumerableWithSideEffects().Sum(x => x.DeleteCyberware(false));

                // Remove the Gear Weapon created by the Gear if applicable.
                if (!WeaponID.IsEmptyGuid())
                {
                    foreach (Weapon objDeleteWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                    {
                        decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                    }

                    decReturn += _objCharacter.Vehicles.AsEnumerableWithSideEffects().Sum(objVehicle =>
                    {
                        decimal decInnerReturn = 0;
                        foreach (Weapon objDeleteWeapon in objVehicle.Weapons
                                                                     .DeepWhere(x => x.Children,
                                                                         x => x.ParentID == InternalId).ToList())
                        {
                            decInnerReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                        }

                        decInnerReturn += objVehicle.Mods.AsEnumerableWithSideEffects().Sum(objMod =>
                        {
                            decimal decInnerReturn2 = 0;
                            foreach (Weapon objDeleteWeapon in objMod.Weapons
                                                                     .DeepWhere(x => x.Children,
                                                                         x => x.ParentID == InternalId).ToList())
                            {
                                decInnerReturn2 += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                            }

                            return decInnerReturn2;
                        });

                        decInnerReturn += objVehicle.WeaponMounts.AsEnumerableWithSideEffects().Sum(objMount =>
                        {
                            decimal decInnerReturn2 = 0;
                            foreach (Weapon objDeleteWeapon in objMount.Weapons
                                                                       .DeepWhere(x => x.Children,
                                                                           x => x.ParentID == InternalId).ToList())
                            {
                                decInnerReturn2 += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                            }

                            decInnerReturn2 += objMount.Mods.AsEnumerableWithSideEffects().Sum(objMod =>
                            {
                                decimal decInnerReturn3 = 0;
                                foreach (Weapon objDeleteWeapon in objMod.Weapons
                                                                         .DeepWhere(x => x.Children,
                                                                             x => x.ParentID == InternalId).ToList())
                                {
                                    decInnerReturn3 += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                                }

                                return decInnerReturn3;
                            });

                            return decInnerReturn2;
                        });

                        return decInnerReturn;
                    });
                }

                if (!WeaponAccessoryID.IsEmptyGuid())
                {
                    // Locate the Weapon Accessory that was added.
                    WeaponAccessory objWeaponAccessory
                        = _objCharacter.Vehicles.FindVehicleWeaponAccessory(WeaponAccessoryID) ??
                          _objCharacter.Weapons.FindWeaponAccessory(WeaponAccessoryID);
                    objWeaponAccessory?.DeleteWeaponAccessory();
                }

                // Remove any Vehicle that the Cyberware created.
                if (!VehicleID.IsEmptyGuid())
                {
                    foreach (Vehicle objLoopVehicle in _objCharacter.Vehicles.Where(x => x.ParentID == InternalId)
                                                                    .ToList())
                    {
                        decReturn += objLoopVehicle.TotalCost + objLoopVehicle.DeleteVehicle();
                    }
                }

                decReturn += ImprovementManager.RemoveImprovements(_objCharacter, SourceType,
                    new[] { InternalId, InternalId + "Pair", InternalId + "WirelessPair" });

                if (PairBonus != null)
                {
                    // This cyberware should not be included in the count to make things easier.
                    List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                                                                             x => x != this
                                                                                 && IncludePair.Contains(x.Name)
                                                                                 && x.Extra == Extra &&
                                                                                 x.IsModularCurrentlyEquipped)
                                                                         .ToList();
                    int intCount = lstPairableCyberwares.Count;
                    // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                    if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                    {
                        int intMatchLocationCount = 0;
                        int intNotMatchLocationCount = 0;
                        foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                        {
                            if (objPairableCyberware.Location != Location)
                                ++intNotMatchLocationCount;
                            else
                                ++intMatchLocationCount;
                        }

                        // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                        intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                    }

                    foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                              objLoopCyberware.InternalId + "Pair");
                        // Go down the list and create pair bonuses for every second item
                        if (intCount > 0 && (intCount & 1) == 0)
                        {
                            if (!string.IsNullOrEmpty(objLoopCyberware.Forced) && objLoopCyberware.Forced != "Left"
                                                                               && objLoopCyberware.Forced != "Right")
                                ImprovementManager.SetForcedValue(objLoopCyberware.Forced, _objCharacter);
                            else if (objLoopCyberware.Bonus != null && !string.IsNullOrEmpty(objLoopCyberware.Extra))
                                ImprovementManager.SetForcedValue(objLoopCyberware.Extra, _objCharacter);
                            ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                                  objLoopCyberware.InternalId + "Pair",
                                                                  objLoopCyberware.PairBonus, objLoopCyberware.Rating,
                                                                  objLoopCyberware.CurrentDisplayNameShort);
                        }

                        --intCount;
                    }
                }

                if (WirelessPairBonus != null)
                {
                    // This cyberware should not be included in the count to make things easier.
                    List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                        x => x != this && IncludeWirelessPair.Contains(x.Name) && x.Extra == Extra &&
                             x.IsModularCurrentlyEquipped).ToList();
                    int intCount = lstPairableCyberwares.Count;
                    // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                    if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                    {
                        int intMatchLocationCount = 0;
                        int intNotMatchLocationCount = 0;
                        foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                        {
                            if (objPairableCyberware.Location != Location)
                                ++intNotMatchLocationCount;
                            else
                                ++intMatchLocationCount;
                        }

                        // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                        intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                    }

                    foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                              objLoopCyberware.InternalId + "WirelessPair");
                        if (objLoopCyberware.WirelessPairBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                        {
                            ImprovementManager.DisableImprovements(_objCharacter,
                                                                   _objCharacter.Improvements
                                                                       .Where(x => x.ImproveSource
                                                                                  == objLoopCyberware.SourceType
                                                                                  && x.SourceName
                                                                                  == objLoopCyberware.InternalId)
                                                                       .ToList());
                        }

                        // Go down the list and create pair bonuses for every second item
                        if (intCount > 0 && intCount % 2 == 0)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                                                                  objLoopCyberware.InternalId + "WirelessPair",
                                                                  objLoopCyberware.WirelessPairBonus,
                                                                  objLoopCyberware.Rating,
                                                                  objLoopCyberware.CurrentDisplayNameShort);
                        }

                        --intCount;
                    }
                }

                decReturn += GearChildren.AsEnumerableWithSideEffects().Sum(x => x.DeleteGear(false));

                // Fix for legacy characters with old addqualities improvements.
                XPathNodeIterator xmlOldAddQualitiesList
                    = this.GetNodeXPath()?.SelectAndCacheExpression("addqualities/addquality");
                if (xmlOldAddQualitiesList?.Count > 0)
                {
                    foreach (XPathNavigator objNode in xmlOldAddQualitiesList)
                    {
                        string strText = objNode.Value;
                        Quality objQuality = _objCharacter.Qualities.FirstOrDefault(x => x.Name == strText);
                        if (objQuality == null)
                            continue;
                        // We need to add in the return cost of deleting the quality, so call this manually
                        string strInternalId = objQuality.InternalId;
                        decReturn += objQuality.DeleteQuality()
                                     + ImprovementManager.RemoveImprovements(
                                         _objCharacter, Improvement.ImprovementSource.CritterPower, strInternalId);
                    }
                }
            }

            if (blnDoDisposal)
                DisposeSelf();

            return decReturn;
        }

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public async Task<decimal> DeleteCyberwareAsync(bool blnDoRemoval = true,
                                                             bool blnIncreaseEssenceHole = true,
                                                             bool blnDoDisposal = true,
                                                             CancellationToken token = default)
        {
            decimal decReturn;
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Unequip all modular children first so that we don't delete them
                Cyberware objModularChild
                    = await Children.DeepFirstOrDefaultAsync(x => x.Children, async x => !string.IsNullOrEmpty(await x.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)), token).ConfigureAwait(false);
                while (objModularChild != null)
                {
                    await Children.RemoveAsync(objModularChild, token).ConfigureAwait(false);
                    await _objCharacter.Cyberware.AddAsync(objModularChild, token).ConfigureAwait(false);
                    await objModularChild.ChangeModularEquipAsync(false, token: token).ConfigureAwait(false);
                    objModularChild
                        = await Children.DeepFirstOrDefaultAsync(x => x.Children,
                            async x => !string.IsNullOrEmpty(await x.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)), token).ConfigureAwait(false);
                }

                // Remove the cyberware from the actual parent
                if (blnDoRemoval)
                {
                    Cyberware objParent = await GetParentAsync(token).ConfigureAwait(false);
                    if (objParent != null)
                    {
                        await objParent.Children.RemoveAsync(this, token).ConfigureAwait(false);
                    }
                    else if (await GetParentVehicleAsync(token).ConfigureAwait(false) != null)
                    {
                        VehicleMod objMod =
                            (await _objCharacter.Vehicles.FindVehicleCyberwareAsync(x => x.InternalId == InternalId, token: token).ConfigureAwait(false)).Item2;
                        await objMod.Cyberware.RemoveAsync(this, token).ConfigureAwait(false);
                    }
                    else if (await _objCharacter.Cyberware.ContainsAsync(this, token).ConfigureAwait(false))
                    {
                        if (blnIncreaseEssenceHole && await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                        {
                            Guid guidSourceId = await GetSourceIDAsync(token).ConfigureAwait(false);
                            if (guidSourceId != EssenceAntiHoleGUID && guidSourceId != EssenceHoleGUID)
                            {
                                // Add essence hole.
                                decimal decEssenceHoleToAdd = await GetCalculatedESSAsync(token).ConfigureAwait(false);
                                await _objCharacter.Cyberware.RemoveAsync(this, token).ConfigureAwait(false);
                                await _objCharacter.IncreaseEssenceHoleAsync(decEssenceHoleToAdd, token: token).ConfigureAwait(false);
                            }
                            else
                                await _objCharacter.Cyberware.RemoveAsync(this, token).ConfigureAwait(false);
                        }
                        else
                            await _objCharacter.Cyberware.RemoveAsync(this, token).ConfigureAwait(false);
                    }
                }

                // Remove any children the Gear may have.
                decReturn = await Children
                                  .SumWithSideEffectsAsync(x => x.DeleteCyberwareAsync(false, token: token),
                                            token: token)
                                  .ConfigureAwait(false);

                // Remove the Gear Weapon created by the Gear if applicable.
                if (!WeaponID.IsEmptyGuid())
                {
                    foreach (Weapon objDeleteWeapon in await _objCharacter.Weapons
                                                                          .DeepWhereAsync(
                                                                              x => x.Children,
                                                                              x => x.ParentID == InternalId, token)
                                                                          .ConfigureAwait(false))
                    {
                        decReturn += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                     + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                    }

                    decReturn += await _objCharacter.Vehicles.SumWithSideEffectsAsync(async objVehicle =>
                    {
                        decimal decInner = 0;
                        foreach (Weapon objDeleteWeapon in await objVehicle.Weapons
                                                                           .DeepWhereAsync(
                                                                               x => x.Children,
                                                                               x => x.ParentID == InternalId, token)
                                                                           .ConfigureAwait(false))
                        {
                            decInner += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                        + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                        }

                        decInner += await objVehicle.Mods.SumWithSideEffectsAsync(async objMod =>
                        {
                            decimal decInner2 = 0;
                            foreach (Weapon objDeleteWeapon in await objMod.Weapons
                                                                           .DeepWhereAsync(
                                                                               x => x.Children,
                                                                               x => x.ParentID == InternalId, token)
                                                                           .ConfigureAwait(false))
                            {
                                decInner2 += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                             + await objDeleteWeapon.DeleteWeaponAsync(token: token)
                                                                    .ConfigureAwait(false);
                            }

                            return decInner2;
                        }, token).ConfigureAwait(false);

                        decInner += await objVehicle.WeaponMounts.SumWithSideEffectsAsync(async objMount =>
                        {
                            decimal decInner2 = 0;
                            foreach (Weapon objDeleteWeapon in await objMount.Weapons
                                                                             .DeepWhereAsync(
                                                                                 x => x.Children,
                                                                                 x => x.ParentID == InternalId, token)
                                                                             .ConfigureAwait(false))
                            {
                                decInner2 += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                             + await objDeleteWeapon.DeleteWeaponAsync(token: token)
                                                                    .ConfigureAwait(false);
                            }

                            decInner2 += await objMount.Mods.SumWithSideEffectsAsync(async objMod =>
                            {
                                decimal decInner3 = 0;
                                foreach (Weapon objDeleteWeapon in await objMod.Weapons
                                                                               .DeepWhereAsync(
                                                                                   x => x.Children,
                                                                                   x => x.ParentID == InternalId, token)
                                                                               .ConfigureAwait(false))
                                {
                                    decInner3 += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                                 + await objDeleteWeapon.DeleteWeaponAsync(token: token)
                                                                        .ConfigureAwait(false);
                                }

                                return decInner3;
                            }, token).ConfigureAwait(false);

                            return decInner2;
                        }, token).ConfigureAwait(false);

                        return decInner;
                    }, token).ConfigureAwait(false);
                }

                if (!WeaponAccessoryID.IsEmptyGuid())
                {
                    // Locate the Weapon Accessory that was added.
                    WeaponAccessory objWeaponAccessory
                        = await _objCharacter.Vehicles.FindVehicleWeaponAccessoryAsync(WeaponAccessoryID, token).ConfigureAwait(false) ??
                          await _objCharacter.Weapons.FindWeaponAccessoryAsync(WeaponAccessoryID, token).ConfigureAwait(false);
                    if (objWeaponAccessory != null)
                        await objWeaponAccessory.DeleteWeaponAccessoryAsync(token: token).ConfigureAwait(false);
                }

                // Remove any Vehicle that the Cyberware created.
                if (!VehicleID.IsEmptyGuid())
                {
                    foreach (Vehicle objLoopVehicle in await _objCharacter.Vehicles
                                                                          .ToListAsync(
                                                                              x => x.ParentID == InternalId,
                                                                              token: token)
                                                                          .ConfigureAwait(false))
                    {
                        decReturn += await objLoopVehicle.GetTotalCostAsync(token).ConfigureAwait(false)
                                     + await objLoopVehicle.DeleteVehicleAsync(token).ConfigureAwait(false);
                    }
                }

                decReturn += await ImprovementManager
                    .RemoveImprovementsAsync(_objCharacter, SourceType,
                        new[] { InternalId, InternalId + "Pair", InternalId + "WirelessPair" }, token)
                    .ConfigureAwait(false);

                if (PairBonus != null)
                {
                    // This cyberware should not be included in the count to make things easier.
                    List<Cyberware> lstPairableCyberwares
                        = await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepWhereAsync(
                            x => x.Children,
                            async x => !ReferenceEquals(x, this) && IncludePair.Contains(x.Name)
                                                                 && x.Extra == Extra &&
                                                                 await x.GetIsModularCurrentlyEquippedAsync(token)
                                                                        .ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    int intCount = lstPairableCyberwares.Count;
                    // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                    if (!string.IsNullOrEmpty(Location) && IncludePair.All(x => x == Name))
                    {
                        int intMatchLocationCount = 0;
                        int intNotMatchLocationCount = 0;
                        foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                        {
                            if (objPairableCyberware.Location != Location)
                                ++intNotMatchLocationCount;
                            else
                                ++intMatchLocationCount;
                        }

                        // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                        intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                    }

                    foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                    {
                        await ImprovementManager.RemoveImprovementsAsync(_objCharacter, objLoopCyberware.SourceType,
                                                                         objLoopCyberware.InternalId + "Pair", token)
                                                .ConfigureAwait(false);
                        // Go down the list and create pair bonuses for every second item
                        if (intCount > 0 && (intCount & 1) == 0)
                        {
                            if (!string.IsNullOrEmpty(objLoopCyberware.Forced) && objLoopCyberware.Forced != "Left"
                                                                               && objLoopCyberware.Forced != "Right")
                                ImprovementManager.SetForcedValue(objLoopCyberware.Forced, _objCharacter);
                            else if (objLoopCyberware.Bonus != null && !string.IsNullOrEmpty(objLoopCyberware.Extra))
                                ImprovementManager.SetForcedValue(objLoopCyberware.Extra, _objCharacter);
                            await ImprovementManager.CreateImprovementsAsync(_objCharacter, objLoopCyberware.SourceType,
                                                                             objLoopCyberware.InternalId + "Pair",
                                                                             objLoopCyberware.PairBonus,
                                                                             await objLoopCyberware
                                                                                 .GetRatingAsync(token)
                                                                                 .ConfigureAwait(false),
                                                                             await objLoopCyberware
                                                                                 .GetCurrentDisplayNameShortAsync(token)
                                                                                 .ConfigureAwait(false), token: token)
                                                    .ConfigureAwait(false);
                        }

                        --intCount;
                    }
                }

                if (WirelessPairBonus != null)
                {
                    // This cyberware should not be included in the count to make things easier.
                    List<Cyberware> lstPairableCyberwares
                        = await (await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)).DeepWhereAsync(
                            x => x.Children,
                            async x => !ReferenceEquals(x, this) && IncludeWirelessPair.Contains(x.Name)
                                                                 && x.Extra == Extra &&
                                                                 await x.GetIsModularCurrentlyEquippedAsync(token)
                                                                        .ConfigureAwait(false),
                            token).ConfigureAwait(false);
                    int intCount = lstPairableCyberwares.Count;
                    // Need to use slightly different logic if this cyberware has a location (Left or Right) and only pairs with itself because Lefts can only be paired with Rights and Rights only with Lefts
                    if (!string.IsNullOrEmpty(Location) && IncludeWirelessPair.All(x => x == Name))
                    {
                        int intMatchLocationCount = 0;
                        int intNotMatchLocationCount = 0;
                        foreach (Cyberware objPairableCyberware in lstPairableCyberwares)
                        {
                            if (objPairableCyberware.Location != Location)
                                ++intNotMatchLocationCount;
                            else
                                ++intMatchLocationCount;
                        }

                        // Set the count to the total number of cyberwares in matching pairs, which would mean 2x the number of whichever location contains the fewest members (since every single one of theirs would have a pair)
                        intCount = Math.Min(intMatchLocationCount, intNotMatchLocationCount) * 2;
                    }

                    foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                    {
                        await ImprovementManager.RemoveImprovementsAsync(_objCharacter, objLoopCyberware.SourceType,
                                                                         objLoopCyberware.InternalId + "WirelessPair",
                                                                         token).ConfigureAwait(false);
                        if (objLoopCyberware.WirelessPairBonus?
                                .SelectSingleNodeAndCacheExpressionAsNavigator(
                                    "@mode", token)?.Value == "replace")
                        {
                            await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                                                              await _objCharacter.Improvements
                                                                                  .ToListAsync(
                                                                                      x => x.ImproveSource
                                                                                          == objLoopCyberware.SourceType
                                                                                          && x.SourceName
                                                                                          == objLoopCyberware
                                                                                              .InternalId,
                                                                                      token: token)
                                                                                  .ConfigureAwait(false), token)
                                                    .ConfigureAwait(false);
                        }

                        // Go down the list and create pair bonuses for every second item
                        if (intCount > 0 && intCount % 2 == 0)
                        {
                            await ImprovementManager.CreateImprovementsAsync(_objCharacter, objLoopCyberware.SourceType,
                                                                             objLoopCyberware.InternalId
                                                                             + "WirelessPair",
                                                                             objLoopCyberware.WirelessPairBonus,
                                                                             await objLoopCyberware
                                                                                 .GetRatingAsync(token)
                                                                                 .ConfigureAwait(false),
                                                                             await objLoopCyberware
                                                                                 .GetCurrentDisplayNameShortAsync(token)
                                                                                 .ConfigureAwait(false),
                                                                             token: token).ConfigureAwait(false);
                        }

                        --intCount;
                    }
                }

                decReturn += await GearChildren.SumWithSideEffectsAsync(x => x.DeleteGearAsync(false, token), token)
                                               .ConfigureAwait(false);

                // Fix for legacy characters with old addqualities improvements.
                XPathNavigator objDataNode = await this.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                XPathNodeIterator xmlOldAddQualitiesList = objDataNode?.SelectAndCacheExpression("addqualities/addquality", token);
                if (xmlOldAddQualitiesList?.Count > 0)
                {
                    foreach (XPathNavigator objNode in xmlOldAddQualitiesList)
                    {
                        string strText = objNode.Value;
                        Quality objQuality = await _objCharacter.Qualities
                                                                .FirstOrDefaultAsync(x => x.Name == strText, token)
                                                                .ConfigureAwait(false);
                        if (objQuality == null)
                            continue;
                        string strQualityId = objQuality.InternalId;
                        // We need to add in the return cost of deleting the quality, so call this manually
                        decReturn += await objQuality.DeleteQualityAsync(token: token).ConfigureAwait(false);
                        decReturn += await ImprovementManager
                                           .RemoveImprovementsAsync(_objCharacter,
                                                                    Improvement.ImprovementSource.CritterPower,
                                                                    strQualityId, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            if (blnDoDisposal)
                await DisposeSelfAsync().ConfigureAwait(false);

            return decReturn;
        }

        /// <summary>
        /// Checks a nominated piece of gear for Availability requirements.
        /// </summary>
        /// <param name="dicRestrictedGearLimits">Dictionary of Restricted Gear availabilities still available with the amount of items that can still use that availability.</param>
        /// <param name="sbdAvailItems">StringBuilder used to list names of gear that are currently over the availability limit.</param>
        /// <param name="sbdRestrictedItems">StringBuilder used to list names of gear that are being used for Restricted Gear.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<int> CheckRestrictedGear(IDictionary<int, int> dicRestrictedGearLimits, StringBuilder sbdAvailItems, StringBuilder sbdRestrictedItems, CancellationToken token = default)
        {
            int intRestrictedCount = 0;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(ParentID))
                {
                    AvailabilityValue objTotalAvail = await TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (!objTotalAvail.AddToParent)
                    {
                        int intAvailInt = objTotalAvail.Value;
                        if (intAvailInt > _objCharacter.Settings.MaximumAvailability)
                        {
                            int intLowestValidRestrictedGearAvail = -1;
                            foreach (int intValidAvail in dicRestrictedGearLimits.Keys)
                            {
                                if (intValidAvail >= intAvailInt && (intLowestValidRestrictedGearAvail < 0
                                                                     || intValidAvail
                                                                     < intLowestValidRestrictedGearAvail))
                                    intLowestValidRestrictedGearAvail = intValidAvail;
                            }

                            string strNameToUse = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                            Cyberware objParent = await GetParentAsync(token).ConfigureAwait(false);
                            if (objParent != null)
                                strNameToUse += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '('
                                                                                                                   + await objParent.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + ')';

                            Grade objGrade = await GetGradeAsync(token).ConfigureAwait(false);
                            if (objGrade.Avail != 0)
                                strNameToUse += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '('
                                                                                                                   + await objGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + ')';

                            if (intLowestValidRestrictedGearAvail >= 0
                                && dicRestrictedGearLimits[intLowestValidRestrictedGearAvail] > 0)
                            {
                                --dicRestrictedGearLimits[intLowestValidRestrictedGearAvail];
                                sbdRestrictedItems.AppendLine().Append("\t\t").Append(strNameToUse);
                            }
                            else
                            {
                                dicRestrictedGearLimits.Remove(intLowestValidRestrictedGearAvail);
                                ++intRestrictedCount;
                                sbdAvailItems.AppendLine().Append("\t\t").Append(strNameToUse);
                            }
                        }
                    }
                }

                intRestrictedCount += await Children
                                            .SumAsync(objChild =>
                                                    objChild
                                                        .CheckRestrictedGear(
                                                            dicRestrictedGearLimits, sbdAvailItems,
                                                            sbdRestrictedItems,
                                                            token), token: token)
                                            .ConfigureAwait(false)
                                      + await GearChildren
                                              .SumAsync(objChild =>
                                                      objChild
                                                          .CheckRestrictedGear(
                                                              dicRestrictedGearLimits,
                                                              sbdAvailItems,
                                                              sbdRestrictedItems,
                                                              token),
                                                  token: token)
                                              .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return intRestrictedCount;
        }

        public void CheckBannedGrades(StringBuilder sbdBannedItems)
        {
            using (LockObject.EnterReadLock())
            {
                if (string.IsNullOrEmpty(ParentID))
                {
                    Grade objGrade = Grade;
                    if (_objCharacter.Settings.BannedWareGrades.Any(s => objGrade.Name.Contains(s)))
                    {
                        sbdBannedItems.AppendLine().Append("\t\t").Append(CurrentDisplayName);
                    }
                }

                if (!_objGrade.GetNodeXPath().RequirementsMet(_objCharacter))
                {
                    sbdBannedItems.AppendLine().Append("\t\t").Append(CurrentDisplayName);
                }

                foreach (Cyberware objChild in Children)
                {
                    objChild.CheckBannedGrades(sbdBannedItems);
                }
            }
        }

        public async Task CheckBannedGradesAsync(StringBuilder sbdBannedItems, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (string.IsNullOrEmpty(ParentID))
                {
                    Grade objGrade = await GetGradeAsync(token).ConfigureAwait(false);
                    if ((await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BannedWareGrades.Any(
                            s => objGrade.Name.Contains(s)))
                    {
                        sbdBannedItems.AppendLine().Append("\t\t")
                            .Append(await GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                    }
                }

                if (!await (await _objGrade.GetNodeXPathAsync(token).ConfigureAwait(false))
                           .RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                {
                    sbdBannedItems.AppendLine().Append("\t\t")
                                  .Append(await GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                }

                await Children.ForEachAsync(objChild => objChild.CheckBannedGradesAsync(sbdBannedItems, token), token)
                              .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        #region UI Methods

        /// <summary>
        /// Build up the Tree for the current piece of Cyberware and all of its children.
        /// </summary>
        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsCyberware, ContextMenuStrip cmsGear, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) &&
                    !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
                    return null;

                TreeNode objNode = new TreeNode
                {
                    Name = InternalId,
                    Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                    Tag = this,
                    ContextMenuStrip = cmsCyberware,
                    ForeColor = PreferredColor,
                    ToolTipText = Notes.WordWrap()
                };

                TreeNodeCollection lstChildNodes = objNode.Nodes;
                await Children.ForEachAsync(async objChild =>
                {
                    TreeNode objLoopNode = await objChild.CreateTreeNode(cmsCyberware, cmsGear, token).ConfigureAwait(false);
                    if (objLoopNode != null)
                        lstChildNodes.Add(objLoopNode);
                }, token).ConfigureAwait(false);

                await GearChildren.ForEachAsync(async objGear =>
                {
                    TreeNode objLoopNode = await objGear.CreateTreeNode(cmsGear, null, token).ConfigureAwait(false);
                    if (objLoopNode != null)
                        lstChildNodes.Add(objLoopNode);
                }, token).ConfigureAwait(false);

                if (lstChildNodes.Count > 0)
                    objNode.Expand();

                return objNode;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Color PreferredColor
        {
            get
            {
                using (LockObject.EnterReadLock())
                {
                    if (!string.IsNullOrEmpty(Notes))
                    {
                        return !string.IsNullOrEmpty(ParentID)
                            ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                            : ColorManager.GenerateCurrentModeColor(NotesColor);
                    }

                    return !string.IsNullOrEmpty(ParentID)
                        ? ColorManager.GrayText
                        : ColorManager.WindowText;
                }
            }
        }

        public void SetupChildrenCyberwareCollectionChanged(bool blnAdd, TreeView treCyberware,
            ContextMenuStrip cmsCyberware = null, ContextMenuStrip cmsCyberwareGear = null, AsyncNotifyCollectionChangedEventHandler funcMakeDirty = null)
        {
            if (blnAdd)
            {
                Task FuncCyberwareBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshChildrenCyberwareClearBindings(treCyberware, y, innerToken);

                Task FuncCyberwareToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshChildrenCyberware(treCyberware, cmsCyberware, cmsCyberwareGear, null, y,
                        funcMakeDirty, token: innerToken);

                Task FuncGearBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshChildrenGearsClearBindings(treCyberware, y, innerToken);

                Task FuncGearToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshChildrenGears(treCyberware, cmsCyberwareGear, null, () => Children.GetCountAsync(innerToken), y,
                        funcMakeDirty, token: innerToken);

                Children.AddTaggedBeforeClearCollectionChanged(treCyberware, FuncCyberwareBeforeClearToAdd);
                Children.AddTaggedCollectionChanged(treCyberware, FuncCyberwareToAdd);
                GearChildren.AddTaggedBeforeClearCollectionChanged(treCyberware, FuncGearBeforeClearToAdd);
                GearChildren.AddTaggedCollectionChanged(treCyberware, FuncGearToAdd);
                if (funcMakeDirty != null)
                {
                    Children.AddTaggedCollectionChanged(treCyberware, funcMakeDirty);
                    GearChildren.AddTaggedCollectionChanged(treCyberware, funcMakeDirty);
                }

                foreach (Cyberware objChild in Children)
                {
                    objChild.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware,
                        cmsCyberwareGear, funcMakeDirty);
                }

                foreach (Gear objGear in GearChildren)
                    objGear.SetupChildrenGearsCollectionChanged(true, treCyberware, cmsCyberwareGear, null,
                        funcMakeDirty);
            }
            else
            {
                Children.RemoveTaggedAsyncBeforeClearCollectionChanged(treCyberware);
                Children.RemoveTaggedAsyncCollectionChanged(treCyberware);
                GearChildren.RemoveTaggedAsyncBeforeClearCollectionChanged(treCyberware);
                GearChildren.RemoveTaggedAsyncCollectionChanged(treCyberware);
                foreach (Cyberware objChild in Children)
                    objChild.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                foreach (Gear objGear in GearChildren)
                    objGear.SetupChildrenGearsCollectionChanged(false, treCyberware);
            }
        }

        public async Task SetupChildrenCyberwareCollectionChangedAsync(bool blnAdd, TreeView treCyberware,
            ContextMenuStrip cmsCyberware = null, ContextMenuStrip cmsCyberwareGear = null,
            AsyncNotifyCollectionChangedEventHandler funcMakeDirty = null, CancellationToken token = default)
        {
            if (blnAdd)
            {
                Task FuncCyberwareBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshChildrenCyberwareClearBindings(treCyberware, y, innerToken);

                Task FuncCyberwareToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshChildrenCyberware(treCyberware, cmsCyberware, cmsCyberwareGear, null, y,
                        funcMakeDirty, token: innerToken);

                Task FuncGearBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshChildrenGearsClearBindings(treCyberware, y, innerToken);

                Task FuncGearToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshChildrenGears(treCyberware, cmsCyberwareGear, null, () => Children.GetCountAsync(innerToken), y,
                        funcMakeDirty, token: innerToken);

                Children.AddTaggedBeforeClearCollectionChanged(treCyberware, FuncCyberwareBeforeClearToAdd);
                Children.AddTaggedCollectionChanged(treCyberware, FuncCyberwareToAdd);
                GearChildren.AddTaggedBeforeClearCollectionChanged(treCyberware, FuncGearBeforeClearToAdd);
                GearChildren.AddTaggedCollectionChanged(treCyberware, FuncGearToAdd);
                if (funcMakeDirty != null)
                {
                    Children.AddTaggedCollectionChanged(treCyberware, funcMakeDirty);
                    GearChildren.AddTaggedCollectionChanged(treCyberware, funcMakeDirty);
                }

                await Children.ForEachWithSideEffectsAsync(
                    objChild => objChild.SetupChildrenCyberwareCollectionChangedAsync(true, treCyberware, cmsCyberware,
                        cmsCyberwareGear, funcMakeDirty, token), token).ConfigureAwait(false);
                await GearChildren.ForEachWithSideEffectsAsync(
                    objChild => objChild.SetupChildrenGearsCollectionChangedAsync(true, treCyberware, cmsCyberwareGear,
                        null, funcMakeDirty, token: token), token).ConfigureAwait(false);
            }
            else
            {
                await Children.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treCyberware, token).ConfigureAwait(false);
                await Children.RemoveTaggedAsyncCollectionChangedAsync(treCyberware, token).ConfigureAwait(false);
                await GearChildren.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treCyberware, token).ConfigureAwait(false);
                await GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treCyberware, token).ConfigureAwait(false);
                await Children.ForEachWithSideEffectsAsync(
                    objChild => objChild.SetupChildrenCyberwareCollectionChangedAsync(false, treCyberware,
                        token: token), token).ConfigureAwait(false);
                await GearChildren.ForEachWithSideEffectsAsync(
                    objChild => objChild.SetupChildrenGearsCollectionChangedAsync(false, treCyberware, token: token),
                    token).ConfigureAwait(false);
            }
        }

        #endregion UI Methods

        #region Hero Lab Importing

        public bool ImportHeroLabCyberware(XPathNavigator xmlCyberwareImportNode, XmlNode xmlParentCyberwareNode,
            IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles, Grade objSelectedGrade = null)
        {
            if (xmlCyberwareImportNode == null)
                return false;
            using (LockObject.EnterUpgradeableReadLock())
            {
                string strOriginalName = xmlCyberwareImportNode.SelectSingleNodeAndCacheExpression("@name")?.Value
                                         ?? string.Empty;
                if (!string.IsNullOrEmpty(strOriginalName))
                {
                    string strGradeName = objSelectedGrade?.Name ?? "Standard";
                    bool blnCyberware = true;
                    Lazy<List<Grade>> objCyberwareGradeList = new Lazy<List<Grade>>(() =>
                        _objCharacter.GetGradesList(Improvement.ImprovementSource.Cyberware));
                    Lazy<List<Grade>> objBiowareGradeList = new Lazy<List<Grade>>(() =>
                        _objCharacter.GetGradesList(Improvement.ImprovementSource.Bioware));
                    if (objSelectedGrade == null)
                    {
                        bool blnDoBiowareGradeCheck = true;
                        foreach (Grade objCyberwareGrade in objCyberwareGradeList.Value)
                        {
                            if (strOriginalName.EndsWith(" (" + objCyberwareGrade.Name + ')', StringComparison.Ordinal))
                            {
                                strGradeName = objCyberwareGrade.Name;
                                strOriginalName = strOriginalName.TrimEndOnce(" (" + objCyberwareGrade.Name + ')');
                                blnDoBiowareGradeCheck = false;
                                break;
                            }
                        }

                        if (blnDoBiowareGradeCheck)
                        {
                            foreach (Grade objCyberwareGrade in objBiowareGradeList.Value)
                            {
                                if (strOriginalName.EndsWith(" (" + objCyberwareGrade.Name + ')',
                                                             StringComparison.Ordinal))
                                {
                                    strGradeName = objCyberwareGrade.Name;
                                    strOriginalName = strOriginalName.TrimEndOnce(" (" + objCyberwareGrade.Name + ')');
                                    break;
                                }
                            }
                        }
                    }

                    XmlDocument xmlCyberwareDocument = _objCharacter.LoadData("cyberware.xml");
                    XmlDocument xmlBiowareDocument = _objCharacter.LoadData("bioware.xml");
                    string strForceValue = string.Empty;
                    XmlNode xmlCyberwareDataNode = null;
                    using (XmlNodeList xmlCyberwareNodeList
                           = xmlCyberwareDocument.SelectNodes("/chummer/cyberwares/cyberware[contains(name, "
                                                              + strOriginalName.CleanXPath() + ")]"))
                    {
                        if (xmlCyberwareNodeList != null)
                        {
                            foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                            {
                                XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails");
                                if (xmlTestNode != null
                                    && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    // Assumes topmost parent is an AND node
                                    continue;
                                }

                                xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails");
                                if (xmlTestNode != null
                                    && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    // Assumes topmost parent is an AND node
                                    continue;
                                }

                                xmlCyberwareDataNode = xmlLoopNode;
                                break;
                            }
                        }
                    }

                    if (xmlCyberwareDataNode == null)
                    {
                        blnCyberware = false;
                        using (XmlNodeList xmlCyberwareNodeList = xmlBiowareDocument.SelectNodes(
                                   "/chummer/biowares/bioware[contains(name, " + strOriginalName.CleanXPath() + ")]"))
                        {
                            if (xmlCyberwareNodeList != null)
                            {
                                foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                {
                                    XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails");
                                    if (xmlTestNode != null
                                        && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails");
                                    if (xmlTestNode != null
                                        && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlCyberwareDataNode = xmlLoopNode;
                                    break;
                                }
                            }
                        }
                    }

                    if (xmlCyberwareDataNode == null)
                    {
                        string[] astrOriginalNameSplit
                            = strOriginalName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                        if (astrOriginalNameSplit.Length > 1)
                        {
                            string strName = astrOriginalNameSplit[0].Trim();
                            blnCyberware = true;
                            using (XmlNodeList xmlCyberwareNodeList
                                   = xmlCyberwareDocument.SelectNodes(
                                       "/chummer/cyberwares/cyberware[contains(name, " + strName.CleanXPath() + ")]"))
                            {
                                if (xmlCyberwareNodeList != null)
                                {
                                    foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                    {
                                        XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails");
                                        if (xmlTestNode != null
                                            && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails");
                                        if (xmlTestNode != null
                                            && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlCyberwareDataNode = xmlLoopNode;
                                        break;
                                    }

                                    if (xmlCyberwareDataNode != null)
                                        strForceValue = astrOriginalNameSplit[1].Trim();
                                    else
                                    {
                                        blnCyberware = false;
                                        using (XmlNodeList xmlCyberwareNodeList2 = xmlBiowareDocument.SelectNodes(
                                                   "/chummer/biowares/bioware[contains(name, " + strName.CleanXPath()
                                                   + ")]"))
                                        {
                                            if (xmlCyberwareNodeList2 != null)
                                            {
                                                foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList2)
                                                {
                                                    XPathNavigator xmlTestNode
                                                        = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails");
                                                    if (xmlTestNode != null
                                                        && xmlParentCyberwareNode
                                                            .ProcessFilterOperationNode(xmlTestNode, false))
                                                    {
                                                        // Assumes topmost parent is an AND node
                                                        continue;
                                                    }

                                                    xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator(
                                                        "required/parentdetails");
                                                    if (xmlTestNode != null
                                                        && !xmlParentCyberwareNode.ProcessFilterOperationNode(
                                                            xmlTestNode, false))
                                                    {
                                                        // Assumes topmost parent is an AND node
                                                        continue;
                                                    }

                                                    xmlCyberwareDataNode = xmlLoopNode;
                                                    break;
                                                }
                                            }
                                        }

                                        if (xmlCyberwareDataNode != null)
                                            strForceValue = astrOriginalNameSplit[1].Trim();
                                    }
                                }
                            }
                        }

                        if (xmlCyberwareDataNode == null)
                        {
                            astrOriginalNameSplit = strOriginalName.Split(',', StringSplitOptions.RemoveEmptyEntries);
                            if (astrOriginalNameSplit.Length > 1)
                            {
                                string strName = astrOriginalNameSplit[0].Trim();
                                blnCyberware = true;
                                using (XmlNodeList xmlCyberwareNodeList
                                       = xmlCyberwareDocument.SelectNodes(
                                           "/chummer/cyberwares/cyberware[contains(name, " + strName.CleanXPath()
                                           + ")]"))
                                {
                                    if (xmlCyberwareNodeList != null)
                                    {
                                        foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                        {
                                            XPathNavigator xmlTestNode
                                                = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails");
                                            if (xmlTestNode != null
                                                && xmlParentCyberwareNode
                                                    .ProcessFilterOperationNode(xmlTestNode, false))
                                            {
                                                // Assumes topmost parent is an AND node
                                                continue;
                                            }

                                            xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/parentdetails");
                                            if (xmlTestNode != null
                                                && !xmlParentCyberwareNode.ProcessFilterOperationNode(
                                                    xmlTestNode, false))
                                            {
                                                // Assumes topmost parent is an AND node
                                                continue;
                                            }

                                            xmlCyberwareDataNode = xmlLoopNode;
                                            break;
                                        }

                                        if (xmlCyberwareDataNode != null)
                                            strForceValue = astrOriginalNameSplit[1].Trim();
                                        else
                                        {
                                            blnCyberware = false;
                                            using (XmlNodeList xmlCyberwareNodeList2 = xmlBiowareDocument.SelectNodes(
                                                       "/chummer/biowares/bioware[contains(name, "
                                                       + strName.CleanXPath()
                                                       + ")]"))
                                            {
                                                if (xmlCyberwareNodeList2 != null)
                                                {
                                                    foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList2)
                                                    {
                                                        XPathNavigator xmlTestNode =
                                                            xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/parentdetails");
                                                        if (xmlTestNode != null
                                                            && xmlParentCyberwareNode.ProcessFilterOperationNode(
                                                                xmlTestNode, false))
                                                        {
                                                            // Assumes topmost parent is an AND node
                                                            continue;
                                                        }

                                                        xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator(
                                                            "required/parentdetails");
                                                        if (xmlTestNode != null
                                                            && !xmlParentCyberwareNode.ProcessFilterOperationNode(
                                                                xmlTestNode, false))
                                                        {
                                                            // Assumes topmost parent is an AND node
                                                            continue;
                                                        }

                                                        xmlCyberwareDataNode = xmlLoopNode;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (xmlCyberwareDataNode != null)
                    {
                        if (objSelectedGrade == null)
                        {
                            objSelectedGrade =
                                (blnCyberware ? objCyberwareGradeList : objBiowareGradeList).Value.Find(x =>
                                    x.Name == strGradeName);
                        }

                        Create(xmlCyberwareDataNode, objSelectedGrade,
                               blnCyberware
                                   ? Improvement.ImprovementSource.Cyberware
                                   : Improvement.ImprovementSource.Bioware,
                               xmlCyberwareImportNode.SelectSingleNodeAndCacheExpression("@rating")?.ValueAsInt ?? 0, lstWeapons,
                               lstVehicles, true, true, strForceValue);
                        Notes = xmlCyberwareImportNode.SelectSingleNodeAndCacheExpression("description")?.Value;

                        ProcessHeroLabCyberwarePlugins(xmlCyberwareImportNode, objSelectedGrade, lstWeapons,
                                                       lstVehicles);

                        return true;
                    }
                }

                return false;
            }
        }

        public void ProcessHeroLabCyberwarePlugins(XPathNavigator xmlGearImportNode, Grade objParentGrade,
            IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles)
        {
            if (xmlGearImportNode == null)
                return;
            using (LockObject.EnterUpgradeableReadLock())
            {
                foreach (string strPluginNodeName in Character.HeroLabPluginNodeNames)
                {
                    foreach (XPathNavigator xmlPluginToAdd in xmlGearImportNode.Select(
                                 strPluginNodeName + "/item[@useradded != \"no\"]"))
                    {
                        bool blnSuccess = false;
                        Cyberware objPlugin = new Cyberware(_objCharacter);
                        try
                        {
                            if (objPlugin.ImportHeroLabCyberware(xmlPluginToAdd, this.GetNode(), lstWeapons,
                                                                 lstVehicles,
                                                                 objParentGrade))
                            {
                                blnSuccess = true;
                                objPlugin.Parent = this;
                                Children.Add(objPlugin);
                            }
                            else
                            {
                                Gear objPluginGear = new Gear(_objCharacter);
                                try
                                {
                                    if (objPluginGear.ImportHeroLabGear(xmlPluginToAdd, this.GetNode(), lstWeapons))
                                    {
                                        objPluginGear.Parent = this;
                                        GearChildren.Add(objPluginGear);
                                    }
                                }
                                catch
                                {
                                    objPluginGear.Dispose();
                                    throw;
                                }
                            }
                        }
                        catch
                        {
                            objPlugin.Dispose();
                            throw;
                        }
                        if (!blnSuccess)
                            objPlugin.Dispose();
                    }

                    foreach (XPathNavigator xmlPluginToAdd in xmlGearImportNode.Select(
                                 strPluginNodeName + "/item[@useradded = \"no\"]"))
                    {
                        string strName = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("@name")?.Value
                                         ?? string.Empty;
                        if (!string.IsNullOrEmpty(strName))
                        {
                            Cyberware objPlugin = Children.FirstOrDefault(x =>
                                                                              x.ParentID == InternalId
                                                                              && (x.Name.Contains(strName)
                                                                                  || strName.Contains(x.Name)));
                            if (objPlugin != null)
                            {
                                objPlugin.Notes = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("description")
                                                                ?.Value;
                                objPlugin.ProcessHeroLabCyberwarePlugins(xmlPluginToAdd, objParentGrade, lstWeapons,
                                                                         lstVehicles);
                            }
                            else
                            {
                                Gear objPluginGear = GearChildren.FirstOrDefault(x =>
                                    x.IncludedInParent && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                if (objPluginGear != null)
                                {
                                    objPluginGear.Quantity = xmlPluginToAdd
                                                             .SelectSingleNodeAndCacheExpression("@quantity")
                                                             ?.ValueAsInt ?? 1;
                                    objPluginGear.Notes = xmlPluginToAdd
                                                          .SelectSingleNodeAndCacheExpression("description")?.Value;
                                    objPluginGear.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons);
                                }
                            }
                        }
                    }
                }

                this.RefreshMatrixAttributeArray(_objCharacter);
            }
        }

        #endregion Hero Lab Importing

        #endregion Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            using (LockObject.EnterUpgradeableReadLock())
            {
                if (Capacity == "[*]" && Parent != null && (!_objCharacter.IgnoreRules || _objCharacter.Created))
                {
                    Program.ShowScrollableMessageBox(
                        LanguageManager.GetString("Message_CannotRemoveCyberware"),
                        LanguageManager.GetString("MessageTitle_CannotRemoveCyberware"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString(
                                                                           SourceType == Improvement.ImprovementSource
                                                                               .Bioware
                                                                               ? "Message_DeleteBioware"
                                                                               : "Message_DeleteCyberware")))
                    return false;

                DeleteCyberware(false);
            }

            DisposeSelf();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                if (Capacity == "[*]" && await GetParentAsync(token).ConfigureAwait(false) != null &&
                    (!await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false) ||
                     await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)))
                {
                    await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync("Message_CannotRemoveCyberware", token: token)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_CannotRemoveCyberware", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                    return false;
                }

                if (blnConfirmDelete && !await CommonFunctions.ConfirmDeleteAsync(await LanguageManager.GetStringAsync(
                            await GetSourceTypeAsync(token).ConfigureAwait(false) == Improvement.ImprovementSource
                                .Bioware
                                ? "Message_DeleteBioware"
                                : "Message_DeleteCyberware", token: token).ConfigureAwait(false), token)
                        .ConfigureAwait(false))
                    return false;

                await DeleteCyberwareAsync(blnDoDisposal: false, token: token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await DisposeSelfAsync().ConfigureAwait(false);
            return true;
        }

        public bool Sell(decimal decPercentage, bool blnConfirmDelete)
        {
            if (!_objCharacter.Created)
                return Remove(blnConfirmDelete);

            using (LockObject.EnterUpgradeableReadLock())
            {
                if (Capacity == "[*]" && Parent != null && (!_objCharacter.IgnoreRules || _objCharacter.Created))
                {
                    Program.ShowScrollableMessageBox(
                        LanguageManager.GetString("Message_CannotRemoveCyberware"),
                        LanguageManager.GetString("MessageTitle_CannotRemoveCyberware"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString(
                                                                           SourceType == Improvement.ImprovementSource
                                                                               .Bioware
                                                                               ? "Message_DeleteBioware"
                                                                               : "Message_DeleteCyberware")))
                    return false;

                string strEntry = LanguageManager.GetString(
                    SourceType == Improvement.ImprovementSource.Cyberware
                        ? "String_ExpenseSoldCyberware"
                        : "String_ExpenseSoldBioware");
                IHasCost objParent = null;
                if (Parent != null)
                    objParent = Parent;
                else if (ParentVehicle != null)
                {
                    _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == InternalId,
                                                                out VehicleMod objMod);
                    objParent = objMod;
                }

                // Record the cost of the Cyberware carrier with the Cyberware.
                decimal decAmount;
                if (objParent != null)
                {
                    decimal decOriginal = objParent.TotalCost;
                    decAmount = DeleteCyberware(blnDoDisposal: false) * decPercentage;
                    decAmount += (decOriginal - objParent.TotalCost) * decPercentage;
                }
                else
                {
                    decimal decOriginal = TotalCost;
                    decAmount = (DeleteCyberware(blnDoDisposal: false) + decOriginal) * decPercentage;
                }
                // Create the Expense Log Entry for the sale.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                objExpense.Create(decAmount, strEntry + ' ' + CurrentDisplayNameShort, ExpenseType.Nuyen,
                    DateTime.Now);
                _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                _objCharacter.Nuyen += decAmount;
            }

            DisposeSelf();

            return true;
        }

        public async Task<bool> SellAsync(decimal decPercentage, bool blnConfirmDelete,
            CancellationToken token = default)
        {
            if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                return await RemoveAsync(blnConfirmDelete, token).ConfigureAwait(false);

            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (Capacity == "[*]" && await GetParentAsync(token).ConfigureAwait(false) != null &&
                    (!await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false) ||
                     await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false)))
                {
                    await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync("Message_CannotRemoveCyberware", token: token)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_CannotRemoveCyberware", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                    return false;
                }

                if (blnConfirmDelete && !await CommonFunctions.ConfirmDeleteAsync(await LanguageManager.GetStringAsync(
                            await GetSourceTypeAsync(token).ConfigureAwait(false) == Improvement.ImprovementSource
                                .Bioware
                                ? "Message_DeleteBioware"
                                : "Message_DeleteCyberware", token: token).ConfigureAwait(false), token)
                        .ConfigureAwait(false))
                    return false;

                string strEntry = await LanguageManager.GetStringAsync(
                    await GetSourceTypeAsync(token).ConfigureAwait(false) == Improvement.ImprovementSource.Cyberware
                        ? "String_ExpenseSoldCyberware"
                        : "String_ExpenseSoldBioware", token: token).ConfigureAwait(false);
                IHasCost objParent = await GetParentAsync(token).ConfigureAwait(false);
                if (objParent == null && await GetParentVehicleAsync(token).ConfigureAwait(false) != null)
                {
                    objParent =
                        (await _objCharacter.Vehicles.FindVehicleCyberwareAsync(x => x.InternalId == InternalId, token: token).ConfigureAwait(false)).Item2;
                }

                // Record the cost of the Cyberware carrier with the Cyberware.
                decimal decAmount;
                if (objParent != null)
                {
                    decimal decOriginal = await objParent.GetTotalCostAsync(token).ConfigureAwait(false);
                    decAmount = await DeleteCyberwareAsync(blnDoDisposal: false, token: token).ConfigureAwait(false) *
                                decPercentage;
                    decAmount += (decOriginal - await objParent.GetTotalCostAsync(token).ConfigureAwait(false)) *
                                 decPercentage;
                }
                else
                {
                    decimal decOriginal = await GetTotalCostAsync(token).ConfigureAwait(false);
                    decAmount = (await DeleteCyberwareAsync(blnDoDisposal: false, token: token).ConfigureAwait(false) +
                                 decOriginal) * decPercentage;
                }

                // Create the Expense Log Entry for the sale.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                objExpense.Create(decAmount,
                    strEntry + ' ' + await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                    ExpenseType.Nuyen,
                    DateTime.Now);
                await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
                await _objCharacter.ModifyNuyenAsync(decAmount, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await DisposeSelfAsync().ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Purchases a selected piece of Cyberware with a given Grade and Rating.
        /// </summary>
        public async Task<bool> Purchase(XmlNode objNode, Improvement.ImprovementSource objImprovementSource, Grade objGrade,
            int intRating, Vehicle objVehicle, IAsyncCollection<Cyberware> lstCyberwareCollection,
            IAsyncCollection<Vehicle> lstVehicleCollection, IAsyncCollection<Weapon> lstWeaponCollection,
            decimal decMarkup = 0, bool blnFree = false, bool blnBlackMarket = false, bool blnForVehicle = false,
            string strExpenseString = "String_ExpensePurchaseCyberware", Cyberware objParent = null, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Create the Cyberware object.
                List<Weapon> lstWeapons = new List<Weapon>(1);
                List<Vehicle> lstVehicles = new List<Vehicle>(1);
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    await CreateAsync(objNode, objGrade, objImprovementSource, intRating, lstWeapons, lstVehicles, true, true,
                        string.Empty, objParent, objVehicle, token: token).ConfigureAwait(false);
                    if (InternalId.IsEmptyGuid())
                    {
                        return false;
                    }

                    if (blnFree)
                        Cost = "0";
                    await SetDiscountCostAsync(blnBlackMarket, token).ConfigureAwait(false);
                }
                finally
                {
                    await objLocker2.DisposeAsync().ConfigureAwait(false);
                }

                Guid guidSourceID = await GetSourceIDAsync(token).ConfigureAwait(false);

                if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                {
                    decimal decCost = 0;
                    // Check the item's Cost and make sure the character can afford it.
                    if (!blnFree)
                    {
                        decCost = await GetTotalCostAsync(token).ConfigureAwait(false);

                        // Multiply the cost if applicable.
                        char chrAvail = (await TotalAvailTupleAsync(token: token).ConfigureAwait(false)).Suffix;
                        CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                        switch (chrAvail)
                        {
                            case 'R' when objSettings.MultiplyRestrictedCost:
                                decCost *= objSettings.RestrictedCostMultiplier;
                                break;

                            case 'F' when objSettings.MultiplyForbiddenCost:
                                decCost *= objSettings.ForbiddenCostMultiplier;
                                break;
                        }

                        // Apply a markup if applicable.
                        if (decMarkup != 0)
                        {
                            decCost *= 1 + decMarkup / 100.0m;
                        }

                        if (decCost > await _objCharacter.GetNuyenAsync(token).ConfigureAwait(false))
                        {
                            await Program.ShowScrollableMessageBoxAsync(
                                await LanguageManager.GetStringAsync("Message_NotEnoughNuyen", token: token).ConfigureAwait(false),
                                await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen", token: token).ConfigureAwait(false),
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                            return false;
                        }
                    }

                    // Create the Expense Log Entry.
                    ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                    string strEntry = await LanguageManager.GetStringAsync(strExpenseString, token: token).ConfigureAwait(false);
                    string strName = await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
                    if (guidSourceID == EssenceHoleGUID || guidSourceID == EssenceAntiHoleGUID)
                    {
                        strName += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '(' +
                                   (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.CultureInfo) + ')';
                    }

                    objExpense.Create(decCost * -1,
                        strEntry + await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) +
                        strName, ExpenseType.Nuyen, DateTime.Now);
                    await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
                    await _objCharacter.ModifyNuyenAsync(-decCost, token).ConfigureAwait(false);

                    if (guidSourceID != EssenceHoleGUID && guidSourceID != EssenceAntiHoleGUID)
                    {
                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateNuyen(
                            blnForVehicle ? NuyenExpenseType.AddVehicleModCyberware : NuyenExpenseType.AddCyberware,
                            InternalId);
                        objExpense.Undo = objUndo;
                    }
                }

                if (guidSourceID == EssenceAntiHoleGUID)
                {
                    await _objCharacter.DecreaseEssenceHoleAsync(await GetRatingAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
                else if (guidSourceID == EssenceHoleGUID)
                {
                    await _objCharacter.IncreaseEssenceHoleAsync(await GetRatingAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
                else
                {
                    if (await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false) && ReferenceEquals(lstCyberwareCollection, await _objCharacter.GetCyberwareAsync(token).ConfigureAwait(false)))
                    {
                        await _objCharacter.DecreaseEssenceHoleAsync(await GetCalculatedESSAsync(token).ConfigureAwait(false), false, token).ConfigureAwait(false);
                    }

                    if (lstCyberwareCollection != null)
                        await lstCyberwareCollection.AddAsync(this, token).ConfigureAwait(false);

                    foreach (Weapon objWeapon in lstWeapons)
                    {
                        await objWeapon.SetParentVehicleAsync(objVehicle, token).ConfigureAwait(false);
                        if (lstWeaponCollection != null)
                            await lstWeaponCollection.AddAsync(objWeapon, token).ConfigureAwait(false);
                    }

                    if (lstVehicleCollection != null)
                    {
                        foreach (Vehicle objLoopVehicle in lstVehicles)
                        {
                            await lstVehicleCollection.AddAsync(objLoopVehicle, token).ConfigureAwait(false);
                        }
                    }
                }

                return true;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task Upgrade(Grade objGrade, int intRating, decimal refundPercentage, bool blnFree,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await LockObject.EnterUpgradeableReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                decimal decSaleCost = await GetTotalCostAsync(token).ConfigureAwait(false) * refundPercentage;
                decimal decOldEssence = await GetCalculatedESSAsync(token).ConfigureAwait(false);

                decimal decNewCost = blnFree
                    ? 0
                    : await CalculatedTotalCostAsync(intRating, objGrade, token).ConfigureAwait(false) - decSaleCost;
                if (decNewCost > await _objCharacter.GetNuyenAsync(token).ConfigureAwait(false))
                {
                    await Program.ShowScrollableMessageBoxAsync(
                        await LanguageManager.GetStringAsync("Message_NotEnoughNuyen", token: token)
                            .ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("MessageTitle_NotEnoughNuyen", token: token)
                            .ConfigureAwait(false),
                        MessageBoxButtons.OK, MessageBoxIcon.Information, token: token).ConfigureAwait(false);
                    return;
                }

                string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                    .ConfigureAwait(false);
                string strExpense =
                    await LanguageManager.GetStringAsync("String_ExpenseUpgradedCyberware", token: token)
                        .ConfigureAwait(false) + strSpace +
                    await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
                Grade objOldGrade = await GetGradeAsync(token).ConfigureAwait(false);
                bool blnDoGradeChange = objOldGrade.Essence != objGrade.Essence;
                int intOldRating = await GetRatingAsync(token).ConfigureAwait(false);
                bool blnDoRatingChange = intOldRating != intRating;
                if (blnDoGradeChange || blnDoRatingChange)
                {
                    strExpense +=
                        '(' + await LanguageManager.GetStringAsync("String_Grade", token: token).ConfigureAwait(false) +
                        strSpace + await objOldGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)
                        + strSpace + "->" + await objGrade.GetCurrentDisplayNameAsync(token).ConfigureAwait(false)
                        + strSpace + await LanguageManager.GetStringAsync(RatingLabel, token: token)
                            .ConfigureAwait(false)
                        + intOldRating.ToString(GlobalSettings.CultureInfo)
                        + strSpace + "->" + strSpace + intRating.ToString(GlobalSettings.CultureInfo) + ')';
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                objExpense.Create(-decNewCost, strExpense, ExpenseType.Nuyen, DateTime.Now);
                await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
                await _objCharacter.ModifyNuyenAsync(-decNewCost, token).ConfigureAwait(false);

                ExpenseUndo objUndo = new ExpenseUndo();
                objUndo.CreateNuyen(NuyenExpenseType.AddGear, InternalId);
                objExpense.Undo = objUndo;
                IAsyncDisposable objLocker2 = await LockObject.EnterWriteLockAsync(token).ConfigureAwait(false);
                try
                {
                    token.ThrowIfCancellationRequested();
                    Interlocked.Decrement(ref _intProcessPropertyChanges);
                    try
                    {
                        await SetRatingAsync(intRating, token).ConfigureAwait(false);
                        await SetGradeAsync(objGrade, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        Interlocked.Increment(ref _intProcessPropertyChanges);
                    }

                    decimal decEssDelta =
                        await GetCalculatedESSPrototypeInvariantAsync(intRating, objGrade, token)
                            .ConfigureAwait(false) - decOldEssence;
                    if (decEssDelta > 0)
                    {
                        //The new Essence cost is greater than the old one.
                        await _objCharacter.DecreaseEssenceHoleAsync(decEssDelta, token: token).ConfigureAwait(false);
                    }
                    else if (decEssDelta < 0)
                    {
                        await _objCharacter.IncreaseEssenceHoleAsync(-decEssDelta, token: token).ConfigureAwait(false);
                    }

                    await DoPropertyChangesAsync(blnDoRatingChange, blnDoGradeChange, token).ConfigureAwait(false);
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
        /// Alias map for SourceDetail control text and tooltip assignation.
        /// </summary>
        /// <param name="sourceControl"></param>
        public void SetSourceDetail(Control sourceControl)
        {
            using (LockObject.EnterReadLock())
            {
                if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                    _objCachedSourceDetail = default;
                SourceDetail.SetControl(sourceControl);
            }
        }

        public async Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                    _objCachedSourceDetail = default;
                await (await GetSourceDetailAsync(token).ConfigureAwait(false)).SetControlAsync(sourceControl, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public async Task<bool> AllowPasteXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await GlobalSettings.EnterClipboardReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                switch (await GlobalSettings.GetClipboardContentTypeAsync(token).ConfigureAwait(false))
                {
                    case ClipboardContentType.Gear:
                        string strCategory =
                            (await GlobalSettings.GetClipboardAsync(token).ConfigureAwait(false))
                                .SelectSingleNodeAndCacheExpressionAsNavigator("/character/gear/category", token)?.Value;
                        string strName =
                            (await GlobalSettings.GetClipboardAsync(token).ConfigureAwait(false))
                                .SelectSingleNodeAndCacheExpressionAsNavigator("/character/gear/name", token)
                                ?.Value;
                        IAsyncDisposable objLocker2 = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
                        try
                        {
                            token.ThrowIfCancellationRequested();
                            if (AllowGear?.SelectSingleNode("gearcategory[. = " + strCategory.CleanXPath() +
                                                            "] | gearname[. = " + strName.CleanXPath() + ']') != null)
                            {
                                return true;
                            }
                        }
                        finally
                        {
                            await objLocker2.DisposeAsync().ConfigureAwait(false);
                        }

                        break;

                    case ClipboardContentType.Cyberware:
                        Utils.BreakIfDebug(); //Currently unimplemented.
                        break;

                    default:
                        return false;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            return false;
        }

        public async Task<bool> AllowPasteObject(object input, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!(input is Cyberware objCyberware))
                return true;
            IAsyncDisposable objLocker = await LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strPlugsIntoModularMount = await objCyberware.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(strPlugsIntoModularMount))
                {
                    if (strPlugsIntoModularMount != await GetHasModularMountAsync(token).ConfigureAwait(false))
                        return false;
                    string strInputHasModularMount = await objCyberware.GetHasModularMountAsync(token).ConfigureAwait(false);
                    if (await Children.AnyAsync(
                            async x => await x.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false) == strInputHasModularMount,
                            token: token).ConfigureAwait(false))
                        return false;

                    objCyberware.Location = await GetLocationAsync(token).ConfigureAwait(false);
                }

                if (await objCyberware.GetSourceTypeAsync(token).ConfigureAwait(false) != await GetSourceTypeAsync(token).ConfigureAwait(false))
                    return true;
                string strAllowedSubsystems = AllowedSubsystems;
                if (!string.IsNullOrEmpty(strAllowedSubsystems))
                {
                    string strCategory = objCyberware.Category;
                    return strAllowedSubsystems.SplitNoAlloc(',').All(strSubsystem => strCategory != strSubsystem);
                }

                if (string.IsNullOrEmpty(await objCyberware.GetHasModularMountAsync(token).ConfigureAwait(false)) &&
                    string.IsNullOrEmpty(await objCyberware.GetBlocksMountsAsync(token).ConfigureAwait(false)))
                    return true;
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                           out HashSet<string> setDisallowedMounts))
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                           out HashSet<string> setHasMounts))
                {
                    foreach (string strLoop in (await objCyberware.GetBlocksMountsAsync(token).ConfigureAwait(false)).SplitNoAlloc(','))
                        setDisallowedMounts.Add(strLoop + await GetLocationAsync(token).ConfigureAwait(false));
                    string strLoopHasModularMount = await GetHasModularMountAsync(token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strLoopHasModularMount))
                        setHasMounts.Add(strLoopHasModularMount);
                    foreach (Cyberware objLoopCyberware in await Children.DeepWhereAsync(
                                 x => x.Children, async x => string.IsNullOrEmpty(await x.GetPlugsIntoModularMountAsync(token).ConfigureAwait(false)), token: token).ConfigureAwait(false))
                    {
                        foreach (string strLoop in (await objLoopCyberware.GetBlocksMountsAsync(token).ConfigureAwait(false)).SplitNoAlloc(
                                     ','))
                        {
                            setDisallowedMounts.Add(strLoop + await objLoopCyberware.GetLocationAsync(token).ConfigureAwait(false));
                        }

                        strLoopHasModularMount = await objLoopCyberware.GetHasModularMountAsync(token).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(strLoopHasModularMount))
                            setHasMounts.Add(strLoopHasModularMount);
                    }

                    if (!string.IsNullOrEmpty(await objCyberware.GetHasModularMountAsync(token).ConfigureAwait(false)) &&
                        setDisallowedMounts.Count > 0)
                    {
                        foreach (string strLoop in setDisallowedMounts)
                        {
                            if (strLoop.EndsWith("Right", StringComparison.Ordinal))
                                continue;
                            string strCheck = strLoop;
                            if (strCheck.EndsWith("Left", StringComparison.Ordinal))
                            {
                                strCheck = strCheck.TrimEndOnce("Left", true);
                                if (!setDisallowedMounts.Contains(strCheck + "Right"))
                                    continue;
                            }

                            if (strCheck == await objCyberware.GetHasModularMountAsync(token).ConfigureAwait(false))
                            {
                                return false;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(await objCyberware.GetBlocksMountsAsync(token).ConfigureAwait(false)))
                        return true;
                    if (string.IsNullOrEmpty(await objCyberware.GetLocationAsync(token).ConfigureAwait(false)) &&
                        string.IsNullOrEmpty(await GetLocationAsync(token).ConfigureAwait(false)) &&
                        (await Children.AllAsync(async x => await x.GetLocationAsync(token).ConfigureAwait(false) != "Left", token: token).ConfigureAwait(false)
                         || await Children.AllAsync(async x => await x.GetLocationAsync(token).ConfigureAwait(false) != "Right",
                             token: token).ConfigureAwait(false)))
                        return true;
                    return (await objCyberware.GetBlocksMountsAsync(token).ConfigureAwait(false)).SplitNoAlloc(',')
                        .All(strLoop => !setHasMounts.Contains(strLoop));
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            using (LockObject.EnterWriteLock())
            {
                foreach (Cyberware objChild in _lstChildren)
                    objChild.Dispose();
                foreach (Gear objChild in _lstGear)
                    objChild.Dispose();
            }

            DisposeSelf();
        }

        private void DisposeSelf()
        {
            using (LockObject.EnterWriteLock())
            {
                _lstChildren.Dispose();
                _lstGear.Dispose();
                Utils.StringHashSetPool.Return(ref _lstIncludeInPairBonus);
                Utils.StringHashSetPool.Return(ref _lstIncludeInWirelessPairBonus);
            }
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                foreach (Cyberware objChild in _lstChildren)
                    await objChild.DisposeAsync().ConfigureAwait(false);
                foreach (Gear objChild in _lstGear)
                    await objChild.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }

            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private async ValueTask DisposeSelfAsync()
        {
            IAsyncDisposable objLocker = await LockObject.EnterWriteLockAsync().ConfigureAwait(false);
            try
            {
                await _lstChildren.DisposeAsync().ConfigureAwait(false);
                await _lstGear.DisposeAsync().ConfigureAwait(false);
                Utils.StringHashSetPool.Return(ref _lstIncludeInPairBonus);
                Utils.StringHashSetPool.Return(ref _lstIncludeInWirelessPairBonus);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public int ChildCostMultiplier => 1;

        /// <inheritdoc />
        public AsyncFriendlyReaderWriterLock LockObject { get; }

        public Character CharacterObject => _objCharacter; // readonly member, no locking required
    }
}
