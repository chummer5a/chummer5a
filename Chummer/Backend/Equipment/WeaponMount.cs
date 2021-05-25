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
    /// Vehicle Modification.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class WeaponMount : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanSell, ICanEquip, IHasSource, ICanSort, IHasStolenProperty, ICanPaste
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private decimal _decMarkup;
        private string _strAvail = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludeInVehicle;
        private bool _blnEquipped = true;
        private readonly TaggedObservableCollection<Weapon> _lstWeapons = new TaggedObservableCollection<Weapon>();
        private string _strNotes = string.Empty;
        private string _strExtra = string.Empty;
        private string _strAllowedWeaponCategories = string.Empty;
        private bool _blnDiscountCost;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strLimit = string.Empty;
        private int _intSlots;
        private string _strCost = string.Empty;
        private string _strLocation = string.Empty;
        private string _strAllowedWeapons = string.Empty;
        private int _intSortOrder;
        private bool _blnStolen;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private readonly TaggedObservableCollection<VehicleMod> _lstMods = new TaggedObservableCollection<VehicleMod>();

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public WeaponMount(Character character, Vehicle vehicle)
        {
            // Create the GUID for the new VehicleMod.
            _guiID = Guid.NewGuid();
            _objCharacter = character;
            Parent = vehicle;
        }

        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="decMarkup">Discount or markup that applies to the base cost of the mod.</param>
        public void Create(XmlNode objXmlMod, decimal decMarkup = 0)
        {
            if (objXmlMod == null) Utils.BreakIfDebug();
            if (!objXmlMod.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlMod });
                Utils.BreakIfDebug();
            }
            else
                _objCachedMyXmlNode = null;
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetStringFieldQuickly("limit", ref _strLimit);
            objXmlMod.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objXmlMod.TryGetStringFieldQuickly("weaponcategories", ref _strAllowedWeaponCategories);
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);
            if (!objXmlMod.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMod.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
            // Check for a Variable Cost.
            objXmlMod.TryGetStringFieldQuickly("cost", ref _strCost);
            if (!string.IsNullOrEmpty(_strCost))
            {
                if (_strCost.StartsWith("Variable(", StringComparison.Ordinal))
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    string strCost = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
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
                        if (decMax > 1000000)
                            decMax = 1000000;
                        using (frmSelectNumber frmPickNumber = new frmSelectNumber(_objCharacter.Options.MaxNuyenDecimals)
                        {
                            Minimum = decMin,
                            Maximum = decMax,
                            Description = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_SelectVariableCost"), DisplayNameShort(GlobalOptions.Language)),
                            AllowCancel = false
                        })
                        {
                            if (frmPickNumber.ShowDialog(Program.MainForm) == DialogResult.Cancel)
                            {
                                _guiID = Guid.Empty;
                                return;
                            }
                            _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                        }
                    }
                }
            }
            _decMarkup = decMarkup;

            objXmlMod.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMod.TryGetStringFieldQuickly("page", ref _strPage);
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail = _objCachedSourceDetail ?? new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language, GlobalOptions.CultureInfo, _objCharacter);

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("weaponmount");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("slots", _intSlots.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("markup", _decMarkup.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("weaponmountcategories", _strAllowedWeaponCategories);
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstWeapons)
            {
                objWeapon.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weaponmountoptions");
            foreach (WeaponMountOption objOption in WeaponMountOptions)
            {
                objOption.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("mods");
            foreach (VehicleMod objMod in _lstMods)
            {
                objMod.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();

            if (!IncludedInVehicle)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the VehicleMod from the XmlNode, returning true if load was successful.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Indicates whether a new item will be created as a copy of this one.</param>
        public bool Load(XmlNode objNode, bool blnCopy = false)
        {
            if (objNode == null)
                return false;
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                if (node != null)
                    node.TryGetGuidFieldQuickly("id", ref _guiSourceID);
                else if (string.IsNullOrEmpty(Name))
                    return false; // No source ID, name, or node means this is probably a malformed weapon mount, stop it from loading
            }

            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
            objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strAllowedWeaponCategories);
            objNode.TryGetStringFieldQuickly("allowedweapons", ref _strAllowedWeapons);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetDecFieldQuickly("markup", ref _decMarkup);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludeInVehicle);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            if (!_blnEquipped)
            {
                objNode.TryGetBoolFieldQuickly("installed", ref _blnEquipped);
            }

            XmlNode xmlChildrenNode = objNode["weapons"];
            using (XmlNodeList xmlWeaponList = xmlChildrenNode?.SelectNodes("weapon"))
            {
                if (xmlWeaponList != null)
                {
                    foreach (XmlNode xmlWeaponNode in xmlWeaponList)
                    {
                        Weapon objWeapon = new Weapon(_objCharacter)
                        {
                            ParentVehicle = Parent,
                            ParentMount = this
                        };
                        objWeapon.Load(xmlWeaponNode, blnCopy);
                        _lstWeapons.Add(objWeapon);
                    }
                }
            }

            xmlChildrenNode = objNode["weaponmountoptions"];
            using (XmlNodeList xmlWeaponMountOptionList = xmlChildrenNode?.SelectNodes("weaponmountoption"))
            {
                if (xmlWeaponMountOptionList != null)
                {
                    foreach (XmlNode xmlWeaponMountOptionNode in xmlWeaponMountOptionList)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        objWeaponMountOption.Load(xmlWeaponMountOptionNode);
                        WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }
            }

            xmlChildrenNode = objNode["mods"];
            using (XmlNodeList xmlModList = xmlChildrenNode?.SelectNodes("mod"))
            {
                if (xmlModList != null)
                {
                    foreach (XmlNode xmlModNode in xmlModList)
                    {
                        VehicleMod objMod = new VehicleMod(_objCharacter)
                        {
                            Parent = Parent,
                            WeaponMountParent = this
                        };
                        objMod.Load(xmlModNode);
                        _lstMods.Add(objMod);
                    }
                }
            }

            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);

            return true;
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("mod");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("limit", Limit);
            objWriter.WriteElementString("slots", Slots.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("source", _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("included", IncludedInVehicle.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in Weapons)
            {
                objWeapon.Print(objWriter, objCulture, strLanguageToPrint);
            }
            foreach (VehicleMod objVehicleMod in Mods)
            {
                objVehicleMod.Print(objWriter, objCulture, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            if (GlobalOptions.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        /// <summary>
        /// Create a weapon mount using names instead of IDs, because user readability is important and untrustworthy.
        /// </summary>
        /// <param name="xmlNode"></param>
        public void CreateByName(XmlNode xmlNode)
        {
            if (xmlNode == null)
                throw new ArgumentNullException(nameof(xmlNode));
            XmlDocument xmlDoc = _objCharacter.LoadData("vehicles.xml");
            WeaponMount objMount = this;
            string strSize = xmlNode["size"]?.InnerText;
            if (string.IsNullOrEmpty(strSize))
                return;
            XmlNode xmlDataNode = xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[name = " + strSize.CleanXPath() + " and category = \"Size\"]");
            if (xmlDataNode != null)
            {
                objMount.Create(xmlDataNode);

                string strFlexibility = xmlNode["flexibility"]?.InnerText;
                if (!string.IsNullOrEmpty(strFlexibility))
                {
                    xmlDataNode = xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[name = " + strFlexibility.CleanXPath() + " and category = \"Flexibility\"]");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        objWeaponMountOption.Create(xmlDataNode);
                        objWeaponMountOption.IncludedInParent = true;
                        objMount.WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                string strControl = xmlNode["control"]?.InnerText;
                if (!string.IsNullOrEmpty(strControl))
                {
                    xmlDataNode = xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[name = " + strControl.CleanXPath() + " and category = \"Control\"]");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        objWeaponMountOption.Create(xmlDataNode);
                        objWeaponMountOption.IncludedInParent = true;
                        objMount.WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                string strVisibility = xmlNode["visibility"]?.InnerText;
                if (!string.IsNullOrEmpty(strVisibility))
                {
                    xmlDataNode = xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[name = " + strVisibility.CleanXPath() + " and category = \"Visibility\"]");
                    if (xmlDataNode != null)
                    {
                        WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                        objWeaponMountOption.Create(xmlDataNode);
                        objWeaponMountOption.IncludedInParent = true;
                        objMount.WeaponMountOptions.Add(objWeaponMountOption);
                    }
                }

                _strLocation = xmlNode["location"]?.InnerText ?? string.Empty;
                _strAllowedWeapons = xmlNode["allowedweapons"]?.InnerText ?? string.Empty;
                xmlDataNode = xmlNode["mods"];
                if (xmlDataNode == null)
                    return;
                using (XmlNodeList xmlModList = xmlDataNode.SelectNodes("mod"))
                {
                    if (xmlModList != null)
                    {
                        foreach (XmlNode xmlModNode in xmlModList)
                        {
                            VehicleMod objMod = new VehicleMod(_objCharacter)
                            {
                                Parent = Parent,
                                WeaponMountParent = this
                            };
                            xmlDataNode = xmlDoc.SelectSingleNode("/chummer/weaponmountmods/mod[name = " + xmlModNode.InnerText.CleanXPath() + "]");
                            objMod.Load(xmlDataNode);
                            _lstMods.Add(objMod);
                        }
                    }
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapons.
        /// </summary>
        public TaggedObservableCollection<Weapon> Weapons => _lstWeapons;

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);
        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value) return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// Where the mount is physically located on the vehicle.
        /// </summary>
        public string Location
        {
            get => _strLocation;
            set => _strLocation = value;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("vehicles.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set => _strCategory = value;
        }

        /// <summary>
        /// Limits the Weapon Selection form to specified categories.
        /// </summary>
        public string AllowedWeaponCategories
        {
            set => _strAllowedWeaponCategories = value;
            get => _strAllowedWeaponCategories;
        }

        public string AllowedWeapons
        {
            get => _strAllowedWeapons;
            set => _strAllowedWeapons = value;
        }

        /// <summary>
        /// Which Vehicle types the Mod is limited to.
        /// </summary>
        public string Limit
        {
            get => _strLimit;
            set => _strLimit = value;
        }

        /// <summary>
        /// Number of Slots the Mod uses.
        /// </summary>
        public int Slots
        {
            get => _intSlots;
            set => _intSlots = value;
        }

        /// <summary>
        /// Cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        /// <summary>
        /// Markup.
        /// </summary>
        public decimal Markup
        {
            get => _decMarkup;
            set => _decMarkup = value;
        }

        /// <summary>
        /// Availability.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// Sourcebook.
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
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Whether or not the Mod included with the Vehicle by default.
        /// </summary>
        public bool IncludedInVehicle
        {
            get => _blnIncludeInVehicle;
            set => _blnIncludeInVehicle = value;
        }

        /// <summary>
        /// Whether or not this Mod is installed and contributing towards the Vehicle's stats.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set => _blnEquipped = value;
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
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = value;
        }

        /// <summary>
        /// Whether or not the Vehicle Mod's cost should be discounted by 10% through the Black Market Pipeline Quality.
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
        /// Vehicle that the Mod is attached to.
        /// </summary>
        public Vehicle Parent { get; }

        /// <summary>
        /// 
        /// </summary>
        public List<WeaponMountOption> WeaponMountOptions { get; } = new List<WeaponMountOption>(3);

        /// <summary>
        /// Is the object stolen via the Stolen Gear quality?
        /// </summary>
        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }
        #endregion

        #region Complex Properties

        /// <summary>
        /// The number of Slots the Mount consumes, including all child items.
        /// </summary>
        public int CalculatedSlots => Slots + WeaponMountOptions.Sum(w => w.Slots) + Mods.AsParallel().Sum(m => m.CalculatedSlots);

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);

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
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');

                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }

                objAvail.CheapReplace(strAvail, "Vehicle Cost", () => Parent?.OwnCost.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                objAvail.CheapReplace(strAvail, "Body", () => Parent?.Body > 0 ? Parent.Body.ToString(GlobalOptions.InvariantCultureInfo) : "0.5");
                objAvail.CheapReplace(strAvail, "Speed", () => Parent?.Speed.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                objAvail.CheapReplace(strAvail, "Acceleration", () => Parent?.Accel.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                objAvail.CheapReplace(strAvail, "Handling", () => Parent?.Handling.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += ((double)objProcess).StandardRound();
            }

            // Run through the Accessories and add in their availability.
            foreach (WeaponMountOption objWeaponMountOption in WeaponMountOptions)
            {
                AvailabilityValue objLoopAvailTuple = objWeaponMountOption.TotalAvailTuple;
                //if (objLoopAvailTuple.Item3)
                    intAvail += objLoopAvailTuple.Value;
                if (objLoopAvailTuple.Suffix == 'F')
                    chrLastAvailChar = 'F';
                else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                    chrLastAvailChar = 'R';
            }

            if (blnCheckChildren)
            {
                // Run through the Vehicle Mods and add in their availability.
                foreach (VehicleMod objVehicleMod in Mods)
                {
                    if (!objVehicleMod.IncludedInVehicle && objVehicleMod.Equipped)
                    {
                        AvailabilityValue objLoopAvailTuple = objVehicleMod.TotalAvailTuple();
                        if (objLoopAvailTuple.AddToParent)
                            intAvail += objLoopAvailTuple.Value;
                        if (objLoopAvailTuple.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvailTuple.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInVehicle);
        }

        /// <summary>
        /// Total cost of the WeaponMount.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal cost = 0;
                if (!IncludedInVehicle && !Stolen)
                {
                    cost += OwnCost;
                }
                return cost + Weapons.Sum(w => w.TotalCost) + WeaponMountOptions.Sum(w => w.TotalCost) + Mods.Sum(m => m.TotalCost);
            }
        }

        /// <summary>
        /// Total cost of the WeaponMount.
        /// </summary>
        public decimal StolenTotalCost
        {
            get
            {
                decimal d = 0;

                if (!IncludedInVehicle && Stolen)
                {
                    d += OwnCost;
                }

                d += Weapons.Sum(w => w.StolenTotalCost) + WeaponMountOptions.Sum(w => w.StolenTotalCost) +
                     Mods.Sum(m => m.StolenTotalCost);

                return d;
            }
        }

        /// <summary>
        /// The cost of just the Vehicle Mod itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                // If the cost is determined by the Rating, evaluate the expression.
                string strCost = Cost;
                StringBuilder objCost = new StringBuilder(strCost);
                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCost, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objCost.CheapReplace(strCost, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }

                objCost.CheapReplace(strCost, "Vehicle Cost", () => Parent?.OwnCost.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                objCost.CheapReplace(strCost, "Body", () => Parent?.Body > 0 ? Parent.Body.ToString(GlobalOptions.InvariantCultureInfo) : "0.5");
                objCost.CheapReplace(strCost, "Speed", () => Parent?.Speed.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                objCost.CheapReplace(strCost, "Acceleration", () => Parent?.Accel.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                objCost.CheapReplace(strCost, "Handling", () => Parent?.Handling.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                decimal decReturn = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Apply a markup if applicable.
                if (_decMarkup != 0)
                {
                    decReturn *= 1 + (_decMarkup / 100.0m);
                }

                return decReturn;
            }
        }

        public TaggedObservableCollection<VehicleMod> Mods => _lstMods;

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            StringBuilder strReturn = new StringBuilder(DisplayNameShort(strLanguage));
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (WeaponMountOptions.Count > 0)
            {
                strReturn.Append(strSpace + '(');
                bool blnCloseParantheses = false;
                foreach (WeaponMountOption objOption in WeaponMountOptions)
                {
                    if (objOption.Name != "None")
                    {
                        blnCloseParantheses = true;
                        strReturn.Append(objOption.DisplayName(strLanguage));
                        strReturn.Append(',' + strSpace);
                    }
                }
                strReturn.Length -= 1 + strSpace.Length;
                if (blnCloseParantheses)
                    strReturn.Append(')');
                if (!string.IsNullOrWhiteSpace(Location))
                {
                    strReturn.Append(strSpace + '-' + strSpace + Location);
                }
            }

            return strReturn.ToString();
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = _objCharacter.LoadData("vehicles.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? "/chummer/weaponmounts/weaponmount[name = " + Name.CleanXPath() + ']'
                        : string.Format(GlobalOptions.InvariantCultureInfo,
                            "/chummer/weaponmounts/weaponmount[id = {0} or id = {1}]",
                            SourceIDString.CleanXPath(), SourceIDString.ToUpperInvariant().CleanXPath()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public decimal DeleteWeaponMount()
        {
            decimal decReturn = 0;

            foreach (Weapon objLoopWeapon in Weapons)
            {
                decReturn += objLoopWeapon.DeleteWeapon();
            }
            foreach (VehicleMod objLoopMod in Mods)
            {
                decReturn += objLoopMod.DeleteVehicleMod();
            }

            return decReturn;
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
        public void CheckRestrictedGear(bool blnRestrictedGearUsed, int intRestrictedCount, string strAvailItems,
            string strRestrictedItem, out bool blnOutRestrictedGearUsed,
            out int intOutRestrictedCount, out string strOutAvailItems, out string strOutRestrictedItem)
        {
            if (!IncludedInVehicle)
            {
                AvailabilityValue objTotalAvail = TotalAvailTuple();
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = objTotalAvail.Value;
                    //TODO: Make this dynamically update without having to validate the character.
                    if (intAvailInt > _objCharacter.Options.MaximumAvailability)
                    {
                        if (intAvailInt <= _objCharacter.RestrictedGear && !blnRestrictedGearUsed)
                        {
                            blnRestrictedGearUsed = true;
                            strRestrictedItem = Parent == null
                                ? CurrentDisplayName
                                : string.Format(GlobalOptions.CultureInfo, "{0}{1}({2})",
                                    CurrentDisplayName, LanguageManager.GetString("String_Space"), Parent.CurrentDisplayName);
                        }
                        else
                        {
                            intRestrictedCount++;
                            strAvailItems += Environment.NewLine + "\t\t" + DisplayNameShort(GlobalOptions.Language);
                        }
                    }
                }
            }

            foreach (Weapon objChild in Weapons)
            {
                objChild.CheckRestrictedGear(blnRestrictedGearUsed, intRestrictedCount, strAvailItems,
                    strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems,
                    out strRestrictedItem);
            }

            foreach (WeaponMountOption objChild in WeaponMountOptions)
            {
                objChild.CheckRestrictedGear(blnRestrictedGearUsed, intRestrictedCount, strAvailItems,
                    strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount,
                    out strAvailItems, out strRestrictedItem);
            }
            strOutAvailItems = strAvailItems;
            intOutRestrictedCount = intRestrictedCount;
            blnOutRestrictedGearUsed = blnRestrictedGearUsed;
            strOutRestrictedItem = strRestrictedItem;
        }
        #region UI Methods
        /// <summary>
        /// Add a Weapon Mount to the TreeView
        /// </summary>
        /// <param name="cmsVehicleWeapon">ContextMenuStrip for Vehicle Weapons</param>
        /// <param name="cmsVehicleWeaponAccessory">ContextMenuStrip for Vehicle Weapon Accessories</param>
        /// <param name="cmsVehicleWeaponAccessoryGear">ContextMenuStrip for Vehicle Weapon Gear</param>
        /// <param name="cmsVehicleWeaponMount">ContextMenuStrip for Vehicle Weapon Mounts</param>
        /// <param name="cmsCyberware">ContextMenuStrip for Cyberware.</param>
        /// <param name="cmsCyberwareGear">ContextMenuStrip for Gear in Cyberware.</param>
        /// <param name="cmsVehicleMod">ContextMenuStrip for Vehicle Mods.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleMod)
        {
            if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsVehicleWeaponMount,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // VehicleMods.
            foreach (VehicleMod objMod in Mods)
            {
                TreeNode objLoopNode = objMod.CreateTreeNode(cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }
            foreach (Weapon objWeapon in Weapons)
            {
                TreeNode objLoopNode = objWeapon.CreateTreeNode(cmsVehicleWeapon, cmsVehicleWeaponAccessory, cmsVehicleWeaponAccessoryGear);
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
                    return IncludedInVehicle
                        ? ColorManager.GrayHasNotesColor
                        : ColorManager.HasNotesColor;
                }
                return IncludedInVehicle
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }
        #endregion
        #endregion

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeaponMount")))
                    return false;
            }

            DeleteWeaponMount();
            return Parent.WeaponMounts.Remove(this);
        }

        public void Sell(decimal percentage)
        {
            // Record the cost of the Armor with the ArmorMod.
            decimal decOriginal = Parent.TotalCost;

            // Create the Expense Log Entry for the sale.
            decimal decAmount = (decOriginal - Parent.TotalCost) * percentage;
            decAmount += DeleteWeaponMount() * percentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldArmorMod") + ' ' + DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen += decAmount;

            Parent.WeaponMounts.Remove(this);
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }

        public bool AllowPasteXml
        {
            get
            {
                switch (GlobalOptions.ClipboardContentType)
                {
                    case ClipboardContentType.Weapon:
                    {
                        if (!string.IsNullOrEmpty(AllowedWeapons))
                        {
                            string strCheckValue = GlobalOptions.Clipboard.SelectSingleNode("name")?.InnerText;
                            if (string.IsNullOrEmpty(strCheckValue) || !AllowedWeapons.Contains(strCheckValue))
                                return false;
                        }
                        if (!string.IsNullOrEmpty(AllowedWeaponCategories))
                        {
                            string strCheckValue = GlobalOptions.Clipboard.SelectSingleNode("category")?.InnerText;
                            if (string.IsNullOrEmpty(strCheckValue) || !AllowedWeaponCategories.Contains(strCheckValue))
                                return false;
                        }

                        return Weapons.Count == 0; //todo: something something sizes
                    }
                    default:
                        return false;
                }
            }
        }

        public bool AllowPasteObject(object input)
        {
            throw new NotImplementedException();
        }
    }

    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class WeaponMountOption : IHasName, IHasXmlNode
    {
        private readonly Character _objCharacter;
        private string _strAvail;
        private string _strName;
        private Guid _guiSourceID;
        private Guid _guiID;
        private string _strCost;
        private string _strCategory;
        private int _intSlots;
        private string _strAllowedWeaponCategories;
        private string _strAllowedWeapons;
        private bool _blnIncludedInParent;

        #region Constructor, Create, Save and Load Methods
        public WeaponMountOption(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a Weapon Mount Option from an XmlNode, returns true if creation was successful.
        /// </summary>
        /// <param name="objXmlMod">XmlNode of the option.</param>
        public bool Create(XmlNode objXmlMod)
        {
            if (objXmlMod == null)
            {
                Utils.BreakIfDebug();
                return false;
            }

            _guiID = Guid.NewGuid();
            objXmlMod.TryGetField("id", Guid.TryParse, out _guiSourceID);
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objXmlMod.TryGetStringFieldQuickly("weaponcategories", ref _strAllowedWeaponCategories);
            objXmlMod.TryGetStringFieldQuickly("weapons", ref _strAllowedWeapons);
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);

            // Check for a Variable Cost.
            // ReSharper disable once PossibleNullReferenceException
            _strCost = objXmlMod["cost"]?.InnerText ?? "0";
            if (_strCost.StartsWith("Variable(", StringComparison.Ordinal))
            {
                int intMin;
                int intMax = 0;
                string strCost = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                if (strCost.Contains('-'))
                {
                    string[] strValues = strCost.Split('-');
                    intMin = Convert.ToInt32(strValues[0], GlobalOptions.InvariantCultureInfo);
                    intMax = Convert.ToInt32(strValues[1], GlobalOptions.InvariantCultureInfo);
                }
                else
                    intMin = Convert.ToInt32(strCost.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                if (intMin != 0 || intMax != 0)
                {
                    if (intMax == 0)
                        intMax = 1000000;
                    using (frmSelectNumber frmPickNumber = new frmSelectNumber(_objCharacter.Options.MaxNuyenDecimals)
                    {
                        Minimum = intMin,
                        Maximum = intMax,
                        Description = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_SelectVariableCost"), CurrentDisplayName),
                        AllowCancel = false
                    })
                    {
                        if (frmPickNumber.ShowDialog(Program.MainForm) == DialogResult.Cancel)
                            return false;
                        _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                    }
                }
            }
            return true;
        }

        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        public string DisplayName(string strLanguage)
        {
            return DisplayNameShort(strLanguage);
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("weaponmountoption");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalID);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("slots", _intSlots.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("includedinparent", _blnIncludedInParent.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Weapon Mount Option from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            _objCachedMyXmlNode = null;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strAllowedWeaponCategories);
            objNode.TryGetStringFieldQuickly("allowedweapons", ref _strAllowedWeapons);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetBoolFieldQuickly("includedinparent", ref _blnIncludedInParent);
        }
        #endregion

        #region Properties

        public string InternalID => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value) return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// The cost of just the WeaponMountOption itself.
        /// </summary>
        public decimal Cost
        {
            get
            {
                string strCost = _strCost;
                StringBuilder objCost = new StringBuilder(strCost);
                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCost, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objCost.CheapReplace(strCost, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
            }
        }

        public decimal TotalCost => IncludedInParent ? 0 : Cost;
        /// <summary>
        /// Slots consumed by the WeaponMountOption.
        /// </summary>
        public int Slots => _intSlots;

        /// <summary>
        /// Availability string of the Mount.
        /// </summary>
        public string Avail => _strAvail;

        /// <summary>
        /// Category of the weapon mount.
        /// </summary>
        public string Category => _strCategory;

        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        public int StolenTotalCost { get; set; }

        /// <summary>
        /// Does the option come with the parent object?
        /// </summary>
        public bool IncludedInParent
        {
            get => _blnIncludedInParent;
            set => _blnIncludedInParent = value;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Display text for the category of the weapon mount.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Category;

            return _objCharacter.LoadDataXPath("vehicles.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = " + Category.CleanXPath() + "]/@translate")?.Value ?? Category;
        }

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(CultureInfo objCulture, string strLanguage)
        {
            return TotalAvailTuple.ToString(objCulture, strLanguage);
        }

        /// <summary>
        /// Total Availability as a triple.
        /// </summary>
        public AvailabilityValue TotalAvailTuple
        {
            get
            {
                bool blnModifyParentAvail = false;
                string strAvail = Avail;
                char chrLastAvailChar = ' ';
                int intAvail = 0;
                if (strAvail.Length > 0)
                {
                    chrLastAvailChar = strAvail[strAvail.Length - 1];
                    if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                    {
                        strAvail = strAvail.Substring(0, strAvail.Length - 1);
                    }

                    blnModifyParentAvail = strAvail.StartsWith('+', '-');

                    StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));

                    foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                    {
                        objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                        objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                    }

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                    if (blnIsSuccess)
                        intAvail += ((double)objProcess).StandardRound();
                }

                return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
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
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = _objCharacter.LoadData("vehicles.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? "/chummer/weaponmounts/weaponmount[name = " + Name.CleanXPath() + ']'
                        : string.Format(GlobalOptions.InvariantCultureInfo,
                            "/chummer/weaponmounts/weaponmount[id = {0} or id = {1}]",
                            SourceIDString.CleanXPath(), SourceIDString.ToUpperInvariant().CleanXPath()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion
        #region Methods

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
        public void CheckRestrictedGear(bool blnRestrictedGearUsed, int intRestrictedCount, string strAvailItems,
            string strRestrictedItem, out bool blnOutRestrictedGearUsed,
            out int intOutRestrictedCount, out string strOutAvailItems, out string strOutRestrictedItem)
        {
            AvailabilityValue objTotalAvail = TotalAvailTuple;
            if (!objTotalAvail.AddToParent)
            {
                int intAvailInt = objTotalAvail.Value;
                if (intAvailInt > _objCharacter.Options.MaximumAvailability)
                {
                    if (intAvailInt <= _objCharacter.RestrictedGear && !blnRestrictedGearUsed)
                    {
                        blnRestrictedGearUsed = true;
                        strRestrictedItem = CurrentDisplayName;
                    }
                    else
                    {
                        intRestrictedCount++;
                        strAvailItems += Environment.NewLine + "\t\t" + DisplayNameShort(GlobalOptions.Language);
                    }
                }
            }
            strOutAvailItems = strAvailItems;
            intOutRestrictedCount = intRestrictedCount;
            blnOutRestrictedGearUsed = blnRestrictedGearUsed;
            strOutRestrictedItem = strRestrictedItem;
        }
        #endregion
    }
}
