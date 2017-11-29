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
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A piece of Armor Modification.
    /// </summary>
    public class ArmorMod : INamedItemWithGuidAndNode
    {
        private Guid _guiID = new Guid();
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strArmorCapacity = "[0]";
        private string _strGearCapacity = string.Empty;
        private int _intA = 0;
        private int _intMaxRating = 0;
        private int _intRating = 0;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludedInArmor = false;
        private bool _blnEquipped = true;
        private string _strExtra = string.Empty;
        private Guid _guiWeaponID = new Guid();
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = true;
        private readonly Character _objCharacter;
        private List<Gear> _lstGear = new List<Gear>();
        private string _strNotes = string.Empty;
        private string _strAltName = string.Empty;
        private string _strAltCategory = string.Empty;
        private string _strAltPage = string.Empty;
        private bool _blnDiscountCost = false;
        private Armor _objParent;

        #region Constructor, Create, Save, Load, and Print Methods
        public ArmorMod(Character objCharacter)
        {
            // Create the GUID for the new Armor Mod.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Armor Modification from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="intRating">Rating of the selected ArmorMod.</param>
        /// <param name="objWeapons">List of Weapons that are created by the Armor.</param>
        /// <param name="objWeaponNodes">List of Weapon Nodes that are created by the Armor.</param>
        /// <param name="blnSkipCost">Whether or not creating the Armor should skip the Variable price dialogue (should only be used by frmSelectArmor).</param>
        public void Create(XmlNode objXmlArmorNode, TreeNode objNode, ContextMenuStrip cmsArmorGear, int intRating, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, bool blnSkipCost = false, bool blnSkipSelectForms = false)
        {
            objXmlArmorNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArmorNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlArmorNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlArmorNode.TryGetStringFieldQuickly("gearcapacity", ref _strGearCapacity);
            _intRating = intRating;
            objXmlArmorNode.TryGetInt32FieldQuickly("armor", ref _intA);
            objXmlArmorNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
            objXmlArmorNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlArmorNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArmorNode.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objXmlArmorNode["bonus"];
            _nodWirelessBonus = objXmlArmorNode["wirelessbonus"];
            _blnWirelessOn = _nodWirelessBonus != null;

            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
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
                XmlNode objXmlArmorCostNode = objXmlArmorNode["cost"];

                if (objXmlArmorCostNode.InnerText.StartsWith("Variable"))
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
                        _strCost = frmPickNumber.SelectedValue.ToString();
                    }
                }
                else
                {
                    _strCost = objXmlArmorCostNode.InnerText;
                }
            }

            if (objXmlArmorNode["bonus"] != null && !blnSkipCost && !blnSkipSelectForms)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, _guiID.ToString(), objXmlArmorNode["bonus"], false, intRating, DisplayNameShort))
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

            // Add any Gear that comes with the Armor.
            if (objXmlArmorNode["gears"] != null)
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
                        objCommlink.Create(objXmlGear, objGearNode, intRating, lstWeapons, lstWeaponNodes, strForceValue, false, false, !blnSkipCost && !blnSkipSelectForms);
                        objGear = objCommlink;
                    }
                    else
                        objGear.Create(objXmlGear, objGearNode, intRating, lstWeapons, lstWeaponNodes, strForceValue, false, false, !blnSkipCost && !blnSkipSelectForms);
                    objGear.Capacity = "[0]";
                    objGear.ArmorCapacity = "[0]";
                    objGear.Cost = "0";
                    objGear.MaxRating = objGear.Rating;
                    objGear.MinRating = objGear.Rating;
                    objGear.ParentID = InternalId;
                    _lstGear.Add(objGear);

                    objGearNode.ForeColor = SystemColors.GrayText;
                    objGearNode.ContextMenuStrip = cmsArmorGear;
                    objNode.Nodes.Add(objGearNode);
                    objNode.Expand();
                }
            }

            // Add Weapons if applicable.
            if (objXmlArmorNode.InnerXml.Contains("<addweapon>"))
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddWeapon in objXmlArmorNode.SelectNodes("addweapon"))
                {
                    var objXmlWeapon = helpers.Guid.IsGuid(objXmlAddWeapon.InnerText)
                        ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + objXmlAddWeapon.InnerText + "\" and starts-with(category, \"Cyberware\")]")
                        : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlAddWeapon.InnerText + "\" and starts-with(category, \"Cyberware\")]");

                    List<TreeNode> lstGearWeaponNodes = new List<TreeNode>();
                    Weapon objGearWeapon = new Weapon(_objCharacter);
                    objGearWeapon.Create(objXmlWeapon, lstGearWeaponNodes, null, null, objWeapons, null, true, !blnSkipCost && !blnSkipSelectForms);
                    objGearWeapon.ParentID = InternalId;
                    foreach (TreeNode objLoopNode in lstGearWeaponNodes)
                    {
                        objLoopNode.ForeColor = SystemColors.GrayText;
                        objWeaponNodes.Add(objLoopNode);
                    }
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
            objWriter.WriteStartElement("armormod");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("armor", _intA.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("gearcapacity", _strGearCapacity);
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("rating", _intRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
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
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("included", _blnIncludedInArmor.ToString());
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("extra", _strExtra);
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString());
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether or not we are copying an existing node.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
            }
            else
            {
                _guiID = Guid.Parse(objNode["guid"].InnerText);
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetInt32FieldQuickly("armor", ref _intA);
            objNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objNode.TryGetStringFieldQuickly("gearcapacity", ref _strGearCapacity);
            objNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludedInArmor);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = _nodWirelessBonus != null;
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            if (objNode["weaponguid"] != null)
            {
                _guiWeaponID = Guid.Parse(objNode["weaponguid"].InnerText);
            }
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);

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
            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
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
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture)
        {
            objWriter.WriteStartElement("armormod");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("name_english", _strName);
            objWriter.WriteElementString("category", DisplayCategory);
            objWriter.WriteElementString("category_english", _strCategory);
            objWriter.WriteElementString("armor", _intA.ToString(objCulture));
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(objCulture));
            objWriter.WriteElementString("rating", _intRating.ToString(objCulture));
            objWriter.WriteElementString("avail", TotalAvail);
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("included", _blnIncludedInArmor.ToString());
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in _lstGear)
            {
                // Use the Gear's SubClass if applicable.
                Commlink objCommlink = objGear as Commlink;
                if (objCommlink != null)
                {
                    objCommlink.Print(objWriter, objCulture);
                }
                else
                {
                    objGear.Print(objWriter, objCulture);
                }
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(_strExtra));
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
        /// Guid of a Cyberware Weapon.
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
        /// Name of the Mod.
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
        /// The name of the object as it should be displayed on printouts (translated name only).
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

                if (_intRating > 0)
                    strReturn += " (" + LanguageManager.GetString("String_Rating") + " " + _intRating.ToString() + ")";
                if (!string.IsNullOrEmpty(_strExtra))
                    strReturn += " (" + LanguageManager.TranslateExtra(_strExtra) + ")";
                return strReturn;
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
        /// Special Armor Mod Category.
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
        /// Mod's Armor value modifier.
        /// </summary>
        public int Armor
        {
            get
            {
                return _intA;
            }
            set
            {
                _intA = value;
            }
        }

        /// <summary>
        /// Armor capacity.
        /// </summary>
        public string ArmorCapacity
        {
            get
            {
                return _strArmorCapacity;
            }
            set
            {
                _strArmorCapacity = value;
            }
        }

        /// <summary>
        /// Capacity for gear plugins.
        /// </summary>
        public string GearCapacity
        {
            get
            {
                return _strGearCapacity;
            }
            set
            {
                _strGearCapacity = value;
            }
        }

        /// <summary>
        /// Mod's Maximum Rating.
        /// </summary>
        public int MaximumRating
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
        /// Mod's current Rating.
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
        /// Mod's Availability.
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
        /// The Mod's cost.
        /// </summary>
        public string Cost
        {
            get
            {
                if (_strCost.Contains("Rating"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    string strCostExpression = _strCost;

                    string strCost = strCostExpression.Replace("Rating", _intRating.ToString());
                    return CommonFunctions.EvaluateInvariantXPath(strCost).ToString();
                }
                else
                {
                    // Just a straight cost, so return the value.
                    return _strCost;
                }
            }
            set
            {
                _strCost = value;
            }
        }

        /// <summary>
        /// Mod's Sourcebook.
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
        /// Whether or not an Armor Mod is equipped and should be included in the Armor's totals.
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
        /// Whether or not an Armor Mod's wireless bonus is enabled
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
        /// Whether or not this Mod is part of the base Armor configuration.
        /// </summary>
        public bool IncludedInArmor
        {
            get
            {
                return _blnIncludedInArmor;
            }
            set
            {
                _blnIncludedInArmor = value;
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
        /// Value that was selected during the Improvement Manager dialogue.
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
        /// Whether or not the Armor Mod's cost should be discounted by 10% through the Black Market Pipeline Quality.
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
        /// Parent Armor.
        /// </summary>
        public Armor Parent
        {
            get
            {
                return _objParent;
            }
            set
            {
                _objParent = value;
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
        #endregion

        #region Complex Properties
        /// <summary>
        /// Total Availablility of the Gear and its accessories.
        /// </summary>
        public string TotalAvail
        {
            get
            {
                // If the Avail contains "+", return the base string and don't try to calculate anything since we're looking at a child component.
                if (_strAvail.Contains('+'))
                    return _strAvail;

                string strCalculated;

                if (_strAvail.Contains("Rating"))
                {
                    // If the availability is determined by the Rating, evaluate the expression.
                    string strAvail = string.Empty;
                    string strAvailExpr = _strAvail;

                    if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                    {
                        strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                        // Remove the trailing character if it is "F" or "R".
                        strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                    }
                    strCalculated = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", _intRating.ToString()))).ToString() + strAvail;
                }
                else
                {
                    // Just a straight cost, so return the value.
                    if (_strAvail.EndsWith('F') || _strAvail.EndsWith('R'))
                    {
                        strCalculated = Convert.ToInt32(_strAvail.Substring(0, _strAvail.Length - 1)).ToString() + _strAvail.Substring(_strAvail.Length - 1, 1);
                    }
                    else
                        strCalculated = Convert.ToInt32(_strAvail).ToString();
                }

                int intAvail;
                string strAvailText = string.Empty;
                if (strCalculated.EndsWith('F') || strCalculated.EndsWith('R'))
                {
                    strAvailText = strCalculated.Substring(strCalculated.Length - 1);
                    // Translate the Avail string.
                    if (strAvailText == "R")
                        strAvailText = LanguageManager.GetString("String_AvailRestricted");
                    else if (strAvailText == "F")
                        strAvailText = LanguageManager.GetString("String_AvailForbidden");
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
                            string strAvailExpression = (objChild.Avail);

                            string strAvailability = strAvailExpression.Replace("Rating", objChild.Rating.ToString());
                            if (strAvailability.EndsWith('R') || strAvailability.EndsWith('F'))
                            {
                                if (strAvailText != "F")
                                    strAvailText = objChild.Avail.Substring(strAvailability.Length - 1);
                                strAvailability = strAvailability.Substring(0, strAvailability.Length - 1);
                            }
                            if (strAvailability.StartsWith('+'))
                                strAvailability = strAvailability.Substring(1);
                            intAvail += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailability));
                        }
                        else
                        {
                            if (objChild.Avail.EndsWith('R') || objChild.Avail.EndsWith('F'))
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

                string strReturn = intAvail.ToString() + strAvailText;

                return strReturn;
            }
        }

        /// <summary>
        /// Caculated Gear Capacity of the Armor Mod.
        /// </summary>
        public string CalculatedGearCapacity
        {
            get
            {
                if (string.IsNullOrEmpty(_strGearCapacity))
                    return "0";
                string strCapacity = _strGearCapacity;
                if (strCapacity.StartsWith("FixedValues"))
                {
                    string[] strValues = strCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    strCapacity = strValues[Math.Min(_intRating, strValues.Length) - 1];
                }
                strCapacity = strCapacity.CheapReplace("Capacity", () => Convert.ToDecimal(_objParent.ArmorCapacity, GlobalOptions.CultureInfo).ToString(GlobalOptions.InvariantCultureInfo));
                strCapacity = strCapacity.Replace("Rating", _intRating.ToString());

                //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                string strReturn = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity)).ToString("#,0.##", GlobalOptions.CultureInfo);

                return strReturn;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Gear.
        /// </summary>
        public decimal GearCapacityRemaining
        {
            get
            {
                decimal decCapacity = 0;
                string strMyCapacity = CalculatedGearCapacity;
                // Get the Gear base Capacity.
                if (strMyCapacity.Contains("/["))
                {
                    // If this is a multiple-capacity item, use only the first half.
                    int intPos = strMyCapacity.IndexOf("/[");
                    strMyCapacity = strMyCapacity.Substring(0, intPos);
                    decCapacity = Convert.ToDecimal(strMyCapacity, GlobalOptions.CultureInfo);
                }
                else
                    decCapacity = Convert.ToDecimal(strMyCapacity, GlobalOptions.CultureInfo);

                // Run through its Children and deduct the Capacity costs.
                foreach (Gear objChildGear in Gear)
                {
                    string strCapacity = objChildGear.CalculatedCapacity;
                    if (strCapacity.Contains("/["))
                    {
                        // If this is a multiple-capacity item, use only the second half.
                        int intPos = strCapacity.IndexOf("/[");
                        strCapacity = strCapacity.Substring(intPos + 1);
                    }

                    // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                    if (strCapacity.Contains('['))
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    else
                        strCapacity = "0";
                    decCapacity -= (Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo) * objChildGear.Quantity);
                }

                return decCapacity;
            }
        }

        /// <summary>
        /// Caculated Capacity of the Armor Mod.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                if (string.IsNullOrEmpty(_strArmorCapacity))
                    return "0";
                string strCapacity = _strArmorCapacity;
                if (strCapacity.StartsWith("FixedValues"))
                {
                    string[] strValues = strCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    strCapacity = strValues[Math.Min(_intRating, strValues.Length) - 1];
                }
                strCapacity = strCapacity.CheapReplace("Capacity", () => Convert.ToDecimal(_objParent.ArmorCapacity, GlobalOptions.CultureInfo).ToString(GlobalOptions.InvariantCultureInfo));
                strCapacity = strCapacity.Replace("Rating", _intRating.ToString());
                bool blnSquareBrackets = strCapacity.Contains('[');
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                string strReturn = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity)).ToString("#,0.##", GlobalOptions.CultureInfo);
                if (blnSquareBrackets)
                    strReturn = "[" + strReturn + "]";

                return strReturn;
            }
        }

        /// <summary>
        /// Total cost of the Armor Mod.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decReturn = 0.0m;
                string strCostExpr = _strCost;
                if (strCostExpr.StartsWith("FixedValues"))
                {
                    string[] strValues = strCostExpr.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (_intRating > 0)
                        strCostExpr = strValues[Math.Min(_intRating, strValues.Length) - 1];
                }
                decimal decArmorCost = 0.0m;
                if (_objParent != null)
                    decArmorCost = _objParent.Cost;

                if (strCostExpr.Contains("Armor Cost") || strCostExpr.Contains("Rating"))
                {
                    strCostExpr = strCostExpr.Replace("Armor Cost", decArmorCost.ToString(GlobalOptions.InvariantCultureInfo));
                    strCostExpr = strCostExpr.Replace("Rating", _intRating.ToString());
                    decReturn = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCostExpr), GlobalOptions.InvariantCultureInfo);
                }
                else
                    decReturn = Convert.ToDecimal(_strCost, GlobalOptions.InvariantCultureInfo);

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Go through all of the Gear for this piece of Armor and add the Cost value.
                foreach (Gear objGear in _lstGear)
                    decReturn += objGear.TotalCost;

                return decReturn;
            }
        }

        /// <summary>
        /// Cost for just the Armor Mod.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                return TotalCost;
            }
        }

        public XmlNode MyXmlNode
        {
            get
            {
                return XmlManager.Load("armor.xml")?.SelectSingleNode("/chummer/mods/mod[name = \"" + Name + "\"]");
            }
        }
        #endregion
    }
}
