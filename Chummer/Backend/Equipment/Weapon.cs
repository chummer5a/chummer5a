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
    [DebuggerDisplay("{DisplayName(null, \"en-us\")}")]
    public sealed class Weapon : IHasChildren<Weapon>, IHasName, IHasSourceId, IHasInternalId, IHasXmlDataNode,
        IHasMatrixAttributes, IHasNotes, ICanSell, IHasCustomName, IHasLocation, ICanEquip, IHasSource, ICanSort,
        IHasWirelessBonus, IHasStolenProperty, ICanPaste, IHasRating, ICanBlackMarketDiscount, IDisposable,
        IAsyncDisposable
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private string _strReach = "0";
        private int _intAmmoSlots = 1;
        private string _strDamage = string.Empty;
        private string _strAP = "0";
        private string _strMode = string.Empty;
        private string _strRC = string.Empty;
        private string _strAmmo = string.Empty;
        private string _strAmmoCategory = string.Empty;
        private string _strWeaponType = string.Empty;
        private string _strConceal = "0";
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
        private readonly TaggedObservableCollection<WeaponAccessory> _lstAccessories;
        private readonly TaggedObservableCollection<Weapon> _lstUnderbarrel;
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
        private bool _blnSkipEvents;
        private bool _blnDiscountCost;
        private bool _blnRequireAmmo = true;
        private string _strAccuracy = string.Empty;
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
        private XmlElement _nodWirelessWeaponBonus;
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
            _lstUnderbarrel = new TaggedObservableCollection<Weapon>(objCharacter.LockObject);
            _lstUnderbarrel.AddTaggedCollectionChanged(this, ChildrenOnCollectionChanged);
            _lstAccessories = new TaggedObservableCollection<WeaponAccessory>(objCharacter.LockObject);
            _lstAccessories.AddTaggedCollectionChanged(this, AccessoriesOnCollectionChanged);
        }

        private async Task AccessoriesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e.Action == NotifyCollectionChangedAction.Move || _blnSkipEvents)
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
                            await RemoveAmmoSlotsAsync(objOldItem, token).ConfigureAwait(false);
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
                            await RemoveAmmoSlotsAsync(objOldItem, token).ConfigureAwait(false);
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
                await RecreateInternalClipAsync(token).ConfigureAwait(false);
            if (blnDoEncumbranceRefresh)
                await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token)
                    .ConfigureAwait(false);
        }

        private async Task ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (e.Action == NotifyCollectionChangedAction.Move)
                return;
            bool blnEverDoEncumbranceRefresh = !_blnSkipEvents && _objCharacter?.IsLoading == false && Equipped && ParentVehicle == null;
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
                                || await objNewItem.WeaponAccessories.AnyAsync(
                                    x => x.Equipped && (!string.IsNullOrEmpty(x.Weight)
                                                        || x.GearChildren.DeepAny(
                                                            y => y.Children.Where(z => z.Equipped),
                                                            y => y.Equipped && !string.IsNullOrEmpty(y.Weight))),
                                    token: token).ConfigureAwait(false)
                                || await objNewItem.Children.DeepAnyAsync(
                                    async x => await x.Children.ToListAsync(y => y.Equipped, token).ConfigureAwait(false),
                                    async z => z.Equipped && (!string.IsNullOrEmpty(z.Weight)
                                                              || await z.WeaponAccessories.AnyAsync(
                                                                  async x => x.Equipped
                                                                             && (!string.IsNullOrEmpty(x.Weight)
                                                                                 || await x.GearChildren.DeepAnyAsync(
                                                                                     async y => await y.Children
                                                                                         .ToListAsync(
                                                                                             t => t.Equipped, token).ConfigureAwait(false),
                                                                                     y => y.Equipped
                                                                                         && !string.IsNullOrEmpty(
                                                                                             y.Weight), token).ConfigureAwait(false)),
                                                                  token).ConfigureAwait(false)),
                                    token).ConfigureAwait(false)))
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
                                || await objOldItem.WeaponAccessories.AnyAsync(
                                    x => x.Equipped && (!string.IsNullOrEmpty(x.Weight)
                                                        || x.GearChildren.DeepAny(
                                                            y => y.Children.Where(z => z.Equipped),
                                                            y => y.Equipped && !string.IsNullOrEmpty(y.Weight))),
                                    token: token).ConfigureAwait(false)
                                || await objOldItem.Children.DeepAnyAsync(
                                    async x => await x.Children.ToListAsync(y => y.Equipped, token).ConfigureAwait(false),
                                    async z => z.Equipped && (!string.IsNullOrEmpty(z.Weight)
                                                              || await z.WeaponAccessories.AnyAsync(
                                                                  async x => x.Equipped
                                                                             && (!string.IsNullOrEmpty(x.Weight)
                                                                                 || await x.GearChildren.DeepAnyAsync(
                                                                                     async y => await y.Children
                                                                                         .ToListAsync(t => t.Equipped,
                                                                                             token).ConfigureAwait(false),
                                                                                     y => y.Equipped
                                                                                         && !string.IsNullOrEmpty(
                                                                                             y.Weight), token).ConfigureAwait(false)),
                                                                  token).ConfigureAwait(false)), token).ConfigureAwait(false)))
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
                                || await objOldItem.WeaponAccessories.AnyAsync(
                                    x => x.Equipped && (!string.IsNullOrEmpty(x.Weight)
                                                        || x.GearChildren.DeepAny(
                                                            y => y.Children.Where(z => z.Equipped),
                                                            y => y.Equipped && !string.IsNullOrEmpty(y.Weight))),
                                    token: token).ConfigureAwait(false)
                                || await objOldItem.Children.DeepAnyAsync(
                                    async x => await x.Children.ToListAsync(y => y.Equipped, token).ConfigureAwait(false),
                                    async z => z.Equipped && (!string.IsNullOrEmpty(z.Weight)
                                                              || await z.WeaponAccessories.AnyAsync(
                                                                  async x => x.Equipped
                                                                             && (!string.IsNullOrEmpty(x.Weight)
                                                                                 || await x.GearChildren.DeepAnyAsync(
                                                                                     async y => await y.Children
                                                                                         .ToListAsync(t => t.Equipped,
                                                                                             token).ConfigureAwait(false),
                                                                                     y => y.Equipped
                                                                                         && !string.IsNullOrEmpty(
                                                                                             y.Weight), token).ConfigureAwait(false)),
                                                                  token).ConfigureAwait(false)), token).ConfigureAwait(false)))
                            blnDoEncumbranceRefresh = true;
                    }

                    foreach (Weapon objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (blnEverDoEncumbranceRefresh && !blnDoEncumbranceRefresh && objNewItem.Equipped
                            && (!string.IsNullOrEmpty(Weight)
                                || !string.IsNullOrEmpty(objNewItem.Weight)
                                || await objNewItem.WeaponAccessories.AnyAsync(
                                    x => x.Equipped && (!string.IsNullOrEmpty(x.Weight)
                                                        || x.GearChildren.DeepAny(
                                                            y => y.Children.Where(z => z.Equipped),
                                                            y => y.Equipped && !string.IsNullOrEmpty(y.Weight))),
                                    token: token).ConfigureAwait(false)
                                || await objNewItem.Children.DeepAnyAsync(
                                    async x => await x.Children.ToListAsync(y => y.Equipped, token).ConfigureAwait(false),
                                    async z => z.Equipped && (!string.IsNullOrEmpty(z.Weight)
                                                              || await z.WeaponAccessories.AnyAsync(
                                                                  async x => x.Equipped
                                                                             && (!string.IsNullOrEmpty(x.Weight)
                                                                                 || await x.GearChildren.DeepAnyAsync(
                                                                                     async y => await y.Children
                                                                                         .ToListAsync(
                                                                                             t => t.Equipped, token).ConfigureAwait(false),
                                                                                     y => y.Equipped
                                                                                         && !string.IsNullOrEmpty(
                                                                                             y.Weight), token).ConfigureAwait(false)),
                                                                  token).ConfigureAwait(false)),
                                    token).ConfigureAwait(false)))
                            blnDoEncumbranceRefresh = true;
                    }

                    break;

                case NotifyCollectionChangedAction.Reset:
                    blnDoEncumbranceRefresh = blnEverDoEncumbranceRefresh;
                    break;
            }

            await this.RefreshMatrixAttributeArrayAsync(_objCharacter, token).ConfigureAwait(false);
            if (blnDoEncumbranceRefresh)
                await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token)
                    .ConfigureAwait(false);
        }

        /// <summary>
        /// Create a Weapon from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlWeapon">XmlNode to create the object from.</param>
        /// <param name="lstWeapons">List of child Weapons to generate.</param>
        /// <param name="blnCreateChildren">Whether child items should be created.</param>
        /// <param name="blnCreateImprovements">Whether bonuses should be created.</param>
        /// <param name="blnSkipCost">Whether forms asking to determine variable costs should be displayed.</param>
        /// <param name="intRating">Rating of the weapon</param>
        /// <param name="blnForSelectForm">Whether this weapon is being created for a Selection form (which means a lot of expensive code should be skipped).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlWeapon, ICollection<Weapon> lstWeapons, bool blnCreateChildren = true,
            bool blnCreateImprovements = true, bool blnSkipCost = false, int intRating = 0, bool blnForSelectForm = false,
            CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlWeapon, lstWeapons, blnCreateChildren,
                blnCreateImprovements, blnSkipCost, intRating, blnForSelectForm, token), token);
        }

        /// <summary>
        /// Create a Weapon from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlWeapon">XmlNode to create the object from.</param>
        /// <param name="lstWeapons">List of child Weapons to generate.</param>
        /// <param name="blnCreateChildren">Whether child items should be created.</param>
        /// <param name="blnCreateImprovements">Whether bonuses should be created.</param>
        /// <param name="blnSkipCost">Whether forms asking to determine variable costs should be displayed.</param>
        /// <param name="intRating">Rating of the weapon</param>
        /// <param name="blnForSelectForm">Whether this weapon is being created for a Selection form (which means a lot of expensive code should be skipped).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task CreateAsync(XmlNode objXmlWeapon, ICollection<Weapon> lstWeapons, bool blnCreateChildren = true,
            bool blnCreateImprovements = true, bool blnSkipCost = false, int intRating = 0, bool blnForSelectForm = false,
            CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlWeapon, lstWeapons, blnCreateChildren, blnCreateImprovements,
                blnSkipCost, intRating, blnForSelectForm, token);
        }

        private async Task CreateCoreAsync(bool blnSync, XmlNode objXmlWeapon, ICollection<Weapon> lstWeapons,
            bool blnCreateChildren = true,
            bool blnCreateImprovements = true, bool blnSkipCost = false, int intRating = 0, bool blnForSelectForm = false,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
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

            _blnSkipEvents = !blnForSelectForm;
            _blnEquipped = !blnForSelectForm && blnCreateImprovements;
            objXmlWeapon.TryGetStringFieldQuickly("name", ref _strName);
            objXmlWeapon.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlWeapon.TryGetStringFieldQuickly("type", ref _strType);
            objXmlWeapon.TryGetStringFieldQuickly("reach", ref _strReach);
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

            if (!blnForSelectForm)
            {
                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlWeapon.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);
            }

            _intRating = blnSync
                ? Math.Max(Math.Min(intRating, MaxRatingValue), MinRatingValue)
                : Math.Max(Math.Min(intRating, await GetMaxRatingValueAsync(token).ConfigureAwait(false)),
                           await GetMinRatingValueAsync(token).ConfigureAwait(false));
            if (objXmlWeapon["accessorymounts"] != null)
            {
                XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("accessorymounts/mount");
                if (objXmlMountList?.Count > 0)
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
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
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
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
            _nodWirelessWeaponBonus = objXmlWeapon["wirelessweaponbonus"];
            objXmlWeapon.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn);
            objXmlWeapon.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            if (!objXmlWeapon.TryGetInt32FieldQuickly("ammoslots", ref _intAmmoSlots))
                _intAmmoSlots = 1;
            objXmlWeapon.TryGetStringFieldQuickly("rc", ref _strRC);
            objXmlWeapon.TryGetStringFieldQuickly("conceal", ref _strConceal);
            objXmlWeapon.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlWeapon.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlWeapon.TryGetStringFieldQuickly("weight", ref _strWeight);
            objXmlWeapon.TryGetBoolFieldQuickly("stolen", ref _blnStolen);

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

                        if (blnSync)
                        {
                            using (ThreadSafeForm<SelectNumber> frmPickNumber
                                   // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                   = ThreadSafeForm<SelectNumber>.Get(() =>
                                       new SelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
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

                                _strCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings
                                    .InvariantCultureInfo);
                            }
                        }
                        else
                        {
                            string strDescription = string.Format(
                                GlobalSettings.CultureInfo,
                                await LanguageManager.GetStringAsync("String_SelectVariableCost", token: token)
                                    .ConfigureAwait(false),
                                await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false));
                            int intDecimalPlaces = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetMaxNuyenDecimalsAsync(token).ConfigureAwait(false);
                            using (ThreadSafeForm<SelectNumber> frmPickNumber
                                   = await ThreadSafeForm<SelectNumber>.GetAsync(() =>
                                       new SelectNumber(intDecimalPlaces)
                                       {
                                           Minimum = decMin,
                                           Maximum = decMax,
                                           Description = strDescription,
                                           AllowCancel = false
                                       }, token).ConfigureAwait(false))
                            {
                                if (await frmPickNumber.ShowDialogSafeAsync(_objCharacter, token)
                                        .ConfigureAwait(false) == DialogResult.Cancel)
                                {
                                    _guiID = Guid.Empty;
                                    return;
                                }

                                _strCost = frmPickNumber.MyForm.SelectedValue.ToString(GlobalSettings
                                    .InvariantCultureInfo);
                            }
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

            if (!blnForSelectForm && GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    Notes = CommonFunctions.GetBookNotes(objXmlWeapon, Name, CurrentDisplayName, Source,
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        Page, DisplayPage(GlobalSettings.Language), _objCharacter, token);
                else
                    await SetNotesAsync(await CommonFunctions.GetBookNotesAsync(objXmlWeapon, Name,
                        await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter,
                        token).ConfigureAwait(false), token).ConfigureAwait(false);
            }

            XmlDocument objXmlDocument = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("weapons.xml", token: token)
                : await _objCharacter.LoadDataAsync("weapons.xml", token: token).ConfigureAwait(false);

            if (!objXmlWeapon.TryGetStringFieldQuickly("weapontype", ref _strWeaponType))
                _strWeaponType = objXmlDocument
                                     .SelectSingleNodeAndCacheExpressionAsNavigator(
                                         "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@type", token)
                                     ?.Value
                                 ?? Category.ToLowerInvariant();

            // Populate the Range if it differs from the Weapon's Category.
            XmlElement objRangeNode = objXmlWeapon["range"];
            if (objRangeNode != null)
            {
                _strRange = objRangeNode.InnerText;
                string strMultiply = objRangeNode.Attributes["multiply"]?.InnerText;
                if (!string.IsNullOrEmpty(strMultiply))
                {
                    decimal.TryParse(strMultiply, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _decRangeMultiplier);
                }
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
                        XmlNode objXmlWeaponNode =
                            objXmlDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", objXmlUnderbarrel.InnerText);
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverload
                            objUnderbarrelWeapon.Create(objXmlWeaponNode, lstWeapons, true, blnCreateImprovements,
                                blnSkipCost, blnForSelectForm: blnForSelectForm, token: token);
                        else
                            await objUnderbarrelWeapon.CreateAsync(objXmlWeaponNode, lstWeapons, true,
                                blnCreateImprovements, blnSkipCost, blnForSelectForm: blnForSelectForm, token: token).ConfigureAwait(false);
                        if (!AllowAccessory)
                            objUnderbarrelWeapon.AllowAccessory = false;
                        objUnderbarrelWeapon.ParentID = InternalId;
                        objUnderbarrelWeapon.Cost = "0";
                        objUnderbarrelWeapon.IncludedInWeapon = true;
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            _lstUnderbarrel.Add(objUnderbarrelWeapon);
                        else
                            await _lstUnderbarrel.AddAsync(objUnderbarrelWeapon, token).ConfigureAwait(false);
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
                        XmlNode objXmlAccessory =
                            objXmlDocument.TryGetNodeByNameOrId("/chummer/accessories/accessory", strName);
                        WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
                        int intAccessoryRating = 0;
                        if (objXmlWeaponAccessory["rating"] != null)
                        {
                            intAccessoryRating = Convert.ToInt32(objXmlWeaponAccessory["rating"].InnerText,
                                GlobalSettings.InvariantCultureInfo);
                        }

                        if (blnSync)
                        {
                            if (objXmlWeaponAccessory.InnerXml.Contains("mount"))
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                objAccessory.Create(objXmlAccessory,
                                    objXmlWeaponAccessory.InnerXml.Contains("<extramount>")
                                        ? new Tuple<string, string>(objXmlAccessory["mount"].InnerText,
                                            objXmlAccessory["extramount"].InnerText)
                                        : new Tuple<string, string>(objXmlAccessory["mount"].InnerText, "None"),
                                    intAccessoryRating, false, blnCreateChildren, blnCreateImprovements, token);
                            }
                            else
                            {
                                // ReSharper disable once MethodHasAsyncOverload
                                objAccessory.Create(objXmlAccessory, new Tuple<string, string>("Internal", "None"),
                                    intAccessoryRating, false, blnCreateChildren, blnCreateImprovements, token);
                            }
                        }
                        else if (objXmlWeaponAccessory.InnerXml.Contains("mount"))
                        {
                            await objAccessory.CreateAsync(objXmlAccessory,
                                    objXmlWeaponAccessory.InnerXml.Contains("<extramount>")
                                        ? new Tuple<string, string>(objXmlAccessory["mount"].InnerText,
                                            objXmlAccessory["extramount"].InnerText)
                                        : new Tuple<string, string>(objXmlAccessory["mount"].InnerText, "None"),
                                    intAccessoryRating, false, blnCreateChildren, blnCreateImprovements, token)
                                .ConfigureAwait(false);
                        }
                        else
                        {
                            await objAccessory.CreateAsync(objXmlAccessory,
                                    new Tuple<string, string>("Internal", "None"),
                                    intAccessoryRating, false, blnCreateChildren, blnCreateImprovements, token)
                                .ConfigureAwait(false);
                        }

                        // Add any extra Gear that comes with the Weapon Accessory.
                        XmlElement xmlGearsNode = objXmlWeaponAccessory["gears"];
                        if (xmlGearsNode != null)
                        {
                            XmlDocument objXmlGearDocument = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? _objCharacter.LoadData("gear.xml", token: token)
                                : await _objCharacter.LoadDataAsync("gear.xml", token: token).ConfigureAwait(false);
                            foreach (XmlNode objXmlAccessoryGear in xmlGearsNode.SelectNodes("usegear"))
                            {
                                XmlElement objXmlAccessoryGearName = objXmlAccessoryGear["name"];
                                XmlElement objXmlAccessoryGearCategory = objXmlAccessoryGear["category"];
                                XmlAttributeCollection objXmlAccessoryGearNameAttributes =
                                    objXmlAccessoryGearName.Attributes;
                                
                                string strChildForceSource = objXmlAccessoryGear["source"]?.InnerText ?? string.Empty;
                                string strChildForcePage = objXmlAccessoryGear["page"]?.InnerText ?? string.Empty;
                                string strChildForceValue = objXmlAccessoryGearNameAttributes?["select"]?.InnerText ??
                                                            string.Empty;
                                bool blnChildCreateChildren =
                                    objXmlAccessoryGearNameAttributes?["createchildren"]?.InnerText != bool.FalseString;
                                bool blnAddChildImprovements = blnCreateImprovements &&
                                                               objXmlAccessoryGearNameAttributes?["addimprovements"]
                                                                   ?.InnerText != bool.FalseString;
                                int intGearRating = 0;
                                if (objXmlAccessoryGear["rating"] != null
                                    && !int.TryParse(objXmlAccessoryGear["rating"].InnerText, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out intGearRating))
                                {
                                    intGearRating = 0;
                                }
                                decimal decGearQty = 1;
                                if (objXmlAccessoryGearNameAttributes?["qty"] != null
                                    && !decimal.TryParse(objXmlAccessoryGearNameAttributes["qty"].InnerText, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decGearQty))
                                {
                                    decGearQty = 1;
                                }
                                string strFilter = "/chummer/gears/gear";
                                if (objXmlAccessoryGearName != null || objXmlAccessoryGearCategory != null)
                                {
                                    strFilter += '[';
                                    if (objXmlAccessoryGearName != null)
                                    {
                                        strFilter += "name = " + objXmlAccessoryGearName.InnerText.CleanXPath();
                                        if (objXmlAccessoryGearCategory != null)
                                            strFilter += " and category = " +
                                                         objXmlAccessoryGearCategory.InnerText.CleanXPath();
                                    }
                                    else
                                        strFilter += "category = " + objXmlAccessoryGearCategory.InnerText.CleanXPath();

                                    strFilter += ']';
                                }

                                XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode(strFilter);
                                Gear objGear = new Gear(_objCharacter);

                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objGear.Create(objXmlGear, intGearRating, lstWeapons, strChildForceValue,
                                        blnAddChildImprovements, blnChildCreateChildren, token: token);
                                    objGear.Quantity = decGearQty;
                                }
                                else
                                {
                                    await objGear.CreateAsync(objXmlGear, intGearRating, lstWeapons, strChildForceValue,
                                            blnAddChildImprovements, blnChildCreateChildren, token: token)
                                        .ConfigureAwait(false);
                                    await objGear.SetQuantityAsync(decGearQty, token).ConfigureAwait(false);
                                }

                                objGear.Cost = "0";
                                objGear.ParentID = InternalId;

                                if (!string.IsNullOrEmpty(strChildForceSource))
                                    objGear.Source = strChildForceSource;
                                if (!string.IsNullOrEmpty(strChildForcePage))
                                    objGear.Page = strChildForcePage;
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objAccessory.GearChildren.Add(objGear);
                                else
                                    await objAccessory.GearChildren.AddAsync(objGear, token).ConfigureAwait(false);

                                // Change the Capacity of the child if necessary.
                                if (objXmlAccessoryGear["capacity"] != null)
                                    objGear.Capacity = '[' + objXmlAccessoryGear["capacity"].InnerText + ']';
                            }
                        }

                        objAccessory.IncludedInWeapon = true;
                        objAccessory.Parent = this;
                        if (blnSync)
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            _lstAccessories.Add(objAccessory);
                        else
                            await _lstAccessories.AddAsync(objAccessory, token).ConfigureAwait(false);
                    }
                }
            }

            if (blnCreateImprovements)
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    RefreshWirelessBonuses();
                else
                    await RefreshWirelessBonusesAsync(token).ConfigureAwait(false);
            }

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
                    Weapon objSubWeapon = new Weapon(_objCharacter);
                    int intAddWeaponRating = 0;
                    string strRating = objXmlAddWeapon.Attributes["rating"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRating))
                    {
                        intAddWeaponRating = Convert.ToInt32(blnSync
                                ? strRating
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    .CheapReplace(
                                        "{Rating}",
                                        () => Rating.ToString(
                                            GlobalSettings
                                                .InvariantCultureInfo))
                                : await strRating
                                    .CheapReplaceAsync(
                                        "{Rating}",
                                        async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(
                                            GlobalSettings
                                                .InvariantCultureInfo), token: token).ConfigureAwait(false),
                            GlobalSettings.InvariantCultureInfo);
                    }

                    if (blnSync)
                        // ReSharper disable once MethodHasAsyncOverload
                        objSubWeapon.Create(objXmlSubWeapon, lstWeapons, blnCreateChildren, blnCreateImprovements,
                            blnSkipCost, intAddWeaponRating, blnForSelectForm, token);
                    else
                        await objSubWeapon.CreateAsync(objXmlSubWeapon, lstWeapons, blnCreateChildren,
                            blnCreateImprovements,
                            blnSkipCost, intAddWeaponRating, blnForSelectForm, token).ConfigureAwait(false);
                    objSubWeapon.ParentID = InternalId;
                    objSubWeapon.Cost = "0";
                    lstWeapons.Add(objSubWeapon);
                }
            }

            foreach (Weapon objLoopWeapon in lstWeapons)
            {
                if (blnSync)
                    objLoopWeapon.ParentVehicle = ParentVehicle;
                else
                    await objLoopWeapon.SetParentVehicleAsync(ParentVehicle, token).ConfigureAwait(false);
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
            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("spec", _strSpec);
            objWriter.WriteElementString("spec2", _strSpec2);
            objWriter.WriteElementString("reach", _strReach);
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

            objWriter.WriteElementString("activeammoslot",
                _intActiveAmmoSlot.ToString(GlobalSettings.InvariantCultureInfo));
            if (_lstAmmo.Exists(x => x.AmmoGear != null || x.Ammo != 0))
            {
                objWriter.WriteStartElement("clips");
                foreach (Clip clip in _lstAmmo)
                {
                    clip.Save(objWriter);
                }

                objWriter.WriteEndElement();
            }

            objWriter.WriteElementString("conceal", _strConceal);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("weight", _strWeight);
            objWriter.WriteElementString("useskill", _strUseSkill);
            objWriter.WriteElementString("useskillspec", _strUseSkillSpec);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("alternaterange", _strAlternateRange);
            objWriter.WriteElementString("rangemultiply",
                _decRangeMultiplier.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("singleshot", _intSingleShot.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("shortburst", _intShortBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("longburst", _intLongBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("fullburst", _intFullBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("suppressive", _intSuppressive.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowsingleshot",
                _blnAllowSingleShot.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowshortburst",
                _blnAllowShortBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowlongburst",
                _blnAllowLongBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowfullburst",
                _blnAllowFullBurst.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("allowsuppressive",
                _blnAllowSuppressive.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("allowaccessory",
                _blnAllowAccessory.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("weaponname", _strWeaponName);
            objWriter.WriteElementString("included",
                _blnIncludedInWeapon.ToString(GlobalSettings.InvariantCultureInfo));
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
            objWriter.WriteElementString("notes", _strNotes.CleanOfXmlInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost",
                _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("weaponslots", _strWeaponSlots);
            objWriter.WriteElementString("doubledcostweaponslots", _strDoubledCostWeaponSlots);

            objWriter.WriteElementString("active",
                this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("homenode",
                this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
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
            objWriter.WriteElementString("canswapattributes",
                _blnCanSwapAttributes.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmfilled",
                _intMatrixCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            if (_nodWirelessWeaponBonus != null)
                objWriter.WriteRaw(_nodWirelessWeaponBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessweaponbonus", string.Empty);
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("weapontype", _strWeaponType);
            objWriter.WriteEndElement();
        }

        private static readonly ReadOnlyCollection<string> s_OldClipValues = Array.AsReadOnly(new[]
            { string.Empty, "2", "3", "4" });

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
            if (ammoString.IndexOfAny('×', 'x', '+') != -1 ||
                ammoString.Contains(" or ", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("Special", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("External Source", StringComparison.OrdinalIgnoreCase))
            {
                string strWeaponAmmo = ammoString.FastEscape("External Source", StringComparison.OrdinalIgnoreCase);
                strWeaponAmmo = strWeaponAmmo.ToLowerInvariant();
                // Get rid of or belt, and + energy.
                strWeaponAmmo = strWeaponAmmo.FastEscapeOnceFromEnd(" + energy", eComparison: StringComparison.OrdinalIgnoreCase)
                    .Replace(" or belt", " or 250(belt)");

                foreach (string strAmmo in strWeaponAmmo.SplitNoAlloc(" or ", StringSplitOptions.RemoveEmptyEntries, StringComparison.OrdinalIgnoreCase))
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
                if (int.TryParse(strAmmo, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                        out int intNewAmmoCount))
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
        private async Task RecreateInternalClipAsync(CancellationToken token = default)
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
            string ammoString =
                await CalculatedAmmoAsync(GlobalSettings.CultureInfo, GlobalSettings.DefaultLanguage, token)
                    .ConfigureAwait(false);
            // Determine which loading methods are available to the Weapon.
            if (ammoString.IndexOfAny('×', 'x', '+') != -1 ||
                ammoString.Contains(" or ", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("Special", StringComparison.OrdinalIgnoreCase) ||
                ammoString.Contains("External Source", StringComparison.OrdinalIgnoreCase))
            {
                string strWeaponAmmo = ammoString.FastEscape("External Source", StringComparison.OrdinalIgnoreCase);
                strWeaponAmmo = strWeaponAmmo.ToLowerInvariant();
                // Get rid of or belt, and + energy.
                strWeaponAmmo = strWeaponAmmo.FastEscapeOnceFromEnd(" + energy", eComparison: StringComparison.OrdinalIgnoreCase)
                    .Replace(" or belt", " or 250(belt)");

                foreach (string strAmmo in strWeaponAmmo.SplitNoAlloc(" or ", StringSplitOptions.RemoveEmptyEntries, StringComparison.OrdinalIgnoreCase))
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
                if (int.TryParse(strAmmo, NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                        out int intNewAmmoCount))
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
            objNode.TryGetStringFieldQuickly("reach", ref _strReach);
            objNode.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
            objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            // Legacy catch for if a damage expression is not empty but has no attributes associated with it.
            if (_objCharacter.LastSavedVersion < new ValueVersion(5, 214, 98) && !string.IsNullOrEmpty(_strDamage) &&
                !_strDamage.Contains('{') && AttributeSection.AttributeStrings.Any(x => _strDamage.Contains(x)))
            {
                objMyNode.Value?.TryGetStringFieldQuickly("damage", ref _strDamage);
            }

            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            // Needed for legacy reasons
            _intRating = Math.Max(Math.Min(_intRating, MaxRatingValue), MinRatingValue);
            if (objNode["firingmode"] != null)
                _eFiringMode = ConvertToFiringMode(objNode["firingmode"].InnerText);
            // Legacy shim
            if (Name.Contains("Osmium Mace (STR"))
            {
                XmlNode objNewOsmiumMaceNode = _objCharacter.LoadData("weapons.xml")
                    .SelectSingleNode("/chummer/weapons/weapon[name = \"Osmium Mace\"]");
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
            objNode.TryGetStringFieldQuickly("conceal", ref _strConceal);
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

            XmlElement xmlAccessoriesNode = objNode["accessories"];
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
                    XmlElement clipNode = objNode["clips"];
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
                    XmlElement clipNode = objNode["clips"];
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
                                : _objCharacter.Gear.DeepFindById(guid.ToString("D",
                                    GlobalSettings.InvariantCultureInfo));
                            // Load clips into weapon slots first
                            if (i < _intAmmoSlots)
                            {
                                _lstAmmo.Add(new Clip(_objCharacter, null, this, objGear, intAmmo));
                                ++i;
                            }
                            // Then load clips into accessory-provided slots
                            else if (i < _intAmmoSlots + lstWeaponAccessoriesWithClipSlots.Count)
                            {
                                _lstAmmo.Add(new Clip(_objCharacter,
                                    lstWeaponAccessoriesWithClipSlots[i - _intAmmoSlots],
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
                        _lstAmmo.Add(new Clip(_objCharacter, lstWeaponAccessoriesWithClipSlots[i - _intAmmoSlots], this,
                            null, 0));
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
                        Clip objLoopClip = Clip.Load(node, _objCharacter, this,
                            lstWeaponAccessoriesWithClipSlots[i - _intAmmoSlots]);
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
                    _lstAmmo.Add(new Clip(_objCharacter, lstWeaponAccessoriesWithClipSlots[i - _intAmmoSlots], this,
                        null, 0));
                }
            }

            _nodWirelessBonus = objNode["wirelessbonus"];
            _nodWirelessWeaponBonus = objNode["wirelessweaponbonus"];
            // Legacy sweep
            if (_objCharacter.LastSavedVersion < new ValueVersion(5, 225, 933) && _nodWirelessWeaponBonus == null)
            {
                _nodWirelessWeaponBonus = objMyNode.Value?["wirelessweaponbonus"];
            }
            objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn);

            //#1544 Ammunition not loading or available.
            if (_strUseSkill == "Throwing Weapons"
                && _strAmmo != "1")
            {
                _strAmmo = "1";
            }

            XmlElement xmlUnderbarrelNode = objNode["underbarrel"];
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
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
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
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
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
        public async Task Print(XmlWriter objWriter, CultureInfo objCulture, string strLanguageToPrint,
            CancellationToken token = default)
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
                                                        .DeepFirstOrDefaultAsync(x => x.GearChildren,
                                                            x => x.InternalId == ParentID,
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
                                                .DeepFirstOrDefaultAsync(x => x.GearChildren,
                                                    x => x.InternalId == ParentID, token: token)
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
                                                            .DeepFirstOrDefaultAsync(x => x.GearChildren,
                                                                x => x.InternalId == ParentID,
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
                                                            .DeepFirstOrDefaultAsync(x => x.GearChildren,
                                                                x => x.InternalId == ParentID,
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
                                            if (await objVehicleMod.Cyberware.GetCountAsync(token)
                                                    .ConfigureAwait(false) ==
                                                0
                                                && await objVehicleMod.Weapons.GetCountAsync(token)
                                                    .ConfigureAwait(false) ==
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
                                                    .DeepFirstOrDefaultAsync(x => x.GearChildren,
                                                        x => x.InternalId == ParentID, token: token)
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
                                                                .DeepFirstOrDefaultAsync(x => x.GearChildren,
                                                                    x => x.InternalId == ParentID,
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
            XmlElementWriteHelper objBaseElement =
                await objWriter.StartElementAsync("weapon", token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name",
                        await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname",
                        await DisplayNameAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname_english",
                    await DisplayNameAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token)
                        .ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category",
                        await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("type", RangeType, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("reach",
                        (await GetTotalReachAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawreach", Reach, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("accuracy",
                        await GetAccuracyAsync(objCulture, strLanguageToPrint, token: token).ConfigureAwait(false),
                        token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("accuracy_noammo",
                    await GetAccuracyAsync(objCulture, strLanguageToPrint, false, token: token).ConfigureAwait(false),
                    token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("accuracy_english",
                    await GetAccuracyAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                        token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("accuracy_english_noammo",
                    await GetAccuracyAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, false,
                        token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawaccuracy", Accuracy, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("damage",
                    await CalculatedDamageAsync(objCulture, strLanguageToPrint, token: token).ConfigureAwait(false),
                    token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("damage_noammo",
                    await CalculatedDamageAsync(objCulture, strLanguageToPrint, false, token: token)
                        .ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("damage_english",
                    await CalculatedDamageAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                        token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("damage_noammo_english",
                    await CalculatedDamageAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                        false, token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawdamage", Damage, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ap",
                        await TotalAPAsync(objCulture, strLanguageToPrint, token: token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ap_noammo",
                    await TotalAPAsync(objCulture, strLanguageToPrint, false, token: token).ConfigureAwait(false),
                    token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ap_english",
                    await TotalAPAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                        token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ap_english_noammo",
                    await TotalAPAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, false,
                        token: token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawap", AP, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("mode",
                        await CalculatedModeAsync(strLanguageToPrint, token: token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("mode_noammo",
                        await CalculatedModeAsync(strLanguageToPrint, false, token: token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("mode_english",
                    await CalculatedModeAsync(GlobalSettings.DefaultLanguage, token: token).ConfigureAwait(false),
                    token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("mode_english_noammo",
                    await CalculatedModeAsync(GlobalSettings.DefaultLanguage, false, token: token)
                        .ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rc",
                        (await TotalRCAsync(objCulture, strLanguageToPrint, token: token).ConfigureAwait(false)).Item1, token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rc_noammo",
                    (await TotalRCAsync(objCulture, strLanguageToPrint, blnIncludeAmmo: false, token: token)
                        .ConfigureAwait(false)).Item1, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rc_english",
                    (await TotalRCAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                        token: token).ConfigureAwait(false)).Item1, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rc_english_noammo",
                    (await TotalRCAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                        blnIncludeAmmo: false, token: token).ConfigureAwait(false)).Item1, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawrc", RC, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ammo",
                        await CalculatedAmmoAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("ammo_english",
                    await CalculatedAmmoAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                        token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("maxammo", Ammo, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("conceal",
                        (await CalculatedConcealabilityAsync(token).ConfigureAwait(false)).ToString("+0;-0;0", objCulture), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("rawconceal", Concealability, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("availablemounts", await DisplayAccessoryMountsAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("availablemounts_english", await DisplayAccessoryMountsAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                if (objGear != null)
                {
                    await objWriter.WriteElementStringAsync("avail",
                        await objGear.TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false),
                        token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("avail_english",
                        await objGear
                            .TotalAvailAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage, token)
                            .ConfigureAwait(false), token).ConfigureAwait(false);
                    string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("cost",
                        (await objGear.GetTotalCostAsync(token).ConfigureAwait(false)).ToString(
                            strNuyenFormat, objCulture), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("owncost",
                        (await objGear.GetOwnCostAsync(token).ConfigureAwait(false)).ToString(
                            strNuyenFormat, objCulture), token).ConfigureAwait(false);
                    string strWeightFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetWeightFormatAsync(token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("weight",
                            objGear.TotalWeight.ToString(strWeightFormat, objCulture), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("ownweight",
                            objGear.OwnWeight.ToString(strWeightFormat, objCulture), token)
                        .ConfigureAwait(false);
                }
                else
                {
                    await objWriter.WriteElementStringAsync("avail",
                            await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("avail_english",
                        await TotalAvailAsync(GlobalSettings.InvariantCultureInfo, GlobalSettings.DefaultLanguage,
                            token).ConfigureAwait(false), token).ConfigureAwait(false);
                    string strNuyenFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetNuyenFormatAsync(token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("cost",
                        (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(
                            strNuyenFormat, objCulture), token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("owncost",
                        (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(
                            strNuyenFormat, objCulture), token).ConfigureAwait(false);
                    string strWeightFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetWeightFormatAsync(token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("weight",
                            TotalWeight.ToString(strWeightFormat, objCulture), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("ownweight",
                            OwnWeight.ToString(strWeightFormat, objCulture), token)
                        .ConfigureAwait(false);
                }

                await objWriter.WriteElementStringAsync("source",
                    await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false),
                    token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("page",
                        await DisplayPageAsync(strLanguageToPrint, token: token).ConfigureAwait(false), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("weaponname", CustomName, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("location",
                    Location != null
                        ? await Location.DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false)
                        : string.Empty, token).ConfigureAwait(false);

                await objWriter.WriteElementStringAsync("attack",
                    (await this.GetTotalMatrixAttributeAsync("Attack", token).ConfigureAwait(false)).ToString(
                        objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sleaze",
                    (await this.GetTotalMatrixAttributeAsync("Sleaze", token).ConfigureAwait(false)).ToString(
                        objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("dataprocessing",
                    (await this.GetTotalMatrixAttributeAsync("Data Processing", token).ConfigureAwait(false)).ToString(
                        objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("firewall",
                    (await this.GetTotalMatrixAttributeAsync("Firewall", token).ConfigureAwait(false)).ToString(
                        objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("devicerating",
                    (await this.GetTotalMatrixAttributeAsync("Device Rating", token).ConfigureAwait(false)).ToString(
                        objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("programlimit",
                    (await this.GetTotalMatrixAttributeAsync("Program Limit", token).ConfigureAwait(false)).ToString(
                        objCulture), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("iscommlink",
                    (await GetIsCommlinkAsync(token).ConfigureAwait(false)).ToString(
                        GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter
                    .WriteElementStringAsync("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo),
                        token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("active",
                    (await this.IsActiveCommlinkAsync(_objCharacter, token).ConfigureAwait(false)).ToString(
                        GlobalSettings.InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("homenode",
                    (await this.IsHomeNodeAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings
                        .InvariantCultureInfo), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("conditionmonitor", MatrixCM.ToString(objCulture), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("matrixcmfilled", MatrixCMFilled.ToString(objCulture), token)
                    .ConfigureAwait(false);

                if (await WeaponAccessories.GetCountAsync(token).ConfigureAwait(false) > 0)
                {
                    // <accessories>
                    XmlElementWriteHelper objAccessoriesElement =
                        await objWriter.StartElementAsync("accessories", token).ConfigureAwait(false);
                    try
                    {
                        await WeaponAccessories.ForEachAsync(
                                x => x.Print(objWriter, objCulture, strLanguageToPrint, token), token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        // </accessories>
                        await objAccessoriesElement.DisposeAsync().ConfigureAwait(false);
                    }
                }

                Dictionary<string, string> dicRanges =
                    await GetRangeStringsAsync(objCulture, true, token: token).ConfigureAwait(false);

                Dictionary<string, string> dicRangesNoAmmo =
                    await GetRangeStringsAsync(objCulture, false, token).ConfigureAwait(false);
                bool blnRangeAmmoChange = !CommonFunctions.DictionaryValuesEqual(dicRanges, dicRangesNoAmmo);

                // <ranges>
                XmlElementWriteHelper objRangesElement =
                    await objWriter.StartElementAsync("ranges", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("name",
                            await DisplayRangeAsync(strLanguageToPrint, blnRangeAmmoChange, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english",
                            await DisplayRangeAsync(GlobalSettings.DefaultLanguage, false, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("short", dicRanges["short"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("medium", dicRanges["medium"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("long", dicRanges["long"], token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("extreme", dicRanges["extreme"], token)
                        .ConfigureAwait(false);
                }
                finally
                {
                    // </ranges>
                    await objRangesElement.DisposeAsync().ConfigureAwait(false);
                }

                // <alternateranges>
                XmlElementWriteHelper objAlternateRangesElement =
                    await objWriter.StartElementAsync("alternateranges", token).ConfigureAwait(false);
                try
                {
                    await objWriter.WriteElementStringAsync("name",
                            await DisplayAlternateRangeAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("name_english",
                        await DisplayAlternateRangeAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false),
                        token).ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("short", dicRanges["alternateshort"], token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("medium", dicRanges["alternatemedium"], token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("long", dicRanges["alternatelong"], token)
                        .ConfigureAwait(false);
                    await objWriter.WriteElementStringAsync("extreme", dicRanges["alternateextreme"], token)
                        .ConfigureAwait(false);
                }
                finally
                {
                    // </alternateranges>
                    await objAlternateRangesElement.DisposeAsync().ConfigureAwait(false);
                }

                // Only include the unloaded range numbers if the dictionary values differ
                if (blnRangeAmmoChange)
                {
                    // <ranges>
                    XmlElementWriteHelper objRangesNoAmmoElement =
                    await objWriter.StartElementAsync("ranges", token).ConfigureAwait(false);
                    try
                    {
                        await objWriter.WriteElementStringAsync("name",
                                await DisplayRangeAsync(strLanguageToPrint, false, token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("name_english",
                                await DisplayRangeAsync(GlobalSettings.DefaultLanguage, true, token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("short", dicRangesNoAmmo["short"], token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("medium", dicRangesNoAmmo["medium"], token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("long", dicRangesNoAmmo["long"], token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("extreme", dicRangesNoAmmo["extreme"], token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        // </ranges>
                        await objRangesNoAmmoElement.DisposeAsync().ConfigureAwait(false);
                    }

                    // <alternateranges>
                    XmlElementWriteHelper objAlternateRangesNoAmmoElement =
                        await objWriter.StartElementAsync("alternateranges", token).ConfigureAwait(false);
                    try
                    {
                        await objWriter.WriteElementStringAsync("name",
                                await DisplayAlternateRangeAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("name_english",
                            await DisplayAlternateRangeAsync(GlobalSettings.DefaultLanguage, token).ConfigureAwait(false),
                            token).ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("short", dicRangesNoAmmo["alternateshort"], token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("medium", dicRangesNoAmmo["alternatemedium"], token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("long", dicRangesNoAmmo["alternatelong"], token)
                            .ConfigureAwait(false);
                        await objWriter.WriteElementStringAsync("extreme", dicRangesNoAmmo["alternateextreme"], token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        // </alternateranges>
                        await objAlternateRangesNoAmmoElement.DisposeAsync().ConfigureAwait(false);
                    }
                }

                await Children.ForEachAsync(async objUnderbarrel =>
                {
                    // <underbarrel>
                    XmlElementWriteHelper objUnderbarrelElement =
                        await objWriter.StartElementAsync("underbarrel", token).ConfigureAwait(false);
                    try
                    {
                        await objUnderbarrel.Print(objWriter, objCulture, strLanguageToPrint, token: token)
                            .ConfigureAwait(false);
                    }
                    finally
                    {
                        // </underbarrel>
                        await objUnderbarrelElement.DisposeAsync().ConfigureAwait(false);
                    }
                }, token).ConfigureAwait(false);

                // Currently loaded Ammo.
                Clip objLoadedClip = GetClip(_intActiveAmmoSlot);
                await objWriter.WriteElementStringAsync("availableammo", GetAvailableAmmo.ToString(objCulture), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("currentammo",
                        await objLoadedClip.DisplayAmmoNameAsync(strLanguageToPrint, token).ConfigureAwait(false),
                        token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("currentammo_english",
                    await objLoadedClip.DisplayAmmoNameAsync(GlobalSettings.DefaultLanguage, token)
                        .ConfigureAwait(false), token).ConfigureAwait(false);

                // <clips>
                XmlElementWriteHelper objClipsElement =
                    await objWriter.StartElementAsync("clips", token).ConfigureAwait(false);
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

                await objWriter.WriteElementStringAsync("dicepool",
                        (await GetDicePoolAsync(token: token).ConfigureAwait(false)).ToString(objCulture), token)
                    .ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("dicepool_noammo",
                        (await GetDicePoolAsync(false, token: token).ConfigureAwait(false)).ToString(objCulture), token)
                    .ConfigureAwait(false);
                Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                await objWriter
                    .WriteElementStringAsync(
                        "skill",
                        objSkill != null
                            ? await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                            : string.Empty, token).ConfigureAwait(false);

                await objWriter
                    .WriteElementStringAsync("wirelesson", WirelessOn.ToString(GlobalSettings.InvariantCultureInfo),
                        token).ConfigureAwait(false);
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", await GetNotesAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
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
        public TaggedObservableCollection<WeaponAccessory> WeaponAccessories => _lstAccessories;

        /// <summary>
        /// Underbarrel Weapon.
        /// </summary>
        public TaggedObservableCollection<Weapon> UnderbarrelWeapons => _lstUnderbarrel;

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

        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            return objNode != null
                ? objNode.SelectSingleNodeAndCacheExpression("translate", token: token)?.Value ?? Name
                : Name;
        }

        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) =>
            DisplayNameShortAsync(GlobalSettings.Language, token);

        public string CurrentDisplayName => DisplayName(GlobalSettings.CultureInfo, GlobalSettings.Language);

        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) =>
            DisplayNameAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Display name.
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
                strReturn += strSpace + '(' + LanguageManager.GetString(RatingLabel, strLanguage) + strSpace +
                             intRating.ToString(objCulture) + ')';
            }
            if (!string.IsNullOrEmpty(CustomName))
                strReturn += strSpace + "(\"" + CustomName + "\")";
            return strReturn;
        }

        /// <summary>
        /// Display name.
        /// </summary>
        public async Task<string> DisplayNameAsync(CultureInfo objCulture, string strLanguage,
            CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);
            string strSpace = await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                .ConfigureAwait(false);
            int intRating = await GetRatingAsync(token).ConfigureAwait(false);
            if (intRating > 0)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.CultureInfo;
                strReturn += strSpace + '(' +
                             await LanguageManager.GetStringAsync(RatingLabel, strLanguage, token: token)
                                 .ConfigureAwait(false) + strSpace + intRating.ToString(objCulture) + ')';
            }
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
            get => Math.Max(Math.Min(_intRating, MaxRatingValue), MinRatingValue);
            set
            {
                value = Math.Max(Math.Min(value, MaxRatingValue), MinRatingValue);
                if (Interlocked.Exchange(ref _intRating, value) == value)
                    return;
                if (Equipped && ParentVehicle == null
                    && (Weight.ContainsAny("FixedValues", "Rating")
                        || Children.DeepAny(x => x.Children, x => x.Equipped && x.Weight.Contains("Parent Rating"))
                        || WeaponAccessories.Any(x => x.Equipped && x.Weight.Contains("Parent Rating"))))
                {
                    bool blnDoPropertyChange = true;
                    Weapon objWeapon = Parent;
                    for (Weapon objParent = objWeapon.Parent; objParent != null; objParent = objWeapon.Parent)
                    {
                        objWeapon = objParent;
                        if (!objWeapon.Equipped || objWeapon.ParentVehicle != null)
                        {
                            blnDoPropertyChange = false;
                            break;
                        }
                    }
                    if (blnDoPropertyChange)
                    {
                        _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
                    }
                }
                if (Children.Count > 0)
                {
                    foreach (Weapon objChild in Children)
                    {
                        if (!objChild.MaxRating.Contains("Parent") && objChild.MinRating.Contains("Parent"))
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
            return Math.Max(Math.Min(_intRating, await GetMaxRatingValueAsync(token).ConfigureAwait(false)),
                await GetMinRatingValueAsync(token).ConfigureAwait(false));
        }

        public async Task SetRatingAsync(int value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            value = Math.Max(Math.Min(value, await GetMaxRatingValueAsync(token).ConfigureAwait(false)),
                           await GetMinRatingValueAsync(token).ConfigureAwait(false));
            if (Interlocked.Exchange(ref _intRating, value) == value)
                return;
            if (Equipped && ParentVehicle == null
                && (Weight.ContainsAny("FixedValues", "Rating")
                    || await Children.DeepAnyAsync(x => x.Children, x => x.Equipped && x.Weight.Contains("Parent Rating"), token).ConfigureAwait(false)
                    || await WeaponAccessories.AnyAsync(x => x.Equipped && x.Weight.Contains("Parent Rating"), token).ConfigureAwait(false)))
            {
                bool blnDoPropertyChange = true;
                Weapon objWeapon = Parent;
                for (Weapon objParent = objWeapon.Parent; objParent != null; objParent = objWeapon.Parent)
                {
                    objWeapon = objParent;
                    if (!objWeapon.Equipped || objWeapon.ParentVehicle != null)
                    {
                        blnDoPropertyChange = false;
                        break;
                    }
                }
                if (blnDoPropertyChange)
                {
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token).ConfigureAwait(false);
                }
            }
            if (await Children.CountAsync(token).ConfigureAwait(false) > 0)
            {
                await Children.ForEachAsync(async objChild =>
                {
                    if (!objChild.MaxRating.Contains("Parent") && objChild.MinRating.Contains("Parent"))
                        return;
                    // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                    int intCurrentRating = await objChild.GetRatingAsync(token).ConfigureAwait(false);
                    await objChild.SetRatingAsync(intCurrentRating).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            }
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
                return string.IsNullOrEmpty(strExpression) ? 0 : ProcessRatingString(strExpression, _intRating);
            }
            set => MinRating = value.ToString(GlobalSettings.InvariantCultureInfo);
        }

        /// <summary>
        /// Maximum Rating (value form).
        /// </summary>
        public int MaxRatingValue
        {
            get
            {
                string strExpression = MaxRating;
                return string.IsNullOrEmpty(strExpression) ? int.MaxValue : ProcessRatingString(strExpression, _intRating);
            }
            set => MaxRating = value.ToString(GlobalSettings.InvariantCultureInfo);
        }

        /// <summary>
        /// Minimum Rating (value form).
        /// </summary>
        public Task<int> GetMinRatingValueAsync(CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled<int>(token);
            string strExpression = MinRating;
            return string.IsNullOrEmpty(strExpression) ? Task.FromResult(0) : ProcessRatingStringAsync(strExpression, _intRating, token);
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
        /// <param name="strExpression"></param>
        /// <returns></returns>
        private int ProcessRatingString(string strExpression, int intRating)
        {
            strExpression = strExpression.ProcessFixedValuesString(intRating);

            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    sbdValue.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    ProcessAttributesInXPath(sbdValue, strExpression);
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }

            return decValue.StandardRound();
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        /// <param name="strExpression"></param>
        /// <returns></returns>
        private async Task<int> ProcessRatingStringAsync(string strExpression, int intRating, CancellationToken token = default)
        {
            strExpression = strExpression.ProcessFixedValuesString(intRating);

            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    sbdValue.Replace("{Rating}", intRating.ToString(GlobalSettings.InvariantCultureInfo));
                    await ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token).ConfigureAwait(false);
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }

            return decValue.StandardRound();
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
        public async Task<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            switch (Category)
            {
                // If Categories are actually the name of object types, so pull them from the language file.
                case "Gear":
                    return await LanguageManager.GetStringAsync("String_SelectPACKSKit_Gear", strLanguage, token: token)
                        .ConfigureAwait(false);

                case "Cyberware":
                    return await LanguageManager
                        .GetStringAsync("String_SelectPACKSKit_Cyberware", strLanguage, token: token)
                        .ConfigureAwait(false);

                case "Bioware":
                    return await LanguageManager
                        .GetStringAsync("String_SelectPACKSKit_Bioware", strLanguage, token: token)
                        .ConfigureAwait(false);
            }

            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return (await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token)
                    .ConfigureAwait(false))
                .SelectSingleNodeAndCacheExpression(
                    "/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate",
                    token: token)?.Value ?? Category;
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
        public async Task<string> DisplayAmmoCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return AmmoCategory;

            return (await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token)
                    .ConfigureAwait(false))
                .SelectSingleNodeAndCacheExpression(
                    "/chummer/categories/category[. = " + AmmoCategory.CleanXPath() + "]/@translate",
                    token: token)?.Value ?? AmmoCategory;
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
        public string Reach
        {
            get => _strReach;
            set => _strReach = value;
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
                if (objGear.Quantity > intCurrentAmmo - value)
                    objGear.Quantity -= intCurrentAmmo - value;
                else
                    objGear.DeleteGear();
            }
        }

        /// <summary>
        /// The number of rounds remaining in the Weapon.
        /// </summary>
        public Task SetAmmoRemaining(int value, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            Clip objCurrentClip = GetClip(_intActiveAmmoSlot);
            int intCurrentAmmo = objCurrentClip.Ammo;
            if (intCurrentAmmo == value)
                return Task.CompletedTask;
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            objCurrentClip.Ammo = value;
            Gear objGear = objCurrentClip.AmmoGear;
            if (objGear == null)
                return Task.CompletedTask;
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            int intDiff = intCurrentAmmo - value;
            if (objGear.Quantity > intDiff)
                return objGear.SetQuantityAsync(objGear.Quantity - intCurrentAmmo - value, token);
            else
                return objGear.DeleteGearAsync(token: token);
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

        private static readonly Lazy<Regex> s_RgxAmmoCapacityFirst = new Lazy<Regex>(() => new Regex("^[0-9]*[0-9]*x",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled));

        private static readonly Lazy<Regex> s_RgxAmmoCapacitySecond = new Lazy<Regex>(() =>
            new Regex(@"(?<=\))(x[0-9]*[0-9]*$)*",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant |
                RegexOptions.Compiled));

        /// <summary>
        /// The type of Ammunition loaded in the Weapon.
        /// </summary>
        public Gear AmmoLoaded
        {
            get => GetClip(_intActiveAmmoSlot).AmmoGear;
            set => GetClip(_intActiveAmmoSlot).AmmoGear = value;
        }

        /// <summary>
        /// The type of Ammunition loaded in the Weapon.
        /// </summary>
        public Task SetAmmoLoadedAsync(Gear value, CancellationToken token = default)
        {
            return token.IsCancellationRequested
                ? Task.FromCanceled(token)
                : GetClip(_intActiveAmmoSlot).SetAmmoGearAsync(value, token);
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
        public string Concealability
        {
            get => _strConceal;
            set => _strConceal = value;
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

        public async Task<Tuple<string, decimal>> DisplayCost(decimal decMarkup = 0.0m, CancellationToken token = default)
        {
            string strReturn = Cost;
            string strFormat = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                .GetNuyenFormatAsync(token).ConfigureAwait(false);
            string strNuyen = await LanguageManager.GetStringAsync("String_NuyenSymbol", token: token).ConfigureAwait(false);
            if (strReturn.StartsWith("Variable(", StringComparison.Ordinal))
            {
                strReturn = strReturn.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                if (strReturn.Contains('-'))
                {
                    string[] strValues = strReturn.SplitFixedSizePooledArray('-', 2);
                    try
                    {
                        decMin = Convert.ToDecimal(strValues[0], GlobalSettings.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strValues[1], GlobalSettings.InvariantCultureInfo);
                    }
                    finally
                    {
                        ArrayPool<string>.Shared.Return(strValues);
                    }
                }
                else
                    decMin = Convert.ToDecimal(strReturn.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

                if (decMax == decimal.MaxValue)
                    strReturn = decMin.ToString(strFormat, GlobalSettings.CultureInfo) + strNuyen + '+';
                else
                    strReturn = decMin.ToString(strFormat, GlobalSettings.CultureInfo) + " - " +
                                decMax.ToString(strFormat, GlobalSettings.CultureInfo) + strNuyen;

                return new Tuple<string, decimal>(strReturn, decMin);
            }

            decimal.TryParse(strReturn, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out decimal decTotalCost);

            decTotalCost *= 1.0m + decMarkup;

            if (DiscountCost)
                decTotalCost *= 0.9m;

            return new Tuple<string, decimal>(decTotalCost.ToString(strFormat, GlobalSettings.CultureInfo) + strNuyen, decTotalCost);
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

        public Weapon Parent
        {
            get => _objParent;
            set
            {
                if (Interlocked.Exchange(ref _objParent, value) != value)
                {
                    if (value?.ParentVehicle != null)
                    {
                        // Includes ParentVehicle setters
                        ParentMount = value.ParentMount;
                        ParentVehicleMod = value.ParentVehicleMod;
                    }
                    else
                    {
                        // Includes ParentVehicle setters
                        ParentMount = null;
                        ParentVehicleMod = null;
                    }
                }
            }
        }

        public async Task SetParentAsync(Weapon value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Interlocked.Exchange(ref _objParent, value) != value)
            {
                if (value?.ParentVehicle != null)
                {
                    // Includes ParentVehicle setters
                    await SetParentMountAsync(value.ParentMount, token).ConfigureAwait(false);
                    await SetParentVehicleModAsync(value.ParentVehicleMod, token).ConfigureAwait(false);
                }
                else
                {
                    // Includes ParentVehicle setters
                    await SetParentVehicleAsync(null, token).ConfigureAwait(false);
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
        public Location Location { get; set; }

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
        /// Vehicle to which the weapon is mounted (if none, returns null)
        /// </summary>
        public Vehicle ParentVehicle
        {
            get => _objMountedVehicle;
            set
            {
                if (Interlocked.Exchange(ref _objMountedVehicle, value) != value)
                {
                    foreach (WeaponAccessory objAccessory in WeaponAccessories.AsEnumerableWithSideEffects())
                    {
                        foreach (Gear objGear in objAccessory.GearChildren.AsEnumerableWithSideEffects())
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

                foreach (Weapon objChild in Children.AsEnumerableWithSideEffects())
                    objChild.ParentVehicle = value;
            }
        }

        /// <summary>
        /// Vehicle to which the weapon is mounted (if none, returns null)
        /// </summary>
        public async Task SetParentVehicleAsync(Vehicle value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Interlocked.Exchange(ref _objMountedVehicle, value) != value)
            {
                await WeaponAccessories.ForEachWithSideEffectsAsync(async objAccessory =>
                {
                    await objAccessory.GearChildren.ForEachWithSideEffectsAsync(async objGear =>
                    {
                        if (value != null)
                            await objGear.ChangeEquippedStatusAsync(false, token: token).ConfigureAwait(false);
                        else if (Equipped && objGear.Equipped)
                            await objGear.ChangeEquippedStatusAsync(true, token: token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            }

            if (value == null)
            {
                _objWeaponMount = null;
                _objVehicleMod = null;
            }

            await Children.ForEachWithSideEffectsAsync(x => x.SetParentVehicleAsync(value, token), token).ConfigureAwait(false);
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
                    if (value != null)
                    {
                        _objVehicleMod = null;
                        ParentVehicle = value.Parent;
                    }
                    else if (_objVehicleMod == null)
                    {
                        ParentVehicle = null;
                    }
                }

                foreach (Weapon objChild in Children.AsEnumerableWithSideEffects())
                    objChild.ParentMount = value;
            }
        }

        /// <summary>
        /// WeaponMount to which the weapon is mounted (if none, returns null)
        /// </summary>
        public async Task SetParentMountAsync(WeaponMount value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Interlocked.Exchange(ref _objWeaponMount, value) != value)
            {
                if (value != null)
                {
                    _objVehicleMod = null;
                    await SetParentVehicleAsync(value.Parent, token).ConfigureAwait(false);
                }
                else if (_objVehicleMod == null)
                {
                    await SetParentVehicleAsync(null, token).ConfigureAwait(false);
                }
            }

            await Children.ForEachWithSideEffectsAsync(x => x.SetParentMountAsync(value, token), token).ConfigureAwait(false);
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
                    if (value != null)
                    {
                        _objWeaponMount = null;
                        ParentVehicle = value.Parent;
                    }
                    else if (_objWeaponMount == null)
                    {
                        ParentVehicle = null;
                    }
                }

                foreach (Weapon objChild in Children.AsEnumerableWithSideEffects())
                    objChild.ParentVehicleMod = value;
            }
        }

        /// <summary>
        /// VehicleMod to which the weapon is mounted (if none, returns null)
        /// </summary>
        public async Task SetParentVehicleModAsync(VehicleMod value, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (Interlocked.Exchange(ref _objVehicleMod, value) != value)
            {
                if (value != null)
                {
                    _objWeaponMount = null;
                    await SetParentVehicleAsync(value.Parent, token).ConfigureAwait(false);
                }
                else if (_objWeaponMount == null)
                {
                    await SetParentVehicleAsync(null, token).ConfigureAwait(false);
                }
            }

            await Children.ForEachWithSideEffectsAsync(x => x.SetParentVehicleModAsync(value, token), token).ConfigureAwait(false);
        }

        /// <summary>
        /// Whether the Underbarrel Weapon is part of the parent Weapon by default.
        /// </summary>
        public bool IncludedInWeapon
        {
            get => _blnIncludedInWeapon;
            set => _blnIncludedInWeapon = value;
        }

        /// <summary>
        /// Whether the Underbarrel Weapon is installed.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set
            {
                if (_blnEquipped == value)
                    return;
                _blnEquipped = value;
                if (ParentVehicle == null && _objCharacter?.IsLoading == false)
                {
                    if (WeaponAccessories.Any(x => x.Equipped && x.GearChildren.Count > 0))
                    {
                        if (value)
                        {
                            WeaponAccessories.AsEnumerableWithSideEffects().ForEach(objAccessory =>
                            {
                                objAccessory.GearChildren.AsEnumerableWithSideEffects().ForEach(objGear =>
                                {
                                    if (objGear.Equipped)
                                        objGear.ChangeEquippedStatus(objAccessory.Equipped, true);
                                });
                            });
                        }
                        else
                        {
                            WeaponAccessories.AsEnumerableWithSideEffects().ForEach(objAccessory =>
                                objAccessory.GearChildren.AsEnumerableWithSideEffects()
                                    .ForEach(objGear => objGear.ChangeEquippedStatus(false, true)));
                        }
                    }

                    if (Children.Count > 0)
                    {
                        if (value)
                        {
                            foreach (Weapon objChild in Children
                                         .DeepWhere(x => x.Children, x => x.WeaponAccessories.Count > 0).ToList())
                            {
                                bool blnAllParentsEquipped = objChild.Equipped;
                                Weapon objLoopParent = objChild.Parent;
                                while (blnAllParentsEquipped && objLoopParent != null)
                                {
                                    blnAllParentsEquipped = objLoopParent.Equipped;
                                    objLoopParent = objLoopParent.Parent;
                                }

                                if (blnAllParentsEquipped)
                                {
                                    objChild.WeaponAccessories.AsEnumerableWithSideEffects().ForEach(objAccessory =>
                                    {
                                        objAccessory.GearChildren.AsEnumerableWithSideEffects().ForEach(objGear =>
                                        {
                                            if (objGear.Equipped)
                                                objGear.ChangeEquippedStatus(objAccessory.Equipped, true);
                                        });
                                    });
                                }
                                else
                                {
                                    objChild.WeaponAccessories.AsEnumerableWithSideEffects().ForEach(objAccessory =>
                                        objAccessory.GearChildren.AsEnumerableWithSideEffects()
                                            .ForEach(objGear => objGear.ChangeEquippedStatus(false, true)));
                                }
                            }
                        }
                        else
                        {
                            foreach (Weapon objChild in Children
                                         .DeepWhere(x => x.Children, x => x.WeaponAccessories.Count > 0).ToList())
                            {
                                objChild.WeaponAccessories.AsEnumerableWithSideEffects().ForEach(objAccessory =>
                                    objAccessory.GearChildren.AsEnumerableWithSideEffects()
                                        .ForEach(objGear => objGear.ChangeEquippedStatus(false, true)));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(Weight)
                        || WeaponAccessories.Any(x => !string.IsNullOrEmpty(x.Weight)
                                                      || x.GearChildren.DeepAny(
                                                          y => y.Children, y => !string.IsNullOrEmpty(y.Weight)))
                        || Children.DeepAny(x => x.Children,
                            z => !string.IsNullOrEmpty(z.Weight)
                                 || WeaponAccessories.Any(
                                     x => !string.IsNullOrEmpty(x.Weight)
                                          || x.GearChildren.DeepAny(
                                              y => y.Children, y => !string.IsNullOrEmpty(y.Weight)))))
                        _objCharacter.OnPropertyChanged(nameof(Character.TotalCarriedWeight));
                }
            }
        }

        public async Task SetEquippedAsync(bool value, CancellationToken token = default)
        {
            if (_blnEquipped == value)
                return;
            _blnEquipped = value;
            if (ParentVehicle == null && _objCharacter?.IsLoading == false)
            {
                if (await WeaponAccessories
                        .AnyAsync(
                            async x => x.Equipped &&
                                       await x.GearChildren.GetCountAsync(token).ConfigureAwait(false) > 0,
                            token: token).ConfigureAwait(false))
                {
                    if (value)
                    {
                        await WeaponAccessories.ForEachWithSideEffectsAsync(objAccessory =>
                            objAccessory.GearChildren.ForEachWithSideEffectsAsync(async objGear =>
                            {
                                if (objGear.Equipped)
                                    await objGear.ChangeEquippedStatusAsync(objAccessory.Equipped, true, token)
                                        .ConfigureAwait(false);
                            }, token: token), token: token).ConfigureAwait(false);
                    }
                    else
                    {
                        await WeaponAccessories.ForEachWithSideEffectsAsync(objAccessory =>
                                objAccessory.GearChildren.ForEachWithSideEffectsAsync(
                                    objGear => objGear.ChangeEquippedStatusAsync(false, true, token), token: token),
                            token: token).ConfigureAwait(false);
                    }
                }

                if (await Children.GetCountAsync(token).ConfigureAwait(false) > 0)
                {
                    if (value)
                    {
                        foreach (Weapon objChild in await Children.DeepWhereAsync(x => x.Children,
                                     async x => await x.WeaponAccessories.GetCountAsync(token).ConfigureAwait(false) >
                                                0, token: token).ConfigureAwait(false))
                        {
                            bool blnAllParentsEquipped = objChild.Equipped;
                            Weapon objLoopParent = objChild.Parent;
                            while (blnAllParentsEquipped && objLoopParent != null)
                            {
                                blnAllParentsEquipped = objLoopParent.Equipped;
                                objLoopParent = objLoopParent.Parent;
                            }

                            if (blnAllParentsEquipped)
                            {
                                await objChild.WeaponAccessories.ForEachWithSideEffectsAsync(objAccessory =>
                                    objAccessory.GearChildren.ForEachWithSideEffectsAsync(async objGear =>
                                    {
                                        if (objGear.Equipped)
                                            await objGear.ChangeEquippedStatusAsync(objAccessory.Equipped, true, token)
                                                .ConfigureAwait(false);
                                    }, token: token), token: token).ConfigureAwait(false);
                            }
                            else
                            {
                                await objChild.WeaponAccessories.ForEachWithSideEffectsAsync(objAccessory =>
                                        objAccessory.GearChildren.ForEachWithSideEffectsAsync(
                                            objGear => objGear.ChangeEquippedStatusAsync(false, true, token),
                                            token: token),
                                    token: token).ConfigureAwait(false);
                            }
                        }
                    }
                    else
                    {
                        foreach (Weapon objChild in await Children.DeepWhereAsync(x => x.Children,
                                     async x => await x.WeaponAccessories.GetCountAsync(token).ConfigureAwait(false) >
                                                0, token: token).ConfigureAwait(false))
                        {
                            await objChild.WeaponAccessories.ForEachWithSideEffectsAsync(objAccessory =>
                                    objAccessory.GearChildren.ForEachWithSideEffectsAsync(
                                        objGear => objGear.ChangeEquippedStatusAsync(false, true, token), token: token),
                                token: token).ConfigureAwait(false);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Weight)
                    || await WeaponAccessories
                        .AnyAsync(
                            async x => !string.IsNullOrEmpty(x.Weight) || await x.GearChildren
                                .DeepAnyAsync(y => y.Children, y => !string.IsNullOrEmpty(y.Weight), token: token)
                                .ConfigureAwait(false), token: token).ConfigureAwait(false)
                    || await Children.DeepAnyAsync(x => x.Children,
                        async z => !string.IsNullOrEmpty(z.Weight)
                                   || await WeaponAccessories.AnyAsync(
                                           async x => !string.IsNullOrEmpty(x.Weight)
                                                      || await x.GearChildren.DeepAnyAsync(
                                                          y => y.Children, y => !string.IsNullOrEmpty(y.Weight),
                                                          token: token).ConfigureAwait(false), token: token)
                                       .ConfigureAwait(false), token: token).ConfigureAwait(false))
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.TotalCarriedWeight), token)
                        .ConfigureAwait(false);
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
        /// Whether the Armor's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost;
            set => _blnDiscountCost = value;
        }

        /// <summary>
        /// Whether the Weapon requires Ammo to be reloaded.
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
            XmlDocument objDoc = blnSync
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

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage,
            CancellationToken token = default)
        {
            XPathNavigator objReturn = _objCachedMyXPathNode;
            if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath("weapons.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token)
                    .ConfigureAwait(false);
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
        /// Wireless Weapon Bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessWeaponBonus => _nodWirelessWeaponBonus;

        /// <summary>
        /// Whether the Weapon's wireless bonus is enabled
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
        private string RelevantAutosoft =>
            RangeType == "Melee" && _objCharacter.Settings.BookEnabled("R5") ? "[Weapon] Melee Autosoft" : "[Weapon] Targeting Autosoft";

        /// <summary>
        /// Name of Autosoft that is used for a weapon's dicepool. Melee weapons use Melee autosoft per R5 127
        /// </summary>
        private async Task<string> GetRelevantAutosoftAsync(CancellationToken token = default)
        {
            return RangeType == "Melee"
                   && await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false))
                       .BookEnabledAsync("R5", token).ConfigureAwait(false)
                ? "[Weapon] Melee Autosoft"
                : "[Weapon] Targeting Autosoft";
        }

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications in the program's current language.
        /// </summary>
        public string DisplayConcealability => CalculatedConcealability().ToString("+0;-0;0", GlobalSettings.CultureInfo);

        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications in the program's current language.
        /// </summary>
        public async Task<string> GetDisplayConcealabilityAsync(CancellationToken token = default)
        {
            decimal decConceal = await CalculatedConcealabilityAsync(token).ConfigureAwait(false);
            return decConceal.ToString("+0;-0;0", GlobalSettings.CultureInfo);
        }

        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications.
        /// </summary>
        public decimal CalculatedConcealability()
        {
            string strConceal = Concealability;
            if (strConceal.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdConceal))
                {
                    if (!string.IsNullOrEmpty(strConceal))
                        sbdConceal.Append('(').Append(strConceal.TrimStartOnce('+')).Append(')');
                    foreach (WeaponAccessory objAccessory in WeaponAccessories)
                    {
                        if (!objAccessory.Equipped)
                            continue;
                        string strLoopConceal = objAccessory.Concealability;
                        if (!string.IsNullOrEmpty(strLoopConceal))
                        {
                            strLoopConceal = strLoopConceal.TrimStartOnce('+')
                                .CheapReplace("{Rating}", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace("Rating", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            sbdConceal.Append(sbdConceal.Length > 0 ? " + (" : "(").Append(strLoopConceal).Append(')');
                        }
                    }

                    sbdConceal.CheapReplace(strConceal, "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    sbdConceal.CheapReplace(strConceal, "Rating", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    ProcessAttributesInXPath(sbdConceal);
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdConceal.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal((double)objProcess);
                }
            }
            // Factor in the character's Concealability modifiers.
            decReturn += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Concealability);
            return decReturn;
        }

        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications.
        /// </summary>
        public async Task<decimal> CalculatedConcealabilityAsync(CancellationToken token = default)
        {
            string strConceal = Concealability;
            if (strConceal.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdConceal))
                {
                    if (!string.IsNullOrEmpty(strConceal))
                        sbdConceal.Append('(').Append(strConceal.TrimStartOnce('+')).Append(')');
                    await WeaponAccessories.ForEachAsync(async objAccessory =>
                    {
                        if (!objAccessory.Equipped)
                            return;
                        string strLoopConceal = objAccessory.Concealability;
                        if (!string.IsNullOrEmpty(strLoopConceal))
                        {
                            strLoopConceal = await strLoopConceal.TrimStartOnce('+')
                                .CheapReplaceAsync("{Rating}", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                .CheapReplaceAsync("Rating", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                            sbdConceal.Append(sbdConceal.Length > 0 ? " + (" : "(").Append(strLoopConceal).Append(')');
                        }
                    }, token).ConfigureAwait(false);

                    await sbdConceal.CheapReplaceAsync(strConceal, "{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdConceal.CheapReplaceAsync(strConceal, "Rating", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await ProcessAttributesInXPathAsync(sbdConceal, token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdConceal.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal((double)objProcess);
                }
            }
            // Factor in the character's Concealability modifiers.
            decReturn += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Concealability, token: token).ConfigureAwait(false);
            return decReturn;
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition in the program's current language.
        /// </summary>
        public string DisplayDamage => CalculatedDamage(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition in the program's current language.
        /// </summary>
        public Task<string> GetDisplayDamageAsync(CancellationToken token = default) => CalculatedDamageAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token: token);

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        public string CalculatedDamage(CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true)
        {
            return Utils.SafelyRunSynchronously(() =>
                CalculatedDamageCoreAsync(true, objCulture, strLanguage, blnIncludeAmmo));
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        public Task<string> CalculatedDamageAsync(CultureInfo objCulture, string strLanguage,
            bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            return CalculatedDamageCoreAsync(false, objCulture, strLanguage, blnIncludeAmmo, token);
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        private async Task<string> CalculatedDamageCoreAsync(bool blnSync, CultureInfo objCulture, string strLanguage,
            bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            // If the cost is determined by the Rating, evaluate the expression.
            string strDamageType = string.Empty;
            string strDamageExtra = string.Empty;
            string strDamage;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdDamage))
            {
                sbdDamage.Append(Damage);
                if (blnSync)
                {
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    ProcessAttributesInXPath(sbdDamage, Damage);
                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                    sbdDamage.CheapReplace(Damage, "{Rating}",
                        () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                }
                else
                {
                    await ProcessAttributesInXPathAsync(sbdDamage, Damage, token: token).ConfigureAwait(false);
                    await sbdDamage.CheapReplaceAsync(Damage,
                            "{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                        .ConfigureAwait(false);
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
                foreach (string strValue in strMin.TrimStartOnce("min(", true).TrimEndOnce(')')
                             .SplitNoAlloc(',', StringSplitOptions.RemoveEmptyEntries))
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

                if (blnSync)
                {
                    string strUseSkill = Skill?.DictionaryKey ?? string.Empty;
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
                        decImprove += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedDV,
                            token: token);
                    }

                    // This should also add any UnarmedDV bonus to Unarmed physical weapons if the option is enabled.
                    else if (strUseSkill == "Unarmed Combat"
                             && _objCharacter.Settings.UnarmedImprovementsApplyToWeapons)
                    {
                        decImprove += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedDV,
                            token: token);
                    }
                    // ReSharper restore MethodHasAsyncOverload
                }
                else
                {
                    Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                    string strUseSkill = objSkill != null
                        ? await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                        : string.Empty;
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
                                    _objCharacter, Improvement.ImprovementType.UnarmedDVPhysical, token: token)
                                .ConfigureAwait(false))
                            .Count > 0)
                            strDamageType = "P";
                        decImprove += await ImprovementManager
                            .ValueOfAsync(_objCharacter, Improvement.ImprovementType.UnarmedDV, token: token)
                            .ConfigureAwait(false);
                    }

                    // This should also add any UnarmedDV bonus to Unarmed physical weapons if the option is enabled.
                    else if (strUseSkill == "Unarmed Combat"
                             && _objCharacter.Settings.UnarmedImprovementsApplyToWeapons)
                    {
                        decImprove += await ImprovementManager
                            .ValueOfAsync(_objCharacter, Improvement.ImprovementType.UnarmedDV, token: token)
                            .ConfigureAwait(false);
                    }
                }

                if (_objCharacter.Settings.MoreLethalGameplay)
                    decImprove += 2;
            }

            bool blnDamageReplaced = false;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                       out StringBuilder sbdBonusDamage))
            {
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    // Change the Weapon's Damage Type.
                    if (WirelessWeaponBonus["damagetype"] != null)
                    {
                        strDamageType = string.Empty;
                        strDamageExtra = WirelessWeaponBonus["damagetype"].InnerText;
                    }

                    // Adjust the Weapon's Damage.
                    string strTemp = WirelessWeaponBonus["damage"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp) && strTemp != "0" && strTemp != "+0" && strTemp != "-0")
                        sbdBonusDamage.Append(" + ").Append(strTemp.TrimStartOnce('+'));
                    strTemp = WirelessWeaponBonus["damagereplace"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        blnDamageReplaced = true;
                        strDamage = strTemp;
                    }
                }

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
                        if (WirelessOn && objAccessory.WirelessOn && objAccessory.WirelessWeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            if (objAccessory.WirelessWeaponBonus["damagetype"] != null)
                            {
                                strDamageType = string.Empty;
                                strDamageExtra = objAccessory.WirelessWeaponBonus["damagetype"].InnerText;
                            }

                            // Adjust the Weapon's Damage.
                            string strTemp = objAccessory.WirelessWeaponBonus["damage"]?.InnerText;
                            if (!string.IsNullOrEmpty(strTemp) && strTemp != "0" && strTemp != "+0" && strTemp != "-0")
                                sbdBonusDamage.Append(" + ").Append(strTemp.TrimStartOnce('+'));
                            strTemp = objAccessory.WirelessWeaponBonus["damagereplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strTemp))
                            {
                                blnDamageReplaced = true;
                                strDamage = strTemp;
                            }
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
                            if (!string.IsNullOrEmpty(strTemp) && strTemp != "0" && strTemp != "+0" && strTemp != "-0")
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
                                     ? objGear.Children.DeepWhere(x => x.Children.Where(y => y.Equipped), x => x.Equipped, token)
                                     : await objGear.Children.DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token)
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
                                if (!string.IsNullOrEmpty(strTemp) && strTemp != "0" && strTemp != "+0" && strTemp != "-0")
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
                                if (!string.IsNullOrEmpty(strTemp) && strTemp != "0" && strTemp != "+0" && strTemp != "-0")
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
                else if (strDamage.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
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
                else
                {
                    int intDamage = (decValue + decImprove).StandardRound();
                    if (Name == "Unarmed Attack (Smashing Blow)")
                        intDamage *= 2;
                    strDamage = intDamage.ToString(objCulture);
                    strReturn = strDamage + strDamageType + strDamageExtra;
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
                else if (strDamage.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
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
                else
                {
                    int intDamage = (decValue + decImprove).StandardRound();
                    if (Name == "Unarmed Attack (Smashing Blow)")
                        intDamage *= 2;
                    strDamage = intDamage.ToString(objCulture);
                    strReturn = strDamage + strDamageType + strDamageExtra;
                }
            }

            // If the string couldn't be parsed (resulting in NaN which will happen if it is a special string like "Grenade", "Chemical", etc.), return the Weapon's Damage string.
            if (strReturn.StartsWith("NaN", StringComparison.Ordinal))
                strReturn = Damage;

            // Translate the Damage Code.
            return blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? ReplaceDamageStrings(strReturn, strLanguage, token)
                : await ReplaceDamageStringsAsync(strReturn, strLanguage, token).ConfigureAwait(false);
        }

        public static string ReplaceDamageStrings(string strInput, string strLanguage,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strInput;
            Lazy<string> strStun
                = new Lazy<string>(
                    () => LanguageManager.GetString("String_DamageStun", strLanguage, token: token));
            Lazy<string> strPhysical
                = new Lazy<string>(
                    () => LanguageManager.GetString("String_DamagePhysical", strLanguage, token: token));
            return ReplaceStrings(strInput, strLanguage, token)
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
        }

        public static async Task<string> ReplaceDamageStringsAsync(string strInput, string strLanguage,
            CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return strInput;
            AsyncLazy<string> strStun = new AsyncLazy<string>(
                () => LanguageManager.GetStringAsync("String_DamageStun", strLanguage, token: token),
                Utils.JoinableTaskFactory);
            AsyncLazy<string> strPhysical = new AsyncLazy<string>(
                () => LanguageManager.GetStringAsync("String_DamagePhysical", strLanguage, token: token),
                Utils.JoinableTaskFactory);
            return await (await ReplaceStringsAsync(strInput, strLanguage, token).ConfigureAwait(false))
                .CheapReplaceAsync("0S", async () => '0' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("1S", async () => '1' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("2S", async () => '2' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("3S", async () => '3' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("4S", async () => '4' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("5S", async () => '5' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("6S", async () => '6' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("7S", async () => '7' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("8S", async () => '8' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("9S", async () => '9' + await strStun.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("0P", async () => '0' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("1P", async () => '1' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("2P", async () => '2' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("3P", async () => '3' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("4P", async () => '4' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("5P", async () => '5' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("6P", async () => '6' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("7P", async () => '7' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("8P", async () => '8' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token)
                .CheapReplaceAsync("9P", async () => '9' + await strPhysical.GetValueAsync(token).ConfigureAwait(false),
                    token: token).ConfigureAwait(false);
        }

        public static string ReplaceStrings(string strInput, string strLanguage, CancellationToken token = default)
        {
            return strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
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

        public static async Task<string> ReplaceStringsAsync(string strInput, string strLanguage,
            CancellationToken token = default)
        {
            return strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                ? strInput
                : await strInput
                    .CheapReplaceAsync(
                        "Special",
                        () => LanguageManager.GetStringAsync("String_DamageSpecial", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "P or S", () => LanguageManager.GetStringAsync("String_DamagePOrS", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "Chemical",
                        () => LanguageManager.GetStringAsync("String_DamageChemical", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "(e)", () => LanguageManager.GetStringAsync("String_DamageElectric", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "(f)",
                        () => LanguageManager.GetStringAsync("String_DamageFlechette", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "(fire)", () => LanguageManager.GetStringAsync("String_DamageFire", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "Grenade",
                        () => LanguageManager.GetStringAsync("String_DamageGrenade", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "Missile",
                        () => LanguageManager.GetStringAsync("String_DamageMissile", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "Mortar",
                        () => LanguageManager.GetStringAsync("String_DamageMortar", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "Rocket",
                        () => LanguageManager.GetStringAsync("String_DamageRocket", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "Torpedo",
                        () => LanguageManager.GetStringAsync("String_DamageTorpedo", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "Radius",
                        () => LanguageManager.GetStringAsync("String_DamageRadius", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync("As Drug/Toxin",
                        () => LanguageManager.GetStringAsync(
                            "String_DamageAsDrugToxin", strLanguage, token: token), token: token)
                    .CheapReplaceAsync(
                        "as round",
                        () => LanguageManager.GetStringAsync("String_DamageAsRound", strLanguage, token: token),
                        token: token)
                    .CheapReplaceAsync(
                        "/m",
                        async () => '/' + await LanguageManager
                            .GetStringAsync("String_DamageMeter", strLanguage, token: token).ConfigureAwait(false),
                        token: token)
                    .CheapReplaceAsync(
                        "(M)", () => LanguageManager.GetStringAsync("String_DamageMatrix", strLanguage, token: token),
                        token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// Calculated Ammo capacity in the program's current language.
        /// </summary>
        public string DisplayAmmo => CalculatedAmmo(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Calculated Ammo capacity in the program's current language.
        /// </summary>
        public Task<string> GetDisplayAmmoAsync(CancellationToken token = default) => CalculatedAmmoAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        public string CalculatedAmmo(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => CalculatedAmmoCoreAsync(true, objCulture, strLanguage, token),
                token);
        }

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        public Task<string> CalculatedAmmoAsync(CultureInfo objCulture, string strLanguage,
            CancellationToken token = default)
        {
            return CalculatedAmmoCoreAsync(false, objCulture, strLanguage, token);
        }

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        private async Task<string> CalculatedAmmoCoreAsync(bool blnSync, CultureInfo objCulture, string strLanguage,
            CancellationToken token = default)
        {
            IEnumerable<string> lstAmmos = Ammo.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries);
            decimal decAmmoBonus = 0;
            decimal decAmmoBonusFlat = 0;
            decimal decAmmoBonusPercent = 1.0m;
            if (blnSync)
            {
                if (WeaponAccessories.Count != 0)
                {
                    decAmmoBonus += WeaponAccessories.Sum(x => x.Equipped, objAccessory =>
                    {
                        // Replace the Ammo value.
                        if (!string.IsNullOrEmpty(objAccessory.AmmoReplace))
                        {
                            lstAmmos = objAccessory.AmmoReplace.SplitNoAlloc(' ',
                                StringSplitOptions.RemoveEmptyEntries);
                        }

                        return objAccessory.TotalAmmoBonus;
                    }, token);
                }

                if (ParentMount != null)
                {
                    decAmmoBonusFlat += ParentMount.Mods.Sum(x => x.Equipped, objMod =>
                    {
                        if (!string.IsNullOrEmpty(objMod.AmmoReplace))
                        {
                            lstAmmos = objMod.AmmoReplace.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (objMod.AmmoBonusPercent != 0)
                        {
                            decAmmoBonusPercent *= objMod.AmmoBonusPercent / 100.0m;
                        }
                        return objMod.AmmoBonus;
                    }, token);
                }
            }
            else
            {
                if (await WeaponAccessories.GetCountAsync(token).ConfigureAwait(false) != 0)
                {
                    decAmmoBonus += await WeaponAccessories.SumAsync(x => x.Equipped, async objAccessory =>
                    {
                        // Replace the Ammo value.
                        if (!string.IsNullOrEmpty(objAccessory.AmmoReplace))
                        {
                            lstAmmos = objAccessory.AmmoReplace.SplitNoAlloc(' ',
                                StringSplitOptions.RemoveEmptyEntries);
                        }

                        return await objAccessory.GetTotalAmmoBonusAsync(token).ConfigureAwait(false);
                    }, token).ConfigureAwait(false);
                }
                if (ParentMount != null)
                {
                    decAmmoBonusFlat += await ParentMount.Mods.SumAsync(x => x.Equipped, objMod =>
                    {
                        if (!string.IsNullOrEmpty(objMod.AmmoReplace))
                        {
                            lstAmmos = objMod.AmmoReplace.SplitNoAlloc(' ', StringSplitOptions.RemoveEmptyEntries);
                        }

                        if (objMod.AmmoBonusPercent != 0)
                        {
                            decAmmoBonusPercent *= objMod.AmmoBonusPercent / 100.0m;
                        }
                        return objMod.AmmoBonus;
                    }, token).ConfigureAwait(false);
                }
            }

            string strSpace = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? LanguageManager.GetString("String_Space", strLanguage, token: token)
                : await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false);
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
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
                            strThisAmmo = strThisAmmo.Substring(intPos + 1);
                        }
                        else
                        {
                            intPos = strThisAmmo.IndexOf('×');
                            if (intPos != -1)
                            {
                                strPrepend = strThisAmmo.Substring(0, intPos + 1);
                                strThisAmmo = strThisAmmo.Substring(intPos + 1);
                            }
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
                                        using (new FetchSafelyFromObjectPool<StringBuilder>(
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

                        if (strThisAmmo.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                        {
                            strThisAmmo = blnSync
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                ? strThisAmmo.CheapReplace("Weapon", () => AmmoCapacity(Ammo))
                                : await strThisAmmo.CheapReplaceAsync("Weapon", () => AmmoCapacity(Ammo), token: token)
                                    .ConfigureAwait(false);
                            (bool blnIsSuccess, object objProcess) = blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? CommonFunctions.EvaluateInvariantXPath(strThisAmmo, token)
                                : await CommonFunctions.EvaluateInvariantXPathAsync(strThisAmmo, token)
                                    .ConfigureAwait(false);
                            if (blnIsSuccess)
                            {
                                decimal decAmmo = Convert.ToDecimal((double)objProcess) + decAmmoBonusFlat;
                                if (decAmmoBonus != 0.0m)
                                    decAmmo += (decAmmo * decAmmoBonus) / 100.0m;
                                if (decAmmoBonusPercent != 1.0m)
                                    decAmmo *= decAmmoBonusPercent;

                                strThisAmmo = decAmmo.StandardRound().ToString(objCulture)
                                              + strAmmo.Substring(strAmmo.IndexOf('('),
                                                  strAmmo.Length - strAmmo.IndexOf('('));
                            }
                        }
                        else
                        {
                            decimal decAmmo = decValue + decAmmoBonusFlat;
                            if (decAmmoBonus != 0.0m)
                                decAmmo += (decAmmo * decAmmoBonus) / 100.0m;
                            if (decAmmoBonusPercent != 1.0m)
                                decAmmo *= decAmmoBonusPercent;
                            strThisAmmo = decAmmo.StandardRound().ToString(objCulture)
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
                                () => strSpace + LanguageManager.GetString("String_Or", strLanguage, token: token) +
                                      strSpace,
                                StringComparison.OrdinalIgnoreCase)
                            .CheapReplace(
                                " Belt", () => LanguageManager.GetString("String_AmmoBelt", strLanguage, token: token),
                                StringComparison.OrdinalIgnoreCase)
                            .CheapReplace(
                                " Energy",
                                () => LanguageManager.GetString("String_AmmoEnergy", strLanguage, token: token),
                                StringComparison.OrdinalIgnoreCase)
                            .CheapReplace(" External Source",
                                () => LanguageManager.GetString(
                                    "String_AmmoExternalSource", strLanguage, token: token),
                                StringComparison.OrdinalIgnoreCase)
                            .CheapReplace(
                                " Special",
                                () => LanguageManager.GetString("String_AmmoSpecial", strLanguage, token: token),
                                StringComparison.OrdinalIgnoreCase)
                            .CheapReplace(
                                "(b)",
                                () => '(' + LanguageManager.GetString("String_AmmoBreakAction", strLanguage,
                                              token: token)
                                          + ')')
                            .CheapReplace(
                                "(belt)",
                                () => '(' + LanguageManager.GetString("String_AmmoBelt", strLanguage, token: token) +
                                      ')')
                            .CheapReplace(
                                "(box)",
                                () => '(' + LanguageManager.GetString("String_AmmoBox", strLanguage, token: token) +
                                      ')')
                            .CheapReplace(
                                "(c)",
                                () => '(' + LanguageManager.GetString("String_AmmoClip", strLanguage, token: token) +
                                      ')')
                            .CheapReplace(
                                "(cy)",
                                () => '(' +
                                      LanguageManager.GetString("String_AmmoCylinder", strLanguage, token: token) + ')')
                            .CheapReplace(
                                "(d)",
                                () => '(' + LanguageManager.GetString("String_AmmoDrum", strLanguage, token: token) +
                                      ')')
                            .CheapReplace(
                                "(m)",
                                () => '(' +
                                      LanguageManager.GetString("String_AmmoMagazine", strLanguage, token: token) + ')')
                            .CheapReplace(
                                "(ml)",
                                () => '(' + LanguageManager.GetString("String_AmmoMuzzleLoad", strLanguage,
                                              token: token)
                                          + ')');
                        // ReSharper restore MethodHasAsyncOverloadWithCancellation
                    }
                    else
                    {
                        strReturn = await strReturn
                            .CheapReplaceAsync(
                                " or ",
                                async () => strSpace + await LanguageManager
                                                         .GetStringAsync("String_Or", strLanguage, token: token)
                                                         .ConfigureAwait(false)
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
                                                    "String_AmmoBreakAction", strLanguage, token: token)
                                                .ConfigureAwait(false) + ')', token: token)
                            .CheapReplaceAsync(
                                "(belt)",
                                async () => '('
                                            + await LanguageManager.GetStringAsync(
                                                "String_AmmoBelt", strLanguage, token: token).ConfigureAwait(false) +
                                            ')', token: token)
                            .CheapReplaceAsync(
                                "(box)",
                                async () => '('
                                            + await LanguageManager.GetStringAsync(
                                                "String_AmmoBox", strLanguage, token: token).ConfigureAwait(false) +
                                            ')', token: token)
                            .CheapReplaceAsync(
                                "(c)",
                                async () => '('
                                            + await LanguageManager.GetStringAsync(
                                                "String_AmmoClip", strLanguage, token: token).ConfigureAwait(false) +
                                            ')', token: token)
                            .CheapReplaceAsync(
                                "(cy)",
                                async () => '('
                                            + await LanguageManager.GetStringAsync(
                                                    "String_AmmoCylinder", strLanguage, token: token)
                                                .ConfigureAwait(false) + ')', token: token)
                            .CheapReplaceAsync(
                                "(d)",
                                async () => '('
                                            + await LanguageManager.GetStringAsync(
                                                "String_AmmoDrum", strLanguage, token: token).ConfigureAwait(false) +
                                            ')', token: token)
                            .CheapReplaceAsync(
                                "(m)",
                                async () => '('
                                            + await LanguageManager.GetStringAsync(
                                                    "String_AmmoMagazine", strLanguage, token: token)
                                                .ConfigureAwait(false) + ')', token: token)
                            .CheapReplaceAsync(
                                "(ml)",
                                async () => '('
                                            + await LanguageManager.GetStringAsync(
                                                    "String_AmmoMuzzleLoad", strLanguage, token: token)
                                                .ConfigureAwait(false) + ')', token: token).ConfigureAwait(false);
                    }
                }

                return strReturn;
            }
        }

        public bool AllowSingleShot => (RangeType == "Melee" && Ammo != "0") // Melee Weapons with Ammo are considered to be Single Shot.
                                       || (_blnAllowSingleShot
                                           && AllowModes(GlobalSettings.Language,
                                                         LanguageManager.GetString("String_ModeSingleShot"),
                                                         LanguageManager.GetString("String_ModeSemiAutomatic")));

        public bool AllowShortBurst => _blnAllowShortBurst
                                       && AllowModes(GlobalSettings.Language,
                                                     LanguageManager.GetString("String_ModeBurstFire"),
                                                     LanguageManager.GetString("String_ModeSemiAutomatic"),
                                                     LanguageManager.GetString("String_ModeFullAutomatic"));

        public bool AllowLongBurst => _blnAllowLongBurst
                                      && AllowModes(GlobalSettings.Language,
                                                    LanguageManager.GetString("String_ModeBurstFire"),
                                                    LanguageManager.GetString("String_ModeFullAutomatic"));

        public bool AllowFullBurst => _blnAllowFullBurst
                                      && AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));

        public bool AllowSuppressive => _blnAllowSuppressive
                                        && AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));

        public async Task<bool> GetAllowSingleShotAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return (RangeType == "Melee" && Ammo != "0") // Melee Weapons with Ammo are considered to be Single Shot.
                || (_blnAllowSingleShot
                    && await AllowModesAsync(token,
                        await LanguageManager.GetStringAsync("String_ModeSingleShot", token: token).ConfigureAwait(false),
                        await LanguageManager.GetStringAsync("String_ModeSemiAutomatic", token: token).ConfigureAwait(false)).ConfigureAwait(false));
        }

        public async Task<bool> GetAllowShortBurstAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _blnAllowShortBurst
                && await AllowModesAsync(token,
                    await LanguageManager.GetStringAsync("String_ModeBurstFire", token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("String_ModeSemiAutomatic", token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("String_ModeFullAutomatic", token: token).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task<bool> GetAllowLongBurstAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _blnAllowLongBurst
                && await AllowModesAsync(token,
                    await LanguageManager.GetStringAsync("String_ModeBurstFire", token: token).ConfigureAwait(false),
                    await LanguageManager.GetStringAsync("String_ModeFullAutomatic", token: token).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task<bool> GetAllowFullBurstAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _blnAllowFullBurst
                && await AllowModeAsync(await LanguageManager.GetStringAsync("String_ModeFullAutomatic", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        public async Task<bool> GetAllowSuppressiveAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return _blnAllowSuppressive
                && await AllowModeAsync(await LanguageManager.GetStringAsync("String_ModeFullAutomatic", token: token).ConfigureAwait(false), token: token).ConfigureAwait(false);
        }

        /// <summary>
        /// The Weapon's Firing Mode including Modifications in the program's current language.
        /// </summary>
        public string DisplayMode => CalculatedMode(GlobalSettings.Language);

        /// <summary>
        /// The Weapon's Firing Mode including Modifications in the program's current language.
        /// </summary>
        public Task<string> GetDisplayModeAsync(CancellationToken token = default) => CalculatedModeAsync(GlobalSettings.Language, token: token);

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        public string CalculatedMode(string strLanguage, bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => CalculatedModeCoreAsync(true, strLanguage, blnIncludeAmmo, token),
                token);
        }

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        public Task<string> CalculatedModeAsync(string strLanguage, bool blnIncludeAmmo = true,
            CancellationToken token = default)
        {
            return CalculatedModeCoreAsync(false, strLanguage, blnIncludeAmmo, token);
        }

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        private async Task<string> CalculatedModeCoreAsync(bool blnSync, string strLanguage, bool blnIncludeAmmo = true,
            CancellationToken token = default)
        {
            // Move the contents of the array to a list so it's easier to work with.
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                       out HashSet<string> setModes))
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                       out HashSet<string> setNewModes))
            {
                setModes.AddRange(_strMode.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries));

                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    string strFireMode = WirelessWeaponBonus["firemode"]?.InnerText;
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

                    strFireMode = WirelessWeaponBonus["modereplace"]?.InnerText;
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

                // Do the same for any accessories/modifications.
                if (blnSync)
                {
                    WeaponAccessories.ForEach(objAccessory =>
                    {
                        if (!objAccessory.Equipped)
                            return;
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

                        if (WirelessOn && objAccessory.WirelessOn && objAccessory.WirelessWeaponBonus != null)
                        {
                            string strFireMode = objAccessory.WirelessWeaponBonus["firemode"]?.InnerText;
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

                            strFireMode = objAccessory.WirelessWeaponBonus["modereplace"]?.InnerText;
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
                    }, token);
                }
                else
                {
                    await WeaponAccessories.ForEachAsync(objAccessory =>
                    {
                        if (!objAccessory.Equipped)
                            return;
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

                        if (WirelessOn && objAccessory.WirelessOn && objAccessory.WirelessWeaponBonus != null)
                        {
                            string strFireMode = objAccessory.WirelessWeaponBonus["firemode"]?.InnerText;
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

                            strFireMode = objAccessory.WirelessWeaponBonus["modereplace"]?.InnerText;
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
                    }, token).ConfigureAwait(false);
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
                                     ? objGear.Children.DeepWhere(x => x.Children.Where(y => y.Equipped), x => x.Equipped, token)
                                     : await objGear.Children.DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token)
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

                setModes.UnionWith(setNewModes);

                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
                {
                    if (blnSync)
                    {
                        if (setModes.Contains("SS"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeSingleShot", strLanguage,
                                    token: token))
                                .Append('/');
                        if (setModes.Contains("SA"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeSemiAutomatic", strLanguage,
                                    token: token))
                                .Append('/');
                        if (setModes.Contains("BF"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeBurstFire", strLanguage,
                                    token: token))
                                .Append('/');
                        if (setModes.Contains("FA"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeFullAutomatic", strLanguage,
                                    token: token))
                                .Append('/');
                        if (setModes.Contains("Special"))
                            // ReSharper disable once MethodHasAsyncOverload
                            sbdReturn.Append(LanguageManager.GetString("String_ModeSpecial", strLanguage, token: token))
                                .Append('/');
                    }
                    else
                    {
                        if (setModes.Contains("SS"))
                            sbdReturn.Append(await LanguageManager
                                    .GetStringAsync("String_ModeSingleShot", strLanguage, token: token)
                                    .ConfigureAwait(false))
                                .Append('/');
                        if (setModes.Contains("SA"))
                            sbdReturn.Append(await LanguageManager
                                    .GetStringAsync("String_ModeSemiAutomatic", strLanguage, token: token)
                                    .ConfigureAwait(false))
                                .Append('/');
                        if (setModes.Contains("BF"))
                            sbdReturn.Append(await LanguageManager
                                    .GetStringAsync("String_ModeBurstFire", strLanguage, token: token)
                                    .ConfigureAwait(false))
                                .Append('/');
                        if (setModes.Contains("FA"))
                            sbdReturn.Append(await LanguageManager
                                    .GetStringAsync("String_ModeFullAutomatic", strLanguage, token: token)
                                    .ConfigureAwait(false))
                                .Append('/');
                        if (setModes.Contains("Special"))
                            sbdReturn.Append(await LanguageManager
                                    .GetStringAsync("String_ModeSpecial", strLanguage, token: token)
                                    .ConfigureAwait(false))
                                .Append('/');
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
        /// Determine if the Weapon is capable of firing in one of a set of particular modes.
        /// </summary>
        /// <param name="astrModes">Firing modes to find.</param>
        /// <param name="strLanguage">Language of <paramref name="strFindMode"/>. Uses current UI language if unset.</param>
        public bool AllowModes(string strLanguage, params string[] astrModes)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                       out HashSet<string> setModes))
            {
                setModes.AddRange(astrModes);
                return CalculatedMode(strLanguage).SplitNoAlloc('/').Any(x => setModes.Contains(x));
            }
        }

        /// <summary>
        /// Determine if the Weapon is capable of firing in a particular mode.
        /// </summary>
        /// <param name="strFindMode">Firing mode to find.</param>
        /// <param name="strLanguage">Language of <paramref name="strFindMode"/>. Uses current UI language if unset.</param>
        public async Task<bool> AllowModeAsync(string strFindMode, string strLanguage = "", CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            return (await CalculatedModeAsync(strLanguage, token: token).ConfigureAwait(false)).SplitNoAlloc('/').Contains(strFindMode);
        }

        /// <summary>
        /// Determine if the Weapon is capable of firing in one of a set of particular modes.
        /// </summary>
        /// <param name="astrModes">Firing modes to find.</param>
        public Task<bool> AllowModesAsync(string strLanguage, params string[] astrModes)
        {
            return AllowModesAsync(strLanguage, default, astrModes);
        }

        /// <summary>
        /// Determine if the Weapon is capable of firing in one of a set of particular modes.
        /// </summary>
        /// <param name="astrModes">Firing modes to find.</param>
        public Task<bool> AllowModesAsync(CancellationToken token, params string[] astrModes)
        {
            return AllowModesAsync(GlobalSettings.Language, token, astrModes);
        }

        /// <summary>
        /// Determine if the Weapon is capable of firing in one of a set of particular modes.
        /// </summary>
        /// <param name="astrModes">Firing modes to find.</param>
        /// <param name="strLanguage">Language of <paramref name="strFindMode"/>. Uses current UI language if unset.</param>
        public async Task<bool> AllowModesAsync(string strLanguage, CancellationToken token, params string[] astrModes)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.Language;
            using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                       out HashSet<string> setModes))
            {
                token.ThrowIfCancellationRequested();
                setModes.AddRange(astrModes);
                token.ThrowIfCancellationRequested();
                return (await CalculatedModeAsync(strLanguage, token: token).ConfigureAwait(false)).SplitNoAlloc('/').Any(x => setModes.Contains(x));
            }
        }

        /// <summary>
        /// Weapon Cost to use when working with Total Cost price modifiers for Weapon Mods.
        /// </summary>
        public decimal MultipliableCost(WeaponAccessory objExcludeAccessory, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return OwnCost
                   // Run through the list of Weapon Mods.
                   + WeaponAccessories.Sum(x => objExcludeAccessory != x && x.Equipped && !x.IncludedInWeapon,
                       x => x.TotalCost, token);
        }

        /// <summary>
        /// Weapon Cost to use when working with Total Cost price modifiers for Weapon Mods.
        /// </summary>
        public async Task<decimal> MultipliableCostAsync(WeaponAccessory objExcludeAccessory,
            CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await GetOwnCostAsync(token).ConfigureAwait(false)
                   // Run through the list of Weapon Mods.
                   + await WeaponAccessories
                       .SumAsync(x => objExcludeAccessory != x && x.Equipped && !x.IncludedInWeapon,
                           x => x.GetTotalCostAsync(token), token).ConfigureAwait(false);
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

        public IEnumerable<string> GetAccessoryMounts(bool blnWithInternalAndNone = true)
        {
            string strSlots = ModificationSlots;
            if (string.IsNullOrEmpty(strSlots))
            {
                if (blnWithInternalAndNone)
                    yield return "None";
                yield break;
            }
            int intOldValue;
            Dictionary<string, int> dicMounts = new Dictionary<string, int>();
            foreach (string strMount in strSlots.SplitNoAlloc(
                                '/', StringSplitOptions.RemoveEmptyEntries))
            {
                if (dicMounts.TryGetValue(strMount, out intOldValue))
                    dicMounts[strMount] = intOldValue + 1;
                else
                    dicMounts.Add(strMount, 1);
            }
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                if (!objAccessory.Equipped)
                    continue;
                string strLoop = objAccessory.AddMount;
                if (!string.IsNullOrEmpty(strLoop))
                {
                    foreach (string strMount in strLoop.SplitNoAlloc(
                                '/', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (dicMounts.TryGetValue(strMount, out intOldValue))
                            dicMounts[strMount] = intOldValue + 1;
                        else
                            dicMounts.Add(strMount, 1);
                    }
                }
                strLoop = objAccessory.Mount;
                if (!string.IsNullOrEmpty(strLoop) && dicMounts.TryGetValue(strLoop, out intOldValue))
                {
                    dicMounts[strLoop] = intOldValue - 1;
                }
                strLoop = objAccessory.ExtraMount;
                if (!string.IsNullOrEmpty(strLoop) && dicMounts.TryGetValue(strLoop, out intOldValue))
                {
                    dicMounts[strLoop] = intOldValue - 1;
                }
            }
            foreach (Weapon objWeapon in UnderbarrelWeapons)
            {
                if (!objWeapon.Equipped)
                    continue;
                string strLoop = objWeapon.Mount;
                if (!string.IsNullOrEmpty(strLoop) && dicMounts.TryGetValue(strLoop, out intOldValue))
                {
                    dicMounts[strLoop] = intOldValue - 1;
                }
                strLoop = objWeapon.ExtraMount;
                if (!string.IsNullOrEmpty(strLoop) && dicMounts.TryGetValue(strLoop, out intOldValue))
                {
                    dicMounts[strLoop] = intOldValue - 1;
                }
            }

            foreach (KeyValuePair<string, int> kvpMount in dicMounts)
            {
                for (int i = 0; i < kvpMount.Value; ++i)
                {
                    yield return kvpMount.Key;
                }
            }
            if (blnWithInternalAndNone)
            {
                if (!dicMounts.ContainsKey("Internal"))
                    yield return "Internal";
                if (!dicMounts.ContainsKey("None"))
                    yield return "None";
            }
        }

        public async Task<List<string>> GetAccessoryMountsAsync(bool blnWithInternalAndNone = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSlots = ModificationSlots;
            if (string.IsNullOrEmpty(strSlots))
            {
                return blnWithInternalAndNone
                    ? new List<string>(1)
                        {
                            "None"
                        }
                    : new List<string>();
            }
            List<string> lstReturn = new List<string>(blnWithInternalAndNone ? 3 : 1);
            int intOldValue;
            Dictionary<string, int> dicMounts = new Dictionary<string, int>();
            foreach (string strMount in strSlots.SplitNoAlloc(
                             '/', StringSplitOptions.RemoveEmptyEntries))
            {
                token.ThrowIfCancellationRequested();
                if (dicMounts.TryGetValue(strMount, out intOldValue))
                    dicMounts[strMount] = intOldValue + 1;
                else
                    dicMounts.Add(strMount, 1);
            }
            await WeaponAccessories.ForEachAsync(objAccessory =>
            {
                if (!objAccessory.Equipped)
                    return;
                string strLoop = objAccessory.AddMount;
                if (!string.IsNullOrEmpty(strLoop))
                {
                    foreach (string strMount in strLoop.SplitNoAlloc(
                                '/', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (dicMounts.TryGetValue(strMount, out intOldValue))
                            dicMounts[strMount] = intOldValue + 1;
                        else
                            dicMounts.Add(strMount, 1);
                    }
                }
                strLoop = objAccessory.Mount;
                if (!string.IsNullOrEmpty(strLoop) && dicMounts.TryGetValue(strLoop, out intOldValue))
                {
                    dicMounts[strLoop] = intOldValue - 1;
                }
                strLoop = objAccessory.ExtraMount;
                if (!string.IsNullOrEmpty(strLoop) && dicMounts.TryGetValue(strLoop, out intOldValue))
                {
                    dicMounts[strLoop] = intOldValue - 1;
                }
            }, token).ConfigureAwait(false);
            await UnderbarrelWeapons.ForEachAsync(objWeapon =>
            {
                if (!objWeapon.Equipped)
                    return;
                string strLoop = objWeapon.Mount;
                if (!string.IsNullOrEmpty(strLoop) && dicMounts.TryGetValue(strLoop, out intOldValue))
                {
                    dicMounts[strLoop] = intOldValue - 1;
                }
                strLoop = objWeapon.ExtraMount;
                if (!string.IsNullOrEmpty(strLoop) && dicMounts.TryGetValue(strLoop, out intOldValue))
                {
                    dicMounts[strLoop] = intOldValue - 1;
                }
            }, token).ConfigureAwait(false);

            foreach (KeyValuePair<string, int> kvpMount in dicMounts)
            {
                token.ThrowIfCancellationRequested();
                for (int i = 0; i < kvpMount.Value; ++i)
                {
                    token.ThrowIfCancellationRequested();
                    lstReturn.Add(kvpMount.Key);
                }
            }

            if (blnWithInternalAndNone)
            {
                if (!dicMounts.ContainsKey("Internal"))
                    lstReturn.Add("Internal");
                if (!dicMounts.ContainsKey("None"))
                    lstReturn.Add("None");
            }
            return lstReturn;
        }

        public string DisplayAccessoryMounts(string strLanguage)
        {
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.DefaultLanguage;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdMounts))
            {
                foreach (string strMount in GetAccessoryMounts())
                {
                    sbdMounts.Append(strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                        ? strMount
                        : LanguageManager.GetString("String_Mount" + strMount))
                        .Append('/');
                }
                sbdMounts.Length -= 1; // Trim off the last slash
                return sbdMounts.ToString();
            }
        }

        public async Task<string> DisplayAccessoryMountsAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalSettings.DefaultLanguage;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdMounts))
            {
                token.ThrowIfCancellationRequested();
                foreach (string strMount in await GetAccessoryMountsAsync(token: token).ConfigureAwait(false))
                {
                    token.ThrowIfCancellationRequested();
                    sbdMounts.Append(strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase)
                        ? strMount
                        : await LanguageManager.GetStringAsync("String_Mount" + strMount, token: token).ConfigureAwait(false))
                        .Append('/');
                }
                token.ThrowIfCancellationRequested();
                sbdMounts.Length -= 1; // Trim off the last slash
                return sbdMounts.ToString();
            }
        }

        public string CurrentDisplayAccessoryMounts => DisplayAccessoryMounts(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayAccessoryMounts(CancellationToken token = default) => DisplayAlternateRangeAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The Weapon's total cost including Accessories and Modifications.
        /// </summary>
        public decimal TotalCost => OwnCost + WeaponAccessories.Sum(x => x.TotalCost) + Children.Sum(x => x.TotalCost);

        /// <summary>
        /// The Weapon's total cost including Accessories and Modifications.
        /// </summary>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await GetOwnCostAsync(token).ConfigureAwait(false)
                   // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
                   + await WeaponAccessories
                       .SumAsync(objAccessory => objAccessory.GetTotalCostAsync(token), token)
                       .ConfigureAwait(false)
                   // Include the cost of any Underbarrel Weapon.
                   + await Children
                       .SumAsync(objUnderbarrel => objUnderbarrel.GetTotalCostAsync(token),
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
                string strCostExpression = Cost;
                if (strCostExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                    {
                        sbdCost.Append(strCostExpression.TrimStart('+'));
                        _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdCost, strCostExpression);
                        sbdCost.CheapReplace(strCostExpression, "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                        (bool blnIsSuccess, object objProcess)
                            = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString());
                        if (blnIsSuccess)
                            decReturn = Convert.ToDecimal((double)objProcess);
                    }
                }

                if (DiscountCost)
                    decReturn *= 0.9m;

                if (!string.IsNullOrEmpty(Parent?.DoubledCostModificationSlots))
                {
                    bool blnBreakAfterFound = string.IsNullOrEmpty(Mount) || string.IsNullOrEmpty(ExtraMount);
                    foreach (string strDoubledCostSlot in Parent.DoubledCostModificationSlots.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (strDoubledCostSlot == Mount || strDoubledCostSlot == ExtraMount)
                        {
                            decReturn *= 2;
                            if (blnBreakAfterFound)
                                break;
                            else
                                blnBreakAfterFound = true;
                        }
                    }
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Weapon itself.
        /// </summary>
        public async Task<decimal> GetOwnCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            // If this is a Cyberware or Gear Weapon, remove the Weapon Cost from this since it has already been paid for through the parent item (but is needed to calculate Mod price).
            if (Cyberware || Category == "Gear")
                return 0;
            string strCostExpression = Cost;
            if (strCostExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReturn))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCostExpression.TrimStart('+'));
                    await _objCharacter.AttributeSection
                        .ProcessAttributesInXPathAsync(sbdCost, strCostExpression, token: token).ConfigureAwait(false);

                    await sbdCost
                        .CheapReplaceAsync(strCostExpression, "{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                            token: token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdCost.ToString(), token)
                            .ConfigureAwait(false);
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal((double)objProcess);
                }
            }

            if (DiscountCost)
                decReturn *= 0.9m;

            if (!string.IsNullOrEmpty(Parent?.DoubledCostModificationSlots))
            {
                bool blnBreakAfterFound = string.IsNullOrEmpty(Mount) || string.IsNullOrEmpty(ExtraMount);
                foreach (string strDoubledCostSlot in Parent.DoubledCostModificationSlots.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (strDoubledCostSlot == Mount || strDoubledCostSlot == ExtraMount)
                    {
                        decReturn *= 2;
                        if (blnBreakAfterFound)
                            break;
                        else
                            blnBreakAfterFound = true;
                    }
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
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdWeight))
                {
                    sbdWeight.Append(strWeightExpression.TrimStart('+'));
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdWeight, strWeightExpression);
                    sbdWeight.CheapReplace(strWeightExpression, "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    (bool blnIsSuccess, object objProcess) =
                        CommonFunctions.EvaluateInvariantXPath(sbdWeight.ToString());
                    if (blnIsSuccess)
                        decReturn = Convert.ToDecimal((double)objProcess);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The Weapon's total AP including Ammunition in the program's current language.
        /// </summary>
        public string DisplayTotalAP => TotalAP(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// The Weapon's total AP including Ammunition in the program's current language.
        /// </summary>
        public Task<string> GetDisplayTotalAPAsync(CancellationToken token = default) => TotalAPAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token: token);

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
        public Task<string> TotalAPAsync(CultureInfo objCulture, string strLanguage, bool blnIncludeAmmo = true,
            CancellationToken token = default)
        {
            return TotalAPCoreAsync(false, objCulture, strLanguage, blnIncludeAmmo, token);
        }

        /// <summary>
        /// The Weapon's total AP including Ammunition.
        /// </summary>
        private async Task<string> TotalAPCoreAsync(bool blnSync, CultureInfo objCulture, string strLanguage,
            bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            string strAP = AP;

            int intImprove = 0;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdBonusAP))
            {
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    // Change the Weapon's Damage Type.
                    string strAPReplace = WirelessWeaponBonus["apreplace"]?.InnerText;
                    if (!string.IsNullOrEmpty(strAPReplace))
                        strAP = strAPReplace;
                    // Adjust the Weapon's Damage.
                    string strAPAdd = WirelessWeaponBonus["ap"]?.InnerText;
                    if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                        sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                }

                if (blnSync)
                {
                    foreach (WeaponAccessory objAccessory in WeaponAccessories)
                    {
                        if (!objAccessory.Equipped)
                            continue;
                        // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                        if (!(objAccessory.DamageType.Contains("(f)") && Damage.Contains("(f)")))
                        {
                            // Change the Weapon's AP value.
                            string strAPReplace = objAccessory.APReplacement;
                            if (!string.IsNullOrEmpty(strAPReplace))
                            {
                                strAPReplace = strAPReplace
                                    .CheapReplace("{Rating}", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                strAP = strAPReplace;
                            }
                            // Adjust the Weapon's AP value.
                            string strAPAdd = objAccessory.AP;
                            if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                            {
                                strAPAdd = strAPAdd
                                    .CheapReplace("{Rating}", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                            }

                            if (objAccessory.WirelessOn && WirelessOn && objAccessory.WirelessWeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                strAPReplace = objAccessory.WirelessWeaponBonus["apreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPReplace))
                                {
                                    strAPReplace = strAPReplace
                                        .CheapReplace("{Rating}", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    strAP = strAPReplace;
                                }
                                // Adjust the Weapon's Damage.
                                strAPAdd = objAccessory.WirelessWeaponBonus["ap"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                                {
                                    strAPAdd = strAPAdd
                                        .CheapReplace("{Rating}", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                                }
                            }
                        }
                    }
                }
                else
                {
                    await WeaponAccessories.ForEachAsync(async objAccessory =>
                    {
                        if (!objAccessory.Equipped)
                            return;
                        // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                        if (!(objAccessory.DamageType.Contains("(f)") && Damage.Contains("(f)")))
                        {
                            // Change the Weapon's AP value.
                            string strAPReplace = objAccessory.APReplacement;
                            if (!string.IsNullOrEmpty(strAPReplace))
                            {
                                strAPReplace = await strAPReplace
                                    .CheapReplaceAsync("{Rating}", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                strAP = strAPReplace;
                            }
                            // Adjust the Weapon's AP value.
                            string strAPAdd = objAccessory.AP;
                            if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                            {
                                strAPAdd = await strAPAdd
                                    .CheapReplaceAsync("{Rating}", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                            }

                            if (objAccessory.WirelessOn && WirelessOn && objAccessory.WirelessWeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                strAPReplace = objAccessory.WirelessWeaponBonus["apreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPReplace))
                                {
                                    strAPReplace = await strAPReplace
                                        .CheapReplaceAsync("{Rating}", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    strAP = strAPReplace;
                                }
                                // Adjust the Weapon's Damage.
                                strAPAdd = objAccessory.WirelessWeaponBonus["ap"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                                {
                                    strAPAdd = await strAPAdd
                                        .CheapReplaceAsync("{Rating}", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                                }
                            }
                        }
                    }, token).ConfigureAwait(false);
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
                            // Change the Weapon's Damage Type.
                            string strAPReplace = objGear.FlechetteWeaponBonus["apreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPReplace))
                            {
                                strAPReplace = blnSync
                                    ? strAPReplace
                                        .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    : await strAPReplace
                                        .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                strAP = strAPReplace;
                            }
                            // Adjust the Weapon's Damage.
                            string strAPAdd = objGear.FlechetteWeaponBonus["ap"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                            {
                                strAPAdd = blnSync
                                    ? strAPAdd
                                        .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    : await strAPAdd
                                        .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                            }
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAPReplace = objGear.WeaponBonus["apreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPReplace))
                            {
                                strAPReplace = blnSync
                                    ? strAPReplace
                                        .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    : await strAPReplace
                                        .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                strAP = strAPReplace;
                            }
                            // Adjust the Weapon's Damage.
                            string strAPAdd = objGear.WeaponBonus["ap"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                            {
                                strAPAdd = blnSync
                                    ? strAPAdd
                                        .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    : await strAPAdd
                                        .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in blnSync
                                     ? objGear.Children.DeepWhere(x => x.Children.Where(y => y.Equipped), x => x.Equipped, token)
                                     : await objGear.Children.DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token)
                                         .ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" &&
                                objChild.FlechetteWeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAPReplace = objChild.FlechetteWeaponBonus["apreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPReplace))
                                {
                                    strAPReplace = blnSync
                                        ? strAPReplace
                                            .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                            .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        : await strAPReplace
                                            .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    strAP = strAPReplace;
                                }
                                // Adjust the Weapon's Damage.
                                string strAPAdd = objChild.FlechetteWeaponBonus["ap"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                                {
                                    strAPAdd = blnSync
                                        ? strAPAdd
                                            .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                            .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        : await strAPAdd
                                            .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAPReplace = objChild.WeaponBonus["apreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPReplace))
                                {
                                    strAPReplace = blnSync
                                        ? strAPReplace
                                            .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                            .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        : await strAPReplace
                                            .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    strAP = strAPReplace;
                                }
                                // Adjust the Weapon's Damage.
                                string strAPAdd = objChild.WeaponBonus["ap"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAPAdd) && strAPAdd != "0" && strAPAdd != "+0" && strAPAdd != "-0")
                                {
                                    strAPAdd = blnSync
                                        ? strAPAdd
                                            .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                            .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        : await strAPAdd
                                            .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    sbdBonusAP.Append(" + ").Append(strAPAdd.TrimStartOnce('+'));
                                }
                            }
                        }
                    }
                }

                if (_objCharacter != null)
                {
                    Skill objSkill = blnSync ? Skill : await GetSkillAsync(token).ConfigureAwait(false);
                    string strSkillDictionaryKey = objSkill != null
                        ? blnSync
                            ? objSkill.DictionaryKey
                            : await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                        : string.Empty;
                    if (Name == "Unarmed Attack" || strSkillDictionaryKey == "Unarmed Combat" &&
                        _objCharacter.Settings.UnarmedImprovementsApplyToWeapons)
                    {
                        // Add any UnarmedAP bonus for the Unarmed Attack item.
                        intImprove += (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedAP,
                                    token: token)
                                : await ImprovementManager.ValueOfAsync(
                                        _objCharacter, Improvement.ImprovementType.UnarmedAP, token: token)
                                    .ConfigureAwait(false))
                            .StandardRound();
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
                            .CheapReplace("-half",
                                () => LanguageManager.GetString("String_APHalf", strLanguage, token: token)),
                        strLanguage,
                        token)
                    : await ReplaceStringsAsync(await strAP.Replace("//", "/")
                        .CheapReplaceAsync(
                            "-half",
                            () => LanguageManager.GetStringAsync("String_APHalf", strLanguage, token: token),
                            token: token).ConfigureAwait(false), strLanguage, token).ConfigureAwait(false);
            }

            int intAP;
            if (strAP.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decAP))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAP))
                {
                    sbdAP.Append(strAP);
                    if (blnSync)
                    {
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        sbdAP.CheapReplace(strAP, "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        ProcessAttributesInXPath(sbdAP, strAP);
                    }
                    else
                    {
                        await sbdAP.CheapReplaceAsync(strAP, "{Rating}",
                            async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        await ProcessAttributesInXPathAsync(sbdAP, strAP, token: token).ConfigureAwait(false);
                    }

                    try
                    {
                        (bool blnIsSuccess, object objProcess) = blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? CommonFunctions.EvaluateInvariantXPath(sbdAP.ToString(), token)
                            : await CommonFunctions.EvaluateInvariantXPathAsync(sbdAP.ToString(), token)
                                .ConfigureAwait(false);
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
                                        "-half",
                                        () => LanguageManager.GetString("String_APHalf", strLanguage, token: token)),
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
            }
            else
                intAP = decAP.StandardRound();

            intAP += intImprove;
            if (intAP == 0)
                return "-";
            if (intAP > 0)
                return '+' + intAP.ToString(objCulture);
            return intAP.ToString(objCulture);
        }

        public Tuple<string, string> DisplayTotalRC => TotalRC(GlobalSettings.CultureInfo, GlobalSettings.Language, true);

        public Task<Tuple<string, string>> GetDisplayTotalRCAsync(CancellationToken token = default) => TotalRCAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, true, token: token);

        /// <summary>
        /// The Weapon's total RC including Accessories and Modifications. The first item is the RC, the second is the tooltip.
        /// </summary>
        public Tuple<string, string> TotalRC(CultureInfo objCulture, string strLanguage, bool blnWithTooltip = false,
            bool blnIncludeAmmo = true)
        {
            return Utils.SafelyRunSynchronously(() =>
                TotalRCCoreAsync(true, objCulture, strLanguage, blnWithTooltip, blnIncludeAmmo));
        }

        /// <summary>
        /// The Weapon's total RC including Accessories and Modifications. The first item is the RC, the second is the tooltip.
        /// </summary>
        public Task<Tuple<string, string>> TotalRCAsync(CultureInfo objCulture, string strLanguage, bool blnWithTooltip = false,
            bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            return TotalRCCoreAsync(false, objCulture, strLanguage, blnWithTooltip, blnIncludeAmmo, token);
        }

        /// <summary>
        /// The Weapon's total RC including Accessories and Modifications. The first item is the RC, the second is the tooltip.
        /// </summary>
        private async Task<Tuple<string, string>> TotalRCCoreAsync(bool blnSync, CultureInfo objCulture, string strLanguage,
            bool blnWithTooltip, bool blnIncludeAmmo = true, CancellationToken token = default)
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
                : await strRC.CheapReplaceAsync("{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                    token: token).ConfigureAwait(false);
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
                    strRCFull = strRC.Substring(intPos);
                }
            }
            else
            {
                // The string contains only RC from fixed pieces - "x" only.
                strRCBase = strRC;
                strRCFull = strRC;
            }

            string strTooltip = string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdRCTip))
            {
                if (blnWithTooltip)
                {
                    sbdRCTip.Append(1.ToString(GlobalSettings.CultureInfo)).Append(strSpace);
                    if (strRCBase != "0" && strRCBase != "+0")
                    {
                        sbdRCTip.Append('+').Append(strSpace)
                            .Append(blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? LanguageManager.GetString("Label_Base", strLanguage, token: token)
                                : await LanguageManager.GetStringAsync("Label_Base", strLanguage, token: token)
                                    .ConfigureAwait(false))
                            .Append('(').Append(strRCBase.TrimStartOnce('+')).Append(')');
                    }
                }

                int.TryParse(strRCBase, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intRCBase);
                int.TryParse(strRCFull.Trim('(', ')'), NumberStyles.Any, GlobalSettings.InvariantCultureInfo,
                    out int intRCFull);

                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    string strRCBonus = WirelessWeaponBonus["rc"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRCBonus) && int.TryParse(strRCBonus, out int intLoopRCBonus))
                    {
                        intRCBase += intLoopRCBonus;
                        intRCFull += intLoopRCBonus;

                        if (blnWithTooltip)
                            sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                .Append(blnSync
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    ? LanguageManager.GetString("String_Wireless", strLanguage, token: token)
                                    : await LanguageManager.GetStringAsync("String_Wireless", strLanguage, token: token)
                                        .ConfigureAwait(false))
                                .Append(strSpace)
                                .Append('(').Append(strRCBonus.TrimStartOnce('+')).Append(')');
                    }
                }

                // Now that we know the Weapon's RC values, run through all of the Accessories and add theirs to the mix.
                // Only add in the values for items that do not come with the weapon.
                bool blnRestrictRecoil = blnSync
                    ? _objCharacter.Settings.RestrictRecoil
                    : await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).GetRestrictRecoilAsync(token).ConfigureAwait(false);
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (!objAccessory.Equipped || string.IsNullOrEmpty(objAccessory.RC))
                        continue;
                    if (blnRestrictRecoil && objAccessory.RCGroup != 0)
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
                                        : await objAccessory.DisplayNameAsync(strLanguage, token).ConfigureAwait(false),
                                    intItemRC);
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

                        if (blnWithTooltip)
                            sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                .Append(blnSync
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    ? objAccessory.DisplayName(strLanguage)
                                    : await objAccessory.DisplayNameAsync(strLanguage, token).ConfigureAwait(false))
                                .Append(strSpace).Append('(')
                                .Append(objAccessory.RC.TrimStartOnce('+')).Append(')');
                    }

                    if (objAccessory.WirelessOn && WirelessOn && objAccessory.WirelessWeaponBonus != null)
                    {
                        string strRCBonus = objAccessory.WirelessWeaponBonus["rc"]?.InnerText;
                        if (!string.IsNullOrEmpty(strRCBonus) && int.TryParse(strRCBonus, out int intLoopRCBonus))
                        {
                            intRCBase += intLoopRCBonus;
                            intRCFull += intLoopRCBonus;

                            if (blnWithTooltip)
                            {
                                sbdRCTip.Append(strSpace).Append('+').Append(strSpace);
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    sbdRCTip.Append(await objAccessory.DisplayNameAsync(strLanguage, token).ConfigureAwait(false))
                                        .Append(strSpace)
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        .Append(LanguageManager.GetString("String_Wireless", strLanguage, token: token));
                                }
                                else
                                {
                                    sbdRCTip.Append(objAccessory.DisplayName(strLanguage))
                                        .Append(strSpace)
                                        .Append(await LanguageManager.GetStringAsync("String_Wireless", strLanguage, token: token).ConfigureAwait(false));
                                }
                                sbdRCTip.Append(strSpace).Append('(').Append(strRCBonus.TrimStartOnce('+')).Append(')');
                            }
                        }
                    }
                }

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

                                if (blnWithTooltip)
                                    sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(blnSync
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            ? objGear.DisplayName(objCulture, strLanguage)
                                            : await objGear
                                                .DisplayNameAsync(objCulture, strLanguage, token: token)
                                                .ConfigureAwait(false))
                                        .Append(strSpace)
                                        .Append('(').Append(strRCBonus.TrimStartOnce('+')).Append(')');
                            }
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            string strRCBonus = objGear.WeaponBonus["rc"]?.InnerText;
                            if (!string.IsNullOrEmpty(strRCBonus) && int.TryParse(strRCBonus, out int intLoopRCBonus))
                            {
                                intRCBase += intLoopRCBonus;
                                intRCFull += intLoopRCBonus;

                                if (blnWithTooltip)
                                    sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(blnSync
                                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                            ? objGear.DisplayName(objCulture, strLanguage)
                                            : await objGear
                                                .DisplayNameAsync(objCulture, strLanguage, token: token)
                                                .ConfigureAwait(false))
                                        .Append(strSpace)
                                        .Append('(').Append(strRCBonus.TrimStartOnce('+')).Append(')');
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in blnSync
                                     ? objGear.Children.DeepWhere(x => x.Children.Where(y => y.Equipped), x => x.Equipped, token)
                                     : await objGear.Children.DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token)
                                         .ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" &&
                                objChild.FlechetteWeaponBonus != null)
                            {
                                string strRCBonus = objChild.FlechetteWeaponBonus["rc"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRCBonus) &&
                                    int.TryParse(strRCBonus, out int intLoopRCBonus))
                                {
                                    intRCBase += intLoopRCBonus;
                                    intRCFull += intLoopRCBonus;

                                    if (blnWithTooltip)
                                        sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(blnSync
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                ? objChild.DisplayName(objCulture, strLanguage)
                                                : await objChild
                                                    .DisplayNameAsync(objCulture, strLanguage, token: token)
                                                    .ConfigureAwait(false))
                                            .Append(strSpace)
                                            .Append('(').Append(strRCBonus.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                string strRCBonus = objChild.WeaponBonus["rc"]?.InnerText;
                                if (!string.IsNullOrEmpty(strRCBonus) &&
                                    int.TryParse(strRCBonus, out int intLoopRCBonus))
                                {
                                    intRCBase += intLoopRCBonus;
                                    intRCFull += intLoopRCBonus;

                                    if (blnWithTooltip)
                                        sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(blnSync
                                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                ? objChild.DisplayName(objCulture, strLanguage)
                                                : await objChild
                                                    .DisplayNameAsync(objCulture, strLanguage, token: token)
                                                    .ConfigureAwait(false))
                                            .Append(strSpace)
                                            .Append('(').Append(strRCBonus.TrimStartOnce('+')).Append(')');
                                }
                            }
                        }
                    }
                }

                foreach ((string strGroup, int intRecoil) in lstRCGroups)
                {
                    if (!string.IsNullOrEmpty(strGroup))
                    {
                        // Add in the Recoil Group bonuses.
                        intRCBase += intRecoil;
                        intRCFull += intRecoil;
                        if (blnWithTooltip)
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
                        if (blnWithTooltip)
                            sbdRCTip.Append(strSpace).Append('+').Append(strSpace).AppendFormat(
                                objCulture,
                                blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? LanguageManager.GetString("Tip_RecoilAccessories", strLanguage, token: token)
                                    : await LanguageManager
                                        .GetStringAsync("Tip_RecoilAccessories", strLanguage, token: token)
                                        .ConfigureAwait(false),
                                strGroup,
                                intRecoil);
                    }
                }

                int intUseSTR = 0;
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = blnSync
                            ? ParentVehicle.TotalBody
                            : await ParentVehicle.GetTotalBodyAsync(token).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(ParentID))
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent;
                            VehicleMod objVehicleMod;
                            if (blnSync)
                                objWeaponParent =
                                    _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID,
                                        out objVehicleMod);
                            else
                                (objWeaponParent, objVehicleMod) =
                                    await _objCharacter.Vehicles.FindVehicleCyberwareAsync(
                                        x => x.InternalId == ParentID, token).ConfigureAwait(false);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                int intSTR = blnSync
                                    ? objAttributeSource.GetAttributeTotalValue("STR")
                                    : await objAttributeSource.GetAttributeTotalValueAsync("STR", token)
                                        .ConfigureAwait(false);
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                int intAGI = blnSync
                                    ? objAttributeSource.GetAttributeTotalValue("AGI")
                                    : await objAttributeSource.GetAttributeTotalValueAsync("AGI", token)
                                        .ConfigureAwait(false);
                                while (objAttributeSource != null)
                                {
                                    if (intSTR != 0 || intAGI != 0)
                                        break;
                                    objAttributeSource = blnSync ? objAttributeSource.Parent : await objAttributeSource.GetParentAsync(token).ConfigureAwait(false);
                                    if (objAttributeSource == null) continue;
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    intSTR = blnSync
                                        ? objAttributeSource.GetAttributeTotalValue("STR")
                                        : await objAttributeSource.GetAttributeTotalValueAsync("STR", token)
                                            .ConfigureAwait(false);
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    intAGI = blnSync
                                        ? objAttributeSource.GetAttributeTotalValue("AGI")
                                        : await objAttributeSource.GetAttributeTotalValueAsync("AGI", token)
                                            .ConfigureAwait(false);
                                }

                                intUseSTR = intSTR;

                                if (intUseSTR == 0)
                                    intUseSTR = blnSync
                                        ? objVehicleMod.TotalStrength
                                        : await objVehicleMod.GetTotalStrengthAsync(token).ConfigureAwait(false);
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent
                            = blnSync
                                ? _objCharacter.Cyberware.DeepFirstOrDefault(
                                    x => x.Children, x => x.InternalId == ParentID, token)
                                : await _objCharacter.Cyberware.DeepFirstOrDefaultAsync(
                                    x => x.Children, x => x.InternalId == ParentID, token).ConfigureAwait(false);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            int intSTR = blnSync
                                ? objAttributeSource.GetAttributeTotalValue("STR")
                                : await objAttributeSource.GetAttributeTotalValueAsync("STR", token)
                                    .ConfigureAwait(false);
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            int intAGI = blnSync
                                ? objAttributeSource.GetAttributeTotalValue("AGI")
                                : await objAttributeSource.GetAttributeTotalValueAsync("AGI", token)
                                    .ConfigureAwait(false);
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = blnSync ? objAttributeSource.Parent : await objAttributeSource.GetParentAsync(token).ConfigureAwait(false);
                                if (objAttributeSource == null)
                                    continue;
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                intSTR = blnSync
                                    ? objAttributeSource.GetAttributeTotalValue("STR")
                                    : await objAttributeSource.GetAttributeTotalValueAsync("STR", token)
                                        .ConfigureAwait(false);
                            }

                            intUseSTR = intSTR;
                        }

                        if (intUseSTR == 0)
                            intUseSTR = blnSync
                                ? _objCharacter.STR.TotalValue
                                : await (await _objCharacter.GetAttributeAsync("STR", token: token)
                                    .ConfigureAwait(false)).GetTotalValueAsync(token).ConfigureAwait(false);
                    }
                }
                else if (ParentVehicle == null)
                {
                    intUseSTR = blnSync
                        ? _objCharacter.STR.TotalValue
                        : await (await _objCharacter.GetAttributeAsync("STR", token: token).ConfigureAwait(false))
                            .GetTotalValueAsync(token).ConfigureAwait(false);
                }

                Skill objSkill = blnSync ? Skill : await GetSkillAsync(token).ConfigureAwait(false);
                string strSkillDictionaryKey = objSkill != null
                    ? blnSync
                        ? objSkill.DictionaryKey
                        : await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                    : string.Empty;
                if (Category == "Throwing Weapons" || strSkillDictionaryKey == "Throwing Weapons")
                    intUseSTR += (blnSync
                            // ReSharper disable once MethodHasAsyncOverload
                            ? ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR,
                                token: token)
                            : await ImprovementManager.ValueOfAsync(_objCharacter,
                                Improvement.ImprovementType.ThrowSTR, token: token).ConfigureAwait(false))
                        .StandardRound();

                int intStrRC = intUseSTR.DivAwayFromZero(3);

                intRCBase += intStrRC + 1;
                intRCFull += intStrRC + 1;
                if (blnWithTooltip)
                    sbdRCTip.Append(strSpace).Append('+').Append(strSpace)
                        .Append(blnSync
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            ? _objCharacter.STR.GetDisplayAbbrev(strLanguage)
                            : await (await _objCharacter.GetAttributeAsync("STR", token: token).ConfigureAwait(false)).GetDisplayAbbrevAsync(strLanguage, token).ConfigureAwait(false))
                        .Append(strSpace)
                        .Append('[')
                        .Append(intUseSTR.ToString(objCulture)).Append(strSpace).Append('/').Append(strSpace)
                        .Append(3.ToString(objCulture)).Append(strSpace).Append('=').Append(strSpace)
                        .Append(intStrRC.ToString(objCulture)).Append(']');
                // If the full RC is not higher than the base, only the base value is shown.
                strRC = intRCBase.ToString(objCulture);
                if (intRCFull > intRCBase)
                    strRC += strSpace + '(' + intRCFull.ToString(objCulture) + ')';

                if (blnWithTooltip)
                    strTooltip = sbdRCTip.ToString();
            }

            return new Tuple<string, string>(strRC, strTooltip);
        }

        /// <summary>
        /// The full Reach of the Weapons including the Character's Reach.
        /// </summary>
        public int TotalReach
        {
            get
            {
                string strReach = Reach;
                if (strReach.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReach)
                    || WeaponAccessories.Any(x => x.Equipped && !string.IsNullOrEmpty(x.Reach)))
                {
                    string strToEvaluate;
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReach))
                    {
                        if (!string.IsNullOrEmpty(strReach))
                            sbdReach.Append('(').Append(strReach.TrimStartOnce('+')).Append(')');
                        foreach (WeaponAccessory objAccessory in WeaponAccessories)
                        {
                            if (objAccessory.Equipped && !string.IsNullOrEmpty(objAccessory.Reach))
                            {
                                string strLoopReach = objAccessory.Reach.TrimStartOnce('+')
                                    .CheapReplace("{Rating}", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                sbdReach.Append(sbdReach.Length > 0 ? " + (" : "(").Append(strLoopReach).Append(')');
                            }
                        }

                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        sbdReach.CheapReplace("{Rating}", strReach, () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        ProcessAttributesInXPath(sbdReach);
                        strToEvaluate = sbdReach.ToString();
                    }
                    try
                    {
                        (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strToEvaluate);
                        if (blnIsSuccess)
                            decReach = Convert.ToDecimal((double)objProcess);
                    }
                    catch (OverflowException)
                    {
                        // swallow this
                    }
                    catch (InvalidCastException)
                    {
                        // swallow this
                    }
                }
                if (RangeType == "Melee")
                {
                    // Run through the Character's Improvements and add any Reach Improvements.
                    decReach += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Reach,
                        strImprovedName: Name, blnIncludeNonImproved: true);
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
        public async Task<int> GetTotalReachAsync(CancellationToken token = default)
        {
            string strReach = Reach;
            if (strReach.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decReach)
                || await WeaponAccessories.AnyAsync(x => x.Equipped && !string.IsNullOrEmpty(x.Reach), token).ConfigureAwait(false))
            {
                string strToEvaluate;
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReach))
                {
                    if (!string.IsNullOrEmpty(strReach))
                        sbdReach.Append('(').Append(strReach.TrimStartOnce('+')).Append(')');
                    await WeaponAccessories.ForEachAsync(async objAccessory =>
                    {
                        if (objAccessory.Equipped && !string.IsNullOrEmpty(objAccessory.Reach))
                        {
                            string strLoopReach = await objAccessory.Reach.TrimStartOnce('+')
                                .CheapReplaceAsync("{Rating}", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplaceAsync("Rating", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo));
                            sbdReach.Append(sbdReach.Length > 0 ? " + (" : "(").Append(strLoopReach).Append(')');
                        }
                    }, token).ConfigureAwait(false);

                    await sbdReach.CheapReplaceAsync("{Rating}", strReach, async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await ProcessAttributesInXPathAsync(sbdReach, token: token).ConfigureAwait(false);
                    strToEvaluate = sbdReach.ToString();
                }
                try
                {
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strToEvaluate, token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        decReach = Convert.ToDecimal((double)objProcess);
                }
                catch (OverflowException)
                {
                    // swallow this
                }
                catch (InvalidCastException)
                {
                    // swallow this
                }
            }
            if (RangeType == "Melee")
            {
                // Run through the Character's Improvements and add any Reach Improvements.
                decReach += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.Reach,
                    strImprovedName: Name, blnIncludeNonImproved: true, token: token).ConfigureAwait(false);
            }

            if (await _objCharacter.Settings.GetUnarmedImprovementsApplyToWeaponsAsync(token).ConfigureAwait(false))
            {
                Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                string strSkillDictionaryKey = objSkill != null
                    ? await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                    : string.Empty;
                if (Name == "Unarmed Attack" || strSkillDictionaryKey == "Unarmed Combat")
                {
                    decReach += await ImprovementManager
                        .ValueOfAsync(_objCharacter, Improvement.ImprovementType.UnarmedReach, token: token)
                        .ConfigureAwait(false);
                }
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
            Func<string> funcPhysicalLimitString;
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
            else
                funcPhysicalLimitString = () => _objCharacter.LimitPhysical.ToString(GlobalSettings.InvariantCultureInfo);
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdBonusAccuracy))
            {
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    // Change the Weapon's Damage Type.
                    string strAccuracyReplace = WirelessWeaponBonus["accuracyreplace"]?.InnerText;
                    if (!string.IsNullOrEmpty(strAccuracyReplace))
                        strAccuracy = strAccuracyReplace;
                    // Adjust the Weapon's Damage.
                    string strAccuracyAdd = WirelessWeaponBonus["accuracy"]?.InnerText;
                    if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                        sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                }

                List<string> lstNonStackingAccessoryBonuses = new List<string>();
                foreach (WeaponAccessory objWeaponAccessory in WeaponAccessories)
                {
                    if (objWeaponAccessory.Equipped)
                    {
                        string strLoopAccuracy = objWeaponAccessory.Accuracy;
                        if (!string.IsNullOrEmpty(strLoopAccuracy) && strLoopAccuracy != "0" && strLoopAccuracy != "+0" && strLoopAccuracy != "-0")
                        {
                            strLoopAccuracy = strLoopAccuracy
                                .CheapReplace("{Rating}", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace("Rating", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            if (!objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && !objWeaponAccessory.Name.Contains("Sight"))
                                sbdBonusAccuracy.Append(" + ").Append(strLoopAccuracy.TrimStartOnce('+'));
                            else
                                lstNonStackingAccessoryBonuses.Add(strLoopAccuracy);
                        }
                        if (objWeaponAccessory.WirelessOn && WirelessOn && objWeaponAccessory.WirelessWeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objWeaponAccessory.WirelessWeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                            {
                                strAccuracyReplace = strAccuracyReplace
                                    .CheapReplace("{Rating}", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                strAccuracy = strAccuracyReplace;
                            }
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objWeaponAccessory.WirelessWeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                            {
                                strAccuracyAdd = strAccuracyAdd
                                    .CheapReplace("{Rating}", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                if (!objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && !objWeaponAccessory.Name.Contains("Sight"))
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                                else if (!string.IsNullOrEmpty(objWeaponAccessory.Accuracy))
                                    lstNonStackingAccessoryBonuses[lstNonStackingAccessoryBonuses.Count - 1] = lstNonStackingAccessoryBonuses[lstNonStackingAccessoryBonuses.Count - 1] + " + " + strAccuracyAdd.TrimStartOnce('+');
                                else
                                    lstNonStackingAccessoryBonuses.Add(strAccuracyAdd);
                            }
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
                            string strLoopAccuracy = objWeaponAccessory.Accuracy;
                            if (!string.IsNullOrEmpty(strLoopAccuracy))
                            {
                                strLoopAccuracy = strLoopAccuracy
                                    .CheapReplace("{Rating}", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                lstNonStackingAccessoryBonuses.Add(strLoopAccuracy);
                            }
                        }
                    }
                }
                if (lstNonStackingAccessoryBonuses.Count > 0)
                {
                    if (lstNonStackingAccessoryBonuses.Count > 1)
                    {
                        int intIndexToUse = 0;
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAccuracy))
                        {
                            string strLoopAccuracy = lstNonStackingAccessoryBonuses[0];
                            sbdAccuracy.Append(strLoopAccuracy);
                            ProcessAttributesInXPath(sbdAccuracy, strLoopAccuracy);
                            sbdAccuracy.CheapReplace(strAccuracy, "Physical", funcPhysicalLimitString)
                                .CheapReplace(strAccuracy, "Missile", funcPhysicalLimitString);
                            (bool blnIsSuccess, object objProcess)
                                = CommonFunctions.EvaluateInvariantXPath(sbdAccuracy.ToString());
                            int intBestAccuracy = blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                            for (int i = 1; i < lstNonStackingAccessoryBonuses.Count; ++i)
                            {
                                sbdAccuracy.Clear();
                                strLoopAccuracy = lstNonStackingAccessoryBonuses[i];
                                sbdAccuracy.Append(strLoopAccuracy);
                                ProcessAttributesInXPath(sbdAccuracy, strLoopAccuracy);
                                sbdAccuracy.CheapReplace(strAccuracy, "Physical", funcPhysicalLimitString)
                                    .CheapReplace(strAccuracy, "Missile", funcPhysicalLimitString);
                                (blnIsSuccess, objProcess)
                                    = CommonFunctions.EvaluateInvariantXPath(sbdAccuracy.ToString());
                                if (blnIsSuccess)
                                {
                                    int intLoopAccuracy = ((double)objProcess).StandardRound();
                                    if (intLoopAccuracy > intBestAccuracy)
                                    {
                                        intIndexToUse = i;
                                        intBestAccuracy = intLoopAccuracy;
                                    }
                                }
                            }
                        }
                        sbdBonusAccuracy.Append(" + ").Append(lstNonStackingAccessoryBonuses[intIndexToUse]);
                    }
                    else
                        sbdBonusAccuracy.Append(" + ").Append(lstNonStackingAccessoryBonuses[0]);
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
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objGear.FlechetteWeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                            {
                                strAccuracyReplace = strAccuracyReplace
                                    .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                strAccuracy = strAccuracyReplace;
                            }
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objGear.FlechetteWeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                            {
                                strAccuracyAdd = strAccuracyAdd
                                    .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                            }
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objGear.WeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                            {
                                strAccuracyReplace = strAccuracyReplace
                                    .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                strAccuracy = strAccuracyReplace;
                            }
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objGear.WeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                            {
                                strAccuracyAdd = strAccuracyAdd
                                    .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in objGear.Children.DeepWhere(x => x.Children.Where(y => y.Equipped), x => x.Equipped))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" &&
                                objChild.FlechetteWeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAccuracyReplace = objChild.FlechetteWeaponBonus["accuracyreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyReplace))
                                {
                                    strAccuracyReplace = strAccuracyReplace
                                        .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    strAccuracy = strAccuracyReplace;
                                }
                                // Adjust the Weapon's Damage.
                                string strAccuracyAdd = objChild.FlechetteWeaponBonus["accuracy"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                                {
                                    strAccuracyAdd = strAccuracyAdd
                                        .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAccuracyReplace = objChild.WeaponBonus["accuracyreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyReplace))
                                {
                                    strAccuracyReplace = strAccuracyReplace
                                        .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    strAccuracy = strAccuracyReplace;
                                }
                                // Adjust the Weapon's Damage.
                                string strAccuracyAdd = objChild.WeaponBonus["accuracy"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                                {
                                    strAccuracyAdd = strAccuracyAdd
                                        .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                                }
                            }
                        }
                    }

                    if (sbdBonusAccuracy.Length != 0)
                        strAccuracy = '(' + strAccuracy + ')' + sbdBonusAccuracy;
                }
            }

            if (strAccuracy.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decAccuracy))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAccuracy))
                {
                    sbdAccuracy.Append(strAccuracy);
                    sbdAccuracy.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    ProcessAttributesInXPath(sbdAccuracy, strAccuracy);
                    sbdAccuracy.CheapReplace(strAccuracy, "Physical", funcPhysicalLimitString)
                        .CheapReplace(strAccuracy, "Missile", funcPhysicalLimitString);
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdAccuracy.ToString());
                    if (blnIsSuccess)
                        intAccuracy = ((double)objProcess).StandardRound();
                }
            }
            else
                intAccuracy = decAccuracy.StandardRound();

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
        public async Task<int> GetTotalAccuracyAsync(bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            int intAccuracy = 0;
            string strAccuracy = Accuracy;
            Func<Task<string>> funcPhysicalLimitString;
            if (ParentVehicle != null)
            {
                funcPhysicalLimitString = async () =>
                {
                    string strHandling = await ParentVehicle.GetTotalHandlingAsync(token).ConfigureAwait(false);
                    int intSlashIndex = strHandling.IndexOf('/');
                    if (intSlashIndex != -1)
                        strHandling = strHandling.Substring(0, intSlashIndex);
                    return strHandling;
                };
            }
            else
                funcPhysicalLimitString = async () =>
                    (await _objCharacter.GetLimitPhysicalAsync(token).ConfigureAwait(false)).ToString(GlobalSettings
                        .InvariantCultureInfo);
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool,
                               out StringBuilder sbdBonusAccuracy))
            {
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    // Change the Weapon's Damage Type.
                    string strAccuracyReplace = WirelessWeaponBonus["accuracyreplace"]?.InnerText;
                    if (!string.IsNullOrEmpty(strAccuracyReplace))
                        strAccuracy = strAccuracyReplace;
                    // Adjust the Weapon's Damage.
                    string strAccuracyAdd = WirelessWeaponBonus["accuracy"]?.InnerText;
                    if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                        sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                }

                List<string> lstNonStackingAccessoryBonuses = new List<string>();
                foreach (WeaponAccessory objWeaponAccessory in WeaponAccessories)
                {
                    if (objWeaponAccessory.Equipped)
                    {
                        string strLoopAccuracy = objWeaponAccessory.Accuracy;
                        if (!string.IsNullOrEmpty(strLoopAccuracy) && strLoopAccuracy != "0" && strLoopAccuracy != "+0" && strLoopAccuracy != "-0")
                        {
                            strLoopAccuracy = await strLoopAccuracy
                                .CheapReplaceAsync("{Rating}", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                .CheapReplaceAsync("Rating", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .ConfigureAwait(false);
                            if (!objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && !objWeaponAccessory.Name.Contains("Sight"))
                                sbdBonusAccuracy.Append(" + ").Append(strLoopAccuracy.TrimStartOnce('+'));
                            else
                                lstNonStackingAccessoryBonuses.Add(strLoopAccuracy);
                        }
                        if (objWeaponAccessory.WirelessOn && WirelessOn && objWeaponAccessory.WirelessWeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objWeaponAccessory.WirelessWeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                            {
                                strAccuracyReplace = await strAccuracyReplace
                                    .CheapReplaceAsync("{Rating}", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .ConfigureAwait(false);
                                strAccuracy = strAccuracyReplace;
                            }
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objWeaponAccessory.WirelessWeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                            {
                                strAccuracyAdd = await strAccuracyAdd
                                    .CheapReplaceAsync("{Rating}", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .ConfigureAwait(false);
                                if (!objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && !objWeaponAccessory.Name.Contains("Sight"))
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                                else if (!string.IsNullOrEmpty(objWeaponAccessory.Accuracy))
                                    lstNonStackingAccessoryBonuses[lstNonStackingAccessoryBonuses.Count - 1] = lstNonStackingAccessoryBonuses[lstNonStackingAccessoryBonuses.Count - 1] + " + " + strAccuracyAdd.TrimStartOnce('+');
                                else
                                    lstNonStackingAccessoryBonuses.Add(strAccuracyAdd);
                            }
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
                            string strLoopAccuracy = objWeaponAccessory.Accuracy;
                            if (!string.IsNullOrEmpty(strLoopAccuracy))
                            {
                                strLoopAccuracy = await strLoopAccuracy
                                    .CheapReplaceAsync("{Rating}", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", () => objWeaponAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .ConfigureAwait(false);
                                lstNonStackingAccessoryBonuses.Add(strLoopAccuracy);
                            }
                        }
                    }
                }
                if (lstNonStackingAccessoryBonuses.Count > 0)
                {
                    if (lstNonStackingAccessoryBonuses.Count > 1)
                    {
                        int intIndexToUse = 0;
                        using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAccuracy))
                        {
                            string strLoopAccuracy = lstNonStackingAccessoryBonuses[0];
                            sbdAccuracy.Append(strLoopAccuracy);
                            await ProcessAttributesInXPathAsync(sbdAccuracy, strLoopAccuracy, token: token).ConfigureAwait(false);
                            await (await sbdAccuracy.CheapReplaceAsync(strLoopAccuracy, "Physical", funcPhysicalLimitString,
                                    token: token).ConfigureAwait(false))
                                .CheapReplaceAsync(strLoopAccuracy, "Missile", funcPhysicalLimitString, token: token)
                                .ConfigureAwait(false);
                            (bool blnIsSuccess, object objProcess)
                                = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAccuracy.ToString(), token)
                                    .ConfigureAwait(false);
                            int intBestAccuracy = blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                            for (int i = 1; i < lstNonStackingAccessoryBonuses.Count; ++i)
                            {
                                sbdAccuracy.Clear();
                                strLoopAccuracy = lstNonStackingAccessoryBonuses[i];
                                await ProcessAttributesInXPathAsync(sbdAccuracy, strLoopAccuracy, token: token).ConfigureAwait(false);
                                await (await sbdAccuracy.CheapReplaceAsync(strLoopAccuracy, "Physical", funcPhysicalLimitString,
                                        token: token).ConfigureAwait(false))
                                    .CheapReplaceAsync(strLoopAccuracy, "Missile", funcPhysicalLimitString, token: token)
                                    .ConfigureAwait(false);
                                (blnIsSuccess, objProcess)
                                    = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAccuracy.ToString(), token)
                                        .ConfigureAwait(false);
                                if (blnIsSuccess)
                                {
                                    int intLoopAccuracy = ((double)objProcess).StandardRound();
                                    if (intLoopAccuracy > intBestAccuracy)
                                    {
                                        intIndexToUse = i;
                                        intBestAccuracy = intLoopAccuracy;
                                    }
                                }
                            }
                        }
                        sbdBonusAccuracy.Append(" + ").Append(lstNonStackingAccessoryBonuses[intIndexToUse]);
                    }
                    else
                        sbdBonusAccuracy.Append(" + ").Append(lstNonStackingAccessoryBonuses[0]);
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
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objGear.FlechetteWeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                            {
                                strAccuracyReplace = await strAccuracyReplace
                                    .CheapReplaceAsync("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .ConfigureAwait(false);
                                strAccuracy = strAccuracyReplace;
                            }
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objGear.FlechetteWeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                            {
                                strAccuracyAdd = await strAccuracyAdd
                                    .CheapReplaceAsync("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .ConfigureAwait(false);
                                sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                            }
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type.
                            string strAccuracyReplace = objGear.WeaponBonus["accuracyreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyReplace))
                            {
                                strAccuracyReplace = await strAccuracyReplace
                                    .CheapReplaceAsync("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .ConfigureAwait(false);
                                strAccuracy = strAccuracyReplace;
                            }
                            // Adjust the Weapon's Damage.
                            string strAccuracyAdd = objGear.WeaponBonus["accuracy"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                            {
                                strAccuracyAdd = await strAccuracyAdd
                                    .CheapReplaceAsync("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .ConfigureAwait(false);
                                sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in await objGear.Children
                                        .DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token)
                                        .ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" &&
                                objChild.FlechetteWeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAccuracyReplace = objChild.FlechetteWeaponBonus["accuracyreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyReplace))
                                {
                                    strAccuracyReplace = await strAccuracyReplace
                                        .CheapReplaceAsync("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .ConfigureAwait(false);
                                    strAccuracy = strAccuracyReplace;
                                }
                                // Adjust the Weapon's Damage.
                                string strAccuracyAdd = objChild.FlechetteWeaponBonus["accuracy"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                                {
                                    strAccuracyAdd = await strAccuracyAdd
                                        .CheapReplaceAsync("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .ConfigureAwait(false);
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                // Change the Weapon's Damage Type.
                                string strAccuracyReplace = objChild.WeaponBonus["accuracyreplace"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyReplace))
                                {
                                    strAccuracyReplace = await strAccuracyReplace
                                        .CheapReplaceAsync("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .ConfigureAwait(false);
                                    strAccuracy = strAccuracyReplace;
                                }
                                // Adjust the Weapon's Damage.
                                string strAccuracyAdd = objChild.WeaponBonus["accuracy"]?.InnerText;
                                if (!string.IsNullOrEmpty(strAccuracyAdd) && strAccuracyAdd != "0" && strAccuracyAdd != "+0" && strAccuracyAdd != "-0")
                                {
                                    strAccuracyAdd = await strAccuracyAdd
                                        .CheapReplaceAsync("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .ConfigureAwait(false);
                                    sbdBonusAccuracy.Append(" + ").Append(strAccuracyAdd.TrimStartOnce('+'));
                                }
                            }
                        }
                    }

                    if (sbdBonusAccuracy.Length != 0)
                        strAccuracy = '(' + strAccuracy + ')' + sbdBonusAccuracy;
                }
            }

            if (strAccuracy.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decAccuracy))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAccuracy))
                {
                    sbdAccuracy.Append(strAccuracy);
                    await sbdAccuracy.CheapReplaceAsync(strAccuracy, "{Rating}",
                        async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await ProcessAttributesInXPathAsync(sbdAccuracy, strAccuracy, token: token).ConfigureAwait(false);
                    await (await sbdAccuracy.CheapReplaceAsync(strAccuracy, "Physical", funcPhysicalLimitString,
                            token: token).ConfigureAwait(false))
                        .CheapReplaceAsync(strAccuracy, "Missile", funcPhysicalLimitString, token: token)
                        .ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAccuracy.ToString(), token)
                            .ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAccuracy = ((double)objProcess).StandardRound();
                }
            }
            else
                intAccuracy = decAccuracy.StandardRound();

            string strNameUpper = Name.ToUpperInvariant();

            decimal decImproveAccuracy = await ImprovementManager.ValueOfAsync(
                _objCharacter, Improvement.ImprovementType.WeaponSkillAccuracy, strImprovedName: Name,
                blnIncludeNonImproved: true, token: token).ConfigureAwait(false);
            Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
            string strSkillDictionaryKey = objSkill != null
                ? await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                : string.Empty;
            if (!string.IsNullOrEmpty(strSkillDictionaryKey))
                decImproveAccuracy += await ImprovementManager.ValueOfAsync(_objCharacter,
                    Improvement.ImprovementType.WeaponSkillAccuracy,
                    strImprovedName: strSkillDictionaryKey, token: token).ConfigureAwait(false);
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
        /// Displays the base and Total Accuracy of the weapon in the same format as it appears in rulebooks in the program's current language.
        /// </summary>
        public Task<string> GetDisplayAccuracyAsync(CancellationToken token = default) => GetAccuracyAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token: token);

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
        public async Task<string> GetAccuracyAsync(CultureInfo objCulture, string strLanguage,
            bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            int intTotalAccuracy = await GetTotalAccuracyAsync(blnIncludeAmmo, token).ConfigureAwait(false);
            if (int.TryParse(Accuracy, out int intAccuracy) && intAccuracy != intTotalAccuracy)
                return string.Format(objCulture, "{0}{1}({2})",
                    intAccuracy,
                    await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token)
                        .ConfigureAwait(false), intTotalAccuracy);
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

        public Task<string> GetCurrentDisplayRangeAsync(CancellationToken token = default) =>
            DisplayRangeAsync(GlobalSettings.Language, false, token);

        /// <summary>
        /// The string for the Weapon's Range category
        /// </summary>
        public string DisplayRange(string strLanguage, bool blnIncludeAmmoName = false)
        {
            string strRange = Range;
            if (string.IsNullOrWhiteSpace(strRange))
                strRange = Category;
            if (!string.IsNullOrWhiteSpace(strRange) &&
                !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("ranges.xml", strLanguage);
                XPathNavigator objXmlCategoryNode =
                    objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRange);
                XPathNavigator xmlTranslateNode = objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("translate");
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.Value;
                }
                else
                {
                    objXmlDocument = _objCharacter.LoadDataXPath("weapons.xml", strLanguage);
                    objXmlCategoryNode =
                        objXmlDocument.SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " +
                                                                          strRange.CleanXPath() + ']');
                    xmlTranslateNode = objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("@translate");
                    if (xmlTranslateNode != null)
                        strRange = xmlTranslateNode.Value;
                }
            }
            if (!blnIncludeAmmoName || AmmoLoaded == null)
            {
                return strRange;
            }
            strRange += " (" + AmmoLoaded?.DisplayNameShort(strLanguage) + ')';

            return strRange;
        }

        /// <summary>
        /// The string for the Weapon's Range category
        /// </summary>
        public async Task<string> DisplayRangeAsync(string strLanguage, bool blnIncludeAmmoName = false, CancellationToken token = default)
        {
            string strRange = Range;
            if (string.IsNullOrWhiteSpace(strRange))
                strRange = Category;

            // First look at any changes caused by the weapon being wireless
            if (WirelessOn && WirelessWeaponBonus != null)
            {
                string strNewRange = string.Empty;
                if (WirelessWeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                {
                    strRange = strNewRange;
                }
            }

            await WeaponAccessories.ForEachAsync(x =>
            {
                if (x.Equipped)
                {
                    string strNewRange = x.ReplaceRange;
                    if (!string.IsNullOrEmpty(strNewRange))
                        strRange = strNewRange;
                }
            }, token).ConfigureAwait(false);

            if (blnIncludeAmmoName)
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
                            strRange = strNewRange;
                        }
                    }
                    else if (objGear.WeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                    {
                        strRange = strNewRange;
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in await objGear.Children
                                 .DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token)
                                 .ConfigureAwait(false))
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear"
                                                   && objChild.FlechetteWeaponBonus != null)
                        {
                            if (objChild.FlechetteWeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                            {
                                strRange = strNewRange;
                            }
                        }
                        else if (objChild.WeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                        {
                            strRange = strNewRange;
                        }
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(strRange) &&
                !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objXmlDocument = await _objCharacter
                    .LoadDataXPathAsync("ranges.xml", strLanguage, token: token).ConfigureAwait(false);
                XPathNavigator objXmlCategoryNode =
                    objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRange);
                XPathNavigator xmlTranslateNode =
                    objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("translate", token: token);
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.Value;
                }
                else
                {
                    objXmlDocument = await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token)
                        .ConfigureAwait(false);
                    objXmlCategoryNode = objXmlDocument
                        .SelectSingleNodeAndCacheExpression(
                            "/chummer/categories/category[. = " + strRange.CleanXPath() + ']',
                            token: token);
                    if (objXmlCategoryNode != null)
                    {
                        xmlTranslateNode
                            = objXmlCategoryNode.SelectSingleNodeAndCacheExpression("@translate", token: token);
                        if (xmlTranslateNode != null)
                            strRange = xmlTranslateNode.Value;
                    }
                }
            }
            if (!blnIncludeAmmoName || AmmoLoaded == null)
            {
                return strRange;
            }
            strRange += " (" + (await AmmoLoaded?.DisplayNameShortAsync(strLanguage, token: token)) + ')';

            return strRange;
        }

        public string CurrentDisplayAlternateRange => DisplayAlternateRange(GlobalSettings.Language);

        public Task<string> GetCurrentDisplayAlternateRangeAsync(CancellationToken token = default) =>
            DisplayAlternateRangeAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The string for the Weapon's Range category (setter is English-only).
        /// </summary>
        public string DisplayAlternateRange(string strLanguage)
        {
            string strRange = AlternateRange.Trim();
            if (!string.IsNullOrEmpty(strRange) &&
                !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objXmlDocument = _objCharacter.LoadDataXPath("ranges.xml", strLanguage);
                XPathNavigator objXmlCategoryNode =
                    objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRange);
                XPathNavigator xmlTranslateNode = objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("translate");
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.Value;
                }
                else
                {
                    objXmlDocument = _objCharacter.LoadDataXPath("weapons.xml", strLanguage);
                    objXmlCategoryNode =
                        objXmlDocument.SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " +
                                                                          strRange.CleanXPath() + ']');
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
        public async Task<string> DisplayAlternateRangeAsync(string strLanguage, CancellationToken token = default)
        {
            string strRange = AlternateRange.Trim();
            if (!string.IsNullOrEmpty(strRange) &&
                !strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objXmlDocument = await _objCharacter
                    .LoadDataXPathAsync("ranges.xml", strLanguage, token: token).ConfigureAwait(false);
                XPathNavigator objXmlCategoryNode =
                    objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRange);
                XPathNavigator xmlTranslateNode =
                    objXmlCategoryNode?.SelectSingleNodeAndCacheExpression("translate", token: token);
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.Value;
                }
                else
                {
                    objXmlDocument = await _objCharacter.LoadDataXPathAsync("weapons.xml", strLanguage, token: token)
                        .ConfigureAwait(false);
                    objXmlCategoryNode = objXmlDocument
                        .SelectSingleNodeAndCacheExpression(
                            "/chummer/categories/category[. = " + strRange.CleanXPath() + ']',
                            token: token);
                    if (objXmlCategoryNode != null)
                    {
                        xmlTranslateNode
                            = objXmlCategoryNode.SelectSingleNodeAndCacheExpression("@translate", token: token);
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
        private decimal GetRange(string strFindRange, bool blnUseAlternateRange, bool blnIncludeAmmo = true)
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

            // First look at any changes caused by the weapon being wireless
            if (WirelessOn && WirelessWeaponBonus != null)
            {
                string strNewRange = string.Empty;
                if (WirelessWeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                {
                    strRangeCategory = strNewRange;
                }
            }

            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                if (objAccessory.Equipped)
                {
                    string strNewRange = objAccessory.ReplaceRange;
                    if (!string.IsNullOrEmpty(strNewRange))
                        strRangeCategory = strNewRange;
                }
            }

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
                    foreach (Gear objChild in objGear.Children.DeepWhere(x => x.Children.Where(y => y.Equipped), x => x.Equipped))
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
            XPathNavigator objXmlCategoryNode =
                objXmlDocument.TryGetNodeByNameOrId("/chummer/ranges/range", strRangeCategory);
            if (objXmlCategoryNode?.SelectSingleNode(strFindRange) == null)
            {
                return -1;
            }

            string strRange = objXmlCategoryNode.SelectSingleNode(strFindRange)?.Value ?? string.Empty;
            if (strRange.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decRange))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdRange))
                {
                    sbdRange.Append(strRange);
                    sbdRange.CheapReplace(strRange, "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo))
                        .CheapReplace(strRange, "Rating", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                    ProcessAttributesInXPath(sbdRange, strRange, true);
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(sbdRange.ToString());
                    if (blnIsSuccess)
                        decRange = Convert.ToDecimal((double)objProcess);
                    else
                        return -1;
                }
            }

            if (Category == "Throwing Weapons" || Skill?.DictionaryKey == "Throwing Weapons")
            {
                decRange += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowRange);
            }

            return decRange * _decRangeMultiplier;
        }

        /// <summary>
        /// Evaluate and return the requested Range for the Weapon.
        /// </summary>
        /// <param name="strFindRange">Range node to use.</param>
        /// <param name="blnUseAlternateRange">Use alternate range instead of the weapon's main range.</param>
        /// <param name="blnIncludeAmmo">Whether to include any range modifications from the currently loaded ammo.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        private async Task<decimal> GetRangeAsync(string strFindRange, bool blnUseAlternateRange,
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

            // First look at any changes caused by the weapon being wireless
            if (WirelessOn && WirelessWeaponBonus != null)
            {
                string strNewRange = string.Empty;
                if (WirelessWeaponBonus.TryGetStringFieldQuickly("userange", ref strNewRange))
                {
                    strRangeCategory = strNewRange;
                }
            }

            await WeaponAccessories.ForEachAsync(x =>
            {
                if (x.Equipped)
                {
                    string strNewRange = x.ReplaceRange;
                    if (!string.IsNullOrEmpty(strNewRange))
                        strRangeCategory = strNewRange;
                }
            }, token).ConfigureAwait(false);

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
                    foreach (Gear objChild in await objGear.Children
                                 .DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token)
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
            if (strRange.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decRange))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdRange))
                {
                    sbdRange.Append(strRange);
                    await sbdRange.CheapReplaceAsync(strRange, "{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await sbdRange.CheapReplaceAsync(strRange, "Rating", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                    await ProcessAttributesInXPathAsync(sbdRange, strRange, true, token).ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions
                        .EvaluateInvariantXPathAsync(sbdRange.ToString(), token)
                        .ConfigureAwait(false);

                    if (blnIsSuccess)
                        decRange = Convert.ToDecimal((double)objProcess);
                    else
                        return -1;
                }
            }

            bool blnAddImprovements = Category == "Throwing Weapons";
            if (!blnAddImprovements)
            {
                Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                string strSkillDictionaryKey = objSkill != null
                    ? await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                    : string.Empty;
                blnAddImprovements = strSkillDictionaryKey == "Throwing Weapons";
            }
            if (blnAddImprovements)
            {
                decRange += await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowRange, token: token)
                    .ConfigureAwait(false);
            }

            return decRange * _decRangeMultiplier;
        }

        /// <summary>
        /// Weapon's total Range bonus from Accessories.
        /// </summary>
        public decimal GetRangeBonus(bool blnIncludeAmmo = true)
        {
            string strToEvaluate;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdRangeBonus))
            {
                string strRangeBonus = string.Empty;
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null && WirelessWeaponBonus.TryGetStringFieldQuickly("rangebonus", ref strRangeBonus)
                    && strRangeBonus != "0" && strRangeBonus != "+0" && strRangeBonus != "-0")
                {
                    sbdRangeBonus.Append('(').Append(strRangeBonus.TrimStartOnce('+')).Append(')');
                }

                // Weapon Mods.
                WeaponAccessories.ForEach(x =>
                {
                    if (!x.Equipped)
                        return;
                    string strInnerBonus = x.RangeBonus;
                    if (!string.IsNullOrEmpty(strInnerBonus) && strInnerBonus != "0" && strInnerBonus != "+0" && strInnerBonus != "-0")
                    {
                        strInnerBonus = strInnerBonus.TrimStartOnce('+')
                                .CheapReplace("{Rating}", () => x.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace("Rating", () => x.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdRangeBonus.Append(sbdRangeBonus.Length > 0 ? " + (" : "(").Append(strInnerBonus).Append(')');
                    }
                    if (WirelessOn && x.WirelessOn && x.WirelessWeaponBonus != null)
                    {
                        strInnerBonus = string.Empty;
                        if (x.WirelessWeaponBonus.TryGetStringFieldQuickly("rangebonus", ref strInnerBonus)
                            && !string.IsNullOrEmpty(strInnerBonus) && strInnerBonus != "0" && strInnerBonus != "+0" && strInnerBonus != "-0")
                        {
                            strInnerBonus = strInnerBonus.TrimStartOnce('+')
                                .CheapReplace("{Rating}", () => x.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace("Rating", () => x.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            sbdRangeBonus.Append(sbdRangeBonus.Length > 0 ? " + (" : "(").Append(strInnerBonus).Append(')');
                        }
                    }
                });

                if (blnIncludeAmmo)
                {
                    // Check if the Weapon has Ammunition loaded and look for any Range bonus.
                    Gear objGear = AmmoLoaded;
                    if (objGear != null)
                    {
                        string strInnerBonus = string.Empty;
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        {
                            strInnerBonus = objGear.FlechetteWeaponBonusRange.TrimStartOnce('+');
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            strInnerBonus = objGear.WeaponBonusRange.TrimStartOnce('+');
                        }
                        if (!string.IsNullOrEmpty(strInnerBonus) && strInnerBonus != "0" && strInnerBonus != "+0" && strInnerBonus != "-0")
                        {
                            strInnerBonus = strInnerBonus
                                .CheapReplace("{Rating}", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace("Rating", () => objGear.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            sbdRangeBonus.Append(sbdRangeBonus.Length > 0 ? " + (" : "(").Append(strInnerBonus).Append(')');
                        }
                    }
                }

                sbdRangeBonus.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo))
                    .CheapReplace("Rating", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                ProcessAttributesInXPath(sbdRangeBonus, blnForRange: true);
                strToEvaluate = sbdRangeBonus.ToString();
            }

            (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strToEvaluate);
            return blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
        }

        /// <summary>
        /// Weapon's total Range bonus from Accessories.
        /// </summary>
        public async Task<decimal> GetRangeBonusAsync(bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            string strToEvaluate;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdRangeBonus))
            {
                string strRangeBonus = string.Empty;
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null
                    && WirelessWeaponBonus.TryGetStringFieldQuickly("rangebonus", ref strRangeBonus)
                        && strRangeBonus != "0" && strRangeBonus != "+0" && strRangeBonus != "-0")
                {
                    sbdRangeBonus.Append('(').Append(strRangeBonus.TrimStartOnce('+')).Append(')');
                }

                // Weapon Mods.
                await WeaponAccessories.ForEachAsync(async x =>
                {
                    if (!x.Equipped)
                        return;
                    string strInnerBonus = x.RangeBonus;
                    if (!string.IsNullOrEmpty(strInnerBonus) && strInnerBonus != "0" && strInnerBonus != "+0" && strInnerBonus != "-0")
                    {
                        strInnerBonus = await strInnerBonus.TrimStartOnce('+')
                            .CheapReplaceAsync("{Rating}", async () => (await x.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                            .CheapReplaceAsync("Rating", async () => (await x.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                        sbdRangeBonus.Append(sbdRangeBonus.Length > 0 ? " + (" : "(").Append(strInnerBonus).Append(')');
                    }
                    if (WirelessOn && x.WirelessOn && x.WirelessWeaponBonus != null)
                    {
                        strInnerBonus = string.Empty;
                        if (x.WirelessWeaponBonus.TryGetStringFieldQuickly("rangebonus", ref strInnerBonus)
                            && !string.IsNullOrEmpty(strInnerBonus) && strInnerBonus != "0" && strInnerBonus != "+0" && strInnerBonus != "-0")
                        {
                            strInnerBonus = await strInnerBonus.TrimStartOnce('+')
                                .CheapReplaceAsync("{Rating}", async () => (await x.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                .CheapReplaceAsync("Rating", async () => (await x.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                            sbdRangeBonus.Append(sbdRangeBonus.Length > 0 ? " + (" : "(").Append(strInnerBonus).Append(')');
                        }
                    }
                }, token).ConfigureAwait(false);

                if (blnIncludeAmmo)
                {
                    // Check if the Weapon has Ammunition loaded and look for any Range bonus.
                    Gear objGear = AmmoLoaded;
                    if (objGear != null)
                    {
                        string strInnerBonus = string.Empty;
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objGear.FlechetteWeaponBonus != null)
                        {
                            strInnerBonus = objGear.FlechetteWeaponBonusRange.TrimStartOnce('+');
                        }
                        else if (objGear.WeaponBonus != null)
                        {
                            strInnerBonus = objGear.WeaponBonusRange.TrimStartOnce('+');
                        }
                        if (!string.IsNullOrEmpty(strInnerBonus) && strInnerBonus != "0" && strInnerBonus != "+0" && strInnerBonus != "-0")
                        {
                            strInnerBonus = await strInnerBonus
                                .CheapReplaceAsync("{Rating}", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                .CheapReplaceAsync("Rating", async () => (await objGear.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                .ConfigureAwait(false);
                            sbdRangeBonus.Append(sbdRangeBonus.Length > 0 ? " + (" : "(").Append(strInnerBonus).Append(')');
                        }
                    }
                }

                await sbdRangeBonus.CheapReplaceAsync("{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                    .ConfigureAwait(false);
                await sbdRangeBonus.CheapReplaceAsync("Rating", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                    .ConfigureAwait(false);
                await ProcessAttributesInXPathAsync(sbdRangeBonus, blnForRange: true, token: token).ConfigureAwait(false);
                strToEvaluate = sbdRangeBonus.ToString();
            }

            (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strToEvaluate, token).ConfigureAwait(false);
            return blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
        }

        public string RangeModifier(string strRange)
        {
            if (string.IsNullOrEmpty(strRange))
                return string.Empty;
            string strToEvaluate = string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdBaseModifier))
            {
                string strBaseModifier = _objCharacter.LoadDataXPath("ranges.xml")
                        .SelectSingleNodeAndCacheExpression("chummer/modifiers/" + strRange.ToLowerInvariant())?.Value;
                if (!string.IsNullOrEmpty(strBaseModifier) && strBaseModifier != "0" && strBaseModifier != "+0")
                    sbdBaseModifier.Append('(').Append(strBaseModifier.TrimStartOnce('+')).Append(')');

                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (!objAccessory.Equipped)
                        continue;
                    string strLoopModifier = objAccessory.RangeModifier;
                    if (!string.IsNullOrEmpty(strLoopModifier) && strLoopModifier != "0" && strLoopModifier != "+0")
                    {
                        strLoopModifier = strLoopModifier.TrimStartOnce('+')
                            .CheapReplace("{Rating}", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplace("Rating", () => objAccessory.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                        sbdBaseModifier.Append(sbdBaseModifier.Length > 0 ? " + (" : "(").Append(strLoopModifier).Append(')');
                    }
                }

                sbdBaseModifier.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdBaseModifier.CheapReplace("Rating", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                ProcessAttributesInXPath(sbdBaseModifier, blnForRange: true);
                strToEvaluate = sbdBaseModifier.ToString();
            }

            (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strToEvaluate);
            decimal decModifier = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;

            string strNameUpper = Name.ToUpperInvariant();
            foreach (Improvement objImprovement in ImprovementManager.GetCachedImprovementListForValueOf(_objCharacter,
                         Improvement.ImprovementType.WeaponRangeModifier))
            {
                string strImprovedName = objImprovement.ImprovedName;
                if (string.IsNullOrEmpty(strImprovedName) || strImprovedName == Name
                                                          || strImprovedName.StartsWith(
                                                              "[contains]", StringComparison.Ordinal)
                                                          && strNameUpper.Contains(
                                                              strImprovedName.TrimStartOnce("[contains]", true),
                                                              StringComparison.OrdinalIgnoreCase))
                    decModifier += objImprovement.Value;
            }

            return string.Format(GlobalSettings.InvariantCultureInfo,
                LanguageManager.GetString("Label_Range" + strRange), decModifier.StandardRound());
        }

        public async Task<string> RangeModifierAsync(string strRange, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strRange))
                return string.Empty;
            string strToEvaluate = string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdBaseModifier))
            {
                string strBaseModifier = (await _objCharacter.LoadDataXPathAsync("ranges.xml", token: token).ConfigureAwait(false))
                        .SelectSingleNodeAndCacheExpression("chummer/modifiers/" + strRange.ToLowerInvariant(), token)?.Value;
                if (!string.IsNullOrEmpty(strBaseModifier) && strBaseModifier != "0" && strBaseModifier != "+0")
                    sbdBaseModifier.Append('(').Append(strBaseModifier.TrimStartOnce('+')).Append(')');

                await WeaponAccessories.ForEachAsync(async objAccessory =>
                {
                    if (!objAccessory.Equipped)
                        return;
                    string strLoopModifier = objAccessory.RangeModifier;
                    if (!string.IsNullOrEmpty(strLoopModifier) && strLoopModifier != "0" && strLoopModifier != "+0")
                    {
                        strLoopModifier = await strLoopModifier.TrimStartOnce('+')
                            .CheapReplaceAsync("{Rating}", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo))
                            .CheapReplaceAsync("Rating", async () => (await objAccessory.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo))
                            .ConfigureAwait(false);
                        sbdBaseModifier.Append(sbdBaseModifier.Length > 0 ? " + (" : "(").Append(strLoopModifier).Append(')');
                    }
                }, token).ConfigureAwait(false);

                await sbdBaseModifier.CheapReplaceAsync("{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                await sbdBaseModifier.CheapReplaceAsync("Rating", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                await ProcessAttributesInXPathAsync(sbdBaseModifier, blnForRange: true, token: token).ConfigureAwait(false);
                strToEvaluate = sbdBaseModifier.ToString();
            }

            (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strToEvaluate, token).ConfigureAwait(false);
            decimal decModifier = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;

            string strNameUpper = Name.ToUpperInvariant();
            foreach (Improvement objImprovement in await ImprovementManager.GetCachedImprovementListForValueOfAsync(_objCharacter,
                         Improvement.ImprovementType.WeaponRangeModifier, token: token).ConfigureAwait(false))
            {
                string strImprovedName = objImprovement.ImprovedName;
                if (string.IsNullOrEmpty(strImprovedName) || strImprovedName == Name
                                                          || strImprovedName.StartsWith(
                                                              "[contains]", StringComparison.Ordinal)
                                                          && strNameUpper.Contains(
                                                              strImprovedName.TrimStartOnce("[contains]", true),
                                                              StringComparison.OrdinalIgnoreCase))
                    decModifier += objImprovement.Value;
            }

            return string.Format(GlobalSettings.InvariantCultureInfo,
                await LanguageManager.GetStringAsync("Label_Range" + strRange, token: token).ConfigureAwait(false), decModifier.StandardRound());
        }

        /// <summary>
        /// Dictionary where keys are range categories (short, medium, long, extreme, alternateshort, etc.), values are strings depicting range values for the category.
        /// </summary>
        public Dictionary<string, string> GetRangeStrings(CultureInfo objCulture, bool blnIncludeAmmo = true)
        {
            decimal decRangeModifier = 1.0m + GetRangeBonus(blnIncludeAmmo) / 100.0m;
            decimal decMin = GetRange("min", false, blnIncludeAmmo) * decRangeModifier;
            decimal decShort = GetRange("short", false, blnIncludeAmmo) * decRangeModifier;
            decimal decMedium = GetRange("medium", false, blnIncludeAmmo) * decRangeModifier;
            decimal decLong = GetRange("long", false, blnIncludeAmmo) * decRangeModifier;
            decimal decExtreme = GetRange("extreme", false, blnIncludeAmmo) * decRangeModifier;
            decimal decAlternateMin = GetRange("min", true, blnIncludeAmmo) * decRangeModifier;
            decimal decAlternateShort = GetRange("short", true, blnIncludeAmmo) * decRangeModifier;
            decimal decAlternateMedium = GetRange("medium", true, blnIncludeAmmo) * decRangeModifier;
            decimal decAlternateLong = GetRange("long", true, blnIncludeAmmo) * decRangeModifier;
            decimal decAlternateExtreme = GetRange("extreme", true, blnIncludeAmmo) * decRangeModifier;
            int intMin = decMin.StandardRound();
            int intShort = decShort.StandardRound();
            int intMedium = decMedium.StandardRound();
            int intLong = decLong.StandardRound();
            int intExtreme = decExtreme.StandardRound();
            int intAlternateMin = decAlternateMin.StandardRound();
            int intAlternateShort = decAlternateShort.StandardRound();
            int intAlternateMedium = decAlternateMedium.StandardRound();
            int intAlternateLong = decAlternateLong.StandardRound();
            int intAlternateExtreme = decAlternateExtreme.StandardRound();
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
        public async Task<Dictionary<string, string>> GetRangeStringsAsync(CultureInfo objCulture,
            bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            decimal decRangeModifier = 1.0m + await GetRangeBonusAsync(blnIncludeAmmo, token).ConfigureAwait(false) / 100.0m;
            decimal decMin = await GetRangeAsync("min", false, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decShort = await GetRangeAsync("short", false, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decMedium = await GetRangeAsync("medium", false, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decLong = await GetRangeAsync("long", false, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decExtreme = await GetRangeAsync("extreme", false, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decAlternateMin = await GetRangeAsync("min", true, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decAlternateShort = await GetRangeAsync("short", true, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decAlternateMedium = await GetRangeAsync("medium", true, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decAlternateLong = await GetRangeAsync("long", true, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            decimal decAlternateExtreme = await GetRangeAsync("extreme", true, blnIncludeAmmo, token).ConfigureAwait(false) * decRangeModifier;
            int intMin = decMin.StandardRound();
            int intShort = decShort.StandardRound();
            int intMedium = decMedium.StandardRound();
            int intLong = decLong.StandardRound();
            int intExtreme = decExtreme.StandardRound();
            int intAlternateMin = decAlternateMin.StandardRound();
            int intAlternateShort = decAlternateShort.StandardRound();
            int intAlternateMedium = decAlternateMedium.StandardRound();
            int intAlternateLong = decAlternateLong.StandardRound();
            int intAlternateExtreme = decAlternateExtreme.StandardRound();
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
            decimal decDicePoolModifier = 0;
            switch (FireMode)
            {
                case FiringMode.DogBrain:
                {
                    string strAutosoft = RelevantAutosoft;
                    string strName = Name;
                    string strDisplayName = CurrentDisplayName;
                    Gear objAutosoft = null;
                    if (_objCharacter.ActiveCommlink is Gear objCommlink && objCommlink.Category == "Rigger Command Consoles")
                    {
                        objAutosoft = _objCharacter.Gear.DeepFirstOrDefault(
                            x => x.Children.Where(y => y.Equipped),
                            x => x.Name == strAutosoft && x.Equipped &&
                                 (x.Extra == strName || x.Extra == strDisplayName));
                    }

                    if (ParentVehicle != null)
                    {
                        intDicePool = ParentVehicle.Pilot;
                        if (objAutosoft == null)
                        {
                            objAutosoft = ParentVehicle.GearChildren.DeepFirstOrDefault(
                                x => x.Children.Where(y => y.Equipped),
                                x => x.Name == strAutosoft && x.Equipped &&
                                     (x.Extra == strName || x.Extra == strDisplayName));
                        }

                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            if (_objCharacter.Settings.BookEnabled("R5"))
                            {
                                if (ParentVehicle.GearChildren.DeepAny(
                                        x => x.Children.Where(y => y.Equipped),
                                        x => x.Name == "Smartsoft" && x.Equipped))
                                {
                                    ++decDicePoolModifier;
                                }
                            }
                            else if (ParentVehicle.GearChildren.DeepAny(
                                         x => x.Children.Where(y => y.Equipped),
                                         x => x.Name == "Camera" && x.Equipped &&
                                              x.GearChildren.Any(y => y.Name == "Smartlink" && y.Equipped)))
                            {
                                ++decDicePoolModifier;
                            }
                        }
                    }

                    intDicePool += objAutosoft?.Rating ?? -1;

                    break;
                }
                case FiringMode.RemoteOperated:
                {
                    intDicePool = _objCharacter.SkillsSection.GetActiveSkill("Gunnery").PoolOtherAttribute("LOG");

                    if (WirelessOn && HasWirelessSmartgun)
                    {
                        decimal decSmartlinkBonus = ImprovementManager.ValueOf(_objCharacter,
                            Improvement.ImprovementType.Smartlink);
                        foreach (Gear objLoopGear in ParentVehicle.GearChildren.DeepWhere(x => x.Children.Where(y => y.Equipped),
                                     x => x.Equipped && x.Bonus?.InnerXml.Contains("<smartlink>") == true))
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
                                intDicePool += objSpec.SpecializationBonus;
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

            string strToEvaluate;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdExtraModifier))
            {
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    string strWeaponBonusPool = WirelessWeaponBonus["pool"]?.InnerText;
                    if (!string.IsNullOrEmpty(strWeaponBonusPool) && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                    {
                        sbdExtraModifier.Append('(').Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                    }
                    if (HasWirelessSmartgun)
                    {
                        strWeaponBonusPool = WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool) && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                        {
                            sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                        }
                    }
                }

                decDicePoolModifier += WeaponAccessories.Sum(a => a.Equipped, a =>
                {
                    if (WirelessOn && a.WirelessOn && a.WirelessWeaponBonus != null)
                    {
                        string strWeaponBonusPool = a.WirelessWeaponBonus["pool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool)
                            && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                        {
                            strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                .CheapReplace("{Rating}", () => a.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                .CheapReplace("Rating", () => a.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                            sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                .Append(strWeaponBonusPool).Append(')');
                        }
                        if (HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = a.WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                            {
                                strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                    .CheapReplace("{Rating}", () => a.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => a.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                    .Append(strWeaponBonusPool).Append(')');
                            }
                        }
                    }
                    return a.DicePool;
                });

                if (blnIncludeAmmo)
                {
                    Gear objAmmo = AmmoLoaded;
                    if (objAmmo != null)
                    {
                        string strWeaponBonusPool;
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objAmmo.FlechetteWeaponBonus != null)
                        {
                            strWeaponBonusPool = objAmmo.FlechetteWeaponBonusPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                            {
                                strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                    .CheapReplace("{Rating}", () => objAmmo.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objAmmo.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                    .Append(strWeaponBonusPool).Append(')');
                            }
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objAmmo.FlechetteWeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                    && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                {
                                    strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                        .CheapReplace("{Rating}", () => objAmmo.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objAmmo.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                        .Append(strWeaponBonusPool).Append(')');
                                }
                            }
                        }
                        else if (objAmmo.WeaponBonus != null)
                        {
                            strWeaponBonusPool = objAmmo.WeaponBonusPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                            {
                                strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                    .CheapReplace("{Rating}", () => objAmmo.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                    .CheapReplace("Rating", () => objAmmo.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                    .Append(strWeaponBonusPool).Append(')');
                            }
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objAmmo.WeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                    && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                {
                                    strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                        .CheapReplace("{Rating}", () => objAmmo.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objAmmo.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                        .Append(strWeaponBonusPool).Append(')');
                                }
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in objAmmo.Children.DeepWhere(x => x.Children.Where(y => y.Equipped), x => x.Equipped))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                            {
                                strWeaponBonusPool = objChild.FlechetteWeaponBonusPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                    && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                {
                                    strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                        .CheapReplace("{Rating}", () => objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                        .Append(strWeaponBonusPool).Append(')');
                                }
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    strWeaponBonusPool = objChild.FlechetteWeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                        && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                    {
                                        strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                            .CheapReplace("{Rating}", () => objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                            .CheapReplace("Rating", () => objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                        sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                            .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                strWeaponBonusPool = objChild.WeaponBonusPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                    && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                {
                                    strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                        .CheapReplace("{Rating}", () => objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                        .CheapReplace("Rating", () => objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                    sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                        .Append(strWeaponBonusPool).Append(')');
                                }
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    strWeaponBonusPool = objChild.WeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                        && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                    {
                                        strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+')
                                            .CheapReplace("{Rating}", () => objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo))
                                            .CheapReplace("Rating", () => objChild.Rating.ToString(GlobalSettings.InvariantCultureInfo));
                                        sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                            .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                            }
                        }
                    }
                }

                sbdExtraModifier.CheapReplace("{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                sbdExtraModifier.CheapReplace("Rating", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));
                ProcessAttributesInXPath(sbdExtraModifier);
                strToEvaluate = sbdExtraModifier.ToString();
            }

            (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strToEvaluate);
            decimal decExtraModifier = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
            return intDicePool + (decDicePoolModifier + decExtraModifier).StandardRound();
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to use the Weapon.
        /// </summary>
        public async Task<int> GetDicePoolAsync(bool blnIncludeAmmo = true, CancellationToken token = default)
        {
            int intDicePool = 0;
            decimal decDicePoolModifier = 0;
            switch (FireMode)
            {
                case FiringMode.DogBrain:
                {
                    string strAutosoft = await GetRelevantAutosoftAsync(token).ConfigureAwait(false);
                    string strName = Name;
                    string strDisplayName = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                    Gear objAutosoft = null;
                    if (await _objCharacter.GetActiveCommlinkAsync(token).ConfigureAwait(false) is Gear objCommlink && objCommlink.Category == "Rigger Command Consoles")
                    {
                        objAutosoft = await _objCharacter.Gear.DeepFirstOrDefaultAsync(
                            async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false),
                            x => x.Name == strAutosoft && x.Equipped &&
                                 (x.Extra == strName || x.Extra == strDisplayName), token: token).ConfigureAwait(false);
                    }
                    if (ParentVehicle != null)
                    {
                        intDicePool = await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false);
                        if (objAutosoft == null)
                        {
                            objAutosoft = await ParentVehicle.GearChildren.DeepFirstOrDefaultAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false),
                                x => x.Name == strAutosoft && x.Equipped &&
                                     (x.Extra == strName || x.Extra == strDisplayName), token: token).ConfigureAwait(false);
                        }

                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            if (await _objCharacter.Settings.BookEnabledAsync("R5", token).ConfigureAwait(false))
                            {
                                if (await ParentVehicle.GearChildren.DeepAnyAsync(
                                            async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Name == "Smartsoft" && x.Equipped, token)
                                        .ConfigureAwait(false))
                                {
                                    ++decDicePoolModifier;
                                }
                            }
                            else if (await ParentVehicle.GearChildren.DeepAnyAsync(
                                             async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false),
                                             async x => x.Name == "Camera" && x.Equipped &&
                                                  await x.GearChildren.AnyAsync(y => y.Name == "Smartlink" && y.Equipped, token).ConfigureAwait(false), token)
                                         .ConfigureAwait(false))
                            {
                                ++decDicePoolModifier;
                            }
                        }
                    }

                    intDicePool += objAutosoft != null
                        ? await objAutosoft.GetRatingAsync(token).ConfigureAwait(false)
                        : -1;

                    break;
                }
                case FiringMode.RemoteOperated:
                {
                    intDicePool =
                        await (await _objCharacter.SkillsSection.GetActiveSkillAsync("Gunnery", token)
                            .ConfigureAwait(false)).PoolOtherAttributeAsync("LOG", token: token).ConfigureAwait(false);

                    if (WirelessOn && HasWirelessSmartgun)
                    {
                        decimal decSmartlinkBonus = await ImprovementManager.ValueOfAsync(_objCharacter,
                            Improvement.ImprovementType.Smartlink, token: token).ConfigureAwait(false);
                        foreach (Gear objLoopGear in await ParentVehicle.GearChildren.DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false),
                                         x => x.Bonus?.InnerXml.Contains("<smartlink>") == true, token)
                                     .ConfigureAwait(false))
                        {
                            string strLoopBonus = string.Empty;
                            if (objLoopGear.Bonus.TryGetStringFieldQuickly("smartlink", ref strLoopBonus))
                            {
                                decSmartlinkBonus = Math.Max(decSmartlinkBonus, await ImprovementManager
                                    .ValueToDecAsync(
                                        _objCharacter, strLoopBonus,
                                        await objLoopGear.GetRatingAsync(token).ConfigureAwait(false), token)
                                    .ConfigureAwait(false));
                            }
                        }

                        decDicePoolModifier += decSmartlinkBonus;
                    }

                    decDicePoolModifier
                        += await ImprovementManager.ValueOfAsync(_objCharacter,
                            Improvement.ImprovementType.WeaponCategoryDice,
                            false, Category, token: token).ConfigureAwait(false);
                    break;
                }
                case FiringMode.GunneryCommandDevice:
                case FiringMode.ManualOperation:
                {
                    Skill objSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync("Gunnery", token)
                        .ConfigureAwait(false);
                    if (Cyberware && Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(objSkill.Attribute)
                                  && _objMountedVehicle == null)
                    {
                        Cyberware objAttributeSource = await _objCharacter.Cyberware
                            .DeepFindByIdAsync(ParentID, token: token).ConfigureAwait(false);
                        while (objAttributeSource != null
                               && await objAttributeSource.GetAttributeTotalValueAsync(objSkill.Attribute, token)
                                   .ConfigureAwait(false) == 0)
                        {
                            objAttributeSource = await objAttributeSource.GetParentAsync(token).ConfigureAwait(false);
                        }

                        if (objAttributeSource != null)
                            intDicePool = await objSkill.PoolOtherAttributeAsync(
                                objSkill.Attribute, false,
                                await objAttributeSource.GetAttributeTotalValueAsync(objSkill.Attribute, token)
                                    .ConfigureAwait(false), token).ConfigureAwait(false);
                        else
                            intDicePool = await objSkill.GetPoolAsync(token).ConfigureAwait(false);
                    }
                    else
                        intDicePool = await objSkill.GetPoolAsync(token).ConfigureAwait(false);

                    if (WirelessOn && HasWirelessSmartgun)
                    {
                        decDicePoolModifier +=
                            await ImprovementManager
                                .ValueOfAsync(_objCharacter, Improvement.ImprovementType.Smartlink, token: token)
                                .ConfigureAwait(false);
                    }

                    decDicePoolModifier
                        += await ImprovementManager.ValueOfAsync(_objCharacter,
                            Improvement.ImprovementType.WeaponCategoryDice,
                            false, Category, token: token).ConfigureAwait(false);
                    break;
                }
                case FiringMode.Skill:
                {
                    Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                    if (objSkill != null)
                    {
                        if (Cyberware && Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(objSkill.Attribute))
                        {
                            Cyberware objAttributeSource = _objMountedVehicle != null
                                ? (await _objCharacter.Vehicles
                                    .FindVehicleCyberwareAsync(x => x.InternalId == ParentID, token)
                                    .ConfigureAwait(false)).Item1
                                : await _objCharacter.Cyberware.DeepFindByIdAsync(ParentID, token)
                                    .ConfigureAwait(false);
                            while (objAttributeSource != null
                                   && await objAttributeSource.GetAttributeTotalValueAsync(objSkill.Attribute, token)
                                       .ConfigureAwait(false) == 0)
                            {
                                objAttributeSource = await objAttributeSource.GetParentAsync(token).ConfigureAwait(false);
                            }

                            if (objAttributeSource != null)
                                intDicePool = await objSkill.PoolOtherAttributeAsync(
                                    objSkill.Attribute, false,
                                    await objAttributeSource.GetAttributeTotalValueAsync(objSkill.Attribute, token)
                                        .ConfigureAwait(false), token).ConfigureAwait(false);
                            else
                                intDicePool = await objSkill.GetPoolAsync(token).ConfigureAwait(false);
                        }
                        else
                            intDicePool = await objSkill.GetPoolAsync(token).ConfigureAwait(false);

                        if (WirelessOn && HasWirelessSmartgun)
                        {
                            decDicePoolModifier
                                += await ImprovementManager
                                    .ValueOfAsync(_objCharacter, Improvement.ImprovementType.Smartlink, token: token)
                                    .ConfigureAwait(false);
                        }

                        decDicePoolModifier
                            += await ImprovementManager.ValueOfAsync(_objCharacter,
                                Improvement.ImprovementType.WeaponCategoryDice,
                                false, Category, token: token).ConfigureAwait(false);
                        decDicePoolModifier += await ImprovementManager.ValueOfAsync(
                            _objCharacter, Improvement.ImprovementType.WeaponSpecificDice, false, InternalId,
                            token: token).ConfigureAwait(false);

                        // If the character has a Specialization, include it in the Dice Pool string.
                        if (!objSkill.IsExoticSkill &&
                            await objSkill.Specializations.GetCountAsync(token).ConfigureAwait(false) > 0)
                        {
                            SkillSpecialization objSpec =
                                await objSkill
                                    .GetSpecializationAsync(
                                        await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token)
                                    .ConfigureAwait(false) ??
                                await objSkill.GetSpecializationAsync(Name, token).ConfigureAwait(false) ??
                                await objSkill
                                    .GetSpecializationAsync(
                                        await DisplayCategoryAsync(GlobalSettings.Language, token)
                                            .ConfigureAwait(false), token).ConfigureAwait(false) ??
                                await objSkill.GetSpecializationAsync(Category, token).ConfigureAwait(false);

                            if (objSpec == null && objSkill.Specializations.Count > 0)
                            {
                                objSpec = await objSkill.GetSpecializationAsync(Spec, token).ConfigureAwait(false) ??
                                          await objSkill.GetSpecializationAsync(Spec2, token).ConfigureAwait(false);
                            }

                            if (objSpec != null)
                            {
                                intDicePool +=
                                    await objSpec.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                            }
                        }
                    }

                    break;
                }
            }

            if (FireMode == FiringMode.GunneryCommandDevice || FireMode == FiringMode.RemoteOperated ||
                FireMode == FiringMode.ManualOperation)
            {
                Skill objSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync("Gunnery", token)
                    .ConfigureAwait(false);
                if (objSkill != null)
                {
                    string strSpec = await GetRelevantSpecializationAsync(token).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(strSpec) && strSpec != "None" &&
                        await objSkill.Specializations.GetCountAsync(token).ConfigureAwait(false) > 0)
                    {
                        intDicePool += await objSkill.GetSpecializationBonusAsync(strSpec, token).ConfigureAwait(false);
                    }
                }
            }

            string strToEvaluate;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdExtraModifier))
            {
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    string strWeaponBonusPool = WirelessWeaponBonus["pool"]?.InnerText;
                    if (!string.IsNullOrEmpty(strWeaponBonusPool)
                        && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                    {
                        strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+');
                        sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                            .Append(strWeaponBonusPool).Append(')');
                    }
                    if (HasWirelessSmartgun)
                    {
                        strWeaponBonusPool = WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool)
                            && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                        {
                            strWeaponBonusPool = strWeaponBonusPool.TrimStartOnce('+');
                            sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                .Append(strWeaponBonusPool).Append(')');
                        }
                    }
                }

                decDicePoolModifier += await WeaponAccessories.SumAsync(a => a.Equipped, async a =>
                {
                    if (WirelessOn && a.WirelessOn && a.WirelessWeaponBonus != null)
                    {
                        string strWeaponBonusPool = a.WirelessWeaponBonus["pool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool)
                            && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                        {
                            strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                .CheapReplaceAsync("{Rating}", async () => (await a.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                .CheapReplaceAsync("Rating", async () => (await a.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                            sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                .Append(strWeaponBonusPool).Append(')');
                        }
                        if (HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = a.WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                            {
                                strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                    .CheapReplaceAsync("{Rating}", async () => (await a.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", async () => (await a.GetRatingAsync(token)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                    .Append(strWeaponBonusPool).Append(')');
                            }
                        }
                    }
                    return await a.GetDicePoolAsync(token).ConfigureAwait(false);
                }, token: token).ConfigureAwait(false);

                if (blnIncludeAmmo)
                {
                    Gear objAmmo = AmmoLoaded;
                    if (objAmmo != null)
                    {
                        string strWeaponBonusPool;
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objAmmo.FlechetteWeaponBonus != null)
                        {
                            strWeaponBonusPool = objAmmo.FlechetteWeaponBonusPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                            {
                                strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                    .CheapReplaceAsync("{Rating}", async () => (await objAmmo.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", async () => (await objAmmo.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                    .Append(strWeaponBonusPool).Append(')');
                            }
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objAmmo.FlechetteWeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                    && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                {
                                    strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                        .CheapReplaceAsync("{Rating}", async () => (await objAmmo.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objAmmo.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                        .Append(strWeaponBonusPool).Append(')');
                                }
                            }
                        }
                        else if (objAmmo.WeaponBonus != null)
                        {
                            strWeaponBonusPool = objAmmo.WeaponBonusPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                            {
                                strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                    .CheapReplaceAsync("{Rating}", async () => (await objAmmo.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                    .CheapReplaceAsync("Rating", async () => (await objAmmo.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                    .Append(strWeaponBonusPool).Append(')');
                            }
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objAmmo.WeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                    && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                {
                                    strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                        .CheapReplaceAsync("{Rating}", async () => (await objAmmo.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objAmmo.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                        .Append(strWeaponBonusPool).Append(')');
                                }
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in await objAmmo.Children
                                     .DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token).ConfigureAwait(false))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                            {
                                strWeaponBonusPool = objChild.FlechetteWeaponBonusPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                    && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                {
                                    strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                        .CheapReplaceAsync("{Rating}", async () => (await objChild.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objChild.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                        .Append(strWeaponBonusPool).Append(')');
                                }
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    strWeaponBonusPool = objChild.FlechetteWeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                        && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                    {
                                        strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                            .CheapReplaceAsync("{Rating}", async () => (await objChild.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .CheapReplaceAsync("Rating", async () => (await objChild.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                        sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                            .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                strWeaponBonusPool = objChild.WeaponBonusPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                    && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                {
                                    strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                        .CheapReplaceAsync("{Rating}", async () => (await objChild.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                        .CheapReplaceAsync("Rating", async () => (await objChild.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                    sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                        .Append(strWeaponBonusPool).Append(')');
                                }
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    strWeaponBonusPool = objChild.WeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strWeaponBonusPool)
                                        && strWeaponBonusPool != "0" && strWeaponBonusPool != "+0" && strWeaponBonusPool != "-0")
                                    {
                                        strWeaponBonusPool = await strWeaponBonusPool.TrimStartOnce('+')
                                            .CheapReplaceAsync("{Rating}", async () => (await objChild.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token)
                                            .CheapReplaceAsync("Rating", async () => (await objChild.GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                                        sbdExtraModifier.Append(sbdExtraModifier.Length > 0 ? " + (" : "(")
                                            .Append(strWeaponBonusPool).Append(')');
                                    }
                                }
                            }
                        }
                    }
                }

                await sbdExtraModifier.CheapReplaceAsync("{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                await sbdExtraModifier.CheapReplaceAsync("Rating", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);
                await ProcessAttributesInXPathAsync(sbdExtraModifier, token: token).ConfigureAwait(false);
                strToEvaluate = sbdExtraModifier.ToString();
            }

            (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strToEvaluate, token).ConfigureAwait(false);
            decimal decExtraModifier = blnIsSuccess ? Convert.ToDecimal((double)objProcess) : 0;
            return intDicePool + (decDicePoolModifier + decExtraModifier).StandardRound();
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

                string strGunnerySpec = this.GetNodeXPath()?.SelectSingleNodeAndCacheExpression("category/@gunneryspec")
                    ?.Value;
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

        internal async Task<string> GetRelevantSpecializationAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(_strRelevantSpec))
            {
                return _strRelevantSpec;
            }

            XPathNavigator xmlNode = await this.GetNodeXPathAsync(token).ConfigureAwait(false);
            if (xmlNode != null)
            {
                string strGunnerySpec =
                    xmlNode.SelectSingleNodeAndCacheExpression("category/@gunneryspec", token)?.Value;
                if (string.IsNullOrEmpty(strGunnerySpec))
                {
                    strGunnerySpec = (await _objCharacter.LoadDataXPathAsync("weapons.xml", token: token)
                            .ConfigureAwait(false))
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

                // If this is a Special Weapon, use the Range to determine the required Active Skill (if present).
                if (strCategory == "Special Weapons" && !string.IsNullOrEmpty(Range))
                    strCategory = Range;
                string strSkill = GetSkillDictionaryKey(strCategory);

                // Use the Skill defined by the Weapon if one is present.
                if (!string.IsNullOrEmpty(UseSkill))
                {
                    strSkill = UseSkill;

                    (bool blnIsExotic, string strSkillName)
                        = ExoticSkill.IsExoticSkillNameTuple(_objCharacter, strSkill);
                    if (blnIsExotic)
                    {
                        strSkill = strSkillName + " (" + UseSkillSpec + ')';
                    }
                }

                // Locate the Active Skill to be used.
                return _objCharacter.SkillsSection.GetActiveSkill(strSkill);
            }
        }

        private async Task<Skill> GetSkillAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strCategory = Category;

            // If this is a Special Weapon, use the Range to determine the required Active Skill (if present).
            if (strCategory == "Special Weapons" && !string.IsNullOrEmpty(Range))
                strCategory = Range;

            // Exotic Skills require a matching Specialization.
            string strSkill = GetSkillDictionaryKey(strCategory);

            // Use the Skill defined by the Weapon if one is present.
            if (!string.IsNullOrEmpty(UseSkill))
            {
                strSkill = UseSkill;

                (bool blnIsExotic, string strSkillName)
                    = await ExoticSkill.IsExoticSkillNameTupleAsync(_objCharacter, strSkill, token)
                        .ConfigureAwait(false);
                if (blnIsExotic)
                {
                    strSkill = strSkillName + " (" + UseSkillSpec + ')';
                }
            }

            // Locate the Active Skill to be used.
            return await _objCharacter.SkillsSection.GetActiveSkillAsync(strSkill, token)
                .ConfigureAwait(false);
        }

        private string GetSkillDictionaryKey(string strCategory)
        {
            switch (strCategory)
            {
                case "Bows":
                case "Crossbows":
                    return "Archery";

                case "Assault Rifles":
                case "Carbines":
                case "Machine Pistols":
                case "Submachine Guns":
                    return "Automatics";

                case "Blades":
                    return "Blades";

                case "Clubs":
                case "Improvised Weapons":
                    return "Clubs";

                case "Exotic Melee Weapons":
                    return "Exotic Melee Weapon (" + UseSkillSpec + ')';

                case "Exotic Ranged Weapons":
                case "Special Weapons":
                    return "Exotic Ranged Weapon (" + UseSkillSpec + ')';

                case "Flamethrowers":
                    return "Exotic Ranged Weapon (Flamethrowers)";

                case "Laser Weapons":
                    return "Exotic Ranged Weapon (Laser Weapons)";

                case "Assault Cannons":
                case "Grenade Launchers":
                case "Missile Launchers":
                case "Light Machine Guns":
                case "Medium Machine Guns":
                case "Heavy Machine Guns":
                    return "Heavy Weapons";

                case "Shotguns":
                case "Sniper Rifles":
                case "Sporting Rifles":
                    return "Longarms";

                case "Throwing Weapons":
                    return "Throwing Weapons";

                case "Unarmed":
                    return "Unarmed Combat";

                default:
                    return "Pistols";
            }
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
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdExtra))
                {
                    // First look at any changes caused by the weapon being wireless
                    if (WirelessOn && WirelessWeaponBonus != null)
                    {
                        string strWeaponBonusPool = WirelessWeaponBonus["pool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool))
                        {
                            string strWireless = LanguageManager.GetString("String_Wireless");
                            if (HasWirelessSmartgun)
                            {
                                string strInner = WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strInner))
                                {
                                    if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                        && decimal.TryParse(strInner, out decimal decTemp2))
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(strWireless).Append(strSpace)
                                            .Append('(')
                                            .Append((decTemp + decTemp2).StandardRound()
                                                .ToString(GlobalSettings.CultureInfo)).Append(')');
                                    }
                                    else
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(strWireless).Append(strSpace)
                                            .Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                            .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(strWireless).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(strWireless).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                        else if (HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(LanguageManager.GetString("String_Wireless")).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                    }

                    foreach (WeaponAccessory wa in WeaponAccessories.Where(a => a.Equipped))
                    {
                        decimal decDicePool = wa.DicePool;
                        if (decDicePool != 0)
                        {
                            sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                strSpace, wa.CurrentDisplayName, decDicePool.ToString("+#,0.##;-#,0.##;0.##", GlobalSettings.CultureInfo));
                        }

                        if (WirelessOn && wa.WirelessOn && wa.WirelessWeaponBonus != null)
                        {
                            string strWeaponBonusPool = wa.WirelessWeaponBonus["pool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                string strWireless = wa.CurrentDisplayName + strSpace + LanguageManager.GetString("String_Wireless");
                                if (HasWirelessSmartgun)
                                {
                                    string strInner = wa.WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                                    if (!string.IsNullOrEmpty(strInner))
                                    {
                                        if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                            && decimal.TryParse(strInner, out decimal decTemp2))
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(strWireless).Append(strSpace)
                                                .Append('(')
                                                .Append((decTemp + decTemp2).StandardRound()
                                                    .ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                        else
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(strWireless).Append(strSpace)
                                                .Append('(')
                                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                                .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(strWireless).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                    }
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(strWireless).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else if (HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = wa.WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(wa.CurrentDisplayName)
                                        .Append(strSpace)
                                        .Append(LanguageManager.GetString("String_Wireless")).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                        }
                    }

                    Gear objLoadedAmmo = AmmoLoaded;
                    if (objLoadedAmmo != null)
                    {
                        string strWeaponBonusPool;
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" &&
                            objLoadedAmmo.FlechetteWeaponBonus != null)
                        {
                            strWeaponBonusPool = objLoadedAmmo.FlechetteWeaponBonusPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    string strInner = objLoadedAmmo.FlechetteWeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strInner))
                                    {
                                        if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                            && decimal.TryParse(strInner, out decimal decTemp2))
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace)
                                                .Append('(')
                                                .Append((decTemp + decTemp2).StandardRound()
                                                    .ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                        else
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace)
                                                .Append('(')
                                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                                .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                    }
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objLoadedAmmo.FlechetteWeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                        }
                        else if (objLoadedAmmo.WeaponBonus != null)
                        {
                            strWeaponBonusPool = objLoadedAmmo.WeaponBonusPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    string strInner = objLoadedAmmo.WeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strInner))
                                    {
                                        if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                            && decimal.TryParse(strInner, out decimal decTemp2))
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace)
                                                .Append('(')
                                                .Append((decTemp + decTemp2).StandardRound()
                                                    .ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                        else
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace)
                                                .Append('(')
                                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                                .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                    }
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objLoadedAmmo.WeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(objLoadedAmmo.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in objLoadedAmmo.Children.DeepWhere(x => x.Children.Where(y => y.Equipped), x => x.Equipped))
                        {
                            if (Damage.Contains("(f)") && AmmoCategory != "Gear" &&
                                objChild.FlechetteWeaponBonus != null)
                            {
                                strWeaponBonusPool = objChild.FlechetteWeaponBonusPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    if (WirelessOn && HasWirelessSmartgun)
                                    {
                                        string strInner = objChild.FlechetteWeaponBonusSmartlinkPool;
                                        if (!string.IsNullOrEmpty(strInner))
                                        {
                                            if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                                && decimal.TryParse(strInner, out decimal decTemp2))
                                            {
                                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objChild.CurrentDisplayNameShort).Append(strSpace)
                                                    .Append('(')
                                                    .Append((decTemp + decTemp2).StandardRound()
                                                        .ToString(GlobalSettings.CultureInfo)).Append(')');
                                            }
                                            else
                                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objChild.CurrentDisplayNameShort).Append(strSpace)
                                                    .Append('(')
                                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                                    .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                        }
                                        else
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                        }
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                    }
                                }
                                else if (WirelessOn && HasWirelessSmartgun)
                                {
                                    strWeaponBonusPool = objChild.FlechetteWeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                    }
                                }
                            }
                            else if (objChild.WeaponBonus != null)
                            {
                                strWeaponBonusPool = objChild.WeaponBonusPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    if (WirelessOn && HasWirelessSmartgun)
                                    {
                                        string strInner = objChild.WeaponBonusSmartlinkPool;
                                        if (!string.IsNullOrEmpty(strInner))
                                        {
                                            if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                                && decimal.TryParse(strInner, out decimal decTemp2))
                                            {
                                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objChild.CurrentDisplayNameShort).Append(strSpace)
                                                    .Append('(')
                                                    .Append((decTemp + decTemp2).StandardRound()
                                                        .ToString(GlobalSettings.CultureInfo)).Append(')');
                                            }
                                            else
                                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                    .Append(objChild.CurrentDisplayNameShort).Append(strSpace)
                                                    .Append('(')
                                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                                    .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                        }
                                        else
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                        }
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                    }
                                }
                                else if (WirelessOn && HasWirelessSmartgun)
                                {
                                    strWeaponBonusPool = objChild.WeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(objChild.CurrentDisplayNameShort).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
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
                                if (ParentVehicle != null)
                                {
                                    string strBonusName = string.Empty;
                                    if (_objCharacter.Settings.BookEnabled("R5"))
                                    {
                                        Gear objSmartsoft = ParentVehicle.GearChildren.DeepFirstOrDefault(
                                            x => x.Children.Where(y => y.Equipped),
                                            x => x.Name == "Smartsoft" && x.Equipped);
                                        if (objSmartsoft != null)
                                            strBonusName = objSmartsoft.CurrentDisplayNameShort;
                                    }
                                    else
                                    {
                                        if (ParentVehicle.GearChildren.DeepAny(
                                                x => x.Children.Where(y => y.Equipped),
                                                x => x.Name == "Camera" && x.Equipped &&
                                                     x.GearChildren.Any(y => y.Name == "Smartlink" && y.Equipped)))
                                            strBonusName = LanguageManager.GetString("Tip_Skill_Smartlink");
                                    }
                                    if (!string.IsNullOrEmpty(strBonusName))
                                    {
                                        sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                            strSpace, strBonusName, 1);
                                    }
                                }

                                break;

                            case FiringMode.RemoteOperated:
                                if (ParentVehicle != null)
                                {
                                    foreach (Gear objLoopGear in ParentVehicle.GearChildren.DeepWhere(x => x.Children.Where(y => y.Equipped),
                                                 x => x.Equipped && x.Bonus?.InnerXml.Contains("<smartlink>") == true))
                                    {
                                        string strLoopBonus = string.Empty;
                                        if (objLoopGear.Bonus.TryGetStringFieldQuickly("smartlink", ref strLoopBonus))
                                        {
                                            decSmartlinkBonus = Math.Max(decSmartlinkBonus,
                                                ImprovementManager.ValueToDec(
                                                    _objCharacter, strLoopBonus,
                                                    objLoopGear.Rating));
                                        }
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
                        string strAutosoft = RelevantAutosoft;
                        string strName = Name;
                        string strDisplayName = CurrentDisplayName;
                        Gear objAutosoft = null;
                        if (_objCharacter.ActiveCommlink is Gear objCommlink && objCommlink.Category == "Rigger Command Consoles")
                        {
                            objAutosoft = _objCharacter.Gear.DeepFirstOrDefault(
                                x => x.Children.Where(y => y.Equipped),
                                x => x.Name == strAutosoft && x.Equipped &&
                                     (x.Extra == strName || x.Extra == strDisplayName));
                        }

                        if (ParentVehicle != null && objAutosoft == null)
                        {
                            objAutosoft = ParentVehicle.GearChildren.DeepFirstOrDefault(
                                x => x.Children.Where(y => y.Equipped),
                                x => x.Name == strAutosoft && x.Equipped &&
                                     (x.Extra == strName || x.Extra == strDisplayName));
                        }

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

                        strReturn = objSkill.CompileDicepoolTooltip("LOG", objSkill.CurrentDisplayName + strSpace,
                            strExtra);
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
                        while (objAttributeSource != null &&
                               objAttributeSource.GetAttributeTotalValue(objSkill.Attribute) == 0)
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
                                    (objSkill.GetSpecialization(CurrentDisplayNameShort) ??
                                     objSkill.GetSpecialization(Name) ??
                                     objSkill.GetSpecialization(DisplayCategory(GlobalSettings.Language)) ??
                                     objSkill.GetSpecialization(Category)) ?? (objSkill.GetSpecialization(Category.EndsWith('s')
                                        ? Category.TrimEndOnce('s')
                                        : Category + 's') ?? (objSkill.GetSpecialization(Spec) ??
                                                              objSkill.GetSpecialization(Spec2)));

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

                                while (objAttributeSource != null &&
                                       objAttributeSource.GetAttributeTotalValue(objSkill.Attribute) == 0)
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
        /// Tooltip information for the Dice Pool.
        /// </summary>
        public async Task<string> GetDicePoolTooltipAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false);
            string strExtra;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdExtra))
            {
                // First look at any changes caused by the weapon being wireless
                if (WirelessOn && WirelessWeaponBonus != null)
                {
                    string strWeaponBonusPool = WirelessWeaponBonus["pool"]?.InnerText;
                    if (!string.IsNullOrEmpty(strWeaponBonusPool))
                    {
                        string strWireless = await LanguageManager.GetStringAsync("String_Wireless", token: token).ConfigureAwait(false);
                        if (HasWirelessSmartgun)
                        {
                            string strInner = WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strInner))
                            {
                                if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                    && decimal.TryParse(strInner, out decimal decTemp2))
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(strWireless).Append(strSpace)
                                        .Append('(')
                                        .Append((decTemp + decTemp2).StandardRound()
                                            .ToString(GlobalSettings.CultureInfo)).Append(')');
                                }
                                else
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(strWireless).Append(strSpace)
                                        .Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                        .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                            }
                            else
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(strWireless).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                        else
                        {
                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                .Append(strWireless).Append(strSpace).Append('(')
                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                        }
                    }
                    else if (HasWirelessSmartgun)
                    {
                        strWeaponBonusPool = WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool.TrimStartOnce('+')))
                        {
                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                .Append(await LanguageManager.GetStringAsync("String_Wireless", token: token).ConfigureAwait(false)).Append(strSpace).Append('(')
                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                        }
                    }
                }

                await WeaponAccessories.ForEachAsync(async wa =>
                {
                    if (!wa.Equipped)
                        return;
                    decimal decDicePool = await wa.GetDicePoolAsync(token).ConfigureAwait(false);
                    if (decDicePool != 0)
                    {
                        sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})", strSpace,
                            await wa.GetCurrentDisplayNameAsync(token).ConfigureAwait(false), decDicePool.ToString("+#,0.##;-#,0.##;0.##", GlobalSettings.CultureInfo));
                    }
                    if (WirelessOn && wa.WirelessOn && wa.WirelessWeaponBonus != null)
                    {
                        string strWeaponBonusPool = wa.WirelessWeaponBonus["pool"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool))
                        {
                            string strWireless = await wa.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + strSpace + await LanguageManager.GetStringAsync("String_Wireless", token: token).ConfigureAwait(false);
                            if (HasWirelessSmartgun)
                            {
                                string strInner = wa.WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strInner))
                                {
                                    if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                        && decimal.TryParse(strInner, out decimal decTemp2))
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(strWireless).Append(strSpace)
                                            .Append('(')
                                            .Append((decTemp + decTemp2).StandardRound()
                                                .ToString(GlobalSettings.CultureInfo)).Append(')');
                                    }
                                    else
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(strWireless).Append(strSpace)
                                            .Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                            .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(strWireless).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(strWireless).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                        else if (HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = wa.WirelessWeaponBonus["smartlinkpool"]?.InnerText;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(await wa.GetCurrentDisplayNameAsync(token).ConfigureAwait(false))
                                    .Append(strSpace)
                                    .Append(await LanguageManager.GetStringAsync("String_Wireless", token: token).ConfigureAwait(false)).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                    }
                }, token).ConfigureAwait(false);

                Gear objLoadedAmmo = AmmoLoaded;
                if (objLoadedAmmo != null)
                {
                    string strWeaponBonusPool;
                    if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objLoadedAmmo.FlechetteWeaponBonus != null)
                    {
                        strWeaponBonusPool = objLoadedAmmo.FlechetteWeaponBonusPool;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool))
                        {
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                string strInner = objLoadedAmmo.FlechetteWeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strInner))
                                {
                                    if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                        && decimal.TryParse(strInner, out decimal decTemp2))
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false)).Append(strSpace).Append('(')
                                            .Append((decTemp + decTemp2).StandardRound()
                                                .ToString(GlobalSettings.CultureInfo)).Append(')');
                                    }
                                    else
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false)).Append(strSpace)
                                            .Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                            .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                            .ConfigureAwait(false)).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                        .ConfigureAwait(false)).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                        else if (WirelessOn && HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = objLoadedAmmo.FlechetteWeaponBonusSmartlinkPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                        .ConfigureAwait(false)).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                    }
                    else if (objLoadedAmmo.WeaponBonus != null)
                    {
                        strWeaponBonusPool = objLoadedAmmo.WeaponBonusPool;
                        if (!string.IsNullOrEmpty(strWeaponBonusPool))
                        {
                            if (WirelessOn && HasWirelessSmartgun)
                            {
                                string strInner = objLoadedAmmo.WeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strInner))
                                {
                                    if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                        && decimal.TryParse(strInner, out decimal decTemp2))
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false)).Append(strSpace).Append('(')
                                            .Append((decTemp + decTemp2).StandardRound()
                                                .ToString(GlobalSettings.CultureInfo)).Append(')');
                                    }
                                    else
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false)).Append(strSpace)
                                            .Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                            .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                            .ConfigureAwait(false)).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                        .ConfigureAwait(false)).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                        else if (WirelessOn && HasWirelessSmartgun)
                        {
                            strWeaponBonusPool = objLoadedAmmo.WeaponBonusSmartlinkPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                    .Append(await objLoadedAmmo.GetCurrentDisplayNameShortAsync(token)
                                        .ConfigureAwait(false)).Append(strSpace).Append('(')
                                    .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                            }
                        }
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in await objLoadedAmmo.Children
                                 .DeepWhereAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false), x => x.Equipped, token: token).ConfigureAwait(false))
                    {
                        if (Damage.Contains("(f)") && AmmoCategory != "Gear" && objChild.FlechetteWeaponBonus != null)
                        {
                            strWeaponBonusPool = objChild.FlechetteWeaponBonusPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    string strInner = objChild.FlechetteWeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strInner))
                                    {
                                        if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                            && decimal.TryParse(strInner, out decimal decTemp2))
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                                    .ConfigureAwait(false)).Append(strSpace).Append('(')
                                                .Append((decTemp + decTemp2).StandardRound()
                                                    .ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                        else
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                                    .ConfigureAwait(false)).Append(strSpace)
                                                .Append('(')
                                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                                .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false)).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                    }
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                            .ConfigureAwait(false)).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objChild.FlechetteWeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                            .ConfigureAwait(false)).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                        }
                        else if (objChild.WeaponBonus != null)
                        {
                            strWeaponBonusPool = objChild.WeaponBonusPool;
                            if (!string.IsNullOrEmpty(strWeaponBonusPool))
                            {
                                if (WirelessOn && HasWirelessSmartgun)
                                {
                                    string strInner = objChild.WeaponBonusSmartlinkPool;
                                    if (!string.IsNullOrEmpty(strInner))
                                    {
                                        if (decimal.TryParse(strWeaponBonusPool, out decimal decTemp)
                                            && decimal.TryParse(strInner, out decimal decTemp2))
                                        {
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                                    .ConfigureAwait(false)).Append(strSpace).Append('(')
                                                .Append((decTemp + decTemp2).StandardRound()
                                                    .ToString(GlobalSettings.CultureInfo)).Append(')');
                                        }
                                        else
                                            sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                                .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                                    .ConfigureAwait(false)).Append(strSpace)
                                                .Append('(')
                                                .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(strSpace).Append('+')
                                                .Append(strSpace).Append(strInner.TrimStartOnce('+')).Append(')');
                                    }
                                    else
                                    {
                                        sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                            .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                                .ConfigureAwait(false)).Append(strSpace).Append('(')
                                            .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                    }
                                }
                                else
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                            .ConfigureAwait(false)).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
                                }
                            }
                            else if (WirelessOn && HasWirelessSmartgun)
                            {
                                strWeaponBonusPool = objChild.WeaponBonusSmartlinkPool;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                {
                                    sbdExtra.Append(strSpace).Append('+').Append(strSpace)
                                        .Append(await objChild.GetCurrentDisplayNameShortAsync(token)
                                            .ConfigureAwait(false)).Append(strSpace).Append('(')
                                        .Append(strWeaponBonusPool.TrimStartOnce('+')).Append(')');
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
                            await ImprovementManager
                                .ValueOfAsync(_objCharacter, Improvement.ImprovementType.Smartlink, token: token)
                                .ConfigureAwait(false);
                        if (decSmartlinkBonus != 0)
                        {
                            sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                strSpace,
                                await LanguageManager.GetStringAsync("Tip_Skill_Smartlink", token: token)
                                    .ConfigureAwait(false),
                                decSmartlinkBonus);
                        }
                    }

                    foreach (Improvement objImprovement in (await ImprovementManager
                                 .GetCachedImprovementListForValueOfAsync(
                                     _objCharacter,
                                     Improvement.ImprovementType.WeaponCategoryDice,
                                     Category, token: token).ConfigureAwait(false))
                             .Concat(
                                 await ImprovementManager
                                     .GetCachedImprovementListForValueOfAsync(
                                         _objCharacter,
                                         Improvement.ImprovementType
                                             .WeaponSpecificDice, InternalId, token: token).ConfigureAwait(false)))
                    {
                        sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                            strSpace,
                            await _objCharacter.GetObjectNameAsync(objImprovement, token: token).ConfigureAwait(false),
                            objImprovement.Value);
                    }
                }
                else if (WirelessOn && HasWirelessSmartgun)
                {
                    decimal decSmartlinkBonus
                        = await ImprovementManager
                            .ValueOfAsync(_objCharacter, Improvement.ImprovementType.Smartlink, token: token)
                            .ConfigureAwait(false);
                    switch (FireMode)
                    {
                        case FiringMode.DogBrain:
                            if (ParentVehicle != null)
                            {
                                string strBonusName = string.Empty;
                                if (await _objCharacter.Settings.BookEnabledAsync("R5", token).ConfigureAwait(false))
                                {
                                    Gear objSmartsoft = await ParentVehicle.GearChildren.DeepFirstOrDefaultAsync(
                                            async x => await x.Children.ToListAsync(y => y.Equipped, token: token)
                                                .ConfigureAwait(false), x => x.Name == "Smartsoft" && x.Equipped, token)
                                        .ConfigureAwait(false);
                                    if (objSmartsoft != null)
                                        strBonusName = await objSmartsoft.GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false);
                                }
                                else
                                {
                                    if (await ParentVehicle.GearChildren.DeepAnyAsync(
                                                async x => await x.Children.ToListAsync(y => y.Equipped, token: token)
                                                    .ConfigureAwait(false),
                                                async x => x.Name == "Camera" && x.Equipped &&
                                                           await x.GearChildren.AnyAsync(
                                                               y => y.Name == "Smartlink" && y.Equipped, token).ConfigureAwait(false), token)
                                            .ConfigureAwait(false))
                                        strBonusName = await LanguageManager.GetStringAsync("Tip_Skill_Smartlink", token: token).ConfigureAwait(false);
                                }
                                if (!string.IsNullOrEmpty(strBonusName))
                                {
                                    sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                        strSpace, strBonusName, 1);
                                }
                            }

                            break;

                        case FiringMode.RemoteOperated:
                            if (ParentVehicle != null)
                            {
                                foreach (Gear objLoopGear in await ParentVehicle.GearChildren.DeepWhereAsync(
                                                 async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false),
                                                 x => x.Equipped && x.Bonus?.InnerXml.Contains("<smartlink>") == true, token: token)
                                             .ConfigureAwait(false))
                                {
                                    string strLoopBonus = string.Empty;
                                    if (objLoopGear.Bonus.TryGetStringFieldQuickly("smartlink", ref strLoopBonus))
                                    {
                                        decSmartlinkBonus = Math.Max(decSmartlinkBonus, await ImprovementManager
                                            .ValueToDecAsync(
                                                _objCharacter, strLoopBonus,
                                                await objLoopGear.GetRatingAsync(token).ConfigureAwait(false), token)
                                            .ConfigureAwait(false));
                                    }
                                }
                            }

                            if (decSmartlinkBonus != 0)
                            {
                                sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                    strSpace,
                                    await LanguageManager.GetStringAsync("Tip_Skill_Smartlink", token: token)
                                        .ConfigureAwait(false),
                                    decSmartlinkBonus);
                            }

                            break;

                        case FiringMode.GunneryCommandDevice:
                        case FiringMode.ManualOperation:
                            if (decSmartlinkBonus != 0)
                            {
                                sbdExtra.AppendFormat(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                    strSpace,
                                    await LanguageManager.GetStringAsync("Tip_Skill_Smartlink", token: token)
                                        .ConfigureAwait(false),
                                    decSmartlinkBonus);
                            }

                            break;
                    }
                }

                strExtra = sbdExtra.ToString();
            }

            string strReturn =
                await LanguageManager.GetStringAsync("String_Special", token: token).ConfigureAwait(false);
            switch (FireMode)
            {
                case FiringMode.DogBrain:
                {
                    strReturn = string.Format(GlobalSettings.CultureInfo, "{1}{0}({2})",
                        strSpace,
                        await LanguageManager.GetStringAsync("String_Pilot", token: token).ConfigureAwait(false),
                        ParentVehicle != null ? await ParentVehicle.GetPilotAsync(token).ConfigureAwait(false) : 0);
                    string strAutosoft = await GetRelevantAutosoftAsync(token).ConfigureAwait(false);
                    string strName = Name;
                    string strDisplayName = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false);
                    Gear objAutosoft = null;
                    if (await _objCharacter.GetActiveCommlinkAsync(token).ConfigureAwait(false) is Gear objCommlink && objCommlink.Category == "Rigger Command Consoles")
                    {
                        objAutosoft = await _objCharacter.Gear.DeepFirstOrDefaultAsync(
                            async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false),
                            x => x.Name == strAutosoft && x.Equipped &&
                                 (x.Extra == strName || x.Extra == strDisplayName), token: token).ConfigureAwait(false);
                    }
                    if (objAutosoft == null && ParentVehicle != null)
                    {
                        objAutosoft = await ParentVehicle.GearChildren.DeepFirstOrDefaultAsync(async x => await x.Children.ToListAsync(y => y.Equipped, token: token).ConfigureAwait(false),
                            x => x.Name == strAutosoft && x.Equipped &&
                                 (x.Extra == strName || x.Extra == strDisplayName), token: token).ConfigureAwait(false);
                    }
                    if (objAutosoft != null)
                    {
                        strReturn += string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                            strSpace, await objAutosoft.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                            await objAutosoft.GetRatingAsync(token).ConfigureAwait(false));
                    }
                    else
                    {
                        strReturn += string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                            strSpace,
                            await LanguageManager.GetStringAsync("Tip_Skill_Defaulting", token: token)
                                .ConfigureAwait(false), -1);
                    }

                    strReturn += strExtra;
                    break;
                }
                case FiringMode.RemoteOperated:
                {
                    Skill objSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync("Gunnery", token)
                        .ConfigureAwait(false);
                    if (await objSkill.Specializations.GetCountAsync(token).ConfigureAwait(false) > 0)
                    {
                        string strRelevantSpec = await GetRelevantSpecializationAsync(token).ConfigureAwait(false);
                        if (strRelevantSpec != "None")
                        {
                            SkillSpecialization spec = await objSkill.GetSpecializationAsync(strRelevantSpec, token)
                                .ConfigureAwait(false);
                            if (spec != null)
                            {
                                int intSpecBonus = await spec.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                                if (intSpecBonus != 0)
                                    strExtra = string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                        strSpace, await spec.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                        intSpecBonus) + strExtra;
                            }
                        }
                    }

                    strReturn = await objSkill.CompileDicepoolTooltipAsync("LOG",
                        await objSkill.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + strSpace, strExtra,
                        token: token).ConfigureAwait(false);
                    break;
                }
                case FiringMode.GunneryCommandDevice:
                case FiringMode.ManualOperation:
                {
                    Skill objSkill = await _objCharacter.SkillsSection.GetActiveSkillAsync("Gunnery", token)
                        .ConfigureAwait(false);
                    if (await objSkill.Specializations.GetCountAsync(token).ConfigureAwait(false) > 0)
                    {
                        string strRelevantSpec = await GetRelevantSpecializationAsync(token).ConfigureAwait(false);
                        if (strRelevantSpec != "None")
                        {
                            SkillSpecialization spec = await objSkill.GetSpecializationAsync(strRelevantSpec, token)
                                .ConfigureAwait(false);
                            if (spec != null)
                            {
                                int intSpecBonus = await spec.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                                if (intSpecBonus != 0)
                                    strExtra = string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                        strSpace, await spec.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                        intSpecBonus) + strExtra;
                            }
                        }
                    }

                    Cyberware objAttributeSource =
                        Cyberware &&
                        Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(await objSkill.GetAttributeAsync(token)
                            .ConfigureAwait(false)) &&
                        _objMountedVehicle == null
                            ? await _objCharacter.Cyberware.DeepFindByIdAsync(ParentID, token: token)
                                .ConfigureAwait(false)
                            : null;
                    while (objAttributeSource != null && await objAttributeSource
                               .GetAttributeTotalValueAsync(
                                   await objSkill.GetAttributeAsync(token).ConfigureAwait(false), token)
                               .ConfigureAwait(false) == 0)
                    {
                        objAttributeSource = await objAttributeSource.GetParentAsync(token).ConfigureAwait(false);
                    }

                    strReturn = await objSkill.CompileDicepoolTooltipAsync(string.Empty,
                        await objSkill.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + strSpace, strExtra,
                        !Cyberware || _objMountedVehicle != null, objAttributeSource, token).ConfigureAwait(false);
                    break;
                }
                case FiringMode.Skill:
                {
                    Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                    if (objSkill != null)
                    {
                        // If the character has a Specialization, include it in the Dice Pool string.
                        if (await objSkill.Specializations.GetCountAsync(token).ConfigureAwait(false) > 0 &&
                            !objSkill.IsExoticSkill)
                        {
                            SkillSpecialization spec =
                                (await objSkill
                                     .GetSpecializationAsync(
                                         await GetCurrentDisplayNameShortAsync(token).ConfigureAwait(false), token)
                                     .ConfigureAwait(false) ??
                                 await objSkill.GetSpecializationAsync(Name, token).ConfigureAwait(false) ??
                                 await objSkill
                                     .GetSpecializationAsync(
                                         await DisplayCategoryAsync(GlobalSettings.Language, token)
                                             .ConfigureAwait(false), token).ConfigureAwait(false) ??
                                 await objSkill.GetSpecializationAsync(Category, token).ConfigureAwait(false)) ??
                                (await objSkill.GetSpecializationAsync(Category.EndsWith('s')
                                     ? Category.TrimEndOnce('s')
                                     : Category + 's', token).ConfigureAwait(false) ??
                                 (await objSkill.GetSpecializationAsync(Spec, token).ConfigureAwait(false) ??
                                  await objSkill.GetSpecializationAsync(Spec2, token).ConfigureAwait(false)));

                            if (spec != null)
                            {
                                int intSpecBonus = await spec.GetSpecializationBonusAsync(token).ConfigureAwait(false);
                                if (intSpecBonus != 0)
                                    strExtra = string.Format(GlobalSettings.CultureInfo, "{0}+{0}{1}{0}({2})",
                                        strSpace, await spec.GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                                        intSpecBonus) + strExtra;
                            }
                        }

                        Cyberware objAttributeSource = null;
                        if (Cyberware &&
                            Equipment.Cyberware.CyberlimbAttributeAbbrevs.Contains(
                                await objSkill.GetAttributeAsync(token).ConfigureAwait(false)))
                        {
                            objAttributeSource =
                                _objMountedVehicle != null
                                    ? (await _objMountedVehicle.FindVehicleCyberwareAsync(x =>
                                          x.InternalId == ParentID, token).ConfigureAwait(false)).Item1 ??
                                      await _objCharacter.Cyberware.DeepFindByIdAsync(ParentID, token: token)
                                          .ConfigureAwait(false)
                                    : await _objCharacter.Cyberware.DeepFindByIdAsync(ParentID, token: token)
                                        .ConfigureAwait(false);

                            while (objAttributeSource != null && await objAttributeSource
                                       .GetAttributeTotalValueAsync(
                                           await objSkill.GetAttributeAsync(token).ConfigureAwait(false), token)
                                       .ConfigureAwait(false) == 0)
                            {
                                objAttributeSource = await objAttributeSource.GetParentAsync(token).ConfigureAwait(false);
                            }
                        }

                        strReturn = await objSkill.CompileDicepoolTooltipAsync(string.Empty,
                            await objSkill.GetCurrentDisplayNameAsync(token).ConfigureAwait(false) + strSpace, strExtra,
                            !Cyberware,
                            objAttributeSource, token).ConfigureAwait(false);
                    }

                    break;
                }
            }

            return strReturn;
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
        /// Total Availability.
        /// </summary>
        public async Task<string> TotalAvailAsync(CultureInfo objCulture, string strLanguage, CancellationToken token = default)
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
                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));
                        sbdAvail.CheapReplace(strAvail, "{Rating}", () => Rating.ToString(GlobalSettings.InvariantCultureInfo));

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

                        _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdAvail, strAvail);
                        (bool blnIsSuccess, object objProcess)
                            = CommonFunctions.EvaluateInvariantXPath(sbdAvail.ToString());
                        if (blnIsSuccess)
                            intAvail += ((double)objProcess).StandardRound();
                    }
                }
                else
                    intAvail += decValue.StandardRound();
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
        public async Task<AvailabilityValue> TotalAvailTupleAsync(bool blnCheckChildren = true, CancellationToken token = default)
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
                if (strAvail.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
                {
                    using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdAvail))
                    {
                        sbdAvail.Append(strAvail.TrimStart('+'));
                        await sbdAvail.CheapReplaceAsync(strAvail, "{Rating}", async () => (await GetRatingAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token: token).ConfigureAwait(false);

                        if (blnCheckUnderbarrels && strAvail.Contains("{Children Avail}"))
                        {
                            blnCheckUnderbarrels = false;
                            int intMaxChildAvail = 0;
                            intAvail += await UnderbarrelWeapons.SumAsync(x => x.ParentID != InternalId, async objUnderbarrel =>
                            {
                                AvailabilityValue objLoopAvail = await objUnderbarrel.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                                if (objLoopAvail.AddToParent)
                                {
                                    int intLoopChildAvail = await objLoopAvail.GetValueAsync(token).ConfigureAwait(false);
                                    if (intLoopChildAvail > intMaxChildAvail)
                                        intMaxChildAvail = intLoopChildAvail;
                                }
                                if (objLoopAvail.Suffix == 'F')
                                    chrLastAvailChar = 'F';
                                else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                                    chrLastAvailChar = 'R';
                                return objLoopAvail.AddToParent ? await objLoopAvail.GetValueAsync(token).ConfigureAwait(false) : 0;
                            }, token).ConfigureAwait(false);

                            sbdAvail.Replace("{Children Avail}",
                                             intMaxChildAvail.ToString(GlobalSettings.InvariantCultureInfo));
                        }

                        await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdAvail, strAvail, token: token).ConfigureAwait(false);
                        (bool blnIsSuccess, object objProcess)
                            = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                        if (blnIsSuccess)
                            intAvail += ((double)objProcess).StandardRound();
                    }
                }
                else
                    intAvail += decValue.StandardRound();
            }

            if (blnCheckUnderbarrels)
            {
                intAvail += await UnderbarrelWeapons.SumAsync(x => x.ParentID != InternalId, async objUnderbarrel =>
                {
                    AvailabilityValue objLoopAvail = await objUnderbarrel.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? await objLoopAvail.GetValueAsync(token).ConfigureAwait(false) : 0;
                }, token).ConfigureAwait(false);
            }

            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                intAvail += await WeaponAccessories.SumAsync(x => !x.IncludedInWeapon && x.Equipped, async objAccessory =>
                {
                    AvailabilityValue objLoopAvail = await objAccessory.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? await objLoopAvail.GetValueAsync(token).ConfigureAwait(false) : 0;
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
                return BaseMatrixBoxes + this.GetTotalMatrixAttribute("Device Rating").DivAwayFromZero(2) + TotalBonusMatrixBoxes;
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

                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Weapon, InternalId + "Wireless", WirelessBonus, 1, CurrentDisplayNameShort);
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

            foreach (Weapon objWeapon in Children.AsEnumerableWithSideEffects())
                objWeapon.RefreshWirelessBonuses();
            foreach (WeaponAccessory objAccessory in WeaponAccessories.AsEnumerableWithSideEffects())
                objAccessory.RefreshWirelessBonuses();
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this weapon accessory.
        /// </summary>
        public async Task RefreshWirelessBonusesAsync(CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(WirelessBonus?.InnerText))
            {
                if (WirelessOn && Equipped && Parent?.WirelessOn != false)
                {
                    if (WirelessBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
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
                    if (WirelessBonus?.SelectSingleNodeAndCacheExpressionAsNavigator("@mode", token)?.Value == "replace")
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

            await Children.ForEachWithSideEffectsAsync(x => x.RefreshWirelessBonusesAsync(token), token: token).ConfigureAwait(false);
            await WeaponAccessories.ForEachWithSideEffectsAsync(x => x.RefreshWirelessBonusesAsync(token), token: token).ConfigureAwait(false);
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
            decimal decReturn = Children.AsEnumerableWithSideEffects().Sum(x => x.DeleteWeapon(false))
                                + WeaponAccessories.AsEnumerableWithSideEffects().Sum(x => x.DeleteWeaponAccessory(false));

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

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Weapon, InternalId + "Wireless");

            DisposeSelf();

            return decReturn;
        }

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public async Task<decimal> DeleteWeaponAsync(bool blnDoRemoval = true, CancellationToken token = default)
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
            decimal decReturn = await Children.SumWithSideEffectsAsync(x => x.DeleteWeaponAsync(false, token), token: token)
                                              .ConfigureAwait(false)
                                + await WeaponAccessories
                                        .SumWithSideEffectsAsync(x => x.DeleteWeaponAccessoryAsync(false, token), token)
                                        .ConfigureAwait(false);

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
        public async Task<int> CheckRestrictedGear(IDictionary<int, int> dicRestrictedGearLimits,
                                                        StringBuilder sbdAvailItems, StringBuilder sbdRestrictedItems,
                                                        CancellationToken token = default)
        {
            int intRestrictedCount = 0;
            if (!IncludedInWeapon && string.IsNullOrEmpty(ParentID))
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

            intRestrictedCount += await UnderbarrelWeapons.SumAsync(objChild =>
                                                                        objChild.CheckRestrictedGear(
                                                                            dicRestrictedGearLimits, sbdAvailItems,
                                                                            sbdRestrictedItems, token), token)
                                                          .ConfigureAwait(false)
                                  + await WeaponAccessories.SumAsync(objChild =>
                                                                         objChild.CheckRestrictedGear(
                                                                             dicRestrictedGearLimits, sbdAvailItems,
                                                                             sbdRestrictedItems, token), token)
                                                           .ConfigureAwait(false);

            return intRestrictedCount;
        }

        public async Task Reload(IAsyncCollection<Gear> lstGears, TreeView treGearView,
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
                if (ammoString.IndexOfAny('×', 'x', '+') != -1 ||
                    ammoString.Contains(" or ", StringComparison.OrdinalIgnoreCase) ||
                    ammoString.Contains("Special", StringComparison.OrdinalIgnoreCase) ||
                    ammoString.Contains("External Source", StringComparison.OrdinalIgnoreCase))
                {
                    string strWeaponAmmo = ammoString.FastEscape("External Source", StringComparison.OrdinalIgnoreCase);
                    strWeaponAmmo = strWeaponAmmo.ToLowerInvariant();
                    // Get rid of or belt, and + energy.
                    strWeaponAmmo = strWeaponAmmo.FastEscapeOnceFromEnd(" + energy", eComparison: StringComparison.OrdinalIgnoreCase)
                                                 .Replace(" or belt", " or 250(belt)");

                    foreach (string strAmmo in
                             strWeaponAmmo.SplitNoAlloc(" or ", StringSplitOptions.RemoveEmptyEntries, StringComparison.OrdinalIgnoreCase))
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
            if (ammoString.IndexOfAny('×', 'x', '+') != -1 ||
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
                strWeaponAmmo = strWeaponAmmo.FastEscapeOnceFromEnd(" + energy", eComparison: StringComparison.OrdinalIgnoreCase)
                                             .Replace(" or belt", " or 250(belt)");

                foreach (string strAmmo in
                         strWeaponAmmo.SplitNoAlloc(" or ", StringSplitOptions.RemoveEmptyEntries, StringComparison.OrdinalIgnoreCase))
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
                    await Program.ShowScrollableMessageBoxAsync(string.Format(GlobalSettings.CultureInfo,
                            await LanguageManager
                                .GetStringAsync("Message_OutOfAmmoType", token: token)
                                .ConfigureAwait(false),
                            await GetCurrentDisplayNameAsync(token).ConfigureAwait(false)),
                        await LanguageManager.GetStringAsync("Message_OutOfAmmo", token: token)
                            .ConfigureAwait(false),
                        icon: MessageBoxIcon.Warning, token: token).ConfigureAwait(false);
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
                decimal decQty = await frmReloadWeapon.MyForm.GetSelectedCountAsync(token).ConfigureAwait(false);
                string strSelectedAmmo = await frmReloadWeapon.MyForm.GetSelectedAmmoAsync(token).ConfigureAwait(false);
                // If an External Source is not being used, consume ammo.
                if (strSelectedAmmo != objExternalSource?.InternalId)
                {
                    objSelectedAmmo = await lstGears.DeepFindByIdAsync(strSelectedAmmo, token: token).ConfigureAwait(false);

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
                            await objDuplicatedParent.SetQuantityAsync(1, token).ConfigureAwait(false);
                            await lstGears.AddAsync(objDuplicatedParent, token).ConfigureAwait(false);
                            await objParent.SetQuantityAsync(objParent.Quantity - 1, token).ConfigureAwait(false);
                            Gear objNewSelectedAmmo
                                = await objDuplicatedParent.Children.DeepFindByIdAsync(strSelectedAmmo, token: token).ConfigureAwait(false);
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
                            await objCurrentlyLoadedAmmo.SetQuantityAsync(objCurrentlyLoadedAmmo.Quantity - objSelectedAmmo.Quantity, token).ConfigureAwait(false);
                            await objSelectedAmmo.DeleteGearAsync(token: token).ConfigureAwait(false);
                            GetClip(_intActiveAmmoSlot).Ammo
                                = objCurrentlyLoadedAmmo.Quantity
                                                        .ToInt32(); // Bypass AmmoRemaining so as not to alter the gear quantity
                        }
                        else
                        {
                            await objCurrentlyLoadedAmmo.SetQuantityAsync(decQty, token).ConfigureAwait(false);
                            await objSelectedAmmo.SetQuantityAsync(objSelectedAmmo.Quantity - decTopUp, token).ConfigureAwait(false);
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
                        await objNewSelectedAmmo.SetQuantityAsync(decQty, token).ConfigureAwait(false);
                        await lstGears.AddAsync(objNewSelectedAmmo, token).ConfigureAwait(false);
                        await objSelectedAmmo.SetQuantityAsync(objSelectedAmmo.Quantity + decQty, token).ConfigureAwait(false);
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

                await SetAmmoLoadedAsync(objSelectedAmmo, token).ConfigureAwait(false);
                if (objCurrentlyLoadedAmmo != objSelectedAmmo)
                {
                    if (objCurrentlyLoadedAmmo != null)
                    {
                        string strId = objCurrentlyLoadedAmmo.InternalId;
                        string strText = await objCurrentlyLoadedAmmo.GetCurrentDisplayNameAsync(token)
                            .ConfigureAwait(false);
                        if (objSelectedAmmo != null)
                        {
                            string strId2 = objSelectedAmmo.InternalId;
                            string strText2 = await objSelectedAmmo.GetCurrentDisplayNameAsync(token)
                                .ConfigureAwait(false);
                            await treGearView.DoThreadSafeAsync(x =>
                            {
                                // Refresh the Gear tree.
                                TreeNode objSelectedNode = x.FindNode(strId);
                                if (objSelectedNode != null)
                                    objSelectedNode.Text = strText;
                                objSelectedNode = x.FindNode(strId2);
                                if (objSelectedNode != null)
                                    objSelectedNode.Text = strText2;
                            }, token: token).ConfigureAwait(false);
                        }
                        else
                        {
                            await treGearView.DoThreadSafeAsync(x =>
                            {
                                // Refresh the Gear tree.
                                TreeNode objSelectedNode = x.FindNode(strId);
                                if (objSelectedNode != null)
                                    objSelectedNode.Text = strText;
                            }, token: token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        string strId = objSelectedAmmo.InternalId;
                        string strText = await objSelectedAmmo.GetCurrentDisplayNameAsync(token)
                            .ConfigureAwait(false);
                        await treGearView.DoThreadSafeAsync(x =>
                        {
                            // Refresh the Gear tree.
                            TreeNode objSelectedNode = x.FindNode(strId);
                            if (objSelectedNode != null)
                                objSelectedNode.Text = strText;
                        }, token: token).ConfigureAwait(false);
                    }
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

        private static async Task<Gear> UnloadGearAsync(IAsyncCollection<Gear> lstGears, Clip clipToUnload,
                                                             CancellationToken token = default)
        {
            Gear objAmmo = clipToUnload.AmmoGear;
            if (objAmmo == null)
                return null;
            await clipToUnload.SetAmmoGearAsync(null, token).ConfigureAwait(false);
            Gear objMergeGear = await lstGears
                                      .FirstOrDefaultAsync(
                                          x => x.InternalId != objAmmo.InternalId
                                               && x.IsIdenticalToOtherGear(objAmmo, true), token).ConfigureAwait(false);
            if (objMergeGear != null)
            {
                await objMergeGear.SetQuantityAsync(objMergeGear.Quantity + objAmmo.Quantity, token).ConfigureAwait(false);
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
                foreach (Gear objGear in lstGears.DeepWhere(x => x.Children.Where(y => y.Equipped), x =>
                    x.Quantity > 0
                    && x.Equipped
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
                    foreach (Gear objGear in lstGears.DeepWhere(x => x.Children.Where(y => y.Equipped), x =>
                                                                    x.Quantity > 0
                                                                    && x.Equipped
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
                    foreach (Gear objGear in lstGears.DeepWhere(x => x.Children.Where(y => y.Equipped), x =>
                                                                    x.Quantity > 0
                                                                    && x.Equipped
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
                foreach (Gear objGear in lstGears.DeepWhere(x => x.Children.Where(y => y.Equipped), x =>
                    x.Quantity > 0
                    && x.Equipped
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
                foreach (Gear objGear in lstGears.DeepWhere(x => x.Children.Where(y => y.Equipped), x =>
                    x.Quantity > 0
                    && x.Equipped
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
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if ((Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)) && !string.IsNullOrEmpty(Source) && !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                Tag = this,
                ContextMenuStrip = cmsWeapon,
                ForeColor = await GetPreferredColorAsync(token).ConfigureAwait(false),
                ToolTipText = (await GetNotesAsync(token).ConfigureAwait(false)).WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // Add Underbarrel Weapons.
            await UnderbarrelWeapons.ForEachAsync(async objUnderbarrelWeapon =>
            {
                TreeNode objLoopNode =
                    await objUnderbarrelWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear,
                        token).ConfigureAwait(false);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }, token).ConfigureAwait(false);
            // Add attached Weapon Accessories.
            await WeaponAccessories.ForEachAsync(async objAccessory =>
            {
                TreeNode objLoopNode = await objAccessory.CreateTreeNode(cmsWeaponAccessory, cmsWeaponAccessoryGear, token).ConfigureAwait(false);
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
                    return Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        public async Task<Color> GetPreferredColorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(await GetNotesAsync(token).ConfigureAwait(false)))
            {
                return Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)
                    ? ColorManager.GenerateCurrentModeDimmedColor(await GetNotesColorAsync(token).ConfigureAwait(false))
                    : ColorManager.GenerateCurrentModeColor(await GetNotesColorAsync(token).ConfigureAwait(false));
            }
            return Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
        }

        public void SetupChildrenWeaponsCollectionChanged(bool blnAdd, TreeView treWeapons, ContextMenuStrip cmsWeapon = null, ContextMenuStrip cmsWeaponAccessory = null, ContextMenuStrip cmsWeaponAccessoryGear = null, AsyncNotifyCollectionChangedEventHandler funcMakeDirty = null)
        {
            if (blnAdd)
            {
                Task FuncUnderbarrelWeaponsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshChildrenWeaponsClearBindings(treWeapons, y, innerToken);

                Task FuncUnderbarrelWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshChildrenWeapons(treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear,
                        null, y, funcMakeDirty, token: innerToken);

                Task FuncWeaponAccessoriesBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshWeaponAccessoriesClearBindings(treWeapons, y, innerToken);

                Task FuncWeaponAccessoriesToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshWeaponAccessories(treWeapons, cmsWeaponAccessory, cmsWeaponAccessoryGear,
                        () => UnderbarrelWeapons.GetCountAsync(innerToken), y, funcMakeDirty, token: innerToken);

                UnderbarrelWeapons.AddTaggedBeforeClearCollectionChanged(treWeapons,
                    FuncUnderbarrelWeaponsBeforeClearToAdd);
                UnderbarrelWeapons.AddTaggedCollectionChanged(treWeapons, FuncUnderbarrelWeaponsToAdd);
                WeaponAccessories.AddTaggedBeforeClearCollectionChanged(treWeapons,
                    FuncWeaponAccessoriesBeforeClearToAdd);
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
                    Task FuncWeaponAccessoryGearBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                        CancellationToken innerToken = default) =>
                        this.RefreshChildrenGearsClearBindings(treWeapons, y, innerToken);

                    Task FuncWeaponAccessoryGearToAdd(object x, NotifyCollectionChangedEventArgs y,
                        CancellationToken innerToken = default) =>
                        objChild.RefreshChildrenGears(treWeapons, cmsWeaponAccessoryGear, null, null, y, funcMakeDirty,
                            token: innerToken);

                    objChild.GearChildren.AddTaggedBeforeClearCollectionChanged(treWeapons,
                        FuncWeaponAccessoryGearBeforeClearToAdd);
                    objChild.GearChildren.AddTaggedCollectionChanged(treWeapons, FuncWeaponAccessoryGearToAdd);
                    if (funcMakeDirty != null)
                        objChild.GearChildren.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                    foreach (Gear objGear in objChild.GearChildren)
                        objGear.SetupChildrenGearsCollectionChanged(true, treWeapons, cmsWeaponAccessoryGear, null, funcMakeDirty);
                }
            }
            else
            {
                UnderbarrelWeapons.RemoveTaggedAsyncBeforeClearCollectionChanged(treWeapons);
                UnderbarrelWeapons.RemoveTaggedAsyncCollectionChanged(treWeapons);
                WeaponAccessories.RemoveTaggedAsyncBeforeClearCollectionChanged(treWeapons);
                WeaponAccessories.RemoveTaggedAsyncCollectionChanged(treWeapons);
                foreach (Weapon objChild in UnderbarrelWeapons)
                {
                    objChild.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                }
                foreach (WeaponAccessory objChild in WeaponAccessories)
                {
                    objChild.GearChildren.RemoveTaggedAsyncBeforeClearCollectionChanged(treWeapons);
                    objChild.GearChildren.RemoveTaggedAsyncCollectionChanged(treWeapons);
                    foreach (Gear objGear in objChild.GearChildren)
                        objGear.SetupChildrenGearsCollectionChanged(false, treWeapons);
                }
            }
        }

        public async Task SetupChildrenWeaponsCollectionChangedAsync(bool blnAdd, TreeView treWeapons, ContextMenuStrip cmsWeapon = null, ContextMenuStrip cmsWeaponAccessory = null, ContextMenuStrip cmsWeaponAccessoryGear = null, AsyncNotifyCollectionChangedEventHandler funcMakeDirty = null, CancellationToken token = default)
        {
            if (blnAdd)
            {
                Task FuncUnderbarrelWeaponsBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshChildrenWeaponsClearBindings(treWeapons, y, innerToken);

                Task FuncUnderbarrelWeaponsToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshChildrenWeapons(treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear,
                        null, y, funcMakeDirty, token: innerToken);

                Task FuncWeaponAccessoriesBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                    CancellationToken innerToken = default) =>
                    this.RefreshWeaponAccessoriesClearBindings(treWeapons, y, innerToken);

                Task FuncWeaponAccessoriesToAdd(object x, NotifyCollectionChangedEventArgs y, CancellationToken innerToken = default) =>
                    this.RefreshWeaponAccessories(treWeapons, cmsWeaponAccessory, cmsWeaponAccessoryGear,
                        () => UnderbarrelWeapons.GetCountAsync(innerToken), y, funcMakeDirty, token: innerToken);

                UnderbarrelWeapons.AddTaggedBeforeClearCollectionChanged(treWeapons,
                    FuncUnderbarrelWeaponsBeforeClearToAdd);
                UnderbarrelWeapons.AddTaggedCollectionChanged(treWeapons, FuncUnderbarrelWeaponsToAdd);
                WeaponAccessories.AddTaggedBeforeClearCollectionChanged(treWeapons,
                    FuncWeaponAccessoriesBeforeClearToAdd);
                WeaponAccessories.AddTaggedCollectionChanged(treWeapons, FuncWeaponAccessoriesToAdd);
                if (funcMakeDirty != null)
                {
                    UnderbarrelWeapons.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                    WeaponAccessories.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                }
                await UnderbarrelWeapons.ForEachWithSideEffectsAsync(
                    objChild => objChild.SetupChildrenWeaponsCollectionChangedAsync(true, treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, funcMakeDirty, token),
                    token).ConfigureAwait(false);

                await WeaponAccessories.ForEachWithSideEffectsAsync(async objChild =>
                {
                    Task FuncWeaponAccessoryGearBeforeClearToAdd(object x, NotifyCollectionChangedEventArgs y,
                        CancellationToken innerToken = default) =>
                        this.RefreshChildrenGearsClearBindings(treWeapons, y, innerToken);

                    Task FuncWeaponAccessoryGearToAdd(object x, NotifyCollectionChangedEventArgs y,
                        CancellationToken innerToken = default) =>
                        objChild.RefreshChildrenGears(treWeapons, cmsWeaponAccessoryGear, null, null, y, funcMakeDirty,
                            token: innerToken);

                    objChild.GearChildren.AddTaggedBeforeClearCollectionChanged(treWeapons,
                        FuncWeaponAccessoryGearBeforeClearToAdd);
                    objChild.GearChildren.AddTaggedCollectionChanged(treWeapons, FuncWeaponAccessoryGearToAdd);
                    if (funcMakeDirty != null)
                        objChild.GearChildren.AddTaggedCollectionChanged(treWeapons, funcMakeDirty);
                    await objChild.GearChildren.ForEachWithSideEffectsAsync(
                        objGear => objGear.SetupChildrenGearsCollectionChangedAsync(true, treWeapons,
                            cmsWeaponAccessoryGear, null, funcMakeDirty, token),
                        token).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
            }
            else
            {
                await UnderbarrelWeapons.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                await UnderbarrelWeapons.RemoveTaggedAsyncCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                await WeaponAccessories.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                await WeaponAccessories.RemoveTaggedAsyncCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                await UnderbarrelWeapons.ForEachWithSideEffectsAsync(
                    objChild => objChild.SetupChildrenWeaponsCollectionChangedAsync(false, treWeapons, token: token),
                    token).ConfigureAwait(false);
                await WeaponAccessories.ForEachWithSideEffectsAsync(async objChild =>
                {
                    await objChild.GearChildren.RemoveTaggedAsyncBeforeClearCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                    await objChild.GearChildren.RemoveTaggedAsyncCollectionChangedAsync(treWeapons, token).ConfigureAwait(false);
                    await objChild.GearChildren.ForEachWithSideEffectsAsync(
                        objGear => objGear.SetupChildrenGearsCollectionChangedAsync(false, treWeapons, token: token),
                        token).ConfigureAwait(false);
                }, token).ConfigureAwait(false);
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

                                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
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

                                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
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

        private async Task RemoveAmmoSlotsAsync(WeaponAccessory objAccessory, CancellationToken token = default)
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
            ThreadSafeObservableCollection<Gear> lstGear = ParentVehicle != null
                        ? ParentVehicle.GearChildren
                        : _objCharacter.Gear;
            foreach (Clip objClip in Clips)
            {
                UnloadGear(lstGear, objClip);
            }
        }

        /// <summary>
        /// Unload every clip.
        /// </summary>
        public async Task UnloadAllAsync(CancellationToken token = default)
        {
            ThreadSafeObservableCollection<Gear> lstGear = ParentVehicle != null
                ? ParentVehicle.GearChildren
                : _objCharacter.Gear;
            foreach (Clip objClip in Clips)
            {
                await UnloadGearAsync(lstGear, objClip, token).ConfigureAwait(false);
            }
        }

        private Clip GetClip(int clip)
        {
            //1 indexed due legacy
            return _lstAmmo[clip - 1];
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

        private async Task<IHasMatrixAttributes> GetMatrixAttributesOverrideAsync(CancellationToken token = default)
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

        public Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(true, token);

        public Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenTotalCostAsync(false, token);

        public async Task<decimal> CalculatedStolenTotalCostAsync(bool blnStolen, CancellationToken token = default)
        {
            decimal decReturn = Stolen == blnStolen ? await GetOwnCostAsync(token).ConfigureAwait(false) : 0;

            // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
            decReturn += await WeaponAccessories.SumAsync(objAccessory => objAccessory.CalculatedStolenTotalCostAsync(blnStolen, token), token).ConfigureAwait(false);

            // Include the cost of any Underbarrel Weapon.
            decReturn += await Children.SumAsync(objUnderbarrel => objUnderbarrel.CalculatedStolenTotalCostAsync(blnStolen, token), token).ConfigureAwait(false);

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

            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
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
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }
            return decValue.StandardRound();
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

            if (strExpression.DoesNeedXPathProcessingToBeConvertedToNumber(out decimal decValue))
            {
                using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
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
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token)
                            .ConfigureAwait(false);
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }
            return decValue.StandardRound();
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
            IHasMatrixAttributes objThis = await GetMatrixAttributesOverrideAsync(token).ConfigureAwait(false);
            if (objThis != null)
                return await objThis.GetBonusMatrixAttributeAsync(strAttributeName, token).ConfigureAwait(false);
            int intReturn = await GetOverclockedAsync(token).ConfigureAwait(false) == strAttributeName ? 1 : 0;

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            intReturn += await Children.SumAsync(x => x.Equipped && x.ParentID != InternalId,
                x => x.GetTotalMatrixAttributeAsync(strAttributeName, token), token).ConfigureAwait(false);

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

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!CanBeRemoved)
                return false;
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteWeapon", token: token)
                            .ConfigureAwait(false), token: token).ConfigureAwait(false))
                return false;
            await DeleteWeaponAsync(token: token).ConfigureAwait(false);
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

        public bool Sell(decimal decPercentage, bool blnConfirmDelete)
        {
            if (!_objCharacter.Created)
                return Remove(blnConfirmDelete);
            if (!CanBeRemoved)
                return false;
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeapon")))
                return false;

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
            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = objParent.TotalCost;
                decAmount = DeleteWeapon() * decPercentage;
                decAmount += (decOriginal - objParent.TotalCost) * decPercentage;
            }
            else
            {
                decimal decOriginal = TotalCost;
                decAmount = (DeleteWeapon() + decOriginal) * decPercentage;
            }
            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString(strExpense) + ' ' + CurrentDisplayNameShort, ExpenseType.Nuyen, DateTime.Now);
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
                        await LanguageManager.GetStringAsync("Message_DeleteWeapon", token: token).ConfigureAwait(false),
                        token).ConfigureAwait(false))
                return false;

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
            decimal decAmount;
            if (objParent != null)
            {
                decimal decOriginal = await objParent.GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = await DeleteWeaponAsync(token: token).ConfigureAwait(false) * decPercentage;
                decAmount += (decOriginal - await objParent.GetTotalCostAsync(token).ConfigureAwait(false)) * decPercentage;
            }
            else
            {
                decimal decOriginal = await GetTotalCostAsync(token).ConfigureAwait(false);
                decAmount = (await DeleteWeaponAsync(token: token).ConfigureAwait(false) + decOriginal) * decPercentage;
            }

            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount,
                await LanguageManager.GetStringAsync(strExpense, token: token).ConfigureAwait(false) +
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
                    case ClipboardContentType.WeaponAccessory:
                        XPathNavigator checkNode =
                            (await GlobalSettings.GetClipboardAsync(token).ConfigureAwait(false)).SelectSingleNodeAndCacheExpressionAsNavigator(
                                "/character/weaponaccessories/accessory", token);
                        if (await CheckAccessoryRequirementsAsync(checkNode, token).ConfigureAwait(false))
                            return true;
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

        public Task<bool> AllowPasteObject(object input, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether a given WeaponAccessory is allowed to be added to this weapon.
        /// </summary>
        public bool CheckAccessoryRequirements(XPathNavigator objXmlAccessory)
        {
            if (objXmlAccessory == null)
                return false;
            string strPossibleMounts = objXmlAccessory.SelectSingleNodeAndCacheExpression("mount")?.Value ?? string.Empty;
            string strPossibleExtraMounts = objXmlAccessory.SelectSingleNodeAndCacheExpression("extramount")?.Value ?? string.Empty;
            bool blnMountFound = string.IsNullOrEmpty(strPossibleMounts);
            bool blnExtraMountFound = string.IsNullOrEmpty(strPossibleExtraMounts);
            if (!blnMountFound)
            {
                if (!blnExtraMountFound)
                {
                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                        out HashSet<string> setPossibleMounts))
                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                        out HashSet<string> setPossibleExtraMounts))
                    {
                        foreach (string strLoopMount in strPossibleMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                        {
                            setPossibleMounts.Add(strLoopMount);
                        }
                        foreach (string strLoopMount in strPossibleExtraMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                        {
                            setPossibleExtraMounts.Add(strLoopMount);
                        }
                        if (!setPossibleMounts.SetEquals(setPossibleExtraMounts))
                        {
                            // First loop: find and remove mounts that only match one of the two
                            foreach (string strMount in GetAccessoryMounts())
                            {
                                if (setPossibleMounts.Contains(strMount))
                                {
                                    if (!setPossibleExtraMounts.Contains(strMount))
                                    {
                                        blnMountFound = true;
                                        if (blnExtraMountFound)
                                            break;
                                    }
                                }
                                else if (setPossibleExtraMounts.Contains(strMount))
                                {
                                    blnExtraMountFound = true;
                                    if (blnMountFound)
                                        break;
                                }
                            }
                        }
                        // Only remaining mounts (if not found) are ones that are in both strings, so much main mount first and then extra mount
                        if (!blnMountFound || !blnExtraMountFound)
                        {
                            foreach (string strMount in GetAccessoryMounts())
                            {
                                if (!blnMountFound && setPossibleMounts.Contains(strMount))
                                {
                                    blnMountFound = true;
                                    if (blnExtraMountFound)
                                        break;
                                }
                                else if (!blnExtraMountFound && setPossibleExtraMounts.Contains(strMount))
                                {
                                    blnExtraMountFound = true;
                                    if (blnMountFound)
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                        out HashSet<string> setPossibleMounts))
                    {
                        foreach (string strLoopMount in strPossibleExtraMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                            setPossibleMounts.Add(strLoopMount);
                        blnMountFound = GetAccessoryMounts().Any(x => setPossibleMounts.Contains(x));
                    }
                }
            }
            else if (!blnExtraMountFound)
            {
                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                    out HashSet<string> setPossibleExtraMounts))
                {
                    foreach (string strLoopMount in strPossibleExtraMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                        setPossibleExtraMounts.Add(strLoopMount);
                    blnExtraMountFound = GetAccessoryMounts().Any(x => setPossibleExtraMounts.Contains(x));
                }
            }

            if (!blnMountFound || !blnExtraMountFound)
                return false;

            if (!objXmlAccessory.RequirementsMet(_objCharacter, this))
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
        public async Task<bool> CheckAccessoryRequirementsAsync(XPathNavigator objXmlAccessory,
                                                                     CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objXmlAccessory == null)
                return false;
            string strPossibleMounts = objXmlAccessory.SelectSingleNodeAndCacheExpression("mount", token)?.Value ?? string.Empty;
            string strPossibleExtraMounts = objXmlAccessory.SelectSingleNodeAndCacheExpression("extramount", token)?.Value ?? string.Empty;
            bool blnMountFound = string.IsNullOrEmpty(strPossibleMounts);
            bool blnExtraMountFound = string.IsNullOrEmpty(strPossibleExtraMounts);
            if (!blnMountFound)
            {
                if (!blnExtraMountFound)
                {
                    List<string> lstAvailableMounts = await GetAccessoryMountsAsync(token: token).ConfigureAwait(false);
                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                        out HashSet<string> setPossibleMounts))
                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                        out HashSet<string> setPossibleExtraMounts))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (string strLoopMount in strPossibleMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                        {
                            token.ThrowIfCancellationRequested();
                            setPossibleMounts.Add(strLoopMount);
                        }
                        foreach (string strLoopMount in strPossibleExtraMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                        {
                            token.ThrowIfCancellationRequested();
                            setPossibleExtraMounts.Add(strLoopMount);
                        }
                        token.ThrowIfCancellationRequested();
                        if (!setPossibleMounts.SetEquals(setPossibleExtraMounts))
                        {
                            token.ThrowIfCancellationRequested();
                            // First loop: find and remove mounts that only match one of the two
                            foreach (string strMount in lstAvailableMounts)
                            {
                                token.ThrowIfCancellationRequested();
                                if (setPossibleMounts.Contains(strMount))
                                {
                                    if (!setPossibleExtraMounts.Contains(strMount))
                                    {
                                        blnMountFound = true;
                                        if (blnExtraMountFound)
                                            break;
                                    }
                                }
                                else if (setPossibleExtraMounts.Contains(strMount))
                                {
                                    blnExtraMountFound = true;
                                    if (blnMountFound)
                                        break;
                                }
                            }
                        }
                        // Only remaining mounts (if not found) are ones that are in both strings, so much main mount first and then extra mount
                        if (!blnMountFound || !blnExtraMountFound)
                        {
                            token.ThrowIfCancellationRequested();
                            foreach (string strMount in lstAvailableMounts)
                            {
                                token.ThrowIfCancellationRequested();
                                if (!blnMountFound && setPossibleMounts.Contains(strMount))
                                {
                                    blnMountFound = true;
                                    if (blnExtraMountFound)
                                        break;
                                }
                                else if (!blnExtraMountFound && setPossibleExtraMounts.Contains(strMount))
                                {
                                    blnExtraMountFound = true;
                                    if (blnMountFound)
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                        out HashSet<string> setPossibleMounts))
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (string strLoopMount in strPossibleMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                        {
                            token.ThrowIfCancellationRequested();
                            setPossibleMounts.Add(strLoopMount);
                        }
                        token.ThrowIfCancellationRequested();
                        blnMountFound = (await GetAccessoryMountsAsync(token: token)).Any(x => setPossibleMounts.Contains(x));
                    }
                }
            }
            else if (!blnExtraMountFound)
            {
                using (new FetchSafelyFromSafeObjectPool<HashSet<string>>(Utils.StringHashSetPool,
                    out HashSet<string> setPossibleExtraMounts))
                {
                    token.ThrowIfCancellationRequested();
                    foreach (string strLoopMount in strPossibleExtraMounts.SplitNoAlloc('/', StringSplitOptions.RemoveEmptyEntries))
                    {
                        token.ThrowIfCancellationRequested();
                        setPossibleExtraMounts.Add(strLoopMount);
                    }
                    token.ThrowIfCancellationRequested();
                    blnExtraMountFound = (await GetAccessoryMountsAsync(token: token)).Any(x => setPossibleExtraMounts.Contains(x));
                }
            }

            if (!blnMountFound || !blnExtraMountFound)
                return false;

            if (!await objXmlAccessory.RequirementsMetAsync(_objCharacter, this, token: token).ConfigureAwait(false))
                return false;

            XPathNavigator xmlTestNode = objXmlAccessory
                                               .SelectSingleNodeAndCacheExpression(
                                                   "forbidden/weapondetails", token);
            if (xmlTestNode != null)
            {
                XPathNavigator xmlRequirementsNode = await this.GetNodeXPathAsync(token).ConfigureAwait(false);
                // Assumes topmost parent is an AND node
                if (await xmlRequirementsNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token)
                                             .ConfigureAwait(false))
                    return false;
                xmlTestNode = objXmlAccessory
                                    .SelectSingleNodeAndCacheExpression("required/weapondetails", token);
                // Assumes topmost parent is an AND node
                return xmlTestNode == null || await xmlRequirementsNode
                                                    .ProcessFilterOperationNodeAsync(xmlTestNode, false, token)
                                                    .ConfigureAwait(false);
            }

            xmlTestNode = objXmlAccessory.SelectSingleNodeAndCacheExpression("required/weapondetails", token);
            // Assumes topmost parent is an AND node
            return xmlTestNode == null || await (await this.GetNodeXPathAsync(token).ConfigureAwait(false))
                                                .ProcessFilterOperationNodeAsync(xmlTestNode, false, token)
                                                .ConfigureAwait(false);
        }

        public string ProcessAttributesInXPath(string strInput, bool blnForRange = false)
        {
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdInput))
            {
                sbdInput.Append(strInput);
                ProcessAttributesInXPath(sbdInput, strInput, blnForRange);
                return sbdInput.ToString();
            }
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
            if (ParentVehicle != null)
                ParentVehicle.ProcessAttributesInXPath(sbdInput, strOriginal, dicValueOverrides: dicAttributeOverrides);
            else
            {
                Vehicle.FillAttributesInXPathWithDummies(sbdInput);
                _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdInput, strOriginal, dicAttributeOverrides);
            }
        }

        public async Task<string> ProcessAttributesInXPathAsync(string strInput, bool blnForRange = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strInput))
                return string.Empty;
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdInput))
            {
                sbdInput.Append(strInput);
                await ProcessAttributesInXPathAsync(sbdInput, strInput, blnForRange, token).ConfigureAwait(false);
                return sbdInput.ToString();
            }
        }

        public async Task ProcessAttributesInXPathAsync(StringBuilder sbdInput, string strOriginal = "", bool blnForRange = false, CancellationToken token = default)
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
                            (Cyberware objWeaponParent, VehicleMod objVehicleMod) = await _objCharacter.Vehicles.FindVehicleCyberwareAsync(x => x.InternalId == ParentID, token).ConfigureAwait(false);
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
                                    objAttributeSource = await objAttributeSource.GetParentAsync(token).ConfigureAwait(false);
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
                        Cyberware objWeaponParent = await _objCharacter.Cyberware.DeepFirstOrDefaultAsync(x => x.Children, x => x.InternalId == ParentID, token).ConfigureAwait(false);
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
                                objAttributeSource = await objAttributeSource.GetParentAsync(token).ConfigureAwait(false);
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

                if (Category == "Throwing Weapons")
                    intUseSTR += (blnForRange
                            ? await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowSTR, token: token).ConfigureAwait(false) +
                              await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowRangeSTR, token: token).ConfigureAwait(false)
                            : await ImprovementManager.ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowSTR, token: token).ConfigureAwait(false))
                        .StandardRound();
                else
                {
                    Skill objSkill = await GetSkillAsync(token).ConfigureAwait(false);
                    string strSkillDictionaryKey = objSkill != null
                        ? await objSkill.GetDictionaryKeyAsync(token).ConfigureAwait(false)
                        : string.Empty;
                    if (strSkillDictionaryKey == "Throwing Weapons")
                        intUseSTR += (blnForRange
                                ? await ImprovementManager
                                      .ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowSTR, token: token)
                                      .ConfigureAwait(false) +
                                  await ImprovementManager.ValueOfAsync(_objCharacter,
                                      Improvement.ImprovementType.ThrowRangeSTR, token: token).ConfigureAwait(false)
                                : await ImprovementManager
                                    .ValueOfAsync(_objCharacter, Improvement.ImprovementType.ThrowSTR, token: token)
                                    .ConfigureAwait(false))
                            .StandardRound();
                }

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
            if (ParentVehicle != null)
                await ParentVehicle.ProcessAttributesInXPathAsync(sbdInput, strOriginal, dicValueOverrides: dicAttributeOverrides, token: token).ConfigureAwait(false);
            else
            {
                Vehicle.FillAttributesInXPathWithDummies(sbdInput);
                await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdInput, strOriginal, dicAttributeOverrides, token).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _lstAccessories.EnumerateWithSideEffects().ForEach(x => x.Dispose());
            _lstUnderbarrel.EnumerateWithSideEffects().ForEach(x => x.Dispose());
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
            await _lstAccessories.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
            await _lstUnderbarrel.ForEachWithSideEffectsAsync(async x => await x.DisposeAsync().ConfigureAwait(false)).ConfigureAwait(false);
            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private async ValueTask DisposeSelfAsync()
        {
            await _lstAccessories.DisposeAsync().ConfigureAwait(false);
            await _lstUnderbarrel.DisposeAsync().ConfigureAwait(false);
        }

        public Character CharacterObject => _objCharacter;
    }
}
