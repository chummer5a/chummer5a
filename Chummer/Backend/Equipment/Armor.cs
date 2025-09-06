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
using System.Buffers;
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
using Chummer.Backend.Enums;
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A specific piece of Armor.
    /// </summary>
    [HubClassTag("SourceID", true, "TotalArmor", "Extra")]
    [DebuggerDisplay("{DisplayName(null, \"en-us\")}")]
    public sealed class Armor : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, ICanSell, IHasChildrenAndCost<Gear>, IHasCustomName, IHasLocation, ICanEquip, IHasSource, IHasRating, ICanSort, IHasWirelessBonus, IHasStolenProperty, ICanPaste, IHasGear, IHasMatrixAttributes, ICanBlackMarketDiscount, IDisposable, IAsyncDisposable
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private Guid _guiWeaponID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strArmorValue = "0";
        private string _strArmorOverrideValue = string.Empty;
        private string _strArmorCapacity = "0";
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strWeight = string.Empty;
        private int _intRating;
        private string _strMaxRating;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strArmorName = string.Empty;
        private string _strExtra = string.Empty;
        private string _strRatingLabel = "String_Rating";
        private int _intDamage;
        private bool _blnEquipped = true;
        private readonly Character _objCharacter;
        private readonly TaggedObservableCollection<ArmorMod> _lstArmorMods;
        private readonly TaggedObservableCollection<Gear> _lstGear;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private Location _objLocation;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnDiscountCost;
        private int _intSortOrder;
        private bool _blnStolen;
        private bool _blnEncumbrance = true;
        private bool _blnSkipEvents;

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
        private bool _blnCanSwapAttributes;
        private bool _blnWirelessOn = true;
        private int _intMatrixCMBonus;
        private int _intMatrixCMFilled;

        #region Constructor, Create, Save, Load, and Print Methods

        public Armor(Character objCharacter)
        {
            // Create the GUID for the new piece of Armor.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            _lstArmorMods = new TaggedObservableCollection<ArmorMod>(objCharacter.LockObject);
            _lstArmorMods.AddTaggedCollectionChanged(this, ArmorModsOnCollectionChanged);
            _lstGear = new TaggedObservableCollection<Gear>(objCharacter.LockObject);
            _lstGear.AddTaggedCollectionChanged(this, GearOnCollectionChanged);
        }

        private async Task ArmorModsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            bool blnDoEquippedArmorRefresh = false;
            bool blnDoArmorEncumbranceRefresh = false;
            bool blnEverDoArmorEncumbranceRefresh = Equipped && Encumbrance && (ArmorValue.StartsWith('+') || ArmorValue.StartsWith('-')
                || ArmorOverrideValue.StartsWith('+') || ArmorOverrideValue.StartsWith('-'));
            List<ArmorMod> lstImprovementSourcesToProcess = new List<ArmorMod>(e.NewItems?.Count ?? 0);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // ReSharper disable once PossibleNullReferenceException
                    foreach (ArmorMod objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (!_blnSkipEvents && objNewItem.Equipped)
                        {
                            blnDoEquippedArmorRefresh = Equipped;
                            blnDoArmorEncumbranceRefresh = blnEverDoArmorEncumbranceRefresh && objNewItem.Encumbrance;
                            lstImprovementSourcesToProcess.Add(objNewItem);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ArmorMod objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if (!_blnSkipEvents && objOldItem.Equipped)
                        {
                            blnDoEquippedArmorRefresh = Equipped;
                            blnDoArmorEncumbranceRefresh = blnEverDoArmorEncumbranceRefresh && objOldItem.Encumbrance;
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    // ReSharper disable once AssignNullToNotNullAttribute
                    HashSet<ArmorMod> setNewItems = e.NewItems.OfType<ArmorMod>().ToHashSet();
                    foreach (ArmorMod objOldItem in e.OldItems)
                    {
                        if (setNewItems.Contains(objOldItem))
                            continue;
                        objOldItem.Parent = null;
                        if (!_blnSkipEvents && objOldItem.Equipped)
                        {
                            blnDoEquippedArmorRefresh = Equipped;
                            blnDoArmorEncumbranceRefresh = blnEverDoArmorEncumbranceRefresh && objOldItem.Encumbrance;
                        }
                    }

                    foreach (ArmorMod objNewItem in setNewItems)
                    {
                        objNewItem.Parent = this;
                        if (!_blnSkipEvents && objNewItem.Equipped)
                        {
                            blnDoEquippedArmorRefresh = Equipped;
                            blnDoArmorEncumbranceRefresh = blnEverDoArmorEncumbranceRefresh && objNewItem.Encumbrance;
                            lstImprovementSourcesToProcess.Add(objNewItem);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    blnDoArmorEncumbranceRefresh = !_blnSkipEvents && blnEverDoArmorEncumbranceRefresh;
                    break;
            }

            // Short-circuits this in case we are adding mods to an armor that is not on the character (happens when browsing for new armor to add)
            if (_blnSkipEvents || _objCharacter?.IsLoading != false || !await _objCharacter.Armor.ContainsAsync(this, token).ConfigureAwait(false))
                return;

            using (new FetchSafelyFromSafeObjectPool<Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>>>(
                       Utils.DictionaryForMultiplePropertyChangedPool,
                       out Dictionary<INotifyMultiplePropertiesChangedAsync, HashSet<string>> dicChangedProperties))
            {
                try
                {
                    if (blnDoEquippedArmorRefresh)
                    {
                        if (!dicChangedProperties.TryGetValue(_objCharacter,
                                                              out HashSet<string> setChangedProperties))
                        {
                            setChangedProperties = Utils.StringHashSetPool.Get();
                            dicChangedProperties.Add(_objCharacter, setChangedProperties);
                        }

                        setChangedProperties.Add(nameof(Character.TotalCarriedWeight));
                        setChangedProperties.Add(nameof(Character.GetArmorRating));
                    }

                    if (blnDoArmorEncumbranceRefresh)
                    {
                        if (!dicChangedProperties.TryGetValue(_objCharacter,
                                                              out HashSet<string> setChangedProperties))
                        {
                            setChangedProperties = Utils.StringHashSetPool.Get();
                            dicChangedProperties.Add(_objCharacter, setChangedProperties);
                        }

                        setChangedProperties.Add(nameof(Character.ArmorEncumbrance));
                    }

                    if (lstImprovementSourcesToProcess.Count > 0 && !_objCharacter.IsLoading)
                    {
                        foreach (ArmorMod objItem in lstImprovementSourcesToProcess)
                        {
                            // Needed in order to properly process named sources where
                            // the tooltip was built before the object was added to the character
                            await _objCharacter.Improvements.ForEachAsync(objImprovement =>
                            {
                                if (objImprovement.SourceName.TrimEndOnce("Wireless")
                                    == objItem.InternalId
                                    && objImprovement.Enabled)
                                {
                                    foreach ((INotifyMultiplePropertiesChangedAsync objItemToUpdate,
                                                 string strPropertyToUpdate) in objImprovement
                                                 .GetRelevantPropertyChangers())
                                    {
                                        if (dicChangedProperties.TryGetValue(
                                                objItemToUpdate, out HashSet<string> setChangedProperties))
                                            setChangedProperties.Add(strPropertyToUpdate);
                                        else
                                        {
                                            HashSet<string> setTemp = Utils.StringHashSetPool.Get();
                                            setTemp.Add(strPropertyToUpdate);
                                            dicChangedProperties.Add(objItemToUpdate, setTemp);
                                        }
                                    }
                                }
                            }, token: token).ConfigureAwait(false);
                        }
                    }

                    foreach (KeyValuePair<INotifyMultiplePropertiesChangedAsync, HashSet<string>> kvpToProcess in
                             dicChangedProperties)
                    {
                        await kvpToProcess.Key.OnMultiplePropertiesChangedAsync(kvpToProcess.Value.ToList(), token).ConfigureAwait(false);
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

        private async Task GearOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        await objNewItem.SetParentAsync(this, token).ConfigureAwait(false);
                        if (!_blnSkipEvents && Equipped)
                            await objNewItem.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        await objOldItem.SetParentAsync(null, token).ConfigureAwait(false);
                        if (!_blnSkipEvents && Equipped)
                            await objOldItem.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (Gear objOldItem in e.OldItems)
                    {
                        await objOldItem.SetParentAsync(null, token).ConfigureAwait(false);
                        if (!_blnSkipEvents && Equipped)
                            await objOldItem.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                    }
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        await objNewItem.SetParentAsync(this, token).ConfigureAwait(false);
                        if (!_blnSkipEvents && Equipped)
                            await objNewItem.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                    }
                    break;
            }
        }

        /// <summary>
        /// Create an Armor from an XmlNode.
        /// </summary>
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="intRating">Rating of the item.</param>
        /// <param name="lstWeapons">List of Weapons that added to the character's weapons.</param>
        /// <param name="blnSkipCost">Whether creating the Armor should skip the Variable price dialogue (should only be used by SelectArmor form).</param>
        /// <param name="blnCreateChildren">Whether child items should be created.</param>
        /// <param name="blnSkipSelectForms">Whether to skip forms that are created for bonuses like Custom Fit (Stack).</param>
        /// <param name="blnForSelectForm">Whether this armor is being created for a Selection form (which means a lot of expensive code should be skipped).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlArmorNode, int intRating, IList<Weapon> lstWeapons, bool blnSkipCost = false,
            bool blnCreateChildren = true, bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlArmorNode, intRating, lstWeapons,
                blnSkipCost, blnCreateChildren, blnSkipSelectForms, blnForSelectForm, token), token);
        }

        /// <summary>
        /// Create an Armor from an XmlNode.
        /// </summary>
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="intRating">Rating of the item.</param>
        /// <param name="lstWeapons">List of Weapons that added to the character's weapons.</param>
        /// <param name="blnSkipCost">Whether creating the Armor should skip the Variable price dialogue (should only be used by SelectArmor form).</param>
        /// <param name="blnCreateChildren">Whether child items should be created.</param>
        /// <param name="blnSkipSelectForms">Whether to skip forms that are created for bonuses like Custom Fit (Stack).</param>
        /// <param name="blnForSelectForm">Whether this armor is being created for a Selection form (which means a lot of expensive code should be skipped).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task CreateAsync(XmlNode objXmlArmorNode, int intRating, IList<Weapon> lstWeapons, bool blnSkipCost = false,
            bool blnCreateChildren = true, bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlArmorNode, intRating, lstWeapons, blnSkipCost, blnCreateChildren,
                blnSkipSelectForms, blnForSelectForm, token);
        }

        private async Task CreateCoreAsync(bool blnSync, XmlNode objXmlArmorNode, int intRating, IList<Weapon> lstWeapons, bool blnSkipCost = false,
            bool blnCreateChildren = true, bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!objXmlArmorNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for armor xmlnode", objXmlArmorNode });
                Utils.BreakIfDebug();
            }
            else
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            _blnSkipEvents = !blnForSelectForm;
            _blnEquipped = !blnForSelectForm && !blnSkipSelectForms;
            objXmlArmorNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArmorNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlArmorNode.TryGetStringFieldQuickly("armor", ref _strArmorValue);
            if (objXmlArmorNode.TryGetStringFieldQuickly("armoroverride", ref _strArmorOverrideValue) && _strArmorOverrideValue == "0")
                _strArmorOverrideValue = string.Empty;
            objXmlArmorNode.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            _intRating = intRating; // Set first to make MaxRatingValue work properly
            _intRating = Math.Min(intRating, blnSync ? MaxRatingValue : await GetMaxRatingValueAsync(token).ConfigureAwait(false));
            objXmlArmorNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objXmlArmorNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlArmorNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlArmorNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArmorNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlArmorNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArmorNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            if (!blnForSelectForm)
            {
                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlArmorNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);

                if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
                {
                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        Notes = CommonFunctions.GetBookNotes(objXmlArmorNode, Name, CurrentDisplayName, Source,
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            Page, DisplayPage(GlobalSettings.Language), _objCharacter, token);
                    else
                        await SetNotesAsync(await CommonFunctions.GetBookNotesAsync(objXmlArmorNode, Name,
                            await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                            await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter,
                            token).ConfigureAwait(false), token).ConfigureAwait(false);
                }
            }

            if (!objXmlArmorNode.TryGetBoolFieldQuickly("encumbrance", ref _blnEncumbrance))
                _blnEncumbrance = true;
            _nodBonus = objXmlArmorNode["bonus"];
            _nodWirelessBonus = objXmlArmorNode["wirelessbonus"];
            _blnWirelessOn = false;

            objXmlArmorNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlArmorNode.TryGetStringFieldQuickly("weight", ref _strWeight);

            // Check for a Variable Cost.
            if (!blnForSelectForm && !blnSkipCost && _strCost.StartsWith("Variable(", StringComparison.Ordinal))
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
                        decimal.TryParse(strFirstHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);
                        decimal.TryParse(strSecondHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax);
                    }
                    else
                        decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);

                    if (decMin != decimal.MinValue || decMax != decimal.MaxValue)
                    {
                        if (decMax > 1000000)
                            decMax = 1000000;

                        if (blnSync)
                        {
                            using (ThreadSafeForm<SelectNumber> frmPickNumber
                                   // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                   = ThreadSafeForm<SelectNumber>.Get(() => new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
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
                                _strCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                            }
                        }
                        else
                        {
                            string strDescription = string.Format(
                                GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync("String_SelectVariableCost", token: token).ConfigureAwait(false),
                                await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false));
                            int intDecimalPlaces = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMaxNuyenDecimalsAsync(token).ConfigureAwait(false);
                            using (ThreadSafeForm<SelectNumber> frmPickNumber
                                   = await ThreadSafeForm<SelectNumber>.GetAsync(() => new SelectNumber(intDecimalPlaces)
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
                                _strCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }
                    else
                        _strCost = strFirstHalf;
                }
                else
                    _strCost = strFirstHalf;
            }

            if (!blnForSelectForm && !blnSkipSelectForms)
            {
                if (Bonus != null)
                {
                    if (blnSync)
                    {
                        // ReSharper disable once MethodHasAsyncOverload
                        if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor,
                                _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, Rating,
                                CurrentDisplayNameShort, token: token))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                    }
                    else if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Armor,
                                 _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, await GetRatingAsync(token),
                                 await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    if (!string.IsNullOrEmpty(strSelectedValue))
                    {
                        _strExtra = strSelectedValue;
                    }
                }

                XmlElement xmlSelectModesFromCategory = objXmlArmorNode["selectmodsfromcategory"];
                if (xmlSelectModesFromCategory != null)
                {
                    XmlDocument objXmlDocument = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData("armor.xml", token: token)
                        : await _objCharacter.LoadDataAsync("armor.xml", token: token).ConfigureAwait(false);

                    // More than one Weapon can be added, so loop through all occurrences.
                    foreach (XmlNode objXmlCategoryNode in xmlSelectModesFromCategory)
                    {
                        using (ThreadSafeForm<SelectArmorMod> frmPickArmorMod = blnSync
                                   // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                   ? ThreadSafeForm<SelectArmorMod>.Get(
                                       () => new SelectArmorMod(_objCharacter, this)
                                       {
                                           AllowedCategories = objXmlCategoryNode.InnerText,
                                           ExcludeGeneralCategory = true
                                       })
                                   : await ThreadSafeForm<SelectArmorMod>.GetAsync(
                                       () => new SelectArmorMod(_objCharacter, this)
                                       {
                                           AllowedCategories = objXmlCategoryNode.InnerText,
                                           ExcludeGeneralCategory = true
                                       }, token).ConfigureAwait(false))
                        {
                            if ((blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? frmPickArmorMod.ShowDialogSafe(_objCharacter, token)
                                    : await frmPickArmorMod.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false)) == DialogResult.Cancel)
                            {
                                _guiID = Guid.Empty;
                                return;
                            }

                            // Locate the selected piece.
                            XmlNode objXmlMod = objXmlDocument.TryGetNodeByNameOrId("/chummer/mods/mod",
                                frmPickArmorMod.MyForm.SelectedArmorMod);

                            ArmorMod objMod = new ArmorMod(_objCharacter);
                            try
                            {
                                if (objXmlMod != null)
                                {

                                    // ReSharper disable once AccessToModifiedClosure
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverload
                                        objMod.Create(objXmlMod, intRating, lstWeapons, blnSkipCost, blnSkipSelectForms, blnForSelectForm, token);
                                    else
                                        await objMod.CreateAsync(objXmlMod, intRating, lstWeapons, blnSkipCost, blnSkipSelectForms, blnForSelectForm, token).ConfigureAwait(false);
                                    objMod.IncludedInArmor = true;
                                    objMod.ArmorCapacity = "[0]";
                                    objMod.Cost = "0";
                                    objMod.MaxRating = (blnSync ? objMod.Rating : await objMod.GetRatingAsync(token).ConfigureAwait(false))
                                        .ToString(GlobalSettings.InvariantCultureInfo);
                                }
                                else
                                {
                                    objMod.Name = _strName;
                                    objMod.Category = "Features";
                                    objMod.Avail = "0";
                                    objMod.Source = _strSource;
                                    objMod.Page = _strPage;
                                    objMod.IncludedInArmor = true;
                                    objMod.ArmorCapacity = "[0]";
                                    objMod.Cost = "0";
                                    objMod.MaxRating = "0";
                                    if (blnSync)
                                        Rating = 0;
                                    else
                                        await SetRatingAsync(0, token).ConfigureAwait(false);
                                }

                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    _lstArmorMods.Add(objMod);
                                else
                                    await _lstArmorMods.AddAsync(objMod, token).ConfigureAwait(false);
                            }
                            catch
                            {
                                if (blnSync)
                                    objMod.DeleteArmorMod();
                                else
                                    await objMod.DeleteArmorModAsync(token: CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }
                        }
                    }
                }
            }

            // Add any Armor Mods that come with the Armor.
            if (objXmlArmorNode["mods"] != null && blnCreateChildren)
            {
                XmlNodeList xmlArmorList = objXmlArmorNode.SelectNodes("mods/name");
                if (xmlArmorList != null)
                {
                    XmlDocument objXmlArmorDocument = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData("armor.xml", token: token)
                        : await _objCharacter.LoadDataAsync("armor.xml", token: token).ConfigureAwait(false);
                    foreach (XmlNode objXmlArmorMod in xmlArmorList)
                    {
                        XmlAttributeCollection objXmlAttributes = objXmlArmorMod.Attributes;
                        string strForceValue = string.Empty;
                        if (objXmlAttributes != null)
                        {
                            int.TryParse(objXmlAttributes["rating"]?.InnerText, NumberStyles.Any,
                                GlobalSettings.InvariantCultureInfo, out intRating);
                            strForceValue = objXmlAttributes["select"]?.InnerText ?? string.Empty;
                        }

                        XmlNode objXmlMod = objXmlArmorDocument.TryGetNodeByNameOrId("/chummer/mods/mod", objXmlArmorMod.InnerText);
                        ArmorMod objMod = new ArmorMod(_objCharacter);
                        try
                        {
                            if (objXmlMod != null)
                            {
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objMod.Create(objXmlMod, intRating, lstWeapons, blnSkipCost, blnSkipSelectForms, blnForSelectForm, token);
                                else
                                    await objMod.CreateAsync(objXmlMod, intRating, lstWeapons, blnSkipCost, blnSkipSelectForms, blnForSelectForm, token).ConfigureAwait(false);
                                if (string.IsNullOrWhiteSpace(objMod.Extra))
                                {
                                    objMod.Extra = strForceValue;
                                }

                                objMod.IncludedInArmor = true;
                                objMod.ArmorCapacity = "[0]";
                                objMod.Cost = "0";
                                string strMaxRating = objXmlAttributes?["maxrating"]?.InnerText;
                                //If maxrating is being specified, we're intentionally bypassing the normal maximum rating. Set the maxrating first, then the rating again.
                                if (!string.IsNullOrEmpty(strMaxRating))
                                {
                                    objMod.MaxRating = strMaxRating;
                                    int intDummy = intRating;
                                    string strOverrideRating = objXmlAttributes["rating"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strOverrideRating))
                                        int.TryParse(strOverrideRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                            out intDummy);
                                    if (blnSync)
                                        objMod.Rating = intDummy;
                                    else
                                        await objMod.SetRatingAsync(intDummy, token).ConfigureAwait(false);
                                }
                                else
                                {
                                    objMod.MaxRating = (blnSync ? objMod.Rating : await objMod.GetRatingAsync(token).ConfigureAwait(false))
                                        .ToString(GlobalSettings.InvariantCultureInfo);
                                }
                            }
                            else
                            {
                                int intLoopRating = 0;
                                string strLoopMaximumRating = string.Empty;
                                if (objXmlAttributes != null)
                                {
                                    int.TryParse(objXmlAttributes["rating"]?.InnerText, NumberStyles.Any,
                                        GlobalSettings.InvariantCultureInfo, out intLoopRating);
                                    strLoopMaximumRating = objXmlAttributes["maxrating"]?.InnerText ?? string.Empty;
                                }
                                objMod.Name = _strName;
                                objMod.Category = "Features";
                                objMod.Avail = "0";
                                objMod.Source = _strSource;
                                objMod.Page = _strPage;
                                objMod.IncludedInArmor = true;
                                objMod.ArmorCapacity = "[0]";
                                objMod.Cost = "0";
                                objMod.MaxRating = strLoopMaximumRating;
                                objMod.Extra = strForceValue;
                                if (blnSync)
                                    Rating = intLoopRating;
                                else
                                    await SetRatingAsync(intLoopRating, token).ConfigureAwait(false);
                            }

                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                _lstArmorMods.Add(objMod);
                            else
                                await _lstArmorMods.AddAsync(objMod, token).ConfigureAwait(false);
                        }
                        catch
                        {
                            if (blnSync)
                                objMod.DeleteArmorMod();
                            else
                                await objMod.DeleteArmorModAsync(token: CancellationToken.None).ConfigureAwait(false);
                            throw;
                        }
                    }
                }
            }

            // Add any Gear that comes with the Armor.
            if (objXmlArmorNode["gears"] != null && blnCreateChildren)
            {
                XmlDocument objXmlGearDocument = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("gear.xml", token: token)
                    : await _objCharacter.LoadDataAsync("gear.xml", token: token).ConfigureAwait(false);

                XmlNodeList objXmlGearList = objXmlArmorNode["gears"].SelectNodes("usegear");
                List<Weapon> lstChildWeapons = new List<Weapon>(1);
                foreach (XmlNode objXmlArmorGear in objXmlGearList)
                {
                    Gear objGear = new Gear(_objCharacter);
                    try
                    {
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            if (!objGear.CreateFromNode(objXmlGearDocument, objXmlArmorGear, lstChildWeapons, !blnSkipSelectForms))
                            {
                                objGear.DeleteGear();
                                continue;
                            }
                        }
                        else if (!await objGear.CreateFromNodeAsync(objXmlGearDocument, objXmlArmorGear, lstChildWeapons, !blnSkipSelectForms, token: token).ConfigureAwait(false))
                        {
                            await objGear.DeleteGearAsync(token: CancellationToken.None).ConfigureAwait(false);
                            continue;
                        }
                        foreach (Weapon objWeapon in lstChildWeapons)
                        {
                            objWeapon.ParentID = InternalId;
                        }
                        if (blnSync)
                            objGear.Parent = this;
                        else
                            await objGear.SetParentAsync(this, token).ConfigureAwait(false);
                        objGear.ParentID = InternalId;
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            GearChildren.Add(objGear);
                        else
                            await GearChildren.AddAsync(objGear, token).ConfigureAwait(false);
                    }
                    catch
                    {
                        if (blnSync)
                            objGear.DeleteGear();
                        else
                            await objGear.DeleteGearAsync(token: CancellationToken.None).ConfigureAwait(false);
                        throw;
                    }
                }
                lstWeapons?.AddRange(lstChildWeapons);
            }

            objXmlArmorNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objXmlArmorNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlArmorNode.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlArmorNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlArmorNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlArmorNode.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            }
            else
            {
                _blnCanSwapAttributes = true;
                string[] strArray = _strAttributeArray.SplitFixedSizePooledArray(',', 4);
                try
                {
                    _strAttack = strArray[0];
                    _strSleaze = strArray[1];
                    _strDataProcessing = strArray[2];
                    _strFirewall = strArray[3];
                }
                finally
                {
                    ArrayPool<string>.Shared.Return(strArray);
                }
            }
            objXmlArmorNode.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlArmorNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlArmorNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlArmorNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlArmorNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlArmorNode.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
            objXmlArmorNode.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            objXmlArmorNode.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);

            if (!blnForSelectForm && !blnSkipSelectForms)
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    RefreshWirelessBonuses();
                else
                    await RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
            }

            XmlDocument objXmlWeaponDocument = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("weapons.xml", token: token)
                : await _objCharacter.LoadDataAsync("weapons.xml", token: token).ConfigureAwait(false);

            // More than one Weapon can be added, so loop through all occurrences.
            foreach (XPathNavigator objXmlAddWeapon in objXmlArmorNode.CreateNavigator().Select("addweapon"))
            {
                XmlNode objXmlWeapon = objXmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon",
                    objXmlAddWeapon.Value);

                if (objXmlWeapon != null)
                {
                    int intAddWeaponRating = 0;
                    string strLoopRating = objXmlAddWeapon.SelectSingleNodeAndCacheExpression("@rating", token)?.Value;
                    if (!string.IsNullOrEmpty(strLoopRating))
                    {
                        intAddWeaponRating = blnSync
                            ? ProcessRatingString(strLoopRating, () => Rating)
                            : await ProcessRatingStringAsync(strLoopRating, () => GetRatingAsync(token), token);
                    }

                    Weapon objGearWeapon = new Weapon(_objCharacter);
                    try
                    {
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverload
                            objGearWeapon.Create(objXmlWeapon, lstWeapons, true,
                                !blnSkipSelectForms,
                                blnSkipSelectForms, intAddWeaponRating, blnForSelectForm, token);
                        else
                            await objGearWeapon.CreateAsync(objXmlWeapon, lstWeapons, true,
                                !blnSkipSelectForms,
                                blnSkipSelectForms, intAddWeaponRating, blnForSelectForm, token).ConfigureAwait(false);
                        objGearWeapon.ParentID = InternalId;
                        objGearWeapon.Cost = "0";
                        if (Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID))
                            lstWeapons.Add(objGearWeapon);
                        else
                            _guiWeaponID = Guid.Empty;
                    }
                    catch
                    {
                        if (blnSync)
                            objGearWeapon.DeleteWeapon();
                        else
                            await objGearWeapon.DeleteWeaponAsync(token: CancellationToken.None).ConfigureAwait(false);
                        throw;
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
            objWriter.WriteStartElement("armor");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("armor", _strArmorValue);
            objWriter.WriteElementString("armoroverride", _strArmorOverrideValue);
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("weight", _strWeight);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("armorname", _strArmorName);
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("devicerating", _strDeviceRating);
            objWriter.WriteElementString("programlimit", _strProgramLimit);
            objWriter.WriteElementString("overclocked", _strOverclocked);
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
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmbonus",
                _intMatrixCMBonus.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("canformpersona", _strCanFormPersona);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("damage", _intDamage.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("ratinglabel", _strRatingLabel);
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("emcumbrance", _blnEncumbrance.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteStartElement("armormods");
            foreach (ArmorMod objMod in _lstArmorMods)
            {
                objMod.Save(objWriter);
            }
            objWriter.WriteEndElement();
            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    objGear.Save(objWriter);
                }
                objWriter.WriteEndElement();
            }
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Armor from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether we are loading a copy of an existing armor.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objNode, blnCopy));
        }

        /// <summary>
        /// Load the Armor from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether we are loading a copy of an existing armor.</param>
        public Task LoadAsync(XmlNode objNode, bool blnCopy = false, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objNode, blnCopy, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode objNode, bool blnCopy = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objNode == null)
                return;
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            Lazy<XPathNavigator> objMyNode = null;
            Microsoft.VisualStudio.Threading.AsyncLazy<XPathNavigator> objMyNodeAsync = null;
            if (blnSync)
                objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath());
            else
                objMyNodeAsync = new Microsoft.VisualStudio.Threading.AsyncLazy<XPathNavigator>(() => this.GetNodeXPathAsync(token), Utils.JoinableTaskFactory);
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
                _objLocation = null;
            }
            else
            {
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                {
                    _guiID = Guid.NewGuid();
                }
                string strLocation = objNode["location"]?.InnerText;
                if (!string.IsNullOrEmpty(strLocation))
                {
                    if (blnSync)
                    {
                        if (Guid.TryParse(strLocation, out Guid temp))
                        {
                            string strLocationId = temp.ToString();
                            // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                            Location =
                                _objCharacter.ArmorLocations.FirstOrDefault(location =>
                                    location.InternalId == strLocationId);
                        }
                        else
                        {
                            //Legacy. Location is a string.
                            Location =
                                _objCharacter.ArmorLocations.FirstOrDefault(location =>
                                    location.Name == strLocation);
                        }
                        _objLocation?.Children.Add(this);
                    }
                    else
                    {
                        if (Guid.TryParse(strLocation, out Guid temp))
                        {
                            string strLocationId = temp.ToString();
                            // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                            Location =
                                await (await _objCharacter.GetArmorLocationsAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(location =>
                                    location.InternalId == strLocationId, token).ConfigureAwait(false);
                        }
                        else
                        {
                            //Legacy. Location is a string.
                            Location =
                                await (await _objCharacter.GetArmorLocationsAsync(token).ConfigureAwait(false)).FirstOrDefaultAsync(location =>
                                    location.Name == strLocation, token).ConfigureAwait(false);
                        }
                        if (_objLocation != null)
                            await _objLocation.Children.AddAsync(this, token).ConfigureAwait(false);
                    }
                }
            }

            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            if (!objNode.TryGetBoolFieldQuickly("encumbrance", ref _blnEncumbrance))
                _blnEncumbrance = true;

            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("armor", ref _strArmorValue);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            if (!objNode.TryGetStringFieldQuickly("weight", ref _strWeight))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("weight", ref _strWeight);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            if (objNode.TryGetStringFieldQuickly("armoroverride", ref _strArmorOverrideValue) && _strArmorOverrideValue == "0")
                _strArmorOverrideValue = string.Empty;
            objNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            _intRating = Math.Min(_intRating, MaxRatingValue);
            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("armorname", ref _strArmorName);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("damage", ref _intDamage);
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = false;
            bool blnIsActive = false;
            if (blnSync)
            {
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
            }
            else
            {
                if (objNode.TryGetBoolFieldQuickly("active", ref blnIsActive) && blnIsActive)
                    await this.SetActiveCommlinkAsync(_objCharacter, true, token).ConfigureAwait(false);
                if (blnCopy)
                {
                    await this.SetHomeNodeAsync(_objCharacter, false, token).ConfigureAwait(false);
                }
                else
                {
                    bool blnIsHomeNode = false;
                    if (objNode.TryGetBoolFieldQuickly("homenode", ref blnIsHomeNode) && blnIsHomeNode)
                    {
                        await this.SetHomeNodeAsync(_objCharacter, true, token).ConfigureAwait(false);
                    }
                }
            }

            if (!objNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objNode.TryGetStringFieldQuickly("programlimit", ref _strProgramLimit))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
            objNode.TryGetStringFieldQuickly("overclocked", ref _strOverclocked);
            if (!objNode.TryGetStringFieldQuickly("attack", ref _strAttack))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("attack", ref _strAttack);
            if (!objNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
            if (!objNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("firewall", ref _strFirewall))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            if (!objNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray);
            if (!objNode.TryGetStringFieldQuickly("modattack", ref _strModAttack))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            if (!objNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            if (!objNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            if (!objNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
            if (!objNode.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona))
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            XmlElement xmlChildrenNode = objNode["armormods"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList nodMods = xmlChildrenNode.SelectNodes("armormod"))
                {
                    if (nodMods != null)
                    {
                        if (blnSync)
                        {
                            foreach (XmlNode nodMod in nodMods)
                            {
                                ArmorMod objMod = new ArmorMod(_objCharacter);
                                try
                                {
                                    objMod.Load(nodMod, blnCopy);
                                    _lstArmorMods.Add(objMod);
                                }
                                catch
                                {
                                    objMod.DeleteArmorMod();
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            foreach (XmlNode nodMod in nodMods)
                            {
                                ArmorMod objMod = new ArmorMod(_objCharacter);
                                try
                                {
                                    await objMod.LoadAsync(nodMod, blnCopy, token).ConfigureAwait(false);
                                    await _lstArmorMods.AddAsync(objMod, token).ConfigureAwait(false);
                                }
                                catch
                                {
                                    await objMod.DeleteArmorModAsync(token: CancellationToken.None).ConfigureAwait(false);
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
            xmlChildrenNode = objNode["gears"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList nodGears = xmlChildrenNode.SelectNodes("gear"))
                {
                    if (nodGears != null)
                    {
                        if (blnSync)
                        {
                            foreach (XmlNode nodGear in nodGears)
                            {
                                Gear objGear = new Gear(_objCharacter);
                                try
                                {
                                    objGear.Load(nodGear, blnCopy);
                                    _lstGear.Add(objGear);
                                }
                                catch
                                {
                                    objGear.DeleteGear();
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            foreach (XmlNode nodGear in nodGears)
                            {
                                Gear objGear = new Gear(_objCharacter);
                                try
                                {
                                    await objGear.LoadAsync(nodGear, blnCopy, token).ConfigureAwait(false);
                                    await _lstGear.AddAsync(objGear, token).ConfigureAwait(false);
                                }
                                catch
                                {
                                    await objGear.DeleteGearAsync(token: CancellationToken.None).ConfigureAwait(false);
                                    throw;
                                }
                            }
                        }
                    }
                }
            }

            if (!blnCopy)
                return;

            if (blnSync)
            {
                if (Bonus != null)
                {
                    if (!string.IsNullOrEmpty(Extra))
                        ImprovementManager.SetForcedValue(Extra, _objCharacter);
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, Rating, CurrentDisplayNameShort, token: token);
                    string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    if (!string.IsNullOrEmpty(strSelectedValue))
                    {
                        Extra = strSelectedValue;
                    }
                }

                if (WirelessOn && WirelessBonus != null)
                {
                    ImprovementManager.SetForcedValue(Extra, _objCharacter);

                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), WirelessBonus, Rating, CurrentDisplayNameShort, token: token))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }

                if (!_blnEquipped)
                {
                    _blnEquipped = true;
                    Equipped = false;
                }

                RefreshWirelessBonuses();
            }
            else
            {
                if (Bonus != null)
                {
                    if (!string.IsNullOrEmpty(Extra))
                        ImprovementManager.SetForcedValue(Extra, _objCharacter);
                    await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, await GetRatingAsync(token), await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                    string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    if (!string.IsNullOrEmpty(strSelectedValue))
                    {
                        Extra = strSelectedValue;
                    }
                }

                if (WirelessOn && WirelessBonus != null)
                {
                    ImprovementManager.SetForcedValue(Extra, _objCharacter);

                    if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), WirelessBonus, await GetRatingAsync(token), await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token: token).ConfigureAwait(false))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }

                if (!_blnEquipped)
                {
                    _blnEquipped = true;
                    await SetEquippedAsync(false, token).ConfigureAwait(false);
                }

                await RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            // <armor>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("armor", token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname_english", await DisplayNameAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category", DisplayCategory(strLanguageToPrint), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("armor", await GetDisplayArmorValueAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("totalarmorcapacity", await TotalArmorCapacityAsync(objCulture, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("calculatedcapacity", await CalculatedCapacityAsync(objCulture, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("capacityremaining", (await GetCapacityRemainingAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("avail", await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("cost", (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("owncost", (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture), token).ConfigureAwait(false);
                string strWeightFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetWeightFormatAsync(token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weight", TotalWeight.ToString(strWeightFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ownweight", OwnWeight.ToString(strWeightFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("armorname", CustomName, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("equipped", Equipped.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ratinglabel", RatingLabel, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                // <armormods>
                XmlElementWriteHelper objArmorModsElement = await objWriter.StartElementAsync("armormods", token).ConfigureAwait(false);
                try
                {
                    foreach (ArmorMod objMod in ArmorMods)
                    {
                        await objMod.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    // </armormods>
                    await objArmorModsElement.DisposeAsync().ConfigureAwait(false);
                }
                // <gears>
                XmlElementWriteHelper objGearsElement = await objWriter.StartElementAsync("gears", token).ConfigureAwait(false);
                try
                {
                    foreach (Gear objGear in GearChildren)
                    {
                        await objGear.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                    }
                }
                finally
                {
                    // </gears>
                    await objGearsElement.DisposeAsync().ConfigureAwait(false);
                }
                await objWriter.WriteElementStringAsync("extra", await _objCharacter.TranslateExtraAsync(Extra, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("location", Location != null ? await Location.DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false) : string.Empty, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("attack", (await this.GetTotalMatrixAttributeAsync("Attack", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sleaze", (await this.GetTotalMatrixAttributeAsync("Sleaze", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("dataprocessing",
                                                        (await this.GetTotalMatrixAttributeAsync("Data Processing", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("firewall", (await this.GetTotalMatrixAttributeAsync("Firewall", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("devicerating",
                                                        (await this.GetTotalMatrixAttributeAsync("Device Rating", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("programlimit",
                                                        (await this.GetTotalMatrixAttributeAsync("Program Limit", token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("iscommlink", (await GetIsCommlinkAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("active",
                                                        (await this.IsActiveCommlinkAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("homenode",
                                                        (await this.IsHomeNodeAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("conditionmonitor", MatrixCM.ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("matrixcmfilled", MatrixCMFilled.ToString(objCulture), token).ConfigureAwait(false);
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                // </armor>
                await objBaseElement.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Armor in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Name of the Armor.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (Interlocked.Exchange(ref _strName, value) == value)
                    return;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
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
        /// Wireless Bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set => _nodWirelessBonus = value;
        }

        /// <summary>
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("armor.xml", strLanguage)
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value
                   ?? Category;
        }

        /// <summary>
        /// Armor's Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Whether the Armor contributes to Encumbrance.
        /// </summary>
        public bool Encumbrance => _blnEncumbrance;

        /// <summary>
        /// Armor's Armor value.
        /// </summary>
        public string ArmorValue
        {
            get => _strArmorValue;
            set => _strArmorValue = value;
        }

        /// <summary>
        /// Armor's Armor Override value.
        /// </summary>
        public string ArmorOverrideValue
        {
            get => _strArmorOverrideValue;
            set => _strArmorOverrideValue = value == "0" ? string.Empty : value;
        }

        /// <summary>
        /// Damage done to the Armor's Armor Rating.
        /// </summary>
        public int ArmorDamage
        {
            get => _intDamage;
            set
            {
                if (value < 0)
                    value = 0;
                if (Interlocked.Exchange(ref _intDamage, value) != value && Equipped && _objCharacter != null)
                {
                    _objCharacter.OnPropertyChanged(nameof(Character.GetArmorRating));
                    _objCharacter.RefreshArmorEncumbrance();
                }
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => Math.Min(_intRating, MaxRatingValue);
            set
            {
                value = Math.Min(value, MaxRatingValue);
                if (Interlocked.Exchange(ref _intRating, value) == value)
                    return;
                if (Equipped && _objCharacter != null)
                {
                    if (Weight.ContainsAny("FixedValues", "Rating") || GearChildren.Any(x => x.Equipped && x.Weight.Contains("Parent Rating")))
                    {
                        if (ArmorValue.ContainsAny("FixedValues", "Rating") || ArmorOverrideValue.ContainsAny("FixedValues", "Rating"))
                        {
                            _objCharacter.OnMultiplePropertyChanged(nameof(Character.TotalCarriedWeight), nameof(Character.GetArmorRating));
                            _objCharacter.RefreshArmorEncumbrance();
                        }
                        else
                        {
                            _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
                        }
                    }
                    else if (ArmorValue.ContainsAny("FixedValues", "Rating") || ArmorOverrideValue.ContainsAny("FixedValues", "Rating"))
                    {
                        _objCharacter.OnPropertyChanged(nameof(Character.GetArmorRating));
                        _objCharacter.RefreshArmorEncumbrance();
                    }
                }
                if (GearChildren.Count > 0)
                {
                    foreach (Gear objChild in GearChildren.Where(x => x.MaxRating.Contains("Parent") || x.MinRating.Contains("Parent")))
                    {
                        // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                        int intCurrentRating = objChild.Rating;
                        objChild.Rating = intCurrentRating;
                    }
                }
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public async Task<int> GetRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Math.Min(_intRating, await GetMaxRatingValueAsync(token).ConfigureAwait(false));
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public async Task SetRatingAsync(int value, CancellationToken token = default)
        {
            value = Math.Min(value, await GetMaxRatingValueAsync(token).ConfigureAwait(false));
            if (Interlocked.Exchange(ref _intRating, value) == value)
                return;
            if (Equipped && _objCharacter != null)
            {
                if (Weight.ContainsAny("FixedValues", "Rating") || await GearChildren.AnyAsync(x => x.Equipped && x.Weight.Contains("Parent Rating"), token).ConfigureAwait(false))
                {
                    if (ArmorValue.ContainsAny("FixedValues", "Rating") || ArmorOverrideValue.ContainsAny("FixedValues", "Rating"))
                    {
                        await _objCharacter.OnMultiplePropertyChangedAsync(token, nameof(Character.TotalCarriedWeight), nameof(Character.GetArmorRating)).ConfigureAwait(false);
                        await _objCharacter.RefreshArmorEncumbranceAsync(token).ConfigureAwait(false);
                    }
                    else
                    {
                        await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token).ConfigureAwait(false);
                    }
                }
                else if (ArmorValue.ContainsAny("FixedValues", "Rating") || ArmorOverrideValue.ContainsAny("FixedValues", "Rating"))
                {
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.GetArmorRating), token).ConfigureAwait(false);
                    await _objCharacter.RefreshArmorEncumbranceAsync(token).ConfigureAwait(false);
                }
            }
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
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public string MaxRating
        {
            get => _strMaxRating;
            set => _strMaxRating = value;
        }

        /// <summary>
        /// Maximum Rating (value form).
        /// </summary>
        public int MaxRatingValue
        {
            get
            {
                string strExpression = MaxRating;
                if (string.IsNullOrEmpty(strExpression))
                    return int.MaxValue;
                return string.IsNullOrEmpty(strExpression) ? int.MaxValue : ProcessRatingString(strExpression, _intRating);
            }
            set => MaxRating = value.ToString(GlobalSettings.InvariantCultureInfo);
        }

        /// <summary>
        /// Maximum Rating (value form).
        /// </summary>
        public Task<int> GetMaxRatingValueAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<int>(token);
            string strExpression = MaxRating;
            return string.IsNullOrEmpty(strExpression) ? Task.FromResult(int.MaxValue) : ProcessRatingStringAsync(strExpression, _intRating, token);
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private int ProcessRatingString(string strExpression, int intRating)
        {
            return ProcessRatingString(strExpression, () => intRating);
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private int ProcessRatingString(string strExpression, Func<int> funcRating)
        {
            return ProcessRatingStringAsDec(strExpression, funcRating, out bool _).StandardRound();
        }

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        private decimal ProcessRatingStringAsDec(string strExpression, int intRating)
        {
            return ProcessRatingStringAsDec(strExpression, () => intRating, out bool _);
        }

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        private decimal ProcessRatingStringAsDec(string strExpression, Func<int> funcRating)
        {
            return ProcessRatingStringAsDec(strExpression, funcRating, out bool _);
        }

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        private decimal ProcessRatingStringAsDec(string strExpression, Func<int> funcRating, out bool blnIsSuccess)
        {
            blnIsSuccess = true;
            if (string.IsNullOrEmpty(strExpression))
                return 0;
            strExpression = strExpression.ProcessFixedValuesString(funcRating).TrimStart('+');
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                blnIsSuccess = false;
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    Lazy<string> strRating = new Lazy<string>(() => funcRating().ToString(GlobalSettings.InvariantCultureInfo));
                    sbdValue.CheapReplace("{Rating}", () => strRating.Value);
                    sbdValue.CheapReplace("Rating", () => strRating.Value);
                    _objCharacter.ProcessAttributesInXPath(sbdValue, strExpression);
                    object objProcess;
                    (blnIsSuccess, objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                    return blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
                }
            }

            return decValue;
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private Task<int> ProcessRatingStringAsync(string strExpression, int intRating, CancellationToken token = default)
        {
            return ProcessRatingStringAsync(strExpression, () => Task.FromResult(intRating), token);
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private async Task<int> ProcessRatingStringAsync(string strExpression, Func<Task<int>> funcRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await ProcessRatingStringAsDecAsync(strExpression, funcRating, token).ConfigureAwait(false)).Item1.StandardRound();
        }

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        private Task<Tuple<decimal, bool>> ProcessRatingStringAsDecAsync(string strExpression, int intRating, CancellationToken token = default)
        {
            return ProcessRatingStringAsDecAsync(strExpression, () => Task.FromResult(intRating), token);
        }

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        private async Task<Tuple<decimal, bool>> ProcessRatingStringAsDecAsync(string strExpression, Func<Task<int>> funcRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strExpression))
                return new Tuple<decimal, bool>(0, true);
            bool blnIsSuccess = true;
            strExpression = (await strExpression.ProcessFixedValuesStringAsync(funcRating, token).ConfigureAwait(false)).TrimStart('+');
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                blnIsSuccess = false;
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        Microsoft.VisualStudio.Threading.AsyncLazy<string> strRating = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(async () => (await funcRating().ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), Utils.JoinableTaskFactory);
                        await sbdValue.CheapReplaceAsync("{Rating}", () => strRating.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync("Rating", () => strRating.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await _objCharacter
                            .ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                        strExpression = sbdValue.ToString();
                    }
                }
                object objProcess;
                (blnIsSuccess, objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(strExpression, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    decValue = Convert.ToDecimal((double)objProcess);
            }

            return new Tuple<decimal, bool>(decValue, blnIsSuccess);
        }

        /// <summary>
        /// How the rating should be referred to in UI.
        /// </summary>
        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
        }

        /// <summary>
        /// Armor's Capacity string.
        /// </summary>
        public string ArmorCapacity
        {
            get => _strArmorCapacity;
            set => _strArmorCapacity = value;
        }

        /// <summary>
        /// Armor's Capacity.
        /// </summary>
        public string TotalArmorCapacity(CultureInfo objCultureInfo)
        {
            string strArmorCapacity = ArmorCapacity;
            if (string.IsNullOrEmpty(strArmorCapacity))
                return 0.0m.ToString("#,0.##", objCultureInfo);
            strArmorCapacity = strArmorCapacity.ProcessFixedValuesString(() => Rating);

            if (strArmorCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                // If the Capacity is determined by the Rating, evaluate the expression.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                bool blnSquareBrackets = strArmorCapacity.StartsWith('[');
                string strCapacity = strArmorCapacity;
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                decimal decCapacity = ProcessRatingStringAsDec(strCapacity, () => Rating, out bool blnIsSuccess);
                string strReturn = blnIsSuccess ? decCapacity.ToString("#,0.##", objCultureInfo) : strCapacity;
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

                return strReturn;
            }

            return decValue.ToString("#,0.##", objCultureInfo);
        }

        /// <summary>
        /// Armor's Capacity.
        /// </summary>
        public async Task<string> TotalArmorCapacityAsync(CultureInfo objCultureInfo, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strArmorCapacity = ArmorCapacity;
            if (string.IsNullOrEmpty(strArmorCapacity))
                return 0.0m.ToString("#,0.##", objCultureInfo);
            strArmorCapacity = await strArmorCapacity.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false);
            if (strArmorCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                // If the Capacity is determined by the Rating, evaluate the expression.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                bool blnSquareBrackets = strArmorCapacity.StartsWith('[');
                string strCapacity = strArmorCapacity;
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                (decimal decCapacity, bool blnIsSuccess) = await ProcessRatingStringAsDecAsync(strCapacity, () => GetRatingAsync(token), token).ConfigureAwait(false);
                string strReturn = blnIsSuccess
                    ? decCapacity.ToString("#,0.##", objCultureInfo)
                    : strCapacity;
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

                return strReturn;
            }

            return decValue.ToString("#,0.##", objCultureInfo);
        }

        public string CurrentTotalArmorCapacity => TotalArmorCapacity(GlobalSettings.CultureInfo);

        public Task<string> GetCurrentTotalArmorCapacityAsync(CancellationToken token = default) => TotalArmorCapacityAsync(GlobalSettings.CultureInfo, token);

        /// <summary>
        /// Armor's Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// Armor's Cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Armor's Weight.
        /// </summary>
        public string Weight
        {
            get => _strWeight;
            set => _strWeight = value;
        }

        public async Task<Tuple<string, decimal>> DisplayCost(bool blnUseRating = true, decimal decMarkup = 0.0m, CancellationToken token = default)
        {
            decimal decItemCost = 0;
            string strReturn = Cost;
            strReturn = blnUseRating
                ? await strReturn.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false)
                : await strReturn.ProcessFixedValuesStringAsync(() => GetMaxRatingValueAsync(token), token).ConfigureAwait(false);
            string strNuyenSymbol = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
            if (strReturn.StartsWith("Variable(", StringComparison.Ordinal))
            {
                string strFirstHalf = strReturn.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                string strSecondHalf = string.Empty;
                int intHyphenIndex = strFirstHalf.IndexOf('-');
                if (intHyphenIndex != -1)
                {
                    if (intHyphenIndex + 1 < strFirstHalf.Length)
                        strSecondHalf = strFirstHalf.Substring(intHyphenIndex + 1);
                    strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
                }
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                if (intHyphenIndex != -1)
                {
                    decimal.TryParse(strFirstHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);
                    decimal.TryParse(strSecondHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax);
                }
                else
                    decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);

                string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                if (decMax == decimal.MaxValue)
                    strReturn = decMin.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + strNuyenSymbol + '+';
                else
                    strReturn = decMin.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + " - " + decMax.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + strNuyenSymbol;

                decItemCost = decMin;
                return new Tuple<string, decimal>(strReturn, decItemCost);
            }

            if (blnUseRating)
            {
                decimal decTotalCost = (await ProcessRatingStringAsDecAsync(strReturn, () => GetRatingAsync(token), token).ConfigureAwait(false)).Item1;

                decTotalCost *= 1.0m + decMarkup;

                if (DiscountCost)
                    decTotalCost *= 0.9m;

                decItemCost = decTotalCost;
                string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                strReturn = decTotalCost.ToString(strNuyenFormat, GlobalSettings.CultureInfo) + strNuyenSymbol;
                return new Tuple<string, decimal>(strReturn, decItemCost);
            }

            return new Tuple<string, decimal>(await strReturn.CheapReplaceAsync("Rating", () => LanguageManager.GetStringAsync(RatingLabel, token: token), token: token).ConfigureAwait(false) + strNuyenSymbol, decItemCost);
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail =>
            _objCachedSourceDetail == default
                ? _objCachedSourceDetail = SourceString.GetSourceString(Source,
                    DisplayPage(GlobalSettings.Language),
                    GlobalSettings.Language,
                    GlobalSettings.CultureInfo,
                    _objCharacter)
                : _objCachedSourceDetail;

        public async Task<SourceString> GetSourceDetailAsync(CancellationToken token = default)
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

        /// <summary>
        /// Armor's Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Guid of a Weapon created from the Armour.
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
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async Task<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            string strReturn = objNode?.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
        }

        /// <summary>
        /// Whether the Armor is equipped and should be considered for highest Armor Rating or Armor Encumbrance.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set
            {
                if (_blnEquipped == value)
                    return;
                _blnEquipped = value;
                if (value)
                {
                    // Add the Armor's Improvements to the character.
                    ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Armor && x.SourceName == InternalId));
                    // Add the Improvements from any Armor Mods in the Armor.
                    foreach (ArmorMod objMod in ArmorMods.AsEnumerableWithSideEffects())
                    {
                        if (objMod.Equipped)
                        {
                            ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId));
                            // Add the Improvements from any Gear in the Armor.
                            foreach (Gear objGear in objMod.GearChildren.AsEnumerableWithSideEffects())
                            {
                                if (objGear.Equipped)
                                {
                                    objGear.ChangeEquippedStatus(true, true);
                                }
                            }
                        }
                    }
                    // Add the Improvements from any Gear in the Armor.
                    foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                    {
                        if (objGear.Equipped)
                        {
                            objGear.ChangeEquippedStatus(true, true);
                        }
                    }
                }
                else
                {
                    // Add the Armor's Improvements to the character.
                    ImprovementManager.DisableImprovements(_objCharacter,
                                                           _objCharacter.Improvements.Where(
                                                               x => x.ImproveSource
                                                                    == Improvement.ImprovementSource.Armor
                                                                    && x.SourceName == InternalId));
                    // Add the Improvements from any Armor Mods in the Armor.
                    foreach (ArmorMod objMod in ArmorMods.AsEnumerableWithSideEffects())
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                                                               _objCharacter.Improvements.Where(
                                                                   x => x.ImproveSource
                                                                        == Improvement.ImprovementSource.ArmorMod
                                                                        && x.SourceName == InternalId));
                        // Add the Improvements from any Gear in the Armor.
                        foreach (Gear objGear in objMod.GearChildren.AsEnumerableWithSideEffects())
                        {
                            objGear.ChangeEquippedStatus(false, true);
                        }
                    }
                    // Add the Improvements from any Gear in the Armor.
                    foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                    {
                        objGear.ChangeEquippedStatus(false, true);
                    }
                }

                if (!_objCharacter.IsLoading)
                    _objCharacter.OnMultiplePropertyChanged(nameof(Character.ArmorEncumbrance),
                                                            nameof(Character.TotalCarriedWeight),
                                                            nameof(Character.TotalArmorRating));
            }
        }

        /// <summary>
        /// Whether the Armor is equipped and should be considered for highest Armor Rating or Armor Encumbrance.
        /// </summary>
        public async Task SetEquippedAsync(bool value, CancellationToken token = default)
        {
            if (_blnEquipped == value)
                return;
            _blnEquipped = value;
            if (value)
            {
                // Add the Armor's Improvements to the character.
                await ImprovementManager.EnableImprovementsAsync(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Armor && x.SourceName == InternalId), token).ConfigureAwait(false);
                // Add the Improvements from any Armor Mods in the Armor.
                await ArmorMods.ForEachWithSideEffectsAsync(async objMod =>
                {
                    if (objMod.Equipped)
                    {
                        await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                            _objCharacter.Improvements.Where(x =>
                                x.ImproveSource == Improvement.ImprovementSource.ArmorMod &&
                                x.SourceName == InternalId), token).ConfigureAwait(false);
                        // Add the Improvements from any Gear in the Armor.
                        await objMod.GearChildren.ForEachWithSideEffectsAsync(async objGear =>
                        {
                            if (objGear.Equipped)
                            {
                                await objGear.ChangeEquippedStatusAsync(true, true, token).ConfigureAwait(false);
                            }
                        }, token).ConfigureAwait(false);
                    }
                }, token).ConfigureAwait(false);
                // Add the Improvements from any Gear in the Armor.
                await GearChildren.ForEachWithSideEffectsAsync(objGear => objGear.ChangeEquippedStatusAsync(true, true, token), token).ConfigureAwait(false);
            }
            else
            {
                // Add the Armor's Improvements to the character.
                await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                    _objCharacter.Improvements.Where(
                        x => x.ImproveSource
                             == Improvement.ImprovementSource.Armor
                             && x.SourceName == InternalId), token).ConfigureAwait(false);
                // Add the Improvements from any Armor Mods in the Armor.
                await ArmorMods.ForEachWithSideEffectsAsync(async objMod =>
                {
                    await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                        _objCharacter.Improvements.Where(
                            x => x.ImproveSource
                                 == Improvement.ImprovementSource.ArmorMod
                                 && x.SourceName == InternalId), token).ConfigureAwait(false);
                    // Add the Improvements from any Gear in the Armor.
                    await objMod.GearChildren.ForEachWithSideEffectsAsync(
                        objGear => objGear.ChangeEquippedStatusAsync(false, true, token), token).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
                // Add the Improvements from any Gear in the Armor.
                await GearChildren.ForEachWithSideEffectsAsync(objGear => objGear.ChangeEquippedStatusAsync(false, true, token), token).ConfigureAwait(false);
            }

            if (_objCharacter?.IsLoading == false)
                await _objCharacter.OnMultiplePropertyChangedAsync(token, nameof(Character.ArmorEncumbrance),
                    nameof(Character.TotalCarriedWeight),
                    nameof(Character.TotalArmorRating)).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether Wireless is turned on for this armor
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
        /// The Armor's armor value excluding modifications
        /// </summary>
        private int GetOwnArmorValue(bool blnUseOverrideValue = false)
        {
            return ProcessRatingString(blnUseOverrideValue ? ArmorOverrideValue : ArmorValue, () => Rating);
        }

        /// <summary>
        /// The Armor's armor value excluding modifications
        /// </summary>
        private Task<int> GetOwnArmorValueAsync(bool blnUseOverrideValue = false, CancellationToken token = default)
        {
            return ProcessRatingStringAsync(blnUseOverrideValue ? ArmorOverrideValue : ArmorValue, () => GetRatingAsync(token), token);
        }

        /// <summary>
        /// The Armor's armor value excluding modifications
        /// </summary>
        public int OwnArmor => GetOwnArmorValue();

        /// <summary>
        /// The Armor's armor value excluding modifications
        /// </summary>
        public Task<int> GetOwnArmorAsync(CancellationToken token = default) => GetOwnArmorValueAsync(token: token);

        /// <summary>
        /// The Armor's total Armor value including Modifications.
        /// </summary>
        public int GetTotalArmor(bool blnForEncumbrance = false)
        {
            if (blnForEncumbrance && !ArmorValue.StartsWith('+') && !ArmorValue.StartsWith('-'))
                return 0;
            // Go through all of the Mods for this piece of Armor and add the Armor value.
            int intTotalArmor = OwnArmor + (blnForEncumbrance ? ArmorMods.Sum(o => o.Equipped && o.Encumbrance, o => o.Armor) : ArmorMods.Sum(o => o.Equipped, o => o.Armor));
            if (_objCharacter?.Settings.ArmorDegradation == true)
                intTotalArmor -= ArmorDamage;

            return Math.Max(intTotalArmor, 0);
        }

        /// <summary>
        /// The Armor's total Armor value including Modifications.
        /// </summary>
        public async Task<int> GetTotalArmorAsync(bool blnForEncumbrance = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnForEncumbrance && !ArmorValue.StartsWith('+') && !ArmorValue.StartsWith('-'))
                return 0;
            // Go through all of the Mods for this piece of Armor and add the Armor value.
            int intTotalArmor = await GetOwnArmorAsync(token).ConfigureAwait(false) + (blnForEncumbrance
                ? await ArmorMods.SumAsync(o => o.Equipped && o.Encumbrance, o => o.Armor, token: token).ConfigureAwait(false)
                : await ArmorMods.SumAsync(o => o.Equipped, o => o.Armor, token: token).ConfigureAwait(false));
            if (_objCharacter != null && await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetArmorDegradationAsync(token).ConfigureAwait(false))
                intTotalArmor -= ArmorDamage;

            return Math.Max(intTotalArmor, 0);
        }

        /// <summary>
        /// The Armor's bonus armor value excluding modifications
        /// </summary>
        public int OwnOverrideArmor => GetOwnArmorValue(true);

        /// <summary>
        /// The Armor's bonus armor value excluding modifications
        /// </summary>
        public Task<int> GetOwnOverrideArmorAsync(CancellationToken token = default) => GetOwnArmorValueAsync(true, token);

        /// <summary>
        /// The Armor's total bonus Armor value including Modifications.
        /// </summary>
        public int GetTotalOverrideArmor(bool blnForEncumbrance = false)
        {
            if (blnForEncumbrance && !ArmorOverrideValue.StartsWith('+') && !ArmorOverrideValue.StartsWith('-'))
                return 0;
            // Go through all of the Mods for this piece of Armor and add the Armor value.
            int intTotalArmor = OwnOverrideArmor + (blnForEncumbrance ? ArmorMods.Sum(o => o.Equipped && o.Encumbrance, o => o.Armor) : ArmorMods.Sum(o => o.Equipped, o => o.Armor));
            if (_objCharacter?.Settings.ArmorDegradation == true)
                intTotalArmor -= ArmorDamage;

            return Math.Max(intTotalArmor, 0);
        }

        /// <summary>
        /// The Armor's total bonus Armor value including Modifications.
        /// </summary>
        public async Task<int> GetTotalOverrideArmorAsync(bool blnForEncumbrance = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnForEncumbrance && !ArmorOverrideValue.StartsWith('+') && !ArmorOverrideValue.StartsWith('-'))
                return 0;
            // Go through all of the Mods for this piece of Armor and add the Armor value.
            int intTotalArmor = await GetOwnOverrideArmorAsync(token).ConfigureAwait(false) + (blnForEncumbrance
                ? await ArmorMods.SumAsync(o => o.Equipped && o.Encumbrance, o => o.Armor, token: token).ConfigureAwait(false)
                : await ArmorMods.SumAsync(o => o.Equipped, o => o.Armor, token: token).ConfigureAwait(false));
            if (_objCharacter != null && await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetArmorDegradationAsync(token).ConfigureAwait(false))
                intTotalArmor -= ArmorDamage;

            return Math.Max(intTotalArmor, 0);
        }

        public string DisplayArmorValue
        {
            get
            {
                string strArmorOverrideValue = ArmorOverrideValue;
                int intArmor = GetTotalArmor();
                if (!string.IsNullOrWhiteSpace(strArmorOverrideValue))
                {
                    return intArmor.ToString(GlobalSettings.CultureInfo) + '/' + strArmorOverrideValue;
                }

                string strArmor = ArmorValue;
                char chrFirstArmorChar = strArmor.Length > 0 ? strArmor[0] : ' ';
                if (chrFirstArmorChar == '+' || chrFirstArmorChar == '-')
                {
                    return intArmor.ToString("+0;-0;0", GlobalSettings.CultureInfo);
                }
                return intArmor.ToString(GlobalSettings.CultureInfo);
            }
        }

        public async Task<string> GetDisplayArmorValueAsync(CancellationToken token = default)
        {
            string strArmorOverrideValue = ArmorOverrideValue;
            int intArmor = await GetTotalArmorAsync(token: token).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(strArmorOverrideValue))
            {
                return intArmor.ToString(GlobalSettings.CultureInfo) + '/' + strArmorOverrideValue;
            }

            string strArmor = ArmorValue;
            char chrFirstArmorChar = strArmor.Length > 0 ? strArmor[0] : ' ';
            if (chrFirstArmorChar == '+' || chrFirstArmorChar == '-')
            {
                return intArmor.ToString("+0;-0;0", GlobalSettings.CultureInfo);
            }
            return intArmor.ToString(GlobalSettings.CultureInfo);
        }

        public decimal StolenTotalCost => CalculatedStolenTotalCost(true);

        public decimal NonStolenTotalCost => CalculatedStolenTotalCost(false);

        public decimal CalculatedStolenTotalCost(bool blnStolen)
        {
            decimal decTotalCost = 0;
            if (Stolen == blnStolen)
                decTotalCost += OwnCost;

            // Go through all of the Mods for this piece of Armor and add the Cost value.
            decTotalCost += ArmorMods.Sum(mod => mod.CalculatedStolenTotalCost(blnStolen));

            // Go through all of the Gear for this piece of Armor and add the Cost value.
            decTotalCost += GearChildren.Sum(g => g.CalculatedStolenTotalCost(blnStolen));

            return decTotalCost;
        }

        public Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(true, token);

        public Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(false, token);

        public async Task<decimal> CalculatedStolenTotalCostAsync(bool blnStolen, CancellationToken token = default)
        {
            decimal decTotalCost = 0;
            if (Stolen == blnStolen)
                decTotalCost += await GetOwnCostAsync(token).ConfigureAwait(false);

            // Go through all of the Mods for this piece of Armor and add the Cost value.
            decTotalCost += await ArmorMods.SumAsync(mod => mod.CalculatedStolenTotalCostAsync(blnStolen, token), token).ConfigureAwait(false);

            // Go through all of the Gear for this piece of Armor and add the Cost value.
            decTotalCost += await GearChildren.SumAsync(g => g.CalculatedStolenTotalCostAsync(blnStolen, token), token).ConfigureAwait(false);

            return decTotalCost;
        }

        /// <summary>
        /// The Armor's total Cost including Modifications.
        /// </summary>
        public decimal TotalCost => OwnCost + ArmorMods.Sum(x => x.TotalCost) + GearChildren.Sum(x => x.TotalCost);

        /// <summary>
        /// The Armor's total Cost including Modifications.
        /// </summary>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await GetOwnCostAsync(token).ConfigureAwait(false)
                   + await ArmorMods.SumAsync(x => x.GetTotalCostAsync(token), token).ConfigureAwait(false)
                   + await GearChildren.SumAsync(x => x.GetTotalCostAsync(token), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Cost multiplier for Gear Children attached to this Armor.
        /// </summary>
        public int ChildCostMultiplier => 1;

        /// <summary>
        /// Cost for just the Armor.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                decimal decReturn = ProcessRatingStringAsDec(Cost, () => Rating);
                if (DiscountCost)
                    decReturn *= 0.9m;
                return decReturn;
            }
        }

        /// <summary>
        /// Cost for just the Armor.
        /// </summary>
        public async Task<decimal> GetOwnCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = (await ProcessRatingStringAsDecAsync(Cost, () => GetRatingAsync(token), token)).Item1;
            if (DiscountCost)
                decReturn *= 0.9m;
            return decReturn;
        }

        /// <summary>
        /// The Armor's total Weight including Modifications.
        /// </summary>
        public decimal TotalWeight => OwnWeight + ArmorMods.Sum(x => x.Equipped, x => x.TotalWeight)
                                                + GearChildren.Sum(x => x.Equipped, x => x.TotalWeight);

        /// <summary>
        /// Weight for just the Armor.
        /// </summary>
        public decimal OwnWeight
        {
            get
            {
                return ProcessRatingStringAsDec(Weight, () => Rating);
            }
        }

        /// <summary>
        /// The Modifications currently applied to the Armor.
        /// </summary>
        public TaggedObservableCollection<ArmorMod> ArmorMods => _lstArmorMods;

        /// <summary>
        /// The Gear currently applied to the Armor.
        /// </summary>
        public TaggedObservableCollection<Gear> GearChildren => _lstGear;

        /// <summary>
        /// Location.
        /// </summary>
        public Location Location
        {
            get => _objLocation;
            set => _objLocation = value;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        public Task<string> GetNotesAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<string>(token);
            return Task.FromResult(_strNotes);
        }

        public Task SetNotesAsync(string value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _strNotes = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        public Task<Color> GetNotesColorAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<Color>(token);
            return Task.FromResult(_colNotes);
        }

        public Task SetNotesColorAsync(Color value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _colNotes = value;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Whether the Armor's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost;
            set => _blnDiscountCost = value;
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
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public TaggedObservableCollection<Gear> Children => GearChildren;

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        /// <summary>
        /// Device Rating string.
        /// </summary>
        public string DeviceRating
        {
            get => _strDeviceRating;
            set => _strDeviceRating = value;
        }

        /// <summary>
        /// Attack.
        /// </summary>
        public string Attack
        {
            get => _strAttack;
            set => _strAttack = value;
        }

        /// <summary>
        /// Sleaze.
        /// </summary>
        public string Sleaze
        {
            get => _strSleaze;
            set => _strSleaze = value;
        }

        /// <summary>
        /// Data Processing.
        /// </summary>
        public string DataProcessing
        {
            get => _strDataProcessing;
            set => _strDataProcessing = value;
        }

        /// <summary>
        /// Firewall.
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

        public IEnumerable<IHasMatrixAttributes> ChildrenWithMatrixAttributes => Children;

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get => _strProgramLimit;
            set => _strProgramLimit = value;
        }

        /// <inheritdoc />
        public string Overclocked
        {
            get => _objCharacter.Overclocker ? _strOverclocked : string.Empty;
            set => _strOverclocked = value;
        }

        /// <inheritdoc />
        public async Task<string> GetOverclockedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await _objCharacter.GetOverclockerAsync(token).ConfigureAwait(false) ? _strOverclocked : string.Empty;
        }

        /// <summary>
        /// Returns true if this is a cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get => _blnCanSwapAttributes;
            set => _blnCanSwapAttributes = value;
        }

        /// <inheritdoc />
        public bool IsProgram => false;

        /// <inheritdoc />
        public string CanFormPersona
        {
            get => _strCanFormPersona;
            set => _strCanFormPersona = value;
        }

        /// <inheritdoc />
        public Task<string> GetCanFormPersonaAsync(CancellationToken token = default) => token.IsCancellationRequested
            ? Task.FromCanceled<string>(token)
            : Task.FromResult(_strCanFormPersona);

        /// <inheritdoc />
        public bool IsCommlink => CanFormPersona.Contains("Self") || Children.Any(x => x.CanFormPersona.Contains("Parent"));

        /// <inheritdoc />
        public async Task<bool> GetIsCommlinkAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await GetCanFormPersonaAsync(token).ConfigureAwait(false)).Contains("Self") || await Children
                .AnyAsync(async x => (await x.GetCanFormPersonaAsync(token).ConfigureAwait(false)).Contains("Parent"),
                    token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Base Matrix Boxes.
        /// </summary>
        public int BaseMatrixBoxes => 8;

        /// <summary>
        /// Bonus Matrix Boxes.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get => _intMatrixCMBonus;
            set => _intMatrixCMBonus = value;
        }

        /// <summary>
        /// Total Bonus Matrix Boxes (including all children).
        /// </summary>
        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intReturn = BonusMatrixBoxes;
                intReturn += Children.Sum(g => g.Equipped, loopGear => loopGear.TotalBonusMatrixBoxes);
                return intReturn;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM => BaseMatrixBoxes + this.GetTotalMatrixAttribute("Device Rating").DivAwayFromZero(2)
                                               + TotalBonusMatrixBoxes;

        /// <summary>
        /// Matrix Condition Monitor boxes filled.
        /// </summary>
        public int MatrixCMFilled
        {
            get => _intMatrixCMFilled;
            set => _intMatrixCMFilled = value;
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
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
        }

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
            bool blnModifyParentAvail = false;
            string strAvail = Avail;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                strAvail = strAvail.ProcessFixedValuesString(() => Rating);

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                intAvail = ProcessRatingString(strAvail, () => Rating);
            }

            if (blnCheckChildren)
            {
                // Run through armor mod children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (ArmorMod objChild in ArmorMods)
                {
                    if (!objChild.IncludedInArmor)
                    {
                        AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }

                // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Gear objChild in GearChildren)
                {
                    if (objChild.ParentID != InternalId)
                    {
                        AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

            intAvail += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: SourceIDString, blnIncludeNonImproved: true).StandardRound();

            // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async Task<AvailabilityValue> TotalAvailTupleAsync(bool blnCheckChildren = true, CancellationToken token = default)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Avail;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                strAvail = await strAvail.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false);

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                intAvail = await ProcessRatingStringAsync(strAvail, () => GetRatingAsync(token), token).ConfigureAwait(false);
            }

            if (blnCheckChildren)
            {
                // Run through armor mod children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                intAvail += await ArmorMods.SumAsync(x => !x.IncludedInArmor, async objChild =>
                            {
                                AvailabilityValue objLoopAvailTuple = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                                if (objLoopAvailTuple.Suffix == 'F')
                                    chrLastAvailChar = 'F';
                                else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                                    chrLastAvailChar = 'R';
                                return objLoopAvailTuple.AddToParent ? await objLoopAvailTuple.GetValueAsync(token).ConfigureAwait(false) : 0;
                            }, token).ConfigureAwait(false)
                            // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                            + await GearChildren.SumAsync(x => x.ParentID != InternalId, async objChild =>
                            {
                                AvailabilityValue objLoopAvailTuple = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                                if (objLoopAvailTuple.Suffix == 'F')
                                    chrLastAvailChar = 'F';
                                else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                                    chrLastAvailChar = 'R';
                                return objLoopAvailTuple.AddToParent ? await objLoopAvailTuple.GetValueAsync(token).ConfigureAwait(false) : 0;
                            }, token).ConfigureAwait(false);
            }

            intAvail += (await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Availability, strImprovedName: SourceIDString, blnIncludeNonImproved: true, token: token).ConfigureAwait(false)).StandardRound();

            // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Calculated Capacity of the Armor.
        /// </summary>
        public string CalculatedCapacity(CultureInfo objCultureInfo)
        {
            string strReturn;
            if (ArmorMods.Any(x => x.ArmorCapacity.StartsWith('-') || x.ArmorCapacity.StartsWith("[-")))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                {
                    string strCapacity = TotalArmorCapacity(GlobalSettings.InvariantCultureInfo);
                    // If an Armor Capacity is specified for the Armor, use that value.
                    if (string.IsNullOrEmpty(strCapacity) || strCapacity == "0")
                        sbdReturn.Append("(0)");
                    else
                        sbdReturn.Append('(' + strCapacity + ')');

                    foreach (ArmorMod objMod in ArmorMods)
                    {
                        string strArmorModCapacity = objMod.ArmorCapacity;
                        if (!strArmorModCapacity.StartsWith('-') && !strArmorModCapacity.StartsWith("[-", StringComparison.Ordinal))
                            continue;
                        sbdReturn.Append("-(" + objMod.GetCalculatedCapacity(GlobalSettings.InvariantCultureInfo).Trim('[', ']') + ')');
                    }

                    strReturn = sbdReturn.ToString();
                }

                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strReturn);
                if (blnIsSuccess)
                    strReturn = Convert.ToDecimal((double)objProcess).ToString(objCultureInfo);
            }
            else
            {
                strReturn = TotalArmorCapacity(objCultureInfo);
                // If an Armor Capacity is specified for the Armor, use that value.
                if (string.IsNullOrEmpty(strReturn) || strReturn == "0")
                    strReturn = 0.0m.ToString("#,0.##", objCultureInfo);
                else if (decimal.TryParse(strReturn, NumberStyles.Any, objCultureInfo, out decimal decReturn))
                    strReturn = decReturn.ToString("#,0.##", objCultureInfo);
            }

            return strReturn;
        }

        /// <summary>
        /// Calculated Capacity of the Armor.
        /// </summary>
        public async Task<string> CalculatedCapacityAsync(CultureInfo objCultureInfo, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strReturn;
            if (await ArmorMods.AnyAsync(x => x.ArmorCapacity.StartsWith('-') || x.ArmorCapacity.StartsWith("[-"), token).ConfigureAwait(false))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                {
                    string strCapacity = await TotalArmorCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false);
                    // If an Armor Capacity is specified for the Armor, use that value.
                    if (string.IsNullOrEmpty(strCapacity) || strCapacity == "0")
                        sbdReturn.Append("(0)");
                    else
                        sbdReturn.Append('(' + strCapacity + ')');

                    await ArmorMods.ForEachAsync(async objMod =>
                    {
                        string strArmorModCapacity = objMod.ArmorCapacity;
                        if (!strArmorModCapacity.StartsWith('-') && !strArmorModCapacity.StartsWith("[-", StringComparison.Ordinal))
                            return;
                        sbdReturn.Append("-(" + (await objMod.GetCalculatedCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false)).Trim('[', ']') + ')');
                    }, token).ConfigureAwait(false);

                    strReturn = sbdReturn.ToString();
                }

                (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strReturn, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    strReturn = Convert.ToDecimal((double)objProcess).ToString(objCultureInfo);
            }
            else
            {
                strReturn = await TotalArmorCapacityAsync(objCultureInfo, token).ConfigureAwait(false);
                // If an Armor Capacity is specified for the Armor, use that value.
                if (string.IsNullOrEmpty(strReturn) || strReturn == "0")
                    strReturn = 0.0m.ToString("#,0.##", objCultureInfo);
                else if (decimal.TryParse(strReturn, NumberStyles.Any, objCultureInfo, out decimal decReturn))
                    strReturn = decReturn.ToString("#,0.##", objCultureInfo);
            }

            return strReturn;
        }

        public string CurrentCalculatedCapacity => CalculatedCapacity(GlobalSettings.CultureInfo);

        public Task<string> GetCurrentCalculatedCapacityAsync(CancellationToken token = default) => CalculatedCapacityAsync(GlobalSettings.CultureInfo, token);

        /// <summary>
        /// The amount of Capacity remaining in the Armor.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                // Get the Armor base Capacity.
                // If there is no Capacity (meaning that the Armor Suit Capacity or Maximum Armor Modification rule is turned off depending on the type of Armor), don't bother to calculate the remaining
                // Capacity since it's disabled and return 0 instead.
                if (!decimal.TryParse(CalculatedCapacity(GlobalSettings.InvariantCultureInfo), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decCapacity)
                    || decCapacity == 0)
                    return 0;

                // Calculate the remaining Capacity for a Suit of Armor.
                string strArmorCapacity = TotalArmorCapacity(GlobalSettings.InvariantCultureInfo);
                if (strArmorCapacity != "0" && !string.IsNullOrEmpty(strArmorCapacity)) // && _objCharacter.Settings.ArmorSuitCapacity)
                {
                    // Run through its Armor Mods and deduct the Capacity costs. Mods that confer capacity (ie negative values) are excluded, as they're processed in TotalArmorCapacity.
                    if (ArmorMods.Count > 0)
                        decCapacity -= ArmorMods.Sum(x => !x.IncludedInArmor, x => Math.Max(x.TotalCapacity, 0));
                    // Run through its Gear and deduct the Armor Capacity costs.
                    if (GearChildren.Count > 0)
                        decCapacity -= GearChildren.Sum(x => !x.IncludedInParent, x => x.PluginArmorCapacity * x.Quantity);
                }
                // Calculate the remaining Capacity for a standard piece of Armor using the Maximum Armor Modifications rules.
                else // if (_objCharacter.Settings.MaximumArmorModifications)
                {
                    // Run through its Armor Mods and deduct the Rating (or 1 if it has no Rating).
                    decCapacity -= ArmorMods.Sum(x => !x.IncludedInArmor, x => Math.Max(x.Rating, 1));

                    // Run through its Gear and deduct the Rating (or 1 if it has no Rating).
                    decCapacity -= GearChildren.Sum(x => !x.IncludedInParent, x => Math.Max(x.Rating, 1));
                }

                return decCapacity;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Armor.
        /// </summary>
        public async Task<decimal> GetCapacityRemainingAsync(CancellationToken token = default)
        {
            // Get the Armor base Capacity.
            // Get the Armor base Capacity.
            // If there is no Capacity (meaning that the Armor Suit Capacity or Maximum Armor Modification rule is turned off depending on the type of Armor), don't bother to calculate the remaining
            // Capacity since it's disabled and return 0 instead.
            if (!decimal.TryParse(await CalculatedCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decCapacity)
                || decCapacity == 0)
                return 0;

            // Calculate the remaining Capacity for a Suit of Armor.
            string strArmorCapacity = await TotalArmorCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false);
            if (strArmorCapacity != "0" && !string.IsNullOrEmpty(strArmorCapacity)) // && _objCharacter.Settings.ArmorSuitCapacity)
            {
                // Run through its Armor Mods and deduct the Capacity costs. Mods that confer capacity (ie negative values) are excluded, as they're processed in TotalArmorCapacity.
                if (await ArmorMods.GetCountAsync(token).ConfigureAwait(false) > 0)
                    decCapacity -= await ArmorMods.SumAsync(x => !x.IncludedInArmor, async x => Math.Max(await x.GetTotalCapacityAsync(token).ConfigureAwait(false), 0), token: token).ConfigureAwait(false);
                // Run through its Gear and deduct the Armor Capacity costs.
                if (await GearChildren.GetCountAsync(token).ConfigureAwait(false) > 0)
                    decCapacity -= await GearChildren.SumAsync(x => x.ParentID != InternalId, async x => await x.GetPluginArmorCapacityAsync(token).ConfigureAwait(false) * x.Quantity, token: token).ConfigureAwait(false);
            }
            // Calculate the remaining Capacity for a standard piece of Armor using the Maximum Armor Modifications rules.
            else // if (_objCharacter.Settings.MaximumArmorModifications)
            {
                // Run through its Armor Mods and deduct the Rating (or 1 if it has no Rating).
                decCapacity -= await ArmorMods.SumAsync(x => !x.IncludedInArmor, async x => Math.Max(await x.GetRatingAsync(token).ConfigureAwait(false), 1), token: token).ConfigureAwait(false);

                // Run through its Gear and deduct the Rating (or 1 if it has no Rating).
                decCapacity -= await GearChildren.SumAsync(x => x.ParentID != InternalId, async x => Math.Max(await x.GetRatingAsync(token).ConfigureAwait(false), 1), token: token).ConfigureAwait(false);
            }

            return decCapacity;
        }

        public string DisplayCapacity
        {
            get
            {
                string strCalculatedCapacity = CurrentCalculatedCapacity;
                if (strCalculatedCapacity.Contains('[') && !strCalculatedCapacity.Contains("/["))
                    return strCalculatedCapacity;
                return string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_CapacityRemaining"),
                                     strCalculatedCapacity, CapacityRemaining.ToString("#,0.##", GlobalSettings.CultureInfo));
            }
        }

        public async Task<string> GetDisplayCapacityAsync(CancellationToken token = default)
        {
            string strCalculatedCapacity = await GetCurrentCalculatedCapacityAsync(token).ConfigureAwait(false);
            if (strCalculatedCapacity.Contains('[') && !strCalculatedCapacity.Contains("/["))
                return strCalculatedCapacity;
            return string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_CapacityRemaining", token: token).ConfigureAwait(false),
                strCalculatedCapacity, (await GetCapacityRemainingAsync(token).ConfigureAwait(false)).ToString("#,0.##", GlobalSettings.CultureInfo));
        }

        /// <summary>
        /// Capacity display style.
        /// </summary>
        public CapacityStyle CapacityDisplayStyle
        {
            get
            {
                string strArmorCapacity = ArmorCapacity;
                if (!string.IsNullOrEmpty(strArmorCapacity) && strArmorCapacity != "0")
                {
                    return CapacityStyle.Standard;
                }

                return CapacityStyle.Zero;
            }
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return this.GetNodeXPath(strLanguage, token)?.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            return objNode != null ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name : Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strReturn = DisplayNameShort(strLanguage, token);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage, token: token);
            if (!string.IsNullOrEmpty(CustomName))
                strReturn += strSpace + "(\"" + CustomName + "\")";
            int intRating = Rating;
            if (intRating > 0)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                strReturn += strSpace + '(' + LanguageManager.GetString(RatingLabel, strLanguage, token: token) + strSpace + intRating.ToString(objCulture) + ')';
            }
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + '(' + _objCharacter.TranslateExtra(Extra, strLanguage, token: token) + ')';
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(CustomName))
                strReturn += strSpace + "(\"" + CustomName + "\")";
            int intRating = await GetRatingAsync(token).ConfigureAwait(false);
            if (intRating > 0)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                strReturn += strSpace + '(' + await LanguageManager.GetStringAsync(RatingLabel, strLanguage, token: token).ConfigureAwait(false) + strSpace + intRating.ToString(objCulture) + ')';
            }
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + '(' + await _objCharacter.TranslateExtraAsync(Extra, strLanguage, token: token).ConfigureAwait(false) + ')';
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// A custom name for the Armor assigned by the player.
        /// </summary>
        public string CustomName
        {
            get => _strArmorName;
            set => _strArmorName = value;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("armor.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("armor.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/armors/armor", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/armors/armor", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            _objCachedMyXmlNode = objReturn;
            _strCachedXmlNodeLanguage = strLanguage;
            return objReturn;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XPathNavigator objReturn = _objCachedMyXPathNode;
            if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath("armor.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("armor.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/armors/armor", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/armors/armor", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExpression = "2";
                        break;

                    case "Program Limit":
                        if (IsCommlink)
                        {
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                strExpression = "2";
                        }
                        else
                            strExpression = "0";
                        break;

                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            strExpression = "2";
                        break;

                    default:
                        strExpression = "0";
                        break;
                }
            }

            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                        {
                            sbdValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + '}', () => "0");
                            sbdValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + '}', () => "0");
                            if (Children.Count > 0 && strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                            {
                                int intTotalChildrenValue = Children.Sum(x => x.Equipped, x => x.GetBaseMatrixAttribute(strMatrixAttribute));
                                sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                                 intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                            }
                        }

                        _objCharacter.ProcessAttributesInXPath(sbdValue, strExpression);
                        strExpression = sbdValue.ToString();
                    }
                }
                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                (bool blnIsSuccess, object objProcess)
                    = CommonFunctions.EvaluateInvariantXPath(strExpression);
                return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
            }
            return decValue.StandardRound();
        }

        public async Task<int> GetBaseMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExpression = "2";
                        break;

                    case "Program Limit":
                        if (await GetIsCommlinkAsync(token).ConfigureAwait(false))
                        {
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                strExpression = "2";
                        }
                        else
                            strExpression = "0";

                        break;

                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            strExpression = "2";
                        break;

                    default:
                        strExpression = "0";
                        break;
                }
            }

            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                        {
                            await sbdValue
                                .CheapReplaceAsync(strExpression, "{Gear " + strMatrixAttribute + '}', () => "0",
                                    token: token).ConfigureAwait(false);
                            await sbdValue.CheapReplaceAsync(strExpression, "{Parent " + strMatrixAttribute + '}',
                                () => "0", token: token).ConfigureAwait(false);
                            if (await Children.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                                strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                            {
                                int intTotalChildrenValue = await Children.SumAsync(x => x.Equipped,
                                        x => x.GetBaseMatrixAttributeAsync(strAttributeName, token), token)
                                    .ConfigureAwait(false);

                                sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                    intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                            }
                        }

                        await _objCharacter
                            .ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                        strExpression = sbdValue.ToString();
                    }
                }
                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(strExpression, token)
                        .ConfigureAwait(false);
                return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
            }
            return decValue.StandardRound();
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = Overclocked == strAttributeName ? 1 : 0;

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            intReturn += Children.Sum(x => x.Equipped && x.ParentID != InternalId, x => x.GetTotalMatrixAttribute(strAttributeName));

            return intReturn;
        }

        /// <summary>
        /// Get the bonus value of a Matrix attribute of this gear from children and Overclocker
        /// </summary>
        public async Task<int> GetBonusMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = await GetOverclockedAsync(token).ConfigureAwait(false) == strAttributeName ? 1 : 0;

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            intReturn += await Children.SumAsync(x => x.Equipped, x => x.GetTotalMatrixAttributeAsync(strAttributeName, token), token).ConfigureAwait(false);

            return intReturn;
        }

        #endregion Complex Properties

        #region Methods

        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteArmor()
        {
            _objCharacter.Armor.Remove(this);

            // Remove any Improvements created by the Armor and its children.
            decimal decReturn = ArmorMods.AsEnumerableWithSideEffects().Sum(x => x.DeleteArmorMod(false))
                                + GearChildren.AsEnumerableWithSideEffects().Sum(x => x.DeleteGear(false));

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Armor, InternalId);

            // Remove the Cyberweapon created by the Mod if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                List<Weapon> lstWeapons = _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList();
                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    lstWeapons.AddRange(objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId));
                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        lstWeapons.AddRange(objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId));
                    }
                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        lstWeapons.AddRange(objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId));
                        foreach (VehicleMod objMod in objMount.Mods)
                        {
                            lstWeapons.AddRange(objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId));
                        }
                    }
                }

                decReturn += lstWeapons.Sum(objDeleteWeapon => objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon());
            }

            DisposeSelf();

            return decReturn;
        }

        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        public async Task<decimal> DeleteArmorAsync(CancellationToken token = default)
        {
            await _objCharacter.Armor.RemoveAsync(this, token).ConfigureAwait(false);

            // Remove any Improvements created by the Armor and its children.
            decimal decReturn = await ArmorMods.SumWithSideEffectsAsync(x => x.DeleteArmorModAsync(false, token), token)
                                               .ConfigureAwait(false)
                                + await GearChildren.SumWithSideEffectsAsync(x => x.DeleteGearAsync(false, token), token)
                                                    .ConfigureAwait(false);

            decReturn += await ImprovementManager
                               .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Armor, InternalId,
                                                        token).ConfigureAwait(false);

            // Remove the Cyberweapon created by the Mod if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                List<Weapon> lstWeapons = await _objCharacter.Weapons
                    .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token).ConfigureAwait(false);
                await _objCharacter.Vehicles.ForEachAsync(async objVehicle =>
                {
                    lstWeapons.AddRange(await objVehicle.Weapons
                        .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token)
                        .ConfigureAwait(false));
                    await objVehicle.Mods.ForEachAsync(async objMod =>
                    {
                        lstWeapons.AddRange(await objMod.Weapons
                            .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token)
                            .ConfigureAwait(false));
                    }, token).ConfigureAwait(false);

                    await objVehicle.WeaponMounts.ForEachAsync(async objMount =>
                    {
                        lstWeapons.AddRange(await objMount.Weapons
                            .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token)
                            .ConfigureAwait(false));
                        await objMount.Mods.ForEachAsync(async objMod =>
                        {
                            lstWeapons.AddRange(await objMod.Weapons
                                .DeepWhereAsync(x => x.Children, x => x.ParentID == InternalId, token)
                                .ConfigureAwait(false));
                        }, token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                }, token).ConfigureAwait(false);

                decReturn += await lstWeapons.SumAsync(async objDeleteWeapon =>
                        await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                        + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
            }

            await DisposeSelfAsync().ConfigureAwait(false);

            return decReturn;
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this armor.
        /// </summary>
        public void RefreshWirelessBonuses()
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped)
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                                                               _objCharacter.Improvements.Where(x =>
                                                                   x.ImproveSource == Improvement.ImprovementSource
                                                                       .Armor &&
                                                                   x.SourceName == InternalId));
                    }

                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, InternalId + "Wireless", WirelessBonus, Rating, CurrentDisplayNameShort);

                    string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = strSelectedValue;
                }
                else
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                    {
                        ImprovementManager.EnableImprovements(_objCharacter,
                                                              _objCharacter.Improvements.Where(x =>
                                                                  x.ImproveSource == Improvement.ImprovementSource
                                                                      .Armor &&
                                                                  x.SourceName == InternalId));
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    ImprovementManager.RemoveImprovements(_objCharacter,
                                                          _objCharacter.Improvements.Where(x =>
                                                              x.ImproveSource == Improvement.ImprovementSource.Armor
                                                              &&
                                                              x.SourceName == strSourceNameToRemove).ToList());
                }
            }

            foreach (ArmorMod objArmorMod in ArmorMods.AsEnumerableWithSideEffects())
                objArmorMod.RefreshWirelessBonuses();
            foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                objGear.RefreshWirelessBonuses();
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this armor.
        /// </summary>
        public async Task RefreshWirelessBonusesAsync(CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped)
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
                    {
                        await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                                                          await _objCharacter.Improvements.ToListAsync(
                                                                              x => x.ImproveSource
                                                                                  == Improvement.ImprovementSource.Armor
                                                                                  && x.SourceName == InternalId, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Armor,
                                                                     InternalId + "Wireless", WirelessBonus, await GetRatingAsync(token).ConfigureAwait(false),
                                                                     await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                                                     token: token).ConfigureAwait(false);

                    string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
                    if (!string.IsNullOrEmpty(strSelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = strSelectedValue;
                }
                else
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
                    {
                        await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                                                                         await _objCharacter.Improvements.ToListAsync(
                                                                             x => x.ImproveSource
                                                                                 == Improvement.ImprovementSource.Armor
                                                                                 && x.SourceName == InternalId, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                                                     await _objCharacter.Improvements.ToListAsync(
                                                                         x => x.ImproveSource
                                                                              == Improvement.ImprovementSource.Armor
                                                                              && x.SourceName == strSourceNameToRemove,
                                                                         token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
            }

            await ArmorMods.ForEachWithSideEffectsAsync(x => x.RefreshWirelessBonusesAsync(token), token: token).ConfigureAwait(false);
            await GearChildren.ForEachWithSideEffectsAsync(x => x.RefreshWirelessBonusesAsync(token), token: token).ConfigureAwait(false);
        }

        #region UI Methods

        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                Tag = this,
                ContextMenuStrip = cmsArmor,
                ForeColor = await GetPreferredColorAsync(token).ConfigureAwait(false),
                ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            await ArmorMods.ForEachAsync(async objMod =>
            {
                TreeNode objLoopNode = await objMod.CreateTreeNode(cmsArmorMod, cmsArmorGear, token).ConfigureAwait(false);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }, token).ConfigureAwait(false);
            await GearChildren.ForEachAsync(async objGear =>
            {
                TreeNode objLoopNode = await objGear.CreateTreeNode(cmsArmorGear, null, token).ConfigureAwait(false);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }, token).ConfigureAwait(false);
            if (lstChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }

        public Color PreferredColor =>
            !string.IsNullOrEmpty(Notes)
                ? ColorManager.GenerateCurrentModeColor(NotesColor)
                : ColorManager.WindowText;

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return !string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false))
                ? ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                : ColorManager.WindowText;
        }

        #endregion UI Methods

        #endregion Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmor")))
                return false;
            DeleteArmor();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteArmor", token: token)
                            .ConfigureAwait(false), token).ConfigureAwait(false))
                return false;

            await DeleteArmorAsync(token: token).ConfigureAwait(false);
            return true;
        }

        public bool Sell(decimal decPercentage, bool blnConfirmDelete)
        {
            if (!_objCharacter.Created)
                return Remove(blnConfirmDelete);

            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmor")))
                return false;

            // Create the Expense Log Entry for the sale.
            decimal decAmount = TotalCost * decPercentage;
            decAmount += DeleteArmor() * decPercentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldArmor") + ' ' + CurrentDisplayNameShort, ExpenseType.Nuyen, DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen += decAmount;
            return true;
        }

        public async Task<bool> SellAsync(decimal decPercentage, bool blnConfirmDelete,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!await _objCharacter.GetCreatedAsync(token).ConfigureAwait(false))
                return await RemoveAsync(blnConfirmDelete, token).ConfigureAwait(false);

            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteArmor", token: token).ConfigureAwait(false),
                        token).ConfigureAwait(false))
                return false;

            // Create the Expense Log Entry for the sale.
            decimal decAmount = await GetTotalCostAsync(token).ConfigureAwait(false) * decPercentage;
            decAmount += await DeleteArmorAsync(token).ConfigureAwait(false) * decPercentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount,
                await LanguageManager.GetStringAsync("String_ExpenseSoldArmor", token: token).ConfigureAwait(false) +
                ' ' + await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), ExpenseType.Nuyen,
                DateTime.Now);
            await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
            await _objCharacter.ModifyNuyenAsync(decAmount, token).ConfigureAwait(false);
            return true;
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

        public async Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            await (await GetSourceDetailAsync(token).ConfigureAwait(false)).SetControlAsync(sourceControl, token).ConfigureAwait(false);
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
            AvailabilityValue objTotalAvail = await TotalAvailTupleAsync(token: token).ConfigureAwait(false);
            int intAvailInt = await objTotalAvail.GetValueAsync(token).ConfigureAwait(false);
            int intRestrictedCount = 0;
            if (intAvailInt > await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMaximumAvailabilityAsync(token).ConfigureAwait(false))
            {
                int intLowestValidRestrictedGearAvail = -1;
                foreach (int intValidAvail in dicRestrictedGearLimits.Keys)
                {
                    if (intValidAvail >= intAvailInt && (intLowestValidRestrictedGearAvail < 0
                                                         || intValidAvail < intLowestValidRestrictedGearAvail))
                        intLowestValidRestrictedGearAvail = intValidAvail;
                }

                if (intLowestValidRestrictedGearAvail >= 0 && dicRestrictedGearLimits[intLowestValidRestrictedGearAvail] > 0)
                {
                    --dicRestrictedGearLimits[intLowestValidRestrictedGearAvail];
                    sbdRestrictedItems.AppendLine().Append("\t\t").Append(CurrentDisplayName);
                }
                else
                {
                    dicRestrictedGearLimits.Remove(intLowestValidRestrictedGearAvail);
                    ++intRestrictedCount;
                    sbdAvailItems.AppendLine().Append("\t\t").Append(CurrentDisplayName);
                }
            }

            intRestrictedCount += await Children
                                        .SumAsync(objChild =>
                                                objChild
                                                    .CheckRestrictedGear(
                                                        dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                        token), token: token)
                                        .ConfigureAwait(false)
                                  + await ArmorMods
                                          .SumAsync(objChild =>
                                                  objChild
                                                      .CheckRestrictedGear(
                                                          dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                          token), token: token)
                                          .ConfigureAwait(false);
            return intRestrictedCount;
        }

        public async Task<bool> AllowPasteXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCapacity = await CalculatedCapacityAsync(GlobalSettings.InvariantCultureInfo, token)
                .ConfigureAwait(false);
            if (string.IsNullOrEmpty(strCapacity) || strCapacity == "0")
                return false;
            IAsyncDisposable objLocker = await GlobalSettings.EnterClipboardReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                string strPasteCategory = (await GlobalSettings.GetClipboardAsync(token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpressionAsNavigator("category", token)?.Value ?? string.Empty;
                switch (await GlobalSettings.GetClipboardContentTypeAsync(token).ConfigureAwait(false))
                {
                    case ClipboardContentType.ArmorMod:
                    {
                        XPathNavigator xmlNode = await this.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                        if (xmlNode == null)
                            return strPasteCategory == "General";
                        XPathNavigator xmlForceModCategory =
                            xmlNode.SelectSingleNodeAndCacheExpression("forcemodcategory", token);
                        if (xmlForceModCategory != null)
                            return xmlForceModCategory.Value == strPasteCategory;
                        if (strPasteCategory == "General")
                            return true;
                        XPathNodeIterator xmlAddonCategoryList = xmlNode.SelectAndCacheExpression("addoncategory", token);
                        return xmlAddonCategoryList.Count <= 0 || xmlAddonCategoryList.Cast<XPathNavigator>()
                            .Any(xmlCategory => xmlCategory.Value == strPasteCategory);
                    }
                    case ClipboardContentType.Gear:
                    {
                        XPathNavigator xmlNode = await this.GetNodeXPathAsync(token: token).ConfigureAwait(false);
                        if (xmlNode == null)
                            return false;
                        XPathNodeIterator xmlAddonCategoryList = xmlNode.SelectAndCacheExpression("addoncategory", token);
                        return xmlAddonCategoryList.Count <= 0 || xmlAddonCategoryList.Cast<XPathNavigator>()
                            .Any(xmlCategory => xmlCategory.Value == strPasteCategory);
                    }
                    default:
                        return false;
                }
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public Task<bool> AllowPasteObject(object input, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _lstArmorMods.EnumerateWithSideEffects().ForEach(x => x.Dispose());
            _lstGear.EnumerateWithSideEffects().ForEach(x => x.Dispose());
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstArmorMods.Dispose();
            _lstGear.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _lstArmorMods.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
            await _lstGear.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private async ValueTask DisposeSelfAsync()
        {
            await _lstArmorMods.DisposeAsync().ConfigureAwait(false);
            await _lstGear.DisposeAsync().ConfigureAwait(false);
        }

        public Character CharacterObject => _objCharacter;
    }
}
