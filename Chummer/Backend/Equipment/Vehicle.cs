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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Annotations;
using NLog;
using TreeNode = System.Windows.Forms.TreeNode;
using TreeNodeCollection = System.Windows.Forms.TreeNodeCollection;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Vehicle.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", null)]
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public sealed class Vehicle : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasMatrixAttributes, IHasNotes, ICanSell, IHasCustomName, IHasPhysicalConditionMonitor, IHasLocation, IHasSource, ICanSort, IHasGear, IHasStolenProperty, ICanPaste, ICanBlackMarketDiscount, IDisposable, IAsyncDisposable
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private int _intHandling;
        private int _intOffroadHandling;
        private int _intAccel;
        private int _intOffroadAccel;
        private int _intSpeed;
        private int _intOffroadSpeed;
        private int _intPilot;
        private int _intBody;
        private int _intArmor;
        private int _intSensor;
        private int _intSeats;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strVehicleName = string.Empty;
        private int _intAddSlots;
        private int _intDroneModSlots;
        private int _intAddPowertrainModSlots;
        private int _intAddProtectionModSlots;
        private int _intAddWeaponModSlots;
        private int _intAddBodyModSlots;
        private int _intAddElectromagneticModSlots;
        private int _intAddCosmeticModSlots;
        private readonly TaggedObservableCollection<VehicleMod> _lstVehicleMods;
        private readonly TaggedObservableCollection<Gear> _lstGear;
        private readonly TaggedObservableCollection<Weapon> _lstWeapons;
        private readonly TaggedObservableCollection<WeaponMount> _lstWeaponMounts;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private Location _objLocation;
        private readonly TaggedObservableCollection<Location> _lstLocations;
        private bool _blnDiscountCost;
        private bool _blnDealerConnectionDiscount;
        private string _strParentID = string.Empty;

        private readonly Character _objCharacter;

        public Character CharacterObject => _objCharacter; // readonly member, no locking required

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
        private int _intSortOrder;
        private bool _blnStolen;
        private bool _blnSkipEvents;

        // Condition Monitor Progress.
        private int _intPhysicalCMFilled;

        private int _intMatrixCMFilled;
        private Guid _guiSourceID;

        #region Constructor, Create, Save, Load, and Print Methods

        public Vehicle(Character objCharacter)
        {
            // Create the GUID for the new Vehicle.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
            _lstGear = new TaggedObservableCollection<Gear>(objCharacter.LockObject);
            _lstGear.AddTaggedCollectionChanged(this, MatrixAttributeChildrenOnCollectionChanged);
            _lstWeapons = new TaggedObservableCollection<Weapon>(objCharacter.LockObject);
            _lstWeapons.AddTaggedCollectionChanged(this, MatrixAttributeChildrenOnCollectionChanged);
            _lstVehicleMods = new TaggedObservableCollection<VehicleMod>(objCharacter.LockObject);
            _lstVehicleMods.AddTaggedCollectionChanged(this, VehicleModsOnCollectionChanged);
            _lstWeaponMounts = new TaggedObservableCollection<WeaponMount>(objCharacter.LockObject);
            _lstLocations = new TaggedObservableCollection<Location>(objCharacter.LockObject);
        }

        private async Task VehicleModsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (_blnSkipEvents)
                return;
            if (e.Action != NotifyCollectionChangedAction.Move)
            {
                bool blnDoAI = false;
                bool blnDoSensors = false;
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    blnDoAI = true;
                    blnDoSensors = true;
                }
                else
                {
                    if (e.NewItems != null)
                    {
                        foreach (VehicleMod objMod in e.NewItems)
                        {
                            if (!objMod.Equipped || objMod.IncludedInVehicle)
                                continue;
                            blnDoAI = true;
                            if (objMod.Bonus?["sensor"] != null ||
                                (objMod.WirelessOn && objMod.WirelessBonus?["sensor"] != null))
                            {
                                blnDoSensors = true;
                                break;
                            }
                        }
                    }
                    // Don't need to check blnDoAI because it will always be true if blnDoSensors is true
                    if (!blnDoSensors && e.OldItems != null)
                    {
                        foreach (VehicleMod objMod in e.OldItems)
                        {
                            if (!objMod.Equipped || objMod.IncludedInVehicle)
                                continue;
                            blnDoAI = true;
                            if (objMod.Bonus?["sensor"] != null ||
                                (objMod.WirelessOn && objMod.WirelessBonus?["sensor"] != null))
                            {
                                blnDoSensors = true;
                                break;
                            }
                        }
                    }
                }

                if (blnDoSensors)
                {
                    Gear objGear = await GearChildren
                        .FirstOrDefaultAsync(
                            x => x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent,
                            token: token).ConfigureAwait(false);
                    if (objGear != null)
                        await objGear.SetRatingAsync(await GetCalculatedSensorAsync(token).ConfigureAwait(false), token)
                            .ConfigureAwait(false);
                }

                if (blnDoAI && await _objCharacter.GetIsAIAsync(token).ConfigureAwait(false)
                            && this == await _objCharacter.GetHomeNodeAsync(token).ConfigureAwait(false))
                {
                    await _objCharacter.OnPropertyChangedAsync(nameof(Character.PhysicalCM), token)
                        .ConfigureAwait(false);
                }
            }
        }

        private Task MatrixAttributeChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromCanceled(token);
            return e.Action != NotifyCollectionChangedAction.Move && !_blnSkipEvents
                ? this.RefreshMatrixAttributeArrayAsync(_objCharacter, token)
                : Task.CompletedTask;
        }

        /// <summary>
        /// Create a Vehicle from an XmlNode.
        /// </summary>
        /// <param name="objXmlVehicle">XmlNode of the Vehicle to create.</param>
        /// <param name="blnSkipSelectForms">Whether to skip forms that are created for bonuses.</param>
        /// <param name="blnCreateChildren">Whether child items should be created.</param>
        /// <param name="blnCreateImprovements">Whether bonuses should be created.</param>
        /// <param name="blnSkipCost">Whether creating the Vehicle should skip the Variable price dialogue (should only be used by SelectVehicle form).</param>
        /// <param name="blnForSelectForm">Whether this vehicle is being created for a Selection form (which means a lot of expensive code should be skipped).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public void Create(XmlNode objXmlVehicle, bool blnSkipCost = false, bool blnCreateChildren = true,
            bool blnCreateImprovements = true, bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            Utils.SafelyRunSynchronously(() => CreateCoreAsync(true, objXmlVehicle, blnSkipCost, blnCreateChildren,
                blnCreateImprovements, blnSkipSelectForms, blnForSelectForm, token), token);
        }

        /// <summary>
        /// Create a Vehicle from an XmlNode.
        /// </summary>
        /// <param name="objXmlVehicle">XmlNode of the Vehicle to create.</param>
        /// <param name="blnSkipSelectForms">Whether to skip forms that are created for bonuses.</param>
        /// <param name="blnCreateChildren">Whether child items should be created.</param>
        /// <param name="blnCreateImprovements">Whether bonuses should be created.</param>
        /// <param name="blnSkipCost">Whether creating the Vehicle should skip the Variable price dialogue (should only be used by SelectVehicle form).</param>
        /// <param name="blnForSelectForm">Whether this vehicle is being created for a Selection form (which means a lot of expensive code should be skipped).</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public Task CreateAsync(XmlNode objXmlVehicle, bool blnSkipCost = false, bool blnCreateChildren = true,
            bool blnCreateImprovements = true, bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            return CreateCoreAsync(false, objXmlVehicle, blnSkipCost, blnCreateChildren, blnCreateImprovements,
                blnSkipSelectForms, blnForSelectForm, token);
        }

        private async Task CreateCoreAsync(bool blnSync, XmlNode objXmlVehicle, bool blnSkipCost = false, bool blnCreateChildren = true,
            bool blnCreateImprovements = true, bool blnSkipSelectForms = false, bool blnForSelectForm = false, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!objXmlVehicle.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlVehicle });
                Utils.BreakIfDebug();
            }
            else
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            _blnSkipEvents = blnForSelectForm;
            objXmlVehicle.TryGetStringFieldQuickly("name", ref _strName);
            objXmlVehicle.TryGetStringFieldQuickly("category", ref _strCategory);
            string strTemp = objXmlVehicle["handling"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                //Some vehicles have different Offroad Handling speeds. If so, we want to split this up for use with mods and such later.
                if (strTemp.Contains('/'))
                {
                    string[] strHandlingArray = strTemp.Split('/');
                    if (!int.TryParse(strHandlingArray[0], out _intHandling))
                        _intHandling = 0;
                    if (!int.TryParse(strHandlingArray[1], out _intOffroadHandling))
                        _intOffroadHandling = 0;
                }
                else
                {
                    if (!int.TryParse(strTemp, out _intHandling))
                        _intHandling = 0;
                    _intOffroadHandling = _intHandling;
                }
            }
            strTemp = objXmlVehicle["accel"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.Contains('/'))
                {
                    string[] strAccelArray = strTemp.Split('/');
                    if (!int.TryParse(strAccelArray[0], out _intAccel))
                        _intAccel = 0;
                    if (!int.TryParse(strAccelArray[1], out _intOffroadAccel))
                        _intOffroadAccel = 0;
                }
                else
                {
                    if (!int.TryParse(strTemp, out _intAccel))
                        _intAccel = 0;
                    _intOffroadAccel = _intAccel;
                }
            }
            strTemp = objXmlVehicle["speed"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.Contains('/'))
                {
                    string[] strSpeedArray = strTemp.Split('/');
                    if (!int.TryParse(strSpeedArray[0], out _intSpeed))
                        _intSpeed = 0;
                    if (!int.TryParse(strSpeedArray[1], out _intOffroadSpeed))
                        _intOffroadSpeed = 0;
                }
                else
                {
                    if (!int.TryParse(strTemp, out _intSpeed))
                        _intSpeed = 0;
                    _intOffroadSpeed = _intSpeed;
                }
            }
            objXmlVehicle.TryGetInt32FieldQuickly("pilot", ref _intPilot);
            objXmlVehicle.TryGetInt32FieldQuickly("body", ref _intBody);
            objXmlVehicle.TryGetInt32FieldQuickly("armor", ref _intArmor);
            objXmlVehicle.TryGetInt32FieldQuickly("sensor", ref _intSensor);
            objXmlVehicle.TryGetInt32FieldQuickly("seats", ref _intSeats);
            if (!objXmlVehicle.TryGetInt32FieldQuickly("modslots", ref _intDroneModSlots))
                _intDroneModSlots = _intBody;
            objXmlVehicle.TryGetInt32FieldQuickly("powertrainmodslots", ref _intAddPowertrainModSlots);
            objXmlVehicle.TryGetInt32FieldQuickly("protectionmodslots", ref _intAddProtectionModSlots);
            objXmlVehicle.TryGetInt32FieldQuickly("weaponmodslots", ref _intAddWeaponModSlots);
            objXmlVehicle.TryGetInt32FieldQuickly("bodymodslots", ref _intAddBodyModSlots);
            objXmlVehicle.TryGetInt32FieldQuickly("electromagneticmodslots", ref _intAddElectromagneticModSlots);
            objXmlVehicle.TryGetInt32FieldQuickly("cosmeticmodslots", ref _intAddCosmeticModSlots);
            objXmlVehicle.TryGetStringFieldQuickly("avail", ref _strAvail);
            if (!objXmlVehicle.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlVehicle.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            if (!blnForSelectForm)
            {
                string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
                objXmlVehicle.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
                _colNotes = ColorTranslator.FromHtml(sNotesColor);
            }

            _strCost = objXmlVehicle["cost"]?.InnerText ?? string.Empty;
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
                    // Check for a Variable Cost.
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    if (intHyphenIndex != -1)
                    {
                        decMin = Convert.ToDecimal(strFirstHalf, GlobalSettings.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strSecondHalf, GlobalSettings.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strFirstHalf.FastEscape('+'), GlobalSettings.InvariantCultureInfo);

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
                    else
                        _strCost = strFirstHalf;
                }
                else
                    _strCost = strFirstHalf;
            }

            DealerConnectionDiscount = blnSync
                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                ? DoesDealerConnectionCurrentlyApply()
                : await DoesDealerConnectionCurrentlyApplyAsync(token).ConfigureAwait(false);

            objXmlVehicle.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlVehicle.TryGetStringFieldQuickly("page", ref _strPage);

            if (!blnForSelectForm && GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
            {
                if (blnSync)
                    // ReSharper disable once MethodHasAsyncOverload
                    Notes = CommonFunctions.GetBookNotes(objXmlVehicle, Name, CurrentDisplayName, Source,
                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                        Page, DisplayPage(GlobalSettings.Language), _objCharacter, token);
                else
                    Notes = await CommonFunctions.GetBookNotesAsync(objXmlVehicle, Name,
                        await GetCurrentDisplayNameAsync(token).ConfigureAwait(false), Source, Page,
                        await DisplayPageAsync(GlobalSettings.Language, token).ConfigureAwait(false), _objCharacter, token).ConfigureAwait(false);
            }

            objXmlVehicle.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objXmlVehicle.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlVehicle.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlVehicle.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlVehicle.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlVehicle.TryGetStringFieldQuickly("firewall", ref _strFirewall);
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
            objXmlVehicle.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlVehicle.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlVehicle.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlVehicle.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlVehicle.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlVehicle.TryGetStringFieldQuickly("programs", ref _strProgramLimit);

            if (blnCreateChildren)
            {
                // If there are any VehicleMods that come with the Vehicle, add them.
                XmlElement xmlMods = objXmlVehicle["mods"];
                if (xmlMods != null)
                {
                    XmlDocument objXmlDocument = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData("vehicles.xml", token: token)
                        : await _objCharacter.LoadDataAsync("vehicles.xml", token: token).ConfigureAwait(false);

                    using (XmlNodeList objXmlModList = xmlMods.SelectNodes("name"))
                    {
                        if (objXmlModList != null)
                        {
                            foreach (XmlNode objXmlVehicleMod in objXmlModList)
                            {
                                XmlNode objXmlMod = objXmlDocument.TryGetNodeByNameOrId("/chummer/mods/mod", objXmlVehicleMod.InnerText);
                                if (objXmlMod != null)
                                {
                                    VehicleMod objMod = new VehicleMod(_objCharacter)
                                    {
                                        IncludedInVehicle = true
                                    };
                                    string strForcedValue = objXmlVehicleMod.Attributes?["select"]?.InnerText ?? string.Empty;
                                    if (!int.TryParse(objXmlVehicleMod.Attributes?["rating"]?.InnerText, out int intRating))
                                        intRating = 0;

                                    objMod.Extra = strForcedValue;
                                    if (blnSync)
                                    {
                                        // ReSharper disable once MethodHasAsyncOverload
                                        objMod.Create(objXmlMod, intRating, this, 0, strForcedValue,
                                            blnSkipSelectForms, token);
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        _lstVehicleMods.Add(objMod);
                                    }
                                    else
                                    {
                                        await objMod.CreateAsync(objXmlMod, intRating, this, 0, strForcedValue,
                                            blnSkipSelectForms, token).ConfigureAwait(false);
                                        await _lstVehicleMods.AddAsync(objMod, token).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }

                    using (XmlNodeList objXmlModList = xmlMods.SelectNodes("mod"))
                    {
                        if (objXmlModList != null)
                        {
                            foreach (XmlNode objXmlVehicleMod in objXmlModList)
                            {
                                string strName = objXmlVehicleMod["name"]?.InnerText;
                                if (string.IsNullOrEmpty(strName))
                                    continue;
                                XmlNode objXmlMod = objXmlDocument.TryGetNodeByNameOrId("/chummer/mods/mod", strName);
                                if (objXmlMod != null)
                                {
                                    VehicleMod objMod = new VehicleMod(_objCharacter)
                                    {
                                        IncludedInVehicle = true
                                    };
                                    string strForcedValue = objXmlVehicleMod.SelectSingleNodeAndCacheExpressionAsNavigator("name/@select", token)?.Value ?? string.Empty;
                                    if (!int.TryParse(objXmlVehicleMod["rating"]?.InnerText, out int intRating))
                                        intRating = 0;

                                    objMod.Extra = strForcedValue;
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverload
                                        objMod.Create(objXmlMod, intRating, this, 0, strForcedValue, blnSkipSelectForms, token);
                                    else
                                        await objMod.CreateAsync(objXmlMod, intRating, this, 0, strForcedValue, blnSkipSelectForms, token).ConfigureAwait(false);

                                    XmlElement xmlSubsystemsNode = objXmlVehicleMod["subsystems"];
                                    if (xmlSubsystemsNode != null)
                                    {
                                        // Load Cyberware subsystems first
                                        using (XmlNodeList objXmlSubSystemNameList = xmlSubsystemsNode.SelectNodes("cyberware"))
                                        {
                                            if (objXmlSubSystemNameList?.Count > 0)
                                            {
                                                XmlDocument objXmlWareDocument = blnSync
                                                    // ReSharper disable once MethodHasAsyncOverload
                                                    ? _objCharacter.LoadData("cyberware.xml", token: token)
                                                    : await _objCharacter.LoadDataAsync("cyberware.xml", token: token).ConfigureAwait(false);
                                                foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                                                {
                                                    string strSubsystemName = objXmlSubsystemNode["name"]?.InnerText;
                                                    if (string.IsNullOrEmpty(strSubsystemName))
                                                        continue;
                                                    XmlNode objXmlSubsystem
                                                        = objXmlWareDocument.TryGetNodeByNameOrId(
                                                            "/chummer/cyberwares/cyberware", strSubsystemName);
                                                    if (objXmlSubsystem == null)
                                                        continue;
                                                    Cyberware objSubsystem = new Cyberware(_objCharacter);
                                                    int.TryParse(objXmlSubsystemNode["rating"]?.InnerText, NumberStyles.Any,
                                                        GlobalSettings.InvariantCultureInfo, out int intSubSystemRating);
                                                    if (blnSync)
                                                        // ReSharper disable once MethodHasAsyncOverload
                                                        objSubsystem.Create(objXmlSubsystem,
                                                            new Grade(_objCharacter,
                                                                Improvement.ImprovementSource.Cyberware),
                                                            Improvement.ImprovementSource.Cyberware,
                                                            intSubSystemRating, _lstWeapons, _objCharacter.Vehicles,
                                                            false, true,
                                                            objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty,
                                                            token: token);
                                                    else
                                                        await objSubsystem.CreateAsync(objXmlSubsystem,
                                                            new Grade(_objCharacter,
                                                                Improvement.ImprovementSource.Cyberware),
                                                            Improvement.ImprovementSource.Cyberware,
                                                            intSubSystemRating, _lstWeapons, _objCharacter.Vehicles,
                                                            false, true,
                                                            objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty,
                                                            token: token).ConfigureAwait(false);
                                                    objSubsystem.ParentID = InternalId;
                                                    objSubsystem.Cost = "0";
                                                    if (blnSync)
                                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                                        objMod.Cyberware.Add(objSubsystem);
                                                    else
                                                        await objMod.Cyberware.AddAsync(objSubsystem, token).ConfigureAwait(false);
                                                }
                                            }
                                        }
                                    }
                                    if (blnSync)
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        _lstVehicleMods.Add(objMod);
                                    else
                                        await _lstVehicleMods.AddAsync(objMod, token).ConfigureAwait(false);
                                }
                            }
                        }
                    }

                    XPathNavigator objAddSlotsNode = objXmlVehicle.SelectSingleNodeAndCacheExpressionAsNavigator("mods/addslots", token);
                    if (objAddSlotsNode != null && !int.TryParse(objAddSlotsNode.Value, out _intAddSlots))
                        _intAddSlots = 0;
                }

                // If there are any Weapon Mounts that come with the Vehicle, add them.
                XmlElement xmlWeaponMounts = objXmlVehicle["weaponmounts"];
                if (xmlWeaponMounts != null)
                {
                    foreach (XmlNode objXmlVehicleMod in xmlWeaponMounts.SelectNodes("weaponmount"))
                    {
                        WeaponMount objWeaponMount = new WeaponMount(_objCharacter, this);
                        if (blnSync)
                        {
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            objWeaponMount.CreateByName(objXmlVehicleMod);
                            objWeaponMount.IncludedInVehicle = true;
                            // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                            WeaponMounts.Add(objWeaponMount);
                        }
                        else
                        {
                            await objWeaponMount.CreateByNameAsync(objXmlVehicleMod, token: token).ConfigureAwait(false);
                            objWeaponMount.IncludedInVehicle = true;
                            await WeaponMounts.AddAsync(objWeaponMount, token).ConfigureAwait(false);
                        }
                    }
                }

                // If there is any Gear that comes with the Vehicle, add them.
                XmlElement xmlGears = objXmlVehicle["gears"];
                if (xmlGears != null)
                {
                    XmlDocument objXmlDocument = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData("gear.xml", token: token)
                        : await _objCharacter.LoadDataAsync("gear.xml", token: token).ConfigureAwait(false);

                    using (XmlNodeList objXmlGearList = xmlGears.SelectNodes("gear"))
                    {
                        if (objXmlGearList?.Count > 0)
                        {
                            List<Weapon> lstWeapons = new List<Weapon>(1);

                            foreach (XmlNode objXmlVehicleGear in objXmlGearList)
                            {
                                Gear objGear = new Gear(_objCharacter);
                                if (blnSync
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        ? objGear.CreateFromNode(objXmlDocument, objXmlVehicleGear, lstWeapons,
                                            blnCreateImprovements, blnSkipSelectForms)
                                        : await objGear.CreateFromNodeAsync(objXmlDocument, objXmlVehicleGear,
                                            lstWeapons, blnCreateImprovements, blnSkipSelectForms, token).ConfigureAwait(false))
                                {
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

                            foreach (Weapon objWeapon in lstWeapons)
                            {
                                if (blnSync)
                                {
                                    objWeapon.ParentVehicle = this;
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    Weapons.Add(objWeapon);
                                }
                                else
                                {
                                    await objWeapon.SetParentVehicleAsync(this, token).ConfigureAwait(false);
                                    await Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }

                // If there are any Weapons that come with the Vehicle, add them.
                XmlElement xmlWeapons = objXmlVehicle["weapons"];
                if (xmlWeapons != null)
                {
                    XmlDocument objXmlWeaponDocument = blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? _objCharacter.LoadData("weapons.xml", token: token)
                        : await _objCharacter.LoadDataAsync("weapons.xml", token: token).ConfigureAwait(false);

                    foreach (XmlNode objXmlWeapon in xmlWeapons.SelectNodes("weapon"))
                    {
                        string strWeaponName = objXmlWeapon["name"]?.InnerText;
                        if (string.IsNullOrEmpty(strWeaponName))
                            continue;
                        bool blnAttached = false;
                        Weapon objWeapon = new Weapon(_objCharacter);

                        List<Weapon> lstSubWeapons = new List<Weapon>(1);
                        XmlNode objXmlWeaponNode = objXmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", strWeaponName);
                        if (blnSync)
                        {
                            objWeapon.ParentVehicle = this;
                            // ReSharper disable once MethodHasAsyncOverload
                            objWeapon.Create(objXmlWeaponNode, lstSubWeapons, blnCreateChildren,
                                !blnSkipSelectForms && blnCreateImprovements, blnSkipCost, blnForSelectForm: blnForSelectForm, token: token);
                        }
                        else
                        {
                            await objWeapon.SetParentVehicleAsync(this, token).ConfigureAwait(false);
                            await objWeapon.CreateAsync(objXmlWeaponNode, lstSubWeapons, blnCreateChildren,
                                !blnSkipSelectForms && blnCreateImprovements, blnSkipCost, blnForSelectForm: blnForSelectForm, token: token).ConfigureAwait(false);
                        }

                        objWeapon.ParentID = InternalId;
                        objWeapon.Cost = "0";

                        // Find the first free Weapon Mount in the Vehicle.
                        WeaponMount objWeaponMount = blnSync
                            ? _lstWeaponMounts.FirstOrDefault(x => !x.IsWeaponsFull &&
                                                                   (x.AllowedWeaponCategories.Contains(
                                                                        objWeapon.SizeCategory) ||
                                                                    x.AllowedWeapons.Contains(objWeapon.Name) ||
                                                                    string.IsNullOrEmpty(x.AllowedWeaponCategories)))
                            : await _lstWeaponMounts.FirstOrDefaultAsync(x => !x.IsWeaponsFull &&
                                                                              (x.AllowedWeaponCategories.Contains(
                                                                                   objWeapon.SizeCategory) ||
                                                                               x.AllowedWeapons
                                                                                   .Contains(objWeapon.Name) ||
                                                                               string.IsNullOrEmpty(
                                                                                   x.AllowedWeaponCategories)), token).ConfigureAwait(false);
                        if (objWeaponMount != null)
                        {
                            if (blnSync)
                                // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                objWeaponMount.Weapons.Add(objWeapon);
                            else
                                await objWeaponMount.Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);
                        }
                        else
                        {
                            VehicleMod objMod = (blnSync
                                                    ? _lstVehicleMods.FirstOrDefault(x =>
                                                        x.Name.Contains("Weapon Mount") ||
                                                        !string.IsNullOrEmpty(x.WeaponMountCategories) &&
                                                        x.WeaponMountCategories.Contains(objWeapon.SizeCategory) &&
                                                        x.Weapons.Count == 0)
                                                    : await _lstVehicleMods.FirstOrDefaultAsync(async x =>
                                                        x.Name.Contains("Weapon Mount") ||
                                                        !string.IsNullOrEmpty(x.WeaponMountCategories) &&
                                                        x.WeaponMountCategories.Contains(objWeapon.SizeCategory) &&
                                                        await x.Weapons.GetCountAsync(token).ConfigureAwait(false) == 0, token).ConfigureAwait(false)) ??
                                                (blnSync
                                                    ? _lstVehicleMods.FirstOrDefault(x =>
                                                        x.Name.Contains("Weapon Mount") ||
                                                        !string.IsNullOrEmpty(x.WeaponMountCategories) &&
                                                        x.WeaponMountCategories.Contains(objWeapon.SizeCategory))
                                                    : await _lstVehicleMods.FirstOrDefaultAsync(x =>
                                                            x.Name.Contains("Weapon Mount") ||
                                                            !string.IsNullOrEmpty(x.WeaponMountCategories) &&
                                                            x.WeaponMountCategories.Contains(objWeapon.SizeCategory),
                                                        token).ConfigureAwait(false));
                            if (objMod != null)
                            {
                                if (blnSync)
                                {
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objMod.Weapons.Add(objWeapon);
                                    foreach (Weapon objSubWeapon in lstSubWeapons)
                                        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                        objMod.Weapons.Add(objSubWeapon);
                                }
                                else
                                {
                                    await objMod.Weapons.AddAsync(objWeapon, token).ConfigureAwait(false);
                                    foreach (Weapon objSubWeapon in lstSubWeapons)
                                        await objMod.Weapons.AddAsync(objSubWeapon, token).ConfigureAwait(false);
                                }
                            }
                        }

                        // Look for Weapon Accessories.
                        XmlElement xmlAccessories = objXmlWeapon["accessories"];
                        if (xmlAccessories != null)
                        {
                            foreach (XmlNode objXmlAccessory in xmlAccessories.SelectNodes("accessory"))
                            {
                                string strAccessoryName = objXmlWeapon["name"]?.InnerText;
                                if (string.IsNullOrEmpty(strAccessoryName))
                                    continue;
                                XmlNode objXmlAccessoryNode = objXmlWeaponDocument.TryGetNodeByNameOrId("/chummer/accessories/accessory", strAccessoryName);
                                WeaponAccessory objMod = new WeaponAccessory(_objCharacter);
                                string strMount = "Internal";
                                objXmlAccessory.TryGetStringFieldQuickly("mount", ref strMount);
                                string strExtraMount = "None";
                                objXmlAccessory.TryGetStringFieldQuickly("extramount", ref strExtraMount);
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverload
                                    objMod.Create(objXmlAccessoryNode,
                                        new Tuple<string, string>(strMount, strExtraMount), 0, blnSkipCost,
                                        blnCreateChildren, !blnSkipSelectForms && blnCreateImprovements, token);
                                else
                                    await objMod.CreateAsync(objXmlAccessoryNode,
                                        new Tuple<string, string>(strMount, strExtraMount), 0, blnSkipCost,
                                        blnCreateChildren, !blnSkipSelectForms && blnCreateImprovements, token).ConfigureAwait(false);
                                objMod.Cost = "0";
                                if (blnSync)
                                    // ReSharper disable once MethodHasAsyncOverloadWithCancellation
                                    objWeapon.WeaponAccessories.Add(objMod);
                                else
                                    await objWeapon.WeaponAccessories.AddAsync(objMod, token).ConfigureAwait(false);
                            }
                        }
                    }
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
            objWriter.WriteStartElement("vehicle");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("handling", _intHandling.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("offroadhandling", _intOffroadHandling.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("accel", _intAccel.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("offroadaccel", _intOffroadAccel.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("speed", _intSpeed.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("offroadspeed", _intOffroadSpeed.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("pilot", _intPilot.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("body", _intBody.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("seats", _intSeats.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("armor", _intArmor.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("sensor", _intSensor.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("addslots", _intAddSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("modslots", _intDroneModSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("powertrainmodslots", _intAddPowertrainModSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("protectionmodslots", _intAddProtectionModSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("weaponmodslots", _intAddWeaponModSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("bodymodslots", _intAddBodyModSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("electromagneticmodslots", _intAddElectromagneticModSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("cosmeticmodslots", _intAddCosmeticModSlots.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("vehiclename", _strVehicleName);
            objWriter.WriteStartElement("mods");
            _lstVehicleMods.ForEach(x => x.Save(objWriter));
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weaponmounts");
            _lstWeaponMounts.ForEach(x => x.Save(objWriter));
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("gears");
            _lstGear.ForEach(x => x.Save(objWriter));
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weapons");
            _lstWeapons.ForEach(x => x.Save(objWriter));
            objWriter.WriteEndElement();
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("dealerconnection", _blnDealerConnectionDiscount.ToString(GlobalSettings.InvariantCultureInfo));
            if (_lstLocations.Count > 0)
            {
                // <locations>
                objWriter.WriteStartElement("locations");
                _lstLocations.ForEach(x => x.Save(objWriter));
                // </locations>
                objWriter.WriteEndElement();
            }
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
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Vehicle from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether we are copying an existing vehicle.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (objNode == null)
                return;
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
            bool blnIsActive = false;
            if (objNode.TryGetBoolFieldQuickly("active", ref blnIsActive) && blnIsActive)
                this.SetActiveCommlink(_objCharacter, true);

            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            string strTemp = objNode["handling"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                //Some vehicles have different Offroad Handling speeds. If so, we want to split this up for use with mods and such later.
                if (strTemp.Contains('/'))
                {
                    string[] lstHandlings = strTemp.Split('/');
                    int.TryParse(lstHandlings[0], NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intHandling);
                    int.TryParse(lstHandlings[1], NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intOffroadHandling);
                }
                else
                {
                    int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intHandling);
                    strTemp = objNode["offroadhandling"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intOffroadHandling);
                    }
                }
            }
            strTemp = objNode["accel"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.Contains('/'))
                {
                    string[] lstAccels = strTemp.Split('/');
                    int.TryParse(lstAccels[0], NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intAccel);
                    int.TryParse(lstAccels[1], NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intOffroadAccel);
                }
                else
                {
                    int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intAccel);
                    strTemp = objNode["offroadaccel"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intOffroadAccel);
                    }
                }
            }
            strTemp = objNode["speed"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.Contains('/'))
                {
                    string[] lstSpeeds = strTemp.Split('/');
                    int.TryParse(lstSpeeds[0], NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intSpeed);
                    int.TryParse(lstSpeeds[1], NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intOffroadSpeed);
                }
                else
                {
                    int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intSpeed);
                    strTemp = objNode["offroadspeed"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        int.TryParse(strTemp, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out _intOffroadSpeed);
                    }
                }
            }
            objNode.TryGetInt32FieldQuickly("seats", ref _intSeats);
            objNode.TryGetInt32FieldQuickly("pilot", ref _intPilot);
            objNode.TryGetInt32FieldQuickly("body", ref _intBody);
            objNode.TryGetInt32FieldQuickly("armor", ref _intArmor);
            objNode.TryGetInt32FieldQuickly("sensor", ref _intSensor);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetInt32FieldQuickly("addslots", ref _intAddSlots);
            objNode.TryGetInt32FieldQuickly("modslots", ref _intDroneModSlots);
            objNode.TryGetInt32FieldQuickly("powertrainmodslots", ref _intAddPowertrainModSlots);
            objNode.TryGetInt32FieldQuickly("protectionmodslots", ref _intAddProtectionModSlots);
            objNode.TryGetInt32FieldQuickly("weaponmodslots", ref _intAddWeaponModSlots);
            objNode.TryGetInt32FieldQuickly("bodymodslots", ref _intAddBodyModSlots);
            objNode.TryGetInt32FieldQuickly("electromagneticmodslots", ref _intAddElectromagneticModSlots);
            objNode.TryGetInt32FieldQuickly("cosmeticmodslots", ref _intAddCosmeticModSlots);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("parentid", ref _strParentID);
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetInt32FieldQuickly("physicalcmfilled", ref _intPhysicalCMFilled);
            objNode.TryGetStringFieldQuickly("vehiclename", ref _strVehicleName);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            string strNodeInnerXml = objNode.InnerXml;

            // Load gear first so that ammo stuff for weapons get loaded in properly
            if (strNodeInnerXml.Contains("<gears>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Gear objGear = new Gear(_objCharacter);
                    objGear.Load(nodChild, blnCopy);
                    _lstGear.Add(objGear);
                    objGear.Parent = this;
                }
            }

            if (strNodeInnerXml.Contains("<mods>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("mods/mod");
                foreach (XmlNode nodChild in nodChildren)
                {
                    VehicleMod objMod = new VehicleMod(_objCharacter)
                    {
                        Parent = this
                    };
                    objMod.Load(nodChild, blnCopy);
                    _lstVehicleMods.Add(objMod);
                }
            }

            if (strNodeInnerXml.Contains("<weaponmounts>"))
            {
                bool blnKrakePassDone = false;
                XmlNodeList nodChildren = objNode.SelectNodes("weaponmounts/weaponmount");
                foreach (XmlNode nodChild in nodChildren)
                {
                    WeaponMount wm = new WeaponMount(_objCharacter, this);
                    if (wm.Load(nodChild, blnCopy))
                        WeaponMounts.Add(wm);
                    else
                    {
                        // Compatibility sweep for malformed weapon mount on Proteus Krake
                        Guid guidDummy = Guid.Empty;
                        if (Name.StartsWith("Proteus Krake", StringComparison.Ordinal)
                            && !blnKrakePassDone
                            && _objCharacter.LastSavedVersion < new Version(5, 213, 28)
                            && (!nodChild.TryGetGuidFieldQuickly("sourceid", ref guidDummy) || guidDummy == Guid.Empty))
                        {
                            blnKrakePassDone = true;
                            // If there are any Weapon Mounts that come with the Vehicle, add them.
                            XmlNode xmlVehicleDataNode = objMyNode.Value;
                            if (xmlVehicleDataNode != null)
                            {
                                XmlElement xmlDataNodesForMissingKrakeStuff = xmlVehicleDataNode["weaponmounts"];
                                if (xmlDataNodesForMissingKrakeStuff != null)
                                {
                                    foreach (XmlNode objXmlVehicleMod in xmlDataNodesForMissingKrakeStuff.SelectNodes("weaponmount"))
                                    {
                                        WeaponMount objWeaponMount = new WeaponMount(_objCharacter, this);
                                        objWeaponMount.CreateByName(objXmlVehicleMod);
                                        objWeaponMount.IncludedInVehicle = true;
                                        WeaponMounts.Add(objWeaponMount);
                                    }
                                }

                                xmlDataNodesForMissingKrakeStuff = xmlVehicleDataNode["weapons"];
                                if (xmlDataNodesForMissingKrakeStuff != null)
                                {
                                    XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                                    foreach (XmlNode objXmlWeapon in xmlDataNodesForMissingKrakeStuff.SelectNodes("weapon"))
                                    {
                                        string strWeaponName = objXmlWeapon["name"]?.InnerText;
                                        if (string.IsNullOrEmpty(strWeaponName))
                                            continue;
                                        bool blnAttached = false;
                                        Weapon objWeapon = new Weapon(_objCharacter);

                                        List<Weapon> lstSubWeapons = new List<Weapon>(1);
                                        XmlNode objXmlWeaponNode = objXmlWeaponDocument.TryGetNodeByNameOrId("/chummer/weapons/weapon", strWeaponName);
                                        objWeapon.ParentVehicle = this;
                                        objWeapon.Create(objXmlWeaponNode, lstSubWeapons);
                                        objWeapon.ParentID = InternalId;
                                        objWeapon.Cost = "0";

                                        // Find the first free Weapon Mount in the Vehicle.
                                        WeaponMounts.ForEachWithBreak(objWeaponMount =>
                                        {
                                            if (objWeaponMount.IsWeaponsFull)
                                                return true;
                                            if (!objWeaponMount.AllowedWeaponCategories
                                                    .Contains(objWeapon.SizeCategory) &&
                                                !objWeaponMount.AllowedWeapons.Contains(objWeapon.Name) &&
                                                !string.IsNullOrEmpty(objWeaponMount.AllowedWeaponCategories))
                                                return true;
                                            objWeaponMount.Weapons.Add(objWeapon);
                                            blnAttached = true;
                                            foreach (Weapon objSubWeapon in lstSubWeapons)
                                                objWeaponMount.Weapons.Add(objSubWeapon);
                                            return false;
                                        });

                                        // If a free Weapon Mount could not be found, just attach it to the first one found and let the player deal with it.
                                        if (!blnAttached)
                                        {
                                            _lstVehicleMods.ForEachWithBreak(objMod =>
                                            {
                                                if (objMod.Name.Contains("Weapon Mount") ||
                                                    !string.IsNullOrEmpty(objMod.WeaponMountCategories) &&
                                                    objMod.WeaponMountCategories.Contains(objWeapon.SizeCategory) &&
                                                    objMod.Weapons.Count == 0)
                                                {
                                                    objMod.Weapons.Add(objWeapon);
                                                    foreach (Weapon objSubWeapon in lstSubWeapons)
                                                        objMod.Weapons.Add(objSubWeapon);
                                                    return false;
                                                }

                                                return true;
                                            });
                                            if (!blnAttached)
                                            {
                                                _lstVehicleMods.ForEachWithBreak(objMod =>
                                                {
                                                    if (objMod.Name.Contains("Weapon Mount") ||
                                                        !string.IsNullOrEmpty(objMod.WeaponMountCategories) &&
                                                        objMod.WeaponMountCategories.Contains(objWeapon.SizeCategory))
                                                    {
                                                        objMod.Weapons.Add(objWeapon);
                                                        foreach (Weapon objSubWeapon in lstSubWeapons)
                                                            objMod.Weapons.Add(objSubWeapon);
                                                        return false;
                                                    }

                                                    return true;
                                                });
                                            }
                                        }

                                        // Look for Weapon Accessories.
                                        XmlElement xmlAccessories = objXmlWeapon["accessories"];
                                        if (xmlAccessories != null)
                                        {
                                            foreach (XmlNode objXmlAccessory in xmlAccessories.SelectNodes("accessory"))
                                            {
                                                string strAccessoryName = objXmlWeapon["name"]?.InnerText;
                                                if (string.IsNullOrEmpty(strAccessoryName))
                                                    continue;
                                                XmlNode objXmlAccessoryNode = objXmlWeaponDocument.TryGetNodeByNameOrId("/chummer/accessories/accessory", strAccessoryName);
                                                WeaponAccessory objMod = new WeaponAccessory(_objCharacter);
                                                string strMount = "Internal";
                                                objXmlAccessory.TryGetStringFieldQuickly("mount", ref strMount);
                                                string strExtraMount = "None";
                                                objXmlAccessory.TryGetStringFieldQuickly("extramount", ref strExtraMount);
                                                objMod.Create(objXmlAccessoryNode, new Tuple<string, string>(strMount, strExtraMount), 0);
                                                objMod.Cost = "0";
                                                objWeapon.WeaponAccessories.Add(objMod);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (strNodeInnerXml.Contains("<weapons>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("weapons/weapon");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Weapon objWeapon = new Weapon(_objCharacter)
                    {
                        ParentVehicle = this
                    };
                    objWeapon.Load(nodChild, blnCopy);
                    _lstWeapons.Add(objWeapon);
                }
            }

            string strLocation = objNode["location"]?.InnerText;
            if (!string.IsNullOrEmpty(strLocation))
            {
                if (Guid.TryParse(strLocation, out Guid temp))
                {
                    // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                    _objLocation =
                        _objCharacter.VehicleLocations.FirstOrDefault(location =>
                            location.InternalId == temp.ToString());
                }
                else
                {
                    //Legacy. Location is a string.
                    _objLocation =
                        _objCharacter.VehicleLocations.FirstOrDefault(location =>
                            location.Name == strLocation);
                }
                _objLocation?.Children.Add(this);
            }
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            if (!objNode.TryGetBoolFieldQuickly("dealerconnection", ref _blnDealerConnectionDiscount))
            {
                _blnDealerConnectionDiscount = DoesDealerConnectionCurrentlyApply();
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

            if (objNode["locations"] != null)
            {
                // Locations.
                foreach (XmlNode objXmlLocation in objNode.SelectNodes("locations/location"))
                {
                    Location objLocation = new Location(_objCharacter, _lstLocations);
                    objLocation.Load(objXmlLocation);
                }
            }
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
            await objWriter.WriteStartElementAsync("vehicle", token: token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync(
                      "name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                  .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync(
                      "fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                  .ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync(
                      "category", await DisplayCategoryAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                  .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("category_english", Category, token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("isdrone", IsDrone.ToString(GlobalSettings.InvariantCultureInfo), token)
                  .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("handling", await GetTotalHandlingAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("accel", await GetTotalAccelAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("speed", await GetTotalSpeedAsync(token).ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("pilot", (await GetPilotAsync(token).ConfigureAwait(false)).ToString(objCulture), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("body", (await GetTotalBodyAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                           .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("armor", (await GetTotalArmorAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                           .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("seats", (await GetTotalSeatsAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                           .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("sensor", (await GetCalculatedSensorAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                           .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("avail", await TotalAvailAsync(objCulture, strLanguageToPrint, token).ConfigureAwait(false), token)
                           .ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("cost", (await GetTotalCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat, objCulture),
                                           token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("owncost", (await GetOwnCostAsync(token).ConfigureAwait(false)).ToString(_objCharacter.Settings.NuyenFormat, objCulture),
                                           token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync(
                      "source",
                      await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token)
                                         .ConfigureAwait(false), token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token)
                           .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("physicalcm", PhysicalCM.ToString(objCulture), token)
                           .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("physicalcmfilled", PhysicalCMFilled.ToString(objCulture), token)
                           .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("vehiclename", CustomName, token).ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("maneuver", (await GetManeuverAsync(token).ConfigureAwait(false)).ToString(objCulture), token)
                           .ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync(
                      "location",
                      Location != null
                          ? await Location.DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false)
                          : string.Empty, token).ConfigureAwait(false);

            await objWriter
                  .WriteElementStringAsync("attack", (await this.GetTotalMatrixAttributeAsync("Attack", token).ConfigureAwait(false)).ToString(objCulture), token)
                  .ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("sleaze", (await this.GetTotalMatrixAttributeAsync("Sleaze", token).ConfigureAwait(false)).ToString(objCulture), token)
                  .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("dataprocessing",
                                                    (await this.GetTotalMatrixAttributeAsync("Data Processing", token).ConfigureAwait(false))
                                                        .ToString(objCulture), token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("firewall", (await this.GetTotalMatrixAttributeAsync("Firewall", token).ConfigureAwait(false)).ToString(objCulture),
                                           token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("devicerating",
                                           (await this.GetTotalMatrixAttributeAsync("Device Rating", token).ConfigureAwait(false)).ToString(objCulture), token)
                  .ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("programlimit",
                                           (await this.GetTotalMatrixAttributeAsync("Program Limit", token).ConfigureAwait(false)).ToString(objCulture), token)
                  .ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("iscommlink", (await GetIsCommlinkAsync(token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                                           token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo), token)
                  .ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync(
                      "active", (await this.IsActiveCommlinkAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo),
                      token).ConfigureAwait(false);
            await objWriter
                  .WriteElementStringAsync(
                      "homenode", (await this.IsHomeNodeAsync(_objCharacter, token).ConfigureAwait(false)).ToString(GlobalSettings.InvariantCultureInfo), token)
                  .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("matrixcm", MatrixCM.ToString(objCulture), token)
                           .ConfigureAwait(false);
            await objWriter.WriteElementStringAsync("matrixcmfilled", MatrixCMFilled.ToString(objCulture), token)
                           .ConfigureAwait(false);

            await objWriter.WriteStartElementAsync("mods", token: token).ConfigureAwait(false);
            await Mods.ForEachAsync(x => x.Print(objWriter, objCulture, strLanguageToPrint, token), token).ConfigureAwait(false);
            await WeaponMounts.ForEachAsync(x => x.Print(objWriter, objCulture, strLanguageToPrint, token), token).ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            await objWriter.WriteStartElementAsync("gears", token: token).ConfigureAwait(false);
            await GearChildren.ForEachAsync(x => x.Print(objWriter, objCulture, strLanguageToPrint, token), token).ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            await objWriter.WriteStartElementAsync("weapons", token: token).ConfigureAwait(false);
            await Weapons.ForEachAsync(x => x.Print(objWriter, objCulture, strLanguageToPrint, token), token).ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
            if (GlobalSettings.PrintNotes)
                await objWriter.WriteElementStringAsync("notes", Notes, token).ConfigureAwait(false);
            await objWriter.WriteEndElementAsync().ConfigureAwait(false);
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
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
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("vehicles.xml", strLanguage).SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public async Task<string> DisplayCategoryAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return (await _objCharacter.LoadDataXPathAsync("vehicles.xml", strLanguage, token: token).ConfigureAwait(false))
                .SelectSingleNodeAndCacheExpression("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate", token: token)
                   ?.Value ?? Category;
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
        /// Is this vehicle a drone?
        /// </summary>
        public bool IsDrone => Category.Contains("Drone");

        /// <summary>
        /// Handling.
        /// </summary>
        public int Handling
        {
            get => _intHandling;
            set => _intHandling = value;
        }

        /// <summary>
        /// Seats.
        /// </summary>
        public int Seats
        {
            get => _intSeats;
            set => _intSeats = value;
        }

        /// <summary>
        /// Offroad Handling.
        /// </summary>
        public int OffroadHandling
        {
            get => _intOffroadHandling;
            set => _intOffroadHandling = value;
        }

        /// <summary>
        /// Acceleration.
        /// </summary>
        public int Accel
        {
            get => _intAccel;
            set => _intAccel = value;
        }

        /// <summary>
        /// Offroad Acceleration.
        /// </summary>
        public int OffroadAccel
        {
            get => _intOffroadAccel;
            set => _intOffroadAccel = value;
        }

        /// <summary>
        /// Speed.
        /// </summary>
        public int Speed
        {
            get => _intSpeed;
            set => _intSpeed = value;
        }

        /// <summary>
        /// Speed.
        /// </summary>
        public int OffroadSpeed
        {
            get => _intOffroadSpeed;
            set => _intOffroadSpeed = value;
        }

        /// <summary>
        /// Pilot.
        /// </summary>
        public int Pilot
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    int intReturn = _intPilot;
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (!objMod.IncludedInVehicle && objMod.Equipped)
                        {
                            string strBonusPilot = objMod.WirelessOn
                                ? objMod.WirelessBonus?["pilot"]?.InnerText ?? objMod.Bonus?["pilot"]?.InnerText
                                : objMod.Bonus?["pilot"]?.InnerText;
                            intReturn = Math.Max(ParseBonus(strBonusPilot, objMod.Rating, _intPilot, "Pilot", false),
                                intReturn);
                        }
                    }

                    return intReturn;
                }
            }
            set => _intPilot = value;
        }

        /// <summary>
        /// Pilot.
        /// </summary>
        public async Task<int> GetPilotAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intReturn = _intPilot;
                await Mods.ForEachAsync(async objMod =>
                {
                    if (!objMod.IncludedInVehicle && objMod.Equipped)
                    {
                        string strBonusPilot = objMod.WirelessOn
                            ? objMod.WirelessBonus?["pilot"]?.InnerText ?? objMod.Bonus?["pilot"]?.InnerText
                            : objMod.Bonus?["pilot"]?.InnerText;
                        intReturn = Math.Max(
                            await ParseBonusAsync(strBonusPilot, await objMod.GetRatingAsync(token).ConfigureAwait(false), _intPilot, "Pilot", false, token)
                                .ConfigureAwait(false),
                            intReturn);
                    }
                }, token).ConfigureAwait(false);
                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Body.
        /// </summary>
        public int Body
        {
            get => _intBody;
            set => _intBody = value;
        }

        /// <summary>
        /// Armor.
        /// </summary>
        public int Armor
        {
            get => _intArmor;
            set => _intArmor = value;
        }

        /// <summary>
        /// Sensor.
        /// </summary>
        public int BaseSensor
        {
            get => _intSensor;
            set => _intSensor = value;
        }

        /// <summary>
        /// Base Matrix Boxes.
        /// </summary>
        public int BaseMatrixBoxes => 8;

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM => BaseMatrixBoxes + (this.GetTotalMatrixAttribute("Device Rating") + 1) / 2 + TotalBonusMatrixBoxes;

        /// <summary>
        /// Matrix Condition Monitor boxes filled.
        /// </summary>
        public int MatrixCMFilled
        {
            get => _intMatrixCMFilled;
            set => _intMatrixCMFilled = value;
        }

        /// <summary>
        /// Base Physical Boxes. 12 for vehicles, 6 for Drones.
        /// </summary>
        public int BasePhysicalBoxes
        {
            get
            {
                //TODO: Move to user-accessible options
                if (IsDrone)
                    //Rigger 5: p145
                    return Category == "Drones: Anthro" ? 8 : 6;
                return 12;
            }
        }

        /// <summary>
        /// Physical Condition Monitor boxes.
        /// </summary>
        public int PhysicalCM
        {
            get
            {
                return BasePhysicalBoxes + (TotalBody + 1) / 2 + Mods.Sum(objMod => objMod?.ConditionMonitor ?? 0);
            }
        }

        /// <summary>
        /// Physical Condition Monitor boxes filled.
        /// </summary>
        public int PhysicalCMFilled
        {
            get => _intPhysicalCMFilled;
            set => _intPhysicalCMFilled = value;
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
        public async Task<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = (await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false))?.SelectSingleNodeAndCacheExpression("altpage", token)?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
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
        /// Location.
        /// </summary>
        public Location Location
        {
            get => _objLocation;
            set => _objLocation = value;
        }

        /// <summary>
        /// Vehicle Modifications applied to the Vehicle.
        /// </summary>
        public TaggedObservableCollection<VehicleMod> Mods => _lstVehicleMods;

        /// <summary>
        /// Gear applied to the Vehicle.
        /// </summary>
        public TaggedObservableCollection<Gear> GearChildren => _lstGear;

        /// <summary>
        /// Weapons applied to the Vehicle through Gear.
        /// </summary>
        public TaggedObservableCollection<Weapon> Weapons => _lstWeapons;

        /// <summary>
        /// Weapon mounts applied to the Vehicle.
        /// </summary>
        public TaggedObservableCollection<WeaponMount> WeaponMounts => _lstWeaponMounts;

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public Task<string> GetDisplayTotalAvailAsync(CancellationToken token = default) => TotalAvailAsync(GlobalSettings.CultureInfo, GlobalSettings.Language, token);

        /// <summary>
        /// Calculated Availability of the Vehicle.
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
        public AvailabilityValue TotalAvailTuple(bool blnIncludeChildren = true)
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

            if (blnIncludeChildren)
            {
                foreach (VehicleMod objChild in Mods)
                {
                    if (objChild.IncludedInVehicle || !objChild.Equipped) continue;
                    AvailabilityValue objLoopAvail = objChild.TotalAvailTuple();
                    if (objLoopAvail.AddToParent)
                        intAvail += objLoopAvail.Value;
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                }

                foreach (WeaponMount objChild in WeaponMounts)
                {
                    if (objChild.IncludedInVehicle || !objChild.Equipped) continue;
                    AvailabilityValue objLoopAvail = objChild.TotalAvailTuple();
                    if (objLoopAvail.AddToParent)
                        intAvail += objLoopAvail.Value;
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                }

                foreach (Weapon objChild in Weapons)
                {
                    if (objChild.ParentID == InternalId || !objChild.Equipped) continue;
                    AvailabilityValue objLoopAvail = objChild.TotalAvailTuple();
                    if (objLoopAvail.AddToParent)
                        intAvail += objLoopAvail.Value;
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                }

                foreach (Gear objChild in GearChildren)
                {
                    if (objChild.ParentID == InternalId) continue;
                    AvailabilityValue objLoopAvail = objChild.TotalAvailTuple();
                    if (objLoopAvail.AddToParent)
                        intAvail += objLoopAvail.Value;
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                }
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public async Task<AvailabilityValue> TotalAvailTupleAsync(bool blnIncludeChildren = true, CancellationToken token = default)
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
                    await (await _objCharacter.GetAttributeSectionAsync(token).ConfigureAwait(false))
                        .ProcessAttributesInXPathAsync(sbdAvail, strAvail, token: token)
                        .ConfigureAwait(false);
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdAvail.ToString(), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }
            }

            if (blnIncludeChildren)
            {
                intAvail += await Mods.SumAsync(async objChild =>
                {
                    if (objChild.IncludedInVehicle || !objChild.Equipped)
                        return 0;
                    AvailabilityValue objLoopAvail
                        = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? objLoopAvail.Value : 0;
                }, token).ConfigureAwait(false) + await WeaponMounts.SumAsync(async objChild =>
                {
                    if (objChild.IncludedInVehicle || !objChild.Equipped)
                        return 0;
                    AvailabilityValue objLoopAvail
                        = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? objLoopAvail.Value : 0;
                }, token).ConfigureAwait(false) + await Weapons.SumAsync(async objChild =>
                {
                    if (objChild.ParentID == InternalId || !objChild.Equipped)
                        return 0;
                    AvailabilityValue objLoopAvail
                        = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? objLoopAvail.Value : 0;
                }, token).ConfigureAwait(false) + await GearChildren.SumAsync(async objChild =>
                {
                    if (objChild.ParentID == InternalId)
                        return 0;
                    AvailabilityValue objLoopAvail
                        = await objChild.TotalAvailTupleAsync(token: token).ConfigureAwait(false);
                    if (objLoopAvail.Suffix == 'F')
                        chrLastAvailChar = 'F';
                    else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                        chrLastAvailChar = 'R';
                    return objLoopAvail.AddToParent ? objLoopAvail.Value : 0;
                }, token).ConfigureAwait(false);
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Number of Slots the Vehicle has for Modifications.
        /// </summary>
        public int Slots =>
            // A Vehicle has 4 or BODY slots, whichever is higher.
            Math.Max(TotalBody, 4) + _intAddSlots;

        /// <summary>
        /// Number of Slots the Vehicle has for Modifications.
        /// </summary>
        public async Task<int> GetSlotsAsync(CancellationToken token = default)
        {
            // A Vehicle has 4 or BODY slots, whichever is higher.
            return Math.Max(await GetTotalBodyAsync(token).ConfigureAwait(false), 4) + _intAddSlots;
        }

        /// <summary>
        /// Calculate the Vehicle's Sensor Rating based on the items within its Sensor.
        /// </summary>
        public int CalculatedSensor
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    int intTotalSensor = _intSensor;
                    // First check for mods that overwrite the Sensor value
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;

                        string strLoop = objMod.Bonus?["sensor"]?.InnerText;
                        intTotalSensor = Math.Max(intTotalSensor,
                            ParseBonus(strLoop, objMod.Rating, intTotalSensor, "Sensor", false));
                        if (!objMod.WirelessOn || objMod.WirelessBonus == null)
                            continue;
                        strLoop = objMod.WirelessBonus?["sensor"]?.InnerText;
                        intTotalSensor = Math.Max(intTotalSensor,
                            ParseBonus(strLoop, objMod.Rating, intTotalSensor, "Sensor", false));
                    }

                    // Then check for mods that modify the sensor value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                    int intTotalBonusSensor = 0;
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;
                        intTotalBonusSensor += ParseBonus(objMod.Bonus?["sensor"]?.InnerText, objMod.Rating,
                            intTotalSensor, "Sensor");

                        if (objMod.WirelessOn)
                        {
                            intTotalBonusSensor += ParseBonus(objMod.WirelessBonus?["sensor"]?.InnerText, objMod.Rating,
                                intTotalSensor, "Sensor");
                        }
                    }

                    return intTotalSensor + intTotalBonusSensor;
                }
            }
        }

        /// <summary>
        /// Calculate the Vehicle's Sensor Rating based on the items within its Sensor.
        /// </summary>
        public async Task<int> GetCalculatedSensorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intTotalSensor = _intSensor;
                // First check for mods that overwrite the Sensor value
                await Mods.ForEachAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return;

                    string strLoop = objMod.Bonus?["sensor"]?.InnerText;
                    intTotalSensor = Math.Max(intTotalSensor,
                        await ParseBonusAsync(strLoop, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalSensor, "Sensor", false, token)
                            .ConfigureAwait(false));
                    if (!objMod.WirelessOn || objMod.WirelessBonus == null)
                        return;
                    strLoop = objMod.WirelessBonus?["sensor"]?.InnerText;
                    intTotalSensor = Math.Max(intTotalSensor,
                        await ParseBonusAsync(strLoop, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalSensor, "Sensor", false, token)
                            .ConfigureAwait(false));
                }, token).ConfigureAwait(false);

                // Then check for mods that modify the sensor value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSensor = await Mods.SumAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return 0;

                    string strLoop = objMod.Bonus?["sensor"]?.InnerText;
                    int intTemp = await ParseBonusAsync(strLoop, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalSensor, "Sensor", token: token)
                        .ConfigureAwait(false);
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        strLoop = objMod.WirelessBonus?["sensor"]?.InnerText;
                        intTemp += await ParseBonusAsync(strLoop, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalSensor, "Sensor", token: token)
                            .ConfigureAwait(false);
                    }

                    return intTemp;
                }, token: token).ConfigureAwait(false);

                return intTotalSensor + intTotalBonusSensor;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Parse a given string from a Mod's bonus node to calculate new bonus or base value.
        /// </summary>
        /// <param name="strBonus">String that will be parsed, replacing values.</param>
        /// <param name="intModRating">Current Rating of the relevant Mod.</param>
        /// <param name="intTotalRating">Total current Rating of the value that is being improved.</param>
        /// <param name="strReplaceRating">String value that will be replaced by intModRating.</param>
        /// <param name="blnBonus">Whether the value must be prefixed with + or - to return a value.</param>
        /// <returns></returns>
        private static int ParseBonus(string strBonus, int intModRating, int intTotalRating, string strReplaceRating, bool blnBonus = true)
        {
            if (!string.IsNullOrEmpty(strBonus))
            {
                char chrFirstCharacter = strBonus[0];
                //Value is a bonus
                if ((chrFirstCharacter == '+' || chrFirstCharacter == '-') && blnBonus)
                {
                    // If the bonus is determined by the existing number, evaluate the expression.
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strBonus.TrimStart('+')
                        .Replace("Rating", intModRating.ToString(GlobalSettings.InvariantCultureInfo))
                        .Replace(strReplaceRating, intTotalRating.ToString(GlobalSettings.InvariantCultureInfo)));
                    if (blnIsSuccess)
                        return ((double)objProcess).StandardRound();
                }
                if (chrFirstCharacter != '+' && chrFirstCharacter != '-' && !blnBonus)
                {
                    // If the bonus is determined by the existing number, evaluate the expression.
                    (bool blnIsSuccess, object objProcess) = CommonFunctions.EvaluateInvariantXPath(strBonus.TrimStart('+')
                        .Replace("Rating", intModRating.ToString(GlobalSettings.InvariantCultureInfo))
                        .Replace(strReplaceRating, intTotalRating.ToString(GlobalSettings.InvariantCultureInfo)));
                    if (blnIsSuccess)
                        return ((double)objProcess).StandardRound();
                }
            }
            return 0;
        }

        /// <summary>
        /// Parse a given string from a Mod's bonus node to calculate new bonus or base value.
        /// </summary>
        /// <param name="strBonus">String that will be parsed, replacing values.</param>
        /// <param name="intModRating">Current Rating of the relevant Mod.</param>
        /// <param name="intTotalRating">Total current Rating of the value that is being improved.</param>
        /// <param name="strReplaceRating">String value that will be replaced by intModRating.</param>
        /// <param name="blnBonus">Whether the value must be prefixed with + or - to return a value.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        private static async Task<int> ParseBonusAsync(string strBonus, int intModRating, int intTotalRating, string strReplaceRating, bool blnBonus = true, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(strBonus))
            {
                char chrFirstCharacter = strBonus[0];
                //Value is a bonus
                if ((chrFirstCharacter == '+' || chrFirstCharacter == '-') && blnBonus)
                {
                    // If the bonus is determined by the existing number, evaluate the expression.
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strBonus.TrimStart('+')
                        .Replace("Rating", intModRating.ToString(GlobalSettings.InvariantCultureInfo))
                        .Replace(strReplaceRating, intTotalRating.ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        return ((double)objProcess).StandardRound();
                }
                if (chrFirstCharacter != '+' && chrFirstCharacter != '-' && !blnBonus)
                {
                    // If the bonus is determined by the existing number, evaluate the expression.
                    (bool blnIsSuccess, object objProcess) = await CommonFunctions.EvaluateInvariantXPathAsync(strBonus.TrimStart('+')
                        .Replace("Rating", intModRating.ToString(GlobalSettings.InvariantCultureInfo))
                        .Replace(strReplaceRating, intTotalRating.ToString(GlobalSettings.InvariantCultureInfo)), token).ConfigureAwait(false);
                    if (blnIsSuccess)
                        return ((double)objProcess).StandardRound();
                }
            }
            return 0;
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
        /// A custom name for the Vehicle assigned by the player.
        /// </summary>
        public string CustomName
        {
            get => _strVehicleName;
            set => _strVehicleName = value;
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public async Task<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            XPathNavigator xmlDataNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            if (xmlDataNode == null)
                return Name;
            return xmlDataNode.SelectSingleNodeAndCacheExpression("translate", token)?.Value ?? Name;
        }

        /// <summary>
        /// Display name.
        /// </summary>
        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        /// <summary>
        /// Display name.
        /// </summary>
        public Task<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Display name.
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        /// <summary>
        /// Display name.
        /// </summary>
        public Task<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(CustomName))
            {
                strReturn += LanguageManager.GetString("String_Space") + "(\"" + CustomName + "\")";
            }

            return strReturn;
        }

        /// <summary>
        /// Display name.
        /// </summary>
        public async Task<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            string strReturn = await DisplayNameShortAsync(strLanguage, token).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(CustomName))
            {
                strReturn += await LanguageManager.GetStringAsync("String_Space", token: token).ConfigureAwait(false) + "(\"" + CustomName + "\")";
            }

            return strReturn;
        }

        /// <summary>
        /// Locations.
        /// </summary>
        public TaggedObservableCollection<Location> Locations => _lstLocations;

        /// <summary>
        /// Whether the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
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
        /// Whether the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
        /// </summary>
        public bool DealerConnectionDiscount
        {
            get => _blnDealerConnectionDiscount;
            set => _blnDealerConnectionDiscount = value;
        }

        /// <summary>
        /// Check whether a vehicle's cost should be discounted by 10% through the Dealer Connection quality on a character.
        /// </summary>
        public bool DoesDealerConnectionCurrentlyApply()
        {
            if (_objCharacter?.DealerConnectionDiscount != true || string.IsNullOrEmpty(_strCategory))
                return false;
            string strUniqueToSearchFor = string.Empty;
            if (_strCategory.StartsWith("Drones", StringComparison.Ordinal))
            {
                strUniqueToSearchFor = "Drones";
            }
            else
            {
                switch (_strCategory)
                {
                    case "Fixed-Wing Aircraft":
                    case "LTAV":
                    case "Rotorcraft":
                    case "VTOL/VSTOL":
                        strUniqueToSearchFor = "Aircraft";
                        break;

                    case "Boats":
                    case "Submarines":
                        strUniqueToSearchFor = "Watercraft";
                        break;

                    case "Bikes":
                    case "Cars":
                    case "Trucks":
                    case "Municipal/Construction":
                    case "Corpsec/Police/Military":
                        strUniqueToSearchFor = "Groundcraft";
                        break;
                }
            }

            return !string.IsNullOrEmpty(strUniqueToSearchFor) && ImprovementManager
                                                                  .GetCachedImprovementListForValueOf(
                                                                      _objCharacter,
                                                                      Improvement.ImprovementType.DealerConnection)
                                                                  .Exists(x => x.UniqueName == strUniqueToSearchFor);
        }

        /// <summary>
        /// Check whether a vehicle's cost should be discounted by 10% through the Dealer Connection quality on a character.
        /// </summary>
        public async Task<bool> DoesDealerConnectionCurrentlyApplyAsync(CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(_strCategory) || _objCharacter == null || !await _objCharacter.GetDealerConnectionDiscountAsync(token).ConfigureAwait(false))
                return false;
            string strUniqueToSearchFor = string.Empty;
            if (_strCategory.StartsWith("Drones", StringComparison.Ordinal))
            {
                strUniqueToSearchFor = "Drones";
            }
            else
            {
                switch (_strCategory)
                {
                    case "Fixed-Wing Aircraft":
                    case "LTAV":
                    case "Rotorcraft":
                    case "VTOL/VSTOL":
                        strUniqueToSearchFor = "Aircraft";
                        break;

                    case "Boats":
                    case "Submarines":
                        strUniqueToSearchFor = "Watercraft";
                        break;

                    case "Bikes":
                    case "Cars":
                    case "Trucks":
                    case "Municipal/Construction":
                    case "Corpsec/Police/Military":
                        strUniqueToSearchFor = "Groundcraft";
                        break;
                }
            }

            return !string.IsNullOrEmpty(strUniqueToSearchFor) && (await ImprovementManager
                    .GetCachedImprovementListForValueOfAsync(
                        _objCharacter,
                        Improvement.ImprovementType.DealerConnection, token: token).ConfigureAwait(false))
                .Exists(x => x.UniqueName == strUniqueToSearchFor);
        }

        /// <summary>
        /// Check whether a vehicle's cost should be discounted by 10% through the Dealer Connection quality on a character.
        /// </summary>
        /// <param name="lstUniques">Collection of DealerConnection improvement uniques.</param>
        /// <param name="strCategory">Vehicle's category.</param>
        /// <returns></returns>
        public static bool DoesDealerConnectionApply(ICollection<string> lstUniques, string strCategory)
        {
            if (lstUniques.Count == 0 || string.IsNullOrEmpty(strCategory))
                return false;
            if (strCategory.StartsWith("Drones", StringComparison.Ordinal))
                return lstUniques.Contains("Drones");
            switch (strCategory)
            {
                case "Fixed-Wing Aircraft":
                case "LTAV":
                case "Rotorcraft":
                case "VTOL/VSTOL":
                    return lstUniques.Contains("Aircraft");

                case "Boats":
                case "Submarines":
                    return lstUniques.Contains("Watercraft");

                case "Bikes":
                case "Cars":
                case "Trucks":
                case "Municipal/Construction":
                case "Corpsec/Police/Military":
                    return lstUniques.Contains("Groundcraft");
            }
            return false;
        }

        #endregion Properties

        #region Complex Properties

        /// <summary>
        /// The number of Slots on the Vehicle that are used by Mods.
        /// </summary>
        public int SlotsUsed
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    return Mods.Sum(objMod => !objMod.IncludedInVehicle && objMod.Equipped,
                               objMod => objMod.CalculatedSlots)
                           + WeaponMounts.Sum(wm => !wm.IncludedInVehicle && wm.Equipped, wm => wm.CalculatedSlots);
                }
            }
        }

        /// <summary>
        /// The number of Slots on the Vehicle that are used by Mods.
        /// </summary>
        public async Task<int> GetSlotsUsedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await Mods.SumAsync(objMod => !objMod.IncludedInVehicle && objMod.Equipped,
                           objMod => objMod.GetCalculatedSlotsAsync(token), token).ConfigureAwait(false)
                       + await WeaponMounts.SumAsync(wm => !wm.IncludedInVehicle && wm.Equipped,
                           wm => wm.GetCalculatedSlotsAsync(token), token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Number of Slots the Drone has for Modifications. (Rigger 5)
        /// </summary>
        public int DroneModSlots
        {
            get
            {
                int intDowngraded = 0;
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    // Mods that are included with a Vehicle by default do not count toward the Slots used.
                    return _intDroneModSlots + Mods.Sum(objMod => !objMod.IncludedInVehicle && objMod.Equipped,
                        objMod =>
                        {
                            int intLoopSlots = objMod.CalculatedSlots;
                            if (intLoopSlots < 0)
                            {
                                //You receive only one additional Mod Point from Downgrades
                                if (objMod.Downgrade)
                                {
                                    if (Interlocked.Increment(ref intDowngraded) == 1)
                                    {
                                        return -intLoopSlots;
                                    }
                                }
                                else
                                {
                                    return -intLoopSlots;
                                }
                            }

                            return 0;
                        });
                }
            }
        }

        /// <summary>
        /// Total Number of Slots the Drone has for Modifications. (Rigger 5)
        /// </summary>
        public async Task<int> GetDroneModSlotsAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intDowngraded = 0;
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // Mods that are included with a Vehicle by default do not count toward the Slots used.
                return _intDroneModSlots + await Mods.SumAsync(objMod => !objMod.IncludedInVehicle && objMod.Equipped,
                    async objMod =>
                    {
                        int intLoopSlots = await objMod.GetCalculatedSlotsAsync(token).ConfigureAwait(false);
                        if (intLoopSlots < 0)
                        {
                            //You receive only one additional Mod Point from Downgrades
                            if (objMod.Downgrade)
                            {
                                if (Interlocked.Increment(ref intDowngraded) == 1)
                                {
                                    return -intLoopSlots;
                                }
                            }
                            else
                            {
                                return -intLoopSlots;
                            }
                        }

                        return 0;
                    }, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// The number of Slots on the Drone that are used by Mods.
        /// </summary>
        public int DroneModSlotsUsed
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    //Downgrade mods apply a bonus to the maximum number of mods and pre-installed mods are already accounted for in the statblock.
                    int intModSlotsUsed =
                        Mods.Sum(objMod => !objMod.IncludedInVehicle && !objMod.Downgrade && objMod.Equipped,
                            objMod => objMod.CalculatedSlots);

                    intModSlotsUsed +=
                        WeaponMounts.Sum(wm => !wm.IncludedInVehicle && wm.Equipped, wm => wm.CalculatedSlots);
                    return intModSlotsUsed;
                }
            }
        }

        /// <summary>
        /// The number of Slots on the Drone that are used by Mods.
        /// </summary>
        public async Task<int> GetDroneModSlotsUsedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                //Downgrade mods apply a bonus to the maximum number of mods and pre-installed mods are already accounted for in the statblock.
                int intModSlotsUsed =
                    await Mods.SumAsync(objMod => !objMod.IncludedInVehicle && !objMod.Downgrade && objMod.Equipped,
                        objMod => objMod.GetCalculatedSlotsAsync(token), token).ConfigureAwait(false);

                intModSlotsUsed +=
                    await WeaponMounts.SumAsync(wm => !wm.IncludedInVehicle && wm.Equipped,
                        wm => wm.GetCalculatedSlotsAsync(token), token).ConfigureAwait(false);
                return intModSlotsUsed;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total cost of the Vehicle including all after-market Modification.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    return OwnCost + Mods.Sum(objMod =>
                    {
                        // Do not include the price of Mods that are part of the base configuration.
                        if (!objMod.IncludedInVehicle)
                        {
                            return objMod.TotalCost;
                        }

                        // If the Mod is a part of the base config, check the items attached to it since their cost still counts.
                        return objMod.Weapons.Sum(objWeapon => objWeapon.TotalCost)
                               + objMod.Cyberware.Sum(objCyberware => objCyberware.TotalCost);
                    }) + WeaponMounts.Sum(wm => wm.TotalCost) + GearChildren.Sum(objGear => objGear.TotalCost);
                }
            }
        }

        /// <summary>
        /// Total cost of the Vehicle including all after-market Modification.
        /// </summary>
        public async Task<decimal> GetTotalCostAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return await GetOwnCostAsync(token).ConfigureAwait(false) + await Mods.SumAsync(async objMod =>
                       {
                           // Do not include the price of Mods that are part of the base configuration.
                           if (!objMod.IncludedInVehicle)
                           {
                               return await objMod.GetTotalCostAsync(token).ConfigureAwait(false);
                           }

                           // If the Mod is a part of the base config, check the items attached to it since their cost still counts.
                           return await objMod.Weapons.SumAsync(objWeapon => objWeapon.GetTotalCostAsync(token),
                                      token).ConfigureAwait(false)
                                  + await objMod.Cyberware.SumAsync(
                                      objCyberware => objCyberware.GetTotalCostAsync(token),
                                      token).ConfigureAwait(false);
                       }, token).ConfigureAwait(false) + await WeaponMounts
                           .SumAsync(wm => wm.GetTotalCostAsync(token), token).ConfigureAwait(false)
                       + await GearChildren.SumAsync(objGear => objGear.GetTotalCostAsync(token), token)
                           .ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        public decimal StolenTotalCost => CalculatedStolenCost(true);

        public decimal NonStolenTotalCost => CalculatedStolenCost(false);

        public decimal CalculatedStolenCost(bool blnStolen)
        {
            decimal decCost = Stolen == blnStolen ? OwnCost : 0;

            decCost += Mods.Sum(objMod =>
                       {
                           // Do not include the price of Mods that are part of the base configureation.
                           if (!objMod.IncludedInVehicle)
                           {
                               return objMod.CalculatedStolenTotalCost(blnStolen);
                           }

                           // If the Mod is a part of the base config, check the items attached to it since their cost still counts.
                           return objMod.Weapons.Sum(objWeapon => objWeapon.CalculatedStolenTotalCost(blnStolen))
                                  + objMod.Cyberware.Sum(objCyberware =>
                                                             objCyberware.CalculatedStolenTotalCost(blnStolen));
                       })
                       + WeaponMounts.Sum(wm => wm.CalculatedStolenTotalCost(blnStolen))
                       + GearChildren.Sum(objGear => objGear.CalculatedStolenTotalCost(blnStolen));

            return decCost;
        }

        public Task<decimal> GetStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenCostAsync(true, token);

        public Task<decimal> GetNonStolenTotalCostAsync(CancellationToken token = default) => CalculatedStolenCostAsync(false, token);

        public async Task<decimal> CalculatedStolenCostAsync(bool blnStolen, CancellationToken token = default)
        {
            decimal decCost = Stolen == blnStolen ? await GetOwnCostAsync(token).ConfigureAwait(false) : 0;

            decCost += await Mods.SumAsync(async objMod =>
                       {
                           // Do not include the price of Mods that are part of the base configureation.
                           if (!objMod.IncludedInVehicle)
                           {
                               return await objMod.CalculatedStolenTotalCostAsync(blnStolen, token)
                                                  .ConfigureAwait(false);
                           }

                           // If the Mod is a part of the base config, check the items attached to it since their cost still counts.
                           return await objMod.Weapons.SumAsync(
                                      objWeapon => objWeapon.CalculatedStolenTotalCostAsync(blnStolen, token),
                                      token).ConfigureAwait(false)
                                  + await objMod.Cyberware.SumAsync(
                                      objCyberware =>
                                          objCyberware.CalculatedStolenTotalCostAsync(blnStolen, token),
                                      token).ConfigureAwait(false);
                       }, token).ConfigureAwait(false)
                       + await WeaponMounts
                               .SumAsync(wm => wm.CalculatedStolenTotalCostAsync(blnStolen, token), token)
                               .ConfigureAwait(false)
                       + await GearChildren
                               .SumAsync(objGear => objGear.CalculatedStolenTotalCostAsync(blnStolen, token),
                                         token).ConfigureAwait(false);

            return decCost;
        }

        /// <summary>
        /// The cost of just the Vehicle itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                decimal decCost = 0;
                string strCost = Cost;
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
                {
                    sbdCost.Append(strCost);
                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdCost, strCost);
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString());
                    if (blnIsSuccess)
                        decCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
                }

                if (DiscountCost)
                    decCost *= 0.9m;

                if (DealerConnectionDiscount)
                    decCost *= 0.9m;

                return decCost;
            }
        }

        /// <summary>
        /// The cost of just the Vehicle itself.
        /// </summary>
        public async Task<decimal> GetOwnCostAsync(CancellationToken token = default)
        {
            decimal decCost = 0;
            string strCost = Cost;
            using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdCost))
            {
                sbdCost.Append(strCost);
                await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdCost, strCost, token: token).ConfigureAwait(false);
                (bool blnIsSuccess, object objProcess)
                    = await CommonFunctions.EvaluateInvariantXPathAsync(sbdCost.ToString(), token).ConfigureAwait(false);
                if (blnIsSuccess)
                    decCost = Convert.ToDecimal(objProcess, GlobalSettings.InvariantCultureInfo);
            }

            if (DiscountCost)
                decCost *= 0.9m;

            if (DealerConnectionDiscount)
                decCost *= 0.9m;

            return decCost;
        }

        /// <summary>
        /// Total Seats of the Vehicle including Modifications.
        /// </summary>
        public int TotalSeats => GetTotalSeats();

        /// <summary>
        /// Total Seats of the Vehicle including Modifications.
        /// </summary>
        public int GetTotalSeats(VehicleMod objExcludeMod = null)
        {
            using (_objCharacter.LockObject.EnterReadLock())
            {
                // First check for mods that overwrite the seat value
                int intTotalSeats = Seats;
                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped || objMod == objExcludeMod)
                        continue;

                    string strBonusSeats = objMod.WirelessOn
                        ? objMod.WirelessBonus?["seats"]?.InnerText ?? objMod.Bonus?["seats"]?.InnerText
                        : objMod.Bonus?["seats"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonusSeats))
                        intTotalSeats = Math.Max(ParseBonus(strBonusSeats, objMod.Rating, Seats, "Seats", false),
                            intTotalSeats);
                }

                // Then check for mods that modify the seat value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSeats = 0;
                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped || objMod == objExcludeMod)
                        continue;
                    string strText = objMod.Bonus?["seats"]?.InnerText;
                    if (!string.IsNullOrEmpty(strText))
                        intTotalBonusSeats += ParseBonus(strText, objMod.Rating,
                            intTotalSeats, "Seats");

                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        strText = objMod.WirelessBonus?["seats"]?.InnerText;
                        if (!string.IsNullOrEmpty(strText))
                            intTotalBonusSeats += ParseBonus(strText, objMod.Rating,
                                intTotalSeats, "Seats");
                    }
                }

                return intTotalSeats + intTotalBonusSeats;
            }
        }

        /// <summary>
        /// Total Seats of the Vehicle including Modifications.
        /// </summary>
        public Task<int> GetTotalSeatsAsync(CancellationToken token = default) => GetTotalSeatsAsync(null, token);

        /// <summary>
        /// Total Seats of the Vehicle including Modifications.
        /// </summary>
        public async Task<int> GetTotalSeatsAsync(VehicleMod objExcludeMod, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // First check for mods that overwrite the seat value
                int intTotalSeats = Seats;
                await Mods.ForEachAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped || objMod == objExcludeMod)
                        return;

                    string strBonusSeats = objMod.WirelessOn
                        ? objMod.WirelessBonus?["seats"]?.InnerText ?? objMod.Bonus?["seats"]?.InnerText
                        : objMod.Bonus?["seats"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonusSeats))
                        intTotalSeats =
                            Math.Max(
                                await ParseBonusAsync(strBonusSeats, await objMod.GetRatingAsync(token).ConfigureAwait(false), Seats, "Seats",
                                        false, token)
                                    .ConfigureAwait(false),
                                intTotalSeats);
                }, token).ConfigureAwait(false);

                // Then check for mods that modify the seat value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSeats = await Mods.SumAsync(
                    objMod => !objMod.IncludedInVehicle && objMod.Equipped && objMod != objExcludeMod, async objMod =>
                    {
                        int intTemp = 0;
                        string strText = objMod.Bonus?["seats"]?.InnerText;
                        if (!string.IsNullOrEmpty(strText))
                            intTemp += await ParseBonusAsync(strText, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalSeats,
                                "Seats", token: token).ConfigureAwait(false);

                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            strText = objMod.WirelessBonus?["seats"]?.InnerText;
                            if (!string.IsNullOrEmpty(strText))
                                intTemp += await ParseBonusAsync(strText, await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                    intTotalSeats, "Seats", token: token).ConfigureAwait(false);
                        }

                        return intTemp;
                    }, token).ConfigureAwait(false);

                return intTotalSeats + intTotalBonusSeats;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Speed of the Vehicle including Modifications.
        /// </summary>
        public string TotalSpeed
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    int intTotalSpeed = Speed;
                    int intBaseOffroadSpeed = OffroadSpeed;
                    int intTotalArmor = Armor;
                    int intModArmor = 0;

                    // First check for mods that overwrite the speed value or add to armor
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;

                        string strBonus = objMod.WirelessOn
                            ? objMod.WirelessBonus?["speed"]?.InnerText ?? objMod.Bonus?["speed"]?.InnerText
                            : objMod.Bonus?["speed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intTotalSpeed = Math.Max(ParseBonus(strBonus, objMod.Rating, Speed, "Speed", false),
                                intTotalSpeed);

                        strBonus = objMod.WirelessOn
                            ? objMod.WirelessBonus?["offroadspeed"]?.InnerText ??
                              objMod.Bonus?["offroadspeed"]?.InnerText
                            : objMod.Bonus?["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intBaseOffroadSpeed =
                                Math.Max(ParseBonus(strBonus, objMod.Rating, OffroadSpeed, "OffroadSpeed", false),
                                    intTotalSpeed);
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            strBonus = objMod.Bonus?["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBonus))
                                intTotalArmor = Math.Max(
                                    ParseBonus(strBonus, objMod.Rating, intTotalArmor, "Armor", false),
                                    intTotalArmor);
                            if (objMod.WirelessOn && objMod.WirelessBonus != null)
                            {
                                strBonus = objMod.WirelessBonus["armor"]?.InnerText;
                                if (!string.IsNullOrEmpty(strBonus))
                                    intTotalArmor =
                                        Math.Max(ParseBonus(strBonus, objMod.Rating, intTotalArmor, "Armor", false),
                                            intTotalArmor);
                            }
                        }
                    }

                    // Then check for mods that modify the speed value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                    int intTotalBonusSpeed = 0;
                    int intTotalBonusOffroadSpeed = 0;
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;
                        if (objMod.Bonus != null)
                        {
                            intTotalBonusSpeed += ParseBonus(objMod.Bonus["speed"]?.InnerText, objMod.Rating,
                                intTotalSpeed, "Speed");
                            intTotalBonusOffroadSpeed += ParseBonus(objMod.Bonus["offroadspeed"]?.InnerText,
                                objMod.Rating, intTotalSpeed, "OffroadSpeed");
                            if (IsDrone && _objCharacter.Settings.DroneMods)
                                intModArmor += ParseBonus(objMod.Bonus["armor"]?.InnerText, objMod.Rating,
                                    intTotalArmor, "Armor");
                        }

                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            intTotalBonusSpeed += ParseBonus(objMod.WirelessBonus["speed"]?.InnerText, objMod.Rating,
                                intTotalSpeed, "Speed");
                            intTotalBonusOffroadSpeed += ParseBonus(objMod.WirelessBonus["offroadspeed"]?.InnerText,
                                objMod.Rating, intTotalSpeed, "OffroadSpeed");
                            if (IsDrone && _objCharacter.Settings.DroneMods)
                                intModArmor += ParseBonus(objMod.WirelessBonus["armor"]?.InnerText, objMod.Rating,
                                    intTotalArmor, "Armor");
                        }
                    }

                    // Reduce speed of the drone if there is too much armor
                    int intPenalty = Math.Max((Math.Min(intTotalArmor + intModArmor, MaxArmor) - TotalBody * 3) / 3, 0);

                    if (Speed != OffroadSpeed || intTotalSpeed + intTotalBonusSpeed !=
                        intBaseOffroadSpeed + intTotalBonusOffroadSpeed)
                    {
                        return (intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString(GlobalSettings
                                   .InvariantCultureInfo) + '/' +
                               (intBaseOffroadSpeed + intTotalBonusOffroadSpeed - intPenalty).ToString(GlobalSettings
                                   .InvariantCultureInfo);
                    }

                    return (intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString(GlobalSettings
                        .InvariantCultureInfo);
                }
            }
        }

        /// <summary>
        /// Total Speed of the Vehicle including Modifications.
        /// </summary>
        public async Task<string> GetTotalSpeedAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intTotalSpeed = Speed;
                int intBaseOffroadSpeed = OffroadSpeed;
                int intTotalArmor = Armor;

                // First check for mods that overwrite the speed value or add to armor
                await Mods.ForEachAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return;

                    string strBonus = objMod.WirelessOn
                        ? objMod.WirelessBonus?["speed"]?.InnerText ?? objMod.Bonus?["speed"]?.InnerText
                        : objMod.Bonus?["speed"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonus))
                        intTotalSpeed = Math.Max(
                            await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), Speed, "Speed", false,
                                    token)
                                .ConfigureAwait(false),
                            intTotalSpeed);

                    strBonus = objMod.WirelessOn
                        ? objMod.WirelessBonus?["offroadspeed"]?.InnerText ?? objMod.Bonus?["offroadspeed"]?.InnerText
                        : objMod.Bonus?["offroadspeed"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonus))
                        intBaseOffroadSpeed =
                            Math.Max(
                                await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), OffroadSpeed,
                                        "OffroadSpeed", false, token)
                                    .ConfigureAwait(false),
                                intTotalSpeed);
                    if (IsDrone && _objCharacter.Settings.DroneMods)
                    {
                        strBonus = objMod.Bonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intTotalArmor =
                                Math.Max(
                                    await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalArmor,
                                            "Armor", false, token)
                                        .ConfigureAwait(false),
                                    intTotalArmor);
                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            strBonus = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBonus))
                                intTotalArmor =
                                    Math.Max(
                                        await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                                intTotalArmor, "Armor", false, token)
                                            .ConfigureAwait(false),
                                        intTotalArmor);
                        }
                    }
                }, token).ConfigureAwait(false);

                // Then check for mods that modify the speed value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSpeed = 0;
                int intTotalBonusOffroadSpeed = 0;
                int intModArmor = await Mods.SumAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return 0;
                    int intTemp = 0;
                    if (objMod.Bonus != null)
                    {
                        intTotalBonusSpeed += await ParseBonusAsync(objMod.Bonus["speed"]?.InnerText,
                            await objMod.GetRatingAsync(token).ConfigureAwait(false),
                            intTotalSpeed, "Speed", token: token).ConfigureAwait(false);
                        intTotalBonusOffroadSpeed += await ParseBonusAsync(objMod.Bonus["offroadspeed"]?.InnerText,
                                await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalSpeed, "OffroadSpeed", token: token)
                            .ConfigureAwait(false);
                        if (IsDrone && await _objCharacter.Settings.GetDroneModsAsync(token).ConfigureAwait(false))
                            intTemp += await ParseBonusAsync(objMod.Bonus["armor"]?.InnerText,
                                await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                intTotalArmor, "Armor", token: token).ConfigureAwait(false);
                    }

                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        intTotalBonusSpeed += await ParseBonusAsync(objMod.WirelessBonus["speed"]?.InnerText,
                            await objMod.GetRatingAsync(token).ConfigureAwait(false),
                            intTotalSpeed, "Speed", token: token).ConfigureAwait(false);
                        intTotalBonusOffroadSpeed += await ParseBonusAsync(
                                objMod.WirelessBonus["offroadspeed"]?.InnerText,
                                await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalSpeed, "OffroadSpeed", token: token)
                            .ConfigureAwait(false);
                        if (IsDrone && await _objCharacter.Settings.GetDroneModsAsync(token).ConfigureAwait(false))
                            intTemp += await ParseBonusAsync(objMod.WirelessBonus["armor"]?.InnerText,
                                await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                intTotalArmor, "Armor", token: token).ConfigureAwait(false);
                    }

                    return intTemp;
                }, token).ConfigureAwait(false);

                // Reduce speed of the drone if there is too much armor
                int intPenalty =
                    Math.Max(
                        (Math.Min(intTotalArmor + intModArmor, await GetMaxArmorAsync(token).ConfigureAwait(false)) -
                         await GetTotalBodyAsync(token).ConfigureAwait(false) * 3) / 3, 0);

                if (Speed != OffroadSpeed ||
                    intTotalSpeed + intTotalBonusSpeed != intBaseOffroadSpeed + intTotalBonusOffroadSpeed)
                {
                    return (intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString(GlobalSettings
                               .InvariantCultureInfo) + '/' +
                           (intBaseOffroadSpeed + intTotalBonusOffroadSpeed - intPenalty).ToString(GlobalSettings
                               .InvariantCultureInfo);
                }

                return (intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString(GlobalSettings.InvariantCultureInfo);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Accel of the Vehicle including Modifications.
        /// </summary>
        public string TotalAccel
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    int intTotalAccel = Accel;
                    int intBaseOffroadAccel = OffroadAccel;
                    int intTotalArmor = Armor;
                    int intModArmor = 0;

                    // First check for mods that overwrite the accel value or add to armor
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;
                        string strBonus = objMod.WirelessOn
                            ? objMod.WirelessBonus?["accel"]?.InnerText ?? objMod.Bonus?["accel"]?.InnerText
                            : objMod.Bonus?["accel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intTotalAccel = Math.Max(ParseBonus(strBonus, objMod.Rating, Accel, "Accel", false),
                                intTotalAccel);

                        strBonus = objMod.WirelessOn
                            ? objMod.WirelessBonus?["offroadaccel"]?.InnerText ??
                              objMod.Bonus?["offroadaccel"]?.InnerText
                            : objMod.Bonus?["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intBaseOffroadAccel =
                                Math.Max(ParseBonus(strBonus, objMod.Rating, OffroadAccel, "OffroadAccel", false),
                                    intTotalAccel);
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            strBonus = objMod.Bonus?["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBonus))
                                intTotalArmor = Math.Max(
                                    ParseBonus(strBonus, objMod.Rating, intTotalArmor, "Armor", false),
                                    intTotalArmor);
                            if (objMod.WirelessOn && objMod.WirelessBonus != null)
                            {
                                strBonus = objMod.WirelessBonus["armor"]?.InnerText;
                                if (!string.IsNullOrEmpty(strBonus))
                                    intTotalArmor =
                                        Math.Max(ParseBonus(strBonus, objMod.Rating, intTotalArmor, "Armor", false),
                                            intTotalArmor);
                            }
                        }
                    }

                    // Then check for mods that modify the accel value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                    int intTotalBonusAccel = 0;
                    int intTotalBonusOffroadAccel = 0;
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;
                        if (objMod.Bonus != null)
                        {
                            intTotalBonusAccel += ParseBonus(objMod.Bonus["accel"]?.InnerText, objMod.Rating,
                                intTotalAccel, "Accel");
                            intTotalBonusOffroadAccel += ParseBonus(objMod.Bonus["offroadaccel"]?.InnerText,
                                objMod.Rating, intTotalAccel, "OffroadAccel");
                            if (IsDrone && _objCharacter.Settings.DroneMods)
                                intModArmor += ParseBonus(objMod.Bonus["armor"]?.InnerText, objMod.Rating,
                                    intTotalArmor, "Armor");
                        }

                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            intTotalBonusAccel += ParseBonus(objMod.WirelessBonus["accel"]?.InnerText, objMod.Rating,
                                intTotalAccel, "Accel");
                            intTotalBonusOffroadAccel += ParseBonus(objMod.WirelessBonus["offroadaccel"]?.InnerText,
                                objMod.Rating, intTotalAccel, "OffroadAccel");
                            if (IsDrone && _objCharacter.Settings.DroneMods)
                                intModArmor += ParseBonus(objMod.WirelessBonus["armor"]?.InnerText, objMod.Rating,
                                    intTotalArmor, "Armor");
                        }
                    }

                    // Reduce acceleration of the drone if there is too much armor
                    int intPenalty = Math.Max((Math.Min(intTotalArmor + intModArmor, MaxArmor) - TotalBody * 3) / 6, 0);

                    if (Accel != OffroadAccel || intTotalAccel + intTotalBonusAccel !=
                        intBaseOffroadAccel + intTotalBonusOffroadAccel)
                    {
                        return (intTotalAccel + intTotalBonusAccel - intPenalty).ToString(GlobalSettings
                                   .InvariantCultureInfo) + '/' +
                               (intBaseOffroadAccel + intTotalBonusOffroadAccel - intPenalty).ToString(GlobalSettings
                                   .InvariantCultureInfo);
                    }

                    return (intTotalAccel + intTotalBonusAccel - intPenalty).ToString(GlobalSettings
                        .InvariantCultureInfo);
                }
            }
        }

        /// <summary>
        /// Total Accel of the Vehicle including Modifications.
        /// </summary>
        public async Task<string> GetTotalAccelAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intTotalAccel = Accel;
                int intBaseOffroadAccel = OffroadAccel;
                int intTotalArmor = Armor;

                // First check for mods that overwrite the accel value or add to armor
                await Mods.ForEachAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return;
                    string strBonus = objMod.WirelessOn
                        ? objMod.WirelessBonus?["accel"]?.InnerText ?? objMod.Bonus?["accel"]?.InnerText
                        : objMod.Bonus?["accel"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonus))
                        intTotalAccel = Math.Max(
                            await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), Accel, "Accel", false, token)
                                .ConfigureAwait(false),
                            intTotalAccel);

                    strBonus = objMod.WirelessOn
                        ? objMod.WirelessBonus?["offroadaccel"]?.InnerText ?? objMod.Bonus?["offroadaccel"]?.InnerText
                        : objMod.Bonus?["offroadaccel"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonus))
                        intBaseOffroadAccel =
                            Math.Max(
                                await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), OffroadAccel, "OffroadAccel", false,
                                        token)
                                    .ConfigureAwait(false),
                                intTotalAccel);
                    if (IsDrone && _objCharacter.Settings.DroneMods)
                    {
                        strBonus = objMod.Bonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intTotalArmor =
                                Math.Max(
                                    await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalArmor, "Armor", false, token)
                                        .ConfigureAwait(false),
                                    intTotalArmor);
                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            strBonus = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBonus))
                                intTotalArmor =
                                    Math.Max(
                                        await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalArmor, "Armor", false,
                                                token)
                                            .ConfigureAwait(false),
                                        intTotalArmor);
                        }
                    }
                }, token).ConfigureAwait(false);

                // Then check for mods that modify the accel value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusAccel = 0;
                int intTotalBonusOffroadAccel = 0;
                int intModArmor = await Mods.SumAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return 0;
                    int intTemp = 0;
                    if (objMod.Bonus != null)
                    {
                        intTotalBonusAccel += await ParseBonusAsync(objMod.Bonus["accel"]?.InnerText, await objMod.GetRatingAsync(token).ConfigureAwait(false),
                            intTotalAccel, "Accel", token: token).ConfigureAwait(false);
                        intTotalBonusOffroadAccel += await ParseBonusAsync(objMod.Bonus["offroadaccel"]?.InnerText,
                            await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalAccel, "OffroadAccel", token: token).ConfigureAwait(false);
                        if (IsDrone && await _objCharacter.Settings.GetDroneModsAsync(token).ConfigureAwait(false))
                            intTemp += await ParseBonusAsync(objMod.Bonus["armor"]?.InnerText, await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                intTotalArmor, "Armor", token: token).ConfigureAwait(false);
                    }

                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        intTotalBonusAccel += await ParseBonusAsync(objMod.WirelessBonus["accel"]?.InnerText,
                            await objMod.GetRatingAsync(token).ConfigureAwait(false),
                            intTotalAccel, "Accel", token: token).ConfigureAwait(false);
                        intTotalBonusOffroadAccel += await ParseBonusAsync(
                            objMod.WirelessBonus["offroadaccel"]?.InnerText,
                            await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalAccel, "OffroadAccel", token: token).ConfigureAwait(false);
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                            intTemp += await ParseBonusAsync(objMod.WirelessBonus["armor"]?.InnerText, await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                intTotalArmor, "Armor", token: token).ConfigureAwait(false);
                    }

                    return intTemp;
                }, token).ConfigureAwait(false);

                // Reduce acceleration of the drone if there is too much armor
                int intPenalty =
                    Math.Max(
                        (Math.Min(intTotalArmor + intModArmor, await GetMaxArmorAsync(token).ConfigureAwait(false)) -
                         await GetTotalBodyAsync(token).ConfigureAwait(false) * 3) / 6, 0);

                if (Accel != OffroadAccel ||
                    intTotalAccel + intTotalBonusAccel != intBaseOffroadAccel + intTotalBonusOffroadAccel)
                {
                    return (intTotalAccel + intTotalBonusAccel - intPenalty).ToString(GlobalSettings
                               .InvariantCultureInfo) + '/' +
                           (intBaseOffroadAccel + intTotalBonusOffroadAccel - intPenalty).ToString(GlobalSettings
                               .InvariantCultureInfo);
                }

                return (intTotalAccel + intTotalBonusAccel - intPenalty).ToString(GlobalSettings.InvariantCultureInfo);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Body of the Vehicle including Modifications.
        /// </summary>
        public int TotalBody => GetTotalBody();

        public int GetTotalBody(VehicleMod objExcludeMod = null)
        {
            using (_objCharacter.LockObject.EnterReadLock())
            {
                return Body + Mods.Sum(x => !x.IncludedInVehicle && x.Equipped && x != objExcludeMod, objMod =>
                {
                    int intTemp = 0;
                    // Add the Modification's Body to the Vehicle's base Body.
                    string strBonus = objMod.Bonus?["body"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonus))
                        intTemp += ParseBonus(strBonus, objMod.Rating, Body, "Body");
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        strBonus = objMod.WirelessBonus?["body"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intTemp += ParseBonus(strBonus, objMod.Rating, Body,
                                "Body");
                    }

                    return intTemp;
                });
            }
        }

        /// <summary>
        /// Total Body of the Vehicle including Modifications.
        /// </summary>
        public Task<int> GetTotalBodyAsync(CancellationToken token = default) => GetTotalBodyAsync(null, token);

        /// <summary>
        /// Total Body of the Vehicle including Modifications.
        /// </summary>
        public async Task<int> GetTotalBodyAsync(VehicleMod objExcludeMod, CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                return Body + await Mods.SumAsync(x => !x.IncludedInVehicle && x.Equipped && x != objExcludeMod,
                    async objMod =>
                    {
                        int intTemp = 0;
                        // Add the Modification's Body to the Vehicle's base Body.
                        string strBonus = objMod.Bonus?["body"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intTemp += await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), Body, "Body",
                                token: token).ConfigureAwait(false);
                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            strBonus = objMod.WirelessBonus?["body"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBonus))
                                intTemp += await ParseBonusAsync(strBonus,
                                    await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                    Body, "Body", token: token).ConfigureAwait(false);
                        }

                        return intTemp;
                    }, token).ConfigureAwait(false);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Handling of the Vehicle including Modifications.
        /// </summary>
        public string TotalHandling
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    int intBaseHandling = Handling;
                    int intBaseOffroadHandling = OffroadHandling;
                    int intTotalArmor = Armor;
                    int intModArmor = 0;

                    // First check for mods that overwrite the handling value or add to armor
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;
                        string strBonus = objMod.WirelessOn
                            ? objMod.WirelessBonus?["handling"]?.InnerText ?? objMod.Bonus?["handling"]?.InnerText
                            : objMod.Bonus?["handling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intBaseHandling = Math.Max(ParseBonus(strBonus, objMod.Rating, Handling, "Handling", false),
                                intBaseHandling);

                        strBonus = objMod.WirelessOn
                            ? objMod.WirelessBonus?["offroadhandling"]?.InnerText ??
                              objMod.Bonus?["offroadhandling"]?.InnerText
                            : objMod.Bonus?["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intBaseOffroadHandling =
                                Math.Max(ParseBonus(strBonus, objMod.Rating, OffroadHandling, "OffroadHandling", false),
                                    intBaseOffroadHandling);
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            strBonus = objMod.Bonus?["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBonus))
                                intTotalArmor = Math.Max(
                                    ParseBonus(strBonus, objMod.Rating, intTotalArmor, "Armor", false),
                                    intTotalArmor);
                            if (objMod.WirelessOn && objMod.WirelessBonus != null)
                            {
                                strBonus = objMod.WirelessBonus["armor"]?.InnerText;
                                if (!string.IsNullOrEmpty(strBonus))
                                    intTotalArmor =
                                        Math.Max(ParseBonus(strBonus, objMod.Rating, intTotalArmor, "Armor", false),
                                            intTotalArmor);
                            }
                        }
                    }

                    // Then check for mods that modify the handling value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                    int intTotalBonusHandling = 0;
                    int intTotalBonusOffroadHandling = 0;
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;
                        if (objMod.Bonus != null)
                        {
                            intTotalBonusHandling += ParseBonus(objMod.Bonus["handling"]?.InnerText, objMod.Rating,
                                intBaseOffroadHandling, "Handling");
                            intTotalBonusOffroadHandling += ParseBonus(objMod.Bonus["offroadhandling"]?.InnerText,
                                objMod.Rating, intBaseOffroadHandling, "OffroadHandling");
                            if (IsDrone && _objCharacter.Settings.DroneMods)
                                intModArmor += ParseBonus(objMod.Bonus["armor"]?.InnerText, objMod.Rating,
                                    intTotalArmor, "Armor");
                        }

                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            intTotalBonusHandling += ParseBonus(objMod.WirelessBonus["handling"]?.InnerText,
                                objMod.Rating, intBaseOffroadHandling, "Handling");
                            intTotalBonusOffroadHandling +=
                                ParseBonus(objMod.WirelessBonus["offroadhandling"]?.InnerText, objMod.Rating,
                                    intBaseOffroadHandling, "OffroadHandling");
                            if (IsDrone && _objCharacter.Settings.DroneMods)
                                intModArmor += ParseBonus(objMod.WirelessBonus["armor"]?.InnerText, objMod.Rating,
                                    intTotalArmor, "Armor");
                        }
                    }

                    // Reduce handling of the drone if there is too much armor
                    int intPenalty = Math.Max((Math.Min(intTotalArmor + intModArmor, MaxArmor) - TotalBody * 3) / 3, 0);

                    if (Handling != OffroadHandling
                        || intBaseHandling + intTotalBonusHandling !=
                        intBaseOffroadHandling + intTotalBonusOffroadHandling)
                    {
                        return (intBaseHandling + intTotalBonusHandling - intPenalty).ToString(GlobalSettings
                                   .InvariantCultureInfo)
                               + '/'
                               + (intBaseOffroadHandling + intTotalBonusOffroadHandling - intPenalty).ToString(
                                   GlobalSettings.InvariantCultureInfo);
                    }

                    return (intBaseHandling + intTotalBonusHandling - intPenalty).ToString(GlobalSettings
                        .InvariantCultureInfo);
                }
            }
        }

        /// <summary>
        /// Total Handling of the Vehicle including Modifications.
        /// </summary>
        public async Task<string> GetTotalHandlingAsync(CancellationToken token = default)
        {
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intBaseHandling = Handling;
                int intBaseOffroadHandling = OffroadHandling;
                int intTotalArmor = Armor;

                // First check for mods that overwrite the handling value or add to armor
                await Mods.ForEachAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return;
                    string strBonus = objMod.WirelessOn
                        ? objMod.WirelessBonus?["handling"]?.InnerText ?? objMod.Bonus?["handling"]?.InnerText
                        : objMod.Bonus?["handling"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonus))
                        intBaseHandling =
                            Math.Max(
                                await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), Handling, "Handling", false, token)
                                    .ConfigureAwait(false),
                                intBaseHandling);

                    strBonus = objMod.WirelessOn
                        ? objMod.WirelessBonus?["offroadhandling"]?.InnerText ??
                          objMod.Bonus?["offroadhandling"]?.InnerText
                        : objMod.Bonus?["offroadhandling"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonus))
                        intBaseOffroadHandling =
                            Math.Max(
                                await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), OffroadHandling, "OffroadHandling",
                                    false,
                                    token).ConfigureAwait(false), intBaseOffroadHandling);
                    if (IsDrone && await _objCharacter.Settings.GetDroneModsAsync(token).ConfigureAwait(false))
                    {
                        strBonus = objMod.Bonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonus))
                            intTotalArmor =
                                Math.Max(
                                    await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalArmor, "Armor", false, token)
                                        .ConfigureAwait(false),
                                    intTotalArmor);
                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            strBonus = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBonus))
                                intTotalArmor =
                                    Math.Max(
                                        await ParseBonusAsync(strBonus, await objMod.GetRatingAsync(token).ConfigureAwait(false), intTotalArmor, "Armor", false,
                                                token)
                                            .ConfigureAwait(false),
                                        intTotalArmor);
                        }
                    }
                }, token).ConfigureAwait(false);

                // Then check for mods that modify the handling value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusHandling = 0;
                int intTotalBonusOffroadHandling = 0;
                int intModArmor = await Mods.SumAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return 0;
                    int intTemp = 0;
                    if (objMod.Bonus != null)
                    {
                        intTotalBonusHandling += await ParseBonusAsync(objMod.Bonus["handling"]?.InnerText,
                            await objMod.GetRatingAsync(token).ConfigureAwait(false),
                            intBaseOffroadHandling, "Handling", token: token).ConfigureAwait(false);
                        intTotalBonusOffroadHandling += await ParseBonusAsync(
                                objMod.Bonus["offroadhandling"]?.InnerText,
                                await objMod.GetRatingAsync(token).ConfigureAwait(false), intBaseOffroadHandling, "OffroadHandling", token: token)
                            .ConfigureAwait(false);
                        if (IsDrone && await _objCharacter.Settings.GetDroneModsAsync(token).ConfigureAwait(false))
                            intTemp += await ParseBonusAsync(objMod.Bonus["armor"]?.InnerText, await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                intTotalArmor,
                                "Armor", token: token).ConfigureAwait(false);
                    }

                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        intTotalBonusHandling += await ParseBonusAsync(objMod.WirelessBonus["handling"]?.InnerText,
                            await objMod.GetRatingAsync(token).ConfigureAwait(false), intBaseOffroadHandling, "Handling", token: token).ConfigureAwait(false);
                        intTotalBonusOffroadHandling += await ParseBonusAsync(
                            objMod.WirelessBonus["offroadhandling"]?.InnerText, await objMod.GetRatingAsync(token).ConfigureAwait(false), intBaseOffroadHandling,
                            "OffroadHandling", token: token).ConfigureAwait(false);
                        if (IsDrone && await _objCharacter.Settings.GetDroneModsAsync(token).ConfigureAwait(false))
                            intTemp += await ParseBonusAsync(objMod.WirelessBonus["armor"]?.InnerText, await objMod.GetRatingAsync(token).ConfigureAwait(false),
                                intTotalArmor, "Armor", token: token).ConfigureAwait(false);
                    }

                    return intTemp;
                }, token).ConfigureAwait(false);

                // Reduce handling of the drone if there is too much armor
                int intPenalty =
                    Math.Max(
                        (Math.Min(intTotalArmor + intModArmor, await GetMaxArmorAsync(token).ConfigureAwait(false)) -
                         await GetTotalBodyAsync(token).ConfigureAwait(false) * 3) / 3, 0);

                if (Handling != OffroadHandling
                    || intBaseHandling + intTotalBonusHandling != intBaseOffroadHandling + intTotalBonusOffroadHandling)
                {
                    return (intBaseHandling + intTotalBonusHandling - intPenalty).ToString(GlobalSettings
                               .InvariantCultureInfo)
                           + '/'
                           + (intBaseOffroadHandling + intTotalBonusOffroadHandling - intPenalty).ToString(
                               GlobalSettings.InvariantCultureInfo);
                }

                return (intBaseHandling + intTotalBonusHandling - intPenalty).ToString(GlobalSettings
                    .InvariantCultureInfo);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Total Armor of the Vehicle including Modifications.
        /// </summary>
        public int TotalArmor
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    int intArmor = Armor;

                    // First check for mods that overwrite the armor value
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;

                        string strLoop = objMod.Bonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strLoop))
                            intArmor = Math.Max(intArmor, ParseBonus(strLoop, objMod.Rating, intArmor, "Armor", false));
                        if (!objMod.WirelessOn || objMod.WirelessBonus == null)
                            continue;
                        strLoop = objMod.WirelessBonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strLoop))
                            intArmor = Math.Max(intArmor, ParseBonus(strLoop, objMod.Rating, intArmor, "Armor", false));
                    }

                    int intModArmor = 0;

                    // Add the Modification's Armor to the Vehicle's base Armor.
                    foreach (VehicleMod objMod in Mods)
                    {
                        if (objMod.IncludedInVehicle || !objMod.Equipped)
                            continue;

                        string strLoop = objMod.Bonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strLoop))
                            intModArmor += ParseBonus(strLoop, objMod.Rating, intArmor, "Armor");
                        if (!objMod.WirelessOn || objMod.WirelessBonus == null)
                            continue;
                        strLoop = objMod.WirelessBonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strLoop))
                            intModArmor += ParseBonus(strLoop, objMod.Rating, intArmor, "Armor");
                    }

                    return Math.Min(MaxArmor, intModArmor + intArmor);
                }
            }
        }

        /// <summary>
        /// Total Armor of the Vehicle including Modifications.
        /// </summary>
        public async Task<int> GetTotalArmorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                int intArmor = Armor;

                // First check for mods that overwrite the armor value
                await Mods.ForEachAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return;

                    string strLoop = objMod.Bonus?["armor"]?.InnerText;
                    if (!string.IsNullOrEmpty(strLoop))
                        intArmor = Math.Max(intArmor,
                            await ParseBonusAsync(strLoop, await objMod.GetRatingAsync(token).ConfigureAwait(false), intArmor, "Armor", false,
                                    token)
                                .ConfigureAwait(false));
                    if (!objMod.WirelessOn || objMod.WirelessBonus == null)
                        return;
                    strLoop = objMod.WirelessBonus?["armor"]?.InnerText;
                    if (!string.IsNullOrEmpty(strLoop))
                        intArmor = Math.Max(intArmor,
                            await ParseBonusAsync(strLoop, await objMod.GetRatingAsync(token).ConfigureAwait(false), intArmor, "Armor", false,
                                    token)
                                .ConfigureAwait(false));
                }, token).ConfigureAwait(false);

                // Add the Modification's Armor to the Vehicle's base Armor.
                int intModArmor = await Mods.SumAsync(async objMod =>
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        return 0;

                    string strLoop = objMod.Bonus?["armor"]?.InnerText;
                    int intTemp = 0;
                    if (!string.IsNullOrEmpty(strLoop))
                        intTemp += await ParseBonusAsync(strLoop, await objMod.GetRatingAsync(token).ConfigureAwait(false), intArmor,
                                "Armor", token: token)
                            .ConfigureAwait(false);
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        strLoop = objMod.WirelessBonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strLoop))
                            intTemp += await ParseBonusAsync(strLoop, await objMod.GetRatingAsync(token).ConfigureAwait(false), intArmor,
                                    "Armor", token: token)
                                .ConfigureAwait(false);
                    }

                    return intTemp;
                }, token: token).ConfigureAwait(false);

                return Math.Min(await GetMaxArmorAsync(token).ConfigureAwait(false), intModArmor + intArmor);
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum amount of each Armor type the Vehicle can hold.
        /// </summary>
        public int MaxArmor
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    // If ignoring the rules, do not limit Armor to the Vehicle's standard rules.
                    if (_objCharacter.IgnoreRules)
                        return int.MaxValue;

                    // Drones have no theoretical armor cap in the optional rules, otherwise, it's capped
                    if (IsDrone && _objCharacter.Settings.DroneMods)
                        return int.MaxValue;
                    // Rigger 5 says max extra armor is Body + starting Armor, p159
                    // When you need to use a 0 for the math, use 0.5 instead
                    int intReturn = IsDrone && _objCharacter.Settings.DroneArmorMultiplierEnabled
                        ? ((Math.Max(Body, 0.5m) + Armor) * _objCharacter.Settings.DroneArmorMultiplier).StandardRound()
                        : Math.Max(Body + Armor, 1);

                    return intReturn;
                }
            }
        }

        /// <summary>
        /// Maximum amount of each Armor type the Vehicle can hold.
        /// </summary>
        public async Task<int> GetMaxArmorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                // If ignoring the rules, do not limit Armor to the Vehicle's standard rules.
                if (await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
                    return int.MaxValue;

                CharacterSettings objSettings = await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false);
                // Drones have no theoretical armor cap in the optional rules, otherwise, it's capped
                if (IsDrone && await objSettings.GetDroneModsAsync(token).ConfigureAwait(false))
                    return int.MaxValue;
                // Rigger 5 says max extra armor is Body + starting Armor, p159
                // When you need to use a 0 for the math, use 0.5 instead
                int intReturn = IsDrone &&
                                await objSettings.GetDroneArmorMultiplierEnabledAsync(token).ConfigureAwait(false)
                    ? ((Math.Max(Body, 0.5m) + Armor) * await objSettings.GetDroneArmorMultiplierAsync(token).ConfigureAwait(false)).StandardRound()
                    : Math.Max(Body + Armor, 1);

                return intReturn;
            }
            finally
            {
                await objLocker.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Maximum Speed attribute allowed for the Vehicle
        /// </summary>
        public int MaxSpeed
        {
            get
            {
                //Drone's attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules)
                {
                    return Math.Max(Speed * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Speed attribute allowed for the Vehicle
        /// </summary>
        public async Task<int> GetMaxSpeedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //Drone's attributes can never by higher than twice their starting value (R5, p123)
            //When you need to use a 0 for the math, use 0.5 instead
            if (IsDrone && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
            {
                return Math.Max(Speed * 2, 1);
            }

            return int.MaxValue;
        }

        /// <summary>
        /// Maximum Handling attribute allowed for the Vehicle
        /// </summary>
        public int MaxHandling
        {
            get
            {
                //Drone's attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules)
                {
                    return Math.Max(Handling * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Handling attribute allowed for the Vehicle
        /// </summary>
        public async Task<int> GetMaxHandlingAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //Drone's attributes can never by higher than twice their starting value (R5, p123)
            //When you need to use a 0 for the math, use 0.5 instead
            if (IsDrone && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
            {
                return Math.Max(Handling * 2, 1);
            }

            return int.MaxValue;
        }

        /// <summary>
        /// Maximum Acceleration attribute allowed for the Vehicle
        /// </summary>
        public int MaxAcceleration
        {
            get
            {
                //Drone's attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules)
                {
                    return Math.Max(Accel * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Acceleration attribute allowed for the Vehicle
        /// </summary>
        public async Task<int> GetMaxAccelerationAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //Drone's attributes can never by higher than twice their starting value (R5, p123)
            //When you need to use a 0 for the math, use 0.5 instead
            if (IsDrone && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
            {
                return Math.Max(Accel * 2, 1);
            }

            return int.MaxValue;
        }

        /// <summary>
        /// Maximum Sensor attribute allowed for the Vehicle
        /// </summary>
        public int MaxSensor
        {
            get
            {
                //Drone's attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules)
                {
                    return Math.Max(BaseSensor * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Sensor attribute allowed for the Vehicle
        /// </summary>
        public async Task<int> GetMaxSensorAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //Drone's attributes can never by higher than twice their starting value (R5, p123)
            //When you need to use a 0 for the math, use 0.5 instead
            if (IsDrone && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false))
            {
                return Math.Max(BaseSensor * 2, 1);
            }

            return int.MaxValue;
        }

        /// <summary>
        /// Maximum Pilot attribute allowed for the Vehicle
        /// </summary>
        public int MaxPilot
        {
            get
            {
                //Drone's attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules && _objCharacter.Settings.DroneModsMaximumPilot)
                {
                    return Math.Max(Pilot * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Pilot attribute allowed for the Vehicle
        /// </summary>
        public async Task<int> GetMaxPilotAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            //Drone's attributes can never by higher than twice their starting value (R5, p123)
            //When you need to use a 0 for the math, use 0.5 instead
            if (IsDrone && !await _objCharacter.GetIgnoreRulesAsync(token).ConfigureAwait(false) &&
                (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).DroneModsMaximumPilot)
            {
                return Math.Max(await GetPilotAsync(token).ConfigureAwait(false) * 2, 1);
            }

            return int.MaxValue;
        }

        public static readonly ReadOnlyCollection<string> ModCategoryStrings = Array.AsReadOnly(new[]
            {"Powertrain", "Protection", "Weapons", "Body", "Electromagnetic", "Cosmetic"});

        /// <summary>
        /// Check if the vehicle is over capacity in any category
        /// </summary>
        public bool OverR5Capacity(string strCheckCapacity = "")
        {
            return !string.IsNullOrEmpty(strCheckCapacity) && ModCategoryStrings.Contains(strCheckCapacity)
                ? CalcCategoryAvail(strCheckCapacity) < 0
                : ModCategoryStrings.Any(strCategory => CalcCategoryAvail(strCategory) < 0);
        }

        /// <summary>
        /// Check if the vehicle is over capacity in any category
        /// </summary>
        public async Task<bool> OverR5CapacityAsync(string strCheckCapacity = "", CancellationToken token = default)
        {
            return !string.IsNullOrEmpty(strCheckCapacity) && ModCategoryStrings.Contains(strCheckCapacity)
                ? await CalcCategoryAvailAsync(strCheckCapacity, token).ConfigureAwait(false) < 0
                : await ModCategoryStrings.AnyAsync(async strCategory => await CalcCategoryAvailAsync(strCategory, token).ConfigureAwait(false) < 0, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string PowertrainModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = Body + _intAddPowertrainModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Powertrain") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string ProtectionModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = Body + _intAddProtectionModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Protection") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string WeaponModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = Body + _intAddWeaponModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Weapons") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Body Mod Slots as Used/Total
        /// </summary>
        public string BodyModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = Body + _intAddBodyModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Body") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Electromagnetic Mod Slots as Used/Total
        /// </summary>
        public string ElectromagneticModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = Body + _intAddElectromagneticModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Electromagnetic") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Cosmetic Mod Slots as Used/Total
        /// </summary>
        public string CosmeticModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = Body + _intAddCosmeticModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Cosmetic") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public async Task<string> PowertrainModSlotsUsedAsync(int intModSlots = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intTotal = Body + _intAddPowertrainModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - await CalcCategoryAvailAsync("Powertrain", token).ConfigureAwait(false) + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public async Task<string> ProtectionModSlotsUsedAsync(int intModSlots = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intTotal = Body + _intAddProtectionModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - await CalcCategoryAvailAsync("Protection", token).ConfigureAwait(false) + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public async Task<string> WeaponModSlotsUsedAsync(int intModSlots = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intTotal = Body + _intAddWeaponModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - await CalcCategoryAvailAsync("Weapons", token).ConfigureAwait(false) + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Body Mod Slots as Used/Total
        /// </summary>
        public async Task<string> BodyModSlotsUsedAsync(int intModSlots = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intTotal = Body + _intAddBodyModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - await CalcCategoryAvailAsync("Body", token).ConfigureAwait(false) + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Electromagnetic Mod Slots as Used/Total
        /// </summary>
        public async Task<string> ElectromagneticModSlotsUsedAsync(int intModSlots = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intTotal = Body + _intAddElectromagneticModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - await CalcCategoryAvailAsync("Electromagnetic", token).ConfigureAwait(false) + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Cosmetic Mod Slots as Used/Total
        /// </summary>
        public async Task<string> CosmeticModSlotsUsedAsync(int intModSlots = 0, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intTotal = Body + _intAddCosmeticModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - await CalcCategoryAvailAsync("Cosmetic", token).ConfigureAwait(false) + intModSlots, intTotal);
        }

        /// <summary>
        /// Vehicle's Maneuver AutoSoft Rating.
        /// </summary>
        public int Maneuver
        {
            get
            {
                using (_objCharacter.LockObject.EnterReadLock())
                {
                    Gear objGear = GearChildren.DeepFirstOrDefault(x => x.Children,
                        x => x.Name == "[Model] Maneuvering Autosoft" && x.Extra == Name &&
                             !x.InternalId.IsEmptyGuid());
                    return objGear?.Rating ?? 0;
                }
            }
        }

        /// <summary>
        /// Vehicle's Maneuver AutoSoft Rating.
        /// </summary>
        public async Task<int> GetManeuverAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            IAsyncDisposable objLocker = await _objCharacter.LockObject.EnterReadLockAsync(token).ConfigureAwait(false);
            try
            {
                token.ThrowIfCancellationRequested();
                Gear objGear = await GearChildren.DeepFirstOrDefaultAsync(x => x.Children,
                    x => x.Name == "[Model] Maneuvering Autosoft" && x.Extra == Name && !x.InternalId.IsEmptyGuid(),
                    token: token).ConfigureAwait(false);
                return objGear != null ? await objGear.GetRatingAsync(token).ConfigureAwait(false) : 0;
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
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlDocument objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("vehicles.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("vehicles.xml", strLanguage, token: token).ConfigureAwait(false);
            objReturn = objDoc.TryGetNodeById("/chummer/vehicles/vehicle", SourceID);
            if (objReturn == null && SourceID != Guid.Empty)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/vehicles/vehicle", Name);
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
            objReturn = objDoc.TryGetNodeById("/chummer/vehicles/vehicle", SourceID);
            if (objReturn == null && SourceID != Guid.Empty)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/vehicles/vehicle", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
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

        /// <inheritdoc />
        public string Overclocked
        {
            get => _strOverclocked;
            set => _strOverclocked = value;
        }

        /// <inheritdoc />
        public async Task<string> GetOverclockedAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await _objCharacter.GetOverclockerAsync(token).ConfigureAwait(false) ? _strOverclocked : string.Empty;
        }

        /// <summary>
        /// Empty for Vehicles.
        /// </summary>
        public string CanFormPersona
        {
            get => string.Empty;
            set
            {
                // Dummy
            }
        }

        /// <summary>
        /// String to determine if gear can form persona or grants persona forming to its parent.
        /// </summary>
        public Task<string> GetCanFormPersonaAsync(CancellationToken token = default) => token.IsCancellationRequested
            ? Task.FromCanceled<string>(token)
            : Task.FromResult(string.Empty);

        public bool IsCommlink => GearChildren.Any(x => x.CanFormPersona.Contains("Parent")) &&
                                  this.GetTotalMatrixAttribute("Device Rating") > 0;

        /// <summary>
        /// String to determine if gear can form persona or grants persona forming to its parent.
        /// </summary>
        public async Task<bool> GetIsCommlinkAsync(CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return await GearChildren.AnyAsync(
                       async x => (await x.GetCanFormPersonaAsync(token).ConfigureAwait(false)).Contains("Parent"),
                       token: token).ConfigureAwait(false) &&
                   await this.GetTotalMatrixAttributeAsync("Device Rating", token).ConfigureAwait(false) > 0;
        }

        /// <summary>
        /// 0 for Vehicles.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get => 0;
            set
            {
                // Dummy
            }
        }

        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intReturn = 0;
                foreach (Gear objGear in GearChildren)
                {
                    if (objGear.Equipped)
                    {
                        intReturn += objGear.TotalBonusMatrixBoxes;
                    }
                }
                foreach (VehicleMod objMod in Mods)
                {
                    string strBonusBoxes = objMod.Bonus?["matrixcmbonus"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonusBoxes))
                    {
                        // Add the Modification's Device Rating to the Vehicle's base Device Rating.
                        intReturn += Convert.ToInt32(strBonusBoxes, GlobalSettings.InvariantCultureInfo);
                    }
                    if (objMod.WirelessOn)
                    {
                        strBonusBoxes = objMod.WirelessBonus?["matrixcmbonus"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonusBoxes))
                        {
                            intReturn += Convert.ToInt32(strBonusBoxes, GlobalSettings.InvariantCultureInfo);
                        }
                    }
                }
                return intReturn;
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
        /// Returns true if this is a cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get => _blnCanSwapAttributes;
            set => _blnCanSwapAttributes = value;
        }

        public IEnumerable<IHasMatrixAttributes> ChildrenWithMatrixAttributes => GearChildren.Concat<IHasMatrixAttributes>(Weapons);

        #endregion Complex Properties

        #region Methods

        /// <summary>
        /// Total number of slots used by vehicle mods (and weapon mounts) in a given Rigger 5 vehicle mod category.
        /// </summary>
        public int CalcCategoryUsed(string strCategory)
        {
            int intBase = Mods.Sum(objMod =>
            {
                if (objMod.IncludedInVehicle || !objMod.Equipped || objMod.Category != strCategory)
                    return 0;
                // Subtract the Modification's Slots from the Vehicle's base Body.
                return Math.Max(objMod.CalculatedSlots, 0);
            });

            if (string.Equals(strCategory, "Weapons", StringComparison.OrdinalIgnoreCase))
            {
                intBase += WeaponMounts.Sum(objMount =>
                {
                    if (objMount.IncludedInVehicle || !objMount.Equipped)
                        return 0;
                    // Subtract the Weapon Mount's Slots from the Vehicle's base Body.
                    return Math.Max(objMount.CalculatedSlots, 0);
                });
            }

            return intBase;
        }

        /// <summary>
        /// Total number of slots used by vehicle mods (and weapon mounts) in a given Rigger 5 vehicle mod category.
        /// </summary>
        public async Task<int> CalcCategoryUsedAsync(string strCategory, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intBase = await Mods.SumAsync(
                objMod => !objMod.IncludedInVehicle && objMod.Equipped && objMod.Category == strCategory,
                // Subtract the Modification's Slots from the Vehicle's base Body.
                async objMod => Math.Max(await objMod.GetCalculatedSlotsAsync(token).ConfigureAwait(false), 0),
                token: token).ConfigureAwait(false);

            if (string.Equals(strCategory, "Weapons", StringComparison.OrdinalIgnoreCase))
            {
                intBase += await WeaponMounts.SumAsync(
                    objMount => !objMount.IncludedInVehicle && objMount.Equipped,
                    // Subtract the Weapon Mount's Slots from the Vehicle's base Body.
                    async objMod => Math.Max(await objMod.GetCalculatedSlotsAsync(token).ConfigureAwait(false), 0),
                    token: token).ConfigureAwait(false);
            }

            return intBase;
        }

        /// <summary>
        /// Total number of slots still available for vehicle mods (and weapon mounts) in a given Rigger 5 vehicle mod category.
        /// </summary>
        public int CalcCategoryAvail(string strCategory)
        {
            int intBase = Body;

            switch (strCategory.ToUpperInvariant())
            {
                case "POWERTRAIN":
                    intBase += _intAddPowertrainModSlots;
                    break;

                case "WEAPONS":
                    intBase += _intAddWeaponModSlots;
                    break;

                case "BODY":
                    intBase += _intAddBodyModSlots;
                    break;

                case "ELECTROMAGNETIC":
                    intBase += _intAddElectromagneticModSlots;
                    break;

                case "PROTECTION":
                    intBase += _intAddProtectionModSlots;
                    break;

                case "COSMETIC":
                    intBase += _intAddCosmeticModSlots;
                    break;
            }

            intBase -= CalcCategoryUsed(strCategory);
            return intBase;
        }

        /// <summary>
        /// Total number of slots still available for vehicle mods (and weapon mounts) in a given Rigger 5 vehicle mod category.
        /// </summary>
        public async Task<int> CalcCategoryAvailAsync(string strCategory, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            int intBase = Body;

            switch (strCategory.ToUpperInvariant())
            {
                case "POWERTRAIN":
                    intBase += _intAddPowertrainModSlots;
                    break;

                case "WEAPONS":
                    intBase += _intAddWeaponModSlots;
                    break;

                case "BODY":
                    intBase += _intAddBodyModSlots;
                    break;

                case "ELECTROMAGNETIC":
                    intBase += _intAddElectromagneticModSlots;
                    break;

                case "PROTECTION":
                    intBase += _intAddProtectionModSlots;
                    break;

                case "COSMETIC":
                    intBase += _intAddCosmeticModSlots;
                    break;
            }

            intBase -= await CalcCategoryUsedAsync(strCategory, token).ConfigureAwait(false);
            return intBase;
        }

        public decimal DeleteVehicle()
        {
            _objCharacter.Vehicles.Remove(this);

            decimal decReturn = GearChildren.AsEnumerableWithSideEffects().Sum(x => x.DeleteGear(false))
                                + Weapons.AsEnumerableWithSideEffects().Sum(x => x.DeleteWeapon(false))
                                + Mods.AsEnumerableWithSideEffects().Sum(x => x.DeleteVehicleMod(false))
                                + WeaponMounts.AsEnumerableWithSideEffects().Sum(x => x.DeleteWeaponMount(false));

            DisposeSelf();

            return decReturn;
        }

        public async Task<decimal> DeleteVehicleAsync(CancellationToken token = default)
        {
            await _objCharacter.Vehicles.RemoveAsync(this, token).ConfigureAwait(false);

            decimal decReturn = await GearChildren.SumWithSideEffectsAsync(x => x.DeleteGearAsync(false, token), token)
                                                  .ConfigureAwait(false)
                                + await Weapons.SumWithSideEffectsAsync(x => x.DeleteWeaponAsync(false, token), token)
                                               .ConfigureAwait(false)
                                + await Mods.SumWithSideEffectsAsync(x => x.DeleteVehicleModAsync(false, token), token)
                                            .ConfigureAwait(false)
                                + await WeaponMounts
                                        .SumWithSideEffectsAsync(x => x.DeleteWeaponMountAsync(false, token), token)
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
                                                                 || intValidAvail < intLowestValidRestrictedGearAvail))
                                intLowestValidRestrictedGearAvail = intValidAvail;
                        }

                        if (intLowestValidRestrictedGearAvail >= 0 && dicRestrictedGearLimits[intLowestValidRestrictedGearAvail] > 0)
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

            intRestrictedCount += await Mods
                                        .SumAsync(objChild =>
                                                objChild
                                                    .CheckRestrictedGear(
                                                        dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                        token), token: token)
                                        .ConfigureAwait(false)
                                  + await GearChildren
                                          .SumAsync(objChild =>
                                                  objChild
                                                      .CheckRestrictedGear(
                                                          dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                          token), token: token)
                                          .ConfigureAwait(false)
                                  + await Weapons
                                          .SumAsync(objChild =>
                                                  objChild
                                                      .CheckRestrictedGear(
                                                          dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                          token), token: token)
                                          .ConfigureAwait(false)
                                  + await WeaponMounts
                                          .SumAsync(objChild =>
                                                  objChild
                                                      .CheckRestrictedGear(
                                                          dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems,
                                                          token), token: token)
                                          .ConfigureAwait(false);

            return intRestrictedCount;
        }

        /// <summary>
        /// Checks whether a given VehicleMod is allowed to be added to this vehicle.
        /// </summary>
        public async Task<bool> CheckModRequirementsAsync(XPathNavigator objXmlMod,
                                                                     CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (objXmlMod == null)
                return false;

            if (!await objXmlMod.RequirementsMetAsync(_objCharacter, this, token: token)
                    .ConfigureAwait(false))
                return false;

            XPathNavigator xmlTestNode = objXmlMod
                                               .SelectSingleNodeAndCacheExpression(
                                                   "forbidden/vehicledetails", token);
            if (xmlTestNode != null)
            {
                XPathNavigator xmlRequirementsNode = await this.GetNodeXPathAsync(token).ConfigureAwait(false);
                // Assumes topmost parent is an AND node
                if (await xmlRequirementsNode.ProcessFilterOperationNodeAsync(xmlTestNode, false, token)
                                             .ConfigureAwait(false))
                    return false;
            }

            xmlTestNode = objXmlMod.SelectSingleNodeAndCacheExpression("required/vehicledetails", token);
            // Assumes topmost parent is an AND node
            return xmlTestNode == null || await (await this.GetNodeXPathAsync(token).ConfigureAwait(false))
                                                .ProcessFilterOperationNodeAsync(xmlTestNode, false, token)
                                                .ConfigureAwait(false);
        }

        #region UI Methods

        /// <summary>
        /// Add a Vehicle to the TreeView.
        /// </summary>
        /// <param name="cmsVehicle">ContextMenuStrip for the Vehicle Node.</param>
        /// <param name="cmsVehicleLocation">ContextMenuStrip for Vehicle Location Nodes.</param>
        /// <param name="cmsVehicleWeapon">ContextMenuStrip for Vehicle Weapon Nodes.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip for Vehicle Weapon Accessory Nodes.</param>
        /// <param name="cmsWeaponAccessoryGear">ContextMenuStrip for Gear in Vehicle Weapon Accessory Nodes.</param>
        /// <param name="cmsVehicleGear">ContextMenuStrip for Vehicle Gear Nodes.</param>
        /// <param name="cmsVehicleWeaponMount">ContextMenuStrip for Vehicle Weapon Mounts.</param>
        /// <param name="cmsCyberware">ContextMenuStrip for Cyberware.</param>
        /// <param name="cmsCyberwareGear">ContextMenuStrip for Gear in Cyberware.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<TreeNode> CreateTreeNode(ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !await _objCharacter.Settings.BookEnabledAsync(Source, token).ConfigureAwait(false))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = await GetCurrentDisplayNameAsync(token).ConfigureAwait(false),
                Tag = this,
                ContextMenuStrip = cmsVehicle,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // Populate the list of Vehicle Locations.
            await Locations
                .ForEachAsync(
                    async objLocation =>
                        lstChildNodes.Add(await objLocation.CreateTreeNode(cmsVehicleLocation, token)
                            .ConfigureAwait(false)), token).ConfigureAwait(false);

            // VehicleMods.
            await Mods.ForEachAsync(async objMod =>
            {
                TreeNode objLoopNode = await objMod.CreateTreeNode(cmsVehicle, cmsCyberware, cmsCyberwareGear,
                    cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, token).ConfigureAwait(false);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }, token).ConfigureAwait(false);
            if (await WeaponMounts.GetCountAsync(token).ConfigureAwait(false) > 0)
            {
                TreeNode nodMountsNode = new TreeNode
                {
                    Tag = "String_WeaponMounts",
                    Text = await LanguageManager.GetStringAsync("String_WeaponMounts", token: token).ConfigureAwait(false)
                };

                // Weapon Mounts
                await WeaponMounts.ForEachAsync(async objWeaponMount =>
                {
                    TreeNode objLoopNode = await objWeaponMount.CreateTreeNode(cmsVehicleWeaponMount, cmsVehicleWeapon,
                        cmsWeaponAccessory, cmsWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicle, token).ConfigureAwait(false);
                    if (objLoopNode != null)
                    {
                        nodMountsNode.Nodes.Add(objLoopNode);
                        nodMountsNode.Expand();
                    }
                }, token).ConfigureAwait(false);

                if (nodMountsNode.Nodes.Count > 0)
                    lstChildNodes.Add(nodMountsNode);
            }
            // Vehicle Weapons (not attached to a mount).
            await Weapons.ForEachAsync(async objWeapon =>
            {
                TreeNode objLoopNode = await objWeapon.CreateTreeNode(cmsVehicleWeapon, cmsWeaponAccessory,
                    cmsWeaponAccessoryGear, token).ConfigureAwait(false);
                if (objLoopNode != null)
                {
                    TreeNode objParent = objNode;
                    if (objWeapon.Location != null)
                    {
                        foreach (TreeNode objFind in lstChildNodes)
                        {
                            if (objFind.Tag != objWeapon.Location)
                                continue;
                            objParent = objFind;
                            break;
                        }
                    }

                    objParent.Nodes.Add(objLoopNode);
                    objParent.Expand();
                }
            }, token).ConfigureAwait(false);

            // Vehicle Gear.
            await GearChildren.ForEachAsync(async objGear =>
            {
                TreeNode objLoopNode = await objGear.CreateTreeNode(cmsVehicleGear, null, token).ConfigureAwait(false);
                if (objLoopNode != null)
                {
                    TreeNode objParent = objNode;
                    if (objGear.Location != null)
                    {
                        foreach (TreeNode objFind in lstChildNodes)
                        {
                            if (objFind.Tag != objGear.Location)
                                continue;
                            objParent = objFind;
                            break;
                        }
                    }

                    objParent.Nodes.Add(objLoopNode);
                    objParent.Expand();
                }
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
                    return !string.IsNullOrEmpty(ParentID)
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return !string.IsNullOrEmpty(ParentID)
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }

        #endregion UI Methods

        /// <summary>
        /// Locate a piece of Cyberware within this vehicle based on a predicate.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        public Cyberware FindVehicleCyberware([NotNull] Func<Cyberware, bool> funcPredicate)
        {
            return FindVehicleCyberware(funcPredicate, out VehicleMod _);
        }

        /// <summary>
        /// Locate a piece of Cyberware within this vehicle based on a predicate.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        /// <param name="objFoundVehicleMod">Vehicle Mod to which the Cyberware belongs.</param>
        public Cyberware FindVehicleCyberware([NotNull] Func<Cyberware, bool> funcPredicate, out VehicleMod objFoundVehicleMod)
        {
            foreach (VehicleMod objMod in Mods)
            {
                Cyberware objReturn = objMod.Cyberware.DeepFirstOrDefault(x => x.Children, funcPredicate);
                if (objReturn != null)
                {
                    objFoundVehicleMod = objMod;
                    return objReturn;
                }
            }

            foreach (WeaponMount objMount in WeaponMounts)
            {
                foreach (VehicleMod objMod in objMount.Mods)
                {
                    Cyberware objReturn = objMod.Cyberware.DeepFirstOrDefault(x => x.Children, funcPredicate);
                    if (objReturn != null)
                    {
                        objFoundVehicleMod = objMod;
                        return objReturn;
                    }
                }
            }

            objFoundVehicleMod = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Cyberware within this vehicle based on a predicate.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<Tuple<Cyberware, VehicleMod>> FindVehicleCyberwareAsync([NotNull] Func<Cyberware, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            Cyberware objReturn = null;
            VehicleMod objReturnMod = null;
            await Mods.ForEachWithBreakAsync(async objMod =>
            {
                objReturn = await objMod.Cyberware.DeepFirstOrDefaultAsync(x => x.Children, funcPredicate, token: token).ConfigureAwait(false);
                if (objReturn == null)
                    return true;
                objReturnMod = objMod;
                return false;
            }, token).ConfigureAwait(false);
            if (objReturn != null)
                return new Tuple<Cyberware, VehicleMod>(objReturn, objReturnMod);

            await WeaponMounts.ForEachWithBreakAsync(async objMount =>
            {
                await objMount.Mods.ForEachWithBreakAsync(async objMod =>
                {
                    objReturn = await objMod.Cyberware.DeepFirstOrDefaultAsync(x => x.Children, funcPredicate, token: token).ConfigureAwait(false);
                    if (objReturn == null)
                        return true;
                    objReturnMod = objMod;
                    return false;
                }, token).ConfigureAwait(false);
                return objReturn == null;
            }, token).ConfigureAwait(false);

            return new Tuple<Cyberware, VehicleMod>(objReturn, objReturnMod);
        }

        /// <summary>
        /// Locate a VehicleMod within this vehicle based on a predicate.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        public VehicleMod FindVehicleMod([NotNull] Func<VehicleMod, bool> funcPredicate)
        {
            return FindVehicleMod(funcPredicate, out WeaponMount _);
        }

        /// <summary>
        /// Locate a VehicleMod within this vehicle based on a predicate.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        /// <param name="objFoundWeaponMount">Weapon Mount that the VehicleMod was found in.</param>
        public VehicleMod FindVehicleMod([NotNull] Func<VehicleMod, bool> funcPredicate, out WeaponMount objFoundWeaponMount)
        {
            VehicleMod objMod = Mods.FirstOrDefault(funcPredicate);
            if (objMod != null)
            {
                objFoundWeaponMount = null;
                return objMod;
            }

            foreach (WeaponMount objMount in WeaponMounts)
            {
                objMod = objMount.Mods.FirstOrDefault(funcPredicate);
                if (objMod != null)
                {
                    objFoundWeaponMount = objMount;
                    return objMod;
                }
            }

            objFoundWeaponMount = null;
            return null;
        }

        /// <summary>
        /// Locate a VehicleMod within this vehicle based on a predicate.
        /// </summary>
        /// <param name="funcPredicate">Predicate to locate the Cyberware.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<Tuple<VehicleMod, WeaponMount>> FindVehicleModAsync([NotNull] Func<VehicleMod, bool> funcPredicate, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            VehicleMod objMod = await Mods.FirstOrDefaultAsync(funcPredicate, token).ConfigureAwait(false);
            if (objMod != null)
                return new Tuple<VehicleMod, WeaponMount>(objMod, null);

            WeaponMount objReturnMount = null;
            await WeaponMounts.ForEachWithBreakAsync(async objMount =>
            {
                objMod = await objMount.Mods.FirstOrDefaultAsync(funcPredicate, token).ConfigureAwait(false);
                if (objMod == null)
                    return true;
                objReturnMount = objMount;
                return false;
            }, token).ConfigureAwait(false);

            return new Tuple<VehicleMod, WeaponMount>(objMod, objReturnMount);
        }

        /// <summary>
        /// Locate a piece of Gear within one of a character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        public Gear FindVehicleGear(string strGuid)
        {
            return FindVehicleGear(strGuid, out WeaponAccessory _, out Cyberware _);
        }

        /// <summary>
        /// Locate a piece of Gear within one of a character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="objFoundWeaponAccessory">Weapon Accessory that the Gear was found in.</param>
        /// <param name="objFoundCyberware">Cyberware that the Gear was found in.</param>
        public Gear FindVehicleGear(string strGuid, out WeaponAccessory objFoundWeaponAccessory, out Cyberware objFoundCyberware)
        {
            if (!string.IsNullOrEmpty(strGuid) && !strGuid.IsEmptyGuid())
            {
                Gear objReturn = GearChildren.DeepFindById(strGuid);
                if (objReturn != null)
                {
                    objFoundWeaponAccessory = null;
                    objFoundCyberware = null;
                    return objReturn;
                }

                // Look for any Gear that might be attached to this Vehicle through Weapon Accessories or Cyberware.
                foreach (VehicleMod objMod in Mods)
                {
                    // Weapon Accessories.
                    objReturn = objMod.Weapons.FindWeaponGear(strGuid, out WeaponAccessory objAccessory);

                    if (objReturn != null)
                    {
                        objFoundWeaponAccessory = objAccessory;
                        objFoundCyberware = null;
                        return objReturn;
                    }

                    // Cyberware.
                    objReturn = objMod.Cyberware.FindCyberwareGear(strGuid, out Cyberware objCyberware);

                    if (objReturn != null)
                    {
                        objFoundWeaponAccessory = null;
                        objFoundCyberware = objCyberware;
                        return objReturn;
                    }
                }
            }

            objFoundWeaponAccessory = null;
            objFoundCyberware = null;
            return null;
        }

        /// <summary>
        /// Locate a piece of Gear within one of a character's Vehicles.
        /// </summary>
        /// <param name="strGuid">InternalId of the Gear to find.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async Task<Tuple<Gear, WeaponAccessory, Cyberware>> FindVehicleGearAsync(string strGuid, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(strGuid) || strGuid.IsEmptyGuid())
                return new Tuple<Gear, WeaponAccessory, Cyberware>(null, null, null);
            Gear objReturn = await GearChildren.DeepFindByIdAsync(strGuid, token: token).ConfigureAwait(false);
            if (objReturn != null)
                return new Tuple<Gear, WeaponAccessory, Cyberware>(objReturn, null, null);

            WeaponAccessory objReturnAccessory = null;
            Cyberware objReturnCyberware = null;
            // Look for any Gear that might be attached to this Vehicle through Weapon Accessories or Cyberware.
            await Mods.ForEachWithBreakAsync(async objMod =>
            {
                // Weapon Accessories.
                (objReturn, objReturnAccessory) = await objMod.Weapons.FindWeaponGearAsync(strGuid, token).ConfigureAwait(false);

                if (objReturn != null)
                    return false;

                // Cyberware.
                (objReturn, objReturnCyberware) = await objMod.Cyberware.FindCyberwareGearAsync(strGuid, token).ConfigureAwait(false);

                return objReturn == null;
            }, token).ConfigureAwait(false);

            return new Tuple<Gear, WeaponAccessory, Cyberware>(objReturn, objReturnAccessory, objReturnCyberware);
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        return Pilot;

                    case "Program Limit":
                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            return Pilot;
                        break;

                    default:
                        return 0;
                }
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    if (ChildrenWithMatrixAttributes.Any())
                    {
                        foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                        {
                            if (strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                            {
                                int intTotalChildrenValue = 0;
                                foreach (IHasMatrixAttributes objChild in ChildrenWithMatrixAttributes)
                                {
                                    if (objChild is Gear objGear && objGear.Equipped ||
                                        objChild is Weapon objWeapon && objWeapon.Equipped)
                                    {
                                        intTotalChildrenValue += objChild.GetBaseMatrixAttribute(strMatrixAttribute);
                                    }
                                }

                                sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                                 intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                            }
                        }
                    }

                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString());
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }

            return !int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn) ? 0 : intReturn;
        }

        public async Task<int> GetBaseMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        return await GetPilotAsync(token).ConfigureAwait(false);

                    case "Program Limit":
                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            return await GetPilotAsync(token).ConfigureAwait(false);
                        break;

                    default:
                        return 0;
                }
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                using (new FetchSafelyFromPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdValue))
                {
                    sbdValue.Append(strExpression);
                    if (ChildrenWithMatrixAttributes.Any())
                    {
                        foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                        {
                            if (strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                            {
                                int intTotalChildrenValue = await ChildrenWithMatrixAttributes.SumAsync(async objChild =>
                                {
                                    if (objChild is Gear objGear && objGear.Equipped ||
                                        objChild is Weapon objWeapon && objWeapon.Equipped)
                                    {
                                        return await objChild.GetBaseMatrixAttributeAsync(strMatrixAttribute, token).ConfigureAwait(false);
                                    }

                                    return 0;
                                }, token).ConfigureAwait(false);

                                sbdValue.Replace("{Children " + strMatrixAttribute + '}',
                                                 intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                            }
                        }
                    }

                    await _objCharacter.AttributeSection.ProcessAttributesInXPathAsync(sbdValue, strExpression, token: token).ConfigureAwait(false);
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    (bool blnIsSuccess, object objProcess)
                        = await CommonFunctions.EvaluateInvariantXPathAsync(sbdValue.ToString(), token).ConfigureAwait(false);
                    return blnIsSuccess ? ((double)objProcess).StandardRound() : 0;
                }
            }

            return !int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn) ? 0 : intReturn;
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = Overclocked == strAttributeName ? 1 : 0;

            string strAttributeNodeName = string.Empty;
            switch (strAttributeName)
            {
                case "Device Rating":
                    strAttributeNodeName = "devicerating";
                    break;

                case "Program Limit":
                    strAttributeNodeName = "programs";
                    break;
            }
            if (!string.IsNullOrEmpty(strAttributeNodeName))
            {
                foreach (VehicleMod objMod in Mods)
                {
                    XmlElement objBonus = objMod.Bonus?[strAttributeNodeName];
                    if (objBonus != null)
                    {
                        intReturn += Convert.ToInt32(objBonus.InnerText, GlobalSettings.InvariantCultureInfo);
                    }
                    objBonus = objMod.WirelessOn ? objMod.WirelessBonus?[strAttributeNodeName] : null;
                    if (objBonus != null)
                    {
                        intReturn += Convert.ToInt32(objBonus.InnerText, GlobalSettings.InvariantCultureInfo);
                    }
                }
            }

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Gear loopGear in GearChildren)
            {
                if (loopGear.Equipped)
                {
                    intReturn += loopGear.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        public async Task<int> GetBonusMatrixAttributeAsync(string strAttributeName, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            int intReturn = await GetOverclockedAsync(token).ConfigureAwait(false) == strAttributeName ? 1 : 0;

            string strAttributeNodeName = string.Empty;
            switch (strAttributeName)
            {
                case "Device Rating":
                    strAttributeNodeName = "devicerating";
                    break;

                case "Program Limit":
                    strAttributeNodeName = "programs";
                    break;
            }

            if (!string.IsNullOrEmpty(strAttributeNodeName))
            {
                intReturn += await Mods.SumAsync(objMod =>
                {
                    int intInnerReturn = 0;
                    XmlElement objBonus = objMod.Bonus?[strAttributeNodeName];
                    if (objBonus != null)
                    {
                        intInnerReturn += Convert.ToInt32(objBonus.InnerText, GlobalSettings.InvariantCultureInfo);
                    }

                    objBonus = objMod.WirelessOn ? objMod.WirelessBonus?[strAttributeNodeName] : null;
                    if (objBonus != null)
                    {
                        intInnerReturn += Convert.ToInt32(objBonus.InnerText, GlobalSettings.InvariantCultureInfo);
                    }

                    return intInnerReturn;
                }, token).ConfigureAwait(false);
            }

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            intReturn += await GearChildren
                .SumAsync(x => x.Equipped, x => x.GetTotalMatrixAttributeAsync(strAttributeName, token), token)
                .ConfigureAwait(false);

            return intReturn;
        }

        #endregion Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicle")))
                return false;

            DeleteVehicle();
            return true;
        }

        public async Task<bool> RemoveAsync(bool blnConfirmDelete = true, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            if (blnConfirmDelete && !await CommonFunctions
                    .ConfirmDeleteAsync(
                        await LanguageManager.GetStringAsync("Message_DeleteVehicle", token: token)
                            .ConfigureAwait(false), token).ConfigureAwait(false))
                return false;

            await DeleteVehicleAsync(token).ConfigureAwait(false);
            return true;
        }

        public bool Sell(decimal decPercentage, bool blnConfirmDelete)
        {
            if (!_objCharacter.Created)
                return Remove(blnConfirmDelete);

            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicle")))
                return false;

            // Create the Expense Log Entry for the sale.
            decimal decAmount = TotalCost * decPercentage;
            decAmount += DeleteVehicle() * decPercentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldVehicle") + ' ' + CurrentDisplayNameShort, ExpenseType.Nuyen, DateTime.Now);
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
                        await LanguageManager.GetStringAsync("Message_DeleteVehicle", token: token)
                            .ConfigureAwait(false),
                        token).ConfigureAwait(false))
                return false;

            // Create the Expense Log Entry for the sale.
            decimal decAmount = await GetTotalCostAsync(token).ConfigureAwait(false) * decPercentage;
            decAmount += await DeleteVehicleAsync(token).ConfigureAwait(false) * decPercentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount,
                await LanguageManager.GetStringAsync("String_ExpenseSoldVehicle", token: token).ConfigureAwait(false) +
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
                    case ClipboardContentType.Gear:
                    {
                        string strClipboardCategory = (await GlobalSettings.GetClipboardAsync(token).ConfigureAwait(false))
                            .SelectSingleNodeAndCacheExpressionAsNavigator("category", token)?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(strClipboardCategory))
                        {
                            XPathNodeIterator xmlAddonCategoryList =
                                (await this.GetNodeXPathAsync(token: token).ConfigureAwait(false))
                                ?.SelectAndCacheExpression("addoncategory", token);
                            return xmlAddonCategoryList?.Count > 0 && xmlAddonCategoryList.Cast<XPathNavigator>()
                                .Any(xmlLoop => xmlLoop.Value == strClipboardCategory);
                        }

                        return false;
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

        public const int MaxWheels = 50;

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (VehicleMod objChild in _lstVehicleMods)
                objChild.Dispose();
            foreach (Gear objChild in _lstGear)
                objChild.Dispose();
            foreach (Weapon objChild in _lstWeapons)
                objChild.Dispose();
            foreach (WeaponMount objChild in _lstWeaponMounts)
                objChild.Dispose();
            foreach (Location objChild in _lstLocations)
                objChild.Dispose();
            DisposeSelf();
        }

        private void DisposeSelf()
        {
            _lstVehicleMods.Dispose();
            _lstGear.Dispose();
            _lstWeapons.Dispose();
            _lstWeaponMounts.Dispose();
            _lstLocations.Dispose();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            foreach (VehicleMod objChild in _lstVehicleMods)
                await objChild.DisposeAsync().ConfigureAwait(false);
            foreach (Gear objChild in _lstGear)
                await objChild.DisposeAsync().ConfigureAwait(false);
            foreach (Weapon objChild in _lstWeapons)
                await objChild.DisposeAsync().ConfigureAwait(false);
            foreach (WeaponMount objChild in _lstWeaponMounts)
                await objChild.DisposeAsync().ConfigureAwait(false);
            foreach (Location objChild in _lstLocations)
                await objChild.DisposeAsync().ConfigureAwait(false);
            await DisposeSelfAsync().ConfigureAwait(false);
        }

        private async ValueTask DisposeSelfAsync()
        {
            await _lstVehicleMods.DisposeAsync().ConfigureAwait(false);
            await _lstGear.DisposeAsync().ConfigureAwait(false);
            await _lstWeapons.DisposeAsync().ConfigureAwait(false);
            await _lstWeaponMounts.DisposeAsync().ConfigureAwait(false);
            await _lstLocations.DisposeAsync().ConfigureAwait(false);
        }
    }
}
