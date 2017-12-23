using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Vehicle.
    /// </summary>
    public class Vehicle : IHasInternalId, IHasName, IHasXmlNode, IHasMatrixAttributes
    {
        private Guid _guiID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private int _intHandling = 0;
        private int _intOffroadHandling = 0;
        private int _intAccel = 0;
        private int _intOffroadAccel = 0;
        private int _intSpeed = 0;
        private int _intOffroadSpeed = 0;
        private int _intPilot = 0;
        private int _intBody = 0;
        private int _intArmor = 0;
        private int _intSensor = 0;
        private int _intSeats = 0;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strVehicleName = string.Empty;
        private int _intAddSlots = 0;
        private int _intDroneModSlots = 0;
        private int _intAddPowertrainModSlots = 0;
        private int _intAddProtectionModSlots = 0;
        private int _intAddWeaponModSlots = 0;
        private int _intAddBodyModSlots = 0;
        private int _intAddElectromagneticModSlots = 0;
        private int _intAddCosmeticModSlots = 0;
        private List<VehicleMod> _lstVehicleMods = new List<VehicleMod>();
        private List<Gear> _lstGear = new List<Gear>();
        private List<Weapon> _lstWeapons = new List<Weapon>();
        private List<WeaponMount> _lstWeaponMounts = new List<WeaponMount>();
        private string _strNotes = string.Empty;
        private List<string> _lstLocations = new List<string>();
        private bool _blnBlackMarketDiscount = false;
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
        private bool _blnCanSwapAttributes = false;

        // Condition Monitor Progress.
        private int _intPhysicalCMFilled = 0;
        private int _intMatrixCMFilled = 0;
        private Guid _sourceID;

        #region Constructor, Create, Save, Load, and Print Methods
        public Vehicle(Character objCharacter)
        {
            // Create the GUID for the new Vehicle.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Vehicle from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlVehicle">XmlNode of the Vehicle to create.</param>
        /// <param name="objNode">TreeNode to add to a TreeView.</param>
        /// <param name="cmsVehicle">ContextMenuStrip to attach to Weapon Mounts.</param>
        /// <param name="cmsVehicleGear">ContextMenuStrip to attach to Gear.</param>
        /// <param name="cmsVehicleWeapon">ContextMenuStrip to attach to Vehicle Weapons.</param>
        /// <param name="cmsVehicleWeaponAccessory">ContextMenuStrip to attach to Weapon Accessories.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        public void Create(XmlNode objXmlVehicle, TreeNode objNode, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear = null, ContextMenuStrip cmsVehicleWeaponMount = null, bool blnCreateChildren = true)
        {
            if (objXmlVehicle.TryGetField("id", Guid.TryParse, out _sourceID))
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
                    int.TryParse(strHandlingArray[0], out _intHandling);
                    int.TryParse(strHandlingArray[1], out _intOffroadHandling);
                }
                else
                {
                    int.TryParse(strTemp, out _intHandling);
                    _intOffroadHandling = _intHandling;
                }
            }
            strTemp = objXmlVehicle["accel"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.Contains('/'))
                {
                    string[] strAccelArray = strTemp.Split('/');
                    int.TryParse(strAccelArray[0], out _intAccel);
                    int.TryParse(strAccelArray[1], out _intOffroadAccel);
                }
                else
                {
                    int.TryParse(strTemp, out _intAccel);
                    _intOffroadAccel = _intAccel;
                }
            }
            strTemp = objXmlVehicle["speed"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.Contains('/'))
                {
                    string[] strSpeedArray = strTemp.Split('/');
                    int.TryParse(strSpeedArray[0], out _intSpeed);
                    int.TryParse(strSpeedArray[1], out _intOffroadSpeed);
                }
                else
                {
                    int.TryParse(strTemp, out _intSpeed);
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
            objXmlVehicle.TryGetStringFieldQuickly("notes", ref _strNotes);
            _strCost = objXmlVehicle["cost"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(_strCost))
            {
                // Check for a Variable Cost.
                if (_strCost.StartsWith("Variable"))
                {
                    decimal decMin = 0;
                    decimal decMax = decimal.MaxValue;
                    string strCost = _strCost.TrimStart("Variable", true).Trim("()".ToCharArray());
                    if (strCost.Contains('-'))
                    {
                        string[] strValues = strCost.Split('-');
                        decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    if (decMin != 0 || decMax != decimal.MaxValue)
                    {
                        string strNuyenFormat = _objCharacter.Options.NuyenFormat;
                        int intDecimalPlaces = strNuyenFormat.IndexOf('.');
                        if (intDecimalPlaces == -1)
                            intDecimalPlaces = 0;
                        else
                            intDecimalPlaces = strNuyenFormat.Length - intDecimalPlaces - 1;
                        frmSelectNumber frmPickNumber = new frmSelectNumber(intDecimalPlaces);
                        if (decMax > 1000000)
                            decMax = 1000000;
                        frmPickNumber.Minimum = decMin;
                        frmPickNumber.Maximum = decMax;
                        frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost", GlobalOptions.Language).Replace("{0}", DisplayNameShort(GlobalOptions.Language));
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                    }
                }
            }
            objXmlVehicle.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlVehicle.TryGetStringFieldQuickly("page", ref _strPage);

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

            objNode.Text = DisplayName(GlobalOptions.Language);
            objNode.Tag = _guiID.ToString();

            // If there are any VehicleMods that come with the Vehicle, add them.
            if (objXmlVehicle.InnerXml.Contains("<mods>") && blnCreateChildren)
            {
                XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");

                XmlNodeList objXmlModList = objXmlVehicle.SelectNodes("mods/name");
                foreach (XmlNode objXmlVehicleMod in objXmlModList)
                {
                    XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + objXmlVehicleMod.InnerText + "\"]");
                    if (objXmlMod != null)
                    {
                        TreeNode objModNode = new TreeNode();
                        VehicleMod objMod = new VehicleMod(_objCharacter);
                        int intRating = 0;

                        if (objXmlVehicleMod.Attributes["rating"] != null)
                            int.TryParse(objXmlVehicleMod.Attributes["rating"].InnerText, out intRating);

                        if (objXmlVehicleMod.Attributes["select"] != null)
                            objMod.Extra = objXmlVehicleMod.Attributes["select"].InnerText;

                        objMod.Create(objXmlMod, objModNode, intRating, this);
                        objMod.IncludedInVehicle = true;

                        _lstVehicleMods.Add(objMod);
                        objModNode.ForeColor = SystemColors.GrayText;
                        objModNode.ContextMenuStrip = cmsVehicle;

                        objNode.Nodes.Add(objModNode);
                        objNode.Expand();
                    }
                }
                XmlNode objAddSlotsNode = objXmlVehicle.SelectSingleNode("mods/addslots");
                if (objAddSlotsNode != null)
                    int.TryParse(objAddSlotsNode.InnerText, out _intAddSlots);
            }

            // If there are any Weapon Mounts that come with the Vehicle, add them.
            if (objXmlVehicle.InnerXml.Contains("<weaponmounts>") && blnCreateChildren)
            {

                XmlNodeList objXmlModList = objXmlVehicle.SelectNodes("weaponmounts/weaponmount");
                foreach (XmlNode objXmlVehicleMod in objXmlModList)
                {
                    TreeNode t = new TreeNode();
                    WeaponMount w = new WeaponMount(_objCharacter, this);
                    w.CreateByName(objXmlVehicleMod, t);
                    w.IncludedInVehicle = true;
                    WeaponMounts.Add(w);
                }
            }

            // If there is any Gear that comes with the Vehicle, add them.
            if (objXmlVehicle.InnerXml.Contains("<gears>") && blnCreateChildren)
			{
				XmlDocument objXmlDocument = XmlManager.Load("gear.xml");

                XmlNodeList objXmlGearList = objXmlVehicle.SelectNodes("gears/gear");
                foreach (XmlNode objXmlVehicleGear in objXmlGearList)
                {
                    XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlVehicleGear.InnerText + "\"]");
                    if (objXmlGear != null)
                    {
                        TreeNode objGearNode = new TreeNode();
                        Gear objGear = new Gear(_objCharacter);
                        int intRating = 0;
                        decimal decQty = 1;
                        string strForceValue = string.Empty;

                        XmlAttributeCollection objXmlVehicleGearAttributes = objXmlVehicleGear.Attributes;
                        if (objXmlVehicleGearAttributes["rating"] != null)
                            intRating = Convert.ToInt32(objXmlVehicleGearAttributes["rating"].InnerText);

                        int intMaxRating = intRating;
                        if (objXmlVehicleGearAttributes["maxrating"] != null)
                            intMaxRating = Convert.ToInt32(objXmlVehicleGearAttributes["maxrating"].InnerText);

                        if (objXmlVehicleGearAttributes["qty"] != null)
                            decQty = Convert.ToDecimal(objXmlVehicleGearAttributes["qty"].InnerText, GlobalOptions.InvariantCultureInfo);

                        if (objXmlVehicleGearAttributes["select"] != null)
                            strForceValue = objXmlVehicleGearAttributes["select"].InnerText;

                        List<Weapon> objWeapons = new List<Weapon>();
                        List<TreeNode> objWeaponNodes = new List<TreeNode>();

                        objGear.Create(objXmlGear, objGearNode, intRating, objWeapons, objWeaponNodes, strForceValue);

                        objGear.Cost = "0";
                        objGear.Quantity = decQty;
                        objGear.MaxRating = intMaxRating;
                        objGear.ParentID = InternalId;
                        objGearNode.Text = objGear.DisplayName(GlobalOptions.Language);
                        objGearNode.ContextMenuStrip = cmsVehicleGear;

                        foreach (Weapon objWeapon in objWeapons)
                            objWeapon.ParentVehicle = this;

                        _lstGear.Add(objGear);

                        objNode.Nodes.Add(objGearNode);
                        objNode.Expand();
                    }
                }
            }

            // If there are any Weapons that come with the Vehicle, add them.
            if (objXmlVehicle.InnerXml.Contains("<weapons>") && blnCreateChildren)
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                foreach (XmlNode objXmlWeapon in objXmlVehicle.SelectNodes("weapons/weapon"))
                {
                    bool blnAttached = false;
                    List<TreeNode> lstWeaponNodes = new List<TreeNode>();
                    Weapon objWeapon = new Weapon(_objCharacter);

                    List<Weapon> objSubWeapons = new List<Weapon>();
                    XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlWeapon["name"].InnerText + "\"]");
                    objWeapon.ParentVehicle = this;
                    objWeapon.Create(objXmlWeaponNode, lstWeaponNodes, cmsVehicleWeapon, cmsVehicleWeaponAccessory, objSubWeapons, cmsVehicleWeaponAccessoryGear);
                    objWeapon.ParentID = InternalId;
                    objWeapon.Cost = 0;

                    // Find the first free Weapon Mount in the Vehicle.
                    foreach (WeaponMount w in _lstWeaponMounts)
                    {
                        if (!String.IsNullOrWhiteSpace(w.WeaponMountCategories) && w.WeaponMountCategories.Contains(objWeapon.Category) && w.Weapons.Count == 0)
                        {
                            w.Weapons.Add(objWeapon);
                            ((List<Weapon>)w.Weapons).AddRange(objSubWeapons);
                            foreach (TreeNode objModNode in objNode.Nodes)
                            {
                                if (objModNode.Tag.ToString() == w.InternalId)
                                {
                                    foreach (TreeNode objWeaponNode in lstWeaponNodes)
                                    {
                                        objWeaponNode.ContextMenuStrip = cmsVehicleWeapon;
                                        objModNode.Nodes.Add(objWeaponNode);
                                    }
                                    objModNode.Expand();
                                    blnAttached = true;
                                    break;
                                }
                            }
                            break;
                        }
                    }

					// If a free Weapon Mount could not be found, just attach it to the first one found and let the player deal with it.
					if (!blnAttached)
					{
                        foreach (VehicleMod objMod in _lstVehicleMods)
                        {
                            if ((objMod.Name.Contains("Weapon Mount") || (!String.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.Category) && objMod.Weapons.Count == 0)))
                            {
                                objMod.Weapons.Add(objWeapon);
                                ((List<Weapon>)objMod.Weapons).AddRange(objSubWeapons);
                                foreach (TreeNode objModNode in objNode.Nodes)
                                {
                                    if (objModNode.Tag.ToString() == objMod.InternalId)
                                    {
                                        foreach (TreeNode objWeaponNode in lstWeaponNodes)
                                        {
                                            objWeaponNode.ContextMenuStrip = cmsVehicleWeapon;
                                            objModNode.Nodes.Add(objWeaponNode);
                                        }
                                        objModNode.Expand();
                                        blnAttached = true;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (!blnAttached)
                        {
                            foreach (VehicleMod objMod in _lstVehicleMods)
                            {
                                if (objMod.Name.Contains("Weapon Mount") || (!String.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.Category)))
                                {
                                    objMod.Weapons.Add(objWeapon);
                                    ((List<Weapon>)objMod.Weapons).AddRange(objSubWeapons);
                                    foreach (TreeNode objModNode in objNode.Nodes)
                                    {
                                        if (objModNode.Tag.ToString() == objMod.InternalId)
                                        {
                                            foreach (TreeNode objWeaponNode in lstWeaponNodes)
                                            {
                                                objWeaponNode.ContextMenuStrip = cmsVehicleWeapon;
                                                objModNode.Nodes.Add(objWeaponNode);
                                            }
                                            objModNode.Expand();
                                            blnAttached = true;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    // Look for Weapon Accessories.
                    if (objXmlWeapon["accessories"] != null && lstWeaponNodes.Count > 0)
                    {
                        foreach (XmlNode objXmlAccessory in objXmlWeapon.SelectNodes("accessories/accessory"))
                        {
                            XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + objXmlAccessory["name"].InnerText + "\"]");
                            WeaponAccessory objMod = new WeaponAccessory(_objCharacter);
                            TreeNode objModNode = new TreeNode();
                            string strMount = "Internal";
                            objXmlAccessory.TryGetStringFieldQuickly("mount", ref strMount);
                            string strExtraMount = "None";
                            objXmlAccessory.TryGetStringFieldQuickly("extramount", ref strExtraMount);
                            objMod.Create(objXmlAccessoryNode, objModNode, new Tuple<string, string>(strMount, strExtraMount), 0, cmsVehicleGear, false, blnCreateChildren);

                            objMod.Cost = "0";
                            objModNode.ContextMenuStrip = cmsVehicleWeaponAccessory;

                            objWeapon.WeaponAccessories.Add(objMod);

                            lstWeaponNodes[0].Nodes.Add(objModNode);
                            lstWeaponNodes[0].Expand();
                        }
                    }
                }
            }

            if (WeaponMounts.Count > 0)
            {
                TreeNode mountsNode = new TreeNode()
                {
                    Tag = "String_WeaponMounts",
                    Text = LanguageManager.GetString("String_WeaponMounts", GlobalOptions.Language)
                };
                objNode.Nodes.Add(mountsNode);
                // Weapon Mounts
                foreach (WeaponMount wm in WeaponMounts)
                    CommonFunctions.CreateWeaponMountTreeNode(wm, mountsNode, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleGear, cmsVehicleWeaponMount);
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("vehicle");
            objWriter.WriteElementString("id", _sourceID.ToString());
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("handling", _intHandling.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("offroadhandling", _intOffroadHandling.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("accel", _intAccel.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("offroadaccel", _intOffroadAccel.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("speed", _intSpeed.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("offroadspeed", _intOffroadSpeed.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("pilot", _intPilot.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("body", _intBody.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("seats", _intSeats.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("armor", _intArmor.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("sensor", _intSensor.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("devicerating", Pilot.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("addslots", _intAddSlots.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("modslots", _intDroneModSlots.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("powertrainmodslots", _intAddPowertrainModSlots.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("protectionmodslots", _intAddProtectionModSlots.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("weaponmodslots", _intAddWeaponModSlots.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("bodymodslots", _intAddBodyModSlots.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("electromagneticmodslots", _intAddElectromagneticModSlots.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("cosmeticmodslots", _intAddCosmeticModSlots.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("vehiclename", _strVehicleName);
            objWriter.WriteStartElement("mods");
            foreach (VehicleMod objMod in _lstVehicleMods)
                objMod.Save(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weaponmounts");
            foreach (WeaponMount wm in WeaponMounts)
                wm.Save(objWriter);
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
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", DealerConnectionDiscount.ToString());
            if (_lstLocations.Count > 0)
            {
                // <locations>
                objWriter.WriteStartElement("locations");
                foreach (string strLocation in _lstLocations)
                {
                    objWriter.WriteElementString("location", strLocation);
                }
                // </locations>
                objWriter.WriteEndElement();
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString());
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString());
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
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString());
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Vehicle from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
                this.SetHomeNode(_objCharacter, false);
            }
            else
            {
                _guiID = Guid.Parse(objNode["guid"].InnerText);
                bool blnIsHomeNode = false;
                if (objNode.TryGetBoolFieldQuickly("homenode", ref blnIsHomeNode) && blnIsHomeNode)
                {
                    this.SetHomeNode(_objCharacter, true);
                }
            }
            bool blnIsActive = false;
            if (objNode.TryGetBoolFieldQuickly("active", ref blnIsActive) && blnIsActive)
                this.SetActiveCommlink(_objCharacter, true);

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetField("id", Guid.TryParse, out _sourceID) || _sourceID.Equals(Guid.Empty))
            {
                XmlNode sourceNode = XmlManager.Load("vehicles.xml")?.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + Name + "\"]");
                if (sourceNode?.TryGetField("id", Guid.TryParse, out _sourceID) == true)
                    _objCachedMyXmlNode = null;
            }
            else
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            string strTemp = objNode["handling"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                //Some vehicles have different Offroad Handling speeds. If so, we want to split this up for use with mods and such later.
                if (strTemp.Contains('/'))
                {
                    string[] lstHandlings = strTemp.Split('/');
                    _intHandling = Convert.ToInt32(lstHandlings[0]);
                    _intOffroadHandling = Convert.ToInt32(lstHandlings[1]);
                }
                else
                {
                    _intHandling = Convert.ToInt32(strTemp);
                    strTemp = objNode["offroadhandling"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        _intOffroadHandling = Convert.ToInt32(strTemp);
                    }
                }
            }
            strTemp = objNode["accel"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.Contains('/'))
                {
                    string[] lstAccels = strTemp.Split('/');
                    _intAccel = Convert.ToInt32(lstAccels[0]);
                    _intOffroadAccel = Convert.ToInt32(lstAccels[1]);
                }
                else
                {
                    _intAccel = Convert.ToInt32(strTemp);
                    strTemp = objNode["offroadaccel"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        _intOffroadAccel = Convert.ToInt32(strTemp);
                    }
                }
            }
            strTemp = objNode["speed"]?.InnerText;
            if (!string.IsNullOrEmpty(strTemp))
            {
                if (strTemp.Contains('/'))
                {
                    string[] lstSpeeds = strTemp.Split('/');
                    _intSpeed = Convert.ToInt32(strTemp[0]);
                    _intOffroadSpeed = Convert.ToInt32(strTemp[1]);
                }
                else
                {
                    _intSpeed = Convert.ToInt32(strTemp);
                    strTemp = objNode["offroadspeed"]?.InnerText;
                    if (!string.IsNullOrEmpty(strTemp))
                    {
                        _intOffroadSpeed = Convert.ToInt32(strTemp);
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
                XmlNodeList nodChildren = objNode.SelectNodes("weaponmounts/weaponmount");
                foreach (XmlNode nodChild in nodChildren)
                {
                    WeaponMount wm = new WeaponMount(_objCharacter, this)
                    {
                        Parent = this
                    };
                    wm.Load(nodChild, this, blnCopy);
                    WeaponMounts.Add(wm);
                }
            }

            if (strNodeInnerXml.Contains("<gears>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
                foreach (XmlNode nodChild in nodChildren)
                {
                    string strChildCategory = nodChild["category"].InnerText;

                    Gear objGear = new Gear(_objCharacter);
                    objGear.Load(nodChild, blnCopy);
                    _lstGear.Add(objGear);
                }
            }

            if (strNodeInnerXml.Contains("<weapons>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("weapons/weapon");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Weapon objWeapon = new Weapon(_objCharacter);
                    objWeapon.ParentVehicle = this;
                    objWeapon.Load(nodChild, blnCopy);
                    _lstWeapons.Add(objWeapon);
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            
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

            this.RefreshMatrixAttributeArray();

            if (objNode["locations"] != null)
            {
                // Locations.
                foreach (XmlNode objXmlLocation in objNode.SelectNodes("locations/location"))
                {
                    _lstLocations.Add(objXmlLocation.InnerText);
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("vehicle");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("handling", TotalHandling);
            objWriter.WriteElementString("accel", TotalAccel);
            objWriter.WriteElementString("speed", TotalSpeed);
            objWriter.WriteElementString("pilot", Pilot.ToString(objCulture));
            objWriter.WriteElementString("body", TotalBody.ToString(objCulture));
            objWriter.WriteElementString("armor", TotalArmor.ToString(objCulture));
            objWriter.WriteElementString("seats", TotalSeats.ToString(objCulture));
            objWriter.WriteElementString("sensor", CalculatedSensor.ToString(objCulture));
            objWriter.WriteElementString("avail", CalculatedAvail(strLanguageToPrint));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("physicalcm", PhysicalCM.ToString(objCulture));
            objWriter.WriteElementString("matrixcm", MatrixCM.ToString(objCulture));
            objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString(objCulture));
            objWriter.WriteElementString("vehiclename", _strVehicleName);
            objWriter.WriteElementString("maneuver", Maneuver.ToString(objCulture));
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString());
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString());
            objWriter.WriteElementString("iscommlink", IsCommlink.ToString());
            objWriter.WriteElementString("attack", this.GetTotalMatrixAttribute("Attack").ToString(objCulture));
            objWriter.WriteElementString("sleaze", this.GetTotalMatrixAttribute("Sleaze").ToString(objCulture));
            objWriter.WriteElementString("dataprocessing", this.GetTotalMatrixAttribute("Data Processing").ToString(objCulture));
            objWriter.WriteElementString("firewall", this.GetTotalMatrixAttribute("Firewall").ToString(objCulture));
            objWriter.WriteElementString("devicerating", this.GetTotalMatrixAttribute("Device Rating").ToString(objCulture));
            objWriter.WriteElementString("programlimit", this.GetTotalMatrixAttribute("Program Limit").ToString(objCulture));
            objWriter.WriteStartElement("mods");
            foreach (VehicleMod objMod in Mods)
                objMod.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in Gear)
            {
                objGear.Print(objWriter, objCulture, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in Weapons)
                objWeapon.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }

        public Guid SourceID
        {
            get
            {
                return _sourceID;
            }
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get
            {
                return _strName;
            }
            set
            {
                _strName = value;
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("vehicles.xml", strLanguage)?.SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]")?.Attributes?["translate"]?.InnerText ?? Category;
        }

        /// <summary>
        /// Category.
        /// </summary>
        public string Category
        {
            get
            {
                return _strCategory;
            }
            set
            {
                _strCategory = value;
            }
        }

        /// <summary>
        /// Is this vehicle a drone?
        /// </summary>
        public Boolean IsDrone
        {
            get
            {
                return Category.Contains("Drone");
            }
        }

        /// <summary>
        /// Handling.
        /// </summary>
        public int Handling
        {
            get
            {
                return _intHandling;
            }
            set
            {
                _intHandling = value;
            }
        }


        /// <summary>
        /// Seats.
        /// </summary>
        public int Seats
        {
            get
            {
                return _intSeats;
            }
            set
            {
                _intSeats = value;
            }
        }


        /// <summary>
        /// Offroad Handling.
        /// </summary>
        public int OffroadHandling
        {
            get
            {
                return _intOffroadHandling;
            }
            set
            {
                _intOffroadHandling = value;
            }
        }

        /// <summary>
        /// Acceleration.
        /// </summary>
        public int Accel
        {
            get
            {
                return _intAccel;
            }
            set
            {
                _intAccel = value;
            }
        }

        /// <summary>
        /// Offroad Acceleration.
        /// </summary>
        public int OffroadAccel
        {
            get
            {
                return _intOffroadAccel;
            }
            set
            {
                _intOffroadAccel = value;
            }
        }

        /// <summary>
        /// Speed.
        /// </summary>
        public int Speed
        {
            get
            {
                return _intSpeed;
            }
            set
            {
                _intSpeed = value;
            }
        }

        /// <summary>
        /// Speed.
        /// </summary>
        public int OffroadSpeed
        {
            get
            {
                return _intOffroadSpeed;
            }
            set
            {
                _intOffroadSpeed = value;
            }
        }

        /// <summary>
        /// Pilot.
        /// </summary>
        public int Pilot
        {
            get
            {
                int intReturn = _intPilot;
                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    if (!objMod.IncludedInVehicle && objMod.Installed)
                    {
                        // Set the Vehicle's Pilot to the Modification's bonus.
                        if (objMod.Bonus != null && objMod.Bonus.InnerXml.Contains("<pilot>"))
                        {
                            int intTest = Convert.ToInt32(objMod.Bonus["pilot"].InnerText.Replace("Rating", objMod.Rating.ToString()));
                            if (intTest > intReturn)
                                intReturn = intTest;
                        }
                        else if (objMod.WirelessOn && objMod.WirelessBonus != null && objMod.WirelessBonus.InnerXml.Contains("<pilot>"))
                        {
                            int intTest = Convert.ToInt32(objMod.WirelessBonus["pilot"].InnerText.Replace("Rating", objMod.Rating.ToString()));
                            if (intTest > intReturn)
                                intReturn = intTest;
                        }
                    }
                }
                return intReturn;
            }
            set
            {
                _intPilot = value;
            }
        }

        /// <summary>
        /// Body.
        /// </summary>
        public int Body
        {
            get
            {
                return _intBody;
            }
            set
            {
                _intBody = value;
            }
        }

        /// <summary>
        /// Armor.
        /// </summary>
        public int Armor
        {
            get
            {
                return _intArmor;
            }
            set
            {
                _intArmor = value;
            }
        }

        /// <summary>
        /// Sensor.
        /// </summary>
        public int BaseSensor
        {
            get
            {
                return _intSensor;
            }
            set
            {
                _intSensor = value;
            }
        }

        /// <summary>
        /// Base Matrix Boxes.
        /// </summary>
        public int BaseMatrixBoxes
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM
        {
            get
            {
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
                return _intMatrixCMFilled;
            }
            set
            {
                _intMatrixCMFilled = value;
            }
        }

        /// <summary>
        /// Base Physical Boxes. 12 for vehicles, 6 for Drones.
        /// </summary>
        public int BasePhysicalBoxes
        {
            get
            {
                if (IsDrone)
                    return 6;
                else
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
                return BasePhysicalBoxes + (TotalBody + 1) / 2 + Mods.Sum(objMod => objMod.ConditionMonitor);
            }
        }

        /// <summary>
        /// Physical Condition Monitor boxes filled.
        /// </summary>
        public int PhysicalCMFilled
        {
            get
            {
                return _intPhysicalCMFilled;
            }
            set
            {
                _intPhysicalCMFilled = value;
            }
        }

        /// <summary>
        /// Availability.
        /// </summary>
        public string Avail
        {
            get
            {
                return _strAvail;
            }
            set
            {
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
                return _strCost;
            }
            set
            {
                _strCost = value;
            }
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get
            {
                return _strSource;
            }
            set
            {
                _strSource = value;
            }
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// ID of the object that added this weapon (if any).
        /// </summary>
        public string ParentID
        {
            get
            {
                return _strParentID;
            }
            set
            {
                _strParentID = value;
            }
        }

        /// <summary>
        /// Vehicle Modifications applied to the Vehicle.
        /// </summary>
        public IList<VehicleMod> Mods
        {
            get
            {
                return _lstVehicleMods;
            }
        }

        /// <summary>
        /// Gear applied to the Vehicle.
        /// </summary>
        public IList<Gear> Gear
        {
            get
            {
                return _lstGear;
            }
        }

        /// <summary>
        /// Weapons applied to the Vehicle through Gear.
        /// </summary>
        public IList<Weapon> Weapons => _lstWeapons;

        public IList<WeaponMount> WeaponMounts
        {
            get
            {
                return _lstWeaponMounts;
            }
        }

        /// <summary>
        /// Calculated Availablility of the Vehicle.
        /// </summary>
        public string CalculatedAvail(string strLanguage)
        {
            string strReturn = _strAvail;

            // Translate the Avail string.
            if (strReturn.Contains('R'))
                strReturn = strReturn.Replace("R", LanguageManager.GetString("String_AvailRestricted", strLanguage));
            else if (strReturn.Contains('F'))
                strReturn = strReturn.Replace("F", LanguageManager.GetString("String_AvailForbidden", strLanguage));

            return strReturn;
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
                else
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
                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    if (!objMod.IncludedInVehicle && objMod.Installed)
                    {
                        if (objMod.Bonus != null && objMod.Bonus.InnerXml.Contains("<sensor>"))
                        {
                            string strSensor = objMod.Bonus["sensor"].InnerText.Replace("Rating", objMod.Rating.ToString()).FastEscape('+');
                            intSensor = Math.Max(Convert.ToInt32(strSensor, GlobalOptions.InvariantCultureInfo), intSensor);
                        }
                        if (objMod.WirelessOn && objMod.WirelessBonus != null && objMod.WirelessBonus.InnerXml.Contains("<sensor>"))
                        {
                            string strSensor = objMod.WirelessBonus["sensor"].InnerText.Replace("Rating", objMod.Rating.ToString()).FastEscape('+');
                            intSensor = Math.Max(Convert.ToInt32(strSensor, GlobalOptions.InvariantCultureInfo), intSensor);
                        }
                    }
                }

                // Step through all the Gear looking for the Sensor Array that was built it. Set the rating to the current Sensor value.
                // The display value of this gets updated by UpdateSensor when RefreshSelectedVehicle gets called.
                foreach (Gear objGear in _lstGear)
                {
                    if (objGear.Category == "Sensors" && objGear.Name == "Sensor Array" && objGear.IncludedInParent)
                    {
                        objGear.MaxRating = Math.Max(intSensor, 0);
                        objGear.Rating = Math.Max(intSensor, 0);
                    }
                    break;
                }

                return intSensor;
            }
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get
            {
                return _strNotes;
            }
            set
            {
                _strNotes = value;
            }
        }

        /// <summary>
        /// A custom name for the Vehicle assigned by the player.
        /// </summary>
        public string VehicleName
        {
            get
            {
                return _strVehicleName;
            }
            set
            {
                _strVehicleName = value;
            }
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(_strVehicleName))
            {
                strReturn += " (\"" + _strVehicleName + "\")";
            }

            return strReturn;
        }

        /// <summary>
        /// Locations.
        /// </summary>
        public IList<string> Locations
        {
            get
            {
                return _lstLocations;
            }
        }

        /// <summary>
        /// Whether or not the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
        /// </summary>
        public bool BlackMarketDiscount
        {
            get
            {
                return _blnBlackMarketDiscount;
            }
            set
            {
                _blnBlackMarketDiscount = value;
            }
        }

        /// <summary>
        /// Whether or not the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
        /// </summary>
        public bool DealerConnectionDiscount
        {
            get
            {
                return UpdateDealerConnectionDiscount();
            }
        }

        /// <summary>
        /// Update info on Whether or not the Vehicle's cost should be discounted by 10% through the Dealer Connection Quality.
        /// </summary>
        public bool UpdateDealerConnectionDiscount()
        {
            foreach (Improvement objImprovement in _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DealerConnection))
            {
                if (
                        (objImprovement.UniqueName == "Drones" && (
                            _strCategory.StartsWith("Drones"))) ||
                        (objImprovement.UniqueName == "Aircraft" && (
                            _strCategory == "Fixed-Wing Aircraft" ||
                            _strCategory == "LTAV" ||
                            _strCategory == "Rotorcraft" ||
                            _strCategory == "VTOL/VSTOL")) ||
                        (objImprovement.UniqueName == "Watercraft" && (
                            _strCategory == "Boats" ||
                            _strCategory == "Submarines")) ||
                        (objImprovement.UniqueName == "Groundcraft" && (
                            _strCategory == "Bikes" ||
                            _strCategory == "Cars" ||
                            _strCategory == "Trucks" ||
                            _strCategory == "Municipal/Construction" ||
                            _strCategory == "Corpsec/Police/Military"))
                        )
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// The number of Slots on the Vehicle that are used by Mods.
        /// </summary>
        public int SlotsUsed
        {
            get
            {
                return _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed).AsParallel().Sum(objMod => objMod.CalculatedSlots) + WeaponMounts.Where(wm => !wm.IncludedInVehicle && wm.Installed).AsParallel().Sum(wm => wm.CalculatedSlots);
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
                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    // Mods that are included with a Vehicle by default do not count toward the Slots used.
                    if (!objMod.IncludedInVehicle && objMod.Installed)
                    {
                        if (objMod.CalculatedSlots < 0)
                            //You recieve only one additional Mod Point from Downgrades
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
                intDroneModSlots += WeaponMounts.Where(wm => !wm.IncludedInVehicle && wm.Installed).AsParallel().Sum(wm => wm.CalculatedSlots);

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

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
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
                                    intActualSlots -= 1;
                                }
                                break;
                            case "Speed":
                                intActualSlots -= _intSpeed;
                                if (!blnSpeed)
                                {
                                    blnSpeed = true;
                                    intActualSlots -= 1;
                                }
                                break;
                            case "Acceleration":
                                intActualSlots -= _intAccel;
                                if (!blnAccel)
                                {
                                    blnAccel = true;
                                    intActualSlots -= 1;
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
                                    intActualSlots -= 1;
                                }
                                break;
                        }

                        if (intActualSlots < 0)
                            intActualSlots = 0;

                        intModSlotsUsed += intActualSlots;
                    }
                }
                intModSlotsUsed += WeaponMounts.Where(wm => !wm.IncludedInVehicle && wm.Installed).AsParallel().Sum(wm => wm.CalculatedSlots);
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

                foreach (VehicleMod objMod in _lstVehicleMods.AsParallel())
                {
                    // Do not include the price of Mods that are part of the base configureation.
                    if (!objMod.IncludedInVehicle)
                    {
                        decCost += objMod.TotalCost;
                    }
                    else
                    {
                        // If the Mod is a part of the base config, check the items attached to it since their cost still counts.
                        decCost += objMod.Weapons.AsParallel().Sum(objWeapon => objWeapon.TotalCost);
                        decCost += objMod.Cyberware.AsParallel().Sum(objCyberware => objCyberware.TotalCost);
                    }
                }
                decCost += WeaponMounts.AsParallel().Sum(wm => wm.TotalCost);
                decCost += _lstGear.AsParallel().Sum(objGear => objGear.TotalCost);

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
                decimal decCost = Convert.ToDecimal(_strCost, GlobalOptions.InvariantCultureInfo);
                if (BlackMarketDiscount)
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
                char chrFirstCharacter = (char)0;
                // First check for mods that overwrite the seat value
                int intTotalSeats = Seats;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
                    XmlNode objBonusNode = null;
                    if (objMod.Bonus != null && objMod.Bonus.InnerXml.Contains("<seats>"))
                        objBonusNode = objMod.Bonus;
                    if (objMod.WirelessOn && objMod.WirelessBonus != null && objMod.WirelessBonus.InnerXml.Contains("<seats>"))
                        objBonusNode = objMod.WirelessBonus;
                    if (objBonusNode != null)
                    {
                        chrFirstCharacter = objBonusNode["seats"].InnerText[0];
                        if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                        {
                            intTotalSeats = Math.Max(Convert.ToInt32(objBonusNode["seats"].InnerText.Replace("Rating", objMod.Rating.ToString())), intTotalSeats);
                        }
                    }
                }

                // Then check for mods that modify the seat value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSeats = 0;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
                    if (objMod.Bonus != null && objMod.Bonus.InnerXml.Contains("<seats>"))
                    {
                        chrFirstCharacter = objMod.Bonus["seats"].InnerText[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing seat number, evaluate the expression.
                            intTotalBonusSeats += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(objMod.Bonus["seats"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Seats", intTotalSeats.ToString())), GlobalOptions.InvariantCultureInfo);
                        }
                    }
                    if (objMod.WirelessOn && objMod.WirelessBonus != null && objMod.WirelessBonus.InnerXml.Contains("<seats>"))
                    {
                        chrFirstCharacter = objMod.WirelessBonus["seats"].InnerText[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing seat number, evaluate the expression.
                            intTotalBonusSeats += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(objMod.WirelessBonus["seats"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Seats", intTotalSeats.ToString())), GlobalOptions.InvariantCultureInfo);
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
                char chrFirstCharacter = (char)0;
                int intTotalSpeed = Speed;
                int intBaseOffroadSpeed = OffroadSpeed;
                int intTotalArmor = 0;

                // First check for mods that overwrite the speed value or add to armor
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
                    if (objMod.Bonus != null)
                    {
                        string strSpeed = objMod.Bonus["speed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intTotalSpeed = Math.Max(intTotalSpeed, Convert.ToInt32(strSpeed.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        strSpeed = objMod.Bonus["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadSpeed = Math.Max(intBaseOffroadSpeed, Convert.ToInt32(strSpeed.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        if (IsDrone && GlobalOptions.Dronemods)
                        {
                            string strArmor = objMod.Bonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString()));
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
                                intTotalSpeed = Math.Max(intTotalSpeed, Convert.ToInt32(strSpeed.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        strSpeed = objMod.WirelessBonus["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadSpeed = Math.Max(intBaseOffroadSpeed, Convert.ToInt32(strSpeed.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        if (IsDrone && GlobalOptions.Dronemods)
                        {
                            string strArmor = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString()));
                            }
                        }
                    }
                }

                // Then check for mods that modify the speed value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSpeed = 0;
                int intTotalBonusOffroadSpeed = 0;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
                    if (objMod.Bonus != null)
                    {
                        string strSpeed = objMod.Bonus["speed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing speed number, evaluate the expression.
                                intTotalBonusSpeed += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strSpeed.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Speed", intTotalSpeed.ToString())), GlobalOptions.CultureInfo);
                            }
                        }
                        strSpeed = objMod.Bonus["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing speed number, evaluate the expression.
                                intTotalBonusOffroadSpeed += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strSpeed.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadSpeed", intBaseOffroadSpeed.ToString())), GlobalOptions.InvariantCultureInfo);
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
                                intTotalBonusSpeed += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strSpeed.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Speed", intTotalSpeed.ToString())), GlobalOptions.CultureInfo);
                            }
                        }
                        strSpeed = objMod.WirelessBonus["offroadspeed"]?.InnerText;
                        if (!string.IsNullOrEmpty(strSpeed))
                        {
                            chrFirstCharacter = strSpeed[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing speed number, evaluate the expression.
                                intTotalBonusOffroadSpeed += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strSpeed.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadSpeed", intBaseOffroadSpeed.ToString())), GlobalOptions.InvariantCultureInfo);
                            }
                        }
                    }
                }

                // Reduce speed of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 3, 0);

                if (Speed != OffroadSpeed || intTotalSpeed + intTotalBonusSpeed != intBaseOffroadSpeed + intTotalBonusOffroadSpeed)
                {
                    return ((intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString() + '/' + (intBaseOffroadSpeed + intTotalBonusOffroadSpeed - intPenalty).ToString());
                }
                else
                {
                    return ((intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString());
                }
            }
        }

        /// <summary>
        /// Total Accel of the Vehicle including Modifications.
        /// </summary>
        public string TotalAccel
        {
            get
            {
                char chrFirstCharacter = (char)0;
                int intTotalAccel = Accel;
                int intBaseOffroadAccel = OffroadAccel;
                int intTotalArmor = 0;

                // First check for mods that overwrite the accel value or add to armor
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
                    if (objMod.Bonus != null)
                    {
                        string strAccel = objMod.Bonus["accel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intTotalAccel = Math.Max(intTotalAccel, Convert.ToInt32(strAccel.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        strAccel = objMod.Bonus["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadAccel = Math.Max(intBaseOffroadAccel, Convert.ToInt32(strAccel.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        if (IsDrone && GlobalOptions.Dronemods)
                        {
                            string strArmor = objMod.Bonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString()));
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
                                intTotalAccel = Math.Max(intTotalAccel, Convert.ToInt32(strAccel.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        strAccel = objMod.WirelessBonus["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadAccel = Math.Max(intBaseOffroadAccel, Convert.ToInt32(strAccel.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        if (IsDrone && GlobalOptions.Dronemods)
                        {
                            string strArmor = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString()));
                            }
                        }
                    }
                }

                // Then check for mods that modify the accel value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusAccel = 0;
                int intTotalBonusOffroadAccel = 0;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
                    if (objMod.Bonus != null)
                    {
                        string strAccel = objMod.Bonus["accel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                intTotalBonusAccel += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAccel.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Accel", intTotalAccel.ToString())), GlobalOptions.CultureInfo);
                            }
                        }
                        strAccel = objMod.Bonus["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                intTotalBonusOffroadAccel += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAccel.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadAccel", intBaseOffroadAccel.ToString())), GlobalOptions.InvariantCultureInfo);
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
                                intTotalBonusAccel += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAccel.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Accel", intTotalAccel.ToString())), GlobalOptions.CultureInfo);
                            }
                        }
                        strAccel = objMod.WirelessBonus["offroadaccel"]?.InnerText;
                        if (!string.IsNullOrEmpty(strAccel))
                        {
                            chrFirstCharacter = strAccel[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                intTotalBonusOffroadAccel += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAccel.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadAccel", intBaseOffroadAccel.ToString())), GlobalOptions.InvariantCultureInfo);
                            }
                        }
                    }
                }

                // Reduce acceleration of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 6, 0);

                if (Accel != OffroadAccel || intTotalAccel + intTotalBonusAccel != intBaseOffroadAccel + intTotalBonusOffroadAccel)
                {
                    return ((intTotalAccel + intTotalBonusAccel - intPenalty).ToString() + '/' + (intBaseOffroadAccel + intTotalBonusOffroadAccel - intPenalty).ToString());
                }
                else
                {
                    return ((intTotalAccel + intTotalBonusAccel - intPenalty).ToString());
                }
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
                
                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    if (!objMod.IncludedInVehicle && objMod.Installed)
                    {
                        // Add the Modification's Body to the Vehicle's base Body.
                        string strBodyElement = objMod.Bonus?["body"]?.InnerText;
                        if (!string.IsNullOrEmpty(strBodyElement))
                        {
                            strBodyElement = strBodyElement.TrimStart('+');
                            if (strBodyElement.Contains("Rating"))
                            {
                                // If the cost is determined by the Rating, evaluate the expression.
                                string strBody = strBodyElement.Replace("Rating", objMod.Rating.ToString());
                                intBody += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strBody));
                            }
                            else
                            {
                                intBody += Convert.ToInt32(strBodyElement);
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
                                    string strBody = strBodyElement.Replace("Rating", objMod.Rating.ToString());
                                    intBody += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strBody));
                                }
                                else
                                {
                                    intBody += Convert.ToInt32(strBodyElement.TrimStart('+'));
                                }
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
                char chrFirstCharacter = (char)0;
                int intBaseHandling = Handling;
                int intBaseOffroadHandling = OffroadHandling;
                int intTotalArmor = 0;

                // First check for mods that overwrite the handling value or add to armor
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
                    if (objMod.Bonus != null)
                    {
                        string strHandling = objMod.Bonus["handling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseHandling = Math.Max(intBaseHandling, Convert.ToInt32(strHandling.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        strHandling = objMod.Bonus["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadHandling = Math.Max(intBaseOffroadHandling, Convert.ToInt32(strHandling.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        if (IsDrone && GlobalOptions.Dronemods)
                        {
                            string strArmor = objMod.Bonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString()));
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
                                intBaseHandling = Math.Max(intBaseHandling, Convert.ToInt32(strHandling.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        strHandling = objMod.WirelessBonus["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                            {
                                intBaseOffroadHandling = Math.Max(intBaseOffroadHandling, Convert.ToInt32(strHandling.Replace("Rating", objMod.Rating.ToString())));
                            }
                        }
                        if (IsDrone && GlobalOptions.Dronemods)
                        {
                            string strArmor = objMod.WirelessBonus["armor"]?.InnerText;
                            if (!string.IsNullOrEmpty(strArmor))
                            {
                                intTotalArmor = Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString()));
                            }
                        }
                    }
                }

                // Then check for mods that modify the handling value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusHandling = 0;
                int intTotalBonusOffroadHandling = 0;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed))
                {
                    if (objMod.Bonus != null)
                    {
                        string strHandling = objMod.Bonus["handling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                intTotalBonusHandling += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strHandling.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Handling", intBaseHandling.ToString())), GlobalOptions.CultureInfo);
                            }
                        }
                        strHandling = objMod.Bonus["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                intTotalBonusOffroadHandling += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strHandling.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadHandling", intBaseOffroadHandling.ToString())), GlobalOptions.InvariantCultureInfo);
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
                                intTotalBonusHandling += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strHandling.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Handling", intBaseHandling.ToString())), GlobalOptions.CultureInfo);
                            }
                        }
                        strHandling = objMod.WirelessBonus["offroadhandling"]?.InnerText;
                        if (!string.IsNullOrEmpty(strHandling))
                        {
                            chrFirstCharacter = strHandling[0];
                            if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                            {
                                // If the bonus is determined by the existing accel number, evaluate the expression.
                                intTotalBonusOffroadHandling += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strHandling.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadHandling", intBaseOffroadHandling.ToString())), GlobalOptions.InvariantCultureInfo);
                            }
                        }
                    }
                }

                // Reduce handling of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 3, 0);

                if (Handling != OffroadHandling || intBaseHandling + intTotalBonusHandling != intBaseOffroadHandling + intTotalBonusOffroadHandling)
                {
                    return ((intBaseHandling + intTotalBonusHandling - intPenalty).ToString() + '/' + (intBaseOffroadHandling + intTotalBonusOffroadHandling - intPenalty).ToString());
                }
                else
                {
                    return ((intBaseHandling + intTotalBonusHandling - intPenalty).ToString());
                }
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
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => (!objMod.IncludedInVehicle && objMod.Installed)))
                {
                    string strArmor = objMod.Bonus?["armor"]?.InnerText;
                    if (!string.IsNullOrEmpty(strArmor))
                    {
                        intModArmor += Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString()));
                    }
                    if (objMod.WirelessOn)
                    {
                        strArmor = objMod.WirelessBonus?["armor"]?.InnerText;
                        if (!string.IsNullOrEmpty(strArmor))
                        {
                            intModArmor += Convert.ToInt32(strArmor.Replace("Rating", objMod.Rating.ToString()));
                        }
                    }
                }
                // Rigger5 Drone Armor starts at 0. All other vehicles start with their base armor.
                if (IsDrone && GlobalOptions.Dronemods && intModArmor <= 0)
                {
                    intModArmor += _intArmor;
                }
                // Drones have no theoretical armor cap in the optional rules, otherwise, it's capped
                if (!IsDrone || !GlobalOptions.Dronemods)
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

                if (IsDrone && _objCharacter.Options.DroneArmorMultiplierEnabled)
                {
                    intReturn *= _objCharacter.Options.DroneArmorMultiplier;
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
                if (IsDrone && !_objCharacter.IgnoreRules && GlobalOptions.DronemodsMaximumPilot)
                {
                    return Math.Max(_intPilot * 2, 1);
                }
                return int.MaxValue;
            }
        }

        private static readonly string[] s_LstModCategoryStrings = { "Powertrain", "Protection", "Weapons", "Body", "Electromagnetic", "Cosmetic" };
        /// <summary>
        /// Check if the vehicle is over capacity in any category
        /// </summary>
        public bool OverR5Capacity(string strCheckCapacity = "")
        {
            return !string.IsNullOrEmpty(strCheckCapacity) && s_LstModCategoryStrings.Contains(strCheckCapacity)
                ? CalcCategoryUsed(strCheckCapacity) > CalcCategoryAvail(strCheckCapacity)
                : s_LstModCategoryStrings.Any(strCategory => CalcCategoryUsed(strCategory) > CalcCategoryAvail(strCategory));
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string PowertrainModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddPowertrainModSlots;
            return $"{intTotal - PowertrainModSlots + intModSlots}/{intTotal}";
        }

        /// <summary>
        /// Calculate remaining Powertrain slots
        /// </summary>
        public int PowertrainModSlots
        {
            get
            {
                int intPowertrain = _intBody + _intAddPowertrainModSlots;

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Category == "Powertrain"))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    int intSlots = objMod.CalculatedSlots;
                    if (intSlots > 0)
                        intPowertrain -= intSlots;
                }

                return intPowertrain;
            }
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string ProtectionModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddProtectionModSlots;
            return $"{intTotal - ProtectionModSlots + intModSlots}/{intTotal}";
        }

        /// <summary>
        /// Calculate remaining Protection slots
        /// </summary>
        public int ProtectionModSlots
        {
            get
            {
                int intProtection = _intBody + _intAddProtectionModSlots;

                foreach (VehicleMod objMod in _lstVehicleMods.AsParallel().Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Category == "Protection"))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    int intSlots = objMod.CalculatedSlots;
                    if (intSlots > 0)
                        intProtection -= intSlots;
                }
                return intProtection;
            }
        }

        /// <summary>
        /// Display the Weapon Mod Slots as Used/Total
        /// </summary>
        public string WeaponModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddWeaponModSlots;
            return $"{intTotal - WeaponModSlots + intModSlots}/{intTotal}";
        }

        /// <summary>
        /// Calculate remaining Weapon slots
        /// </summary>
        public int WeaponModSlots
        {
            get
            {
                int intWeaponsmod = _intBody + _intAddWeaponModSlots;

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Category == "Weapons"))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    int intSlots = objMod.CalculatedSlots;
                    if (intSlots > 0)
                        intWeaponsmod -= intSlots;
                }
                foreach (WeaponMount wm in WeaponMounts.AsParallel().Where(wm => !wm.IncludedInVehicle && wm.Installed))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    int intSlots = wm.CalculatedSlots;
                    if (intSlots > 0)
                        intWeaponsmod -= intSlots;
                }

                return intWeaponsmod;
            }
        }

        /// <summary>
        /// Display the Body Mod Slots as Used/Total
        /// </summary>
        public string BodyModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddBodyModSlots;
            return $"{intTotal - BodyModSlots + intModSlots}/{intTotal}";
        }

        /// <summary>
        /// Calculate remaining Bodymod slots
        /// </summary>
        public int BodyModSlots
        {
            get
            {
                int intBodymod = _intBody + _intAddBodyModSlots;

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Category == "Body"))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    int intSlots = objMod.CalculatedSlots;
                    if (intSlots > 0)
                        intBodymod -= intSlots;
                }

                return intBodymod;
            }
        }

        /// <summary>
        /// Display the Electromagnetic Mod Slots as Used/Total
        /// </summary>
        public string ElectromagneticModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddElectromagneticModSlots;
            return $"{intTotal - ElectromagneticModSlots + intModSlots}/{intTotal}";
        }

        /// <summary>
        /// Calculate remaining Electromagnetic slots
        /// </summary>
        public int ElectromagneticModSlots
        {
            get
            {
                int intElectromagnetic = _intBody + _intAddElectromagneticModSlots;

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Category == "Electromagnetic"))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    int intSlots = objMod.CalculatedSlots;
                    if (intSlots > 0)
                        intElectromagnetic -= intSlots;
                }

                return intElectromagnetic;
            }
        }

        /// <summary>
        /// Calculate remaining Cosmetic slots
        /// </summary>
        public int CosmeticModSlots
        {
            get
            {
                int intCosmetic = _intBody +_intAddCosmeticModSlots;

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Category == "Cosmetic"))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    int intSlots = objMod.CalculatedSlots;
                    if (intSlots > 0)
                        intCosmetic -= intSlots;
                }

                return intCosmetic;
            }
        }

        /// <summary>
        /// Display the Cosmetic Mod Slots as Used/Total
        /// </summary>
        public string CosmeticModSlotsUsed(int intModSlots = 0)
        {
            int intTotal = _intBody + _intAddCosmeticModSlots;
            return $"{intTotal - CosmeticModSlots + intModSlots}/{intTotal}";
        }

        /// <summary>
        /// Vehicle's Maneuver AutoSoft Rating.
        /// </summary>
        public int Maneuver
        {
            get
            {
                Gear objGear = Gear.DeepFirstOrDefault(x => x.Children, x => x.Name == "[Model] Maneuvering Autosoft" && x.Extra == Name && x.InternalId != Guid.Empty.ToString());
                if (objGear != null)
                {
                    return objGear.Rating;
                }
                return 0;
            }
        }

        private XmlNode _objCachedMyXmlNode = null;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("vehicles.xml", strLanguage)?.SelectSingleNode("/chummer/vehicles/vehicle[id = \"" + _sourceID.ToString() + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        public bool IsProgram => false;

        /// <summary>
        /// Device rating string for Cyberware. If it's empty, then GetBaseMatrixAttribute for Device Rating will fetch the grade's DR.
        /// </summary>
        public string DeviceRating
        {
            get
            {
                return _strDeviceRating;
            }
            set
            {
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
                return _strAttack;
            }
            set
            {
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
                return _strSleaze;
            }
            set
            {
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
                return _strDataProcessing;
            }
            set
            {
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
                return _strFirewall;
            }
            set
            {
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
                return _strModAttack;
            }
            set
            {
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
                return _strModSleaze;
            }
            set
            {
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
                return _strModDataProcessing;
            }
            set
            {
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
                return _strModFirewall;
            }
            set
            {
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
                return _strAttributeArray;
            }
            set
            {
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
                return _strModAttributeArray;
            }
            set
            {
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
                return _strOverclocked;
            }
            set
            {
                _strOverclocked = value;
            }
        }

        /// <summary>
        /// Empty for Vehicles.
        /// </summary>
        public string CanFormPersona { get => string.Empty; set { } }

        public bool IsCommlink => Gear.Any(x => x.CanFormPersona.Contains("Parent")) && this.GetTotalMatrixAttribute("Device Rating") > 0;

        /// <summary>
        /// 0 for Vehicles.
        /// </summary>
        public int BonusMatrixBoxes { get => 0; set { } }

        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intReturn = 0;
                foreach (Gear objGear in Gear)
                {
                    if (objGear.Equipped)
                    {
                        intReturn += objGear.TotalBonusMatrixBoxes;
                    }
                }
                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    if (objMod.Bonus != null)
                    {
                        // Add the Modification's Device Rating to the Vehicle's base Device Rating.
                        if (objMod.Bonus.InnerXml.Contains("<matrixcmbonus>"))
                            intReturn += Convert.ToInt32(objMod.Bonus["matrixcmbonus"].InnerText);
                    }
                    if (objMod.WirelessOn && objMod.WirelessBonus != null)
                    {
                        // Add the Modification's Wireless Device Rating to the Vehicle's base Device Rating if wireless is on.
                        if (objMod.WirelessBonus.InnerXml.Contains("<matrixcmbonus>"))
                            intReturn += Convert.ToInt32(objMod.WirelessBonus["matrixcmbonus"].InnerText);
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
            get
            {
                return _strProgramLimit;
            }
            set
            {
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
                return _blnCanSwapAttributes;
            }
            set
            {
                _blnCanSwapAttributes = value;
            }
        }

        public IList<IHasMatrixAttributes> ChildrenWithMatrixAttributes
        {
            get
            {
                return Gear.Cast<IHasMatrixAttributes>().ToList();
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Calculate remaining slots by provided Category
        /// </summary>
        public int CalcCategoryUsed(string strCategory)
        {
            int intBase = 0;

            foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Category == strCategory))
            {
                // Subtract the Modification's Slots from the Vehicle's base Body.
                int intSlots = objMod.CalculatedSlots;
                if (intSlots > 0)
                    intBase += intSlots;
            }

            return intBase;
        }

        /// <summary>
        /// Total Number of Slots a Vehicle has used for Modifications. (Rigger 5)
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
            foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Category == strCategory))
            {
                int intSlots = objMod.CalculatedSlots;
                if (intSlots < 0)
                    intBase -= intSlots;
            }
            return intBase;
        }

        /// <summary>
        /// Whether or not the Vehicle has the Modular Electronics Vehicle Modification installed.
        /// </summary>
        public bool HasModularElectronics()
        {
            return _lstVehicleMods.Any(objMod => objMod.Name == "Modular Electronics");
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
                    case "Attack":
                    case "Sleaze":
                    default:
                        return 0;
                }
            }

            if (strExpression.Contains('{') || strExpression.Contains('+') || strExpression.Contains('-') || strExpression.Contains('*') || strExpression.Contains("div"))
            {
                StringBuilder objValue = new StringBuilder(strExpression);
                List<IHasMatrixAttributes> lstChildrenWithMatrixAttributes = new List<IHasMatrixAttributes>(ChildrenWithMatrixAttributes);
                foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                {
                    if (lstChildrenWithMatrixAttributes.Count > 0 && strExpression.Contains("{Children " + strMatrixAttribute + "}"))
                    {
                        int intTotalChildrenValue = 0;
                        foreach (Gear loopGear in lstChildrenWithMatrixAttributes)
                        {
                            if (loopGear.Equipped)
                            {
                                intTotalChildrenValue += loopGear.GetBaseMatrixAttribute(strMatrixAttribute);
                            }
                        }
                        objValue.Replace("{Children " + strMatrixAttribute + "}", intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    }
                }
                foreach (string strCharAttributeName in Attributes.AttributeSection.AttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "}", () => _objCharacter.GetAttribute(strCharAttributeName).TotalValue.ToString());
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "Base}", () => _objCharacter.GetAttribute(strCharAttributeName).TotalBase.ToString());
                }
                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                return Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(objValue.ToString())));
            }
            int.TryParse(strExpression, out int intReturn);
            return intReturn;
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                intReturn += 1;
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
                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    XmlNode objBonus = objMod.Bonus?[strAttributeNodeName];
                    if (objBonus != null)
                    {
                        intReturn += Convert.ToInt32(objBonus.InnerText);
                    }
                    objBonus = objMod.WirelessOn ? objMod.WirelessBonus?[strAttributeNodeName] : null;
                    if (objBonus != null)
                    {
                        intReturn += Convert.ToInt32(objBonus.InnerText);
                    }
                }
            }

            if (!strAttributeName.StartsWith("Mod "))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Gear loopGear in Gear)
            {
                if (loopGear.Equipped)
                {
                    intReturn += loopGear.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }
        #endregion
    }
}
