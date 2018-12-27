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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Attributes;
using Chummer.helpers;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A specific piece of Armor.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    [HubClassTag("SourceID", true, "TotalArmor")]
    public class Armor : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanSell, IHasChildrenAndCost<Gear>, IHasCustomName, IHasLocation, ICanEquip, IHasSource, IHasRating
    {
        private Guid _sourceID = Guid.Empty;
        private Guid _guiID;
        private Guid _guiWeaponID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strArmorValue = "0";
        private string _strArmorOverrideValue = string.Empty;
        private string _strArmorCapacity = "0";
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private int _intRating;
        private int _intMaxRating;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strArmorName = string.Empty;
        private string _strExtra = string.Empty;
        private int _intDamage;
        private bool _blnEquipped = true;
        private readonly Character _objCharacter;
        private readonly TaggedObservableCollection<ArmorMod> _lstArmorMods = new TaggedObservableCollection<ArmorMod>();
        private readonly TaggedObservableCollection<Gear> _lstGear = new TaggedObservableCollection<Gear>();
        private string _strNotes = string.Empty;
        private Location _objLocation;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = true;
        private bool _blnDiscountCost;

        #region Constructor, Create, Save, Load, and Print Methods
        public Armor(Character objCharacter)
        {
            // Create the GUID for the new piece of Armor.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _lstArmorMods.CollectionChanged += ArmorModsOnCollectionChanged;
            _lstGear.CollectionChanged += GearOnCollectionChanged;
        }

        private void ArmorModsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool blnDoEncumbranceRefresh = false;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ArmorMod objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (!blnDoEncumbranceRefresh && objNewItem.Equipped)
                            blnDoEncumbranceRefresh = true;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ArmorMod objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if (!blnDoEncumbranceRefresh && objOldItem.Equipped)
                            blnDoEncumbranceRefresh = true;
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (ArmorMod objOldItem in e.OldItems)
                    {
                        objOldItem.Parent = null;
                        if (!blnDoEncumbranceRefresh && objOldItem.Equipped)
                            blnDoEncumbranceRefresh = true;
                    }
                    foreach (ArmorMod objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        if (!blnDoEncumbranceRefresh && objNewItem.Equipped)
                            blnDoEncumbranceRefresh = true;
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    blnDoEncumbranceRefresh = true;
                    break;
            }

            if (blnDoEncumbranceRefresh && Equipped)
            {
                _objCharacter?.OnPropertyChanged(nameof(Character.ArmorRating));
                _objCharacter?.RefreshEncumbrance();
            }
        }

        private void GearOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    foreach (Gear objNewItem in e.NewItems)
                    {
                        objNewItem.Parent = this;
                        objNewItem.ChangeEquippedStatus(Equipped);
                    }
                    break;
            }
        }

        /// Create an Armor from an XmlNode.
        /// <param name="objXmlArmorNode">XmlNode to create the object from.</param>
        /// <param name="blnSkipCost">Whether or not creating the Armor should skip the Variable price dialogue (should only be used by frmSelectArmor).</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        /// <param name="intRating">Rating of the item.</param>
        /// <param name="lstWeapons">List of Weapons that added to the character's weapons.</param>
        /// <param name="blnSkipSelectForms">Whether or not to skip forms that are created for bonuses like Custom Fit (Stack).</param>
        public void Create(XmlNode objXmlArmorNode, int intRating, List<Weapon> lstWeapons, bool blnSkipCost = false, bool blnCreateChildren = true, bool blnSkipSelectForms = false)
        {
            objXmlArmorNode.TryGetField("id", Guid.TryParse, out _sourceID);
            if (objXmlArmorNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlArmorNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlArmorNode.TryGetStringFieldQuickly("armor", ref _strArmorValue);
            if (objXmlArmorNode.TryGetStringFieldQuickly("armoroverride", ref _strArmorOverrideValue) && _strArmorOverrideValue == "0")
                _strArmorOverrideValue = string.Empty;
            _intRating = intRating;
            objXmlArmorNode.TryGetInt32FieldQuickly("rating", ref _intMaxRating);
            objXmlArmorNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
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
                    decimal decMin;
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
                        frmSelectNumber frmPickNumber = new frmSelectNumber(_objCharacter.Options.NuyenDecimals);
                        if (decMax > 1000000)
                            decMax = 1000000;
                        frmPickNumber.Minimum = decMin;
                        frmPickNumber.Maximum = decMax;
                        frmPickNumber.Description = string.Format(LanguageManager.GetString("String_SelectVariableCost", GlobalOptions.Language), DisplayNameShort(GlobalOptions.Language));
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

            if (!blnSkipSelectForms)
            {
                if (Bonus != null)
                {
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D"), Bonus, false, 1, DisplayNameShort(GlobalOptions.Language)))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    {
                        _strExtra = ImprovementManager.SelectedValue;
                    }
                }

                if (WirelessOn && WirelessBonus != null)
                {
                    ImprovementManager.ForcedValue = Extra;

                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D"), WirelessBonus, false, 1, DisplayNameShort(GlobalOptions.Language)))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }
            }

            if (!blnSkipSelectForms)
            {
                XmlNode xmlSelectModesFromCategory = objXmlArmorNode["selectmodsfromcategory"];
                if (xmlSelectModesFromCategory != null)
                {
                    XmlDocument objXmlDocument = XmlManager.Load("armor.xml");

                    // More than one Weapon can be added, so loop through all occurrences.
                    foreach (XmlNode objXmlCategoryNode in xmlSelectModesFromCategory)
                    {
                        frmSelectArmorMod frmPickArmorMod = new frmSelectArmorMod(_objCharacter)
                        {
                            AllowedCategories = objXmlCategoryNode.InnerText,
                            ExcludeGeneralCategory = true
                        };
                        frmPickArmorMod.ShowDialog();

                        if (frmPickArmorMod.DialogResult == DialogResult.Cancel)
                            return;

                        // Locate the selected piece.
                        XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/mods/mod[id = \"" + frmPickArmorMod.SelectedArmorMod + "\"]");

                        if (objXmlMod != null)
                        {
                            ArmorMod objMod = new ArmorMod(_objCharacter);

                            objMod.Create(objXmlMod, intRating, lstWeapons, blnSkipCost, blnSkipSelectForms);
                            objMod.IncludedInArmor = true;
                            objMod.ArmorCapacity = "[0]";
                            objMod.Cost = "0";
                            objMod.MaximumRating = objMod.Rating;
                            _lstArmorMods.Add(objMod);
                        }
                        else
                        {
                            ArmorMod objMod = new ArmorMod(_objCharacter)
                            {
                                Name = _strName,
                                Category = "Features",
                                Avail = "0",
                                Source = _strSource,
                                Page = _strPage,
                                IncludedInArmor = true,
                                ArmorCapacity = "[0]",
                                Cost = "0",
                                Rating = 0,
                                MaximumRating = 0
                            };
                            _lstArmorMods.Add(objMod);
                        }
                    }
                }
            }

            // Add any Armor Mods that come with the Armor.
            if (objXmlArmorNode["mods"] != null && blnCreateChildren)
            {
                XmlDocument objXmlArmorDocument = XmlManager.Load("armor.xml");

                foreach (XmlNode objXmlArmorMod in objXmlArmorNode.SelectNodes("mods/name"))
                {
                    XmlAttributeCollection objXmlAttributes = objXmlArmorMod.Attributes;
                    intRating = Convert.ToInt32(objXmlAttributes?["rating"]?.InnerText);
                    string strForceValue = objXmlAttributes?["select"]?.InnerText ?? string.Empty;

                    XmlNode objXmlMod = objXmlArmorDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + objXmlArmorMod.InnerText + "\"]");
                    if (objXmlMod != null)
                    {
                        ArmorMod objMod = new ArmorMod(_objCharacter);

                        objMod.Create(objXmlMod, intRating, lstWeapons, blnSkipCost, blnSkipSelectForms);
                        if (string.IsNullOrWhiteSpace(objMod.Extra))
                        {
                            objMod.Extra = strForceValue;
                        }
                        objMod.IncludedInArmor = true;
                        objMod.ArmorCapacity = "[0]";
                        objMod.Cost = "0";
                        objMod.MaximumRating = objMod.Rating;
                        _lstArmorMods.Add(objMod);
                    }
                    else
                    {
                        ArmorMod objMod = new ArmorMod(_objCharacter)
                        {
                            Name = _strName,
                            Category = "Features",
                            Avail = "0",
                            Source = _strSource,
                            Page = _strPage,
                            IncludedInArmor = true,
                            ArmorCapacity = "[0]",
                            Cost = "0",
                            Rating = 0,
                            MaximumRating = 0,
                            Extra = strForceValue
                        };
                        _lstArmorMods.Add(objMod);
                    }
                }
            }

            // Add any Gear that comes with the Armor.
            if (objXmlArmorNode["gears"] != null && blnCreateChildren)
            {
                XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");

                XmlNodeList objXmlGearList = objXmlArmorNode["gears"].SelectNodes("usegear");
                IList<Weapon> lstChildWeapons = new List<Weapon>();
                foreach (XmlNode objXmlVehicleGear in objXmlGearList)
                {
                    Gear objGear = new Gear(_objCharacter);
                    if (!objGear.CreateFromNode(objXmlGearDocument, objXmlVehicleGear, lstChildWeapons, blnSkipSelectForms))
                        continue;
                    foreach (Weapon objWeapon in lstChildWeapons)
                    {
                        objWeapon.ParentID = InternalId;
                    }
                    objGear.Parent = this;
                    objGear.ParentID = InternalId;
                    Gear.Add(objGear);
                    lstChildWeapons.AddRange(lstWeapons);
                }
                lstWeapons.AddRange(lstChildWeapons);
            }

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
                objGearWeapon.Cost = "0";
                lstWeapons.Add(objGearWeapon);

                Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID);
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("armor");
            objWriter.WriteElementString("sourceid", _sourceID.ToString("D"));
            objWriter.WriteElementString("guid",InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("armor", _strArmorValue);
            objWriter.WriteElementString("armoroverride", _strArmorOverrideValue);
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("armorname", _strArmorName);
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("damage", _intDamage.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(GlobalOptions.InvariantCultureInfo));
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
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString());
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D"));
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
                _objLocation = null;
            }
            else
            {
                if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
                    _guiID = Guid.NewGuid();
                if (objNode["location"] != null)
                {
                    if (Guid.TryParse(objNode["location"].InnerText, out Guid temp))
                    {
                        // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                        Location =
                            _objCharacter.ArmorLocations.FirstOrDefault(location =>
                                location.InternalId == temp.ToString());
                    }
                    else
                    {
                        //Legacy. Location is a string. 
                        Location =
                            _objCharacter.ArmorLocations.FirstOrDefault(location =>
                                location.Name == objNode["location"].InnerText);
                    }
                    _objLocation?.Children.Add(this);
                }
            }

            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (objNode["sourceid"] == null || !objNode.TryGetField("sourceid", Guid.TryParse, out _sourceID))
            {
                XmlNode objArmorNode = GetNode();
                objArmorNode?.TryGetField("id", Guid.TryParse, out _sourceID);
            }
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("armor", ref _strArmorValue);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            if (objNode.TryGetStringFieldQuickly("armoroverride", ref _strArmorOverrideValue) && _strArmorOverrideValue == "0")
                _strArmorOverrideValue = string.Empty;
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
            XmlNode xmlChildrenNode = objNode["armormods"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList nodMods = xmlChildrenNode.SelectNodes("armormod"))
                    if (nodMods != null)
                        foreach (XmlNode nodMod in nodMods)
                        {
                            ArmorMod objMod = new ArmorMod(_objCharacter);
                            objMod.Load(nodMod, blnCopy);
                            _lstArmorMods.Add(objMod);
                        }
            }
            xmlChildrenNode = objNode["gears"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList nodGears = xmlChildrenNode.SelectNodes("gear"))
                    if (nodGears != null)
                        foreach (XmlNode nodGear in nodGears)
                        {
                            Gear objGear = new Gear(_objCharacter);
                            objGear.Load(nodGear, blnCopy);
                            _lstGear.Add(objGear);
                        }
            }

            if (blnCopy)
            {
                if (Bonus != null)
                {
                    if (!string.IsNullOrEmpty(Extra))
                        ImprovementManager.ForcedValue = Extra;
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D"), Bonus, false, 1, DisplayNameShort(GlobalOptions.Language));
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    {
                        Extra = ImprovementManager.SelectedValue;
                    }
                }

                if (WirelessOn && WirelessBonus != null)
                {
                    ImprovementManager.ForcedValue = Extra;

                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Armor, _guiID.ToString("D"), WirelessBonus, false, 1, DisplayNameShort(GlobalOptions.Language)))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                }

                if (!_blnEquipped)
                {
                    _blnEquipped = true;
                    Equipped = false;
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
            objWriter.WriteStartElement("armor");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("armor", DisplayArmorValue);
            objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("armorname", CustomName);
            objWriter.WriteElementString("equipped", Equipped.ToString());
            objWriter.WriteElementString("wirelesson", WirelessOn.ToString());
            objWriter.WriteStartElement("armormods");
            foreach (ArmorMod objMod in ArmorMods)
            {
                objMod.Print(objWriter, objCulture, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("gears");
            foreach (Gear objGear in Gear)
            {
                objGear.Print(objWriter, objCulture, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            objWriter.WriteElementString("location", Location?.DisplayNameShort(GlobalOptions.Language));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this piece of Armor in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Name of the Armor.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                    _objCachedMyXmlNode = null;
                _strName = value;
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// Wireless Bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set => _nodWirelessBonus = value;
        }

        /// <summary>
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        [HubTag(true)]
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
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
        /// Armor's Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Armor's Armor value.
        /// </summary>
        public string ArmorValue
        {
            get => _strArmorValue;
            set => _strArmorValue = value;
        }

        /// <summary>
        /// Armor's Armor Override value.
        /// </summary>
        public string ArmorOverrideValue
        {
            get => _strArmorOverrideValue;
            set => _strArmorOverrideValue = value == "0" ? string.Empty : value;
        }

        /// <summary>
        /// Damage done to the Armor's Armor Rating.
        /// </summary>
        public int ArmorDamage
        {
            get => _intDamage;
            set
            {
                int intOldValue = _intDamage;
                _intDamage = value;

                int intTotalArmor = Convert.ToInt32(ArmorValue);

                // Go through all of the Mods for this piece of Armor and add the Armor value.
                foreach (ArmorMod objMod in ArmorMods)
                {
                    if (objMod.Equipped)
                        intTotalArmor += objMod.Armor;
                }

                if (_intDamage < 0)
                    _intDamage = 0;
                if (_intDamage > intTotalArmor)
                    _intDamage = intTotalArmor;

                if (_intDamage != intOldValue)
                {
                    if (Equipped)
                    {
                        _objCharacter?.OnPropertyChanged(nameof(Character.ArmorRating));
                        _objCharacter?.RefreshEncumbrance();
                    }
                }
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        [HubTag(true)]
        public int Rating
        {
            get => Math.Min(_intRating, MaxRating);
            set
            {
                int intNewValue = Math.Min(value, MaxRating);
                if (_intRating != intNewValue)
                {
                    _intRating = intNewValue;
                    if (Equipped)
                    {
                        if (ArmorValue.Contains("Rating") || ArmorOverrideValue.Contains("Rating"))
                        {
                            _objCharacter?.OnPropertyChanged(nameof(Character.ArmorRating));
                            _objCharacter?.RefreshEncumbrance();
                        }
                    }
                    if (Gear.Count > 0)
                    {
                        foreach (Gear objChild in Gear.Where(x => x.MaxRating.Contains("Parent") || x.MinRating.Contains("Parent")))
                        {
                            // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                            objChild.Rating = objChild.Rating;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int MaxRating
        {
            get => _intMaxRating;
            set => _intMaxRating = value;
        }

        /// <summary>
        /// Armor's Capacity string.
        /// </summary>
        public string ArmorCapacity
        {
            get => _strArmorCapacity;
            set => _strArmorCapacity = value;
        }

        /// <summary>
        /// Armor's Capacity.
        /// </summary>
        public string TotalArmorCapacity
        {
            get
            {
                string strArmorCapacity = ArmorCapacity;
                if (strArmorCapacity.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strArmorCapacity.StartsWith('[');
                    string strCapacity = strArmorCapacity;
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString()), out bool blnIsSuccess);
                    string strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : objProcess.ToString();
                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';

                    return strReturn;
                }
                else if (decimal.TryParse(strArmorCapacity, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                    return decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);

                return strArmorCapacity;
            }
        }

        /// <summary>
        /// Armor's Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// Armor's Cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        public string DisplayCost(out decimal decItemCost, bool blnUseRating = true, decimal decMarkup = 0.0m)
        {
            decItemCost = 0;
            string strReturn = Cost;
            if (strReturn.StartsWith("Variable("))
            {
                strReturn = strReturn.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                decimal decMin;
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

            if (blnUseRating)
            {
                decimal decTotalCost;
                // If the cost is determined by the Rating, evaluate the expression.
                if (strReturn.Contains("Rating"))
                {
                    string strCost = strReturn.Replace("Rating", Rating.ToString());
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strCost, out bool blnIsSuccess);
                    decTotalCost = blnIsSuccess ? Convert.ToDecimal(objProcess.ToString(), GlobalOptions.InvariantCultureInfo) : 0;
                }
                else
                {
                    decTotalCost = Convert.ToDecimal(strReturn, GlobalOptions.InvariantCultureInfo);
                }

                decTotalCost *= 1.0m + decMarkup;

                if (DiscountCost)
                    decTotalCost *= 0.9m;

                decItemCost = decTotalCost;

                return decTotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            }

            return strReturn.CheapReplace("Rating", () => LanguageManager.GetString("String_Rating", GlobalOptions.Language)) + '¥';
        }


        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == null)
                {
                    string strSource = Source;
                    string strPage = Page(GlobalOptions.Language);
                    if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                    {
                        _objCachedSourceDetail = new SourceString(strSource, strPage, GlobalOptions.Language);
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                    }
                }

                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Armor's Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Guid of a Weapon created from the Armour.
        /// </summary>
        public string WeaponID
        {
            get => _guiWeaponID.ToString("D");
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiWeaponID = guiTemp;
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
        /// Whether or not the Armor is equipped and should be considered for highest Armor Rating or Armor Encumbrance.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set
            {
                if (_blnEquipped != value)
                {
                    _blnEquipped = value;
                    if (value)
                    {
                        // Add the Armor's Improevments to the character.
                        ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Armor && x.SourceName == InternalId).ToList());
                        // Add the Improvements from any Armor Mods in the Armor.
                        foreach (ArmorMod objMod in ArmorMods)
                        {
                            if (objMod.Equipped)
                            {
                                ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId).ToList());
                                // Add the Improvements from any Gear in the Armor.
                                foreach (Gear objGear in objMod.Gear)
                                {
                                    if (objGear.Equipped)
                                    {
                                        objGear.ChangeEquippedStatus(true);
                                    }
                                }
                            }
                        }
                        // Add the Improvements from any Gear in the Armor.
                        foreach (Gear objGear in Gear)
                        {
                            if (objGear.Equipped)
                            {
                                objGear.ChangeEquippedStatus(true);
                            }
                        }
                    }
                    else
                    {
                        // Add the Armor's Improevments to the character.
                        ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Armor && x.SourceName == InternalId).ToList());
                        // Add the Improvements from any Armor Mods in the Armor.
                        foreach (ArmorMod objMod in ArmorMods)
                        {
                            ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId).ToList());
                            // Add the Improvements from any Gear in the Armor.
                            foreach (Gear objGear in objMod.Gear)
                            {
                                objGear.ChangeEquippedStatus(false);
                            }
                        }
                        // Add the Improvements from any Gear in the Armor.
                        foreach (Gear objGear in Gear)
                        {
                            objGear.ChangeEquippedStatus(false);
                        }
                    }

                    _objCharacter?.OnPropertyChanged(nameof(Character.ArmorRating));
                    _objCharacter?.RefreshEncumbrance();
                }
            }
        }

        /// <summary>
        /// Whether or not Wireless is turned on for this armor
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set => _blnWirelessOn = value;
        }

        /// <summary>
        /// The Armor's total Armor value including Modifications.
        /// </summary>
        public int TotalArmor
        {
            get
            {
                int.TryParse(ArmorValue.Replace("Rating", Rating.ToString()), out int intTotalArmor);
                // Go through all of the Mods for this piece of Armor and add the Armor value.
                intTotalArmor += ArmorMods.Where(o => o.Equipped).Sum(o => o.Armor);
                intTotalArmor -= ArmorDamage;

                return Math.Max(intTotalArmor, 0);
            }
        }

        /// <summary>
        /// The Armor's total bonus Armor value including Modifications.
        /// </summary>
        public int TotalOverrideArmor
        {
            get
            {
                int.TryParse(ArmorOverrideValue.Replace("Rating", Rating.ToString()), out int intTotalArmor);
                // Go through all of the Mods for this piece of Armor and add the Armor value.
                intTotalArmor += ArmorMods.Where(o => o.Equipped).Sum(o => o.Armor);
                intTotalArmor -= ArmorDamage;

                return Math.Max(intTotalArmor, 0);
            }
        }

        public string DisplayArmorValue
        {
            get
            {
                string strArmorOverrideValue = ArmorOverrideValue;
                if (!string.IsNullOrWhiteSpace(strArmorOverrideValue))
                {
                    return $"{TotalArmor}/{strArmorOverrideValue}";
                }

                string strArmor = ArmorValue;
                char chrFirstArmorChar = strArmor.Length > 0 ? strArmor[0] : ' ';
                if (chrFirstArmorChar == '+' || chrFirstArmorChar == '-')
                {
                    return $"{TotalArmor:+0;-0;0}";
                }
                return TotalArmor.ToString();
            }
        }
        /// <summary>
        /// The Armor's total Cost including Modifications.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decTotalCost = OwnCost;

                // Go through all of the Mods for this piece of Armor and add the Cost value.
                foreach (ArmorMod objMod in ArmorMods)
                    decTotalCost += objMod.TotalCost;

                // Go through all of the Gear for this piece of Armor and add the Cost value.
                foreach (Gear objGear in Gear)
                    decTotalCost += objGear.TotalCost;

                return decTotalCost;
            }
        }

        /// <summary>
        /// Cost multiplier for Gear Children attached to this Armor.
        /// </summary>
        public int ChildCostMultiplier => 1;

        /// <summary>
        /// Cost for just the Armor.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                // If the cost is determined by the Rating, evaluate the expression.
                string strCostExpression = Cost;

                StringBuilder objCost = new StringBuilder(strCostExpression.TrimStart('+'));
                objCost.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                decimal decTotalCost = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;

                if (DiscountCost)
                    decTotalCost *= 0.9m;

                return decTotalCost;
            }
        }

        /// <summary>
        /// The Modifications currently applied to the Armor.
        /// </summary>
        public TaggedObservableCollection<ArmorMod> ArmorMods => _lstArmorMods;

        /// <summary>
        /// The Gear currently applied to the Armor.
        /// </summary>
        public TaggedObservableCollection<Gear> Gear => _lstGear;

        /// <summary>
        /// Location.
        /// </summary>
        public Location Location
        {
            get => _objLocation;
            set => _objLocation = value;
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
        /// Whether or not the Armor's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost && _objCharacter.BlackMarketDiscount;
            set => _blnDiscountCost = value;
        }
        public Guid SourceID => _sourceID;

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
                    string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');

                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));
                objAvail.Replace("Rating", Rating.ToString());

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail = Convert.ToInt32(objProcess);
            }

            if (blnCheckChildren)
            {
                // Run through armor mod children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (ArmorMod objChild in ArmorMods)
                {
                    if (!objChild.IncludedInArmor)
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
        /// Calculated Capacity of the Armor.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strReturn = TotalArmorCapacity;

                // If an Armor Capacity is specified for the Armor, use that value.
                if (string.IsNullOrEmpty(strReturn) || strReturn == "0")
                    strReturn = (0.0m).ToString("#,0.##", GlobalOptions.CultureInfo);
                else if (strReturn == "Rating")
                    strReturn = Rating.ToString();
                else if (decimal.TryParse(strReturn, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                    strReturn = decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);

                foreach (ArmorMod objArmorMod in ArmorMods)
                {
                    if (objArmorMod.ArmorCapacity.StartsWith('-') || objArmorMod.ArmorCapacity.StartsWith("[-"))
                    {
                        // If the Capaicty is determined by the Capacity of the parent, evaluate the expression. Generally used for providing a percentage of armour capacity as bonus, ie YNT Softweave.
                        // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                        string strCapacity = objArmorMod.ArmorCapacity
                            .FastEscape('[', ']')
                            .CheapReplace("Capacity", () => TotalArmorCapacity)
                            .Replace("Rating", Rating.ToString());

                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity, out bool blnIsSuccess);
                        if (blnIsSuccess)
                        {
                            strCapacity = (Convert.ToDecimal(strReturn) - Convert.ToDecimal(objProcess, GlobalOptions.CultureInfo)).ToString("#,0.##", GlobalOptions.CultureInfo);
                        }
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
                string strArmorCapacity = TotalArmorCapacity;
                if (strArmorCapacity != "0" && !string.IsNullOrEmpty(strArmorCapacity)) // && _objCharacter.Options.ArmorSuitCapacity)
                {
                    // Run through its Armor Mods and deduct the Capacity costs.
                    foreach (ArmorMod objMod in ArmorMods.Where(mod => !mod.IncludedInArmor))
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
                        if (blnSoftweave)
                            continue;
                        string strCapacity = objMod.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                        {
                            // If this is a multiple-capacity item, use only the second half.
                            strCapacity = strCapacity.Substring(intPos + 1);
                        }

                        if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }

                    // Run through its Gear and deduct the Armor Capacity costs.
                    if (Gear.Count <= 0) return decCapacity;
                    object decCapacityLock = new object();
                    // Run through its Children and deduct the Capacity costs.
                    Parallel.ForEach(Gear.Where(gear => !gear.IncludedInParent), objChildGear =>
                    {
                        decimal decLoop = objChildGear.PluginArmorCapacity * objChildGear.Quantity;
                        lock (decCapacityLock)
                            decCapacity -= decLoop;
                    });
                }
                // Calculate the remaining Capacity for a standard piece of Armor using the Maximum Armor Modifications rules.
                else // if (_objCharacter.Options.MaximumArmorModifications)
                {
                    // Run through its Armor Mods and deduct the Rating (or 1 if it has no Rating).
                    foreach (ArmorMod objMod in ArmorMods.Where(mod => !mod.IncludedInArmor))
                    {
                        if (objMod.Rating > 0)
                            decCapacity -= objMod.Rating;
                        else
                            decCapacity -= 1;
                    }

                    // Run through its Gear and deduct the Rating (or 1 if it has no Rating).
                    foreach (Gear objGear in Gear.Where(gear => !gear.IncludedInParent))
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
                string strArmorCapacity = ArmorCapacity;
                if (!string.IsNullOrEmpty(strArmorCapacity) && strArmorCapacity != "0")
                {
                    return CapacityStyle.Standard;
                }

                return CapacityStyle.Zero;
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
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
            if (!string.IsNullOrEmpty(CustomName))
                strReturn += strSpaceCharacter + "(\"" + CustomName + "\")";
            if (Rating > 0)
                strReturn += strSpaceCharacter + '(' + LanguageManager.GetString("String_Rating", strLanguage) + strSpaceCharacter + Rating.ToString() + ')';
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpaceCharacter + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            return strReturn;
        }

        /// <summary>
        /// A custom name for the Armor assigned by the player.
        /// </summary>
        public string CustomName
        {
            get => _strArmorName;
            set => _strArmorName = value;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("armor.xml", strLanguage).SelectSingleNode("/chummer/armors/armor[id = \"" + SourceID.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteArmor()
        {
            decimal decReturn = 0.0m;
            // Remove any Improvements created by the Armor and its children.
            foreach (ArmorMod objMod in ArmorMods)
                decReturn += objMod.DeleteArmorMod();
            // Remove any Improvements created by the Armor's Gear.
            foreach (Gear objGear in Gear)
                decReturn += objGear.DeleteGear();

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Armor, InternalId);

            // Remove the Cyberweapon created by the Mod if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>>();
                foreach (Weapon objWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                {
                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, null, null, null));
                }
                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                    {
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, null, null));
                    }

                    foreach (VehicleMod objVehicleMod in objVehicle.Mods)
                    {
                        foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                        {
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, objVehicleMod, null));
                        }
                    }

                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        foreach (Weapon objWeapon in objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                        {
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, null, objMount));
                        }
                    }
                }
                foreach (Tuple<Weapon, Vehicle, VehicleMod, WeaponMount> objLoopTuple in lstWeaponsToDelete)
                {
                    Weapon objDeleteWeapon = objLoopTuple.Item1;
                    decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
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
            }

            return decReturn;
        }

        #region UI Methods
        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        /// <param name="cmsArmor">ContextMenuStrip for the Armor Node.</param>
        /// <param name="cmsArmorMod">ContextMenuStrip for Armor Mod Nodes.</param>
        /// <param name="cmsArmorGear">ContextMenuStrip for Armor Gear Nodes.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsArmor, ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear)
        {
            //if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
            //return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = cmsArmor,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            foreach (ArmorMod objMod in ArmorMods)
            {
                TreeNode objLoopNode = objMod.CreateTreeNode(cmsArmorMod, cmsArmorGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }
            foreach (Gear objGear in Gear)
            {
                TreeNode objLoopNode = objGear.CreateTreeNode(cmsArmorGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
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
                    return Color.SaddleBrown;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion
        #endregion

        public bool Remove(Character characterObject, bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                if (!characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmor",
                    GlobalOptions.Language)))
                    return false;
            }
            DeleteArmor();
            return characterObject.Armor.Remove(this);
        }

        public void Sell(Character characterObject, decimal percentage)
        {
            characterObject.Armor.Remove(this);

            // Create the Expense Log Entry for the sale.
            decimal decAmount = TotalCost * percentage;
            decAmount += DeleteArmor() * percentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(characterObject);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldArmor", GlobalOptions.Language) + ' ' + DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
            characterObject.ExpenseEntries.AddWithSort(objExpense);
            characterObject.Nuyen += decAmount;
        }

        /// <summary>
        /// Alias map for SourceDetail control text and tooltip assignation. 
        /// </summary>
        /// <param name="sourceControl"></param>
        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }

        public TaggedObservableCollection<Gear> Children => Gear;
    }
}
