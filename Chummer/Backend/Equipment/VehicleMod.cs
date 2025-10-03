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
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Vehicle Modification.
    /// </summary>
    [DebuggerDisplay("{DisplayName(null, \"en-us\")}")]
    public sealed class VehicleMod : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, ICanEquip, IHasSource, IHasRating, ICanSort, IHasStolenProperty, ICanPaste, ICanSell, ICanBlackMarketDiscount, IDisposable, IAsyncDisposable, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strLimit = string.Empty;
        private string _strSlots = "0";
        private int _intRating;
        private string _strRatingLabel = "String_Rating";
        private string _strMaxRating = string.Empty;
        private string _strCost = string.Empty;
        private string _strAvail = string.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = true;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludeInVehicle;
        private bool _blnEquipped = true;
        private int _intConditionMonitor;
        private readonly TaggedObservableCollection<Weapon> _lstVehicleWeapons;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private string _strSubsystems = string.Empty;
        private readonly TaggedObservableCollection<Cyberware> _lstCyberware;
        private string _strExtra = string.Empty;
        private string _strWeaponMountCategories = string.Empty;
        private bool _blnDiscountCost;
        private bool _blnDowngrade;
        private string _strCapacity = string.Empty;

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private string _strAmmoReplace;
        private decimal _decAmmoBonus;
        private decimal _decAmmoBonusPercent;
        private int _intSortOrder;
        private bool _blnStolen;
        private readonly Character _objCharacter;
        private Vehicle _objParent;
        private WeaponMount _objWeaponMountParent;

        #region Constructor, Create, Save, Load, and Print Methods

        public VehicleMod(Character objCharacter)
        {
            // Create the GUID for the new VehicleMod.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            _lstVehicleWeapons = new TaggedObservableCollection<Weapon>(objCharacter.LockObject);
            _lstVehicleWeapons.AddTaggedCollectionChanged(this, ChildrenWeaponsOnCollectionChanged);
            _lstCyberware = new TaggedObservableCollection<Cyberware>(objCharacter.LockObject);
            _lstCyberware.AddTaggedCollectionChanged(this, ChildrenCyberwareOnCollectionChanged);
        }

        private async Task ChildrenCyberwareOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Cyberware objNewItem in e.NewItems)
                        await objNewItem.SetParentVehicleAsync(Parent, token).ConfigureAwait(false);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Cyberware objOldItem in e.OldItems)
                        await objOldItem.SetParentVehicleAsync(null, token).ConfigureAwait(false);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (Cyberware objOldItem in e.OldItems)
                        await objOldItem.SetParentVehicleAsync(null, token).ConfigureAwait(false);
                    foreach (Cyberware objNewItem in e.NewItems)
                        await objNewItem.SetParentVehicleAsync(Parent, token).ConfigureAwait(false);
                    break;
            }
        }

        private async Task ChildrenWeaponsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Weapon objNewItem in e.NewItems)
                        await objNewItem.SetParentVehicleAsync(Parent, token).ConfigureAwait(false);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Weapon objOldItem in e.OldItems)
                        await objOldItem.SetParentVehicleAsync(null, token).ConfigureAwait(false);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (Weapon objOldItem in e.OldItems)
                        await objOldItem.SetParentVehicleAsync(null, token).ConfigureAwait(false);
                    foreach (Weapon objNewItem in e.NewItems)
                        await objNewItem.SetParentVehicleAsync(Parent, token).ConfigureAwait(false);
                    break;
            }
        }

        /// <summary>
        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="objParent">Vehicle that the mod will be attached to.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnSkipSelectForms">Whether bonuses should be created.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlMod, int intRating, Vehicle objParent,
            string strForcedValue = "", bool blnSkipSelectForms = false, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlMod, intRating, objParent,
                strForcedValue, blnSkipSelectForms, token), token);
        }

        /// <summary>
        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="objParent">Vehicle that the mod will be attached to.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnSkipSelectForms">Whether bonuses should be created.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task CreateAsync(XmlNode objXmlMod, int intRating, Vehicle objParent,
            string strForcedValue = "", bool blnSkipSelectForms = false, CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlMod, intRating, objParent, strForcedValue,
                blnSkipSelectForms, token);
        }

        private async Task CreateCoreAsync(bool blnSync, XmlNode objXmlMod, int intRating, Vehicle objParent,
            string strForcedValue = "", bool blnSkipSelectForms = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objParent == null)
                throw new ArgumentNullException(nameof(objParent));
            await SetParentAsync(objParent, token).ConfigureAwait(false);
            if (objXmlMod == null) Utils.BreakIfDebug();
            if (!objXmlMod.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlMod });
                Utils.BreakIfDebug();
            }
            else
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            if (objXmlMod.TryGetStringFieldQuickly("name", ref _strName))
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetStringFieldQuickly("limit", ref _strLimit);
            objXmlMod.TryGetStringFieldQuickly("slots", ref _strSlots);
            _blnDowngrade = objXmlMod?["downgrade"] != null;
            if (!objXmlMod.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMod.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlMod.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
            objXmlMod.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objXmlMod.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            switch (_strMaxRating.ToUpperInvariant())
            {
                case "QTY":
                    _strRatingLabel = "Label_Qty";
                    break;
                case "SEATS":
                    _strRatingLabel = "Label_Seats";
                    break;
            }
            _intRating = string.IsNullOrEmpty(_strMaxRating)
                ? 0
                : Math.Min(Math.Max(intRating, 1), blnSync
                    ? MaxRating
                    : await GetMaxRatingAsync(token).ConfigureAwait(false));
            objXmlMod.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objXmlMod.TryGetInt32FieldQuickly("conditionmonitor", ref _intConditionMonitor);
            objXmlMod.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
            objXmlMod.TryGetStringFieldQuickly("ammoreplace", ref _strAmmoReplace);
            objXmlMod.TryGetDecFieldQuickly("ammobonus", ref _decAmmoBonus);
            objXmlMod.TryGetDecFieldQuickly("ammobonuspercent", ref _decAmmoBonusPercent);
            // Add Subsystem information if applicable.
            XmlElement xmlSubsystemsNode = objXmlMod?["subsystems"];
            if (xmlSubsystemsNode != null)
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdSubsystems))
                {
                    using (XmlNodeList xmlSubsystemList = xmlSubsystemsNode.SelectNodes("subsystem"))
                    {
                        if (xmlSubsystemList?.Count > 0)
                        {
                            foreach (XmlNode objXmlSubsystem in xmlSubsystemList)
                            {
                                sbdSubsystems.Append(objXmlSubsystem.InnerTextViaPool(token), ',');
                            }
                        }
                    }

                    // Remove last ","
                    if (sbdSubsystems.Length > 0)
                        --sbdSubsystems.Length;
                    _strSubsystems = sbdSubsystems.ToString();
                }
            }
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);

            _strCost = objXmlMod?["cost"]?.InnerTextViaPool(token) ?? string.Empty;
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
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                if (intHyphenIndex != -1)
                {
                    decimal.TryParse(strFirstHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);
                    decimal.TryParse(strSecondHalf, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMax);
                }
                else
                    decimal.TryParse(strFirstHalf.FastEscape('+'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decMin);

                if (decMin != 0 || decMax != decimal.MaxValue)
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
            }

            objXmlMod.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMod.TryGetStringFieldQuickly("page", ref _strPage);

            if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    Notes = CommonFunctions.GetBookNotes(objXmlMod, Name, CurrentDisplayName, Source,
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        Page, DisplayPage(GlobalSettings.Language), _objCharacter, token);
                else
                    await SetNotesAsync(await CommonFunctions.GetBookNotesAsync(objXmlMod, Name,
                        await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter, token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            _nodBonus = objXmlMod?["bonus"];
            _nodWirelessBonus = objXmlMod?["wirelessbonus"];
            _blnWirelessOn = false;

            if (Bonus != null && !blnSkipSelectForms)
            {
                ImprovementManager.SetForcedValue(strForcedValue, _objCharacter);
                if (blnSync)
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.VehicleMod,
                            _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, intRating,
                            CurrentDisplayNameShort, false, token))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }
                else if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.VehicleMod,
                             _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, intRating,
                             await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), false, token).ConfigureAwait(false))
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
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("mod");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("slots", _strSlots);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("ratinglabel", _strRatingLabel);
            objWriter.WriteElementString("conditionmonitor", _intConditionMonitor.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("subsystems", _strSubsystems);
            objWriter.WriteElementString("weaponmountcategories", _strWeaponMountCategories);
            objWriter.WriteElementString("ammobonus", _decAmmoBonus.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("ammobonuspercent", _decAmmoBonusPercent.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("ammoreplace", _strAmmoReplace);
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstVehicleWeapons)
                objWeapon.Save(objWriter);
            objWriter.WriteEndElement();
            if (_lstCyberware.Count > 0)
            {
                objWriter.WriteStartElement("cyberwares");
                _lstCyberware.ForEach(x => x.Save(objWriter));
                objWriter.WriteEndElement();
            }
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXmlViaPool());
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXmlViaPool());
            objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Vehicle Mod from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Are we loading a copy of an existing Vehicle Mod?</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            Utils.SafelyRunSynchronously(() => LoadCoreAsync(true, objNode, blnCopy));
        }

        /// <summary>
        /// Load the Vehicle Mod from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Are we loading a copy of an existing Vehicle Mod?</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task LoadAsync(XmlNode objNode, bool blnCopy = false, CancellationToken token = default)
        {
            return LoadCoreAsync(false, objNode, blnCopy, token);
        }

        private async Task LoadCoreAsync(bool blnSync, XmlNode objNode, bool blnCopy, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objNode == null)
                return;
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            Lazy<XPathNavigator> objMyNode = null;
            Microsoft.VisualStudio.Threading.AsyncLazy<XPathNavigator> objMyNodeAsync = null;
            if (blnSync)
                objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath(token));
            else
                objMyNodeAsync = new Microsoft.VisualStudio.Threading.AsyncLazy<XPathNavigator>(() => this.GetNodeXPathAsync(token), Utils.JoinableTaskFactory);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                (blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false))?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
            objNode.TryGetStringFieldQuickly("slots", ref _strSlots);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetInt32FieldQuickly("conditionmonitor", ref _intConditionMonitor);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludeInVehicle);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            if (!_blnEquipped)
            {
                objNode.TryGetBoolFieldQuickly("installed", ref _blnEquipped);
            }
            objNode.TryGetDecFieldQuickly("ammobonuspercent", ref _decAmmoBonusPercent);
            objNode.TryGetDecFieldQuickly("ammobonus", ref _decAmmoBonus);
            objNode.TryGetStringFieldQuickly("ammoreplace", ref _strAmmoReplace);
            objNode.TryGetStringFieldQuickly("subsystems", ref _strSubsystems);
            // Legacy Shims
            if (Name.StartsWith("Gecko Tips (Bod", StringComparison.Ordinal))
            {
                Name = "Gecko Tips";
                XPathNavigator objNewNode = blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false);
                if (objNewNode != null)
                {
                    objNewNode.TryGetStringFieldQuickly("cost", ref _strCost);
                    objNewNode.TryGetStringFieldQuickly("slots", ref _strSlots);
                }
            }
            if (Name.StartsWith("Gliding System (Bod", StringComparison.Ordinal))
            {
                Name = "Gliding System";
                XPathNavigator objNewNode = blnSync ? objMyNode.Value : await objMyNodeAsync.GetValueAsync(token).ConfigureAwait(false);
                if (objNewNode != null)
                {
                    objNewNode.TryGetStringFieldQuickly("cost", ref _strCost);
                    objNewNode.TryGetStringFieldQuickly("slots", ref _strSlots);
                    objNewNode.TryGetStringFieldQuickly("avail", ref _strAvail);
                }
            }

            XmlElement xmlChildrenNode = objNode["weapons"];
            using (XmlNodeList xmlNodeList = xmlChildrenNode?.SelectNodes("weapon"))
            {
                if (xmlNodeList?.Count > 0)
                {
                    if (blnSync)
                    {
                        foreach (XmlNode nodChild in xmlNodeList)
                        {
                            Weapon objWeapon = new Weapon(_objCharacter);
                            try
                            {
                                objWeapon.ParentVehicle = Parent;
                                objWeapon.ParentVehicleMod = this;
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                objWeapon.Load(nodChild, blnCopy);
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                _lstVehicleWeapons.Add(objWeapon);
                            }
                            catch
                            {
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                objWeapon.DeleteWeapon();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode nodChild in xmlNodeList)
                        {
                            Weapon objWeapon = new Weapon(_objCharacter);
                            try
                            {
                                await objWeapon.SetParentVehicleAsync(Parent, token).ConfigureAwait(false);
                                await objWeapon.SetParentVehicleModAsync(this, token).ConfigureAwait(false);
                                await objWeapon.LoadAsync(nodChild, blnCopy, token).ConfigureAwait(false);
                                await _lstVehicleWeapons.AddAsync(objWeapon, token).ConfigureAwait(false);
                            }
                            catch
                            {
                                await objWeapon.DeleteWeaponAsync(token: CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }
                        }
                    }
                }
            }

            xmlChildrenNode = objNode["cyberwares"];
            using (XmlNodeList xmlNodeList = xmlChildrenNode?.SelectNodes("cyberware"))
            {
                if (xmlNodeList?.Count > 0)
                {
                    if (blnSync)
                    {
                        foreach (XmlNode nodChild in xmlNodeList)
                        {
                            Cyberware objCyberware = new Cyberware(_objCharacter);
                            try
                            {
                                objCyberware.ParentVehicle = Parent;
                                // ReSharper disable once MethodHasAsyncOverload
                                objCyberware.Load(nodChild, blnCopy, token);
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                _lstCyberware.Add(objCyberware);
                            }
                            catch
                            {
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                objCyberware.DeleteCyberware();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode nodChild in xmlNodeList)
                        {
                            Cyberware objCyberware = new Cyberware(_objCharacter);
                            try
                            {
                                await objCyberware.SetParentVehicleAsync(Parent, token).ConfigureAwait(false);
                                await objCyberware.LoadAsync(nodChild, blnCopy, token).ConfigureAwait(false);
                                await _lstCyberware.AddAsync(objCyberware, token).ConfigureAwait(false);
                            }
                            catch
                            {
                                await objCyberware.DeleteCyberwareAsync(token: CancellationToken.None).ConfigureAwait(false);
                                throw;
                            }
                        }
                    }
                }
            }

            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = false;
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
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
            await objWriter.WriteStartElementAsync("mod", token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("fullname_english", await DisplayNameAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("category", await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("limit", Limit, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("slots", Slots, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("rating", (await GetRatingAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("ratinglabel", RatingLabel, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("avail", await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
            string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("cost", (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("owncost", (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("included", IncludedInVehicle.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
            await objWriter.WriteStartElementAsync("weapons", token).ConfigureAwait(false);
            await Weapons.ForEachAsync(x => x.Print(objWriter, objCulture, strLanguageToPrint, token), token).ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            await objWriter.WriteStartElementAsync("cyberwares", token).ConfigureAwait(false);
            await Cyberware.ForEachAsync(x => x.Print(objWriter, objCulture, strLanguageToPrint, token), token).ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            if (GlobalSettings.PrintNotes)
                await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Weapons.
        /// </summary>
        public TaggedObservableCollection<Weapon> Weapons => _lstVehicleWeapons;

        public TaggedObservableCollection<Cyberware> Cyberware => _lstCyberware;

        public WeaponMount WeaponMountParent
        {
            get => _objWeaponMountParent;
            set
            {
                if (Interlocked.Exchange(ref _objWeaponMountParent, value) == value)
                    return;
                Vehicle objNewParent = value?.Parent;
                if (objNewParent != null)
                    Parent = objNewParent;
            }
        }

        public Task SetWeaponMountParentAsync(WeaponMount value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            if (Interlocked.Exchange(ref _objWeaponMountParent, value) == value)
                return Task.CompletedTask;
            Vehicle objNewParent = value?.Parent;
            return objNewParent != null ? SetParentAsync(objNewParent, token) : Task.CompletedTask;
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Name.
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
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("vehicles.xml", strLanguage)
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value
                   ?? Category;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public async Task<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return (await _objCharacter.LoadDataXPathAsync("vehicles.xml", strLanguage, token: token)
                    .ConfigureAwait(false))
                .SelectSingleNodeAndCacheExpression(
                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate",
                    token: token)?.Value ?? Category;
        }

        /// <summary>
        /// Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Limits the Weapon Selection form to specified categories.
        /// </summary>
        public string WeaponMountCategories
        {
            set => _strWeaponMountCategories = value;
            get => _strWeaponMountCategories;
        }

        /// <summary>
        /// Which Vehicle types the Mod is limited to.
        /// </summary>
        public string Limit
        {
            get => _strLimit;
            set => _strLimit = value;
        }

        /// <summary>
        /// Number of Slots the Mod uses.
        /// </summary>
        public string Slots
        {
            get => _strSlots;
            set => _strSlots = value;
        }

        /// <summary>
        /// Vehicle Mod capacity.
        /// </summary>
        public string Capacity
        {
            get => _strCapacity;
            set => _strCapacity = value;
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        public int ProcessRatingString(string strExpression, int intRating) => ProcessRatingStringAsDec(strExpression, () => intRating).StandardRound();

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        public int ProcessRatingString(string strExpression, Func<int> funcRating) => ProcessRatingStringAsDec(strExpression, funcRating).StandardRound();

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        public async Task<int> ProcessRatingStringAsync(string strExpression, int intRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await ProcessRatingStringAsDecAsync(strExpression, intRating, token).ConfigureAwait(false)).Item1.StandardRound();
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        public async Task<int> ProcessRatingStringAsync(string strExpression, Func<Task<int>> funcRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await ProcessRatingStringAsDecAsync(strExpression, funcRating, token).ConfigureAwait(false)).Item1.StandardRound();
        }

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        public decimal ProcessRatingStringAsDec(string strExpression, int intRating) => ProcessRatingStringAsDec(strExpression, () => intRating, out bool _);

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        public decimal ProcessRatingStringAsDec(string strExpression, Func<int> funcRating) => ProcessRatingStringAsDec(strExpression, funcRating, out bool _);

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        public decimal ProcessRatingStringAsDec(string strExpression, Func<int> funcRating, out bool blnIsSuccess)
        {
            blnIsSuccess = true;
            if (string.IsNullOrEmpty(strExpression))
                return 0;
            strExpression = strExpression.ProcessFixedValuesString(funcRating).TrimStart('+');
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                blnIsSuccess = false;
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        if (strExpression.Contains("Rating"))
                        {
                            string strRating = funcRating().ToString(GlobalSettings.InvariantCultureInfo);
                            sbdValue.Replace("{Rating}", strRating);
                            sbdValue.Replace("Rating", strRating);
                        }
                        if (strExpression.Contains("Parent Cost") || strExpression.Contains("Parent Slots"))
                        {
                            WeaponMount objMount = WeaponMountParent;
                            if (objMount != null)
                            {
                                if (strExpression.Contains("Parent Cost"))
                                {
                                    string strMountCost = objMount.OwnCost.ToString(GlobalSettings.InvariantCultureInfo);
                                    sbdValue.Replace("{Parent Cost}", strMountCost).Replace("Parent Cost", strMountCost);
                                }
                                if (strExpression.Contains("Parent Slots"))
                                {
                                    string strMountCost = objMount.CalculatedSlots.ToString(GlobalSettings.InvariantCultureInfo);
                                    sbdValue.Replace("{Parent Slots}", strMountCost).Replace("Parent Slots", strMountCost);
                                }
                            }
                        }
                        Vehicle objVehicle = Parent;
                        if (objVehicle != null)
                        {
                            objVehicle.ProcessAttributesInXPath(sbdValue, strExpression, this);
                        }
                        else
                        {
                            Vehicle.FillAttributesInXPathWithDummies(sbdValue);
                            _objCharacter.ProcessAttributesInXPath(sbdValue, strExpression);
                        }
                        strExpression = sbdValue.ToString();
                    }
                }
                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                object objProcess;
                (blnIsSuccess, objProcess)
                    = CommonFunctions.EvaluateInvariantXPath(strExpression);
                if (blnIsSuccess)
                    return Convert.ToDecimal((double)objProcess);
            }

            return decValue;
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        public Task<ValueTuple<decimal, bool>> ProcessRatingStringAsDecAsync(string strExpression, int intRating, CancellationToken token = default) => ProcessRatingStringAsDecAsync(strExpression, () => Task.FromResult(intRating), token);

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        public async Task<ValueTuple<decimal, bool>> ProcessRatingStringAsDecAsync(string strExpression, Func<Task<int>> funcRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strExpression))
                return new ValueTuple<decimal, bool>(0, true);
            bool blnIsSuccess = true;
            strExpression = (await strExpression.ProcessFixedValuesStringAsync(funcRating, token).ConfigureAwait(false)).TrimStart('+');
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                if (strExpression.HasValuesNeedingReplacementForXPathProcessing())
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                    {
                        sbdValue.Append(strExpression);
                        if (strExpression.Contains("Rating"))
                        {
                            string strRating = (await funcRating().ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo);
                            sbdValue.Replace("{Rating}", strRating);
                            sbdValue.Replace("Rating", strRating);
                        }
                        if (strExpression.Contains("Parent Cost") || strExpression.Contains("Parent Slots"))
                        {
                            WeaponMount objMount = WeaponMountParent;
                            if (objMount != null)
                            {
                                if (strExpression.Contains("Parent Cost"))
                                {
                                    string strMountCost = (await objMount.GetOwnCostAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo);
                                    sbdValue.Replace("{Parent Cost}", strMountCost).Replace("Parent Cost", strMountCost);
                                }
                                if (strExpression.Contains("Parent Slots"))
                                {
                                    string strMountSlots = (await objMount.GetCalculatedSlotsAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo);
                                    sbdValue.Replace("{Parent Slots}", strMountSlots).Replace("Parent Slots", strMountSlots);
                                }
                            }
                        }
                        Vehicle objVehicle = Parent;
                        if (objVehicle != null)
                        {
                            await objVehicle.ProcessAttributesInXPathAsync(sbdValue, strExpression, this, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            Vehicle.FillAttributesInXPathWithDummies(sbdValue);
                            await _objCharacter
                                .ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                        }
                        strExpression = sbdValue.ToString();
                    }
                }
                object objProcess;
                (blnIsSuccess, objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(strExpression, token).ConfigureAwait(false);
                if (blnIsSuccess)
                    return new ValueTuple<decimal, bool>(Convert.ToDecimal((double)objProcess), true);
            }

            return new ValueTuple<decimal, bool>(decValue, blnIsSuccess);
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => Math.Min(_intRating, MaxRating);
            set
            {
                int intNewRating = Math.Min(Math.Max(1, value), MaxRating);
                if (Interlocked.Exchange(ref _intRating, intNewRating) != intNewRating && !IncludedInVehicle && Equipped)
                {
                    if (Parent != null && (Bonus?["sensor"] != null || (WirelessOn && WirelessBonus?["sensor"] != null)))
                    {
                        // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                        Gear objGear = Parent.GearChildren
                            .FirstOrDefault(x =>
                                x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent);
                        if (objGear != null)
                            objGear.Rating = Parent.GetCalculatedSensor(this);
                    }
                    if (_objCharacter.IsAI && _objCharacter.HomeNode is Vehicle objVehicle && objVehicle == Parent)
                        _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
                }
            }
        }

        public async Task<int> GetRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Math.Min(_intRating, await GetMaxRatingAsync(token).ConfigureAwait(false));
        }

        public async Task SetRatingAsync(int value, CancellationToken token = default)
        {
            int intNewRating = Math.Min(Math.Max(1, value), await GetMaxRatingAsync(token).ConfigureAwait(false));
            if (Interlocked.Exchange(ref _intRating, intNewRating) != intNewRating && !IncludedInVehicle && Equipped)
            {
                if (Parent != null && (Bonus?["sensor"] != null || (WirelessOn && WirelessBonus?["sensor"] != null)))
                {
                    // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                    Gear objGear = await _objParent.GearChildren
                        .FirstOrDefaultAsync(x =>
                            x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent, token: token).ConfigureAwait(false);
                    if (objGear != null)
                        await objGear.SetRatingAsync(await Parent.GetCalculatedSensorAsync(this, token).ConfigureAwait(false), token).ConfigureAwait(false);
                }
                if (await _objCharacter.GetIsAIAsync(token).ConfigureAwait(false) && await _objCharacter.GetHomeNodeAsync(token).ConfigureAwait(false) is Vehicle objVehicle && objVehicle == Parent)
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.PhysicalCM), token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum Rating.
        /// </summary>
        public int MaxRating
        {
            get
            {
                if (string.IsNullOrEmpty(_strMaxRating))
                    return 0;
                string strText = _strMaxRating;
                int intReturn;
                switch (strText.ToUpperInvariant())
                {
                    case "QTY":
                        intReturn = Vehicle.MaxWheels;
                        break;
                    case "SEATS":
                        intReturn = Parent.GetTotalSeats(this);
                        break;
                    case "BODY":
                        intReturn = Parent.GetTotalBody(this);
                        break;
                    default:
                    {
                        intReturn = strText.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decNumber)
                            ? ProcessRatingString(strText, _intRating)
                            : decNumber.StandardRound();
                    }
                        break;
                }

                int intMaxValue = int.MaxValue;
                if (Name.StartsWith("Armor", StringComparison.OrdinalIgnoreCase))
                    intMaxValue = Parent.MaxArmor;
                else
                {
                    switch (Category.ToUpperInvariant())
                    {
                        case "HANDLING":
                            intMaxValue = Parent.MaxHandling;
                            break;
                        case "SPEED":
                            intMaxValue = Parent.MaxSpeed;
                            break;
                        case "ACCELERATION":
                            intMaxValue = Parent.MaxAcceleration;
                            break;
                        case "SENSOR":
                            intMaxValue = Parent.MaxSensor;
                            break;
                        case "PILOT":
                            intMaxValue = Parent.MaxPilot;
                            break;
                        default:
                            if (Name.StartsWith("Pilot Program", StringComparison.OrdinalIgnoreCase))
                            {
                                intMaxValue = Parent.MaxPilot;
                            }
                            break;
                    }
                }

                return Math.Min(intReturn, intMaxValue);
            }
        }

        public async Task<int> GetMaxRatingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(_strMaxRating))
                return 0;
            string strText = _strMaxRating;
            int intReturn;
            switch (strText.ToUpperInvariant())
            {
                case "QTY":
                    intReturn = Vehicle.MaxWheels;
                    break;
                case "SEATS":
                    intReturn = await Parent.GetTotalSeatsAsync(this, token).ConfigureAwait(false);
                    break;
                case "BODY":
                    intReturn = await Parent.GetTotalBodyAsync(this, token).ConfigureAwait(false);
                    break;
                default:
                    {
                        if (strText.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decNumber))
                            intReturn = await ProcessRatingStringAsync(strText, _intRating, token).ConfigureAwait(false);
                        else
                            intReturn = decNumber.StandardRound();
                    }
                    break;
            }

            int intMaxValue = int.MaxValue;
            if (Name.StartsWith("Armor", StringComparison.OrdinalIgnoreCase))
                intMaxValue = await Parent.GetMaxArmorAsync(token).ConfigureAwait(false);
            else
            {
                switch (Category.ToUpperInvariant())
                {
                    case "HANDLING":
                        intMaxValue = await Parent.GetMaxHandlingAsync(token).ConfigureAwait(false);
                        break;
                    case "SPEED":
                        intMaxValue = await Parent.GetMaxSpeedAsync(token).ConfigureAwait(false);
                        break;
                    case "ACCELERATION":
                        intMaxValue = await Parent.GetMaxAccelerationAsync(token).ConfigureAwait(false);
                        break;
                    case "SENSOR":
                        intMaxValue = await Parent.GetMaxSensorAsync(token).ConfigureAwait(false);
                        break;
                    case "PILOT":
                        intMaxValue = await Parent.GetMaxPilotAsync(token).ConfigureAwait(false);
                        break;
                    default:
                        if (Name.StartsWith("Pilot Program", StringComparison.OrdinalIgnoreCase))
                        {
                            intMaxValue = await Parent.GetMaxPilotAsync(token).ConfigureAwait(false);
                        }
                        break;
                }
            }

            return Math.Min(intReturn, intMaxValue);
        }

        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
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
        /// Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
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
        /// Bonus node.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set
            {
                XmlNode xmlOldValue = Interlocked.Exchange(ref _nodBonus, value);
                if (xmlOldValue != value && !IncludedInVehicle && Equipped)
                {
                    if (_objParent != null && (xmlOldValue?["sensor"] != null || value?["sensor"] != null))
                    {
                        // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                        Gear objGear = _objParent.GearChildren
                            .FirstOrDefault(x =>
                                x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent);
                        if (objGear != null)
                            objGear.Rating = _objParent.GetCalculatedSensor(this);
                    }
                    if (_objCharacter.IsAI && _objCharacter.HomeNode is Vehicle objVehicle && ReferenceEquals(objVehicle, _objParent))
                        _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
                }
            }
        }

        /// <summary>
        /// Wireless Bonus node.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set
            {
                XmlNode xmlOldValue = Interlocked.Exchange(ref _nodWirelessBonus, value);
                if (xmlOldValue != value && !IncludedInVehicle && Equipped && WirelessOn)
                {
                    if (_objParent != null && (xmlOldValue?["sensor"] != null || value?["sensor"] != null))
                    {
                        // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                        Gear objGear = _objParent.GearChildren
                            .FirstOrDefault(x =>
                                x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent);
                        if (objGear != null)
                            objGear.Rating = _objParent.GetCalculatedSensor(this);
                    }
                    if (_objCharacter.IsAI && _objCharacter.HomeNode is Vehicle objVehicle && ReferenceEquals(objVehicle, _objParent))
                        _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
                }
            }
        }

        /// <summary>
        /// Whether the vehicle mod's wireless is enabled
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set
            {
                _blnWirelessOn = value;
                if (!IncludedInVehicle && Equipped)
                {
                    if (_objParent != null && WirelessBonus?["sensor"] != null)
                    {
                        // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                        Gear objGear = _objParent.GearChildren
                            .FirstOrDefault(x =>
                                x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent);
                        if (objGear != null)
                            objGear.Rating = _objParent.GetCalculatedSensor(this);
                    }
                    if (_objCharacter.IsAI && _objCharacter.HomeNode is Vehicle objVehicle && ReferenceEquals(objVehicle, _objParent))
                        _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
                }
            }
        }

        /// <summary>
        /// Whether the Mod included with the Vehicle by default.
        /// </summary>
        public bool IncludedInVehicle
        {
            get => _blnIncludeInVehicle;
            set
            {
                _blnIncludeInVehicle = value;
                if (Equipped)
                {
                    if (_objParent != null && (Bonus?["sensor"] != null || (WirelessOn && WirelessBonus?["sensor"] != null)))
                    {
                        // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                        Gear objGear = _objParent.GearChildren
                            .FirstOrDefault(x =>
                                x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent);
                        if (objGear != null)
                            objGear.Rating = _objParent.GetCalculatedSensor(this);
                    }
                    if (_objCharacter.IsAI && _objCharacter.HomeNode is Vehicle objVehicle && ReferenceEquals(objVehicle, _objParent))
                        _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
                }
            }
        }

        public async Task SetIncludedInVehicleAsync(bool value, CancellationToken token = default)
        {
            _blnIncludeInVehicle = value;
            if (Equipped)
            {
                if (_objParent != null && (Bonus?["sensor"] != null || (WirelessOn && WirelessBonus?["sensor"] != null)))
                {
                    // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                    Gear objGear = await _objParent.GearChildren
                        .FirstOrDefaultAsync(x =>
                            x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent, token).ConfigureAwait(false);
                    if (objGear != null)
                        objGear.Rating = await _objParent.GetCalculatedSensorAsync(this, token).ConfigureAwait(false);
                }
                if (await _objCharacter.GetIsAIAsync(token).ConfigureAwait(false)
                    && await _objCharacter.GetHomeNodeAsync(token).ConfigureAwait(false) is Vehicle objVehicle && ReferenceEquals(objVehicle, _objParent))
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.PhysicalCM), token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether this Mod is installed and contributing towards the Vehicle's stats.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set
            {
                _blnEquipped = value;
                if (!IncludedInVehicle)
                {
                    if (_objParent != null && (Bonus?["sensor"] != null || (WirelessOn && WirelessBonus?["sensor"] != null)))
                    {
                        // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                        Gear objGear = _objParent.GearChildren
                            .FirstOrDefault(x =>
                                x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent);
                        if (objGear != null)
                            objGear.Rating = _objParent.GetCalculatedSensor(this);
                    }
                    if (_objCharacter.IsAI && _objCharacter.HomeNode is Vehicle objVehicle && ReferenceEquals(objVehicle, _objParent))
                        _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
                }
            }
        }

        /// <summary>
        /// Whether this Mod is installed and contributing towards the Vehicle's stats.
        /// </summary>
        public async Task SetEquippedAsync(bool value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            _blnEquipped = value;
            if (!IncludedInVehicle)
            {
                if (_objParent != null && (Bonus?["sensor"] != null || (WirelessOn && WirelessBonus?["sensor"] != null)))
                {
                    // Any time any vehicle mod is changed, update our sensory array's rating, just in case
                    Gear objGear = await _objParent.GearChildren
                        .FirstOrDefaultAsync(x =>
                            x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent, token: token).ConfigureAwait(false);
                    if (objGear != null)
                        await objGear.SetRatingAsync(await _objParent.GetCalculatedSensorAsync(this, token).ConfigureAwait(false), token).ConfigureAwait(false);
                }
                if (await _objCharacter.GetIsAIAsync(token).ConfigureAwait(false) && await _objCharacter.GetHomeNodeAsync(token).ConfigureAwait(false) is Vehicle objVehicle && ReferenceEquals(objVehicle, _objParent))
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.PhysicalCM), token).ConfigureAwait(false);
            }
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
        /// Whether the Vehicle Mod allows Cyberware Plugins.
        /// </summary>
        public bool AllowCyberware => !string.IsNullOrEmpty(_strSubsystems);

        /// <summary>
        /// Allowed Cyberware Subsystems.
        /// </summary>
        public string Subsystems
        {
            get => _strSubsystems;
            set => _strSubsystems = value;
        }

        /// <summary>
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = value;
        }

        /// <summary>
        /// Whether the Vehicle Mod's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost;
            set => _blnDiscountCost = value;
        }

        /// <summary>
        /// Whether the Vehicle Mod is a downgrade for drone attributes
        /// </summary>
        public bool Downgrade => _blnDowngrade;

        /// <summary>
        /// Bonus/Penalty to the parent vehicle that this mod provides.
        /// </summary>
        public int ConditionMonitor => _intConditionMonitor;

        /// <summary>
        /// Vehicle that the Mod is attached to.
        /// </summary>
        public Vehicle Parent
        {
            get => _objParent;
            set
            {
                if (Interlocked.Exchange(ref _objParent, value) == value)
                    return;
                if (WeaponMountParent?.Parent != value)
                    WeaponMountParent = null;
                foreach (Weapon objChild in Weapons)
                    objChild.ParentVehicle = value;
                foreach (Cyberware objCyberware in Cyberware)
                    objCyberware.ParentVehicle = value;
            }
        }

        /// <summary>
        /// Vehicle that the Mod is attached to.
        /// </summary>
        public async Task SetParentAsync(Vehicle value, CancellationToken token = default)
        {
            if (Interlocked.Exchange(ref _objParent, value) == value)
                return;
            if (WeaponMountParent?.Parent != value)
                await SetWeaponMountParentAsync(null, token).ConfigureAwait(false);
            await Weapons.ForEachWithSideEffectsAsync(x => x.SetParentVehicleAsync(value, token), token).ConfigureAwait(false);
            await Cyberware.ForEachWithSideEffectsAsync(x => x.SetParentVehicleAsync(value, token), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Adjust the Weapon's Ammo amount by the specified flat value.
        /// </summary>
        public decimal AmmoBonus
        {
            get => _decAmmoBonus;
            set => _decAmmoBonus = value;
        }

        /// <summary>
        /// Adjust the Weapon's Ammo amount by the specified percentage.
        /// </summary>
        public decimal AmmoBonusPercent
        {
            get => _decAmmoBonusPercent;
            set => _decAmmoBonusPercent = value;
        }

        /// <summary>
        /// Replace the Weapon's Ammo value with the Weapon Mod's value.
        /// </summary>
        public string AmmoReplace
        {
            get => _strAmmoReplace;
            set => _strAmmoReplace = value;
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
        /// Is the object stolen via the Stolen Gear quality?
        /// </summary>
        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
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
        /// Total Availability of the VehicleMod.
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
                // Reordered to process fixed value strings
                strAvail = strAvail.ProcessFixedValuesString(() => Rating);

                if (strAvail.StartsWith("Range(", StringComparison.Ordinal))
                {
                    // If the Availability code is based on the current Rating of the item, separate the Availability string into an array and find the first bracket that the Rating is lower than or equal to.
                    foreach (string strValue in strAvail.CheapReplace("MaxRating", () => MaxRating.ToString(GlobalSettings.InvariantCultureInfo)).TrimStartOnce("Range(", true).TrimEndOnce(')').SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] astrValue = strValue.SplitFixedSizePooledArray('[', 2);
                        try
                        {
                            string strAvailCode = astrValue[1].Trim('[', ']');
                            int.TryParse(astrValue[0], NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                out int intMax);
                            int intRating = Rating;
                            if (intRating > intMax)
                                continue;
                            strAvail = intRating.ToString(GlobalSettings.InvariantCultureInfo) + strAvailCode;
                        }
                        finally
                        {
                            ArrayPool<string>.Shared.Return(astrValue);
                        }
                        break;
                    }
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                intAvail += ProcessRatingString(strAvail, () => Rating);
            }

            if (blnCheckChildren)
            {
                // Run through cyberware children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Cyberware objChild in Cyberware)
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

                // Run through weapon children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Weapon objChild in Weapons)
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

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInVehicle);
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
                // Reordered to process fixed value strings
                strAvail = await strAvail.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false);

                if (strAvail.StartsWith("Range(", StringComparison.Ordinal))
                {
                    // If the Availability code is based on the current Rating of the item, separate the Availability string into an array and find the first bracket that the Rating is lower than or equal to.
                    foreach (string strValue in (await strAvail.CheapReplaceAsync("MaxRating",
                            async () => (await GetMaxRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false)).TrimStartOnce("Range(", true).TrimEndOnce(')')
                        .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        string[] astrValue = strValue.SplitFixedSizePooledArray('[', 2);
                        try
                        {
                            string strAvailCode = astrValue[1].Trim('[', ']');
                            int.TryParse(astrValue[0], NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                out int intMax);
                            int intRating = await GetRatingAsync(token).ConfigureAwait(false);
                            if (intRating > intMax)
                                continue;
                            strAvail = intRating.ToString(GlobalSettings.InvariantCultureInfo) + strAvailCode;
                        }
                        finally
                        {
                            ArrayPool<string>.Shared.Return(astrValue);
                        }
                        break;
                    }
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                intAvail += await ProcessRatingStringAsync(strAvail, () => GetRatingAsync(token), token).ConfigureAwait(false);
            }

            if (blnCheckChildren)
            {
                // Run through cyberware children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                intAvail += await Cyberware.SumAsync(x => x.ParentID != InternalId, async objChild =>
                {
                    AvailabilityValue objLoopAvailTuple
                        = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvailTuple.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvailTuple.AddToParent ? await objLoopAvailTuple.GetValueAsync(token).ConfigureAwait(false) : 0;
                }, token).ConfigureAwait(false) + await Weapons.SumAsync(x => x.ParentID != InternalId, async objChild =>
                {
                    AvailabilityValue objLoopAvailTuple
                        = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
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

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInVehicle);
        }

        /// <summary>
        /// Calculated Capacity of the Vehicle Mod.
        /// </summary>
        public string CalculatedCapacity => GetCalculatedCapacity(GlobalSettings.CultureInfo);

        /// <summary>
        /// Calculated Capacity of the Vehicle Mod.
        /// </summary>
        public string GetCalculatedCapacity(CultureInfo objCulture)
        {
            string strReturn = Capacity;
            if (string.IsNullOrEmpty(strReturn))
                return 0.0m.ToString("#,0.##", objCulture);

            strReturn = strReturn.ProcessFixedValuesString(() => Rating);

            int intPos = strReturn.IndexOf("/[", StringComparison.Ordinal);
            if (intPos != -1)
            {
                string strFirstHalf = strReturn.Substring(0, intPos);
                string strSecondHalf = strReturn.Substring(intPos + 1);
                bool blnSquareBrackets = strFirstHalf.StartsWith('[');

                if (blnSquareBrackets && strFirstHalf.Length > 2)
                    strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                if (strFirstHalf == "[*]")
                    strReturn = "*";
                else
                {
                    strFirstHalf = strFirstHalf.ProcessFixedValuesString(() => Rating);

                    if (strFirstHalf.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue2))
                    {
                        decValue2 = ProcessRatingStringAsDec(strFirstHalf, () => Rating, out bool blnIsSuccess);
                        strReturn = blnIsSuccess ? decValue2.ToString("#,0.##", objCulture) : strFirstHalf;
                    }
                    else
                        strReturn = decValue2.ToString("#,0.##", objCulture);
                }

                if (blnSquareBrackets)
                    strReturn = "[" + strReturn + "]";

                if (strSecondHalf.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    strSecondHalf = strSecondHalf.Trim('[', ']');
                    decValue = ProcessRatingStringAsDec(strFirstHalf, () => Rating, out bool blnIsSuccess);
                    strSecondHalf = "[" + (blnIsSuccess ? decValue.ToString("#,0.##", objCulture) : strSecondHalf) + "]";
                }
                else
                    strSecondHalf = decValue.ToString("#,0.##", objCulture);

                strReturn += "/" + strSecondHalf;
            }
            else if (strReturn.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
            {
                // If the Capacity is determined by the Rating, evaluate the expression.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                bool blnSquareBrackets = strReturn.StartsWith('[');
                string strCapacity = strReturn;
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                decReturn = ProcessRatingStringAsDec(strCapacity, () => Rating, out bool blnIsSuccess);
                strReturn = blnIsSuccess ? decReturn.ToString("#,0.##", objCulture) : strCapacity;
                if (blnSquareBrackets)
                    strReturn = "[" + strReturn + "]";
            }
            else
                return decReturn.ToString("#,0.##", objCulture);

            return strReturn;
        }

        /// <summary>
        /// Calculated Capacity of the Vehicle Mod.
        /// </summary>
        public Task<string> GetCalculatedCapacityAsync(CancellationToken token = default)
        {
            return GetCalculatedCapacityAsync(GlobalSettings.CultureInfo, token);
        }

        /// <summary>
        /// Calculated Capacity of the Vehicle Mod.
        /// </summary>
        public async Task<string> GetCalculatedCapacityAsync(CultureInfo objCulture, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strReturn = Capacity;
            if (string.IsNullOrEmpty(strReturn))
                return 0.0m.ToString("#,0.##", objCulture);

            strReturn = await strReturn.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false);

            int intPos = strReturn.IndexOf("/[", StringComparison.Ordinal);
            if (intPos != -1)
            {
                string strFirstHalf = strReturn.Substring(0, intPos);
                string strSecondHalf = strReturn.Substring(intPos + 1);
                bool blnSquareBrackets = strFirstHalf.StartsWith('[');

                if (blnSquareBrackets && strFirstHalf.Length > 2)
                    strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                if (strFirstHalf == "[*]")
                    strReturn = "*";
                else
                {
                    strFirstHalf = await strFirstHalf.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false);

                    if (strFirstHalf.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue2))
                    {
                        bool blnIsSuccess;
                        (decValue2, blnIsSuccess) = await ProcessRatingStringAsDecAsync(strFirstHalf, () => GetRatingAsync(token), token).ConfigureAwait(false);
                        strReturn = blnIsSuccess ? decValue2.ToString("#,0.##", objCulture) : strFirstHalf;
                    }
                    else
                        strReturn = decValue2.ToString("#,0.##", objCulture);
                }

                if (blnSquareBrackets)
                    strReturn = "[" + strReturn + "]";

                if (strSecondHalf.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    strSecondHalf = strSecondHalf.Trim('[', ']');
                    bool blnIsSuccess;
                    (decValue, blnIsSuccess) = await ProcessRatingStringAsDecAsync(strSecondHalf, () => GetRatingAsync(token), token).ConfigureAwait(false);
                    strSecondHalf = "[" + (blnIsSuccess ? decValue.ToString("#,0.##", objCulture) : strSecondHalf) + "]";
                }
                else
                    strSecondHalf = decValue.ToString("#,0.##", objCulture);

                strReturn += "/" + strSecondHalf;
            }
            else if (strReturn.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
            {
                // If the Capacity is determined by the Rating, evaluate the expression.
                // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                bool blnSquareBrackets = strReturn.StartsWith('[');
                string strCapacity = strReturn;
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                bool blnIsSuccess;
                (decReturn, blnIsSuccess) = await ProcessRatingStringAsDecAsync(strCapacity, () => GetRatingAsync(token), token).ConfigureAwait(false);
                strReturn = blnIsSuccess ? decReturn.ToString("#,0.##", objCulture) : strCapacity;
                if (blnSquareBrackets)
                    strReturn = "[" + strReturn + "]";
            }
            else
                return decReturn.ToString("#,0.##", objCulture);

            return strReturn;
        }

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                if (string.IsNullOrEmpty(Capacity))
                    return 0.0m;
                if (Capacity.Contains("/["))
                {
                    // Get the Cyberware base Capacity.
                    string strBaseCapacity = GetCalculatedCapacity(GlobalSettings.InvariantCultureInfo);
                    strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                    decimal.TryParse(strBaseCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decReturn);
                    return decReturn
                           // Run through its Children and deduct the Capacity costs.
                           - Cyberware.Sum(objCyberware =>
                           {
                               string strCapacity = objCyberware.GetCalculatedCapacity(GlobalSettings.InvariantCultureInfo);
                               int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                               if (intPos != -1)
                                   strCapacity = strCapacity.Substring(intPos + 2,
                                       strCapacity.LastIndexOf(']') - intPos - 2);
                               else if (strCapacity.StartsWith('['))
                                   strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                               if (strCapacity == "*")
                                   strCapacity = "0";
                               decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decInner);
                               return decInner;
                           });
                }

                if (!Capacity.Contains('['))
                {
                    // Get the Cyberware base Capacity.
                    decimal.TryParse(GetCalculatedCapacity(GlobalSettings.InvariantCultureInfo), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decReturn);
                    return decReturn
                           // Run through its Children and deduct the Capacity costs.
                           - Cyberware.Sum(objCyberware =>
                           {
                               string strCapacity = objCyberware.GetCalculatedCapacity(GlobalSettings.InvariantCultureInfo);
                               int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                               if (intPos != -1)
                                   strCapacity = strCapacity.Substring(intPos + 2,
                                       strCapacity.LastIndexOf(']') - intPos - 2);
                               else if (strCapacity.StartsWith('['))
                                   strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                               if (strCapacity == "*")
                                   strCapacity = "0";
                               decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decInner);
                               return decInner;
                           });
                }

                return 0.0m;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public async Task<decimal> GetCapacityRemainingAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(Capacity))
                return 0.0m;
            if (Capacity.Contains("/["))
            {
                // Get the Cyberware base Capacity.
                string strBaseCapacity = await GetCalculatedCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false);
                strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                decimal.TryParse(strBaseCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decReturn);
                return decReturn
                              // Run through its Children and deduct the Capacity costs.
                              - await Cyberware.SumAsync(async objCyberware =>
                              {
                                  string strCapacity = await objCyberware.GetCalculatedCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false);
                                  int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                                  if (intPos != -1)
                                      strCapacity = strCapacity.Substring(intPos + 2,
                                          strCapacity.LastIndexOf(']') - intPos - 2);
                                  else if (strCapacity.StartsWith('['))
                                      strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                                  if (strCapacity == "*")
                                      strCapacity = "0";
                                  decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decInner);
                                  return decInner;
                              }, token).ConfigureAwait(false);
            }

            if (!Capacity.Contains('['))
            {
                // Get the Cyberware base Capacity.
                decimal.TryParse(await GetCalculatedCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decReturn);
                return decReturn
                       // Run through its Children and deduct the Capacity costs.
                       - await Cyberware.SumAsync(async objCyberware =>
                       {
                           string strCapacity = await objCyberware.GetCalculatedCapacityAsync(GlobalSettings.InvariantCultureInfo, token).ConfigureAwait(false);
                           int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                           if (intPos != -1)
                               strCapacity = strCapacity.Substring(intPos + 2,
                                   strCapacity.LastIndexOf(']') - intPos - 2);
                           else if (strCapacity.StartsWith('['))
                               strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                           if (strCapacity == "*")
                               strCapacity = "0";
                           decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decInner);
                           return decInner;
                       }, token).ConfigureAwait(false);
            }

            return 0.0m;
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

        public async Task<string> GetDisplayCapacityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Capacity.Contains('[') && !Capacity.Contains("/["))
                return await GetCalculatedCapacityAsync(token).ConfigureAwait(false);
            return string.Format(GlobalSettings.CultureInfo, await LanguageManager.GetStringAsync("String_CapacityRemaining", token: token).ConfigureAwait(false),
                await GetCalculatedCapacityAsync(token).ConfigureAwait(false), (await GetCapacityRemainingAsync(token).ConfigureAwait(false)).ToString("#,0.##", GlobalSettings.CultureInfo));
        }

        /// <summary>
        /// Total cost of the VehicleMod.
        /// </summary>
        public async Task<decimal> TotalCostInMountCreation(int intSlots, CancellationToken token = default)
        {
            decimal decReturn = 0;
            if (!IncludedInVehicle)
            {
                // If the cost is determined by the Rating, evaluate the expression.
                string strCostExpr = Cost;
                if (string.IsNullOrEmpty(strCostExpr))
                    return 0;
                if (strCostExpr.Contains("Slots"))
                {
                    string strValue = intSlots.ToString(GlobalSettings.InvariantCultureInfo);
                    strCostExpr = strCostExpr
                        .Replace("{Parent Slots}", strValue).Replace("Parent Slots", strValue)
                        .Replace("{Slots}", strValue).Replace("Slots", strValue);
                }
                decReturn = (await ProcessRatingStringAsDecAsync(strCostExpr, () => GetRatingAsync(token), token).ConfigureAwait(false)).Item1;
                
                if (DiscountCost)
                    decReturn *= 0.9m;
            }

            return decReturn + await Weapons.SumAsync(x => x.ParentID != InternalId, x => x.GetTotalCostAsync(token), token).ConfigureAwait(false)
                             + await Cyberware.SumAsync(x => x.ParentID != InternalId, x => x.GetTotalCostAsync(token), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Total cost of the VehicleMod.
        /// </summary>
        public decimal TotalCost => (IncludedInVehicle ? 0 : OwnCost) + Weapons.Sum(x => x.TotalCost) + Cyberware.Sum(x => x.TotalCost);

        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            return (IncludedInVehicle ? 0 : await GetOwnCostAsync(token).ConfigureAwait(false))
                   + await Weapons.SumAsync(x => x.GetTotalCostAsync(token), token).ConfigureAwait(false)
                   + await Cyberware.SumAsync(x => x.GetTotalCostAsync(token), token).ConfigureAwait(false);
        }

        /// <summary>
        /// The cost of just the Vehicle Mod itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                // If the cost is determined by the Rating, evaluate the expression.
                string strCostExpr = Cost;
                if (string.IsNullOrEmpty(strCostExpr))
                    return 0;
                decimal decReturn = ProcessRatingStringAsDec(strCostExpr, () => Rating);

                if (DiscountCost)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Vehicle Mod itself.
        /// </summary>
        public async Task<decimal> GetOwnCostAsync(CancellationToken token = default)
        {
            // If the cost is determined by the Rating, evaluate the expression.
            string strCostExpr = Cost;
            if (string.IsNullOrEmpty(strCostExpr))
                return 0;

            decimal decReturn = (await ProcessRatingStringAsDecAsync(strCostExpr, () => GetRatingAsync(token), token).ConfigureAwait(false)).Item1;
            
            if (DiscountCost)
                decReturn *= 0.9m;

            return decReturn;
        }

        /// <summary>
        /// The number of Slots the Mod consumes.
        /// </summary>
        public int CalculatedSlots => ProcessRatingString(Slots, () => Rating);

        /// <summary>
        /// The number of Slots the Mod consumes.
        /// </summary>
        public Task<int> GetCalculatedSlotsAsync(CancellationToken token = default) => ProcessRatingStringAsync(Slots, () => GetRatingAsync(token), token);

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
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            return objNode?.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name;
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + "(" + _objCharacter.TranslateExtra(Extra, strLanguage) + ")";
            int intRating = Rating;
            if (intRating > 0)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                strReturn += strSpace + "(" + LanguageManager.GetString(RatingLabel, strLanguage) + strSpace + intRating.ToString(objCulture) + ")";
            }
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + "(" + await _objCharacter.TranslateExtraAsync(Extra, strLanguage, token: token).ConfigureAwait(false) + ")";
            int intRating = await GetRatingAsync(token).ConfigureAwait(false);
            if (intRating > 0)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                strReturn += strSpace + "(" + await LanguageManager.GetStringAsync(RatingLabel, strLanguage, token: token).ConfigureAwait(false) + strSpace + intRating.ToString(objCulture) + ")";
            }
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        private static readonly string[] s_astrLimbStrings = { "ARM", "LEG" };

        /// <summary>
        /// Vehicle arm/leg Strength.
        /// </summary>
        public int TotalStrength
        {
            get
            {
                if (!Name.ContainsAny(s_astrLimbStrings, StringComparison.OrdinalIgnoreCase))
                    return 0;
                int intAttribute = 0;
                int intBody = 1;
                if (Parent != null)
                {
                    intBody = Parent.GetTotalBody(this);
                    intAttribute = Math.Max(intBody, 0);
                }
                int intBonus = 0;

                foreach (Cyberware objChild in Cyberware)
                {
                    switch (objChild.Name)
                    {
                        // If the limb has Customized Strength, this is its new base value.
                        case "Customized Strength":
                            intAttribute = objChild.GetRating(true);
                            break;
                        // If the limb has Enhanced Strength, this adds to the limb's value.
                        case "Enhanced Strength":
                            intBonus = objChild.GetRating(true);
                            break;
                    }
                }
                return Math.Min(intAttribute + intBonus, Math.Max(intBody * 2, 1));
            }
        }

        /// <summary>
        /// Vehicle arm/leg Agility.
        /// </summary>
        public int TotalAgility
        {
            get
            {
                if (!Name.ContainsAny(s_astrLimbStrings, StringComparison.OrdinalIgnoreCase))
                    return 0;

                int intAttribute = 0;
                int intPilot = 1;
                if (Parent != null)
                {
                    intPilot = Parent.GetPilot(this);
                    intAttribute = Math.Max(intPilot, 0);
                }
                int intBonus = 0;

                foreach (Cyberware objChild in Cyberware)
                {
                    switch (objChild.Name)
                    {
                        // If the limb has Customized Agility, this is its new base value.
                        case "Customized Agility":
                            intAttribute = objChild.GetRating(true);
                            break;
                        // If the limb has Enhanced Agility, this adds to the limb's value.
                        case "Enhanced Agility":
                            intBonus = objChild.GetRating(true);
                            break;
                    }
                }

                return Math.Min(intAttribute + intBonus, Math.Max(intPilot * 2, 1));
            }
        }

        /// <summary>
        /// Vehicle arm/leg Strength.
        /// </summary>
        public async Task<int> GetTotalStrengthAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!Name.ContainsAny(s_astrLimbStrings, StringComparison.OrdinalIgnoreCase))
                return 0;
            int intAttribute = 0;
            int intBody = 1;
            if (Parent != null)
            {
                intBody = await Parent.GetTotalBodyAsync(this, token).ConfigureAwait(false);
                intAttribute = Math.Max(intBody, 0);
            }

            int intBonus = 0;

            await Cyberware.ForEachAsync(async objChild =>
            {
                switch (objChild.Name)
                {
                    // If the limb has Customized Strength, this is its new base value.
                    case "Customized Strength":
                        intAttribute = await objChild.GetRatingAsync(true, token).ConfigureAwait(false);
                        break;
                    // If the limb has Enhanced Strength, this adds to the limb's value.
                    case "Enhanced Strength":
                        intBonus = await objChild.GetRatingAsync(true, token).ConfigureAwait(false);
                        break;
                }
            }, token: token).ConfigureAwait(false);

            return Math.Min(intAttribute + intBonus, Math.Max(intBody * 2, 1));
        }

        /// <summary>
        /// Vehicle arm/leg Agility.
        /// </summary>
        public async Task<int> GetTotalAgilityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!Name.ContainsAny(s_astrLimbStrings, StringComparison.OrdinalIgnoreCase))
                return 0;

            int intAttribute = 0;
            int intPilot = 1;
            if (Parent != null)
            {
                intPilot = await Parent.GetPilotAsync(token).ConfigureAwait(false);
                intAttribute = Math.Max(intPilot, 0);
            }

            int intBonus = 0;

            await Cyberware.ForEachAsync(async objChild =>
            {
                switch (objChild.Name)
                {
                    // If the limb has Customized Strength, this is its new base value.
                    case "Customized Agility":
                        intAttribute = await objChild.GetRatingAsync(true, token).ConfigureAwait(false);
                        break;
                    // If the limb has Enhanced Strength, this adds to the limb's value.
                    case "Enhanced Agility":
                        intBonus = await objChild.GetRatingAsync(true, token).ConfigureAwait(false);
                        break;
                }
            }, token: token).ConfigureAwait(false);

            return Math.Min(intAttribute + intBonus, Math.Max(intPilot * 2, 1));
        }

        /// <summary>
        /// Whether the Mod is allowed to accept Cyberware Modular Plugins.
        /// </summary>
        public bool AllowModularPlugins
        {
            get
            {
                return Cyberware.Any(objChild => objChild.AllowedSubsystems.Contains("Modular Plug-In"));
            }
        }

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("vehicles.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("vehicles.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/mods/mod", SourceID)
                            ?? objDoc.TryGetNodeById("/chummer/weaponmountmods/mod", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/mods/mod", Name)
                            ?? objDoc.TryGetNodeByNameOrId("/chummer/weaponmountmods/mod", Name);
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
                ? _objCharacter.LoadDataXPath("vehicles.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("vehicles.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/mods/mod", SourceID)
                            ?? objDoc.TryGetNodeById("/chummer/weaponmountmods/mod", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/mods/mod", Name)
                            ?? objDoc.TryGetNodeByNameOrId("/chummer/weaponmountmods/mod", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        #endregion Complex Properties

        #region Methods

        public decimal DeleteVehicleMod(bool blnDoRemoval = true)
        {
            if (blnDoRemoval)
            {
                if (WeaponMountParent != null)
                    WeaponMountParent.Mods.Remove(this);
                else
                    Parent?.Mods.Remove(this);
            }

            decimal decReturn = Weapons.AsEnumerableWithSideEffects().Sum(x => x.DeleteWeapon(false))
                                + Cyberware.AsEnumerableWithSideEffects().Sum(x => x.DeleteCyberware(false));

            DisposeSelf();

            return decReturn;
        }

        public async Task<decimal> DeleteVehicleModAsync(bool blnDoRemoval = true,
                                                              CancellationToken token = default)
        {
            if (blnDoRemoval)
            {
                if (WeaponMountParent != null)
                    await WeaponMountParent.Mods.RemoveAsync(this, token).ConfigureAwait(false);
                else if (Parent != null)
                    await Parent.Mods.RemoveAsync(this, token).ConfigureAwait(false);
            }

            decimal decReturn = await Weapons.SumWithSideEffectsAsync(x => x.DeleteWeaponAsync(false, token), token)
                                             .ConfigureAwait(false)
                                + await Cyberware.SumWithSideEffectsAsync(x => x.DeleteCyberwareAsync(false, token: token),
                                                           token).ConfigureAwait(false);

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
            if (!IncludedInVehicle)
            {
                AvailabilityValue objTotalAvail = await TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = await objTotalAvail.GetValueAsync(token).ConfigureAwait(false);
                    if (intAvailInt > await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMaximumAvailabilityAsync(token).ConfigureAwait(false))
                    {
                        int intLowestValidRestrictedGearAvail = -1;
                        foreach (int intValidAvail in dicRestrictedGearLimits.Keys)
                        {
                            if (intValidAvail >= intAvailInt && (intLowestValidRestrictedGearAvail < 0
                                                                 || intValidAvail < intLowestValidRestrictedGearAvail))
                                intLowestValidRestrictedGearAvail = intValidAvail;
                        }

                        string strNameToUse = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        if (Parent != null)
                            strNameToUse += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + "(" + await Parent.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + ")";

                        if (intLowestValidRestrictedGearAvail >= 0
                            && dicRestrictedGearLimits[intLowestValidRestrictedGearAvail] > 0)
                        {
                            --dicRestrictedGearLimits[intLowestValidRestrictedGearAvail];
                            sbdRestrictedItems.AppendLine().Append("\t\t", strNameToUse);
                        }
                        else
                        {
                            dicRestrictedGearLimits.Remove(intLowestValidRestrictedGearAvail);
                            ++intRestrictedCount;
                            sbdAvailItems.AppendLine().Append("\t\t", strNameToUse);
                        }
                    }
                }
            }

            intRestrictedCount += await Weapons
                                        .SumAsync(objChild =>
                                                objChild
                                                    .CheckRestrictedGear(
                                                        dicRestrictedGearLimits, sbdAvailItems,
                                                        sbdRestrictedItems,
                                                        token), token: token)
                                        .ConfigureAwait(false)
                                  + await Cyberware
                                          .SumAsync(objChild =>
                                                  objChild
                                                      .CheckRestrictedGear(
                                                          dicRestrictedGearLimits,
                                                          sbdAvailItems,
                                                          sbdRestrictedItems,
                                                          token),
                                              token: token)
                                          .ConfigureAwait(false);

            return intRestrictedCount;
        }

        #region UI Methods

        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsVehicleMod, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookEnabledAsync(Source, token).ConfigureAwait(false))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                Tag = this,
                ContextMenuStrip = cmsVehicleMod,
                ForeColor = await GetPreferredColorAsync(token).ConfigureAwait(false),
                ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // Cyberware.
            await Cyberware.ForEachAsync(async objCyberware =>
            {
                TreeNode objLoopNode = await objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear, token).ConfigureAwait(false);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }, token).ConfigureAwait(false);

            // VehicleWeapons.
            await Weapons.ForEachAsync(async objWeapon =>
            {
                TreeNode objLoopNode = await objWeapon.CreateTreeNode(cmsVehicleWeapon, cmsVehicleWeaponAccessory,
                    cmsVehicleWeaponAccessoryGear, token).ConfigureAwait(false);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }, token).ConfigureAwait(false);

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
                    return IncludedInVehicle
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return IncludedInVehicle
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
            {
                return IncludedInVehicle
                    ? ColorManager.GenerateCurrentModeDimmedColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                    : ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
            }
            return IncludedInVehicle
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
        }

        public decimal StolenTotalCost => CalculatedStolenTotalCost(true);

        public decimal NonStolenTotalCost => CalculatedStolenTotalCost(false);

        public decimal CalculatedStolenTotalCost(bool blnStolen)
        {
            decimal decReturn = !IncludedInVehicle && Stolen == blnStolen ? OwnCost : 0;
            return decReturn + Weapons.Sum(objWeapon => objWeapon.CalculatedStolenTotalCost(blnStolen)) +
                   Cyberware.Sum(objCyberware => objCyberware.CalculatedStolenTotalCost(blnStolen));
        }

        public Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(true, token);

        public Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(false, token);

        public async Task<decimal> CalculatedStolenTotalCostAsync(bool blnStolen, CancellationToken token = default)
        {
            decimal decReturn = !IncludedInVehicle && Stolen == blnStolen ? await GetOwnCostAsync(token).ConfigureAwait(false) : 0;
            return decReturn
                   + await Weapons
                       .SumAsync(objWeapon => objWeapon.CalculatedStolenTotalCostAsync(blnStolen, token), token)
                       .ConfigureAwait(false)
                   + await Cyberware
                       .SumAsync(objCyberware => objCyberware.CalculatedStolenTotalCostAsync(blnStolen, token), token)
                       .ConfigureAwait(false);
        }

        #endregion UI Methods

        #endregion Methods

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

        public async Task<bool> AllowPasteXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (await GlobalSettings.GetClipboardContentTypeAsync(token).ConfigureAwait(false))
            {
                case ClipboardContentType.Weapon:
                {
                    // TODO: Make this not depend on string names
                    return Name.StartsWith("Mechanical Arm", StringComparison.Ordinal) ||
                           Name.Contains("Drone Arm");
                }
                default:
                    return false;
            }
        }

        public Task<bool> AllowPasteObject(object input, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicleMod")))
                return false;

            DeleteVehicleMod();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager
                            .GetStringAsync("Message_DeleteVehicleMod", token: token)
                            .ConfigureAwait(false), token).ConfigureAwait(false))
                return false;

            await DeleteVehicleModAsync(token: token).ConfigureAwait(false);
            return true;
        }

        public bool Sell(decimal decPercentage, bool blnConfirmDelete)
        {
            if (!_objCharacter.Created)
                return Remove(blnConfirmDelete);

            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicleMod")))
                return false;

            IHasCost objParent = (IHasCost)WeaponMountParent ?? Parent;
            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = objParent.TotalCost;
                decAmount = DeleteVehicleMod() * decPercentage;
                decAmount += (decOriginal - objParent.TotalCost) * decPercentage;
            }
            else
            {
                decimal decOriginal = TotalCost;
                decAmount = (DeleteVehicleMod() + decOriginal) * decPercentage;
            }
            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldVehicleMod") + " " + CurrentDisplayNameShort, ExpenseType.Nuyen, DateTime.Now);
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
                        await LanguageManager.GetStringAsync("Message_DeleteVehicleMod", token: token).ConfigureAwait(false),
                        token).ConfigureAwait(false))
                return false;

            IHasCost objParent = (IHasCost)WeaponMountParent ?? Parent;
            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = await objParent.GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = await DeleteVehicleModAsync(token: token).ConfigureAwait(false) * decPercentage;
                decAmount += (decOriginal - await objParent.GetTotalCostAsync(token).ConfigureAwait(false)) * decPercentage;
            }
            else
            {
                decimal decOriginal = await GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = (await DeleteVehicleModAsync(token: token).ConfigureAwait(false) + decOriginal) * decPercentage;
            }

            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount,
                await LanguageManager.GetStringAsync("String_ExpenseSoldVehicleMod", token: token).ConfigureAwait(false) +
                " " + await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), ExpenseType.Nuyen,
                DateTime.Now);
            await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
            await _objCharacter.ModifyNuyenAsync(decAmount, token).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _lstVehicleWeapons.EnumerateWithSideEffects().ForEach(x => x.Dispose());
            _lstCyberware.EnumerateWithSideEffects().ForEach(x => x.Dispose());
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstVehicleWeapons.Dispose();
            _lstCyberware.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _lstVehicleWeapons.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
            await _lstCyberware.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private async ValueTask DisposeSelfAsync()
        {
            await _lstVehicleWeapons.DisposeAsync().ConfigureAwait(false);
            await _lstCyberware.DisposeAsync().ConfigureAwait(false);
        }

        public Character CharacterObject => _objCharacter;
    }
}
