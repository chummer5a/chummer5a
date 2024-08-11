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
    /// Vehicle Modification.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public sealed class WeaponMount : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, ICanSell, ICanEquip, IHasSource, ICanSort, IHasStolenProperty, ICanPaste, ICanBlackMarketDiscount, IDisposable, IAsyncDisposable, IHasCharacterObject
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private Guid _guiSourceID;
        private decimal _decMarkup;
        private string _strAvail = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludeInVehicle;
        private bool _blnEquipped = true;
        private int _intWeaponCapacity = 1;
        private readonly TaggedObservableCollection<Weapon> _lstWeapons;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private string _strExtra = string.Empty;
        private string _strAllowedWeaponCategories = string.Empty;
        private bool _blnDiscountCost;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strLimit = string.Empty;
        private int _intSlots;
        private bool _blnFreeCost;
        private string _strCost = string.Empty;
        private string _strLocation = string.Empty;
        private string _strAllowedWeapons = string.Empty;
        private int _intSortOrder;
        private bool _blnStolen;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private readonly TaggedObservableCollection<VehicleMod> _lstMods;

        private readonly Character _objCharacter;

        public Character CharacterObject => _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public WeaponMount(Character character, Vehicle vehicle)
        {
            // Create the GUID for the new VehicleMod.
            _guiID = Guid.NewGuid();
            _objCharacter = character;
            Parent = vehicle;
            _lstWeapons = new TaggedObservableCollection<Weapon>(character.LockObject);
            _lstWeapons.AddTaggedCollectionChanged(this, EnforceWeaponCapacity);
            _lstMods = new TaggedObservableCollection<VehicleMod>(character.LockObject);
            _lstMods.AddTaggedCollectionChanged(this, SetModWeaponMountParent);

            void EnforceWeaponCapacity(object sender, NotifyCollectionChangedEventArgs args)
            {
                if (args.Action != NotifyCollectionChangedAction.Add)
                    return;
                int intNumFullWeapons = _lstWeapons.Count(x => string.IsNullOrEmpty(x.ParentID) || _lstWeapons.DeepFindById(x.ParentID) == null);
                if (intNumFullWeapons <= _intWeaponCapacity)
                    return;
                // If you ever hit this, you done fucked up. Make sure that weapon mounts cannot actually equip more weapons than they're allowed in the first place
                Utils.BreakIfDebug();
                foreach (Weapon objNewWeapon in args.NewItems.OfType<Weapon>().Reverse())
                {
                    if (!string.IsNullOrEmpty(objNewWeapon.ParentID) && _lstWeapons.DeepFindById(objNewWeapon.ParentID) != null)
                        continue;
                    _lstWeapons.Remove(objNewWeapon);
                    --intNumFullWeapons;
                    if (intNumFullWeapons <= _intWeaponCapacity)
                        break;
                }
            }

            async Task SetModWeaponMountParent(object sender, NotifyCollectionChangedEventArgs args, CancellationToken token = default)
            {
                token.ThrowIfCancellationRequested();
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (VehicleMod objNewMod in args.NewItems)
                        {
                            await objNewMod.SetWeaponMountParentAsync(this, token).ConfigureAwait(false);
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (VehicleMod objOldMod in args.OldItems)
                        {
                            if (objOldMod.WeaponMountParent != this)
                                continue;
                            await objOldMod.SetParentAsync(null, token).ConfigureAwait(false);
                        }
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        foreach (VehicleMod objOldMod in args.OldItems)
                        {
                            if (objOldMod.WeaponMountParent != this)
                                continue;
                            await objOldMod.SetParentAsync(null, token).ConfigureAwait(false);
                        }
                        foreach (VehicleMod objNewMod in args.NewItems)
                        {
                            await objNewMod.SetWeaponMountParentAsync(this, token).ConfigureAwait(false);
                        }
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        foreach (VehicleMod objMod in Mods)
                        {
                            await objMod.SetWeaponMountParentAsync(this, token).ConfigureAwait(false);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="decMarkup">Discount or markup that applies to the base cost of the mod.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlMod, decimal decMarkup = 0, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlMod, decMarkup, token), token);
        }

        /// <summary>
        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="decMarkup">Discount or markup that applies to the base cost of the mod.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task CreateAsync(XmlNode objXmlMod, decimal decMarkup = 0, CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlMod, decMarkup, token);
        }

        private async Task CreateCoreAsync(bool blnSync, XmlNode objXmlMod, decimal decMarkup = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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

            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetStringFieldQuickly("limit", ref _strLimit);
            objXmlMod.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objXmlMod.TryGetStringFieldQuickly("weaponcategories", ref _strAllowedWeaponCategories);
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);
            if (objXmlMod.TryGetInt32FieldQuickly("weaponcapacity", ref _intWeaponCapacity) && IsWeaponsFull)
                // If you ever hit this, you done fucked up. Make sure that weapon mounts cannot actually equip more weapons than they're allowed in the first place
                Utils.BreakIfDebug();
            if (!objXmlMod.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMod.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlMod.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            // Check for a Variable Cost.
            objXmlMod.TryGetStringFieldQuickly("cost", ref _strCost);
            if (!string.IsNullOrEmpty(_strCost) && _strCost.StartsWith("Variable(", StringComparison.Ordinal))
            {
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                string strCost = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                if (strCost.Contains('-'))
                {
                    string[] strValues = strCost.Split('-');
                    decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                    decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                }
                else
                    decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

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
                        using (ThreadSafeForm<SelectNumber> frmPickNumber
                               = await ThreadSafeForm<SelectNumber>.GetAsync(() => new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
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
            _decMarkup = decMarkup;

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
                    Notes = await CommonFunctions.GetBookNotesAsync(objXmlMod, Name,
                        await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter, token).ConfigureAwait(false);
            }
        }

        private SourceString _objCachedBackupSourceDetail;
        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
                // we instead display them as if they were one of the CRB mounts, but give them a different name
                if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                {
                    return _objCachedBackupSourceDetail == default
                        ? _objCachedBackupSourceDetail = SourceString.GetSourceString(
                            this.GetNodeXPath()?.SelectSingleNodeAndCacheExpression("source")?.Value ?? Source,
                            DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo,
                            _objCharacter)
                        : _objCachedBackupSourceDetail;
                }
                return _objCachedSourceDetail == default
                    ? _objCachedSourceDetail = SourceString.GetSourceString(Source,
                        DisplayPage(GlobalSettings.Language),
                        GlobalSettings.Language,
                        GlobalSettings.CultureInfo,
                        _objCharacter)
                    : _objCachedSourceDetail;
            }
        }

        public async Task<SourceString> GetSourceDetailAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
            // we instead display them as if they were one of the CRB mounts, but give them a different name
            if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
            {
                return _objCachedBackupSourceDetail == default
                    ? _objCachedBackupSourceDetail = await SourceString.GetSourceStringAsync(
                        (await this.GetNodeXPathAsync(token: token).ConfigureAwait(false))?.SelectSingleNodeAndCacheExpression("source", token)?.Value ?? Source,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), GlobalSettings.Language, GlobalSettings.CultureInfo,
                        _objCharacter, token).ConfigureAwait(false)
                    : _objCachedBackupSourceDetail;
            }
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
            objWriter.WriteStartElement("weaponmount");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("slots", _intSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("freecost", _blnFreeCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("markup", _decMarkup.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("weaponmountcategories", _strAllowedWeaponCategories);
            objWriter.WriteElementString("weaponcapacity", _intWeaponCapacity.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstWeapons)
            {
                objWeapon.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weaponmountoptions");
            foreach (WeaponMountOption objOption in WeaponMountOptions)
            {
                objOption.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("mods");
            foreach (VehicleMod objMod in _lstMods)
            {
                objMod.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the VehicleMod from the XmlNode, returning true if load was successful.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Indicates whether a new item will be created as a copy of this one.</param>
        public bool Load(XmlNode objNode, bool blnCopy = false)
        {
            if (objNode == null)
                return false;
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XPathNavigator node = this.GetNodeXPath();
                if (node != null)
                    node.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                else if (string.IsNullOrEmpty(Name))
                    return false; // No source ID, name, or node means this is probably a malformed weapon mount, stop it from loading
            }

            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
            objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strAllowedWeaponCategories);
            objNode.TryGetStringFieldQuickly("allowedweapons", ref _strAllowedWeapons);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetBoolFieldQuickly("freecost", ref _blnFreeCost);
            objNode.TryGetDecFieldQuickly("markup", ref _decMarkup);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludeInVehicle);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            if (!_blnEquipped)
            {
                objNode.TryGetBoolFieldQuickly("installed", ref _blnEquipped);
            }
            objNode.TryGetInt32FieldQuickly("weaponcapacity", ref _intWeaponCapacity);

            XmlElement xmlChildrenNode = objNode["weaponmountoptions"];
            using (XmlNodeList xmlWeaponMountOptionList = xmlChildrenNode?.SelectNodes("weaponmountoption"))
            {
                if (xmlWeaponMountOptionList != null)
                {
                    foreach (XmlNode xmlWeaponMountOptionNode in xmlWeaponMountOptionList)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        objWeaponMountOption.Load(xmlWeaponMountOptionNode);
                        WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }
            }

            xmlChildrenNode = objNode["mods"];
            using (XmlNodeList xmlModList = xmlChildrenNode?.SelectNodes("mod"))
            {
                if (xmlModList != null)
                {
                    foreach (XmlNode xmlModNode in xmlModList)
                    {
                        VehicleMod objMod = new VehicleMod(_objCharacter);
                        objMod.Load(xmlModNode);
                        Mods.Add(objMod);
                    }
                }
            }

            xmlChildrenNode = objNode["weapons"];
            using (XmlNodeList xmlWeaponList = xmlChildrenNode?.SelectNodes("weapon"))
            {
                if (xmlWeaponList != null)
                {
                    foreach (XmlNode xmlWeaponNode in xmlWeaponList)
                    {
                        if (Weapons.Count >= WeaponCapacity)
                        {
                            // Stop loading more weapons than we can actually mount and dump the rest into the character's basic inventory
                            Weapon objWeapon = new Weapon(_objCharacter);
                            objWeapon.Load(xmlWeaponNode, blnCopy);
                            _objCharacter.Weapons.Add(objWeapon);
                        }
                        else
                        {
                            Weapon objWeapon = new Weapon(_objCharacter)
                            {
                                ParentMount = this
                            };
                            objWeapon.Load(xmlWeaponNode, blnCopy);
                            Weapons.Add(objWeapon);
                        }
                    }
                }
            }

            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);

            return true;
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint,
            CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            await objWriter.WriteStartElementAsync("mod", token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("guid", InternalId, token: token).ConfigureAwait(false);
            // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
            // we instead display them as if they were one of the CRB mounts, but give them a different name
            if (IncludedInVehicle && !string.IsNullOrEmpty(Source) &&
                !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
            {
                XPathNavigator xmlOverrideNode =
                    await this.GetNodeXPathAsync(strLanguageToPrint, token: token).ConfigureAwait(false);
                if (xmlOverrideNode != null)
                {
                    await objWriter.WriteElementStringAsync(
                        "sourceid",
                        xmlOverrideNode.SelectSingleNodeAndCacheExpression("id", token: token)?.Value ?? SourceIDString,
                        token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync(
                        "source",
                        await _objCharacter.LanguageBookShortAsync(
                            xmlOverrideNode.SelectSingleNodeAndCacheExpression("source", token: token)?.Value ?? Source,
                            strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
                else
                {
                    await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token: token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("source",
                        await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                            .ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
            }
            else
            {
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token: token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("source",
                    await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false),
                    token: token).ConfigureAwait(false);
            }

            await objWriter.WriteElementStringAsync("name",
                await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("name_english", Name, token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("fullname",
                await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("category", DisplayCategory(strLanguageToPrint), token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("category_english", Category, token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("limit", Limit, token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("slots", Slots.ToString(GlobalSettings.InvariantCultureInfo),
                token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("avail",
                await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("cost",
                (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat,
                    objCulture), token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("owncost",
                (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat,
                    objCulture), token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false),
                token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("location", Location, token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("included",
                IncludedInVehicle.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
            await objWriter.WriteStartElementAsync("weapons", token: token).ConfigureAwait(false);
            await Weapons
                .ForEachAsync(objWeapon => objWeapon.Print(objWriter, objCulture, strLanguageToPrint, token), token)
                .ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            await objWriter.WriteStartElementAsync("mods", token: token).ConfigureAwait(false);
            await Mods.ForEachAsync(
                    objVehicleMod => objVehicleMod.Print(objWriter, objCulture, strLanguageToPrint, token), token)
                .ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            if (GlobalSettings.PrintNotes)
                await objWriter.WriteElementStringAsync("notes", Notes, token: token).ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Create a weapon mount using names instead of IDs, because user readability is important and untrustworthy.
        /// </summary>
        /// <param name="xmlNode">XmlNode to create the object from.</param>
        /// <param name="decMarkup">Discount or markup that applies to the base cost of the mod.</param>
        public void CreateByName(XmlNode xmlNode, decimal decMarkup = 0)
        {
            if (xmlNode == null)
                throw new ArgumentNullException(nameof(xmlNode));
            XmlDocument xmlDoc = _objCharacter.LoadData("vehicles.xml");
            string strSize = xmlNode["size"]?.InnerText;
            if (string.IsNullOrEmpty(strSize))
                return;
            XmlNode xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSize, "category = \"Size\"");
            if (xmlDataNode != null)
            {
                Create(xmlDataNode, decMarkup);

                string strFlexibility = xmlNode["flexibility"]?.InnerText;
                if (!string.IsNullOrEmpty(strFlexibility))
                {
                    xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strFlexibility, "category = \"Flexibility\"");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        objWeaponMountOption.Create(xmlDataNode);
                        objWeaponMountOption.IncludedInParent = true;
                        WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                string strControl = xmlNode["control"]?.InnerText;
                if (!string.IsNullOrEmpty(strControl))
                {
                    xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strControl, "category = \"Control\"");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        objWeaponMountOption.Create(xmlDataNode);
                        objWeaponMountOption.IncludedInParent = true;
                        WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                string strVisibility = xmlNode["visibility"]?.InnerText;
                if (!string.IsNullOrEmpty(strVisibility))
                {
                    xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strVisibility, "category = \"Visibility\"");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        objWeaponMountOption.Create(xmlDataNode);
                        objWeaponMountOption.IncludedInParent = true;
                        WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                _strLocation = xmlNode["location"]?.InnerText ?? string.Empty;
                _strAllowedWeapons = xmlNode["allowedweapons"]?.InnerText ?? string.Empty;
                xmlDataNode = xmlNode["mods"];
                if (xmlDataNode == null)
                    return;
                using (XmlNodeList xmlModList = xmlDataNode.SelectNodes("mod"))
                {
                    if (xmlModList != null)
                    {
                        foreach (XmlNode xmlModNode in xmlModList)
                        {
                            VehicleMod objMod = new VehicleMod(_objCharacter)
                            {
                                IncludedInVehicle = true
                            };
                            xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmountmods/mod", xmlModNode.InnerText);
                            objMod.Load(xmlDataNode);
                            Mods.Add(objMod);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create a weapon mount using names instead of IDs, because user readability is important and untrustworthy.
        /// </summary>
        /// <param name="xmlNode">XmlNode to create the object from.</param>
        /// <param name="decMarkup">Discount or markup that applies to the base cost of the mod.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task CreateByNameAsync(XmlNode xmlNode, decimal decMarkup = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (xmlNode == null)
                throw new ArgumentNullException(nameof(xmlNode));
            XmlDocument xmlDoc = await _objCharacter.LoadDataAsync("vehicles.xml", token: token).ConfigureAwait(false);
            string strSize = xmlNode["size"]?.InnerText;
            if (string.IsNullOrEmpty(strSize))
                return;
            XmlNode xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strSize, "category = \"Size\"");
            if (xmlDataNode != null)
            {
                await CreateAsync(xmlDataNode, decMarkup, token).ConfigureAwait(false);

                string strFlexibility = xmlNode["flexibility"]?.InnerText;
                if (!string.IsNullOrEmpty(strFlexibility))
                {
                    xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strFlexibility, "category = \"Flexibility\"");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        await objWeaponMountOption.CreateAsync(xmlDataNode, token).ConfigureAwait(false);
                        objWeaponMountOption.IncludedInParent = true;
                        WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                string strControl = xmlNode["control"]?.InnerText;
                if (!string.IsNullOrEmpty(strControl))
                {
                    xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strControl, "category = \"Control\"");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        await objWeaponMountOption.CreateAsync(xmlDataNode, token).ConfigureAwait(false);
                        objWeaponMountOption.IncludedInParent = true;
                        WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                string strVisibility = xmlNode["visibility"]?.InnerText;
                if (!string.IsNullOrEmpty(strVisibility))
                {
                    xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strVisibility, "category = \"Visibility\"");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        await objWeaponMountOption.CreateAsync(xmlDataNode, token).ConfigureAwait(false);
                        objWeaponMountOption.IncludedInParent = true;
                        WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                _strLocation = xmlNode["location"]?.InnerText ?? string.Empty;
                _strAllowedWeapons = xmlNode["allowedweapons"]?.InnerText ?? string.Empty;
                xmlDataNode = xmlNode["mods"];
                if (xmlDataNode == null)
                    return;
                using (XmlNodeList xmlModList = xmlDataNode.SelectNodes("mod"))
                {
                    if (xmlModList != null)
                    {
                        foreach (XmlNode xmlModNode in xmlModList)
                        {
                            VehicleMod objMod = new VehicleMod(_objCharacter)
                            {
                                IncludedInVehicle = true
                            };
                            xmlDataNode = xmlDoc.TryGetNodeByNameOrId("/chummer/weaponmountmods/mod", xmlModNode.InnerText);
                            objMod.Load(xmlDataNode);
                            await Mods.AddAsync(objMod, token).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Weapons.
        /// </summary>
        public TaggedObservableCollection<Weapon> Weapons => _lstWeapons;

        /// <summary>
        /// Maximum number of weapons this mount can have.
        /// </summary>
        public int WeaponCapacity => _intWeaponCapacity;

        /// <summary>
        /// Whether this mount can accommodate any more weapons or not
        /// </summary>
        public bool IsWeaponsFull => Weapons.Count(x => string.IsNullOrEmpty(x.ParentID) || Weapons.DeepFindById(x.ParentID) == null) >= _intWeaponCapacity;

        /// <summary>
        /// Whether this mount can accommodate any more weapons or not
        /// </summary>
        public async Task<bool> GetIsWeaponsFullAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await Weapons.CountAsync(async x =>
                       string.IsNullOrEmpty(x.ParentID) || await Weapons.DeepFindByIdAsync(x.ParentID, token: token).ConfigureAwait(false) == null, token: token).ConfigureAwait(false) >=
                   _intWeaponCapacity;
        }

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value)
                    return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Where the mount is physically located on the vehicle.
        /// </summary>
        public string Location
        {
            get => _strLocation;
            set => _strLocation = value;
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
        public string AllowedWeaponCategories
        {
            set => _strAllowedWeaponCategories = value;
            get => _strAllowedWeaponCategories;
        }

        public string AllowedWeapons
        {
            get => _strAllowedWeapons;
            set => _strAllowedWeapons = value;
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
        public int Slots
        {
            get => _intSlots;
            set => _intSlots = value;
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
        /// Whether to consider this weapon mount free. Needs to be a separate thing from the Cost string because weapon mounts can be edited.
        /// </summary>
        public bool FreeCost
        {
            get => _blnFreeCost;
            set => _blnFreeCost = value;
        }

        /// <summary>
        /// Markup.
        /// </summary>
        public decimal Markup
        {
            get => _decMarkup;
            set => _decMarkup = value;
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
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
                // we instead display them as if they were one of the CRB mounts, but give them a different name
                if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                {
                    string strReturn = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("page")?.Value ?? Page;
                    return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
                }
                return Page;
            }
            string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
                // we instead display them as if they were one of the CRB mounts, but give them a different name
                if (IncludedInVehicle && !string.IsNullOrEmpty(Source) &&
                    !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
                {
                    string strReturn = (await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))
                        ?.SelectSingleNodeAndCacheExpression("page", token)?.Value ?? Page;
                    return !string.IsNullOrWhiteSpace(strReturn) ? strReturn : Page;
                }

                return Page;
            }

            string s = (await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))
                ?.SelectSingleNodeAndCacheExpression("altpage", token)?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Whether the Mod included with the Vehicle by default.
        /// </summary>
        public bool IncludedInVehicle
        {
            get => _blnIncludeInVehicle;
            set => _blnIncludeInVehicle = value;
        }

        /// <summary>
        /// Whether this Mod is installed and contributing towards the Vehicle's stats.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set => _blnEquipped = value;
        }

        /// <summary>
        /// Whether this Mod is installed and contributing towards the Vehicle's stats.
        /// </summary>
        public Task SetEquippedAsync(bool value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            _blnEquipped = value;
            return Task.CompletedTask;
        }

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
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }

        /// <summary>
        /// Vehicle that the Mod is attached to.
        /// </summary>
        public Vehicle Parent { get; }

        /// <summary>
        /// Extra settings for this weapon (visibility, flexibility, and control)
        /// </summary>
        public List<WeaponMountOption> WeaponMountOptions { get; } = new List<WeaponMountOption>(3);

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
        /// The number of Slots the Mount consumes, including all child items.
        /// </summary>
        public int CalculatedSlots => Slots + WeaponMountOptions.Sum(w => w.Slots) + Mods.Sum(x => !x.IncludedInVehicle, m => m.CalculatedSlots);

        /// <summary>
        /// The number of Slots the Mount consumes, including all child items.
        /// </summary>
        public async Task<int> GetCalculatedSlotsAsync(CancellationToken token = default)
        {
            return Slots + WeaponMountOptions.Sum(w => w.Slots) + await Mods.SumAsync(x => !x.IncludedInVehicle, m => m.GetCalculatedSlotsAsync(token), token).ConfigureAwait(false);
        }

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
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                {
                    sbdAvail.Append(strAvail.TrimStart('+'));
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdAvail, strAvail);
                    sbdAvail.CheapReplace(strAvail, "Vehicle Cost",
                                          () => Parent?.OwnCost.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                    sbdAvail.CheapReplace(strAvail, "Body",
                                          () => Parent?.Body > 0
                                              ? Parent.Body.ToString(GlobalSettings.InvariantCultureInfo)
                                              : "0.5");
                    sbdAvail.CheapReplace(strAvail, "Speed",
                                          () => Parent?.Speed.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    sbdAvail.CheapReplace(strAvail, "Acceleration",
                                          () => Parent?.Accel.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    sbdAvail.CheapReplace(strAvail, "Handling",
                                          () => Parent?.Handling.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");

                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString());
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
            }

            // Run through the Accessories and add in their availability.
            foreach (AvailabilityValue objLoopAvailTuple in WeaponMountOptions.Select(x => x.TotalAvailTuple))
            {
                //if (objLoopAvailTuple.Item3)
                intAvail += objLoopAvailTuple.Value;
                if (objLoopAvailTuple.Suffix == 'F')
                    chrLastAvailChar = 'F';
                else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                    chrLastAvailChar = 'R';
            }

            if (blnCheckChildren)
            {
                // Run through the Vehicle Mods and add in their availability.
                foreach (VehicleMod objVehicleMod in Mods)
                {
                    if (!objVehicleMod.IncludedInVehicle && objVehicleMod.Equipped)
                    {
                        AvailabilityValue objLoopAvailTuple = objVehicleMod.TotalAvailTuple();
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

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
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                {
                    sbdAvail.Append(strAvail.TrimStart('+'));
                    await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdAvail, strAvail, token: token).ConfigureAwait(false);

                    await sbdAvail.CheapReplaceAsync(strAvail, "Vehicle Cost",
                                                     () => Parent?.OwnCost.ToString(GlobalSettings.InvariantCultureInfo) ?? "0", token: token).ConfigureAwait(false);
                    // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                    await sbdAvail.CheapReplaceAsync(strAvail, "Body",
                                                     () => Parent?.Body > 0
                                                         ? Parent.Body.ToString(GlobalSettings.InvariantCultureInfo)
                                                         : "0.5", token: token).ConfigureAwait(false);
                    await sbdAvail.CheapReplaceAsync(strAvail, "Speed",
                                                     () => Parent?.Speed.ToString(GlobalSettings.InvariantCultureInfo) ?? "0", token: token).ConfigureAwait(false);
                    await sbdAvail.CheapReplaceAsync(strAvail, "Acceleration",
                                                     () => Parent?.Accel.ToString(GlobalSettings.InvariantCultureInfo) ?? "0", token: token).ConfigureAwait(false);
                    await sbdAvail.CheapReplaceAsync(strAvail, "Handling",
                                                     () => Parent?.Handling.ToString(GlobalSettings.InvariantCultureInfo) ?? "0", token: token).ConfigureAwait(false);

                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
            }

            // Run through the Accessories and add in their availability.
            foreach (WeaponMountOption objLoopOption in WeaponMountOptions)
            {
                AvailabilityValue objLoopAvailTuple
                    = await objLoopOption.GetTotalAvailTupleAsync(token).ConfigureAwait(false);
                //if (objLoopAvailTuple.Item3)
                intAvail += objLoopAvailTuple.Value;
                if (objLoopAvailTuple.Suffix == 'F')
                    chrLastAvailChar = 'F';
                else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                    chrLastAvailChar = 'R';
            }

            if (blnCheckChildren)
            {
                // Run through the Vehicle Mods and add in their availability.
                intAvail += await Mods.SumAsync(async objVehicleMod =>
                {
                    if (objVehicleMod.IncludedInVehicle || !objVehicleMod.Equipped)
                        return 0;
                    AvailabilityValue objLoopAvailTuple
                        = await objVehicleMod.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvailTuple.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvailTuple.AddToParent ? objLoopAvailTuple.Value : 0;
                }, token).ConfigureAwait(false);
            }

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInVehicle);
        }

        /// <summary>
        /// Total cost of the WeaponMount.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                if (IncludedInVehicle || FreeCost)
                    return Weapons.Sum(w => w.TotalCost) + Mods.Sum(m => m.TotalCost);
                
                decimal decOptionCost = WeaponMountOptions.Sum(w => w.TotalCost);
                if (DiscountCost)
                    decOptionCost *= 0.9m;

                // Apply a markup if applicable.
                if (_decMarkup != 0)
                {
                    decOptionCost *= 1 + _decMarkup / 100.0m;
                }

                return OwnCost + decOptionCost + Weapons.Sum(w => w.TotalCost) + Mods.Sum(m => m.TotalCost);
            }
        }

        /// <summary>
        /// Total cost of the WeaponMount.
        /// </summary>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            if (IncludedInVehicle || FreeCost)
                return await Weapons.SumAsync(w => w.GetTotalCostAsync(token), token).ConfigureAwait(false)
                       + await Mods.SumAsync(m => m.GetTotalCostAsync(token), token).ConfigureAwait(false);

            decimal decOptionCost = 0;
            foreach (WeaponMountOption objOption in WeaponMountOptions)
                decOptionCost += await objOption.GetTotalCostAsync(token).ConfigureAwait(false);
            if (DiscountCost)
                decOptionCost *= 0.9m;

            // Apply a markup if applicable.
            if (_decMarkup != 0)
            {
                decOptionCost *= 1 + _decMarkup / 100.0m;
            }

            return await GetOwnCostAsync(token).ConfigureAwait(false)
                   + decOptionCost
                   + await Weapons.SumAsync(w => w.GetTotalCostAsync(token), token).ConfigureAwait(false)
                   + await Mods.SumAsync(m => m.GetTotalCostAsync(token), token).ConfigureAwait(false);
        }

        public decimal StolenTotalCost => CalculatedStolenTotalCost(true);

        public decimal NonStolenTotalCost => CalculatedStolenTotalCost(false);

        public decimal CalculatedStolenTotalCost(bool blnStolen)
        {
            decimal d = 0;

            if (Stolen == blnStolen)
            {
                if (!IncludedInVehicle)
                    d += OwnCost;
                decimal decOptionCost = 0;
                if (!FreeCost)
                {
                    decOptionCost = WeaponMountOptions.Sum(w => w.TotalCost);
                    if (DiscountCost)
                        decOptionCost *= 0.9m;

                    // Apply a markup if applicable.
                    if (_decMarkup != 0)
                    {
                        decOptionCost *= 1 + _decMarkup / 100.0m;
                    }
                }
                d += decOptionCost;
            }

            d += Weapons.Sum(w => w.CalculatedStolenTotalCost(blnStolen)) + Mods.Sum(m => m.CalculatedStolenTotalCost(blnStolen));

            return d;
        }

        public Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(true, token);

        public Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(false, token);

        public async Task<decimal> CalculatedStolenTotalCostAsync(bool blnStolen, CancellationToken token = default)
        {
            if (Stolen != blnStolen || IncludedInVehicle || FreeCost)
                return await Weapons.SumAsync(w => w.CalculatedStolenTotalCostAsync(blnStolen, token),
                                              token).ConfigureAwait(false)
                       + await Mods.SumAsync(m => m.CalculatedStolenTotalCostAsync(blnStolen, token), token).ConfigureAwait(false);
            
            decimal decOptionCost = 0;
            foreach (WeaponMountOption objOption in WeaponMountOptions)
                decOptionCost += await objOption.GetTotalCostAsync(token).ConfigureAwait(false);
            if (DiscountCost)
                decOptionCost *= 0.9m;

            // Apply a markup if applicable.
            if (_decMarkup != 0)
            {
                decOptionCost *= 1 + _decMarkup / 100.0m;
            }

            return await GetOwnCostAsync(token).ConfigureAwait(false) + decOptionCost
                                                                      + await Weapons.SumAsync(
                                                                          w => w.CalculatedStolenTotalCostAsync(blnStolen, token),
                                                                          token).ConfigureAwait(false) + await Mods.SumAsync(
                                                                          m => m.CalculatedStolenTotalCostAsync(blnStolen, token),
                                                                          token).ConfigureAwait(false);
        }

        /// <summary>
        /// The cost of just the Vehicle Mod itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                if (FreeCost)
                    return 0;
                decimal decReturn = 0;
                // If the cost is determined by the Rating, evaluate the expression.
                string strCost = Cost;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCost);
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdCost, strCost);

                    sbdCost.CheapReplace(strCost, "Vehicle Cost",
                                         () => Parent?.OwnCost.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                    sbdCost.CheapReplace(strCost, "Body",
                                         () => Parent?.Body > 0
                                             ? Parent.Body.ToString(GlobalSettings.InvariantCultureInfo)
                                             : "0.5");
                    sbdCost.CheapReplace(strCost, "Armor",
                                         () => Parent?.Armor.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    sbdCost.CheapReplace(strCost, "Speed",
                                         () => Parent?.Speed.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    sbdCost.CheapReplace(strCost, "Acceleration",
                                         () => Parent?.Accel.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    sbdCost.CheapReplace(strCost, "Handling",
                                         () => Parent?.Handling.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    sbdCost.CheapReplace(strCost, "Sensor",
                                         () => Parent?.BaseSensor.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");
                    sbdCost.CheapReplace(strCost, "Pilot",
                                         () => Parent?.Pilot.ToString(GlobalSettings.InvariantCultureInfo) ?? "0");

                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Apply a markup if applicable.
                if (_decMarkup != 0)
                {
                    decReturn *= 1 + _decMarkup / 100.0m;
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Vehicle Mod itself.
        /// </summary>
        public async Task<decimal> GetOwnCostAsync(CancellationToken token = default)
        {
            if (FreeCost)
                return 0;
            decimal decReturn = 0;
            // If the cost is determined by the Rating, evaluate the expression.
            string strCost = Cost;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
            {
                sbdCost.Append(strCost);
                await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdCost, strCost, token: token).ConfigureAwait(false);

                await sbdCost.CheapReplaceAsync(strCost, "Vehicle Cost",
                                                async () => Parent != null
                                                    ? (await Parent.GetOwnCostAsync(token).ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo)
                                                    : "0", token: token).ConfigureAwait(false);
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                await sbdCost.CheapReplaceAsync(strCost, "Body",
                                                () => Parent?.Body > 0
                                                    ? Parent.Body.ToString(GlobalSettings.InvariantCultureInfo)
                                                    : "0.5", token: token).ConfigureAwait(false);
                await sbdCost.CheapReplaceAsync(strCost, "Armor",
                                                () => Parent?.Armor.ToString(GlobalSettings.InvariantCultureInfo)
                                                      ?? "0", token: token).ConfigureAwait(false);
                await sbdCost.CheapReplaceAsync(strCost, "Speed",
                                                () => Parent?.Speed.ToString(GlobalSettings.InvariantCultureInfo)
                                                      ?? "0", token: token).ConfigureAwait(false);
                await sbdCost.CheapReplaceAsync(strCost, "Acceleration",
                                                () => Parent?.Accel.ToString(GlobalSettings.InvariantCultureInfo)
                                                      ?? "0", token: token).ConfigureAwait(false);
                await sbdCost.CheapReplaceAsync(strCost, "Handling",
                                                () => Parent?.Handling.ToString(GlobalSettings.InvariantCultureInfo)
                                                      ?? "0", token: token).ConfigureAwait(false);
                await sbdCost.CheapReplaceAsync(strCost, "Sensor",
                                                () => Parent?.BaseSensor.ToString(GlobalSettings.InvariantCultureInfo)
                                                      ?? "0", token: token).ConfigureAwait(false);
                await sbdCost.CheapReplaceAsync(strCost, "Pilot",
                    async () => Parent != null
                        ? (await Parent.GetPilotAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo)
                        : "0", token: token).ConfigureAwait(false);

                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(sbdCost.ToString(), token).ConfigureAwait(false);
                if (blnIsSuccess)
                    decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
            }

            if (DiscountCost)
                decReturn *= 0.9m;

            // Apply a markup if applicable.
            if (_decMarkup != 0)
            {
                decReturn *= 1 + _decMarkup / 100.0m;
            }

            return decReturn;
        }

        public TaggedObservableCollection<VehicleMod> Mods => _lstMods;

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
                // we instead display them as if they were one of the CRB mounts, but give them a different name
                if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                    return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("name")?.Value ?? Name;
                return Name;
            }

            return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            XPathNavigator xmlDataNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            if (xmlDataNode == null)
                return Name;
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
                // we instead display them as if they were one of the CRB mounts, but give them a different name
                if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
                    return xmlDataNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? Name;
                return Name;
            }

            return xmlDataNode.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? Name;
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            if (WeaponMountOptions.Count > 0)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                {
                    sbdReturn.Append(strReturn);
                    string strSpace = LanguageManager.GetString("String_Space", strLanguage);
                    sbdReturn.Append(strSpace).Append('(');
                    bool blnCloseParantheses = false;
                    foreach (WeaponMountOption objOption in WeaponMountOptions)
                    {
                        if (objOption.Name != "None")
                        {
                            blnCloseParantheses = true;
                            sbdReturn.Append(objOption.DisplayName(strLanguage)).Append(',').Append(strSpace);
                        }
                    }

                    sbdReturn.Length -= 1 + strSpace.Length;
                    if (blnCloseParantheses)
                        sbdReturn.Append(')');
                    if (!string.IsNullOrWhiteSpace(Location))
                        sbdReturn.Append(strSpace).Append('-').Append(strSpace).Append(Location);
                    strReturn = sbdReturn.ToString();
                }
            }

            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public async Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            if (WeaponMountOptions.Count > 0)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                {
                    sbdReturn.Append(strReturn);
                    string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
                    sbdReturn.Append(strSpace).Append('(');
                    bool blnCloseParantheses = false;
                    foreach (WeaponMountOption objOption in WeaponMountOptions)
                    {
                        if (objOption.Name != "None")
                        {
                            blnCloseParantheses = true;
                            sbdReturn.Append(await objOption.DisplayNameAsync(strLanguage, token).ConfigureAwait(false)).Append(',').Append(strSpace);
                        }
                    }

                    sbdReturn.Length -= 1 + strSpace.Length;
                    if (blnCloseParantheses)
                        sbdReturn.Append(')');
                    if (!string.IsNullOrWhiteSpace(Location))
                        sbdReturn.Append(strSpace).Append('-').Append(strSpace).Append(Location);
                    strReturn = sbdReturn.ToString();
                }
            }

            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
            // we instead display them as if they were one of the CRB mounts, but give them a different name
            if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
            {
                string strOverrideId = AllowedWeaponCategories.ContainsAny("Machine Guns", "Launchers", "Cannons")
                    // Id for Heavy [SR5] mount
                    ? "a567c5d3-38b8-496a-add8-1e176384e935"
                    // Id for Standard [SR5] mount
                    : "079a5c61-aee6-4383-81b7-32540f7a0a0b";
                XmlNode xmlOverrideDataNode
                    = (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData("vehicles.xml", strLanguage, token: token)
                        : await _objCharacter.LoadDataAsync("vehicles.xml", strLanguage, token: token).ConfigureAwait(false))
                    .TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strOverrideId);
                if (xmlOverrideDataNode != null)
                    return xmlOverrideDataNode; // Do not cache this node
            }

            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("vehicles.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("vehicles.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/weaponmounts/weaponmount", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", Name);
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
            // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
            // we instead display them as if they were one of the CRB mounts, but give them a different name
            if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
            {
                string strOverrideId = AllowedWeaponCategories.ContainsAny("Machine Guns", "Launchers", "Cannons")
                    // Id for Heavy [SR5] mount
                    ? "a567c5d3-38b8-496a-add8-1e176384e935"
                    // Id for Standard [SR5] mount
                    : "079a5c61-aee6-4383-81b7-32540f7a0a0b";
                XPathNavigator xmlOverrideDataNode
                    = (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadDataXPath("vehicles.xml", strLanguage, token: token)
                        : await _objCharacter.LoadDataXPathAsync("vehicles.xml", strLanguage, token: token).ConfigureAwait(false))
                    .TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", strOverrideId);
                if (xmlOverrideDataNode != null)
                    return xmlOverrideDataNode; // Do not cache this node
            }

            XPathNavigator objReturn = _objCachedMyXPathNode;
            if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath("vehicles.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("vehicles.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/weaponmounts/weaponmount", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        #endregion Complex Properties

        #region Methods

        public decimal DeleteWeaponMount(bool blnDoRemoval = true)
        {
            if (blnDoRemoval)
                Parent.WeaponMounts.Remove(this);

            decimal decReturn = Weapons.AsEnumerableWithSideEffects().Sum(x => x.DeleteWeapon(false))
                                + Mods.AsEnumerableWithSideEffects().Sum(x => x.DeleteVehicleMod(false));

            DisposeSelf();

            return decReturn;
        }

        public async Task<decimal> DeleteWeaponMountAsync(bool blnDoRemoval = true,
                                                               CancellationToken token = default)
        {
            if (blnDoRemoval)
                await Parent.WeaponMounts.RemoveAsync(this, token).ConfigureAwait(false);

            decimal decReturn = await Weapons.SumWithSideEffectsAsync(x => x.DeleteWeaponAsync(false, token), token)
                                             .ConfigureAwait(false)
                                + await Mods.SumWithSideEffectsAsync(x => x.DeleteVehicleModAsync(false, token), token)
                                            .ConfigureAwait(false);

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

            intRestrictedCount += await Weapons.SumAsync(objChild =>
                                                             objChild.CheckRestrictedGear(
                                                                 dicRestrictedGearLimits, sbdAvailItems,
                                                                 sbdRestrictedItems, token), token)
                                               .ConfigureAwait(false);

            foreach (WeaponMountOption objChild in WeaponMountOptions)
            {
                intRestrictedCount += await objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, token).ConfigureAwait(false);
            }

            return intRestrictedCount;
        }

        #region UI Methods

        /// <summary>
        /// Add a Weapon Mount to the TreeView
        /// </summary>
        /// <param name="cmsVehicleWeapon">ContextMenuStrip for Vehicle Weapons</param>
        /// <param name="cmsVehicleWeaponAccessory">ContextMenuStrip for Vehicle Weapon Accessories</param>
        /// <param name="cmsVehicleWeaponAccessoryGear">ContextMenuStrip for Vehicle Weapon Gear</param>
        /// <param name="cmsVehicleWeaponMount">ContextMenuStrip for Vehicle Weapon Mounts</param>
        /// <param name="cmsCyberware">ContextMenuStrip for Cyberware.</param>
        /// <param name="cmsCyberwareGear">ContextMenuStrip for Gear in Cyberware.</param>
        /// <param name="cmsVehicleMod">ContextMenuStrip for Vehicle Mods.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleMod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // Because of the weird way in which weapon mounts work with and without Rigger 5.0, instead of hiding built-in mounts from disabled sourcebooks,
            // we instead display them as if they were one of the CRB mounts, but give them a different name.

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                Tag = this,
                ContextMenuStrip = cmsVehicleWeaponMount,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // VehicleMods.
            await Mods.ForEachAsync(async objMod =>
            {
                TreeNode objLoopNode = await objMod.CreateTreeNode(cmsVehicleMod, cmsCyberware, cmsCyberwareGear,
                    cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear, token).ConfigureAwait(false);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }, token).ConfigureAwait(false);
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

        #endregion UI Methods

        #endregion Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeaponMount")))
                return false;

            DeleteWeaponMount();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteWeaponMount", token: token)
                            .ConfigureAwait(false), token).ConfigureAwait(false))
                return false;

            await DeleteWeaponMountAsync(token: token).ConfigureAwait(false);
            return true;
        }

        public bool Sell(decimal decPercentage, bool blnConfirmDelete)
        {
            if (!_objCharacter.Created)
                return Remove(blnConfirmDelete);

            if (blnConfirmDelete &&
                !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeaponMount")))
                return false;

            // Record the cost of the Vehicle with the Weapon Mount.
            Vehicle objParent = Parent;
            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = objParent.TotalCost;
                decAmount = DeleteWeaponMount() * decPercentage;
                decAmount += (decOriginal - objParent.TotalCost) * decPercentage;
            }
            else
            {
                decimal decOriginal = TotalCost;
                decAmount = (DeleteWeaponMount() + decOriginal) * decPercentage;
            }

            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount,
                LanguageManager.GetString("String_ExpenseSoldVehicleWeaponMount") + ' ' + CurrentDisplayNameShort,
                ExpenseType.Nuyen, DateTime.Now);
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
                        await LanguageManager.GetStringAsync("Message_DeleteWeaponMount", token: token).ConfigureAwait(false),
                        token).ConfigureAwait(false))
                return false;

            Vehicle objParent = Parent;
            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = await objParent.GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = await DeleteWeaponMountAsync(token: token).ConfigureAwait(false) * decPercentage;
                decAmount += (decOriginal - await objParent.GetTotalCostAsync(token).ConfigureAwait(false)) * decPercentage;
            }
            else
            {
                decimal decOriginal = await GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = (await DeleteWeaponMountAsync(token: token).ConfigureAwait(false) + decOriginal) * decPercentage;
            }

            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount,
                await LanguageManager.GetStringAsync("String_ExpenseSoldVehicleWeaponMount", token: token).ConfigureAwait(false) +
                ' ' + await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), ExpenseType.Nuyen,
                DateTime.Now);
            await _objCharacter.ExpenseEntries.AddWithSortAsync(objExpense, token: token).ConfigureAwait(false);
            await _objCharacter.ModifyNuyenAsync(decAmount, token).ConfigureAwait(false);
            return true;
        }

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
            IAsyncDisposable objLocker = await GlobalSettings.EnterClipboardReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                switch (await GlobalSettings.GetClipboardContentTypeAsync(token).ConfigureAwait(false))
                {
                    case ClipboardContentType.Weapon:
                    {
                        if (!string.IsNullOrEmpty(AllowedWeapons))
                        {
                            string strCheckValue = (await GlobalSettings.GetClipboardAsync(token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpressionAsNavigator("name", token)?.Value ?? string.Empty;
                            if (string.IsNullOrEmpty(strCheckValue) || !AllowedWeapons.Contains(strCheckValue))
                                return false;
                        }

                        if (!string.IsNullOrEmpty(AllowedWeaponCategories))
                        {
                            string strCheckValue = (await GlobalSettings.GetClipboardAsync(token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpressionAsNavigator("category", token)?.Value ?? string.Empty;
                            if (string.IsNullOrEmpty(strCheckValue) || !AllowedWeaponCategories.Contains(strCheckValue))
                                return false;
                        }

                        return await GetIsWeaponsFullAsync(token).ConfigureAwait(false);
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
            foreach (Weapon objChild in _lstWeapons)
                objChild.Dispose();
            foreach (VehicleMod objChild in _lstMods)
                objChild.Dispose();
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstWeapons.Dispose();
            _lstMods.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            foreach (Weapon objChild in _lstWeapons)
                await objChild.DisposeAsync().ConfigureAwait(false);
            foreach (VehicleMod objChild in _lstMods)
                await objChild.DisposeAsync().ConfigureAwait(false);
            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private async ValueTask DisposeSelfAsync()
        {
            await _lstWeapons.DisposeAsync().ConfigureAwait(false);
            await _lstMods.DisposeAsync().ConfigureAwait(false);
        }
    }

    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public class WeaponMountOption : IHasName, IHasXmlDataNode, IHasCost, IHasCharacterObject
    {
        private readonly Character _objCharacter;
        private string _strAvail;
        private string _strName;
        private Guid _guiSourceID;
        private Guid _guiID;
        private string _strCost;
        private string _strCategory;
        private int _intSlots;
        private string _strAllowedWeaponCategories;
        private string _strAllowedWeapons;
        private bool _blnIncludedInParent;

        #region Constructor, Create, Save and Load Methods

        public WeaponMountOption(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a Weapon Mount Option from an XmlNode, returns true if creation was successful.
        /// </summary>
        /// <param name="objXmlMod">XmlNode of the option.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public bool Create(XmlNode objXmlMod, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlMod, token), token);
        }

        /// <summary>
        /// Create a Weapon Mount Option from an XmlNode, returns true if creation was successful.
        /// </summary>
        /// <param name="objXmlMod">XmlNode of the option.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task<bool> CreateAsync(XmlNode objXmlMod, CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlMod, token);
        }

        private async Task<bool> CreateCoreAsync(bool blnSync, XmlNode objXmlMod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objXmlMod == null)
            {
                Utils.BreakIfDebug();
                return false;
            }

            _guiID = Guid.NewGuid();
            objXmlMod.TryGetField("id", Guid.TryParse, out _guiSourceID);
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objXmlMod.TryGetStringFieldQuickly("weaponcategories", ref _strAllowedWeaponCategories);
            objXmlMod.TryGetStringFieldQuickly("weapons", ref _strAllowedWeapons);
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);

            // Check for a Variable Cost.
            // ReSharper disable once PossibleNullReferenceException
            _strCost = objXmlMod["cost"]?.InnerText ?? "0";
            if (_strCost.StartsWith("Variable(", StringComparison.Ordinal))
            {
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                string strCost = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                if (strCost.Contains('-'))
                {
                    string[] strValues = strCost.Split('-');
                    decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                    decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                }
                else
                    decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

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
                                return false;
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
                        using (ThreadSafeForm<SelectNumber> frmPickNumber
                               = await ThreadSafeForm<SelectNumber>.GetAsync(() => new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
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
                                return false;
                            }
                            _strCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                        }
                    }
                }
            }
            return true;
        }

        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        public string DisplayName(string strLanguage)
        {
            return DisplayNameShort(strLanguage);
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            return objNode?.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? Name;
        }

        public Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            return DisplayNameShortAsync(strLanguage, token);
        }

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("weaponmountoption");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalID);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("slots", _intSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("includedinparent", _blnIncludedInParent.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Weapon Mount Option from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strAllowedWeaponCategories);
            objNode.TryGetStringFieldQuickly("allowedweapons", ref _strAllowedWeapons);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetBoolFieldQuickly("includedinparent", ref _blnIncludedInParent);
        }

        #endregion Constructor, Create, Save and Load Methods

        #region Properties

        public Character CharacterObject => _objCharacter;

        public string InternalID => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value)
                    return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// The cost of just the WeaponMountOption itself.
        /// </summary>
        public decimal Cost
        {
            get
            {
                string strCost = _strCost;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCost);
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdCost, strCost);

                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString());
                    return blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) : 0;
                }
            }
        }

        /// <summary>
        /// The cost of just the WeaponMountOption itself.
        /// </summary>
        public async Task<decimal> GetCostAsync(CancellationToken token = default)
        {
            string strCost = _strCost;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
            {
                sbdCost.Append(strCost);
                await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdCost, strCost, token: token).ConfigureAwait(false);

                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(sbdCost.ToString(), token).ConfigureAwait(false);
                return blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) : 0;
            }
        }

        public decimal TotalCost => IncludedInParent ? 0 : Cost;

        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default) => IncludedInParent ? 0 : await GetCostAsync(token).ConfigureAwait(false);

        /// <summary>
        /// Slots consumed by the WeaponMountOption.
        /// </summary>
        public int Slots => _intSlots;

        /// <summary>
        /// Availability string of the Mount.
        /// </summary>
        public string Avail => _strAvail;

        /// <summary>
        /// Category of the weapon mount.
        /// </summary>
        public string Category => _strCategory;

        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Does the option come with the parent object?
        /// </summary>
        public bool IncludedInParent
        {
            get => _blnIncludedInParent;
            set => _blnIncludedInParent = value;
        }

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// Display text for the category of the weapon mount.
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
            return TotalAvailTuple.ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Calculated Availability of the Vehicle.
        /// </summary>
        public async Task<string> TotalAvailAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return await (await GetTotalAvailTupleAsync(token: token).ConfigureAwait(false)).ToStringAsync(objCulture, strLanguage, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple
        {
            get
            {
                bool blnModifyParentAvail = false;
                string strAvail = Avail;
                char chrLastAvailChar = ' ';
                int intAvail = 0;
                if (strAvail.Length > 0)
                {
                    chrLastAvailChar = strAvail[strAvail.Length - 1];
                    if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                    {
                        strAvail = strAvail.Substring(0, strAvail.Length - 1);
                    }

                    blnModifyParentAvail = strAvail.StartsWith('+', '-');

                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));
                        _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdAvail, strAvail);

                        (bool blnIsSuccess, object objProcess)
                            = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString());
                        if (blnIsSuccess)
                            intAvail += ((double)objProcess).StandardRound();
                    }
                }

                return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
            }
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async Task<AvailabilityValue> GetTotalAvailTupleAsync(CancellationToken token = default)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Avail;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            if (strAvail.Length > 0)
            {
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                {
                    sbdAvail.Append(strAvail.TrimStart('+'));
                    await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdAvail, strAvail, token: token).ConfigureAwait(false);

                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
            }

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
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
                ? _objCharacter.LoadData("vehicles.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("vehicles.xml", strLanguage, token: token)
                                     .ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/weaponmounts/weaponmount", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", Name);
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
                : await _objCharacter.LoadDataXPathAsync("vehicles.xml", strLanguage, token: token)
                                     .ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/weaponmounts/weaponmount", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/weaponmounts/weaponmount", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        #endregion Complex Properties

        #region Methods

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
            if (!IncludedInParent)
            {
                AvailabilityValue objTotalAvail = await GetTotalAvailTupleAsync(token).ConfigureAwait(false);
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

                        if (intLowestValidRestrictedGearAvail >= 0
                            && dicRestrictedGearLimits[intLowestValidRestrictedGearAvail] > 0)
                        {
                            --dicRestrictedGearLimits[intLowestValidRestrictedGearAvail];
                            sbdRestrictedItems.AppendLine().Append("\t\t").Append(await GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                        }
                        else
                        {
                            dicRestrictedGearLimits.Remove(intLowestValidRestrictedGearAvail);
                            ++intRestrictedCount;
                            sbdAvailItems.AppendLine().Append("\t\t").Append(await GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                        }
                    }
                }
            }

            return intRestrictedCount;
        }

        #endregion Methods
    }
}
