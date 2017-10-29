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
    /// A specific piece of Armor.
    /// </summary>
    public class Armor : INamedItemWithGuidAndNode
    {
        private Guid _sourceID = new Guid();
        private Guid _guiID = new Guid();
        private Guid _guiWeaponID = new Guid();
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strA = "0";
        private string _strO = "0";
        private string _strArmorCapacity = "0";
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private int _intRating = 0;
        private int _intMaxRating = 0;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strArmorName = string.Empty;
        private string _strExtra = string.Empty;
        private int _intDamage = 0;
        private bool _blnEquipped = true;
        private readonly Character _objCharacter;
        private List<ArmorMod> _lstArmorMods = new List<ArmorMod>();
        private List<Gear> _lstGear = new List<Gear>();
        private string _strNotes = string.Empty;
        private string _strLocation = string.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = true;
        private string _strAltName = string.Empty;
        private string _strAltCategory = string.Empty;
        private string _strAltPage = string.Empty;
        private bool _blnDiscountCost = false;

        #region Constructor, Create, Save, Load, and Print Methods
        public Armor(Character objCharacter)
        {
            // Create the GUID for the new piece of Armor.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create an Armor from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="cmsArmorMod">ContextMenuStrip to apply to Armor Mode TreeNodes.</param>
        /// <param name="blnSkipCost">Whether or not creating the Armor should skip the Variable price dialogue (should only be used by frmSelectArmor).</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        /// <param name="intRating">Rating of the item.</param>
        /// <param name="objWeapons">List of Weapons that added to the character's weapons.</param>
        /// <param name="blnSkipSelectForms">Whether or not to skip forms that are created for bonuses like Custom Fit (Stack).</param>
        public void Create(XmlNode objXmlArmorNode, TreeNode objNode, ContextMenuStrip cmsArmorMod, int intRating, List<Weapon> objWeapons, bool blnSkipCost = false, bool blnCreateChildren = true, bool blnSkipSelectForms = false)
        {
            objXmlArmorNode.TryGetField("id", Guid.TryParse, out _sourceID);
            objXmlArmorNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArmorNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlArmorNode.TryGetStringFieldQuickly("armor", ref _strA);
            objXmlArmorNode.TryGetStringFieldQuickly("armoroverride", ref _strO);
            _intRating = intRating;
            objXmlArmorNode.TryGetInt32FieldQuickly("rating", ref _intMaxRating);
            objXmlArmorNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlArmorNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlArmorNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArmorNode.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objXmlArmorNode["bonus"];
            _nodWirelessBonus = objXmlArmorNode["wirelessbonus"];
            _blnWirelessOn = _nodWirelessBonus != null;

            if (GlobalOptions.Language != "en-us")
            {
                XmlNode objArmorNode = MyXmlNode;
                if (objArmorNode != null)
                {
                    objArmorNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objArmorNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                XmlDocument objXmlDocument = XmlManager.Load("armor.xml");
                objArmorNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                _strAltCategory = objArmorNode?.Attributes?["translate"]?.InnerText;
            }

            // Check for a Variable Cost.
            if (blnSkipCost)
                _strCost = "0";
            else if (objXmlArmorNode["cost"] != null)
            {
                if (objXmlArmorNode["cost"].InnerText.StartsWith("Variable") && !blnSkipSelectForms)
                {
                    decimal decMin = 0.0m;
                    decimal decMax = decimal.MaxValue;
                    char[] charParentheses = { '(', ')' };
                    string strCost = objXmlArmorNode["cost"].InnerText.TrimStart("Variable", true).Trim(charParentheses);
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
                        frmSelectNumber frmPickNumber = new frmSelectNumber();
                        if (decMax > 1000000)
                            decMax = 1000000;
                        frmPickNumber.Minimum = decMin;
                        frmPickNumber.Maximum = decMax;
                        frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _strCost = frmPickNumber.SelectedValue.ToString();
                    }
                }
                else if (objXmlArmorNode["cost"].InnerText.StartsWith("Rating"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    string strCostExpression = _strCost;

                    string strCost = strCostExpression.Replace("Rating", _intRating.ToString(CultureInfo.InvariantCulture));
                    XPathExpression xprCost = nav.Compile(strCost);
                    _strCost = nav.Evaluate(xprCost).ToString();
                }
                else
                {
                    _strCost = objXmlArmorNode["cost"].InnerText;
                }
            }

            if (objXmlArmorNode["bonus"] != null && !blnSkipSelectForms)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString(), objXmlArmorNode["bonus"], false, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                    objNode.Text += " (" + ImprovementManager.SelectedValue + ")";
                }
            }

            if (objXmlArmorNode.InnerXml.Contains("<selectmodsfromcategory>") && !blnSkipSelectForms)
            {
                XmlDocument objXmlDocument = XmlManager.Load("armor.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlCategoryNode in objXmlArmorNode["selectmodsfromcategory"])
                {
                    frmSelectArmorMod frmPickArmorMod = new frmSelectArmorMod(_objCharacter);
                    frmPickArmorMod.AllowedCategories = objXmlCategoryNode.InnerText;
                    frmPickArmorMod.ExcludeGeneralCategory = true;
                    frmPickArmorMod.ShowDialog();

                    if (frmPickArmorMod.DialogResult == DialogResult.Cancel)
                        return;

                    // Locate the selected piece.
                    XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + frmPickArmorMod.SelectedArmorMod + "\"]");

                    if (objXmlMod != null)
                    {
                        ArmorMod objMod = new ArmorMod(_objCharacter);
                        List<Weapon> lstWeapons = new List<Weapon>();
                        List<TreeNode> lstWeaponNodes = new List<TreeNode>();

                        TreeNode objModNode = new TreeNode();

                        objMod.Create(objXmlMod, objModNode, intRating, lstWeapons, lstWeaponNodes, blnSkipCost);
                        objMod.Parent = this;
                        objMod.IncludedInArmor = true;
                        objMod.ArmorCapacity = "[0]";
                        objMod.Cost = "0";
                        objMod.MaximumRating = objMod.Rating;
                        _lstArmorMods.Add(objMod);

                        objModNode.ContextMenuStrip = cmsArmorMod;
                        objModNode.ForeColor = SystemColors.GrayText;
                        objNode.Nodes.Add(objModNode);
                        objNode.Expand();
                    }
                    else
                    {
                        ArmorMod objMod = new ArmorMod(_objCharacter);
                        List<Weapon> lstWeapons = new List<Weapon>();
                        List<TreeNode> lstWeaponNodes = new List<TreeNode>();

                        TreeNode objModNode = new TreeNode();

                        objMod.Name = _strName;
                        objMod.Category = "Features";
                        objMod.Avail = "0";
                        objMod.Source = _strSource;
                        objMod.Page = _strPage;
                        objMod.Parent = this;
                        objMod.IncludedInArmor = true;
                        objMod.ArmorCapacity = "[0]";
                        objMod.Cost = "0";
                        objMod.Rating = 0;
                        objMod.MaximumRating = objMod.Rating;
                        _lstArmorMods.Add(objMod);

                        objModNode.ContextMenuStrip = cmsArmorMod;
                        objModNode.ForeColor = SystemColors.GrayText;
                        objNode.Nodes.Add(objModNode);
                        objNode.Expand();
                    }
                }
            }

            // Add any Armor Mods that come with the Armor.
            if (objXmlArmorNode["mods"] != null && blnCreateChildren)
            {
                XmlDocument objXmlArmorDocument = XmlManager.Load("armor.xml");

                foreach (XmlNode objXmlArmorMod in objXmlArmorNode.SelectNodes("mods/name"))
                {
                    intRating = 0;
                    string strForceValue = string.Empty;
                    objXmlArmorMod.TryGetInt32FieldQuickly("rating", ref intRating);
                    objXmlArmorMod.TryGetStringFieldQuickly("select", ref strForceValue);

                    XmlNode objXmlMod = objXmlArmorDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + objXmlArmorMod.InnerText + "\"]");
                    if (objXmlMod != null)
                    {
                        ArmorMod objMod = new ArmorMod(_objCharacter);
                        List<Weapon> lstWeapons = new List<Weapon>();
                        List<TreeNode> lstWeaponNodes = new List<TreeNode>();

                        TreeNode objModNode = new TreeNode();

                        objMod.Create(objXmlMod, objModNode, intRating, lstWeapons, lstWeaponNodes, blnSkipCost, blnSkipSelectForms);
                        objMod.Parent = this;
                        objMod.IncludedInArmor = true;
                        objMod.ArmorCapacity = "[0]";
                        objMod.Cost = "0";
                        objMod.MaximumRating = objMod.Rating;
                        _lstArmorMods.Add(objMod);

                        objModNode.ContextMenuStrip = cmsArmorMod;
                        objModNode.ForeColor = SystemColors.GrayText;
                        objNode.Nodes.Add(objModNode);
                        objNode.Expand();
                    }
                    else
                    {
                        ArmorMod objMod = new ArmorMod(_objCharacter);
                        List<Weapon> lstWeapons = new List<Weapon>();
                        List<TreeNode> lstWeaponNodes = new List<TreeNode>();

                        TreeNode objModNode = new TreeNode();

                        objMod.Name = _strName;
                        objMod.Category = "Features";
                        objMod.Avail = "0";
                        objMod.Source = _strSource;
                        objMod.Page = _strPage;
                        objMod.Parent = this;
                        objMod.IncludedInArmor = true;
                        objMod.ArmorCapacity = "[0]";
                        objMod.Cost = "0";
                        objMod.Rating = 0;
                        objMod.MaximumRating = objMod.Rating;
                        _lstArmorMods.Add(objMod);

                        objModNode.ContextMenuStrip = cmsArmorMod;
                        objModNode.ForeColor = SystemColors.GrayText;
                        objNode.Nodes.Add(objModNode);
                        objNode.Expand();
                    }
                }
            }

            // Add any Gear that comes with the Armor.
            if (objXmlArmorNode["gears"] != null && blnCreateChildren)
            {
                XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                foreach (XmlNode objXmlArmorGear in objXmlArmorNode.SelectNodes("gears/usegear"))
                {
                    intRating = 0;
                    string strForceValue = string.Empty;
                    objXmlArmorGear.TryGetInt32FieldQuickly("rating", ref intRating);
                    objXmlArmorGear.TryGetStringFieldQuickly("select", ref strForceValue);

                    XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlArmorGear.InnerText + "\"]");
                    Gear objGear = new Gear(_objCharacter);

                    TreeNode objGearNode = new TreeNode();
                    List<Weapon> lstWeapons = new List<Weapon>();
                    List<TreeNode> lstWeaponNodes = new List<TreeNode>();

                    if (!string.IsNullOrEmpty(objXmlGear["devicerating"]?.InnerText))
                    {
                        Commlink objCommlink = new Commlink(_objCharacter);
                        objCommlink.Create(objXmlGear, objGearNode, intRating, lstWeapons, lstWeaponNodes, strForceValue, false, false, !blnSkipCost);
                        objGear = objCommlink;
                    }
                    else
                        objGear.Create(objXmlGear, objGearNode, intRating, lstWeapons, lstWeaponNodes, strForceValue, false, false, !blnSkipCost);
                    objGear.Capacity = "[0]";
                    objGear.ArmorCapacity = "[0]";
                    objGear.Cost = "0";
                    objGear.MaxRating = objGear.Rating;
                    objGear.MinRating = objGear.Rating;
                    objGear.IncludedInParent = true;
                    _lstGear.Add(objGear);

                    objGearNode.ForeColor = SystemColors.GrayText;
                    objNode.Nodes.Add(objGearNode);
                    objNode.Expand();
                }
            }

            if (objXmlArmorNode.InnerXml.Contains("<addweapon>"))
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddWeapon in objXmlArmorNode.SelectNodes("addweapon"))
                {
                    var objXmlWeapon = helpers.Guid.IsGuid(objXmlAddWeapon.InnerText)
                        ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + objXmlAddWeapon.InnerText + "\"]")
                        : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlAddWeapon.InnerText + "\"]");

                    TreeNode objGearWeaponNode = new TreeNode();
                    Weapon objGearWeapon = new Weapon(_objCharacter);
                    objGearWeapon.Create(objXmlWeapon, objGearWeaponNode, null, null);
                    objGearWeapon.ParentID = InternalId;
                    objWeapons.Add(objGearWeapon);

                    _guiWeaponID = Guid.Parse(objGearWeapon.InternalId);
                }
            }

            objNode.Text = DisplayName;
            objNode.Tag = _guiID.ToString();
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("armor");
            objWriter.WriteElementString("sourceid", _sourceID.ToString());
            objWriter.WriteElementString("guid",InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("armor", _strA);
            objWriter.WriteElementString("armoroverride", _strO);
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("armorname", _strArmorName);
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("damage", _intDamage.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("rating", _intRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("rating", _intMaxRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteStartElement("armormods");
            foreach (ArmorMod objMod in _lstArmorMods)
            {
                objMod.Save(objWriter);
            }
            objWriter.WriteEndElement();
            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    // Use the Gear's SubClass if applicable.
                    if (objGear.GetType() == typeof(Commlink))
                    {
                        Commlink objCommlink = objGear as Commlink;
                        objCommlink?.Save(objWriter);
                    }
                    else
                    {
                        objGear.Save(objWriter);
                    }
                }
                objWriter.WriteEndElement();
            }
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString());
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Check if we are copying an existing item.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
                _strLocation = string.Empty;
            }
            else
            {
                _guiID = Guid.Parse(objNode["guid"].InnerText);
                objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            }

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (objNode["sourceid"] == null)
            {
                XmlNode objArmorNode = MyXmlNode;
                if (objArmorNode != null)
                {
                    _sourceID = Guid.Parse(objArmorNode["id"].InnerText);
                }
            }
            else
            {
                _sourceID = Guid.Parse(objNode["sourceid"].InnerText);
            }
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("armor", ref _strA);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("armoroverride", ref _strO);
            objNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("armorname", ref _strArmorName);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("damage", ref _intDamage);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = _nodWirelessBonus != null;
            if (objNode.InnerXml.Contains("armormods"))
            {
                XmlNodeList nodMods = objNode.SelectNodes("armormods/armormod");
                foreach (XmlNode nodMod in nodMods)
                {
                    ArmorMod objMod = new ArmorMod(_objCharacter);
                    objMod.Load(nodMod, blnCopy);
                    objMod.Parent = this;
                    _lstArmorMods.Add(objMod);
                }
            }
            if (objNode.InnerXml.Contains("gears"))
            {
                XmlNodeList nodGears = objNode.SelectNodes("gears/gear");
                foreach (XmlNode nodGear in nodGears)
                {
                    if (nodGear["iscommlink"]?.InnerText == System.Boolean.TrueString || (nodGear["category"].InnerText == "Commlinks" ||
                        nodGear["category"].InnerText == "Commlink Accessories" || nodGear["category"].InnerText == "Cyberdecks" || nodGear["category"].InnerText == "Rigger Command Consoles"))
                    {
                        Gear objCommlink = new Commlink(_objCharacter);
                        objCommlink.Load(nodGear, blnCopy);
                        _lstGear.Add(objCommlink);
                    }
                    else
                    {
                        Gear objGear = new Gear(_objCharacter);
                        objGear.Load(nodGear, blnCopy);
                        _lstGear.Add(objGear);
                    }
                }
            }

            if (GlobalOptions.Language != "en-us")
            {
                XmlNode objArmorNode = MyXmlNode;
                if (objArmorNode != null)
                {
                    objArmorNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objArmorNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                XmlDocument objXmlArmorDocument = XmlManager.Load("armor.xml");
                objArmorNode = objXmlArmorDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                _strAltCategory = objArmorNode?.Attributes?["translate"]?.InnerText;
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("armor");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("name_english", _strName);
            objWriter.WriteElementString("category", DisplayCategory);
            objWriter.WriteElementString("category_english", _strCategory);
            objWriter.WriteElementString("armor", TotalArmor.ToString());
            objWriter.WriteElementString("avail", TotalAvail);
            objWriter.WriteElementString("cost", TotalCost.ToString());
            objWriter.WriteElementString("owncost", OwnCost.ToString());
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("armorname", _strArmorName);
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteStartElement("armormods");
            foreach (ArmorMod objMod in _lstArmorMods)
            {
                objMod.Print(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
            {
                // Use the Gear's SubClass if applicable.
                if (objGear.GetType() == typeof(Commlink))
                {
                    Commlink objCommlink = objGear as Commlink;
                    objCommlink?.Print(objWriter);
                }
                else
                {
                    objGear.Print(objWriter);
                }
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(_strExtra));
            objWriter.WriteElementString("location", _strLocation);
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this piece of Armor in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }

        /// <summary>
        /// Name of the Armor.
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
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get
            {
                return _nodBonus;
            }
            set
            {
                _nodBonus = value;
            }
        }

        /// <summary>
        /// Wireless Bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get
            {
                return _nodWirelessBonus;
            }
            set
            {
                _nodWirelessBonus = value;
            }
        }

        /// <summary>
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        public string Extra
        {
            get
            {
                return _strExtra;
            }
            set
            {
                _strExtra = value;
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
        /// Armor's Category.
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
        /// Armor's Armor value.
        /// </summary>
        public string ArmorValue
        {
            get
            {
                return _strA;
            }
            set
            {
                _strA = value;
            }
        }

        /// <summary>
        /// Armor's Armor Override value.
        /// </summary>
        public string ArmorOverrideValue
        {
            get
            {
                return _strO;
            }
            set
            {
                _strO = value;
            }
        }

        /// <summary>
        /// Damage done to the Armor's Armor Rating.
        /// </summary>
        public int ArmorDamage
        {
            get
            {
                return _intDamage;
            }
            set
            {
                _intDamage = value;

                int intTotalArmor = Convert.ToInt32(_strA);

                // Go through all of the Mods for this piece of Armor and add the Armor value.
                foreach (ArmorMod objMod in _lstArmorMods)
                {
                    if (objMod.Equipped)
                        intTotalArmor += objMod.Armor;
                }

                if (_intDamage < 0)
                    _intDamage = 0;
                if (_intDamage > intTotalArmor)
                    _intDamage = intTotalArmor;
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get
            {
                return _intRating;
            }
            set
            {
                _intRating = value;
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int MaxRating
        {
            get
            {
                return _intMaxRating;
            }
            set
            {
                _intMaxRating = value;
            }
        }

        /// <summary>
        /// Armor's Capacity.
        /// </summary>
        public string ArmorCapacity
        {
            get
            {
                if (_strArmorCapacity.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = _strArmorCapacity.Contains('[');
                    string strCapacity = _strArmorCapacity;
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

                    string strReturn = Convert.ToDecimal(nav.Evaluate(xprCapacity)).ToString("N2", GlobalOptions.CultureInfo);
                    if (blnSquareBrackets)
                        strReturn = "[" + strReturn + "]";

                    return strReturn;
                }
                else
                {
                    decimal decReturn;
                    if (decimal.TryParse(_strArmorCapacity, out decReturn))
                        return decReturn.ToString("N2", GlobalOptions.CultureInfo);
                    return _strArmorCapacity;
                }
            }
            set
            {
                _strArmorCapacity = value;
            }
        }

        /// <summary>
        /// Armor's Availability.
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
        /// Armor's Cost.
        /// </summary>
        public decimal Cost
        {
            get
            {
                if (_strCost.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    string strCost = _strCost;
                    if (strCost.Contains('['))
                        strCost = strCost.Substring(1, strCost.Length - 2);
                    XPathExpression xprCapacity = nav.Compile(strCost.Replace("Rating", _intRating.ToString()));

                    return Convert.ToDecimal(nav.Evaluate(xprCapacity), GlobalOptions.InvariantCultureInfo);
                }
                else
                {
                    return Convert.ToDecimal(_strCost, GlobalOptions.InvariantCultureInfo);
                }
            }
            set
            {
                _strCost = value.ToString();
            }
        }

        /// <summary>
        /// Armor's Sourcebook.
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
        /// Guid of a Weapon created from the Armour.
        /// </summary>
        public string WeaponID
        {
            get
            {
                return _guiWeaponID.ToString();
            }
            set
            {
                _guiWeaponID = Guid.Parse(value);
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
        /// Whether or not the Armor is equipped and should be considered for highest Armor Rating or Armor Encumbrance.
        /// </summary>
        public bool Equipped
        {
            get
            {
                return _blnEquipped;
            }
            set
            {
                _blnEquipped = value;
            }
        }

        /// <summary>
        /// Whether or not Wireless is turned on for this armor
        /// </summary>
        public bool WirelessOn
        {
            get
            {
                return _blnWirelessOn;
            }
            set
            {
                _blnWirelessOn = value;
            }
        }

        /// <summary>
        /// The Armor's total Armor value including Modifications.
        /// </summary>
        public int TotalArmor
        {
            get
            {
                bool blnUseBase = false;
                bool blnCustomFitted = false;
                bool blnHighest = true;
                int intOverride = 0;

                foreach (Armor a in _objCharacter.Armor.Where(a => a.Equipped))
                {
                    if (a.ArmorValue.Substring(0, 1) != "+")
                        blnUseBase = true;
                    if (Convert.ToInt32(a.ArmorOverrideValue) > 0)
                        intOverride += 1;
                    if (a.Name != _strName)
                    {
                        if (Convert.ToInt32(a.ArmorOverrideValue) > Convert.ToInt32(_strO))
                            blnHighest = false;
                    }
                    if (a.Name == _strName)
                    {
                        //Check for Custom Fitted armour
                        foreach (ArmorMod objMod in a.ArmorMods.Where(objMod => objMod.Name == "Custom Fit (Stack)" && (objMod.Extra.Length > 0)).Where(objMod => _objCharacter.Armor.Any(objArmor => objArmor.Equipped && objMod.Extra == objArmor.Name)))
                        {
                            blnCustomFitted = true;
                        }
                    }
                }

                if (!blnHighest || Convert.ToInt32(_strO) == 0)
                    blnUseBase = true;

                int intTotalArmor;
                int.TryParse(_strA.Replace("Rating", _intRating.ToString()), out intTotalArmor);
                // if there's zero or usebase is true, we're all done. Calculate as normal.
                if (blnCustomFitted || (!blnUseBase && intOverride > 1 && !blnHighest))
                {
                    int.TryParse(_strO, out intTotalArmor);
                }

                // Go through all of the Mods for this piece of Armor and add the Armor value.
                foreach (ArmorMod objMod in _lstArmorMods)
                {
                    if (objMod.Equipped)
                        intTotalArmor += objMod.Armor;
                }

                intTotalArmor -= _intDamage;

                return intTotalArmor;
            }
        }

        /// <summary>
        /// The Armor's total Cost including Modifications.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decTotalCost = 0.0m;
                if (_strCost.Contains("Rating"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    string strCostExpression = _strCost;

                    string strCost = strCostExpression.Replace("Rating", _intRating.ToString());
                    XPathExpression xprCost = nav.Compile(strCost);
                    decTotalCost = Convert.ToDecimal(nav.Evaluate(xprCost).ToString(), GlobalOptions.InvariantCultureInfo);
                }
                else
                {
                    decTotalCost = Convert.ToDecimal(_strCost, GlobalOptions.InvariantCultureInfo);
                }
                if (DiscountCost)
                    decTotalCost *= 0.9m;

                // Go through all of the Mods for this piece of Armor and add the Cost value.
                foreach (ArmorMod objMod in _lstArmorMods)
                    decTotalCost += objMod.TotalCost;

                // Go through all of the Gear for this piece of Armor and add the Cost value.
                foreach (Gear objGear in _lstGear)
                    decTotalCost += objGear.TotalCost;

                return decTotalCost;
            }
        }

        /// <summary>
        /// Cost for just the Armor.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                decimal decTotalCost = 0.0m;
                if (_strCost.Contains("Rating"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    string strCostExpression = _strCost;

                    string strCost = strCostExpression.Replace("Rating", _intRating.ToString());
                    XPathExpression xprCost = nav.Compile(strCost);
                    decTotalCost = Convert.ToDecimal(nav.Evaluate(xprCost).ToString(), GlobalOptions.InvariantCultureInfo);
                }
                else
                {
                    decTotalCost = Convert.ToDecimal(_strCost, GlobalOptions.InvariantCultureInfo);
                }

                if (DiscountCost)
                    decTotalCost *= 0.9m;

                return decTotalCost;
            }
        }

        /// <summary>
        /// The Modifications currently applied to the Armor.
        /// </summary>
        public List<ArmorMod> ArmorMods
        {
            get
            {
                return _lstArmorMods;
            }
        }

        /// <summary>
        /// The Gear currently applied to the Armor.
        /// </summary>
        public List<Gear> Gear
        {
            get
            {
                return _lstGear;
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
        public Guid SourceID
        {
            get
            {
                return _sourceID;
            }
        }
        #endregion

        #region Complex Properties
        private static char[] chrAvails = { 'F', 'R' };
        /// <summary>
        /// Total Availablility of the Armor and its Modifications and Gear.
        /// </summary>
        public string TotalAvail
        {
            get
            {
                // If the Avail contains "+", return the base string and don't try to calculate anything since we're looking at a child component.
                if (_strAvail.Contains('+'))
                    return _strAvail;

                string strCalculated;

                // Just a straight cost, so return the value.
                if (_strAvail.EndsWith("F") || _strAvail.EndsWith("R"))
                {
                    strCalculated = Convert.ToInt32(_strAvail.Substring(0, _strAvail.Length - 1)).ToString() + _strAvail.Substring(_strAvail.Length - 1, 1);
                }
                else
                    strCalculated = Convert.ToInt32(_strAvail).ToString();

                int intAvail;
                string strAvailText = string.Empty;
                if (strCalculated.EndsWith("F") || strCalculated.EndsWith("R"))
                {
                    strAvailText = strCalculated.Substring(strCalculated.Length - 1);
                    intAvail = Convert.ToInt32(strCalculated.Substring(0, strCalculated.Length - 1));
                }
                else
                    intAvail = Convert.ToInt32(strCalculated);

                // Run through the child items and increase the Avail by any Mod whose Avail contains "+".
                foreach (Gear objChild in _lstGear)
                {
                    if (objChild.Avail.Contains('+') && !objChild.IncludedInParent)
                    {
                        if (objChild.Avail.Contains("Rating"))
                        {
                            // If the cost is determined by the Rating, evaluate the expression.
                            XmlDocument objXmlDocument = new XmlDocument();
                            XPathNavigator nav = objXmlDocument.CreateNavigator();

                            string strAvailExpression = (objChild.Avail);

                            string strAvailability = strAvailExpression.Replace("Rating", objChild.Rating.ToString());
                            if (strAvailability.EndsWith("R") || strAvailability.EndsWith("F"))
                            {
                                if (strAvailText != "F")
                                    strAvailText = objChild.Avail.Substring(strAvailability.Length - 1);
                                strAvailability = strAvailability.Substring(0, strAvailability.Length - 1);
                            }
                            if (strAvailability.StartsWith("+"))
                                strAvailability = strAvailability.Substring(1);
                            XPathExpression xprCost = nav.Compile(strAvailability);
                            intAvail += Convert.ToInt32(nav.Evaluate(xprCost));
                        }
                        else
                        {
                            if (objChild.Avail.EndsWith("R") || objChild.Avail.EndsWith("F"))
                            {
                                if (strAvailText != "F")
                                    strAvailText = objChild.Avail.Substring(objChild.Avail.Length - 1);
                                intAvail += Convert.ToInt32(objChild.Avail.Substring(0, objChild.Avail.Length - 1));
                            }
                            else
                                intAvail += Convert.ToInt32(objChild.Avail);
                        }
                    }
                }

                // Run through the child items and increase the Avail by any Mod whose Avail contains "+".
                foreach (ArmorMod objChild in _lstArmorMods)
                {
                    if (objChild.Avail.Contains('+') && !objChild.IncludedInArmor)
                    {
                        if (objChild.Avail.EndsWith("R") || objChild.Avail.EndsWith("F"))
                        {
                            if (strAvailText != "F")
                                strAvailText = objChild.Avail.Substring(objChild.Avail.Length - 1);
                            intAvail += Convert.ToInt32(objChild.Avail.Substring(0, objChild.Avail.Length - 1));
                        }
                        else
                            intAvail += Convert.ToInt32(objChild.Avail);
                    }
                }

                // Translate the Avail string.
                if (strAvailText == "R")
                    strAvailText = LanguageManager.GetString("String_AvailRestricted");
                else if (strAvailText == "F")
                    strAvailText = LanguageManager.GetString("String_AvailForbidden");

                string strReturn = intAvail.ToString() + strAvailText;

                return strReturn;
            }
        }

        /// <summary>
        /// Calculated Capacity of the Armor.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strReturn;

                // If an Armor Capacity is specified for the Armor, use that value. Otherwise, use the higher of 6 or (Highest Armor Rating * 1.5, round up).
                if (string.IsNullOrEmpty(_strArmorCapacity) || _strArmorCapacity == "0")
                {
                    // This is only calculated if the Maximum Armor Modification rule is enabled.
                    if (_objCharacter.Options.MaximumArmorModifications)
                    {
                        int intA = (3 * Convert.ToInt32(_strA, GlobalOptions.CultureInfo) + 1) / 2;
                        strReturn = Math.Max(intA, 6).ToString();
                    }
                    else
                        strReturn = "0";
                }
                else if (_strArmorCapacity == "Rating")
                {
                    strReturn = _intRating.ToString();
                }
                else
                {
                    decimal decReturn;
                    if (decimal.TryParse(_strArmorCapacity, out decReturn))
                        strReturn = decReturn.ToString("N2", GlobalOptions.CultureInfo);
                    else
                        strReturn = _strArmorCapacity;
                }

                foreach (ArmorMod objArmorMod in ArmorMods)
                {
                    if (objArmorMod.ArmorCapacity.StartsWith("-") || objArmorMod.ArmorCapacity.StartsWith("[-"))
                    {
                        // If the Capaicty is determined by the Capacity of the parent, evaluate the expression. Generally used for providing a percentage of armour capacity as bonus, ie YNT Softweave.
                        XmlDocument objXmlDocument = new XmlDocument();
                        XPathNavigator nav = objXmlDocument.CreateNavigator();

                        // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                        string strCapacity = objArmorMod.ArmorCapacity;
                        strCapacity = strCapacity.Replace("[-", string.Empty);
                        strCapacity = strCapacity.FastEscape("[]".ToCharArray());
                        strCapacity = strCapacity.Replace("Capacity", _strArmorCapacity);
                        strCapacity = strCapacity.Replace("Rating", _intRating.ToString());
                        XPathExpression xprCapacity = nav.Compile(strCapacity);

                        strCapacity = nav.Evaluate(xprCapacity).ToString();
                        strCapacity = (Convert.ToDecimal(strCapacity) + Convert.ToDecimal(strReturn)).ToString("N2", GlobalOptions.CultureInfo);
                        strReturn = strCapacity;
                    }
                }

                return strReturn;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Gear.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                // Get the Armor base Capacity.
                decimal decCapacity = Convert.ToDecimal(CalculatedCapacity, GlobalOptions.CultureInfo);

                // If there is no Capacity (meaning that the Armor Suit Capacity or Maximum Armor Modification rule is turned off depending on the type of Armor), don't bother to calculate the remaining
                // Capacity since it's disabled and return 0 instead.
                if (decCapacity == 0)
                    return 0;

                // Calculate the remaining Capacity for a Suit of Armor.
                if (_strArmorCapacity != "0" && !string.IsNullOrEmpty(_strArmorCapacity)) // && _objCharacter.Options.ArmorSuitCapacity)
                {
                    // Run through its Armor Mods and deduct the Capacity costs.
                    foreach (ArmorMod objMod in _lstArmorMods)
                    {
                        bool blnSoftweave = false;
                        if (objMod.Bonus != null)
                        {
                            blnSoftweave = objMod.Bonus.SelectSingleNode("softweave") != null;
                        }
                        if (objMod.WirelessOn && objMod.WirelessBonus != null)
                        {
                            blnSoftweave = objMod.WirelessBonus.SelectSingleNode("softweave") != null;
                        }
                        if (blnSoftweave) continue;
                        string strCapacity = objMod.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                        {
                            // If this is a multiple-capacity item, use only the second half.
                            int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                            strCapacity = strCapacity.Substring(intPos + 1);
                        }

                        if (strCapacity.Contains('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }

                    // Run through its Gear and deduct the Armor Capacity costs.
                    foreach (Gear objGear in _lstGear)
                    {
                        string strCapacity = objGear.CalculatedArmorCapacity;
                        if (strCapacity.Contains("/["))
                        {
                            // If this is a multiple-capacity item, use only the second half.
                            int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                            strCapacity = strCapacity.Substring(intPos + 1);
                        }

                        if (strCapacity.Contains('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }
                }

                // Calculate the remaining Capacity for a standard piece of Armor using the Maximum Armor Modifications rules.
                if (string.IsNullOrEmpty(_strArmorCapacity) || _strArmorCapacity == "0") // && _objCharacter.Options.MaximumArmorModifications)
                {
                    // Run through its Armor Mods and deduct the Rating (or 1 if it has no Rating).
                    foreach (ArmorMod objMod in _lstArmorMods)
                    {
                        if (objMod.Rating > 0)
                            decCapacity -= objMod.Rating;
                        else
                            decCapacity -= 1;
                    }

                    // Run through its Gear and deduct the Rating (or 1 if it has no Rating).
                    foreach (Gear objGear in _lstGear)
                    {
                        if (objGear.Rating > 0)
                            decCapacity -= objGear.Rating;
                        else
                            decCapacity -= 1;
                    }
                }

                return decCapacity;
            }
        }

        /// <summary>
        /// Capacity display style;
        /// </summary>
        public CapacityStyle CapacityDisplayStyle
        {
            get
            {
                CapacityStyle objReturn = CapacityStyle.Zero;
                if (string.IsNullOrEmpty(_strArmorCapacity) || _strArmorCapacity == "0") // && _objCharacter.Options.MaximumArmorModifications)
                    objReturn = CapacityStyle.PerRating;
                if (!string.IsNullOrEmpty(_strArmorCapacity) && _strArmorCapacity != "0") // && _objCharacter.Options.ArmorSuitCapacity)
                    objReturn = CapacityStyle.Standard;

                return objReturn;
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
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = DisplayNameShort;

                if (!string.IsNullOrEmpty(_strArmorName))
                    strReturn += " (\"" + _strArmorName + "\")";
                if (_intRating > 0)
                    strReturn += " (" + LanguageManager.GetString("String_Rating") + " " + _intRating.ToString() + ")";
                if (!string.IsNullOrEmpty(_strExtra))
                    strReturn += " (" + LanguageManager.TranslateExtra(_strExtra) + ")";
                return strReturn;
            }
        }

        /// <summary>
        /// A custom name for the Armor assigned by the player.
        /// </summary>
        public string ArmorName
        {
            get
            {
                return _strArmorName;
            }
            set
            {
                _strArmorName = value;
            }
        }

        public XmlNode MyXmlNode
        {
            get
            {
                return XmlManager.Load("armor.xml")?.SelectSingleNode("/chummer/armors/armor[name = \"" + Name + "\"]");
            }
        }
        #endregion
    }
}
