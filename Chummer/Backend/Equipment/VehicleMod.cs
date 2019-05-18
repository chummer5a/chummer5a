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
using Chummer.Backend.Attributes;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Vehicle Modification.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class VehicleMod : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanEquip, IHasSource, IHasRating, ICanSort, IHasStolenProperty
    {
        private Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strLimit = string.Empty;
        private string _strSlots = "0";
        private int _intRating;
        private string _strMaxRating = "0";
        private string _strCost = string.Empty;
        private decimal _decMarkup;
        private string _strAvail = string.Empty;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = false;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludeInVehicle;
        private bool _blnEquipped = true;
        private int _intConditionMonitor;
        private readonly TaggedObservableCollection<Weapon> _lstVehicleWeapons = new TaggedObservableCollection<Weapon>();
        private string _strNotes = string.Empty;
        private string _strSubsystems = string.Empty;
        private readonly TaggedObservableCollection<Cyberware> _lstCyberware = new TaggedObservableCollection<Cyberware>();
        private string _strExtra = string.Empty;
        private string _strWeaponMountCategories = string.Empty;
        private bool _blnDiscountCost;
        private bool _blnDowngrade;
        private string _strCapacity = string.Empty;

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private string _strAmmoReplace;
        private int _intAmmoBonus;
        private int _intSortOrder;
        private bool _blnStolen;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public VehicleMod(Character objCharacter)
        {
            // Create the GUID for the new VehicleMod.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _lstVehicleWeapons.CollectionChanged += ChildrenWeaponsOnCollectionChanged;
            _lstCyberware.CollectionChanged += ChildrenCyberwareOnCollectionChanged;
        }

        private void ChildrenCyberwareOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Cyberware objNewItem in e.NewItems)
                        objNewItem.ParentVehicle = Parent;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Cyberware objOldItem in e.OldItems)
                        objOldItem.ParentVehicle = null;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Cyberware objOldItem in e.OldItems)
                        objOldItem.ParentVehicle = null;
                    foreach (Cyberware objNewItem in e.NewItems)
                        objNewItem.ParentVehicle = Parent;
                    break;
            }
        }

        private void ChildrenWeaponsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Weapon objNewItem in e.NewItems)
                        objNewItem.ParentVehicle = Parent;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Weapon objOldItem in e.OldItems)
                        objOldItem.ParentVehicle = null;
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Weapon objOldItem in e.OldItems)
                        objOldItem.ParentVehicle = null;
                    foreach (Weapon objNewItem in e.NewItems)
                        objNewItem.ParentVehicle = Parent;
                    break;
            }
        }

        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="objParent">Vehicle that the mod will be attached to.</param>
        /// <param name="decMarkup">Discount or markup that applies to the base cost of the mod.</param>
        /// <param name="strForcedValue">Value to forcefully select for any ImprovementManager prompts.</param>
        public void Create(XmlNode objXmlMod, int intRating, Vehicle objParent, decimal decMarkup = 0, string strForcedValue = "")
        {
            Parent = objParent ?? throw new ArgumentNullException(nameof(objParent));
            if (objXmlMod == null) Utils.BreakIfDebug();
            if (!objXmlMod.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlMod });
                Utils.BreakIfDebug();
            }
            else
                _objCachedMyXmlNode = null;
            if (objXmlMod.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetStringFieldQuickly("limit", ref _strLimit);
            objXmlMod.TryGetStringFieldQuickly("slots", ref _strSlots);
            _intRating = intRating;
            _blnDowngrade = objXmlMod?["downgrade"] != null;
            if (!objXmlMod.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMod.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlMod.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objXmlMod.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            objXmlMod.TryGetInt32FieldQuickly("conditionmonitor", ref _intConditionMonitor);
            objXmlMod.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
            objXmlMod.TryGetStringFieldQuickly("ammoreplace", ref _strAmmoReplace);
            objXmlMod.TryGetInt32FieldQuickly("ammobonus", ref _intAmmoBonus);
            // Add Subsytem information if applicable.
            XmlNode xmlSubsystemsNode = objXmlMod?["subsystems"];
            if (xmlSubsystemsNode != null)
            {
                StringBuilder objSubsystem = new StringBuilder();
                using (XmlNodeList xmlSubsystemList = xmlSubsystemsNode.SelectNodes("subsystem"))
                    if (xmlSubsystemList != null)
                        foreach (XmlNode objXmlSubsystem in xmlSubsystemList)
                        {
                            objSubsystem.Append(objXmlSubsystem.InnerText + ",");
                        }
                // Remove last ","
                if (objSubsystem.Length > 0)
                    objSubsystem.Length -= 1;
                _strSubsystems = objSubsystem.ToString();
            }
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);

            _strCost = objXmlMod?["cost"]?.InnerText ?? string.Empty;
            // Check for a Variable Cost.
            if (_strCost.StartsWith("Variable("))
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
            }
            _decMarkup = decMarkup;

            objXmlMod.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMod.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objXmlMod?["bonus"];
            _nodWirelessBonus = objXmlMod?["wirelessbonus"];
            _blnWirelessOn = false;

            if (Bonus != null)
            {
                ImprovementManager.ForcedValue = strForcedValue;
                if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.VehicleMod, InternalId, Bonus, false, intRating, DisplayNameShort(GlobalOptions.Language), false))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                {
                    _strExtra = ImprovementManager.SelectedValue;
                }
            }
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail ?? (_objCachedSourceDetail =
                                                new SourceString(Source, Page(GlobalOptions.Language), GlobalOptions.Language));

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("mod");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("slots", _strSlots);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("conditionmonitor", _intConditionMonitor.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("markup", _decMarkup.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString());
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteElementString("subsystems", _strSubsystems);
            objWriter.WriteElementString("weaponmountcategories", _strWeaponMountCategories);
            objWriter.WriteElementString("ammobonus", _intAmmoBonus.ToString());
            objWriter.WriteElementString("ammoreplace", _strAmmoReplace);
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstVehicleWeapons)
                objWeapon.Save(objWriter);
            objWriter.WriteEndElement();
            if (_lstCyberware.Count > 0)
            {
                objWriter.WriteStartElement("cyberwares");
                foreach (Cyberware objCyberware in _lstCyberware)
                    objCyberware.Save(objWriter);
                objWriter.WriteEndElement();
            }
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString());
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString());
            objWriter.WriteElementString("stolen", _blnStolen.ToString());
            objWriter.WriteEndElement();

            if (!IncludedInVehicle)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the VehicleMod from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Indicates whether a new item will be created as a copy of this one.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
            objNode.TryGetStringFieldQuickly("slots", ref _strSlots);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetInt32FieldQuickly("conditionmonitor", ref _intConditionMonitor);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetDecFieldQuickly("markup", ref _decMarkup);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludeInVehicle);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            if (!_blnEquipped)
            {
                objNode.TryGetBoolFieldQuickly("installed", ref _blnEquipped);
            }
            objNode.TryGetInt32FieldQuickly("ammobonus", ref _intAmmoBonus);
            objNode.TryGetStringFieldQuickly("ammoreplace", ref _strAmmoReplace);
            objNode.TryGetStringFieldQuickly("subsystems", ref _strSubsystems);
            // Legacy Shims
            if (Name.StartsWith("Gecko Tips (Bod"))
            {
                Name = "Gecko Tips";
                XmlNode objNewNode = GetNode();
                if (objNewNode != null)
                {
                    objNewNode.TryGetStringFieldQuickly("cost", ref _strCost);
                    objNewNode.TryGetStringFieldQuickly("slots", ref _strSlots);
                }
            }
            if (Name.StartsWith("Gliding System (Bod"))
            {
                Name = "Gliding System";
                XmlNode objNewNode = GetNode();
                if (objNewNode != null)
                {
                    objNewNode.TryGetStringFieldQuickly("cost", ref _strCost);
                    objNewNode.TryGetStringFieldQuickly("slots", ref _strSlots);
                    objNewNode.TryGetStringFieldQuickly("avail", ref _strAvail);
                }
            }

            XmlNode xmlChildrenNode = objNode["weapons"];
            using (XmlNodeList xmlNodeList = xmlChildrenNode?.SelectNodes("weapon"))
                if (xmlNodeList != null)
                    foreach (XmlNode nodChild in xmlNodeList)
                    {
                        Weapon objWeapon = new Weapon(_objCharacter)
                        {
                            ParentVehicle = Parent
                        };
                        objWeapon.Load(nodChild, blnCopy);
                        _lstVehicleWeapons.Add(objWeapon);
                    }

            xmlChildrenNode = objNode["cyberwares"];
            using (XmlNodeList xmlNodeList = xmlChildrenNode?.SelectNodes("cyberware"))
                if (xmlNodeList != null)
                    foreach (XmlNode nodChild in xmlNodeList)
                    {
                        Cyberware objCyberware = new Cyberware(_objCharacter)
                        {
                            ParentVehicle = Parent
                        };
                        objCyberware.Load(nodChild, blnCopy);
                        _lstCyberware.Add(objCyberware);
                    }

            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = false;
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("mod");
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("limit", Limit);
            objWriter.WriteElementString("slots", Slots);
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("wirelesson", WirelessOn.ToString());
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("included", IncludedInVehicle.ToString());
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in Weapons)
                objWeapon.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("cyberwares");
            foreach (Cyberware objCyberware in Cyberware)
                objCyberware.Print(objWriter, objCulture, strLanguageToPrint);
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapons.
        /// </summary>
        public TaggedObservableCollection<Weapon> Weapons => _lstVehicleWeapons;

        public TaggedObservableCollection<Cyberware> Cyberware => _lstCyberware;

        public WeaponMount WeaponMountParent { get; set; }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D");

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    _objCachedMyXmlNode = null;
                    _strName = value;
                }
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("vehicles.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
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
        public string WeaponMountCategories
        {
            set => _strWeaponMountCategories = value;
            get => _strWeaponMountCategories;
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
        public string Slots
        {
            get => _strSlots;
            set => _strSlots = value;
        }

        /// <summary>
        /// Vehicle Mod capacity.
        /// </summary>
        public string Capacity
        {
            get => _strCapacity;
            set => _strCapacity = value;
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set
            {
                int intNewRating = Math.Max(0, value);
                if (_intRating != intNewRating)
                {
                    _intRating = intNewRating;
                    if (!IncludedInVehicle && Equipped && _objCharacter.IsAI && _objCharacter.HomeNode is Vehicle)
                        _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
                }
            }
        }

        /// <summary>
        /// Maximum Rating.
        /// </summary>
        public string MaxRating
        {
            get => _strMaxRating;
            set => _strMaxRating = value;
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
        public string Page(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// Bonus node.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set
            {
                if (_nodBonus != value)
                {
                    _nodBonus = value;
                    if (!IncludedInVehicle && Equipped && _objCharacter.IsAI && _objCharacter.HomeNode is Vehicle)
                        _objCharacter.OnPropertyChanged(nameof(Character.PhysicalCM));
                }
            }
        }

        /// <summary>
        /// Wireless Bonus node.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set => _nodWirelessBonus = value;
        }

        /// <summary>
        /// Whether the vehicle mod's wireless is enabled
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set => _blnWirelessOn = value;
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
        /// Whether or not the Vehicle Mod allows Cyberware Plugins.
        /// </summary>
        public bool AllowCyberware => !string.IsNullOrEmpty(_strSubsystems);

        /// <summary>
        /// Allowed Cyberwarre Subsystems.
        /// </summary>
        public string Subsystems
        {
            get => _strSubsystems;
            set => _strSubsystems = value;
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
        /// Whether or not the Vehicle Mod is a downgrade for drone attributes
        /// </summary>
        public bool Downgrade => _blnDowngrade;

        /// <summary>
        /// Bonus/Penalty to the parent vehicle that this mod provides.
        /// </summary>
        public int ConditionMonitor => _intConditionMonitor;

        /// <summary>
        /// Vehicle that the Mod is attached to.
        /// </summary>
        public Vehicle Parent { internal get; set; }

        /// <summary>
        /// Adjust the Weapon's Ammo amount by the specified percent.
        /// </summary>
        public int AmmoBonus
        {
            get => _intAmmoBonus;
            set => _intAmmoBonus = value;
        }

        /// <summary>
        /// Replace the Weapon's Ammo value with the Weapon Mod's value.
        /// </summary>
        public string AmmoReplace
        {
            get => _strAmmoReplace;
            set => _strAmmoReplace = value;
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
        /// Total Availablility of the VehicleMod.
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
                // Reordered to process fixed value strings
                if (strAvail.StartsWith("FixedValues("))
                {
                    string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                if (strAvail.StartsWith("Range("))
                {
                    // If the Availability code is based on the current Rating of the item, separate the Availability string into an array and find the first bracket that the Rating is lower than or equal to.
                    string[] strValues = strAvail.Replace("MaxRating", MaxRating).TrimStartOnce("Range(", true).TrimEndOnce(')').Split(',');
                    foreach (string strValue in strValues)
                    {
                        string strAvailCode = strValue.Split('[')[1].Trim('[', ']');
                        int intMax = Convert.ToInt32(strValue.Split('[')[0]);
                        if (Rating > intMax)
                            continue;
                        strAvail = $"{Rating}{strAvailCode}";
                        break;
                    }
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

                // If the availability is determined by the Rating, evaluate the expression.
                objAvail.CheapReplace(strAvail, "Vehicle Cost", () => Parent?.OwnCost.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                objAvail.CheapReplace(strAvail, "Body", () => Parent?.Body > 0 ? Parent.Body.ToString() : "0.5");
                objAvail.CheapReplace(strAvail, "Speed", () => Parent?.Speed.ToString() ?? "0");
                objAvail.CheapReplace(strAvail, "Acceleration", () => Parent?.Accel.ToString() ?? "0");
                objAvail.CheapReplace(strAvail, "Handling", () => Parent?.Handling.ToString() ?? "0");

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess);
            }

            if (blnCheckChildren)
            {
                // Run through cyberware children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Cyberware objChild in Cyberware)
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

                // Run through weapon children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Weapon objChild in Weapons)
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

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
        }

        /// <summary>
        /// Caculated Capacity of the Vehicle Mod.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strReturn = _strCapacity;
                if (string.IsNullOrEmpty(strReturn))
                    return (0.0m).ToString("#,0.##", GlobalOptions.CultureInfo);

                if (strReturn.StartsWith("FixedValues("))
                {
                    string[] strValues = strReturn.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strReturn = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                int intPos = strReturn.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    string strFirstHalf = strReturn.Substring(0, intPos);
                    string strSecondHalf = strReturn.Substring(intPos + 1, strReturn.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.StartsWith('[');

                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                    if (strFirstHalf == "[*]")
                        strReturn = "*";
                    else
                    {
                        if (strFirstHalf.StartsWith("FixedValues("))
                        {
                            string[] strValues = strFirstHalf.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                            strFirstHalf = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                        }

                        try
                        {
                            object objProcess = CommonFunctions.EvaluateInvariantXPath(strFirstHalf.Replace("Rating", Rating.ToString()), out bool blnIsSuccess);
                            strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strFirstHalf;
                        }
                        catch (OverflowException) // Result is text and not a double
                        {
                            strReturn = strFirstHalf;
                        }
                        catch (InvalidCastException) // Result is text and not a double
                        {
                            strReturn = strFirstHalf;
                        }
                    }

                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';

                    if (strSecondHalf.Contains("Rating"))
                    {
                        strSecondHalf = strSecondHalf.Trim('[', ']');
                        try
                        {
                            object objProcess = CommonFunctions.EvaluateInvariantXPath(strSecondHalf.Replace("Rating", Rating.ToString()), out bool blnIsSuccess);
                            strSecondHalf = '[' + (blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strSecondHalf) + ']';
                        }
                        catch (OverflowException) // Result is text and not a double
                        {
                            strSecondHalf = '[' + strSecondHalf + ']';
                        }
                        catch (InvalidCastException) // Result is text and not a double
                        {
                            strSecondHalf = '[' + strSecondHalf + ']';
                        }
                    }

                    strReturn += "/" + strSecondHalf;
                }
                else if (strReturn.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strReturn.StartsWith('[');
                    string strCapacity = strReturn;
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString()), out bool blnIsSuccess);
                    strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strCapacity;
                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                }
                else if (decimal.TryParse(strReturn, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                    return decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);

                return strReturn;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                if (string.IsNullOrEmpty(_strCapacity))
                    return 0.0m;
                decimal decCapacity = 0;
                if (_strCapacity.Contains("/["))
                {
                    // Get the Cyberware base Capacity.
                    string strBaseCapacity = CalculatedCapacity;
                    strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                    decCapacity = Convert.ToDecimal(strBaseCapacity, GlobalOptions.CultureInfo);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Cyberware)
                    {
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }
                }
                else if (!_strCapacity.Contains('['))
                {
                    // Get the Cyberware base Capacity.
                    decCapacity = Convert.ToDecimal(CalculatedCapacity, GlobalOptions.CultureInfo);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Cyberware)
                    {
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                        if (intPos != -1)
                            strCapacity = strCapacity.Substring(intPos + 2, strCapacity.LastIndexOf(']') - intPos - 2);
                        else if (strCapacity.StartsWith('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }
                }

                return decCapacity;
            }
        }

        /// <summary>
        /// Total cost of the VehicleMod.
        /// </summary>
        public decimal TotalCostInMountCreation(int intSlots)
        {
            // If the cost is determined by the Rating, evaluate the expression.
            string strCostExpr = Cost;
            if (strCostExpr.StartsWith("FixedValues("))
            {
                string[] strValues = strCostExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strCostExpr = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
            }

            StringBuilder objCost = new StringBuilder(strCostExpr.TrimStart('+'));
            objCost.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));

            foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
            {
                objCost.CheapReplace(strCostExpr, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                objCost.CheapReplace(strCostExpr, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
            }

            objCost.CheapReplace(strCostExpr, "Vehicle Cost", () => Parent?.OwnCost.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
            // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
            objCost.CheapReplace(strCostExpr, "Body", () => Parent?.Body > 0 ? Parent.Body.ToString() : "0.5");
            objCost.CheapReplace(strCostExpr, "Speed", () => Parent?.Speed.ToString() ?? "0");
            objCost.CheapReplace(strCostExpr, "Acceleration", () => Parent?.Accel.ToString() ?? "0");
            objCost.CheapReplace(strCostExpr, "Handling", () => Parent?.Handling.ToString() ?? "0");
            objCost.Replace("Slots", intSlots.ToString());

            object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
            decimal decReturn = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;

            if (DiscountCost)
                decReturn *= 0.9m;

            // Apply a markup if applicable.
            if (_decMarkup != 0)
            {
                decReturn *= 1 + (_decMarkup / 100.0m);
            }

            return decReturn + Weapons.AsParallel().Sum(objWeapon => objWeapon.TotalCost) + Cyberware.AsParallel().Sum(objCyberware => objCyberware.TotalCost);
        }

        /// <summary>
        /// Total cost of the VehicleMod.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                return OwnCost + Weapons.AsParallel().Sum(objWeapon => objWeapon.TotalCost) + Cyberware.AsParallel().Sum(objCyberware => objCyberware.TotalCost);
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
                string strCostExpr = Cost;
                if (strCostExpr.StartsWith("FixedValues("))
                {
                    string[] strValues = strCostExpr.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCostExpr = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                StringBuilder objCost = new StringBuilder(strCostExpr.TrimStart('+'));
                objCost.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCostExpr, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objCost.CheapReplace(strCostExpr, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                objCost.CheapReplace(strCostExpr, "Vehicle Cost", () => Parent?.OwnCost.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                objCost.CheapReplace(strCostExpr, "Body", () => Parent?.Body > 0 ? Parent.Body.ToString() : "0.5");
                objCost.CheapReplace(strCostExpr, "Speed", () => Parent?.Speed.ToString() ?? "0");
                objCost.CheapReplace(strCostExpr, "Acceleration", () => Parent?.Accel.ToString() ?? "0");
                objCost.CheapReplace(strCostExpr, "Handling", () => Parent?.Handling.ToString() ?? "0");
                objCost.CheapReplace(strCostExpr, "Slots", () => WeaponMountParent?.CalculatedSlots.ToString() ?? "0");

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

        /// <summary>
        /// The number of Slots the Mod consumes.
        /// </summary>
        public int CalculatedSlots
        {
            get
            {
                string strSlotsExpression = Slots;
                if (strSlotsExpression.StartsWith("FixedValues("))
                {
                    string[] strValues = strSlotsExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strSlotsExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                StringBuilder objSlots = new StringBuilder(strSlotsExpression.TrimStart('+'));
                objSlots.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objSlots.CheapReplace(strSlotsExpression, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objSlots.CheapReplace(strSlotsExpression, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }
                objSlots.CheapReplace(strSlotsExpression, "Vehicle Cost", () => Parent?.OwnCost.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                objSlots.CheapReplace(strSlotsExpression, "Body", () => Parent?.Body > 0 ? Parent.Body.ToString() : "0.5");
                objSlots.CheapReplace(strSlotsExpression, "Speed", () => Parent?.Speed.ToString() ?? "0");
                objSlots.CheapReplace(strSlotsExpression, "Acceleration", () => Parent?.Accel.ToString() ?? "0");
                objSlots.CheapReplace(strSlotsExpression, "Handling", () => Parent?.Handling.ToString() ?? "0");

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objSlots.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToInt32(objProcess) : 0;
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
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpaceCharacter + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            if (Rating > 0)
                strReturn += strSpaceCharacter + '(' + LanguageManager.GetString("String_Rating", strLanguage) + strSpaceCharacter + Rating.ToString() + ')';
            return strReturn;
        }

        /// <summary>
        /// Vehicle arm/leg Strength.
        /// </summary>
        public int TotalStrength
        {
            get
            {
                string strName = Name.ToLower();
                if (!strName.Contains("arm") && !strName.Contains("leg"))
                    return 0;
                int intAttribute = 0;
                int bod = 1;
                if (Parent != null)
                {
                    bod = Parent.TotalBody * 2;
                    intAttribute = Math.Max(Parent.TotalBody, 0);
                }
                int intBonus = 0;

                foreach (Cyberware objChild in Cyberware)
                {
                    // If the limb has Customized Strength, this is its new base value.
                    if (objChild.Name == "Customized Strength")
                        intAttribute = objChild.Rating;
                    // If the limb has Enhanced Strength, this adds to the limb's value.
                    if (objChild.Name == "Enhanced Strength")
                        intBonus = objChild.Rating;
                }
                return Math.Min(intAttribute + intBonus, Math.Max(bod, 1));
            }
        }

        /// <summary>
        /// Vehicle arm/leg Agility.
        /// </summary>
        public int TotalAgility
        {
            get
            {
                string strName = Name.ToLower();
                if (!strName.Contains("arm") && !strName.Contains("leg"))
                    return 0;

                int intAttribute = 0;
                int pilot = 1;
                if (Parent != null)
                {
                    pilot = Parent.TotalBody * 2;
                    intAttribute = Math.Max(Parent.Pilot, 0);
                }
                int intBonus = 0;

                foreach (Cyberware objChild in Cyberware)
                {
                    // If the limb has Customized Agility, this is its new base value.
                    if (objChild.Name == "Customized Agility")
                        intAttribute = objChild.Rating;
                    // If the limb has Enhanced Agility, this adds to the limb's value.
                    if (objChild.Name == "Enhanced Agility")
                        intBonus = objChild.Rating;
                }

                return Math.Min(intAttribute + intBonus, Math.Max(pilot, 1));
            }
        }

        /// <summary>
        /// Whether or not the Mod is allowed to accept Cyberware Modular Plugins.
        /// </summary>
        public bool AllowModularPlugins
        {
            get
            {
                return Cyberware.Any(objChild => objChild.AllowedSubsystems.Contains("Modular Plug-In"));
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
                XmlDocument objDoc = XmlManager.Load("vehicles.xml", strLanguage);
                _objCachedMyXmlNode = (XmlManager.Load("vehicles.xml", strLanguage)
                                             .SelectSingleNode($"/chummer/mods/mod[id = \"{SourceIDString}\" or id = \"{SourceIDString}\"]") ??
                                       objDoc.SelectSingleNode($"/chummer/weaponmountmods/mod[id = \"{SourceIDString}\" or id = \"{SourceIDString}\"]")) ??
                                       objDoc.SelectSingleNode($"/chummer/mods/mod[name = \"{Name}\"]") ??
                                       objDoc.SelectSingleNode($"/chummer/weaponmountmods/mod[name = \"{Name}\"]");

                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public decimal DeleteVehicleMod()
        {
            decimal decReturn = 0;

            foreach (Weapon objLoopWeapon in Weapons)
            {
                decReturn += objLoopWeapon.DeleteWeapon();
            }
            foreach (Cyberware objLoopCyberware in Cyberware)
            {
                decReturn += objLoopCyberware.DeleteCyberware();
            }

            return decReturn;
        }

        #region UI Methods
        /// <summary>
        /// Add a piece of Armor to the Armor TreeView.
        /// </summary>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsVehicleMod, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsVehicleWeaponAccessory, ContextMenuStrip cmsVehicleWeaponAccessoryGear)
        {
            if (IncludedInVehicle && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = cmsVehicleMod,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // Cyberware.
            foreach (Cyberware objCyberware in Cyberware)
            {
                TreeNode objLoopNode = objCyberware.CreateTreeNode(cmsCyberware, cmsCyberwareGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }

            // VehicleWeapons.
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
                    return Color.SaddleBrown;
                }
                if (IncludedInVehicle)
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }

        public decimal StolenTotalCost
        {
            get
            {
                decimal d = 0;
                if (Stolen)
                    d += OwnCost;
                d += Weapons.AsParallel().Sum(objWeapon => objWeapon.StolenTotalCost);
                d += Cyberware.AsParallel().Sum(objCyberware => objCyberware.StolenTotalCost);
                return d;
            }
        }

        #endregion
        #endregion

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
