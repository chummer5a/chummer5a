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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Attributes;
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A piece of Armor Modification.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class ArmorMod : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanSell, ICanEquip, IHasSource, IHasRating, ICanSort, IHasWirelessBonus, IHasStolenProperty, ICanPaste
	{
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private Guid _guiID;
	    private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strArmorCapacity = "[0]";
        private string _strGearCapacity = string.Empty;
        private int _intArmorValue;
        private int _intMaxRating;
        private int _intRating;
	    private string _strRatingLabel = "String_Rating";
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludedInArmor;
        private bool _blnEquipped = true;
        private string _strExtra = string.Empty;
        private Guid _guiWeaponID = Guid.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = false;
        private readonly Character _objCharacter;
        private readonly TaggedObservableCollection<Gear> _lstGear = new TaggedObservableCollection<Gear>();
        private string _strNotes = string.Empty;
        private bool _blnDiscountCost;
	    private bool _blnStolen;
        private int _intSortOrder;

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
        /// <param name="blnSkipCost">Whether or not creating the ArmorMod should skip the Variable price dialogue (should only be used by frmSelectArmor).</param>
        /// <param name="blnSkipSelectForms">Whether or not to skip selection forms (related to improvements) when creating this ArmorMod.</param>
        public void Create(XmlNode objXmlArmorNode, int intRating, List<Weapon> lstWeapons, bool blnSkipCost = false, bool blnSkipSelectForms = false)
        {
            if (!objXmlArmorNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for armor mod xmlnode", objXmlArmorNode });
                Utils.BreakIfDebug();
            }
            else
                _objCachedMyXmlNode = null;
            objXmlArmorNode.TryGetStringFieldQuickly("name", ref _strName);
            objXmlArmorNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlArmorNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlArmorNode.TryGetStringFieldQuickly("gearcapacity", ref _strGearCapacity);
            _intRating = intRating;
            objXmlArmorNode.TryGetInt32FieldQuickly("armor", ref _intArmorValue);
            objXmlArmorNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
            objXmlArmorNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objXmlArmorNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlArmorNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArmorNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlArmorNode.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArmorNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            _nodBonus = objXmlArmorNode["bonus"];
            _nodWirelessBonus = objXmlArmorNode["wirelessbonus"];

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
                        decMin = Convert.ToDecimal(strFirstHalf, GlobalOptions.Instance.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strSecondHalf, GlobalOptions.Instance.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strFirstHalf.FastEscape('+'), GlobalOptions.Instance.InvariantCultureInfo);

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
                        _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.Instance.InvariantCultureInfo);
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
            XmlNode xmlChildrenNode = objXmlArmorNode["gears"];
            if (xmlChildrenNode != null)
            {
                XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                using (XmlNodeList xmlUseGearList = xmlChildrenNode.SelectNodes("usegear"))
                    if (xmlUseGearList != null)
                        foreach (XmlNode objXmlArmorGear in xmlUseGearList)
                        {
                            intRating = 0;
                            string strForceValue = string.Empty;
                            objXmlArmorGear.TryGetInt32FieldQuickly("rating", ref intRating);
                            objXmlArmorGear.TryGetStringFieldQuickly("select", ref strForceValue);

                            XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = " + objXmlArmorGear.InnerText.CleanXPath() + "]");
                            Gear objGear = new Gear(_objCharacter);

                            objGear.Create(objXmlGear, intRating, lstWeapons, strForceValue, !blnSkipSelectForms);

                            objGear.Capacity = "[0]";
                            objGear.ArmorCapacity = "[0]";
                            objGear.Cost = "0";
                            objGear.ParentID = InternalId;
                            _lstGear.Add(objGear);
                        }
            }

            // Add Weapons if applicable.
            if (objXmlArmorNode.InnerXml.Contains("<addweapon>"))
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                using (XmlNodeList xmlAddWeaponList = objXmlArmorNode.SelectNodes("addweapon"))
                    if (xmlAddWeaponList != null)
                        foreach (XmlNode objXmlAddWeapon in xmlAddWeaponList)
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
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("armormod");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("armor", _intArmorValue.ToString(GlobalOptions.Instance.InvariantCultureInfo));
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("gearcapacity", _strGearCapacity);
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(GlobalOptions.Instance.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.Instance.InvariantCultureInfo));
            objWriter.WriteElementString("ratinglabel", _strRatingLabel);
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
            objWriter.WriteElementString("stolen", _blnStolen.ToString());
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D"));
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString());
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString());
            objWriter.WriteEndElement();

            if (!IncludedInArmor)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether or not we are copying an existing node.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetInt32FieldQuickly("armor", ref _intArmorValue);
            objNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objNode.TryGetStringFieldQuickly("gearcapacity", ref _strGearCapacity);
            objNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludedInArmor);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = false;
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetField("weaponguid", Guid.TryParse, out _guiWeaponID);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);

            XmlNode xmlChildrenNode = objNode["gears"];
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

            if (!blnCopy) return;
            if (!string.IsNullOrEmpty(Extra))
                ImprovementManager.ForcedValue = Extra;
            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, _guiID.ToString("D"), Bonus, false, 1, DisplayNameShort(GlobalOptions.Language));
            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
            {
                Extra = ImprovementManager.SelectedValue;
            }
            ToggleWirelessBonuses(WirelessOn);
            if (_blnEquipped) return;
            _blnEquipped = true;
            Equipped = false;
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("armormod");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("armor", Armor.ToString(objCulture));
            objWriter.WriteElementString("maxrating", MaximumRating.ToString(objCulture));
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("ratinglabel", RatingLabel);
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
        public string InternalId => _guiID.ToString("D");

	    /// <summary>
	    /// Identifier of the object within data files.
	    /// </summary>
	    public Guid SourceID => _guiSourceID;

	    /// <summary>
	    /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
	    /// </summary>
	    public string SourceIDString => _guiSourceID.ToString("D");

        /// <summary>
        /// Guid of a Cyberware Weapon.
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
        /// Name of the Mod.
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
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
            if (Rating > 0)
                strReturn += strSpaceCharacter + '(' + LanguageManager.GetString(RatingLabel, strLanguage) + strSpaceCharacter + Rating.ToString() + ')';
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpaceCharacter + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
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
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Mod's Armor value modifier.
        /// </summary>
        public int Armor
        {
            get => _intArmorValue;
            set
            {
                if (_intArmorValue != value)
                {
                    _intArmorValue = value;
                    if (Equipped && Parent?.Equipped == true)
                    {
                        _objCharacter?.OnPropertyChanged(nameof(Character.ArmorRating));
                        _objCharacter?.RefreshEncumbrance();
                    }
                }
            }
        }

        /// <summary>
        /// Armor capacity.
        /// </summary>
        public string ArmorCapacity
        {
            get => _strArmorCapacity;
            set => _strArmorCapacity = value;
        }

        /// <summary>
        /// Capacity for gear plugins.
        /// </summary>
        public string GearCapacity
        {
            get => _strGearCapacity;
            set => _strGearCapacity = value;
        }

        /// <summary>
        /// Mod's Maximum Rating.
        /// </summary>
        public int MaximumRating
        {
            get => _intMaxRating;
            set => _intMaxRating = value;
        }

        /// <summary>
        /// Mod's current Rating.
        /// </summary>
        public int Rating
        {
            get => Math.Min(_intRating, MaximumRating);
            set
            {
                _intRating = Math.Min(value, MaximumRating);
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

	    public string RatingLabel
	    {
	        get => _strRatingLabel;
	        set => _strRatingLabel = value;
	    }
        /// <summary>
        /// Mod's Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// The Mod's cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Mod's Sourcebook.
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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Was the object stolen  via the Stolen Gear quality?
        /// </summary>
	    public bool Stolen
	    {
	        get => _blnStolen;
	        set => _blnStolen = value;
	    }

        private SourceString _objCachedSourceDetail;
	    public SourceString SourceDetail => _objCachedSourceDetail ?? (_objCachedSourceDetail =
	                                            new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language));



        /// <summary>
        /// Whether or not an Armor Mod is equipped and should be included in the Armor's totals.
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
                        if (Parent?.Equipped == true)
                        {
                            ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId).ToList());
                            // Add the Improvements from any Gear in the Armor.
                            foreach (Gear objGear in Gear)
                            {
                                if (objGear.Equipped)
                                {
                                    objGear.ChangeEquippedStatus(true);
                                }
                            }
                        }
                    }
                    else
                    {
                        ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId).ToList());
                        // Add the Improvements from any Gear in the Armor.
                        foreach (Gear objGear in Gear)
                        {
                            objGear.ChangeEquippedStatus(false);
                        }
                    }

                    if (Parent?.Equipped == true)
                    {
                        _objCharacter?.OnPropertyChanged(nameof(Character.ArmorRating));
                        _objCharacter?.RefreshEncumbrance();
                    }
                }
            }
        }

        /// <summary>
        /// Whether or not an Armor Mod's wireless bonus is enabled
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set
            {
                if (value == _blnWirelessOn)
                    return;
                ToggleWirelessBonuses(value);
                _blnWirelessOn = value;
            }
        }

        /// <summary>
        /// Whether or not this Mod is part of the base Armor configuration.
        /// </summary>
        public bool IncludedInArmor
        {
            get => _blnIncludedInArmor;
            set => _blnIncludedInArmor = value;
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
        /// Value that was selected during the Improvement Manager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
        }

        /// <summary>
        /// Whether or not the Armor Mod's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost && _objCharacter.BlackMarketDiscount;
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
        /// Parent Armor.
        /// </summary>
        public Armor Parent { get; set; }

        /// <summary>
        /// The Gear currently applied to the Armor.
        /// </summary>
        public TaggedObservableCollection<Gear> Gear => _lstGear;

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

                blnModifyParentAvail = strAvail.StartsWith('+', '-') && !IncludedInArmor;

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

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInArmor);
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
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCapacity = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }
                strCapacity = strCapacity.CheapReplace("Capacity", () => Convert.ToDecimal(Parent?.TotalArmorCapacity, GlobalOptions.Instance.CultureInfo).ToString(GlobalOptions.Instance.InvariantCultureInfo))
                    .Replace("Rating", Rating.ToString());

                //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity, out bool blnIsSuccess);
                string strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.Instance.CultureInfo) : strCapacity;

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
                decimal decCapacity;
                string strMyCapacity = CalculatedGearCapacity;
                // Get the Gear base Capacity.
                int intPos = strMyCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    // If this is a multiple-capacity item, use only the first half.
                    strMyCapacity = strMyCapacity.Substring(0, intPos);
                    decCapacity = Convert.ToDecimal(strMyCapacity, GlobalOptions.Instance.CultureInfo);
                }
                else
                    decCapacity = Convert.ToDecimal(strMyCapacity, GlobalOptions.Instance.CultureInfo);

                // Run through its Children and deduct the Capacity costs.
                foreach (Gear objChildGear in Gear)
                {
                    string strCapacity = objChildGear.CalculatedArmorCapacity;
                    intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                    if (intPos != -1)
                    {
                        // If this is a multiple-capacity item, use only the second half.
                        strCapacity = strCapacity.Substring(intPos + 1);
                    }

                    // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                    strCapacity = strCapacity.StartsWith('[') ? strCapacity.Substring(1, strCapacity.Length - 2) : "0";
                    decCapacity -= (Convert.ToDecimal(strCapacity, GlobalOptions.Instance.CultureInfo) * objChildGear.Quantity);
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
                    return (0.0m).ToString("#,0.##", GlobalOptions.Instance.CultureInfo);
                if (strCapacity.StartsWith("FixedValues("))
                {
                    string[] strValues = strCapacity.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCapacity = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }
                strCapacity = strCapacity.CheapReplace("Capacity", () => Convert.ToDecimal(Parent?.TotalArmorCapacity, GlobalOptions.Instance.CultureInfo).ToString(GlobalOptions.Instance.InvariantCultureInfo))
                    .Replace("Rating", Rating.ToString());
                bool blnSquareBrackets = strCapacity.StartsWith('[');
                if (blnSquareBrackets)
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                //Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity, out bool blnIsSuccess);
                string strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.Instance.CultureInfo) : strCapacity;
                if (blnSquareBrackets)
                    strReturn = '[' + strReturn + ']';

                return strReturn;
            }
        }

        public decimal TotalCapacity
        {
            get
            {
                string strCapacity = CalculatedCapacity;
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
                return Convert.ToDecimal(strCapacity, GlobalOptions.Instance.CultureInfo);
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
	    /// Total cost of the Armor Mod.
	    /// </summary>
	    public decimal StolenTotalCost
	    {
	        get
	        {
	            decimal decReturn = 0;
	            if (Stolen) decReturn += OwnCost;

	            // Go through all of the Gear for this piece of Armor and add the Cost value.
	            decReturn += Gear.Where(g => g.Stolen).AsParallel().Sum(objGear => objGear.StolenTotalCost);

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
                string strCostExpr = Cost;
                if (strCostExpr.StartsWith("FixedValues("))
                {
                    string[] strValues = strCostExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCostExpr = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                StringBuilder objCost = new StringBuilder(strCostExpr.TrimStart('+'));
                objCost.CheapReplace(strCostExpr, "Rating", () => Rating.ToString(GlobalOptions.Instance.InvariantCultureInfo));
                objCost.CheapReplace(strCostExpr, "Armor Cost", () => (Parent?.OwnCost ?? 0.0m).ToString(GlobalOptions.Instance.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCostExpr, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objCost.CheapReplace(strCostExpr, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                decimal decReturn = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.Instance.InvariantCultureInfo) : 0;

                if (DiscountCost)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode != null && strLanguage == _strCachedXmlNodeLanguage && !GlobalOptions.Instance.LiveCustomData) return _objCachedMyXmlNode;
            _objCachedMyXmlNode = SourceID == Guid.Empty
                ? XmlManager.Load("armor.xml", strLanguage)
                    .SelectSingleNode($"/chummer/mods/mod[name = \"{Name}\"]")
                : XmlManager.Load("armor.xml", strLanguage)
                    .SelectSingleNode($"/chummer/mods/mod[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method to delete an Armor object. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteArmorMod()
        {
            decimal decReturn = 0.0m;
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

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod, InternalId);
            // Remove any Improvements created by the Armor's Gear.
            foreach (Gear objGear in Gear)
                decReturn += objGear.DeleteGear();

            return decReturn;
        }

        /// <summary>
        /// Toggle the Wireless Bonus for this armor mod.
        /// </summary>
        /// <param name="enable"></param>
        public void ToggleWirelessBonuses(bool enable)
        {
            if (enable)
            {
                if (WirelessBonus?.Attributes?.Count > 0)
                {
                    if (WirelessBonus.Attributes["mode"].InnerText == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId).ToList());
                    }
                }
                if (WirelessBonus?.InnerText != null)
                {
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod,
                        _guiID.ToString("D") + "Wireless", WirelessBonus, false, Rating,
                        DisplayNameShort(GlobalOptions.Language));
                }

                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                    _strExtra = ImprovementManager.SelectedValue;
            }
            else
            {
                if (WirelessBonus?.Attributes?.Count > 0)
                {
                    if (WirelessBonus.Attributes?["mode"].InnerText == "replace")
                    {
                        ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId).ToList());
                    }
                }
                ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId + "Wireless").ToList());
            }
        }


        /// <summary>
        /// Checks a nominated piece of gear for Availability requirements.
        /// </summary>
        /// <param name="blnRestrictedGearUsed">Whether Restricted Gear is already being used.</param>
        /// <param name="intRestrictedCount">Amount of gear that is currently over the availability limit.</param>
        /// <param name="strAvailItems">String used to list names of gear that are currently over the availability limit.</param>
        /// <param name="strRestrictedItem">Item that is being used for Restricted Gear.</param>
        /// <param name="blnOutRestrictedGearUsed">Whether Restricted Gear is already being used (tracked across gear children).</param>
        /// <param name="intOutRestrictedCount">Amount of gear that is currently over the availability limit (tracked across gear children).</param>
        /// <param name="strOutAvailItems">String used to list names of gear that are currently over the availability limit (tracked across gear children).</param>
        /// <param name="strOutRestrictedItem">Item that is being used for Restricted Gear (tracked across gear children).</param>
        public void CheckRestrictedGear(bool blnRestrictedGearUsed, int intRestrictedCount, string strAvailItems, string strRestrictedItem, out bool blnOutRestrictedGearUsed, out int intOutRestrictedCount, out string strOutAvailItems, out string strOutRestrictedItem)
        {
            AvailabilityValue objTotalAvail = TotalAvailTuple();
            if (!objTotalAvail.AddToParent)
            {
                int intAvailInt = objTotalAvail.Value;
                if (intAvailInt > _objCharacter.MaximumAvailability && !_blnIncludedInArmor)
                {
                    if (intAvailInt <= _objCharacter.RestrictedGear && !blnRestrictedGearUsed)
                    {
                        blnRestrictedGearUsed = true;
                        strRestrictedItem = Parent == null
                            ? DisplayName(GlobalOptions.Language)
                            : $"{DisplayName(GlobalOptions.Language)} ({Parent})";
                    }
                    else
                    {
                        intRestrictedCount++;
                        strAvailItems += Environment.NewLine + "\t\t" + DisplayNameShort(GlobalOptions.Language);
                    }
                }
            }

            foreach (Gear objChild in Gear)
            {
                objChild.CheckRestrictedGear(blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
            }
            strOutAvailItems = strAvailItems;
            intOutRestrictedCount = intRestrictedCount;
            blnOutRestrictedGearUsed = blnRestrictedGearUsed;
            strOutRestrictedItem = strRestrictedItem;
        }
        #endregion

        #region UI Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsArmorMod, ContextMenuStrip cmsArmorGear)
        {
            if (IncludedInArmor && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = string.IsNullOrEmpty(GearCapacity) ? cmsArmorMod : cmsArmorGear,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
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
                if (IncludedInArmor)
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion

        public bool Remove(Character characterObject, bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                if (!characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteArmor",
                    GlobalOptions.Language)))
                    return false;
            }
            DeleteArmorMod();
            return Parent.ArmorMods.Remove(this);
        }

        public void Sell(Character characterObject, decimal percentage)
        {
            // Record the cost of the Armor with the ArmorMod.
            decimal decOriginal = Parent.TotalCost;

            // Create the Expense Log Entry for the sale.
            decimal decAmount = (decOriginal - Parent.TotalCost) * percentage;
            decAmount += DeleteArmorMod() * percentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(characterObject);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldArmorMod", GlobalOptions.Language) + ' ' + DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
            characterObject.ExpenseEntries.AddWithSort(objExpense);
            characterObject.Nuyen += decAmount;

            Parent.ArmorMods.Remove(this);
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

        public bool AllowPasteXml { get; }

        bool ICanPaste.AllowPasteObject(object input)
        {
            throw new NotImplementedException();
        }
    }
}
