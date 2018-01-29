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
    public class ArmorMod : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strArmorCapacity = "[0]";
        private string _strGearCapacity = string.Empty;
        private int _intArmorValue = 0;
        private int _intMaxRating = 0;
        private int _intRating = 0;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludedInArmor = false;
        private bool _blnEquipped = true;
        private string _strExtra = string.Empty;
        private Guid _guiWeaponID = Guid.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = true;
        private readonly Character _objCharacter;
        private List<Gear> _lstGear = new List<Gear>();
        private string _strNotes = string.Empty;
        private bool _blnDiscountCost = false;
        private Armor _objParent;

        #region Constructor, Create, Save, Load, and Print Methods
        public ArmorMod(Character objCharacter)
        {
            // Create the GUID for the new Armor Mod.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Armor Modification from an XmlNode.
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="intRating">Rating of the selected ArmorMod.</param>
        /// <param name="lstWeapons">List of Weapons that are created by the Armor.</param>
        /// <param name="objWeaponNodes">List of Weapon Nodes that are created by the Armor.</param>
        /// <param name="blnSkipCost">Whether or not creating the Armor should skip the Variable price dialogue (should only be used by frmSelectArmor).</param>
        public void Create(XmlNode objXmlArmorNode, int intRating, List<Weapon> lstWeapons, bool blnSkipCost = false, bool blnSkipSelectForms = false)
        {
            if (objXmlArmorNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlArmorNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlArmorNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlArmorNode.TryGetStringFieldQuickly("gearcapacity", ref _strGearCapacity);
            _intRating = intRating;
            objXmlArmorNode.TryGetInt32FieldQuickly("armor", ref _intArmorValue);
            objXmlArmorNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
            objXmlArmorNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlArmorNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArmorNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlArmorNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArmorNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _nodBonus = objXmlArmorNode["bonus"];
            _nodWirelessBonus = objXmlArmorNode["wirelessbonus"];
            _blnWirelessOn = _nodWirelessBonus != null;

            objXmlArmorNode.TryGetStringFieldQuickly("cost", ref _strCost);

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

                if (!blnSkipSelectForms)
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

            if (objXmlArmorNode["bonus"] != null && !blnSkipSelectForms)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, _guiID.ToString("D"), objXmlArmorNode["bonus"], false, intRating, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
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
                    
                    objGear.Create(objXmlGear, intRating, lstWeapons, strForceValue, !blnSkipSelectForms);

                    objGear.Capacity = "[0]";
                    objGear.ArmorCapacity = "[0]";
                    objGear.Cost = "0";
                    objGear.MaxRating = objGear.Rating;
                    objGear.MinRating = objGear.Rating;
                    objGear.ParentID = InternalId;
                    _lstGear.Add(objGear);
                }
            }

            // Add Weapons if applicable.
            if (objXmlArmorNode.InnerXml.Contains("<addweapon>"))
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddWeapon in objXmlArmorNode.SelectNodes("addweapon"))
                {
                    string strLoopID = objXmlAddWeapon.InnerText;
                    XmlNode objXmlWeapon = strLoopID.IsGuid()
                        ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                        : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");
                    
                    Weapon objGearWeapon = new Weapon(_objCharacter);
                    objGearWeapon.Create(objXmlWeapon, lstWeapons, true, !blnSkipSelectForms, blnSkipCost);
                    objGearWeapon.ParentID = InternalId;
                    lstWeapons.Add(objGearWeapon);

                    Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID);
                }
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("armormod");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("armor", _intArmorValue.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("gearcapacity", _strGearCapacity);
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    objGear.Save(objWriter);
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
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D"));
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString());
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
            if (blnCopy || !Guid.TryParse(objNode["guid"].InnerText, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetInt32FieldQuickly("armor", ref _intArmorValue);
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
                Guid.TryParse(objNode["weaponguid"].InnerText, out _guiWeaponID);
            }
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);

            if (objNode.InnerXml.Contains("gears"))
            {
                XmlNodeList nodGears = objNode.SelectNodes("gears/gear");
                foreach (XmlNode nodGear in nodGears)
                {
                    Gear objGear = new Gear(_objCharacter);
                    objGear.Load(nodGear, blnCopy);
                    _lstGear.Add(objGear);
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("armormod");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("armor", Armor.ToString(objCulture));
            objWriter.WriteElementString("maxrating", MaximumRating.ToString(objCulture));
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("included", IncludedInArmor.ToString());
            objWriter.WriteElementString("equipped", Equipped.ToString());
            objWriter.WriteElementString("wirelesson", WirelessOn.ToString());
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in Gear)
            {
                objGear.Print(objWriter, objCulture, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(_strExtra, strLanguageToPrint));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
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
                return _guiID.ToString("D");
            }
        }

        /// <summary>
        /// Guid of a Cyberware Weapon.
        /// </summary>
        public string WeaponID
        {
            get
            {
                return _guiWeaponID.ToString("D");
            }
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiWeaponID = guiTemp;
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
                if (_strName != value)
                    _objCachedMyXmlNode = null;
                _strName = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (Rating > 0)
                strReturn += " (" + LanguageManager.GetString("String_Rating", strLanguage) + ' ' + Rating.ToString() + ')';
            if (!string.IsNullOrEmpty(Extra))
                strReturn += " (" + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            return strReturn;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("armor.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
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
                return _intArmorValue;
            }
            set
            {
                _intArmorValue = value;
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
            get => Math.Min(_intRating, MaximumRating);
            set => _intRating = Math.Min(value, MaximumRating);
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
                return _strCost;
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
                _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Whether or not the Armor Mod's cost should be discounted by 10% through the Black Market Pipeline Quality.
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
        public IList<Gear> Gear
        {
            get
            {
                return _lstGear;
            }
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple().ToString(objCulture, strLanguage);
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
            if (strAvail.Length > 0)
            {
                if (strAvail.StartsWith("FixedValues("))
                {
                    string[] strValues = strAvail.TrimStart("FixedValues(", true).TrimEnd(')').Split(',');
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                strAvail = strAvail.TrimStart('+');

                strAvail = strAvail.Replace("Rating", Rating.ToString());

                try
                {
                    intAvail = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvail));
                }
                catch (XPathException)
                {
                }
            }

            if (blnCheckChildren)
            {
                // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Gear objChild in Gear)
                {
                    if (objChild.ParentID != InternalId)
                    {
                        AvailabilityValue objLoopAvailTuple = objChild.TotalAvailTuple();
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

            // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Caculated Gear Capacity of the Armor Mod.
        /// </summary>
        public string CalculatedGearCapacity
        {
            get
            {
                string strCapacity = GearCapacity;
                if (string.IsNullOrEmpty(strCapacity))
                    return "0";
                if (strCapacity.StartsWith("FixedValues("))
                {
                    string[] strValues = strCapacity.TrimStart("FixedValues(", true).TrimEnd(')').Split(',');
                    strCapacity = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }
                strCapacity = strCapacity.CheapReplace("Capacity", () => Convert.ToDecimal(Parent.TotalArmorCapacity, GlobalOptions.CultureInfo).ToString(GlobalOptions.InvariantCultureInfo));
                strCapacity = strCapacity.Replace("Rating", Rating.ToString());

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
                string strCapacity = ArmorCapacity;
                if (string.IsNullOrEmpty(strCapacity))
                    return "0";
                if (strCapacity.StartsWith("FixedValues("))
                {
                    string[] strValues = strCapacity.TrimStart("FixedValues(", true).TrimEnd(')').Split(',');
                    strCapacity = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }
                strCapacity = strCapacity.CheapReplace("Capacity", () => Convert.ToDecimal(Parent.TotalArmorCapacity, GlobalOptions.CultureInfo).ToString(GlobalOptions.InvariantCultureInfo));
                strCapacity = strCapacity.Replace("Rating", Rating.ToString());
                bool blnSquareBrackets = strCapacity.Contains('[');
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                string strReturn = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity)).ToString("#,0.##", GlobalOptions.CultureInfo);
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

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
                decimal decReturn = OwnCost;

                // Go through all of the Gear for this piece of Armor and add the Cost value.
                foreach (Gear objGear in Gear)
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
                decimal decReturn = 0.0m;
                string strCostExpr = Cost;
                if (strCostExpr.StartsWith("FixedValues("))
                {
                    string[] strValues = strCostExpr.TrimStart("FixedValues(", true).TrimEnd(')').Split(',');
                    strCostExpr = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }
                decimal decArmorCost = Parent?.OwnCost ?? 0.0m;

                if (strCostExpr.Contains("Armor Cost") || strCostExpr.Contains("Rating"))
                {
                    strCostExpr = strCostExpr.Replace("Armor Cost", decArmorCost.ToString(GlobalOptions.InvariantCultureInfo));
                    strCostExpr = strCostExpr.Replace("Rating", Rating.ToString());
                    decReturn = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCostExpr), GlobalOptions.InvariantCultureInfo);
                }
                else
                    decReturn = Convert.ToDecimal(strCostExpr, GlobalOptions.InvariantCultureInfo);

                if (DiscountCost)
                    decReturn *= 0.9m;

                return decReturn;
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
                _objCachedMyXmlNode = XmlManager.Load("armor.xml", strLanguage).SelectSingleNode("/chummer/mods/mod[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        /// <param name="objCharacter">Parent character.</param>
        /// <param name="objArmorMod">Armor Mod to delete.</param>
        /// <param name="treWeapons">TreeView that holds the list of Weapons.</param>
        /// <param name="treVehicles">TreeView that holds the list of Vehicles.</param>
        public decimal DeleteArmorMod(TreeView treWeapons, TreeView treVehicles)
        {
            decimal decReturn = 0.0m;
            // Remove the Cyberweapon created by the Mod if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                List<string> lstNodesToRemoveIds = new List<string>();
                List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>>();
                foreach (Weapon objWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                {
                    lstNodesToRemoveIds.Add(objWeapon.InternalId);
                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, null, null, null));
                }
                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                    {
                        lstNodesToRemoveIds.Add(objWeapon.InternalId);
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, null, null));
                    }

                    foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                    {
                        foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                        {
                            lstNodesToRemoveIds.Add(objWeapon.InternalId);
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, objVehicleMod, null));
                        }
                    }

                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        foreach (Weapon objWeapon in objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                        {
                            lstNodesToRemoveIds.Add(objWeapon.InternalId);
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, null, objMount));
                        }
                    }
                }
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
                    TreeNode objLoopNode = treWeapons.FindNode(strNodeId) ?? treVehicles.FindNode(strNodeId);
                    objLoopNode?.Remove();
                }
            }

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, InternalId);
            // Remove any Improvements created by the Armor's Gear.
            foreach (Gear objGear in Gear)
                decReturn += objGear.DeleteGear(treWeapons, treVehicles);

            return decReturn;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = InternalId,
                ContextMenuStrip = string.IsNullOrEmpty(GearCapacity) ? cmsArmorMod : cmsArmorGear
            };
            if (!string.IsNullOrEmpty(Notes))
                objNode.ForeColor = Color.SaddleBrown;
            else if (IncludedInArmor)
                objNode.ForeColor = SystemColors.GrayText;
            objNode.ToolTipText = Notes.WordWrap(100);

            TreeNodeCollection lstModChildNodes = objNode.Nodes;
            foreach (Gear objGear in Gear)
            {
                lstModChildNodes.Add(objGear.CreateTreeNode(cmsArmorGear));
            }
            if (lstModChildNodes.Count > 0)
                objNode.Expand();

            return objNode;
        }
        #endregion
    }
}
