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
    public class Weapon : IHasChildren<Weapon>, INamedItem, IItemWithGuid, IItemWithNode
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
        private decimal _decCost = 0;
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
        private string _strNotes = string.Empty;
        private string _strAltName = string.Empty;
        private string _strAltCategory = string.Empty;
        private string _strAltPage = string.Empty;
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
        /// <param name="lstNodes">List of TreeNodes to populate a TreeView.</param>
        /// <param name="cmsWeapon">ContextMenuStrip to use for Weapons.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip to use for Accessories.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        public void Create(XmlNode objXmlWeapon, IList<TreeNode> lstNodes, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, IList<Weapon> objWeapons, ContextMenuStrip cmsWeaponAccessoryGear = null, bool blnCreateChildren = true, bool blnCreateImprovements = true)
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
            objXmlWeapon.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlWeapon.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            objXmlWeapon.TryGetStringFieldQuickly("rc", ref _strRC);
            objXmlWeapon.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objXmlWeapon.TryGetStringFieldQuickly("avail", ref _strAvail);
            string strCostElement = objXmlWeapon["cost"]?.InnerText;
            if (!string.IsNullOrEmpty(strCostElement))
            {
                // Check for a Variable Cost.
                if (strCostElement.StartsWith("Variable"))
                {
                    decimal decMin = 0;
                    decimal decMax = decimal.MaxValue;
                    string strCost = strCostElement.TrimStart("Variable", true).Trim("()".ToCharArray());
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
                        frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _decCost = frmPickNumber.SelectedValue;
                    }
                }
                else
                {
                    decimal.TryParse(strCostElement, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out _decCost);
                }
            }

            if (objXmlWeapon["cyberware"]?.InnerText == "yes")
                _blnCyberware = true;
            objXmlWeapon.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlWeapon.TryGetStringFieldQuickly("page", ref _strPage);

            XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");

            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
            {
                XmlNode objWeaponNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + _strName + "\"]");
                if (objWeaponNode != null)
                {
                    objWeaponNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objWeaponNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                objWeaponNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                _strAltCategory = objWeaponNode?.Attributes["translate"]?.InnerText;
            }

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

            TreeNode objNode = null;
            if (lstNodes != null)
            {
                objNode = new TreeNode
                {
                    Text = DisplayName,
                    Tag = _guiID.ToString()
                };
                lstNodes.Add(objNode);
            }

            // If the Weapon comes with an Underbarrel Weapon, add it.
            if (objXmlWeapon.InnerXml.Contains("<underbarrels>") && blnCreateChildren)
            {
                foreach (XmlNode objXmlUnderbarrel in objXmlWeapon["underbarrels"].ChildNodes)
                {
                    Weapon objUnderbarrelWeapon = new Weapon(_objCharacter);
                    List<TreeNode> lstUnderbarrelNodes = lstNodes == null ? null : new List<TreeNode>();
                    XmlNode objXmlWeaponNode =
                        objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlUnderbarrel.InnerText + "\"]");
                    objUnderbarrelWeapon.Create(objXmlWeaponNode, lstUnderbarrelNodes, cmsWeapon, cmsWeaponAccessory, objWeapons, cmsWeaponAccessoryGear, true, blnCreateImprovements);
                    if (!AllowAccessory)
                        objUnderbarrelWeapon.AllowAccessory = false;
                    objUnderbarrelWeapon.ParentID = InternalId;
                    objUnderbarrelWeapon.IncludedInWeapon = true;
                    objUnderbarrelWeapon.Parent = this;
                    _lstUnderbarrel.Add(objUnderbarrelWeapon);
                    if (lstNodes != null)
                    {
                        foreach (TreeNode objLoopNode in lstUnderbarrelNodes)
                        {
                            objLoopNode.ContextMenuStrip = cmsWeapon;
                            objLoopNode.ForeColor = SystemColors.GrayText;
                            objNode.Nodes.Add(objLoopNode);
                        }
                    }
                }
            }

            //#1544 Ammunition not loading or available.
            if (_strUseSkill == "Throwing Weapons"
                && _strAmmo != "1")
            {
                _strAmmo = "1";
            }

            // If there are any Accessories that come with the Weapon, add them.
            if (objXmlWeapon.InnerXml.Contains("<accessories>") && blnCreateChildren)
            {
                XmlNodeList objXmlAccessoryList = objXmlWeapon.SelectNodes("accessories/accessory");
                foreach (XmlNode objXmlWeaponAccessory in objXmlAccessoryList)
                {
                    XmlNode objXmlAccessory = objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + objXmlWeaponAccessory["name"].InnerText + "\"]");
                    TreeNode objAccessoryNode = new TreeNode();
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
                            objAccessory.Create(objXmlAccessory, objAccessoryNode, new Tuple<string, string>(objXmlAccessory["mount"].InnerText, objXmlAccessory["extramount"].InnerText), intAccessoryRating, cmsWeaponAccessoryGear, false, blnCreateChildren, blnCreateImprovements);
                        }
                        else
                        {
                            objAccessory.Create(objXmlAccessory, objAccessoryNode, new Tuple<string, string>(objXmlAccessory["mount"].InnerText, "None"), intAccessoryRating, cmsWeaponAccessoryGear, false, blnCreateChildren, blnCreateImprovements);
                        }
                    }
                    else
                    {
                        objAccessory.Create(objXmlAccessory, objAccessoryNode, new Tuple<string, string>("Internal", "None"), intAccessoryRating, cmsWeaponAccessoryGear, false, blnCreateChildren, blnCreateImprovements);
                    }
                    // Add any extra Gear that comes with the Weapon Accessory.
                    if (objXmlWeaponAccessory["gears"] != null)
                    {
                        XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                        foreach (XmlNode objXmlAccessoryGear in objXmlWeaponAccessory.SelectNodes("gears/usegear"))
                        {
                            XmlNode objXmlAccessoryGearName = objXmlAccessoryGear["name"];
                            XmlAttributeCollection objXmlAccessoryGearNameAttributes = objXmlAccessoryGearName.Attributes;
                            int intGearRating = 0;
                            decimal decGearQty = 1;
                            string strChildForceSource = string.Empty;
                            string strChildForcePage = string.Empty;
                            string strChildForceValue = string.Empty;
                            bool blnStartCollapsed = objXmlAccessoryGearNameAttributes?["startcollapsed"]?.InnerText == "yes";
                            bool blnChildCreateChildren = objXmlAccessoryGearNameAttributes?["createchildren"]?.InnerText != "no";
                            bool blnAddChildImprovements = true;
                            if (objXmlAccessoryGearNameAttributes?["addimprovements"]?.InnerText == "no")
                                blnAddChildImprovements = false;
                            if (objXmlAccessoryGear["rating"] != null)
                                intGearRating = Convert.ToInt32(objXmlAccessoryGear["rating"].InnerText);
                            if (objXmlAccessoryGearNameAttributes?["qty"] != null)
                                decGearQty = Convert.ToDecimal(objXmlAccessoryGearNameAttributes["qty"].InnerText, GlobalOptions.InvariantCultureInfo);
                            if (objXmlAccessoryGearNameAttributes?["select"] != null)
                                strChildForceValue = objXmlAccessoryGearNameAttributes["select"].InnerText;
                            if (objXmlAccessoryGear["source"] != null)
                                strChildForceSource = objXmlAccessoryGear["source"].InnerText;
                            if (objXmlAccessoryGear["page"] != null)
                                strChildForcePage = objXmlAccessoryGear["page"].InnerText;

                            XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlAccessoryGearName.InnerText + "\" and category = \"" + objXmlAccessoryGear["category"].InnerText + "\"]");
                            Gear objGear = new Gear(_objCharacter);

                            TreeNode objGearNode = new TreeNode();
                            List<Weapon> lstWeapons = new List<Weapon>();
                            List<TreeNode> lstWeaponNodes = new List<TreeNode>();

                            objGear.Create(objXmlGear, objGearNode, intGearRating, lstWeapons, lstWeaponNodes, strChildForceValue, false, false, blnAddChildImprovements, blnChildCreateChildren);

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
                                objGear.Capacity = "[" + objXmlAccessoryGear["capacity"].InnerText + "]";

                            objGearNode.ContextMenuStrip = cmsWeaponAccessoryGear;
                            objGearNode.ForeColor = SystemColors.GrayText;
                            objAccessoryNode.Nodes.Add(objGearNode);
                            if (!blnStartCollapsed)
                                objAccessoryNode.Expand();
                        }
                    }

                    objAccessory.IncludedInWeapon = true;
                    objAccessory.Parent = this;
                    objAccessoryNode.ContextMenuStrip = cmsWeaponAccessory;
                    _lstAccessories.Add(objAccessory);
                    objAccessoryNode.Text = objAccessory.DisplayName;
                    objAccessoryNode.ForeColor = SystemColors.GrayText;
                    if (objNode != null)
                    {
                        objNode.Nodes.Add(objAccessoryNode);
                        objNode.Expand();
                    }
                }
            }

            // Add Subweapons (not underbarrels) if applicable.
            if (objWeapons != null && objXmlWeapon.InnerXml.Contains("<addweapon>"))
            {
                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddWeapon in objXmlWeapon.SelectNodes("addweapon"))
                {
                    string strLoopID = objXmlAddWeapon.InnerText;
                    XmlNode objXmlSubWeapon = strLoopID.IsGuid()
                        ? objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                        : objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");

                    List<TreeNode> lstSubWeaponNodes = lstNodes == null ? null : new List<TreeNode>();
                    Weapon objSubWeapon = new Weapon(_objCharacter);
                    objSubWeapon.ParentVehicle = ParentVehicle;
                    objSubWeapon.Create(objXmlSubWeapon, lstSubWeaponNodes, cmsWeapon, cmsWeaponAccessory, objWeapons, cmsWeaponAccessoryGear, blnCreateChildren, blnCreateImprovements);
                    objSubWeapon.ParentID = InternalId;
                    if (lstNodes != null)
                    {
                        foreach (TreeNode objLoopNode in lstSubWeaponNodes)
                        {
                            objLoopNode.ForeColor = SystemColors.GrayText;
                            lstNodes.Add(objLoopNode);
                        }
                    }
                    objWeapons.Add(objSubWeapon);
                }
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("sourceid", _sourceID.ToString());
            objWriter.WriteElementString("guid", _guiID.ToString());
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

            objWriter.WriteStartElement("clips");
            foreach (Clip clip in _ammo)
            {
                if (string.IsNullOrWhiteSpace(clip.AmmoName))
                {
                    clip.AmmoName = GetAmmoName(clip.Guid);
                }
                clip.Save(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteElementString("conceal", _intConceal.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _decCost.ToString(CultureInfo.InvariantCulture));
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
            objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
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
                            LoopClip.AmmoName = GetAmmoName(LoopClip.Guid);
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
            objNode.TryGetDecFieldQuickly("cost", ref _decCost);
            objNode.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objNode.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("parentid", ref _strParentID);
            if (!objNode.TryGetBoolFieldQuickly("allowaccessory", ref _blnAllowAccessory))
                _blnAllowAccessory = MyXmlNode?["allowaccessory"]?.InnerText != System.Boolean.FalseString;
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
                XmlNode objWeaponNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + _strName + "\"]");
                if (objWeaponNode?["alternaterange"] != null)
                {
                    _strAlternateRange = objWeaponNode["alternaterange"].InnerText;
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

            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
            {
                XmlNode objWeaponNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + _strName + "\"]");
                if (objWeaponNode != null)
                {
                    objWeaponNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objWeaponNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                objWeaponNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                _strAltCategory = objWeaponNode?.Attributes?["translate"]?.InnerText;
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
                    Weapon objUnderbarrel = new Weapon(_objCharacter);
                    objUnderbarrel.ParentVehicle = ParentVehicle;
                    objUnderbarrel.Load(nodWeapon, blnCopy);
                    objUnderbarrel.Parent = this;
                    _lstUnderbarrel.Add(objUnderbarrel);
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture)
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
            Gear objGear = CommonFunctions.DeepFindById(ParentID, lstGearToSearch);

            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("name_english", _strName);
            objWriter.WriteElementString("category", DisplayCategory);
            objWriter.WriteElementString("category_english", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("reach", TotalReach.ToString(objCulture));
            objWriter.WriteElementString("accuracy", TotalAccuracy.ToString(objCulture));
            objWriter.WriteElementString("damage", CalculatedDamage(false, objCulture));
            objWriter.WriteElementString("damage_english", CalculatedDamage(true, objCulture));
            objWriter.WriteElementString("rawdamage", _strDamage);
            objWriter.WriteElementString("ap", TotalAP);
            objWriter.WriteElementString("mode", CalculatedMode);
            objWriter.WriteElementString("rc", TotalRC);
            objWriter.WriteElementString("ammo", CalculatedAmmo(false, objCulture));
            objWriter.WriteElementString("ammo_english", CalculatedAmmo(true, objCulture));
            objWriter.WriteElementString("conceal", CalculatedConcealability(objCulture));
            if (objGear != null)
            {
                objWriter.WriteElementString("avail", objGear.TotalAvail(true));
                objWriter.WriteElementString("cost", objGear.TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
                objWriter.WriteElementString("owncost", objGear.OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            }
            else
            {
                objWriter.WriteElementString("avail", TotalAvail);
                objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
                objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            }
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("weaponname", _strWeaponName);
            objWriter.WriteElementString("location", _strLocation);
            if (_lstAccessories.Count > 0)
            {
                objWriter.WriteStartElement("accessories");
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                    objAccessory.Print(objWriter, objCulture);
                objWriter.WriteEndElement();
            }

            Dictionary<string, string> dictionaryRanges = GetRangeStrings(objCulture);
            // <ranges>
            objWriter.WriteStartElement("ranges");
            objWriter.WriteElementString("name", Range);
            objWriter.WriteElementString("short", dictionaryRanges["short"]);
            objWriter.WriteElementString("medium", dictionaryRanges["medium"]);
            objWriter.WriteElementString("long", dictionaryRanges["long"]);
            objWriter.WriteElementString("extreme", dictionaryRanges["extreme"]);
            // </ranges>
            objWriter.WriteEndElement();

            // <alternateranges>
            objWriter.WriteStartElement("alternateranges");
            objWriter.WriteElementString("name", AlternateRange);
            objWriter.WriteElementString("short", dictionaryRanges["alternateshort"]);
            objWriter.WriteElementString("medium", dictionaryRanges["alternatemedium"]);
            objWriter.WriteElementString("long", dictionaryRanges["alternatelong"]);
            objWriter.WriteElementString("extreme", dictionaryRanges["alternateextreme"]);
            // </alternateranges>
            objWriter.WriteEndElement();

            if (_lstUnderbarrel.Count > 0)
            {
                foreach (Weapon objUnderbarrel in _lstUnderbarrel)
                {
                    objWriter.WriteStartElement("underbarrel");
                    objUnderbarrel.Print(objWriter, objCulture);
                    objWriter.WriteEndElement();
                }
            }

            // Currently loaded Ammo.
            Guid guiAmmo = GetClip(_intActiveAmmoSlot).Guid;

            objWriter.WriteElementString("currentammo", GetAmmoName(guiAmmo));
            objWriter.WriteStartElement("clips");
            foreach (Clip objClip in _ammo)
            {
                if (string.IsNullOrWhiteSpace(objClip.AmmoName))
                {
                    objClip.AmmoName = GetAmmoName(objClip.Guid);
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
                objWriter.WriteElementString("notes", _strNotes);

            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Get the name of Ammo from the character or Vehicle.
        /// </summary>
        /// <param name="guiAmmo">InternalId of the Ammo to find.</param>
        private string GetAmmoName(Guid guiAmmo)
        {
            if (guiAmmo == Guid.Empty)
                return string.Empty;
            else
            {
                Gear objAmmo = CommonFunctions.DeepFindById(guiAmmo.ToString(), _objCharacter.Gear);
                if (objAmmo == null)
                {
                    objAmmo = CommonFunctions.FindVehicleGear(guiAmmo.ToString(), _objCharacter.Vehicles);
                }

                if (objAmmo != null)
                    return objAmmo.DisplayNameShort;
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
        public string InternalId => _guiID.ToString();

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

                if (!string.IsNullOrEmpty(_strWeaponName))
                {
                    strReturn += " (\"" + _strWeaponName + "\")";
                }

                return strReturn;
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
        public string DisplayCategory
        {
            get
            {
                string strReturn = _strCategory;
                if (!string.IsNullOrEmpty(_strAltCategory))
                    strReturn = _strAltCategory;

                // So Categories are actually the name of object types, so pull them from the language file.
                if (strReturn == "Gear")
                {
                    strReturn = LanguageManager.GetString("String_SelectPACKSKit_Gear");
                }
                if (strReturn == "Cyberware")
                {
                    strReturn = LanguageManager.GetString("String_SelectPACKSKit_Cyberware");
                }
                if (strReturn == "Bioware")
                {
                    strReturn = LanguageManager.GetString("String_SelectPACKSKit_Bioware");
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Translated Ammo Category.
        /// </summary>
        public string DisplayAmmoCategory
        {
            get
            {
                string strReturn = AmmoCategory;
                // Get the translated name if applicable.
                if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");
                    XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                    strReturn = objNode?.Attributes?["translate"]?.InnerText;
                }

                return strReturn;
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
            get { return GetClip(_intActiveAmmoSlot).Guid.ToString(); }
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
        public decimal Cost
        {
            get
            {
                return _decCost;
            }
            set
            {
                _decCost = value;
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
                return _blnDiscountCost;
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

        private XmlNode _objCachedMyXmlNode = null;
        public XmlNode MyXmlNode
        {
            get
            {
                if (_objCachedMyXmlNode == null || GlobalOptions.LiveCustomData)
                    _objCachedMyXmlNode = XmlManager.Load("weapons.xml")?.SelectSingleNode("/chummer/weapons/weapon[id = \"" + _sourceID.ToString() + "\"]");
                return _objCachedMyXmlNode;
            }
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
                strReturn = "+" + intReturn.ToString(objCulture);
            else
                strReturn = intReturn.ToString(objCulture);

            return strReturn;
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        public string CalculatedDamage(bool blnForceEnglish = false, CultureInfo objCulture = null)
        {
            if (objCulture == null)
                objCulture = GlobalOptions.CultureInfo;
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
                        Cyberware objWeaponParent = CommonFunctions.FindVehicleCyberware(ParentID, _objCharacter.Vehicles, out VehicleMod objVehicleMod);
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
            if (strDamage.Contains("min") && !strDamage.Contains("mini") && !strDamage.Contains("mine"))
            {
                string strMin = string.Empty;
                int intStart = strDamage.IndexOf("min");
                int intEnd = strDamage.IndexOf(')', intStart);
                strMin = strDamage.Substring(intStart, intEnd - intStart + 1);

                string strExpression = strMin;
                strExpression = strExpression.Replace("min", string.Empty).FastEscape("()".ToCharArray());

                string[] strValue = strExpression.Split(',');
                strExpression = Math.Min(Convert.ToInt32(strValue[0]), Convert.ToInt32(strValue[1])).ToString();

                strDamage = strDamage.Replace(strMin, strExpression);
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
                strDamageExtra += " " + strSplash;
                strDamage = strDamage.Replace(strSplash, string.Empty).Trim();
            }

            // Replace the division sign with "div" since we're using XPath.
            strDamage = strDamage.Replace("/", " div ");

            // Include WeaponCategoryDV Improvements.
            int intImprove = 0;
            if (_objCharacter != null)
            {
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.WeaponCategoryDV && objImprovement.Enabled && (objImprovement.ImprovedName == _strCategory || "Cyberware " + objImprovement.ImprovedName == _strCategory))
                        intImprove += objImprovement.Value;
                    if (!string.IsNullOrEmpty(_strUseSkill))
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.WeaponCategoryDV && objImprovement.Enabled && (objImprovement.ImprovedName == _strUseSkill || "Cyberware " + objImprovement.ImprovedName == _strCategory))
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
                Gear objGear = CommonFunctions.DeepFindById(AmmoLoaded, _objCharacter.Gear);
                if (objGear == null)
                {
                    objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles);
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
            if (!blnForceEnglish)
            {
                strReturn = strReturn.CheapReplace("S", () => LanguageManager.GetString("String_DamageStun"));
                strReturn = strReturn.CheapReplace("P", () => LanguageManager.GetString("String_DamagePhysical"));

                strReturn = strReturn.CheapReplace("Chemical", () => LanguageManager.GetString("String_DamageChemical"));
                strReturn = strReturn.CheapReplace("Special", () => LanguageManager.GetString("String_DamageSpecial"));
                strReturn = strReturn.CheapReplace("(e)", () => LanguageManager.GetString("String_DamageElectric"));
                strReturn = strReturn.CheapReplace("(f)", () => LanguageManager.GetString("String_DamageFlechette"));
                strReturn = strReturn.CheapReplace("P or S", () => LanguageManager.GetString("String_DamagePOrS"));
                strReturn = strReturn.CheapReplace("Grenade", () => LanguageManager.GetString("String_DamageGrenade"));
                strReturn = strReturn.CheapReplace("Missile", () => LanguageManager.GetString("String_DamageMissile"));
                strReturn = strReturn.CheapReplace("Mortar", () => LanguageManager.GetString("String_DamageMortar"));
                strReturn = strReturn.CheapReplace("Rocket", () => LanguageManager.GetString("String_DamageRocket"));
                strReturn = strReturn.CheapReplace("Radius", () => LanguageManager.GetString("String_DamageRadius"));
                strReturn = strReturn.CheapReplace("As Drug/Toxin", () => LanguageManager.GetString("String_DamageAsDrugToxin"));
                strReturn = strReturn.CheapReplace("as round", () => LanguageManager.GetString("String_DamageAsRound"));
                strReturn = strReturn.CheapReplace("/m", () => "/" + LanguageManager.GetString("String_DamageMeter"));
            }

            return strReturn;
        }

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        public string CalculatedAmmo(bool blnForceEnglish = false, CultureInfo objCulture = null)
        {
            if (objCulture == null)
                objCulture = GlobalOptions.CultureInfo;
            string[] strSplit = new string[] { " " };
            string[] strAmmos = _strAmmo.Split(strSplit, StringSplitOptions.None);
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

                    // If this is an Underbarrel Weapons that has been added, cut the Ammo capacity in half.
                    try
                    {
                        if (IsUnderbarrelWeapon && !IncludedInWeapon)
                            intAmmo = Convert.ToInt32(strThisAmmo) / 2;
                        else
                            intAmmo = Convert.ToInt32(strThisAmmo);
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
                strReturn += strThisAmmo + " ";
            }

            strReturn = strReturn.Trim();

            if (!blnForceEnglish)
            {
                // Translate the Ammo string.
                strReturn = strReturn.CheapReplace(" or ", () => " " + LanguageManager.GetString("String_Or") + " ");
                strReturn = strReturn.CheapReplace(" belt", () => LanguageManager.GetString("String_AmmoBelt"));
                strReturn = strReturn.CheapReplace(" Energy", () => LanguageManager.GetString("String_AmmoEnergy"));
                strReturn = strReturn.CheapReplace(" external source", () => LanguageManager.GetString("String_AmmoExternalSource"));
                strReturn = strReturn.CheapReplace(" Special", () => LanguageManager.GetString("String_AmmoSpecial"));

                strReturn = strReturn.CheapReplace("(b)", () => "(" + LanguageManager.GetString("String_AmmoBreakAction") + ")");
                strReturn = strReturn.CheapReplace("(belt)", () => "(" + LanguageManager.GetString("String_AmmoBelt") + ")");
                strReturn = strReturn.CheapReplace("(box)", () => "(" + LanguageManager.GetString("String_AmmoBox") + ")");
                strReturn = strReturn.CheapReplace("(c)", () => "(" + LanguageManager.GetString("String_AmmoClip") + ")");
                strReturn = strReturn.CheapReplace("(cy)", () => "(" + LanguageManager.GetString("String_AmmoCylinder") + ")");
                strReturn = strReturn.CheapReplace("(d)", () => "(" + LanguageManager.GetString("String_AmmoDrum") + ")");
                strReturn = strReturn.CheapReplace("(m)", () => "(" + LanguageManager.GetString("String_AmmoMagazine") + ")");
                strReturn = strReturn.CheapReplace("(ml)", () => "(" + LanguageManager.GetString("String_AmmoMuzzleLoad") + ")");
            }

            return strReturn;
        }

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        public string CalculatedMode
        {
            get
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
                    Gear objGear = CommonFunctions.DeepFindById(AmmoLoaded, _objCharacter.Gear);
                    if (objGear == null)
                    {
                        objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles);
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
                    strReturn += LanguageManager.GetString("String_ModeSingleShot") + "/";
                if (lstModes.Contains("SA"))
                    strReturn += LanguageManager.GetString("String_ModeSemiAutomatic") + "/";
                if (lstModes.Contains("BF"))
                    strReturn += LanguageManager.GetString("String_ModeBurstFire") + "/";
                if (lstModes.Contains("FA"))
                    strReturn += LanguageManager.GetString("String_ModeFullAutomatic") + "/";
                if (lstModes.Contains("Special"))
                    strReturn += LanguageManager.GetString("String_ModeSpecial") + "/";

                // Remove the trailing "/".
                if (!string.IsNullOrEmpty(strReturn))
                    strReturn = strReturn.Substring(0, strReturn.Length - 1);

                return strReturn;
            }
        }

        /// <summary>
        /// Determine if the Weapon is capable of firing in a particular mode.
        /// </summary>
        /// <param name="strFindMode">Firing mode to find.</param>
        public bool AllowMode(string strFindMode)
        {
            string[] strModes = CalculatedMode.Split('/');
            return strModes.Any(strMode => strMode == strFindMode);
        }

        /// <summary>
        /// Weapon Cost to use when working with Total Cost price modifiers for Weapon Mods.
        /// </summary>
        public decimal MultipliableCost
        {
            get
            {
                decimal decReturn = _decCost;

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
                XmlDocument objXmlDocument = XmlManager.Load("weapons.xml");
                XmlNode objAccessoryNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + _strName + "\"]");
                string strMounts = string.Empty;
                XmlNodeList objXmlMountList = objAccessoryNode?.SelectNodes("accessorymounts/mount");

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
                if (_blnCyberware || _strCategory == "Gear")
                    return 0;
                else
                {
                    decimal decReturn = _decCost;
                    if (DiscountCost)
                        decReturn *= 0.9m;
                    return decReturn;
                }
            }
        }

        /// <summary>
        /// The Weapon's total AP including Ammunition.
        /// </summary>
        public string TotalAP
        {
            get
            {
                string strAP = _strAP;

                int intAP = 0;

                // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
                if (!string.IsNullOrEmpty(AmmoLoaded))
                {
                    // Look for Ammo on the character.
                    Gear objGear = CommonFunctions.DeepFindById(AmmoLoaded, _objCharacter.Gear);
                    if (objGear == null)
                    {
                        objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles);
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
                                intAP -= 5;
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
                                    intAP += Convert.ToInt32(strAPAdd);
                            }
                        }
                    }

                    if (_objCharacter != null)
                    {
                        // Add any UnarmedAP bonus for the Unarmed Attack item.
                        if (_strName == "Unarmed Attack" || Skill != null && Skill.Name == "Unarmed Combat" && _objCharacter.Options.UnarmedImprovementsApplyToWeapons)
                        {
                            intAP += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedAP);
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
                            intAP -= 5;
                        }
                        else
                        {
                            // Change the Weapon's AP value.
                            if (!string.IsNullOrEmpty(objAccessory.APReplacement))
                                strAP = objAccessory.APReplacement;
                            // Adjust the Weapon's AP value.
                            if (!string.IsNullOrEmpty(objAccessory.AP))
                                intAP += Convert.ToInt32(objAccessory.AP);
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
                            Cyberware objWeaponParent = CommonFunctions.FindVehicleCyberware(ParentID, _objCharacter.Vehicles, out VehicleMod objVehicleMod);
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
                    return strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf"));
                }
                catch (OverflowException)
                {
                    // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                    return strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf"));
                }
                catch (XPathException)
                {
                    // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                    return strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf"));
                }

                if (intAP == 0)
                    return "-";
                else if (intAP > 0)
                    return "+" + intAP.ToString();
                else
                    return intAP.ToString();
            }
        }

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

                int intRCGroup1 = 0;
                int intRCGroup2 = 0;
                int intRCGroup3 = 0;
                int intRCGroup4 = 0;
                int intRCGroup5 = 0;
                string strRCGroup1 = string.Empty;
                string strRCGroup2 = string.Empty;
                string strRCGroup3 = string.Empty;
                string strRCGroup4 = string.Empty;
                string strRCGroup5 = string.Empty;

                int intRCDeployGroup1 = 0;
                int intRCDeployGroup2 = 0;
                int intRCDeployGroup3 = 0;
                int intRCDeployGroup4 = 0;
                int intRCDeployGroup5 = 0;
                string strRCDeployGroup1 = string.Empty;
                string strRCDeployGroup2 = string.Empty;
                string strRCDeployGroup3 = string.Empty;
                string strRCDeployGroup4 = string.Empty;
                string strRCDeployGroup5 = string.Empty;

                if (_strRC.Contains('('))
                {
                    if (_strRC.Substring(0, 1) == "(")
                    {
                        // The string contains only RC from pieces that can be removed - "(x)" only.
                        strRCFull = _strRC;
                    }
                    else
                    {
                        // The string contains a mix of both fixed and removable RC. "x(y)".
                        int intPos = _strRC.IndexOf('(');
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
                    strRCTip += "+ "+ LanguageManager.GetString("Label_Base") + "(" + strRCBase + ")";
                }

                intRCBase = Convert.ToInt32(strRCBase);
                intRCFull = Convert.ToInt32(strRCFull.Trim("()".ToCharArray()));

                if (intRCBase < 0)
                {
                    intRCModifier = intRCBase;
                    intRCBase = 0;
                }

                // Check if the Weapon has Ammunition loaded and look for any Recoil bonus.
                if (!string.IsNullOrEmpty(AmmoLoaded) && AmmoLoaded != "00000000-0000-0000-0000-000000000000")
                {
                    Gear objGear = CommonFunctions.DeepFindById(AmmoLoaded, _objCharacter.Gear);
                    if (objGear == null)
                    {
                        objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles);
                    }

                    // Change the Weapon's Damage Type.
                    if (objGear?.WeaponBonus?["rc"] != null)
                    {
                        intRCBase += Convert.ToInt32(objGear.WeaponBonus["rc"].InnerText);
                        intRCFull += Convert.ToInt32(objGear.WeaponBonus["rc"].InnerText);

                        strRCTip += " + " + objGear.DisplayName + " (" + objGear.WeaponBonus["rc"].InnerText + ")";
                    }
                }

                // Now that we know the Weapon's RC values, run through all of the Accessories and add theirs to the mix.
                // Only add in the values for items that do not come with the weapon.
                foreach (WeaponAccessory objAccessory in _lstAccessories.Where(objAccessory => !string.IsNullOrEmpty(objAccessory.RC) && objAccessory.Installed))
                {
                    if (_objCharacter.Options.RestrictRecoil && objAccessory.RCGroup != 0)
                    {
                        int intItemRC = Convert.ToInt32(objAccessory.RC);
                        if (!objAccessory.RCDeployable)
                        {
                            switch (objAccessory.RCGroup)
                            {
                                case 1:
                                    if (intRCGroup1 < intItemRC)
                                    {
                                        intRCGroup1 = intItemRC;
                                        strRCGroup1 = objAccessory.DisplayName;
                                    }
                                    break;
                                case 2:
                                    if (intRCGroup2 < intItemRC)
                                    {
                                        intRCGroup2 = intItemRC;
                                        strRCGroup2 = objAccessory.DisplayName;
                                    }
                                    break;
                                case 3:
                                    if (intRCGroup3 < intItemRC)
                                    {
                                        intRCGroup3 = intItemRC;
                                        strRCGroup3 = objAccessory.DisplayName;
                                    }
                                    break;
                                case 4:
                                    if (intRCGroup4 < intItemRC)
                                    {
                                        intRCGroup4 = intItemRC;
                                        strRCGroup4 = objAccessory.DisplayName;
                                    }
                                    break;
                                case 5:
                                    if (intRCGroup5 < intItemRC)
                                    {
                                        intRCGroup5 = intItemRC;
                                        strRCGroup5 = objAccessory.DisplayName;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (objAccessory.RCGroup)
                            {
                                case 1:
                                    if (intRCDeployGroup1 < intItemRC)
                                    {
                                        intRCDeployGroup1 = intItemRC;
                                        strRCDeployGroup1 = objAccessory.DisplayName;
                                    }
                                    break;
                                case 2:
                                    if (intRCDeployGroup2 < intItemRC)
                                    {
                                        intRCDeployGroup2 = intItemRC;
                                        strRCDeployGroup2 = objAccessory.DisplayName;
                                    }
                                    break;
                                case 3:
                                    if (intRCDeployGroup3 < intItemRC)
                                    {
                                        intRCDeployGroup3 = intItemRC;
                                        strRCDeployGroup3 = objAccessory.DisplayName;
                                    }
                                    break;
                                case 4:
                                    if (intRCDeployGroup4 < intItemRC)
                                    {
                                        intRCDeployGroup4 = intItemRC;
                                        strRCDeployGroup4 = objAccessory.DisplayName;
                                    }
                                    break;
                                case 5:
                                    if (intRCDeployGroup5 < intItemRC)
                                    {
                                        intRCDeployGroup5 = intItemRC;
                                        strRCDeployGroup5 = objAccessory.DisplayName;
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        intRCFull += Convert.ToInt32(objAccessory.RC);
                        if (!objAccessory.RCDeployable)
                        {
                            intRCBase += Convert.ToInt32(objAccessory.RC);
                        }
                        strRCTip += " + " + objAccessory.DisplayName + " (" + objAccessory.RC + ")";
                    }
                }

                // Add in the Recoil Group bonuses.
                intRCBase += intRCGroup1 + intRCGroup2 + intRCGroup3 + intRCGroup4 + intRCGroup5;
                intRCFull += intRCGroup1 + intRCGroup2 + intRCGroup3 + intRCGroup4 + intRCGroup5;
                intRCFull += intRCDeployGroup1 + intRCDeployGroup2 + intRCDeployGroup3 + intRCDeployGroup4 + intRCDeployGroup5;

                if (!string.IsNullOrEmpty(strRCGroup1))
                    strRCTip += $" + {strRCGroup1} ({intRCGroup1})";
                if (!string.IsNullOrEmpty(strRCGroup2))
                    strRCTip += $" + {strRCGroup2} ({intRCGroup2})";
                if (!string.IsNullOrEmpty(strRCGroup3))
                    strRCTip += $" + {strRCGroup3} ({intRCGroup3})";
                if (!string.IsNullOrEmpty(strRCGroup4))
                    strRCTip += $" + {strRCGroup4} ({intRCGroup4})";
                if (!string.IsNullOrEmpty(strRCGroup5))
                    strRCTip += $" + {strRCGroup5} ({intRCGroup5})";

                if (!string.IsNullOrEmpty(strRCDeployGroup1))
                    strRCTip += LanguageManager.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup1).Replace("{1}", intRCDeployGroup1.ToString());
                if (!string.IsNullOrEmpty(strRCDeployGroup2))
                    strRCTip += LanguageManager.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup2).Replace("{1}", intRCDeployGroup2.ToString());
                if (!string.IsNullOrEmpty(strRCDeployGroup3))
                    strRCTip += LanguageManager.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup3).Replace("{1}", intRCDeployGroup3.ToString());
                if (!string.IsNullOrEmpty(strRCDeployGroup4))
                    strRCTip += LanguageManager.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup4).Replace("{1}", intRCDeployGroup4.ToString());
                if (!string.IsNullOrEmpty(strRCDeployGroup5))
                    strRCTip += LanguageManager.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup5).Replace("{1}", intRCDeployGroup5.ToString());

                int intUseSTR = 0;
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = CommonFunctions.FindVehicleCyberware(ParentID, _objCharacter.Vehicles, out VehicleMod objVehicleMod);
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
                                intSTR = objAttributeSource.TotalStrength;
                            }

                            intUseSTR = intSTR;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                    }
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
                            Cyberware objWeaponParent = CommonFunctions.FindVehicleCyberware(ParentID, _objCharacter.Vehicles, out VehicleMod objVehicleMod);
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
                    .Where(i => i.ImproveType == Improvement.ImprovementType.WeaponAccuracy && (i.ImprovedName == string.Empty || i.ImprovedName == Name || i.ImprovedName.Contains("[contains]") && s.Contains(i.ImprovedName.Replace("[contains]",string.Empty).ToLower())))
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
                                strSpec = DisplayNameShort;
                                break;
                            case "Exotic Ranged Weapons":
                            case "Special Weapons":
                                strSkill = "Exotic Ranged Weapon";
                                strSpec = DisplayNameShort;
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
                            strSkill += $" ({DisplayNameShort})";

                        // Use the Skill defined by the Weapon if one is present.
                        if (!string.IsNullOrEmpty(_strUseSkill))
                        {
                            strSkill = _strUseSkill;

                            if (strSkill.StartsWith("Exotic"))
                                strSkill += $"({DisplayNameShort})";
                        }

                        if (strPowerSkill == strSkill)
                            if (!string.IsNullOrEmpty(strSpec))
                            {
                                if (_objCharacter.SkillsSection.Skills.Any(objSkill => objSkill.Name.StartsWith("Exotic") && objSkill.DisplaySpecialization == strSpec))
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

        /// <summary>
        /// The string for the Weapon's Range category (setter is English-only).
        /// </summary>
        public string Range
        {
            get
            {
                string strRange = _strRange;
                if (string.IsNullOrWhiteSpace(strRange))
                    strRange = _strCategory;
                if (!string.IsNullOrWhiteSpace(strRange) && GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    XmlDocument objXmlDocument = XmlManager.Load("ranges.xml");
                    XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[name = \"" + strRange + "\"]");
                    XmlNode xmlTranslateNode = objXmlCategoryNode?["translate"];
                    if (xmlTranslateNode != null)
                    {
                        strRange = xmlTranslateNode.InnerText;
                    }
                    else
                    {
                        objXmlDocument = XmlManager.Load("weapons.xml");
                        objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strRange + "\"]");
                        xmlTranslateNode = objXmlCategoryNode?.Attributes?["translate"];
                        if (xmlTranslateNode != null)
                            strRange = xmlTranslateNode.InnerText;
                    }
                }
                return strRange;
            }
            set => _strRange = value;
        }

        /// <summary>
        /// The string for the Weapon's Range category (setter is English-only).
        /// </summary>
        public string AlternateRange
        {
            get
            {
                string strRange = _strAlternateRange.Trim();
                if (!string.IsNullOrEmpty(strRange) && GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    XmlDocument objXmlDocument = XmlManager.Load("ranges.xml");
                    XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[name = \"" + strRange + "\"]");
                    XmlNode xmlTranslateNode = objXmlCategoryNode?["translate"];
                    if (xmlTranslateNode != null)
                    {
                        strRange = xmlTranslateNode.InnerText;
                    }
                    else
                    {
                        objXmlDocument = XmlManager.Load("weapons.xml");
                        objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strRange + "\"]");
                        xmlTranslateNode = objXmlCategoryNode?.Attributes?["translate"];
                        if (xmlTranslateNode != null)
                            strRange = xmlTranslateNode.InnerText;
                    }
                }
                return strRange;
            }
            set => _strAlternateRange = value;
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
                        Cyberware objWeaponParent = CommonFunctions.FindVehicleCyberware(ParentID, _objCharacter.Vehicles, out VehicleMod objVehicleMod);
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
                    Gear objGear = CommonFunctions.DeepFindById(AmmoLoaded, _objCharacter.Gear);
                    if (objGear == null)
                    {
                        objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles);
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
        public Dictionary<string, string> GetRangeStrings(CultureInfo objCulture)
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
                    { "short", (intMin < 0 || intShort < 0) ? string.Empty : (intMin).ToString(objCulture) + "-" + intShort.ToString(objCulture) },
                    { "medium", (intShort < 0 || intMedium < 0) ? string.Empty : (intShort + 1).ToString(objCulture) + "-" + intMedium.ToString(objCulture) },
                    { "long", (intMedium < 0 || intLong < 0) ? string.Empty : (intMedium + 1).ToString(objCulture) + "-" + intLong.ToString(objCulture) },
                    { "extreme", (intLong < 0 || intExtreme < 0) ? string.Empty : (intLong + 1).ToString(objCulture) + "-" + intExtreme.ToString(objCulture) },
                    { "alternateshort", (intAlternateMin < 0 || intAlternateShort < 0) ? string.Empty : (intAlternateMin).ToString(objCulture) + "-" + intAlternateShort.ToString(objCulture) },
                    { "alternatemedium", (intAlternateShort < 0 || intAlternateMedium < 0) ? string.Empty : (intAlternateShort + 1).ToString(objCulture) + "-" + intAlternateMedium.ToString(objCulture) },
                    { "alternatelong", (intAlternateMedium < 0 || intAlternateLong < 0) ? string.Empty : (intAlternateMedium + 1).ToString(objCulture) + "-" + intAlternateLong.ToString(objCulture) },
                    { "alternateextreme", (intAlternateLong < 0 || intAlternateExtreme < 0) ? string.Empty : (intAlternateLong + 1).ToString(objCulture) + "-" + intAlternateExtreme.ToString(objCulture) }
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
            Skill objSkill = Skill;

            int intDicePool = 0;
            int intSmartlinkBonus = 0;
            int intDicePoolModifier = 0;

            foreach (Gear objGear in _objCharacter.Gear)
            {
                if (objGear.InternalId == AmmoLoaded)
                {
                    if (objGear.WeaponBonus != null)
                    {
                        if (objGear.WeaponBonus["pool"] != null)
                            intDicePoolModifier += Convert.ToInt32(objGear.WeaponBonus["pool"].InnerText);
                    }
                }
            }

            if (objSkill != null)
            {
                intDicePool = objSkill.Pool;
            }
            foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.WeaponCategoryDice && x.ImprovedName == Category))
            {
                intDicePoolModifier += objImprovement.Value;
            }

            int intRating = intDicePool + intSmartlinkBonus + intDicePoolModifier;
            string strReturn = intRating.ToString(objCulture);

            // If the character has a Specialization, include it in the Dice Pool string.
            if (objSkill != null && (objSkill.Specializations.Count > 0 && !objSkill.IsExoticSkill))
            {
                if (objSkill.HasSpecialization(DisplayNameShort) || objSkill.HasSpecialization(_strName) || objSkill.HasSpecialization(DisplayCategory) || objSkill.HasSpecialization(_strCategory) || (!string.IsNullOrEmpty(objSkill.Specialization) && (objSkill.HasSpecialization(_strSpec) || objSkill.HasSpecialization(_strSpec2))))
                    strReturn += " (" + (intRating + 2).ToString(objCulture) + ")";
            }

            return strReturn;
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
                        strSpec = DisplayNameShort;
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
                    strSpec = DisplayNameShort;
                    break;
                case "Exotic Ranged Weapons":
                case "Special Weapons":
                    strSkill = "Exotic Ranged Weapon";
                    strSpec = DisplayNameShort;
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
                        strSpec = DisplayNameShort;
                        break;
                    case "Exotic Ranged Weapons":
                    case "Special Weapons":
                        strSkill = "Exotic Ranged Weapon";
                        strSpec = DisplayNameShort;
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
                    strKey += " (" + strSpec + ")";
                Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill(strKey);
                int intDicePool = 0;
                if (objSkill != null)
                {
                    intDicePool = objSkill.Pool;
                }

                strReturn = strSkill + " (" + intDicePool.ToString() + ")";

                if (objSkill != null && (!string.IsNullOrEmpty(objSkill.Specialization) && !objSkill.IsExoticSkill))
                {
                    if (objSkill.HasSpecialization(DisplayNameShort) || objSkill.HasSpecialization(_strName) || objSkill.HasSpecialization(DisplayCategory) || objSkill.HasSpecialization(_strCategory) || (!string.IsNullOrEmpty(objSkill.Specialization) && (objSkill.HasSpecialization(_strSpec) || objSkill.HasSpecialization(_strSpec2))))
                        strReturn += " + " + LanguageManager.GetString("String_ExpenseSpecialization") + " (2)";
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail
        {
            get
            {
                Tuple<int, string> objAvailPair = TotalAvailPair;
                string strAvail = objAvailPair.Item2;
                // Translate the Avail string.
                if (strAvail == "F")
                    strAvail = LanguageManager.GetString("String_AvailForbidden");
                else if (strAvail == "R")
                    strAvail = LanguageManager.GetString("String_AvailRestricted");

                return objAvailPair.Item1.ToString() + strAvail;
            }
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
                        else if (objLoopAvail.Item2.EndsWith('R') && strAvail != "F")
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
                        if (strAccAvail.StartsWith('+') || strAccAvail.StartsWith('-'))
                        {
                            strAccAvail = objAccessory.TotalAvail;
                            if (strAccAvail.EndsWith(LanguageManager.GetString("String_AvailForbidden")))
                            {
                                strAvail = "F";
                                strAccAvail = strAccAvail.Substring(0, strAccAvail.Length - 1);
                            }
                            else if (strAccAvail.EndsWith(LanguageManager.GetString("String_AvailRestricted")))
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
                    writer.WriteElementString("id", Guid.ToString());
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
