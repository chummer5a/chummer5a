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
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A piece of Armor Modification.
    /// </summary>
    [DebuggerDisplay("{DisplayName(null, \"en-us\")}")]
    public sealed class ArmorMod : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, ICanSell, ICanEquip, IHasSource, IHasRating, ICanSort, IHasWirelessBonus, IHasStolenProperty, ICanPaste, IHasGear, ICanBlackMarketDiscount, IDisposable, IAsyncDisposable, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strArmorCapacity = "[0]";
        private string _strGearCapacity = string.Empty;
        private int _intArmorValue;
        private string _strMaxRating;
        private int _intRating;
        private string _strRatingLabel = "String_Rating";
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strWeight = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludedInArmor;
        private bool _blnEquipped = true;
        private string _strExtra = string.Empty;
        private Guid _guiWeaponID = Guid.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = true;
        private readonly Character _objCharacter;
        private readonly TaggedObservableCollection<Gear> _lstGear;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private bool _blnDiscountCost;
        private bool _blnStolen;
        private bool _blnEncumbrance = true;
        private bool _blnSkipEvents;
        private int _intSortOrder;

        #region Constructor, Create, Save, Load, and Print Methods

        public ArmorMod(Character objCharacter)
        {
            // Create the GUID for the new Armor Mod.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            _lstGear = new TaggedObservableCollection<Gear>(objCharacter.LockObject);
            _lstGear.AddTaggedCollectionChanged(this, GearOnCollectionChanged);
        }

        private async Task GearOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnDoEquipped = !_blnSkipEvents && _objCharacter?.IsLoading == false && Equipped && Parent?.Equipped == true;
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
                        await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token).ConfigureAwait(false);
                    break;
            }
        }

        /// <summary>
        /// Create a Armor Modification from an XmlNode.
        /// </summary>
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="intRating">Rating of the selected ArmorMod.</param>
        /// <param name="lstWeapons">List of Weapons that are created by the Armor.</param>
        /// <param name="blnSkipCost">Whether creating the ArmorMod should skip the Variable price dialogue (should only be used by frmSelectArmor).</param>
        /// <param name="blnForSelectForm">Whether this armor mod is being created for a Selection form (which means a lot of expensive code should be skipped).</param>
        /// <param name="blnSkipSelectForms">Whether to skip selection forms (related to improvements) when creating this ArmorMod.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlArmorNode, int intRating, IList<Weapon> lstWeapons, bool blnSkipCost = false,
            bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlArmorNode, intRating, lstWeapons,
                blnSkipCost, blnSkipSelectForms, blnForSelectForm, token), token);
        }

        /// <summary>
        /// Create a Armor Modification from an XmlNode.
        /// </summary>
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="intRating">Rating of the selected ArmorMod.</param>
        /// <param name="lstWeapons">List of Weapons that are created by the Armor.</param>
        /// <param name="blnSkipCost">Whether creating the ArmorMod should skip the Variable price dialogue (should only be used by frmSelectArmor).</param>
        /// <param name="blnSkipSelectForms">Whether to skip selection forms (related to improvements) when creating this ArmorMod.</param>
        /// <param name="blnForSelectForm">Whether this armor mod is being created for a Selection form (which means a lot of expensive code should be skipped).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task CreateAsync(XmlNode objXmlArmorNode, int intRating, IList<Weapon> lstWeapons, bool blnSkipCost = false,
            bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlArmorNode, intRating, lstWeapons, blnSkipCost, blnSkipSelectForms, blnForSelectForm,
                token);
        }

        private async Task CreateCoreAsync(bool blnSync, XmlNode objXmlArmorNode, int intRating, IList<Weapon> lstWeapons, bool blnSkipCost = false, bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!objXmlArmorNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for armor mod xmlnode", objXmlArmorNode });
                Utils.BreakIfDebug();
            }
            else
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            _blnSkipEvents = blnForSelectForm;
            _blnEquipped = !blnSkipSelectForms && !blnForSelectForm;
            objXmlArmorNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArmorNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlArmorNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlArmorNode.TryGetStringFieldQuickly("gearcapacity", ref _strGearCapacity);
            objXmlArmorNode.TryGetInt32FieldQuickly("armor", ref _intArmorValue);
            objXmlArmorNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            _intRating = intRating;
            _intRating = Math.Min(intRating, blnSync ? MaxRatingValue : await GetMaxRatingValueAsync(token).ConfigureAwait(false));
            objXmlArmorNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
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

            objXmlArmorNode.TryGetBoolFieldQuickly("encumbrance", ref _blnEncumbrance);

            _nodBonus = objXmlArmorNode["bonus"];
            _nodWirelessBonus = objXmlArmorNode["wirelessbonus"];

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
                        decMin = Convert.ToDecimal(strFirstHalf, GlobalSettings.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strSecondHalf, GlobalSettings.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strFirstHalf.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

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

            if (!blnForSelectForm && objXmlArmorNode["bonus"] != null && !blnSkipSelectForms)
            {
                if (blnSync)
                {
                    // ReSharper disable once MethodHasAsyncOverload
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod,
                            _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), objXmlArmorNode["bonus"], intRating,
                            CurrentDisplayNameShort, token: token))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }
                else if (!await ImprovementManager.CreateImprovementsAsync(_objCharacter, Improvement.ImprovementSource.ArmorMod,
                             _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), objXmlArmorNode["bonus"], intRating,
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

            // Add any Gear that comes with the Armor.
            XmlElement xmlChildrenNode = objXmlArmorNode["gears"];
            if (xmlChildrenNode != null)
            {
                XmlDocument objXmlGearDocument = blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? _objCharacter.LoadData("gear.xml", token: token)
                    : await _objCharacter.LoadDataAsync("gear.xml", token: token).ConfigureAwait(false);
                using (XmlNodeList xmlUseGearList = xmlChildrenNode.SelectNodes("usegear"))
                {
                    if (xmlUseGearList != null)
                    {
                        foreach (XmlNode objXmlArmorGear in xmlUseGearList)
                        {
                            Gear objGear = new Gear(_objCharacter);
                            if (blnSync
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    ? !objGear.CreateFromNode(objXmlGearDocument, objXmlArmorGear, lstWeapons,
                                        !blnSkipSelectForms)
                                    : !await objGear.CreateFromNodeAsync(objXmlGearDocument, objXmlArmorGear, lstWeapons,
                                        !blnSkipSelectForms, token: token).ConfigureAwait(false))
                                continue;
                            foreach (Weapon objWeapon in lstWeapons)
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
                    }
                }
            }

            // Add Weapons if applicable.
            // More than one Weapon can be added, so loop through all occurrences.
            using (XmlNodeList xmlAddWeaponList = objXmlArmorNode.SelectNodes("addweapon"))
            {
                if (xmlAddWeaponList != null)
                {
                    XmlDocument objXmlWeaponDocument = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData("weapons.xml", token: token)
                        : await _objCharacter.LoadDataAsync("weapons.xml", token: token).ConfigureAwait(false);

                    foreach (XmlNode objXmlAddWeapon in xmlAddWeaponList)
                    {
                        XmlNode objXmlWeapon = objXmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon",
                            objXmlAddWeapon.InnerText);

                        if (objXmlWeapon != null)
                        {
                            int intAddWeaponRating = 0;
                            string strLoopRating = objXmlAddWeapon.Attributes?["rating"]?.InnerText;
                            if (!string.IsNullOrEmpty(strLoopRating))
                            {
                                strLoopRating = blnSync
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    ? strLoopRating.CheapReplace("{Rating}",
                                        () => Rating.ToString(
                                            GlobalSettings
                                                .InvariantCultureInfo))
                                    : await strLoopRating.CheapReplaceAsync("{Rating}",
                                        () => Rating.ToString(
                                            GlobalSettings
                                                .InvariantCultureInfo), token: token).ConfigureAwait(false);
                                int.TryParse(strLoopRating, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                                             out intAddWeaponRating);
                            }

                            Weapon objGearWeapon = new Weapon(_objCharacter);
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

            objWriter.WriteStartElement("armormod");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("armor", _intArmorValue.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("gearcapacity", _strGearCapacity);
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("ratinglabel", _strRatingLabel);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("weight", _strWeight);
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
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("included", _blnIncludedInArmor.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether we are copying an existing node.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (objNode == null)
                return;
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            Lazy<XPathNavigator> objMyNode = new Lazy<XPathNavigator>(() => this.GetNodeXPath());
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            if (!objNode.TryGetStringFieldQuickly("category", ref _strCategory)
                || (_strCategory.EndsWith("Liners") && _objCharacter.LastSavedVersion < new ValueVersion(5, 255, 949)))
            {
                // Legacy shim for liners
                objMyNode.Value?.TryGetStringFieldQuickly("category", ref _strCategory);
            }
            objNode.TryGetInt32FieldQuickly("armor", ref _intArmorValue);
            objNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objNode.TryGetStringFieldQuickly("gearcapacity", ref _strGearCapacity);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            _intRating = Math.Min(_intRating, MaxRatingValue);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            if (!objNode.TryGetStringFieldQuickly("weight", ref _strWeight))
                objMyNode.Value?.TryGetStringFieldQuickly("weight", ref _strWeight);
            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludedInArmor);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = false;
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetField("weaponguid", Guid.TryParse, out _guiWeaponID);
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetBoolFieldQuickly("encumbrance", ref _blnEncumbrance);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);

            XmlElement xmlChildrenNode = objNode["gears"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList nodGears = xmlChildrenNode.SelectNodes("gear"))
                {
                    if (nodGears != null)
                    {
                        foreach (XmlNode nodGear in nodGears)
                        {
                            Gear objGear = new Gear(_objCharacter);
                            objGear.Load(nodGear, blnCopy);
                            _lstGear.Add(objGear);
                        }
                    }
                }
            }

            if (!blnCopy)
                return;
            if (!string.IsNullOrEmpty(Extra))
                ImprovementManager.SetForcedValue(Extra, _objCharacter);
            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, _guiID.ToString("D", GlobalSettings.InvariantCultureInfo), Bonus, 1, CurrentDisplayNameShort);
            string strSelectedValue = ImprovementManager.GetSelectedValue(_objCharacter);
            if (!string.IsNullOrEmpty(strSelectedValue))
            {
                Extra = strSelectedValue;
            }

            if (!_blnEquipped)
            {
                _blnEquipped = true;
                Equipped = false;
            }
            RefreshWirelessBonuses();
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
            // <armormod>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("armormod", token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category", await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("armor", Armor.ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("maxrating", MaxRating.ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rating", (await GetRatingAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ratinglabel", RatingLabel, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("avail", await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("cost", (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("owncost", (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(strNuyenFormat, objCulture), token).ConfigureAwait(false);
                string strWeightFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetWeightFormatAsync(token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weight", TotalWeight.ToString(strWeightFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ownweight", OwnWeight.ToString(strWeightFormat, objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("included", IncludedInArmor.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("equipped", Equipped.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
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
                await objWriter.WriteElementStringAsync("extra", await _objCharacter.TranslateExtraAsync(_strExtra, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            }
            finally
            {
                // </armormod>
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
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

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
        /// Name of the Mod.
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
            return objNode != null ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name : Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            int intRating = Rating;
            if (intRating > 0)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                strReturn += strSpace + '(' + LanguageManager.GetString(RatingLabel, strLanguage) + strSpace + intRating.ToString(objCulture) + ')';
            }
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpace + '(' + _objCharacter.TranslateExtra(Extra, strLanguage) + ')';
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
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
        /// Translated Category.
        /// </summary>
        public async Task<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return (await _objCharacter.LoadDataXPathAsync("armor.xml", strLanguage, token: token)
                    .ConfigureAwait(false))
                .SelectSingleNodeAndCacheExpression(
                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate",
                    token: token)?.Value ?? Category;
        }

        /// <summary>
        /// Special Armor Mod Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Mod's Armor value modifier.
        /// </summary>
        public int Armor
        {
            get => _intArmorValue;
            set
            {
                if (Interlocked.Exchange(ref _intArmorValue, value) != value && Equipped && Parent?.Equipped == true)
                {
                    _objCharacter?.OnPropertyChanged(nameof(Character.GetArmorRating));
                    _objCharacter?.RefreshArmorEncumbrance();
                }
            }
        }

        /// <summary>
        /// Whether the Armor Mod contributes to Encumbrance.
        /// </summary>
        public bool Encumbrance => _blnEncumbrance;

        /// <summary>
        /// Armor capacity.
        /// </summary>
        public string ArmorCapacity
        {
            get => _strArmorCapacity;
            set => _strArmorCapacity = value;
        }

        /// <summary>
        /// Capacity for gear plugins.
        /// </summary>
        public string GearCapacity
        {
            get => _strGearCapacity;
            set => _strGearCapacity = value;
        }

        /// <summary>
        /// Mod's current Rating.
        /// </summary>
        public int Rating
        {
            get => Math.Min(_intRating, MaxRatingValue);
            set
            {
                value = Math.Min(value, MaxRatingValue);
                if (Interlocked.Exchange(ref _intRating, value) == value)
                    return;
                if (Equipped && Parent.Equipped && _objCharacter != null &&
                    (Weight.ContainsAny("FixedValues", "Rating") ||
                        GearChildren.Any(x => x.Equipped && x.Weight.Contains("Parent Rating"))))
                {
                    _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
                }

                if (GearChildren.Count > 0)
                {
                    foreach (Gear objChild in GearChildren)
                    {
                        if (!objChild.MaxRating.Contains("Parent") && !objChild.MinRating.Contains("Parent"))
                            continue;
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
            if (Equipped && Parent.Equipped && _objCharacter != null
                && (Weight.ContainsAny("FixedValues", "Rating") || GearChildren.Any(x => x.Equipped && x.Weight.Contains("Parent Rating"), token)))
            {
                await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token).ConfigureAwait(false);
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
        /// Mod's Maximum Rating.
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
        private int ProcessRatingString(string strExpression, int intRating) => ProcessRatingStringAsDec(strExpression, () => intRating, out bool _).StandardRound();

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private int ProcessRatingString(string strExpression, Func<int> funcRating) => ProcessRatingStringAsDec(strExpression, funcRating, out bool _).StandardRound();

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private async Task<int> ProcessRatingStringAsync(string strExpression, int intRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (await ProcessRatingStringAsDecAsync(strExpression, () => Task.FromResult(intRating), token).ConfigureAwait(false)).Item1.StandardRound();
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
        private decimal ProcessRatingStringAsDec(string strExpression, int intRating) => ProcessRatingStringAsDec(strExpression, () => intRating, out bool _);

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        private decimal ProcessRatingStringAsDec(string strExpression, Func<int> funcRating) => ProcessRatingStringAsDec(strExpression, funcRating, out bool _);

        /// <summary>
        /// Processes a string into a decimal based on logical processing.
        /// </summary>
        private decimal ProcessRatingStringAsDec(string strExpression, Func<int> funcRating, out bool blnIsSuccess)
        {
            blnIsSuccess = true;
            strExpression = strExpression.ProcessFixedValuesString(funcRating);
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                blnIsSuccess = false;
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    Armor objParent = Parent;
                    if (objParent != null)
                    {
                        Lazy<string> strParentRating = new Lazy<string>(() => objParent.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.CheapReplace(strExpression, "{Armor Rating}", () => strParentRating.Value);
                        sbdValue.CheapReplace(strExpression, "Armor Rating", () => strParentRating.Value);
                        sbdValue.CheapReplace(strExpression, "{Parent Rating}", () => strParentRating.Value);
                        sbdValue.CheapReplace(strExpression, "Parent Rating", () => strParentRating.Value);
                        Lazy<string> strParentOwnCost = new Lazy<string>(() => objParent.OwnCost.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.CheapReplace(strExpression, "{Armor Cost}", () => strParentOwnCost.Value);
                        sbdValue.CheapReplace(strExpression, "Armor Cost", () => strParentOwnCost.Value);
                        sbdValue.CheapReplace(strExpression, "{Parent Cost}", () => strParentOwnCost.Value);
                        sbdValue.CheapReplace(strExpression, "Parent Cost", () => strParentOwnCost.Value);
                        Lazy<string> strParentOwnWeight = new Lazy<string>(() => objParent.OwnWeight.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.CheapReplace(strExpression, "{Armor Weight}", () => strParentOwnWeight.Value);
                        sbdValue.CheapReplace(strExpression, "Armor Weight", () => strParentOwnWeight.Value);
                        sbdValue.CheapReplace(strExpression, "{Parent Weight}", () => strParentOwnWeight.Value);
                        sbdValue.CheapReplace(strExpression, "Parent Weight", () => strParentOwnWeight.Value);
                        Lazy<string> strParentCapacity = new Lazy<string>(() => objParent.TotalArmorCapacity(GlobalSettings.InvariantCultureInfo));
                        sbdValue.CheapReplace(strExpression, "{Armor Capacity}", () => strParentCapacity.Value);
                        sbdValue.CheapReplace(strExpression, "Armor Capacity", () => strParentCapacity.Value);
                        sbdValue.CheapReplace(strExpression, "{Parent Capacity}", () => strParentCapacity.Value);
                        sbdValue.CheapReplace(strExpression, "Parent Capacity", () => strParentCapacity.Value);
                        sbdValue.CheapReplace(strExpression, "{Capacity}", () => strParentCapacity.Value);
                        sbdValue.CheapReplace(strExpression, "Capacity", () => strParentCapacity.Value);
                    }
                    else
                    {
                        sbdValue.Replace("{Armor Rating}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Armor Rating", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Parent Rating}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Parent Rating", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Armor Cost}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Armor Cost", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Parent Cost}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Parent Cost", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Armor Weight}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Armor Weight", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Parent Weight}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Parent Weight", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Armor Capacity}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Armor Capacity", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Parent Capacity}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Parent Capacity", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Capacity}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Capacity", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                    }
                    sbdValue.CheapReplace("{Rating}", () => funcRating().ToString(GlobalSettings.InvariantCultureInfo));
                    sbdValue.CheapReplace("Rating", () => funcRating().ToString(GlobalSettings.InvariantCultureInfo));
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    object objProcess;
                    (blnIsSuccess, objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                    if (blnIsSuccess)
                        return Convert.ToDecimal((double)objProcess);
                }
            }

            return decValue;
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private Task<Tuple<decimal, bool>> ProcessRatingStringAsDecAsync(string strExpression, int intRating, CancellationToken token = default) => ProcessRatingStringAsDecAsync(strExpression, () => Task.FromResult(intRating), token);

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        private async Task<Tuple<decimal, bool>> ProcessRatingStringAsDecAsync(string strExpression, Func<Task<int>> funcRating, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            bool blnIsSuccess = true;
            strExpression = await strExpression.ProcessFixedValuesStringAsync(funcRating, token).ConfigureAwait(false);
            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    Armor objParent = Parent;
                    if (objParent != null)
                    {
                        Microsoft.VisualStudio.Threading.AsyncLazy<string> strParentRating = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(
                            async () => (await objParent.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), Utils.JoinableTaskFactory);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Armor Rating}", () => strParentRating.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Armor Rating", () => strParentRating.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent Rating}", () => strParentRating.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Parent Rating", () => strParentRating.GetValueAsync(token), token: token).ConfigureAwait(false);
                        Microsoft.VisualStudio.Threading.AsyncLazy<string> strParentOwnCost = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(
                            async () => (await objParent.GetOwnCostAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), Utils.JoinableTaskFactory);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Armor Cost}", () => strParentOwnCost.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Armor Cost", () => strParentOwnCost.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent Cost}", () => strParentOwnCost.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Parent Cost", () => strParentOwnCost.GetValueAsync(token), token: token).ConfigureAwait(false);
                        Lazy<string> strParentOwnWeight = new Lazy<string>(() => objParent.OwnWeight.ToString(GlobalSettings.InvariantCultureInfo));
                        await sbdValue.CheapReplaceAsync(strExpression, "{Armor Weight}", () => strParentOwnWeight.Value, token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Armor Weight", () => strParentOwnWeight.Value, token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent Weight}", () => strParentOwnWeight.Value, token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Parent Weight", () => strParentOwnWeight.Value, token: token).ConfigureAwait(false);
                        Microsoft.VisualStudio.Threading.AsyncLazy<string> strParentCapacity = new Microsoft.VisualStudio.Threading.AsyncLazy<string>(
                            () => objParent.TotalArmorCapacityAsync(GlobalSettings.InvariantCultureInfo, token), Utils.JoinableTaskFactory);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Armor Capacity}", () => strParentCapacity.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Armor Capacity", () => strParentCapacity.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent Capacity}", () => strParentCapacity.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Parent Capacity", () => strParentCapacity.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Capacity}", () => strParentCapacity.GetValueAsync(token), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "Capacity", () => strParentCapacity.GetValueAsync(token), token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        sbdValue.Replace("{Armor Rating}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Armor Rating", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Parent Rating}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Parent Rating", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Armor Cost}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Armor Cost", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Parent Cost}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Parent Cost", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Armor Weight}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Armor Weight", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Parent Weight}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Parent Weight", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Armor Capacity}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Armor Capacity", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Parent Capacity}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Parent Capacity", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("{Capacity}", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdValue.Replace("Capacity", int.MaxValue.ToString(GlobalSettings.InvariantCultureInfo));
                    }
                    await sbdValue.CheapReplaceAsync(strExpression, "{Rating}", async () => (await funcRating().ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdValue.CheapReplaceAsync(strExpression, "Rating", async () => (await funcRating().ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await (await _objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false))
                        .ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    object objProcess;
                    (blnIsSuccess, objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        return new Tuple<decimal, bool>(Convert.ToDecimal((double)objProcess), true);
                }
            }

            return new Tuple<decimal, bool>(decValue, blnIsSuccess);
        }

        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
        }

        /// <summary>
        /// Mod's Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// The Mod's cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// The Mod's weight.
        /// </summary>
        public string Weight
        {
            get => _strWeight;
            set => _strWeight = value;
        }

        /// <summary>
        /// Mod's Sourcebook.
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
        /// Was the object stolen  via the Stolen Gear quality?
        /// </summary>
        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
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
        /// Whether an Armor Mod is equipped and should be included in the Armor's totals.
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
                    if (Parent?.Equipped == true)
                    {
                        ImprovementManager.EnableImprovements(_objCharacter,
                            _objCharacter.Improvements.Where(
                                x => x.ImproveSource
                                     == Improvement.ImprovementSource.ArmorMod
                                     && x.SourceName == InternalId));
                        // Add the Improvements from any Gear in the Armor.
                        foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                        {
                            if (objGear.Equipped)
                            {
                                objGear.ChangeEquippedStatus(true, true);
                            }
                        }
                    }
                }
                else
                {
                    ImprovementManager.DisableImprovements(_objCharacter,
                        _objCharacter.Improvements.Where(
                            x => x.ImproveSource == Improvement.ImprovementSource
                                .ArmorMod && x.SourceName == InternalId));
                    // Add the Improvements from any Gear in the Armor.
                    foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                    {
                        objGear.ChangeEquippedStatus(false, true);
                    }
                }

                if (Parent?.Equipped == true && _objCharacter?.IsLoading == false)
                {
                    _objCharacter.OnMultiplePropertyChanged(nameof(Character.ArmorEncumbrance),
                        nameof(Character.TotalCarriedWeight), nameof(Character.GetArmorRating));
                }
            }
        }

        /// <summary>
        /// Whether an Armor Mod is equipped and should be included in the Armor's totals.
        /// </summary>
        public async Task SetEquippedAsync(bool value, CancellationToken token = default)
        {
            if (_blnEquipped == value)
                return;
            _blnEquipped = value;
            if (value)
            {
                if (Parent?.Equipped == true)
                {
                    await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                        _objCharacter.Improvements.Where(
                            x => x.ImproveSource
                                 == Improvement.ImprovementSource.ArmorMod
                                 && x.SourceName == InternalId), token).ConfigureAwait(false);
                    // Add the Improvements from any Gear in the Armor.
                    await GearChildren.ForEachWithSideEffectsAsync(async objGear =>
                    {
                        if (objGear.Equipped)
                        {
                            await objGear.ChangeEquippedStatusAsync(true, true, token).ConfigureAwait(false);
                        }
                    }, token).ConfigureAwait(false);
                }
            }
            else
            {
                await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                    _objCharacter.Improvements.Where(
                        x => x.ImproveSource == Improvement.ImprovementSource
                            .ArmorMod && x.SourceName == InternalId), token).ConfigureAwait(false);
                // Add the Improvements from any Gear in the Armor.
                await GearChildren.ForEachWithSideEffectsAsync(objGear => objGear.ChangeEquippedStatusAsync(false, true, token), token).ConfigureAwait(false);
            }

            if (Parent?.Equipped == true && _objCharacter?.IsLoading == false)
            {
                await _objCharacter.OnMultiplePropertyChangedAsync(token, nameof(Character.ArmorEncumbrance),
                    nameof(Character.TotalCarriedWeight), nameof(Character.GetArmorRating)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Whether an Armor Mod's wireless bonus is enabled
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
        /// Whether this Mod is part of the base Armor configuration.
        /// </summary>
        public bool IncludedInArmor
        {
            get => _blnIncludedInArmor;
            set => _blnIncludedInArmor = value;
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
        /// Value that was selected during the Improvement Manager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = _objCharacter.ReverseTranslateExtra(value);
        }

        /// <summary>
        /// Whether the Armor Mod's cost should be discounted by 10% through the Black Market Pipeline Quality.
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
        /// Parent Armor.
        /// </summary>
        public Armor Parent { get; set; }

        /// <summary>
        /// The Gear currently applied to the Armor.
        /// </summary>
        public TaggedObservableCollection<Gear> GearChildren => _lstGear;

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

                blnModifyParentAvail = strAvail.StartsWith('+', '-') && !IncludedInArmor;

                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    intAvail = ProcessRatingString(strAvail.TrimStart('+'), () => Rating);
                else
                    intAvail = decValue.StandardRound();
            }

            if (blnCheckChildren)
            {
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

            // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInArmor);
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

                blnModifyParentAvail = strAvail.StartsWith('+', '-') && !IncludedInArmor;

                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                    intAvail = await ProcessRatingStringAsync(strAvail.TrimStart('+'), () => GetRatingAsync(token), token).ConfigureAwait(false);
                else
                    intAvail = decValue.StandardRound();
            }

            if (blnCheckChildren)
            {
                // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                intAvail += await GearChildren.SumAsync(x => x.ParentID != InternalId, async objChild =>
                {
                    AvailabilityValue objLoopAvailTuple = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvailTuple.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvailTuple.AddToParent ? await objLoopAvailTuple.GetValueAsync(token).ConfigureAwait(false) : 0;
                }, token).ConfigureAwait(false);
            }

            // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInArmor);
        }

        /// <summary>
        /// Calculated Gear Capacity of the Armor Mod.
        /// </summary>
        public string CalculatedGearCapacity
        {
            get
            {
                string strCapacity = GearCapacity;
                if (string.IsNullOrEmpty(strCapacity))
                    return "0";
                strCapacity = strCapacity.ProcessFixedValuesString(() => Rating);

                if (strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    decValue = ProcessRatingStringAsDec(strCapacity, () => Rating, out bool blnIsSuccess);
                    return blnIsSuccess
                        ? decValue.ToString("#,0.##", GlobalSettings.CultureInfo)
                        : strCapacity;
                }

                return decValue.ToString("#,0.##", GlobalSettings.CultureInfo);
            }
        }

        /// <summary>
        /// Calculated Gear Capacity of the Armor Mod.
        /// </summary>
        public async Task<string> GetCalculatedGearCapacityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCapacity = GearCapacity;
            if (string.IsNullOrEmpty(strCapacity))
                return "0";
            strCapacity = await strCapacity.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false);

            if (strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                bool blnIsSuccess;
                (decValue, blnIsSuccess) = await ProcessRatingStringAsDecAsync(strCapacity, () => GetRatingAsync(token), token).ConfigureAwait(false);
                return blnIsSuccess
                    ? decValue.ToString("#,0.##", GlobalSettings.CultureInfo)
                    : strCapacity;
            }

            return decValue.ToString("#,0.##", GlobalSettings.CultureInfo);
        }

        /// <summary>
        /// The amount of Capacity remaining in the Gear.
        /// </summary>
        public decimal GearCapacityRemaining
        {
            get
            {
                decimal decCapacity;
                string strMyCapacity = CalculatedGearCapacity;
                // Get the Gear base Capacity.
                int intPos = strMyCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    // If this is a multiple-capacity item, use only the first half.
                    strMyCapacity = strMyCapacity.Substring(0, intPos);
                }
                decCapacity = Convert.ToDecimal(strMyCapacity, GlobalSettings.CultureInfo);

                // Run through its Children and deduct the Capacity costs.
                decCapacity -= GearChildren.Sum(objChildGear =>
                {
                    string strCapacity = objChildGear.CalculatedArmorCapacity;
                    intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                    if (intPos != -1)
                    {
                        // If this is a multiple-capacity item, use only the second half.
                        strCapacity = strCapacity.Substring(intPos + 1);
                    }

                    // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                    if (strCapacity.StartsWith('[') && decimal.TryParse(strCapacity.Substring(1, strCapacity.Length - 2), NumberStyles.Any, GlobalSettings.CultureInfo, out decimal decTemp))
                        return decTemp * objChildGear.Quantity;
                    return 0;
                });

                return decCapacity;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Gear.
        /// </summary>
        public async Task<decimal> GetGearCapacityRemainingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decCapacity;
            string strMyCapacity = await GetCalculatedGearCapacityAsync(token).ConfigureAwait(false);
            // Get the Gear base Capacity.
            int intPos = strMyCapacity.IndexOf("/[", StringComparison.Ordinal);
            if (intPos != -1)
            {
                // If this is a multiple-capacity item, use only the first half.
                strMyCapacity = strMyCapacity.Substring(0, intPos);
            }
            decCapacity = Convert.ToDecimal(strMyCapacity, GlobalSettings.CultureInfo);

            // Run through its Children and deduct the Capacity costs.
            decCapacity -= await GearChildren.SumAsync(async objChildGear =>
            {
                string strCapacity = await objChildGear.GetCalculatedArmorCapacityAsync(token).ConfigureAwait(false);
                intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    // If this is a multiple-capacity item, use only the second half.
                    strCapacity = strCapacity.Substring(intPos + 1);
                }

                // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                if (strCapacity.StartsWith('[') && decimal.TryParse(strCapacity.Substring(1, strCapacity.Length - 2), NumberStyles.Any, GlobalSettings.CultureInfo, out decimal decTemp))
                    return decTemp * objChildGear.Quantity;
                return 0;
            }, token: token).ConfigureAwait(false);

            return decCapacity;
        }

        /// <summary>
        /// Caculated Capacity of the Armor Mod.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strCapacity = ArmorCapacity;
                if (string.IsNullOrEmpty(strCapacity))
                    return 0.0m.ToString("#,0.##", GlobalSettings.CultureInfo);
                strCapacity = strCapacity.ProcessFixedValuesString(() => Rating);

                bool blnSquareBrackets = strCapacity.StartsWith('[');
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                string strReturn;
                if (strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    decValue = ProcessRatingStringAsDec(strCapacity, () => Rating, out bool blnIsSuccess);
                    strReturn = blnIsSuccess
                        ? decValue.ToString("#,0.##", GlobalSettings.CultureInfo)
                        : strCapacity;
                }
                else
                    strReturn = decValue.ToString("#,0.##", GlobalSettings.CultureInfo);
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

                return strReturn;
            }
        }

        /// <summary>
        /// Caculated Capacity of the Armor Mod.
        /// </summary>
        public async Task<string> GetCalculatedCapacityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCapacity = ArmorCapacity;
            if (string.IsNullOrEmpty(strCapacity))
                return 0.0m.ToString("#,0.##", GlobalSettings.CultureInfo);
            strCapacity = await strCapacity.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false);

            bool blnSquareBrackets = strCapacity.StartsWith('[');
            if (blnSquareBrackets)
                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
            string strReturn;
            if (strCapacity.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                bool blnIsSuccess;
                (decValue, blnIsSuccess) = await ProcessRatingStringAsDecAsync(strCapacity, () => GetRatingAsync(token), token).ConfigureAwait(false);
                strReturn = blnIsSuccess
                    ? decValue.ToString("#,0.##", GlobalSettings.CultureInfo)
                    : strCapacity;
            }
            else
                strReturn = decValue.ToString("#,0.##", GlobalSettings.CultureInfo);
            if (blnSquareBrackets)
                strReturn = '[' + strReturn + ']';
            return strReturn;
        }

        public decimal TotalCapacity
        {
            get
            {
                string strCapacity = CalculatedCapacity;
                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    // If this is a multiple-capacity item, use only the second half.
                    strCapacity = strCapacity.Substring(intPos + 1);
                }

                if (strCapacity.StartsWith('['))
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                if (strCapacity == "*")
                    return 0;
                decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.CultureInfo, out decimal decReturn);
                return decReturn;
            }
        }

        public async Task<decimal> GetTotalCapacityAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCapacity = await GetCalculatedCapacityAsync(token).ConfigureAwait(false);
            int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
            if (intPos != -1)
            {
                // If this is a multiple-capacity item, use only the second half.
                strCapacity = strCapacity.Substring(intPos + 1);
            }

            if (strCapacity.StartsWith('['))
                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
            if (strCapacity == "*")
                return 0;
            decimal.TryParse(strCapacity, NumberStyles.Any, GlobalSettings.CultureInfo, out decimal decReturn);
            return decReturn;
        }

        /// <summary>
        /// Total cost of the Armor Mod.
        /// </summary>
        public decimal TotalCost => OwnCost + GearChildren.Sum(x => x.TotalCost);

        /// <summary>
        /// Total cost of the Armor Mod.
        /// </summary>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await GetOwnCostAsync(token).ConfigureAwait(false)
                   + await GearChildren.SumAsync(x => x.GetTotalCostAsync(token), token).ConfigureAwait(false);
        }

        public decimal StolenTotalCost => CalculatedStolenTotalCost(true);

        public decimal NonStolenTotalCost => CalculatedStolenTotalCost(false);

        public decimal CalculatedStolenTotalCost(bool blnStolen)
        {
            decimal decReturn = 0;
            if (Stolen == blnStolen)
                decReturn += OwnCost;

            // Go through all of the Gear for this piece of Armor and add the Cost value.
            decReturn += GearChildren.Sum(objGear => objGear.CalculatedStolenTotalCost(blnStolen));

            return decReturn;
        }

        public Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(true, token);

        public Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(false, token);

        public async Task<decimal> CalculatedStolenTotalCostAsync(bool blnStolen, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            decimal decReturn = 0;
            if (Stolen == blnStolen)
                decReturn += await GetOwnCostAsync(token).ConfigureAwait(false);

            // Go through all of the Gear for this piece of Armor and add the Cost value.
            decReturn += await GearChildren.SumAsync(objGear => objGear.CalculatedStolenTotalCostAsync(blnStolen, token), token).ConfigureAwait(false);

            return decReturn;
        }

        /// <summary>
        /// Cost for just the Armor Mod.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                string strCostExpr = Cost;
                if (string.IsNullOrEmpty(strCostExpr))
                    return 0;
                strCostExpr = strCostExpr.ProcessFixedValuesString(() => Rating);

                if (strCostExpr.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
                {
                    decReturn = ProcessRatingStringAsDec(strCostExpr.TrimStart('+'), () => Rating);
                }

                if (DiscountCost)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// Cost for just the Armor Mod.
        /// </summary>
        public async Task<decimal> GetOwnCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCostExpr = Cost;
            if (string.IsNullOrEmpty(strCostExpr))
                return 0;
            strCostExpr = await strCostExpr.ProcessFixedValuesStringAsync(() => GetRatingAsync(token), token).ConfigureAwait(false);

            if (strCostExpr.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
            {
                decReturn = (await ProcessRatingStringAsDecAsync(strCostExpr.TrimStart('+'), () => GetRatingAsync(token), token).ConfigureAwait(false)).Item1;
            }

            if (DiscountCost)
                decReturn *= 0.9m;

            return decReturn;
        }

        /// <summary>
        /// Total weight of the Armor Mod.
        /// </summary>
        public decimal TotalWeight => OwnWeight + GearChildren.Sum(x => x.Equipped, x => x.TotalWeight);

        /// <summary>
        /// Weight for just the Armor Mod.
        /// </summary>
        public decimal OwnWeight
        {
            get
            {
                if (IncludedInArmor)
                    return 0;
                string strWeightExpression = Weight;
                if (string.IsNullOrEmpty(strWeightExpression))
                    return 0;
                strWeightExpression = strWeightExpression.ProcessFixedValuesString(() => Rating);
                if (strWeightExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
                {
                    decReturn = ProcessRatingStringAsDec(strWeightExpression.TrimStart('+'), () => Rating);
                }
                return decReturn;
            }
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
                objReturn = objDoc.TryGetNodeById("/chummer/mods/mod", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/mods/mod", Name);
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
                objReturn = objDoc.TryGetNodeById("/chummer/mods/mod", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/mods/mod", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        #endregion Complex Properties

        #region Methods

        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteArmorMod(bool blnDoRemoval = true)
        {
            if (blnDoRemoval)
                Parent?.ArmorMods.Remove(this);

            decimal decReturn = 0.0m;
            // Remove any Improvements created by the Armor Mod's Gear.
            foreach (Gear objGear in GearChildren)
                decReturn += objGear.DeleteGear(false);

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

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, InternalId);

            DisposeSelf();

            return decReturn;
        }

        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        public async Task<decimal> DeleteArmorModAsync(bool blnDoRemoval = true, CancellationToken token = default)
        {
            if (blnDoRemoval && Parent != null)
                await Parent.ArmorMods.RemoveAsync(this, token).ConfigureAwait(false);

            // Remove any Improvements created by the Armor Mod's Gear.
            decimal decReturn = await GearChildren.SumWithSideEffectsAsync(x => x.DeleteGearAsync(false, token), token)
                                                  .ConfigureAwait(false);

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

            decReturn += await ImprovementManager
                               .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.ArmorMod,
                                                        InternalId, token).ConfigureAwait(false);

            await DisposeSelfAsync().ConfigureAwait(false);

            return decReturn;
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this armor mod.
        /// </summary>
        public void RefreshWirelessBonuses()
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped && Parent.WirelessOn)
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                                                               _objCharacter.Improvements.Where(x =>
                                                                   x.ImproveSource == Improvement.ImprovementSource
                                                                       .ArmorMod &&
                                                                   x.SourceName == InternalId));
                    }

                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, InternalId + "Wireless", WirelessBonus, Rating, CurrentDisplayNameShort);

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
                                                                      .ArmorMod &&
                                                                  x.SourceName == InternalId));
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    ImprovementManager.RemoveImprovements(_objCharacter,
                                                          _objCharacter.Improvements.Where(x =>
                                                              x.ImproveSource == Improvement.ImprovementSource
                                                                  .ArmorMod &&
                                                              x.SourceName == strSourceNameToRemove).ToList());
                }
            }

            foreach (Gear objGear in GearChildren.AsEnumerableWithSideEffects())
                objGear.RefreshWirelessBonuses();
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this armor mod.
        /// </summary>
        public async Task RefreshWirelessBonusesAsync(CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped && Parent.WirelessOn)
                {
                    if (WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
                    {
                        await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                                                          await _objCharacter.Improvements.ToListAsync(x =>
                                                                              x.ImproveSource == Improvement.ImprovementSource
                                                                                  .ArmorMod &&
                                                                              x.SourceName == InternalId, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    await ImprovementManager.CreateImprovementsAsync(_objCharacter,
                                                                     Improvement.ImprovementSource.ArmorMod,
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
                                                                         await _objCharacter.Improvements.ToListAsync(x =>
                                                                             x.ImproveSource == Improvement.ImprovementSource
                                                                                 .ArmorMod &&
                                                                             x.SourceName == InternalId, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                                                     await _objCharacter.Improvements.ToListAsync(x =>
                                                                             x.ImproveSource == Improvement
                                                                                 .ImprovementSource
                                                                                 .ArmorMod &&
                                                                             x.SourceName == strSourceNameToRemove,
                                                                         token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
            }

            await GearChildren.ForEachWithSideEffectsAsync(x => x.RefreshWirelessBonusesAsync(token), token: token).ConfigureAwait(false);
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
            if (!IncludedInArmor)
            {
                AvailabilityValue objTotalAvail = await TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = await objTotalAvail.GetValueAsync(token).ConfigureAwait(false);
                    if (intAvailInt > await _objCharacter.Settings.GetMaximumAvailabilityAsync(token).ConfigureAwait(false))
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
                            strNameToUse += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '(' + await Parent.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + ')';

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

            intRestrictedCount += await GearChildren
                                        .SumAsync(objChild =>
                                                objChild
                                                    .CheckRestrictedGear(
                                                        dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                        token), token: token)
                                        .ConfigureAwait(false);

            return intRestrictedCount;
        }

        #endregion Methods

        #region UI Methods

        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (IncludedInArmor && !string.IsNullOrEmpty(Source) && !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                Tag = this,
                ContextMenuStrip = string.IsNullOrEmpty(GearCapacity) ? cmsArmorMod : cmsArmorGear,
                ForeColor = await GetPreferredColorAsync(token).ConfigureAwait(false),
                ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
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

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return IncludedInArmor
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return IncludedInArmor
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
            {
                return IncludedInArmor
                    ? ColorManager.GenerateCurrentModeDimmedColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                    : ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
            }
            return IncludedInArmor
                ? ColorManager.GrayText
                : ColorManager.WindowText;
        }

        #endregion UI Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmorMod")))
                return false;

            DeleteArmorMod();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteArmorMod", token: token)
                            .ConfigureAwait(false), token).ConfigureAwait(false))
                return false;

            await DeleteArmorModAsync(token: token).ConfigureAwait(false);
            return true;
        }

        public bool Sell(decimal decPercentage, bool blnConfirmDelete)
        {
            if (!_objCharacter.Created)
                return Remove(blnConfirmDelete);

            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmorMod")))
                return false;

            // Record the cost of the Armor with the ArmorMod.
            Armor objParent = Parent;
            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = objParent.TotalCost;
                decAmount = DeleteArmorMod() * decPercentage;
                decAmount += (decOriginal - objParent.TotalCost) * decPercentage;
            }
            else
            {
                decimal decOriginal = TotalCost;
                decAmount = (DeleteArmorMod() + decOriginal) * decPercentage;
            }
            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldArmorMod") + ' ' + CurrentDisplayNameShort, ExpenseType.Nuyen, DateTime.Now);
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
                        await LanguageManager.GetStringAsync("Message_DeleteArmorMod", token: token).ConfigureAwait(false),
                        token).ConfigureAwait(false))
                return false;

            Armor objParent = Parent;
            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = await objParent.GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = await DeleteArmorModAsync(token: token).ConfigureAwait(false) * decPercentage;
                decAmount += (decOriginal - await objParent.GetTotalCostAsync(token).ConfigureAwait(false)) * decPercentage;
            }
            else
            {
                decimal decOriginal = await GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = (await DeleteArmorModAsync(token: token).ConfigureAwait(false) + decOriginal) * decPercentage;
            }

            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount,
                await LanguageManager.GetStringAsync("String_ExpenseSoldArmorMod", token: token).ConfigureAwait(false) +
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

        public async Task<bool> AllowPasteXml(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strGearCapacity = await GetCalculatedGearCapacityAsync(token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strGearCapacity) || strGearCapacity == "0")
                return false;
            IAsyncDisposable objLocker = await GlobalSettings.EnterClipboardReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                switch (await GlobalSettings.GetClipboardContentTypeAsync(token).ConfigureAwait(false))
                {
                    case ClipboardContentType.Gear:
                    {
                        XPathNodeIterator xmlAddonCategoryList =
                            (await this.GetNodeXPathAsync(token: token).ConfigureAwait(false))
                            ?.SelectAndCacheExpression("addoncategory", token);
                        if (!(xmlAddonCategoryList?.Count > 0))
                            return true;
                        string strGearCategory = (await GlobalSettings.GetClipboardAsync(token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpressionAsNavigator("category", token)?.Value ?? string.Empty;
                        return xmlAddonCategoryList.Cast<XPathNavigator>()
                            .Any(xmlCategory => xmlCategory.Value == strGearCategory);
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
            _lstGear.EnumerateWithSideEffects().ForEach(x => x.Dispose());
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstGear.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await _lstGear.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private ValueTask DisposeSelfAsync()
        {
            return _lstGear.DisposeAsync();
        }

        public Character CharacterObject => _objCharacter;
    }
}
