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
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;
using Chummer.Backend.Attributes;
using NLog;
using TreeNode = System.Windows.Forms.TreeNode;
using TreeNodeCollection = System.Windows.Forms.TreeNodeCollection;
using TreeView = System.Windows.Forms.TreeView;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Vehicle.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", null)]
    [DebuggerDisplay("{DisplayName(GlobalSettings.DefaultLanguage)}")]
    public class Vehicle : IHasInternalId, IHasName, IHasXmlNode, IHasMatrixAttributes, IHasNotes, ICanSell, IHasCustomName, IHasPhysicalConditionMonitor, IHasLocation, IHasSource, ICanSort, IHasGear, IHasStolenProperty, ICanPaste, ICanBlackMarketDiscount
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
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
        private readonly TaggedObservableCollection<VehicleMod> _lstVehicleMods = new TaggedObservableCollection<VehicleMod>();
        private readonly TaggedObservableCollection<Gear> _lstGear = new TaggedObservableCollection<Gear>();
        private readonly TaggedObservableCollection<Weapon> _lstWeapons = new TaggedObservableCollection<Weapon>();
        private readonly TaggedObservableCollection<WeaponMount> _lstWeaponMounts = new TaggedObservableCollection<WeaponMount>();
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private Location _objLocation;
        private readonly TaggedObservableCollection<Location> _lstLocations = new TaggedObservableCollection<Location>();
        private bool _blnDiscountCost;
        private bool _blnDealerConnectionDiscount;
        private string _strParentID = string.Empty;

        private readonly Character _objCharacter;

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

            _lstGear.AddTaggedCollectionChanged(this, MatrixAttributeChildrenOnCollectionChanged);
            _lstWeapons.AddTaggedCollectionChanged(this, MatrixAttributeChildrenOnCollectionChanged);
            _lstVehicleMods.AddTaggedCollectionChanged(this, LstVehicleModsOnCollectionChanged);
        }

        private void LstVehicleModsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_objCharacter.IsAI && this == _objCharacter.HomeNode && e.Action != NotifyCollectionChangedAction.Move)
                _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
        }

        private void MatrixAttributeChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Move)
                this.RefreshMatrixAttributeArray();
        }

        /// Create a Vehicle from an XmlNode.
        /// <param name="objXmlVehicle">XmlNode of the Vehicle to create.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        public void Create(XmlNode objXmlVehicle, bool blnCreateChildren = true)
        {
            if (!objXmlVehicle.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlVehicle });
                Utils.BreakIfDebug();
            }
            else
                _objCachedMyXmlNode = null;
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

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlVehicle.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            _strCost = objXmlVehicle["cost"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(_strCost) && _strCost.StartsWith("Variable(", StringComparison.Ordinal))
            {
                // Check for a Variable Cost.
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
                    using (frmSelectNumber frmPickNumber = new frmSelectNumber(_objCharacter.Settings.MaxNuyenDecimals)
                    {
                        Minimum = decMin,
                        Maximum = decMax,
                        Description = string.Format(GlobalSettings.CultureInfo, LanguageManager.GetString("String_SelectVariableCost"), DisplayNameShort(GlobalSettings.Language)),
                        AllowCancel = false
                    })
                    {
                        if (frmPickNumber.ShowDialog(Program.MainForm) == DialogResult.Cancel)
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                        _strCost = frmPickNumber.SelectedValue.ToString(GlobalSettings.InvariantCultureInfo);
                    }
                }
            }

            DealerConnectionDiscount = DoesDealerConnectionCurrentlyApply();

            objXmlVehicle.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlVehicle.TryGetStringFieldQuickly("page", ref _strPage);

            if (string.IsNullOrEmpty(Notes))
            {
                Notes = CommonFunctions.GetBookNotes(objXmlVehicle, Name, CurrentDisplayName, Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter);
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
                XmlNode xmlMods = objXmlVehicle["mods"];
                if (xmlMods != null)
                {
                    XmlDocument objXmlDocument = _objCharacter.LoadData("vehicles.xml");

                    using (XmlNodeList objXmlModList = xmlMods.SelectNodes("name"))
                    {
                        if (objXmlModList != null)
                        {
                            foreach (XmlNode objXmlVehicleMod in objXmlModList)
                            {
                                XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = " + objXmlVehicleMod.InnerText.CleanXPath() + "]");
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
                                    objMod.Create(objXmlMod, intRating, this, 0, strForcedValue);

                                    _lstVehicleMods.Add(objMod);
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
                                XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = " + strName.CleanXPath() + "]");
                                if (objXmlMod != null)
                                {
                                    VehicleMod objMod = new VehicleMod(_objCharacter)
                                    {
                                        IncludedInVehicle = true
                                    };
                                    string strForcedValue = objXmlVehicleMod.SelectSingleNode("name/@select")?.Value ?? string.Empty;
                                    if (!int.TryParse(objXmlVehicleMod["rating"]?.InnerText, out int intRating))
                                        intRating = 0;

                                    objMod.Extra = strForcedValue;
                                    objMod.Create(objXmlMod, intRating, this, 0, strForcedValue);

                                    XmlNode xmlSubsystemsNode = objXmlVehicleMod["subsystems"];
                                    if (xmlSubsystemsNode != null)
                                    {
                                        // Load Cyberware subsystems first
                                        using (XmlNodeList objXmlSubSystemNameList = xmlSubsystemsNode.SelectNodes("cyberware"))
                                        {
                                            if (objXmlSubSystemNameList?.Count > 0)
                                            {
                                                XmlDocument objXmlWareDocument = _objCharacter.LoadData("cyberware.xml");
                                                foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                                                {
                                                    string strSubsystemName = objXmlSubsystemNode["name"]?.InnerText;
                                                    if (string.IsNullOrEmpty(strSubsystemName))
                                                        continue;
                                                    XmlNode objXmlSubsystem = objXmlWareDocument.SelectSingleNode(
                                                        "/chummer/cyberwares/cyberware[name = " + strSubsystemName.CleanXPath() + "]");
                                                    if (objXmlSubsystem == null) continue;
                                                    Cyberware objSubsystem = new Cyberware(_objCharacter);
                                                    int.TryParse(objXmlSubsystemNode["rating"]?.InnerText, NumberStyles.Any,
                                                        GlobalSettings.InvariantCultureInfo, out int intSubSystemRating);
                                                    objSubsystem.Create(objXmlSubsystem, new Grade(_objCharacter, Improvement.ImprovementSource.Cyberware), Improvement.ImprovementSource.Cyberware,
                                                        intSubSystemRating, _lstWeapons, _objCharacter.Vehicles, false, true,
                                                        objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty);
                                                    objSubsystem.ParentID = InternalId;
                                                    objSubsystem.Cost = "0";

                                                    objMod.Cyberware.Add(objSubsystem);
                                                }
                                            }
                                        }
                                    }
                                    _lstVehicleMods.Add(objMod);
                                }
                            }
                        }
                    }

                    XmlNode objAddSlotsNode = objXmlVehicle.SelectSingleNode("mods/addslots");
                    if (objAddSlotsNode != null && !int.TryParse(objAddSlotsNode.InnerText, out _intAddSlots))
                        _intAddSlots = 0;
                }

                // If there are any Weapon Mounts that come with the Vehicle, add them.
                XmlNode xmlWeaponMounts = objXmlVehicle["weaponmounts"];
                if (xmlWeaponMounts != null)
                {
                    foreach (XmlNode objXmlVehicleMod in xmlWeaponMounts.SelectNodes("weaponmount"))
                    {
                        WeaponMount objWeaponMount = new WeaponMount(_objCharacter, this);
                        objWeaponMount.CreateByName(objXmlVehicleMod);
                        objWeaponMount.IncludedInVehicle = true;
                        WeaponMounts.Add(objWeaponMount);
                    }
                }

                List<Weapon> lstWeapons = new List<Weapon>(1);

                // If there is any Gear that comes with the Vehicle, add them.
                XmlNode xmlGears = objXmlVehicle["gears"];
                if (xmlGears != null)
                {
                    XmlDocument objXmlDocument = _objCharacter.LoadData("gear.xml");

                    using (XmlNodeList objXmlGearList = xmlGears.SelectNodes("gear"))
                    {
                        if (objXmlGearList?.Count > 0)
                        {
                            foreach (XmlNode objXmlVehicleGear in objXmlGearList)
                            {
                                Gear objGear = new Gear(_objCharacter);
                                if (objGear.CreateFromNode(objXmlDocument, objXmlVehicleGear, lstWeapons))
                                {
                                    objGear.Parent = this;
                                    objGear.ParentID = InternalId;
                                    GearChildren.Add(objGear);
                                    foreach (Weapon objWeapon in lstWeapons)
                                    {
                                        objWeapon.ParentVehicle = this;
                                        Weapons.Add(objWeapon);
                                    }
                                }
                            }
                        }
                    }
                }

                // If there are any Weapons that come with the Vehicle, add them.
                XmlNode xmlWeapons = objXmlVehicle["weapons"];
                if (xmlWeapons != null)
                {
                    XmlDocument objXmlWeaponDocument = _objCharacter.LoadData("weapons.xml");

                    foreach (XmlNode objXmlWeapon in xmlWeapons.SelectNodes("weapon"))
                    {
                        string strWeaponName = objXmlWeapon["name"]?.InnerText;
                        if (string.IsNullOrEmpty(strWeaponName))
                            continue;
                        bool blnAttached = false;
                        Weapon objWeapon = new Weapon(_objCharacter);

                        List<Weapon> objSubWeapons = new List<Weapon>(1);
                        XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = " + strWeaponName.CleanXPath() + "]");
                        objWeapon.ParentVehicle = this;
                        objWeapon.Create(objXmlWeaponNode, objSubWeapons);
                        objWeapon.ParentID = InternalId;
                        objWeapon.Cost = "0";

                        // Find the first free Weapon Mount in the Vehicle.
                        foreach (WeaponMount objWeaponMount in _lstWeaponMounts)
                        {
                            if (objWeaponMount.IsWeaponsFull)
                                continue;
                            if (!objWeaponMount.AllowedWeaponCategories.Contains(objWeapon.SizeCategory) &&
                                !objWeaponMount.AllowedWeapons.Contains(objWeapon.Name) &&
                                !string.IsNullOrEmpty(objWeaponMount.AllowedWeaponCategories))
                                continue;
                            objWeaponMount.Weapons.Add(objWeapon);
                            blnAttached = true;
                            foreach (Weapon objSubWeapon in objSubWeapons)
                                objWeaponMount.Weapons.Add(objSubWeapon);
                            break;
                        }

                        // If a free Weapon Mount could not be found, just attach it to the first one found and let the player deal with it.
                        if (!blnAttached)
                        {
                            foreach (VehicleMod objMod in _lstVehicleMods)
                            {
                                if (objMod.Name.Contains("Weapon Mount") || !string.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.SizeCategory) && objMod.Weapons.Count == 0)
                                {
                                    objMod.Weapons.Add(objWeapon);
                                    foreach (Weapon objSubWeapon in objSubWeapons)
                                        objMod.Weapons.Add(objSubWeapon);
                                    break;
                                }
                            }
                            if (!blnAttached)
                            {
                                foreach (VehicleMod objMod in _lstVehicleMods)
                                {
                                    if (objMod.Name.Contains("Weapon Mount") || !string.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.SizeCategory))
                                    {
                                        objMod.Weapons.Add(objWeapon);
                                        foreach (Weapon objSubWeapon in objSubWeapons)
                                            objMod.Weapons.Add(objSubWeapon);
                                        break;
                                    }
                                }
                            }
                        }

                        // Look for Weapon Accessories.
                        XmlNode xmlAccessories = objXmlWeapon["accessories"];
                        if (xmlAccessories != null)
                        {
                            foreach (XmlNode objXmlAccessory in xmlAccessories.SelectNodes("accessory"))
                            {
                                string strAccessoryName = objXmlWeapon["name"]?.InnerText;
                                if (string.IsNullOrEmpty(strAccessoryName))
                                    continue;
                                XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[name = " + strAccessoryName.CleanXPath() + "]");
                                WeaponAccessory objMod = new WeaponAccessory(_objCharacter);
                                string strMount = "Internal";
                                objXmlAccessory.TryGetStringFieldQuickly("mount", ref strMount);
                                string strExtraMount = "None";
                                objXmlAccessory.TryGetStringFieldQuickly("extramount", ref strExtraMount);
                                objMod.Create(objXmlAccessoryNode, new Tuple<string, string>(strMount, strExtraMount), 0, false, blnCreateChildren);

                                objMod.Cost = "0";

                                objWeapon.WeaponAccessories.Add(objMod);
                            }
                        }
                    }
                }

                foreach (Weapon objWeapon in lstWeapons)
                {
                    objWeapon.ParentVehicle = this;
                    Weapons.Add(objWeapon);
                }
            }
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = new SourceString(Source,
                        DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo,
                        _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
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
            objWriter.WriteElementString("devicerating", Pilot.ToString(GlobalSettings.InvariantCultureInfo));
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
            objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("vehiclename", _strVehicleName);
            objWriter.WriteStartElement("mods");
            foreach (VehicleMod objMod in _lstVehicleMods)
                objMod.Save(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weaponmounts");
            foreach (WeaponMount objWeaponMount in _lstWeaponMounts)
                objWeaponMount.Save(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
            {
                objGear.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstWeapons)
                objWeapon.Save(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("dealerconnection", _blnDealerConnectionDiscount.ToString(GlobalSettings.InvariantCultureInfo));
            if (_lstLocations.Count > 0)
            {
                // <locations>
                objWriter.WriteStartElement("locations");
                foreach (Location objLocation in _lstLocations)
                {
                    objLocation.Save(objWriter);
                }
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
        /// <param name="blnCopy">Whether or not we are copying an existing vehicle.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (objNode == null)
                return;
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalSettings.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
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
                        if (Name.StartsWith("Proteus Krake")
                            && !blnKrakePassDone
                            && _objCharacter.LastSavedVersion < new Version(5, 213, 28)
                            && (!nodChild.TryGetGuidFieldQuickly("sourceid", ref guidDummy) || guidDummy == Guid.Empty))
                        {
                            blnKrakePassDone = true;
                            // If there are any Weapon Mounts that come with the Vehicle, add them.
                            XmlNode xmlVehicleDataNode = GetNode();
                            if (xmlVehicleDataNode != null)
                            {
                                XmlNode xmlDataNodesForMissingKrakeStuff = xmlVehicleDataNode["weaponmounts"];
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

                                        List<Weapon> objSubWeapons = new List<Weapon>(1);
                                        XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = " + strWeaponName.CleanXPath() + "]");
                                        objWeapon.ParentVehicle = this;
                                        objWeapon.Create(objXmlWeaponNode, objSubWeapons);
                                        objWeapon.ParentID = InternalId;
                                        objWeapon.Cost = "0";

                                        // Find the first free Weapon Mount in the Vehicle.
                                        foreach (WeaponMount objWeaponMount in WeaponMounts)
                                        {
                                            if (objWeaponMount.IsWeaponsFull)
                                                continue;
                                            if (!objWeaponMount.AllowedWeaponCategories.Contains(objWeapon.SizeCategory) &&
                                                !objWeaponMount.AllowedWeapons.Contains(objWeapon.Name) &&
                                                !string.IsNullOrEmpty(objWeaponMount.AllowedWeaponCategories))
                                                continue;
                                            objWeaponMount.Weapons.Add(objWeapon);
                                            blnAttached = true;
                                            foreach (Weapon objSubWeapon in objSubWeapons)
                                                objWeaponMount.Weapons.Add(objSubWeapon);
                                            break;
                                        }

                                        // If a free Weapon Mount could not be found, just attach it to the first one found and let the player deal with it.
                                        if (!blnAttached)
                                        {
                                            foreach (VehicleMod objMod in _lstVehicleMods)
                                            {
                                                if (objMod.Name.Contains("Weapon Mount") || !string.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.SizeCategory) && objMod.Weapons.Count == 0)
                                                {
                                                    objMod.Weapons.Add(objWeapon);
                                                    foreach (Weapon objSubWeapon in objSubWeapons)
                                                        objMod.Weapons.Add(objSubWeapon);
                                                    break;
                                                }
                                            }
                                            if (!blnAttached)
                                            {
                                                foreach (VehicleMod objMod in _lstVehicleMods)
                                                {
                                                    if (objMod.Name.Contains("Weapon Mount") || !string.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.SizeCategory))
                                                    {
                                                        objMod.Weapons.Add(objWeapon);
                                                        foreach (Weapon objSubWeapon in objSubWeapons)
                                                            objMod.Weapons.Add(objSubWeapon);
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        // Look for Weapon Accessories.
                                        XmlNode xmlAccessories = objXmlWeapon["accessories"];
                                        if (xmlAccessories != null)
                                        {
                                            foreach (XmlNode objXmlAccessory in xmlAccessories.SelectNodes("accessory"))
                                            {
                                                string strAccessoryName = objXmlWeapon["name"]?.InnerText;
                                                if (string.IsNullOrEmpty(strAccessoryName))
                                                    continue;
                                                XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[name = " + strAccessoryName.CleanXPath() + "]");
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
                GetNode()?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objNode.TryGetStringFieldQuickly("programlimit", ref _strProgramLimit))
                GetNode()?.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
            objNode.TryGetStringFieldQuickly("overclocked", ref _strOverclocked);
            if (!objNode.TryGetStringFieldQuickly("attack", ref _strAttack))
                GetNode()?.TryGetStringFieldQuickly("attack", ref _strAttack);
            if (!objNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze))
                GetNode()?.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
            if (!objNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing))
                GetNode()?.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("firewall", ref _strFirewall))
                GetNode()?.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            if (!objNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
                GetNode()?.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray);
            if (!objNode.TryGetStringFieldQuickly("modattack", ref _strModAttack))
                GetNode()?.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            if (!objNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze))
                GetNode()?.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            if (!objNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing))
                GetNode()?.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall))
                GetNode()?.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            if (!objNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray))
                GetNode()?.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

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
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("vehicle");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("isdrone", IsDrone.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("handling", TotalHandling);
            objWriter.WriteElementString("accel", TotalAccel);
            objWriter.WriteElementString("speed", TotalSpeed);
            objWriter.WriteElementString("pilot", Pilot.ToString(objCulture));
            objWriter.WriteElementString("body", TotalBody.ToString(objCulture));
            objWriter.WriteElementString("armor", TotalArmor.ToString(objCulture));
            objWriter.WriteElementString("seats", TotalSeats.ToString(objCulture));
            objWriter.WriteElementString("sensor", CalculatedSensor.ToString(objCulture));
            objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Settings.NuyenFormat, objCulture));
            objWriter.WriteElementString("source", _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("physicalcm", PhysicalCM.ToString(objCulture));
            objWriter.WriteElementString("physicalcmfilled", PhysicalCMFilled.ToString(objCulture));
            objWriter.WriteElementString("vehiclename", CustomName);
            objWriter.WriteElementString("maneuver", Maneuver.ToString(objCulture));
            objWriter.WriteElementString("location", Location?.DisplayName(strLanguageToPrint));

            objWriter.WriteElementString("attack", this.GetTotalMatrixAttribute("Attack").ToString(objCulture));
            objWriter.WriteElementString("sleaze", this.GetTotalMatrixAttribute("Sleaze").ToString(objCulture));
            objWriter.WriteElementString("dataprocessing", this.GetTotalMatrixAttribute("Data Processing").ToString(objCulture));
            objWriter.WriteElementString("firewall", this.GetTotalMatrixAttribute("Firewall").ToString(objCulture));
            objWriter.WriteElementString("devicerating", this.GetTotalMatrixAttribute("Device Rating").ToString(objCulture));
            objWriter.WriteElementString("programlimit", this.GetTotalMatrixAttribute("Program Limit").ToString(objCulture));
            objWriter.WriteElementString("iscommlink", IsCommlink.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("isprogram", IsProgram.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcm", MatrixCM.ToString(objCulture));
            objWriter.WriteElementString("matrixcmfilled", MatrixCMFilled.ToString(objCulture));

            objWriter.WriteStartElement("mods");
            foreach (VehicleMod objMod in Mods)
                objMod.Print(objWriter, objCulture, strLanguageToPrint);
            foreach (WeaponMount objMount in WeaponMounts)
                objMount.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in GearChildren)
                objGear.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in Weapons)
                objWeapon.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();
            if (GlobalSettings.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
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

            return _objCharacter.LoadDataXPath("vehicles.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
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
                int intReturn = _intPilot;
                foreach (VehicleMod objMod in Mods)
                {
                    if (!objMod.IncludedInVehicle && objMod.Equipped)
                    {
                        string strBonusPilot = objMod.Bonus?["pilot"]?.InnerText;
                        // Set the Vehicle's Pilot to the Modification's bonus.
                        if (!string.IsNullOrEmpty(strBonusPilot))
                        {
                            if (int.TryParse(strBonusPilot.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intTest)
                                && intTest > intReturn)
                                intReturn = intTest;
                        }
                        else if (objMod.WirelessOn)
                        {
                            strBonusPilot = objMod.WirelessBonus?["pilot"]?.InnerText;
                            if (!string.IsNullOrEmpty(strBonusPilot) && int.TryParse(strBonusPilot.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intTest) && intTest > intReturn)
                            {
                                intReturn = intTest;
                            }
                        }
                    }
                }
                return intReturn;
            }
            set => _intPilot = value;
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
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
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

        public TaggedObservableCollection<WeaponMount> WeaponMounts => _lstWeaponMounts;

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalSettings.CultureInfo, GlobalSettings.Language);

        /// <summary>
        /// Calculated Availability of the Vehicle.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
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
        /// Number of Slots the Vehicle has for Modifications.
        /// </summary>
        public int Slots
        {
            get
            {
                // A Vehicle has 4 or BODY slots, whichever is higher.
                if (TotalBody > 4)
                    return TotalBody + _intAddSlots;
                return 4 + _intAddSlots;
            }
        }

        /// <summary>
        /// Calculate the Vehicle's Sensor Rating based on the items within its Sensor.
        /// </summary>
        public int CalculatedSensor
        {
            get
            {
                int intSensor = _intSensor;
                foreach (VehicleMod objMod in Mods)
                {
                    if (!objMod.IncludedInVehicle && objMod.Equipped)
                    {
                        string strSensor = objMod.Bonus?["sensor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSensor) && int.TryParse(strSensor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).FastEscape('+'), out int intTemp))
                        {
                            intSensor = Math.Max(intTemp, intSensor);
                        }

                        if (objMod.WirelessOn)
                        {
                            strSensor = objMod.WirelessBonus?["sensor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strSensor) && int.TryParse(strSensor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).FastEscape('+'), out int intTemp2))
                            {
                                intSensor = Math.Max(intTemp2, intSensor);
                            }
                        }
                    }
                }

                // Step through all the Gear looking for the Sensor Array that was built it. Set the rating to the current Sensor value.
                // The display value of this gets updated by UpdateSensor when RefreshSelectedVehicle gets called.
                Gear objGear = GearChildren.FirstOrDefault(x => x.Category == "Sensors" && x.Name == "Sensor Array" && x.IncludedInParent);
                if (objGear != null)
                    objGear.Rating = Math.Max(intSensor, 0);

                return intSensor;
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

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// Display name.
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

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
        /// Locations.
        /// </summary>
        public TaggedObservableCollection<Location> Locations => _lstLocations;

        /// <summary>
        /// Whether or not the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
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
        /// Whether or not the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
        /// </summary>
        public bool DealerConnectionDiscount
        {
            get => _blnDealerConnectionDiscount;
            set => _blnDealerConnectionDiscount = value;
        }

        /// <summary>
        /// Check whether or not a vehicle's cost should be discounted by 10% through the Dealer Connection quality on a character.
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
                                                                  .Any(x => x.UniqueName == strUniqueToSearchFor);
        }

        /// <summary>
        /// Check whether or not a vehicle's cost should be discounted by 10% through the Dealer Connection quality on a character.
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
                return Mods.Where(objMod => !objMod.IncludedInVehicle && objMod.Equipped)
                           .Sum(objMod => objMod.CalculatedSlots)
                       + WeaponMounts.Where(wm => !wm.IncludedInVehicle && wm.Equipped)
                                     .Sum(wm => wm.CalculatedSlots);
            }
        }

        /// <summary>
        /// Total Number of Slots the Drone has for Modifications. (Rigger 5)
        /// </summary>
        public int DroneModSlots
        {
            get
            {
                int intDroneModSlots = _intDroneModSlots;
                bool blnDowngraded = false;
                foreach (VehicleMod objMod in Mods)
                {
                    // Mods that are included with a Vehicle by default do not count toward the Slots used.
                    if (!objMod.IncludedInVehicle && objMod.Equipped && objMod.CalculatedSlots < 0)
                    {
                        //You receive only one additional Mod Point from Downgrades
                        if (objMod.Downgrade)
                        {
                            if (!blnDowngraded)
                            {
                                intDroneModSlots -= objMod.CalculatedSlots;
                                blnDowngraded = true;
                            }
                        }
                        else
                        {
                            intDroneModSlots -= objMod.CalculatedSlots;
                        }
                    }
                }

                return intDroneModSlots;
            }
        }

        /// <summary>
        /// The number of Slots on the Drone that are used by Mods.
        /// </summary>
        public int DroneModSlotsUsed
        {
            get
            {
                int intModSlotsUsed = 0;

                bool blnHandling = false;
                bool blnSpeed = false;
                bool blnAccel = false;
                bool blnArmor = false;
                bool blnSensor = false;

                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        continue;
                    int intActualSlots = objMod.CalculatedSlots;
                    if (intActualSlots > 0)
                    {
                        switch (objMod.Category)
                        {
                            case "Handling":
                                intActualSlots -= _intHandling;
                                if (!blnHandling)
                                {
                                    blnHandling = true;
                                    --intActualSlots;
                                }
                                break;

                            case "Speed":
                                intActualSlots -= _intSpeed;
                                if (!blnSpeed)
                                {
                                    blnSpeed = true;
                                    --intActualSlots;
                                }
                                break;

                            case "Acceleration":
                                intActualSlots -= _intAccel;
                                if (!blnAccel)
                                {
                                    blnAccel = true;
                                    --intActualSlots;
                                }
                                break;

                            case "Armor":
                                int intThird = (objMod.Rating - _intArmor + 2) / 3;

                                if (!blnArmor)
                                {
                                    blnArmor = true;
                                    intActualSlots = intThird - 1;
                                }
                                else
                                {
                                    intActualSlots = intThird;
                                }
                                break;

                            case "Sensor":
                                intActualSlots -= _intSensor;
                                if (!blnSensor)
                                {
                                    blnSensor = true;
                                    --intActualSlots;
                                }
                                break;
                        }

                        if (intActualSlots < 0)
                            intActualSlots = 0;

                        intModSlotsUsed += intActualSlots;
                    }
                }
                intModSlotsUsed += WeaponMounts.Where(wm => !wm.IncludedInVehicle && wm.Equipped).Sum(wm => wm.CalculatedSlots);
                return intModSlotsUsed;
            }
        }

        /// <summary>
        /// Total cost of the Vehicle including all after-market Modification.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decCost = OwnCost;

                foreach (VehicleMod objMod in Mods)
                {
                    // Do not include the price of Mods that are part of the base configuration.
                    if (!objMod.IncludedInVehicle)
                    {
                        decCost += objMod.TotalCost;
                    }
                    else
                    {
                        // If the Mod is a part of the base config, check the items attached to it since their cost still counts.
                        decCost += objMod.Weapons.Sum(objWeapon => objWeapon.TotalCost);
                        decCost += objMod.Cyberware.Sum(objCyberware => objCyberware.CurrentTotalCost);
                    }
                }
                decCost += WeaponMounts.Sum(wm => wm.TotalCost);
                decCost += GearChildren.Sum(objGear => objGear.TotalCost);

                return decCost;
            }
        }

        /// <summary>
        /// Total cost of the Vehicle including all after-market Modification.
        /// </summary>
        public decimal StolenTotalCost
        {
            get
            {
                decimal decCost = 0;
                if (Stolen) decCost += OwnCost;

                foreach (VehicleMod objMod in Mods)
                {
                    // Do not include the price of Mods that are part of the base configureation.
                    if (!objMod.IncludedInVehicle)
                    {
                        decCost += objMod.StolenTotalCost;
                    }
                    else
                    {
                        // If the Mod is a part of the base config, check the items attached to it since their cost still counts.
                        decCost += objMod.Weapons.Where(objGear => objGear.Stolen).Sum(objWeapon => objWeapon.StolenTotalCost);
                        decCost += objMod.Cyberware.Where(objGear => objGear.Stolen).Sum(objCyberware => objCyberware.StolenTotalCost);
                    }
                }
                decCost += WeaponMounts.Where(objGear => objGear.Stolen).Sum(wm => wm.StolenTotalCost);
                decCost += GearChildren.Where(objGear => objGear.Stolen).Sum(objGear => objGear.StolenTotalCost);

                return decCost;
            }
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
                    // Keeping enumerations separate reduces heap allocations
                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList)
                    {
                        sbdCost.CheapReplace(strCost, objLoopAttribute.Abbrev,
                                             () => objLoopAttribute.TotalValue.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                        sbdCost.CheapReplace(strCost, objLoopAttribute.Abbrev + "Base",
                                             () => objLoopAttribute.TotalBase.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                    }

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.SpecialAttributeList)
                    {
                        sbdCost.CheapReplace(strCost, objLoopAttribute.Abbrev,
                                             () => objLoopAttribute.TotalValue.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                        sbdCost.CheapReplace(strCost, objLoopAttribute.Abbrev + "Base",
                                             () => objLoopAttribute.TotalBase.ToString(
                                                 GlobalSettings.InvariantCultureInfo));
                    }

                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdCost.ToString(), out bool blnIsSuccess);
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
        /// Total Seats of the Vehicle including Modifications.
        /// </summary>
        public int TotalSeats
        {
            get
            {
                char chrFirstCharacter;
                // First check for mods that overwrite the seat value
                int intTotalSeats = Seats;
                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        continue;
                    string strBonusSeats = objMod.WirelessOn ? objMod.WirelessBonus?["seats"]?.InnerText ?? objMod.Bonus?["seats"]?.InnerText : objMod.Bonus?["seats"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonusSeats))
                    {
                        chrFirstCharacter = strBonusSeats[0];
                        if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                        {
                            intTotalSeats = Math.Max(Convert.ToInt32(strBonusSeats.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo), intTotalSeats);
                        }
                    }
                }

                // Then check for mods that modify the seat value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSeats = 0;
                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        continue;
                    string strBonusSeats = objMod.Bonus?["seats"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBonusSeats))
                    {
                        chrFirstCharacter = strBonusSeats[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing seat number, evaluate the expression.
                            object objProcess = CommonFunctions.EvaluateInvariantXPath(strBonusSeats.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("Seats", intTotalSeats.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                            if (blnIsSuccess)
                                intTotalBonusSeats += ((double)objProcess).StandardRound();
                        }
                    }

                    if (objMod.WirelessOn)
                    {
                        strBonusSeats = objMod.WirelessBonus?["seats"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBonusSeats))
                        {
                            chrFirstCharacter = strBonusSeats[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing seat number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strBonusSeats.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("Seats", intTotalSeats.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusSeats += ((double)objProcess).StandardRound();
                            }
                        }
                    }
                }

                return intTotalSeats + intTotalBonusSeats;
            }
        }

        /// <summary>
        /// Total Speed of the Vehicle including Modifications.
        /// </summary>
        public string TotalSpeed
        {
            get
            {
                char chrFirstCharacter;
                int intTotalSpeed = Speed;
                int intBaseOffroadSpeed = OffroadSpeed;
                int intTotalArmor = 0;

                // First check for mods that overwrite the speed value or add to armor
                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        continue;
                    if (objMod.Bonus != null)
                    {
                        string strSpeed = objMod.Bonus["speed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intTotalSpeed = Math.Max(intTotalSpeed, Convert.ToInt32(strSpeed.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        strSpeed = objMod.Bonus["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadSpeed = Math.Max(intBaseOffroadSpeed, Convert.ToInt32(strSpeed.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            string strArmor = objMod.Bonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        string strSpeed = objMod.WirelessBonus["speed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intTotalSpeed = Math.Max(intTotalSpeed, Convert.ToInt32(strSpeed.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        strSpeed = objMod.WirelessBonus["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadSpeed = Math.Max(intBaseOffroadSpeed, Convert.ToInt32(strSpeed.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            string strArmor = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo);
                            }
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
                        string strSpeed = objMod.Bonus["speed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing speed number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strSpeed.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("Speed", intTotalSpeed.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusSpeed += ((double)objProcess).StandardRound();
                            }
                        }
                        strSpeed = objMod.Bonus["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing speed number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strSpeed.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("OffroadSpeed", intBaseOffroadSpeed.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusOffroadSpeed += ((double)objProcess).StandardRound();
                            }
                        }
                    }
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        string strSpeed = objMod.WirelessBonus["speed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing speed number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strSpeed.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("Speed", intTotalSpeed.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusSpeed += ((double)objProcess).StandardRound();
                            }
                        }
                        strSpeed = objMod.WirelessBonus["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing speed number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strSpeed.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("OffroadSpeed", intBaseOffroadSpeed.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusOffroadSpeed += ((double)objProcess).StandardRound();
                            }
                        }
                    }
                }

                // Reduce speed of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 3, 0);

                if (Speed != OffroadSpeed || intTotalSpeed + intTotalBonusSpeed != intBaseOffroadSpeed + intTotalBonusOffroadSpeed)
                {
                    return (intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString(GlobalSettings.InvariantCultureInfo) + '/' + (intBaseOffroadSpeed + intTotalBonusOffroadSpeed - intPenalty).ToString(GlobalSettings.InvariantCultureInfo);
                }

                return (intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString(GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Total Accel of the Vehicle including Modifications.
        /// </summary>
        public string TotalAccel
        {
            get
            {
                char chrFirstCharacter;
                int intTotalAccel = Accel;
                int intBaseOffroadAccel = OffroadAccel;
                int intTotalArmor = 0;

                // First check for mods that overwrite the accel value or add to armor
                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        continue;
                    if (objMod.Bonus != null)
                    {
                        string strAccel = objMod.Bonus["accel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intTotalAccel = Math.Max(intTotalAccel, Convert.ToInt32(strAccel.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        strAccel = objMod.Bonus["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadAccel = Math.Max(intBaseOffroadAccel, Convert.ToInt32(strAccel.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            string strArmor = objMod.Bonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        string strAccel = objMod.WirelessBonus["accel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intTotalAccel = Math.Max(intTotalAccel, Convert.ToInt32(strAccel.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        strAccel = objMod.WirelessBonus["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadAccel = Math.Max(intBaseOffroadAccel, Convert.ToInt32(strAccel.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            string strArmor = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo);
                            }
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
                        string strAccel = objMod.Bonus["accel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strAccel.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("Accel", intTotalAccel.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusAccel += ((double)objProcess).StandardRound();
                            }
                        }
                        strAccel = objMod.Bonus["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strAccel.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("OffroadAccel", intBaseOffroadAccel.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusOffroadAccel += ((double)objProcess).StandardRound();
                            }
                        }
                    }
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        string strAccel = objMod.WirelessBonus["accel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strAccel.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("Accel", intTotalAccel.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusAccel += ((double)objProcess).StandardRound();
                            }
                        }
                        strAccel = objMod.WirelessBonus["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strAccel.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("OffroadAccel", intBaseOffroadAccel.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusOffroadAccel += ((double)objProcess).StandardRound();
                            }
                        }
                    }
                }

                // Reduce acceleration of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 6, 0);

                if (Accel != OffroadAccel || intTotalAccel + intTotalBonusAccel != intBaseOffroadAccel + intTotalBonusOffroadAccel)
                {
                    return (intTotalAccel + intTotalBonusAccel - intPenalty).ToString(GlobalSettings.InvariantCultureInfo) + '/' + (intBaseOffroadAccel + intTotalBonusOffroadAccel - intPenalty).ToString(GlobalSettings.InvariantCultureInfo);
                }

                return (intTotalAccel + intTotalBonusAccel - intPenalty).ToString(GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Total Body of the Vehicle including Modifications.
        /// </summary>
        public int TotalBody
        {
            get
            {
                int intBody = _intBody;

                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        continue;
                    // Add the Modification's Body to the Vehicle's base Body.
                    string strBodyElement = objMod.Bonus?["body"]?.InnerText;
                    if (!string.IsNullOrEmpty(strBodyElement))
                    {
                        strBodyElement = strBodyElement.TrimStart('+');
                        if (strBodyElement.Contains("Rating"))
                        {
                            // If the cost is determined by the Rating, evaluate the expression.
                            object objProcess = CommonFunctions.EvaluateInvariantXPath(strBodyElement.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                            if (blnIsSuccess)
                                intBody += ((double)objProcess).StandardRound();
                        }
                        else
                        {
                            intBody += Convert.ToInt32(strBodyElement, GlobalSettings.InvariantCultureInfo);
                        }
                    }
                    if (objMod.WirelessOn)
                    {
                        strBodyElement = objMod.WirelessBonus?["body"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBodyElement))
                        {
                            strBodyElement = strBodyElement.TrimStart('+');
                            if (strBodyElement.Contains("Rating"))
                            {
                                // If the cost is determined by the Rating, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strBodyElement.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intBody += ((double)objProcess).StandardRound();
                            }
                            else
                            {
                                intBody += Convert.ToInt32(strBodyElement.TrimStart('+'), GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }
                }

                return intBody;
            }
        }

        /// <summary>
        /// Total Handling of the Vehicle including Modifications.
        /// </summary>
        public string TotalHandling
        {
            get
            {
                char chrFirstCharacter;
                int intBaseHandling = Handling;
                int intBaseOffroadHandling = OffroadHandling;
                int intTotalArmor = 0;

                // First check for mods that overwrite the handling value or add to armor
                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        continue;
                    if (objMod.Bonus != null)
                    {
                        string strHandling = objMod.Bonus["handling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseHandling = Math.Max(intBaseHandling, Convert.ToInt32(strHandling.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        strHandling = objMod.Bonus["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadHandling = Math.Max(intBaseOffroadHandling, Convert.ToInt32(strHandling.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            string strArmor = objMod.Bonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo);
                            }
                        }
                    }
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        string strHandling = objMod.WirelessBonus["handling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseHandling = Math.Max(intBaseHandling, Convert.ToInt32(strHandling.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        strHandling = objMod.WirelessBonus["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadHandling = Math.Max(intBaseOffroadHandling, Convert.ToInt32(strHandling.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo));
                            }
                        }
                        if (IsDrone && _objCharacter.Settings.DroneMods)
                        {
                            string strArmor = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo);
                            }
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
                        string strHandling = objMod.Bonus["handling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strHandling.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("Handling", intBaseHandling.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusHandling += ((double)objProcess).StandardRound();
                            }
                        }
                        strHandling = objMod.Bonus["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strHandling.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("OffroadHandling", intBaseOffroadHandling.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusOffroadHandling += ((double)objProcess).StandardRound();
                            }
                        }
                    }
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        string strHandling = objMod.WirelessBonus["handling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strHandling.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("Handling", intBaseHandling.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusHandling += ((double)objProcess).StandardRound();
                            }
                        }
                        strHandling = objMod.WirelessBonus["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                object objProcess = CommonFunctions.EvaluateInvariantXPath(strHandling.TrimStart('+').Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)).Replace("OffroadHandling", intBaseOffroadHandling.ToString(GlobalSettings.InvariantCultureInfo)), out bool blnIsSuccess);
                                if (blnIsSuccess)
                                    intTotalBonusOffroadHandling += ((double)objProcess).StandardRound();
                            }
                        }
                    }
                }

                // Reduce handling of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 3, 0);

                if (Handling != OffroadHandling
                    || intBaseHandling + intTotalBonusHandling != intBaseOffroadHandling + intTotalBonusOffroadHandling)
                {
                    return (intBaseHandling + intTotalBonusHandling - intPenalty).ToString(GlobalSettings.InvariantCultureInfo)
                           + '/'
                           + (intBaseOffroadHandling + intTotalBonusOffroadHandling - intPenalty).ToString(GlobalSettings.InvariantCultureInfo);
                }

                return (intBaseHandling + intTotalBonusHandling - intPenalty).ToString(GlobalSettings.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// Total Armor of the Vehicle including Modifications.
        /// </summary>
        public int TotalArmor
        {
            get
            {
                int intModArmor = 0;

                // Add the Modification's Armor to the Vehicle's base Armor.
                foreach (VehicleMod objMod in Mods)
                {
                    if (objMod.IncludedInVehicle || !objMod.Equipped)
                        continue;
                    string strArmor = objMod.Bonus?["armor"]?.InnerText;
                    if (!string.IsNullOrEmpty(strArmor))
                    {
                        intModArmor += Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo);
                    }
                    if (objMod.WirelessOn)
                    {
                        strArmor = objMod.WirelessBonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strArmor))
                        {
                            intModArmor += Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString(GlobalSettings.InvariantCultureInfo)), GlobalSettings.InvariantCultureInfo);
                        }
                    }
                }
                // Rigger5 Drone Armor starts at 0. All other vehicles start with their base armor.
                if (IsDrone && _objCharacter.Settings.DroneMods)
                {
                    if (intModArmor <= 0)
                        intModArmor += _intArmor;
                }
                // Drones have no theoretical armor cap in the optional rules, otherwise, it's capped
                else
                {
                    intModArmor = Math.Min(MaxArmor, intModArmor + _intArmor);
                }
                return intModArmor;
            }
        }

        /// <summary>
        /// Maximum amount of each Armor type the Vehicle can hold.
        /// </summary>
        public int MaxArmor
        {
            get
            {
                // If ignoring the rules, do not limit Armor to the Vehicle's standard rules.
                if (_objCharacter.IgnoreRules)
                    return int.MaxValue;

                // Rigger 5 says max extra armor is Body + starting Armor, p159
                int intReturn = _intBody + _intArmor;

                if (IsDrone && _objCharacter.Settings.DroneArmorMultiplierEnabled)
                {
                    intReturn *= _objCharacter.Settings.DroneArmorMultiplier;
                }

                //When you need to use a 0 for the math, use 0.5 instead
                return Math.Max(intReturn, 1);
            }
        }

        /// <summary>
        /// Maximum Speed attribute allowed for the Vehicle
        /// </summary>
        public int MaxSpeed
        {
            get
            {
                //Drone’s attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules)
                {
                    return Math.Max(_intSpeed * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Handling attribute allowed for the Vehicle
        /// </summary>
        public int MaxHandling
        {
            get
            {
                //Drone’s attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules)
                {
                    return Math.Max(_intHandling * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Acceleration attribute allowed for the Vehicle
        /// </summary>
        public int MaxAcceleration
        {
            get
            {
                //Drone’s attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules)
                {
                    return Math.Max(_intAccel * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Sensor attribute allowed for the Vehicle
        /// </summary>
        public int MaxSensor
        {
            get
            {
                //Drone’s attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules)
                {
                    return Math.Max(_intSensor * 2, 1);
                }
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Maximum Sensor attribute allowed for the Vehicle
        /// </summary>
        public int MaxPilot
        {
            get
            {
                //Drone’s attributes can never by higher than twice their starting value (R5, p123)
                //When you need to use a 0 for the math, use 0.5 instead
                if (IsDrone && !_objCharacter.IgnoreRules && _objCharacter.Settings.DroneModsMaximumPilot)
                {
                    return Math.Max(_intPilot * 2, 1);
                }
                return int.MaxValue;
            }
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
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string PowertrainModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddPowertrainModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Powertrain") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string ProtectionModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddProtectionModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Protection") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string WeaponModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddWeaponModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Weapons") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Body Mod Slots as Used/Total
        /// </summary>
        public string BodyModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddBodyModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Body") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Electromagnetic Mod Slots as Used/Total
        /// </summary>
        public string ElectromagneticModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddElectromagneticModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Electromagnetic") + intModSlots, intTotal);
        }

        /// <summary>
        /// Display the Cosmetic Mod Slots as Used/Total
        /// </summary>
        public string CosmeticModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddCosmeticModSlots;
            return string.Format(GlobalSettings.CultureInfo, "{0}/{1}", intTotal - CalcCategoryAvail("Cosmetic") + intModSlots, intTotal);
        }

        /// <summary>
        /// Vehicle's Maneuver AutoSoft Rating.
        /// </summary>
        public int Maneuver
        {
            get
            {
                Gear objGear = GearChildren.DeepFirstOrDefault(x => x.Children, x => x.Name == "[Model] Maneuvering Autosoft" && x.Extra == Name && !x.InternalId.IsEmptyGuid());
                return objGear?.Rating ?? 0;
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalSettings.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage && !GlobalSettings.LiveCustomData)
                return _objCachedMyXmlNode;
            _objCachedMyXmlNode = _objCharacter.LoadData("vehicles.xml", strLanguage)
                                               .SelectSingleNode(SourceID == Guid.Empty
                                                                     ? "/chummer/vehicles/vehicle[name = "
                                                                       + Name.CleanXPath() + ']'
                                                                     : "/chummer/vehicles/vehicle[id = "
                                                                       + SourceIDString.CleanXPath() + " or id = "
                                                                       + SourceIDString.ToUpperInvariant().CleanXPath()
                                                                       + ']');
            _strCachedXmlNodeLanguage = strLanguage;
            return _objCachedMyXmlNode;
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
        /// Empty for Vehicles.
        /// </summary>
        public string CanFormPersona {
            get => string.Empty;
            set
            {
                // Dummy
            }
        }

        public bool IsCommlink => GearChildren.Any(x => x.CanFormPersona.Contains("Parent")) && this.GetTotalMatrixAttribute("Device Rating") > 0;

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
            int intBase = 0;

            foreach (VehicleMod objMod in Mods)
            {
                if (objMod.IncludedInVehicle || !objMod.Equipped || objMod.Category != strCategory)
                    continue;
                // Subtract the Modification's Slots from the Vehicle's base Body.
                int intSlots = objMod.CalculatedSlots;
                if (intSlots > 0)
                    intBase += intSlots;
            }

            if (strCategory == "Weapons")
            {
                foreach (WeaponMount objMount in WeaponMounts)
                {
                    if (objMount.IncludedInVehicle || !objMount.Equipped)
                        continue;
                    // Subtract the Weapon Mount's Slots from the Vehicle's base Body.
                    int intSlots = objMount.CalculatedSlots;
                    if (intSlots > 0)
                        intBase += intSlots;
                }
            }

            return intBase;
        }

        /// <summary>
        /// Total number of slots still available for vehicle mods (and weapon mounts) in a given Rigger 5 vehicle mod category.
        /// </summary>
        public int CalcCategoryAvail(string strCategory)
        {
            int intBase = _intBody;

            switch (strCategory)
            {
                case "Powertrain":
                    intBase += _intAddPowertrainModSlots;
                    break;

                case "Weapons":
                    intBase += _intAddWeaponModSlots;
                    break;

                case "Body":
                    intBase += _intAddBodyModSlots;
                    break;

                case "Electromagnetic":
                    intBase += _intAddElectromagneticModSlots;
                    break;

                case "Protection":
                    intBase += _intAddProtectionModSlots;
                    break;

                case "Cosmetic":
                    intBase += _intAddCosmeticModSlots;
                    break;
            }

            intBase -= CalcCategoryUsed(strCategory);
            return intBase;
        }

        public decimal DeleteVehicle()
        {
            decimal decReturn = 0;

            foreach (Gear objGear in GearChildren)
            {
                decReturn += objGear.DeleteGear();
            }
            foreach (Weapon objLoopWeapon in Weapons)
            {
                decReturn += objLoopWeapon.DeleteWeapon();
            }
            foreach (VehicleMod objLoopMod in Mods)
            {
                decReturn += objLoopMod.DeleteVehicleMod();
            }
            foreach (WeaponMount objLoopMount in WeaponMounts)
            {
                decReturn += objLoopMount.DeleteWeaponMount();
            }

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
                }
            }

            foreach (VehicleMod objChild in Mods)
            {
                objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, ref intRestrictedCount);
            }
            foreach (Gear objChild in GearChildren)
            {
                objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, ref intRestrictedCount);
            }
            foreach (Weapon objChild in Weapons)
            {
                objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, ref intRestrictedCount);
            }
            foreach (WeaponMount objChild in WeaponMounts)
            {
                objChild.CheckRestrictedGear(dicRestrictedGearLimits, sbdAvailItems, sbdRestrictedItems, ref intRestrictedCount);
            }
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
        public TreeNode CreateTreeNode(ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleLocation, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear)
        {
            if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsVehicle,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // Populate the list of Vehicle Locations.
            foreach (Location objLocation in Locations)
            {
                lstChildNodes.Add(objLocation.CreateTreeNode(cmsVehicleLocation));
            }

            // VehicleMods.
            foreach (VehicleMod objMod in Mods)
            {
                TreeNode objLoopNode = objMod.CreateTreeNode(cmsVehicle, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }
            if (WeaponMounts.Count > 0)
            {
                TreeNode nodMountsNode = new TreeNode
                {
                    Tag = "String_WeaponMounts",
                    Text = LanguageManager.GetString("String_WeaponMounts")
                };

                // Weapon Mounts
                foreach (WeaponMount objWeaponMount in WeaponMounts)
                {
                    TreeNode objLoopNode = objWeaponMount.CreateTreeNode(cmsVehicleWeaponMount, cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, cmsCyberware, cmsCyberwareGear, cmsVehicle);
                    if (objLoopNode != null)
                    {
                        nodMountsNode.Nodes.Add(objLoopNode);
                        nodMountsNode.Expand();
                    }
                }

                if (nodMountsNode.Nodes.Count > 0)
                    lstChildNodes.Add(nodMountsNode);
            }
            // Vehicle Weapons (not attached to a mount).
            foreach (Weapon objWeapon in Weapons)
            {
                TreeNode objLoopNode = objWeapon.CreateTreeNode(cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objLoopNode != null)
                {
                    TreeNode objParent = objNode;
                    if (objWeapon.Location != null)
                    {
                        foreach (TreeNode objFind in lstChildNodes)
                        {
                            if (objFind.Tag != objWeapon.Location) continue;
                            objParent = objFind;
                            break;
                        }
                    }

                    objParent.Nodes.Add(objLoopNode);
                    objParent.Expand();
                }
            }

            // Vehicle Gear.
            foreach (Gear objGear in GearChildren)
            {
                TreeNode objLoopNode = objGear.CreateTreeNode(cmsVehicleGear);
                if (objLoopNode != null)
                {
                    TreeNode objParent = objNode;
                    if (objGear.Location != null)
                    {
                        foreach (TreeNode objFind in lstChildNodes)
                        {
                            if (objFind.Tag != objGear.Location) continue;
                            objParent = objFind;
                            break;
                        }
                    }

                    objParent.Nodes.Add(objLoopNode);
                    objParent.Expand();
                }
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
                objMod = Mods.FirstOrDefault(funcPredicate);
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
        /// Change the size of a Vehicle's Sensor -- This appears to be obsolete code
        /// </summary>
        /// <param name="blnIncrease">True if the Sensor should increase in size, False if it should decrease.</param>
        /// <param name="treVehicles">TreeView where the vehicle's node would be.</param>
        public void ChangeVehicleSensor(TreeView treVehicles, bool blnIncrease)
        {
            Gear objCurrentSensor = null;
            Gear objNewSensor = new Gear(_objCharacter);

            List<Weapon> lstWeapons = new List<Weapon>(1);
            foreach (Gear objCurrentGear in GearChildren)
            {
                if (objCurrentGear.Name == "Microdrone Sensor")
                {
                    if (blnIncrease)
                    {
                        XmlNode xmlNewGear = _objCharacter.LoadData("gear.xml").SelectSingleNode("/chummer/gears/gear[name = \"Minidrone Sensor\" and category = \"Sensors\"]");
                        objNewSensor.Create(xmlNewGear, 0, lstWeapons);
                        objCurrentSensor = objCurrentGear;
                    }
                    break;
                }
                if (objCurrentGear.Name == "Minidrone Sensor")
                {
                    XmlNode xmlNewGear = _objCharacter.LoadData("gear.xml").SelectSingleNode(blnIncrease ? "/chummer/gears/gear[name = \"Small Drone Sensor\" and category = \"Sensors\"]" : "/chummer/gears/gear[name = \"Microdrone Sensor\" and category = \"Sensors\"]");
                    objNewSensor.Create(xmlNewGear, 0, lstWeapons);
                    objCurrentSensor = objCurrentGear;
                    break;
                }
                if (objCurrentGear.Name == "Small Drone Sensor")
                {
                    XmlNode xmlNewGear = _objCharacter.LoadData("gear.xml").SelectSingleNode(blnIncrease ? "/chummer/gears/gear[name = \"Medium Drone Sensor\" and category = \"Sensors\"]" : "/chummer/gears/gear[name = \"Minidrone Sensor\" and category = \"Sensors\"]");
                    objNewSensor.Create(xmlNewGear, 0, lstWeapons);
                    objCurrentSensor = objCurrentGear;
                    break;
                }
                if (objCurrentGear.Name == "Medium Drone Sensor")
                {
                    XmlNode xmlNewGear = _objCharacter.LoadData("gear.xml").SelectSingleNode(blnIncrease ? "/chummer/gears/gear[name = \"Large Drone Sensor\" and category = \"Sensors\"]" : "/chummer/gears/gear[name = \"Small Drone Sensor\" and category = \"Sensors\"]");
                    objNewSensor.Create(xmlNewGear, 0, lstWeapons);
                    objCurrentSensor = objCurrentGear;
                    break;
                }
                if (objCurrentGear.Name == "Large Drone Sensor")
                {
                    XmlNode xmlNewGear = _objCharacter.LoadData("gear.xml").SelectSingleNode(blnIncrease ? "/chummer/gears/gear[name = \"Vehicle Sensor\" and category = \"Sensors\"]" : "/chummer/gears/gear[name = \"Medium Drone Sensor\" and category = \"Sensors\"]");
                    objNewSensor.Create(xmlNewGear, 0, lstWeapons);
                    objCurrentSensor = objCurrentGear;
                    break;
                }
                if (objCurrentGear.Name == "Vehicle Sensor")
                {
                    XmlNode xmlNewGear = _objCharacter.LoadData("gear.xml").SelectSingleNode(blnIncrease ? "/chummer/gears/gear[name = \"Extra-Large Vehicle Sensor\" and category = \"Sensors\"]" : "/chummer/gears/gear[name = \"Large Drone Sensor\" and category = \"Sensors\"]");
                    objNewSensor.Create(xmlNewGear, 0, lstWeapons);
                    objCurrentSensor = objCurrentGear;
                    break;
                }
                if (objCurrentGear.Name == "Extra-Large Vehicle Sensor")
                {
                    if (!blnIncrease)
                    {
                        XmlNode xmlNewGear = _objCharacter.LoadData("gear.xml").SelectSingleNode("/chummer/gears/gear[name = \"Vehicle Sensor\" and category = \"Sensors\"]");
                        objNewSensor.Create(xmlNewGear, 0, lstWeapons);
                        objCurrentSensor = objCurrentGear;
                    }
                    break;
                }
            }

            // If the item was found, update the Vehicle Sensor information.
            if (objCurrentSensor != null)
            {
                objCurrentSensor.Name = objNewSensor.Name;
                objCurrentSensor.Rating = objNewSensor.Rating;
                objCurrentSensor.Capacity = objNewSensor.Capacity;
                objCurrentSensor.DeviceRating = objNewSensor.DeviceRating;
                objCurrentSensor.Avail = objNewSensor.Avail;
                objCurrentSensor.Cost = objNewSensor.Cost;
                objCurrentSensor.Source = objNewSensor.Source;
                objCurrentSensor.Page = objNewSensor.Page;

                // Update the name of the item in the TreeView.
                TreeNode objNode = treVehicles.FindNode(objCurrentSensor.InternalId);
                if (objNode != null)
                    objNode.Text = objCurrentSensor.DisplayNameShort(GlobalSettings.Language);

                foreach (Weapon objWeapon in lstWeapons)
                {
                    Weapons.Add(objWeapon);
                }
            }
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
                            if (strExpression.Contains("{Children " + strMatrixAttribute + "}"))
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

                                sbdValue.Replace("{Children " + strMatrixAttribute + "}",
                                                 intTotalChildrenValue.ToString(GlobalSettings.InvariantCultureInfo));
                            }
                        }
                    }

                    _objCharacter.AttributeSection.ProcessAttributesInXPath(sbdValue, strExpression);
                    // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    object objProcess
                        = CommonFunctions.EvaluateInvariantXPath(sbdValue.ToString(), out bool blnIsSuccess);
                    return blnIsSuccess ? ((double) objProcess).StandardRound() : 0;
                }
            }

            return !int.TryParse(strExpression, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intReturn) ? 0 : intReturn;
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
                    XmlNode objBonus = objMod.Bonus?[strAttributeNodeName];
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

        #endregion Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicle")))
                return false;

            DeleteVehicle();
            return _objCharacter.Vehicles.Remove(this);
        }

        public void Sell(decimal percentage)
        {
            if (!Remove(GlobalSettings.ConfirmDelete))
                return;

            // Create the Expense Log Entry for the sale.
            decimal decAmount = TotalCost * percentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldVehicle") + ' ' + DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen, DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen += decAmount;
        }

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
                        {
                            string strClipboardCategory = GlobalSettings.Clipboard.SelectSingleNode("category")?.Value;
                            if (!string.IsNullOrEmpty(strClipboardCategory))
                            {
                                using (XmlNodeList xmlAddonCategoryList = GetNode()?.SelectNodes("addoncategory"))
                                {
                                    if (xmlAddonCategoryList?.Count > 0)
                                    {
                                        foreach (XmlNode xmlLoop in xmlAddonCategoryList)
                                        {
                                            if (xmlLoop.InnerText == strClipboardCategory)
                                                return true;
                                        }
                                    }
                                }
                            }
                            return false;
                        }
                    default:
                        return false;
                }
            }
        }

        public bool AllowPasteObject(object input)
        {
            throw new NotImplementedException();
        }

        public const int MaxWheels = 50;
    }
}
