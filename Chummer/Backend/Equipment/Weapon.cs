using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Skills;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A Weapon.
    /// </summary>
    public class Weapon : INamedParentWithGuid<Weapon>
    {
        private Guid _sourceID = new Guid();
        private Guid _guiID = new Guid();
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
        //private Guid _guiAmmoLoaded = new Guid();
        //private Guid _guiAmmoLoaded2 = new Guid();
        //private Guid _guiAmmoLoaded3 = new Guid();
        //private Guid _guiAmmoLoaded4 = new Guid();
        private int _intActiveAmmoSlot = 1;
        private string _strAvail = string.Empty;
        private int _intCost = 0;
        private string _strRange = string.Empty;
        private double _dblRangeMultiplier = 1;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strWeaponName = string.Empty;
        private int _intFullBurst = 10;
        private int _intSuppressive = 20;
        private List<WeaponAccessory> _lstAccessories = new List<WeaponAccessory>();
        private List<Weapon> _lstUnderbarrel = new List<Weapon>();
        private bool _blnUnderbarrel = false;
        private bool _blnVehicleMounted = false;
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
        /// <param name="objCharacter">Character that the Weapon is being added to.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="cmsWeapon">ContextMenuStrip to use for Weapons.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip to use for Accessories.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        public void Create(XmlNode objXmlWeapon, Character objCharacter, TreeNode objNode, ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear = null, bool blnCreateChildren = true)
        {
            objXmlWeapon.TryGetField("id", Guid.TryParse, out _sourceID);
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
                if (strMounts.EndsWith("/"))
                {
                    strMounts = strMounts.Substring(0, strMounts.Length - 1);
                }
                _strWeaponSlots = strMounts;
            }
            objXmlWeapon.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            objXmlWeapon.TryGetStringFieldQuickly("rc", ref _strRC);
            objXmlWeapon.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objXmlWeapon.TryGetStringFieldQuickly("avail", ref _strAvail);
            if (objXmlWeapon["cost"] != null)
            {
                // Check for a Variable Cost.
                if (objXmlWeapon["cost"].InnerText.StartsWith("Variable"))
                {
                    int intMin = 0;
                    int intMax = 0;
                    char[] chrParentheses = { '(', ')' };
                    string strCost = objXmlWeapon["cost"].InnerText.Replace("Variable", string.Empty).Trim(chrParentheses);
                    if (strCost.Contains("-"))
                    {
                        string[] strValues = strCost.Split('-');
                        intMin = Convert.ToInt32(strValues[0]);
                        intMax = Convert.ToInt32(strValues[1]);
                    }
                    else
                        intMin = Convert.ToInt32(strCost.Replace("+", string.Empty));

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
                        _intCost = frmPickNumber.SelectedValue;
                    }
                }
                else
                {
                    int.TryParse(objXmlWeapon["cost"].InnerText, out _intCost);
                }
            }

            if (objXmlWeapon["cyberware"]?.InnerText == "yes")
                _blnCyberware = true;
            objXmlWeapon.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlWeapon.TryGetStringFieldQuickly("page", ref _strPage);

            XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");

            if (GlobalOptions.Instance.Language != "en-us")
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

            // Populate the Range if it differs from the Weapon's Category.
            if (objXmlWeapon["range"] != null)
            {
                _strRange = objXmlWeapon["range"].InnerText;
                if (objXmlWeapon["range"].Attributes["multiply"] != null)
                    _dblRangeMultiplier = Convert.ToDouble(objXmlWeapon["range"].Attributes["multiply"].InnerText, GlobalOptions.InvariantCultureInfo);
            }

            objXmlWeapon.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objXmlWeapon.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);

            objXmlWeapon.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
            objXmlWeapon.TryGetBoolFieldQuickly("requireammo", ref _blnRequireAmmo);
            objXmlWeapon.TryGetStringFieldQuickly("spec", ref _strSpec);
            objXmlWeapon.TryGetStringFieldQuickly("spec2", ref _strSpec2);

            objNode.Text = DisplayName;
            objNode.Tag = _guiID.ToString();

            // If the Weapon comes with an Underbarrel Weapon, add it.
            if (objXmlWeapon.InnerXml.Contains("<underbarrels>") && blnCreateChildren)
            {
                foreach (XmlNode objXmlUnderbarrel in objXmlWeapon["underbarrels"].ChildNodes)
                {
                    Weapon objUnderbarrelWeapon = new Weapon(_objCharacter);
                    TreeNode objUnderbarrelNode = new TreeNode();
                    XmlNode objXmlWeaponNode =
                        objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlUnderbarrel.InnerText + "\"]");
                    objUnderbarrelWeapon.Create(objXmlWeaponNode, _objCharacter, objUnderbarrelNode, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                    objUnderbarrelWeapon.IncludedInWeapon = true;
                    objUnderbarrelWeapon.IsUnderbarrelWeapon = true;
                    _lstUnderbarrel.Add(objUnderbarrelWeapon);
                    objUnderbarrelNode.ContextMenuStrip = cmsWeapon;
                    objNode.Nodes.Add(objUnderbarrelNode);
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
                    if (objXmlWeaponAccessory.InnerXml.Contains("<rating>"))
                    {
                        intAccessoryRating = Convert.ToInt32(objXmlWeaponAccessory["rating"].InnerText);
                    }
                    if (objXmlWeaponAccessory.InnerXml.Contains("mount"))
                    {
                        if (objXmlWeaponAccessory.InnerXml.Contains("<extramount>"))
                        {
                            objAccessory.Create(objXmlAccessory, objAccessoryNode, new Tuple<string, string>(objXmlAccessory["mount"].InnerText, objXmlAccessory["extramount"].InnerText), intAccessoryRating, cmsWeaponAccessoryGear, false, blnCreateChildren);
                        }
                        else
                        {
                            objAccessory.Create(objXmlAccessory, objAccessoryNode, new Tuple<string, string>(objXmlAccessory["mount"].InnerText, "None"), intAccessoryRating, cmsWeaponAccessoryGear, false, blnCreateChildren);
                        }
                    }
                    else
                    {
                        objAccessory.Create(objXmlAccessory, objAccessoryNode, new Tuple<string, string>("Internal" , "None"), intAccessoryRating, cmsWeaponAccessoryGear, false, blnCreateChildren);
                    }
                    // Add any extra Gear that comes with the Weapon Accessory.
                    if (objXmlWeaponAccessory["gears"] != null && blnCreateChildren)
                    {
                        XmlDocument objXmlGearDocument = XmlManager.Instance.Load("gear.xml");
                        foreach (XmlNode objXmlAccessoryGear in objXmlWeaponAccessory.SelectNodes("gears/usegear"))
                        {
                            int intGearRating = 0;
                            string strForceValue = string.Empty;
                            if (objXmlAccessoryGear.Attributes["rating"] != null)
                                intGearRating = Convert.ToInt32(objXmlAccessoryGear.Attributes["rating"].InnerText);
                            if (objXmlAccessoryGear.Attributes["select"] != null)
                                strForceValue = objXmlAccessoryGear.Attributes["select"].InnerText;

                            XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlAccessoryGear.InnerText + "\"]");
                            Gear objGear = new Gear(_objCharacter);

                            TreeNode objGearNode = new TreeNode();
                            List<Weapon> lstWeapons = new List<Weapon>();
                            List<TreeNode> lstWeaponNodes = new List<TreeNode>();

                            objGear.Create(objXmlGear, _objCharacter, objGearNode, intGearRating, lstWeapons, lstWeaponNodes, strForceValue, false, false, true);
                            objGear.Cost = "0";
                            objGear.MaxRating = objGear.Rating;
                            objGear.MinRating = objGear.Rating;
                            objGear.IncludedInParent = true;
                            objAccessory.Gear.Add(objGear);

                            objGearNode.ContextMenuStrip = cmsWeaponAccessoryGear;
                            objAccessoryNode.Nodes.Add(objGearNode);
                            objAccessoryNode.Expand();
                        }
                    }

                    objAccessory.IncludedInWeapon = true;
                    objAccessory.Parent = this;
                    objAccessoryNode.ContextMenuStrip = cmsWeaponAccessory;
                    _lstAccessories.Add(objAccessory);
                    objAccessoryNode.Text = objAccessory.DisplayName;
                    objNode.Nodes.Add(objAccessoryNode);
                    objNode.Expand();
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
                clip.Save(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteElementString("conceal", _intConceal.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _intCost.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("useskill", _strUseSkill);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("rangemultiply", _dblRangeMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("fullburst", _intFullBurst.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("suppressive", _intSuppressive.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("weaponname", _strWeaponName);
            objWriter.WriteElementString("included", _blnIncludedInWeapon.ToString());
            objWriter.WriteElementString("installed", _blnInstalled.ToString());
            objWriter.WriteElementString("requireammo", _blnRequireAmmo.ToString());
            objWriter.WriteElementString("accuracy", _strAccuracy);
            objWriter.WriteElementString("mount",_mount);
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
            if (objNode["sourceid"] == null)
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
                XmlNode objWeaponNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + _strName + "\"]");
                if (objWeaponNode != null)
                {
                    _sourceID = Guid.Parse(objWeaponNode["id"].InnerText);
                }
            }
            else
            {
                _sourceID = Guid.Parse(objNode["sourceid"].InnerText);
            }
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
            objNode.TryGetStringFieldQuickly("ap", ref _strAP);
            objNode.TryGetStringFieldQuickly("mode", ref _strMode);
            objNode.TryGetStringFieldQuickly("rc", ref _strRC);
            objNode.TryGetStringFieldQuickly("ammo", ref _strAmmo);
            objNode.TryGetBoolFieldQuickly("cyberware", ref _blnCyberware);
            objNode.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            objNode.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetInt32FieldQuickly("cost", ref _intCost);
            objNode.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objNode.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
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
            objNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
            objNode.TryGetDoubleFieldQuickly("rangemultiply", ref _dblRangeMultiplier);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludedInWeapon);
            objNode.TryGetBoolFieldQuickly("installed", ref _blnInstalled);
            objNode.TryGetBoolFieldQuickly("requireammo", ref _blnRequireAmmo);


            //#1544 Ammunition not loading or available.
            if (_strUseSkill == "Throwing Weapons"
                && _strAmmo != "1")
            {
                _strAmmo = "1";
            }

            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
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
                    objUnderbarrel.Load(nodWeapon, blnCopy);
                    objUnderbarrel.IsUnderbarrelWeapon = true;
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
        public void Print(XmlTextWriter objWriter)
        {
            // Find the piece of Gear that created this item if applicable.
            Gear objGear = CommonFunctions.FindGearByWeaponID(_guiID.ToString(), _objCharacter.Gear);

            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("name_english", _strName);
            objWriter.WriteElementString("category", DisplayCategory);
            objWriter.WriteElementString("category_english", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("reach", TotalReach.ToString());
            objWriter.WriteElementString("accuracy", TotalAccuracy);
            objWriter.WriteElementString("damage", CalculatedDamage());
            objWriter.WriteElementString("damage_english", CalculatedDamage(0, true));
            objWriter.WriteElementString("rawdamage", _strDamage);
            objWriter.WriteElementString("ap", TotalAP);
            objWriter.WriteElementString("mode", CalculatedMode);
            objWriter.WriteElementString("rc", TotalRC);
            objWriter.WriteElementString("ammo", CalculatedAmmo());
            objWriter.WriteElementString("ammo_english", CalculatedAmmo(true));
            objWriter.WriteElementString("conceal", CalculatedConcealability());
            if (objGear != null)
            {
                objWriter.WriteElementString("avail", objGear.TotalAvail(true));
                objWriter.WriteElementString("cost", objGear.TotalCost.ToString());
                objWriter.WriteElementString("owncost", objGear.OwnCost.ToString());
            }
            else
            {
                objWriter.WriteElementString("avail", TotalAvail);
                objWriter.WriteElementString("cost", TotalCost.ToString());
                objWriter.WriteElementString("owncost", OwnCost.ToString());
            }
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("weaponname", _strWeaponName);
            objWriter.WriteElementString("location", _strLocation);
            if (_lstAccessories.Count > 0)
            {
                objWriter.WriteStartElement("accessories");
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                    objAccessory.Print(objWriter);
                objWriter.WriteEndElement();
            }

            // <ranges>
            objWriter.WriteStartElement("ranges");
            objWriter.WriteElementString("short", RangeShort);
            objWriter.WriteElementString("medium", RangeMedium);
            objWriter.WriteElementString("long", RangeLong);
            objWriter.WriteElementString("extreme", RangeExtreme);
            // </ranges>
            objWriter.WriteEndElement();

            if (_lstUnderbarrel.Count > 0)
            {
                foreach (Weapon objUnderbarrel in _lstUnderbarrel)
                {
                    objWriter.WriteStartElement("underbarrel");
                    objUnderbarrel.Print(objWriter);
                    objWriter.WriteEndElement();
                }
            }

            // Currently loaded Ammo.
            Guid guiAmmo = GetClip(_intActiveAmmoSlot).Guid;

            objWriter.WriteElementString("currentammo", GetAmmoName(guiAmmo));
            objWriter.WriteStartElement("clips");
            foreach (Clip objClip in _ammo)
            {
                objClip.AmmoName = GetAmmoName(objClip.Guid);
                objClip.Save(objWriter);
            }
            objWriter.WriteEndElement();

            //Don't seem to be used
            //objWriter.WriteElementString("ammoslot1", GetAmmoName(_guiAmmoLoaded));
            //objWriter.WriteElementString("ammoslot2", GetAmmoName(_guiAmmoLoaded2));
            //objWriter.WriteElementString("ammoslot3", GetAmmoName(_guiAmmoLoaded3));
            //objWriter.WriteElementString("ammoslot4", GetAmmoName(_guiAmmoLoaded4));

            objWriter.WriteElementString("dicepool", DicePool);
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
                    Vehicle objVehicle = null;
                    objAmmo = CommonFunctions.FindVehicleGear(guiAmmo.ToString(), _objCharacter.Vehicles, out objVehicle);
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
        public List<WeaponAccessory> WeaponAccessories => _lstAccessories;

        /// <summary>
        /// Underbarrel Weapon.
        /// </summary>
        public List<Weapon> UnderbarrelWeapons => _lstUnderbarrel;

        /// <summary>
        /// Children as Underbarrel Weapon.
        /// </summary>
        public List<Weapon> Children => UnderbarrelWeapons;

        /// <summary>
        /// Whether or not this Weapon is an Underbarrel Weapon.
        /// </summary>
        public bool IsUnderbarrelWeapon
        {
            get
            {
                return _blnUnderbarrel;
            }
            set
            {
                _blnUnderbarrel = value;
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
                    strReturn = LanguageManager.Instance.GetString("String_SelectPACKSKit_Gear");
                }
                if (strReturn == "Cyberware")
                {
                    strReturn = LanguageManager.Instance.GetString("String_SelectPACKSKit_Cyberware");
                }
                if (strReturn == "Bioware")
                {
                    strReturn = LanguageManager.Instance.GetString("String_SelectPACKSKit_Bioware");
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
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
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
            get { return  GetClip(_intActiveAmmoSlot).Guid.ToString(); }
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
        public int Cost
        {
            get
            {
                return _intCost;
            }
            set
            {
                _intCost = value;
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
        /// Whether or not the Weapon is mounted on a Vehicle.
        /// </summary>
        public bool VehicleMounted
        {
            get
            {
                return _blnVehicleMounted;
            }
            set
            {
                _blnVehicleMounted = value;
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
        #endregion

        #region Complex Properties
        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications.
        /// </summary>
        public string CalculatedConcealability()
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
            ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
            intReturn += objImprovementManager.ValueOf(Improvement.ImprovementType.Concealability);

            string strReturn = string.Empty;
            if (intReturn >= 0)
                strReturn = "+" + intReturn.ToString();
            else
                strReturn = intReturn.ToString();

            return strReturn;
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        public string CalculatedDamage(int intUseSTR = 0, bool blnForceEnglish = false)
        {
            string strReturn = _strDamage;

            // If the cost is determined by the Rating, evaluate the expression.
            XmlDocument objXmlDocument = new XmlDocument();
            XPathNavigator nav = objXmlDocument.CreateNavigator();

            string strDamage = string.Empty;
            string strDamageExpression = _strDamage;
            string strDamageType = string.Empty;
            string strDamageExtra = string.Empty;
            XPathExpression xprDamage;

            if (_objCharacter != null)
            {
                ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);

                if (_blnCyberware)
                {
                    int intSTR = _objCharacter.STR.TotalValue;
                    // Look to see if this is attached to a Cyberlimb and use its STR instead.
                    foreach (Cyberware objCyberware in _objCharacter.Cyberware)
                    {
                        if (!string.IsNullOrEmpty(objCyberware.LimbSlot))
                        {
                            if (objCyberware.WeaponID == InternalId)
                                intSTR = objCyberware.TotalStrength;
                            else
                            {
                                foreach (Cyberware objChild in objCyberware.Children)
                                {
                                    if (objChild.WeaponID == InternalId)
                                    {
                                        intSTR = objCyberware.TotalStrength;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // A STR value has been passed, so use that instead.
                    if (intUseSTR > 0)
                        intSTR = intUseSTR;

                    if (_strCategory == "Throwing Weapons" || _strUseSkill == "Throwing Weapons")
                        intSTR += objImprovementManager.ValueOf(Improvement.ImprovementType.ThrowSTR);

                    strDamage = strDamageExpression.Replace("STR", intSTR.ToString());
                }
                else
                {
                    int intThrowDV = 0;
                    if (_strCategory == "Throwing Weapons" || _strUseSkill == "Throwing Weapons")
                        intThrowDV = objImprovementManager.ValueOf(Improvement.ImprovementType.ThrowSTR);

                    // A STR value has been passed, so use that instead.
                    if (intUseSTR > 0)
                        strDamage = strDamageExpression.Replace("STR", (intUseSTR + intThrowDV).ToString());
                    else
                        strDamage = strDamageExpression.Replace("STR", (_objCharacter.STR.TotalValue + intThrowDV).ToString());
                }
            }
            else
            {
                // If the character is null, this is a vehicle.
                strDamage = strDamageExpression.Replace("STR", intUseSTR.ToString());
            }

            // Evaluate the min expression if there is one.
            if (strDamage.Contains("min") && !strDamage.Contains("mini") && !strDamage.Contains("mine"))
            {
                string strMin = string.Empty;
                int intStart = strDamage.IndexOf("min");
                int intEnd = strDamage.IndexOf(')', intStart);
                strMin = strDamage.Substring(intStart, intEnd - intStart + 1);

                string strExpression = strMin;
                strExpression = strExpression.Replace("min", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);

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
                if (strDamage.Contains("P"))
                {
                    strDamageType = "P";
                    strDamage = strDamage.Replace("P", string.Empty);
                }
                if (strDamage.Contains("S"))
                {
                    strDamageType = "S";
                    strDamage = strDamage.Replace("S", string.Empty);
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
                intImprove += _objCharacter.ObjImprovementManager.ValueOf(Improvement.ImprovementType.UnarmedDV);
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
                    Vehicle objFoundVehicle;
                    objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles, out objFoundVehicle);
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
                double dblDamage = 0;
                try
                {
                    xprDamage = nav.Compile(strDamage);
                    dblDamage = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprDamage), GlobalOptions.InvariantCultureInfo) + intBonus);
                }
                catch (XPathException) { }
                if (_strName == "Unarmed Attack (Smashing Blow)")
                    dblDamage *= 2.0;
                strReturn = dblDamage.ToString(GlobalOptions.CultureInfo) + strDamageType + strDamageExtra;
            }
            else
            {
                // Place the Damage Type (P or S) into a string and remove it from the expression.
                if (strDamage.Contains("P or S"))
                {
                    strDamageType = "P or S";
                    strDamage = strDamage.Replace("P or S", string.Empty);
                }
                if (strDamage.Contains("P"))
                {
                    strDamageType = "P";
                    strDamage = strDamage.Replace("P", string.Empty);
                }
                if (strDamage.Contains("S"))
                {
                    strDamageType = "S";
                    strDamage = strDamage.Replace("S", string.Empty);
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

                double dblDamage = 0;
                try
                {
                    xprDamage = nav.Compile(strDamage);
                    dblDamage = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprDamage), GlobalOptions.InvariantCultureInfo) + intBonus);
                }
                catch (XPathException) { }
                if (_strName == "Unarmed Attack (Smashing Blow)")
                    dblDamage *= 2.0;
                strReturn = dblDamage.ToString(GlobalOptions.CultureInfo) + strDamageType + strDamageExtra;
            }

            // If the string couldn't be parsed (resulting in NaN which will happen if it is a special string like "Grenade", "Chemical", etc.), return the Weapon's Damage string.
            if (strReturn.StartsWith("NaN"))
                strReturn = _strDamage;

            // Translate the Damage Code.
            if (!blnForceEnglish)
            {
                strReturn = strReturn.Replace("S", LanguageManager.Instance.GetString("String_DamageStun"));
                strReturn = strReturn.Replace("P", LanguageManager.Instance.GetString("String_DamagePhysical"));

                strReturn = strReturn.Replace("Chemical", LanguageManager.Instance.GetString("String_DamageChemical"));
                strReturn = strReturn.Replace("Special", LanguageManager.Instance.GetString("String_DamageSpecial"));
                strReturn = strReturn.Replace("(e)", LanguageManager.Instance.GetString("String_DamageElectric"));
                strReturn = strReturn.Replace("(f)", LanguageManager.Instance.GetString("String_DamageFlechette"));
                strReturn = strReturn.Replace("P or S", LanguageManager.Instance.GetString("String_DamagePOrS"));
                strReturn = strReturn.Replace("Grenade", LanguageManager.Instance.GetString("String_DamageGrenade"));
                strReturn = strReturn.Replace("Missile", LanguageManager.Instance.GetString("String_DamageMissile"));
                strReturn = strReturn.Replace("Mortar", LanguageManager.Instance.GetString("String_DamageMortar"));
                strReturn = strReturn.Replace("Rocket", LanguageManager.Instance.GetString("String_DamageRocket"));
                strReturn = strReturn.Replace("Radius", LanguageManager.Instance.GetString("String_DamageRadius"));
                strReturn = strReturn.Replace("As Drug/Toxin", LanguageManager.Instance.GetString("String_DamageAsDrugToxin"));
                strReturn = strReturn.Replace("as round", LanguageManager.Instance.GetString("String_DamageAsRound"));
                strReturn = strReturn.Replace("/m", "/" + LanguageManager.Instance.GetString("String_DamageMeter"));
            }

            return strReturn;
        }

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        public string CalculatedAmmo(bool blnForceEnglish = false)
        {
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
                if (strThisAmmo.Contains("("))
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

                    strAmmoString = intAmmo.ToString();
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
                strReturn = strReturn.Replace(" or ", " " + LanguageManager.Instance.GetString("String_Or") + " ");
                strReturn = strReturn.Replace(" belt", LanguageManager.Instance.GetString("String_AmmoBelt"));
                strReturn = strReturn.Replace(" Energy", LanguageManager.Instance.GetString("String_AmmoEnergy"));
                strReturn = strReturn.Replace(" external source", LanguageManager.Instance.GetString("String_AmmoExternalSource"));
                strReturn = strReturn.Replace(" Special", LanguageManager.Instance.GetString("String_AmmoSpecial"));

                strReturn = strReturn.Replace("(b)", "(" + LanguageManager.Instance.GetString("String_AmmoBreakAction") + ")");
                strReturn = strReturn.Replace("(belt)", "(" + LanguageManager.Instance.GetString("String_AmmoBelt") + ")");
                strReturn = strReturn.Replace("(box)", "(" + LanguageManager.Instance.GetString("String_AmmoBox") + ")");
                strReturn = strReturn.Replace("(c)", "(" + LanguageManager.Instance.GetString("String_AmmoClip") + ")");
                strReturn = strReturn.Replace("(cy)", "(" + LanguageManager.Instance.GetString("String_AmmoCylinder") + ")");
                strReturn = strReturn.Replace("(d)", "(" + LanguageManager.Instance.GetString("String_AmmoDrum") + ")");
                strReturn = strReturn.Replace("(m)", "(" + LanguageManager.Instance.GetString("String_AmmoMagazine") + ")");
                strReturn = strReturn.Replace("(ml)", "(" + LanguageManager.Instance.GetString("String_AmmoMuzzleLoad") + ")");
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
                        Vehicle objFoundVehicle;
                        objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles, out objFoundVehicle);
                    }
                    if (objGear != null)
                    {
                        if (objGear.WeaponBonus != null)
                        {
                            if (objGear.WeaponBonus["firemode"] != null)
                            {
                                if (objGear.WeaponBonus["firemode"].InnerText.Contains('/'))
                                {
                                    strModes = objGear.WeaponBonus["firemode"].InnerText.Split('/');

                                    // Move the contents of the array to a list so it's easier to work with.
                                    foreach (string strMode in strModes)
                                        lstModes.Add(strMode);
                                }
                                else
                                {
                                    lstModes.Add(objGear.WeaponBonus["firemode"].InnerText);
                                }
                            }
                            if (objGear.WeaponBonus["modereplace"] != null)
                            {
                                lstModes.Clear();
                                if (objGear.WeaponBonus["modereplace"].InnerText.Contains('/'))
                                {
                                    strModes = objGear.WeaponBonus["modereplace"].InnerText.Split('/');
                                }
                                else
                                {
                                    strModes[0] = objGear.WeaponBonus["modereplace"].InnerText;
                                }
                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                        }

                        // Do the same for any plugins.
                        foreach (Gear objChild in objGear.Children)
                        {
                            if (objChild.WeaponBonus != null)
                            {
                                if (objGear.WeaponBonus["firemode"] != null)
                                {
                                    if (objGear.WeaponBonus["firemode"].InnerText.Contains('/'))
                                    {
                                        strModes = objGear.WeaponBonus["firemode"].InnerText.Split('/');

                                        // Move the contents of the array to a list so it's easier to work with.
                                        foreach (string strMode in strModes)
                                            lstModes.Add(strMode);
                                    }
                                    else
                                    {
                                        lstModes.Add(objGear.WeaponBonus["firemode"].InnerText);
                                    }
                                }
                                if (objGear.WeaponBonus["firemodereplace"] != null)
                                {
                                    if (objGear.WeaponBonus["firemodereplace"].InnerText.Contains('/'))
                                    {
                                        lstModes.Clear();
                                        strModes = objGear.WeaponBonus["firemode"].InnerText.Split('/');

                                        // Move the contents of the array to a list so it's easier to work with.
                                        foreach (string strMode in strModes)
                                            lstModes.Add(strMode);
                                    }
                                }
                                break;
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
                                if (objAccessory.FireModeReplacement.Contains('/'))
                                {
                                    lstModes.Clear();
                                    strModes = objAccessory.FireModeReplacement.Split('/');

                                    // Move the contents of the array to a list so it's easier to work with.
                                    foreach (string strMode in strModes)
                                        lstModes.Add(strMode);
                                }
                            }
                            break;
                        }
                    }
                }

                foreach (WeaponAccessory objAccessory in _lstAccessories.Where(x => !string.IsNullOrEmpty(x.AddMode)))
                {
                    lstModes.Add(objAccessory.AddMode);
                }

                string strReturn = string.Empty;
                if (lstModes.Contains("SS"))
                    strReturn += LanguageManager.Instance.GetString("String_ModeSingleShot") + "/";
                if (lstModes.Contains("SA"))
                    strReturn += LanguageManager.Instance.GetString("String_ModeSemiAutomatic") + "/";
                if (lstModes.Contains("BF"))
                    strReturn += LanguageManager.Instance.GetString("String_ModeBurstFire") + "/";
                if (lstModes.Contains("FA"))
                    strReturn += LanguageManager.Instance.GetString("String_ModeFullAutomatic") + "/";
                if (lstModes.Contains("Special"))
                    strReturn += LanguageManager.Instance.GetString("String_ModeSpecial") + "/";

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
        public int MultipliableCost
        {
            get
            {
                int intWeaponCost = _intCost;
                int intReturn = intWeaponCost;

                // Run through the list of Weapon Mods.
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                {
                    if (!objAccessory.IncludedInWeapon)
                    {
                        if (!objAccessory.Cost.StartsWith("Total Cost"))
                            intReturn += objAccessory.TotalCost;
                    }
                }

                return intReturn;
            }
        }

        public string AccessoryMounts
        {
            get
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
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
        public int TotalCost
        {
            get
            {
                int intWeaponCost = _intCost;
                int intReturn = intWeaponCost;

                if (DiscountCost)
                    intReturn = intReturn * 9 / 10;

                // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
                intReturn += _lstAccessories.Where(objAccessory => !objAccessory.IncludedInWeapon).Sum(objAccessory => objAccessory.TotalCost);

                // If this is a Cyberware or Gear Weapon, remove the Weapon Cost from this since it has already been paid for through the parent item (but is needed to calculate Mod price).
                if (_blnCyberware || _strCategory == "Gear")
                    intReturn -= intWeaponCost;

                // Include the cost of any Underbarrel Weapon.
                if (_lstUnderbarrel.Count > 0)
                {
                    intReturn += _lstUnderbarrel.Sum(objUnderbarrel => objUnderbarrel.TotalCost);
                }

                return intReturn;
            }
        }

        /// <summary>
        /// The cost of just the Weapon itself.
        /// </summary>
        public int OwnCost
        {
            get
            {
                int intWeaponCost = _intCost;
                int intReturn = intWeaponCost;

                if (DiscountCost)
                    intReturn = intReturn * 9 / 10;

                // If this is a Cyberware or Gear Weapon, remove the Weapon Cost from this since it has already been paid for through the parent item (but is needed to calculate Mod price).
                if (_blnCyberware || _strCategory == "Gear")
                    intReturn -= intWeaponCost;

                return intReturn;
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
                if (strAP == "-")
                    strAP = "0";
                int intAP = 0;

                try
                {
                    intAP = Convert.ToInt32(strAP);
                }
                catch (FormatException)
                {
                    // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                    return _strAP.Replace("-half", LanguageManager.Instance.GetString("String_APHalf"));
                }

                bool blnAPReplaced = false;

                // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
                if (!string.IsNullOrEmpty(AmmoLoaded))
                {
                    // Look for Ammo on the character.
                    Gear objGear = CommonFunctions.DeepFindById(AmmoLoaded, _objCharacter.Gear);
                    if (objGear == null)
                    {
                        Vehicle objFoundVehicle;
                        objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles, out objFoundVehicle);
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
                                if (objGear.WeaponBonus["apreplace"] != null)
                                {
                                    blnAPReplaced = true;
                                    strAP = objGear.WeaponBonus["apreplace"].InnerText;
                                }
                                // Adjust the Weapon's Damage.
                                if (objGear.WeaponBonus["ap"] != null)
                                    intAP += Convert.ToInt32(objGear.WeaponBonus["ap"].InnerText);
                            }
                        }
                    }

                    if (_objCharacter != null)
                    {
                        // Add any UnarmedAP bonus for the Unarmed Attack item.
                        if (_strName == "Unarmed Attack" || Skill != null && Skill.Name == "Unarmed Combat" && _objCharacter.Options.UnarmedImprovementsApplyToWeapons)
                        {
                            intAP += _objCharacter.ObjImprovementManager.ValueOf(Improvement.ImprovementType.UnarmedAP);
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
                            {
                                blnAPReplaced = true;
                                strAP = objAccessory.APReplacement;
                            }
                            // Adjust the Weapon's AP value.
                            if (!string.IsNullOrEmpty(objAccessory.AP))
                                intAP += Convert.ToInt32(objAccessory.AP);
                        }
                    }
                }

                if (!blnAPReplaced)
                {
                    if (intAP == 0)
                        return "-";
                    else if (intAP > 0)
                        return "+" + intAP.ToString();
                    else
                        return intAP.ToString();
                }
                else
                    return strAP.Replace("-half", LanguageManager.Instance.GetString("String_APHalf"));
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
                    strRCTip += "+ "+ LanguageManager.Instance.GetString("Label_Base") + "(" + strRCBase + ")";
                }

                intRCBase = Convert.ToInt32(strRCBase);
                intRCFull = Convert.ToInt32(strRCFull.Replace("(", string.Empty).Replace(")", string.Empty));

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
                        Vehicle objFoundVehicle;
                        objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles, out objFoundVehicle);
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
                    strRCTip += LanguageManager.Instance.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup1).Replace("{1}", intRCDeployGroup1.ToString());
                if (!string.IsNullOrEmpty(strRCDeployGroup2))
                    strRCTip += LanguageManager.Instance.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup2).Replace("{1}", intRCDeployGroup2.ToString());
                if (!string.IsNullOrEmpty(strRCDeployGroup3))
                    strRCTip += LanguageManager.Instance.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup3).Replace("{1}", intRCDeployGroup3.ToString());
                if (!string.IsNullOrEmpty(strRCDeployGroup4))
                    strRCTip += LanguageManager.Instance.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup4).Replace("{1}", intRCDeployGroup4.ToString());
                if (!string.IsNullOrEmpty(strRCDeployGroup5))
                    strRCTip += LanguageManager.Instance.GetString("Tip_RecoilAccessories").Replace("{0}", strRCDeployGroup5).Replace("{1}", intRCDeployGroup5.ToString());

                int intStrRC = ((_objCharacter.STR.TotalValue - 1)/3) + 1;

                intRCBase += intStrRC + 1;
                intRCFull += intStrRC + 1;
                strRCTip += $" + {_objCharacter.STR.DisplayAbbrev} [{_objCharacter.STR.TotalValue}] /3 = {intStrRC}]";
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
                    intReach += _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.Reach && objImprovement.Enabled).Sum(objImprovement => Convert.ToInt32(objImprovement.Value));
                }
                if (_strName == "Unarmed Attack")
                {
                    intReach += _objCharacter.Improvements.Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.UnarmedReach && objImprovement.Enabled).Sum(objImprovement => Convert.ToInt32(objImprovement.Value));
                }

                return intReach;
            }
        }

        /// <summary>
        /// The full Accuracy of the Weapon including modifiers from accessories.
        /// </summary>
        public string TotalAccuracy
        {
            get
            {
                string strAccuracy = _strAccuracy;
                int intAccuracy;

                if (strAccuracy.StartsWith("Physical"))
                {
                    strAccuracy = strAccuracy.Replace("Physical", _objCharacter.LimitPhysical);

                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    XPathExpression xprAccuracy = nav.Compile(strAccuracy);
                    intAccuracy = Convert.ToInt32(nav.Evaluate(xprAccuracy));
                }
                else if (strAccuracy.StartsWith("Missile"))
                {
                    strAccuracy = strAccuracy.Replace("Missile", _objCharacter.LimitPhysical);

                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    XPathExpression xprAccuracy = nav.Compile(strAccuracy);
                    intAccuracy = Convert.ToInt32(nav.Evaluate(xprAccuracy));
                }
                else
                {
                    intAccuracy = Convert.ToInt32("0" + strAccuracy);
                    foreach (WeaponAccessory wa in _lstAccessories)
                    {
                        if (wa.Name == "Laser Sight")
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
                }

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
                            if (strSpec != string.Empty)
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

                return intAccuracy.ToString();
            }
        }

        /// <summary>
        /// The slots the weapon has for modifications.
        /// </summary>
        public string ModificationSlots => _strWeaponSlots;

        /// <summary>
        /// Permanently alters the Weapon's Range category.
        /// </summary>
        /// <param name="strRange">name of the new Range category to use.</param>
        public void SetRange(string strRange)
        {
            _strRange = strRange;
        }

        /// <summary>
        /// Evalulate and return the requested Range for the Weapon.
        /// </summary>
        /// <param name="strFindRange">Range node to use.</param>
        private int Range(string strFindRange)
        {
            XmlDocument objXmlDocument = XmlManager.Instance.Load("ranges.xml");

            string strRangeCategory = _strCategory;
            if (!string.IsNullOrEmpty(_strRange))
                strRangeCategory = _strRange;

            XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[category = \"" + strRangeCategory + "\"]");
            if (objXmlCategoryNode?[strFindRange] == null)
            {
                return -1;
            }
            string strRange = objXmlCategoryNode[strFindRange].InnerText;

            XPathNavigator nav = objXmlDocument.CreateNavigator();

            int intSTR = _objCharacter.STR.TotalValue;
            int intBOD = _objCharacter.BOD.TotalValue;

            // If this is a Throwing Weapon, include the ThrowRange bonuses in the character's STR.
            if (_strCategory == "Throwing Weapons" || _strUseSkill == "Throwing Weapons")
            {
                ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
                intSTR += objImprovementManager.ValueOf(Improvement.ImprovementType.ThrowRange);
            }

            strRange = strRange.Replace("STR", intSTR.ToString());
            strRange = strRange.Replace("BOD", intBOD.ToString());

            XPathExpression xprRange = nav.Compile(strRange);

            double dblReturn = Convert.ToDouble(nav.Evaluate(xprRange).ToString(), GlobalOptions.InvariantCultureInfo) * _dblRangeMultiplier;
            int intReturn = Convert.ToInt32(Math.Ceiling(dblReturn));

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
                        Vehicle objFoundVehicle;
                        objGear = CommonFunctions.FindVehicleGear(AmmoLoaded, _objCharacter.Vehicles, out objFoundVehicle);
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
        /// Weapon's Short Range.
        /// </summary>
        public string RangeShort
        {
            get
            {
                int intRangeBonus = RangeBonus;
                int intMin = (Range("min") * (100 + intRangeBonus) + 99) / 100;
                int intMax = (Range("short") * (100 + intRangeBonus) + 99) / 100;

                if (intMin == -1 && intMax == -1)
                    return string.Empty;
                else
                    return intMin.ToString() + "-" + intMax.ToString();
            }
        }

        /// <summary>
        /// Weapon's Medium Range.
        /// </summary>
        public string RangeMedium
        {
            get
            {
                int intRangeBonus = RangeBonus;
                int intMin = (Range("short") * (100 + intRangeBonus) + 99) / 100;
                int intMax = (Range("medium") * (100 + intRangeBonus) + 99) / 100;

                if (intMin == -1 && intMax == -1)
                    return string.Empty;
                else
                    return (intMin + 1).ToString() + "-" + intMax.ToString();
            }
        }

        /// <summary>
        /// Weapon's Long Range.
        /// </summary>
        public string RangeLong
        {
            get
            {
                int intRangeBonus = RangeBonus;
                int intMin = (Range("medium") * (100 + intRangeBonus) + 99) / 100;
                int intMax = (Range("long") * (100 + intRangeBonus) + 99) / 100;

                if (intMin == -1 && intMax == -1)
                    return string.Empty;
                else
                    return (intMin + 1).ToString() + "-" + intMax.ToString();
            }
        }

        /// <summary>
        /// Weapon's Extreme Range.
        /// </summary>
        public string RangeExtreme
        {
            get
            {
                int intRangeBonus = RangeBonus;
                int intMin = (Range("long") * (100 + intRangeBonus) + 99) / 100;
                int intMax = (Range("extreme") * (100 + intRangeBonus) + 99) / 100;

                if (intMin == -1 && intMax == -1)
                    return string.Empty;
                else
                    return (intMin + 1).ToString() + "-" + intMax.ToString();
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
        public string DicePool
        {
            get
            {
                var objSkill = Skill;

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

                int intRating = intDicePool + intSmartlinkBonus + intDicePoolModifier;
                var strReturn = intRating.ToString();

                // If the character has a Specialization, include it in the Dice Pool string.
                if (objSkill != null && (objSkill.Specializations.Count > 0 && !objSkill.IsExoticSkill))
                {
                    if (objSkill.HasSpecialization(DisplayNameShort) || objSkill.HasSpecialization(_strName) || objSkill.HasSpecialization(DisplayCategory) || objSkill.HasSpecialization(_strCategory) || (!string.IsNullOrEmpty(objSkill.Specialization) && (objSkill.HasSpecialization(_strSpec) || objSkill.HasSpecialization(_strSpec2))))
                        strReturn += " (" + (intRating + 2).ToString() + ")";
                }

                return strReturn;
            }
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
                Skill objSkill = null;
                foreach (Skill objCharacterSkill in _objCharacter.SkillsSection.Skills)
                {
                    if (!objCharacterSkill.IsKnowledgeSkill && objCharacterSkill.Name == strSkill)
                    {
                        if (string.IsNullOrEmpty(strSpec) || objCharacterSkill.HasSpecialization(strSpec))
                        {
                            objSkill = objCharacterSkill;
                            break;
                        }
                    }
                }
                int intDicePool = 0;
                if (objSkill != null)
                {
                    intDicePool = objSkill.Pool;
                }

                strReturn = strSkill + " (" + intDicePool.ToString() + ")";

                if (objSkill != null && (!string.IsNullOrEmpty(objSkill.Specialization) && !objSkill.IsExoticSkill))
                {
                    if (objSkill.HasSpecialization(DisplayNameShort) || objSkill.HasSpecialization(_strName) || objSkill.HasSpecialization(DisplayCategory) || objSkill.HasSpecialization(_strCategory) || (!string.IsNullOrEmpty(objSkill.Specialization) && (objSkill.HasSpecialization(_strSpec) || objSkill.HasSpecialization(_strSpec2))))
                        strReturn += " + " + LanguageManager.Instance.GetString("String_ExpenseSpecialization") + " (2)";
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
                string strAvail = string.Empty;
                string strAvailExpr = string.Empty;
                int intAvail = 0;

                if (_strAvail.Substring(_strAvail.Length - 1, 1) == "F" || _strAvail.Substring(_strAvail.Length - 1, 1) == "R")
                {
                    strAvail = _strAvail.Substring(_strAvail.Length - 1, 1);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = _strAvail.Substring(0, _strAvail.Length - 1);
                    intAvail = Convert.ToInt32(strAvailExpr);
                }
                else
                {
                    intAvail = Convert.ToInt32(_strAvail);
                }

                // Run through the Accessories and add in their availability.
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                {
                    string strAccAvail = string.Empty;
                    int intAccAvail = 0;

                    if (!objAccessory.IncludedInWeapon)
                    {
                        if (strAccAvail.StartsWith("+") || strAccAvail.StartsWith("-"))
                        {
                            strAccAvail += objAccessory.TotalAvail;
                            if (strAccAvail.EndsWith("F"))
                                strAvail = "F";
                            if (strAccAvail.EndsWith("F") || strAccAvail.EndsWith("R"))
                                strAccAvail = strAccAvail.Substring(0, strAccAvail.Length - 1);
                            intAccAvail = Convert.ToInt32(strAccAvail);
                            intAvail += intAccAvail;
                        }
                    }
                }

                string strReturn = intAvail.ToString() + strAvail;
                // Translate the Avail string.
                strReturn = strReturn.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted"));
                strReturn = strReturn.Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

                return strReturn;
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

        private class Clip
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
