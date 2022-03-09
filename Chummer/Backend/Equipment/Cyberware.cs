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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using NLog;
using Version = System.Version;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A piece of Cyberware.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayName(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage)}")]
    public sealed class Cyberware : ICanPaste, IHasChildrenAndCost<Cyberware>, IHasGear, IHasName, IHasInternalId, IHasXmlDataNode,
        IHasMatrixAttributes, IHasNotes, ICanSell, IHasRating, IHasSource, ICanSort, IHasStolenProperty,
        IHasWirelessBonus, ICanBlackMarketDiscount, IDisposable
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
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

        private readonly TaggedObservableCollection<Cyberware> _lstChildren =
            new TaggedObservableCollection<Cyberware>();

        private readonly TaggedObservableCollection<Gear> _lstGear = new TaggedObservableCollection<Gear>();
        private XmlNode _nodBonus;
        private XmlNode _nodPairBonus;
        private XmlNode _nodWirelessBonus;
        private XmlNode _nodWirelessPairBonus;
        private readonly HashSet<string> _lstIncludeInPairBonus = Utils.StringHashSetPool.Get();
        private readonly HashSet<string> _lstIncludeInWirelessPairBonus = Utils.StringHashSetPool.Get();
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

        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> s_AttributeCustomizationCyberwares
            = new Dictionary<string, IReadOnlyCollection<string>>
            {
                {"AGI", new HashSet<string> {"Customized Agility", "Cyberlimb Customization, Agility (2050)"}},
                {"STR", new HashSet<string> {"Customized Strength", "Cyberlimb Customization, Strength (2050)"}}
            };

        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> s_AttributeEnhancementCyberwares
            = new Dictionary<string, IReadOnlyCollection<string>>
            {
                {"AGI", new HashSet<string> {"Enhanced Agility", "Cyberlimb Augmentation, Agility (2050)"}},
                {"STR", new HashSet<string> {"Enhanced Strength", "Cyberlimb Augmentation, Strength (2050)"}}
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

        public static IReadOnlyCollection<string> CyberlimbAttributeAbbrevs { get; } = new HashSet<string>(s_AttributeAffectingCyberwares.Keys);

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
            foreach (Grade objGrade in objCharacter.GetGradeList(objSource, true))
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
            _objCharacter = objCharacter;

            _lstChildren.AddTaggedCollectionChanged(this, CyberwareChildrenOnCollectionChanged);
            _lstGear.AddTaggedCollectionChanged(this, GearChildrenOnCollectionChanged);
        }

        private void CyberwareChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            // If we are loading (or we are not attached to a character), only manage parent setting, don't do property updating
            if (!_blnDoPropertyChangedInCollectionChanged || _objCharacter == null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (Cyberware objNewItem in e.NewItems)
                            objNewItem.Parent = this;
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (Cyberware objOldItem in e.OldItems)
                        {
                            if (objOldItem.Parent == this)
                                objOldItem.Parent = null;
                        }
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        HashSet<Cyberware> setNewItems = e.NewItems.OfType<Cyberware>().ToHashSet();
                        foreach (Cyberware objOldItem in e.OldItems)
                        {
                            if (!setNewItems.Contains(objOldItem) && objOldItem.Parent == this)
                                objOldItem.Parent = null;
                        }
                        foreach (Cyberware objNewItem in setNewItems)
                            objNewItem.Parent = this;
                        break;
                    case NotifyCollectionChangedAction.Move:
                        return;
                    case NotifyCollectionChangedAction.Reset:
                        break;
                }
                this.RefreshMatrixAttributeArray();
                return;
            }

            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setAttributesToRefresh))
            {
                bool blnDoEssenceImprovementsRefresh = false;
                bool blnDoRedlinerRefresh = false;
                bool blnDoEncumbranceRefresh = false;
                List<Cyberware> lstImprovementSourcesToProcess = new List<Cyberware>(e.NewItems?.Count ?? 0);
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        // ReSharper disable once PossibleNullReferenceException
                        foreach (Cyberware objNewItem in e.NewItems)
                        {
                            objNewItem.Parent = this;
                            if (objNewItem.IsModularCurrentlyEquipped)
                            {
                                blnDoEncumbranceRefresh = IsModularCurrentlyEquipped && ParentVehicle == null;
                                lstImprovementSourcesToProcess.Add(objNewItem);
                            }

                            if (setAttributesToRefresh.Count < CyberlimbAttributeAbbrevs.Count &&
                                Category == "Cyberlimb" && Parent?.InheritAttributes != false
                                && ParentVehicle == null
                                &&
                                _objCharacter?.Settings.DontUseCyberlimbCalculation != true &&
                                !string.IsNullOrWhiteSpace(LimbSlot) &&
                                _objCharacter?.Settings.ExcludeLimbSlot.Contains(LimbSlot) != true)
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
                                        if (!setAttributesToRefresh.Contains(kvpToCheck.Key) && kvpToCheck.Value.Contains(objNewItem.Name))
                                            setAttributesToRefresh.Add(kvpToCheck.Key);
                                    }
                                }
                            }

                            if (!blnDoEssenceImprovementsRefresh && (Parent == null || AddToParentESS) &&
                                string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                                blnDoEssenceImprovementsRefresh = true;
                        }

                        this.RefreshMatrixAttributeArray();
                        blnDoRedlinerRefresh = true;
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (Cyberware objOldItem in e.OldItems)
                        {
                            if (objOldItem.IsModularCurrentlyEquipped)
                                blnDoEncumbranceRefresh = IsModularCurrentlyEquipped && ParentVehicle == null;
                            if (objOldItem.Parent == this)
                                objOldItem.Parent = null;

                            if (setAttributesToRefresh.Count < CyberlimbAttributeAbbrevs.Count &&
                                Category == "Cyberlimb" && Parent?.InheritAttributes != false
                                && ParentVehicle == null
                                &&
                                !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                                !string.IsNullOrWhiteSpace(LimbSlot) &&
                                !_objCharacter.Settings.ExcludeLimbSlot.Contains(LimbSlot))
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
                                        if (!setAttributesToRefresh.Contains(kvpToCheck.Key) && kvpToCheck.Value.Contains(objOldItem.Name))
                                            setAttributesToRefresh.Add(kvpToCheck.Key);
                                    }
                                }
                            }

                            if (!blnDoEssenceImprovementsRefresh && (Parent == null || AddToParentESS) &&
                                string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                                blnDoEssenceImprovementsRefresh = true;
                        }

                        this.RefreshMatrixAttributeArray();
                        blnDoRedlinerRefresh = true;
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        // ReSharper disable once AssignNullToNotNullAttribute
                        HashSet<Cyberware> setNewItems = e.NewItems.OfType<Cyberware>().ToHashSet();
                        foreach (Cyberware objOldItem in e.OldItems)
                        {
                            if (setNewItems.Contains(objOldItem))
                                continue;
                            if (objOldItem.IsModularCurrentlyEquipped)
                                blnDoEncumbranceRefresh = IsModularCurrentlyEquipped && ParentVehicle == null;
                            if (objOldItem.Parent == this)
                                objOldItem.Parent = null;

                            if (setAttributesToRefresh.Count < CyberlimbAttributeAbbrevs.Count &&
                                Category == "Cyberlimb" && Parent?.InheritAttributes != false
                                && ParentVehicle == null
                                &&
                                !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                                !string.IsNullOrWhiteSpace(LimbSlot) &&
                                !_objCharacter.Settings.ExcludeLimbSlot.Contains(LimbSlot))
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
                                        if (!setAttributesToRefresh.Contains(kvpToCheck.Key) && kvpToCheck.Value.Contains(objOldItem.Name))
                                            setAttributesToRefresh.Add(kvpToCheck.Key);
                                    }
                                }
                            }

                            if (!blnDoEssenceImprovementsRefresh && (Parent == null || AddToParentESS) &&
                                string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                                blnDoEssenceImprovementsRefresh = true;
                        }

                        foreach (Cyberware objNewItem in setNewItems)
                        {
                            objNewItem.Parent = this;
                            if (objNewItem.IsModularCurrentlyEquipped)
                            {
                                blnDoEncumbranceRefresh = IsModularCurrentlyEquipped && ParentVehicle == null;
                                lstImprovementSourcesToProcess.Add(objNewItem);
                            }

                            if (setAttributesToRefresh.Count < CyberlimbAttributeAbbrevs.Count &&
                                Category == "Cyberlimb" && Parent?.InheritAttributes != false
                                && ParentVehicle == null
                                &&
                                !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                                !string.IsNullOrWhiteSpace(LimbSlot) &&
                                !_objCharacter.Settings.ExcludeLimbSlot.Contains(LimbSlot))
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
                                        if (!setAttributesToRefresh.Contains(kvpToCheck.Key) && kvpToCheck.Value.Contains(objNewItem.Name))
                                            setAttributesToRefresh.Add(kvpToCheck.Key);
                                    }
                                }
                            }

                            if (!blnDoEssenceImprovementsRefresh && (Parent == null || AddToParentESS) &&
                                string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                                blnDoEssenceImprovementsRefresh = true;
                        }

                        this.RefreshMatrixAttributeArray();
                        blnDoRedlinerRefresh = true;
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        blnDoEssenceImprovementsRefresh = true;
                        blnDoEncumbranceRefresh = IsModularCurrentlyEquipped && ParentVehicle == null;
                        if (Category == "Cyberlimb" && Parent?.InheritAttributes != false
                                                    && ParentVehicle == null
                                                    &&
                                                    !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                                                    !string.IsNullOrWhiteSpace(LimbSlot) &&
                                                    !_objCharacter.Settings.ExcludeLimbSlot.Contains(LimbSlot))
                        {
                            setAttributesToRefresh.AddRange(CyberlimbAttributeAbbrevs);
                        }

                        this.RefreshMatrixAttributeArray();
                        blnDoRedlinerRefresh = true;
                        break;
                }

                using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>(
                           Utils.DictionaryForMultiplePropertyChangedPool,
                           out Dictionary<INotifyMultiplePropertyChanged, HashSet<string>> dicChangedProperties))
                {
                    try
                    {
                        // Note: Movement is always handled whenever AGI or STR is changed, regardless of whether or not we use cyberleg movement
                        foreach (string strAbbrev in setAttributesToRefresh)
                        {
                            foreach (CharacterAttrib objCharacterAttrib in
                                     _objCharacter.GetAllAttributes(strAbbrev))
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
                                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                                    {
                                        if (objImprovement.SourceName.TrimEndOnce("Pair").TrimEndOnce("Wireless")
                                            != objItem.InternalId || !objImprovement.Enabled)
                                            continue;
                                        foreach ((INotifyMultiplePropertyChanged objItemToUpdate,
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
                                    }
                                }
                            }
                        }

                        foreach (KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>> kvpToProcess in
                                 dicChangedProperties)
                        {
                            kvpToProcess.Key.OnMultiplePropertyChanged(kvpToProcess.Value.ToList());
                        }
                    }
                    finally
                    {
                        foreach (HashSet<string> setToReturn in dicChangedProperties.Values)
                            Utils.StringHashSetPool.Return(setToReturn);
                    }
                }
            }
        }

        private void GearChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        objNewItem.ChangeEquippedStatus(IsModularCurrentlyEquipped);
                    }

                    this.RefreshMatrixAttributeArray();
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        objOldItem.ChangeEquippedStatus(false, !IsModularCurrentlyEquipped);
                        objOldItem.Parent = null;
                    }
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        objNewItem.ChangeEquippedStatus(IsModularCurrentlyEquipped);
                    }

                    this.RefreshMatrixAttributeArray();
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        objOldItem.ChangeEquippedStatus(false, !IsModularCurrentlyEquipped);
                        objOldItem.Parent = null;
                    }

                    this.RefreshMatrixAttributeArray();
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this.RefreshMatrixAttributeArray();
                    break;
            }
        }

        /// Create a Cyberware from an XmlNode.
        /// <param name="objXmlCyberware">XmlNode to create the object from.</param>
        /// <param name="objGrade">Grade of the selected piece.</param>
        /// <param name="objSource">Source of the piece.</param>
        /// <param name="intRating">Selected Rating of the piece of Cyberware.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="lstVehicles">List of Vehicles that should be added to the Character.</param>
        /// <param name="blnCreateImprovements">Whether or not Improvements should be created.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        /// <param name="strForced">Force a particular value to be selected by an Improvement prompts.</param>
        /// <param name="objParent">Cyberware to which this new cyberware should be added (needed in creation method for selecting a side).</param>
        /// <param name="objParentVehicle">Vehicle to which this new cyberware will be added (needed in creation method for selecting a side and improvements).</param>
        /// <param name="blnSkipSelectForms">Whether or not to skip forms that are created for bonuses. Use only when creating Gear for previews in selection forms.</param>
        public void Create(XmlNode objXmlCyberware, Grade objGrade, Improvement.ImprovementSource objSource,
            int intRating, IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles, bool blnCreateImprovements = true,
            bool blnCreateChildren = true, string strForced = "", Cyberware objParent = null,
            Vehicle objParentVehicle = null, bool blnSkipSelectForms = false)
        {
            _blnDoPropertyChangedInCollectionChanged = false;
            try
            {
                Parent = objParent;
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
                if (string.IsNullOrEmpty(Notes))
                {
                    Notes = CommonFunctions.GetBookNotes(objXmlCyberware, Name, CurrentDisplayName, Source, Page,
                        DisplayPage(GlobalSettings.Language), _objCharacter);
                }

                _blnAddToParentESS = objXmlCyberware["addtoparentess"] != null;
                _blnAddToParentCapacity = objXmlCyberware["addtoparentcapacity"] != null;
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

                _intRating = Math.Min(Math.Max(intRating, MinRating), MaxRating);

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
                if (objXmlCyberware.InnerXml.Contains("allowsubsystems"))
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdSubsystem))
                    {
                        XmlNodeList lstSubSystems = objXmlCyberware.SelectNodes("allowsubsystems/category");
                        for (int i = 0; i < lstSubSystems?.Count; i++)
                        {
                            sbdSubsystem.Append(lstSubSystems[i].InnerText).Append(',');
                        }

                        if (sbdSubsystem.Length > 0)
                            --sbdSubsystem.Length;
                        _strAllowSubsystems = sbdSubsystem.ToString();
                    }
                }

                XmlNode xmlPairInclude = objXmlCyberware.SelectSingleNode("pairinclude");
                if (xmlPairInclude != null)
                {
                    if (xmlPairInclude.SelectSingleNode("@includeself")?.Value !=
                        bool.FalseString)
                    {
                        _lstIncludeInPairBonus.Add(Name);
                    }

                    using (XmlNodeList xmlPairIncludeNames = xmlPairInclude.SelectNodes("name"))
                    {
                        if (xmlPairIncludeNames?.Count > 0)
                        {
                            foreach (XmlNode objPairNameNode in xmlPairIncludeNames)
                            {
                                _lstIncludeInPairBonus.Add(objPairNameNode.InnerText);
                            }
                        }
                    }
                }
                else
                    _lstIncludeInPairBonus.Add(Name);

                xmlPairInclude = objXmlCyberware.SelectSingleNode("wirelesspairinclude");
                if (xmlPairInclude != null)
                {
                    if (xmlPairInclude.SelectSingleNode("@includeself")?.Value !=
                        bool.FalseString)
                    {
                        _lstIncludeInWirelessPairBonus.Add(Name);
                    }

                    using (XmlNodeList xmlPairIncludeNames = xmlPairInclude.SelectNodes("name"))
                    {
                        if (xmlPairIncludeNames?.Count > 0)
                        {
                            foreach (XmlNode objPairNameNode in xmlPairIncludeNames)
                            {
                                _lstIncludeInWirelessPairBonus.Add(objPairNameNode.InnerText);
                            }
                        }
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

                            DialogResult eResult = Program.GetFormForDialog(_objCharacter).DoThreadSafeFunc(x =>
                            {
                                using (SelectNumber frmPickNumber
                                       = new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
                                       {
                                           Minimum = decMin,
                                           Maximum = decMax,
                                           Description = string.Format(
                                               GlobalSettings.CultureInfo,
                                               LanguageManager.GetString("String_SelectVariableCost"),
                                               CurrentDisplayNameShort),
                                           AllowCancel = false
                                       })
                                {
                                    if (frmPickNumber.ShowDialogSafe(x) != DialogResult.Cancel)
                                        _strCost = frmPickNumber.SelectedValue.ToString(GlobalSettings
                                            .InvariantCultureInfo);
                                    return frmPickNumber.DialogResult;
                                }
                            });
                            if (eResult == DialogResult.Cancel)
                            {
                                _guiID = Guid.Empty;
                                return;
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
                    XmlDocument objXmlWeaponDocument = _objCharacter.LoadData("weapons.xml");

                    // More than one Weapon can be added, so loop through all occurrences.
                    foreach (XmlNode objXmlAddWeapon in objXmlCyberware.SelectNodes("addweapon"))
                    {
                        string strLoopID = objXmlAddWeapon.InnerText;
                        XmlNode objXmlWeapon = strLoopID.IsGuid()
                            ? objXmlWeaponDocument.SelectSingleNode(
                                "/chummer/weapons/weapon[id = " + strLoopID.CleanXPath() + ']')
                            : objXmlWeaponDocument.SelectSingleNode(
                                "/chummer/weapons/weapon[name = " + strLoopID.CleanXPath() + ']');

                        if (objXmlWeapon != null)
                        {
                            Weapon objGearWeapon = new Weapon(_objCharacter)
                            {
                                ParentVehicle = ParentVehicle
                            };
                            int intAddWeaponRating = 0;
                            string strLoopRating = objXmlAddWeapon.Attributes["rating"]?.InnerText;
                            if (!string.IsNullOrEmpty(strLoopRating))
                            {
                                strLoopRating = strLoopRating.CheapReplace("{Rating}",
                                    () => Rating.ToString(
                                        GlobalSettings.InvariantCultureInfo));
                                int.TryParse(strLoopRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                    out intAddWeaponRating);
                            }

                            objGearWeapon.Create(objXmlWeapon, lstWeapons, blnCreateChildren, blnCreateImprovements,
                                blnSkipSelectForms, intAddWeaponRating);
                            objGearWeapon.ParentID = InternalId;
                            objGearWeapon.Cost = "0";

                            if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID))
                                lstWeapons.Add(objGearWeapon);
                        }
                    }

                    string strWeaponId = Parent?.WeaponID;
                    if (!string.IsNullOrEmpty(strWeaponId) && strWeaponId != Guid.Empty.ToString())
                    {
                        Weapon objWeapon = ParentVehicle != null
                            ? ParentVehicle.Weapons.FindById(strWeaponId)
                            : _objCharacter.Weapons.FindById(strWeaponId);

                        if (objWeapon != null)
                        {
                            foreach (XmlNode objXml in objXmlCyberware.SelectNodes("addparentweaponaccessory"))
                            {
                                string strLoopID = objXml.InnerText;
                                XmlNode objXmlAccessory = strLoopID.IsGuid()
                                    ? objXmlWeaponDocument.SelectSingleNode(
                                        "/chummer/accessories/accessory[id = " + strLoopID.CleanXPath() + ']')
                                    : objXmlWeaponDocument.SelectSingleNode(
                                        "/chummer/accessories/accessory[name = " + strLoopID.CleanXPath() + ']');

                                if (objXmlAccessory == null) continue;
                                WeaponAccessory objGearWeapon = new WeaponAccessory(_objCharacter);
                                int intAddWeaponRating = 0;
                                string strLoopRating = objXml.Attributes["rating"]?.InnerText;
                                if (!string.IsNullOrEmpty(strLoopRating))
                                {
                                    strLoopRating = strLoopRating.CheapReplace("{Rating}",
                                        () => Rating.ToString(
                                            GlobalSettings.InvariantCultureInfo));
                                    int.TryParse(strLoopRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                        out intAddWeaponRating);
                                }

                                objGearWeapon.Create(objXmlAccessory, new Tuple<string, string>("", ""),
                                    intAddWeaponRating,
                                    blnSkipSelectForms, true, blnCreateImprovements);
                                objGearWeapon.Cost = "0";
                                objGearWeapon.ParentID = InternalId;
                                objGearWeapon.Parent = objWeapon;

                                if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponAccessoryID))
                                    objWeapon.WeaponAccessories.Add(objGearWeapon);
                            }
                        }
                    }

                    // Add Drone Bodyparts if applicable.
                    XmlDocument objXmlVehicleDocument = _objCharacter.LoadData("vehicles.xml");

                    // More than one Weapon can be added, so loop through all occurrences.
                    foreach (XmlNode xmlAddVehicle in objXmlCyberware.SelectNodes("addvehicle"))
                    {
                        string strLoopID = xmlAddVehicle.InnerText;
                        XmlNode xmlVehicle = strLoopID.IsGuid()
                            ? objXmlVehicleDocument.SelectSingleNode(
                                "/chummer/vehicles/vehicle[id = " + strLoopID.CleanXPath() + ']')
                            : objXmlVehicleDocument.SelectSingleNode(
                                "/chummer/vehicles/vehicle[name = " + strLoopID.CleanXPath() + ']');

                        if (xmlVehicle != null)
                        {
                            Vehicle objVehicle = new Vehicle(_objCharacter);
                            objVehicle.Create(xmlVehicle, blnSkipSelectForms, true, blnCreateImprovements,
                                blnSkipSelectForms);
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
                    string strParentSide = Parent?.Location;
                    if (!string.IsNullOrEmpty(strParentSide))
                    {
                        _strLocation = strParentSide;
                    }
                    else
                    {
                        if (!GetValidLimbSlot(objXmlCyberware.CreateNavigator()))
                            return;
                    }
                }

                // If the piece grants a bonus, pass the information to the Improvement Manager.
                // Modular cyberlimbs only get their bonuses applied when they are equipped onto a limb, so we're skipping those here
                if ((Bonus != null || PairBonus != null) && !blnSkipSelectForms)
                {
                    if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                        ImprovementManager.ForcedValue = _strForced;

                    if (Bonus != null && !ImprovementManager.CreateImprovements(_objCharacter, objSource,
                            _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, Rating,
                            CurrentDisplayNameShort, blnCreateImprovements))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

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
                            intCount = intCount > 0 ? 1 : 0;
                        }

                        if ((intCount & 1) == 1)
                        {
                            if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                                ImprovementManager.ForcedValue = _strForced;
                            else if (Bonus != null && !string.IsNullOrEmpty(_strExtra))
                                ImprovementManager.ForcedValue = _strExtra;
                            if (!ImprovementManager.CreateImprovements(_objCharacter, objSource,
                                    _guiID.ToString("D", GlobalSettings.InvariantCultureInfo) + "Pair", PairBonus,
                                    Rating,
                                    CurrentDisplayNameShort, blnCreateImprovements))
                            {
                                _guiID = Guid.Empty;
                                return;
                            }
                        }
                    }
                }

                // Retrieve the Bioware or Cyberware ESS Cost Multiplier. Bioware Modifiers do not apply to Genetech.
                if (this.GetNodeXPath()?.SelectSingleNode("forcegrade")?.Value != "None")
                {
                    switch (_eImprovementSource)
                    {
                        // Apply the character's Cyberware Essence cost multiplier if applicable.
                        case Improvement.ImprovementSource.Cyberware:
                        {
                            if (ImprovementManager.ValueOf(_objCharacter,
                                    Improvement.ImprovementType.CyberwareEssCostNonRetroactive,
                                    out List<Improvement> lstUsedImprovements) != 0)
                            {
                                decimal decMultiplier = 1;
                                decMultiplier = lstUsedImprovements.Aggregate(decMultiplier,
                                    (current, objImprovement) =>
                                        current - (1m - objImprovement.Value / 100m));
                                _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                            }

                            if (ImprovementManager.ValueOf(_objCharacter,
                                    Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive,
                                    out lstUsedImprovements) != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
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
                            if (ImprovementManager.ValueOf(_objCharacter,
                                    Improvement.ImprovementType.BiowareEssCostNonRetroactive,
                                    out List<Improvement> lstUsedImprovements) != 0)
                            {
                                decimal decMultiplier = 1;
                                decMultiplier = lstUsedImprovements.Aggregate(decMultiplier,
                                    (current, objImprovement) =>
                                        current - (1m - objImprovement.Value / 100m));
                                _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                            }

                            if (ImprovementManager.ValueOf(_objCharacter,
                                    Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive,
                                    out lstUsedImprovements) != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                {
                                    _decExtraESSMultiplicativeMultiplier *=
                                        objImprovement.Value / 100m;
                                }
                            }

                            break;
                        }
                    }
                }

                if (blnCreateChildren)
                    CreateChildren(objXmlCyberware, objGrade, lstWeapons, lstVehicles, blnCreateImprovements);

                if (!string.IsNullOrEmpty(_strPlugsIntoModularMount))
                    ChangeModularEquip(false);

                if (blnCreateImprovements)
                    RefreshWirelessBonuses();
            }
            finally
            {
                _blnDoPropertyChangedInCollectionChanged = true;
                if (Children.Count > 0)
                    CyberwareChildrenOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children));
            }
        }

        public bool GetValidLimbSlot(XPathNavigator xpnCyberware)
        {
            using (SelectSide frmPickSide = new SelectSide
            {
                Description =
                    string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("Label_SelectSide"),
                        CurrentDisplayNameShort)
            })
            {
                string strForcedSide = string.Empty;
                if (_strForced == "Right" || _strForced == "Left")
                    strForcedSide = _strForced;
                if (string.IsNullOrEmpty(strForcedSide) && ParentVehicle == null)
                {
                    IList<Cyberware> lstCyberwareToCheck =
                        Parent == null ? _objCharacter.Cyberware : Parent.Children;
                    Dictionary<string, int> dicNumLeftMountBlockers = new Dictionary<string, int>(6);
                    Dictionary<string, int> dicNumRightMountBlockers = new Dictionary<string, int>(6);
                    foreach (Cyberware objCheckCyberware in lstCyberwareToCheck)
                    {
                        if (string.IsNullOrEmpty(objCheckCyberware.BlocksMounts)) continue;
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
                                continue;
                        }
                        foreach (string strBlockMount in objCheckCyberware.BlocksMounts.SplitNoAlloc(',',
                            StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (dicToUse.TryGetValue(strBlockMount, out int intExistingLimbCount))
                                dicToUse[strBlockMount] = intExistingLimbCount + objCheckCyberware.LimbSlotCount;
                            else
                                dicToUse.Add(strBlockMount, objCheckCyberware.LimbSlotCount);
                        }
                    }

                    bool blnAllowLeft = true;
                    bool blnAllowRight = true;
                    // Potentially expensive checks that can (and therefore should) be parallelized. Normally, this would just be a Parallel.Invoke,
                    // but we want to allow UI messages to happen, just in case this is called on the Main Thread and another thread wants to show a message box.
                    // Not using async-await because this is trivial code and I do not want to infect everything that calls this with async as well.
                    Utils.RunWithoutThreadLock(
                        () =>
                        {
                            blnAllowLeft = xpnCyberware.RequirementsMet(_objCharacter, Parent, string.Empty, string.Empty,
                                               string.Empty, "Left");
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
                            if (string.IsNullOrEmpty(BlocksMounts))
                                return;
                            using (new FetchSafelyFromPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setBlocksMounts))
                            {
                                setBlocksMounts.AddRange(BlocksMounts
                                                             .SplitNoAlloc(
                                                                 ',', StringSplitOptions.RemoveEmptyEntries));
                                foreach (Cyberware x in lstCyberwareToCheck)
                                {
                                    if (string.IsNullOrEmpty(x.HasModularMount))
                                        continue;
                                    if (x.Location != "Left")
                                        continue;
                                    if (!setBlocksMounts.Contains(x.HasModularMount))
                                        continue;
                                    string strLimbTypeOfMount = MountToLimbType(x.HasModularMount);
                                    if (string.IsNullOrEmpty(strLimbTypeOfMount))
                                    {
                                        blnAllowLeft = false;
                                        return;
                                    }

                                    int intLimbSlotCount = LimbSlotCount;
                                    if (dicNumLeftMountBlockers.TryGetValue(x.HasModularMount, out intNumBlockers))
                                        intLimbSlotCount += intNumBlockers;

                                    if (_objCharacter.LimbCount(strLimbTypeOfMount) / 2 < intLimbSlotCount)
                                    {
                                        blnAllowLeft = false;
                                        return;
                                    }
                                }
                            }
                        },
                        () =>
                        {
                            blnAllowRight = xpnCyberware.RequirementsMet(_objCharacter, Parent, string.Empty, string.Empty,
                                               string.Empty, "Right");
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
                            if (string.IsNullOrEmpty(BlocksMounts))
                                return;
                            using (new FetchSafelyFromPool<HashSet<string>>(
                                       Utils.StringHashSetPool, out HashSet<string> setBlocksMounts))
                            {
                                setBlocksMounts.AddRange(BlocksMounts
                                                             .SplitNoAlloc(
                                                                 ',', StringSplitOptions.RemoveEmptyEntries));
                                foreach (Cyberware x in lstCyberwareToCheck)
                                {
                                    if (string.IsNullOrEmpty(x.HasModularMount))
                                        continue;
                                    if (x.Location != "Right")
                                        continue;
                                    if (!setBlocksMounts.Contains(x.HasModularMount))
                                        continue;
                                    string strLimbTypeOfMount = MountToLimbType(x.HasModularMount);
                                    if (string.IsNullOrEmpty(strLimbTypeOfMount))
                                    {
                                        blnAllowRight = false;
                                        return;
                                    }

                                    int intLimbSlotCount = LimbSlotCount;
                                    if (dicNumRightMountBlockers.TryGetValue(x.HasModularMount, out intNumBlockers))
                                        intLimbSlotCount += intNumBlockers;

                                    if (_objCharacter.LimbCount(strLimbTypeOfMount) / 2 < intLimbSlotCount)
                                    {
                                        blnAllowRight = false;
                                        return;
                                    }
                                }
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
                    frmPickSide.ForceValue(strForcedSide);
                // Make sure the dialogue window was not canceled.
                else if (frmPickSide.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                {
                    _guiID = Guid.Empty;
                    return false;
                }

                _strLocation = frmPickSide.SelectedSide;
                return true;
            }
        }

        private void CreateChildren(XmlNode objParentNode, Grade objGrade, IList<Weapon> lstWeapons,
            IList<Vehicle> objVehicles, bool blnCreateImprovements = true)
        {
            // If we've just added a new base item, see if there are any subsystems that should automatically be added.
            XmlNode xmlSubsystemsNode = objParentNode["subsystems"];
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
                            XmlNode objXmlSubsystem = objXmlDocument.SelectSingleNode("/chummer/cyberwares/cyberware[name = " + strName.CleanXPath() + ']');

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
                            XmlNode objXmlSubsystem = objXmlDocument.SelectSingleNode("/chummer/biowares/bioware[name = " + strName.CleanXPath() + ']');

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

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("cyberware");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("limbslot", _strLimbSlot);
            objWriter.WriteElementString("limbslotcount", _strLimbSlotCount);
            objWriter.WriteElementString("inheritattributes", _blnInheritAttributes.ToString(GlobalSettings.InvariantCultureInfo));
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
            objWriter.WriteElementString("extraessadditivemultiplier",
                _decExtraESSAdditiveMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("extraessmultiplicativemultiplier",
                _decExtraESSMultiplicativeMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("forcegrade", _strForceGrade);
            objWriter.WriteElementString("matrixcmfilled",
                _intMatrixCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmbonus", _intMatrixCMBonus.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("prototypetranshuman", (_blnPrototypeTranshuman && _objCharacter.IsPrototypeTranshuman).ToString(GlobalSettings.InvariantCultureInfo));
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
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo));
            if (_guiVehicleID != Guid.Empty)
                objWriter.WriteElementString("vehicleguid", _guiVehicleID.ToString("D", GlobalSettings.InvariantCultureInfo));

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

            objWriter.WriteStartElement("children");
            foreach (Cyberware objChild in _lstChildren)
            {
                objChild.Save(objWriter);
            }

            objWriter.WriteEndElement();

            #endregion Children

            #region Gear

            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    objGear.Save(objWriter);
                }

                objWriter.WriteEndElement();
            }

            #endregion Gear

            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("addtoparentess", _blnAddToParentESS.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("addtoparentcapacity", _blnAddToParentCapacity.ToString(GlobalSettings.InvariantCultureInfo));

            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
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
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether this is a copy of an existing cyberware being loaded.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
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
                Lazy<XmlNode> objMyNode = new Lazy<XmlNode>(this.GetNode);
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
                if (_strName == "Reflex Recorder (Skill)" && _objCharacter.LastSavedVersion <= new Version(5, 198, 31))
                {
                    // This step is needed in case there's a custom data file that has the name "Reflex Recorder (Skill)", in which case we wouldn't want to rename the 'ware
                    XPathNavigator xmlReflexRecorderNode = _eImprovementSource == Improvement.ImprovementSource.Bioware
                        ? _objCharacter.LoadDataXPath("bioware.xml")
                            .SelectSingleNode("/chummer/biowares/bioware[name = \"Reflex Recorder (Skill)\"]")
                        : _objCharacter.LoadDataXPath("cyberware.xml")
                            .SelectSingleNode("/chummer/cyberwares/cyberware[name = \"Reflex Recorder (Skill)\"]");
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
                    _strPlugsIntoModularMount = objMyNode.Value?["plugsintomodularmount"]?.InnerText ?? string.Empty;
                if (!objNode.TryGetStringFieldQuickly("blocksmounts", ref _strBlocksMounts))
                    _strBlocksMounts = objMyNode.Value?["blocksmounts"]?.InnerText ?? string.Empty;
                objNode.TryGetStringFieldQuickly("forced", ref _strForced);
                objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
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

                objNode.TryGetStringFieldQuickly("subsystems", ref _strAllowSubsystems);
                if (objNode["grade"] != null)
                    _objGrade = Grade.ConvertToCyberwareGrade(objNode["grade"].InnerText, _eImprovementSource,
                        _objCharacter);
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
                objNode.TryGetDecFieldQuickly("extraessadditivemultiplier", ref _decExtraESSAdditiveMultiplier);
                objNode.TryGetDecFieldQuickly("extraessmultiplicativemultiplier",
                    ref _decExtraESSMultiplicativeMultiplier);
                objNode.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);
                if (_objCharacter.IsPrototypeTranshuman && SourceType == Improvement.ImprovementSource.Bioware)
                    objNode.TryGetBoolFieldQuickly("prototypetranshuman", ref _blnPrototypeTranshuman);

                _nodBonus = objNode["bonus"] ?? objMyNode.Value?["bonus"];
                _nodPairBonus = objNode["pairbonus"] ?? objMyNode.Value?["pairbonus"];
                XmlNode xmlPairIncludeNode = objNode["pairinclude"];
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
                        _objGrade = Grade.ConvertToCyberwareGrade(_strForceGrade, _eImprovementSource, _objCharacter);
                }

                if (objNode["weaponguid"] != null && !Guid.TryParse(objNode["weaponguid"].InnerText, out _guiWeaponID))
                {
                    _guiWeaponID = Guid.Empty;
                }

                if (objNode["vehicleguid"] != null &&
                    !Guid.TryParse(objNode["vehicleguid"].InnerText, out _guiVehicleID))
                {
                    _guiVehicleID = Guid.Empty;
                }

                if (objNode.InnerXml.Contains("<cyberware>"))
                {
                    XmlNodeList nodChildren = objNode.SelectNodes("children/cyberware");
                    foreach (XmlNode nodChild in nodChildren)
                    {
                        Cyberware objChild = new Cyberware(_objCharacter);
                        objChild.Load(nodChild, blnCopy);
                        _lstChildren.Add(objChild);
                    }
                }

                if (objNode.InnerXml.Contains("<gears>"))
                {
                    XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
                    foreach (XmlNode nodChild in nodChildren)
                    {
                        Gear objGear = new Gear(_objCharacter);
                        objGear.Load(nodChild, blnCopy);
                        _lstGear.Add(objGear);
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

                bool blnIsActive = false;
                if (objNode.TryGetBoolFieldQuickly("active", ref blnIsActive) && blnIsActive)
                    this.SetActiveCommlink(_objCharacter, true);
                if (blnCopy)
                {
                    this.SetHomeNode(_objCharacter, false);
                }
                else
                {
                    bool blnIsHomeNode = false;
                    if (objNode.TryGetBoolFieldQuickly("homenode", ref blnIsHomeNode) && blnIsHomeNode)
                    {
                        this.SetHomeNode(_objCharacter, true);
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
                            ImprovementManager.ForcedValue = _strForced;

                        if (Bonus != null)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, _eImprovementSource,
                                _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, Rating,
                                CurrentDisplayNameShort);
                        }

                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                            _strExtra = ImprovementManager.SelectedValue;

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
                                intCount = intCount > 0 ? 1 : 0;
                            }

                            if ((intCount & 1) == 1)
                            {
                                ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId + "Pair",
                                    PairBonus, Rating, CurrentDisplayNameShort);
                            }
                        }

                        RefreshWirelessBonuses();
                    }

                    if (!IsModularCurrentlyEquipped)
                    {
                        ChangeModularEquip(false);
                    }
                }
            }
            finally
            {
                _blnDoPropertyChangedInCollectionChanged = true;
                if (Children.Count > 0)
                    CyberwareChildrenOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Children));
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>obv
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            // <cyberware>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("cyberware");
            try
            {
                if ((string.IsNullOrWhiteSpace(LimbSlot) && _strCategory != "Cyberlimb") || CyberlimbAttributeAbbrevs.Count == 0)
                {
                    await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint));
                    await objWriter.WriteElementStringAsync("name_english", Name);
                }
                else
                {
                    string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguageToPrint);
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdName))
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdNameEnglish))
                    {
                        sbdName.Append(await DisplayNameShortAsync(strLanguageToPrint)).Append(strSpace).Append('(');
                        sbdNameEnglish.Append(Name).Append(strSpace).Append('(');
                        bool blnFirst = true;
                        foreach (string strAbbrev in CyberlimbAttributeAbbrevs)
                        {
                            int intTotalValue = GetAttributeTotalValue(strAbbrev);
                            if (!blnFirst)
                            {
                                sbdName.Append(',').Append(strSpace);
                                sbdNameEnglish.Append(',').Append(strSpace);
                            }
                            else
                                blnFirst = false;

                            CharacterAttrib objLoopAttribute = _objCharacter.GetAttribute(strAbbrev);
                            if (objLoopAttribute != null)
                                sbdName.Append(await objLoopAttribute.GetDisplayAbbrevAsync(strLanguageToPrint));
                            else
                                sbdName.Append(strAbbrev);
                            sbdName.Append(strSpace).Append(intTotalValue.ToString(objCulture));
                            sbdNameEnglish.Append(strAbbrev).Append(strSpace)
                                          .Append(intTotalValue.ToString(GlobalSettings.InvariantCultureInfo));
                        }
                        sbdName.Append(')');
                        sbdNameEnglish.Append(')');
                        await objWriter.WriteElementStringAsync("name", sbdName.ToString());
                        await objWriter.WriteElementStringAsync("name_english", sbdNameEnglish.ToString());
                    }
                }

                await objWriter.WriteElementStringAsync("category", await DisplayCategoryAsync(strLanguageToPrint));
                await objWriter.WriteElementStringAsync("category_english", Category);

                await objWriter.WriteElementStringAsync("ess",
                    CalculatedESS.ToString(_objCharacter.Settings.EssenceFormat, objCulture));
                await objWriter.WriteElementStringAsync("capacity", Capacity);
                await objWriter.WriteElementStringAsync("avail", TotalAvail(objCulture, strLanguageToPrint));
                await objWriter.WriteElementStringAsync("cost", TotalCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture));
                await objWriter.WriteElementStringAsync("owncost", OwnCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture));
                await objWriter.WriteElementStringAsync("weight", TotalWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture));
                await objWriter.WriteElementStringAsync("ownweight", OwnWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture));
                await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint));
                await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint));
                await objWriter.WriteElementStringAsync("rating", Rating.ToString(objCulture));
                await objWriter.WriteElementStringAsync("minrating", MinRating.ToString(objCulture));
                await objWriter.WriteElementStringAsync("maxrating", MaxRating.ToString(objCulture));
                await objWriter.WriteElementStringAsync("ratinglabel", RatingLabel);
                await objWriter.WriteElementStringAsync("allowsubsystems", AllowedSubsystems);
                await objWriter.WriteElementStringAsync("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("grade", await Grade.DisplayNameAsync(strLanguageToPrint));
                await objWriter.WriteElementStringAsync("location", Location);
                await objWriter.WriteElementStringAsync("extra", await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint));
                await objWriter.WriteElementStringAsync("improvementsource", SourceType.ToString());

                await objWriter.WriteElementStringAsync("attack", this.GetTotalMatrixAttribute("Attack").ToString(objCulture));
                await objWriter.WriteElementStringAsync("sleaze", this.GetTotalMatrixAttribute("Sleaze").ToString(objCulture));
                await objWriter.WriteElementStringAsync("dataprocessing", this.GetTotalMatrixAttribute("Data Processing").ToString(objCulture));
                await objWriter.WriteElementStringAsync("firewall", this.GetTotalMatrixAttribute("Firewall").ToString(objCulture));
                await objWriter.WriteElementStringAsync("devicerating", this.GetTotalMatrixAttribute("Device Rating").ToString(objCulture));
                await objWriter.WriteElementStringAsync("programlimit", this.GetTotalMatrixAttribute("Program Limit").ToString(objCulture));
                await objWriter.WriteElementStringAsync("iscommlink", IsCommlink.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
                await objWriter.WriteElementStringAsync("conditionmonitor", MatrixCM.ToString(objCulture));
                await objWriter.WriteElementStringAsync("matrixcmfilled", MatrixCMFilled.ToString(objCulture));

                if (GearChildren.Count > 0)
                {
                    // <gears>
                    XmlElementWriteHelper objGearsElement = await objWriter.StartElementAsync("gears");
                    try
                    {
                        foreach (Gear objGear in GearChildren)
                        {
                            await objGear.Print(objWriter, objCulture, strLanguageToPrint);
                        }
                    }
                    finally
                    {
                        // </gears>
                        await objGearsElement.DisposeAsync();
                    }
                }

                // <children>
                XmlElementWriteHelper objChildrenElement = await objWriter.StartElementAsync("children");
                try
                {
                    foreach (Cyberware objChild in Children)
                    {
                        await objChild.Print(objWriter, objCulture, strLanguageToPrint);
                    }
                }
                finally
                {
                    // </children>
                    await objChildrenElement.DisposeAsync();
                }
                
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", Notes);
            }
            finally
            {
                // </cyberware>
                await objBaseElement.DisposeAsync();
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Cyberware in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Guid of a Cyberware Weapon.
        /// </summary>
        public string WeaponID
        {
            get => _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo);
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiWeaponID = guiTemp;
            }
        }

        /// <summary>
        /// Guid of a Cyberware Weapon Accessory.
        /// </summary>
        public string WeaponAccessoryID
        {
            get => _guiWeaponAccessoryID.ToString("D", GlobalSettings.InvariantCultureInfo);
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiWeaponAccessoryID = guiTemp;
            }
        }

        /// <summary>
        /// Guid of a Cyberware Drone/Vehicle.
        /// </summary>
        public string VehicleID
        {
            get => _guiVehicleID.ToString("D", GlobalSettings.InvariantCultureInfo);
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiVehicleID = guiTemp;
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// Bonus node from the XML file that only activates for each pair of 'ware.
        /// </summary>
        public XmlNode PairBonus
        {
            get => _nodPairBonus;
            set => _nodPairBonus = value;
        }

        /// <summary>
        /// Bonus node from the XML file that only activates for each pair of 'ware.
        /// </summary>
        public XmlNode WirelessPairBonus
        {
            get => _nodWirelessPairBonus;
            set => _nodWirelessPairBonus = value;
        }

        /// <summary>
        /// Wireless bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set => _nodWirelessBonus = value;
        }

        /// <summary>
        /// Whether the Cyberware's Wireless is enabled
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set
            {
                if (_blnWirelessOn == value)
                    return;
                _blnWirelessOn = value;
                RefreshWirelessBonuses();
            }
        }

        /// <summary>
        /// AllowGear node from the XML file.
        /// </summary>
        public XmlNode AllowGear
        {
            get => _nodAllowGear;
            set => _nodAllowGear = value;
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public Improvement.ImprovementSource SourceType
        {
            get => _eImprovementSource;
            set
            {
                if (_eImprovementSource == value)
                    return;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
                _eImprovementSource = value;
            }
        }

        /// <summary>
        /// Cyberware name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName == value) return;
                string strOldValue = _strName;
                _lstIncludeInPairBonus.Remove(_strName);
                _lstIncludeInPairBonus.Add(value);
                _lstIncludeInWirelessPairBonus.Remove(_strName);
                _lstIncludeInWirelessPairBonus.Add(value);
                _strName = value;
                if (_objParent?.Category != "Cyberlimb" || _objParent.Parent?.InheritAttributes == false ||
                    _objParent.ParentVehicle != null || _objCharacter.Settings.DontUseCyberlimbCalculation ||
                    string.IsNullOrWhiteSpace(_objParent.LimbSlot) ||
                    _objCharacter.Settings.ExcludeLimbSlot.Contains(_objParent.LimbSlot))
                    return;
                // Note: Movement is always handled whenever AGI or STR is changed, regardless of whether or not we use cyberleg movement
                foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in s_AttributeAffectingCyberwares)
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

        public bool InheritAttributes => _blnInheritAttributes;

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async ValueTask<string> DisplayNameShortAsync(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage);
            return objNode != null ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate"))?.Value ?? Name : Name;
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public static Guid EssenceHoleGUID { get; } = new Guid("b57eadaa-7c3b-4b80-8d79-cbbd922c1196");
        public static Guid EssenceAntiHoleGUID { get; } = new Guid("961eac53-0c43-4b19-8741-2872177a3a4c");

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (Rating > 0 && SourceID != EssenceHoleGUID && SourceID != EssenceAntiHoleGUID)
            {
                strReturn += strSpace + '(' + LanguageManager.GetString(RatingLabel, strLanguage) + strSpace + Rating.ToString(objCulture) + ')';
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

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async ValueTask<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage);
            if (Rating > 0 && SourceID != EssenceHoleGUID && SourceID != EssenceAntiHoleGUID)
            {
                strReturn += strSpace + '(' + await LanguageManager.GetStringAsync(RatingLabel, strLanguage) + strSpace + Rating.ToString(objCulture) + ')';
            }

            if (!string.IsNullOrEmpty(Extra))
            {
                // Attempt to retrieve the CharacterAttribute name.
                strReturn += strSpace + '(' + await _objCharacter.TranslateExtraAsync(Extra, strLanguage) + ')';
            }

            if (!string.IsNullOrEmpty(Location))
            {
                string strSide = string.Empty;
                switch (Location)
                {
                    case "Left":
                        strSide = await LanguageManager.GetStringAsync("String_Improvement_SideLeft", strLanguage);
                        break;

                    case "Right":
                        strSide = await LanguageManager.GetStringAsync("String_Improvement_SideRight", strLanguage);
                        break;
                }
                if (!string.IsNullOrEmpty(strSide))
                    strReturn += strSpace + '(' + strSide + ')';
            }

            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath(SourceType == Improvement.ImprovementSource.Cyberware ? "cyberware.xml" : "bioware.xml", strLanguage)
                .SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public async ValueTask<string> DisplayCategoryAsync(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return (await _objCharacter.LoadDataXPathAsync(SourceType == Improvement.ImprovementSource.Cyberware ? "cyberware.xml" : "bioware.xml", strLanguage))
                                .SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Cyberware category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set
            {
                if (_strCategory != value)
                {
                    string strOldValue = _strCategory;
                    _strCategory = value;
                    if ((value == "Cyberlimb" || strOldValue == "Cyberlimb") && Parent?.InheritAttributes != false &&
                        ParentVehicle == null && !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                        !string.IsNullOrWhiteSpace(LimbSlot) &&
                        !_objCharacter.Settings.ExcludeLimbSlot.Contains(LimbSlot))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
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
            get => _strLimbSlot;
            set
            {
                if (_strLimbSlot != value)
                {
                    string strOldValue = _strLimbSlot;
                    _strLimbSlot = value;
                    if (Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                        !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                        (!string.IsNullOrWhiteSpace(value) && !_objCharacter.Settings.ExcludeLimbSlot.Contains(value)) ||
                        (!string.IsNullOrWhiteSpace(strOldValue) &&
                         !_objCharacter.Settings.ExcludeLimbSlot.Contains(strOldValue)))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
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
        /// The amount of body "slots" a Cyberlimb occupies.
        /// </summary>
        public int LimbSlotCount
        {
            get
            {
                if (_strLimbSlotCount == "all")
                {
                    return _objCharacter.LimbCount(LimbSlot);
                }

                int.TryParse(_strLimbSlotCount, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
                return intReturn;
            }
            set
            {
                string strNewValue = value.ToString(GlobalSettings.InvariantCultureInfo);
                if (_strLimbSlotCount != strNewValue)
                {
                    _strLimbSlotCount = strNewValue;
                    if (Category == "Cyberlimb" && Parent?.InheritAttributes != false && ParentVehicle == null &&
                        !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                        !string.IsNullOrWhiteSpace(LimbSlot) &&
                        !_objCharacter.Settings.ExcludeLimbSlot.Contains(LimbSlot))
                    {
                        foreach (CharacterAttrib objCharacterAttrib in _objCharacter.AttributeSection.AttributeList
                            .Concat(_objCharacter.AttributeSection.SpecialAttributeList))
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
        /// How many limbs does this cyberware have?
        /// </summary>
        public int GetCyberlimbCount(ICollection<string> lstExcludeLimbs = null)
        {
            int intCount = 0;
            if (!string.IsNullOrEmpty(LimbSlot) && lstExcludeLimbs?.All(l => l != LimbSlot) != false)
            {
                intCount += LimbSlotCount;
            }
            else
            {
                foreach (Cyberware objCyberwareChild in Children)
                {
                    intCount += objCyberwareChild.GetCyberlimbCount(lstExcludeLimbs);
                }
            }

            return intCount;
        }

        /// <summary>
        /// The location of a Cyberlimb (Left or Right).
        /// </summary>
        public string Location
        {
            get => _strLocation;
            set => _strLocation = value;
        }

        /// <summary>
        /// Original Forced Extra string associated with the 'ware.
        /// </summary>
        public string Forced
        {
            get => _strForced;
            set => _strForced = value;
        }

        /// <summary>
        /// Extra string associated with the 'ware.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = value;
        }

        /// <summary>
        /// Essence cost of the Cyberware.
        /// </summary>
        public string ESS
        {
            get => _strESS;
            set => _strESS = value;
        }

        /// <summary>
        /// Cyberware capacity.
        /// </summary>
        public string Capacity
        {
            get => _strCapacity;
            set => _strCapacity = value;
        }

        /// <summary>
        /// Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// Cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Weight.
        /// </summary>
        public string Weight
        {
            get => _strWeight;
            set => _strWeight = value;
        }

        /// <summary>
        /// Sourcebook.
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
            string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public async ValueTask<string> DisplayPageAsync(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage);
            string s = objNode != null
                ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("altpage"))?.Value ?? Page
                : Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = SourceString.GetSourceString(Source,
                        DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo,
                        _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// ID of the object that added this cyberware (if any).
        /// </summary>
        public string ParentID
        {
            get => _strParentID;
            set => _strParentID = value;
        }

        /// <summary>
        /// The modular mount this cyberware contains. Returns string.Empty if it contains no mount.
        /// </summary>
        public string HasModularMount
        {
            get => _strHasModularMount;
            set => _strHasModularMount = value;
        }

        /// <summary>
        /// What modular mount this cyberware plugs into. Returns string.Empty if it doesn't plug into a modular mount.
        /// </summary>
        public string PlugsIntoModularMount
        {
            get => _strPlugsIntoModularMount;
            set => _strPlugsIntoModularMount = value;
        }

        /// <summary>
        /// Returns whether the 'ware is currently equipped (with improvements applied) or not.
        /// </summary>
        public bool IsModularCurrentlyEquipped
        {
            get
            {
                // Cyberware always equipped if it's not a modular one
                bool blnReturn = string.IsNullOrEmpty(PlugsIntoModularMount);
                Cyberware objCurrentParent = Parent;
                // If top-level parent is one that has a modular mount but also does not plug into another modular mount itself, then return true, otherwise return false
                while (objCurrentParent != null)
                {
                    if (!string.IsNullOrEmpty(objCurrentParent.HasModularMount))
                        blnReturn = true;
                    if (!string.IsNullOrEmpty(objCurrentParent.PlugsIntoModularMount))
                        blnReturn = false;
                    objCurrentParent = objCurrentParent.Parent;
                }

                return blnReturn;
            }
        }

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this weapon accessory.
        /// </summary>
        public void RefreshWirelessBonuses()
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText) || !string.IsNullOrEmpty(WirelessPairBonus?.InnerText))
            {
                if (WirelessOn && ParentVehicle == null && IsModularCurrentlyEquipped && Parent?.WirelessOn != false)
                {
                    if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
                    {
                        if (WirelessBonus?.SelectSingleNode("@mode")?.Value == "replace")
                        {
                            ImprovementManager.DisableImprovements(_objCharacter,
                                                                   _objCharacter.Improvements
                                                                       .Where(x => x.ImproveSource == SourceType
                                                                                  && x.SourceName == InternalId));
                        }

                        string strSourceNameToUse = InternalId + "Wireless";
                        ImprovementManager.RemoveImprovements(_objCharacter,
                                                              _objCharacter.Improvements
                                                                           .Where(x => x.ImproveSource == SourceType && x.SourceName == strSourceNameToUse)
                                                                           .ToList());
                        ImprovementManager.CreateImprovements(_objCharacter, _eImprovementSource,
                                                              strSourceNameToUse, WirelessBonus, Rating,
                                                              CurrentDisplayNameShort);

                        if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                            _strExtra = ImprovementManager.SelectedValue;
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
                            intCount = intCount > 0 ? 1 : 0;
                        }

                        if (intCount % 2 == 1)
                        {
                            if (WirelessPairBonus?.SelectSingleNode("@mode")?.Value == "replace")
                            {
                                ImprovementManager.DisableImprovements(_objCharacter,
                                                                       _objCharacter.Improvements
                                                                           .Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId)
                                                                           .ToList());
                            }

                            string strSourceNameToUse = InternalId + "WirelessPair";
                            ImprovementManager.RemoveImprovements(_objCharacter,
                                                                  _objCharacter.Improvements
                                                                               .Where(x => x.ImproveSource == SourceType && x.SourceName == strSourceNameToUse)
                                                                               .ToList());
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType,
                                                                  strSourceNameToUse, WirelessPairBonus,
                                                                  Rating, CurrentDisplayNameShort);
                        }

                        foreach (Cyberware objLoopCyberware in lstPairableCyberwares)
                        {
                            ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType,
                                objLoopCyberware.InternalId + "WirelessPair");
                            if (objLoopCyberware.WirelessPairBonus?.SelectSingleNode("@mode")?.Value == "replace")
                            {
                                ImprovementManager.DisableImprovements(_objCharacter,
                                                                       _objCharacter.Improvements
                                                                           .Where(x => x.ImproveSource == objLoopCyberware.SourceType && x.SourceName == objLoopCyberware.InternalId)
                                                                           .ToList());
                            }

                            // Go down the list and create pair bonuses for every second item
                            if (intCount > 0 && intCount % 2 == 1)
                            {
                                ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                                    objLoopCyberware.InternalId + "WirelessPair",
                                    objLoopCyberware.WirelessPairBonus, objLoopCyberware.Rating,
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
                        if (WirelessBonus.SelectSingleNode("@mode")?.Value == "replace")
                        {
                            ImprovementManager.EnableImprovements(_objCharacter,
                                _objCharacter.Improvements
                                    .Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId));
                        }

                        string strSourceNameToRemove = InternalId + "Wireless";
                        ImprovementManager.RemoveImprovements(_objCharacter,
                            _objCharacter.Improvements
                                .Where(x => x.ImproveSource == SourceType && x.SourceName == strSourceNameToRemove)
                                .ToList());
                    }

                    if (!string.IsNullOrEmpty(WirelessPairBonus?.InnerText))
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "WirelessPair");
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

                        if (WirelessPairBonus.SelectSingleNode("@mode")?.Value == "replace")
                        {
                            ImprovementManager.EnableImprovements(_objCharacter,
                                _objCharacter.Improvements
                                    .Where(x => x.ImproveSource == SourceType && x.SourceName == InternalId));
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
                                    objLoopCyberware.WirelessPairBonus, objLoopCyberware.Rating,
                                    objLoopCyberware.CurrentDisplayNameShort);
                            }

                            --intCount;
                        }
                    }
                }
            }

            foreach (Cyberware objCyberware in Children)
                objCyberware.RefreshWirelessBonuses();
            foreach (Gear objGear in GearChildren)
                objGear.RefreshWirelessBonuses();
        }

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }

        /// <summary>
        /// Equips a piece of modular cyberware, activating the improvements of it and its children. Call after attaching onto objCharacter.Cyberware or a parent
        /// </summary>
        public void ChangeModularEquip(bool blnEquip, bool blnSkipEncumbranceOnPropertyChanged = false)
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
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (WirelessBonus != null && WirelessOn)
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, WirelessBonus, false, Rating, CurrentDisplayNameShort);
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;
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
                        intCount = intCount > 0 ? 1 : 0;
                    }

                    if ((intCount & 1) == 1)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId + "Pair", PairBonus,
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
                            ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                                objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, objLoopCyberware.Rating,
                                objLoopCyberware.CurrentDisplayNameShort);
                        }

                        --intCount;
                    }
                }
            }

            foreach (Gear objChildGear in GearChildren)
                objChildGear.ChangeEquippedStatus(blnEquip, true);

            foreach (Cyberware objChild in Children)
                objChild.ChangeModularEquip(blnEquip, true);

            RefreshWirelessBonuses();

            if (!blnSkipEncumbranceOnPropertyChanged && ParentVehicle == null)
                _objCharacter?.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
        }

        public bool CanRemoveThroughImprovements
        {
            get
            {
                Cyberware objParent = this;
                bool blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                while (objParent.Parent != null && blnNoParentIsModular)
                {
                    objParent = objParent.Parent;
                    blnNoParentIsModular = string.IsNullOrEmpty(objParent.PlugsIntoModularMount);
                }

                return blnNoParentIsModular;
            }
        }

        /// <summary>
        /// Comma-separated list of mount locations with which this 'ware is mutually exclusive.
        /// </summary>
        public string BlocksMounts
        {
            get => _strBlocksMounts;
            set => _strBlocksMounts = value;
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => Math.Max(Math.Min(_intRating, MaxRating), MinRating);
            set
            {
                int intNewValue = Math.Max(Math.Min(value, MaxRating), MinRating);
                if (_intRating == intNewValue)
                    return;
                _intRating = intNewValue;
                if (GearChildren.Count > 0)
                {
                    foreach (Gear objChild in GearChildren.Where(x =>
                        x.MaxRating.Contains("Parent") || x.MinRating.Contains("Parent")))
                    {
                        // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                        int intCurrentRating = objChild.Rating;
                        objChild.Rating = intCurrentRating;
                    }
                }
                DoPropertyChanges(true, false);
            }
        }

        private bool ProcessPropertyChanges { get; set; } = true;

        private void DoPropertyChanges(bool blnDoRating, bool blnDoGrade)
        {
            // Do not do property changes if we're not directly equipped to a character
            if (!ProcessPropertyChanges || (ParentVehicle != null && string.IsNullOrEmpty(PlugsIntoModularMount)))
                return;
            using (new FetchSafelyFromPool<Dictionary<INotifyMultiplePropertyChanged, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertyChanged, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    if ((blnDoGrade || (blnDoRating && (ESS.Contains("Rating") || ESS.Contains("FixedValues")))) &&
                        (Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount))
                    {
                        if (!dicChangedProperties.TryGetValue(_objCharacter, out HashSet<string> setChangedProperties))
                        {
                            setChangedProperties = Utils.StringHashSetPool.Get();
                            dicChangedProperties.Add(_objCharacter, setChangedProperties);
                        }
                        setChangedProperties.Add(EssencePropertyName);
                    }

                    if (blnDoRating)
                    {
                        if (IsModularCurrentlyEquipped &&
                            ParentVehicle == null &&
                            _objParent?.Category == "Cyberlimb" &&
                            _objParent.Parent?.InheritAttributes != false &&
                            _objParent.ParentVehicle == null && !_objCharacter.Settings.DontUseCyberlimbCalculation &&
                            !string.IsNullOrWhiteSpace(_objParent.LimbSlot) &&
                            !_objCharacter.Settings.ExcludeLimbSlot.Contains(_objParent.LimbSlot))
                        {
                            foreach (KeyValuePair<string, IReadOnlyCollection<string>> kvpToCheck in
                                     s_AttributeAffectingCyberwares)
                            {
                                if (!kvpToCheck.Value.Contains(Name))
                                    continue;
                                foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes(kvpToCheck.Key))
                                {
                                    if (!dicChangedProperties.TryGetValue(objCharacterAttrib, out HashSet<string> setChangedProperties))
                                    {
                                        setChangedProperties = Utils.StringHashSetPool.Get();
                                        dicChangedProperties.Add(objCharacterAttrib, setChangedProperties);
                                    }
                                    setChangedProperties.Add(nameof(CharacterAttrib.TotalValue));
                                }
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
                                ImprovementManager.ForcedValue = _strForced;

                            if (Bonus != null)
                            {
                                ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId);
                                ImprovementManager.CreateImprovements(_objCharacter, SourceType,
                                    InternalId, Bonus, Rating, CurrentDisplayNameShort);
                            }

                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                                _strExtra = ImprovementManager.SelectedValue;

                            if (PairBonus != null)
                            {
                                ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "Pair");
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
                                    intCount = intCount > 0 ? 1 : 0;
                                }

                                if ((intCount & 1) == 1)
                                {
                                    ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId + "Pair",
                                        PairBonus, Rating, CurrentDisplayNameShort);
                                }
                            }

                            if (!IsModularCurrentlyEquipped || ParentVehicle != null)
                                ChangeModularEquip(false);
                            else
                                RefreshWirelessBonuses();
                        }
                    }

                    foreach (KeyValuePair<INotifyMultiplePropertyChanged, HashSet<string>> kvpToProcess in dicChangedProperties)
                    {
                        kvpToProcess.Key.OnMultiplePropertyChanged(kvpToProcess.Value.ToList());
                    }
                }
                finally
                {
                    foreach (HashSet<string> setToReturn in dicChangedProperties.Values)
                        Utils.StringHashSetPool.Return(setToReturn);
                }
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
                string strRating = MinRatingString;

                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strRating) && !int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intReturn))
                {
                    strRating = strRating.CheapReplace("MaximumSTR",
                            () => (ParentVehicle != null
                                ? Math.Max(1, ParentVehicle.TotalBody * 2)
                                : _objCharacter.STR.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                        .CheapReplace("MaximumAGI",
                            () => (ParentVehicle != null
                                ? Math.Max(1, ParentVehicle.Pilot * 2)
                                : _objCharacter.AGI.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                        .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalSettings.InvariantCultureInfo))
                        .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalSettings.InvariantCultureInfo));

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strRating, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intReturn = ((double)objProcess).StandardRound();
                }

                return intReturn;
            }
        }

        /// <summary>
        /// String representing minimum rating before it would be computed.
        /// </summary>
        public string MinRatingString
        {
            get => _strMinRating;
            set => _strMinRating = value;
        }

        /// <summary>
        /// Total Maximum Rating.
        /// </summary>
        public int MaxRating
        {
            get
            {
                int intReturn = 0;
                string strRating = MaxRatingString;

                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strRating) && !int.TryParse(strRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intReturn))
                {
                    strRating = strRating.CheapReplace("MaximumSTR",
                            () => (ParentVehicle != null
                                ? Math.Max(1, ParentVehicle.TotalBody * 2)
                                : _objCharacter.STR.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                        .CheapReplace("MaximumAGI",
                            () => (ParentVehicle != null
                                ? Math.Max(1, ParentVehicle.Pilot * 2)
                                : _objCharacter.AGI.TotalMaximum).ToString(GlobalSettings.InvariantCultureInfo))
                        .CheapReplace("MinimumSTR", () => (ParentVehicle?.TotalBody ?? 3).ToString(GlobalSettings.InvariantCultureInfo))
                        .CheapReplace("MinimumAGI", () => (ParentVehicle?.Pilot ?? 3).ToString(GlobalSettings.InvariantCultureInfo));

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strRating, out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intReturn = ((double)objProcess).StandardRound();
                }

                return intReturn;
            }
        }

        /// <summary>
        /// String representing maximum rating before it would be computed.
        /// </summary>
        public string MaxRatingString
        {
            get => _strMaxRating;
            set => _strMaxRating = value;
        }

        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
        }

        /// <summary>
        /// Grade level of the Cyberware.
        /// </summary>
        public Grade Grade
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ForceGrade) && ForceGrade != _objGrade.Name)
                {
                    return Grade.ConvertToCyberwareGrade(ForceGrade, SourceType, _objCharacter);
                }

                return _objGrade;
            }
            set
            {
                if (_objGrade != value && value != null)
                {
                    bool blnGradeEssenceChanged = _objGrade.Essence != value.Essence;
                    _objGrade = value;
                    // Run through all of the child pieces and make sure their Grade matches.
                    foreach (Cyberware objChild in Children)
                    {
                        bool blnOldProcessPropertyChanges = objChild.ProcessPropertyChanges;
                        try
                        {
                            objChild.ProcessPropertyChanges = ProcessPropertyChanges;
                            objChild.Grade = value;
                        }
                        finally
                        {
                            objChild.ProcessPropertyChanges = blnOldProcessPropertyChanges;
                        }
                    }
                    if (blnGradeEssenceChanged)
                        DoPropertyChanges(false, true);
                }
            }
        }

        /// <summary>
        /// The Categories of allowable Subsystems.
        /// </summary>
        public string AllowedSubsystems
        {
            get => _strAllowSubsystems;
            set => _strAllowSubsystems = value;
        }

        /// <summary>
        /// Whether or not the piece of Cyberware is part of a Cyberware Suite.
        /// </summary>
        public bool Suite
        {
            get => _blnSuite;
            set => _blnSuite = value;
        }

        /// <summary>
        /// Essence cost discount.
        /// </summary>
        public int ESSDiscount
        {
            get => _intEssenceDiscount;
            set
            {
                if (_intEssenceDiscount != value)
                {
                    _intEssenceDiscount = value;
                    if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                        ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (additively stacking, starts at 0).
        /// </summary>
        public decimal ExtraESSAdditiveMultiplier
        {
            get => _decExtraESSAdditiveMultiplier;
            set
            {
                if (_decExtraESSAdditiveMultiplier != value)
                {
                    _decExtraESSAdditiveMultiplier = value;
                    if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                        ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (multiplicative stacking, starts at 1).
        /// </summary>
        public decimal ExtraESSMultiplicativeMultiplier
        {
            get => _decExtraESSMultiplicativeMultiplier;
            set
            {
                if (_decExtraESSMultiplicativeMultiplier != value)
                {
                    _decExtraESSMultiplicativeMultiplier = value;
                    if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                        ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Base Physical Boxes. 12 for vehicles, 6 for Drones.
        /// </summary>
        public int BaseMatrixBoxes => 8;

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM => BaseMatrixBoxes + (this.GetTotalMatrixAttribute("Device Rating") + 1) / 2 +
                               TotalBonusMatrixBoxes;

        /// <summary>
        /// Matrix Condition Monitor boxes filled.
        /// </summary>
        public int MatrixCMFilled
        {
            get => _intMatrixCMFilled;
            set => _intMatrixCMFilled = value;
        }

        /// <summary>
        /// A List of child pieces of Cyberware.
        /// </summary>
        public TaggedObservableCollection<Cyberware> Children
        {
            get
            {
                using (EnterReadLock.Enter(_objCharacter.LockObject))
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
                using (EnterReadLock.Enter(_objCharacter.LockObject))
                    return _lstGear;
            }
        }

        /// <summary>
        /// List of names to include in pair bonus
        /// </summary>
        public HashSet<string> IncludePair => _lstIncludeInPairBonus;

        /// <summary>
        /// List of names to include in pair bonus
        /// </summary>
        public HashSet<string> IncludeWirelessPair => _lstIncludeInWirelessPairBonus;

        /// <summary>
        /// Notes.
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
        /// Whether or not the Cyberware's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost;
            set => _blnDiscountCost = value;
        }

        /// <summary>
        /// Whether or not the Cyberware's ESS cost increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public bool AddToParentESS
        {
            get => _blnAddToParentESS;
            set
            {
                if (_blnAddToParentESS != value)
                {
                    bool blnOldValue = _blnAddToParentESS;
                    _blnAddToParentESS = value;
                    if ((Parent == null || AddToParentESS || blnOldValue) &&
                        string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(EssencePropertyName);
                }
            }
        }

        /// <summary>
        /// Whether or not the Cyberware's ESS cost increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public bool AddToParentCapacity
        {
            get => _blnAddToParentCapacity;
            set
            {
                if (_blnAddToParentCapacity != value)
                {
                    bool blnOldValue = _blnAddToParentCapacity;
                    _blnAddToParentCapacity = value;
                    if ((Parent == null || AddToParentCapacity || blnOldValue) &&
                        string.IsNullOrEmpty(PlugsIntoModularMount) && ParentVehicle == null)
                        _objCharacter.OnPropertyChanged(Capacity);
                }
            }
        }

        /// <summary>
        /// Parent Cyberware.
        /// </summary>
        public Cyberware Parent
        {
            get => _objParent;
            set
            {
                if (_objParent != value)
                {
                    bool blnOldEquipped = IsModularCurrentlyEquipped;
                    _objParent = value;
                    ParentVehicle = value?.ParentVehicle;
                    if (IsModularCurrentlyEquipped != blnOldEquipped)
                    {
                        foreach (Gear objGear in GearChildren)
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
        /// Topmost Parent Cyberware.
        /// </summary>
        public Cyberware TopMostParent
        {
            get
            {
                Cyberware objReturn = this;
                while (objReturn.Parent != null)
                    objReturn = objReturn.Parent;
                return objReturn;
            }
        }

        /// <summary>
        /// Vehicle to which this cyberware is attached (if any)
        /// </summary>
        public Vehicle ParentVehicle
        {
            get => _objParentVehicle;
            set
            {
                if (_objParentVehicle != value)
                {
                    _objParentVehicle = value;
                    bool blnEquipped = IsModularCurrentlyEquipped;
                    foreach (Gear objGear in GearChildren)
                    {
                        if (value != null)
                            objGear.ChangeEquippedStatus(false);
                        else if (objGear.Equipped && blnEquipped)
                            objGear.ChangeEquippedStatus(true);
                    }
                }

                foreach (Cyberware objChild in Children)
                    objChild.ParentVehicle = value;
            }
        }

        /// <summary>
        /// Grade that the Cyberware should be forced to use, if applicable.
        /// </summary>
        public string ForceGrade => _strForceGrade;

        /// <summary>
        /// Is the Bioware's cost affected by Prototype Transhuman?
        /// </summary>
        public bool PrototypeTranshuman
        {
            get => _blnPrototypeTranshuman;
            set
            {
                if (_blnPrototypeTranshuman != value)
                {
                    string strOldEssencePropertyName = EssencePropertyName;
                    _blnPrototypeTranshuman = value;
                    if ((Parent == null || AddToParentESS) && string.IsNullOrEmpty(PlugsIntoModularMount) &&
                        ParentVehicle == null)
                        _objCharacter.OnMultiplePropertyChanged(strOldEssencePropertyName, EssencePropertyName);
                }

                foreach (Cyberware objCyberware in Children)
                    objCyberware.PrototypeTranshuman = value;
            }
        }

        public string EssencePropertyName
        {
            get
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

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage &&
                !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            string strDoc = "cyberware.xml";
            string strPath = "/chummer/cyberwares/cyberware";
            if (SourceType == Improvement.ImprovementSource.Bioware)
            {
                strDoc = "bioware.xml";
                strPath = "/chummer/biowares/bioware";
            }

            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData(strDoc, strLanguage)
                : await _objCharacter.LoadDataAsync(strDoc, strLanguage);
            _objCachedMyXmlNode = objDoc.SelectSingleNode(strPath + (SourceID == Guid.Empty
                                                              ? "[name = " + Name.CleanXPath() + ']'
                                                              : "[id = " + SourceIDString.CleanXPath()
                                                                         + " or id = " + SourceIDString
                                                                             .ToUpperInvariant().CleanXPath()
                                                                         + ']'));
            if (_objCachedMyXmlNode == null && SourceID != Guid.Empty)
            {
                _objCachedMyXmlNode = objDoc.SelectSingleNode(strPath + "[name = " + Name.CleanXPath() + ']');
                _objCachedMyXmlNode?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage)
        {
            if (_objCachedMyXPathNode != null && strLanguage == _strCachedXPathNodeLanguage
                                              && !GlobalSettings.LiveCustomData)
                return _objCachedMyXPathNode;
            string strDoc = "cyberware.xml";
            string strPath = "/chummer/cyberwares/cyberware";
            if (SourceType == Improvement.ImprovementSource.Bioware)
            {
                strDoc = "bioware.xml";
                strPath = "/chummer/biowares/bioware";
            }

            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath(strDoc, strLanguage)
                : await _objCharacter.LoadDataXPathAsync(strDoc, strLanguage);
            _objCachedMyXPathNode = objDoc.SelectSingleNode(strPath + (SourceID == Guid.Empty
                                                                ? "[name = " + Name.CleanXPath() + ']'
                                                                : "[id = " + SourceIDString.CleanXPath()
                                                                           + " or id = " + SourceIDString
                                                                               .ToUpperInvariant().CleanXPath()
                                                                           + ']'));
            if (_objCachedMyXPathNode == null && SourceID != Guid.Empty)
            {
                _objCachedMyXPathNode = objDoc.SelectSingleNode(strPath + "[name = " + Name.CleanXPath() + ']');
                _objCachedMyXPathNode?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _strCachedXPathNodeLanguage = strLanguage;
            return _objCachedMyXPathNode;
        }

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// Ghetto workaround for flagging an object as geneware.
        /// </summary>
        public bool IsGeneware =>
            SourceType == Improvement.ImprovementSource.Bioware &&
            (Category.StartsWith("Genetech", StringComparison.Ordinal) || Category.StartsWith("Genetic Infusions", StringComparison.Ordinal) ||
             Category.StartsWith("Genemods", StringComparison.Ordinal));

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability of the Cyberware and its plugins.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple(bool blnCheckChildren = true)
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
                    string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
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

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                                 _objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev,
                                              () => objLoopAttribute.TotalValue.ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base",
                                              () => objLoopAttribute.TotalBase.ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                    }

                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString(), out bool blnIsSuccess);
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

        /// <summary>
        /// Calculated Capacity of the Cyberware.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strCapacity = Capacity;
                if (strCapacity.StartsWith("FixedValues(", StringComparison.Ordinal))
                {
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                    strCapacity = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                if (string.IsNullOrEmpty(strCapacity))
                    return (0.0m).ToString("#,0.##", GlobalSettings.CultureInfo);
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
                        object objProcess =
                            CommonFunctions.EvaluateInvariantXPath(strFirstHalf.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)),
                                out bool blnIsSuccess);
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
                        object objProcess =
                            CommonFunctions.EvaluateInvariantXPath(strSecondHalf.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)),
                                out bool blnIsSuccess);
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

                    object objProcess =
                        CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString(GlobalSettings.InvariantCultureInfo)),
                            out bool blnIsSuccess);
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
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware.
        /// </summary>
        public decimal CalculatedESS => _objCharacter.IsPrototypeTranshuman && PrototypeTranshuman ? 0 : CalculatedESSPrototypeInvariant;

        /// <summary>
        /// Calculated Essence cost of the Cyberware if Prototype Transhuman is ignored.
        /// </summary>
        public decimal CalculatedESSPrototypeInvariant => GetCalculatedESSPrototypeInvariant(Rating, Grade);

        public decimal GetCalculatedESSPrototypeInvariant(int intRating, Grade objGrade)
        {
            if (Parent != null && !AddToParentESS)
                return 0;
            if (SourceID == EssenceHoleGUID) // Essence hole
            {
                return intRating / 100m;
            }

            if (SourceID == EssenceAntiHoleGUID) // Essence anti-hole
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

                string[] strValues = strESS.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                strESS = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)];
                strESS += strSuffix;
            }

            if (strESS.Contains("Rating") || strESS.IndexOfAny(s_MathOperators) >= 0)
            {
                // If the cost is determined by the Rating or there's a math operation in play, evaluate the expression.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strESS.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo)),
                    out bool blnIsSuccess);
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
            decimal decESSMultiplier = objGrade.Essence + ExtraESSAdditiveMultiplier;
            decimal decTotalESSMultiplier = ExtraESSMultiplicativeMultiplier;

            if (Suite)
                decESSMultiplier -= 0.1m;

            if (ESSDiscount != 0)
            {
                decimal decDiscount = ESSDiscount * 0.01m;
                decTotalESSMultiplier *= 1.0m - decDiscount;
            }

            // Retrieve the Bioware, Geneware or Cyberware ESS Cost Multiplier.
            if (ForceGrade != "None" || IsGeneware)
            {
                switch (SourceType)
                {
                    // Apply the character's Cyberware Essence cost multiplier if applicable.
                    case Improvement.ImprovementSource.Cyberware:
                        UpdateMultipliers(Improvement.ImprovementType.CyberwareEssCost,
                            Improvement.ImprovementType.CyberwareTotalEssMultiplier);
                        break;
                    // Apply the character's Bioware Essence cost multiplier if applicable.
                    case Improvement.ImprovementSource.Bioware when !IsGeneware:
                        UpdateMultipliers(Improvement.ImprovementType.BiowareEssCost,
                            Improvement.ImprovementType.BiowareTotalEssMultiplier);
                        // Apply the character's Basic Bioware Essence cost multiplier if applicable.
                        if (Category == "Basic")
                        {
                            List<Improvement> lstUsedImprovements =
                                ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                                    Improvement.ImprovementType.BasicBiowareEssCost);
                            if (lstUsedImprovements.Count != 0)
                            {
                                foreach (Improvement objImprovement in lstUsedImprovements)
                                    decESSMultiplier -= 1m - objImprovement.Value / 100m;
                            }
                        }

                        break;
                    // Apply the character's Geneware Essence cost multiplier if applicable. Since Geneware does not use Grades, we only check the genetechessmultiplier improvement.
                    case Improvement.ImprovementSource.Bioware when IsGeneware:
                        UpdateMultipliers(Improvement.ImprovementType.GenetechEssMultiplier,
                            Improvement.ImprovementType.None);
                        break;
                }
                void UpdateMultipliers(Improvement.ImprovementType eBaseMultiplier, Improvement.ImprovementType eTotalMultiplier)
                {
                    if (eBaseMultiplier != Improvement.ImprovementType.None)
                    {
                        List<Improvement> lstUsedImprovements =
                            ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, eBaseMultiplier);
                        if (lstUsedImprovements.Count != 0)
                        {
                            foreach (Improvement objImprovement in lstUsedImprovements)
                                decESSMultiplier -= 1m - objImprovement.Value / 100m;
                        }
                    }
                    if (eTotalMultiplier != Improvement.ImprovementType.None)
                    {
                        List<Improvement> lstUsedImprovements =
                            ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, eTotalMultiplier);
                        if (lstUsedImprovements.Count != 0)
                        {
                            foreach (Improvement objImprovement in lstUsedImprovements)
                                decTotalESSMultiplier *= objImprovement.Value / 100m;
                        }
                    }
                }
            }

            decReturn *= Math.Max(0, decESSMultiplier * decTotalESSMultiplier);

            if (_objCharacter?.Settings.DontRoundEssenceInternally == false)
                decReturn = decimal.Round(decReturn, _objCharacter.Settings.EssenceDecimals, MidpointRounding.AwayFromZero);

            if (_objCharacter?.IsPrototypeTranshuman == true)
                decReturn += Children
                             .Where(objChild => objChild.AddToParentESS && !objChild.PrototypeTranshuman).Sum(
                                 objChild => objChild.GetCalculatedESSPrototypeInvariant(objChild.Rating, objGrade));
            else
                decReturn += Children
                             .Where(objChild => objChild.AddToParentESS).Sum(
                                 objChild => objChild.GetCalculatedESSPrototypeInvariant(objChild.Rating, objGrade));
            return decReturn;
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
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
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
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
                                              () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0).ToString(
                                                  GlobalSettings
                                                      .InvariantCultureInfo));
                        sbdValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + '}',
                                              () => Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0");
                        if (Children.Count + GearChildren.Count > 0 &&
                            strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                        {
                            int intTotalChildrenValue = 0;
                            foreach (Cyberware objLoopCyberware in Children)
                            {
                                if (objLoopCyberware.IsModularCurrentlyEquipped)
                                {
                                    intTotalChildrenValue
                                        += objLoopCyberware.GetBaseMatrixAttribute(strMatrixAttribute);
                                }
                            }

                            foreach (Gear objLoopGear in GearChildren)
                            {
                                if (objLoopGear.Equipped)
                                {
                                    intTotalChildrenValue += objLoopGear.GetBaseMatrixAttribute(strMatrixAttribute);
                                }
                            }

                            sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                             intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                        }
                    }

                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);

                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString(), out bool blnIsSuccess);
                    return blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
                }
            }

            int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                ++intReturn;
            }

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Cyberware objLoopCyberware in Children)
            {
                if (objLoopCyberware.IsModularCurrentlyEquipped)
                {
                    intReturn += objLoopCyberware.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            foreach (Gear objLoopGear in GearChildren)
            {
                if (objLoopGear.Equipped)
                {
                    intReturn += objLoopGear.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        /// <summary>
        /// Total cost of the just the Cyberware itself before we factor in any multipliers.
        /// </summary>
        public decimal CalculatedOwnCostPreMultipliers(int intRating, Grade objGrade)
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

                string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                strCostExpression = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)].Trim('[', ']');
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
                decTotalChildrenCost += Children.Sum(loopWare => loopWare.CalculatedTotalCost(loopWare.Rating, objGrade));
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
                sbdCost.Replace("Children Cost", decTotalChildrenCost.ToString(GlobalSettings.InvariantCultureInfo));
                sbdCost.CheapReplace(strCostExpression, "MinRating", () => MinRating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdCost.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                    _objCharacter.AttributeSection.SpecialAttributeList))
                {
                    sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev,
                                         () => objLoopAttribute.TotalValue.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                         () => objLoopAttribute.TotalBase.ToString(GlobalSettings.InvariantCultureInfo));
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
            }

            return decReturn;
        }

        /// <summary>
        /// Total cost of the Cyberware and its plugins.
        /// </summary>
        public decimal CalculatedTotalCost(int intRating, Grade objGrade)
        {
            decimal decReturn = CalculatedTotalCostWithoutModifiers(intRating, objGrade);

            if (_blnSuite)
                decReturn *= 0.9m;

            return decReturn;
        }

        /// <summary>
        /// Identical to TotalCost, but without the Improvement and Suite multipliers which would otherwise be doubled.
        /// </summary>
        private decimal CalculatedTotalCostWithoutModifiers(int intRating, Grade objGrade)
        {
            decimal decCost = CalculatedOwnCostPreMultipliers(intRating, objGrade);
            decimal decReturn = decCost;

            // Factor in the Cost multiplier of the selected CyberwareGrade.
            decReturn *= objGrade.Cost;

            if (DiscountCost)
                decReturn *= 0.9m;

            // Genetech Cost multiplier.
            if (IsGeneware && ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.GenetechCostMultiplier, out List<Improvement> lstUsedImprovements) != 0)
            {
                decimal decMultiplier = lstUsedImprovements.Aggregate(1.0m, (current, objImprovement) => current - (1.0m - (objImprovement.Value / 100.0m)));

                decReturn *= decMultiplier;
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
                    decReturn += objChild.CalculatedTotalCostWithoutModifiers(objChild.Rating, objGrade) * ChildCostMultiplier;
            }

            // Add in the cost of all Gear plugins.
            foreach (Gear objGear in GearChildren)
            {
                decReturn += objGear.TotalCost;
            }

            return decReturn;
        }

        public decimal StolenTotalCost => CalculatedStolenTotalCost(Rating, Grade);

        /// <summary>
        /// Identical to TotalCost, including the modifiers from Suite improvements.
        /// </summary>
        public decimal CalculatedStolenTotalCost(int intRating, Grade objGrade)
        {
            decimal decCost = CalculatedOwnCostPreMultipliers(intRating, objGrade);
            decimal decReturn = decCost;

            // Factor in the Cost multiplier of the selected CyberwareGrade.
            decReturn *= objGrade.Cost;

            if (DiscountCost)
                decReturn *= 0.9m;

            // Add in the cost of all child components.
            foreach (Cyberware objChild in Children)
            {
                if (!objChild.Stolen || objChild.Capacity == "[*]")
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
                    decReturn += objChild.CalculatedTotalCostWithoutModifiers(objChild.Rating, objGrade) * ChildCostMultiplier;
            }

            // Add in the cost of all Gear plugins.
            decReturn += GearChildren.Where(g => g.Stolen).Sum(objGear => objGear.StolenTotalCost);

            if (_blnSuite)
                decReturn *= 0.9m;

            return decReturn;
        }

        /// <summary>
        /// Cost of just the Cyberware itself.
        /// </summary>
        public decimal CalculatedOwnCost(int intRating, Grade objGrade)
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

        public decimal TotalCost => CalculatedTotalCost(Rating, Grade);

        public decimal OwnCost => CalculatedOwnCost(Rating, Grade);

        /// <summary>
        /// Total weight of the just the Cyberware itself before we factor in any multipliers.
        /// </summary>
        public decimal CalculatedOwnWeight(int intRating, Grade objGrade)
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

                string[] strValues = strWeightExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                strWeightExpression = strValues[Math.Max(Math.Min(intRating, strValues.Length) - 1, 0)].Trim('[', ']');
                strWeightExpression += strSuffix;
            }

            string strParentWeight = "0";
            decimal decTotalParentGearWeight = 0;
            if (_objParent != null)
            {
                if (strWeightExpression.Contains("Parent Weight"))
                    strParentWeight = _objParent.Weight;
                if (strWeightExpression.Contains("Parent Gear Weight"))
                    decTotalParentGearWeight += _objParent.GearChildren.Sum(loopGear => loopGear.OwnWeight * loopGear.Quantity);
            }

            decimal decTotalGearWeight = 0;
            if (GearChildren.Count > 0 && strWeightExpression.Contains("Gear Weight"))
            {
                decTotalGearWeight += GearChildren.Sum(loopGear => loopGear.OwnWeight * loopGear.Quantity);
            }

            decimal decTotalChildrenWeight = 0;
            if (Children.Count > 0 && strWeightExpression.Contains("Children Weight"))
            {
                decTotalChildrenWeight += Children.Sum(loopWare => loopWare.CalculatedTotalWeight(loopWare.Rating, objGrade));
            }

            decimal decReturn = 0;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdWeight))
            {
                sbdWeight.Append(strWeightExpression.TrimStart('+'));
                sbdWeight.Replace("Parent Weight", strParentWeight);
                sbdWeight.Replace("Parent Gear Weight",
                                  decTotalParentGearWeight.ToString(GlobalSettings.InvariantCultureInfo));
                sbdWeight.Replace("Gear Weight", decTotalGearWeight.ToString(GlobalSettings.InvariantCultureInfo));
                sbdWeight.Replace("Children Weight", decTotalChildrenWeight.ToString(GlobalSettings.InvariantCultureInfo));
                sbdWeight.CheapReplace(strWeightExpression, "MinRating", () => MinRating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdWeight.Replace("Rating", intRating.ToString(GlobalSettings.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(
                    _objCharacter.AttributeSection.SpecialAttributeList))
                {
                    sbdWeight.CheapReplace(strWeightExpression, objLoopAttribute.Abbrev,
                                           () => objLoopAttribute.TotalValue.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdWeight.CheapReplace(strWeightExpression, objLoopAttribute.Abbrev + "Base",
                                           () => objLoopAttribute.TotalBase.ToString(GlobalSettings.InvariantCultureInfo));
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(sbdWeight.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
            }

            return decReturn;
        }

        /// <summary>
        /// Total weight of the Cyberware and its plugins.
        /// </summary>
        public decimal CalculatedTotalWeight(int intRating, Grade objGrade)
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
            decReturn += GearChildren.Where(x => x.Equipped).Sum(objGear => objGear.TotalWeight);

            return decReturn;
        }

        public decimal TotalWeight => CalculatedTotalWeight(Rating, Grade);

        public decimal OwnWeight => CalculatedOwnWeight(Rating, Grade);

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                decimal decCapacity = 0;
                if (Capacity.Contains("/["))
                {
                    // Get the Cyberware base Capacity.
                    string strBaseCapacity = CalculatedCapacity;
                    strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                    decCapacity = Convert.ToDecimal(strBaseCapacity, GlobalSettings.CultureInfo);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Children)
                    {
                        // Children that are built into the parent
                        if (objChildCyberware.PlugsIntoModularMount == HasModularMount &&
                            !string.IsNullOrWhiteSpace(HasModularMount) ||
                            objChildCyberware.ParentID == InternalId) continue;
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2,
                                strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                    }

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Gear objChildGear in GearChildren)
                    {
                        if (objChildGear.IncludedInParent)
                        {
                            continue;
                        }

                        string strCapacity = objChildGear.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                    }
                }
                else if (!Capacity.Contains('['))
                {
                    // Get the Cyberware base Capacity.
                    decCapacity = Convert.ToDecimal(CalculatedCapacity, GlobalSettings.CultureInfo);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Children)
                    {
                        if (objChildCyberware.PlugsIntoModularMount == HasModularMount &&
                            !string.IsNullOrWhiteSpace(HasModularMount) ||
                            objChildCyberware.ParentID == InternalId) continue;
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                    }

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Gear objChildGear in GearChildren)
                    {
                        if (objChildGear.IncludedInParent)
                        {
                            continue;
                        }

                        string strCapacity = objChildGear.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalSettings.CultureInfo);
                    }
                }

                return decCapacity;
            }
        }

        public string DisplayCapacity
        {
            get
            {
                if (Capacity.Contains('[') && !Capacity.Contains("/["))
                    return CalculatedCapacity;
                return string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_CapacityRemaining"),
                    CalculatedCapacity, CapacityRemaining.ToString("#,0.##", GlobalSettings.CultureInfo));
            }
        }

        /// <summary>
        /// Base Cyberlimb attribute value (before modifiers and customization).
        /// </summary>
        public int GetAttributeBaseValue(string strAbbrev)
        {
            if (Category != "Cyberlimb")
                return 0;
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
            switch (strAbbrev)
            {
                case "STR":
                    // Base Strength for any limb is 3.
                    return ParentVehicle != null ? Math.Max(ParentVehicle.TotalBody, 0) : 3;
                case "AGI":
                    // Base Agility for any limb is 3.
                    return ParentVehicle != null ? Math.Max(ParentVehicle.Pilot, 0) : 3;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Unaugmented Cyberlimb attribute value (before modifiers).
        /// </summary>
        public int GetAttributeValue(string strAbbrev)
        {
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
            int intValue = GetAttributeBaseValue(strAbbrev);
            if (Children.Count > 0 && s_AttributeCustomizationCyberwares.TryGetValue(strAbbrev, out IReadOnlyCollection<string> setNamesToCheck))
            {
                List<Cyberware> lstCustomizationWare = new List<Cyberware>(Children.Count);
                foreach (Cyberware objChild in Children)
                {
                    if (setNamesToCheck.Contains(objChild.Name))
                        lstCustomizationWare.Add(objChild);
                }
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

        /// <summary>
        /// Total value for an attribute on a cyberlimb.
        /// </summary>
        public int GetAttributeTotalValue(string strAbbrev)
        {
            if (!CyberlimbAttributeAbbrevs.Contains(strAbbrev))
                return 0;
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

            if (Category != "Cyberlimb")
                return 0;

            int intBonus = 0;

            if (Children.Count > 0 && s_AttributeEnhancementCyberwares.TryGetValue(strAbbrev, out IReadOnlyCollection<string> setNamesToCheck))
            {
                List<Cyberware> lstEnhancementWare = new List<Cyberware>(Children.Count);
                foreach (Cyberware objChild in Children)
                {
                    if (setNamesToCheck.Contains(objChild.Name))
                        lstEnhancementWare.Add(objChild);
                }
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

        public bool IsProgram => false;

        /// <summary>
        /// Device rating string for Cyberware. If it's empty, then GetBaseMatrixAttribute for Device Rating will fetch the grade's DR.
        /// </summary>
        public string DeviceRating
        {
            get => _strDeviceRating;
            set => _strDeviceRating = value;
        }

        /// <summary>
        /// Attack string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Attack
        {
            get => _strAttack;
            set => _strAttack = value;
        }

        /// <summary>
        /// Sleaze string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Sleaze
        {
            get => _strSleaze;
            set => _strSleaze = value;
        }

        /// <summary>
        /// Data Processing string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string DataProcessing
        {
            get => _strDataProcessing;
            set => _strDataProcessing = value;
        }

        /// <summary>
        /// Firewall string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Firewall
        {
            get => _strFirewall;
            set => _strFirewall = value;
        }

        /// <summary>
        /// Modify Parent's Attack by this.
        /// </summary>
        public string ModAttack
        {
            get => _strModAttack;
            set => _strModAttack = value;
        }

        /// <summary>
        /// Modify Parent's Sleaze by this.
        /// </summary>
        public string ModSleaze
        {
            get => _strModSleaze;
            set => _strModSleaze = value;
        }

        /// <summary>
        /// Modify Parent's Data Processing by this.
        /// </summary>
        public string ModDataProcessing
        {
            get => _strModDataProcessing;
            set => _strModDataProcessing = value;
        }

        /// <summary>
        /// Modify Parent's Firewall by this.
        /// </summary>
        public string ModFirewall
        {
            get => _strModFirewall;
            set => _strModFirewall = value;
        }

        /// <summary>
        /// Cyberdeck's Attribute Array string.
        /// </summary>
        public string AttributeArray
        {
            get => _strAttributeArray;
            set => _strAttributeArray = value;
        }

        /// <summary>
        /// Modify Parent's Attribute Array by this.
        /// </summary>
        public string ModAttributeArray
        {
            get => _strModAttributeArray;
            set => _strModAttributeArray = value;
        }

        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        public string Overclocked
        {
            get => _strOverclocked;
            set => _strOverclocked = value;
        }

        /// <summary>
        /// String to determine if gear can form persona or grants persona forming to its parent.
        /// </summary>
        public string CanFormPersona
        {
            get => _strCanFormPersona;
            set => _strCanFormPersona = value;
        }

        public bool IsCommlink => CanFormPersona.Contains("Self") || (ChildrenWithMatrixAttributes.Any(x => x.CanFormPersona.Contains("Parent")) &&
                                                                      this.GetTotalMatrixAttribute("Device Rating") > 0);

        /// <summary>
        /// Bonus Matrix Boxes.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get => _intMatrixCMBonus;
            set => _intMatrixCMBonus = value;
        }

        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intBonusBoxes = 0;
                foreach (Cyberware objCyberware in Children)
                {
                    intBonusBoxes += objCyberware.TotalBonusMatrixBoxes;
                }
                foreach (Gear objGear in GearChildren)
                {
                    if (objGear.Equipped)
                    {
                        intBonusBoxes += objGear.TotalBonusMatrixBoxes;
                    }
                }

                return intBonusBoxes;
            }
        }

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get => _strProgramLimit;
            set => _strProgramLimit = value;
        }

        /// <summary>
        /// Returns true if this is a Cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get => _blnCanSwapAttributes;
            set => _blnCanSwapAttributes = value;
        }

        public IEnumerable<IHasMatrixAttributes> ChildrenWithMatrixAttributes =>
            GearChildren.Concat<IHasMatrixAttributes>(Children);

        #endregion Complex Properties

        #region Methods

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteCyberware(bool blnDoRemoval = true, bool blnIncreaseEssenceHole = true)
        {
            // Unequip all modular children first so that we don't delete them
            Cyberware objModularChild = Children.DeepFirstOrDefault(x => x.Children, x => !string.IsNullOrEmpty(x.PlugsIntoModularMount));
            while (objModularChild != null)
            {
                Children.Remove(objModularChild);
                _objCharacter.Cyberware.Add(objModularChild);
                objModularChild.ChangeModularEquip(false);
                objModularChild = Children.DeepFirstOrDefault(x => x.Children, x => !string.IsNullOrEmpty(x.PlugsIntoModularMount));
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
                    if (blnIncreaseEssenceHole && _objCharacter.Created && SourceID != EssenceAntiHoleGUID && SourceID != EssenceHoleGUID)
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

            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Cyberware objChild in Children)
                decReturn += objChild.DeleteCyberware(false);

            // Remove the Gear Weapon created by the Gear if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                foreach (Weapon objDeleteWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                {
                    decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                }
                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    foreach (Weapon objDeleteWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                    {
                        decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        foreach (Weapon objDeleteWeapon in objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                        {
                            decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                        }
                    }

                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        foreach (Weapon objDeleteWeapon in objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
                        {
                            decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                        }
                    }
                }
            }
            if (!WeaponAccessoryID.IsEmptyGuid())
            {
                // Locate the Weapon Accessory that was added.
                WeaponAccessory objWeaponAccessory = _objCharacter.Vehicles.FindVehicleWeaponAccessory(WeaponAccessoryID) ??
                                                     _objCharacter.Weapons.FindWeaponAccessory(WeaponAccessoryID);
                objWeaponAccessory?.DeleteWeaponAccessory();
            }

            // Remove any Vehicle that the Cyberware created.
            if (!VehicleID.IsEmptyGuid())
            {
                foreach (Vehicle objLoopVehicle in _objCharacter.Vehicles.Where(x => x.ParentID == InternalId).ToList())
                {
                    decReturn += objLoopVehicle.TotalCost;
                    decReturn += objLoopVehicle.DeleteVehicle();
                }
            }

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId);
            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "Pair");
            if (PairBonus != null)
            {
                // This cyberware should not be included in the count to make things easier.
                List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children,
                        x => x != this && IncludePair.Contains(x.Name) && x.Extra == Extra &&
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
                        ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId + "Pair", objLoopCyberware.PairBonus, objLoopCyberware.Rating,
                            objLoopCyberware.CurrentDisplayNameShort);
                    }

                    --intCount;
                }
            }

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId + "WirelessPair");
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
                    if (objLoopCyberware.WirelessPairBonus?.SelectSingleNode("@mode")?.Value == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                                                               _objCharacter.Improvements
                                                                            .Where(x => x.ImproveSource == objLoopCyberware.SourceType && x.SourceName == objLoopCyberware.InternalId)
                                                                            .ToList());
                    }
                    // Go down the list and create pair bonuses for every second item
                    if (intCount > 0 && intCount % 2 == 0)
                    {
                        ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType,
                            objLoopCyberware.InternalId + "WirelessPair", objLoopCyberware.WirelessPairBonus, objLoopCyberware.Rating,
                            objLoopCyberware.CurrentDisplayNameShort);
                    }

                    --intCount;
                }
            }

            foreach (Gear objLoopGear in GearChildren)
                decReturn += objLoopGear.DeleteGear(false);

            // Fix for legacy characters with old addqualities improvements.
            XPathNodeIterator xmlOldAddQualitiesList = this.GetNodeXPath()?.Select("addqualities/addquality");
            if (xmlOldAddQualitiesList?.Count > 0)
            {
                foreach (XPathNavigator objNode in xmlOldAddQualitiesList)
                {
                    string strText = objNode.Value;
                    Quality objQuality = _objCharacter.Qualities.FirstOrDefault(x => x.Name == strText);
                    if (objQuality == null)
                        continue;
                    // We need to add in the return cost of deleting the quality, so call this manually
                    decReturn += objQuality.DeleteQuality();
                    decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.CritterPower, objQuality.InternalId);
                }
            }

            DisposeSelf();

            return decReturn;
        }

        /// <summary>
        /// Checks a nominated piece of gear for Availability requirements.
        /// </summary>
        /// <param name="dicRestrictedGearLimits">Dictionary of Restricted Gear availabilities still available with the amount of items that can still use that availability.</param>
        /// <param name="sbdAvailItems">StringBuilder used to list names of gear that are currently over the availability limit.</param>
        /// <param name="sbdRestrictedItems">StringBuilder used to list names of gear that are being used for Restricted Gear.</param>
        /// <param name="intRestrictedCount">Number of items that are above availability limits.</param>
        public void CheckRestrictedGear(IDictionary<int, int> dicRestrictedGearLimits, StringBuilder sbdAvailItems, StringBuilder sbdRestrictedItems, ref int intRestrictedCount)
        {
            if (string.IsNullOrEmpty(ParentID))
            {
                AvailabilityValue objTotalAvail = TotalAvailTuple();
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = objTotalAvail.Value;
                    if (intAvailInt > _objCharacter.Settings.MaximumAvailability)
                    {
                        int intLowestValidRestrictedGearAvail = -1;
                        foreach (int intValidAvail in dicRestrictedGearLimits.Keys)
                        {
                            if (intValidAvail >= intAvailInt && (intLowestValidRestrictedGearAvail < 0
                                                                 || intValidAvail < intLowestValidRestrictedGearAvail))
                                intLowestValidRestrictedGearAvail = intValidAvail;
                        }

                        string strNameToUse = CurrentDisplayName;
                        if (Parent != null)
                            strNameToUse += LanguageManager.GetString("String_Space") + '(' + Parent.CurrentDisplayName + ')';

                        if (Grade.Avail != 0)
                            strNameToUse += LanguageManager.GetString("String_Space") + '(' + Grade.CurrentDisplayName + ')';

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

            foreach (Cyberware objChild in Children)
            {
                objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, ref intRestrictedCount);
            }

            foreach (Gear objChild in GearChildren)
            {
                objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, ref intRestrictedCount);
            }
        }

        public void CheckBannedGrades(StringBuilder sbdBannedItems)
        {
            if (string.IsNullOrEmpty(ParentID) && _objCharacter.Settings.BannedWareGrades.Any(s => Grade.Name.Contains(s)))
            {
                sbdBannedItems.AppendLine().Append("\t\t").Append(CurrentDisplayName);
            }
            foreach (Cyberware objChild in Children)
            {
                objChild.CheckBannedGrades(sbdBannedItems);
            }
        }

        #region UI Methods

        /// <summary>
        /// Build up the Tree for the current piece of Cyberware and all of its children.
        /// </summary>
        /// <param name="cmsCyberware">ContextMenuStrip that the new Cyberware TreeNodes should use.</param>
        /// <param name="cmsGear">ContextMenuStrip that the new Gear TreeNodes should use.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsCyberware, ContextMenuStrip cmsGear)
        {
            if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) &&
                !_objCharacter.Settings.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsCyberware,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            foreach (Cyberware objChild in Children)
            {
                TreeNode objLoopNode = objChild.CreateTreeNode(cmsCyberware, cmsGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }

            foreach (Gear objGear in GearChildren)
            {
                TreeNode objLoopNode = objGear.CreateTreeNode(cmsGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }

            if (lstChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }

        public Color PreferredColor
        {
            get
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

        public void SetupChildrenCyberwareCollectionChanged(bool blnAdd, TreeView treCyberware,
            ContextMenuStrip cmsCyberware = null, ContextMenuStrip cmsCyberwareGear = null)
        {
            if (blnAdd)
            {
                Children.AddTaggedCollectionChanged(treCyberware,
                    (x, y) => this.RefreshChildrenCyberware(treCyberware, cmsCyberware, cmsCyberwareGear, null, y));
                GearChildren.AddTaggedCollectionChanged(treCyberware,
                    (x, y) => this.RefreshChildrenGears(treCyberware, cmsCyberwareGear, () => Children.Count, y));

                foreach (Cyberware objChild in Children)
                {
                    objChild.SetupChildrenCyberwareCollectionChanged(true, treCyberware, cmsCyberware,
                                                                     cmsCyberwareGear);
                }

                foreach (Gear objGear in GearChildren)
                    objGear.SetupChildrenGearsCollectionChanged(true, treCyberware, cmsCyberwareGear);
            }
            else
            {
                Children.RemoveTaggedCollectionChanged(treCyberware);
                GearChildren.RemoveTaggedCollectionChanged(treCyberware);
                foreach (Cyberware objChild in Children)
                    objChild.SetupChildrenCyberwareCollectionChanged(false, treCyberware);
                foreach (Gear objGear in GearChildren)
                    objGear.SetupChildrenGearsCollectionChanged(false, treCyberware);
            }
        }

        #endregion UI Methods

        #region Hero Lab Importing

        public bool ImportHeroLabCyberware(XPathNavigator xmlCyberwareImportNode, XmlNode xmlParentCyberwareNode,
            IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles, Grade objSelectedGrade = null)
        {
            if (xmlCyberwareImportNode == null)
                return false;
            string strOriginalName = xmlCyberwareImportNode.SelectSingleNode("@name")?.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(strOriginalName))
            {
                string strGradeName = objSelectedGrade?.Name ?? "Standard";
                bool blnCyberware = true;
                Lazy<List<Grade>> objCyberwareGradeList = new Lazy<List<Grade>>(() =>
                    _objCharacter.GetGradeList(Improvement.ImprovementSource.Cyberware).ToList());
                Lazy<List<Grade>> objBiowareGradeList = new Lazy<List<Grade>>(() =>
                    _objCharacter.GetGradeList(Improvement.ImprovementSource.Bioware).ToList());
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
                            if (strOriginalName.EndsWith(" (" + objCyberwareGrade.Name + ')', StringComparison.Ordinal))
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
                XmlNodeList xmlCyberwareNodeList = xmlCyberwareDocument.SelectNodes("/chummer/cyberwares/cyberware[contains(name, " + strOriginalName.CleanXPath() + ")]");
                if (xmlCyberwareNodeList != null)
                {
                    foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                    {
                        XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                        if (xmlTestNode != null && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                        if (xmlTestNode != null && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlCyberwareDataNode = xmlLoopNode;
                        break;
                    }
                }

                if (xmlCyberwareDataNode == null)
                {
                    blnCyberware = false;
                    xmlCyberwareNodeList = xmlBiowareDocument.SelectNodes("/chummer/biowares/bioware[contains(name, " + strOriginalName.CleanXPath() + ")]");
                    if (xmlCyberwareNodeList != null)
                    {
                        foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                        {
                            XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                            if (xmlTestNode != null && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                            {
                                // Assumes topmost parent is an AND node
                                continue;
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                            if (xmlTestNode != null && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
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
                    string[] astrOriginalNameSplit = strOriginalName.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (astrOriginalNameSplit.Length > 1)
                    {
                        string strName = astrOriginalNameSplit[0].Trim();
                        blnCyberware = true;
                        xmlCyberwareNodeList = xmlCyberwareDocument.SelectNodes("/chummer/cyberwares/cyberware[contains(name, " + strName.CleanXPath() + ")]");
                        if (xmlCyberwareNodeList != null)
                        {
                            foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                            {
                                XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                if (xmlTestNode != null && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    // Assumes topmost parent is an AND node
                                    continue;
                                }

                                xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                if (xmlTestNode != null && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
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
                                xmlCyberwareNodeList = xmlBiowareDocument.SelectNodes("/chummer/biowares/bioware[contains(name, " + strName.CleanXPath() + ")]");
                                if (xmlCyberwareNodeList != null)
                                    foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                    {
                                        XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                        if (xmlTestNode != null && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                        if (xmlTestNode != null && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            // Assumes topmost parent is an AND node
                                            continue;
                                        }

                                        xmlCyberwareDataNode = xmlLoopNode;
                                        break;
                                    }

                                if (xmlCyberwareDataNode != null)
                                    strForceValue = astrOriginalNameSplit[1].Trim();
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
                            xmlCyberwareNodeList = xmlCyberwareDocument.SelectNodes("/chummer/cyberwares/cyberware[contains(name, " + strName.CleanXPath() + ")]");
                            if (xmlCyberwareNodeList != null)
                            {
                                foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                {
                                    XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                    if (xmlTestNode != null && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        // Assumes topmost parent is an AND node
                                        continue;
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                    if (xmlTestNode != null && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
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
                                    xmlCyberwareNodeList = xmlBiowareDocument.SelectNodes("/chummer/biowares/bioware[contains(name, " + strName.CleanXPath() + ")]");
                                    if (xmlCyberwareNodeList != null)
                                        foreach (XmlNode xmlLoopNode in xmlCyberwareNodeList)
                                        {
                                            XmlNode xmlTestNode =
                                                xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                            if (xmlTestNode != null && xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
                                            {
                                                // Assumes topmost parent is an AND node
                                                continue;
                                            }

                                            xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                            if (xmlTestNode != null && !xmlParentCyberwareNode.ProcessFilterOperationNode(xmlTestNode, false))
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

                if (xmlCyberwareDataNode != null)
                {
                    if (objSelectedGrade == null)
                    {
                        objSelectedGrade =
                            (blnCyberware ? objCyberwareGradeList : objBiowareGradeList).Value.Find(x =>
                                x.Name == strGradeName);
                    }

                    Create(xmlCyberwareDataNode, objSelectedGrade,
                        blnCyberware ? Improvement.ImprovementSource.Cyberware : Improvement.ImprovementSource.Bioware,
                        xmlCyberwareImportNode.SelectSingleNode("@rating")?.ValueAsInt ?? 0, lstWeapons,
                        lstVehicles, true, true, strForceValue);
                    Notes = xmlCyberwareImportNode.SelectSingleNode("description")?.Value;

                    ProcessHeroLabCyberwarePlugins(xmlCyberwareImportNode, objSelectedGrade, lstWeapons, lstVehicles);

                    return true;
                }
            }

            return false;
        }

        public void ProcessHeroLabCyberwarePlugins(XPathNavigator xmlGearImportNode, Grade objParentGrade,
            IList<Weapon> lstWeapons, IList<Vehicle> lstVehicles)
        {
            if (xmlGearImportNode == null)
                return;
            foreach (string strPluginNodeName in Character.HeroLabPluginNodeNames)
            {
                foreach (XPathNavigator xmlPluginToAdd in xmlGearImportNode.Select(strPluginNodeName + "/item[@useradded != \"no\"]"))
                {
                    Cyberware objPlugin = new Cyberware(_objCharacter);
                    if (objPlugin.ImportHeroLabCyberware(xmlPluginToAdd, this.GetNode(), lstWeapons, lstVehicles,
                        objParentGrade))
                    {
                        objPlugin.Parent = this;
                        Children.Add(objPlugin);
                    }
                    else
                    {
                        objPlugin.Dispose();
                        Gear objPluginGear = new Gear(_objCharacter);
                        if (objPluginGear.ImportHeroLabGear(xmlPluginToAdd, this.GetNode(), lstWeapons))
                        {
                            objPluginGear.Parent = this;
                            GearChildren.Add(objPluginGear);
                        }
                    }
                }

                foreach (XPathNavigator xmlPluginToAdd in xmlGearImportNode.Select(strPluginNodeName + "/item[@useradded = \"no\"]"))
                {
                    string strName = xmlPluginToAdd.SelectSingleNode("@name")?.Value ?? string.Empty;
                    if (!string.IsNullOrEmpty(strName))
                    {
                        Cyberware objPlugin = Children.FirstOrDefault(x =>
                            x.ParentID == InternalId && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                        if (objPlugin != null)
                        {
                            objPlugin.Notes = xmlPluginToAdd.SelectSingleNode("description")?.Value;
                            objPlugin.ProcessHeroLabCyberwarePlugins(xmlPluginToAdd, objParentGrade, lstWeapons,
                                lstVehicles);
                        }
                        else
                        {
                            Gear objPluginGear = GearChildren.FirstOrDefault(x =>
                                x.IncludedInParent && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                            if (objPluginGear != null)
                            {
                                objPluginGear.Quantity = xmlPluginToAdd.SelectSingleNode("@quantity")?.ValueAsInt ?? 1;
                                objPluginGear.Notes = xmlPluginToAdd.SelectSingleNode("description")?.Value;
                                objPluginGear.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons);
                            }
                        }
                    }
                }
            }

            this.RefreshMatrixAttributeArray();
        }

        #endregion Hero Lab Importing

        #endregion Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (Capacity == "[*]" && Parent != null && (!_objCharacter.IgnoreRules || _objCharacter.Created))
            {
                Program.ShowMessageBox(
                    LanguageManager.GetString("Message_CannotRemoveCyberware"),
                    LanguageManager.GetString("MessageTitle_CannotRemoveCyberware"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString(SourceType == Improvement.ImprovementSource.Bioware
                ? "Message_DeleteBioware"
                : "Message_DeleteCyberware")))
                return false;

            DeleteCyberware();
            return true;
        }

        public bool Sell(decimal percentage, bool blnConfirmDelete)
        {
            if (Capacity == "[*]" && Parent != null && (!_objCharacter.IgnoreRules || _objCharacter.Created))
            {
                Program.ShowMessageBox(
                    LanguageManager.GetString("Message_CannotRemoveCyberware"),
                    LanguageManager.GetString("MessageTitle_CannotRemoveCyberware"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString(SourceType == Improvement.ImprovementSource.Bioware
                                                                       ? "Message_DeleteBioware"
                                                                       : "Message_DeleteCyberware")))
                return false;

            if (!_objCharacter.Created)
            {
                DeleteCyberware();
                return true;
            }

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
            decimal decOriginal = Parent?.TotalCost ?? TotalCost;
            decimal decAmount = DeleteCyberware() * percentage;
            decAmount += (decOriginal - (objParent?.TotalCost ?? 0)) * percentage;
            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, strEntry + ' ' + CurrentDisplayNameShort, ExpenseType.Nuyen,
                DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen += decAmount;
            return true;
        }

        /// <summary>
        /// Purchases a selected piece of Cyberware with a given Grade and Rating.
        /// </summary>
        /// <param name="objNode"></param>
        /// <param name="objGrade"></param>
        /// <param name="objImprovementSource"></param>
        /// <param name="intRating"></param>
        /// <param name="objVehicle"></param>
        /// <param name="lstCyberwareCollection"></param>
        /// <param name="lstVehicleCollection"></param>
        /// <param name="lstWeaponCollection"></param>
        /// <param name="decMarkup"></param>
        /// <param name="blnFree"></param>
        /// <param name="blnBlackMarket"></param>
        /// <param name="blnForVehicle"></param>
        /// <param name="strExpenseString"></param>
        /// <param name="objParent">Cyberware parent that this Cyberware will attach to, if any.</param>
        /// <returns></returns>
        public bool Purchase(XmlNode objNode, Improvement.ImprovementSource objImprovementSource, Grade objGrade,
            int intRating, Vehicle objVehicle, ICollection<Cyberware> lstCyberwareCollection,
            ICollection<Vehicle> lstVehicleCollection, ICollection<Weapon> lstWeaponCollection,
            decimal decMarkup = 0, bool blnFree = false, bool blnBlackMarket = false, bool blnForVehicle = false,
            string strExpenseString = "String_ExpensePurchaseCyberware", Cyberware objParent = null)
        {
            // Create the Cyberware object.
            List<Weapon> lstWeapons = new List<Weapon>(1);
            List<Vehicle> lstVehicles = new List<Vehicle>(1);
            Create(objNode, objGrade, objImprovementSource, intRating, lstWeapons, lstVehicles, true, true,
                string.Empty, objParent, objVehicle);
            if (InternalId.IsEmptyGuid())
            {
                return false;
            }

            if (blnFree)
                Cost = "0";
            DiscountCost = blnBlackMarket;

            if (_objCharacter.Created)
            {
                decimal decCost = 0;
                // Check the item's Cost and make sure the character can afford it.
                if (!blnFree)
                {
                    decCost = TotalCost;

                    // Multiply the cost if applicable.
                    char chrAvail = TotalAvailTuple().Suffix;
                    switch (chrAvail)
                    {
                        case 'R' when _objCharacter.Settings.MultiplyRestrictedCost:
                            decCost *= _objCharacter.Settings.RestrictedCostMultiplier;
                            break;

                        case 'F' when _objCharacter.Settings.MultiplyForbiddenCost:
                            decCost *= _objCharacter.Settings.ForbiddenCostMultiplier;
                            break;
                    }

                    // Apply a markup if applicable.
                    if (decMarkup != 0)
                    {
                        decCost *= 1 + (decMarkup / 100.0m);
                    }

                    if (decCost > _objCharacter.Nuyen)
                    {
                        Program.ShowMessageBox(
                            LanguageManager.GetString("Message_NotEnoughNuyen"),
                            LanguageManager.GetString("MessageTitle_NotEnoughNuyen"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        return false;
                    }
                }

                // Create the Expense Log Entry.
                ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                string strEntry = LanguageManager.GetString(strExpenseString);
                string strName = CurrentDisplayNameShort;
                if (SourceID == EssenceHoleGUID || SourceID == EssenceAntiHoleGUID)
                {
                    strName += LanguageManager.GetString("String_Space") + '(' +
                               Rating.ToString(GlobalSettings.CultureInfo) + ')';
                }

                objExpense.Create(decCost * -1,
                    strEntry + LanguageManager.GetString("String_Space") +
                    strName, ExpenseType.Nuyen, DateTime.Now);
                _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                _objCharacter.Nuyen -= decCost;

                if (SourceID != EssenceHoleGUID && SourceID != EssenceAntiHoleGUID)
                {
                    ExpenseUndo objUndo = new ExpenseUndo();
                    objUndo.CreateNuyen(
                        blnForVehicle ? NuyenExpenseType.AddVehicleModCyberware : NuyenExpenseType.AddCyberware,
                        InternalId);
                    objExpense.Undo = objUndo;
                }
            }

            if (SourceID == EssenceAntiHoleGUID)
            {
                _objCharacter.DecreaseEssenceHole(Rating);
            }
            else if (SourceID == EssenceHoleGUID)
            {
                _objCharacter.IncreaseEssenceHole(Rating);
            }
            else
            {
                if (_objCharacter.Created && ReferenceEquals(lstCyberwareCollection, _objCharacter.Cyberware))
                {
                    _objCharacter.DecreaseEssenceHole(CalculatedESS, SourceID == EssenceAntiHoleGUID);
                }

                lstCyberwareCollection?.Add(this);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    objWeapon.ParentVehicle = objVehicle;
                    lstWeaponCollection?.Add(objWeapon);
                }

                foreach (Vehicle objLoopVehicle in lstVehicles)
                {
                    lstVehicleCollection?.Add(objLoopVehicle);
                }
            }

            return true;
        }

        public void Upgrade(Grade objGrade, int intRating, decimal refundPercentage, bool blnFree)
        {
            decimal decSaleCost = TotalCost * refundPercentage;
            decimal decOldEssence = CalculatedESS;

            decimal decNewCost = blnFree ? 0 : CalculatedTotalCost(intRating, objGrade) - decSaleCost;
            if (decNewCost > _objCharacter.Nuyen)
            {
                Program.ShowMessageBox(
                    LanguageManager.GetString("Message_NotEnoughNuyen"),
                    LanguageManager.GetString("MessageTitle_NotEnoughNuyen"),
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strSpace = LanguageManager.GetString("String_Space");
            string strExpense = LanguageManager.GetString("String_ExpenseUpgradedCyberware") + strSpace +
                                CurrentDisplayNameShort;
            bool blnDoGradeChange = Grade.Essence != objGrade.Essence;
            bool blnDoRatingChange = Rating != intRating;
            if (blnDoGradeChange || blnDoRatingChange)
            {
                strExpense += '(' + LanguageManager.GetString("String_Grade") + strSpace + Grade.CurrentDisplayName + strSpace + "->" + objGrade.CurrentDisplayName
                              + strSpace + LanguageManager.GetString(RatingLabel) + Rating.ToString(GlobalSettings.CultureInfo)
                              + strSpace + "->" + strSpace + intRating.ToString(GlobalSettings.CultureInfo) + ')';
            }

            // Create the Expense Log Entry.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(-decNewCost, strExpense, ExpenseType.Nuyen, DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen -= decNewCost;

            ExpenseUndo objUndo = new ExpenseUndo();
            objUndo.CreateNuyen(NuyenExpenseType.AddGear, InternalId);
            objExpense.Undo = objUndo;
            bool blnOldProcessPropertyChanges = ProcessPropertyChanges;
            ProcessPropertyChanges = false;
            try
            {
                Rating = intRating;
                Grade = objGrade;
            }
            finally
            {
                ProcessPropertyChanges = blnOldProcessPropertyChanges;
            }
            decimal decEssDelta = GetCalculatedESSPrototypeInvariant(intRating, objGrade) - decOldEssence;
            if (decEssDelta > 0)
            {
                //The new Essence cost is greater than the old one.
                _objCharacter.DecreaseEssenceHole(decEssDelta);
            }
            else if (decEssDelta < 0)
            {
                _objCharacter.IncreaseEssenceHole(-decEssDelta);
            }
            DoPropertyChanges(blnDoRatingChange, blnDoGradeChange);
        }

        /// <summary>
        /// Alias map for SourceDetail control text and tooltip assignation.
        /// </summary>
        /// <param name="sourceControl"></param>
        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }

        public bool AllowPasteXml
        {
            get
            {
                switch (GlobalSettings.ClipboardContentType)
                {
                    case ClipboardContentType.Gear:
                        XmlNode objXmlCategoryNode =
                            GlobalSettings.Clipboard.SelectSingleNode("/character/gear/category");
                        XmlNode objXmlNameNode =
                            GlobalSettings.Clipboard.SelectSingleNode("/character/gear/name");
                        if (AllowGear?.ChildNodes.Cast<XmlNode>().Any(objAllowed => (objAllowed.Name == "gearcategory" &&
                                                                                    objAllowed.InnerText ==
                                                                                    objXmlCategoryNode?.InnerText) ||
                                                                                   objAllowed.Name == "gearname" &&
                                                                                   objAllowed.InnerText ==
                                                                                   objXmlNameNode?.InnerText) == true)
                        {
                            return true;
                        }

                        break;

                    case ClipboardContentType.Cyberware:
                        Utils.BreakIfDebug(); //Currently unimplemented.
                        break;

                    default:
                        return false;
                }

                return false;
            }
        }

        public bool AllowPasteObject(object input)
        {
            if (input is Cyberware objCyberware)
            {
                if (!string.IsNullOrEmpty(objCyberware.PlugsIntoModularMount))
                {
                    if (objCyberware.PlugsIntoModularMount != HasModularMount)
                        return false;
                    string strInputHasModularMount = objCyberware.HasModularMount;
                    if (Children.Any(x => x.PlugsIntoModularMount == strInputHasModularMount))
                        return false;

                    objCyberware.Location = Location;
                }

                if (objCyberware.SourceType != SourceType)
                    return true;
                string strAllowedSubsystems = AllowedSubsystems;
                if (!string.IsNullOrEmpty(strAllowedSubsystems))
                {
                    string strCategory = objCyberware.Category;
                    return strAllowedSubsystems.SplitNoAlloc(',').All(strSubsystem => strCategory != strSubsystem);
                }

                if (string.IsNullOrEmpty(objCyberware.HasModularMount) &&
                    string.IsNullOrEmpty(objCyberware.BlocksMounts))
                    return true;
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setDisallowedMounts))
                using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                out HashSet<string> setHasMounts))
                {
                    foreach (string strLoop in BlocksMounts.SplitNoAlloc(','))
                        setDisallowedMounts.Add(strLoop + Location);
                    string strLoopHasModularMount = HasModularMount;
                    if (!string.IsNullOrEmpty(strLoopHasModularMount))
                        setHasMounts.Add(strLoopHasModularMount);
                    foreach (Cyberware objLoopCyberware in Children.DeepWhere(
                                 x => x.Children, x => string.IsNullOrEmpty(x.PlugsIntoModularMount)))
                    {
                        foreach (string strLoop in objLoopCyberware.BlocksMounts.SplitNoAlloc(','))
                        {
                            if (!setDisallowedMounts.Contains(strLoop + objLoopCyberware.Location))
                                setDisallowedMounts.Add(strLoop + objLoopCyberware.Location);
                        }

                        strLoopHasModularMount = objLoopCyberware.HasModularMount;
                        if (string.IsNullOrEmpty(strLoopHasModularMount)) continue;
                        if (!setHasMounts.Contains(strLoopHasModularMount))
                            setHasMounts.Add(strLoopHasModularMount);
                    }

                    if (!string.IsNullOrEmpty(objCyberware.HasModularMount) &&
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

                            if (strCheck == objCyberware.HasModularMount)
                            {
                                return false;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(objCyberware.BlocksMounts))
                        return true;
                    if (string.IsNullOrEmpty(objCyberware.Location) && string.IsNullOrEmpty(Location) &&
                        (Children.All(x => x.Location != "Left") || Children.All(x => x.Location != "Right")))
                        return true;
                    return objCyberware.BlocksMounts.SplitNoAlloc(',').All(strLoop => !setHasMounts.Contains(strLoop));
                }
            }

            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (Cyberware objChild in _lstChildren)
                objChild.Dispose();
            foreach (Gear objChild in _lstGear)
                objChild.Dispose();
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstChildren.Dispose();
            _lstGear.Dispose();
            Utils.StringHashSetPool.Return(_lstIncludeInPairBonus);
            Utils.StringHashSetPool.Return(_lstIncludeInWirelessPairBonus);
        }

        /// <inheritdoc />
        public int ChildCostMultiplier => 1;
    }
}
