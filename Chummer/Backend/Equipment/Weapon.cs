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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
using Microsoft.VisualStudio.Threading;
using NLog;
using IAsyncDisposable = System.IAsyncDisposable;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A Weapon.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", null)]
    [DebuggerDisplay("{DisplayName(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage)}")]
    public sealed class Weapon : IHasChildren<Weapon>, IHasName, IHasSourceId, IHasInternalId, IHasXmlDataNode, IHasMatrixAttributes, IHasNotes, ICanSell, IHasCustomName, IHasLocation, ICanEquip, IHasSource, ICanSort, IHasWirelessBonus, IHasStolenProperty, ICanPaste, IHasRating, ICanBlackMarketDiscount, IDisposable, IAsyncDisposable
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private int _intReach;
        private int _intAmmoSlots = 1;
        private string _strDamage = string.Empty;
        private string _strAP = "0";
        private string _strMode = string.Empty;
        private string _strRC = string.Empty;
        private string _strAmmo = string.Empty;
        private string _strAmmoCategory = string.Empty;
        private string _strWeaponType = string.Empty;
        private int _intConceal;
        private readonly List<Clip> _lstAmmo = new List<Clip>(1);

        //private int _intAmmoRemaining = 0;
        //private int _intAmmoRemaining2 = 0;
        //private int _intAmmoRemaining3 = 0;
        //private int _intAmmoRemaining4 = 0;
        //private Guid _guiAmmoLoaded = Guid.Empty;
        //private Guid _guiAmmoLoaded2 = Guid.Empty;
        //private Guid _guiAmmoLoaded3 = Guid.Empty;
        //private Guid _guiAmmoLoaded4 = Guid.Empty;
        private int _intActiveAmmoSlot = 1;

        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strWeight = string.Empty;
        private string _strRange = string.Empty;
        private string _strAlternateRange = string.Empty;
        private decimal _decRangeMultiplier = 1;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strWeaponName = string.Empty;
        private int _intSingleShot = 1;
        private int _intShortBurst = 3;
        private int _intLongBurst = 6;
        private int _intFullBurst = 10;
        private int _intSuppressive = 20;
        private readonly TaggedObservableCollection<WeaponAccessory> _lstAccessories = new TaggedObservableCollection<WeaponAccessory>();
        private readonly TaggedObservableCollection<Weapon> _lstUnderbarrel = new TaggedObservableCollection<Weapon>();
        private Vehicle _objMountedVehicle;
        private WeaponMount _objWeaponMount;
        private VehicleMod _objVehicleMod;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private string _strUseSkill = string.Empty;
        private string _strUseSkillSpec = string.Empty;
        private Location _objLocation;
        private string _strSpec = string.Empty;
        private string _strSpec2 = string.Empty;
        private bool _blnIncludedInWeapon;
        private bool _blnEquipped = true;
        private bool _blnDiscountCost;
        private bool _blnRequireAmmo = true;
        private string _strAccuracy = string.Empty;
        private string _strRCTip = string.Empty;
        private string _strWeaponSlots = string.Empty;
        private string _strDoubledCostWeaponSlots = string.Empty;
        private bool _blnCyberware;
        private string _strParentID = string.Empty;
        private bool _blnAllowAccessory = true;
        private Weapon _objParent;
        private string _strSizeCategory;
        private int _intRating;
        private string _strMinRating;
        private string _strMaxRating;
        private string _strRatingLabel = "String_Rating";

        private XmlNode _nodWirelessBonus;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private FiringMode _eFiringMode = FiringMode.DogBrain;

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
        private bool _blnCanSwapAttributes;
        private bool _blnWirelessOn = true;
        private int _intMatrixCMFilled;
        private int _intSortOrder;
        private bool _blnStolen;
        private readonly Character _objCharacter;
        private string _strMount;
        private string _strExtraMount;

        private bool _blnAllowSingleShot = true;
        private bool _blnAllowShortBurst = true;
        private bool _blnAllowLongBurst = true;
        private bool _blnAllowFullBurst = true;
        private bool _blnAllowSuppressive = true;

        #region Constructor, Create, Save, Load, and Print Methods

        public Weapon(Character objCharacter)
        {
            // Create the GUID for the new Weapon.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _lstUnderbarrel.AddTaggedCollectionChanged(this, ChildrenOnCollectionChanged);
            _lstAccessories.AddTaggedCollectionChanged(this, AccessoriesOnCollectionChanged);
        }

        private void AccessoriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            bool blnEverDoEncumbranceRefresh = _objCharacter?.IsLoading == false && Equipped && ParentVehicle == null;
            bool blnDoEncumbranceRefresh = false;
            bool blnRecreateInternalClip = false;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (WeaponAccessory objNewItem in e.NewItems)
                    {
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objNewItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objNewItem.Weight)
                                || objNewItem.GearChildren.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                                   x => x.Equipped && !string.IsNullOrEmpty(x.Weight))))
                            blnDoEncumbranceRefresh = true;
                        if (!blnRecreateInternalClip && (!string.IsNullOrWhiteSpace(objNewItem.AmmoReplace) ||
                                                         !string.IsNullOrWhiteSpace(objNewItem.AmmoBonus)))
                        {
                            blnRecreateInternalClip = true;
                        }
                        if (objNewItem.AmmoSlots > 0)
                        {
                            AddAmmoSlots(objNewItem);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (WeaponAccessory objOldItem in e.OldItems)
                    {
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objOldItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objOldItem.Weight)
                                || objOldItem.GearChildren.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                                   x => x.Equipped && !string.IsNullOrEmpty(x.Weight))))
                            blnDoEncumbranceRefresh = true;
                        if (!blnRecreateInternalClip && (!string.IsNullOrWhiteSpace(objOldItem.AmmoReplace) ||
                                                         !string.IsNullOrWhiteSpace(objOldItem.AmmoBonus)))
                        {
                            blnRecreateInternalClip = true;
                        }
                        if (objOldItem.AmmoSlots > 0)
                        {
                            RemoveAmmoSlots(objOldItem);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (WeaponAccessory objOldItem in e.OldItems)
                    {
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objOldItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objOldItem.Weight)
                                || objOldItem.GearChildren.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                                   x => x.Equipped && !string.IsNullOrEmpty(x.Weight))))
                            blnDoEncumbranceRefresh = true;
                        if (!blnRecreateInternalClip && (!string.IsNullOrWhiteSpace(objOldItem.AmmoReplace) ||
                                                         !string.IsNullOrWhiteSpace(objOldItem.AmmoBonus)))
                        {
                            blnRecreateInternalClip = true;
                        }
                        if (objOldItem.AmmoSlots > 0)
                        {
                            RemoveAmmoSlots(objOldItem);
                        }
                    }
                    foreach (WeaponAccessory objNewItem in e.NewItems)
                    {
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objNewItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objNewItem.Weight)
                                || objNewItem.GearChildren.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                                   x => x.Equipped && !string.IsNullOrEmpty(x.Weight))))
                            blnDoEncumbranceRefresh = true;
                        if (!blnRecreateInternalClip && (!string.IsNullOrWhiteSpace(objNewItem.AmmoReplace) ||
                                                         !string.IsNullOrWhiteSpace(objNewItem.AmmoBonus)))
                        {
                            blnRecreateInternalClip = true;
                        }
                        if (objNewItem.AmmoSlots > 0)
                        {
                            AddAmmoSlots(objNewItem);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    blnRecreateInternalClip = true;
                    blnDoEncumbranceRefresh = blnEverDoEncumbranceRefresh;
                    break;
            }

            if (blnRecreateInternalClip)
                RecreateInternalClip();
            if (blnDoEncumbranceRefresh)
                _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            bool blnEverDoEncumbranceRefresh = _objCharacter?.IsLoading == false && Equipped && ParentVehicle == null;
            bool blnDoEncumbranceRefresh = false;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Weapon objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objNewItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objNewItem.Weight)
                                || objNewItem.WeaponAccessories.Any(
                                    x => x.Equipped && (!string.IsNullOrEmpty(x.Weight)
                                                        || x.GearChildren.DeepAny(
                                                            y => y.Children.Where(z => z.Equipped),
                                                            y => y.Equipped && !string.IsNullOrEmpty(y.Weight))))
                                || objNewItem.Children.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                               z => z.Equipped && (!string.IsNullOrEmpty(z.Weight)
                                                                   || WeaponAccessories.Any(
                                                                       x => x.Equipped
                                                                            && (!string.IsNullOrEmpty(x.Weight)
                                                                                || x.GearChildren.DeepAny(
                                                                                    y => y.Children.Where(
                                                                                        t => t.Equipped),
                                                                                    y => y.Equipped
                                                                                        && !string.IsNullOrEmpty(
                                                                                            y.Weight))))))))
                            blnDoEncumbranceRefresh = true;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Weapon objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objOldItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objOldItem.Weight)
                                || objOldItem.WeaponAccessories.Any(
                                    x => x.Equipped && (!string.IsNullOrEmpty(x.Weight)
                                                        || x.GearChildren.DeepAny(
                                                            y => y.Children.Where(z => z.Equipped),
                                                            y => y.Equipped && !string.IsNullOrEmpty(y.Weight))))
                                || objOldItem.Children.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                               z => z.Equipped && (!string.IsNullOrEmpty(z.Weight)
                                                                   || WeaponAccessories.Any(
                                                                       x => x.Equipped
                                                                            && (!string.IsNullOrEmpty(x.Weight)
                                                                                || x.GearChildren.DeepAny(
                                                                                    y => y.Children.Where(
                                                                                        t => t.Equipped),
                                                                                    y => y.Equipped
                                                                                        && !string.IsNullOrEmpty(
                                                                                            y.Weight))))))))
                            blnDoEncumbranceRefresh = true;
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (Weapon objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objOldItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objOldItem.Weight)
                                || objOldItem.WeaponAccessories.Any(
                                    x => x.Equipped && (!string.IsNullOrEmpty(x.Weight)
                                                        || x.GearChildren.DeepAny(
                                                            y => y.Children.Where(z => z.Equipped),
                                                            y => y.Equipped && !string.IsNullOrEmpty(y.Weight))))
                                || objOldItem.Children.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                               z => z.Equipped && (!string.IsNullOrEmpty(z.Weight)
                                                                   || WeaponAccessories.Any(
                                                                       x => x.Equipped
                                                                            && (!string.IsNullOrEmpty(x.Weight)
                                                                                || x.GearChildren.DeepAny(
                                                                                    y => y.Children.Where(
                                                                                        t => t.Equipped),
                                                                                    y => y.Equipped
                                                                                        && !string.IsNullOrEmpty(
                                                                                            y.Weight))))))))
                            blnDoEncumbranceRefresh = true;
                    }
                    foreach (Weapon objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objNewItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objNewItem.Weight)
                                || objNewItem.WeaponAccessories.Any(
                                    x => x.Equipped && (!string.IsNullOrEmpty(x.Weight)
                                                        || x.GearChildren.DeepAny(
                                                            y => y.Children.Where(z => z.Equipped),
                                                            y => y.Equipped && !string.IsNullOrEmpty(y.Weight))))
                                || objNewItem.Children.DeepAny(x => x.Children.Where(y => y.Equipped),
                                                               z => z.Equipped && (!string.IsNullOrEmpty(z.Weight)
                                                                   || WeaponAccessories.Any(
                                                                       x => x.Equipped
                                                                            && (!string.IsNullOrEmpty(x.Weight)
                                                                                || x.GearChildren.DeepAny(
                                                                                    y => y.Children.Where(
                                                                                        t => t.Equipped),
                                                                                    y => y.Equipped
                                                                                        && !string.IsNullOrEmpty(
                                                                                            y.Weight))))))))
                            blnDoEncumbranceRefresh = true;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    blnDoEncumbranceRefresh = blnEverDoEncumbranceRefresh;
                    break;
            }

            this.RefreshMatrixAttributeArray(_objCharacter);
            if (blnDoEncumbranceRefresh)
                _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
        }

        /// <summary>
        /// Create a Weapon from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlWeapon">XmlNode to create the object from.</param>
        /// <param name="lstWeapons">List of child Weapons to generate.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        /// <param name="blnCreateImprovements">Whether or not bonuses should be created.</param>
        /// <param name="blnSkipCost">Whether or not forms asking to determine variable costs should be displayed.</param>
        /// <param name="intRating">Rating of the weapon</param>
        public void Create(XmlNode objXmlWeapon, ICollection<Weapon> lstWeapons, bool blnCreateChildren = true, bool blnCreateImprovements = true, bool blnSkipCost = false, int intRating = 0)
        {
            if (!objXmlWeapon.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for weapon xmlnode", objXmlWeapon });
                Utils.BreakIfDebug();
            }
            else
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            _blnEquipped = blnCreateImprovements;
            objXmlWeapon.TryGetStringFieldQuickly("name", ref _strName);
            objXmlWeapon.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlWeapon.TryGetStringFieldQuickly("type", ref _strType);
            objXmlWeapon.TryGetInt32FieldQuickly("reach", ref _intReach);
            objXmlWeapon.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
            objXmlWeapon.TryGetStringFieldQuickly("damage", ref _strDamage);
            objXmlWeapon.TryGetStringFieldQuickly("ap", ref _strAP);
            objXmlWeapon.TryGetStringFieldQuickly("mode", ref _strMode);
            objXmlWeapon.TryGetStringFieldQuickly("ammo", ref _strAmmo);
            objXmlWeapon.TryGetStringFieldQuickly("mount", ref _strMount);
            objXmlWeapon.TryGetStringFieldQuickly("extramount", ref _strExtraMount);
            objXmlWeapon.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objXmlWeapon.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            if (_strMaxRating == "0")
                _strMaxRating = string.Empty;
            objXmlWeapon.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            if (!objXmlWeapon.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlWeapon.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlWeapon.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            _intRating = Math.Max(Math.Min(intRating, MaxRatingValue), MinRatingValue);
            if (objXmlWeapon["accessorymounts"] != null)
            {
                XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("accessorymounts/mount");
                if (objXmlMountList?.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdMounts))
                    {
                        foreach (XmlNode objXmlMount in objXmlMountList)
                        {
                            sbdMounts.Append(objXmlMount.InnerText).Append('/');
                        }

                        if (sbdMounts.Length > 0)
                            --sbdMounts.Length;
                        _strWeaponSlots = sbdMounts.ToString();
                    }
                }
            }
            if (objXmlWeapon["doubledcostaccessorymounts"] != null)
            {
                XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("doubledcostaccessorymounts/mount");
                if (objXmlMountList?.Count > 0)
                {
                    using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                  out StringBuilder sbdMounts))
                    {
                        foreach (XmlNode objXmlMount in objXmlMountList)
                        {
                            sbdMounts.Append(objXmlMount.InnerText).Append('/');
                        }

                        if (sbdMounts.Length > 0)
                            --sbdMounts.Length;
                        _strDoubledCostWeaponSlots = sbdMounts.ToString();
                    }
                }
            }
            if (!objXmlWeapon.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlWeapon.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            _nodWirelessBonus = objXmlWeapon["wirelessbonus"];
            objXmlWeapon.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn);
            objXmlWeapon.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            if (!objXmlWeapon.TryGetInt32FieldQuickly("ammoslots", ref _intAmmoSlots))
                _intAmmoSlots = 1;
            objXmlWeapon.TryGetStringFieldQuickly("rc", ref _strRC);
            objXmlWeapon.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objXmlWeapon.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlWeapon.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlWeapon.TryGetStringFieldQuickly("weight", ref _strWeight);
            objXmlWeapon.TryGetBoolFieldQuickly("stolen", ref _blnStolen);

            // Check for a Variable Cost.
            if (!blnSkipCost && _strCost.StartsWith("Variable(", StringComparison.Ordinal))
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

                if (blnCreateImprovements)
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
                        using (ThreadSafeForm<SelectNumber> frmPickNumber
                               = ThreadSafeForm<SelectNumber>.Get(() => new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
                               {
                                   Minimum = decMin,
                                   Maximum = decMax,
                                   Description = string.Format(
                                       GlobalSettings.CultureInfo,
                                       LanguageManager.GetString("String_SelectVariableCost"),
                                       CurrentDisplayNameShort),
                                   AllowCancel = false
                               }))
                        {
                            if (frmPickNumber.ShowDialogSafe(_objCharacter) == DialogResult.Cancel)
                            {
                                _guiID = Guid.Empty;
                                return;
                            }
                            _strCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                        }
                    }
                    else
                        _strCost = strFirstHalf;
                }
                else
                    _strCost = strFirstHalf;
            }
            objXmlWeapon.TryGetStringFieldQuickly("sizecategory", ref _strSizeCategory);
            objXmlWeapon.TryGetBoolFieldQuickly("cyberware", ref _blnCyberware);
            objXmlWeapon.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlWeapon.TryGetStringFieldQuickly("page", ref _strPage);

            if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
            {
                Notes = CommonFunctions.GetBookNotes(objXmlWeapon, Name, CurrentDisplayName, Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter);
            }

            XmlDocument objXmlDocument = _objCharacter.LoadData("weapons.xml");

            if (!objXmlWeapon.TryGetStringFieldQuickly("weapontype", ref _strWeaponType))
                _strWeaponType = objXmlDocument
                                 .SelectSingleNodeAndCacheExpressionAsNavigator(
                                     "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@type")?.Value
                                 ?? Category.ToLowerInvariant();

            // Populate the Range if it differs from the Weapon's Category.
            XmlNode objRangeNode = objXmlWeapon["range"];
            if (objRangeNode != null)
            {
                _strRange = objRangeNode.InnerText;
                string strMultiply = objRangeNode.Attributes?["multiply"]?.InnerText;
                if (!string.IsNullOrEmpty(strMultiply))
                    _decRangeMultiplier = Convert.ToDecimal(strMultiply, GlobalSettings.InvariantCultureInfo);
            }
            objXmlWeapon.TryGetStringFieldQuickly("alternaterange", ref _strAlternateRange);

            objXmlWeapon.TryGetInt32FieldQuickly("singleshot", ref _intSingleShot);
            objXmlWeapon.TryGetInt32FieldQuickly("shortburst", ref _intShortBurst);
            objXmlWeapon.TryGetInt32FieldQuickly("longburst", ref _intLongBurst);
            objXmlWeapon.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objXmlWeapon.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);
            objXmlWeapon.TryGetBoolFieldQuickly("allowfullburst", ref _blnAllowFullBurst);
            objXmlWeapon.TryGetBoolFieldQuickly("allowlongburst", ref _blnAllowLongBurst);
            objXmlWeapon.TryGetBoolFieldQuickly("allowshortburst", ref _blnAllowShortBurst);
            objXmlWeapon.TryGetBoolFieldQuickly("allowsingleshot", ref _blnAllowSingleShot);
            objXmlWeapon.TryGetBoolFieldQuickly("allowsuppressive", ref _blnAllowSuppressive);

            objXmlWeapon.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
            objXmlWeapon.TryGetStringFieldQuickly("useskillspec", ref _strUseSkillSpec);
            objXmlWeapon.TryGetBoolFieldQuickly("requireammo", ref _blnRequireAmmo);
            objXmlWeapon.TryGetStringFieldQuickly("spec", ref _strSpec);
            objXmlWeapon.TryGetStringFieldQuickly("spec2", ref _strSpec2);
            objXmlWeapon.TryGetBoolFieldQuickly("allowaccessory", ref _blnAllowAccessory);

            // If the Weapon comes with an Underbarrel Weapon, add it.
            if (blnCreateChildren)
            {
                XmlNodeList xmlUnderbarrelsList = objXmlWeapon["underbarrels"]?.ChildNodes;
                if (xmlUnderbarrelsList?.Count > 0)
                {
                    foreach (XmlNode objXmlUnderbarrel in xmlUnderbarrelsList)
                    {
                        Weapon objUnderbarrelWeapon = new Weapon(_objCharacter);
                        XmlNode objXmlWeaponNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", objXmlUnderbarrel.InnerText);
                        objUnderbarrelWeapon.Create(objXmlWeaponNode, lstWeapons, true, blnCreateImprovements, blnSkipCost);
                        if (!AllowAccessory)
                            objUnderbarrelWeapon.AllowAccessory = false;
                        objUnderbarrelWeapon.ParentID = InternalId;
                        objUnderbarrelWeapon.Cost = "0";
                        objUnderbarrelWeapon.IncludedInWeapon = true;
                        _lstUnderbarrel.Add(objUnderbarrelWeapon);
                    }
                }
            }

            //#1544 Ammunition not loading or available.
            if (_strUseSkill == "Throwing Weapons"
                && _strAmmo != "1")
            {
                _strAmmo = "1";
            }

            objXmlWeapon.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objXmlWeapon.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlWeapon.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlWeapon.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlWeapon.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlWeapon.TryGetStringFieldQuickly("firewall", ref _strFirewall);
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
            objXmlWeapon.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlWeapon.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlWeapon.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlWeapon.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlWeapon.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlWeapon.TryGetStringFieldQuickly("programs", ref _strProgramLimit);

            // create a weapon's initial clips
            CreateClips();

            // If there are any Accessories that come with the Weapon, add them.
            if (blnCreateChildren)
            {
                XmlNodeList objXmlAccessoryList = objXmlWeapon.SelectNodes("accessories/accessory");
                if (objXmlAccessoryList?.Count > 0)
                {
                    foreach (XmlNode objXmlWeaponAccessory in objXmlAccessoryList)
                    {
                        string strName = objXmlWeaponAccessory["name"]?.InnerText;
                        if (string.IsNullOrEmpty(strName))
                            continue;
                        XmlNode objXmlAccessory = objXmlDocument.TryGetNodeByNameOrId("/chummer/accessories/accessory", strName);
                        WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
                        int intAccessoryRating = 0;
                        if (objXmlWeaponAccessory["rating"] != null)
                        {
                            intAccessoryRating = Convert.ToInt32(objXmlWeaponAccessory["rating"].InnerText, GlobalSettings.InvariantCultureInfo);
                        }

                        if (objXmlWeaponAccessory.InnerXml.Contains("mount"))
                        {
                            objAccessory.Create(objXmlAccessory,
                                objXmlWeaponAccessory.InnerXml.Contains("<extramount>")
                                    ? new Tuple<string, string>(objXmlAccessory["mount"].InnerText, objXmlAccessory["extramount"].InnerText)
                                    : new Tuple<string, string>(objXmlAccessory["mount"].InnerText, "None"), intAccessoryRating, false, blnCreateChildren, blnCreateImprovements);
                        }
                        else
                        {
                            objAccessory.Create(objXmlAccessory, new Tuple<string, string>("Internal", "None"), intAccessoryRating, false, blnCreateChildren, blnCreateImprovements);
                        }

                        // Add any extra Gear that comes with the Weapon Accessory.
                        XmlNode xmlGearsNode = objXmlWeaponAccessory["gears"];
                        if (xmlGearsNode != null)
                        {
                            XmlDocument objXmlGearDocument = _objCharacter.LoadData("gear.xml");
                            foreach (XmlNode objXmlAccessoryGear in xmlGearsNode.SelectNodes("usegear"))
                            {
                                XmlNode objXmlAccessoryGearName = objXmlAccessoryGear["name"];
                                XmlNode objXmlAccessoryGearCategory = objXmlAccessoryGear["category"];
                                XmlAttributeCollection objXmlAccessoryGearNameAttributes = objXmlAccessoryGearName.Attributes;
                                int intGearRating = 0;
                                decimal decGearQty = 1;
                                string strChildForceSource = objXmlAccessoryGear["source"]?.InnerText ?? string.Empty;
                                string strChildForcePage = objXmlAccessoryGear["page"]?.InnerText ?? string.Empty;
                                string strChildForceValue = objXmlAccessoryGearNameAttributes?["select"]?.InnerText ?? string.Empty;
                                bool blnChildCreateChildren = objXmlAccessoryGearNameAttributes?["createchildren"]?.InnerText != bool.FalseString;
                                bool blnAddChildImprovements = blnCreateImprovements && objXmlAccessoryGearNameAttributes?["addimprovements"]?.InnerText != bool.FalseString;
                                if (objXmlAccessoryGear["rating"] != null)
                                    intGearRating = Convert.ToInt32(objXmlAccessoryGear["rating"].InnerText, GlobalSettings.InvariantCultureInfo);
                                if (objXmlAccessoryGearNameAttributes?["qty"] != null)
                                    decGearQty = Convert.ToDecimal(objXmlAccessoryGearNameAttributes["qty"].InnerText, GlobalSettings.InvariantCultureInfo);
                                string strFilter = "/chummer/gears/gear";
                                if (objXmlAccessoryGearName != null || objXmlAccessoryGearCategory != null)
                                {
                                    strFilter += '[';
                                    if (objXmlAccessoryGearName != null)
                                    {
                                        strFilter += "name = " + objXmlAccessoryGearName.InnerText.CleanXPath();
                                        if (objXmlAccessoryGearCategory != null)
                                            strFilter += " and category = " + objXmlAccessoryGearCategory.InnerText.CleanXPath();
                                    }
                                    else
                                        strFilter += "category = " + objXmlAccessoryGearCategory.InnerText.CleanXPath();
                                    strFilter += ']';
                                }
                                XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode(strFilter);
                                Gear objGear = new Gear(_objCharacter);

                                objGear.Create(objXmlGear, intGearRating, lstWeapons, strChildForceValue, blnAddChildImprovements, blnChildCreateChildren);

                                objGear.Quantity = decGearQty;
                                objGear.Cost = "0";
                                objGear.ParentID = InternalId;

                                if (!string.IsNullOrEmpty(strChildForceSource))
                                    objGear.Source = strChildForceSource;
                                if (!string.IsNullOrEmpty(strChildForcePage))
                                    objGear.Page = strChildForcePage;
                                objAccessory.GearChildren.Add(objGear);

                                // Change the Capacity of the child if necessary.
                                if (objXmlAccessoryGear["capacity"] != null)
                                    objGear.Capacity = '[' + objXmlAccessoryGear["capacity"].InnerText + ']';
                            }
                        }

                        objAccessory.IncludedInWeapon = true;
                        objAccessory.Parent = this;
                        _lstAccessories.Add(objAccessory);
                    }
                }
            }

            if (blnCreateImprovements)
                RefreshWirelessBonuses();

            // Add Subweapons (not underbarrels) if applicable.
            if (lstWeapons == null)
                return;
            // More than one Weapon can be added, so loop through all occurrences.
            foreach (XmlNode objXmlAddWeapon in objXmlWeapon.SelectNodes("addweapon"))
            {
                string strLoopID = objXmlAddWeapon.InnerText;
                XmlNode objXmlSubWeapon =
                    objXmlDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon",
                        strLoopID);

                if (objXmlSubWeapon != null)
                {
                    Weapon objSubWeapon = new Weapon(_objCharacter)
                    {
                        ParentVehicle = ParentVehicle
                    };
                    int intAddWeaponRating = 0;
                    if (objXmlAddWeapon.Attributes["rating"]?.InnerText != null)
                    {
                        intAddWeaponRating = Convert.ToInt32(objXmlAddWeapon.Attributes["rating"]?.InnerText
                                                                            .CheapReplace(
                                                                                "{Rating}",
                                                                                () => Rating.ToString(
                                                                                    GlobalSettings
                                                                                        .InvariantCultureInfo)),
                                                             GlobalSettings.InvariantCultureInfo);
                    }

                    objSubWeapon.Create(objXmlSubWeapon, lstWeapons, blnCreateChildren, blnCreateImprovements,
                                        blnSkipCost, intAddWeaponRating);
                    objSubWeapon.ParentID = InternalId;
                    objSubWeapon.Cost = "0";
                    lstWeapons.Add(objSubWeapon);
                }
            }
            foreach (Weapon objLoopWeapon in lstWeapons)
                objLoopWeapon.ParentVehicle = ParentVehicle;
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = SourceString.GetSourceString(Source, DisplayPage(GlobalSettings.Language),
                        GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                return _objCachedSourceDetail;
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
            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("spec", _strSpec);
            objWriter.WriteElementString("spec2", _strSpec2);
            objWriter.WriteElementString("reach", _intReach.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("damage", _strDamage);
            objWriter.WriteElementString("ap", _strAP);
            objWriter.WriteElementString("mode", _strMode);
            objWriter.WriteElementString("rc", _strRC);
            objWriter.WriteElementString("ammo", _strAmmo);
            objWriter.WriteElementString("cyberware", _blnCyberware.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("ammocategory", _strAmmoCategory);
            objWriter.WriteElementString("ammoslots", _intAmmoSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sizecategory", _strSizeCategory);
            objWriter.WriteElementString("firingmode", _eFiringMode.ToString());
            objWriter.WriteElementString("minrating", _strMinRating);
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("accuracy", _strAccuracy);

            objWriter.WriteElementString("activeammoslot", _intActiveAmmoSlot.ToString(GlobalSettings.InvariantCultureInfo));
            if (_lstAmmo.Any(x => x.AmmoGear != null || x.Ammo != 0))
            {
                objWriter.WriteStartElement("clips");
                foreach (Clip clip in _lstAmmo)
                {
                    clip.Save(objWriter);
                }

                objWriter.WriteEndElement();
            }

            objWriter.WriteElementString("conceal", _intConceal.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("weight", _strWeight);
            objWriter.WriteElementString("useskill", _strUseSkill);
            objWriter.WriteElementString("useskillspec", _strUseSkillSpec);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("alternaterange", _strAlternateRange);
            objWriter.WriteElementString("rangemultiply", _decRangeMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("singleshot", _intSingleShot.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("shortburst", _intShortBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("longburst", _intLongBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("fullburst", _intFullBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("suppressive", _intSuppressive.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowsingleshot", _blnAllowSingleShot.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowshortburst", _blnAllowShortBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowlongburst", _blnAllowLongBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowfullburst", _blnAllowFullBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowsuppressive", _blnAllowSuppressive.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("allowaccessory", _blnAllowAccessory.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("weaponname", _strWeaponName);
            objWriter.WriteElementString("included", _blnIncludedInWeapon.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("requireammo", _blnRequireAmmo.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("accuracy", _strAccuracy);
            objWriter.WriteElementString("mount", _strMount);
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("extramount", _strExtraMount);
            if (_lstAccessories.Count > 0)
            {
                objWriter.WriteStartElement("accessories");
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                    objAccessory.Save(objWriter);
                objWriter.WriteEndElement();
            }
            if (_lstUnderbarrel.Count > 0)
            {
                foreach (Weapon objUnderbarrel in _lstUnderbarrel)
                {
                    objWriter.WriteStartElement("underbarrel");
                    objUnderbarrel.Save(objWriter);
                    objWriter.WriteEndElement();
                }
            }
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("weaponslots", _strWeaponSlots);
            objWriter.WriteElementString("doubledcostweaponslots", _strDoubledCostWeaponSlots);

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
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("weapontype", _strWeaponType);
            objWriter.WriteEndElement();
        }

        private static readonly ReadOnlyCollection<string> s_OldClipValues = Array.AsReadOnly(new[]
            {string.Empty, "2", "3", "4"});

        /// <summary>
        /// Recreates the single internal clip used by weapons that have an ammo capacity but do not require ammo (i.e. they use charges)
        /// </summary>
        private void RecreateInternalClip()
        {
            if (RequireAmmo || string.IsNullOrWhiteSpace(Ammo) || Ammo == "0")
                return;

            int intAmmoCount = 0;
            if (_lstAmmo.Count > 0)
            {
                Clip objCurrentClip = GetClip(_intActiveAmmoSlot);
                if (objCurrentClip != null)
                    intAmmoCount = objCurrentClip.Ammo;
            }

            _lstAmmo.Clear();
            _intActiveAmmoSlot = 1;

            // First try to get the max ammo capacity for this weapon because that will be the capacity of the internal clip
            List<string> lstCount = new List<string>(1);
            string ammoString = CalculatedAmmo(GlobalSettings.CultureInfo, GlobalSettings.DefaultLanguage);
            // Determine which loading methods are available to the Weapon.
            if (ammoString.IndexOfAny('x', '+') != -1 ||
                ammoString.Contains(" or ", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("Special", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("External Source", StringComparison.OrdinalIgnoreCase))
            {
                string strWeaponAmmo = ammoString.FastEscape("External Source", StringComparison.OrdinalIgnoreCase);
                strWeaponAmmo = strWeaponAmmo.ToLowerInvariant();
                // Get rid of or belt, and + energy.
                strWeaponAmmo = strWeaponAmmo.FastEscapeOnceFromEnd(" + energy")
                    .Replace(" or belt", " or 250(belt)");

                foreach (string strAmmo in strWeaponAmmo.SplitNoAlloc(" or ", StringSplitOptions.RemoveEmptyEntries))
                {
                    lstCount.Add(AmmoCapacity(strAmmo));
                }
            }
            else
            {
                // Nothing weird in the ammo string, so just use the number given.
                string strAmmo = ammoString;
                int intPos = strAmmo.IndexOf('(');
                if (intPos != -1)
                    strAmmo = strAmmo.Substring(0, intPos);
                lstCount.Add(strAmmo);
            }

            foreach (string strAmmo in lstCount)
            {
                if (int.TryParse(strAmmo, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intNewAmmoCount))
                {
                    if (intAmmoCount > intNewAmmoCount)
                        intAmmoCount = intNewAmmoCount;
                    break;
                }
            }

            Clip objInternalClip = new Clip(_objCharacter, null, this, null, intAmmoCount);
            _lstAmmo.Add(objInternalClip);
        }

        /// <summary>
        /// Recreates the single internal clip used by weapons that have an ammo capacity but do not require ammo (i.e. they use charges)
        /// </summary>
        private async ValueTask RecreateInternalClipAsync(CancellationToken token = default)
        {
            if (RequireAmmo || string.IsNullOrWhiteSpace(Ammo) || Ammo == "0")
                return;

            int intAmmoCount = 0;
            if (_lstAmmo.Count > 0)
            {
                Clip objCurrentClip = GetClip(_intActiveAmmoSlot);
                if (objCurrentClip != null)
                    intAmmoCount = objCurrentClip.Ammo;
            }

            _lstAmmo.Clear();
            _intActiveAmmoSlot = 1;

            // First try to get the max ammo capacity for this weapon because that will be the capacity of the internal clip
            List<string> lstCount = new List<string>(1);
            string ammoString = await CalculatedAmmoAsync(GlobalSettings.CultureInfo, GlobalSettings.DefaultLanguage, token).ConfigureAwait(false);
            // Determine which loading methods are available to the Weapon.
            if (ammoString.IndexOfAny('x', '+') != -1 ||
                ammoString.Contains(" or ", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("Special", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("External Source", StringComparison.OrdinalIgnoreCase))
            {
                string strWeaponAmmo = ammoString.FastEscape("External Source", StringComparison.OrdinalIgnoreCase);
                strWeaponAmmo = strWeaponAmmo.ToLowerInvariant();
                // Get rid of or belt, and + energy.
                strWeaponAmmo = strWeaponAmmo.FastEscapeOnceFromEnd(" + energy")
                    .Replace(" or belt", " or 250(belt)");

                foreach (string strAmmo in strWeaponAmmo.SplitNoAlloc(" or ", StringSplitOptions.RemoveEmptyEntries))
                {
                    lstCount.Add(AmmoCapacity(strAmmo));
                }
            }
            else
            {
                // Nothing weird in the ammo string, so just use the number given.
                string strAmmo = ammoString;
                int intPos = strAmmo.IndexOf('(');
                if (intPos != -1)
                    strAmmo = strAmmo.Substring(0, intPos);
                lstCount.Add(strAmmo);
            }

            foreach (string strAmmo in lstCount)
            {
                if (int.TryParse(strAmmo, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intNewAmmoCount))
                {
                    if (intAmmoCount > intNewAmmoCount)
                        intAmmoCount = intNewAmmoCount;
                    break;
                }
            }

            Clip objInternalClip = new Clip(_objCharacter, null, this, null, intAmmoCount);
            _lstAmmo.Add(objInternalClip);
        }

        /// <summary>
        /// Create a weapon's initial clips. Should only be called while the ammo list is empty.
        /// </summary>
        public void CreateClips()
        {
            _lstAmmo.Clear(); // Just in case
            for (int i = 0; i < _intAmmoSlots; ++i)
                _lstAmmo.Add(new Clip(_objCharacter, null, this, null, 0));
            foreach (WeaponAccessory adoptable in GetClipProvidingAccessories())
                _lstAmmo.Add(new Clip(_objCharacter, adoptable, this, null, 0));
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Are we loading a copy of an existing weapon?</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            Lazy<XmlNode> objMyNode = new Lazy<XmlNode>(() => this.GetNode());
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                objMyNode.Value?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            switch (_strCategory)
            {
                case "Hold-Outs":
                    _strCategory = "Holdouts";
                    break;

                case "Cyberware Hold-Outs":
                    _strCategory = "Cyberware Holdouts";
                    break;
            }
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("spec", ref _strSpec);
            objNode.TryGetStringFieldQuickly("spec2", ref _strSpec2);
            objNode.TryGetInt32FieldQuickly("reach", ref _intReach);
            objNode.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
            objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            // Legacy catch for if a damage expression is not empty but has no attributes associated with it.
            if (_objCharacter.LastSavedVersion < new Version(5, 214, 98) && !string.IsNullOrEmpty(_strDamage) &&
                !_strDamage.Contains('{') && AttributeSection.AttributeStrings.Any(x => _strDamage.Contains(x)))
            {
                objMyNode.Value?.TryGetStringFieldQuickly("damage", ref _strDamage);
            }
            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            if (objNode["firingmode"] != null)
                _eFiringMode = ConvertToFiringMode(objNode["firingmode"].InnerText);
            // Legacy shim
            if (Name.Contains("Osmium Mace (STR"))
            {
                XmlNode objNewOsmiumMaceNode = _objCharacter.LoadData("weapons.xml").SelectSingleNode("/chummer/weapons/weapon[name = \"Osmium Mace\"]");
                if (objNewOsmiumMaceNode != null)
                {
                    objNewOsmiumMaceNode.TryGetStringFieldQuickly("name", ref _strName);
                    objNewOsmiumMaceNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
                    _objCachedMyXmlNode = objNewOsmiumMaceNode;
                    objNewOsmiumMaceNode.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
                    objNewOsmiumMaceNode.TryGetStringFieldQuickly("damage", ref _strDamage);
                }
            }
            objNode.TryGetStringFieldQuickly("ap", ref _strAP);
            objNode.TryGetStringFieldQuickly("mode", ref _strMode);
            objNode.TryGetStringFieldQuickly("rc", ref _strRC);
            objNode.TryGetStringFieldQuickly("ammo", ref _strAmmo);
            objNode.TryGetBoolFieldQuickly("cyberware", ref _blnCyberware);
            objNode.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            if (!objNode.TryGetInt32FieldQuickly("ammoslots", ref _intAmmoSlots))
                _intAmmoSlots = 1;
            objNode.TryGetStringFieldQuickly("sizecategory", ref _strSizeCategory);
            objNode.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            if (!objNode.TryGetStringFieldQuickly("weight", ref _strWeight))
                objMyNode.Value?.TryGetStringFieldQuickly("weight", ref _strWeight);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objNode.TryGetInt32FieldQuickly("singleshot", ref _intSingleShot);
            objNode.TryGetInt32FieldQuickly("shortburst", ref _intShortBurst);
            objNode.TryGetInt32FieldQuickly("longburst", ref _intLongBurst);
            objNode.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objNode.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("parentid", ref _strParentID);
            if (!objNode.TryGetBoolFieldQuickly("allowaccessory", ref _blnAllowAccessory))
                _blnAllowAccessory = objMyNode.Value?["allowaccessory"]?.InnerText != bool.FalseString;
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("weaponname", ref _strWeaponName);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("mount", ref _strMount);
            objNode.TryGetStringFieldQuickly("extramount", ref _strExtraMount);
            if (_strRange == "Hold-Outs")
            {
                _strRange = "Holdouts";
            }
            if (!objNode.TryGetStringFieldQuickly("alternaterange", ref _strAlternateRange))
            {
                string strAlternateRange = objMyNode.Value?["alternaterange"]?.InnerText;
                if (!string.IsNullOrEmpty(strAlternateRange))
                {
                    _strAlternateRange = strAlternateRange;
                }
            }
            objNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
            objNode.TryGetStringFieldQuickly("useskillspec", ref _strUseSkillSpec);
            objNode.TryGetDecFieldQuickly("rangemultiply", ref _decRangeMultiplier);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludedInWeapon);
            if (Name == "Unarmed Attack")
                _blnIncludedInWeapon = true; // Unarmed Attack can never be removed
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            if (!_blnEquipped)
            {
                objNode.TryGetBoolFieldQuickly("installed", ref _blnEquipped);
            }
            objNode.TryGetBoolFieldQuickly("requireammo", ref _blnRequireAmmo);
            if (!objNode.TryGetStringFieldQuickly("weapontype", ref _strWeaponType))
                _strWeaponType = objMyNode.Value?["weapontype"]?.InnerText
                                 ?? _objCharacter.LoadDataXPath("weapons.xml")
                                                 .SelectSingleNodeAndCacheExpression(
                                                     "/chummer/categories/category[. = " + Category.CleanXPath()
                                                     + "]/@type")?.Value
                                 ?? Category.ToLowerInvariant();

            XmlNode xmlAccessoriesNode = objNode["accessories"];
            if (xmlAccessoriesNode != null)
            {
                using (XmlNodeList nodChildren = xmlAccessoriesNode.SelectNodes("accessory"))
                {
                    if (nodChildren != null)
                    {
                        foreach (XmlNode nodChild in nodChildren)
                        {
                            WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
                            objAccessory.Load(nodChild, blnCopy);
                            objAccessory.Parent = this;
                            _lstAccessories.Add(objAccessory);
                        }
                    }
                }
            }

            if (blnCopy
                // Shortcut for melee weapons and other ammo types that don't use melee weapons
                || string.IsNullOrWhiteSpace(Ammo) || Ammo == "0")
            {
                _lstAmmo.Clear();
                _intActiveAmmoSlot = 1;
                CreateClips();
            }
            else if (!RequireAmmo)
            {
                if (objNode["clips"] != null)
                {
                    _lstAmmo.Clear();
                    _intActiveAmmoSlot = 1;
                    XmlNode clipNode = objNode["clips"];
                    AddClipNodes(clipNode);
                }
                // Legacy for items that were saved before internal clip tracking for weapons that don't need ammo was implemented
                else
                {
                    RecreateInternalClip();
                }
            }
            else
            {
                objNode.TryGetInt32FieldQuickly("activeammoslot", ref _intActiveAmmoSlot);
                _lstAmmo.Clear();
                if (objNode["clips"] != null)
                {
                    XmlNode clipNode = objNode["clips"];
                    AddClipNodes(clipNode);
                }
                else //Load old clips
                {
                    List<WeaponAccessory> lstWeaponAccessoriesWithClipSlots = GetClipProvidingAccessories().ToList();
                    int i = 0;
                    foreach (string strOldClipValue in s_OldClipValues)
                    {
                        int intAmmo = 0;
                        if (objNode.TryGetInt32FieldQuickly("ammoremaining" + strOldClipValue, ref intAmmo) &&
                            objNode.TryGetField("ammoloaded" + strOldClipValue, Guid.TryParse, out Guid guid) &&
                            intAmmo > 0 && guid != Guid.Empty)
                        {
                            Gear objGear = ParentVehicle != null
                                ? ParentVehicle.FindVehicleGear(guid.ToString("D", GlobalSettings.InvariantCultureInfo))
                                : _objCharacter.Gear.DeepFindById(guid.ToString("D", GlobalSettings.InvariantCultureInfo));
                            // Load clips into weapon slots first
                            if (i < _intAmmoSlots)
                            {
                                _lstAmmo.Add(new Clip(_objCharacter, null, this, objGear, intAmmo));
                                ++i;
                            }
                            // Then load clips into accessory-provided slots
                            else if (i < _intAmmoSlots + lstWeaponAccessoriesWithClipSlots.Count)
                            {
                                _lstAmmo.Add(new Clip(_objCharacter, lstWeaponAccessoriesWithClipSlots[i - _intAmmoSlots],
                                                      this, objGear, intAmmo));
                                ++i;
                            }
                            // Finally, we shouldn't end up in this situation, but just in case, load in the extra clips as part of the base weapon
                            else
                            {
                                Utils.BreakIfDebug();
                                _lstAmmo.Add(new Clip(_objCharacter, null, this, objGear, intAmmo));
                                ++i;
                            }
                        }
                    }
                    // We somehow ended up loading fewer clips than clip slots we have, so fill them up with empties
                    for (; i < _intAmmoSlots; ++i)
                    {
                        _lstAmmo.Add(new Clip(_objCharacter, null, this, null, 0));
                    }
                    for (; i < _intAmmoSlots + lstWeaponAccessoriesWithClipSlots.Count; ++i)
                    {
                        _lstAmmo.Add(new Clip(_objCharacter, lstWeaponAccessoriesWithClipSlots[i - _intAmmoSlots], this, null, 0));
                    }
                }
            }

            void AddClipNodes(XmlNode clipNode)
            {
                List<WeaponAccessory> lstWeaponAccessoriesWithClipSlots = GetClipProvidingAccessories().ToList();
                int i = 0;
                foreach (XmlNode node in clipNode.ChildNodes)
                {
                    // Load clips into weapon slots first
                    if (i < _intAmmoSlots)
                    {
                        Clip objLoopClip = Clip.Load(node, _objCharacter, this, null);
                        if (objLoopClip != null)
                        {
                            _lstAmmo.Add(objLoopClip);
                            ++i;
                        }
                    }
                    // Then load clips into accessory-provided slots
                    else if (i < _intAmmoSlots + lstWeaponAccessoriesWithClipSlots.Count)
                    {
                        Clip objLoopClip = Clip.Load(node, _objCharacter, this, lstWeaponAccessoriesWithClipSlots[i - _intAmmoSlots]);
                        if (objLoopClip != null)
                        {
                            _lstAmmo.Add(objLoopClip);
                            ++i;
                        }
                    }
                    // Finally, we shouldn't end up in this situation, but just in case, load in the extra clips as part of the base weapon
                    else
                    {
                        Utils.BreakIfDebug();
                        Clip objLoopClip = Clip.Load(node, _objCharacter, this, null);
                        if (objLoopClip != null)
                        {
                            _lstAmmo.Add(objLoopClip);
                            ++i;
                        }
                    }
                }
                // We somehow ended up loading fewer clips than clip slots we have, so fill them up with empties
                for (; i < _intAmmoSlots; ++i)
                {
                    _lstAmmo.Add(new Clip(_objCharacter, null, this, null, 0));
                }
                for (; i < _intAmmoSlots + lstWeaponAccessoriesWithClipSlots.Count; ++i)
                {
                    _lstAmmo.Add(new Clip(_objCharacter, lstWeaponAccessoriesWithClipSlots[i - _intAmmoSlots], this, null, 0));
                }
            }

            _nodWirelessBonus = objNode["wirelessbonus"];
            objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn);

            //#1544 Ammunition not loading or available.
            if (_strUseSkill == "Throwing Weapons"
                && _strAmmo != "1")
            {
                _strAmmo = "1";
            }

            XmlNode xmlUnderbarrelNode = objNode["underbarrel"];
            if (xmlUnderbarrelNode != null)
            {
                using (XmlNodeList nodChildren = xmlUnderbarrelNode.SelectNodes("weapon"))
                {
                    if (nodChildren != null)
                    {
                        foreach (XmlNode nodWeapon in nodChildren)
                        {
                            Weapon objUnderbarrel = new Weapon(_objCharacter)
                            {
                                ParentVehicle = ParentVehicle
                            };
                            objUnderbarrel.Load(nodWeapon, blnCopy);
                            _lstUnderbarrel.Add(objUnderbarrel);
                        }
                    }
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            string strLocation = objNode["location"]?.InnerText;
            if (!string.IsNullOrEmpty(strLocation))
            {
                if (Guid.TryParse(strLocation, out Guid temp))
                {
                    // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                    _objLocation =
                        _objCharacter.WeaponLocations.FirstOrDefault(location =>
                            location.InternalId == temp.ToString());
                }
                else
                {
                    //Legacy. Location is a string.
                    _objLocation =
                        _objCharacter.WeaponLocations.FirstOrDefault(location =>
                            location.Name == strLocation);
                }
            }
            _objLocation?.Children.Add(this);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            if (!objNode.TryGetStringFieldQuickly("weaponslots", ref _strWeaponSlots))
            {
                XmlNode objXmlWeapon = objMyNode.Value;
                if (objXmlWeapon?["accessorymounts"] != null)
                {
                    XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("accessorymounts/mount");
                    if (objXmlMountList?.Count > 0)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdMounts))
                        {
                            foreach (XmlNode objXmlMount in objXmlMountList)
                            {
                                sbdMounts.Append(objXmlMount.InnerText).Append('/');
                            }

                            if (sbdMounts.Length > 0)
                                --sbdMounts.Length;
                            _strWeaponSlots = sbdMounts.ToString();
                        }
                    }
                }
            }
            if (!objNode.TryGetStringFieldQuickly("doubledcostweaponslots", ref _strDoubledCostWeaponSlots))
            {
                XmlNode objXmlWeapon = objMyNode.Value;
                if (objXmlWeapon?["doubledcostaccessorymounts"] != null)
                {
                    XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("doubledcostaccessorymounts/mount");
                    if (objXmlMountList?.Count > 0)
                    {
                        using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                                      out StringBuilder sbdMounts))
                        {
                            foreach (XmlNode objXmlMount in objXmlMountList)
                            {
                                sbdMounts.Append(objXmlMount.InnerText).Append('/');
                            }

                            if (sbdMounts.Length > 0)
                                --sbdMounts.Length;
                            _strDoubledCostWeaponSlots = sbdMounts.ToString();
                        }
                    }
                }
            }

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
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async ValueTask Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint, CancellationToken token = default)
        {
            // Find the piece of Gear that created this item if applicable
            Gear objGear = null;
            if (!string.IsNullOrWhiteSpace(ParentID) && !ParentID.IsEmptyGuid())
            {
                objGear = await _objCharacter.Gear
                    .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID, token: token)
                    .ConfigureAwait(false);
                if (objGear == null)
                {
                    foreach (Cyberware objCyberware in await _objCharacter.Cyberware
                                 .DeepWhereAsync(
                                     x => x.Children,
                                     async x => await x.GearChildren
                                                    .GetCountAsync(token)
                                                    .ConfigureAwait(false)
                                                > 0, token).ConfigureAwait(false))
                    {
                        objGear = await objCyberware.GearChildren
                            .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID, token: token)
                            .ConfigureAwait(false);
                        if (objGear != null)
                            break;
                    }

                    if (objGear == null)
                    {
                        foreach (Weapon objWeapon in await _objCharacter.Weapons
                                     .DeepWhereAsync(
                                         x => x.Children,
                                         x => x.WeaponAccessories.AnyAsync(
                                             async y => await y.GearChildren.GetCountAsync(token)
                                                 .ConfigureAwait(false) > 0, token), token)
                                     .ConfigureAwait(false))
                        {
                            await objWeapon.WeaponAccessories
                                .ForEachWithBreakAsync(async objAccessory =>
                                    {
                                        Gear objReturn = await objAccessory.GearChildren
                                            .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID,
                                                token: token)
                                            .ConfigureAwait(false);
                                        if (objReturn != null)
                                        {
                                            objGear = objReturn;
                                            return false;
                                        }

                                        return true;
                                    },
                                    token).ConfigureAwait(false);
                            if (objGear != null)
                                break;
                        }

                        if (objGear == null)
                        {
                            await _objCharacter.Armor
                                .ForEachWithBreakAsync(async objArmor =>
                                {
                                    Gear objReturn = await objArmor.GearChildren
                                        .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID,
                                            token: token)
                                        .ConfigureAwait(false);
                                    if (objReturn != null)
                                    {
                                        objGear = objReturn;
                                        return false;
                                    }

                                    await objArmor.ArmorMods.ForEachWithBreakAsync(async objMod =>
                                    {
                                        Gear objReturn2 = await objMod.GearChildren
                                            .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID,
                                                token: token)
                                            .ConfigureAwait(false);
                                        if (objReturn2 != null)
                                        {
                                            objGear = objReturn2;
                                            return false;
                                        }

                                        return true;
                                    }, token: token).ConfigureAwait(false);

                                    return objGear == null;
                                }, token).ConfigureAwait(false);
                            if (objGear == null)
                            {
                                await _objCharacter.Vehicles.ForEachWithBreakAsync(async objVehicle =>
                                {
                                    Gear objReturn = await objVehicle.GearChildren
                                        .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID,
                                            token: token)
                                        .ConfigureAwait(false);
                                    if (objReturn != null)
                                    {
                                        objGear = objReturn;
                                        return false;
                                    }

                                    foreach (Weapon objWeapon in await objVehicle.Weapons
                                                 .DeepWhereAsync(
                                                     x => x.Children,
                                                     x => x.WeaponAccessories.AnyAsync(
                                                         async y => await y.GearChildren.GetCountAsync(token)
                                                             .ConfigureAwait(false) > 0, token), token)
                                                 .ConfigureAwait(false))
                                    {
                                        await objWeapon.WeaponAccessories
                                            .ForEachWithBreakAsync(async objAccessory =>
                                                {
                                                    Gear objReturn2 = await objAccessory.GearChildren
                                                        .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID,
                                                            token: token)
                                                        .ConfigureAwait(false);
                                                    if (objReturn2 != null)
                                                    {
                                                        objGear = objReturn2;
                                                        return false;
                                                    }

                                                    return true;
                                                },
                                                token).ConfigureAwait(false);
                                        if (objGear != null)
                                            return false;
                                    }

                                    await objVehicle.Mods.ForEachWithBreakAsync(async objVehicleMod =>
                                    {
                                        if (await objVehicleMod.Cyberware.GetCountAsync(token).ConfigureAwait(false) ==
                                            0
                                            && await objVehicleMod.Weapons.GetCountAsync(token).ConfigureAwait(false) ==
                                            0)
                                            return true;

                                        foreach (Cyberware objCyberware in await objVehicleMod.Cyberware
                                                     .DeepWhereAsync(
                                                         x => x.Children,
                                                         async x => await x.GearChildren
                                                                        .GetCountAsync(token)
                                                                        .ConfigureAwait(false)
                                                                    > 0, token).ConfigureAwait(false))
                                        {
                                            objGear = await objCyberware.GearChildren
                                                .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID, token: token)
                                                .ConfigureAwait(false);
                                            if (objGear != null)
                                                return false;
                                        }

                                        foreach (Weapon objWeapon in await objVehicleMod.Weapons
                                                     .DeepWhereAsync(
                                                         x => x.Children,
                                                         x => x.WeaponAccessories.AnyAsync(
                                                             async y => await y.GearChildren.GetCountAsync(token)
                                                                 .ConfigureAwait(false) > 0, token), token)
                                                     .ConfigureAwait(false))
                                        {
                                            await objWeapon.WeaponAccessories
                                                .ForEachWithBreakAsync(async objAccessory =>
                                                    {
                                                        Gear objReturn2 = await objAccessory.GearChildren
                                                            .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID,
                                                                token: token)
                                                            .ConfigureAwait(false);
                                                        if (objReturn2 != null)
                                                        {
                                                            objGear = objReturn2;
                                                            return false;
                                                        }

                                                        return true;
                                                    },
                                                    token).ConfigureAwait(false);
                                            if (objGear != null)
                                                return false;
                                        }

                                        return true;
                                    }, token).ConfigureAwait(false);

                                    if (objGear != null)
                                        return false;

                                    await objVehicle.WeaponMounts.ForEachWithBreakAsync(async objMount =>
                                    {
                                        foreach (Weapon objWeapon in await objMount.Weapons
                                                     .DeepWhereAsync(
                                                         x => x.Children,
                                                         x => x.WeaponAccessories.AnyAsync(
                                                             async y => await y.GearChildren.GetCountAsync(token)
                                                                 .ConfigureAwait(false) > 0, token), token)
                                                     .ConfigureAwait(false))
                                        {
                                            await objWeapon.WeaponAccessories
                                                .ForEachWithBreakAsync(async objAccessory =>
                                                    {
                                                        Gear objReturn2 = await objAccessory.GearChildren
                                                            .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID,
                                                                token: token)
                                                            .ConfigureAwait(false);
                                                        if (objReturn2 != null)
                                                        {
                                                            objGear = objReturn2;
                                                            return false;
                                                        }

                                                        return true;
                                                    },
                                                    token).ConfigureAwait(false);
                                            if (objGear != null)
                                                return false;
                                        }

                                        await objMount.Mods.ForEachWithBreakAsync(async objVehicleMod =>
                                        {
                                            if (await objVehicleMod.Cyberware.GetCountAsync(token).ConfigureAwait(false) ==
                                                0
                                                && await objVehicleMod.Weapons.GetCountAsync(token).ConfigureAwait(false) ==
                                                0)
                                                return true;

                                            foreach (Cyberware objCyberware in await objVehicleMod.Cyberware
                                                         .DeepWhereAsync(
                                                             x => x.Children,
                                                             async x => await x.GearChildren
                                                                            .GetCountAsync(token)
                                                                            .ConfigureAwait(false)
                                                                        > 0, token).ConfigureAwait(false))
                                            {
                                                objGear = await objCyberware.GearChildren
                                                    .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID, token: token)
                                                    .ConfigureAwait(false);
                                                if (objGear != null)
                                                    return false;
                                            }

                                            foreach (Weapon objWeapon in await objVehicleMod.Weapons
                                                         .DeepWhereAsync(
                                                             x => x.Children,
                                                             x => x.WeaponAccessories.AnyAsync(
                                                                 async y => await y.GearChildren.GetCountAsync(token)
                                                                     .ConfigureAwait(false) > 0, token), token)
                                                         .ConfigureAwait(false))
                                            {
                                                await objWeapon.WeaponAccessories
                                                    .ForEachWithBreakAsync(async objAccessory =>
                                                        {
                                                            Gear objReturn2 = await objAccessory.GearChildren
                                                                .DeepFirstOrDefaultAsync(x => x.GearChildren, x => x.InternalId == ParentID,
                                                                    token: token)
                                                                .ConfigureAwait(false);
                                                            if (objReturn2 != null)
                                                            {
                                                                objGear = objReturn2;
                                                                return false;
                                                            }

                                                            return true;
                                                        },
                                                        token).ConfigureAwait(false);
                                                if (objGear != null)
                                                    return false;
                                            }

                                            return true;
                                        }, token).ConfigureAwait(false);

                                        return objGear == null;
                                    }, token: token).ConfigureAwait(false);

                                    return objGear == null;
                                }, token).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            // <weapon>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("weapon", token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category", await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("type", RangeType, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("reach", (await GetTotalReachAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("accuracy", await GetAccuracyAsync(objCulture, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("accuracy_noammo", await GetAccuracyAsync(objCulture, strLanguageToPrint, false, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawaccuracy", Accuracy, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("damage", await CalculatedDamageAsync(objCulture, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("damage_noammo", await CalculatedDamageAsync(objCulture, strLanguageToPrint, false, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("damage_english", await CalculatedDamageAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("damage_noammo_english", await CalculatedDamageAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, false, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawdamage", Damage, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ap", await TotalAPAsync(objCulture, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ap_noammo", await TotalAPAsync(objCulture, strLanguageToPrint, false, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawap", AP, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("mode", await CalculatedModeAsync(strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("mode_noammo", await CalculatedModeAsync(strLanguageToPrint, false, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rc", await TotalRCAsync(objCulture, strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rc_noammo", await TotalRCAsync(objCulture, strLanguageToPrint, blnIncludeAmmo: false, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawrc", RC, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ammo", await CalculatedAmmoAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ammo_english", await CalculatedAmmoAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("maxammo", Ammo, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("conceal", await CalculatedConcealabilityAsync(objCulture, token).ConfigureAwait(false), token).ConfigureAwait(false);
                if (objGear != null)
                {
                    await objWriter.WriteElementStringAsync("avail", await objGear.TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("cost", (await objGear.GetTotalCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat, objCulture), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("owncost", (await objGear.GetOwnCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat, objCulture), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("weight", objGear.TotalWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("ownweight", objGear.OwnWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture), token).ConfigureAwait(false);
                }
                else
                {
                    await objWriter.WriteElementStringAsync("avail", await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("cost", (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat, objCulture), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("owncost", (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat, objCulture), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("weight", TotalWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("ownweight", OwnWeight.ToString(_objCharacter.Settings.WeightFormat, objCulture), token).ConfigureAwait(false);
                }
                await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weaponname", CustomName, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("location", Location != null ? await Location.DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false) : string.Empty, token).ConfigureAwait(false);

                await objWriter.WriteElementStringAsync("attack", this.GetTotalMatrixAttribute("Attack").ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sleaze", this.GetTotalMatrixAttribute("Sleaze").ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("dataprocessing", this.GetTotalMatrixAttribute("Data Processing").ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("firewall", this.GetTotalMatrixAttribute("Firewall").ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("devicerating", this.GetTotalMatrixAttribute("Device Rating").ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("programlimit", this.GetTotalMatrixAttribute("Program Limit").ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("iscommlink", IsCommlink.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("conditionmonitor", MatrixCM.ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("matrixcmfilled", MatrixCMFilled.ToString(objCulture), token).ConfigureAwait(false);

                if (WeaponAccessories.Count > 0)
                {
                    // <accessories>
                    XmlElementWriteHelper objAccessoriesElement = await objWriter.StartElementAsync("accessories", token).ConfigureAwait(false);
                    try
                    {
                        foreach (WeaponAccessory objAccessory in WeaponAccessories)
                            await objAccessory.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                    }
                    finally
                    {
                        // </accessories>
                        await objAccessoriesElement.DisposeAsync().ConfigureAwait(false);
                    }
                }

                Dictionary<string, string> dicRanges = await GetRangeStringsAsync(objCulture, token: token).ConfigureAwait(false);

                // <ranges>
                XmlElementWriteHelper objRangesElement = await objWriter.StartElementAsync("ranges", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("name", await DisplayRangeAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("short", dicRanges["short"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("medium", dicRanges["medium"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("long", dicRanges["long"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("extreme", dicRanges["extreme"], token).ConfigureAwait(false);
                }
                finally
                {
                    // </ranges>
                    await objRangesElement.DisposeAsync().ConfigureAwait(false);
                }

                // <alternateranges>
                XmlElementWriteHelper objAlternateRangesElement = await objWriter.StartElementAsync("alternateranges", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("name", await DisplayAlternateRangeAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("short", dicRanges["alternateshort"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("medium", dicRanges["alternatemedium"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("long", dicRanges["alternatelong"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("extreme", dicRanges["alternateextreme"], token).ConfigureAwait(false);
                }
                finally
                {
                    // </alternateranges>
                    await objAlternateRangesElement.DisposeAsync().ConfigureAwait(false);
                }

                Dictionary<string, string> dicRangesNoAmmo = await GetRangeStringsAsync(objCulture, false, token).ConfigureAwait(false);

                // <ranges>
                XmlElementWriteHelper objRangesNoAmmoElement = await objWriter.StartElementAsync("ranges", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("name", await DisplayRangeAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("short", dicRangesNoAmmo["short"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("medium", dicRangesNoAmmo["medium"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("long", dicRangesNoAmmo["long"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("extreme", dicRangesNoAmmo["extreme"], token).ConfigureAwait(false);
                }
                finally
                {
                    // </ranges>
                    await objRangesNoAmmoElement.DisposeAsync().ConfigureAwait(false);
                }

                // <alternateranges>
                XmlElementWriteHelper objAlternateRangesNoAmmoElement = await objWriter.StartElementAsync("alternateranges", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("name", await DisplayAlternateRangeAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("short", dicRangesNoAmmo["alternateshort"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("medium", dicRangesNoAmmo["alternatemedium"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("long", dicRangesNoAmmo["alternatelong"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("extreme", dicRangesNoAmmo["alternateextreme"], token).ConfigureAwait(false);
                }
                finally
                {
                    // </alternateranges>
                    await objAlternateRangesNoAmmoElement.DisposeAsync().ConfigureAwait(false);
                }

                foreach (Weapon objUnderbarrel in Children)
                {
                    // <underbarrel>
                    XmlElementWriteHelper objUnderbarrelElement = await objWriter.StartElementAsync("underbarrel", token).ConfigureAwait(false);
                    try
                    {
                        await objUnderbarrel.Print(objWriter, objCulture, strLanguageToPrint, token: token).ConfigureAwait(false);
                    }
                    finally
                    {
                        // </underbarrel>
                        await objUnderbarrelElement.DisposeAsync().ConfigureAwait(false);
                    }
                }

                // Currently loaded Ammo.
                Clip objLoadedClip = GetClip(_intActiveAmmoSlot);
                await objWriter.WriteElementStringAsync("availableammo", GetAvailableAmmo.ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("currentammo", await objLoadedClip.DisplayAmmoNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);

                // <clips>
                XmlElementWriteHelper objClipsElement = await objWriter.StartElementAsync("clips", token).ConfigureAwait(false);
                try
                {
                    if (RequireAmmo)
                    {
                        foreach (Clip objClip in _lstAmmo)
                        {
                            await objClip.Print(objWriter, objCulture, strLanguageToPrint, token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        await objLoadedClip.Print(objWriter, objCulture, strLanguageToPrint, token)
                                           .ConfigureAwait(false);
                    }
                }
                finally
                {
                    // </clips>
                    await objClipsElement.DisposeAsync().ConfigureAwait(false);
                }

                await objWriter.WriteElementStringAsync("dicepool", (await GetDicePoolAsync(token: token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("dicepool_noammo", (await GetDicePoolAsync(false, token: token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
                Skill objSkill = Skill;
                await objWriter
                      .WriteElementStringAsync(
                          "skill",
                          objSkill != null
                              ? await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                              : string.Empty, token).ConfigureAwait(false);

                await objWriter.WriteElementStringAsync("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", Notes, token).ConfigureAwait(false);
            }
            finally
            {
                // </weapon>
                await objBaseElement.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Get the amount of available Ammo from the character or Vehicle.
        /// </summary>
        private decimal GetAvailableAmmo
        {
            get
            {
                if (!RequireAmmo)
                {
                    return 0;
                }
                IList<Gear> lstGear = ParentVehicle == null ? _objCharacter.Gear : ParentVehicle.GearChildren;
                return GetAmmoReloadable(lstGear).Sum(x => x.Quantity);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Weapon Accessories.
        /// </summary>
        public TaggedObservableCollection<WeaponAccessory> WeaponAccessories
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                    return _lstAccessories;
            }
        }

        /// <summary>
        /// Underbarrel Weapon.
        /// </summary>
        public TaggedObservableCollection<Weapon> UnderbarrelWeapons
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                    return _lstUnderbarrel;
            }
        }

        /// <summary>
        /// Children as Underbarrel Weapon.
        /// </summary>
        public TaggedObservableCollection<Weapon> Children => UnderbarrelWeapons;

        /// <summary>
        /// Magazines used for holding and tracking ammo.
        /// </summary>
        public IReadOnlyList<Clip> Clips => _lstAmmo;

        /// <summary>
        /// Internal identifier which will be used to identify this Weapon.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        public async ValueTask<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            return objNode != null ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name : Name;
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (Rating > 0)
                strReturn += strSpace + '(' + LanguageManager.GetString(RatingLabel, strLanguage) + strSpace + Rating.ToString(objCulture) + ')';
            if (!string.IsNullOrEmpty(CustomName))
                strReturn += strSpace + "(\"" + CustomName + "\")";
            return strReturn;
        }

        /// <summary>
        /// Display name.
        /// </summary>
        public async ValueTask<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            if (Rating > 0)
                strReturn += strSpace + '(' + await LanguageManager.GetStringAsync(RatingLabel, strLanguage, token: token).ConfigureAwait(false) + strSpace + Rating.ToString(objCulture) + ')';
            if (!string.IsNullOrEmpty(CustomName))
                strReturn += strSpace + "(\"" + CustomName + "\")";
            return strReturn;
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// A custom name for the Weapon assigned by the player.
        /// </summary>
        public string CustomName
        {
            get => _strWeaponName;
            set => _strWeaponName = value;
        }

        public int Rating
        {
            get => _intRating;
            set => _intRating = value;
        }

        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
        }

        /// <summary>
        /// Minimum Rating (string form).
        /// </summary>
        public string MinRating
        {
            get => _strMinRating;
            set => _strMinRating = value;
        }

        /// <summary>
        /// Maximum Rating (string form).
        /// </summary>
        public string MaxRating
        {
            get => _strMaxRating;
            set => _strMaxRating = value;
        }

        /// <summary>
        /// Minimum Rating (value form).
        /// </summary>
        public int MinRatingValue
        {
            get
            {
                string strExpression = MinRating;
                return string.IsNullOrEmpty(strExpression) ? 0 : ProcessRatingString(strExpression);
            }
            set => MinRating = value.ToString(GlobalSettings.InvariantCultureInfo);
        }

        /// <summary>
        /// Maximum Rating (string form).
        /// </summary>
        public int MaxRatingValue
        {
            get
            {
                string strExpression = MaxRating;
                return string.IsNullOrEmpty(strExpression) ? int.MaxValue : ProcessRatingString(strExpression);
            }
            set => MaxRating = value.ToString(GlobalSettings.InvariantCultureInfo);
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        /// <param name="strExpression"></param>
        /// <returns></returns>
        private int ProcessRatingString(string strExpression)
        {
            if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',', StringSplitOptions.RemoveEmptyEntries);
                strExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    sbdValue.Replace("{Rating}", Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);

                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }

            int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);

            return intReturn;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            switch (Category)
            {
                // If Categories are actually the name of object types, so pull them from the language file.
                case "Gear":
                    return LanguageManager.GetString("String_SelectPACKSKit_Gear", strLanguage);

                case "Cyberware":
                    return LanguageManager.GetString("String_SelectPACKSKit_Cyberware", strLanguage);

                case "Bioware":
                    return LanguageManager.GetString("String_SelectPACKSKit_Bioware", strLanguage);
            }

            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("weapons.xml", strLanguage)
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value
                   ?? Category;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public async ValueTask<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            switch (Category)
            {
                // If Categories are actually the name of object types, so pull them from the language file.
                case "Gear":
                    return await LanguageManager.GetStringAsync("String_SelectPACKSKit_Gear", strLanguage, token: token).ConfigureAwait(false);

                case "Cyberware":
                    return await LanguageManager.GetStringAsync("String_SelectPACKSKit_Cyberware", strLanguage, token: token).ConfigureAwait(false);

                case "Bioware":
                    return await LanguageManager.GetStringAsync("String_SelectPACKSKit_Bioware", strLanguage, token: token).ConfigureAwait(false);
            }

            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return (await (await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token)
                                              .ConfigureAwait(false))
                          .SelectSingleNodeAndCacheExpressionAsync(
                              "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate",
                              token: token)
                          .ConfigureAwait(false))?.Value ?? Category;
        }

        /// <summary>
        /// Translated Ammo Category.
        /// </summary>
        public string DisplayAmmoCategory(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return AmmoCategory;

            return _objCharacter.LoadDataXPath("weapons.xml", strLanguage)
                                .SelectSingleNodeAndCacheExpression(
                                    "/chummer/categories/category[. = " + AmmoCategory.CleanXPath() + "]/@translate")
                                ?.Value ?? AmmoCategory;
        }

        /// <summary>
        /// Translated Ammo Category.
        /// </summary>
        public async ValueTask<string> DisplayAmmoCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return AmmoCategory;

            return (await (await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token)
                                              .ConfigureAwait(false))
                          .SelectSingleNodeAndCacheExpressionAsync(
                              "/chummer/categories/category[. = " + AmmoCategory.CleanXPath() + "]/@translate",
                              token: token)
                          .ConfigureAwait(false))?.Value ?? AmmoCategory;
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
        /// Effective size of the weapon for mounting purposes.
        /// </summary>
        public string SizeCategory => string.IsNullOrWhiteSpace(_strSizeCategory) ? Category : _strSizeCategory;

        /// <summary>
        /// Type of Weapon (either Melee or Ranged).
        /// </summary>
        public string RangeType
        {
            get => _strType;
            set => _strType = value;
        }

        /// <summary>
        /// Is this weapon cyberware?
        /// </summary>
        public bool Cyberware => _blnCyberware;

        /// <summary>
        /// Reach.
        /// </summary>
        public int Reach
        {
            get => _intReach;
            set => _intReach = value;
        }

        /// <summary>
        /// Accuracy.
        /// </summary>
        public string Accuracy
        {
            get => _strAccuracy;
            set => _strAccuracy = value;
        }

        /// <summary>
        /// Damage.
        /// </summary>
        public string Damage
        {
            get => _strDamage;
            set => _strDamage = value;
        }

        /// <summary>
        /// Armor Penetration.
        /// </summary>
        public string AP
        {
            get => _strAP;
            set => _strAP = value;
        }

        /// <summary>
        /// Firing Mode.
        /// </summary>
        public string Mode
        {
            get => _strMode;
            set => _strMode = value;
        }

        /// <summary>
        /// Recoil.
        /// </summary>
        public string RC
        {
            get => _strRC;
            set => _strRC = value;
        }

        /// <summary>
        /// Ammo.
        /// </summary>
        public string Ammo
        {
            get => _strAmmo;
            set
            {
                if (Interlocked.Exchange(ref _strAmmo, value) == value)
                    return;
                RecreateInternalClip();
            }
        }

        /// <summary>
        /// Category of Ammo the Weapon uses.
        /// </summary>
        public string AmmoCategory => !string.IsNullOrEmpty(_strAmmoCategory) ? _strAmmoCategory : Category;

        /// <summary>
        /// Whether the weapon is melee or ranged.
        /// </summary>
        public string WeaponType => _strWeaponType;

        /// <summary>
        /// The number of rounds remaining in the Weapon.
        /// </summary>
        public int AmmoRemaining
        {
            get => GetClip(_intActiveAmmoSlot).Ammo;
            set
            {
                Clip objCurrentClip = GetClip(_intActiveAmmoSlot);
                int intCurrentAmmo = objCurrentClip.Ammo;
                if (intCurrentAmmo == value)
                    return;
                objCurrentClip.Ammo = value;
                Gear objGear = objCurrentClip.AmmoGear;
                if (objGear == null)
                    return;
                if (objGear.Quantity + value - intCurrentAmmo <= 0)
                {
                    objGear.DeleteGear();
                }
                else
                {
                    objGear.Quantity += value - intCurrentAmmo;
                }
            }
        }

        /// <summary>
        /// The total number of rounds that the weapon can load.
        /// </summary>
        private static string AmmoCapacity(string strAmmo)
        {
            // Assuming base text of 10(ml)x2
            // matches [2x]10(ml) or [10x]2(ml)
            foreach (Match m in s_RgxAmmoCapacityFirst.Value.Matches(strAmmo))
            {
                strAmmo = strAmmo.TrimStartOnce(m.Value);
            }

            // Matches 2(ml[)x10] (But does not capture the ')') or 10(ml)[x2]
            foreach (Match m in s_RgxAmmoCapacitySecond.Value.Matches(strAmmo))
            {
                strAmmo = strAmmo.TrimEndOnce(m.Value);
            }

            int intPos = strAmmo.IndexOf('(');
            if (intPos != -1)
                strAmmo = strAmmo.Substring(0, intPos);
            return strAmmo;
        }

        private static readonly Lazy<Regex> s_RgxAmmoCapacityFirst = new Lazy<Regex>(() => new Regex(@"^[0-9]*[0-9]*x",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled));
        private static readonly Lazy<Regex> s_RgxAmmoCapacitySecond = new Lazy<Regex>(() => new Regex(@"(?<=\))(x[0-9]*[0-9]*$)*",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled));

        /// <summary>
        /// The type of Ammunition loaded in the Weapon.
        /// </summary>
        public Gear AmmoLoaded
        {
            get => GetClip(_intActiveAmmoSlot).AmmoGear;
            set => GetClip(_intActiveAmmoSlot).AmmoGear = value;
        }

        /// <summary>
        /// Active Ammo slot number.
        /// </summary>
        public int ActiveAmmoSlot
        {
            get => _intActiveAmmoSlot;
            set => _intActiveAmmoSlot = value;
        }

        /// <summary>
        /// Concealability.
        /// </summary>
        public int Concealability
        {
            get => _intConceal;
            set => _intConceal = value;
        }

        /// <summary>
        /// Avail.
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

        public string DisplayCost(out decimal decItemCost, decimal decMarkup = 0.0m)
        {
            string strReturn = Cost;
            if (strReturn.StartsWith("Variable(", StringComparison.Ordinal))
            {
                strReturn = strReturn.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                if (strReturn.Contains('-'))
                {
                    string[] strValues = strReturn.Split('-');
                    decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                    decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                }
                else
                    decMin = Convert.ToDecimal(strReturn.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                if (decMax == decimal.MaxValue)
                    strReturn = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol") + '+';
                else
                    strReturn = decMin.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + " - " + decMax.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol");

                decItemCost = decMin;
                return strReturn;
            }

            decimal decTotalCost = Convert.ToDecimal(strReturn, GlobalSettings.InvariantCultureInfo);

            decTotalCost *= 1.0m + decMarkup;

            if (DiscountCost)
                decTotalCost *= 0.9m;

            decItemCost = decTotalCost;

            return decTotalCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol");
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
        public async ValueTask<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            string s = objNode != null
                ? objNode.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? Page
                : Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        public Weapon Parent
        {
            get => _objParent;
            set
            {
                if (Interlocked.Exchange(ref _objParent, value) != value)
                {
                    // Includes ParentVehicle setter
                    ParentMount = value?.ParentMount;
                }
            }
        }

        /// <summary>
        /// ID of the object that added this weapon (if any).
        /// </summary>
        public string ParentID
        {
            get => _strParentID;
            set => _strParentID = value;
        }

        /// <summary>
        /// Whether the object allows accessories.
        /// </summary>
        public bool AllowAccessory
        {
            get => _blnAllowAccessory;
            set
            {
                if (value)
                    _blnAllowAccessory = true;
                else if (_blnAllowAccessory)
                {
                    _blnAllowAccessory = false;
                    foreach (Weapon objChild in UnderbarrelWeapons)
                        objChild.AllowAccessory = false;
                }
            }
        }

        /// <summary>
        /// Location.
        /// </summary>
        public Location Location
        {
            get; set;
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
        /// Vehicle to which the weapon is mounted (if none, returns null)
        /// </summary>
        public Vehicle ParentVehicle
        {
            get => _objMountedVehicle;
            set
            {
                if (Interlocked.Exchange(ref _objMountedVehicle, value) != value)
                {
                    foreach (WeaponAccessory objAccessory in WeaponAccessories)
                    {
                        foreach (Gear objGear in objAccessory.GearChildren)
                        {
                            if (value != null)
                                objGear.ChangeEquippedStatus(false);
                            else if (Equipped && objGear.Equipped)
                                objGear.ChangeEquippedStatus(true);
                        }
                    }
                }

                if (value == null)
                {
                    _objWeaponMount = null;
                    _objVehicleMod = null;
                }

                foreach (Weapon objChild in Children)
                    objChild.ParentVehicle = value;
            }
        }

        /// <summary>
        /// WeaponMount to which the weapon is mounted (if none, returns null)
        /// </summary>
        public WeaponMount ParentMount
        {
            get => _objWeaponMount;
            set
            {
                if (Interlocked.Exchange(ref _objWeaponMount, value) != value)
                {
                    ParentVehicle = value?.Parent;
                    if (value != null)
                        ParentVehicleMod = null;
                }
                foreach (Weapon objChild in Children)
                    objChild.ParentMount = value;
            }
        }

        /// <summary>
        /// VehicleMod to which the weapon is mounted (if none, returns null)
        /// </summary>
        public VehicleMod ParentVehicleMod
        {
            get => _objVehicleMod;
            set
            {
                if (Interlocked.Exchange(ref _objVehicleMod, value) != value)
                {
                    ParentVehicle = value?.Parent;
                    if (value != null)
                        ParentMount = null;
                }
                foreach (Weapon objChild in Children)
                    objChild.ParentVehicleMod = value;
            }
        }

        /// <summary>
        /// Whether or not the Underbarrel Weapon is part of the parent Weapon by default.
        /// </summary>
        public bool IncludedInWeapon
        {
            get => _blnIncludedInWeapon;
            set => _blnIncludedInWeapon = value;
        }

        /// <summary>
        /// Whether or not the Underbarrel Weapon is installed.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set
            {
                if (_blnEquipped == value)
                    return;
                _blnEquipped = value;
                if (ParentVehicle == null && _objCharacter?.IsLoading == false && (!string.IsNullOrEmpty(Weight)
                        || WeaponAccessories.Any(x => !string.IsNullOrEmpty(x.Weight)
                                                      || x.GearChildren.DeepAny(
                                                          y => y.Children, y => !string.IsNullOrEmpty(y.Weight)))
                        || Children.DeepAny(x => x.Children,
                                            z => !string.IsNullOrEmpty(z.Weight)
                                                 || WeaponAccessories.Any(
                                                     x => !string.IsNullOrEmpty(x.Weight)
                                                          || x.GearChildren.DeepAny(
                                                              y => y.Children, y => !string.IsNullOrEmpty(y.Weight))))))
                    _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
            }
        }

        /// <summary>
        /// Active Skill that should be used with this Weapon instead of the default one.
        /// </summary>
        public string UseSkill
        {
            get => _strUseSkill;
            set => _strUseSkill = value;
        }

        /// <summary>
        /// Active Skill Specialization that should be used with this Weapon instead of the default one.
        /// </summary>
        public string UseSkillSpec
        {
            get => string.IsNullOrWhiteSpace(_strUseSkillSpec) ? _strName : _strUseSkillSpec;
            set => _strUseSkillSpec = value;
        }

        /// <summary>
        /// Whether or not the Armor's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost;
            set => _blnDiscountCost = value;
        }

        /// <summary>
        /// Whether or not the Weapon requires Ammo to be reloaded.
        /// </summary>
        public bool RequireAmmo
        {
            get => _blnRequireAmmo;
            set => _blnRequireAmmo = value;
        }

        /// <summary>
        /// The Active Skill Specialization that this Weapon uses, in addition to any others it would normally use.
        /// </summary>
        public string Spec => _strSpec;

        /// <summary>
        /// The second Active Skill Specialization that this Weapon uses, in addition to any others it would normally use.
        /// </summary>
        public string Spec2 => _strSpec2;

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

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlNode objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("weapons.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("weapons.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/weapons/weapon", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/weapons/weapon", Name);
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
                ? _objCharacter.LoadDataXPath("weapons.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/weapons/weapon", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/weapons/weapon", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
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
        /// Whether or not the Weapon's wireless bonus is enabled
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

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        /// <summary>
        /// Name of Autosoft that is used for a weapon's dicepool. Melee weapons use Melee autosoft per R5 127
        /// </summary>
        private string RelevantAutosoft => RangeType == "Melee" ? "[Weapon] Melee Autosoft" : "[Weapon] Targeting Autosoft";

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications in the program's current language.
        /// </summary>
        public string DisplayConcealability => CalculatedConcealability(GlobalSettings.CultureInfo);

        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications.
        /// </summary>
        public string CalculatedConcealability(CultureInfo objCulture)
        {
            int intReturn = Concealability + WeaponAccessories.Sum(x => x.Equipped, x => x.TotalConcealability)
                                           // Factor in the character's Concealability modifiers.
                                           + ImprovementManager
                                             .ValueOf(_objCharacter, Improvement.ImprovementType.Concealability)
                                             .StandardRound();

            return intReturn >= 0 ? '+' + intReturn.ToString(objCulture) : intReturn.ToString(objCulture);
        }

        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications.
        /// </summary>
        public async ValueTask<string> CalculatedConcealabilityAsync(CultureInfo objCulture, CancellationToken token = default)
        {
            int intReturn = Concealability
                            + await WeaponAccessories
                                    .SumAsync(x => x.Equipped, x => x.GetTotalConcealabilityAsync(token).AsTask(),
                                              token: token).ConfigureAwait(false)
                            // Factor in the character's Concealability modifiers.
                            + (await ImprovementManager
                                     .ValueOfAsync(_objCharacter, Improvement.ImprovementType.Concealability,
                                                   token: token).ConfigureAwait(false)).StandardRound();
            return intReturn >= 0 ? '+' + intReturn.ToString(objCulture) : intReturn.ToString(objCulture);
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition in the program's current language.
        /// </summary>
        public string DisplayDamage => CalculatedDamage(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        public string CalculatedDamage(CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true)
        {
            return Utils.SafelyRunSynchronously(() => CalculatedDamageCoreAsync(true, objCulture, strLanguage, blnIncludeAmmo));
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        public Task<string> CalculatedDamageAsync(CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            return CalculatedDamageCoreAsync(false, objCulture, strLanguage, blnIncludeAmmo, token);
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        private async Task<string> CalculatedDamageCoreAsync(bool blnSync, CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            // If the cost is determined by the Rating, evaluate the expression.
            string strDamageType = string.Empty;
            string strDamageExtra = string.Empty;
            string strDamage;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdDamage))
            {
                sbdDamage.Append(Damage);
                if (blnSync)
                {
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    ProcessAttributesInXPath(sbdDamage, Damage);
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    sbdDamage.CheapReplace(Damage, "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                }
                else
                {
                    await ProcessAttributesInXPathAsync(sbdDamage, Damage, token: token).ConfigureAwait(false);
                    await sbdDamage.CheapReplaceAsync(Damage,
                                                      "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                }

                strDamage = sbdDamage.ToString();
            }

            // Evaluate the min expression if there is one.
            int intStart = strDamage.IndexOf("min(", StringComparison.Ordinal);
            if (intStart != -1)
            {
                int intEnd = strDamage.IndexOf(')', intStart);
                string strMin = strDamage.Substring(intStart, intEnd - intStart + 1);

                int intMinValue = int.MaxValue;
                foreach (string strValue in strMin.TrimStartOnce("min(", true).TrimEndOnce(')').SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    intMinValue = Math.Min(intMinValue, Convert.ToInt32(strValue, GlobalSettings.InvariantCultureInfo));
                }

                strDamage = strDamage.Replace(strMin, intMinValue.ToString(GlobalSettings.InvariantCultureInfo));
            }

            // Place the Damage Type (P or S) into a string and remove it from the expression.
            if (strDamage.Contains("P or S"))
            {
                strDamageType = "P or S";
                strDamage = strDamage.FastEscape("P or S");
            }
            else if (strDamage.Contains('P'))
            {
                strDamageType = "P";
                strDamage = strDamage.FastEscape('P');
            }
            else if (strDamage.Contains('S'))
            {
                strDamageType = "S";
                strDamage = strDamage.FastEscape('S');
            }
            else if (strDamage.Contains("(M)"))
            {
                strDamageType = "M";
                strDamage = strDamage.FastEscape("(M)");
            }
            // Place any extra text like (e) and (f) in a string and remove it from the expression.
            if (strDamage.Contains("(e)"))
            {
                strDamageExtra = "(e)";
                strDamage = strDamage.FastEscape("(e)");
            }
            else if (strDamage.Contains("(f)"))
            {
                strDamageExtra = "(f)";
                strDamage = strDamage.FastEscape("(f)");
            }
            else if (strDamage.Contains("(fire)"))
            {
                strDamageExtra = "(fire)";
                strDamage = strDamage.FastEscape("(fire)");
            }

            // Look for splash damage info.
            if (strDamage.ContainsAny("/m)", " Radius)"))
            {
                int intPos = strDamage.IndexOf('(');
                string strSplash = strDamage.Substring(intPos, strDamage.IndexOf(')') - intPos + 1);
                strDamageExtra += ' ' + strSplash;
                strDamage = strDamage.FastEscape(strSplash).Trim();
            }

            // Include WeaponCategoryDV Improvements.
            decimal decImprove = 0;
            if (_objCharacter != null)
            {
                string strCategory = Category;
                if (strCategory == "Unarmed")
                {
                    strCategory = "Unarmed Combat";
                }

                string strUseSkill = Skill?.DictionaryKey ?? string.Empty;
                if (blnSync)
                {
                    // ReSharper disable MethodHasAsyncOverload
                    decImprove += ImprovementManager.ValueOf(_objCharacter,
                                                             Improvement.ImprovementType.WeaponCategoryDV,
                                                             strImprovedName: strCategory, token: token);
                    if (!string.IsNullOrEmpty(strUseSkill) && strCategory != strUseSkill)
                        decImprove += ImprovementManager.ValueOf(_objCharacter,
                                                                 Improvement.ImprovementType.WeaponCategoryDV,
                                                                 strImprovedName: strUseSkill, token: token);
                    if (strCategory.StartsWith("Cyberware ", StringComparison.Ordinal))
                        decImprove += ImprovementManager.ValueOf(_objCharacter,
                                                                 Improvement.ImprovementType.WeaponCategoryDV,
                                                                 strImprovedName: strCategory.TrimStartOnce(
                                                                     "Cyberware ", true), token: token);

                    // If this is the Unarmed Attack Weapon and the character has the UnarmedDVPhysical Improvement, change the type to Physical.
                    // This should also add any UnarmedDV bonus which only applies to Unarmed Combat, not Unarmed Weapons.
                    if (Name == "Unarmed Attack")
                    {
                        if (strDamageType == "S" && ImprovementManager
                                                    .GetCachedImprovementListForValueOf(
                                                        _objCharacter, Improvement.ImprovementType.UnarmedDVPhysical, token: token)
                                                    .Count > 0)
                            strDamageType = "P";
                        decImprove += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedDV, token: token);
                    }

                    // This should also add any UnarmedDV bonus to Unarmed physical weapons if the option is enabled.
                    else if (strUseSkill == "Unarmed Combat"
                             && _objCharacter.Settings.UnarmedImprovementsApplyToWeapons)
                    {
                        decImprove += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedDV, token: token);
                    }
                    // ReSharper restore MethodHasAsyncOverload
                }
                else
                {
                    decImprove += await ImprovementManager.ValueOfAsync(_objCharacter,
                                                                        Improvement.ImprovementType.WeaponCategoryDV,
                                                                        strImprovedName: strCategory, token: token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strUseSkill) && strCategory != strUseSkill)
                        decImprove += await ImprovementManager.ValueOfAsync(_objCharacter,
                                                                            Improvement.ImprovementType.WeaponCategoryDV,
                                                                            strImprovedName: strUseSkill, token: token).ConfigureAwait(false);
                    if (strCategory.StartsWith("Cyberware ", StringComparison.Ordinal))
                        decImprove += await ImprovementManager.ValueOfAsync(_objCharacter,
                                                                            Improvement.ImprovementType.WeaponCategoryDV,
                                                                            strImprovedName: strCategory.TrimStartOnce(
                                                                                "Cyberware ", true), token: token).ConfigureAwait(false);

                    // If this is the Unarmed Attack Weapon and the character has the UnarmedDVPhysical Improvement, change the type to Physical.
                    // This should also add any UnarmedDV bonus which only applies to Unarmed Combat, not Unarmed Weapons.
                    if (Name == "Unarmed Attack")
                    {
                        if (strDamageType == "S" && (await ImprovementManager
                                                           .GetCachedImprovementListForValueOfAsync(
                                                               _objCharacter, Improvement.ImprovementType.UnarmedDVPhysical, token: token).ConfigureAwait(false))
                                                    .Count > 0)
                            strDamageType = "P";
                        decImprove += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.UnarmedDV, token: token).ConfigureAwait(false);
                    }

                    // This should also add any UnarmedDV bonus to Unarmed physical weapons if the option is enabled.
                    else if (strUseSkill == "Unarmed Combat"
                             && _objCharacter.Settings.UnarmedImprovementsApplyToWeapons)
                    {
                        decImprove += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.UnarmedDV, token: token).ConfigureAwait(false);
                    }
                }

                if (_objCharacter.Settings.MoreLethalGameplay)
                    decImprove += 2;
            }

            bool blnDamageReplaced = false;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                          out StringBuilder sbdBonusDamage))
            {
                // Add in the DV bonus from any Weapon Mods.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped)
                    {
                        if (!string.IsNullOrEmpty(objAccessory.DamageType))
                        {
                            strDamageType = string.Empty;
                            strDamageExtra = objAccessory.DamageType;
                        }

                        // Adjust the Weapon's Damage.
                        if (!string.IsNullOrEmpty(objAccessory.Damage))
                            sbdBonusDamage.Append(" + ").Append(objAccessory.Damage.TrimStartOnce('+'));
                        if (!string.IsNullOrEmpty(objAccessory.DamageReplacement))
                        {
                            blnDamageReplaced = true;
                            strDamage = objAccessory.DamageReplacement;
                        }
                    }
                }

                if (blnIncludeAmmo)
                {
                    // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
                    // Look for Ammo on the character.
                    Gear objGear = AmmoLoaded;
                    if (objGear != null)
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        {
                            if (objGear.FlechetteWeaponBonus["damagetype"] != null)
                            {
                                strDamageType = string.Empty;
                                strDamageExtra = objGear.FlechetteWeaponBonus["damagetype"].InnerText;
                            }

                            // Adjust the Weapon's Damage.
                            string strTemp = objGear.FlechetteWeaponBonus["damage"]?.InnerText;
                            if (!string.IsNullOrEmpty(strTemp))
                                sbdBonusDamage.Append(" + ").Append(strTemp.TrimStartOnce('+'));
                            strTemp = objGear.FlechetteWeaponBonus["damagereplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strTemp))
                            {
                                blnDamageReplaced = true;
                                strDamage = strTemp;
                            }
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            if (objGear.WeaponBonus["damagetype"] != null)
                            {
                                strDamageType = string.Empty;
                                strDamageExtra = objGear.WeaponBonus["damagetype"].InnerText;
                            }

                            // Adjust the Weapon's Damage.
                            string strTemp = objGear.WeaponBonus["damage"]?.InnerText;
                            if (!string.IsNullOrEmpty(strTemp))
                                sbdBonusDamage.Append(" + ").Append(strTemp.TrimStartOnce('+'));
                            strTemp = objGear.WeaponBonus["damagereplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strTemp))
                            {
                                blnDamageReplaced = true;
                                strDamage = strTemp;
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in blnSync
                                     ? objGear.Children.DeepWhere(x => x.Children, x => x.Equipped, token)
                                     : await objGear.Children.DeepWhereAsync(x => x.Children, x => x.Equipped, token)
                                                    .ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear"
                                                       && objChild.FlechetteWeaponBonus != null)
                            {
                                if (objChild.FlechetteWeaponBonus["damagetype"] != null)
                                {
                                    strDamageType = string.Empty;
                                    strDamageExtra = objChild.FlechetteWeaponBonus["damagetype"].InnerText;
                                }

                                // Adjust the Weapon's Damage.
                                string strTemp = objGear.FlechetteWeaponBonus["damage"]?.InnerText;
                                if (!string.IsNullOrEmpty(strTemp))
                                    sbdBonusDamage.Append(" + ").Append(strTemp.TrimStartOnce('+'));
                                strTemp = objGear.FlechetteWeaponBonus["damagereplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strTemp))
                                {
                                    blnDamageReplaced = true;
                                    strDamage = strTemp;
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                if (objChild.WeaponBonus["damagetype"] != null)
                                {
                                    strDamageType = string.Empty;
                                    strDamageExtra = objChild.WeaponBonus["damagetype"].InnerText;
                                }

                                // Adjust the Weapon's Damage.
                                string strTemp = objGear.WeaponBonus["damage"]?.InnerText;
                                if (!string.IsNullOrEmpty(strTemp))
                                    sbdBonusDamage.Append(" + ").Append(strTemp.TrimStartOnce('+'));
                                strTemp = objGear.WeaponBonus["damagereplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strTemp))
                                {
                                    blnDamageReplaced = true;
                                    strDamage = strTemp;
                                }
                            }
                        }
                    }
                }

                if (sbdBonusDamage.Length > 0)
                    strDamage += sbdBonusDamage.ToString();
            }

            string strReturn;
            if (!blnDamageReplaced)
            {
                if (string.IsNullOrEmpty(strDamage))
                    strReturn = strDamageType + strDamageExtra;
                else if (strDamage.Contains("//"))
                    strReturn = strDamage.Replace("//", "/") + strDamageType + strDamageExtra;
                else
                {
                    // Replace the division sign with "div" since we're using XPath.
                    strDamage = strDamage.Replace("/", " div ");
                    try
                    {
                        (bool blnIsSuccess, object objProcess) = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? CommonFunctions.EvaluateInvariantXPath(strDamage, token)
                            : await CommonFunctions.EvaluateInvariantXPathAsync(strDamage, token).ConfigureAwait(false);
                        if (blnIsSuccess)
                        {
                            int intDamage = (Convert.ToDecimal((double)objProcess) + decImprove).StandardRound();
                            if (Name == "Unarmed Attack (Smashing Blow)")
                                intDamage *= 2;
                            strDamage = intDamage.ToString(objCulture);
                            strReturn = strDamage + strDamageType + strDamageExtra;
                        }
                        else
                        {
                            strReturn = "NaN";
                        }
                    }
                    catch (OverflowException)
                    {
                        strReturn = "NaN";
                    } // Result is text and not a double
                    catch (InvalidCastException)
                    {
                        strReturn = "NaN";
                    } // Result is text and not a double
                }
            }
            else
            {
                string strOriginalDamage = strDamage;
                // Place the Damage Type (P or S) into a string and remove it from the expression.
                if (strDamage.Contains("P or S"))
                {
                    strDamageType = "P or S";
                    strDamage = strDamage.FastEscape("P or S");
                }
                else if (strDamage.Contains('P'))
                {
                    strDamageType = "P";
                    strDamage = strDamage.FastEscape('P');
                }
                else if (strDamage.Contains('S'))
                {
                    strDamageType = "S";
                    strDamage = strDamage.FastEscape('S');
                }
                else if (strDamage.Contains("(M)"))
                {
                    strDamageType = "M";
                    strDamage = strDamage.FastEscape("(M)");
                }
                // Place any extra text like (e) and (f) in a string and remove it from the expression.
                if (strDamage.Contains("(e)"))
                {
                    strDamageExtra = "(e)";
                    strDamage = strDamage.FastEscape("(e)");
                }
                else if (strDamage.Contains("(f)"))
                {
                    strDamageExtra = "(f)";
                    strDamage = strDamage.FastEscape("(f)");
                }
                else if (strDamage.Contains("(fire)"))
                {
                    strDamageExtra = "(fire)";
                    strDamage = strDamage.FastEscape("(fire)");
                }

                if (string.IsNullOrEmpty(strDamage))
                    strReturn = strDamageType + strDamageExtra;
                else if (strDamage.Contains("//"))
                    strReturn = strDamage.Replace("//", "/") + strDamageType + strDamageExtra;
                else
                {
                    // Replace the division sign with "div" since we're using XPath.
                    strDamage = strDamage.Replace("/", " div ");
                    try
                    {
                        (bool blnIsSuccess, object objProcess) = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? CommonFunctions.EvaluateInvariantXPath(strDamage, token)
                            : await CommonFunctions.EvaluateInvariantXPathAsync(strDamage, token).ConfigureAwait(false);
                        if (blnIsSuccess)
                        {
                            int intDamage = (Convert.ToDecimal((double)objProcess) + decImprove).StandardRound();
                            if (Name == "Unarmed Attack (Smashing Blow)")
                                intDamage *= 2;
                            strDamage = intDamage.ToString(objCulture);
                            strReturn = strDamage + strDamageType + strDamageExtra;
                        }
                        else
                        {
                            strReturn = strOriginalDamage;
                        }
                    }
                    catch (OverflowException)
                    {
                        strReturn = strOriginalDamage;
                    } // Result is text and not a double
                    catch (InvalidCastException)
                    {
                        strReturn = strOriginalDamage;
                    } // Result is text and not a double
                }
            }

            // If the string couldn't be parsed (resulting in NaN which will happen if it is a special string like "Grenade", "Chemical", etc.), return the Weapon's Damage string.
            if (strReturn.StartsWith("NaN", StringComparison.Ordinal))
                strReturn = Damage;

            // Translate the Damage Code.
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                if (blnSync)
                {
                    // ReSharper disable MethodHasAsyncOverloadWithCancellation
                    Lazy<string> strStun
                        = new Lazy<string>(
                            () => LanguageManager.GetString("String_DamageStun", strLanguage, token: token));
                    Lazy<string> strPhysical
                        = new Lazy<string>(
                            () => LanguageManager.GetString("String_DamagePhysical", strLanguage, token: token));
                    // ReSharper disable once MethodHasAsyncOverload
                    strReturn = ReplaceStrings(strReturn, strLanguage, token)
                                .CheapReplace("0S", () => '0' + strStun.Value)
                                .CheapReplace("1S", () => '1' + strStun.Value)
                                .CheapReplace("2S", () => '2' + strStun.Value)
                                .CheapReplace("3S", () => '3' + strStun.Value)
                                .CheapReplace("4S", () => '4' + strStun.Value)
                                .CheapReplace("5S", () => '5' + strStun.Value)
                                .CheapReplace("6S", () => '6' + strStun.Value)
                                .CheapReplace("7S", () => '7' + strStun.Value)
                                .CheapReplace("8S", () => '8' + strStun.Value)
                                .CheapReplace("9S", () => '9' + strStun.Value)
                                .CheapReplace("0P", () => '0' + strPhysical.Value)
                                .CheapReplace("1P", () => '1' + strPhysical.Value)
                                .CheapReplace("2P", () => '2' + strPhysical.Value)
                                .CheapReplace("3P", () => '3' + strPhysical.Value)
                                .CheapReplace("4P", () => '4' + strPhysical.Value)
                                .CheapReplace("5P", () => '5' + strPhysical.Value)
                                .CheapReplace("6P", () => '6' + strPhysical.Value)
                                .CheapReplace("7P", () => '7' + strPhysical.Value)
                                .CheapReplace("8P", () => '8' + strPhysical.Value)
                                .CheapReplace("9P", () => '9' + strPhysical.Value);
                    // ReSharper restore MethodHasAsyncOverloadWithCancellation
                }
                else
                {
                    AsyncLazy<string> strStun = new AsyncLazy<string>(
                        () => LanguageManager.GetStringAsync("String_DamageStun", strLanguage, token: token),
                        Utils.JoinableTaskFactory);
                    AsyncLazy<string> strPhysical = new AsyncLazy<string>(
                        () => LanguageManager.GetStringAsync("String_DamagePhysical", strLanguage, token: token),
                        Utils.JoinableTaskFactory);
                    strReturn = await ReplaceStringsAsync(strReturn, strLanguage, token).ConfigureAwait(false);
                    strReturn = await strReturn
                                      .CheapReplaceAsync(
                                          "0S",
                                          async () => '0' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "1S",
                                          async () => '1' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "2S",
                                          async () => '2' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "3S",
                                          async () => '3' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "4S",
                                          async () => '4' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "5S",
                                          async () => '5' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "6S",
                                          async () => '6' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "7S",
                                          async () => '7' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "8S",
                                          async () => '8' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "9S",
                                          async () => '9' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "0P",
                                          async () => '0'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "1P",
                                          async () => '1'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "2P",
                                          async () => '2'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "3P",
                                          async () => '3'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "4P",
                                          async () => '4'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "5P",
                                          async () => '5'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "6P",
                                          async () => '6'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "7P",
                                          async () => '7'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "8P",
                                          async () => '8'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token)
                                      .CheapReplaceAsync(
                                          "9P",
                                          async () => '9'
                                                      + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                                          token: token).ConfigureAwait(false);
                }
            }

            return strReturn;
        }

        public static string ReplaceStrings(string strInput, string strLanguage, CancellationToken token = default)
        {
            return strLanguage == GlobalSettings.DefaultLanguage
                ? strInput
                : strInput
                  .CheapReplace(
                      "Special", () => LanguageManager.GetString("String_DamageSpecial", strLanguage, token: token))
                  .CheapReplace(
                      "P or S", () => LanguageManager.GetString("String_DamagePOrS", strLanguage, token: token))
                  .CheapReplace(
                      "Chemical", () => LanguageManager.GetString("String_DamageChemical", strLanguage, token: token))
                  .CheapReplace(
                      "(e)", () => LanguageManager.GetString("String_DamageElectric", strLanguage, token: token))
                  .CheapReplace(
                      "(f)", () => LanguageManager.GetString("String_DamageFlechette", strLanguage, token: token))
                  .CheapReplace(
                      "(fire)", () => LanguageManager.GetString("String_DamageFire", strLanguage, token: token))
                  .CheapReplace(
                      "Grenade", () => LanguageManager.GetString("String_DamageGrenade", strLanguage, token: token))
                  .CheapReplace(
                      "Missile", () => LanguageManager.GetString("String_DamageMissile", strLanguage, token: token))
                  .CheapReplace(
                      "Mortar", () => LanguageManager.GetString("String_DamageMortar", strLanguage, token: token))
                  .CheapReplace(
                      "Rocket", () => LanguageManager.GetString("String_DamageRocket", strLanguage, token: token))
                  .CheapReplace(
                      "Torpedo", () => LanguageManager.GetString("String_DamageTorpedo", strLanguage, token: token))
                  .CheapReplace(
                      "Radius", () => LanguageManager.GetString("String_DamageRadius", strLanguage, token: token))
                  .CheapReplace("As Drug/Toxin",
                                () => LanguageManager.GetString("String_DamageAsDrugToxin", strLanguage, token: token))
                  .CheapReplace(
                      "as round", () => LanguageManager.GetString("String_DamageAsRound", strLanguage, token: token))
                  .CheapReplace(
                      "/m", () => '/' + LanguageManager.GetString("String_DamageMeter", strLanguage, token: token))
                  .CheapReplace(
                      "(M)", () => LanguageManager.GetString("String_DamageMatrix", strLanguage, token: token));
        }

        public static async ValueTask<string> ReplaceStringsAsync(string strInput, string strLanguage, CancellationToken token = default)
        {
            return strLanguage == GlobalSettings.DefaultLanguage
                ? strInput
                : await strInput
                        .CheapReplaceAsync(
                            "Special",
                            () => LanguageManager.GetStringAsync("String_DamageSpecial", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "P or S", () => LanguageManager.GetStringAsync("String_DamagePOrS", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "Chemical",
                            () => LanguageManager.GetStringAsync("String_DamageChemical", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "(e)", () => LanguageManager.GetStringAsync("String_DamageElectric", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "(f)", () => LanguageManager.GetStringAsync("String_DamageFlechette", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "(fire)", () => LanguageManager.GetStringAsync("String_DamageFire", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "Grenade",
                            () => LanguageManager.GetStringAsync("String_DamageGrenade", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "Missile",
                            () => LanguageManager.GetStringAsync("String_DamageMissile", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "Mortar", () => LanguageManager.GetStringAsync("String_DamageMortar", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "Rocket", () => LanguageManager.GetStringAsync("String_DamageRocket", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "Torpedo", () => LanguageManager.GetStringAsync("String_DamageTorpedo", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "Radius", () => LanguageManager.GetStringAsync("String_DamageRadius", strLanguage, token: token), token: token)
                        .CheapReplaceAsync("As Drug/Toxin",
                                           () => LanguageManager.GetStringAsync(
                                               "String_DamageAsDrugToxin", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "as round",
                            () => LanguageManager.GetStringAsync("String_DamageAsRound", strLanguage, token: token), token: token)
                        .CheapReplaceAsync(
                            "/m",
                            async () => '/' + await LanguageManager.GetStringAsync("String_DamageMeter", strLanguage, token: token).ConfigureAwait(false), token: token)
                        .CheapReplaceAsync(
                            "(M)", () => LanguageManager.GetStringAsync("String_DamageMatrix", strLanguage, token: token), token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Calculated Ammo capacity in the program's current language.
        /// </summary>
        public string DisplayAmmo => CalculatedAmmo(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        public string CalculatedAmmo(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => CalculatedAmmoCoreAsync(true, objCulture, strLanguage, token), token);
        }

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        public Task<string> CalculatedAmmoAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return CalculatedAmmoCoreAsync(false, objCulture, strLanguage, token);
        }

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        private async Task<string> CalculatedAmmoCoreAsync(bool blnSync, CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            IEnumerable<string> lstAmmos = Ammo.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries);
            int intAmmoBonus = 0;

            if (WeaponAccessories.Count != 0)
            {
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped)
                    {
                        // Replace the Ammo value.
                        if (!string.IsNullOrEmpty(objAccessory.AmmoReplace))
                        {
                            lstAmmos = objAccessory.AmmoReplace.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries);
                        }

                        intAmmoBonus += objAccessory.TotalAmmoBonus;
                    }
                }
            }

            int intAmmoBonusFlat = 0;
            decimal decAmmoBonusPercent = 1.0m;
            if (ParentMount != null)
            {
                foreach (VehicleMod objMod in ParentMount.Mods)
                {
                    if (!string.IsNullOrEmpty(objMod.AmmoReplace))
                    {
                        lstAmmos = objMod.AmmoReplace.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries);
                    }
                    intAmmoBonusFlat += objMod.AmmoBonus;
                    if (objMod.AmmoBonusPercent != 0)
                    {
                        decAmmoBonusPercent *= objMod.AmmoBonusPercent / 100.0m;
                    }
                }
            }

            string strSpace = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LanguageManager.GetString("String_Space", strLanguage, token: token)
                : await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                foreach (string strAmmo in lstAmmos)
                {
                    string strThisAmmo = strAmmo;
                    int intPos = strThisAmmo.IndexOf('(');
                    if (intPos != -1)
                    {
                        string strPrepend = string.Empty;
                        strThisAmmo = strThisAmmo.Substring(0, intPos);
                        intPos = strThisAmmo.IndexOf('x');
                        if (intPos != -1)
                        {
                            strPrepend = strThisAmmo.Substring(0, intPos + 1);
                            strThisAmmo = strThisAmmo.Substring(intPos + 1, strThisAmmo.Length - (intPos + 1));
                        }

                        if (WeaponAccessories.Count != 0)
                        {
                            foreach (WeaponAccessory objAccessory in WeaponAccessories)
                            {
                                if (objAccessory.Equipped)
                                {
                                    string strModifyAmmoCapacity = objAccessory.ModifyAmmoCapacity;
                                    if (!string.IsNullOrEmpty(strModifyAmmoCapacity))
                                    {
                                        using (new FetchSafelyFromPool<StringBuilder>(
                                                   Utils.StringBuilderPool, out StringBuilder sbdThisAmmo))
                                        {
                                            sbdThisAmmo.Append('(').Append(strThisAmmo).Append(strModifyAmmoCapacity)
                                                       .Append(')');
                                            int intAddParenthesesCount = strModifyAmmoCapacity.Count(x => x == ')')
                                                                         - strModifyAmmoCapacity.Count(x => x == '(');
                                            for (int i = 0; i < intAddParenthesesCount; ++i)
                                                sbdThisAmmo.Insert(0, '(');
                                            for (int i = 0; i < -intAddParenthesesCount; ++i)
                                                sbdThisAmmo.Append(')');
                                            strThisAmmo = sbdThisAmmo.ToString();
                                        }
                                    }
                                }
                            }
                        }

                        strThisAmmo = blnSync
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            ? strThisAmmo.CheapReplace("Weapon", () => AmmoCapacity(Ammo))
                            : await strThisAmmo.CheapReplaceAsync("Weapon", () => AmmoCapacity(Ammo), token: token).ConfigureAwait(false);
                        // Replace the division sign with "div" since we're using XPath.
                        strThisAmmo = strThisAmmo.Replace("/", " div ");
                        (bool blnIsSuccess, object objProcess) = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? CommonFunctions.EvaluateInvariantXPath(strThisAmmo, token)
                            : await CommonFunctions.EvaluateInvariantXPathAsync(strThisAmmo, token).ConfigureAwait(false);
                        if (blnIsSuccess)
                        {
                            int intAmmo = ((double)objProcess).StandardRound() + intAmmoBonusFlat;

                            intAmmo += (intAmmo * intAmmoBonus + 99) / 100;

                            if (decAmmoBonusPercent != 1.0m)
                            {
                                intAmmo = (intAmmo * decAmmoBonusPercent).StandardRound();
                            }

                            strThisAmmo = intAmmo.ToString(objCulture)
                                          + strAmmo.Substring(strAmmo.IndexOf('('),
                                                              strAmmo.Length - strAmmo.IndexOf('('));
                        }

                        if (!string.IsNullOrEmpty(strPrepend))
                            strThisAmmo = strPrepend + strThisAmmo;
                    }

                    sbdReturn.Append(strThisAmmo).Append(strSpace);
                }

                string strReturn = sbdReturn.ToString().Trim();

                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                {
                    // Translate the Ammo string.
                    if (blnSync)
                    {
                        // ReSharper disable MethodHasAsyncOverloadWithCancellation
                        strReturn = strReturn
                                    .CheapReplace(
                                        " or ",
                                        () => strSpace + LanguageManager.GetString("String_Or", strLanguage, token: token) + strSpace,
                                        StringComparison.OrdinalIgnoreCase)
                                    .CheapReplace(
                                        " Belt", () => LanguageManager.GetString("String_AmmoBelt", strLanguage, token: token),
                                        StringComparison.OrdinalIgnoreCase)
                                    .CheapReplace(
                                        " Energy", () => LanguageManager.GetString("String_AmmoEnergy", strLanguage, token: token),
                                        StringComparison.OrdinalIgnoreCase)
                                    .CheapReplace(" External Source",
                                                  () => LanguageManager.GetString(
                                                      "String_AmmoExternalSource", strLanguage, token: token),
                                                  StringComparison.OrdinalIgnoreCase)
                                    .CheapReplace(
                                        " Special", () => LanguageManager.GetString("String_AmmoSpecial", strLanguage, token: token),
                                        StringComparison.OrdinalIgnoreCase)
                                    .CheapReplace(
                                        "(b)",
                                        () => '(' + LanguageManager.GetString("String_AmmoBreakAction", strLanguage, token: token)
                                                  + ')')
                                    .CheapReplace(
                                        "(belt)",
                                        () => '(' + LanguageManager.GetString("String_AmmoBelt", strLanguage, token: token) + ')')
                                    .CheapReplace(
                                        "(box)",
                                        () => '(' + LanguageManager.GetString("String_AmmoBox", strLanguage, token: token) + ')')
                                    .CheapReplace(
                                        "(c)",
                                        () => '(' + LanguageManager.GetString("String_AmmoClip", strLanguage, token: token) + ')')
                                    .CheapReplace(
                                        "(cy)",
                                        () => '(' + LanguageManager.GetString("String_AmmoCylinder", strLanguage, token: token) + ')')
                                    .CheapReplace(
                                        "(d)",
                                        () => '(' + LanguageManager.GetString("String_AmmoDrum", strLanguage, token: token) + ')')
                                    .CheapReplace(
                                        "(m)",
                                        () => '(' + LanguageManager.GetString("String_AmmoMagazine", strLanguage, token: token) + ')')
                                    .CheapReplace(
                                        "(ml)",
                                        () => '(' + LanguageManager.GetString("String_AmmoMuzzleLoad", strLanguage, token: token)
                                                  + ')');
                        // ReSharper restore MethodHasAsyncOverloadWithCancellation
                    }
                    else
                    {
                        strReturn = await strReturn
                                          .CheapReplaceAsync(
                                              " or ",
                                              async () => strSpace + await LanguageManager.GetStringAsync("String_Or", strLanguage, token: token).ConfigureAwait(false)
                                                                   + strSpace,
                                              StringComparison.OrdinalIgnoreCase, token: token)
                                          .CheapReplaceAsync(
                                              " Belt",
                                              () => LanguageManager.GetStringAsync("String_AmmoBelt", strLanguage, token: token),
                                              StringComparison.OrdinalIgnoreCase, token: token)
                                          .CheapReplaceAsync(
                                              " Energy",
                                              () => LanguageManager.GetStringAsync("String_AmmoEnergy", strLanguage, token: token),
                                              StringComparison.OrdinalIgnoreCase, token: token)
                                          .CheapReplaceAsync(" External Source",
                                                             () => LanguageManager.GetStringAsync(
                                                                 "String_AmmoExternalSource", strLanguage, token: token),
                                                             StringComparison.OrdinalIgnoreCase, token: token)
                                          .CheapReplaceAsync(
                                              " Special",
                                              () => LanguageManager.GetStringAsync("String_AmmoSpecial", strLanguage, token: token),
                                              StringComparison.OrdinalIgnoreCase, token: token)
                                          .CheapReplaceAsync(
                                              "(b)",
                                              async () => '('
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_AmmoBreakAction", strLanguage, token: token).ConfigureAwait(false) + ')', token: token)
                                          .CheapReplaceAsync(
                                              "(belt)",
                                              async () => '('
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_AmmoBelt", strLanguage, token: token).ConfigureAwait(false) + ')', token: token)
                                          .CheapReplaceAsync(
                                              "(box)",
                                              async () => '('
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_AmmoBox", strLanguage, token: token).ConfigureAwait(false) + ')', token: token)
                                          .CheapReplaceAsync(
                                              "(c)",
                                              async () => '('
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_AmmoClip", strLanguage, token: token).ConfigureAwait(false) + ')', token: token)
                                          .CheapReplaceAsync(
                                              "(cy)",
                                              async () => '('
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_AmmoCylinder", strLanguage, token: token).ConfigureAwait(false) + ')', token: token)
                                          .CheapReplaceAsync(
                                              "(d)",
                                              async () => '('
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_AmmoDrum", strLanguage, token: token).ConfigureAwait(false) + ')', token: token)
                                          .CheapReplaceAsync(
                                              "(m)",
                                              async () => '('
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_AmmoMagazine", strLanguage, token: token).ConfigureAwait(false) + ')', token: token)
                                          .CheapReplaceAsync(
                                              "(ml)",
                                              async () => '('
                                                          + await LanguageManager.GetStringAsync(
                                                              "String_AmmoMuzzleLoad", strLanguage, token: token).ConfigureAwait(false) + ')', token: token).ConfigureAwait(false);
                    }
                }

                return strReturn;
            }
        }

        public bool AllowSingleShot => RangeType == "Melee"
                                       && Ammo != "0"
                                       || _blnAllowSingleShot
                                       && (AllowMode(LanguageManager.GetString("String_ModeSingleShot"))
                                           || AllowMode(LanguageManager.GetString("String_ModeSemiAutomatic")));

        public bool AllowShortBurst => _blnAllowShortBurst
                                       && (AllowMode(LanguageManager.GetString("String_ModeBurstFire"))
                                           || AllowMode(LanguageManager.GetString("String_ModeSemiAutomatic"))
                                           || AllowMode(LanguageManager.GetString("String_ModeFullAutomatic")));

        public bool AllowLongBurst => _blnAllowLongBurst
                                      && (AllowMode(LanguageManager.GetString("String_ModeBurstFire"))
                                          || AllowMode(LanguageManager.GetString("String_ModeFullAutomatic")));

        public bool AllowFullBurst => _blnAllowFullBurst
                                      && AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));

        public bool AllowSuppressive => _blnAllowSuppressive
                                        && AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));

        /// <summary>
        /// The Weapon's Firing Mode including Modifications in the program's current language.
        /// </summary>
        public string DisplayMode => CalculatedMode(GlobalSettings.Language);

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        public string CalculatedMode(string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => CalculatedModeCoreAsync(true, strLanguage, blnIncludeAmmo, token), token);
        }

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        public Task<string> CalculatedModeAsync(string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            return CalculatedModeCoreAsync(false, strLanguage, blnIncludeAmmo, token);
        }

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        private async Task<string> CalculatedModeCoreAsync(bool blnSync, string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            // Move the contents of the array to a list so it's easier to work with.
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setModes))
            using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                            out HashSet<string> setNewModes))
            {
                setModes.AddRange(_strMode.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries));

                if (blnIncludeAmmo)
                {
                    // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
                    // Look for Ammo on the character.
                    Gear objGear = AmmoLoaded;
                    if (objGear != null)
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        {
                            string strFireMode = objGear.FlechetteWeaponBonus["firemode"]?.InnerText;
                            if (!string.IsNullOrEmpty(strFireMode))
                            {
                                if (strFireMode.Contains('/'))
                                {
                                    // Move the contents of the array to a list so it's easier to work with.
                                    foreach (string strMode in strFireMode.SplitNoAlloc(
                                                 '/', StringSplitOptions.RemoveEmptyEntries))
                                        setNewModes.Add(strMode);
                                }
                                else
                                {
                                    setNewModes.Add(strFireMode);
                                }
                            }

                            strFireMode = objGear.FlechetteWeaponBonus["modereplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strFireMode))
                            {
                                setModes.Clear();
                                if (strFireMode.Contains('/'))
                                {
                                    // Move the contents of the array to a list so it's easier to work with.
                                    foreach (string strMode in strFireMode.SplitNoAlloc(
                                                 '/', StringSplitOptions.RemoveEmptyEntries))
                                        setModes.Add(strMode);
                                }
                                else
                                {
                                    setModes.Add(strFireMode);
                                }
                            }
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            string strFireMode = objGear.WeaponBonus["firemode"]?.InnerText;
                            if (!string.IsNullOrEmpty(strFireMode))
                            {
                                if (strFireMode.Contains('/'))
                                {
                                    // Move the contents of the array to a list so it's easier to work with.
                                    foreach (string strMode in strFireMode.SplitNoAlloc(
                                                 '/', StringSplitOptions.RemoveEmptyEntries))
                                        setNewModes.Add(strMode);
                                }
                                else
                                {
                                    setNewModes.Add(strFireMode);
                                }
                            }

                            strFireMode = objGear.WeaponBonus["modereplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strFireMode))
                            {
                                setModes.Clear();
                                if (strFireMode.Contains('/'))
                                {
                                    // Move the contents of the array to a list so it's easier to work with.
                                    foreach (string strMode in strFireMode.SplitNoAlloc(
                                                 '/', StringSplitOptions.RemoveEmptyEntries))
                                        setModes.Add(strMode);
                                }
                                else
                                {
                                    setModes.Add(strFireMode);
                                }
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in blnSync
                                     ? objGear.Children.DeepWhere(x => x.Children, x => x.Equipped, token)
                                     : await objGear.Children.DeepWhereAsync(x => x.Children, x => x.Equipped, token)
                                                    .ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear"
                                                       && objChild.FlechetteWeaponBonus != null)
                            {
                                string strFireMode = objChild.FlechetteWeaponBonus["firemode"]?.InnerText;
                                if (!string.IsNullOrEmpty(strFireMode))
                                {
                                    if (strFireMode.Contains('/'))
                                    {
                                        // Move the contents of the array to a list so it's easier to work with.
                                        foreach (string strMode in strFireMode.SplitNoAlloc(
                                                     '/', StringSplitOptions.RemoveEmptyEntries))
                                            setNewModes.Add(strMode);
                                    }
                                    else
                                    {
                                        setNewModes.Add(strFireMode);
                                    }
                                }

                                strFireMode = objChild.FlechetteWeaponBonus["modereplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strFireMode))
                                {
                                    setModes.Clear();
                                    if (strFireMode.Contains('/'))
                                    {
                                        // Move the contents of the array to a list so it's easier to work with.
                                        foreach (string strMode in strFireMode.SplitNoAlloc(
                                                     '/', StringSplitOptions.RemoveEmptyEntries))
                                            setModes.Add(strMode);
                                    }
                                    else
                                    {
                                        setModes.Add(strFireMode);
                                    }
                                }
                            }
                            else if (objGear.WeaponBonus != null)
                            {
                                string strFireMode = objChild.WeaponBonus["firemode"]?.InnerText;
                                if (!string.IsNullOrEmpty(strFireMode))
                                {
                                    if (strFireMode.Contains('/'))
                                    {
                                        // Move the contents of the array to a list so it's easier to work with.
                                        foreach (string strMode in strFireMode.SplitNoAlloc(
                                                     '/', StringSplitOptions.RemoveEmptyEntries))
                                            setNewModes.Add(strMode);
                                    }
                                    else
                                    {
                                        setNewModes.Add(strFireMode);
                                    }
                                }

                                strFireMode = objChild.WeaponBonus["modereplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strFireMode))
                                {
                                    setModes.Clear();
                                    if (strFireMode.Contains('/'))
                                    {
                                        // Move the contents of the array to a list so it's easier to work with.
                                        foreach (string strMode in strFireMode.SplitNoAlloc(
                                                     '/', StringSplitOptions.RemoveEmptyEntries))
                                            setModes.Add(strMode);
                                    }
                                    else
                                    {
                                        setModes.Add(strFireMode);
                                    }
                                }
                            }
                        }
                    }
                }

                // Do the same for any accessories/modifications.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (!objAccessory.Equipped)
                        continue;
                    if (!string.IsNullOrEmpty(objAccessory.FireMode))
                    {
                        if (objAccessory.FireMode.Contains('/'))
                        {
                            // Move the contents of the array to a list so it's easier to work with.
                            foreach (string strMode in objAccessory.FireMode.SplitNoAlloc(
                                         '/', StringSplitOptions.RemoveEmptyEntries))
                                setNewModes.Add(strMode);
                        }
                        else
                        {
                            setNewModes.Add(objAccessory.FireMode);
                        }
                    }

                    if (!string.IsNullOrEmpty(objAccessory.FireModeReplacement))
                    {
                        setModes.Clear();
                        if (objAccessory.FireModeReplacement.Contains('/'))
                        {
                            // Move the contents of the array to a list so it's easier to work with.
                            foreach (string strMode in objAccessory.FireModeReplacement.SplitNoAlloc(
                                         '/', StringSplitOptions.RemoveEmptyEntries))
                                setModes.Add(strMode);
                        }
                        else
                        {
                            setModes.Add(objAccessory.FireModeReplacement);
                        }
                    }
                }

                setModes.UnionWith(setNewModes);

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                {
                    if (blnSync)
                    {
                        if (setModes.Contains("SS"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeSingleShot", strLanguage, token: token))
                                     .Append('/');
                        if (setModes.Contains("SA"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeSemiAutomatic", strLanguage, token: token))
                                     .Append('/');
                        if (setModes.Contains("BF"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeBurstFire", strLanguage, token: token))
                                     .Append('/');
                        if (setModes.Contains("FA"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeFullAutomatic", strLanguage, token: token))
                                     .Append('/');
                        if (setModes.Contains("Special"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeSpecial", strLanguage, token: token)).Append('/');
                    }
                    else
                    {
                        if (setModes.Contains("SS"))
                            sbdReturn.Append(await LanguageManager.GetStringAsync("String_ModeSingleShot", strLanguage, token: token).ConfigureAwait(false))
                                     .Append('/');
                        if (setModes.Contains("SA"))
                            sbdReturn.Append(await LanguageManager.GetStringAsync("String_ModeSemiAutomatic", strLanguage, token: token).ConfigureAwait(false))
                                     .Append('/');
                        if (setModes.Contains("BF"))
                            sbdReturn.Append(await LanguageManager.GetStringAsync("String_ModeBurstFire", strLanguage, token: token).ConfigureAwait(false))
                                     .Append('/');
                        if (setModes.Contains("FA"))
                            sbdReturn.Append(await LanguageManager.GetStringAsync("String_ModeFullAutomatic", strLanguage, token: token).ConfigureAwait(false))
                                     .Append('/');
                        if (setModes.Contains("Special"))
                            sbdReturn.Append(await LanguageManager.GetStringAsync("String_ModeSpecial", strLanguage, token: token).ConfigureAwait(false)).Append('/');
                    }

                    // Remove the trailing "/".
                    if (sbdReturn.Length > 0)
                        --sbdReturn.Length;

                    return sbdReturn.ToString();
                }
            }
        }

        /// <summary>
        /// Determine if the Weapon is capable of firing in a particular mode.
        /// </summary>
        /// <param name="strFindMode">Firing mode to find.</param>
        /// <param name="strLanguage">Language of <paramref name="strFindMode"/>. Uses current UI language if unset.</param>
        public bool AllowMode(string strFindMode, string strLanguage = "")
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return CalculatedMode(strLanguage).SplitNoAlloc('/').Contains(strFindMode);
        }

        /// <summary>
        /// Weapon Cost to use when working with Total Cost price modifiers for Weapon Mods.
        /// </summary>
        public decimal MultipliableCost(WeaponAccessory objExcludeAccessory, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return OwnCost
                   // Run through the list of Weapon Mods.
                   + WeaponAccessories.Sum(x => objExcludeAccessory != x && x.Equipped && !x.IncludedInWeapon, x => x.TotalCost, token);
        }

        /// <summary>
        /// Weapon Cost to use when working with Total Cost price modifiers for Weapon Mods.
        /// </summary>
        public async ValueTask<decimal> MultipliableCostAsync(WeaponAccessory objExcludeAccessory, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await GetOwnCostAsync(token).ConfigureAwait(false)
                   // Run through the list of Weapon Mods.
                   + await WeaponAccessories.SumAsync(x => objExcludeAccessory != x && x.Equipped && !x.IncludedInWeapon, x => x.GetTotalCostAsync(token).AsTask(), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Weapon Weight to use when working with Total Weight price modifiers for Weapon Mods.
        /// </summary>
        public decimal MultipliableWeight(WeaponAccessory objExcludeAccessory, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return OwnWeight
                   // Run through the list of Weapon Mods.
                   + WeaponAccessories.Sum(x => objExcludeAccessory != x && x.Equipped, x => x.TotalWeight, token);
        }

        public string AccessoryMounts
        {
            get
            {
                if (string.IsNullOrEmpty(ModificationSlots))
                    return string.Empty;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdMounts))
                {
                    foreach (string strMount in ModificationSlots.SplitNoAlloc(
                                 '/', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (WeaponAccessories.All(objAccessory =>
                                                      !objAccessory.Equipped || objAccessory.Mount != strMount
                                                      && objAccessory.ExtraMount != strMount)
                            && UnderbarrelWeapons.All(weapon => !weapon.Equipped
                                                                || weapon.Mount != strMount
                                                                && weapon.ExtraMount != strMount))
                        {
                            sbdMounts.Append(strMount).Append('/');
                        }
                    }

                    string strReturn = sbdMounts.ToString();
                    return strReturn.Contains("Internal/") ? strReturn + "None" : strReturn + "Internal/None";
                }
            }
        }

        /// <summary>
        /// The Weapon's total cost including Accessories and Modifications.
        /// </summary>
        public decimal TotalCost => OwnCost + WeaponAccessories.Sum(x => x.TotalCost) + Children.Sum(x => x.TotalCost);

        /// <summary>
        /// The Weapon's total cost including Accessories and Modifications.
        /// </summary>
        public async ValueTask<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await GetOwnCostAsync(token).ConfigureAwait(false)
                   // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
                   + await WeaponAccessories
                           .SumAsync(objAccessory => objAccessory.GetTotalCostAsync(token).AsTask(), token)
                           .ConfigureAwait(false)
                   // Include the cost of any Underbarrel Weapon.
                   + await Children
                           .SumAsync(objUnderbarrel => objUnderbarrel.GetTotalCostAsync(token).AsTask(),
                                     token).ConfigureAwait(false);
        }

        /// <summary>
        /// The cost of just the Weapon itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                // If this is a Cyberware or Gear Weapon, remove the Weapon Cost from this since it has already been paid for through the parent item (but is needed to calculate Mod price).
                if (Cyberware || Category == "Gear")
                    return 0;
                decimal decReturn = 0;
                string strCostExpression = Cost;

                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCostExpression.TrimStart('+'));

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.GetAllAttributes())
                    {
                        sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev,
                                             () => objLoopAttribute.TotalValue.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                        sbdCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                             () => objLoopAttribute.TotalBase.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                    }

                    sbdCost.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));

                    // Replace the division sign with "div" since we're using XPath.
                    sbdCost.Replace("/", " div ");
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                if (DiscountCost)
                    decReturn *= 0.9m;

                if (!string.IsNullOrEmpty(Parent?.DoubledCostModificationSlots))
                {
                    string[] astrParentDoubledCostModificationSlots = Parent.DoubledCostModificationSlots.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (astrParentDoubledCostModificationSlots.Contains(Mount) || astrParentDoubledCostModificationSlots.Contains(ExtraMount))
                    {
                        decReturn *= 2;
                    }
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Weapon itself.
        /// </summary>
        public async ValueTask<decimal> GetOwnCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // If this is a Cyberware or Gear Weapon, remove the Weapon Cost from this since it has already been paid for through the parent item (but is needed to calculate Mod price).
            if (Cyberware || Category == "Gear")
                return 0;
            decimal decReturn = 0;
            string strCostExpression = Cost;

            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
            {
                sbdCost.Append(strCostExpression.TrimStart('+'));

                AttributeSection objAttributeSection = await _objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false);
                await (await objAttributeSection.GetAttributeListAsync(token).ConfigureAwait(false)).ForEachAsync(async objLoopAttribute =>
                {
                    await sbdCost.CheapReplaceAsync(strCostExpression, objLoopAttribute.Abbrev,
                                                    async () => (await objLoopAttribute.GetTotalValueAsync(token)
                                                        .ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo), token: token)
                                 .ConfigureAwait(false);
                    await sbdCost.CheapReplaceAsync(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                                    async () => (await objLoopAttribute.GetTotalBaseAsync(token)
                                                        .ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo), token: token)
                                 .ConfigureAwait(false);
                }, token).ConfigureAwait(false);
                await (await objAttributeSection.GetSpecialAttributeListAsync(token).ConfigureAwait(false)).ForEachAsync(async objLoopAttribute =>
                {
                    await sbdCost.CheapReplaceAsync(strCostExpression, objLoopAttribute.Abbrev,
                                                    async () => (await objLoopAttribute.GetTotalValueAsync(token)
                                                        .ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo), token: token)
                                 .ConfigureAwait(false);
                    await sbdCost.CheapReplaceAsync(strCostExpression, objLoopAttribute.Abbrev + "Base",
                                                    async () => (await objLoopAttribute.GetTotalBaseAsync(token)
                                                        .ConfigureAwait(false)).ToString(
                                                        GlobalSettings.InvariantCultureInfo), token: token)
                                 .ConfigureAwait(false);
                }, token).ConfigureAwait(false);

                await sbdCost.CheapReplaceAsync("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

                // Replace the division sign with "div" since we're using XPath.
                sbdCost.Replace("/", " div ");
                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(sbdCost.ToString(), token).ConfigureAwait(false);
                if (blnIsSuccess)
                    decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
            }

            if (DiscountCost)
                decReturn *= 0.9m;

            if (!string.IsNullOrEmpty(Parent?.DoubledCostModificationSlots))
            {
                string[] astrParentDoubledCostModificationSlots
                    = Parent.DoubledCostModificationSlots.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (astrParentDoubledCostModificationSlots.Contains(Mount)
                    || astrParentDoubledCostModificationSlots.Contains(ExtraMount))
                {
                    decReturn *= 2;
                }
            }

            return decReturn;
        }

        /// <summary>
        /// The Weapon's total weight including Accessories and Modifications.
        /// </summary>
        public decimal TotalWeight => OwnWeight + WeaponAccessories.Sum(x => x.Equipped, x => x.TotalWeight)
                                                + Children.Sum(x => x.Equipped, x => x.TotalWeight);

        /// <summary>
        /// The weight of just the Weapon itself.
        /// </summary>
        public decimal OwnWeight
        {
            get
            {
                // If this is a Cyberware or Gear Weapon, remove the Weapon Weight from this since it has already been paid for through the parent item (but is needed to calculate Mod weight).
                if (Cyberware || Category == "Gear" || IncludedInWeapon)
                    return 0;
                string strWeightExpression = Weight;
                if (string.IsNullOrEmpty(strWeightExpression))
                    return 0;

                decimal decReturn = 0;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdWeight))
                {
                    sbdWeight.Append(strWeightExpression.TrimStart('+'));

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.GetAllAttributes())
                    {
                        sbdWeight.CheapReplace(strWeightExpression, objLoopAttribute.Abbrev,
                                               () => objLoopAttribute.TotalValue.ToString(
                                                   GlobalSettings.InvariantCultureInfo));
                        sbdWeight.CheapReplace(strWeightExpression, objLoopAttribute.Abbrev + "Base",
                                               () => objLoopAttribute.TotalBase.ToString(
                                                   GlobalSettings.InvariantCultureInfo));
                    }

                    sbdWeight.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));

                    // Replace the division sign with "div" since we're using XPath.
                    sbdWeight.Replace("/", " div ");
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(sbdWeight.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The Weapon's total AP including Ammunition in the program's current language.
        /// </summary>
        public string DisplayTotalAP => TotalAP(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// The Weapon's total AP including Ammunition.
        /// </summary>
        public string TotalAP(CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true)
        {
            return Utils.SafelyRunSynchronously(() => TotalAPCoreAsync(true, objCulture, strLanguage, blnIncludeAmmo));
        }

        /// <summary>
        /// The Weapon's total AP including Ammunition.
        /// </summary>
        public Task<string> TotalAPAsync(CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            return TotalAPCoreAsync(false, objCulture, strLanguage, blnIncludeAmmo, token);
        }

        /// <summary>
        /// The Weapon's total AP including Ammunition.
        /// </summary>
        private async Task<string> TotalAPCoreAsync(bool blnSync, CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            string strAP = AP;

            int intImprove = 0;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdBonusAP))
            {
                if (blnIncludeAmmo)
                {
                    // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
                    // Look for Ammo on the character.
                    Gear objGear = AmmoLoaded;
                    if (objGear != null)
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAPReplace = objGear.FlechetteWeaponBonus["apreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPReplace))
                                strAP = strAPReplace;
                            // Adjust the Weapon's Damage.
                            string strAPAdd = objGear.FlechetteWeaponBonus["ap"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPAdd))
                                sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAPReplace = objGear.WeaponBonus["apreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPReplace))
                                strAP = strAPReplace;
                            // Adjust the Weapon's Damage.
                            string strAPAdd = objGear.WeaponBonus["ap"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPAdd))
                                sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in blnSync
                                     ? objGear.Children.DeepWhere(x => x.Children, x => x.Equipped, token)
                                     : await objGear.Children.DeepWhereAsync(x => x.Children, x => x.Equipped, token)
                                                    .ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAPReplace = objChild.FlechetteWeaponBonus["apreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPReplace))
                                    strAP = strAPReplace;
                                // Adjust the Weapon's Damage.
                                string strAPAdd = objChild.FlechetteWeaponBonus["ap"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPAdd))
                                    sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAPReplace = objChild.WeaponBonus["apreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPReplace))
                                    strAP = strAPReplace;
                                // Adjust the Weapon's Damage.
                                string strAPAdd = objChild.WeaponBonus["ap"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPAdd))
                                    sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                            }
                        }
                    }
                }

                if (_objCharacter != null && (Name == "Unarmed Attack" || Skill?.DictionaryKey == "Unarmed Combat" &&
                        _objCharacter.Settings.UnarmedImprovementsApplyToWeapons))
                {
                    // Add any UnarmedAP bonus for the Unarmed Attack item.
                    intImprove += (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedAP, token: token)
                            : await ImprovementManager.ValueOfAsync(
                                _objCharacter, Improvement.ImprovementType.UnarmedAP, token: token).ConfigureAwait(false))
                        .StandardRound();
                }

                foreach (WeaponAccessory objAccessory in WeaponAccessories.Where(objAccessory => objAccessory.Equipped))
                {
                    // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                    if (!(objAccessory.DamageType.Contains("(f)") && Damage.Contains("(f)")))
                    {
                        // Change the Weapon's AP value.
                        if (!string.IsNullOrEmpty(objAccessory.APReplacement))
                            strAP = objAccessory.APReplacement;
                        // Adjust the Weapon's AP value.
                        if (!string.IsNullOrEmpty(objAccessory.AP))
                            sbdBonusAP.Append(" + ").Append(objAccessory.AP.TrimStartOnce('+'));
                    }
                }

                if (strAP == "-")
                    strAP = "0";
                if (sbdBonusAP.Length > 0)
                    strAP += sbdBonusAP.ToString();
            }

            if (strAP.Contains("//"))
            {
                if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    return strAP.Replace("//", "/");
                return blnSync
                    // ReSharper disable once MethodHasAsyncOverload
                    ? ReplaceStrings(strAP.Replace("//", "/")
                           // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                           .CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage, token: token)), strLanguage, token)
                    : await ReplaceStringsAsync(await strAP.Replace("//", "/")
                                                           .CheapReplaceAsync(
                                                               "-half",
                                                               () => LanguageManager.GetStringAsync("String_APHalf", strLanguage, token: token),
                                                               token: token).ConfigureAwait(false), strLanguage, token).ConfigureAwait(false);
            }

            int intAP;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAP))
            {
                sbdAP.Append(strAP);
                if (blnSync)
                {
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    sbdAP.CheapReplace("{Rating}", strAP, () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    ProcessAttributesInXPath(sbdAP, strAP);
                }
                else
                {
                    await sbdAP.CheapReplaceAsync(strAP, "{Rating}",
                                                  () => Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await ProcessAttributesInXPathAsync(sbdAP, strAP, token: token).ConfigureAwait(false);
                }

                try
                {
                    // Replace the division sign with "div" since we're using XPath.
                    sbdAP.Replace("/", " div ");
                    (bool blnIsSuccess, object objProcess) = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? CommonFunctions.EvaluateInvariantXPath(sbdAP.ToString(), token)
                        : await CommonFunctions.EvaluateInvariantXPathAsync(sbdAP.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAP = ((double)objProcess).StandardRound();
                    else if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                        return strAP;
                    else
                        return blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? ReplaceStrings(
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                strAP.CheapReplace(
                                    "-half", () => LanguageManager.GetString("String_APHalf", strLanguage, token: token)),
                                strLanguage, token)
                            : await ReplaceStringsAsync(await strAP.CheapReplaceAsync(
                                                            "-half",
                                                            () => LanguageManager.GetStringAsync(
                                                                "String_APHalf", strLanguage, token: token),
                                                            token: token).ConfigureAwait(false), strLanguage, token).ConfigureAwait(false);
                }
                catch (FormatException)
                {
                    // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                    if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                        return strAP;
                    return blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? ReplaceStrings(
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            strAP.CheapReplace(
                                "-half", () => LanguageManager.GetString("String_APHalf", strLanguage, token: token)),
                            strLanguage, token)
                        : await ReplaceStringsAsync(await strAP.CheapReplaceAsync(
                                                        "-half",
                                                        () => LanguageManager.GetStringAsync(
                                                            "String_APHalf", strLanguage, token: token),
                                                        token: token).ConfigureAwait(false), strLanguage, token).ConfigureAwait(false);
                }
                catch (OverflowException)
                {
                    // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                    if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                        return strAP;
                    return blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? ReplaceStrings(
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            strAP.CheapReplace(
                                "-half", () => LanguageManager.GetString("String_APHalf", strLanguage, token: token)),
                            strLanguage, token)
                        : await ReplaceStringsAsync(await strAP.CheapReplaceAsync(
                                                        "-half",
                                                        () => LanguageManager.GetStringAsync(
                                                            "String_APHalf", strLanguage, token: token),
                                                        token: token).ConfigureAwait(false), strLanguage, token).ConfigureAwait(false);
                }
                catch (InvalidCastException)
                {
                    // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                    if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                        return strAP;
                    return blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? ReplaceStrings(
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            strAP.CheapReplace(
                                "-half", () => LanguageManager.GetString("String_APHalf", strLanguage, token: token)),
                            strLanguage, token)
                        : await ReplaceStringsAsync(await strAP.CheapReplaceAsync(
                                                        "-half",
                                                        () => LanguageManager.GetStringAsync(
                                                            "String_APHalf", strLanguage, token: token),
                                                        token: token).ConfigureAwait(false), strLanguage, token).ConfigureAwait(false);
                }
            }

            intAP += intImprove;
            if (intAP == 0)
                return "-";
            if (intAP > 0)
                return '+' + intAP.ToString(objCulture);
            return intAP.ToString(objCulture);
        }

        public string DisplayTotalRC => TotalRC(GlobalSettings.CultureInfo, GlobalSettings.Language, true);

        /// <summary>
        /// The Weapon's total RC including Accessories and Modifications.
        /// </summary>
        public string TotalRC(CultureInfo objCulture, string strLanguage, bool blnRefreshRCToolTip = false, bool blnIncludeAmmo = true)
        {
            return Utils.SafelyRunSynchronously(() => TotalRCCoreAsync(true, objCulture, strLanguage, blnRefreshRCToolTip, blnIncludeAmmo));
        }

        /// <summary>
        /// The Weapon's total RC including Accessories and Modifications.
        /// </summary>
        public Task<string> TotalRCAsync(CultureInfo objCulture, string strLanguage, bool blnRefreshRCToolTip = false, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            return TotalRCCoreAsync(false, objCulture, strLanguage, blnRefreshRCToolTip, blnIncludeAmmo, token);
        }

        /// <summary>
        /// The Weapon's total RC including Accessories and Modifications.
        /// </summary>
        private async Task<string> TotalRCCoreAsync(bool blnSync, CultureInfo objCulture, string strLanguage, bool blnRefreshRCToolTip, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            string strSpace = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LanguageManager.GetString("String_Space", strLanguage, token: token)
                : await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            string strRCBase = "0";
            string strRCFull;
            string strRC = RC;

            List<Tuple<string, int>> lstRCGroups = new List<Tuple<string, int>>(5);
            List<Tuple<string, int>> lstRCDeployGroups = new List<Tuple<string, int>>(5);
            strRC = blnSync
                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                ? strRC.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo))
                : await strRC.CheapReplaceAsync("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
            int intPos = strRC.IndexOf('(');
            if (intPos != -1)
            {
                if (intPos == 0)
                {
                    // The string contains only RC from pieces that can be removed - "(x)" only.
                    strRCFull = strRC;
                }
                // The string contains a mix of both fixed and removable RC. "x(y)".
                else
                {
                    strRCBase = strRC.Substring(0, intPos);
                    strRCFull = strRC.Substring(intPos, strRC.Length - intPos);
                }
            }
            else
            {
                // The string contains only RC from fixed pieces - "x" only.
                strRCBase = strRC;
                strRCFull = strRC;
            }

            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdRCTip))
            {
                sbdRCTip.Append(1.ToString(GlobalSettings.CultureInfo)).Append(strSpace);
                if (blnRefreshRCToolTip && strRCBase != "0")
                {
                    sbdRCTip.Append('+').Append(strSpace)
                            .Append(blnSync
                                        // ReSharper disable once MethodHasAsyncOverload
                                        ? LanguageManager.GetString("Label_Base", strLanguage, token: token)
                                        : await LanguageManager.GetStringAsync("Label_Base", strLanguage, token: token).ConfigureAwait(false))
                            .Append('(').Append(strRCBase).Append(')');
                }

                int.TryParse(strRCBase, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intRCBase);
                int.TryParse(strRCFull.Trim('(', ')'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                             out int intRCFull);

                if (blnIncludeAmmo)
                {
                    // Check if the Weapon has Ammunition loaded and look for any Recoil bonus.
                    Gear objGear = AmmoLoaded;
                    if (objGear != null)
                    {
                        // Change the Weapon's Damage Type.
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        {
                            string strRCBonus = objGear.FlechetteWeaponBonus["rc"]?.InnerText;
                            if (!string.IsNullOrEmpty(strRCBonus) && int.TryParse(strRCBonus, out int intLoopRCBonus))
                            {
                                intRCBase += intLoopRCBonus;
                                intRCFull += intLoopRCBonus;

                                if (blnRefreshRCToolTip)
                                    sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(blnSync
                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                        ? objGear.DisplayName(objCulture, strLanguage)
                                                        : await objGear
                                                                .DisplayNameAsync(objCulture, strLanguage, token: token)
                                                                .ConfigureAwait(false))
                                            .Append(strSpace)
                                            .Append('(').Append(strRCBonus).Append(')');
                            }
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            string strRCBonus = objGear.WeaponBonus["rc"]?.InnerText;
                            if (!string.IsNullOrEmpty(strRCBonus) && int.TryParse(strRCBonus, out int intLoopRCBonus))
                            {
                                intRCBase += intLoopRCBonus;
                                intRCFull += intLoopRCBonus;

                                if (blnRefreshRCToolTip)
                                    sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(blnSync
                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                        ? objGear.DisplayName(objCulture, strLanguage)
                                                        : await objGear
                                                                .DisplayNameAsync(objCulture, strLanguage, token: token)
                                                                .ConfigureAwait(false))
                                            .Append(strSpace)
                                            .Append('(').Append(strRCBonus).Append(')');
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in blnSync
                                     ? objGear.Children.DeepWhere(x => x.Children, x => x.Equipped, token)
                                     : await objGear.Children.DeepWhereAsync(x => x.Children, x => x.Equipped, token)
                                                    .ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                            {
                                string strRCBonus = objChild.FlechetteWeaponBonus["rc"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRCBonus) && int.TryParse(strRCBonus, out int intLoopRCBonus))
                                {
                                    intRCBase += intLoopRCBonus;
                                    intRCFull += intLoopRCBonus;

                                    if (blnRefreshRCToolTip)
                                        sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(blnSync
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            ? objChild.DisplayName(objCulture, strLanguage)
                                                            : await objChild
                                                                    .DisplayNameAsync(objCulture, strLanguage, token: token)
                                                                    .ConfigureAwait(false))
                                                .Append(strSpace)
                                                .Append('(').Append(strRCBonus).Append(')');
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                string strRCBonus = objChild.WeaponBonus["rc"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRCBonus) && int.TryParse(strRCBonus, out int intLoopRCBonus))
                                {
                                    intRCBase += intLoopRCBonus;
                                    intRCFull += intLoopRCBonus;

                                    if (blnRefreshRCToolTip)
                                        sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(blnSync
                                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                            ? objChild.DisplayName(objCulture, strLanguage)
                                                            : await objChild
                                                                    .DisplayNameAsync(objCulture, strLanguage, token: token)
                                                                    .ConfigureAwait(false))
                                                .Append(strSpace)
                                                .Append('(').Append(strRCBonus).Append(')');
                                }
                            }
                        }
                    }
                }

                // Now that we know the Weapon's RC values, run through all of the Accessories and add theirs to the mix.
                // Only add in the values for items that do not come with the weapon.
                foreach (WeaponAccessory objAccessory in WeaponAccessories.Where(
                             objAccessory => !string.IsNullOrEmpty(objAccessory.RC) && objAccessory.Equipped))
                {
                    if (_objCharacter.Settings.RestrictRecoil && objAccessory.RCGroup != 0)
                    {
                        int intItemRC = Convert.ToInt32(objAccessory.RC, GlobalSettings.InvariantCultureInfo);
                        List<Tuple<string, int>> lstLoopRCGroup = lstRCGroups;
                        if (objAccessory.RCDeployable)
                        {
                            lstLoopRCGroup = lstRCDeployGroups;
                        }

                        while (lstLoopRCGroup.Count < objAccessory.RCGroup)
                        {
                            lstLoopRCGroup.Add(new Tuple<string, int>(string.Empty, 0));
                        }

                        if (lstLoopRCGroup[objAccessory.RCGroup - 1].Item2 < intItemRC)
                        {
                            lstLoopRCGroup[objAccessory.RCGroup - 1]
                                = new Tuple<string, int>(
                                    blnSync
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        ? objAccessory.DisplayName(strLanguage)
                                        : await objAccessory.DisplayNameAsync(strLanguage, token).ConfigureAwait(false), intItemRC);
                        }

                        if (objAccessory.RCDeployable)
                        {
                            lstRCDeployGroups = lstLoopRCGroup;
                        }
                        else
                        {
                            lstRCGroups = lstLoopRCGroup;
                        }
                    }
                    else if (!string.IsNullOrEmpty(objAccessory.RC)
                             && int.TryParse(objAccessory.RC, out int intLoopRCBonus))
                    {
                        intRCFull += intLoopRCBonus;
                        if (!objAccessory.RCDeployable)
                        {
                            intRCBase += intLoopRCBonus;
                        }

                        if (blnRefreshRCToolTip)
                            sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                .Append(blnSync
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    ? objAccessory.DisplayName(strLanguage)
                                    : await objAccessory.DisplayNameAsync(strLanguage, token).ConfigureAwait(false)).Append(strSpace).Append('(')
                                .Append(objAccessory.RC).Append(')');
                    }
                }

                foreach ((string strGroup, int intRecoil) in lstRCGroups)
                {
                    if (!string.IsNullOrEmpty(strGroup))
                    {
                        // Add in the Recoil Group bonuses.
                        intRCBase += intRecoil;
                        intRCFull += intRecoil;
                        if (blnRefreshRCToolTip)
                            sbdRCTip.Append(strSpace).Append('+').Append(strSpace).Append(strGroup).Append(strSpace)
                                    .Append('(').Append(intRecoil.ToString(objCulture)).Append(')');
                    }
                }

                foreach ((string strGroup, int intRecoil) in lstRCDeployGroups)
                {
                    if (!string.IsNullOrEmpty(strGroup))
                    {
                        // Add in the Recoil Group bonuses.
                        intRCFull += intRecoil;
                        if (blnRefreshRCToolTip)
                            sbdRCTip.Append(strSpace).Append('+').Append(strSpace).AppendFormat(
                                objCulture,
                                blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? LanguageManager.GetString("Tip_RecoilAccessories", strLanguage, token: token)
                                    : await LanguageManager.GetStringAsync("Tip_RecoilAccessories", strLanguage, token: token).ConfigureAwait(false),
                                strGroup,
                                intRecoil);
                    }
                }

                int intUseSTR = 0;
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = blnSync ? ParentVehicle.TotalBody : await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(ParentID))
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent
                                = _objCharacter.Vehicles.FindVehicleCyberware(
                                    x => x.InternalId == ParentID, out VehicleMod objVehicleMod);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                int intSTR = blnSync ? objAttributeSource.GetAttributeTotalValue("STR") : await objAttributeSource.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                int intAGI = blnSync ? objAttributeSource.GetAttributeTotalValue("AGI") : await objAttributeSource.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                                while (objAttributeSource != null)
                                {
                                    if (intSTR != 0 || intAGI != 0)
                                        break;
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null) continue;
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    intSTR = blnSync ? objAttributeSource.GetAttributeTotalValue("STR") : await objAttributeSource.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    intAGI = blnSync ? objAttributeSource.GetAttributeTotalValue("AGI") : await objAttributeSource.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                                }

                                intUseSTR = intSTR;

                                if (intUseSTR == 0)
                                    intUseSTR = blnSync ? objVehicleMod.TotalStrength : await objVehicleMod.GetTotalStrengthAsync(token).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent
                            = _objCharacter.Cyberware.DeepFirstOrDefault(
                                x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            int intSTR = blnSync ? objAttributeSource.GetAttributeTotalValue("STR") : await objAttributeSource.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            int intAGI = blnSync ? objAttributeSource.GetAttributeTotalValue("AGI") : await objAttributeSource.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null)
                                    continue;
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                intSTR = blnSync ? objAttributeSource.GetAttributeTotalValue("STR") : await objAttributeSource.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                            }

                            intUseSTR = intSTR;
                        }

                        if (intUseSTR == 0)
                            intUseSTR = blnSync
                                ? _objCharacter.STR.TotalValue
                                : await (await _objCharacter.GetAttributeAsync("STR", token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                    }
                }
                else if (ParentVehicle == null)
                {
                    intUseSTR = blnSync
                        ? _objCharacter.STR.TotalValue
                        : await (await _objCharacter.GetAttributeAsync("STR", token: token).ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                }

                if (Category == "Throwing Weapons" || Skill?.DictionaryKey == "Throwing Weapons")
                    intUseSTR += (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR, token: token)
                            : await ImprovementManager.ValueOfAsync(_objCharacter,
                                                                    Improvement.ImprovementType.ThrowSTR, token: token).ConfigureAwait(false))
                        .StandardRound();

                int intStrRC = (intUseSTR + 2) / 3;

                intRCBase += intStrRC + 1;
                intRCFull += intStrRC + 1;
                if (blnRefreshRCToolTip)
                    sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                            .Append(blnSync
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        ? _objCharacter.STR.GetDisplayAbbrev(strLanguage)
                                        : await _objCharacter.STR.GetDisplayAbbrevAsync(strLanguage, token).ConfigureAwait(false)).Append(strSpace)
                            .Append('[')
                            .Append(intUseSTR.ToString(objCulture)).Append(strSpace).Append('/').Append(strSpace)
                            .Append(3.ToString(objCulture)).Append(strSpace).Append('=').Append(strSpace)
                            .Append(intStrRC.ToString(objCulture)).Append(']');
                // If the full RC is not higher than the base, only the base value is shown.
                strRC = intRCBase.ToString(objCulture);
                if (intRCFull > intRCBase)
                    strRC += strSpace + '(' + intRCFull.ToString(objCulture) + ')';

                if (blnRefreshRCToolTip)
                    _strRCTip = sbdRCTip.ToString();
            }

            return strRC;
        }

        /// <summary>
        /// The tooltip showing the sources of RC bonuses
        /// </summary>
        public string RCToolTip => _strRCTip;

        /// <summary>
        /// The full Reach of the Weapons including the Character's Reach.
        /// </summary>
        public int TotalReach
        {
            get
            {
                decimal decReach = Reach + WeaponAccessories.Sum(x => x.Equipped, i => i.Reach);
                if (RangeType == "Melee")
                {
                    // Run through the Character's Improvements and add any Reach Improvements.
                    decReach += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Reach, strImprovedName: Name, blnIncludeNonImproved: true);
                }

                if (Name == "Unarmed Attack" || Skill?.DictionaryKey == "Unarmed Combat" &&
                    _objCharacter.Settings.UnarmedImprovementsApplyToWeapons)
                {
                    decReach += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedReach);
                }

                return decReach.StandardRound();
            }
        }

        /// <summary>
        /// The full Reach of the Weapons including the Character's Reach.
        /// </summary>
        public async ValueTask<int> GetTotalReachAsync(CancellationToken token = default)
        {
            decimal decReach = Reach + await WeaponAccessories.SumAsync(x => x.Equipped, i => i.Reach, token: token).ConfigureAwait(false);
            if (RangeType == "Melee")
            {
                // Run through the Character's Improvements and add any Reach Improvements.
                decReach += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Reach,
                                                                  strImprovedName: Name, blnIncludeNonImproved: true, token: token).ConfigureAwait(false);
            }

            if (_objCharacter.Settings.UnarmedImprovementsApplyToWeapons && (Name == "Unarmed Attack" || (Skill != null && await Skill.GetDictionaryKeyAsync(token).ConfigureAwait(false) == "Unarmed Combat")))
            {
                decReach += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.UnarmedReach, token: token).ConfigureAwait(false);
            }

            return decReach.StandardRound();
        }

        /// <summary>
        /// The full Accuracy of the Weapon including modifiers from accessories.
        /// </summary>
        public int GetTotalAccuracy(bool blnIncludeAmmo = true)
        {
            int intAccuracy = 0;
            string strAccuracy = Accuracy;
            if (blnIncludeAmmo)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdBonusAccuracy))
                {
                    // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
                    // Look for Ammo on the character.
                    Gear objGear = AmmoLoaded;
                    if (objGear != null)
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objGear.FlechetteWeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                                strAccuracy = strAccuracyReplace;
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objGear.FlechetteWeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd))
                                sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objGear.WeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                                strAccuracy = strAccuracyReplace;
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objGear.WeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd))
                                sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in objGear.Children.DeepWhere(x => x.Children, x => x.Equipped))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAccuracyReplace = objChild.FlechetteWeaponBonus["accuracyreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyReplace))
                                    strAccuracy = strAccuracyReplace;
                                // Adjust the Weapon's Damage.
                                string strAccuracyAdd = objChild.FlechetteWeaponBonus["accuracy"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyAdd))
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAccuracyReplace = objChild.WeaponBonus["accuracyreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyReplace))
                                    strAccuracy = strAccuracyReplace;
                                // Adjust the Weapon's Damage.
                                string strAccuracyAdd = objChild.WeaponBonus["accuracy"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyAdd))
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                            }
                        }
                    }

                    if (sbdBonusAccuracy.Length != 0)
                        strAccuracy = '(' + strAccuracy + ')' + sbdBonusAccuracy;
                }
            }

            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAccuracy))
            {
                sbdAccuracy.Append(strAccuracy);
                sbdAccuracy.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                ProcessAttributesInXPath(sbdAccuracy, strAccuracy);
                Func<string> funcPhysicalLimitString = () =>
                    _objCharacter.LimitPhysical.ToString(GlobalSettings.InvariantCultureInfo);
                if (ParentVehicle != null)
                {
                    funcPhysicalLimitString = () =>
                    {
                        string strHandling = ParentVehicle.TotalHandling;
                        int intSlashIndex = strHandling.IndexOf('/');
                        if (intSlashIndex != -1)
                            strHandling = strHandling.Substring(0, intSlashIndex);
                        return strHandling;
                    };
                }

                sbdAccuracy.CheapReplace(strAccuracy, "Physical", funcPhysicalLimitString)
                           .CheapReplace(strAccuracy, "Missile", funcPhysicalLimitString);

                // Replace the division sign with "div" since we're using XPath.
                sbdAccuracy.Replace("/", " div ");
                (bool blnIsSuccess, object objProcess)
                    = CommonFunctions.EvaluateInvariantXPath(sbdAccuracy.ToString());
                if (blnIsSuccess)
                    intAccuracy = ((double) objProcess).StandardRound();
            }

            int intBonusAccuracyFromAccessories = 0;
            int intBonusAccuracyFromNonStackingAccessories = 0;
            foreach (WeaponAccessory objWeaponAccessory in WeaponAccessories)
            {
                if (objWeaponAccessory.Equipped)
                {
                    int intLoopAccuracy = objWeaponAccessory.Accuracy;
                    if (intLoopAccuracy != 0)
                    {
                        if (!objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal)
                            && !objWeaponAccessory.Name.Contains("Sight"))
                            intBonusAccuracyFromAccessories += intLoopAccuracy;
                        else if (intLoopAccuracy > intBonusAccuracyFromNonStackingAccessories)
                            intBonusAccuracyFromNonStackingAccessories = intLoopAccuracy;
                    }
                }
            }

            // Underbarrel weapons that come with their parent weapon (and are of the same type) should inherit the parent weapon's built-in smartgun features
            if (IncludedInWeapon && Parent != null && RangeType == Parent.RangeType)
            {
                foreach (WeaponAccessory objWeaponAccessory in Parent.WeaponAccessories)
                {
                    if (objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal)
                        && objWeaponAccessory.IncludedInWeapon && objWeaponAccessory.Equipped)
                    {
                        int intLoopAccuracy = objWeaponAccessory.Accuracy;
                        if (intLoopAccuracy > intBonusAccuracyFromNonStackingAccessories)
                            intBonusAccuracyFromNonStackingAccessories = intLoopAccuracy;
                    }
                }
            }

            intAccuracy += intBonusAccuracyFromAccessories + intBonusAccuracyFromNonStackingAccessories;

            string strNameUpper = Name.ToUpperInvariant();

            decimal decImproveAccuracy = ImprovementManager.ValueOf(
                _objCharacter, Improvement.ImprovementType.WeaponSkillAccuracy, strImprovedName: Name,
                blnIncludeNonImproved: true);
            string strSkill = Skill?.DictionaryKey ?? string.Empty;
            if (!string.IsNullOrEmpty(strSkill))
                decImproveAccuracy += ImprovementManager.ValueOf(_objCharacter,
                                                                 Improvement.ImprovementType.WeaponSkillAccuracy,
                                                                 strImprovedName: strSkill);
            foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(
                         _objCharacter, Improvement.ImprovementType.WeaponAccuracy))
            {
                string strImprovedName = objImprovement.ImprovedName;
                if (strImprovedName.StartsWith("[contains]", StringComparison.Ordinal)
                    && strNameUpper.Contains(strImprovedName.TrimStartOnce("[contains]", true),
                                             StringComparison.OrdinalIgnoreCase))
                {
                    decImproveAccuracy += objImprovement.Value;
                }
            }

            intAccuracy += decImproveAccuracy.StandardRound();

            return intAccuracy;
        }

        /// <summary>
        /// The full Accuracy of the Weapon including modifiers from accessories.
        /// </summary>
        public async ValueTask<int> GetTotalAccuracyAsync(bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            int intAccuracy = 0;
            string strAccuracy = Accuracy;
            if (blnIncludeAmmo)
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool,
                                                              out StringBuilder sbdBonusAccuracy))
                {
                    // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
                    // Look for Ammo on the character.
                    Gear objGear = AmmoLoaded;
                    if (objGear != null)
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objGear.FlechetteWeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                                strAccuracy = strAccuracyReplace;
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objGear.FlechetteWeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd))
                                sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objGear.WeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                                strAccuracy = strAccuracyReplace;
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objGear.WeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd))
                                sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in await objGear.Children.DeepWhereAsync(x => x.Children, x => x.Equipped, token)
                                                    .ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAccuracyReplace = objChild.FlechetteWeaponBonus["accuracyreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyReplace))
                                    strAccuracy = strAccuracyReplace;
                                // Adjust the Weapon's Damage.
                                string strAccuracyAdd = objChild.FlechetteWeaponBonus["accuracy"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyAdd))
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAccuracyReplace = objChild.WeaponBonus["accuracyreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyReplace))
                                    strAccuracy = strAccuracyReplace;
                                // Adjust the Weapon's Damage.
                                string strAccuracyAdd = objChild.WeaponBonus["accuracy"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyAdd))
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                            }
                        }
                    }

                    if (sbdBonusAccuracy.Length != 0)
                        strAccuracy = '(' + strAccuracy + ')' + sbdBonusAccuracy;
                }
            }

            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAccuracy))
            {
                sbdAccuracy.Append(strAccuracy);
                await sbdAccuracy.CheapReplaceAsync(strAccuracy, "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                await ProcessAttributesInXPathAsync(sbdAccuracy, strAccuracy, token: token).ConfigureAwait(false);
                Func<string> funcPhysicalLimitString = () =>
                    _objCharacter.LimitPhysical.ToString(GlobalSettings.InvariantCultureInfo);
                if (ParentVehicle != null)
                {
                    funcPhysicalLimitString = () =>
                    {
                        string strHandling = ParentVehicle.TotalHandling;
                        int intSlashIndex = strHandling.IndexOf('/');
                        if (intSlashIndex != -1)
                            strHandling = strHandling.Substring(0, intSlashIndex);
                        return strHandling;
                    };
                }

                await (await sbdAccuracy.CheapReplaceAsync(strAccuracy, "Physical", funcPhysicalLimitString,
                                                           token: token).ConfigureAwait(false))
                      .CheapReplaceAsync(strAccuracy, "Missile", funcPhysicalLimitString, token: token).ConfigureAwait(false);

                // Replace the division sign with "div" since we're using XPath.
                sbdAccuracy.Replace("/", " div ");
                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAccuracy.ToString(), token).ConfigureAwait(false);
                if (blnIsSuccess)
                    intAccuracy = ((double)objProcess).StandardRound();
            }

            int intBonusAccuracyFromAccessories = 0;
            int intBonusAccuracyFromNonStackingAccessories = 0;
            foreach (WeaponAccessory objWeaponAccessory in WeaponAccessories)
            {
                if (objWeaponAccessory.Equipped)
                {
                    int intLoopAccuracy = objWeaponAccessory.Accuracy;
                    if (intLoopAccuracy != 0)
                    {
                        if (!objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal)
                            && !objWeaponAccessory.Name.Contains("Sight"))
                            intBonusAccuracyFromAccessories += intLoopAccuracy;
                        else if (intLoopAccuracy > intBonusAccuracyFromNonStackingAccessories)
                            intBonusAccuracyFromNonStackingAccessories = intLoopAccuracy;
                    }
                }
            }

            // Underbarrel weapons that come with their parent weapon (and are of the same type) should inherit the parent weapon's built-in smartgun features
            if (IncludedInWeapon && Parent != null && RangeType == Parent.RangeType)
            {
                foreach (WeaponAccessory objWeaponAccessory in Parent.WeaponAccessories)
                {
                    if (objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal)
                        && objWeaponAccessory.IncludedInWeapon && objWeaponAccessory.Equipped)
                    {
                        int intLoopAccuracy = objWeaponAccessory.Accuracy;
                        if (intLoopAccuracy > intBonusAccuracyFromNonStackingAccessories)
                            intBonusAccuracyFromNonStackingAccessories = intLoopAccuracy;
                    }
                }
            }

            intAccuracy += intBonusAccuracyFromAccessories + intBonusAccuracyFromNonStackingAccessories;

            string strNameUpper = Name.ToUpperInvariant();

            decimal decImproveAccuracy = await ImprovementManager.ValueOfAsync(
                _objCharacter, Improvement.ImprovementType.WeaponSkillAccuracy, strImprovedName: Name,
                blnIncludeNonImproved: true, token: token).ConfigureAwait(false);
            string strSkill = Skill?.DictionaryKey ?? string.Empty;
            if (!string.IsNullOrEmpty(strSkill))
                decImproveAccuracy += await ImprovementManager.ValueOfAsync(_objCharacter,
                                                                            Improvement.ImprovementType.WeaponSkillAccuracy,
                                                                            strImprovedName: strSkill, token: token).ConfigureAwait(false);
            foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(
                         _objCharacter, Improvement.ImprovementType.WeaponAccuracy, token: token).ConfigureAwait(false))
            {
                string strImprovedName = objImprovement.ImprovedName;
                if (strImprovedName.StartsWith("[contains]", StringComparison.Ordinal)
                    && strNameUpper.Contains(strImprovedName.TrimStartOnce("[contains]", true),
                                             StringComparison.OrdinalIgnoreCase))
                {
                    decImproveAccuracy += objImprovement.Value;
                }
            }

            intAccuracy += decImproveAccuracy.StandardRound();

            return intAccuracy;
        }

        /// <summary>
        /// Displays the base and Total Accuracy of the weapon in the same format as it appears in rulebooks in the program's current language.
        /// </summary>
        public string DisplayAccuracy => GetAccuracy(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Displays the base and Total Accuracy of the weapon in the same format as it appears in rulebooks.
        /// </summary>
        public string GetAccuracy(CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true)
        {
            int intTotalAccuracy = GetTotalAccuracy(blnIncludeAmmo);
            if (int.TryParse(Accuracy, out int intAccuracy) && intAccuracy != intTotalAccuracy)
                return string.Format(objCulture, "{0}{1}({2})",
                    intAccuracy, LanguageManager.GetString("String_Space", strLanguage), intTotalAccuracy);
            return intTotalAccuracy.ToString(objCulture);
        }

        /// <summary>
        /// Displays the base and Total Accuracy of the weapon in the same format as it appears in rulebooks.
        /// </summary>
        public async ValueTask<string> GetAccuracyAsync(CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            int intTotalAccuracy = await GetTotalAccuracyAsync(blnIncludeAmmo, token).ConfigureAwait(false);
            if (int.TryParse(Accuracy, out int intAccuracy) && intAccuracy != intTotalAccuracy)
                return string.Format(objCulture, "{0}{1}({2})",
                                     intAccuracy, await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false), intTotalAccuracy);
            return intTotalAccuracy.ToString(objCulture);
        }

        /// <summary>
        /// The slots the weapon has for modifications.
        /// </summary>
        public string ModificationSlots => _strWeaponSlots;

        /// <summary>
        /// What modification slots have their costs doubled.
        /// </summary>
        public string DoubledCostModificationSlots => _strDoubledCostWeaponSlots;

        public string Range
        {
            get => _strRange;
            set => _strRange = value;
        }

        public string AlternateRange
        {
            get => _strAlternateRange;
            set => _strAlternateRange = value;
        }

        public string CurrentDisplayRange => DisplayRange(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayRangeAsync(CancellationToken token = default) =>
            DisplayRangeAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The string for the Weapon's Range category
        /// </summary>
        public string DisplayRange(string strLanguage)
        {
            string strRange = Range;
            if (string.IsNullOrWhiteSpace(strRange))
                strRange = Category;
            if (!string.IsNullOrWhiteSpace(strRange) && !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("ranges.xml", strLanguage);
                XPathNavigator objXmlCategoryNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRange);
                XPathNavigator xmlTranslateNode = objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("translate");
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.Value;
                }
                else
                {
                    objXmlDocument = _objCharacter.LoadDataXPath("weapons.xml", strLanguage);
                    objXmlCategoryNode = objXmlDocument.SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " + strRange.CleanXPath() + ']');
                    xmlTranslateNode = objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("@translate");
                    if (xmlTranslateNode != null)
                        strRange = xmlTranslateNode.Value;
                }
            }
            return strRange;
        }

        /// <summary>
        /// The string for the Weapon's Range category
        /// </summary>
        public async ValueTask<string> DisplayRangeAsync(string strLanguage, CancellationToken token = default)
        {
            string strRange = Range;
            if (string.IsNullOrWhiteSpace(strRange))
                strRange = Category;
            if (!string.IsNullOrWhiteSpace(strRange) && !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objXmlDocument = await _objCharacter.LoadDataXPathAsync("ranges.xml", strLanguage, token: token).ConfigureAwait(false);
                XPathNavigator objXmlCategoryNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRange);
                XPathNavigator xmlTranslateNode = objXmlCategoryNode != null
                    ? await objXmlCategoryNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false)
                    : null;
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.Value;
                }
                else
                {
                    objXmlDocument = await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token).ConfigureAwait(false);
                    objXmlCategoryNode = await objXmlDocument
                                               .SelectSingleNodeAndCacheExpressionAsync(
                                                   "/chummer/categories/category[. = " + strRange.CleanXPath() + ']',
                                                   token: token).ConfigureAwait(false);
                    if (objXmlCategoryNode != null)
                    {
                        xmlTranslateNode
                            = await objXmlCategoryNode.SelectSingleNodeAndCacheExpressionAsync("@translate", token: token).ConfigureAwait(false);
                        if (xmlTranslateNode != null)
                            strRange = xmlTranslateNode.Value;
                    }
                }
            }
            return strRange;
        }

        public string CurrentDisplayAlternateRange => DisplayAlternateRange(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayAlternateRangeAsync(CancellationToken token = default) =>
            DisplayAlternateRangeAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The string for the Weapon's Range category (setter is English-only).
        /// </summary>
        public string DisplayAlternateRange(string strLanguage)
        {
            string strRange = AlternateRange.Trim();
            if (!string.IsNullOrEmpty(strRange) && !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("ranges.xml", strLanguage);
                XPathNavigator objXmlCategoryNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRange);
                XPathNavigator xmlTranslateNode = objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("translate");
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.Value;
                }
                else
                {
                    objXmlDocument = _objCharacter.LoadDataXPath("weapons.xml", strLanguage);
                    objXmlCategoryNode = objXmlDocument.SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " + strRange.CleanXPath() + ']');
                    xmlTranslateNode = objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("@translate");
                    if (xmlTranslateNode != null)
                        strRange = xmlTranslateNode.Value;
                }
            }
            return strRange;
        }

        /// <summary>
        /// The string for the Weapon's Range category (setter is English-only).
        /// </summary>
        public async ValueTask<string> DisplayAlternateRangeAsync(string strLanguage, CancellationToken token = default)
        {
            string strRange = AlternateRange.Trim();
            if (!string.IsNullOrEmpty(strRange) && !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objXmlDocument = await _objCharacter.LoadDataXPathAsync("ranges.xml", strLanguage, token: token).ConfigureAwait(false);
                XPathNavigator objXmlCategoryNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRange);
                XPathNavigator xmlTranslateNode = objXmlCategoryNode != null
                    ? await objXmlCategoryNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false)
                    : null;
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.Value;
                }
                else
                {
                    objXmlDocument = await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token).ConfigureAwait(false);
                    objXmlCategoryNode = await objXmlDocument
                                               .SelectSingleNodeAndCacheExpressionAsync(
                                                   "/chummer/categories/category[. = " + strRange.CleanXPath() + ']',
                                                   token: token).ConfigureAwait(false);
                    if (objXmlCategoryNode != null)
                    {
                        xmlTranslateNode
                            = await objXmlCategoryNode.SelectSingleNodeAndCacheExpressionAsync("@translate", token: token).ConfigureAwait(false);
                        if (xmlTranslateNode != null)
                            strRange = xmlTranslateNode.Value;
                    }
                }
            }
            return strRange;
        }

        /// <summary>
        /// Evaluate and return the requested Range for the Weapon.
        /// </summary>
        /// <param name="strFindRange">Range node to use.</param>
        /// <param name="blnUseAlternateRange">Use alternate range instead of the weapon's main range.</param>
        /// <param name="blnIncludeAmmo">Whether to include any range modifications from the currently loaded ammo.</param>
        private int GetRange(string strFindRange, bool blnUseAlternateRange, bool blnIncludeAmmo = true)
        {
            string strRangeCategory = Category;
            if (blnUseAlternateRange)
            {
                strRangeCategory = AlternateRange;
                if (string.IsNullOrWhiteSpace(strRangeCategory))
                    return -1;
            }
            else if (!string.IsNullOrEmpty(Range))
                strRangeCategory = Range;

            if (blnIncludeAmmo)
            {
                // Check if the Weapon has Ammunition loaded and look for any range replacement.
                // Look for Ammo on the character.
                Gear objGear = AmmoLoaded;
                if (objGear != null)
                {
                    string strNewRange = string.Empty;

                    if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                    {
                        if (objGear.FlechetteWeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                        {
                            strRangeCategory = strNewRange;
                        }
                    }
                    else if (objGear.WeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                    {
                        strRangeCategory = strNewRange;
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in objGear.Children.DeepWhere(x => x.Children, x => x.Equipped))
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear"
                                                   && objChild.FlechetteWeaponBonus != null)
                        {
                            if (objChild.FlechetteWeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                            {
                                strRangeCategory = strNewRange;
                            }
                        }
                        else if (objChild.WeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                        {
                            strRangeCategory = strNewRange;
                        }
                    }
                }
            }

            XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("ranges.xml");
            XPathNavigator objXmlCategoryNode = objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRangeCategory);
            if (objXmlCategoryNode?.SelectSingleNode(strFindRange) == null)
            {
                return -1;
            }
            string strRange = objXmlCategoryNode.SelectSingleNode(strFindRange)?.Value ?? string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdRange))
            {
                sbdRange.Append(strRange);
                ProcessAttributesInXPath(sbdRange, strRange, true);

                if (Category == "Throwing Weapons" || Skill?.DictionaryKey == "Throwing Weapons")
                    sbdRange.Append(" + ").Append(ImprovementManager
                                                  .ValueOf(_objCharacter, Improvement.ImprovementType.ThrowRange)
                                                  .ToString(GlobalSettings.InvariantCultureInfo));

                // Replace the division sign with "div" since we're using XPath.
                sbdRange.Replace("/", " div ");

                (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(sbdRange.ToString());

                return blnIsSuccess
                    ? (Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) * _decRangeMultiplier)
                    .StandardRound()
                    : -1;
            }
        }

        /// <summary>
        /// Evaluate and return the requested Range for the Weapon.
        /// </summary>
        /// <param name="strFindRange">Range node to use.</param>
        /// <param name="blnUseAlternateRange">Use alternate range instead of the weapon's main range.</param>
        /// <param name="blnIncludeAmmo">Whether to include any range modifications from the currently loaded ammo.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async ValueTask<int> GetRangeAsync(string strFindRange, bool blnUseAlternateRange,
                                                   bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            string strRangeCategory = Category;
            if (blnUseAlternateRange)
            {
                strRangeCategory = AlternateRange;
                if (string.IsNullOrWhiteSpace(strRangeCategory))
                    return -1;
            }
            else if (!string.IsNullOrEmpty(Range))
                strRangeCategory = Range;

            if (blnIncludeAmmo)
            {
                // Check if the Weapon has Ammunition loaded and look for any range replacement.
                // Look for Ammo on the character.
                Gear objGear = AmmoLoaded;
                if (objGear != null)
                {
                    string strNewRange = string.Empty;

                    if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                    {
                        if (objGear.FlechetteWeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                        {
                            strRangeCategory = strNewRange;
                        }
                    }
                    else if (objGear.WeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                    {
                        strRangeCategory = strNewRange;
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in await objGear.Children.DeepWhereAsync(x => x.Children, x => x.Equipped, token)
                                                .ConfigureAwait(false))
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear"
                                                   && objChild.FlechetteWeaponBonus != null)
                        {
                            if (objChild.FlechetteWeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                            {
                                strRangeCategory = strNewRange;
                            }
                        }
                        else if (objChild.WeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                        {
                            strRangeCategory = strNewRange;
                        }
                    }
                }
            }

            XPathNavigator objXmlDocument
                = await _objCharacter.LoadDataXPathAsync("ranges.xml", token: token).ConfigureAwait(false);
            XPathNavigator objXmlCategoryNode
                = objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRangeCategory);
            if (objXmlCategoryNode?.SelectSingleNode(strFindRange) == null)
            {
                return -1;
            }

            string strRange = objXmlCategoryNode.SelectSingleNode(strFindRange)?.Value ?? string.Empty;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdRange))
            {
                sbdRange.Append(strRange);
                await ProcessAttributesInXPathAsync(sbdRange, strRange, true, token).ConfigureAwait(false);

                if (Category == "Throwing Weapons" || (Skill != null
                                                       && await Skill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                                                       == "Throwing Weapons"))
                    sbdRange.Append(" + ").Append((await ImprovementManager
                                                         .ValueOfAsync(_objCharacter,
                                                                       Improvement.ImprovementType.ThrowRange,
                                                                       token: token).ConfigureAwait(false))
                                                  .ToString(GlobalSettings.InvariantCultureInfo));

                // Replace the division sign with "div" since we're using XPath.
                sbdRange.Replace("/", " div ");

                (bool blnIsSuccess, object objProcess) = await CommonFunctions
                                                               .EvaluateInvariantXPathAsync(sbdRange.ToString(), token)
                                                               .ConfigureAwait(false);

                return blnIsSuccess
                    ? (Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo) * _decRangeMultiplier)
                    .StandardRound()
                    : -1;
            }
        }

        /// <summary>
        /// Weapon's total Range bonus from Accessories.
        /// </summary>
        public int GetRangeBonus(bool blnIncludeAmmo = true)
        {
            // Weapon Mods.
            int intRangeBonus = WeaponAccessories.Sum(x => x.Equipped, x => x.RangeBonus);

            if (blnIncludeAmmo)
            {
                // Check if the Weapon has Ammunition loaded and look for any Range bonus.
                Gear objGear = AmmoLoaded;
                if (objGear != null)
                {
                    if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        intRangeBonus += objGear.FlechetteWeaponBonusRange;
                    else if (objGear.WeaponBonus != null)
                        intRangeBonus += objGear.WeaponBonusRange;
                }
            }

            return intRangeBonus;
        }

        /// <summary>
        /// Weapon's total Range bonus from Accessories.
        /// </summary>
        public async ValueTask<int> GetRangeBonusAsync(bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            // Weapon Mods.
            int intRangeBonus = await WeaponAccessories.SumAsync(x => x.Equipped, x => x.RangeBonus, token: token).ConfigureAwait(false);

            if (blnIncludeAmmo)
            {
                // Check if the Weapon has Ammunition loaded and look for any Range bonus.
                Gear objGear = AmmoLoaded;
                if (objGear != null)
                {
                    if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        intRangeBonus += objGear.FlechetteWeaponBonusRange;
                    else if (objGear.WeaponBonus != null)
                        intRangeBonus += objGear.WeaponBonusRange;
                }
            }

            return intRangeBonus;
        }

        public string RangeModifier(string strRange)
        {
            if (string.IsNullOrEmpty(strRange))
                return string.Empty;
            int i = _objCharacter.LoadDataXPath("ranges.xml").SelectSingleNodeAndCacheExpression("chummer/modifiers/" + strRange.ToLowerInvariant())?.ValueAsInt ?? 0;
            i += WeaponAccessories.Sum(wa => wa.RangeModifier);

            string strNameUpper = Name.ToUpperInvariant();
            decimal decImproveAccuracy = 0;
            foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter, Improvement.ImprovementType.WeaponRangeModifier))
            {
                string strImprovedName = objImprovement.ImprovedName;
                if (string.IsNullOrEmpty(strImprovedName) || strImprovedName == Name
                                                          || strImprovedName.StartsWith(
                                                              "[contains]", StringComparison.Ordinal)
                                                          && strNameUpper.Contains(
                                                              strImprovedName.TrimStartOnce("[contains]", true),
                                                              StringComparison.OrdinalIgnoreCase))
                    decImproveAccuracy += objImprovement.Value;
            }

            i += decImproveAccuracy.StandardRound();
            i = Math.Min(0, i);
            return string.Format(GlobalSettings.InvariantCultureInfo, LanguageManager.GetString("Label_Range" + strRange), i);
        }

        /// <summary>
        /// Dictionary where keys are range categories (short, medium, long, extreme, alternateshort, etc.), values are strings depicting range values for the category.
        /// </summary>
        public Dictionary<string, string> GetRangeStrings(CultureInfo objCulture, bool blnIncludeAmmo = true)
        {
            int intRangeModifier = GetRangeBonus(blnIncludeAmmo) + 100;
            int intMin = GetRange("min", false, blnIncludeAmmo);
            int intShort = GetRange("short", false, blnIncludeAmmo);
            int intMedium = GetRange("medium", false, blnIncludeAmmo);
            int intLong = GetRange("long", false, blnIncludeAmmo);
            int intExtreme = GetRange("extreme", false, blnIncludeAmmo);
            int intAlternateMin = GetRange("min", true, blnIncludeAmmo);
            int intAlternateShort = GetRange("short", true, blnIncludeAmmo);
            int intAlternateMedium = GetRange("medium", true, blnIncludeAmmo);
            int intAlternateLong = GetRange("long", true, blnIncludeAmmo);
            int intAlternateExtreme = GetRange("extreme", true, blnIncludeAmmo);
            if (intMin > 0)
                intMin = (intMin * intRangeModifier + 99) / 100;
            if (intShort > 0)
                intShort = (intShort * intRangeModifier + 99) / 100;
            if (intMedium > 0)
                intMedium = (intMedium * intRangeModifier + 99) / 100;
            if (intLong > 0)
                intLong = (intLong * intRangeModifier + 99) / 100;
            if (intExtreme > 0)
                intExtreme = (intExtreme * intRangeModifier + 99) / 100;
            if (intAlternateMin > 0)
                intAlternateMin = (intAlternateMin * intRangeModifier + 99) / 100;
            if (intAlternateShort > 0)
                intAlternateShort = (intAlternateShort * intRangeModifier + 99) / 100;
            if (intAlternateMedium > 0)
                intAlternateMedium = (intAlternateMedium * intRangeModifier + 99) / 100;
            if (intAlternateLong > 0)
                intAlternateLong = (intAlternateLong * intRangeModifier + 99) / 100;
            if (intAlternateExtreme > 0)
                intAlternateExtreme = (intAlternateExtreme * intRangeModifier + 99) / 100;

            Dictionary<string, string> dicReturn = new Dictionary<string, string>(8)
            {
                {
                    "short",
                    intMin < 0 || intShort < 0
                        ? string.Empty
                        : intMin.ToString(objCulture) + '-' + intShort.ToString(objCulture)
                },
                {
                    "medium",
                    intShort < 0 || intMedium < 0
                        ? string.Empty
                        : (intShort + 1).ToString(objCulture) + '-' + intMedium.ToString(objCulture)
                },
                {
                    "long",
                    intMedium < 0 || intLong < 0
                        ? string.Empty
                        : (intMedium + 1).ToString(objCulture) + '-' + intLong.ToString(objCulture)
                },
                {
                    "extreme",
                    intLong < 0 || intExtreme < 0
                        ? string.Empty
                        : (intLong + 1).ToString(objCulture) + '-' + intExtreme.ToString(objCulture)
                },
                {
                    "alternateshort",
                    intAlternateMin < 0 || intAlternateShort < 0
                        ? string.Empty
                        : intAlternateMin.ToString(objCulture) + '-' + intAlternateShort.ToString(objCulture)
                },
                {
                    "alternatemedium",
                    intAlternateShort < 0 || intAlternateMedium < 0
                        ? string.Empty
                        : (intAlternateShort + 1).ToString(objCulture) + '-' + intAlternateMedium.ToString(objCulture)
                },
                {
                    "alternatelong",
                    intAlternateMedium < 0 || intAlternateLong < 0
                        ? string.Empty
                        : (intAlternateMedium + 1).ToString(objCulture) + '-' + intAlternateLong.ToString(objCulture)
                },
                {
                    "alternateextreme",
                    intAlternateLong < 0 || intAlternateExtreme < 0
                        ? string.Empty
                        : (intAlternateLong + 1).ToString(objCulture) + '-' + intAlternateExtreme.ToString(objCulture)
                }
            };

            return dicReturn;
        }

        /// <summary>
        /// Dictionary where keys are range categories (short, medium, long, extreme, alternateshort, etc.), values are strings depicting range values for the category.
        /// </summary>
        public async ValueTask<Dictionary<string, string>> GetRangeStringsAsync(CultureInfo objCulture, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            int intRangeModifier = await GetRangeBonusAsync(blnIncludeAmmo, token).ConfigureAwait(false) + 100;
            int intMin = await GetRangeAsync("min", false, blnIncludeAmmo, token).ConfigureAwait(false);
            int intShort = await GetRangeAsync("short", false, blnIncludeAmmo, token).ConfigureAwait(false);
            int intMedium = await GetRangeAsync("medium", false, blnIncludeAmmo, token).ConfigureAwait(false);
            int intLong = await GetRangeAsync("long", false, blnIncludeAmmo, token).ConfigureAwait(false);
            int intExtreme = await GetRangeAsync("extreme", false, blnIncludeAmmo, token).ConfigureAwait(false);
            int intAlternateMin = await GetRangeAsync("min", true, blnIncludeAmmo, token).ConfigureAwait(false);
            int intAlternateShort = await GetRangeAsync("short", true, blnIncludeAmmo, token).ConfigureAwait(false);
            int intAlternateMedium = await GetRangeAsync("medium", true, blnIncludeAmmo, token).ConfigureAwait(false);
            int intAlternateLong = await GetRangeAsync("long", true, blnIncludeAmmo, token).ConfigureAwait(false);
            int intAlternateExtreme = await GetRangeAsync("extreme", true, blnIncludeAmmo, token).ConfigureAwait(false);
            if (intMin > 0)
                intMin = (intMin * intRangeModifier + 99) / 100;
            if (intShort > 0)
                intShort = (intShort * intRangeModifier + 99) / 100;
            if (intMedium > 0)
                intMedium = (intMedium * intRangeModifier + 99) / 100;
            if (intLong > 0)
                intLong = (intLong * intRangeModifier + 99) / 100;
            if (intExtreme > 0)
                intExtreme = (intExtreme * intRangeModifier + 99) / 100;
            if (intAlternateMin > 0)
                intAlternateMin = (intAlternateMin * intRangeModifier + 99) / 100;
            if (intAlternateShort > 0)
                intAlternateShort = (intAlternateShort * intRangeModifier + 99) / 100;
            if (intAlternateMedium > 0)
                intAlternateMedium = (intAlternateMedium * intRangeModifier + 99) / 100;
            if (intAlternateLong > 0)
                intAlternateLong = (intAlternateLong * intRangeModifier + 99) / 100;
            if (intAlternateExtreme > 0)
                intAlternateExtreme = (intAlternateExtreme * intRangeModifier + 99) / 100;

            Dictionary<string, string> dicReturn = new Dictionary<string, string>(8)
            {
                {
                    "short",
                    intMin < 0 || intShort < 0
                        ? string.Empty
                        : intMin.ToString(objCulture) + '-' + intShort.ToString(objCulture)
                },
                {
                    "medium",
                    intShort < 0 || intMedium < 0
                        ? string.Empty
                        : (intShort + 1).ToString(objCulture) + '-' + intMedium.ToString(objCulture)
                },
                {
                    "long",
                    intMedium < 0 || intLong < 0
                        ? string.Empty
                        : (intMedium + 1).ToString(objCulture) + '-' + intLong.ToString(objCulture)
                },
                {
                    "extreme",
                    intLong < 0 || intExtreme < 0
                        ? string.Empty
                        : (intLong + 1).ToString(objCulture) + '-' + intExtreme.ToString(objCulture)
                },
                {
                    "alternateshort",
                    intAlternateMin < 0 || intAlternateShort < 0
                        ? string.Empty
                        : intAlternateMin.ToString(objCulture) + '-' + intAlternateShort.ToString(objCulture)
                },
                {
                    "alternatemedium",
                    intAlternateShort < 0 || intAlternateMedium < 0
                        ? string.Empty
                        : (intAlternateShort + 1).ToString(objCulture) + '-' + intAlternateMedium.ToString(objCulture)
                },
                {
                    "alternatelong",
                    intAlternateMedium < 0 || intAlternateLong < 0
                        ? string.Empty
                        : (intAlternateMedium + 1).ToString(objCulture) + '-' + intAlternateLong.ToString(objCulture)
                },
                {
                    "alternateextreme",
                    intAlternateLong < 0 || intAlternateExtreme < 0
                        ? string.Empty
                        : (intAlternateLong + 1).ToString(objCulture) + '-' + intAlternateExtreme.ToString(objCulture)
                }
            };

            return dicReturn;
        }

        /// <summary>
        /// Number of rounds consumed by Single Shot.
        /// </summary>
        public int SingleShot
        {
            get
            {
                int intReturn = _intSingleShot;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.SingleShot > intReturn)
                        intReturn = objAccessory.SingleShot;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Number of rounds consumed by Short Burst.
        /// </summary>
        public int ShortBurst
        {
            get
            {
                int intReturn = _intShortBurst;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.ShortBurst > intReturn)
                        intReturn = objAccessory.ShortBurst;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Number of rounds consumed by Long Burst.
        /// </summary>
        public int LongBurst
        {
            get
            {
                int intReturn = _intLongBurst;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.LongBurst > intReturn)
                        intReturn = objAccessory.LongBurst;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Number of rounds consumed by Full Burst.
        /// </summary>
        public int FullBurst
        {
            get
            {
                int intReturn = _intFullBurst;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.FullBurst > intReturn)
                        intReturn = objAccessory.FullBurst;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Number of rounds consumed by Suppressive Fire.
        /// </summary>
        public int Suppressive
        {
            get
            {
                int intReturn = _intSuppressive;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.Suppressive > intReturn)
                        intReturn = objAccessory.Suppressive;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Total Accessory Cost multiplier for the Weapon.
        /// </summary>
        public int AccessoryMultiplier
        {
            get
            {
                int intReturn = 0;
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.AccessoryCostMultiplier != 1)
                        intReturn += objAccessory.AccessoryCostMultiplier;
                }

                if (intReturn == 0)
                    intReturn = 1;

                return intReturn;
            }
        }

        /// <summary>
        /// Returns true if the weapon has a smartgun addon/accessory/modification that is wirelessly on
        /// </summary>
        public bool HasWirelessSmartgun
        {
            get
            {
                if (WeaponAccessories.Any(x => x.Name.StartsWith("Smartgun", StringComparison.Ordinal) && x.Equipped
                                              && x.WirelessOn))
                    return true;
                // Underbarrel weapons that come with their parent weapon (and are of the same type) should inherit the parent weapon's built-in smartgun features
                return IncludedInWeapon && Parent != null && RangeType == Parent.RangeType
                       && Parent.WeaponAccessories.Any(x => x.Name.StartsWith("Smartgun", StringComparison.Ordinal)
                                                            && x.IncludedInWeapon
                                                            && x.Equipped
                                                            && x.WirelessOn);
            }
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to use the Weapon.
        /// </summary>
        public int DicePool => GetDicePool();

        /// <summary>
        /// The Dice Pool size for the Active Skill required to use the Weapon.
        /// </summary>
        public int GetDicePool(bool blnIncludeAmmo = true)
        {
            int intDicePool = 0;
            decimal decDicePoolModifier = WeaponAccessories.Sum(a => a.Equipped, a => a.DicePool);
            switch (FireMode)
            {
                case FiringMode.DogBrain:
                {
                    intDicePool = ParentVehicle.Pilot;
                    Gear objAutosoft = ParentVehicle.GearChildren.DeepFirstOrDefault(
                        x => x.Children,
                        x => x.Name == RelevantAutosoft && (x.Extra == Name || x.Extra == CurrentDisplayName));

                    intDicePool += objAutosoft?.Rating ?? -1;

                    if (WirelessOn && HasWirelessSmartgun
                                   && ParentVehicle.GearChildren.DeepAny(
                                       x => x.Children, x => x.Name == "Smartsoft" && x.Equipped))
                    {
                        ++decDicePoolModifier;
                    }

                    break;
                }
                case FiringMode.RemoteOperated:
                {
                    intDicePool = _objCharacter.SkillsSection.GetActiveSkill("Gunnery").PoolOtherAttribute("LOG");

                    if (WirelessOn && HasWirelessSmartgun)
                    {
                        decimal decSmartlinkBonus = ImprovementManager.ValueOf(_objCharacter,
                                                                               Improvement.ImprovementType.Smartlink);
                        foreach (Gear objLoopGear in ParentVehicle.GearChildren.DeepWhere(x => x.Children,
                                     x => x.Bonus?.InnerXml.Contains("<smartlink>") == true))
                        {
                            string strLoopBonus = string.Empty;
                            if (objLoopGear.Bonus.TryGetStringFieldQuickly("smartlink", ref strLoopBonus))
                            {
                                decSmartlinkBonus = Math.Max(decSmartlinkBonus, ImprovementManager.ValueToDec(
                                                                 _objCharacter, strLoopBonus,
                                                                 objLoopGear.Rating));
                            }
                        }

                        decDicePoolModifier += decSmartlinkBonus;
                    }

                    decDicePoolModifier
                        += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice,
                                                      false, Category);
                    break;
                }
                case FiringMode.GunneryCommandDevice:
                case FiringMode.ManualOperation:
                {
                    Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill("Gunnery");
                    if (Cyberware && Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(objSkill.Attribute)
                                  && _objMountedVehicle == null)
                    {
                        Cyberware objAttributeSource = _objCharacter.Cyberware.DeepFindById(ParentID);
                        while (objAttributeSource != null
                               && objAttributeSource.GetAttributeTotalValue(objSkill.Attribute) == 0)
                        {
                            objAttributeSource = objAttributeSource.Parent;
                        }

                        if (objAttributeSource != null)
                            intDicePool = objSkill.PoolOtherAttribute(
                                objSkill.Attribute, false,
                                objAttributeSource.GetAttributeTotalValue(objSkill.Attribute));
                        else
                            intDicePool = objSkill.Pool;
                    }
                    else
                        intDicePool = objSkill.Pool;

                    if (WirelessOn && HasWirelessSmartgun)
                    {
                        decDicePoolModifier +=
                            ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Smartlink);
                    }

                    decDicePoolModifier
                        += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice,
                                                      false, Category);
                    break;
                }
                case FiringMode.Skill:
                {
                    Skill objSkill = Skill;
                    if (objSkill != null)
                    {
                        if (Cyberware && Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(objSkill.Attribute))
                        {
                            Cyberware objAttributeSource = _objMountedVehicle != null
                                ? _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID)
                                : _objCharacter.Cyberware.DeepFindById(ParentID);
                            while (objAttributeSource != null
                                   && objAttributeSource.GetAttributeTotalValue(objSkill.Attribute) == 0)
                            {
                                objAttributeSource = objAttributeSource.Parent;
                            }

                            if (objAttributeSource != null)
                                intDicePool = objSkill.PoolOtherAttribute(
                                    objSkill.Attribute, false,
                                    objAttributeSource.GetAttributeTotalValue(objSkill.Attribute));
                            else
                                intDicePool = objSkill.Pool;
                        }
                        else
                            intDicePool = objSkill.Pool;

                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            decDicePoolModifier
                                += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Smartlink);
                        }

                        decDicePoolModifier
                            += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice,
                                                          false, Category);
                        decDicePoolModifier += ImprovementManager.ValueOf(
                            _objCharacter, Improvement.ImprovementType.WeaponSpecificDice, false, InternalId);

                        // If the character has a Specialization, include it in the Dice Pool string.
                        if (objSkill.Specializations.Count > 0 && !objSkill.IsExoticSkill)
                        {
                            SkillSpecialization objSpec =
                                objSkill.GetSpecialization(CurrentDisplayNameShort) ??
                                objSkill.GetSpecialization(Name) ??
                                objSkill.GetSpecialization(DisplayCategory(GlobalSettings.Language)) ??
                                objSkill.GetSpecialization(Category);

                            if (objSpec == null && objSkill.Specializations.Count > 0)
                            {
                                objSpec = objSkill.GetSpecialization(Spec) ?? objSkill.GetSpecialization(Spec2);
                            }

                            if (objSpec != null)
                            {
                                decDicePoolModifier += objSpec.SpecializationBonus;
                            }
                        }
                    }

                    break;
                }
            }

            if (FireMode == FiringMode.GunneryCommandDevice || FireMode == FiringMode.RemoteOperated ||
                FireMode == FiringMode.ManualOperation)
            {
                Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill("Gunnery");
                if (objSkill != null)
                {
                    string strSpec = RelevantSpecialization;
                    if (!string.IsNullOrEmpty(strSpec) && strSpec != "None" && objSkill.Specializations.Count > 0)
                    {
                        intDicePool += objSkill.GetSpecializationBonus(strSpec);
                    }
                }
            }

            if (blnIncludeAmmo)
            {
                Gear objAmmo = AmmoLoaded;
                if (objAmmo != null)
                {
                    string strWeaponBonusPool;
                    if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objAmmo.FlechetteWeaponBonus != null)
                    {
                        strWeaponBonusPool = objAmmo.FlechetteWeaponBonus["pool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = objAmmo.FlechetteWeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                        }
                    }
                    else if (objAmmo.WeaponBonus != null)
                    {
                        strWeaponBonusPool = objAmmo.WeaponBonus["pool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = objAmmo.WeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                        }
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in objAmmo.Children.DeepWhere(x => x.Children, x => x.Equipped))
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                        {
                            strWeaponBonusPool = objChild.FlechetteWeaponBonus["pool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objChild.FlechetteWeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                            }
                        }
                        else if (objChild.WeaponBonus != null)
                        {
                            strWeaponBonusPool = objChild.WeaponBonus["pool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objChild.WeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }
                }
            }

            return intDicePool + decDicePoolModifier.StandardRound();
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to use the Weapon.
        /// </summary>
        public async ValueTask<int> GetDicePoolAsync(bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            int intDicePool = 0;
            decimal decDicePoolModifier = await WeaponAccessories.SumAsync(a => a.Equipped, a => a.DicePool, token: token).ConfigureAwait(false);
            switch (FireMode)
            {
                case FiringMode.DogBrain:
                    {
                        intDicePool = ParentVehicle.Pilot;
                        Gear objAutosoft = await ParentVehicle.GearChildren.DeepFirstOrDefaultAsync(
                            x => x.Children,
                            x => x.Name == RelevantAutosoft && (x.Extra == Name || x.Extra == CurrentDisplayName), token).ConfigureAwait(false);

                        intDicePool += objAutosoft?.Rating ?? -1;

                        if (WirelessOn && HasWirelessSmartgun
                                       && await ParentVehicle.GearChildren.DeepAnyAsync(
                                           x => x.Children, x => x.Name == "Smartsoft" && x.Equipped, token).ConfigureAwait(false))
                        {
                            ++decDicePoolModifier;
                        }

                        break;
                    }
                case FiringMode.RemoteOperated:
                    {
                        intDicePool = await (await _objCharacter.SkillsSection.GetActiveSkillAsync("Gunnery", token).ConfigureAwait(false)).PoolOtherAttributeAsync("LOG", token: token).ConfigureAwait(false);

                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            decimal decSmartlinkBonus = await ImprovementManager.ValueOfAsync(_objCharacter,
                                Improvement.ImprovementType.Smartlink, token: token).ConfigureAwait(false);
                            foreach (Gear objLoopGear in await ParentVehicle.GearChildren.DeepWhereAsync(x => x.Children,
                                         x => x.Bonus?.InnerXml.Contains("<smartlink>") == true, token).ConfigureAwait(false))
                            {
                                string strLoopBonus = string.Empty;
                                if (objLoopGear.Bonus.TryGetStringFieldQuickly("smartlink", ref strLoopBonus))
                                {
                                    decSmartlinkBonus = Math.Max(decSmartlinkBonus, await ImprovementManager.ValueToDecAsync(
                                                                     _objCharacter, strLoopBonus,
                                                                     objLoopGear.Rating, token).ConfigureAwait(false));
                                }
                            }

                            decDicePoolModifier += decSmartlinkBonus;
                        }

                        decDicePoolModifier
                            += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice,
                                                                     false, Category, token: token).ConfigureAwait(false);
                        break;
                    }
                case FiringMode.GunneryCommandDevice:
                case FiringMode.ManualOperation:
                    {
                        Skill objSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync("Gunnery", token).ConfigureAwait(false);
                        if (Cyberware && Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(objSkill.Attribute)
                                      && _objMountedVehicle == null)
                        {
                            Cyberware objAttributeSource = _objCharacter.Cyberware.DeepFindById(ParentID);
                            while (objAttributeSource != null
                                   && await objAttributeSource.GetAttributeTotalValueAsync(objSkill.Attribute, token).ConfigureAwait(false) == 0)
                            {
                                objAttributeSource = objAttributeSource.Parent;
                            }

                            if (objAttributeSource != null)
                                intDicePool = await objSkill.PoolOtherAttributeAsync(
                                    objSkill.Attribute, false,
                                    await objAttributeSource.GetAttributeTotalValueAsync(objSkill.Attribute, token).ConfigureAwait(false), token).ConfigureAwait(false);
                            else
                                intDicePool = await objSkill.GetPoolAsync(token).ConfigureAwait(false);
                        }
                        else
                            intDicePool = await objSkill.GetPoolAsync(token).ConfigureAwait(false);

                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            decDicePoolModifier +=
                                await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Smartlink, token: token).ConfigureAwait(false);
                        }

                        decDicePoolModifier
                            += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice,
                                                                     false, Category, token: token).ConfigureAwait(false);
                        break;
                    }
                case FiringMode.Skill:
                    {
                        Skill objSkill = Skill;
                        if (objSkill != null)
                        {
                            if (Cyberware && Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(objSkill.Attribute))
                            {
                                Cyberware objAttributeSource = _objMountedVehicle != null
                                    ? _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID)
                                    : _objCharacter.Cyberware.DeepFindById(ParentID);
                                while (objAttributeSource != null
                                       && await objAttributeSource.GetAttributeTotalValueAsync(objSkill.Attribute, token).ConfigureAwait(false) == 0)
                                {
                                    objAttributeSource = objAttributeSource.Parent;
                                }

                                if (objAttributeSource != null)
                                    intDicePool = await objSkill.PoolOtherAttributeAsync(
                                        objSkill.Attribute, false,
                                        await objAttributeSource.GetAttributeTotalValueAsync(objSkill.Attribute, token).ConfigureAwait(false), token).ConfigureAwait(false);
                                else
                                    intDicePool = await objSkill.GetPoolAsync(token).ConfigureAwait(false);
                            }
                            else
                                intDicePool = await objSkill.GetPoolAsync(token).ConfigureAwait(false);

                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                decDicePoolModifier
                                    += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Smartlink, token: token).ConfigureAwait(false);
                            }

                            decDicePoolModifier
                                += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice,
                                                                         false, Category, token: token).ConfigureAwait(false);
                            decDicePoolModifier += await ImprovementManager.ValueOfAsync(
                                _objCharacter, Improvement.ImprovementType.WeaponSpecificDice, false, InternalId, token: token).ConfigureAwait(false);

                            // If the character has a Specialization, include it in the Dice Pool string.
                            if (!objSkill.IsExoticSkill && await objSkill.Specializations.GetCountAsync(token).ConfigureAwait(false) > 0)
                            {
                                SkillSpecialization objSpec =
                                    await objSkill.GetSpecializationAsync(await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token).ConfigureAwait(false) ??
                                    await objSkill.GetSpecializationAsync(Name, token).ConfigureAwait(false) ??
                                    await objSkill.GetSpecializationAsync(await DisplayCategoryAsync(GlobalSettings.Language, token).ConfigureAwait(false), token).ConfigureAwait(false) ??
                                    await objSkill.GetSpecializationAsync(Category, token).ConfigureAwait(false);

                                if (objSpec == null && objSkill.Specializations.Count > 0)
                                {
                                    objSpec = await objSkill.GetSpecializationAsync(Spec, token).ConfigureAwait(false) ?? await objSkill.GetSpecializationAsync(Spec2, token).ConfigureAwait(false);
                                }

                                if (objSpec != null)
                                {
                                    decDicePoolModifier += await objSpec.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                                }
                            }
                        }

                        break;
                    }
            }

            if (FireMode == FiringMode.GunneryCommandDevice || FireMode == FiringMode.RemoteOperated ||
                FireMode == FiringMode.ManualOperation)
            {
                Skill objSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync("Gunnery", token).ConfigureAwait(false);
                if (objSkill != null)
                {
                    string strSpec = await GetRelevantSpecializationAsync(token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSpec) && strSpec != "None" && await objSkill.Specializations.GetCountAsync(token).ConfigureAwait(false) > 0)
                    {
                        intDicePool += await objSkill.GetSpecializationBonusAsync(strSpec, token).ConfigureAwait(false);
                    }
                }
            }

            if (blnIncludeAmmo)
            {
                Gear objAmmo = AmmoLoaded;
                if (objAmmo != null)
                {
                    string strWeaponBonusPool;
                    if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objAmmo.FlechetteWeaponBonus != null)
                    {
                        strWeaponBonusPool = objAmmo.FlechetteWeaponBonus["pool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = objAmmo.FlechetteWeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                        }
                    }
                    else if (objAmmo.WeaponBonus != null)
                    {
                        strWeaponBonusPool = objAmmo.WeaponBonus["pool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = objAmmo.WeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                        }
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in await objAmmo.Children.DeepWhereAsync(x => x.Children, x => x.Equipped, token).ConfigureAwait(false))
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                        {
                            strWeaponBonusPool = objChild.FlechetteWeaponBonus["pool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objChild.FlechetteWeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                            }
                        }
                        else if (objChild.WeaponBonus != null)
                        {
                            strWeaponBonusPool = objChild.WeaponBonus["pool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objChild.WeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    decDicePoolModifier += Convert.ToDecimal(strWeaponBonusPool, GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }
                }
            }

            return intDicePool + decDicePoolModifier.StandardRound();
        }

        private string _strRelevantSpec = string.Empty;

        internal string RelevantSpecialization
        {
            get
            {
                if (!string.IsNullOrEmpty(_strRelevantSpec))
                {
                    return _strRelevantSpec;
                }

                string strGunnerySpec = this.GetNodeXPath()?.SelectSingleNodeAndCacheExpression("category/@gunneryspec")?.Value;
                if (string.IsNullOrEmpty(strGunnerySpec))
                {
                    strGunnerySpec = _objCharacter.LoadDataXPath("weapons.xml")
                                                  .SelectSingleNode(
                                                      "/chummer/categories/category[. = " + Category.CleanXPath()
                                                      + "]/@gunneryspec")?.Value ?? "None";
                }

                _strRelevantSpec = strGunnerySpec;
                return _strRelevantSpec;
            }
        }

        internal async ValueTask<string> GetRelevantSpecializationAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(_strRelevantSpec))
            {
                return _strRelevantSpec;
            }

            XPathNavigator xmlNode = await this.GetNodeXPathAsync(token).ConfigureAwait(false);
            if (xmlNode != null)
            {
                string strGunnerySpec = (await xmlNode.SelectSingleNodeAndCacheExpressionAsync("category/@gunneryspec", token).ConfigureAwait(false))?.Value;
                if (string.IsNullOrEmpty(strGunnerySpec))
                {
                    strGunnerySpec = (await _objCharacter.LoadDataXPathAsync("weapons.xml", token: token).ConfigureAwait(false))
                                                  .SelectSingleNode(
                                                      "/chummer/categories/category[. = " + Category.CleanXPath()
                                                      + "]/@gunneryspec")?.Value ?? "None";
                }

                return _strRelevantSpec = strGunnerySpec;
            }
            return string.Empty;
        }

        private Skill Skill
        {
            get
            {
                string strCategory = Category;
                string strSpec = string.Empty;

                // If this is a Special Weapon, use the Range to determine the required Active Skill (if present).
                if (strCategory == "Special Weapons" && !string.IsNullOrEmpty(Range))
                    strCategory = Range;

                // Exotic Skills require a matching Specialization.
                string strSkill = GetSkillName(strCategory, ref strSpec);

                // Use the Skill defined by the Weapon if one is present.
                if (!string.IsNullOrEmpty(UseSkill))
                {
                    strSkill = UseSkill;
                    strSpec = string.Empty;

                    (bool blnIsExotic, string strSkillName)
                        = ExoticSkill.IsExoticSkillNameTuple(_objCharacter, UseSkill);
                    if (blnIsExotic)
                    {
                        strSkill = strSkillName;
                        strSpec = UseSkillSpec;
                    }
                }

                // Locate the Active Skill to be used.
                Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(strSkill);
                if (string.IsNullOrEmpty(strSpec) || objSkill == null || objSkill.HasSpecialization(strSpec)
                    || objSkill.HasSpecialization(Name)
                    || (!string.IsNullOrEmpty(Spec2) && objSkill.HasSpecialization(Spec2)))
                    return objSkill;
                return null;
            }
        }

        private string GetSkillName(string strCategory, ref string strSpec)
        {
            string strSkill;
            switch (strCategory)
            {
                case "Bows":
                case "Crossbows":
                    strSkill = "Archery";
                    break;

                case "Assault Rifles":
                case "Carbines":
                case "Machine Pistols":
                case "Submachine Guns":
                    strSkill = "Automatics";
                    break;

                case "Blades":
                    strSkill = "Blades";
                    break;

                case "Clubs":
                case "Improvised Weapons":
                    strSkill = "Clubs";
                    break;

                case "Exotic Melee Weapons":
                    strSkill = "Exotic Melee Weapon";
                    strSpec = UseSkillSpec;
                    break;

                case "Exotic Ranged Weapons":
                case "Special Weapons":
                    strSkill = "Exotic Ranged Weapon";
                    strSpec = UseSkillSpec;
                    break;

                case "Flamethrowers":
                    strSkill = "Exotic Ranged Weapon";
                    strSpec = "Flamethrowers";
                    break;

                case "Laser Weapons":
                    strSkill = "Exotic Ranged Weapon";
                    strSpec = "Laser Weapons";
                    break;

                case "Assault Cannons":
                case "Grenade Launchers":
                case "Missile Launchers":
                case "Light Machine Guns":
                case "Medium Machine Guns":
                case "Heavy Machine Guns":
                    strSkill = "Heavy Weapons";
                    break;

                case "Shotguns":
                case "Sniper Rifles":
                case "Sporting Rifles":
                    strSkill = "Longarms";
                    break;

                case "Throwing Weapons":
                    strSkill = "Throwing Weapons";
                    break;

                case "Unarmed":
                    strSkill = "Unarmed Combat";
                    break;

                default:
                    strSkill = "Pistols";
                    break;
            }
            return strSkill;
        }

        /// <summary>
        /// Tooltip information for the Dice Pool.
        /// </summary>
        public string DicePoolTooltip
        {
            get
            {
                string strSpace = LanguageManager.GetString("String_Space");
                string strExtra;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdExtra))
                {
                    foreach (WeaponAccessory wa in WeaponAccessories.Where(a => a.Equipped && a.DicePool != 0))
                    {
                        sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                              strSpace, wa.CurrentDisplayName, wa.DicePool);
                    }

                    Gear objLoadedAmmo = AmmoLoaded;
                    if (objLoadedAmmo != null)
                    {
                        string strWeaponBonusPool;
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objLoadedAmmo.FlechetteWeaponBonus != null)
                        {
                            strWeaponBonusPool = objLoadedAmmo.FlechetteWeaponBonus["pool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    string strInner = objLoadedAmmo.FlechetteWeaponBonus["smartlinkpool"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strInner))
                                    {
                                        if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                            && decimal.TryParse(strInner, out decimal decTemp2))
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                    .Append((decTemp + decTemp2).StandardRound().ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                        else
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace)
                                                    .Append('(')
                                                    .Append(strWeaponBonusPool).Append(strSpace).Append('+')
                                                    .Append(strSpace).Append(strInner).Append(')');
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool).Append(')');
                                }
                            }
                            else if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objLoadedAmmo.FlechetteWeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool).Append(')');
                                }
                            }
                        }
                        else if (objLoadedAmmo.WeaponBonus != null)
                        {
                            strWeaponBonusPool = objLoadedAmmo.WeaponBonus["pool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    string strInner = objLoadedAmmo.WeaponBonus["smartlinkpool"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strInner))
                                    {
                                        if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                            && decimal.TryParse(strInner, out decimal decTemp2))
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                    .Append((decTemp + decTemp2).StandardRound().ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                        else
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace)
                                                    .Append('(')
                                                    .Append(strWeaponBonusPool).Append(strSpace).Append('+')
                                                    .Append(strSpace).Append(strInner).Append(')');
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool).Append(')');
                                }
                            }
                            else if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objLoadedAmmo.WeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool).Append(')');
                                }
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in objLoadedAmmo.Children.DeepWhere(x => x.Children, x => x.Equipped))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                            {
                                strWeaponBonusPool = objChild.FlechetteWeaponBonus["pool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    if (WirelessOn && HasWirelessSmartgun)
                                    {
                                        string strInner = objChild.FlechetteWeaponBonus["smartlinkpool"]?.InnerText;
                                        if (!string.IsNullOrEmpty(strInner))
                                        {
                                            if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                                && decimal.TryParse(strInner, out decimal decTemp2))
                                            {
                                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                        .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                        .Append((decTemp + decTemp2).StandardRound().ToString(GlobalSettings.CultureInfo)).Append(')');
                                            }
                                            else
                                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                        .Append(objChild.CurrentDisplayNameShort).Append(strSpace)
                                                        .Append('(')
                                                        .Append(strWeaponBonusPool).Append(strSpace).Append('+')
                                                        .Append(strSpace).Append(strInner).Append(')');
                                        }
                                        else
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                    .Append(strWeaponBonusPool).Append(')');
                                        }
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                                else if (WirelessOn && HasWirelessSmartgun)
                                {
                                    strWeaponBonusPool = objChild.FlechetteWeaponBonus["smartlinkpool"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                strWeaponBonusPool = objChild.WeaponBonus["pool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    if (WirelessOn && HasWirelessSmartgun)
                                    {
                                        string strInner = objChild.WeaponBonus["smartlinkpool"]?.InnerText;
                                        if (!string.IsNullOrEmpty(strInner))
                                        {
                                            if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                                && decimal.TryParse(strInner, out decimal decTemp2))
                                            {
                                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                        .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                        .Append((decTemp + decTemp2).StandardRound().ToString(GlobalSettings.CultureInfo)).Append(')');
                                            }
                                            else
                                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                        .Append(objChild.CurrentDisplayNameShort).Append(strSpace)
                                                        .Append('(')
                                                        .Append(strWeaponBonusPool).Append(strSpace).Append('+')
                                                        .Append(strSpace).Append(strInner).Append(')');
                                        }
                                        else
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                    .Append(strWeaponBonusPool).Append(')');
                                        }
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                                else if (WirelessOn && HasWirelessSmartgun)
                                {
                                    strWeaponBonusPool = objChild.WeaponBonus["smartlinkpool"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                            }
                        }
                    }

                    if (ParentVehicle == null)
                    {
                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            decimal decSmartlinkBonus =
                                ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Smartlink);
                            if (decSmartlinkBonus != 0)
                                sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                                      strSpace, LanguageManager.GetString("Tip_Skill_Smartlink"),
                                                      decSmartlinkBonus);
                        }

                        foreach (Improvement objImprovement in ImprovementManager
                                                               .GetCachedImprovementListForValueOf(
                                                                   _objCharacter,
                                                                   Improvement.ImprovementType.WeaponCategoryDice,
                                                                   Category)
                                                               .Concat(
                                                                   ImprovementManager
                                                                       .GetCachedImprovementListForValueOf(
                                                                           _objCharacter,
                                                                           Improvement.ImprovementType
                                                                               .WeaponSpecificDice, InternalId)))
                        {
                            sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                                  strSpace, _objCharacter.GetObjectName(objImprovement),
                                                  objImprovement.Value);
                        }
                    }
                    else if (WirelessOn && HasWirelessSmartgun)
                    {
                        decimal decSmartlinkBonus
                            = ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Smartlink);
                        switch (FireMode)
                        {
                            case FiringMode.DogBrain:
                                if (ParentVehicle.GearChildren.DeepAny(x => x.Children,
                                                                       x => x.Name == "Smartsoft" && x.Equipped))
                                    sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                                          strSpace, LanguageManager.GetString("Tip_Skill_Smartlink"),
                                                          1);
                                break;

                            case FiringMode.RemoteOperated:
                                foreach (Gear objLoopGear in ParentVehicle.GearChildren.DeepWhere(x => x.Children,
                                             x => x.Bonus?.InnerXml.Contains("<smartlink>") == true))
                                {
                                    string strLoopBonus = string.Empty;
                                    if (objLoopGear.Bonus.TryGetStringFieldQuickly("smartlink", ref strLoopBonus))
                                    {
                                        decSmartlinkBonus = Math.Max(decSmartlinkBonus, ImprovementManager.ValueToDec(
                                                                         _objCharacter, strLoopBonus,
                                                                         objLoopGear.Rating));
                                    }
                                }

                                if (decSmartlinkBonus != 0)
                                    sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                                          strSpace, LanguageManager.GetString("Tip_Skill_Smartlink"),
                                                          decSmartlinkBonus);
                                break;

                            case FiringMode.GunneryCommandDevice:
                            case FiringMode.ManualOperation:
                                if (decSmartlinkBonus != 0)
                                    sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                                          strSpace, LanguageManager.GetString("Tip_Skill_Smartlink"),
                                                          decSmartlinkBonus);
                                break;
                        }
                    }

                    strExtra = sbdExtra.ToString();
                }

                string strReturn = LanguageManager.GetString("String_Special");
                switch (FireMode)
                {
                    case FiringMode.DogBrain:
                        {
                            strReturn = string.Format(GlobalSettings.CultureInfo, "{1}{0}({2})",
                                strSpace, LanguageManager.GetString("String_Pilot"), ParentVehicle?.Pilot ?? 0);
                            Gear objAutosoft = ParentVehicle?.GearChildren.DeepFirstOrDefault(x => x.Children,
                                x => x.Name == RelevantAutosoft &&
                                     (x.Extra == Name || x.Extra == CurrentDisplayName));
                            if (objAutosoft != null)
                            {
                                strReturn += string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                    strSpace, objAutosoft.CurrentDisplayName, objAutosoft.Rating);
                            }
                            else
                            {
                                strReturn += string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                    strSpace, LanguageManager.GetString("Tip_Skill_Defaulting"), -1);
                            }

                            strReturn += strExtra;
                            break;
                        }
                    case FiringMode.RemoteOperated:
                        {
                            Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill("Gunnery");
                            if (objSkill.Specializations.Count > 0 && RelevantSpecialization != "None")
                            {
                                SkillSpecialization spec = objSkill.GetSpecialization(RelevantSpecialization);
                                if (spec != null)
                                {
                                    int intSpecBonus = spec.SpecializationBonus;
                                    if (intSpecBonus != 0)
                                        strExtra = string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                            strSpace, spec.CurrentDisplayName, intSpecBonus) + strExtra;
                                }
                            }

                            strReturn = objSkill.CompileDicepoolTooltip("LOG", objSkill.CurrentDisplayName + strSpace, strExtra);
                            break;
                        }
                    case FiringMode.GunneryCommandDevice:
                    case FiringMode.ManualOperation:
                        {
                            Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill("Gunnery");
                            if (objSkill.Specializations.Count > 0 && RelevantSpecialization != "None")
                            {
                                SkillSpecialization spec = objSkill.GetSpecialization(RelevantSpecialization);
                                if (spec != null)
                                {
                                    int intSpecBonus = spec.SpecializationBonus;
                                    if (intSpecBonus != 0)
                                        strExtra = string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                            strSpace, spec.CurrentDisplayName, intSpecBonus) + strExtra;
                                }
                            }

                            Cyberware objAttributeSource =
                                Cyberware && Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(objSkill.Attribute) &&
                                _objMountedVehicle == null
                                    ? _objCharacter.Cyberware.DeepFindById(ParentID)
                                    : null;
                            while (objAttributeSource != null && objAttributeSource.GetAttributeTotalValue(objSkill.Attribute) == 0)
                            {
                                objAttributeSource = objAttributeSource.Parent;
                            }

                            strReturn = objSkill.CompileDicepoolTooltip(string.Empty,
                                objSkill.CurrentDisplayName + strSpace, strExtra,
                                !Cyberware || _objMountedVehicle != null, objAttributeSource);
                            break;
                        }
                    case FiringMode.Skill:
                        {
                            Skill objSkill = Skill;
                            if (objSkill != null)
                            {
                                // If the character has a Specialization, include it in the Dice Pool string.
                                if (objSkill.Specializations.Count > 0 && !objSkill.IsExoticSkill)
                                {
                                    SkillSpecialization spec =
                                        objSkill.GetSpecialization(CurrentDisplayNameShort) ??
                                        objSkill.GetSpecialization(Name) ??
                                        objSkill.GetSpecialization(DisplayCategory(GlobalSettings.Language)) ??
                                        objSkill.GetSpecialization(Category);

                                    if (spec == null)
                                    {
                                        spec = objSkill.GetSpecialization(Category.EndsWith('s')
                                            ? Category.TrimEndOnce('s')
                                            : Category + 's');
                                        if (spec == null && objSkill.Specializations.Count > 0)
                                            spec = objSkill.GetSpecialization(Spec) ?? objSkill.GetSpecialization(Spec2);
                                    }
                                    if (spec != null)
                                    {
                                        int intSpecBonus = spec.SpecializationBonus;
                                        if (intSpecBonus != 0)
                                            strExtra = string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                                strSpace, spec.CurrentDisplayName, intSpecBonus) + strExtra;
                                    }
                                }

                                Cyberware objAttributeSource = null;
                                if (Cyberware && Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(objSkill.Attribute))
                                {
                                    objAttributeSource =
                                        _objMountedVehicle?.FindVehicleCyberware(x => x.InternalId == ParentID) ??
                                        _objCharacter.Cyberware.DeepFindById(ParentID);

                                    while (objAttributeSource != null && objAttributeSource.GetAttributeTotalValue(objSkill.Attribute) == 0)
                                    {
                                        objAttributeSource = objAttributeSource.Parent;
                                    }
                                }

                                strReturn = objSkill.CompileDicepoolTooltip(string.Empty,
                                    objSkill.CurrentDisplayName + strSpace, strExtra, !Cyberware,
                                    objAttributeSource);
                            }

                            break;
                        }
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public ValueTask<string> GetDisplayTotalAvailAsync(CancellationToken token = default) => TotalAvailAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Total Availability.
        /// </summary>
        public async ValueTask<string> TotalAvailAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return await (await TotalAvailTupleAsync(token: token).ConfigureAwait(false))
                         .ToStringAsync(objCulture, strLanguage, token).ConfigureAwait(false);
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
            bool blnCheckUnderbarrels = blnCheckChildren;
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
                    sbdAvail.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));

                    if (blnCheckUnderbarrels && strAvail.Contains("{Children Avail}"))
                    {
                        blnCheckUnderbarrels = false;
                        int intMaxChildAvail = 0;
                        foreach (Weapon objUnderbarrel in UnderbarrelWeapons)
                        {
                            if (objUnderbarrel.ParentID != InternalId)
                            {
                                AvailabilityValue objLoopAvail = objUnderbarrel.TotalAvailTuple();
                                if (objLoopAvail.AddToParent)
                                    intAvail += objLoopAvail.Value;
                                else if (objLoopAvail.Value > intMaxChildAvail)
                                    intMaxChildAvail = objLoopAvail.Value;
                                if (objLoopAvail.Suffix == 'F')
                                    chrLastAvailChar = 'F';
                                else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                                    chrLastAvailChar = 'R';
                            }
                        }

                        sbdAvail.Replace("{Children Avail}",
                                         intMaxChildAvail.ToString(GlobalSettings.InvariantCultureInfo));
                    }

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.GetAllAttributes())
                    {
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev,
                                              () => objLoopAttribute.TotalValue.ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                        sbdAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base",
                                              () => objLoopAttribute.TotalBase.ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                    }

                    // Replace the division sign with "div" since we're using XPath.
                    sbdAvail.Replace("/", " div ");
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString());
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
            }

            if (blnCheckUnderbarrels)
            {
                foreach (Weapon objUnderbarrel in UnderbarrelWeapons)
                {
                    if (objUnderbarrel.ParentID != InternalId)
                    {
                        AvailabilityValue objLoopAvail = objUnderbarrel.TotalAvailTuple();
                        if (objLoopAvail.AddToParent)
                            intAvail += objLoopAvail.Value;
                        if (objLoopAvail.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (!objAccessory.IncludedInWeapon && objAccessory.Equipped)
                    {
                        AvailabilityValue objLoopAvail = objAccessory.TotalAvailTuple();
                        if (objLoopAvail.AddToParent)
                            intAvail += objLoopAvail.Value;
                        if (objLoopAvail.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInWeapon);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async ValueTask<AvailabilityValue> TotalAvailTupleAsync(bool blnCheckChildren = true, CancellationToken token = default)
        {
            bool blnModifyParentAvail = false;
            string strAvail = Avail;
            char chrLastAvailChar = ' ';
            int intAvail = 0;
            bool blnCheckUnderbarrels = blnCheckChildren;
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
                    await sbdAvail.CheapReplaceAsync("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

                    if (blnCheckUnderbarrels && strAvail.Contains("{Children Avail}"))
                    {
                        blnCheckUnderbarrels = false;
                        int intMaxChildAvail = 0;
                        intAvail += await UnderbarrelWeapons.SumAsync(async objUnderbarrel =>
                        {
                            if (objUnderbarrel.ParentID == InternalId)
                                return 0;
                            AvailabilityValue objLoopAvail = await objUnderbarrel.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                            if (objLoopAvail.AddToParent && objLoopAvail.Value > intMaxChildAvail)
                                intMaxChildAvail = objLoopAvail.Value;
                            if (objLoopAvail.Suffix == 'F')
                                chrLastAvailChar = 'F';
                            else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                                chrLastAvailChar = 'R';
                            return objLoopAvail.AddToParent ? objLoopAvail.Value : 0;
                        }, token).ConfigureAwait(false);

                        sbdAvail.Replace("{Children Avail}",
                                         intMaxChildAvail.ToString(GlobalSettings.InvariantCultureInfo));
                    }

                    AttributeSection objAttributeSection = await _objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false);
                    await (await objAttributeSection.GetAttributeListAsync(token).ConfigureAwait(false)).ForEachAsync(async objLoopAttribute =>
                    {
                        await sbdAvail.CheapReplaceAsync(strAvail, objLoopAttribute.Abbrev,
                                                         async () => (await objLoopAttribute.GetTotalValueAsync(token)
                                                             .ConfigureAwait(false)).ToString(
                                                             GlobalSettings.InvariantCultureInfo), token: token)
                                      .ConfigureAwait(false);
                        await sbdAvail.CheapReplaceAsync(strAvail, objLoopAttribute.Abbrev + "Base",
                                                         async () => (await objLoopAttribute.GetTotalBaseAsync(token)
                                                             .ConfigureAwait(false)).ToString(
                                                             GlobalSettings.InvariantCultureInfo), token: token)
                                      .ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                    await (await objAttributeSection.GetSpecialAttributeListAsync(token).ConfigureAwait(false)).ForEachAsync(async objLoopAttribute =>
                    {
                        await sbdAvail.CheapReplaceAsync(strAvail, objLoopAttribute.Abbrev,
                                                         async () => (await objLoopAttribute.GetTotalValueAsync(token)
                                                             .ConfigureAwait(false)).ToString(
                                                             GlobalSettings.InvariantCultureInfo), token: token)
                                      .ConfigureAwait(false);
                        await sbdAvail.CheapReplaceAsync(strAvail, objLoopAttribute.Abbrev + "Base",
                                                         async () => (await objLoopAttribute.GetTotalBaseAsync(token)
                                                             .ConfigureAwait(false)).ToString(
                                                             GlobalSettings.InvariantCultureInfo), token: token)
                                      .ConfigureAwait(false);
                    }, token).ConfigureAwait(false);

                    // Replace the division sign with "div" since we're using XPath.
                    sbdAvail.Replace("/", " div ");
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
            }

            if (blnCheckUnderbarrels)
            {
                intAvail += await UnderbarrelWeapons.SumAsync(async objUnderbarrel =>
                {
                    if (objUnderbarrel.ParentID == InternalId)
                        return 0;
                    AvailabilityValue objLoopAvail = await objUnderbarrel.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? objLoopAvail.Value : 0;
                }, token).ConfigureAwait(false);
            }

            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                intAvail += await WeaponAccessories.SumAsync(async objAccessory =>
                {
                    if (objAccessory.IncludedInWeapon || !objAccessory.Equipped) return 0;
                    AvailabilityValue objLoopAvail = await objAccessory.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? objLoopAvail.Value : 0;
                }, token).ConfigureAwait(false);
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInWeapon);
        }

        // Run through the Weapon Mods and see if anything changes the cost multiplier (Vintage mod).
        public int CostMultiplier
        {
            get
            {
                int intReturn = 1;
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.AccessoryCostMultiplier > 1)
                        intReturn = objAccessory.AccessoryCostMultiplier;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Mount slot that is used when mounting this weapon to another weapon.
        /// </summary>
        public string Mount => _strMount;

        /// <summary>
        /// Additional Mount slot that is used when mounting this weapon to another weapon.
        /// </summary>
        public string ExtraMount => _strExtraMount;

        /// <summary>
        /// Method used to fire the Weapon. If not vehicle mounted, always returns Skill.
        /// </summary>
        public FiringMode FireMode
        {
            get => ParentVehicle == null ? FiringMode.Skill : _eFiringMode;
            set => _eFiringMode = value;
        }

        public bool IsProgram
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis?.IsProgram == true;
            }
        }

        /// <summary>
        /// Device rating string for Cyberware. If it's empty, then GetBaseMatrixAttribute for Device Rating will fetch the grade's DR.
        /// </summary>
        public string DeviceRating
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.DeviceRating : _strDeviceRating;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.DeviceRating = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.Attack : _strAttack;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.Attack = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.Sleaze : _strSleaze;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.Sleaze = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.DataProcessing : _strDataProcessing;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.DataProcessing = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.Firewall : _strFirewall;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.Firewall = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.ModAttack : _strModAttack;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModAttack = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.ModSleaze : _strModSleaze;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModSleaze = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.ModDataProcessing : _strModDataProcessing;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModDataProcessing = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.ModFirewall : _strModFirewall;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModFirewall = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.AttributeArray : _strAttributeArray;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.AttributeArray = value;
                else
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.ModAttributeArray : _strModAttributeArray;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModAttributeArray = value;
                else
                    _strModAttributeArray = value;
            }
        }

        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        public string Overclocked
        {
            get
            {
                if (!_objCharacter.Overclocker)
                    return string.Empty;
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.Overclocked : _strOverclocked;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.Overclocked = value;
                else
                    _strOverclocked = value;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetOverclockedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!await _objCharacter.GetOverclockerAsync(token).ConfigureAwait(false))
                return string.Empty;
            IHasMatrixAttributes objThis = await GetMatrixAttributesOverrideAsync(token).ConfigureAwait(false);
            return objThis != null ? await objThis.GetOverclockedAsync(token).ConfigureAwait(false) : _strOverclocked;
        }

        /// <inheritdoc />
        public string CanFormPersona
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.CanFormPersona : string.Empty;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.CanFormPersona = value;
            }
        }

        /// <inheritdoc />
        public async Task<string> GetCanFormPersonaAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IHasMatrixAttributes objThis = await GetMatrixAttributesOverrideAsync(token).ConfigureAwait(false);
            return objThis != null ? await objThis.GetCanFormPersonaAsync(token).ConfigureAwait(false) : string.Empty;
        }

        /// <inheritdoc />
        public bool IsCommlink
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis?.IsCommlink == true;
            }
        }

        /// <inheritdoc />
        public async Task<bool> GetIsCommlinkAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IHasMatrixAttributes objThis = await GetMatrixAttributesOverrideAsync(token).ConfigureAwait(false);
            return objThis != null && await objThis.GetIsCommlinkAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// 0 for Cyberware.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis?.BonusMatrixBoxes ?? 0;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.BonusMatrixBoxes = value;
            }
        }

        public int TotalBonusMatrixBoxes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis?.TotalBonusMatrixBoxes ?? 0;
            }
        }

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.ProgramLimit : _strProgramLimit;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ProgramLimit = value;
                else
                    _strProgramLimit = value;
            }
        }

        /// <summary>
        /// Returns true if this is a cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis?.CanSwapAttributes ?? _blnCanSwapAttributes;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.CanSwapAttributes = value;
                else
                    _blnCanSwapAttributes = value;
            }
        }

        public IEnumerable<IHasMatrixAttributes> ChildrenWithMatrixAttributes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis != null ? objThis.ChildrenWithMatrixAttributes : Children;
            }
        }

        /// <summary>
        /// Base Physical Boxes. 12 for vehicles, 6 for Drones.
        /// </summary>
        public int BaseMatrixBoxes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis?.BaseMatrixBoxes ?? 8;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.MatrixCM;
                return BaseMatrixBoxes + (this.GetTotalMatrixAttribute("Device Rating") + 1) / 2 + TotalBonusMatrixBoxes;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes filled.
        /// </summary>
        public int MatrixCMFilled
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                return objThis?.MatrixCMFilled ?? _intMatrixCMFilled;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.MatrixCMFilled = value;
                else
                    _intMatrixCMFilled = value;
            }
        }

        public enum FiringMode
        {
            Skill,
            GunneryCommandDevice,
            RemoteOperated,
            DogBrain,
            ManualOperation,
            NumFiringModes // 🡐 This one should always be the last defined enum
        }

        #endregion Complex Properties

        #region Helper Methods

        /// <summary>
        /// Convert a string to a Firing Mode.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static FiringMode ConvertToFiringMode(string strValue)
        {
            switch (strValue)
            {
                case "DogBrain":
                    return FiringMode.DogBrain;

                case "GunneryCommandDevice":
                    return FiringMode.GunneryCommandDevice;

                case "RemoteOperated":
                    return FiringMode.RemoteOperated;

                case "ManualOperation":
                    return FiringMode.ManualOperation;

                default:
                    return FiringMode.Skill;
            }
        }

        #endregion Helper Methods

        #region Methods

        /// <summary>
        /// Toggle the Wireless Bonus for this weapon accessory.
        /// </summary>
        public void RefreshWirelessBonuses()
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped && Parent?.WirelessOn != false)
                {
                    if (WirelessBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter,
                            _objCharacter.Improvements.Where(x =>
                                x.ImproveSource == Improvement.ImprovementSource.Weapon &&
                                x.SourceName == InternalId));
                    }

                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, InternalId + "Wireless", WirelessBonus, 1, CurrentDisplayNameShort);
                }
                else
                {
                    if (WirelessBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode")?.Value == "replace")
                    {
                        ImprovementManager.EnableImprovements(_objCharacter,
                            _objCharacter.Improvements.Where(x =>
                                x.ImproveSource == Improvement.ImprovementSource.Weapon &&
                                x.SourceName == InternalId));
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    ImprovementManager.RemoveImprovements(_objCharacter,
                        _objCharacter.Improvements.Where(x =>
                            x.ImproveSource == Improvement.ImprovementSource.Weapon &&
                            x.SourceName == strSourceNameToRemove).ToList());
                }
            }

            foreach (Weapon objWeapon in Children)
                objWeapon.RefreshWirelessBonuses();
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
                objAccessory.RefreshWirelessBonuses();
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this weapon accessory.
        /// </summary>
        public async ValueTask RefreshWirelessBonusesAsync(CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped && Parent?.WirelessOn != false)
                {
                    if (WirelessBonus != null && WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
                    {
                        await ImprovementManager.DisableImprovementsAsync(_objCharacter,
                                                                          await _objCharacter.Improvements.ToListAsync(
                                                                              x => x.ImproveSource
                                                                                  == Improvement.ImprovementSource.Weapon
                                                                                  && x.SourceName == InternalId, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    await ImprovementManager.CreateImprovementsAsync(_objCharacter,
                                                                     Improvement.ImprovementSource.ArmorMod,
                                                                     InternalId + "Wireless", WirelessBonus, 1,
                                                                     await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false),
                                                                     token: token).ConfigureAwait(false);
                }
                else
                {
                    if (WirelessBonus != null && WirelessBonus.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
                    {
                        await ImprovementManager.EnableImprovementsAsync(_objCharacter,
                                                                         await _objCharacter.Improvements.ToListAsync(
                                                                             x => x.ImproveSource
                                                                                 == Improvement.ImprovementSource.Weapon
                                                                                 && x.SourceName == InternalId, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                    }

                    string strSourceNameToRemove = InternalId + "Wireless";
                    await ImprovementManager.RemoveImprovementsAsync(_objCharacter,
                                                                     await _objCharacter.Improvements.ToListAsync(
                                                                         x => x.ImproveSource
                                                                              == Improvement.ImprovementSource.Weapon
                                                                              && x.SourceName == strSourceNameToRemove,
                                                                         token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
                }
            }

            foreach (Weapon objWeapon in Children)
                await objWeapon.RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
                await objAccessory.RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteWeapon(bool blnDoRemoval = true)
        {
            // Remove the Weapon from the character.
            if (blnDoRemoval)
            {
                if (Parent != null)
                    Parent.Children.Remove(this);
                else if (ParentVehicle != null)
                {
                    if (ParentVehicleMod != null)
                        ParentVehicleMod.Weapons.Remove(this);
                    else if (ParentMount != null)
                        ParentMount.Weapons.Remove(this);
                    else
                        ParentVehicle.Weapons.Remove(this);
                }
                else
                    _objCharacter.Weapons.Remove(this);
            }

            // unload any clips before we die
            UnloadAll();

            // Remove any children the Gear may have.
            decimal decReturn = Children.Sum(x => x.DeleteWeapon(false))
                                + WeaponAccessories.Sum(x => x.DeleteWeaponAccessory(false));

            foreach (Weapon objDeleteWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId).ToList())
            {
                decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
            }

            decReturn += _objCharacter.Vehicles.Sum(objVehicle =>
            {
                decimal decInnerReturn = 0;
                foreach (Weapon objDeleteWeapon in objVehicle.Weapons
                                                             .DeepWhere(x => x.Children,
                                                                        x => x.ParentID == InternalId).ToList())
                {
                    decInnerReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                }

                decInnerReturn += objVehicle.Mods.Sum(objMod =>
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

                decInnerReturn += objVehicle.WeaponMounts.Sum(objMount =>
                {
                    decimal decInnerReturn2 = 0;
                    foreach (Weapon objDeleteWeapon in objMount.Weapons
                                                               .DeepWhere(x => x.Children,
                                                                          x => x.ParentID == InternalId).ToList())
                    {
                        decInnerReturn2 += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                    }

                    decInnerReturn2 += objMount.Mods.Sum(objMod =>
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

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Weapon, InternalId + "Wireless");

            DisposeSelf();

            return decReturn;
        }

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public async ValueTask<decimal> DeleteWeaponAsync(bool blnDoRemoval = true, CancellationToken token = default)
        {
            // Remove the Weapon from the character.
            if (blnDoRemoval)
            {
                if (Parent != null)
                    await Parent.Children.RemoveAsync(this, token).ConfigureAwait(false);
                else if (ParentVehicle != null)
                {
                    if (ParentVehicleMod != null)
                        await ParentVehicleMod.Weapons.RemoveAsync(this, token).ConfigureAwait(false);
                    else if (ParentMount != null)
                        await ParentMount.Weapons.RemoveAsync(this, token).ConfigureAwait(false);
                    else
                        await ParentVehicle.Weapons.RemoveAsync(this, token).ConfigureAwait(false);
                }
                else
                    await _objCharacter.Weapons.RemoveAsync(this, token).ConfigureAwait(false);
            }

            // unload any clips before we die
            await UnloadAllAsync(token).ConfigureAwait(false);

            // Remove any children the Gear may have.
            decimal decReturn = await Children.SumAsync(x => x.DeleteWeaponAsync(false, token).AsTask(), token: token)
                                              .ConfigureAwait(false)
                                + await WeaponAccessories
                                        .SumAsync(x => x.DeleteWeaponAccessoryAsync(false, token).AsTask(), token)
                                        .ConfigureAwait(false);

            foreach (Weapon objDeleteWeapon in await _objCharacter.Weapons
                                                                  .DeepWhereAsync(
                                                                      x => x.Children,
                                                                      x => x.ParentID == InternalId, token)
                                                                  .ConfigureAwait(false))
            {
                decReturn += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                             + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
            }

            decReturn += await _objCharacter.Vehicles.SumAsync(async objVehicle =>
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

                decInner += await objVehicle.Mods.SumAsync(async objMod =>
                {
                    decimal decInner2 = 0;
                    foreach (Weapon objDeleteWeapon in await objMod.Weapons
                                                                   .DeepWhereAsync(
                                                                       x => x.Children,
                                                                       x => x.ParentID == InternalId, token)
                                                                   .ConfigureAwait(false))
                    {
                        decInner2 += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                     + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                    }

                    return decInner2;
                }, token).ConfigureAwait(false);

                decInner += await objVehicle.WeaponMounts.SumAsync(async objMount =>
                {
                    decimal decInner2 = 0;
                    foreach (Weapon objDeleteWeapon in await objMount.Weapons
                                                                     .DeepWhereAsync(
                                                                         x => x.Children,
                                                                         x => x.ParentID == InternalId, token)
                                                                     .ConfigureAwait(false))
                    {
                        decInner2 += await objDeleteWeapon.GetTotalCostAsync(token).ConfigureAwait(false)
                                     + await objDeleteWeapon.DeleteWeaponAsync(token: token).ConfigureAwait(false);
                    }

                    decInner2 += await objMount.Mods.SumAsync(async objMod =>
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

            decReturn += await ImprovementManager
                               .RemoveImprovementsAsync(_objCharacter, Improvement.ImprovementSource.Weapon,
                                                        InternalId + "Wireless", token).ConfigureAwait(false);

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
        public async ValueTask<int> CheckRestrictedGear(IDictionary<int, int> dicRestrictedGearLimits,
                                                        StringBuilder sbdAvailItems, StringBuilder sbdRestrictedItems,
                                                        CancellationToken token = default)
        {
            int intRestrictedCount = 0;
            if (!IncludedInWeapon && string.IsNullOrEmpty(ParentID))
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
                            strNameToUse += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + '('
                                + await Parent.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + ')';

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

            intRestrictedCount += await UnderbarrelWeapons.SumAsync(async objChild =>
                                                                        await objChild.CheckRestrictedGear(
                                                                                dicRestrictedGearLimits, sbdAvailItems,
                                                                                sbdRestrictedItems, token)
                                                                            .ConfigureAwait(false), token)
                                                          .ConfigureAwait(false)
                                  + await WeaponAccessories.SumAsync(async objChild =>
                                                                         await objChild.CheckRestrictedGear(
                                                                                 dicRestrictedGearLimits, sbdAvailItems,
                                                                                 sbdRestrictedItems, token)
                                                                             .ConfigureAwait(false), token)
                                                           .ConfigureAwait(false);

            return intRestrictedCount;
        }

        public async ValueTask Reload(IAsyncCollection<Gear> lstGears, TreeView treGearView,
                                      CancellationToken token = default)
        {
            List<string> lstCount = new List<string>(1);
            string ammoString
                = await CalculatedAmmoAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token)
                    .ConfigureAwait(false);
            if (!RequireAmmo)
            {
                // For weapons that have ammo capacities but no requirement for ammo, these are charges
                // We treat this function differently for them, letting the character reload as many charges as they see fit

                Clip objInternalClip = GetClip(ActiveAmmoSlot);
                if (objInternalClip == null)
                {
                    await RecreateInternalClipAsync(token).ConfigureAwait(false);
                    objInternalClip = GetClip(ActiveAmmoSlot);
                    if (objInternalClip == null)
                        throw new InvalidOperationException(nameof(objInternalClip));
                }

                int intCurrentAmmoCount = objInternalClip.Ammo;

                // Determine which loading methods are available to the Weapon.
                if (ammoString.IndexOfAny('x', '+') != -1 ||
                    ammoString.Contains(" or ", StringComparison.OrdinalIgnoreCase) ||
                    ammoString.Contains("Special", StringComparison.OrdinalIgnoreCase) ||
                    ammoString.Contains("External Source", StringComparison.OrdinalIgnoreCase))
                {
                    string strWeaponAmmo = ammoString.FastEscape("External Source", StringComparison.OrdinalIgnoreCase);
                    strWeaponAmmo = strWeaponAmmo.ToLowerInvariant();
                    // Get rid of or belt, and + energy.
                    strWeaponAmmo = strWeaponAmmo.FastEscapeOnceFromEnd(" + energy")
                                                 .Replace(" or belt", " or 250(belt)");

                    foreach (string strAmmo in
                             strWeaponAmmo.SplitNoAlloc(" or ", StringSplitOptions.RemoveEmptyEntries))
                    {
                        lstCount.Add(AmmoCapacity(strAmmo));
                    }
                }
                else
                {
                    // Nothing weird in the ammo string, so just use the number given.
                    string strAmmo = ammoString;
                    int intPos = strAmmo.IndexOf('(');
                    if (intPos != -1)
                        strAmmo = strAmmo.Substring(0, intPos);
                    lstCount.Add(strAmmo);
                }

                int intMaxAmmoCount = 0;

                foreach (string strAmmo in lstCount)
                {
                    if (int.TryParse(strAmmo, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intMaxAmmoCount))
                    {
                        break;
                    }
                }

                if (intMaxAmmoCount <= intCurrentAmmoCount)
                    return;

                string strDescription = string.Format(
                    await LanguageManager.GetStringAsync("Message_SelectNumberOfCharges", token: token)
                                         .ConfigureAwait(false),
                    await GetCurrentDisplayNameAsync(token).ConfigureAwait(false));
                using (ThreadSafeForm<SelectNumber> frmNewAmmoCount = await ThreadSafeForm<SelectNumber>.GetAsync(
                           () => new SelectNumber(0)
                           {
                               AllowCancel = true,
                               Maximum = intMaxAmmoCount,
                               Minimum = intCurrentAmmoCount,
                               Description = strDescription
                           }, token).ConfigureAwait(false))
                {
                    if (await frmNewAmmoCount.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false)
                        != DialogResult.OK)
                        return;

                    objInternalClip.Ammo = frmNewAmmoCount.MyForm.SelectedValue.ToInt32();
                }

                return;
            }

            bool blnExternalSource = false;
            List<Gear> lstAmmo = new List<Gear>(1);
            // Determine which loading methods are available to the Weapon.
            if (ammoString.IndexOfAny('x', '+') != -1 ||
                ammoString.Contains(" or ", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("Special", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("External Source", StringComparison.OrdinalIgnoreCase))
            {
                string strWeaponAmmo = ammoString;
                if (strWeaponAmmo.Contains("External Source", StringComparison.OrdinalIgnoreCase))
                {
                    blnExternalSource = true;
                    strWeaponAmmo = strWeaponAmmo.FastEscape("External Source", StringComparison.OrdinalIgnoreCase);
                }

                strWeaponAmmo = strWeaponAmmo.ToLowerInvariant();
                // Get rid of or belt, and + energy.
                strWeaponAmmo = strWeaponAmmo.FastEscapeOnceFromEnd(" + energy")
                                             .Replace(" or belt", " or 250(belt)");

                foreach (string strAmmo in
                         strWeaponAmmo.SplitNoAlloc(" or ", StringSplitOptions.RemoveEmptyEntries))
                {
                    lstCount.Add(AmmoCapacity(strAmmo));
                }
            }
            else
            {
                // Nothing weird in the ammo string, so just use the number given.
                string strAmmo = ammoString;
                int intPos = strAmmo.IndexOf('(');
                if (intPos != -1)
                    strAmmo = strAmmo.Substring(0, intPos);
                lstCount.Add(strAmmo);
            }

            Gear objExternalSource = null;
            if (blnExternalSource)
            {
                lstCount.Add(await LanguageManager.GetStringAsync("String_ExternalSource", token: token)
                                                  .ConfigureAwait(false));
                objExternalSource = new Gear(_objCharacter)
                {
                    Name = await LanguageManager.GetStringAsync("String_ExternalSource", token: token)
                                                .ConfigureAwait(false),
                    SourceID = Guid.Empty
                };
            }

            if (RequireAmmo)
            {
                lstAmmo.AddRange(GetAmmoReloadable(lstGears));
                // Make sure the character has some form of Ammunition for this Weapon.
                if (lstAmmo.Count == 0)
                {
                    Program.ShowScrollableMessageBox(string.Format(GlobalSettings.CultureInfo,
                                                                   await LanguageManager
                                                                         .GetStringAsync("Message_OutOfAmmoType", token: token)
                                                                         .ConfigureAwait(false),
                                                                   await GetCurrentDisplayNameAsync(token).ConfigureAwait(false)),
                                                     await LanguageManager.GetStringAsync("Message_OutOfAmmo", token: token)
                                                                          .ConfigureAwait(false),
                                                     icon: MessageBoxIcon.Warning);
                    return;
                }
            }

            if (objExternalSource != null)
                lstAmmo.Add(objExternalSource);

            // Show the Ammunition Selection window.
            using (ThreadSafeForm<ReloadWeapon> frmReloadWeapon = await ThreadSafeForm<ReloadWeapon>.GetAsync(
                       () => new ReloadWeapon(this)
                       {
                           Ammo = lstAmmo,
                           Count = lstCount
                       }, token).ConfigureAwait(false))
            {
                if (await frmReloadWeapon.ShowDialogSafeAsync(_objCharacter, token).ConfigureAwait(false)
                    != DialogResult.OK)
                    return;

                Gear objCurrentlyLoadedAmmo = AmmoLoaded;
                Gear objSelectedAmmo;
                decimal decQty = frmReloadWeapon.MyForm.SelectedCount;
                // If an External Source is not being used, consume ammo.
                if (frmReloadWeapon.MyForm.SelectedAmmo != objExternalSource?.InternalId)
                {
                    objSelectedAmmo = lstGears.DeepFindById(frmReloadWeapon.MyForm.SelectedAmmo);

                    // If the Ammo is coming from a Spare Clip, reduce the container quantity instead of the plugin quantity.
                    if (objSelectedAmmo.Parent is Gear objParent &&
                        (objParent.Name.StartsWith("Spare Clip", StringComparison.Ordinal)
                         || objParent.Name.StartsWith("Speed Loader", StringComparison.Ordinal)))
                    {
                        if (objParent.Quantity > 1)
                        {
                            // Duplicate the clip into a new entry where we can directly deduct from the quantity as we fire
                            Gear objDuplicatedParent = new Gear(_objCharacter);
                            objDuplicatedParent.Copy(objParent);
                            objDuplicatedParent.Quantity = 1;
                            await lstGears.AddAsync(objDuplicatedParent, token).ConfigureAwait(false);
                            --objParent.Quantity;
                            Gear objNewSelectedAmmo
                                = objDuplicatedParent.Children.DeepFindById(frmReloadWeapon.MyForm.SelectedAmmo);
                            if (objNewSelectedAmmo == null)
                            {
                                objNewSelectedAmmo = new Gear(_objCharacter);
                                objNewSelectedAmmo.Copy(objSelectedAmmo);
                                await objDuplicatedParent.Children.AddAsync(objNewSelectedAmmo, token)
                                                         .ConfigureAwait(false);
                            }

                            objSelectedAmmo = objNewSelectedAmmo;
                        }

                        string strParentText = await objParent.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        await treGearView.DoThreadSafeAsync(x =>
                        {
                            TreeNode objNode = x.FindNode(objParent.InternalId);
                            if (objNode != null)
                                objNode.Text = strParentText;
                        }, token: token).ConfigureAwait(false);
                    }

                    if (objSelectedAmmo.IsIdenticalToOtherGear(objCurrentlyLoadedAmmo))
                    {
                        // Just top up the currently loaded ammo
                        decimal decTopUp = decQty - objCurrentlyLoadedAmmo.Quantity;
                        if (decTopUp > objSelectedAmmo.Quantity)
                        {
                            // We need more ammo for a full top-up than the quantity of gear, so just merge the gears and delete the old gear.
                            objCurrentlyLoadedAmmo.Quantity += objSelectedAmmo.Quantity;
                            await objSelectedAmmo.DeleteGearAsync(token: token).ConfigureAwait(false);
                            GetClip(_intActiveAmmoSlot).Ammo
                                = objCurrentlyLoadedAmmo.Quantity
                                                        .ToInt32(); // Bypass AmmoRemaining so as not to alter the gear quantity
                        }
                        else
                        {
                            objCurrentlyLoadedAmmo.Quantity = decQty;
                            objSelectedAmmo.Quantity -= decTopUp;
                            string strCurrentlyLoadedText = await objCurrentlyLoadedAmmo
                                                                  .GetCurrentDisplayNameAsync(token)
                                                                  .ConfigureAwait(false);
                            string strSelectedText = await objSelectedAmmo
                                                           .GetCurrentDisplayNameAsync(token)
                                                           .ConfigureAwait(false);
                            await treGearView.DoThreadSafeAsync(x =>
                            {
                                // Refresh the Gear tree.
                                TreeNode objSelectedNode = x.FindNode(objCurrentlyLoadedAmmo.InternalId);
                                if (objSelectedNode != null)
                                    objSelectedNode.Text = strCurrentlyLoadedText;
                                objSelectedNode = x.FindNode(objSelectedAmmo.InternalId);
                                if (objSelectedNode != null)
                                    objSelectedNode.Text = strSelectedText;
                            }, token: token).ConfigureAwait(false);
                            GetClip(_intActiveAmmoSlot).Ammo
                                = decQty.ToInt32(); // Bypass AmmoRemaining so as not to alter the gear quantity
                        }

                        return;
                    }

                    if (objSelectedAmmo.Quantity > decQty)
                    {
                        // Duplicate the ammo into a new entry where we can directly deduct from the quantity as we fire
                        Gear objNewSelectedAmmo = new Gear(_objCharacter);
                        objNewSelectedAmmo.Copy(objSelectedAmmo);
                        objNewSelectedAmmo.Quantity = decQty.ToInt32();
                        await lstGears.AddAsync(objNewSelectedAmmo, token).ConfigureAwait(false);
                        objSelectedAmmo.Quantity -= decQty.ToInt32();
                        string strId2 = objSelectedAmmo.InternalId;
                        string strText2 = await objSelectedAmmo.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                        await treGearView.DoThreadSafeAsync(x =>
                        {
                            // Refresh the Gear tree.
                            TreeNode objSelectedNode = x.FindNode(strId2);
                            if (objSelectedNode != null)
                                objSelectedNode.Text = strText2;
                        }, token: token).ConfigureAwait(false);
                        objSelectedAmmo = objNewSelectedAmmo;
                    }
                    else if (decQty > objSelectedAmmo.Quantity)
                    {
                        decQty = objSelectedAmmo.Quantity;
                    }

                    string strId = objSelectedAmmo.InternalId;
                    string strText = await objSelectedAmmo.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                    await treGearView.DoThreadSafeAsync(x =>
                    {
                        // Refresh the Gear tree.
                        TreeNode objSelectedNode = x.FindNode(strId);
                        if (objSelectedNode != null)
                            objSelectedNode.Text = strText;
                    }, token: token).ConfigureAwait(false);
                }
                else
                {
                    objSelectedAmmo = objExternalSource;
                }

                AmmoLoaded = objSelectedAmmo;
                if (objCurrentlyLoadedAmmo != objSelectedAmmo && objCurrentlyLoadedAmmo != null)
                {
                    string strId = objCurrentlyLoadedAmmo.InternalId;
                    string strText = await objCurrentlyLoadedAmmo.GetCurrentDisplayNameAsync(token)
                                                                 .ConfigureAwait(false);
                    await treGearView.DoThreadSafeAsync(x =>
                    {
                        // Refresh the Gear tree.
                        TreeNode objSelectedNode = x.FindNode(strId);
                        if (objSelectedNode != null)
                            objSelectedNode.Text = strText;
                    }, token: token).ConfigureAwait(false);
                }

                GetClip(_intActiveAmmoSlot).Ammo
                    = decQty.ToInt32(); // Bypass AmmoRemaining so as not to alter the gear quantity
            }
        }

        public async Task Unload(IAsyncCollection<Gear> lstGears, TreeView treGearView, CancellationToken token = default)
        {
            Clip objClip = GetClip(ActiveAmmoSlot);
            Gear objAmmo = await UnloadGearAsync(lstGears, objClip, token).ConfigureAwait(false);
            if (objAmmo == null)
                return;
            string strText = await objAmmo.GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
            await treGearView.DoThreadSafeAsync(x =>
            {
                // Refresh the Gear tree.
                TreeNode objSelectedNode = x.FindNode(objAmmo.InternalId);
                if (objSelectedNode != null)
                    objSelectedNode.Text = strText;
            }, token: token).ConfigureAwait(false);
        }

        private static Gear UnloadGear(IAsyncCollection<Gear> lstGears, Clip clipToUnload)
        {
            Gear objAmmo = clipToUnload.AmmoGear;
            if (objAmmo == null)
                return null;
            clipToUnload.AmmoGear = null;
            Gear objMergeGear = lstGears.FirstOrDefault(x =>
                x.InternalId != objAmmo.InternalId && x.IsIdenticalToOtherGear(objAmmo, true));
            if (objMergeGear != null)
            {
                objMergeGear.Quantity += objAmmo.Quantity;
                objAmmo.DeleteGear();
                return objMergeGear;
            }
            return objAmmo;
        }

        private static async ValueTask<Gear> UnloadGearAsync(IAsyncCollection<Gear> lstGears, Clip clipToUnload,
                                                             CancellationToken token = default)
        {
            Gear objAmmo = clipToUnload.AmmoGear;
            if (objAmmo == null)
                return null;
            clipToUnload.AmmoGear = null;
            Gear objMergeGear = await lstGears
                                      .FirstOrDefaultAsync(
                                          x => x.InternalId != objAmmo.InternalId
                                               && x.IsIdenticalToOtherGear(objAmmo, true), token).ConfigureAwait(false);
            if (objMergeGear != null)
            {
                objMergeGear.Quantity += objAmmo.Quantity;
                await objAmmo.DeleteGearAsync(token: token).ConfigureAwait(false);
                return objMergeGear;
            }

            return objAmmo;
        }

        public IEnumerable<Gear> GetAmmoReloadable(IEnumerable<Gear> lstGears)
        {
            if (!RequireAmmo)
                yield break;
            // Find all of the Ammo for the current Weapon that the character is carrying.
            if (AmmoCategory == "Gear")
            {
                foreach (Gear objGear in lstGears.DeepWhere(x => x.Children, x =>
                    x.Quantity > 0
                    && x.LoadedIntoClip == null
                    && Name == x.Name
                    && (string.IsNullOrEmpty(x.Extra) || x.Extra == AmmoCategory)))
                {
                    yield return objGear;
                }
            }
            else if (Skill?.DictionaryKey == "Throwing Weapons")
            {
                if (Damage.Contains("(f)"))
                {
                    foreach (Gear objGear in lstGears.DeepWhere(x => x.Children, x =>
                                                                    x.Quantity > 0
                                                                    && x.LoadedIntoClip == null
                                                                    && x.IsFlechetteAmmo
                                                                    && x.AmmoForWeaponType.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Contains(WeaponType)
                                                                    && (string.IsNullOrEmpty(x.Extra)
                                                                        || x.Extra == AmmoCategory
                                                                        || x.Name == Name)))
                    {
                        yield return objGear;
                    }
                }
                else
                {
                    foreach (Gear objGear in lstGears.DeepWhere(x => x.Children, x =>
                                                                    x.Quantity > 0
                                                                    && x.LoadedIntoClip == null
                                                                    && x.AmmoForWeaponType.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Contains(WeaponType)
                                                                    && (string.IsNullOrEmpty(x.Extra)
                                                                        || x.Extra == AmmoCategory
                                                                        || x.Name == Name)))
                    {
                        yield return objGear;
                    }
                }
            }
            else if (Damage.Contains("(f)"))
            {
                foreach (Gear objGear in lstGears.DeepWhere(x => x.Children, x =>
                    x.Quantity > 0
                    && x.LoadedIntoClip == null
                    && x.IsFlechetteAmmo
                    && x.AmmoForWeaponType.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Contains(WeaponType)
                    && (string.IsNullOrEmpty(x.Extra)
                        || x.Extra == AmmoCategory)))
                {
                    yield return objGear;
                }
            }
            else
            {
                foreach (Gear objGear in lstGears.DeepWhere(x => x.Children, x =>
                    x.Quantity > 0
                    && x.LoadedIntoClip == null
                    && x.AmmoForWeaponType.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).Contains(WeaponType)
                    && (string.IsNullOrEmpty(x.Extra)
                        || x.Extra == AmmoCategory)))
                {
                    yield return objGear;
                }
            }
        }

        #region UI Methods

        /// <summary>
        /// Add a Weapon to the TreeView.
        /// </summary>
        /// <param name="cmsWeapon">ContextMenuStrip for the Weapon Node.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip for Vehicle Accessory Nodes.</param>
        /// <param name="cmsWeaponAccessoryGear">ContextMenuStrip for Vehicle Weapon Accessory Gear Nodes.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear)
        {
            if ((Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)) && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsWeapon,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // Add Underbarrel Weapons.
            foreach (Weapon objUnderbarrelWeapon in UnderbarrelWeapons)
            {
                TreeNode objLoopNode = objUnderbarrelWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }
            // Add attached Weapon Accessories.
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                TreeNode objLoopNode = objAccessory.CreateTreeNode(cmsWeaponAccessory, cmsWeaponAccessoryGear);
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
                    return Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        public void SetupChildrenWeaponsCollectionChanged(bool blnAdd, TreeView treWeapons, ContextMenuStrip cmsWeapon = null, ContextMenuStrip cmsWeaponAccessory = null, ContextMenuStrip cmsWeaponAccessoryGear = null, NotifyCollectionChangedEventHandler funcMakeDirty = null)
        {
            if (blnAdd)
            {
                async Task FuncUnderbarrelWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken token = default) =>
                    await this.RefreshChildrenWeapons(treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear,
                                                      null, y, funcMakeDirty, token: token).ConfigureAwait(false);
                async Task FuncWeaponAccessoriesToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken token = default) =>
                    await this.RefreshWeaponAccessories(treWeapons, cmsWeaponAccessory, cmsWeaponAccessoryGear,
                                                        () => UnderbarrelWeapons.Count, y, funcMakeDirty, token: token).ConfigureAwait(false);

                UnderbarrelWeapons.AddTaggedCollectionChanged(treWeapons, FuncUnderbarrelWeaponsToAdd);
                WeaponAccessories.AddTaggedCollectionChanged(treWeapons, FuncWeaponAccessoriesToAdd);
                if (funcMakeDirty != null)
                {
                    UnderbarrelWeapons.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                    WeaponAccessories.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                }
                foreach (Weapon objChild in UnderbarrelWeapons)
                {
                    objChild.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, funcMakeDirty);
                }

                foreach (WeaponAccessory objChild in WeaponAccessories)
                {
                    async Task FuncWeaponAccessoryGearToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken token = default) =>
                        await objChild.RefreshChildrenGears(treWeapons, cmsWeaponAccessoryGear, null, null, y, funcMakeDirty, token: token).ConfigureAwait(false);

                    objChild.GearChildren.AddTaggedCollectionChanged(treWeapons, FuncWeaponAccessoryGearToAdd);
                    if (funcMakeDirty != null)
                        objChild.GearChildren.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                    foreach (Gear objGear in objChild.GearChildren)
                        objGear.SetupChildrenGearsCollectionChanged(true, treWeapons, cmsWeaponAccessoryGear, null, funcMakeDirty);
                }
            }
            else
            {
                UnderbarrelWeapons.RemoveTaggedAsyncCollectionChanged(treWeapons);
                WeaponAccessories.RemoveTaggedAsyncCollectionChanged(treWeapons);
                foreach (Weapon objChild in UnderbarrelWeapons)
                {
                    objChild.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                }
                foreach (WeaponAccessory objChild in WeaponAccessories)
                {
                    objChild.GearChildren.RemoveTaggedAsyncCollectionChanged(treWeapons);
                    foreach (Gear objGear in objChild.GearChildren)
                        objGear.SetupChildrenGearsCollectionChanged(false, treWeapons);
                }
            }
        }

        #endregion UI Methods

        #region Hero Lab Importing

        public bool ImportHeroLabWeapon(XPathNavigator xmlWeaponImportNode, IList<Weapon> lstWeapons)
        {
            if (xmlWeaponImportNode == null)
                return false;
            string strOriginalName = xmlWeaponImportNode.SelectSingleNodeAndCacheExpression("@name")?.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(strOriginalName))
            {
                XmlDocument xmlWeaponDocument = _objCharacter.LoadData("weapons.xml");
                XmlNode xmlWeaponDataNode = xmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", strOriginalName);
                if (xmlWeaponDataNode == null)
                {
                    if (strOriginalName.Contains(':'))
                    {
                        string strName = strOriginalName.SplitNoAlloc(':', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? string.Empty;
                        xmlWeaponDataNode = xmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", strName);
                    }
                    if (xmlWeaponDataNode == null && strOriginalName.Contains(','))
                    {
                        string strName = strOriginalName.SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim() ?? string.Empty;
                        xmlWeaponDataNode = xmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", strName);
                    }
                }
                if (xmlWeaponDataNode != null)
                {
                    Create(xmlWeaponDataNode, lstWeapons, true, true, true);
                    if (Cost.Contains("Variable"))
                    {
                        Cost = xmlWeaponImportNode.SelectSingleNodeAndCacheExpression("gearcost/@value")?.Value;
                    }
                    Notes = xmlWeaponImportNode.SelectSingleNodeAndCacheExpression("description")?.Value;

                    ProcessHeroLabWeaponPlugins(xmlWeaponImportNode, lstWeapons);

                    return true;
                }
            }
            return false;
        }

        public void ProcessHeroLabWeaponPlugins(XPathNavigator xmlWeaponImportNode, IList<Weapon> lstWeapons)
        {
            if (xmlWeaponImportNode == null)
                return;
            XmlNode xmlWeaponDataNode = this.GetNode();
            foreach (string strName in Character.HeroLabPluginNodeNames)
            {
                foreach (XPathNavigator xmlWeaponAccessoryToImport in xmlWeaponImportNode.Select(strName + "/item[@useradded != \"no\"]"))
                {
                    Weapon objUnderbarrel = new Weapon(_objCharacter);
                    if (objUnderbarrel.ImportHeroLabWeapon(xmlWeaponAccessoryToImport, lstWeapons))
                    {
                        objUnderbarrel.Parent = this;
                        UnderbarrelWeapons.Add(objUnderbarrel);
                    }
                    else
                    {
                        string strWeaponAccessoryName = xmlWeaponImportNode.SelectSingleNodeAndCacheExpression("@name")?.Value;
                        if (!string.IsNullOrEmpty(strWeaponAccessoryName))
                        {
                            XmlDocument xmlWeaponDocument = _objCharacter.LoadData("weapons.xml");
                            XmlNode xmlWeaponAccessoryData = null;
                            foreach (XmlNode xmlLoopNode in xmlWeaponDocument.SelectNodes("chummer/accessories/accessory[contains(name, " + strWeaponAccessoryName.CleanXPath() + ")]"))
                            {
                                XPathNavigator xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/weapondetails");
                                if (xmlTestNode != null && xmlWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    // Assumes topmost parent is an AND node
                                    continue;
                                }
                                xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/weapondetails");
                                if (xmlTestNode != null && !xmlWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    // Assumes topmost parent is an AND node
                                    continue;
                                }

                                xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("forbidden/oneof");
                                if (xmlTestNode != null)
                                {
                                    //Add to set for O(N log M) runtime instead of O(N * M)

                                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                               out HashSet<string> setForbiddenAccessory))
                                    {
                                        foreach (XPathNavigator node in xmlTestNode.Select("accessory"))
                                        {
                                            setForbiddenAccessory.Add(node.Value);
                                        }

                                        if (WeaponAccessories.Any(objAccessory =>
                                                                      setForbiddenAccessory
                                                                          .Contains(objAccessory.Name)))
                                        {
                                            continue;
                                        }
                                    }
                                }

                                xmlTestNode = xmlLoopNode.SelectSingleNodeAndCacheExpressionAsNavigator("required/oneof");
                                if (xmlTestNode != null)
                                {
                                    //Add to set for O(N log M) runtime instead of O(N * M)

                                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                               out HashSet<string> setRequiredAccessory))
                                    {
                                        foreach (XPathNavigator node in xmlTestNode.Select("accessory"))
                                        {
                                            setRequiredAccessory.Add(node.Value);
                                        }

                                        if (!WeaponAccessories.Any(objAccessory =>
                                                                       setRequiredAccessory
                                                                           .Contains(objAccessory.Name)))
                                        {
                                            continue;
                                        }
                                    }
                                }
                                xmlWeaponAccessoryData = xmlLoopNode;
                                break;
                            }
                            if (xmlWeaponAccessoryData != null)
                            {
                                WeaponAccessory objWeaponAccessory = new WeaponAccessory(_objCharacter);
                                string strMainMount = xmlWeaponAccessoryData["mount"]?.InnerText.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries)
                                    .FirstOrDefault() ?? string.Empty;
                                string strExtraMount = xmlWeaponAccessoryData["extramount"]?.InnerText.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries)
                                    .FirstOrDefault(x => x != strMainMount) ?? string.Empty;

                                objWeaponAccessory.Create(xmlWeaponAccessoryData, new Tuple<string, string>(strMainMount, strExtraMount), xmlWeaponAccessoryToImport.SelectSingleNodeAndCacheExpression("@rating")?.ValueAsInt ?? 0);
                                objWeaponAccessory.Notes = xmlWeaponAccessoryToImport.SelectSingleNodeAndCacheExpression("description")?.Value;
                                objWeaponAccessory.Parent = this;
                                WeaponAccessories.Add(objWeaponAccessory);

                                foreach (string strPluginName in Character.HeroLabPluginNodeNames)
                                {
                                    foreach (XPathNavigator xmlPluginToAdd in xmlWeaponAccessoryToImport.Select(strPluginName + "/item[@useradded != \"no\"]"))
                                    {
                                        Gear objPlugin = new Gear(_objCharacter);
                                        if (objPlugin.ImportHeroLabGear(xmlPluginToAdd, xmlWeaponAccessoryData, lstWeapons))
                                            objWeaponAccessory.GearChildren.Add(objPlugin);
                                    }
                                    foreach (XPathNavigator xmlPluginToAdd in xmlWeaponAccessoryToImport.Select(strPluginName + "/item[@useradded = \"no\"]"))
                                    {
                                        string strGearName = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("@name")?.Value;
                                        if (!string.IsNullOrEmpty(strGearName))
                                        {
                                            Gear objPlugin = objWeaponAccessory.GearChildren.FirstOrDefault(x => x.IncludedInParent && !string.IsNullOrEmpty(x.Name) && (x.Name.Contains(strGearName) || strGearName.Contains(x.Name)));
                                            if (objPlugin != null)
                                            {
                                                objPlugin.Quantity = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("@quantity")?.ValueAsInt ?? 1;
                                                objPlugin.Notes = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("description")?.Value;
                                                objPlugin.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Gear objPlugin = new Gear(_objCharacter);
                                if (objPlugin.ImportHeroLabGear(xmlWeaponAccessoryToImport, null, lstWeapons))
                                    _objCharacter.Gear.Add(objPlugin);
                            }
                        }
                    }
                }
                foreach (XPathNavigator xmlPluginToAdd in xmlWeaponImportNode.Select(strName + "/item[@useradded = \"no\"]"))
                {
                    string strPluginName = xmlWeaponImportNode.SelectSingleNodeAndCacheExpression("@name")?.Value;
                    if (!string.IsNullOrEmpty(strPluginName))
                    {
                        Weapon objUnderbarrel = UnderbarrelWeapons.FirstOrDefault(x => x.IncludedInWeapon && (x.Name.Contains(strPluginName) || strPluginName.Contains(x.Name)));
                        if (objUnderbarrel != null)
                        {
                            objUnderbarrel.Notes = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("description")?.Value;
                            objUnderbarrel.ProcessHeroLabWeaponPlugins(xmlPluginToAdd, lstWeapons);
                        }
                        else
                        {
                            WeaponAccessory objWeaponAccessory = WeaponAccessories.FirstOrDefault(x => x.IncludedInWeapon && (x.Name.Contains(strPluginName) || strPluginName.Contains(x.Name)));
                            if (objWeaponAccessory != null)
                            {
                                objWeaponAccessory.Notes = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("description")?.Value;

                                foreach (string strPluginNodeName in Character.HeroLabPluginNodeNames)
                                {
                                    foreach (XPathNavigator xmlSubPluginToAdd in xmlPluginToAdd.Select(strPluginNodeName + "/item[@useradded != \"no\"]"))
                                    {
                                        Gear objPlugin = new Gear(_objCharacter);
                                        if (objPlugin.ImportHeroLabGear(xmlSubPluginToAdd, objWeaponAccessory.GetNode(), lstWeapons))
                                            objWeaponAccessory.GearChildren.Add(objPlugin);
                                    }
                                    foreach (XPathNavigator xmlSubPluginToAdd in xmlPluginToAdd.Select(strPluginNodeName + "/item[@useradded = \"no\"]"))
                                    {
                                        string strGearName = xmlSubPluginToAdd.SelectSingleNodeAndCacheExpression("@name")?.Value;
                                        if (!string.IsNullOrEmpty(strGearName))
                                        {
                                            Gear objPlugin = objWeaponAccessory.GearChildren.FirstOrDefault(x => x.IncludedInParent && !string.IsNullOrEmpty(x.Name) && (x.Name.Contains(strGearName) || strGearName.Contains(x.Name)));
                                            if (objPlugin != null)
                                            {
                                                objPlugin.Quantity = xmlSubPluginToAdd.SelectSingleNodeAndCacheExpression("@quantity")?.ValueAsInt ?? 1;
                                                objPlugin.Notes = xmlSubPluginToAdd.SelectSingleNodeAndCacheExpression("description")?.Value;
                                                objPlugin.ProcessHeroLabGearPlugins(xmlSubPluginToAdd, lstWeapons);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(strName))
                            {
                                foreach (WeaponAccessory objLoopAccessory in WeaponAccessories)
                                {
                                    Gear objPlugin = objLoopAccessory.GearChildren.DeepFirstOrDefault(x => x.Children, x => x.IncludedInParent && !string.IsNullOrEmpty(x.Name) && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                    if (objPlugin != null)
                                    {
                                        objPlugin.Quantity = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("@quantity")?.ValueAsInt ?? 1;
                                        objPlugin.Notes = xmlPluginToAdd.SelectSingleNodeAndCacheExpression("description")?.Value;
                                        objPlugin.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.RefreshMatrixAttributeArray(_objCharacter);
        }

        #endregion Hero Lab Importing

        private IEnumerable<WeaponAccessory> GetClipProvidingAccessories()
        {
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                for (int i = 0; i < objAccessory.AmmoSlots; i++)
                {
                    yield return objAccessory;
                }
            }
        }

        private void AddAmmoSlots(WeaponAccessory objAccessory)
        {
            for (int i = 0; i < objAccessory.AmmoSlots; i++)
                _lstAmmo.Add(new Clip(_objCharacter, objAccessory, this, null, 0));
        }

        private void RemoveAmmoSlots(WeaponAccessory objAccessory)
        {
            for (int i = _lstAmmo.Count - 1; i >= 0; i--)
            {
                Clip clip = _lstAmmo[i];
                if (clip.OwnedBy == objAccessory.InternalId)
                {
                    if (ActiveAmmoSlot == i + 1)
                        ActiveAmmoSlot = 1;
                    UnloadGear(ParentVehicle != null
                                   ? ParentVehicle.GearChildren
                                   : _objCharacter.Gear, clip);
                    _lstAmmo.RemoveAt(i);
                }
            }
        }

        private async ValueTask RemoveAmmoSlotsAsync(WeaponAccessory objAccessory, CancellationToken token = default)
        {
            for (int i = _lstAmmo.Count - 1; i >= 0; i--)
            {
                Clip clip = _lstAmmo[i];
                if (clip.OwnedBy == objAccessory.InternalId)
                {
                    if (ActiveAmmoSlot == i + 1)
                        ActiveAmmoSlot = 1;
                    await UnloadGearAsync(ParentVehicle != null
                                              ? ParentVehicle.GearChildren
                                              : _objCharacter.Gear, clip, token).ConfigureAwait(false);
                    _lstAmmo.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Unload every clip.
        /// </summary>
        public void UnloadAll()
        {
            IAsyncCollection<Gear> gears = ParentVehicle != null
                        ? ParentVehicle.GearChildren
                        : _objCharacter.Gear;
            foreach (Clip clip in Clips)
            {
                UnloadGear(gears, clip);
            }
        }

        /// <summary>
        /// Unload every clip.
        /// </summary>
        public async ValueTask UnloadAllAsync(CancellationToken token = default)
        {
            IAsyncCollection<Gear> gears = ParentVehicle != null
                ? ParentVehicle.GearChildren
                : _objCharacter.Gear;
            foreach (Clip clip in Clips)
            {
                await UnloadGearAsync(gears, clip, token).ConfigureAwait(false);
            }
        }

        private Clip GetClip(int clip)
        {
            //1 indexed due legacy
            clip--;
            return _lstAmmo[clip];
        }

        #endregion Methods

        private IHasMatrixAttributes GetMatrixAttributesOverride
        {
            get
            {
                if (string.IsNullOrEmpty(ParentID))
                    return null;
                IHasMatrixAttributes objReturn = _objCharacter.Gear.DeepFindById(ParentID);
                if (objReturn != null)
                    return objReturn;
                foreach (Armor objArmor in _objCharacter.Armor)
                {
                    if (objArmor.InternalId == ParentID)
                        return objArmor;
                    objReturn = objArmor.GearChildren.DeepFindById(ParentID);
                    if (objReturn != null)
                        return objReturn;
                    foreach (ArmorMod objMod in objArmor.ArmorMods)
                    {
                        objReturn = objMod.GearChildren.DeepFindById(ParentID);
                        if (objReturn != null)
                            return objReturn;
                    }
                }

                foreach (Weapon objWeapon in _objCharacter.Weapons.GetAllDescendants(x => x.Children))
                {
                    if (objWeapon.InternalId == ParentID)
                        return objWeapon;
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        objReturn = objAccessory.GearChildren.DeepFindById(ParentID);
                        if (objReturn != null)
                            return objReturn;
                    }
                }

                foreach (Cyberware objCyberware in _objCharacter.Cyberware.GetAllDescendants(x => x.Children))
                {
                    if (objCyberware.InternalId == ParentID)
                        return objCyberware;
                    objReturn = objCyberware.GearChildren.DeepFindById(ParentID);
                    if (objReturn != null)
                        return objReturn;
                }

                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    if (objVehicle.InternalId == ParentID)
                        return objVehicle;
                    objReturn = objVehicle.GearChildren.DeepFindById(ParentID);
                    if (objReturn != null)
                        return objReturn;
                    foreach (Weapon objWeapon in objVehicle.Weapons.GetAllDescendants(x => x.Children))
                    {
                        if (objWeapon.InternalId == ParentID)
                            return objWeapon;
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            objReturn = objAccessory.GearChildren.DeepFindById(ParentID);
                            if (objReturn != null)
                                return objReturn;
                        }
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        foreach (Weapon objWeapon in objMod.Weapons.GetAllDescendants(x => x.Children))
                        {
                            if (objWeapon.InternalId == ParentID)
                                return objWeapon;
                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                            {
                                objReturn = objAccessory.GearChildren.DeepFindById(ParentID);
                                if (objReturn != null)
                                    return objReturn;
                            }
                        }

                        foreach (Cyberware objCyberware in objMod.Cyberware.GetAllDescendants(x => x.Children))
                        {
                            if (objCyberware.InternalId == ParentID)
                                return objCyberware;
                            objReturn = objCyberware.GearChildren.DeepFindById(ParentID);
                            if (objReturn != null)
                                return objReturn;
                        }
                    }

                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        foreach (Weapon objWeapon in objMount.Weapons.GetAllDescendants(x => x.Children))
                        {
                            if (objWeapon.InternalId == ParentID)
                                return objWeapon;
                            foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                            {
                                objReturn = objAccessory.GearChildren.DeepFindById(ParentID);
                                if (objReturn != null)
                                    return objReturn;
                            }
                        }

                        foreach (VehicleMod objMod in objMount.Mods)
                        {
                            foreach (Weapon objWeapon in objMod.Weapons.GetAllDescendants(x => x.Children))
                            {
                                if (objWeapon.InternalId == ParentID)
                                    return objWeapon;
                                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                                {
                                    objReturn = objAccessory.GearChildren.DeepFindById(ParentID);
                                    if (objReturn != null)
                                        return objReturn;
                                }
                            }

                            foreach (Cyberware objCyberware in objMod.Cyberware.GetAllDescendants(x => x.Children))
                            {
                                if (objCyberware.InternalId == ParentID)
                                    return objCyberware;
                                objReturn = objCyberware.GearChildren.DeepFindById(ParentID);
                                if (objReturn != null)
                                    return objReturn;
                            }
                        }
                    }
                }

                return null;
            }
        }

        private async ValueTask<IHasMatrixAttributes> GetMatrixAttributesOverrideAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(ParentID))
                return null;
            IHasMatrixAttributes objReturn = await _objCharacter.Gear.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
            if (objReturn != null)
                return objReturn;
            await _objCharacter.Armor.ForEachWithBreakAsync(async objArmor =>
            {
                if (objArmor.InternalId == ParentID)
                {
                    objReturn = objArmor;
                    return false;
                }

                objReturn = await objArmor.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                if (objReturn != null)
                    return false;
                await objArmor.ArmorMods.ForEachWithBreakAsync(async objMod =>
                {
                    objReturn = await objMod.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                    return objReturn == null;
                }, token).ConfigureAwait(false);

                return objReturn == null;
            }, token).ConfigureAwait(false);
            if (objReturn != null)
                return objReturn;

            foreach (Weapon objWeapon in _objCharacter.Weapons.GetAllDescendants(x => x.Children, token))
            {
                if (objWeapon.InternalId == ParentID)
                    return objWeapon;
                await objWeapon.WeaponAccessories.ForEachWithBreakAsync(async objAccessory =>
                {
                    objReturn = await objAccessory.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                    return objReturn == null;
                }, token).ConfigureAwait(false);
                if (objReturn != null)
                    return objReturn;
            }

            foreach (Cyberware objCyberware in _objCharacter.Cyberware.GetAllDescendants(x => x.Children, token))
            {
                if (objCyberware.InternalId == ParentID)
                    return objCyberware;
                objReturn = await objCyberware.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                if (objReturn != null)
                    return objReturn;
            }

            await _objCharacter.Vehicles.ForEachWithBreakAsync(async objVehicle =>
            {
                if (objVehicle.InternalId == ParentID)
                {
                    objReturn = objVehicle;
                    return false;
                }

                objReturn = await objVehicle.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                if (objReturn != null)
                    return false;
                foreach (Weapon objWeapon in objVehicle.Weapons.GetAllDescendants(x => x.Children, token))
                {
                    if (objWeapon.InternalId == ParentID)
                    {
                        objReturn = objWeapon;
                        return false;
                    }

                    await objWeapon.WeaponAccessories.ForEachWithBreakAsync(async objAccessory =>
                    {
                        objReturn = await objAccessory.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                        return objReturn == null;
                    }, token).ConfigureAwait(false);
                    if (objReturn != null)
                        return false;
                }

                await objVehicle.Mods.ForEachWithBreakAsync(async objMod =>
                {
                    foreach (Weapon objWeapon in objMod.Weapons.GetAllDescendants(x => x.Children, token))
                    {
                        if (objWeapon.InternalId == ParentID)
                        {
                            objReturn = objWeapon;
                            return false;
                        }

                        await objWeapon.WeaponAccessories.ForEachWithBreakAsync(async objAccessory =>
                        {
                            objReturn = await objAccessory.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                            return objReturn == null;
                        }, token).ConfigureAwait(false);
                        if (objReturn != null)
                            return false;
                    }

                    foreach (Cyberware objCyberware in objMod.Cyberware.GetAllDescendants(x => x.Children, token))
                    {
                        if (objCyberware.InternalId == ParentID)
                        {
                            objReturn = objCyberware;
                            return false;
                        }

                        objReturn = await objCyberware.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                        if (objReturn != null)
                            return false;
                    }

                    return objReturn == null;
                }, token).ConfigureAwait(false);

                await objVehicle.WeaponMounts.ForEachWithBreakAsync(async objMount =>
                {
                    foreach (Weapon objWeapon in objMount.Weapons.GetAllDescendants(x => x.Children, token))
                    {
                        if (objWeapon.InternalId == ParentID)
                        {
                            objReturn = objWeapon;
                            return false;
                        }

                        await objWeapon.WeaponAccessories.ForEachWithBreakAsync(async objAccessory =>
                        {
                            objReturn = await objAccessory.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                            return objReturn == null;
                        }, token).ConfigureAwait(false);
                        if (objReturn != null)
                            return false;
                    }

                    await objMount.Mods.ForEachWithBreakAsync(async objMod =>
                    {
                        foreach (Weapon objWeapon in objMod.Weapons.GetAllDescendants(x => x.Children, token))
                        {
                            if (objWeapon.InternalId == ParentID)
                            {
                                objReturn = objWeapon;
                                return false;
                            }

                            await objWeapon.WeaponAccessories.ForEachWithBreakAsync(async objAccessory =>
                            {
                                objReturn = await objAccessory.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                                return objReturn == null;
                            }, token).ConfigureAwait(false);
                            if (objReturn != null)
                                return false;
                        }

                        foreach (Cyberware objCyberware in objMod.Cyberware.GetAllDescendants(x => x.Children, token))
                        {
                            if (objCyberware.InternalId == ParentID)
                            {
                                objReturn = objCyberware;
                                return false;
                            }

                            objReturn = await objCyberware.GearChildren.DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                            if (objReturn != null)
                                return false;
                        }

                        return objReturn == null;
                    }, token).ConfigureAwait(false);

                    return objReturn == null;
                }, token).ConfigureAwait(false);
                return objReturn == null;
            }, token).ConfigureAwait(false);

            return null;
        }

        public decimal StolenTotalCost => CalculatedStolenTotalCost(true);

        public decimal NonStolenTotalCost => CalculatedStolenTotalCost(false);

        public decimal CalculatedStolenTotalCost(bool blnStolen)
        {
            decimal decReturn = 0;
            if (Stolen == blnStolen)
                decReturn += OwnCost;

            // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
            decReturn += WeaponAccessories.Sum(objAccessory => objAccessory.CalculatedStolenTotalCost(blnStolen));

            // Include the cost of any Underbarrel Weapon.
            decReturn += Children.Sum(objUnderbarrel => objUnderbarrel.CalculatedStolenTotalCost(blnStolen));

            return decReturn;
        }

        public ValueTask<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(true, token);

        public ValueTask<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(false, token);

        public async ValueTask<decimal> CalculatedStolenTotalCostAsync(bool blnStolen, CancellationToken token = default)
        {
            decimal decReturn = Stolen == blnStolen ? await GetOwnCostAsync(token).ConfigureAwait(false) : 0;

            // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
            decReturn += await WeaponAccessories.SumAsync(objAccessory => objAccessory.CalculatedStolenTotalCostAsync(blnStolen, token).AsTask(), token).ConfigureAwait(false);

            // Include the cost of any Underbarrel Weapon.
            decReturn += await Children.SumAsync(objUnderbarrel => objUnderbarrel.CalculatedStolenTotalCostAsync(blnStolen, token).AsTask(), token).ConfigureAwait(false);

            return decReturn;
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
            if (objThis != null)
                return objThis.GetBaseMatrixAttribute(strAttributeName);
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

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                    {
                        sbdValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + '}',
                                              () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0).ToString(
                                                  GlobalSettings.InvariantCultureInfo));
                        sbdValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + '}',
                                              () => Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0");
                        if (Children.Count > 0 && strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                        {
                            int intTotalChildrenValue = 0;
                            foreach (Weapon objLoopWeapon in Children)
                            {
                                if (objLoopWeapon.Equipped)
                                {
                                    intTotalChildrenValue += objLoopWeapon.GetBaseMatrixAttribute(strMatrixAttribute);
                                }
                            }

                            sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                             intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                        }
                    }

                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);
                    // Replace the division sign with "div" since we're using XPath.
                    sbdValue.Replace("/", " div ");
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }
            int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        public async Task<int> GetBaseMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IHasMatrixAttributes objThis = await GetMatrixAttributesOverrideAsync(token).ConfigureAwait(false);
            if (objThis != null)
                return await objThis.GetBaseMatrixAttributeAsync(strAttributeName, token).ConfigureAwait(false);
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

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                    {
                        await sbdValue.CheapReplaceAsync(strExpression, "{Gear " + strMatrixAttribute + '}',
                            () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0).ToString(
                                GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        await sbdValue.CheapReplaceAsync(strExpression, "{Parent " + strMatrixAttribute + '}',
                                () => Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0", token: token)
                            .ConfigureAwait(false);
                        if (await Children.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                            strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                        {
                            int intTotalChildrenValue = await Children.SumAsync(x => x.Equipped,
                                    x => x.GetBaseMatrixAttributeAsync(strMatrixAttribute, token), token)
                                .ConfigureAwait(false);
                            sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                        }
                    }

                    await _objCharacter.AttributeSection
                        .ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                    // Replace the division sign with "div" since we're using XPath.
                    sbdValue.Replace("/", " div ");
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

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
            if (objThis != null)
                return objThis.GetBonusMatrixAttribute(strAttributeName);
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                ++intReturn;
            }

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Weapon objLoopWeapon in Children)
            {
                if (objLoopWeapon.Equipped && objLoopWeapon.ParentID != InternalId)
                {
                    intReturn += objLoopWeapon.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        public async Task<int> GetBonusMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            IHasMatrixAttributes objThis = await GetMatrixAttributesOverrideAsync(token);
            if (objThis != null)
                return await objThis.GetBonusMatrixAttributeAsync(strAttributeName, token);
            int intReturn = await GetOverclockedAsync(token).ConfigureAwait(false) == strAttributeName ? 1 : 0;

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            intReturn += await Children.SumAsync(x => x.Equipped && x.ParentID != InternalId,
                x => x.GetTotalMatrixAttributeAsync(strAttributeName, token), token);

            return intReturn;
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (!CanBeRemoved)
                return false;
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeapon")))
                return false;
            DeleteWeapon();
            return false;
        }

        /// <summary>
        /// Check whether the weapon is not removable due to various restrictions.
        /// </summary>
        /// <returns></returns>
        private bool CanBeRemoved
        {
            get
            {
                // Cyberweapons cannot be removed through here and must be done by removing the piece of Cyberware.
                if (Cyberware)
                {
                    Program.ShowScrollableMessageBox(
                        LanguageManager.GetString("Message_CannotRemoveCyberweapon"),
                        LanguageManager.GetString("MessageTitle_CannotRemoveCyberweapon"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                // Qualities cannot be removed through here and must be done by removing the piece of Cyberware.
                if (Category.StartsWith("Quality", StringComparison.Ordinal))
                {
                    Program.ShowScrollableMessageBox(
                        LanguageManager.GetString("Message_CannotRemoveQualityWeapon"),
                        LanguageManager.GetString("MessageTitle_CannotRemoveQualityWeapon"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (Category == "Gear")
                {
                    Program.ShowScrollableMessageBox(
                        LanguageManager.GetString(ParentVehicle != null ? "Message_CannotRemoveGearWeaponVehicle" : "Message_CannotRemoveGearWeapon"),
                        LanguageManager.GetString("MessageTitle_CannotRemoveGearWeapon"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                return true;
            }
        }

        public bool Sell(decimal percentage, bool blnConfirmDelete)
        {
            if (!CanBeRemoved)
                return false;
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeapon")))
                return false;

            if (!_objCharacter.Created)
            {
                DeleteWeapon();
                return true;
            }

            string strExpense = ParentVehicle != null ? "String_ExpenseSoldVehicleWeapon" : "String_ExpenseSoldWeapon";

            IHasCost objParent = null;
            if (Parent != null)
                objParent = Parent;
            else if (ParentVehicle != null)
            {
                if (ParentVehicleMod != null)
                    objParent = ParentVehicleMod;
                else if (ParentMount != null)
                    objParent = ParentMount;
                else
                    objParent = ParentVehicle;
            }
            // Record the cost of the Weapon carrier with the Weapon.
            decimal decOriginal = Parent?.TotalCost ?? TotalCost;
            decimal decAmount = DeleteWeapon() * percentage;
            decAmount += (decOriginal - (objParent?.TotalCost ?? 0)) * percentage;
            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString(strExpense) + ' ' + CurrentDisplayNameShort, ExpenseType.Nuyen, DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen += decAmount;
            return true;
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }

        public Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            return SourceDetail.SetControlAsync(sourceControl, token);
        }

        public bool AllowPasteXml
        {
            get
            {
                switch (GlobalSettings.ClipboardContentType)
                {
                    case ClipboardContentType.WeaponAccessory:
                        XPathNavigator checkNode = GlobalSettings.Clipboard.SelectSingleNodeAndCacheExpressionAsNavigator("/character/weaponaccessories/accessory");
                        if (CheckAccessoryRequirements(checkNode))
                            return true;
                        break;

                    default:
                        return false;
                }

                return false;
            }
        }

        public bool AllowPasteObject(object input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether a given WeaponAccessory is allowed to be added to this weapon.
        /// </summary>
        public bool CheckAccessoryRequirements(XPathNavigator objXmlAccessory)
        {
            if (objXmlAccessory == null) return false;
            string[] lstMounts = AccessoryMounts.Split('/', StringSplitOptions.RemoveEmptyEntries);
            XPathNavigator xmlMountNode = objXmlAccessory.SelectSingleNodeAndCacheExpression("mount");
            if (!string.IsNullOrEmpty(xmlMountNode?.Value) && (lstMounts.Length == 0 || xmlMountNode.Value.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries).All(strItem =>
                !string.IsNullOrEmpty(strItem) && lstMounts.All(strAllowedMount => strAllowedMount != strItem))))
            {
                return false;
            }
            xmlMountNode = objXmlAccessory.SelectSingleNodeAndCacheExpression("extramount");
            if (!string.IsNullOrEmpty(xmlMountNode?.Value) && (lstMounts.Length == 0 || xmlMountNode.Value.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries).All(strItem =>
                !string.IsNullOrEmpty(strItem) && lstMounts.All(strAllowedMount => strAllowedMount != strItem))))
            {
                return false;
            }

            if (!objXmlAccessory.RequirementsMet(_objCharacter, this, string.Empty, string.Empty))
                return false;

            XPathNavigator xmlTestNode = objXmlAccessory.SelectSingleNodeAndCacheExpression("forbidden/weapondetails");
            if (xmlTestNode != null)
            {
                XPathNavigator xmlRequirementsNode = this.GetNodeXPath();
                // Assumes topmost parent is an AND node
                if (xmlRequirementsNode.ProcessFilterOperationNode(xmlTestNode, false))
                    return false;
                xmlTestNode = objXmlAccessory.SelectSingleNodeAndCacheExpression("required/weapondetails");
                // Assumes topmost parent is an AND node
                return xmlTestNode == null || xmlRequirementsNode.ProcessFilterOperationNode(xmlTestNode, false);
            }

            xmlTestNode = objXmlAccessory.SelectSingleNodeAndCacheExpression("required/weapondetails");
            // Assumes topmost parent is an AND node
            return xmlTestNode == null || this.GetNodeXPath().ProcessFilterOperationNode(xmlTestNode, false);
        }

        /// <summary>
        /// Checks whether a given WeaponAccessory is allowed to be added to this weapon.
        /// </summary>
        public async ValueTask<bool> CheckAccessoryRequirementsAsync(XPathNavigator objXmlAccessory,
                                                                     CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objXmlAccessory == null)
                return false;
            string[] lstMounts = AccessoryMounts.Split('/', StringSplitOptions.RemoveEmptyEntries);
            XPathNavigator xmlMountNode = await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("mount", token)
                                                               .ConfigureAwait(false);
            if (!string.IsNullOrEmpty(xmlMountNode?.Value) && (lstMounts.Length == 0 || xmlMountNode.Value
                    .SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries).All(strItem =>
                        !string.IsNullOrEmpty(strItem)
                        && lstMounts.All(strAllowedMount => strAllowedMount != strItem))))
            {
                return false;
            }

            xmlMountNode = await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("extramount", token)
                                                .ConfigureAwait(false);
            if (!string.IsNullOrEmpty(xmlMountNode?.Value) && (lstMounts.Length == 0 || xmlMountNode.Value
                    .SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries).All(strItem =>
                        !string.IsNullOrEmpty(strItem)
                        && lstMounts.All(strAllowedMount => strAllowedMount != strItem))))
            {
                return false;
            }

            if (!await objXmlAccessory.RequirementsMetAsync(_objCharacter, this, string.Empty, string.Empty, token: token).ConfigureAwait(false))
                return false;

            XPathNavigator xmlTestNode = await objXmlAccessory
                                               .SelectSingleNodeAndCacheExpressionAsync(
                                                   "forbidden/weapondetails", token).ConfigureAwait(false);
            if (xmlTestNode != null)
            {
                XPathNavigator xmlRequirementsNode = await this.GetNodeXPathAsync(token).ConfigureAwait(false);
                // Assumes topmost parent is an AND node
                if (await xmlRequirementsNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token)
                                             .ConfigureAwait(false))
                    return false;
                xmlTestNode = await objXmlAccessory
                                    .SelectSingleNodeAndCacheExpressionAsync("required/weapondetails", token)
                                    .ConfigureAwait(false);
                // Assumes topmost parent is an AND node
                return xmlTestNode == null || await xmlRequirementsNode
                                                    .ProcessFilterOperationNodeAsync(xmlTestNode, false, token)
                                                    .ConfigureAwait(false);
            }

            xmlTestNode = await objXmlAccessory.SelectSingleNodeAndCacheExpressionAsync("required/weapondetails", token)
                                               .ConfigureAwait(false);
            // Assumes topmost parent is an AND node
            return xmlTestNode == null || await (await this.GetNodeXPathAsync(token).ConfigureAwait(false))
                                                .ProcessFilterOperationNodeAsync(xmlTestNode, false, token)
                                                .ConfigureAwait(false);
        }

        public void ProcessAttributesInXPath(StringBuilder sbdInput, string strOriginal = "", bool blnForRange = false)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            Dictionary<string, int> dicAttributeOverrides = null;
            if (strOriginal.ContainsAny("{STR", "{AGI"))
            {
                int intUseSTR = 0;
                int intUseAGI = 0;
                int intUseSTRUnaug = 0;
                int intUseAGIUnaug = 0;
                int intUseSTRBase = 0;
                int intUseAGIBase = 0;
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        intUseSTRUnaug = intUseSTR;
                        intUseSTRBase = intUseSTR;
                        intUseAGI = ParentVehicle.Pilot;
                        intUseAGIUnaug = intUseAGI;
                        intUseAGIBase = intUseAGI;
                        if (!string.IsNullOrEmpty(ParentID))
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID, out VehicleMod objVehicleMod);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                int intSTR = objAttributeSource.GetAttributeTotalValue("STR");
                                int intSTRValue = objAttributeSource.GetAttributeValue("STR");
                                int intSTRBase = objAttributeSource.GetAttributeBaseValue("STR");
                                int intAGI = objAttributeSource.GetAttributeTotalValue("AGI");
                                int intAGIValue = objAttributeSource.GetAttributeValue("AGI");
                                int intAGIBase = objAttributeSource.GetAttributeBaseValue("AGI");
                                while (objAttributeSource != null && intSTR == 0 && intAGI == 0 && intSTRValue == 0 && intAGIValue == 0 && intSTRBase == 0 && intAGIBase == 0)
                                {
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null)
                                        continue;
                                    intSTR = objAttributeSource.GetAttributeTotalValue("STR");
                                    intSTRValue = objAttributeSource.GetAttributeValue("STR");
                                    intSTRBase = objAttributeSource.GetAttributeBaseValue("STR");
                                    intAGI = objAttributeSource.GetAttributeTotalValue("AGI");
                                    intAGIValue = objAttributeSource.GetAttributeValue("AGI");
                                    intAGIBase = objAttributeSource.GetAttributeBaseValue("AGI");
                                }

                                intUseSTR = intSTR;
                                intUseAGI = intAGI;
                                intUseSTRUnaug = intSTRValue;
                                intUseAGIUnaug = intAGIValue;
                                intUseSTRBase = intSTRBase;
                                intUseAGIBase = intAGIBase;

                                if (intUseSTR == 0)
                                    intUseSTR = objVehicleMod.TotalStrength;
                                if (intUseAGI == 0)
                                    intUseAGI = objVehicleMod.TotalAgility;
                                if (intUseSTRUnaug == 0)
                                    intUseSTRUnaug = objVehicleMod.TotalStrength;
                                if (intUseAGIUnaug == 0)
                                    intUseAGIUnaug = objVehicleMod.TotalAgility;
                                if (intUseSTRBase == 0)
                                    intUseSTRBase = ParentVehicle.TotalBody;
                                if (intUseAGIBase == 0)
                                    intUseAGIBase = ParentVehicle.Pilot;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.GetAttributeTotalValue("STR");
                            int intSTRValue = objAttributeSource.GetAttributeValue("STR");
                            int intSTRBase = objAttributeSource.GetAttributeBaseValue("STR");
                            int intAGI = objAttributeSource.GetAttributeTotalValue("AGI");
                            int intAGIValue = objAttributeSource.GetAttributeValue("AGI");
                            int intAGIBase = objAttributeSource.GetAttributeBaseValue("AGI");
                            while (objAttributeSource != null && intSTR == 0 && intAGI == 0 && intSTRValue == 0 && intAGIValue == 0 && intSTRBase == 0 && intAGIBase == 0)
                            {
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null)
                                    continue;
                                intSTR = objAttributeSource.GetAttributeTotalValue("STR");
                                intSTRValue = objAttributeSource.GetAttributeValue("STR");
                                intSTRBase = objAttributeSource.GetAttributeBaseValue("STR");
                                intAGI = objAttributeSource.GetAttributeTotalValue("AGI");
                                intAGIValue = objAttributeSource.GetAttributeValue("AGI");
                                intAGIBase = objAttributeSource.GetAttributeBaseValue("AGI");
                            }

                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                            intUseSTRUnaug = intSTRValue;
                            intUseAGIUnaug = intAGIValue;
                            intUseSTRBase = intSTRBase;
                            intUseAGIBase = intAGIBase;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                        if (intUseAGI == 0)
                            intUseAGI = _objCharacter.AGI.TotalValue;
                        if (intUseSTRUnaug == 0)
                            intUseSTRUnaug = _objCharacter.STR.Value;
                        if (intUseAGIUnaug == 0)
                            intUseAGIUnaug = _objCharacter.AGI.Value;
                        if (intUseSTRBase == 0)
                            intUseSTRBase = _objCharacter.STR.TotalBase;
                        if (intUseAGIBase == 0)
                            intUseAGIBase = _objCharacter.AGI.TotalBase;
                    }
                }
                else if (ParentVehicle == null)
                {
                    intUseSTR = _objCharacter.STR.TotalValue;
                    intUseAGI = _objCharacter.AGI.TotalValue;
                    intUseSTRUnaug = _objCharacter.STR.Value;
                    intUseAGIUnaug = _objCharacter.AGI.Value;
                    intUseSTRBase = _objCharacter.STR.TotalBase;
                    intUseAGIBase = _objCharacter.AGI.TotalBase;
                }

                if (Category == "Throwing Weapons" || Skill?.DictionaryKey == "Throwing Weapons")
                    intUseSTR += (blnForRange
                            ? ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR) +
                              ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowRangeSTR)
                            : ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR))
                        .StandardRound();
                dicAttributeOverrides = new Dictionary<string, int>(6)
                {
                    {"STR", intUseSTR},
                    {"STRUnaug", intUseSTRUnaug},
                    {"STRBase", intUseSTRBase},
                    {"AGI", intUseAGI},
                    {"AGIUnaug", intUseAGIUnaug},
                    {"AGIBase", intUseAGIBase}
                };
            }
            _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdInput, strOriginal, dicAttributeOverrides);
        }

        public async ValueTask ProcessAttributesInXPathAsync(StringBuilder sbdInput, string strOriginal = "", bool blnForRange = false, CancellationToken token = default)
        {
            if (sbdInput == null || sbdInput.Length <= 0)
                return;
            if (string.IsNullOrEmpty(strOriginal))
                strOriginal = sbdInput.ToString();
            Dictionary<string, int> dicAttributeOverrides = null;
            if (strOriginal.ContainsAny("{STR", "{AGI"))
            {
                int intUseSTR = 0;
                int intUseAGI = 0;
                int intUseSTRUnaug = 0;
                int intUseAGIUnaug = 0;
                int intUseSTRBase = 0;
                int intUseAGIBase = 0;
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                        intUseSTRUnaug = intUseSTR;
                        intUseSTRBase = intUseSTR;
                        intUseAGI = await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false);
                        intUseAGIUnaug = intUseAGI;
                        intUseAGIBase = intUseAGI;
                        if (!string.IsNullOrEmpty(ParentID))
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID, out VehicleMod objVehicleMod);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                int intSTR = await objAttributeSource.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                                int intSTRValue = await objAttributeSource.GetAttributeValueAsync("STR", token).ConfigureAwait(false);
                                int intSTRBase = await objAttributeSource.GetAttributeBaseValueAsync("STR", token).ConfigureAwait(false);
                                int intAGI = await objAttributeSource.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                                int intAGIValue = await objAttributeSource.GetAttributeValueAsync("AGI", token).ConfigureAwait(false);
                                int intAGIBase = await objAttributeSource.GetAttributeBaseValueAsync("AGI", token).ConfigureAwait(false);
                                while (objAttributeSource != null && intSTR == 0 && intAGI == 0 && intSTRValue == 0 && intAGIValue == 0 && intSTRBase == 0 && intAGIBase == 0)
                                {
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null)
                                        continue;
                                    intSTR = await objAttributeSource.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                                    intSTRValue = await objAttributeSource.GetAttributeValueAsync("STR", token).ConfigureAwait(false);
                                    intSTRBase = await objAttributeSource.GetAttributeBaseValueAsync("STR", token).ConfigureAwait(false);
                                    intAGI = await objAttributeSource.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                                    intAGIValue = await objAttributeSource.GetAttributeValueAsync("AGI", token).ConfigureAwait(false);
                                    intAGIBase = await objAttributeSource.GetAttributeBaseValueAsync("AGI", token).ConfigureAwait(false);
                                }

                                intUseSTR = intSTR;
                                intUseAGI = intAGI;
                                intUseSTRUnaug = intSTRValue;
                                intUseAGIUnaug = intAGIValue;
                                intUseSTRBase = intSTRBase;
                                intUseAGIBase = intAGIBase;

                                if (intUseSTR == 0)
                                    intUseSTR = await objVehicleMod.GetTotalStrengthAsync(token).ConfigureAwait(false);
                                if (intUseAGI == 0)
                                    intUseAGI = await objVehicleMod.GetTotalAgilityAsync(token).ConfigureAwait(false);
                                if (intUseSTRUnaug == 0)
                                    intUseSTRUnaug = await objVehicleMod.GetTotalStrengthAsync(token).ConfigureAwait(false);
                                if (intUseAGIUnaug == 0)
                                    intUseAGIUnaug = await objVehicleMod.GetTotalAgilityAsync(token).ConfigureAwait(false);
                                if (intUseSTRBase == 0)
                                    intUseSTRBase = await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                                if (intUseAGIBase == 0)
                                    intUseAGIBase = await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = await objAttributeSource.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                            int intSTRValue = await objAttributeSource.GetAttributeValueAsync("STR", token).ConfigureAwait(false);
                            int intSTRBase = await objAttributeSource.GetAttributeBaseValueAsync("STR", token).ConfigureAwait(false);
                            int intAGI = await objAttributeSource.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                            int intAGIValue = await objAttributeSource.GetAttributeValueAsync("AGI", token).ConfigureAwait(false);
                            int intAGIBase = await objAttributeSource.GetAttributeBaseValueAsync("AGI", token).ConfigureAwait(false);
                            while (objAttributeSource != null && intSTR == 0 && intAGI == 0 && intSTRValue == 0 && intAGIValue == 0 && intSTRBase == 0 && intAGIBase == 0)
                            {
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null)
                                    continue;
                                intSTR = await objAttributeSource.GetAttributeTotalValueAsync("STR", token).ConfigureAwait(false);
                                intSTRValue = await objAttributeSource.GetAttributeValueAsync("STR", token).ConfigureAwait(false);
                                intSTRBase = await objAttributeSource.GetAttributeBaseValueAsync("STR", token).ConfigureAwait(false);
                                intAGI = await objAttributeSource.GetAttributeTotalValueAsync("AGI", token).ConfigureAwait(false);
                                intAGIValue = await objAttributeSource.GetAttributeValueAsync("AGI", token).ConfigureAwait(false);
                                intAGIBase = await objAttributeSource.GetAttributeBaseValueAsync("AGI", token).ConfigureAwait(false);
                            }

                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                            intUseSTRUnaug = intSTRValue;
                            intUseAGIUnaug = intAGIValue;
                            intUseSTRBase = intSTRBase;
                            intUseAGIBase = intAGIBase;
                        }
                        CharacterAttrib objStrength = await _objCharacter.GetAttributeAsync("STR", token: token).ConfigureAwait(false);
                        CharacterAttrib objAgility = await _objCharacter.GetAttributeAsync("AGI", token: token).ConfigureAwait(false);
                        if (intUseSTR == 0)
                            intUseSTR = await objStrength.GetTotalValueAsync(token).ConfigureAwait(false);
                        if (intUseAGI == 0)
                            intUseAGI = await objAgility.GetTotalValueAsync(token).ConfigureAwait(false);
                        if (intUseSTRUnaug == 0)
                            intUseSTRUnaug = await objStrength.GetValueAsync(token).ConfigureAwait(false);
                        if (intUseAGIUnaug == 0)
                            intUseAGIUnaug = await objAgility.GetValueAsync(token).ConfigureAwait(false);
                        if (intUseSTRBase == 0)
                            intUseSTRBase = await objStrength.GetTotalBaseAsync(token).ConfigureAwait(false);
                        if (intUseAGIBase == 0)
                            intUseAGIBase = await objAgility.GetTotalBaseAsync(token).ConfigureAwait(false);
                    }
                }
                else if (ParentVehicle == null)
                {
                    CharacterAttrib objStrength = await _objCharacter.GetAttributeAsync("STR", token: token).ConfigureAwait(false);
                    CharacterAttrib objAgility = await _objCharacter.GetAttributeAsync("AGI", token: token).ConfigureAwait(false);
                    intUseSTR = await objStrength.GetTotalValueAsync(token).ConfigureAwait(false);
                    intUseAGI = await objAgility.GetTotalValueAsync(token).ConfigureAwait(false);
                    intUseSTRUnaug = await objStrength.GetValueAsync(token).ConfigureAwait(false);
                    intUseAGIUnaug = await objAgility.GetValueAsync(token).ConfigureAwait(false);
                    intUseSTRBase = await objStrength.GetTotalBaseAsync(token).ConfigureAwait(false);
                    intUseAGIBase = await objAgility.GetTotalBaseAsync(token).ConfigureAwait(false);
                }

                if (Category == "Throwing Weapons" || Skill?.DictionaryKey == "Throwing Weapons")
                    intUseSTR += (blnForRange
                            ? await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowSTR, token: token).ConfigureAwait(false) +
                              await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowRangeSTR, token: token).ConfigureAwait(false)
                            : await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowSTR, token: token).ConfigureAwait(false))
                        .StandardRound();
                dicAttributeOverrides = new Dictionary<string, int>(6)
                {
                    {"STR", intUseSTR},
                    {"STRUnaug", intUseSTRUnaug},
                    {"STRBase", intUseSTRBase},
                    {"AGI", intUseAGI},
                    {"AGIUnaug", intUseAGIUnaug},
                    {"AGIBase", intUseAGIBase}
                };
            }
            await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdInput, strOriginal, dicAttributeOverrides, token).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (WeaponAccessory objChild in _lstAccessories)
                objChild.Dispose();
            foreach (Weapon objChild in _lstUnderbarrel)
                objChild.Dispose();
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstAccessories.Dispose();
            _lstUnderbarrel.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            foreach (WeaponAccessory objChild in _lstAccessories)
                await objChild.DisposeAsync().ConfigureAwait(false);
            foreach (Weapon objChild in _lstUnderbarrel)
                await objChild.DisposeAsync().ConfigureAwait(false);
            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private async ValueTask DisposeSelfAsync()
        {
            await _lstAccessories.DisposeAsync().ConfigureAwait(false);
            await _lstUnderbarrel.DisposeAsync().ConfigureAwait(false);
        }
    }
}
