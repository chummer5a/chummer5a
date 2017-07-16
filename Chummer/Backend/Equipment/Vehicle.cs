using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Vehicle.
    /// </summary>
    public class Vehicle : INamedItemWithGuid
    {
        private Guid _guiID = new Guid();
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
        private bool _blnHomeNode = false;
        private List<VehicleMod> _lstVehicleMods = new List<VehicleMod>();
        private List<Gear> _lstGear = new List<Gear>();
        private List<Weapon> _lstWeapons = new List<Weapon>();
        private string _strNotes = string.Empty;
        private string _strAltName = string.Empty;
        private string _strAltCategory = string.Empty;
        private string _strAltPage = string.Empty;
        private List<string> _lstLocations = new List<string>();
        private bool _blnDealerConnectionDiscount = false;
        private bool _blnBlackMarketDiscount = false;

        private readonly Character _objCharacter;

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
        public void Create(XmlNode objXmlVehicle, TreeNode objNode, ContextMenuStrip cmsVehicle, ContextMenuStrip cmsVehicleGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear = null, bool blnCreateChildren = true)
        {
            objXmlVehicle.TryGetField("id", Guid.TryParse, out _sourceID);
            objXmlVehicle.TryGetStringFieldQuickly("name", ref _strName);
            objXmlVehicle.TryGetStringFieldQuickly("category", ref _strCategory);
            if (objXmlVehicle["handling"] != null)
            {
                //Some vehicles have different Offroad Handling speeds. If so, we want to split this up for use with mods and such later.
                if (objXmlVehicle["handling"].InnerText.Contains('/'))
                {
                    string[] strHandlingArray = objXmlVehicle["handling"].InnerText.Split('/');
                    int.TryParse(strHandlingArray[0], out _intHandling);
                    int.TryParse(strHandlingArray[1], out _intOffroadHandling);
                }
                else
                {
                    int.TryParse(objXmlVehicle["handling"].InnerText, out _intHandling);
                    _intOffroadHandling = _intHandling;
                }
            }
            if (objXmlVehicle["accel"] != null)
            {
                if (objXmlVehicle["accel"].InnerText.Contains('/'))
                {
                    string[] strAccelArray = objXmlVehicle["accel"].InnerText.Split('/');
                    int.TryParse(strAccelArray[0], out _intAccel);
                    int.TryParse(strAccelArray[1], out _intOffroadAccel);
                }
                else
                {
                    int.TryParse(objXmlVehicle["accel"].InnerText, out _intAccel);
                    _intOffroadAccel = _intAccel;
                }
            }
            if (objXmlVehicle["speed"] != null)
            {
                if (objXmlVehicle["speed"].InnerText.Contains('/'))
                {
                    string[] strSpeedArray = objXmlVehicle["speed"].InnerText.Split('/');
                    int.TryParse(strSpeedArray[0], out _intSpeed);
                    int.TryParse(strSpeedArray[1], out _intOffroadSpeed);
                }
                else
                {
                    int.TryParse(objXmlVehicle["speed"].InnerText, out _intSpeed);
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
            if (objXmlVehicle["cost"] != null)
            {
                // Check for a Variable Cost.
                if (objXmlVehicle["cost"].InnerText.StartsWith("Variable"))
                {
                    int intMin;
                    int intMax = 0;
                    string strCost = objXmlVehicle["cost"].InnerText.Replace("Variable", string.Empty).Trim("()".ToCharArray());
                    if (strCost.Contains("-"))
                    {
                        string[] strValues = strCost.Split('-');
                        int.TryParse(strValues[0], out intMin);
                        int.TryParse(strValues[1], out intMax);
                    }
                    else
                        int.TryParse(strCost.Replace("+", string.Empty), out intMin);

                    if (intMin != 0 || intMax != 0)
                    {
                        frmSelectNumber frmPickNumber = new frmSelectNumber();
                        if (intMax == 0)
                            intMax = 1000000;
                        frmPickNumber.Minimum = intMin;
                        frmPickNumber.Maximum = intMax;
                        frmPickNumber.Description = LanguageManager.Instance.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _strCost = frmPickNumber.SelectedValue.ToString();
                    }
                }
                else
                    _strCost = objXmlVehicle["cost"].InnerText;
            }
            objXmlVehicle.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlVehicle.TryGetStringFieldQuickly("page", ref _strPage);

            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("vehicles.xml");
                XmlNode objVehicleNode = objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + _strName + "\"]");
                if (objVehicleNode != null)
                {
                    objVehicleNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objVehicleNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                objVehicleNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                _strAltCategory = objVehicleNode?.Attributes?["translate"]?.InnerText;
            }

            objNode.Text = DisplayName;
            objNode.Tag = _guiID.ToString();

            // If there are any VehicleMods that come with the Vehicle, add them.
            if (objXmlVehicle.InnerXml.Contains("<mods>") && blnCreateChildren)
            {
                XmlDocument objXmlDocument = new XmlDocument();
                objXmlDocument = XmlManager.Instance.Load("vehicles.xml");

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

            // If there is any Gear that comes with the Vehicle, add them.
            if (objXmlVehicle.InnerXml.Contains("<gears>") && blnCreateChildren)
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("gear.xml");

                XmlNodeList objXmlGearList = objXmlVehicle.SelectNodes("gears/gear");
                foreach (XmlNode objXmlVehicleGear in objXmlGearList)
                {
                    XmlNode objXmlGear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlVehicleGear.InnerText + "\"]");
                    if (objXmlGear != null)
                    {
                        TreeNode objGearNode = new TreeNode();
                        Gear objGear = new Gear(_objCharacter);
                        int intRating = 0;
                        int intQty = 1;
                        string strForceValue = string.Empty;

                        if (objXmlVehicleGear.Attributes["rating"] != null)
                            intRating = Convert.ToInt32(objXmlVehicleGear.Attributes["rating"].InnerText);

                        int intMaxRating = intRating;
                        if (objXmlVehicleGear.Attributes["maxrating"] != null)
                            intMaxRating = Convert.ToInt32(objXmlVehicleGear.Attributes["maxrating"].InnerText);

                        if (objXmlVehicleGear.Attributes["qty"] != null)
                            intQty = Convert.ToInt32(objXmlVehicleGear.Attributes["qty"].InnerText);

                        if (objXmlVehicleGear.Attributes["select"] != null)
                            strForceValue = objXmlVehicleGear.Attributes["select"].InnerText;

                        List<Weapon> objWeapons = new List<Weapon>();
                        List<TreeNode> objWeaponNodes = new List<TreeNode>();
                        objGear.Create(objXmlGear, _objCharacter, objGearNode, intRating, objWeapons, objWeaponNodes, strForceValue);
                        objGear.Cost = "0";
                        objGear.Quantity = intQty;
                        objGear.MaxRating = intMaxRating;
                        objGear.IncludedInParent = true;
                        objGearNode.Text = objGear.DisplayName;
                        objGearNode.ContextMenuStrip = cmsVehicleGear;

                        foreach (Weapon objWeapon in objWeapons)
                            objWeapon.VehicleMounted = true;

                        _lstGear.Add(objGear);

                        objNode.Nodes.Add(objGearNode);
                        objNode.Expand();
                    }
                }
            }

            // If there are any Weapons that come with the Vehicle, add them.
            if (objXmlVehicle.InnerXml.Contains("<weapons>") && blnCreateChildren)
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");

                foreach (XmlNode objXmlWeapon in objXmlVehicle.SelectNodes("weapons/weapon"))
                {
                    bool blnAttached = false;
                    TreeNode objWeaponNode = new TreeNode();
                    Weapon objWeapon = new Weapon(_objCharacter);

                    XmlNode objXmlWeaponNode = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlWeapon["name"].InnerText + "\"]");
                    objWeapon.Create(objXmlWeaponNode, _objCharacter, objWeaponNode, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                    objWeapon.Cost = 0;
                    objWeapon.VehicleMounted = true;

                    // Find the first free Weapon Mount in the Vehicle.
                    foreach (VehicleMod objMod in _lstVehicleMods)
                    {
                        if ((objMod.Name.Contains("Weapon Mount") || (!String.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.Category) && objMod.Weapons.Count == 0)))
                        {
                            objMod.Weapons.Add(objWeapon);
                            foreach (TreeNode objModNode in objNode.Nodes)
                            {
                                if (objModNode.Tag.ToString() == objMod.InternalId)
                                {
                                    objWeaponNode.ContextMenuStrip = cmsVehicleWeapon;
                                    objModNode.Nodes.Add(objWeaponNode);
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
                            if (objMod.Name.Contains("Weapon Mount") || (!String.IsNullOrEmpty(objMod.WeaponMountCategories) && objMod.WeaponMountCategories.Contains(objWeapon.Category)))
                            {
                                objMod.Weapons.Add(objWeapon);
                                foreach (TreeNode objModNode in objNode.Nodes)
                                {
                                    if (objModNode.Tag.ToString() == objMod.InternalId)
                                    {
                                        objWeaponNode.ContextMenuStrip = cmsVehicleWeapon;
                                        objModNode.Nodes.Add(objWeaponNode);
                                        objModNode.Expand();
                                        blnAttached = true;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }

                    // Look for Weapon Accessories.
                    if (objXmlWeapon["accessories"] != null)
                    {
                        foreach (XmlNode objXmlAccessory in objXmlWeapon.SelectNodes("accessories/accessory"))
                        {
                            XmlNode objXmlAccessoryNode = objXmlWeaponDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + objXmlAccessory["name"].InnerText + "\"]");
                            WeaponAccessory objMod = new WeaponAccessory(_objCharacter);
                            TreeNode objModNode = new TreeNode();
                            string strMount = "Internal";
                            int intRating = 0;
                            objXmlAccessory.TryGetStringFieldQuickly("mount", ref strMount);
                            string strExtraMount = "None";
                            if (objXmlAccessory.InnerXml.Contains("<extramount>"))
                                strMount = objXmlAccessory["extramount"].InnerText;
                            objMod.Create(objXmlAccessoryNode, objModNode, new Tuple<string, string>(strMount, strExtraMount), intRating, cmsVehicleGear, false, blnCreateChildren);

                            objMod.Cost = "0";
                            objModNode.ContextMenuStrip = cmsVehicleWeaponAccessory;

                            objWeapon.WeaponAccessories.Add(objMod);

                            objWeaponNode.Nodes.Add(objModNode);
                            objWeaponNode.Expand();
                        }
                    }
                }
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
            objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("vehiclename", _strVehicleName);
            objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
            objWriter.WriteStartElement("mods");
            foreach (VehicleMod objMod in _lstVehicleMods)
                objMod.Save(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
            {
                // Use the Gear's SubClass if applicable.
                if (objGear.GetType() == typeof(Commlink))
                {
                    Commlink objCommlink = new Commlink(_objCharacter);
                    objCommlink = (Commlink)objGear;
                    objCommlink.Save(objWriter);
                }
                else
                {
                    objGear.Save(objWriter);
                }
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
                _blnHomeNode = false;
            }
            else
            {
                _guiID = Guid.Parse(objNode["guid"].InnerText);
                objNode.TryGetBoolFieldQuickly("homenode", ref _blnHomeNode);
            }

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (!objNode.TryGetField("id", Guid.TryParse, out _sourceID))
            {
                XmlDocument doc = XmlManager.Instance.Load("vehicles.xml");
                XmlNode sourceNode = doc.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + Name + "\"]");
                sourceNode.TryGetField("id", Guid.TryParse, out _sourceID);
            }
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            if (objNode["handling"] != null)
            {
                //Some vehicles have different Offroad Handling speeds. If so, we want to split this up for use with mods and such later.
                if (objNode["handling"].InnerText.Contains('/'))
                {
                    _intHandling = Convert.ToInt32(objNode["handling"].InnerText.Split('/')[0]);
                    _intOffroadHandling = Convert.ToInt32(objNode["handling"].InnerText.Split('/')[1]);
                }
                else
                {
                    _intHandling = Convert.ToInt32(objNode["handling"].InnerText);
                    if (objNode.InnerXml.Contains("offroadhandling"))
                    {
                        _intOffroadHandling = Convert.ToInt32(objNode["offroadhandling"].InnerText);
                    }
                }
            }
            if (objNode["accel"] != null)
            {
                if (objNode["accel"].InnerText.Contains('/'))
                {
                    _intAccel = Convert.ToInt32(objNode["accel"].InnerText.Split('/')[0]);
                    _intOffroadAccel = Convert.ToInt32(objNode["accel"].InnerText.Split('/')[1]);
                }
                else
                {
                    _intAccel = Convert.ToInt32(objNode["accel"].InnerText);
                    if (objNode.InnerXml.Contains("offroadaccel"))
                    {
                        _intOffroadAccel = Convert.ToInt32(objNode["offroadaccel"].InnerText);
                    }
                }
            }
            if (objNode["speed"] != null)
            {
                if (objNode["speed"].InnerText.Contains('/'))
                {
                    _intSpeed = Convert.ToInt32(objNode["speed"].InnerText.Split('/')[0]);
                    _intOffroadSpeed = Convert.ToInt32(objNode["speed"].InnerText.Split('/')[1]);
                }
                else
                {
                    _intSpeed = Convert.ToInt32(objNode["speed"].InnerText);
                    if (objNode.InnerXml.Contains("offroadspeed"))
                    {
                        _intOffroadSpeed = Convert.ToInt32(objNode["offroadspeed"].InnerText);
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
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetInt32FieldQuickly("physicalcmfilled", ref _intPhysicalCMFilled);
            objNode.TryGetStringFieldQuickly("vehiclename", ref _strVehicleName);

            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("vehicles.xml");
                XmlNode objVehicleNode = objXmlDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + _strName + "\"]");
                if (objVehicleNode != null)
                {
                    objVehicleNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objVehicleNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                objVehicleNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                _strAltCategory = objVehicleNode?.Attributes?["translate"]?.InnerText;
            }

            if (objNode.InnerXml.Contains("<mods>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("mods/mod");
                foreach (XmlNode nodChild in nodChildren)
                {
                    VehicleMod objMod = new VehicleMod(_objCharacter);
                    objMod.Load(nodChild, this, blnCopy);
                    objMod.Parent = this;
                    _lstVehicleMods.Add(objMod);
                }
            }

            if (objNode.InnerXml.Contains("<gears>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
                foreach (XmlNode nodChild in nodChildren)
                {
                    switch (nodChild["category"].InnerText)
                    {
                        case "Commlinks":
                        case "Commlink Accessories":
                        case "Cyberdecks":
                        case "Rigger Command Consoles":
                            Commlink objCommlink = new Commlink(_objCharacter);
                            objCommlink.Load(nodChild, blnCopy);
                            _lstGear.Add(objCommlink);
                            break;
                        default:
                            Gear objGear = new Gear(_objCharacter);
                            objGear.Load(nodChild, blnCopy);
                            _lstGear.Add(objGear);
                            break;
                    }
                }
            }

            if (objNode.InnerXml.Contains("<weapons>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("weapons/weapon");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Weapon objWeapon = new Weapon(_objCharacter);
                    objWeapon.Load(nodChild, blnCopy);
                    objWeapon.VehicleMounted = true;
                    if (objWeapon.UnderbarrelWeapons.Count > 0)
                    {
                        foreach (Weapon objUnderbarrel in objWeapon.UnderbarrelWeapons)
                            objUnderbarrel.VehicleMounted = true;
                    }
                    _lstWeapons.Add(objWeapon);
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("dealerconnection", ref _blnDealerConnectionDiscount);

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
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("vehicle");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("category", DisplayCategory);
            objWriter.WriteElementString("handling", TotalHandling);
            objWriter.WriteElementString("accel", TotalAccel);
            objWriter.WriteElementString("speed", TotalSpeed);
            objWriter.WriteElementString("pilot", Pilot.ToString());
            objWriter.WriteElementString("body", TotalBody.ToString());
            objWriter.WriteElementString("armor", TotalArmor.ToString());
            objWriter.WriteElementString("seats", TotalSeats.ToString());
            objWriter.WriteElementString("sensor", CalculatedSensor.ToString());
            objWriter.WriteElementString("avail", CalculatedAvail);
            objWriter.WriteElementString("cost", TotalCost.ToString());
            objWriter.WriteElementString("owncost", OwnCost.ToString());
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("physicalcm", PhysicalCM.ToString());
            objWriter.WriteElementString("matrixcm", MatrixCM.ToString());
            objWriter.WriteElementString("physicalcmfilled", _intPhysicalCMFilled.ToString());
            objWriter.WriteElementString("vehiclename", _strVehicleName);
            objWriter.WriteElementString("devicerating", Pilot.ToString());
            objWriter.WriteElementString("maneuver", Maneuver.ToString());
            objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
            objWriter.WriteStartElement("mods");
            foreach (VehicleMod objMod in _lstVehicleMods)
                objMod.Print(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
            {
                // Use the Gear's SubClass if applicable.
                if (objGear.GetType() == typeof(Commlink))
                {
                    Commlink objCommlink = new Commlink(_objCharacter);
                    objCommlink = (Commlink)objGear;
                    objCommlink.Print(objWriter);
                }
                else
                {
                    objGear.Print(objWriter);
                }
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstWeapons)
                objWeapon.Print(objWriter);
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
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
        public string DisplayCategory
        {
            get
            {
                if (!string.IsNullOrEmpty(_strAltCategory))
                    return _strAltCategory;

                return _strCategory;
            }
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
                    if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
                    {
                        // Set the Vehicle's Pilot to the Modification's bonus.
                        if (objMod.Bonus.InnerXml.Contains("<pilot>"))
                        {
                            int intTest = Convert.ToInt32(objMod.Bonus["pilot"].InnerText.Replace("Rating", objMod.Rating.ToString()));
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
        /// Device Rating.
        /// </summary>
        public int DeviceRating
        {
            get
            {
                int intDeviceRating = Pilot;

                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    if (objMod.Bonus != null)
                    {
                        // Add the Modification's Device Rating to the Vehicle's base Device Rating.
                        if (objMod.Bonus.InnerXml.Contains("<devicerating>"))
                            intDeviceRating += Convert.ToInt32(objMod.Bonus["devicerating"].InnerText);
                    }
                }

                // Device Rating cannot go below 1.
                if (intDeviceRating < 1)
                    intDeviceRating = 1;

                return intDeviceRating;
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
                return BaseMatrixBoxes + (DeviceRating + 1) / 2;
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
        public string Page
        {
            get
            {
                if (!string.IsNullOrEmpty(_strAltPage))
                    return _strAltPage;

                return _strPage;
            }
            set
            {
                _strPage = value;
            }
        }

        /// <summary>
        /// Vehicle Modifications applied to the Vehicle.
        /// </summary>
        public List<VehicleMod> Mods
        {
            get
            {
                return _lstVehicleMods;
            }
        }

        /// <summary>
        /// Gear applied to the Vehicle.
        /// </summary>
        public List<Gear> Gear
        {
            get
            {
                return _lstGear;
            }
        }

        /// <summary>
        /// Weapons applied to the Vehicle through Gear.
        /// </summary>
        public List<Weapon> Weapons
        {
            get
            {
                return _lstWeapons;
            }
        }

        /// <summary>
        /// Calculated Availablility of the Vehicle.
        /// </summary>
        public string CalculatedAvail
        {
            get
            {
                string strReturn = _strAvail;

                // Translate the Avail string.
                strReturn = strReturn.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted"));
                strReturn = strReturn.Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

                return strReturn;
            }
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
                    if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
                    {
                        if (objMod.Bonus.InnerXml.Contains("<sensor>"))
                        {
                            string strSensor = objMod.Bonus["sensor"].InnerText.Replace("Rating", objMod.Rating.ToString()).Replace("+", string.Empty);
                            intSensor += Convert.ToInt32(strSensor, GlobalOptions.InvariantCultureInfo);
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
        public string DisplayNameShort
        {
            get
            {
                if (!string.IsNullOrEmpty(_strAltName))
                    return _strAltName;

                return _strName;
            }
        }

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = DisplayNameShort;

                if (!string.IsNullOrEmpty(_strVehicleName))
                {
                    strReturn += " (\"" + _strVehicleName + "\")";
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Whether or not the Vehicle is an A.I.'s Home Node.
        /// </summary>
        public bool HomeNode
        {
            get
            {
                return _blnHomeNode;
            }
            set
            {
                _blnHomeNode = value;
            }
        }

        /// <summary>
        /// Locations.
        /// </summary>
        public List<string> Locations
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
                foreach (Improvement objImprovement in _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.DealerConnection))
                {
                    if (
                            (objImprovement.ImprovedName == "Drones" && (
                                _strCategory.StartsWith("Drones"))) ||
                            (objImprovement.ImprovedName == "Aircraft" && (
                                _strCategory == "Fixed-Wing Aircraft" ||
                                _strCategory == "LTAV" ||
                                _strCategory == "Rotorcraft" ||
                                _strCategory == "VTOL/VSTOL")) ||
                            (objImprovement.ImprovedName == "Watercraft" && (
                                _strCategory == "Boats" ||
                                _strCategory == "Submarines")) ||
                            (objImprovement.ImprovedName == "Groundcraft" && (
                                _strCategory == "Bikes" ||
                                _strCategory == "Cars" ||
                                _strCategory == "Trucks" ||
                                _strCategory == "Municipal/Construction" ||
                                _strCategory == "Corpsec/Police/Military"))
                            )
                    {
                        _blnDealerConnectionDiscount = true;
                    }
                }
                return _blnDealerConnectionDiscount;
            }
            set
            {
                _blnDealerConnectionDiscount = value;
            }
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
                return _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed).Sum(objMod => objMod.CalculatedSlots);
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

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.CalculatedSlots > 0))
                {
                    int intActualSlots = 0;
                    switch (objMod.Category)
                    {
                        case "Handling":
                            intActualSlots = objMod.CalculatedSlots - _intHandling;
                            if (!blnHandling)
                            {
                                blnHandling = true;
                                intActualSlots -= 1;
                            }
                            break;
                        case "Speed":
                            intActualSlots = objMod.CalculatedSlots - _intSpeed;
                            if (!blnSpeed)
                            {
                                blnSpeed = true;
                                intActualSlots -= 1;
                            }
                            break;
                        case "Acceleration":
                            intActualSlots = objMod.CalculatedSlots - _intAccel;
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
                            intActualSlots = objMod.CalculatedSlots - _intSensor;
                            if (!blnSensor)
                            {
                                blnSensor = true;
                                intActualSlots -= 1;
                            }
                            break;
                        default:
                            intActualSlots = objMod.CalculatedSlots;
                            break;
                    }

                    if (intActualSlots < 0)
                        intActualSlots = 0;
                        
                    intModSlotsUsed += intActualSlots;
                }
                return intModSlotsUsed;
            }
        }


        /// <summary>
        /// Total cost of the Vehicle including all after-market Modification.
        /// </summary>
        public int TotalCost
        {
            get
            {
                int intCost = Convert.ToInt32(_strCost);
                if (BlackMarketDiscount)
                    intCost = intCost * 9 / 10;

                if (DealerConnectionDiscount)
                    intCost = intCost * 9 / 10;

                foreach (VehicleMod objMod in _lstVehicleMods)
                {
                    // Do not include the price of Mods that are part of the base configureation.
                    if (!objMod.IncludedInVehicle)
                    {
                        intCost += objMod.TotalCost;
                    }
                    else
                    {
                        // If the Mod is a part of the base config, check the items attached to it since their cost still counts.
                        intCost += objMod.Weapons.Sum(objWeapon => objWeapon.TotalCost);
                        intCost += objMod.Cyberware.Sum(objCyberware => objCyberware.TotalCost);
                    }
                }

                intCost += _lstGear.Sum(objGear => objGear.TotalCost);

                return intCost;
            }
        }

        /// <summary>
        /// The cost of just the Vehicle itself.
        /// </summary>
        public int OwnCost
        {
            get
            {
                int intCost = Convert.ToInt32(_strCost);

                if (BlackMarketDiscount)
                    intCost = intCost * 9 / 10;

                if (DealerConnectionDiscount)
                    intCost = intCost * 9 / 10;

                return intCost;
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
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null).Where(objMod => objMod.Bonus.InnerXml.Contains("<seats>")))
                {
                    chrFirstCharacter = objMod.Bonus["seats"].InnerText[0];
                    if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                    {
                        intTotalSeats = Math.Max(Convert.ToInt32(objMod.Bonus["seats"].InnerText.Replace("Rating", objMod.Rating.ToString())), intTotalSeats);
                    }
                }

                // Then check for mods that modify the seat value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSeats = 0;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null).Where(objMod => objMod.Bonus.InnerXml.Contains("<seats>")))
                {
                    chrFirstCharacter = objMod.Bonus["seats"].InnerText[0];
                    if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                    {
                        // If the bonus is determined by the existing seat number, evaluate the expression.
                        XmlDocument objXmlDocument = new XmlDocument();
                        XPathNavigator nav = objXmlDocument.CreateNavigator();
                        XPathExpression xprSeats = nav.Compile(objMod.Bonus["seats"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Seats", intTotalSeats.ToString()));
                        intTotalBonusSeats += Convert.ToInt32(nav.Evaluate(xprSeats), GlobalOptions.InvariantCultureInfo);
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
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null))
                {
                    if (objMod.Bonus.InnerXml.Contains("<speed>"))
                    {
                        chrFirstCharacter = objMod.Bonus["speed"].InnerText[0];
                        if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                        {
                            intTotalSpeed = Math.Max(intTotalSpeed, Convert.ToInt32(objMod.Bonus["speed"].InnerText.Replace("Rating", objMod.Rating.ToString())));
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<offroadspeed>"))
                    {
                        chrFirstCharacter = objMod.Bonus["offroadspeed"].InnerText[0];
                        if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                        {
                            intBaseOffroadSpeed = Math.Max(intBaseOffroadSpeed, Convert.ToInt32(objMod.Bonus["offroadspeed"].InnerText.Replace("Rating", objMod.Rating.ToString())));
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<armor>"))
                    {
                        if (IsDrone && GlobalOptions.Instance.Dronemods)
                        {
                            intTotalArmor = Convert.ToInt32(objMod.Bonus["armor"].InnerText.Replace("Rating", objMod.Rating.ToString()));
                        }
                    }
                }

                // Then check for mods that modify the speed value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusSpeed = 0;
                int intTotalBonusOffroadSpeed = 0;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null))
                {
                    if (objMod.Bonus.InnerXml.Contains("<speed>"))
                    {
                        chrFirstCharacter = objMod.Bonus["speed"].InnerText[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing speed number, evaluate the expression.
                            XmlDocument objXmlDocument = new XmlDocument();
                            XPathNavigator nav = objXmlDocument.CreateNavigator();
                            XPathExpression xprSeats = nav.Compile(objMod.Bonus["speed"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Speed", intTotalSpeed.ToString()));
                            intTotalBonusSpeed += Convert.ToInt32(nav.Evaluate(xprSeats), GlobalOptions.CultureInfo);
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<offroadspeed>"))
                    {
                        chrFirstCharacter = objMod.Bonus["offroadspeed"].InnerText[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing speed number, evaluate the expression.
                            XmlDocument objXmlDocument = new XmlDocument();
                            XPathNavigator nav = objXmlDocument.CreateNavigator();
                            XPathExpression xprSeats = nav.Compile(objMod.Bonus["offroadspeed"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadSpeed", intBaseOffroadSpeed.ToString()));
                            intTotalBonusOffroadSpeed += Convert.ToInt32(nav.Evaluate(xprSeats), GlobalOptions.InvariantCultureInfo);
                        }
                    }
                }

                // Reduce speed of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 3, 0);

                if (Speed != OffroadSpeed || intTotalSpeed + intTotalBonusSpeed != intBaseOffroadSpeed + intTotalBonusOffroadSpeed)
                {
                    return ((intTotalSpeed + intTotalBonusSpeed - intPenalty).ToString() + '/' + (intBaseOffroadSpeed + intTotalBonusOffroadSpeed - intPenalty));
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
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null))
                {
                    if (objMod.Bonus.InnerXml.Contains("<accel>"))
                    {
                        chrFirstCharacter = objMod.Bonus["accel"].InnerText[0];
                        if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                        {
                            intTotalAccel = Math.Max(intTotalAccel, Convert.ToInt32(objMod.Bonus["accel"].InnerText.Replace("Rating", objMod.Rating.ToString())));
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<offroadaccel>"))
                    {
                        chrFirstCharacter = objMod.Bonus["offroadaccel"].InnerText[0];
                        if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                        {
                            intBaseOffroadAccel = Math.Max(intBaseOffroadAccel, Convert.ToInt32(objMod.Bonus["offroadaccel"].InnerText.Replace("Rating", objMod.Rating.ToString())));
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<armor>"))
                    {
                        if (IsDrone && GlobalOptions.Instance.Dronemods)
                        {
                            intTotalArmor = Convert.ToInt32(objMod.Bonus["armor"].InnerText.Replace("Rating", objMod.Rating.ToString()));
                        }
                    }
                }

                // Then check for mods that modify the accel value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusAccel = 0;
                int intTotalBonusOffroadAccel = 0;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null))
                {
                    if (objMod.Bonus.InnerXml.Contains("<accel>"))
                    {
                        chrFirstCharacter = objMod.Bonus["accel"].InnerText[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing speed number, evaluate the expression.
                            XmlDocument objXmlDocument = new XmlDocument();
                            XPathNavigator nav = objXmlDocument.CreateNavigator();
                            XPathExpression xprSeats = nav.Compile(objMod.Bonus["accel"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Accel", intTotalAccel.ToString()));
                            intTotalBonusAccel += Convert.ToInt32(nav.Evaluate(xprSeats), GlobalOptions.InvariantCultureInfo);
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<offroadaccel>"))
                    {
                        chrFirstCharacter = objMod.Bonus["offroadaccel"].InnerText[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing speed number, evaluate the expression.
                            XmlDocument objXmlDocument = new XmlDocument();
                            XPathNavigator nav = objXmlDocument.CreateNavigator();
                            XPathExpression xprSeats = nav.Compile(objMod.Bonus["offroadaccel"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadAccel", intBaseOffroadAccel.ToString()));
                            intTotalBonusOffroadAccel += Convert.ToInt32(nav.Evaluate(xprSeats), GlobalOptions.InvariantCultureInfo);
                        }
                    }
                }

                // Reduce acceleration of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 6, 0);

                if (Accel != OffroadAccel || intTotalAccel + intTotalBonusAccel != intBaseOffroadAccel + intTotalBonusOffroadAccel)
                {
                    return ((intTotalAccel + intTotalBonusAccel - intPenalty).ToString() + '/' + (intBaseOffroadAccel + intTotalBonusOffroadAccel - intPenalty));
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
                    if (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)
                    {
                        // Add the Modification's Body to the Vehicle's base Body.

                        if (objMod.Bonus.InnerXml.Contains("<body>"))
                        {
                            if (objMod.Bonus["body"].InnerText.Contains("Rating"))
                            {
                                // If the cost is determined by the Rating, evaluate the expression.
                                XmlDocument objXmlDocument = new XmlDocument();
                                XPathNavigator nav = objXmlDocument.CreateNavigator();

                                string strBody = objMod.Bonus["body"].InnerText.Replace("Rating", objMod.Rating.ToString());
                                XPathExpression xprBody = nav.Compile(strBody);
                                intBody += Convert.ToInt32(nav.Evaluate(xprBody).ToString());
                            } else if (objMod.Bonus["body"].InnerText[0] == '-')
                            {
                                intBody += Convert.ToInt32(objMod.Bonus["body"].InnerText);

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
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null))
                {
                    if (objMod.Bonus.InnerXml.Contains("<handling>"))
                    {
                        chrFirstCharacter = objMod.Bonus["handling"].InnerText[0];
                        if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                        {
                            intBaseHandling = Math.Max(intBaseHandling, Convert.ToInt32(objMod.Bonus["handling"].InnerText.Replace("Rating", objMod.Rating.ToString())));
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<offroadhandling>"))
                    {
                        chrFirstCharacter = objMod.Bonus["offroadhandling"].InnerText[0];
                        if (chrFirstCharacter != '+' && chrFirstCharacter != '-')
                        {
                            intBaseOffroadHandling = Math.Max(intBaseOffroadHandling, Convert.ToInt32(objMod.Bonus["offroadhandling"].InnerText.Replace("Rating", objMod.Rating.ToString())));
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<armor>"))
                    {
                        if (IsDrone && GlobalOptions.Instance.Dronemods)
                        {
                            intTotalArmor = Convert.ToInt32(objMod.Bonus["armor"].InnerText.Replace("Rating", objMod.Rating.ToString()));
                        }
                    }
                }

                // Then check for mods that modify the handling value (needs separate loop in case of % modifiers on top of stat-overriding mods)
                int intTotalBonusHandling = 0;
                int intTotalBonusOffroadHandling = 0;
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null))
                {
                    if (objMod.Bonus.InnerXml.Contains("<handling>"))
                    {
                        chrFirstCharacter = objMod.Bonus["handling"].InnerText[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing speed number, evaluate the expression.
                            XmlDocument objXmlDocument = new XmlDocument();
                            XPathNavigator nav = objXmlDocument.CreateNavigator();
                            XPathExpression xprSeats = nav.Compile(objMod.Bonus["handling"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("Handling", intBaseHandling.ToString()));
                            intTotalBonusHandling += Convert.ToInt32(nav.Evaluate(xprSeats), GlobalOptions.InvariantCultureInfo);
                        }
                    }
                    if (objMod.Bonus.InnerXml.Contains("<offroadhandling>"))
                    {
                        chrFirstCharacter = objMod.Bonus["offroadhandling"].InnerText[0];
                        if (chrFirstCharacter == '+' || chrFirstCharacter == '-')
                        {
                            // If the bonus is determined by the existing speed number, evaluate the expression.
                            XmlDocument objXmlDocument = new XmlDocument();
                            XPathNavigator nav = objXmlDocument.CreateNavigator();
                            XPathExpression xprSeats = nav.Compile(objMod.Bonus["offroadhandling"].InnerText.TrimStart('+').Replace("Rating", objMod.Rating.ToString()).Replace("OffroadHandling", intBaseOffroadHandling.ToString()));
                            intTotalBonusOffroadHandling += Convert.ToInt32(nav.Evaluate(xprSeats), GlobalOptions.InvariantCultureInfo);
                        }
                    }
                }

                // Reduce handling of the drone if there is too much armor
                int intPenalty = Math.Max((intTotalArmor - TotalBody * 3) / 3, 0);

                if (Handling != OffroadHandling || intBaseHandling + intTotalBonusHandling != intBaseOffroadHandling + intTotalBonusOffroadHandling)
                {
                    return ((intBaseHandling + intTotalBonusHandling - intPenalty).ToString() + '/' + (intBaseOffroadHandling + intTotalBonusOffroadHandling - intPenalty));
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
                bool blnArmorMod = false;

                // Rigger5 Drone Armor starts at 0. All other vehicles start with their base armor.
                if (IsDrone && GlobalOptions.Instance.Dronemods)
                    intModArmor = 0;
                else
                    intModArmor = _intArmor;
                
                // Add the Modification's Armor to the Vehicle's base Armor. 
                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => (!objMod.IncludedInVehicle && objMod.Installed && objMod.Bonus != null)).Where(objMod => objMod.Bonus.InnerXml.Contains("<armor>")))
                {
                    blnArmorMod = true;
                    intModArmor += Convert.ToInt32(objMod.Bonus["armor"].InnerText.Replace("Rating", objMod.Rating.ToString()));
                }
                // Drones have no theoretical armor cap in the optional rules, otherwise, it's capped
                if (!IsDrone || !GlobalOptions.Instance.Dronemods)
                {
                    intModArmor = Math.Min(MaxArmor, intModArmor);
                }
                else if (!blnArmorMod)
                {
                    // We're a drone, but we didn't have any mods, so keep the base value
                    intModArmor = _intArmor;
                } else if (intModArmor < 0)
                {
                    intModArmor = _intArmor + intModArmor;
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
                int intReturn = 0;

                // Rigger 5 says max armor is Body + starting Armor, p159
                intReturn = _intBody + _intArmor;

                if (IsDrone)
                {
                    if (_objCharacter.Options.DroneArmorMultiplierEnabled)
                    {
                        intReturn = _intArmor*_objCharacter.Options.DroneArmorMultiplier;
                    }
                    else
                    {
                        intReturn = _intArmor*2;
                    }
                }

                // If ignoring the rules, do not limit Armor to the Vehicle's standard rules.
                if (_objCharacter.IgnoreRules)
                    intReturn = 99;

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
                return 99;
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
                return 99;
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
                return 99;
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
                return 99;
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
                if (IsDrone && !_objCharacter.IgnoreRules && GlobalOptions.Instance.DronemodsMaximumPilot)
                {
                    return Math.Max(_intPilot * 2, 1);
                }
                return 99;
            }
        }

        /// <summary>
        /// Check if the vehicle is over capacity in any category
        /// </summary>
        public bool OverR5Capacity(string strCheckCapacity = "")
        {
            string[] arrCategories = new string[6] { "Powertrain", "Protection", "Weapons", "Body", "Electromagnetic", "Cosmetic" };
            return !string.IsNullOrEmpty(strCheckCapacity) && arrCategories.Contains(strCheckCapacity)
                ? arrCategories.Any(strCategory => CalcCategoryUsed(strCheckCapacity) > CalcCategoryAvail(strCheckCapacity))
                : arrCategories.Any(strCategory => CalcCategoryUsed(strCategory) > CalcCategoryAvail(strCategory));
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

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Powertrain")))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    if (objMod.CalculatedSlots > 0)
                        intPowertrain -= Convert.ToInt32(objMod.CalculatedSlots);
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

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Protection")))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    if (objMod.CalculatedSlots > 0)
                        intProtection -= Convert.ToInt32(objMod.CalculatedSlots);
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

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Weapons")))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    if (objMod.CalculatedSlots > 0)
                        intWeaponsmod -= Convert.ToInt32(objMod.CalculatedSlots);
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

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Body")))
                { 
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    if (objMod.CalculatedSlots > 0)
                            intBodymod -= Convert.ToInt32(objMod.CalculatedSlots);
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

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Electromagnetic")))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    if (objMod.CalculatedSlots > 0)
                        intElectromagnetic -= Convert.ToInt32(objMod.CalculatedSlots);
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

                foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == "Cosmetic")))
                {
                    // Subtract the Modification's Slots from the Vehicle's base Body.
                    if (objMod.CalculatedSlots > 0)
                        intCosmetic -= Convert.ToInt32(objMod.CalculatedSlots);
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
                int intReturn = 0;
                Gear objGear = FindGearByName("[Model] Maneuvering Autosoft", _lstGear, Name);
                if (objGear != null)
                {
                    intReturn = objGear.Rating;
                }

                return intReturn;
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

            foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == strCategory)))
            {
                // Subtract the Modification's Slots from the Vehicle's base Body.
                if (objMod.CalculatedSlots > 0)
                    intBase += Convert.ToInt32(objMod.CalculatedSlots);
            }

            return intBase;
        }

        /// <summary>
        /// Total Number of Slots a Vehicle has used for Modifications. (Rigger 5)
        /// </summary>
        public int CalcCategoryAvail(string strCategory)
        {
            int intBase = _intBody;
            foreach (VehicleMod objMod in _lstVehicleMods.Where(objMod => !objMod.IncludedInVehicle && objMod.Installed && (objMod.Category == strCategory)))
            {
                if (objMod.CalculatedSlots < 0)
                    intBase -= objMod.CalculatedSlots;
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

        /// <summary>
        /// Locate a piece of Gear.
        /// </summary>
        /// <param name="strName">Name of the Gear to find.</param>
        /// <param name="lstGear">List of Gear to search.</param>
        /// <param name="strExtra">Extra conditions for Gear to find.</param>
        private Gear FindGearByName(string strName, List<Gear> lstGear, string strExtra = "")
        {
            Gear objReturn = new Gear(_objCharacter);
            foreach (Gear objGear in lstGear)
            {
                if (objGear.Name == strName && (string.IsNullOrEmpty(strExtra) || strExtra == objGear.Extra))
                    objReturn = objGear;
                else
                {
                    if (objGear.Children.Count > 0)
                        objReturn = FindGearByName(strName, objGear.Children, strExtra);
                }

                if (objReturn.InternalId != Guid.Empty.ToString() && !string.IsNullOrEmpty(objReturn.Name))
                    return objReturn;
            }

            return objReturn;
        }
        #endregion
    }
}