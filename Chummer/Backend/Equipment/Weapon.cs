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
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Skills;
using System.Drawing;
using Chummer.Backend.Attributes;
using System.Text;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A Weapon.
    /// </summary>
    public class Weapon : IHasChildren<Weapon>, IHasName, IHasInternalId, IHasXmlNode, IHasMatrixAttributes
    {
        private Guid _sourceID = Guid.Empty;
        private Guid _guiID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private int _intReach = 0;
        private string _strDamage = string.Empty;
        private string _strAP = "0";
        private string _strMode = string.Empty;
        private string _strRC = string.Empty;
        private string _strAmmo = string.Empty;
        private string _strAmmoCategory = string.Empty;
        private int _intConceal = 0;
        private List<Clip> _ammo = new List<Clip>();
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
        private string _strRange = string.Empty;
        private string _strAlternateRange = string.Empty;
        private decimal _decRangeMultiplier = 1;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strWeaponName = string.Empty;
        private int _intFullBurst = 10;
        private int _intSuppressive = 20;
        private List<WeaponAccessory> _lstAccessories = new List<WeaponAccessory>();
        private List<Weapon> _lstUnderbarrel = new List<Weapon>();
        private Vehicle _objMountedVehicle = null;
        private WeaponMount _objWeaponMount = null;
        private string _strNotes = string.Empty;
        private string _strUseSkill = string.Empty;
        private string _strLocation = string.Empty;
        private string _strSpec = string.Empty;
        private string _strSpec2 = string.Empty;
        private bool _blnIncludedInWeapon = false;
        private bool _blnInstalled = true;
        private bool _blnDiscountCost = false;
        private bool _blnRequireAmmo = true;
        private string _strAccuracy = string.Empty;
        private string _strRCTip = string.Empty;
        private string _strWeaponSlots = string.Empty;
        private bool _blnCyberware = false;
        private string _strParentID = string.Empty;
        private bool _blnAllowAccessory = true;

        private XmlNode _objCachedMyXmlNode = null;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private FiringMode _eFiringMode;

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
        private int _intMatrixCMFilled = 0;

        private readonly Character _objCharacter;
        private string _mount;
        private string _extraMount;

        #region Constructor, Create, Save, Load, and Print Methods
        public Weapon(Character objCharacter)
        {
            // Create the GUID for the new Weapon.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Weapon from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlWeapon">XmlNode to create the object from.</param>
        /// <param name="lstWeapons">List of child Weapons to generate.</param>
        /// <param name="lstNodes">List of TreeNodes to populate a TreeView.</param>
        /// <param name="cmsWeapon">ContextMenuStrip to use for Weapons.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip to use for Accessories.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        public void Create(XmlNode objXmlWeapon, List<Weapon> lstWeapons, bool blnCreateChildren = true, bool blnCreateImprovements = true, bool blnSkipCost = false)
        {
            if (objXmlWeapon.TryGetField("id", Guid.TryParse, out _sourceID))
                _objCachedMyXmlNode = null;
            objXmlWeapon.TryGetStringFieldQuickly("name", ref _strName);
            objXmlWeapon.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlWeapon.TryGetStringFieldQuickly("type", ref _strType);
            objXmlWeapon.TryGetInt32FieldQuickly("reach", ref _intReach);
            objXmlWeapon.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
            objXmlWeapon.TryGetStringFieldQuickly("damage", ref _strDamage);
            objXmlWeapon.TryGetStringFieldQuickly("ap", ref _strAP);
            objXmlWeapon.TryGetStringFieldQuickly("mode", ref _strMode);
            objXmlWeapon.TryGetStringFieldQuickly("ammo", ref _strAmmo);
            objXmlWeapon.TryGetStringFieldQuickly("mount", ref _mount);
            objXmlWeapon.TryGetStringFieldQuickly("extramount", ref _extraMount);
            if (objXmlWeapon["accessorymounts"] != null)
            {
                XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("accessorymounts/mount");
                string strMounts = string.Empty;
                foreach (XmlNode objXmlMount in objXmlMountList)
                {
                    strMounts += objXmlMount.InnerText + "/";
                }
                if (strMounts.EndsWith('/'))
                {
                    strMounts = strMounts.Substring(0, strMounts.Length - 1);
                }
                _strWeaponSlots = strMounts;
            }
            if (!objXmlWeapon.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlWeapon.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlWeapon.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            objXmlWeapon.TryGetStringFieldQuickly("rc", ref _strRC);
            objXmlWeapon.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objXmlWeapon.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlWeapon.TryGetStringFieldQuickly("cost", ref _strCost);

            // Check for a Variable Cost.
            if (!blnSkipCost && _strCost.StartsWith("Variable("))
            {
                string strFirstHalf = _strCost.TrimStart("Variable(", true).TrimEnd(')');
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
                    decimal decMin = decimal.MinValue;
                    decimal decMax = decimal.MaxValue;
                    if (intHyphenIndex != -1)
                    {
                        decMin = Convert.ToDecimal(strFirstHalf, GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strSecondHalf, GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strFirstHalf.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    if (decMin != decimal.MinValue || decMax != decimal.MaxValue)
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
                    else
                        _strCost = strFirstHalf;
                }
                else
                    _strCost = strFirstHalf;
            }
            
            objXmlWeapon.TryGetBoolFieldQuickly("cyberware", ref _blnCyberware);
            objXmlWeapon.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlWeapon.TryGetStringFieldQuickly("page", ref _strPage);

            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");

            // Populate the Range if it differs from the Weapon's Category.
            XmlNode objRangeNode = objXmlWeapon["range"];
            if (objRangeNode != null)
            {
                _strRange = objRangeNode.InnerText;
                if (objRangeNode.Attributes["multiply"] != null)
                    _decRangeMultiplier = Convert.ToDecimal(objRangeNode.Attributes["multiply"].InnerText, GlobalOptions.InvariantCultureInfo);
            }
            objXmlWeapon.TryGetStringFieldQuickly("alternaterange", ref _strAlternateRange);

            objXmlWeapon.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objXmlWeapon.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);

            objXmlWeapon.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
            objXmlWeapon.TryGetBoolFieldQuickly("requireammo", ref _blnRequireAmmo);
            objXmlWeapon.TryGetStringFieldQuickly("spec", ref _strSpec);
            objXmlWeapon.TryGetStringFieldQuickly("spec2", ref _strSpec2);
            objXmlWeapon.TryGetBoolFieldQuickly("allowaccessory", ref _blnAllowAccessory);

            // If the Weapon comes with an Underbarrel Weapon, add it.
            if (objXmlWeapon.InnerXml.Contains("<underbarrels>") && blnCreateChildren)
            {
                foreach (XmlNode objXmlUnderbarrel in objXmlWeapon["underbarrels"].ChildNodes)
                {
                    Weapon objUnderbarrelWeapon = new Weapon(_objCharacter);
                    XmlNode objXmlWeaponNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlUnderbarrel.InnerText + "\"]");
                    objUnderbarrelWeapon.Create(objXmlWeaponNode, lstWeapons, true, blnCreateImprovements, blnSkipCost);
                    if (!AllowAccessory)
                        objUnderbarrelWeapon.AllowAccessory = false;
                    objUnderbarrelWeapon.ParentID = InternalId;
                    objUnderbarrelWeapon.IncludedInWeapon = true;
                    objUnderbarrelWeapon.Parent = this;
                    objUnderbarrelWeapon.ParentVehicle = ParentVehicle;
                    _lstUnderbarrel.Add(objUnderbarrelWeapon);
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

            // If there are any Accessories that come with the Weapon, add them.
            XmlNode xmlAccessoriesNode = objXmlWeapon["accessories"];
            if (xmlAccessoriesNode != null && blnCreateChildren)
            {
                XmlNodeList objXmlAccessoryList = xmlAccessoriesNode.SelectNodes("accessory");
                foreach (XmlNode objXmlWeaponAccessory in objXmlAccessoryList)
                {
                    XmlNode objXmlAccessory = objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + objXmlWeaponAccessory["name"].InnerText + "\"]");
                    WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
                    int intAccessoryRating = 0;
                    if (objXmlWeaponAccessory["rating"] != null)
                    {
                        intAccessoryRating = Convert.ToInt32(objXmlWeaponAccessory["rating"].InnerText);
                    }
                    if (objXmlWeaponAccessory.InnerXml.Contains("mount"))
                    {
                        if (objXmlWeaponAccessory.InnerXml.Contains("<extramount>"))
                        {
                            objAccessory.Create(objXmlAccessory, new Tuple<string, string>(objXmlAccessory["mount"].InnerText, objXmlAccessory["extramount"].InnerText), intAccessoryRating, false, blnCreateChildren, blnCreateImprovements);
                        }
                        else
                        {
                            objAccessory.Create(objXmlAccessory, new Tuple<string, string>(objXmlAccessory["mount"].InnerText, "None"), intAccessoryRating, false, blnCreateChildren, blnCreateImprovements);
                        }
                    }
                    else
                    {
                        objAccessory.Create(objXmlAccessory, new Tuple<string, string>("Internal", "None"), intAccessoryRating, false, blnCreateChildren, blnCreateImprovements);
                    }
                    // Add any extra Gear that comes with the Weapon Accessory.
                    XmlNode xmlGearsNode = objXmlWeaponAccessory["gears"];
                    if (xmlGearsNode != null)
                    {
                        XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                        foreach (XmlNode objXmlAccessoryGear in xmlGearsNode.SelectNodes("usegear"))
                        {
                            XmlNode objXmlAccessoryGearName = objXmlAccessoryGear["name"];
                            XmlAttributeCollection objXmlAccessoryGearNameAttributes = objXmlAccessoryGearName.Attributes;
                            int intGearRating = 0;
                            decimal decGearQty = 1;
                            string strChildForceSource = objXmlAccessoryGear["source"]?.InnerText ?? string.Empty;
                            string strChildForcePage = objXmlAccessoryGear["page"]?.InnerText ?? string.Empty;
                            string strChildForceValue = objXmlAccessoryGearNameAttributes?["select"]?.InnerText ??string.Empty;
                            bool blnStartCollapsed = objXmlAccessoryGearNameAttributes?["startcollapsed"]?.InnerText == bool.TrueString;
                            bool blnChildCreateChildren = objXmlAccessoryGearNameAttributes?["createchildren"]?.InnerText != bool.FalseString;
                            bool blnAddChildImprovements = objXmlAccessoryGearNameAttributes?["addimprovements"]?.InnerText != bool.FalseString;
                            if (objXmlAccessoryGear["rating"] != null)
                                intGearRating = Convert.ToInt32(objXmlAccessoryGear["rating"].InnerText);
                            if (objXmlAccessoryGearNameAttributes?["qty"] != null)
                                decGearQty = Convert.ToDecimal(objXmlAccessoryGearNameAttributes["qty"].InnerText, GlobalOptions.InvariantCultureInfo);

                            XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlAccessoryGearName.InnerText + "\" and category = \"" + objXmlAccessoryGear["category"].InnerText + "\"]");
                            Gear objGear = new Gear(_objCharacter);

                            objGear.Create(objXmlGear, intGearRating, lstWeapons, strChildForceValue, blnAddChildImprovements, blnChildCreateChildren);

                            objGear.Quantity = decGearQty;
                            objGear.Cost = "0";
                            objGear.MinRating = intGearRating;
                            objGear.MaxRating = intGearRating;
                            objGear.ParentID = InternalId;
                            if (!string.IsNullOrEmpty(strChildForceSource))
                                objGear.Source = strChildForceSource;
                            if (!string.IsNullOrEmpty(strChildForcePage))
                                objGear.Page = strChildForcePage;
                            objAccessory.Gear.Add(objGear);

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

            // Add Subweapons (not underbarrels) if applicable.
            if (lstWeapons == null)
                return;
            // More than one Weapon can be added, so loop through all occurrences.
            foreach (XmlNode objXmlAddWeapon in objXmlWeapon.SelectNodes("addweapon"))
            {
                string strLoopID = objXmlAddWeapon.InnerText;
                XmlNode objXmlSubWeapon = strLoopID.IsGuid()
                    ? objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                    : objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");

                Weapon objSubWeapon = new Weapon(_objCharacter)
                {
                    ParentVehicle = ParentVehicle
                };
                objSubWeapon.Create(objXmlSubWeapon, lstWeapons, blnCreateChildren, blnCreateImprovements, blnSkipCost);
                objSubWeapon.ParentID = InternalId;
                lstWeapons.Add(objSubWeapon);
            }
            foreach (Weapon objLoopWeapon in lstWeapons)
                objLoopWeapon.ParentVehicle = ParentVehicle;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("sourceid", _sourceID.ToString("D"));
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("spec", _strSpec);
            objWriter.WriteElementString("spec2", _strSpec2);
            objWriter.WriteElementString("reach", _intReach.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("damage", _strDamage);
            objWriter.WriteElementString("ap", _strAP);
            objWriter.WriteElementString("mode", _strMode);
            objWriter.WriteElementString("rc", _strRC);
            objWriter.WriteElementString("ammo", _strAmmo);
            objWriter.WriteElementString("cyberware", _blnCyberware.ToString());
            objWriter.WriteElementString("ammocategory", _strAmmoCategory);
            objWriter.WriteElementString("firingmode",_eFiringMode.ToString());
            objWriter.WriteStartElement("clips");
            foreach (Clip clip in _ammo)
            {
                if (string.IsNullOrWhiteSpace(clip.AmmoName))
                {
                    clip.AmmoName = GetAmmoName(clip.Guid, GlobalOptions.DefaultLanguage);
                }
                clip.Save(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteElementString("conceal", _intConceal.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("useskill", _strUseSkill);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("alternaterange", _strAlternateRange);
            objWriter.WriteElementString("rangemultiply", _decRangeMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("fullburst", _intFullBurst.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("suppressive", _intSuppressive.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("allowaccessory", _blnAllowAccessory.ToString());
            objWriter.WriteElementString("weaponname", _strWeaponName);
            objWriter.WriteElementString("included", _blnIncludedInWeapon.ToString());
            objWriter.WriteElementString("installed", _blnInstalled.ToString());
            objWriter.WriteElementString("requireammo", _blnRequireAmmo.ToString());
            objWriter.WriteElementString("accuracy", _strAccuracy);
            objWriter.WriteElementString("mount", _mount);
            objWriter.WriteElementString("extramount", _extraMount);
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
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString());

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
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
                _ammo = new List<Clip>();
                _intActiveAmmoSlot = 1;
            }
            else
            {
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);
                _ammo.Clear();
                if (objNode["clips"] != null)
                {
                    XmlNode clipNode = objNode["clips"];

                    foreach (XmlNode node in clipNode.ChildNodes)
                    {
                        Clip LoopClip = Clip.Load(node);
                        if (string.IsNullOrWhiteSpace(LoopClip.AmmoName))
                        {
                            LoopClip.AmmoName = GetAmmoName(LoopClip.Guid, GlobalOptions.DefaultLanguage);
                        }
                        _ammo.Add(LoopClip);
                    }
                }
                else //Load old clips
                {
                    foreach (string s in new[] { string.Empty, "2", "3", "4" })
                    {
                        int ammo = 0;
                        Guid guid = Guid.Empty;

                        if (objNode.TryGetInt32FieldQuickly("ammoremaining" + s, ref ammo) &&
                            objNode.TryGetField("ammoloaded" + s, Guid.TryParse, out guid) &&
                            ammo > 0 && guid != Guid.Empty)
                        {
                            _ammo.Add(new Clip(guid, ammo));
                        }
                    }
                }
            }

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");
            if (!objNode.TryGetField("sourceid", Guid.TryParse, out _sourceID) || _sourceID.Equals(Guid.Empty))
            {
                XmlNode objWeaponNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + _strName + "\"]");
                if (objWeaponNode?.TryGetField("id", Guid.TryParse, out _sourceID) == true)
                    _objCachedMyXmlNode = null;
            }
            else
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            if (_strCategory == "Hold-Outs")
                _strCategory = "Holdouts";
            else if (_strCategory == "Cyberware Hold-Outs")
                _strCategory = "Cyberware Holdouts";
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("spec", ref _strSpec);
            objNode.TryGetStringFieldQuickly("spec2", ref _strSpec2);
            objNode.TryGetInt32FieldQuickly("reach", ref _intReach);
            objNode.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
            objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            if (objNode["firingmode"] != null)
                _eFiringMode = ConvertToFiringMode(objNode["firingmode"].InnerText);
            // Legacy shim
            if (Name.Contains("Osmium Mace (STR"))
            {
                XmlNode objNewOsmiumMaceNode = XmlManager.Load("weapons.xml").SelectSingleNode("/chummer/weapons/weapon[name = \"Osmium Mace\"]");
                if (objNewOsmiumMaceNode != null)
                {
                    objNewOsmiumMaceNode.TryGetStringFieldQuickly("name", ref _strName);
                    Guid.TryParse(objNewOsmiumMaceNode["id"].InnerText, out _sourceID);
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
            objNode.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objNode.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("parentid", ref _strParentID);
            if (!objNode.TryGetBoolFieldQuickly("allowaccessory", ref _blnAllowAccessory))
                _blnAllowAccessory = GetNode()?["allowaccessory"]?.InnerText != System.Boolean.FalseString;
            objNode.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("weaponname", ref _strWeaponName);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("mount", ref _mount);
            objNode.TryGetStringFieldQuickly("extramount", ref _extraMount);
            if (_strRange == "Hold-Outs")
            {
                _strRange = "Holdouts";
            }
            if (!objNode.TryGetStringFieldQuickly("alternaterange", ref _strAlternateRange))
            {
                string strAlternateRange = GetNode()?["alternaterange"]?.InnerText;
                if (!string.IsNullOrEmpty(strAlternateRange))
                {
                    _strAlternateRange = strAlternateRange;
                }
            }
            objNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
            objNode.TryGetDecFieldQuickly("rangemultiply", ref _decRangeMultiplier);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludedInWeapon);
            if (Name == "Unarmed Attack")
                _blnIncludedInWeapon = true; // Unarmed Attack can never be removed
            objNode.TryGetBoolFieldQuickly("installed", ref _blnInstalled);
            objNode.TryGetBoolFieldQuickly("requireammo", ref _blnRequireAmmo);


            //#1544 Ammunition not loading or available.
            if (_strUseSkill == "Throwing Weapons"
                && _strAmmo != "1")
            {
                _strAmmo = "1";
            }

            if (objNode.InnerXml.Contains("<accessories>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("accessories/accessory");
                foreach (XmlNode nodChild in nodChildren)
                {
                    WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
                    objAccessory.Load(nodChild, blnCopy);
                    objAccessory.Parent = this;
                    _lstAccessories.Add(objAccessory);
                }
            }

            if (objNode.InnerXml.Contains("<underbarrel>"))
            {
                foreach (XmlNode nodWeapon in objNode.SelectNodes("underbarrel/weapon"))
                {
                    Weapon objUnderbarrel = new Weapon(_objCharacter)
                    {
                        ParentVehicle = ParentVehicle
                    };
                    objUnderbarrel.Load(nodWeapon, blnCopy);
                    objUnderbarrel.Parent = this;
                    _lstUnderbarrel.Add(objUnderbarrel);
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);

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
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);

            this.RefreshMatrixAttributeArray();
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            // Find the piece of Gear that created this item if applicable
            List<Gear> lstGearToSearch = new List<Gear>(_objCharacter.Gear);
            foreach (Cyberware objCyberware in _objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
            {
                lstGearToSearch.AddRange(objCyberware.Gear);
            }
            foreach (Weapon objWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
            {
                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                {
                    lstGearToSearch.AddRange(objAccessory.Gear);
                }
            }
            foreach (Armor objArmor in _objCharacter.Armor)
            {
                lstGearToSearch.AddRange(objArmor.Gear);
            }
            foreach (Vehicle objVehicle in _objCharacter.Vehicles)
            {
                lstGearToSearch.AddRange(objVehicle.Gear);
                foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        lstGearToSearch.AddRange(objAccessory.Gear);
                    }
                }
                foreach (VehicleMod objVehicleMod in objVehicle.Mods.Where(x => x.Cyberware.Count > 0 || x.Weapons.Count > 0))
                {
                    foreach (Cyberware objCyberware in objVehicleMod.Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                    {
                        lstGearToSearch.AddRange(objCyberware.Gear);
                    }
                    foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                    {
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            lstGearToSearch.AddRange(objAccessory.Gear);
                        }
                    }
                }
            }
            Gear objGear = lstGearToSearch.DeepFindById(ParentID);

            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("type", WeaponType);
            objWriter.WriteElementString("reach", TotalReach.ToString(objCulture));
            objWriter.WriteElementString("accuracy", TotalAccuracy.ToString(objCulture));
            objWriter.WriteElementString("damage", CalculatedDamage(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("damage_english", CalculatedDamage(objCulture, GlobalOptions.DefaultLanguage));
            objWriter.WriteElementString("rawdamage", Damage);
            objWriter.WriteElementString("ap", TotalAP(strLanguageToPrint));
            objWriter.WriteElementString("mode", CalculatedMode(strLanguageToPrint));
            objWriter.WriteElementString("rc", TotalRC);
            objWriter.WriteElementString("ammo", CalculatedAmmo(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("ammo_english", CalculatedAmmo(objCulture, GlobalOptions.DefaultLanguage));
            objWriter.WriteElementString("conceal", CalculatedConcealability(objCulture));
            if (objGear != null)
            {
                objWriter.WriteElementString("avail", objGear.TotalAvail(GlobalOptions.CultureInfo, strLanguageToPrint, true));
                objWriter.WriteElementString("cost", objGear.TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
                objWriter.WriteElementString("owncost", objGear.OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            }
            else
            {
                objWriter.WriteElementString("avail", TotalAvail(strLanguageToPrint));
                objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
                objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            }
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("weaponname", WeaponName);
            objWriter.WriteElementString("location", Location);
            if (_lstAccessories.Count > 0)
            {
                objWriter.WriteStartElement("accessories");
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                    objAccessory.Print(objWriter, objCulture, strLanguageToPrint);
                objWriter.WriteEndElement();
            }

            IDictionary<string, string> dictionaryRanges = GetRangeStrings(objCulture);
            // <ranges>
            objWriter.WriteStartElement("ranges");
            objWriter.WriteElementString("name", DisplayRange(strLanguageToPrint));
            objWriter.WriteElementString("short", dictionaryRanges["short"]);
            objWriter.WriteElementString("medium", dictionaryRanges["medium"]);
            objWriter.WriteElementString("long", dictionaryRanges["long"]);
            objWriter.WriteElementString("extreme", dictionaryRanges["extreme"]);
            // </ranges>
            objWriter.WriteEndElement();

            // <alternateranges>
            objWriter.WriteStartElement("alternateranges");
            objWriter.WriteElementString("name", DisplayAlternateRange(strLanguageToPrint));
            objWriter.WriteElementString("short", dictionaryRanges["alternateshort"]);
            objWriter.WriteElementString("medium", dictionaryRanges["alternatemedium"]);
            objWriter.WriteElementString("long", dictionaryRanges["alternatelong"]);
            objWriter.WriteElementString("extreme", dictionaryRanges["alternateextreme"]);
            // </alternateranges>
            objWriter.WriteEndElement();

            foreach (Weapon objUnderbarrel in Children)
            {
                objWriter.WriteStartElement("underbarrel");
                objUnderbarrel.Print(objWriter, objCulture, strLanguageToPrint);
                objWriter.WriteEndElement();
            }

            // Currently loaded Ammo.
            Guid guiAmmo = GetClip(_intActiveAmmoSlot).Guid;

            objWriter.WriteElementString("currentammo", GetAmmoName(guiAmmo, strLanguageToPrint));
            objWriter.WriteStartElement("clips");
            foreach (Clip objClip in _ammo)
            {
                if (string.IsNullOrWhiteSpace(objClip.AmmoName))
                {
                    objClip.AmmoName = GetAmmoName(objClip.Guid, strLanguageToPrint);
                }
                objClip.Save(objWriter);
            }
            objWriter.WriteEndElement();

            //Don't seem to be used
            //objWriter.WriteElementString("ammoslot1", GetAmmoName(_guiAmmoLoaded));
            //objWriter.WriteElementString("ammoslot2", GetAmmoName(_guiAmmoLoaded2));
            //objWriter.WriteElementString("ammoslot3", GetAmmoName(_guiAmmoLoaded3));
            //objWriter.WriteElementString("ammoslot4", GetAmmoName(_guiAmmoLoaded4));

            objWriter.WriteElementString("dicepool", GetDicePool(objCulture));
            objWriter.WriteElementString("skill", Skill?.Name);

            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);

            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Get the name of Ammo from the character or Vehicle.
        /// </summary>
        /// <param name="guiAmmo">InternalId of the Ammo to find.</param>
        private string GetAmmoName(Guid guiAmmo, string strLanguage)
        {
            if (guiAmmo == Guid.Empty)
                return string.Empty;
            else
            {
                string strAmmoGuid = guiAmmo.ToString("D");
                Gear objAmmo = _objCharacter.Gear.DeepFindById(strAmmoGuid) ?? _objCharacter.Vehicles.FindVehicleGear(strAmmoGuid);

                if (objAmmo != null)
                    return objAmmo.DisplayNameShort(strLanguage);
                else
                    return string.Empty;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapon Accessories.
        /// </summary>
        public IList<WeaponAccessory> WeaponAccessories => _lstAccessories;

        /// <summary>
        /// Underbarrel Weapon.
        /// </summary>
        public IList<Weapon> UnderbarrelWeapons => _lstUnderbarrel;

        /// <summary>
        /// Children as Underbarrel Weapon.
        /// </summary>
        public IList<Weapon> Children => UnderbarrelWeapons;
        /// <summary>
        /// Whether or not this Weapon is an Underbarrel Weapon.
        /// </summary>
        public bool IsUnderbarrelWeapon
        {
            get
            {
                return Parent != null;
            }
        }

        /// <summary>
        /// Internal identifier which will be used to identify this Weapon.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

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

            if (!string.IsNullOrEmpty(_strWeaponName))
            {
                strReturn += " (\"" + _strWeaponName + "\")";
            }

            return strReturn;
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
        /// A custom name for the Weapon assigned by the player.
        /// </summary>
        public string WeaponName
        {
            get
            {
                return _strWeaponName;
            }
            set
            {
                _strWeaponName = value;
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            // If Categories are actually the name of object types, so pull them from the language file.
            if (Category == "Gear")
            {
                return LanguageManager.GetString("String_SelectPACKSKit_Gear", strLanguage);
            }
            else if (Category == "Cyberware")
            {
                return LanguageManager.GetString("String_SelectPACKSKit_Cyberware", strLanguage);
            }
            else if (Category == "Bioware")
            {
                return LanguageManager.GetString("String_SelectPACKSKit_Bioware", strLanguage);
            }

            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("weapons.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
        }

        /// <summary>
        /// Translated Ammo Category.
        /// </summary>
        public string DisplayAmmoCategory(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return AmmoCategory;

            return XmlManager.Load("weapons.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + AmmoCategory + "\"]/@translate")?.InnerText ?? AmmoCategory;
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
        /// Type of Weapon (either Melee or Ranged).
        /// </summary>
        public string WeaponType
        {
            get
            {
                return _strType;
            }
            set
            {
                _strType = value;
            }
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
            get
            {
                return _intReach;
            }
            set
            {
                _intReach = value;
            }
        }

        /// <summary>
        /// Accuracy.
        /// </summary>
        public string Accuracy
        {
            get
            {
                return _strAccuracy;
            }
            set
            {
                _strAccuracy = value;
            }
        }

        /// <summary>
        /// Damage.
        /// </summary>
        public string Damage
        {
            get
            {
                return _strDamage;
            }
            set
            {
                _strDamage = value;
            }
        }

        /// <summary>
        /// Armor Penetration.
        /// </summary>
        public string AP
        {
            get
            {
                return _strAP;
            }
            set
            {
                _strAP = value;
            }
        }

        /// <summary>
        /// Firing Mode.
        /// </summary>
        public string Mode
        {
            get
            {
                return _strMode;
            }
            set
            {
                _strMode = value;
            }
        }

        /// <summary>
        /// Recoil.
        /// </summary>
        public string RC
        {
            get
            {
                if (_strRC == "0")
                    return "-";
                else
                    return _strRC;
            }
            set
            {
                _strRC = value;
            }
        }

        /// <summary>
        /// Ammo.
        /// </summary>
        public string Ammo
        {
            get
            {
                return _strAmmo;
            }
            set
            {
                _strAmmo = value;
            }
        }

        /// <summary>
        /// Category of Ammo the Weapon uses.
        /// </summary>
        public string AmmoCategory
        {
            get
            {
                if (!string.IsNullOrEmpty(_strAmmoCategory))
                    return _strAmmoCategory;

                return _strCategory;
            }
        }

        /// <summary>
        /// The number of rounds remaining in the Weapon.
        /// </summary>
        public int AmmoRemaining
        {
            get { return GetClip(_intActiveAmmoSlot).Ammo; }
            set { GetClip(_intActiveAmmoSlot).Ammo = value; }
        }

        /// <summary>
        /// The type of Ammuniation loaded in the Weapon.
        /// </summary>
        public string AmmoLoaded
        {
            get { return GetClip(_intActiveAmmoSlot).Guid.ToString("D"); }
            set { GetClip(_intActiveAmmoSlot).Guid = Guid.Parse(value); }
        }

        /// <summary>
        /// Active Ammo slot number.
        /// </summary>
        public int ActiveAmmoSlot
        {
            get
            {
                return _intActiveAmmoSlot;
            }
            set
            {
                _intActiveAmmoSlot = value;
            }
        }

        /// <summary>
        /// Number of Ammo slots the Weapon has.
        /// </summary>
        public int AmmoSlots
        {
            get
            {
                return 1 + _lstAccessories.Sum(objAccessory => objAccessory.AmmoSlots);
            }
        }

        /// <summary>
        /// Concealability.
        /// </summary>
        public int Concealability
        {
            get
            {
                return _intConceal;
            }
            set
            {
                _intConceal = value;
            }
        }

        /// <summary>
        /// Avail.
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

        public string DisplayCost(out decimal decItemCost, decimal decMarkup = 0.0m)
        {
            decItemCost = 0;
            string strReturn = Cost;
            if (strReturn.StartsWith("Variable("))
            {
                strReturn = strReturn.TrimStart("Variable(", true).TrimEnd(')');
                decimal decMin = 0;
                decimal decMax = decimal.MaxValue;
                if (strReturn.Contains('-'))
                {
                    string[] strValues = strReturn.Split('-');
                    decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                    decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                }
                else
                    decMin = Convert.ToDecimal(strReturn.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                if (decMax == decimal.MaxValue)
                    strReturn = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                else
                    strReturn = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                decItemCost = decMin;
                return strReturn;
            }

            decimal decTotalCost = Convert.ToDecimal(strReturn, GlobalOptions.InvariantCultureInfo);

            decTotalCost *= 1.0m + decMarkup;

            if (DiscountCost)
                decTotalCost *= 0.9m;

            decItemCost = decTotalCost;

            return decTotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
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
                return _strPage;
            }
            set
            {
                _strPage = value;
            }
        }

        public string DisplayPage(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        public Weapon Parent { get; set; } = null;

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
        /// Whether the object allows accessories.
        /// </summary>
        public bool AllowAccessory
        {
            get
            {
                return _blnAllowAccessory;
            }
            set
            {
                _blnAllowAccessory = value;
                if (!_blnAllowAccessory)
                    foreach (Weapon objChild in UnderbarrelWeapons)
                        objChild.AllowAccessory = false;
            }
        }

        /// <summary>
        /// Location.
        /// </summary>
        public string Location
        {
            get
            {
                return _strLocation;
            }
            set
            {
                _strLocation = value;
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
        /// Vehicle to which the weapon is mounted (if none, returns null)
        /// </summary>
        public Vehicle ParentVehicle
        {
            get
            {
                return _objMountedVehicle;
            }
            set
            {
                _objMountedVehicle = value;
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
                _objWeaponMount = value;
                _objMountedVehicle = _objWeaponMount.Parent;
                foreach (Weapon objChild in Children)
                    objChild.ParentMount = value;
            }
        }

        /// <summary>
        /// Whether or not the Underbarrel Weapon is part of the parent Weapon by default.
        /// </summary>
        public bool IncludedInWeapon
        {
            get
            {
                return _blnIncludedInWeapon;
            }
            set
            {
                _blnIncludedInWeapon = value;
            }
        }

        /// <summary>
        /// Whether or not the Underbarrel Weapon is installed.
        /// </summary>
        public bool Installed
        {
            get
            {
                return _blnInstalled;
            }
            set
            {
                _blnInstalled = value;
            }
        }

        /// <summary>
        /// Active Skill that should be used with this Weapon instead of the default one.
        /// </summary>
        public string UseSkill
        {
            get
            {
                return _strUseSkill;
            }
            set
            {
                _strUseSkill = value;
            }
        }

        /// <summary>
        /// Whether or not the Armor's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get
            {
                return _blnDiscountCost && _objCharacter.BlackMarketDiscount;
            }
            set
            {
                _blnDiscountCost = value;
            }
        }

        /// <summary>
        /// Whether or not the Weapon requires Ammo to be reloaded.
        /// </summary>
        public bool RequireAmmo
        {
            get
            {
                return _blnRequireAmmo;
            }
            set
            {
                _blnRequireAmmo = value;
            }
        }

        /// <summary>
        /// The Active Skill Specialization that this Weapon uses, in addition to any others it would normally use.
        /// </summary>
        public string Spec => _strSpec;

        /// <summary>
        /// The second Active Skill Specialization that this Weapon uses, in addition to any others it would normally use.
        /// </summary>
        public string Spec2 => _strSpec2;

        public Guid SourceID
        {
            get
            {
                return _sourceID;
            }
        }

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("weapons.xml", strLanguage).SelectSingleNode("/chummer/weapons/weapon[id = \"" + _sourceID.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications.
        /// </summary>
        public string CalculatedConcealability(CultureInfo objCulture)
        {
            int intReturn = _intConceal;

            foreach (WeaponAccessory objAccessory in _lstAccessories)
            {
                if (objAccessory.Installed)
                    intReturn += objAccessory.Concealability;
            }

            /* Commented out because there's no reference to this in RAW
            // Add +4 for each Underbarrel Weapon installed.
            if (_lstUnderbarrel.Count > 0)
            {
                foreach (Weapon objUnderbarrelWeapon in _lstUnderbarrel)
                {
                    if (objUnderbarrelWeapon.Installed)
                        intReturn += 4;
                }
            }
            */

            // Factor in the character's Concealability modifiers.
            intReturn += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Concealability);

            string strReturn = string.Empty;
            if (intReturn >= 0)
                strReturn = '+' + intReturn.ToString(objCulture);
            else
                strReturn = intReturn.ToString(objCulture);

            return strReturn;
        }
        
        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        public string CalculatedDamage(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = _strDamage;

            // If the cost is determined by the Rating, evaluate the expression.
            string strDamage = _strDamage;
            string strDamageType = string.Empty;
            string strDamageExtra = string.Empty;

            int intUseSTR = 0;
            int intUseAGI = 0;
            if (strDamage.Contains("STR") || strDamage.Contains("AGI"))
            {
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        intUseAGI = ParentVehicle.Pilot;
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(ParentID, out VehicleMod objVehicleMod);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intAGI = objAttributeSource.TotalStrength;
                            }
                            intUseSTR = intSTR;
                            intUseAGI = intAGI;

                            if (intUseSTR == 0)
                                intUseSTR = objVehicleMod.TotalStrength;
                            if (intUseAGI == 0)
                                intUseAGI = objVehicleMod.TotalAgility;
                        }
                    }
                    else
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intAGI = objAttributeSource.TotalStrength;
                            }

                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                        if (intUseAGI == 0)
                            intUseAGI = _objCharacter.AGI.TotalValue;
                    }
                }
                else if (ParentVehicle == null)
                {
                    if (intUseSTR == 0)
                        intUseSTR = _objCharacter.STR.TotalValue;
                    if (intUseAGI == 0)
                        intUseAGI = _objCharacter.AGI.TotalValue;
                }

                if (_strCategory == "Throwing Weapons" || _strUseSkill == "Throwing Weapons")
                    intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);
            }
            
            foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList)
            {
                if (objLoopAttribute.Abbrev == "STR")
                    strDamage = strDamage.Replace("STR", intUseSTR.ToString());
                else if (objLoopAttribute.Abbrev == "AGI")
                    strDamage = strDamage.Replace("AGI", intUseAGI.ToString());
                else
                    strDamage = strDamage.CheapReplace(objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
            }
            foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.SpecialAttributeList)
            {
                if (objLoopAttribute.Abbrev == "STR")
                    strDamage = strDamage.Replace("STR", intUseSTR.ToString());
                else if (objLoopAttribute.Abbrev == "AGI")
                    strDamage = strDamage.Replace("AGI", intUseAGI.ToString());
                else
                    strDamage = strDamage.CheapReplace(objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
            }

            // Evaluate the min expression if there is one.
            if (strDamage.Contains("min("))
            {
                string strMin = string.Empty;
                int intStart = strDamage.IndexOf("min(");
                int intEnd = strDamage.IndexOf(')', intStart);
                strMin = strDamage.Substring(intStart, intEnd - intStart + 1);
                
                string[] strValue = strMin.TrimStart("min(", true).TrimEnd(')').Split(',');
                int intMinValue = Convert.ToInt32(strValue[0]);
                for (int i = 1; i < strValue.Length; ++i)
                {
                    intMinValue = Math.Min(intMinValue,Convert.ToInt32(strValue[i]));
                }

                strDamage = strDamage.Replace(strMin, intMinValue.ToString());
            }

            // Place the Damage Type (P or S) into a string and remove it from the expression.
            if (strDamage.Contains("P or S"))
            {
                strDamageType = "P or S";
                strDamage = strDamage.Replace("P or S", string.Empty);
            }
            else
            {
                if (strDamage.Contains('P'))
                {
                    strDamageType = "P";
                    strDamage = strDamage.FastEscape('P');
                }
                if (strDamage.Contains('S'))
                {
                    strDamageType = "S";
                    strDamage = strDamage.FastEscape('S');
                }
            }
            // Place any extra text like (e) and (f) in a string and remove it from the expression.
            if (strDamage.Contains("(e)"))
            {
                strDamageExtra = "(e)";
                strDamage = strDamage.Replace("(e)", string.Empty);
            }
            if (strDamage.Contains("(f)"))
            {
                strDamageExtra = "(f)";
                strDamage = strDamage.Replace("(f)", string.Empty);
            }

            // Look for splash damage info.
            if (strDamage.Contains("/m)") || strDamage.Contains(" Radius)"))
            {
                string strSplash = strDamage.Substring(strDamage.IndexOf('('), strDamage.IndexOf(')') - strDamage.IndexOf('(') + 1);
                strDamageExtra += ' ' + strSplash;
                strDamage = strDamage.Replace(strSplash, string.Empty).Trim();
            }

            // Replace the division sign with "div" since we're using XPath.
            strDamage = strDamage.Replace("/", " div ");

            // Include WeaponCategoryDV Improvements.
            int intImprove = 0;
            if (_objCharacter != null)
            {
                string strCategory = Category;
                if (strCategory == "Unarmed")
                {
                    strCategory = "Unarmed Combat";
                }
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.WeaponCategoryDV && objImprovement.Enabled && (objImprovement.ImprovedName == strCategory || "Cyberware " + objImprovement.ImprovedName == strCategory))
                        intImprove += objImprovement.Value;
                    if (!string.IsNullOrEmpty(_strUseSkill))
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.WeaponCategoryDV && objImprovement.Enabled && (objImprovement.ImprovedName == _strUseSkill || "Cyberware " + objImprovement.ImprovedName == strCategory))
                            intImprove += objImprovement.Value;
                    }
                }
            }

            // If this is the Unarmed Attack Weapon and the character has the UnarmedDVPhysical Improvement, change the type to Physical.
            // This should also add any UnarmedDV bonus which only applies to Unarmed Combat, not Unarmed Weapons.
            if (_strName == "Unarmed Attack")
            {
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.UnarmedDVPhysical && objImprovement.Enabled)
                        strDamageType = "P";
                    if (objImprovement.ImproveType == Improvement.ImprovementType.UnarmedDV && objImprovement.Enabled)
                        intImprove += objImprovement.Value;
                }
            }

            // This should also add any UnarmedDV bonus to Unarmed physical weapons if the option is enabled.
            else if (Skill != null && Skill.Name == "Unarmed Combat" && _objCharacter.Options.UnarmedImprovementsApplyToWeapons)
            {
                intImprove += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedDV);
            }
            bool blnDamageReplaced = false;

            // Add in the DV bonus from any Weapon Mods.
            foreach (WeaponAccessory objAccessory in _lstAccessories)
            {
                if (objAccessory.Installed)
                {
                    if (!string.IsNullOrEmpty(objAccessory.DamageType))
                    {
                        strDamageType = string.Empty;
                        strDamageExtra = objAccessory.DamageType;
                    }
                    // Adjust the Weapon's Damage.
                    if (!string.IsNullOrEmpty(objAccessory.Damage))
                    {
                        strDamage += " + " + objAccessory.Damage;
                    }
                    if (!string.IsNullOrEmpty(objAccessory.DamageReplacement))
                    {
                        blnDamageReplaced = true;
                        strDamage = objAccessory.DamageReplacement;
                    }
                }
            }
            if (intImprove != 0)
                strDamage += " + " + intImprove.ToString();

            CharacterOptions objOptions = _objCharacter.Options;
            int intBonus = 0;
            if (objOptions.MoreLethalGameplay)
                intBonus = 2;

            // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
            if (!string.IsNullOrEmpty(AmmoLoaded))
            {
                // Look for Ammo on the character.
                Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded);
                if (objGear == null)
                {
                    objGear = _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);
                }
                if (objGear != null)
                {
                    if (objGear.WeaponBonus != null)
                    {
                        // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                        if (!(objGear.WeaponBonus.InnerXml.Contains("(f)") && _strDamage.Contains("(f)")))
                        {
                            if (objGear.WeaponBonus["damagetype"] != null)
                            {
                                strDamageType = string.Empty;
                                strDamageExtra = objGear.WeaponBonus["damagetype"].InnerText;
                            }
                            // Adjust the Weapon's Damage.
                            if (objGear.WeaponBonus["damage"] != null)
                                strDamage += " + " + objGear.WeaponBonus["damage"].InnerText;
                            if (objGear.WeaponBonus["damagereplace"] != null)
                            {
                                blnDamageReplaced = true;
                                strDamage = objGear.WeaponBonus["damagereplace"].InnerText;
                            }
                        }
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in objGear.Children)
                    {
                        if (objChild.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                            if (!(objChild.WeaponBonus.InnerXml.Contains("(f)") && _strDamage.Contains("(f)")))
                            {
                                if (objChild.WeaponBonus["damagetype"] != null)
                                {
                                    strDamageType = string.Empty;
                                    strDamageExtra = objChild.WeaponBonus["damagetype"].InnerText;
                                }
                                // Adjust the Weapon's Damage.
                                if (objChild.WeaponBonus["damage"] != null)
                                    strDamage += " + " + objChild.WeaponBonus["damage"].InnerText;
                                if (objChild.WeaponBonus["damagereplace"] != null)
                                {
                                    blnDamageReplaced = true;
                                    strDamage = objChild.WeaponBonus["damagereplace"].InnerText;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            if (!blnDamageReplaced)
            {
                try
                {
                    int intDamage = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(strDamage))) + intBonus;
                    if (_strName == "Unarmed Attack (Smashing Blow)")
                        intDamage *= 2;
                    strDamage = intDamage.ToString(objCulture);
                }
                catch (XPathException) { }
                catch (OverflowException) { } // Result is text and not a double
                catch (InvalidCastException) { } // Result is text and not a double

                strReturn = strDamage + strDamageType + strDamageExtra;
            }
            else
            {
                // Place the Damage Type (P or S) into a string and remove it from the expression.
                if (strDamage.Contains("P or S"))
                {
                    strDamageType = "P or S";
                    strDamage = strDamage.Replace("P or S", string.Empty);
                }
                if (strDamage.Contains('P'))
                {
                    strDamageType = "P";
                    strDamage = strDamage.FastEscape('P');
                }
                if (strDamage.Contains('S'))
                {
                    strDamageType = "S";
                    strDamage = strDamage.FastEscape('S');
                }
                // Place any extra text like (e) and (f) in a string and remove it from the expression.
                if (strDamage.Contains("(e)"))
                {
                    strDamageExtra = "(e)";
                    strDamage = strDamage.Replace("(e)", string.Empty);
                }
                if (strDamage.Contains("(f)"))
                {
                    strDamageExtra = "(f)";
                    strDamage = strDamage.Replace("(f)", string.Empty);
                }
                // Replace the division sign with "div" since we're using XPath.
                strDamage = strDamage.Replace("/", " div ");

                try
                {
                    int intDamage = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(strDamage))) + intBonus;
                    if (_strName == "Unarmed Attack (Smashing Blow)")
                        intDamage *= 2;
                    strDamage = intDamage.ToString(objCulture);
                }
                catch (XPathException) { }
                catch (OverflowException) { } // Result is text and not a double
                catch (InvalidCastException) { } // Result is text and not a double
                strReturn = strDamage + strDamageType + strDamageExtra;
            }

            // If the string couldn't be parsed (resulting in NaN which will happen if it is a special string like "Grenade", "Chemical", etc.), return the Weapon's Damage string.
            if (strReturn.StartsWith("NaN"))
                strReturn = _strDamage;

            // Translate the Damage Code.
            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                strReturn = strReturn.CheapReplace("S", () => LanguageManager.GetString("String_DamageStun", strLanguage));
                strReturn = strReturn.CheapReplace("P", () => LanguageManager.GetString("String_DamagePhysical", strLanguage));

                strReturn = strReturn.CheapReplace("Chemical", () => LanguageManager.GetString("String_DamageChemical", strLanguage));
                strReturn = strReturn.CheapReplace("Special", () => LanguageManager.GetString("String_DamageSpecial", strLanguage));
                strReturn = strReturn.CheapReplace("(e)", () => LanguageManager.GetString("String_DamageElectric", strLanguage));
                strReturn = strReturn.CheapReplace("(f)", () => LanguageManager.GetString("String_DamageFlechette", strLanguage));
                strReturn = strReturn.CheapReplace("P or S", () => LanguageManager.GetString("String_DamagePOrS", strLanguage));
                strReturn = strReturn.CheapReplace("Grenade", () => LanguageManager.GetString("String_DamageGrenade", strLanguage));
                strReturn = strReturn.CheapReplace("Missile", () => LanguageManager.GetString("String_DamageMissile", strLanguage));
                strReturn = strReturn.CheapReplace("Mortar", () => LanguageManager.GetString("String_DamageMortar", strLanguage));
                strReturn = strReturn.CheapReplace("Rocket", () => LanguageManager.GetString("String_DamageRocket", strLanguage));
                strReturn = strReturn.CheapReplace("Radius", () => LanguageManager.GetString("String_DamageRadius", strLanguage));
                strReturn = strReturn.CheapReplace("As Drug/Toxin", () => LanguageManager.GetString("String_DamageAsDrugToxin", strLanguage));
                strReturn = strReturn.CheapReplace("as round", () => LanguageManager.GetString("String_DamageAsRound", strLanguage));
                strReturn = strReturn.CheapReplace("/m", () => "/" + LanguageManager.GetString("String_DamageMeter", strLanguage));
            }

            return strReturn;
        }

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        public string CalculatedAmmo(CultureInfo objCulture, string strLanguage)
        {
            string[] strAmmos = _strAmmo.Split(' ');
            string strReturn = string.Empty;
            int intAmmoBonus = 0;

            int extendedMax = _lstAccessories.Count != 0 ? _lstAccessories.Max(x => (x.Name.Contains("Extended Clip") ? 1 : 0) * x.Rating) : 0;

            foreach (WeaponAccessory objAccessory in _lstAccessories)
            {
                // Replace the Ammo value.
                if (!string.IsNullOrEmpty(objAccessory.AmmoReplace))
                {
                    strAmmos = new string[] { objAccessory.AmmoReplace };
                    break;
                }
                intAmmoBonus += objAccessory.AmmoBonus;
            }
            if (ParentMount != null)
            {
                foreach (VehicleMod objMod in ParentMount.Mods)
                {
                    if (!string.IsNullOrEmpty(objMod.AmmoReplace))
                    {
                        strAmmos = new string[] { objMod.AmmoReplace };
                        break;
                    }
                    intAmmoBonus += objMod.AmmoBonus;
                }
            }
            foreach (string strAmmo in strAmmos)
            {
                string strThisAmmo = strAmmo;
                if (strThisAmmo.Contains('('))
                {
                    string strAmmoString = string.Empty;
                    string strPrepend = string.Empty;
                    int intAmmo = 0;
                    strThisAmmo = strThisAmmo.Substring(0, strThisAmmo.IndexOf('('));
                    if (strThisAmmo.Contains('x'))
                    {
                        strPrepend = strThisAmmo.Substring(0, strThisAmmo.IndexOf('x') + 1);
                        strThisAmmo = strThisAmmo.Substring(strThisAmmo.IndexOf('x') + 1, strThisAmmo.Length - (strThisAmmo.IndexOf('x') + 1));
                    }
                    strThisAmmo = strThisAmmo.CheapReplace("Weapon", () => _strAmmo);
                    // If this is an Underbarrel Weapons that has been added, cut the Ammo capacity in half.
                    try
                    {
                        if (IsUnderbarrelWeapon && !IncludedInWeapon)
                            intAmmo = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(strThisAmmo))) / 2;
                        else
                            intAmmo = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(strThisAmmo)));
                    }
                    catch (FormatException) { }

                    intAmmo += (intAmmo * intAmmoBonus + 99) / 100;

                    if (extendedMax > 0 && strAmmo.Contains("(c)"))
                    {
                        //Multiply by 2-4 and divide by 2 to get 1, 1.5 or 2 times orginal result
                        intAmmo = (intAmmo*(2 + extendedMax))/2;
                    }

                    strAmmoString = intAmmo.ToString(objCulture);
                    if (!string.IsNullOrEmpty(strPrepend))
                        strAmmoString = strPrepend + strAmmoString;
                    strThisAmmo = strAmmoString + strAmmo.Substring(strAmmo.IndexOf('('), strAmmo.Length - strAmmo.IndexOf('('));
                }
                strReturn += strThisAmmo + ' ';
            }
            strReturn = strReturn.Trim();

            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                // Translate the Ammo string.
                strReturn = strReturn.CheapReplace(" or ", () => ' ' + LanguageManager.GetString("String_Or", strLanguage) + ' ');
                strReturn = strReturn.CheapReplace(" belt", () => LanguageManager.GetString("String_AmmoBelt", strLanguage));
                strReturn = strReturn.CheapReplace(" Energy", () => LanguageManager.GetString("String_AmmoEnergy", strLanguage));
                strReturn = strReturn.CheapReplace(" external source", () => LanguageManager.GetString("String_AmmoExternalSource", strLanguage));
                strReturn = strReturn.CheapReplace(" Special", () => LanguageManager.GetString("String_AmmoSpecial", strLanguage));

                strReturn = strReturn.CheapReplace("(b)", () => '(' + LanguageManager.GetString("String_AmmoBreakAction", strLanguage) + ')');
                strReturn = strReturn.CheapReplace("(belt)", () => '(' + LanguageManager.GetString("String_AmmoBelt", strLanguage) + ')');
                strReturn = strReturn.CheapReplace("(box)", () => '(' + LanguageManager.GetString("String_AmmoBox", strLanguage) + ')');
                strReturn = strReturn.CheapReplace("(c)", () => '(' + LanguageManager.GetString("String_AmmoClip", strLanguage) + ')');
                strReturn = strReturn.CheapReplace("(cy)", () => '(' + LanguageManager.GetString("String_AmmoCylinder", strLanguage) + ')');
                strReturn = strReturn.CheapReplace("(d)", () => '(' + LanguageManager.GetString("String_AmmoDrum", strLanguage) + ')');
                strReturn = strReturn.CheapReplace("(m)", () => '(' + LanguageManager.GetString("String_AmmoMagazine", strLanguage) + ')');
                strReturn = strReturn.CheapReplace("(ml)", () => '(' + LanguageManager.GetString("String_AmmoMuzzleLoad", strLanguage) + ')');
            }

            return strReturn;
        }

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        public string CalculatedMode(string strLanguage)
        {
            List<string> lstModes = new List<string>();
            string[] strModes = _strMode.Split('/');
            // Move the contents of the array to a list so it's easier to work with.
            foreach (string strMode in strModes)
                lstModes.Add(strMode);

            // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
            if (!string.IsNullOrEmpty(AmmoLoaded))
            {
                // Look for Ammo on the character.
                Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded);
                if (objGear == null)
                {
                    objGear = _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);
                }
                if (objGear != null)
                {
                    if (objGear.WeaponBonus != null)
                    {
                        string strFireMode = objGear.WeaponBonus["firemode"]?.InnerText;
                        if (!string.IsNullOrEmpty(strFireMode))
                        {
                            if (strFireMode.Contains('/'))
                            {
                                strModes = strFireMode.Split('/');

                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(strFireMode);
                            }
                        }
                        strFireMode = objGear.WeaponBonus["modereplace"]?.InnerText;
                        if (!string.IsNullOrEmpty(strFireMode))
                        {
                            lstModes.Clear();
                            if (strFireMode.Contains('/'))
                            {
                                strModes = strFireMode.Split('/');
                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(strFireMode);
                            }
                        }
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in objGear.Children.GetAllDescendants(x => x.Children))
                    {
                        if (objChild.WeaponBonus != null)
                        {
                            string strFireMode = objChild.WeaponBonus["firemode"]?.InnerText;
                            if (!string.IsNullOrEmpty(strFireMode))
                            {
                                if (strFireMode.Contains('/'))
                                {
                                    strModes = strFireMode.Split('/');

                                    // Move the contents of the array to a list so it's easier to work with.
                                    foreach (string strMode in strModes)
                                        lstModes.Add(strMode);
                                }
                                else
                                {
                                    lstModes.Add(strFireMode);
                                }
                            }
                            strFireMode = objChild.WeaponBonus["modereplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strFireMode))
                            {
                                lstModes.Clear();
                                if (strFireMode.Contains('/'))
                                {
                                    strModes = strFireMode.Split('/');
                                    // Move the contents of the array to a list so it's easier to work with.
                                    foreach (string strMode in strModes)
                                        lstModes.Add(strMode);
                                }
                                else
                                {
                                    lstModes.Add(strFireMode);
                                }
                                break;
                            }
                        }
                    }

                    // Do the same for any accessories/modifications.
                    foreach (WeaponAccessory objAccessory in _lstAccessories)
                    {
                        if (!string.IsNullOrEmpty(objAccessory.FireMode))
                        {
                            if (objAccessory.FireMode.Contains('/'))
                            {
                                strModes = objAccessory.FireMode.Split('/');

                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(objAccessory.FireMode);
                            }
                        }
                        if (!string.IsNullOrEmpty(objAccessory.FireModeReplacement))
                        {
                            lstModes.Clear();
                            if (objAccessory.FireModeReplacement.Contains('/'))
                            {
                                strModes = objAccessory.FireModeReplacement.Split('/');

                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(objAccessory.FireModeReplacement);
                            }
                            break;
                        }
                    }
                }
            }

            foreach (WeaponAccessory objAccessory in _lstAccessories.Where(x => !string.IsNullOrEmpty(x.AddMode)))
            {
                lstModes.Add(objAccessory.AddMode);
            }

            string strReturn = string.Empty;
            if (lstModes.Contains("SS"))
                strReturn += LanguageManager.GetString("String_ModeSingleShot", strLanguage) + "/";
            if (lstModes.Contains("SA"))
                strReturn += LanguageManager.GetString("String_ModeSemiAutomatic", strLanguage) + "/";
            if (lstModes.Contains("BF"))
                strReturn += LanguageManager.GetString("String_ModeBurstFire", strLanguage) + "/";
            if (lstModes.Contains("FA"))
                strReturn += LanguageManager.GetString("String_ModeFullAutomatic", strLanguage) + "/";
            if (lstModes.Contains("Special"))
                strReturn += LanguageManager.GetString("String_ModeSpecial", strLanguage) + "/";

            // Remove the trailing "/".
            if (!string.IsNullOrEmpty(strReturn))
                strReturn = strReturn.Substring(0, strReturn.Length - 1);

            return strReturn;
        }

        /// <summary>
        /// Determine if the Weapon is capable of firing in a particular mode.
        /// </summary>
        /// <param name="strFindMode">Firing mode to find.</param>
        public bool AllowMode(string strFindMode)
        {
            string[] strModes = CalculatedMode(GlobalOptions.Language).Split('/');
            return strModes.Any(strMode => strMode == strFindMode);
        }

        /// <summary>
        /// Weapon Cost to use when working with Total Cost price modifiers for Weapon Mods.
        /// </summary>
        public decimal MultipliableCost
        {
            get
            {
                decimal decReturn = OwnCost;

                // Run through the list of Weapon Mods.
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                {
                    if (!objAccessory.IncludedInWeapon)
                    {
                        if (!objAccessory.Cost.StartsWith("Total Cost"))
                            decReturn += objAccessory.TotalCost;
                    }
                }

                return decReturn;
            }
        }

        public string AccessoryMounts
        {
            get
            {
                string strMounts = string.Empty;
                XmlNodeList objXmlMountList = GetNode()?.SelectNodes("accessorymounts/mount");

                if (objXmlMountList == null) return strMounts;
                foreach (XmlNode objXmlMount in objXmlMountList)
                {
                    bool blnFound = _lstAccessories.Any(objAccessory => objAccessory.Mount == objXmlMount.InnerText || objAccessory.ExtraMount == objXmlMount.InnerText) || UnderbarrelWeapons.Any(weapon => weapon.Mount == objXmlMount.InnerText || weapon.ExtraMount == objXmlMount.InnerText);
                    if (!blnFound)
                    {
                        strMounts += objXmlMount.InnerText + "/";
                    }
                }

                // Remove the trailing /
                if (!string.IsNullOrEmpty(strMounts) && strMounts.Contains('/'))
                    strMounts = strMounts.Substring(0, strMounts.Length - 1);
                return strMounts;
            }
        }

        /// <summary>
        /// The Weapon's total cost including Accessories and Modifications.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decReturn = OwnCost;

                // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
                decReturn += _lstAccessories.Where(objAccessory => !objAccessory.IncludedInWeapon).AsParallel().Sum(objAccessory => objAccessory.TotalCost);

                // Include the cost of any Underbarrel Weapon.
                if (_lstUnderbarrel.Count > 0)
                {
                    decReturn += _lstUnderbarrel.AsParallel().Sum(objUnderbarrel => objUnderbarrel.TotalCost);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Weapon itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                // If this is a Cyberware or Gear Weapon, remove the Weapon Cost from this since it has already been paid for through the parent item (but is needed to calculate Mod price).
                if (_blnCyberware || _strCategory == "Gear" || !string.IsNullOrEmpty(ParentID))
                    return 0;
                else
                {
                    decimal decReturn = Convert.ToDecimal(Cost);
                    if (DiscountCost)
                        decReturn *= 0.9m;
                    return decReturn;
                }
            }
        }

        /// <summary>
        /// The Weapon's total AP including Ammunition.
        /// </summary>
        public string TotalAP(string strLanguage)
        {
            string strAP = _strAP;

            int intAP = 0;
            int bonusAP = 0;
            // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
            if (!string.IsNullOrEmpty(AmmoLoaded))
            {
                // Look for Ammo on the character.
                Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded);
                if (objGear == null)
                {
                    objGear = _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);
                }
                if (objGear?.WeaponBonus != null)
                {
                    // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                    if (!(objGear.WeaponBonus.InnerXml.Contains("(f)") && _strDamage.Contains("(f)")))
                    {
                        // Armor-Piercing Flechettes (and any other that might come along that does not explicitly add +5 AP) should instead reduce
                        // the AP for Flechette-only Weapons which have the standard Flechette +5 AP built into their stats.
                        if (_strDamage.Contains("(f)") && objGear.Name.Contains("Flechette"))
                        {
                            bonusAP -= 5;
                        }
                        else
                        {
                            // Change the Weapon's Damage Type.
                            string strAPReplace = objGear.WeaponBonus["apreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPReplace))
                                strAP = strAPReplace;
                            // Adjust the Weapon's Damage.
                            string strAPAdd = objGear.WeaponBonus["ap"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPAdd))
                                bonusAP += Convert.ToInt32(strAPAdd);
                        }
                    }
                }

                if (_objCharacter != null)
                {
                    // Add any UnarmedAP bonus for the Unarmed Attack item.
                    if (_strName == "Unarmed Attack" || Skill != null && Skill.Name == "Unarmed Combat" && _objCharacter.Options.UnarmedImprovementsApplyToWeapons)
                    {
                        bonusAP += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedAP);
                    }
                }
            }

            foreach (WeaponAccessory objAccessory in _lstAccessories.Where(objAccessory => objAccessory.Installed))
            {
                // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                if (!(objAccessory.DamageType.Contains("(f)") && _strDamage.Contains("(f)")))
                {
                    // Armor-Piercing Flechettes (and any other that might come along that does not explicitly add +5 AP) should instead reduce
                    // the AP for Flechette-only Weapons which have the standard Flechette +5 AP built into their stats.
                    if (_strDamage.Contains("(f)") && objAccessory.Name.Contains("Flechette"))
                    {
                        bonusAP -= 5;
                    }
                    else
                    {
                        // Change the Weapon's AP value.
                        if (!string.IsNullOrEmpty(objAccessory.APReplacement))
                            strAP = objAccessory.APReplacement;
                        // Adjust the Weapon's AP value.
                        if (!string.IsNullOrEmpty(objAccessory.AP))
                            bonusAP += Convert.ToInt32(objAccessory.AP);
                    }
                }
            }

            if (strAP == "-")
                strAP = "0";

            StringBuilder objAP = new StringBuilder(strAP);

            int intUseSTR = 0;
            int intUseAGI = 0;
            int intUseSTRBase = 0;
            int intUseAGIBase = 0;
            if (strAP.Contains("{STR") || strAP.Contains("{AGI"))
            {
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        intUseSTRBase = intUseSTR;
                        intUseAGI = ParentVehicle.Pilot;
                        intUseAGIBase = intUseAGI;
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(ParentID, out VehicleMod objVehicleMod);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intSTRBase = objAttributeSource.BaseStrength;
                            int intAGI = objAttributeSource.TotalAgility;
                            int intAGIBase = objAttributeSource.BaseAgility;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0 || intSTRBase != 0 || intAGIBase != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intSTRBase = objAttributeSource.BaseStrength;
                                intAGI = objAttributeSource.TotalAgility;
                                intAGIBase = objAttributeSource.BaseAgility;
                            }
                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                            intUseSTRBase = intSTRBase;
                            intUseAGIBase = intAGIBase;

                            if (intUseSTR == 0)
                                intUseSTR = objVehicleMod.TotalStrength;
                            if (intUseAGI == 0)
                                intUseAGI = objVehicleMod.TotalAgility;
                            if (intUseSTRBase == 0)
                                intUseSTRBase = ParentVehicle.TotalBody;
                            if (intUseAGIBase == 0)
                                intUseAGIBase = ParentVehicle.Pilot;
                        }
                    }
                    else
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intSTRBase = objAttributeSource.BaseStrength;
                            int intAGI = objAttributeSource.TotalAgility;
                            int intAGIBase = objAttributeSource.BaseAgility;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0 || intSTRBase != 0 || intAGIBase != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intSTRBase = objAttributeSource.BaseStrength;
                                intAGI = objAttributeSource.TotalAgility;
                                intAGIBase = objAttributeSource.BaseAgility;
                            }

                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                            intUseSTRBase = intSTRBase;
                            intUseAGIBase = intAGIBase;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                        if (intUseAGI == 0)
                            intUseAGI = _objCharacter.AGI.TotalValue;
                        if (intUseSTRBase == 0)
                            intUseSTRBase = _objCharacter.STR.TotalBase;
                        if (intUseAGIBase == 0)
                            intUseAGIBase = _objCharacter.AGI.TotalBase;
                    }
                }
                else if (ParentVehicle == null)
                {
                    if (intUseSTR == 0)
                        intUseSTR = _objCharacter.STR.TotalValue;
                    if (intUseAGI == 0)
                        intUseAGI = _objCharacter.AGI.TotalValue;
                }

                if (_strCategory == "Throwing Weapons" || _strUseSkill == "Throwing Weapons")
                    intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);
            }

            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                CharacterAttrib objLoopAttribute = _objCharacter.GetAttribute(strAttribute);
                if (strAttribute == "STR")
                {
                    objAP.Replace("{" + strAttribute + "}", intUseSTR.ToString());
                    objAP.Replace("{" + strAttribute + "Base}", intUseSTRBase.ToString());
                }
                else if (strAttribute == "AGI")
                {
                    objAP.Replace("{" + strAttribute + "}", intUseAGI.ToString());
                    objAP.Replace("{" + strAttribute + "Base}", intUseAGIBase.ToString());
                }
                else
                {
                    objAP.CheapReplace(_strAccuracy, "{" + strAttribute + "}", () => objLoopAttribute.TotalValue.ToString());
                    objAP.CheapReplace(_strAccuracy, "{" + strAttribute + "Base}", () => objLoopAttribute.TotalBase.ToString());
                }
            }

            try
            {
                intAP = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(objAP.ToString()));
            }
            catch (FormatException)
            {
                // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                return strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage));
            }
            catch (OverflowException)
            {
                // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                return strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage));
            }
            catch (XPathException)
            {
                // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                return strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage));
            }
            catch (InvalidCastException)
            {
                // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                return strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage));
            }
            intAP += bonusAP;
            if (intAP == 0)
                return "-";
            else if (intAP > 0)
                return '+' + intAP.ToString();
            else
                return intAP.ToString();
        }

        private static readonly char[] lstParenthesesChars = "()".ToCharArray();
        /// <summary>
        /// The Weapon's total RC including Accessories and Modifications.
        /// </summary>
        public string TotalRC
        {
            get
            {
                string strRCBase = "0";
                string strRCFull = "0";
                string strRC = string.Empty;
                string strRCTip = string.Empty;
                int intRCBase = 0;
                int intRCFull = 0;
                int intRCModifier = 0;

                List<Tuple<string, int>> lstRCGroups = new List<Tuple<string, int>>(5);
                List<Tuple<string, int>> lstRCDeployGroups = new List<Tuple<string, int>>(5);

                int intPos = _strRC.IndexOf('(');
                if (intPos != -1)
                {
                    if (intPos == 0)
                    {
                        // The string contains only RC from pieces that can be removed - "(x)" only.
                        strRCFull = _strRC;
                    }
                    // The string contains a mix of both fixed and removable RC. "x(y)".
                    else
                    {
                        strRCBase = _strRC.Substring(0, intPos);
                        strRCFull = _strRC.Substring(intPos, _strRC.Length - intPos);
                    }
                }
                else
                {
                    // The string contains only RC from fixed pieces - "x" only.
                    strRCBase = _strRC;
                    strRCFull = _strRC;
                }

                strRCTip = "1 ";
                if (strRCBase != "0")
                {
                    strRCTip += "+ "+ LanguageManager.GetString("Label_Base", GlobalOptions.Language) + "(" + strRCBase + ')';
                }

                intRCBase = Convert.ToInt32(strRCBase);
                intRCFull = Convert.ToInt32(strRCFull.Trim(lstParenthesesChars));

                if (intRCBase < 0)
                {
                    intRCModifier = intRCBase;
                    intRCBase = 0;
                }

                // Check if the Weapon has Ammunition loaded and look for any Recoil bonus.
                if (!string.IsNullOrEmpty(AmmoLoaded) && AmmoLoaded != "00000000-0000-0000-0000-000000000000")
                {
                    Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded);
                    if (objGear == null)
                    {
                        objGear = _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);
                    }

                    // Change the Weapon's Damage Type.
                    string strRCBonus = objGear?.WeaponBonus?["rc"]?.InnerText;
                    if (!string.IsNullOrEmpty(strRCBonus))
                    {
                        intRCBase += Convert.ToInt32(strRCBonus);
                        intRCFull += Convert.ToInt32(strRCBonus);

                        strRCTip += " + " + objGear.DisplayName(GlobalOptions.Language) + " (" + strRCBonus + ')';
                    }
                }

                // Now that we know the Weapon's RC values, run through all of the Accessories and add theirs to the mix.
                // Only add in the values for items that do not come with the weapon.
                foreach (WeaponAccessory objAccessory in _lstAccessories.Where(objAccessory => !string.IsNullOrEmpty(objAccessory.RC) && objAccessory.Installed))
                {
                    if (_objCharacter.Options.RestrictRecoil && objAccessory.RCGroup != 0)
                    {
                        int intItemRC = Convert.ToInt32(objAccessory.RC);
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
                            lstLoopRCGroup[objAccessory.RCGroup - 1] = new Tuple<string, int>(objAccessory.DisplayName(GlobalOptions.Language), intItemRC);
                        }
                    }
                    else
                    {
                        intRCFull += Convert.ToInt32(objAccessory.RC);
                        if (!objAccessory.RCDeployable)
                        {
                            intRCBase += Convert.ToInt32(objAccessory.RC);
                        }
                        strRCTip += " + " + objAccessory.DisplayName(GlobalOptions.Language) + " (" + objAccessory.RC + ')';
                    }
                }
                
                foreach (Tuple<string, int> objRCGroup in lstRCGroups)
                {
                    if (!string.IsNullOrEmpty(objRCGroup.Item1))
                    {
                        // Add in the Recoil Group bonuses.
                        intRCBase += objRCGroup.Item2;
                        intRCFull += objRCGroup.Item2;
                        strRCTip += $" + {objRCGroup.Item1} ({objRCGroup.Item2.ToString()})";
                    }
                }
                foreach (Tuple<string, int> objRCGroup in lstRCDeployGroups)
                {
                    if (!string.IsNullOrEmpty(objRCGroup.Item1))
                    {
                        // Add in the Recoil Group bonuses.
                        intRCFull += objRCGroup.Item2;
                        strRCTip += " +  " + LanguageManager.GetString("Tip_RecoilAccessories", GlobalOptions.Language).Replace("{0}", objRCGroup.Item1).Replace("{1}", objRCGroup.Item2.ToString());
                    }
                }

                int intUseSTR = 0;
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(ParentID, out VehicleMod objVehicleMod);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intAGI = objAttributeSource.TotalStrength;
                            }
                            intUseSTR = intSTR;

                            if (intUseSTR == 0)
                                intUseSTR = objVehicleMod.TotalStrength;
                        }
                    }
                    else
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                            }

                            intUseSTR = intSTR;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                    }
                }
                else if (ParentVehicle == null)
                {
                    if (intUseSTR == 0)
                        intUseSTR = _objCharacter.STR.TotalValue;
                }

                if (_strCategory == "Throwing Weapons" || _strUseSkill == "Throwing Weapons")
                    intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);

                int intStrRC = ((intUseSTR - 1)/3) + 1;

                intRCBase += intStrRC + 1;
                intRCFull += intStrRC + 1;
                strRCTip += $" + {_objCharacter.STR.DisplayAbbrev} [{intUseSTR}] /3 = {intStrRC}]";
                // If the full RC is not higher than the base, only the base value is shown.
                strRC = intRCBase.ToString();
                if (intRCFull > intRCBase)
                {
                    strRC += $" ({intRCFull})";
                }

                _strRCTip = strRCTip;

                return strRC;
            }
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
                int intReach = _intReach;

                if (_strType == "Melee")
                {
                    // Run through the Character's Improvements and add any Reach Improvements.
                    intReach += _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.Reach && objImprovement.Enabled).Sum(objImprovement => objImprovement.Value);
                }
                if (_strName == "Unarmed Attack")
                {
                    intReach += _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.UnarmedReach && objImprovement.Enabled).Sum(objImprovement => objImprovement.Value);
                }

                return intReach;
            }
        }

        /// <summary>
        /// The full Accuracy of the Weapon including modifiers from accessories.
        /// </summary>
        public int TotalAccuracy
        {
            get
            {
                StringBuilder objAccuracy = new StringBuilder(_strAccuracy);
                int intAccuracy = 0;

                int intUseSTR = 0;
                int intUseAGI = 0;
                int intUseSTRBase = 0;
                int intUseAGIBase = 0;
                if (_strAccuracy.Contains("{STR") || _strAccuracy.Contains("{AGI"))
                {
                    if (Cyberware)
                    {
                        if (ParentVehicle != null)
                        {
                            intUseSTR = ParentVehicle.TotalBody;
                            intUseSTRBase = intUseSTR;
                            intUseAGI = ParentVehicle.Pilot;
                            intUseAGIBase = intUseAGI;
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(ParentID, out VehicleMod objVehicleMod);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                int intSTR = objAttributeSource.TotalStrength;
                                int intSTRBase = objAttributeSource.BaseStrength;
                                int intAGI = objAttributeSource.TotalAgility;
                                int intAGIBase = objAttributeSource.BaseAgility;
                                while (objAttributeSource != null)
                                {
                                    if (intSTR != 0 || intAGI != 0 || intSTRBase != 0 || intAGIBase != 0)
                                        break;
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null) continue;
                                    intSTR = objAttributeSource.TotalStrength;
                                    intSTRBase = objAttributeSource.BaseStrength;
                                    intAGI = objAttributeSource.TotalAgility;
                                    intAGIBase = objAttributeSource.BaseAgility;
                                }
                                intUseSTR = intSTR;
                                intUseAGI = intAGI;
                                intUseSTRBase = intSTRBase;
                                intUseAGIBase = intAGIBase;

                                if (intUseSTR == 0)
                                    intUseSTR = objVehicleMod.TotalStrength;
                                if (intUseAGI == 0)
                                    intUseAGI = objVehicleMod.TotalAgility;
                                if (intUseSTRBase == 0)
                                    intUseSTRBase = ParentVehicle.TotalBody;
                                if (intUseAGIBase == 0)
                                    intUseAGIBase = ParentVehicle.Pilot;
                            }
                        }
                        else
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                int intSTR = objAttributeSource.TotalStrength;
                                int intSTRBase = objAttributeSource.BaseStrength;
                                int intAGI = objAttributeSource.TotalAgility;
                                int intAGIBase = objAttributeSource.BaseAgility;
                                while (objAttributeSource != null)
                                {
                                    if (intSTR != 0 || intAGI != 0 || intSTRBase != 0 || intAGIBase != 0)
                                        break;
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null) continue;
                                    intSTR = objAttributeSource.TotalStrength;
                                    intSTRBase = objAttributeSource.BaseStrength;
                                    intAGI = objAttributeSource.TotalAgility;
                                    intAGIBase = objAttributeSource.BaseAgility;
                                }

                                intUseSTR = intSTR;
                                intUseAGI = intAGI;
                                intUseSTRBase = intSTRBase;
                                intUseAGIBase = intAGIBase;
                            }
                            if (intUseSTR == 0)
                                intUseSTR = _objCharacter.STR.TotalValue;
                            if (intUseAGI == 0)
                                intUseAGI = _objCharacter.AGI.TotalValue;
                            if (intUseSTRBase == 0)
                                intUseSTRBase = _objCharacter.STR.TotalBase;
                            if (intUseAGIBase == 0)
                                intUseAGIBase = _objCharacter.AGI.TotalBase;
                        }
                    }
                    else if (ParentVehicle == null)
                    {
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                        if (intUseAGI == 0)
                            intUseAGI = _objCharacter.AGI.TotalValue;
                    }

                    if (_strCategory == "Throwing Weapons" || _strUseSkill == "Throwing Weapons")
                        intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);
                }

                Func<string> funcPhysicalLimitString = () => _objCharacter.LimitPhysical.ToString();
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
                objAccuracy.CheapReplace(_strAccuracy, "Physical", funcPhysicalLimitString);
                objAccuracy.CheapReplace(_strAccuracy, "Missile", funcPhysicalLimitString);
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    CharacterAttrib objLoopAttribute = _objCharacter.GetAttribute(strAttribute);
                    if (strAttribute == "STR")
                    {
                        objAccuracy.Replace("{" + strAttribute + "}", intUseSTR.ToString());
                        objAccuracy.Replace("{" + strAttribute + "Base}", intUseSTRBase.ToString());
                    }
                    else if (strAttribute == "AGI")
                    {
                        objAccuracy.Replace("{" + strAttribute + "}", intUseAGI.ToString());
                        objAccuracy.Replace("{" + strAttribute + "Base}", intUseAGIBase.ToString());
                    }
                    else
                    {
                        objAccuracy.CheapReplace(_strAccuracy, "{" + strAttribute + "}", () => objLoopAttribute.TotalValue.ToString());
                        objAccuracy.CheapReplace(_strAccuracy, "{" + strAttribute + "Base}", () => objLoopAttribute.TotalBase.ToString());
                    }
                }
                
                try
                {
                    intAccuracy = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(objAccuracy.ToString()));
                }
                catch (FormatException)
                {
                }
                catch (OverflowException)
                {
                }
                catch (XPathException)
                {
                }

                foreach (WeaponAccessory wa in _lstAccessories)
                {
                    if (wa.Name == "Laser Sight" || wa.Name == "Holographic Sight")
                    {
                        // Skip it if there is a smartgun on this weapon
                        bool blnFound = false;
                        foreach (WeaponAccessory wal in _lstAccessories)
                        {
                            if (wal.Name.StartsWith("Smartgun"))
                                blnFound = true;
                        }
                        if (!blnFound)
                            intAccuracy += wa.Accuracy;
                    }
                    else
                        intAccuracy += wa.Accuracy;
                }
                string s = Name.ToLower();
                intAccuracy += _objCharacter.Improvements
                    .Where(i => i.ImproveType == Improvement.ImprovementType.WeaponAccuracy && (string.IsNullOrEmpty(i.ImprovedName) || i.ImprovedName == Name || i.ImprovedName.Contains("[contains]") && s.Contains(i.ImprovedName.Replace("[contains]",string.Empty).ToLower())))
                    .Sum(objImprovement => objImprovement.Value);

                // Look for Powers that increase accuracy
                foreach (Power objPower in _objCharacter.Powers)
                {
                    if (objPower.Name.StartsWith("Enhanced Accuracy (skill)"))
                    {
                        string strPowerSkill = objPower.Extra;

                        string strSkill = string.Empty;
                        string strSpec = string.Empty;
                        // Exotic Skills require a matching Specialization.
                        switch (_strCategory)
                        {
                            case "Bows":
                            case "Crossbows":
                                strSkill = "Archery";
                                break;
                            case "Assault Rifles":
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
                                strSpec = Name;
                                break;
                            case "Exotic Ranged Weapons":
                            case "Special Weapons":
                                strSkill = "Exotic Ranged Weapon";
                                strSpec = Name;
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

                        if (strSkill.StartsWith("Exotic"))
                            strSkill += $" ({Name})";

                        // Use the Skill defined by the Weapon if one is present.
                        if (!string.IsNullOrEmpty(_strUseSkill))
                        {
                            strSkill = _strUseSkill;

                            if (strSkill.StartsWith("Exotic"))
                                strSkill += $"({Name})";
                        }

                        if (strPowerSkill == strSkill)
                            if (!string.IsNullOrEmpty(strSpec))
                            {
                                if (_objCharacter.SkillsSection.Skills.Any(objSkill => objSkill.Name.StartsWith("Exotic") && objSkill.DisplaySpecializationMethod(GlobalOptions.DefaultLanguage) == strSpec))
                                {
                                    intAccuracy += 1;
                                }
                            }
                            else
                            {
                                intAccuracy += 1;
                            }
                    }
                }

                return intAccuracy;
            }
        }

        /// <summary>
        /// The slots the weapon has for modifications.
        /// </summary>
        public string ModificationSlots => _strWeaponSlots;

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

        /// <summary>
        /// The string for the Weapon's Range category
        /// </summary>
        public string DisplayRange(string strLanguage)
        {
            string strRange = Range;
            if (string.IsNullOrWhiteSpace(strRange))
                strRange = Category;
            if (!string.IsNullOrWhiteSpace(strRange) && strLanguage != GlobalOptions.DefaultLanguage)
            {
                XmlDocument objXmlDocument = XmlManager.Load("ranges.xml", strLanguage);
                XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[name = \"" + strRange + "\"]");
                XmlNode xmlTranslateNode = objXmlCategoryNode?["translate"];
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.InnerText;
                }
                else
                {
                    objXmlDocument = XmlManager.Load("weapons.xml", strLanguage);
                    objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strRange + "\"]");
                    xmlTranslateNode = objXmlCategoryNode?.Attributes?["translate"];
                    if (xmlTranslateNode != null)
                        strRange = xmlTranslateNode.InnerText;
                }
            }
            return strRange;
        }

        /// <summary>
        /// The string for the Weapon's Range category (setter is English-only).
        /// </summary>
        public string DisplayAlternateRange(string strLanguage)
        {
            string strRange = AlternateRange.Trim();
            if (!string.IsNullOrEmpty(strRange) && strLanguage != GlobalOptions.DefaultLanguage)
            {
                XmlDocument objXmlDocument = XmlManager.Load("ranges.xml", strLanguage);
                XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[name = \"" + strRange + "\"]");
                XmlNode xmlTranslateNode = objXmlCategoryNode?["translate"];
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.InnerText;
                }
                else
                {
                    objXmlDocument = XmlManager.Load("weapons.xml", strLanguage);
                    objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strRange + "\"]");
                    xmlTranslateNode = objXmlCategoryNode?.Attributes?["translate"];
                    if (xmlTranslateNode != null)
                        strRange = xmlTranslateNode.InnerText;
                }
            }
            return strRange;
        }

        /// <summary>
        /// Evalulate and return the requested Range for the Weapon.
        /// </summary>
        /// <param name="strFindRange">Range node to use.</param>
        private int GetRange(string strFindRange, bool blnUseAlternateRange)
        {
            string strRangeCategory = _strCategory;
            if (blnUseAlternateRange)
            {
                strRangeCategory = _strAlternateRange;
                if (string.IsNullOrWhiteSpace(strRangeCategory))
                    return -1;
            }
            else if (!string.IsNullOrEmpty(_strRange))
                strRangeCategory = _strRange;


            XmlDocument objXmlDocument = XmlManager.Load("ranges.xml");
            XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[name = \"" + strRangeCategory + "\"]");
            if (objXmlCategoryNode?[strFindRange] == null)
            {
                return -1;
            }
            string strRange = objXmlCategoryNode[strFindRange].InnerText;
            StringBuilder objRange = new StringBuilder(strRange);

            int intUseSTR = 0;
            int intUseAGI = 0;
            if (strRange.Contains("STR") || strRange.Contains("AGI"))
            {
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        intUseAGI = ParentVehicle.Pilot;
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(ParentID, out VehicleMod objVehicleMod);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intAGI = objAttributeSource.TotalStrength;
                            }
                            intUseSTR = intSTR;
                            intUseAGI = intAGI;

                            if (intUseSTR == 0)
                                intUseSTR = objVehicleMod.TotalStrength;
                            if (intUseAGI == 0)
                                intUseAGI = objVehicleMod.TotalAgility;
                        }
                    }
                    else
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intAGI = objAttributeSource.TotalStrength;
                            }

                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                        if (intUseAGI == 0)
                            intUseAGI = _objCharacter.AGI.TotalValue;
                    }
                }
                else if (ParentVehicle == null)
                {
                    if (intUseSTR == 0)
                        intUseSTR = _objCharacter.STR.TotalValue;
                    if (intUseAGI == 0)
                        intUseAGI = _objCharacter.AGI.TotalValue;
                }

                if (_strCategory == "Throwing Weapons" || _strUseSkill == "Throwing Weapons")
                    intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);
            }

            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                CharacterAttrib objLoopAttribute = _objCharacter.GetAttribute(strAttribute);
                if (strAttribute == "STR")
                {
                    objRange.Replace("STR", intUseSTR.ToString());
                }
                else if (strAttribute == "AGI")
                {
                    objRange.Replace("AGI", intUseAGI.ToString());
                }
                else
                {
                    objRange.CheapReplace(_strAccuracy, strAttribute, () => objLoopAttribute.TotalValue.ToString());
                }
            }
            
            // Replace the division sign with "div" since we're using XPath.
            objRange.Replace("/", " div ");

            decimal decReturn = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(objRange.ToString()), GlobalOptions.InvariantCultureInfo) * _decRangeMultiplier;
            int intReturn = decimal.ToInt32(decimal.Ceiling(decReturn));

            return intReturn;
        }

        /// <summary>
        /// Weapon's total Range bonus from Accessories.
        /// </summary>
        public int RangeBonus
        {
            get
            {
                int intRangeBonus = 0;

                // Weapon Mods.
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                    intRangeBonus += objAccessory.RangeBonus;

                // Check if the Weapon has Ammunition loaded and look for any Range bonus.
                if (!string.IsNullOrEmpty(AmmoLoaded))
                {
                    Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded);
                    if (objGear == null)
                    {
                        objGear = _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);
                    }

                    if (objGear != null)
                    {
                        if (objGear.WeaponBonus != null)
                        {
                            intRangeBonus += objGear.WeaponBonusRange;
                        }
                    }
                }

                return intRangeBonus;
            }
        }

        /// <summary>
        /// Dictionary where keys are range categories (short, medium, long, extreme, alternateshort, etc.), values are strings depicting range values for the category.
        /// </summary>
        public IDictionary<string, string> GetRangeStrings(CultureInfo objCulture)
        {
            int intRangeModifier = RangeBonus + 100;
            int intMin = GetRange("min", false);
            int intShort = GetRange("short", false);
            int intMedium = GetRange("medium", false);
            int intLong = GetRange("long", false);
            int intExtreme = GetRange("extreme", false);
            int intAlternateMin = GetRange("min", true);
            int intAlternateShort = GetRange("short", true);
            int intAlternateMedium = GetRange("medium", true);
            int intAlternateLong = GetRange("long", true);
            int intAlternateExtreme = GetRange("extreme", true);
            if (intMin > 0)
                intMin = (intMin * (intRangeModifier) + 99) / 100;
            if (intShort > 0)
                intShort = (intShort * (intRangeModifier) + 99) / 100;
            if (intMedium > 0)
                intMedium = (intMedium * (intRangeModifier) + 99) / 100;
            if (intLong > 0)
                intLong = (intLong * (intRangeModifier) + 99) / 100;
            if (intExtreme > 0)
                intExtreme = (intExtreme * (intRangeModifier) + 99) / 100;
            if (intAlternateMin > 0)
                intAlternateMin = (intAlternateMin * (intRangeModifier) + 99) / 100;
            if (intAlternateShort > 0)
                intAlternateShort = (intAlternateShort * (intRangeModifier) + 99) / 100;
            if (intAlternateMedium > 0)
                intAlternateMedium = (intAlternateMedium * (intRangeModifier) + 99) / 100;
            if (intAlternateLong > 0)
                intAlternateLong = (intAlternateLong * (intRangeModifier) + 99) / 100;
            if (intAlternateExtreme > 0)
                intAlternateExtreme = (intAlternateExtreme * (intRangeModifier) + 99) / 100;

            Dictionary<string, string> retDictionary = new Dictionary<string, string>(8)
                {
                    { "short", (intMin < 0 || intShort < 0) ? string.Empty : (intMin).ToString(objCulture) + '-' + intShort.ToString(objCulture) },
                    { "medium", (intShort < 0 || intMedium < 0) ? string.Empty : (intShort + 1).ToString(objCulture) + '-' + intMedium.ToString(objCulture) },
                    { "long", (intMedium < 0 || intLong < 0) ? string.Empty : (intMedium + 1).ToString(objCulture) + '-' + intLong.ToString(objCulture) },
                    { "extreme", (intLong < 0 || intExtreme < 0) ? string.Empty : (intLong + 1).ToString(objCulture) + '-' + intExtreme.ToString(objCulture) },
                    { "alternateshort", (intAlternateMin < 0 || intAlternateShort < 0) ? string.Empty : (intAlternateMin).ToString(objCulture) + '-' + intAlternateShort.ToString(objCulture) },
                    { "alternatemedium", (intAlternateShort < 0 || intAlternateMedium < 0) ? string.Empty : (intAlternateShort + 1).ToString(objCulture) + '-' + intAlternateMedium.ToString(objCulture) },
                    { "alternatelong", (intAlternateMedium < 0 || intAlternateLong < 0) ? string.Empty : (intAlternateMedium + 1).ToString(objCulture) + '-' + intAlternateLong.ToString(objCulture) },
                    { "alternateextreme", (intAlternateLong < 0 || intAlternateExtreme < 0) ? string.Empty : (intAlternateLong + 1).ToString(objCulture) + '-' + intAlternateExtreme.ToString(objCulture) }
                };

            return retDictionary;
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
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                {
                    if (objAccessory.FullBurst > intReturn)
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
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                {
                    if (objAccessory.Suppressive > intReturn)
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
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                {
                    if (objAccessory.AccessoryCostMultiplier != 1)
                        intReturn += objAccessory.AccessoryCostMultiplier;
                }

                if (intReturn == 0)
                    intReturn = 1;

                return intReturn;
            }
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to use the Weapon.
        /// </summary>
        public string GetDicePool(CultureInfo objCulture)
        {
            string strExtra = string.Empty;
            int intDicePool = 0;
            int intDicePoolModifier = _lstAccessories.Where(a => a.Installed).Sum(a => a.DicePool);
            switch (FireMode)
            {
                //TODO: Gunnery specialisations (Dear god why is Ballistic a specialisation)
                case FiringMode.DogBrain:
                    {
                        Gear objAutosoft = ParentVehicle.Gear.DeepFirstOrDefault(x => x.Children, x => x.Name == "[Weapon] Targeting Autosoft" && (x.Extra == Name || x.Extra == DisplayName(GlobalOptions.Language)));

                        if (objAutosoft != null)
                        {
                            intDicePool = objAutosoft.Rating + ParentVehicle.Pilot;
                        }

                        foreach (Gear objGear in ParentVehicle.Gear)
                        {
                            if (objGear.InternalId == AmmoLoaded)
                            {
                                string strWeaponBonusPool = objGear.WeaponBonus?["pool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    intDicePoolModifier += Convert.ToInt32(strWeaponBonusPool);
                            }
                        }
                        break;
                    }
                case FiringMode.GunneryCommandDevice:
                    {
                        intDicePool = _objCharacter.SkillsSection.GetActiveSkill("Gunnery").PoolOtherAttribute(_objCharacter.LOG.TotalValue);
                        foreach (Gear objGear in ParentVehicle.Gear)
                        {
                            if (objGear.InternalId == AmmoLoaded)
                            {
                                string strWeaponBonusPool = objGear.WeaponBonus?["pool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    intDicePoolModifier += Convert.ToInt32(strWeaponBonusPool);
                            }
                        }
                        intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice, false, Category);
                        break;
                    }
                case FiringMode.RemoteOperated:
                    {
                        intDicePool = _objCharacter.SkillsSection.GetActiveSkill("Gunnery").PoolOtherAttribute(_objCharacter.LOG.TotalValue);
                        foreach (Gear objGear in ParentVehicle.Gear)
                        {
                            if (objGear.InternalId == AmmoLoaded)
                            {
                                string strWeaponBonusPool = objGear.WeaponBonus?["pool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    intDicePoolModifier += Convert.ToInt32(strWeaponBonusPool);
                            }
                        }
                        intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice, false, Category);
                        break;
                    }
                case FiringMode.ManualOperation:
                    {
                        intDicePool = _objCharacter.SkillsSection.GetActiveSkill("Gunnery").Pool;
                        foreach (Gear objGear in ParentVehicle.Gear)
                        {
                            if (objGear.InternalId == AmmoLoaded)
                            {
                                string strWeaponBonusPool = objGear.WeaponBonus?["pool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    intDicePoolModifier += Convert.ToInt32(strWeaponBonusPool);
                            }
                        }
                        intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice, false, Category);
                        break;
                    }
                case FiringMode.Skill:
                    {
                        Skill objSkill = Skill;
                        if (objSkill != null)
                        {
                            intDicePool = objSkill.Pool;
                            // If the character has a Specialization, include it in the Dice Pool string.
                            if (objSkill.Specializations.Count > 0 && !objSkill.IsExoticSkill)
                            {
                                if (objSkill.HasSpecialization(DisplayNameShort(GlobalOptions.Language)) || objSkill.HasSpecialization(Name) || objSkill.HasSpecialization(DisplayCategory(GlobalOptions.DefaultLanguage)) || objSkill.HasSpecialization(Category) || (!string.IsNullOrEmpty(objSkill.Specialization) && (objSkill.HasSpecialization(_strSpec) || objSkill.HasSpecialization(_strSpec2))))
                                    strExtra = " (" + (intDicePool + intDicePoolModifier + 2).ToString(objCulture) + ')';
                            }
                        }
                        foreach (Gear objGear in _objCharacter.Gear)
                        {
                            if (objGear.InternalId == AmmoLoaded)
                            {
                                string strWeaponBonusPool = objGear.WeaponBonus?["pool"]?.InnerText;
                                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                                    intDicePoolModifier += Convert.ToInt32(strWeaponBonusPool);
                            }
                        }
                        intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice, false, Category);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (intDicePool + intDicePoolModifier).ToString(objCulture) + strExtra;
        }

        private Skill Skill
        {
            get
            {
                string strCategory = _strCategory;
                string strSkill = string.Empty;
                string strSpec = string.Empty;
                string strReturn = string.Empty;

                // If this is a Special Weapon, use the Range to determine the required Active Skill (if present).
                if (strCategory == "Special Weapons" && !string.IsNullOrEmpty(_strRange))
                    strCategory = _strRange;

                // Exotic Skills require a matching Specialization.
                strSkill = GetSkillName(strCategory, ref strSpec);

                // Use the Skill defined by the Weapon if one is present.
                if (!string.IsNullOrEmpty(_strUseSkill))
                {
                    strSkill = _strUseSkill;
                    strSpec = string.Empty;

                    if (_strUseSkill.Contains("Exotic"))
                        strSpec = Name;
                }

                // Locate the Active Skill to be used.
                Skill objSkill = null;
                foreach (Skill objCharacterSkill in _objCharacter.SkillsSection.Skills)
                {
                    if (objCharacterSkill.Name == strSkill)
                    {
                        if (string.IsNullOrEmpty(strSpec) || (objCharacterSkill.HasSpecialization(strSpec)))
                        {
                            objSkill = objCharacterSkill;
                            break;
                        }
                        if (string.IsNullOrEmpty(_strSpec2) || objCharacterSkill.HasSpecialization(_strSpec2))
                        {
                            objSkill = objCharacterSkill;
                            break;
                        }
                    }
                }
                return objSkill;
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
                    strSpec = Name;
                    break;
                case "Exotic Ranged Weapons":
                case "Special Weapons":
                    strSkill = "Exotic Ranged Weapon";
                    strSpec = Name;
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
                string strCategory = _strCategory;
                string strSkill = string.Empty;
                string strSpec = string.Empty;
                string strReturn = string.Empty;

                // If this is a Special Weapon, use the Range to determine the required Active Skill (if present).
                if (strCategory == "Special Weapons" && !string.IsNullOrEmpty(_strRange))
                    strCategory = _strRange;

                // Exotic Skills require a matching Specialization.
                switch (strCategory)
                {
                    case "Bows":
                    case "Crossbows":
                        strSkill = "Archery";
                        break;
                    case "Assault Rifles":
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
                        strSpec = Name;
                        break;
                    case "Exotic Ranged Weapons":
                    case "Special Weapons":
                        strSkill = "Exotic Ranged Weapon";
                        strSpec = Name;
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

                // Use the Skill defined by the Weapon if one is present.
                if (!string.IsNullOrEmpty(_strUseSkill))
                    strSkill = _strUseSkill;

                // Locate the Active Skill to be used.
                string strKey = strSkill;
                if (!string.IsNullOrEmpty(strSpec))
                    strKey += " (" + strSpec + ')';
                Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(strKey);
                int intDicePool = 0;
                if (objSkill != null)
                {
                    intDicePool = objSkill.Pool;
                }

                strReturn = strSkill + " (" + intDicePool.ToString() + ')';

                if (objSkill != null && (!string.IsNullOrEmpty(objSkill.Specialization) && !objSkill.IsExoticSkill))
                {
                    if (objSkill.HasSpecialization(DisplayNameShort(GlobalOptions.Language)) || objSkill.HasSpecialization(Name) || objSkill.HasSpecialization(DisplayCategory(GlobalOptions.DefaultLanguage)) || objSkill.HasSpecialization(_strCategory) || (!string.IsNullOrEmpty(objSkill.Specialization) && (objSkill.HasSpecialization(_strSpec) || objSkill.HasSpecialization(_strSpec2))))
                        strReturn += " + " + LanguageManager.GetString("String_ExpenseSpecialization", GlobalOptions.Language) + " (2)";
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(string strLanguage)
        {
            Tuple<int, string> objAvailPair = TotalAvailPair;
            string strAvail = objAvailPair.Item2;
            // Translate the Avail string.
            if (strAvail == "F")
                strAvail = LanguageManager.GetString("String_AvailForbidden", strLanguage);
            else if (strAvail == "R")
                strAvail = LanguageManager.GetString("String_AvailRestricted", strLanguage);

            return objAvailPair.Item1.ToString() + strAvail;
        }

        /// <summary>
        /// Total Availability as a pair: first item is availability magnitude, second is untranslated Restricted/Forbidden state (empty of neither).
        /// </summary>
        public Tuple<int, string> TotalAvailPair
        {
            get
            {
                if (_strAvail.Length == 0)
                    return new Tuple<int, string>(0, string.Empty);
                string strAvail = string.Empty;
                string strAvailExpr = _strAvail;
                int intAvail = 0;

                if (strAvailExpr.Substring(_strAvail.Length - 1, 1) == "F" || strAvailExpr.Substring(_strAvail.Length - 1, 1) == "R")
                {
                    strAvail = strAvailExpr.Substring(_strAvail.Length - 1, 1);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, _strAvail.Length - 1);
                }
                if (strAvailExpr.Contains("{Children Avail}"))
                {
                    int intMaxChildAvail = 0;
                    foreach (Weapon objUnderbarrel in UnderbarrelWeapons)
                    {
                        Tuple<int, string> objLoopAvail = objUnderbarrel.TotalAvailPair;
                        if (objLoopAvail.Item1 > intMaxChildAvail)
                            intMaxChildAvail = objLoopAvail.Item1;
                        if (objLoopAvail.Item2.EndsWith('F'))
                            strAvail = "F";
                        else if (strAvail != "F" && objLoopAvail.Item2.EndsWith('R'))
                            strAvail = "R";
                    }
                    strAvailExpr = strAvailExpr.Replace("{Children Avail}", intMaxChildAvail.ToString());
                }
                try
                {
                    intAvail = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpr));
                }
                catch (XPathException)
                {
                }

                // Run through the Accessories and add in their availability.
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                {
                    string strAccAvail = objAccessory.Avail;
                    int intAccAvail = 0;

                    if (!objAccessory.IncludedInWeapon)
                    {
                        if (strAccAvail.StartsWith('+', '-'))
                        {
                            strAccAvail = objAccessory.TotalAvail(GlobalOptions.DefaultLanguage);
                            if (strAccAvail.EndsWith(LanguageManager.GetString("String_AvailForbidden", GlobalOptions.DefaultLanguage)))
                            {
                                strAvail = "F";
                                strAccAvail = strAccAvail.Substring(0, strAccAvail.Length - 1);
                            }
                            else if (strAccAvail.EndsWith(LanguageManager.GetString("String_AvailRestricted", GlobalOptions.DefaultLanguage)))
                            {
                                if (string.IsNullOrEmpty(strAvail))
                                    strAvail = "R";
                                strAccAvail = strAccAvail.Substring(0, strAccAvail.Length - 1);
                            }
                                    
                            intAccAvail = Convert.ToInt32(strAccAvail);
                            intAvail += intAccAvail;
                        }
                    }
                }
                return new Tuple<int,string>(intAvail, strAvail);
            }
        }

        // Run through the Weapon Mods and see if anything changes the cost multiplier (Vintage mod).
        public int CostMultiplier
        {
            get
            {
                int intReturn = 1;
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                {
                    if (objAccessory.AccessoryCostMultiplier > 1)
                        intReturn = objAccessory.AccessoryCostMultiplier;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Mount slot that is used when mounting this weapon to another weapon.
        /// </summary>
        public string Mount
        {
            get { return _mount; }
            private set { _mount = value; }
        }
        /// <summary>
        /// Additional Mount slot that is used when mounting this weapon to another weapon.
        /// </summary>
        public string ExtraMount
        {
            get { return _extraMount; }
            private set { _extraMount = value; }
        }

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
                if (objThis != null)
                    return objThis.IsProgram;
                return false;
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
                if (objThis != null)
                    return objThis.DeviceRating;
                return _strDeviceRating;
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
                if (objThis != null)
                    return objThis.Attack;
                return _strAttack;
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
                if (objThis != null)
                    return objThis.Sleaze;
                return _strSleaze;
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
                if (objThis != null)
                    return objThis.DataProcessing;
                return _strDataProcessing;
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
                if (objThis != null)
                    return objThis.Firewall;
                return _strFirewall;
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
                if (objThis != null)
                    return objThis.ModAttack;
                return _strModAttack;
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
                if (objThis != null)
                    return objThis.ModSleaze;
                return _strModSleaze;
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
                if (objThis != null)
                    return objThis.ModDataProcessing;
                return _strModDataProcessing;
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
                if (objThis != null)
                    return objThis.ModFirewall;
                return _strModFirewall;
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
                if (objThis != null)
                    return objThis.AttributeArray;
                return _strAttributeArray;
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
                if (objThis != null)
                    return objThis.ModAttributeArray;
                return _strModAttributeArray;
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
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.Overclocked;
                return _strOverclocked;
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

        /// <summary>
        /// Empty for Weapons.
        /// </summary>
        public string CanFormPersona
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.CanFormPersona;
                return string.Empty;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.CanFormPersona = value;
            }
        }

        public bool IsCommlink
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.IsCommlink;
                return false;
            }
        }

        /// <summary>
        /// 0 for Cyberware.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.BonusMatrixBoxes;
                return 0;
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
                if (objThis != null)
                    return objThis.TotalBonusMatrixBoxes;
                return 0;
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
                if (objThis != null)
                    return objThis.ProgramLimit;
                return _strProgramLimit;
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
                if (objThis != null)
                    return objThis.CanSwapAttributes;
                return _blnCanSwapAttributes;
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

        public IList<IHasMatrixAttributes> ChildrenWithMatrixAttributes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.ChildrenWithMatrixAttributes;
                return Children.Cast<IHasMatrixAttributes>().ToList();
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
                if (objThis != null)
                    return objThis.BaseMatrixBoxes;
                int baseMatrixBoxes = 8;
                return baseMatrixBoxes;
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
                if (objThis != null)
                    return objThis.MatrixCMFilled;
                return _intMatrixCMFilled;
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

        #endregion

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
        #endregion

        #region Methods
        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        /// <param name="objWeapon">Weapon to delete.</param>
        /// <param name="treWeapons">TreeView that holds the list of Weapons.</param>
        /// <param name="treVehicles">TreeView that holds the list of Vehicles.</param>
        public decimal DeleteWeapon(TreeView treWeapons, TreeView treVehicles)
        {
            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Weapon objChild in Children)
                decReturn += objChild.DeleteWeapon(treWeapons, treVehicles);

            foreach (WeaponAccessory objLoopAccessory in WeaponAccessories)
            {
                foreach (Gear objLoopGear in objLoopAccessory.Gear)
                {
                    decReturn += objLoopGear.DeleteGear(treWeapons, treVehicles);
                }
            }

            List<string> lstNodesToRemoveIds = new List<string>();
            List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>>();
            foreach (Weapon objDeleteWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
            {
                lstNodesToRemoveIds.Add(objDeleteWeapon.InternalId);
                lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objDeleteWeapon, null, null, null));
            }
            foreach (Vehicle objVehicle in _objCharacter.Vehicles)
            {
                foreach (Weapon objDeleteWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                {
                    lstNodesToRemoveIds.Add(objDeleteWeapon.InternalId);
                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objDeleteWeapon, objVehicle, null, null));
                }

                foreach (VehicleMod objMod in objVehicle.Mods)
                {
                    foreach (Weapon objDeleteWeapon in objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                    {
                        lstNodesToRemoveIds.Add(objDeleteWeapon.InternalId);
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objDeleteWeapon, objVehicle, objMod, null));
                    }
                }

                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                {
                    foreach (Weapon objDeleteWeapon in objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                    {
                        lstNodesToRemoveIds.Add(objDeleteWeapon.InternalId);
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objDeleteWeapon, objVehicle, null, objMount));
                    }
                }
            }
            // We need this list separate because weapons to remove can contain gear that add more weapons in need of removing
            foreach (Tuple<Weapon, Vehicle, VehicleMod, WeaponMount> objLoopTuple in lstWeaponsToDelete)
            {
                Weapon objDeleteWeapon = objLoopTuple.Item1;
                decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon(treWeapons, treVehicles);
                if (objDeleteWeapon.Parent != null)
                    objDeleteWeapon.Parent.Children.Remove(objDeleteWeapon);
                else if (objLoopTuple.Item4 != null)
                    objLoopTuple.Item4.Weapons.Remove(objDeleteWeapon);
                else if (objLoopTuple.Item3 != null)
                    objLoopTuple.Item3.Weapons.Remove(objDeleteWeapon);
                else if (objLoopTuple.Item2 != null)
                    objLoopTuple.Item2.Weapons.Remove(objDeleteWeapon);
                else
                    _objCharacter.Weapons.Remove(objDeleteWeapon);
            }
            foreach (string strNodeId in lstNodesToRemoveIds)
            {
                // Remove the Weapons from the TreeView.
                TreeNode objLoopNode = treWeapons?.FindNode(strNodeId) ?? treVehicles?.FindNode(strNodeId);
                objLoopNode?.Remove();
            }

            return decReturn;
        }

        /// <summary>
        /// Add a Weapon to the TreeView.
        /// </summary>
        /// <param name="cmsWeapon">ContextMenuStrip for the Weapon Node.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip for Vehicle Accessory Nodes.</param>
        /// <param name="cmsWeaponAccessoryGear">ContextMenuStrip for Vehicle Weapon Accessory Gear Nodes.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = InternalId,
                ContextMenuStrip = cmsWeapon
            };
            if (!string.IsNullOrEmpty(Notes))
                objNode.ForeColor = Color.SaddleBrown;
            else if (Cyberware || Category == "Gear" || Category.StartsWith("Quality") || !string.IsNullOrEmpty(ParentID))
                objNode.ForeColor = SystemColors.GrayText;

            objNode.ToolTipText = Notes.WordWrap(100);

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // Add Underbarrel Weapons.
            foreach (Weapon objUnderbarrelWeapon in UnderbarrelWeapons)
            {
                lstChildNodes.Add(objUnderbarrelWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear));
            }
            // Add attached Weapon Accessories.
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                lstChildNodes.Add(objAccessory.CreateTreeNode(cmsWeaponAccessory, cmsWeaponAccessoryGear));
            }

            if (lstChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }
        #endregion

        private Clip GetClip(int clip)
        {
            //1 indexed due legacy
            clip--;

            for (int i = _ammo.Count; i <= clip; i++)
            {
                _ammo.Add(new Clip(Guid.Empty, 0));
            }


            return _ammo[clip];
        }

        IHasMatrixAttributes GetMatrixAttributesOverride
        {
            get
            {
                IHasMatrixAttributes objReturn = null;
                if (!string.IsNullOrEmpty(ParentID))
                {
                    objReturn = _objCharacter.Gear.DeepFindById(ParentID) ??
                        _objCharacter.Vehicles.FindVehicleGear(ParentID) ??
                        _objCharacter.Weapons.FindWeaponGear(ParentID) ??
                        _objCharacter.Armor.FindArmorGear(ParentID) ??
                        _objCharacter.Cyberware.FindCyberwareGear(ParentID);
                    if (objReturn == null)
                    {
                        objReturn = _objCharacter.Cyberware.DeepFindById(ParentID) ??
                            _objCharacter.Vehicles.FindVehicleCyberware(ParentID);
                        if (objReturn == null)
                        {
                            objReturn = _objCharacter.Weapons.DeepFindById(ParentID) ??
                                    _objCharacter.Vehicles.FindVehicleWeapon(ParentID);
                            if (objReturn == null)
                            {
                                objReturn = _objCharacter.Vehicles.FirstOrDefault(x => x.InternalId == ParentID);
                            }
                        }
                    }
                }
                return objReturn;
            }
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
                    case "Attack":
                    case "Sleaze":
                    default:
                        strExpression = "0";
                        break;
                }
            }
            
            if (strExpression.Contains('{') || strExpression.Contains('+') || strExpression.Contains('-') || strExpression.Contains('*') || strExpression.Contains("div"))
            {
                StringBuilder objValue = new StringBuilder(strExpression);
                foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + "}", () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0).ToString(GlobalOptions.InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + "}", () => (Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0"));
                    if (Children.Count > 0 && strExpression.Contains("{Children " + strMatrixAttribute + "}"))
                    {
                        int intTotalChildrenValue = 0;
                        foreach (Weapon objLoopWeapon in Children)
                        {
                            if (objLoopWeapon.Installed)
                            {
                                intTotalChildrenValue += objLoopWeapon.GetBaseMatrixAttribute(strMatrixAttribute);
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
            IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
            if (objThis != null)
                return objThis.GetBonusMatrixAttribute(strAttributeName);
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                intReturn += 1;
            }

            if (!strAttributeName.StartsWith("Mod "))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Weapon objLoopWeapon in Children)
            {
                if (objLoopWeapon.Installed && objLoopWeapon.ParentID != InternalId)
                {
                    intReturn += objLoopWeapon.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        private sealed class Clip
        {
            internal Guid Guid { get; set; }
            internal int Ammo { get; set; }
            public string AmmoName { get; internal set; }

            internal static Clip Load(XmlNode node)
            {
                if (node != null && node["id"] != null && node["count"] != null)
                {
                    try
                    {
                        return new Clip(Guid.Parse(node["id"].InnerText), int.Parse(node["count"].InnerText));
                    }
                    catch (FormatException) { }
                }
                return null;
            }

            internal void Save(XmlTextWriter writer)
            {
                if (Guid != Guid.Empty || Ammo != 0) //Don't save empty clips, we are recreating them anyway. Save those kb
                {
                    writer.WriteStartElement("clip");
                    writer.WriteElementString("name", AmmoName);
                    writer.WriteElementString("id", Guid.ToString("D"));
                    writer.WriteElementString("count", Ammo.ToString());
                    writer.WriteEndElement();
                }
            }

            internal Clip(Guid guid, int ammo)
            {
                Guid = guid;
                Ammo = ammo;
            }
        }
    }
}
