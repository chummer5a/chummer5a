using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Vehicle Modification.
    /// </summary>
    public class VehicleMod : INamedItemWithGuidAndNode
    {
        private Guid _guiID;
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
        private bool _blnWirelessOn = true;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private bool _blnIncludeInVehicle;
        private bool _blnInstalled = true;
        private int _intResponse;
        private int _intSystem;
        private int _intFirewall;
        private int _intSignal;
        private int _intPilot;
        private int _intConditionMonitor;
        private readonly List<Weapon> _lstVehicleWeapons = new List<Weapon>();
        private string _strNotes = string.Empty;
        private string _strSubsystems = string.Empty;
        private readonly List<Cyberware> _lstCyberware = new List<Cyberware>();
        private string _strAltName = string.Empty;
        private string _strAltCategory = string.Empty;
        private string _strAltPage = string.Empty;
        private string _strExtra = string.Empty;
        private string _strWeaponMountCategories = string.Empty;
        private bool _blnDiscountCost;
        private bool _blnDowngrade;
        private string _strCapacity = string.Empty;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods
        public VehicleMod(Character objCharacter)
        {
            // Create the GUID for the new VehicleMod.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="objParent">Vehicle that the mod will be attached to.</param>
        /// <param name="intMarkup">Discount or markup that applies to the base cost of the mod.</param>
        public void Create(XmlNode objXmlMod, TreeNode objNode, int intRating, Vehicle objParent, decimal decMarkup = 0)
        {
            if (objParent == null) throw new ArgumentNullException(nameof(objParent));
            Parent = objParent;
            if (objXmlMod == null) Utils.BreakIfDebug();
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetStringFieldQuickly("limit", ref _strLimit);
            objXmlMod.TryGetStringFieldQuickly("slots", ref _strSlots);
            _intRating = intRating;
            if (objXmlMod["downgrade"] != null)
            {
                _blnDowngrade = true;
            }

            objXmlMod.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objXmlMod.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            objXmlMod.TryGetInt32FieldQuickly("response", ref _intResponse);
            objXmlMod.TryGetInt32FieldQuickly("system", ref _intSystem);
            objXmlMod.TryGetInt32FieldQuickly("firewall", ref _intFirewall);
            objXmlMod.TryGetInt32FieldQuickly("signal", ref _intSignal);
            objXmlMod.TryGetInt32FieldQuickly("pilot", ref _intPilot);
            objXmlMod.TryGetInt32FieldQuickly("conditionmonitor", ref _intConditionMonitor);
            objXmlMod.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
            // Add Subsytem information if applicable.
            if (objXmlMod.InnerXml.Contains("subsystems"))
            {
                string strSubsystem = string.Empty;
                XmlNodeList objXmlSubsystems = objXmlMod.SelectNodes("subsystems/subsystem");
                if (objXmlSubsystems != null)
                    strSubsystem = objXmlSubsystems.Cast<XmlNode>().Aggregate(strSubsystem, (current, objXmlSubsystem) => current + (objXmlSubsystem.InnerText + ","));
                _strSubsystems = strSubsystem;
            }
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);
            
            // Check for a Variable Cost.
            if (objXmlMod["cost"] != null)
            {
                _strCost = objXmlMod["cost"].InnerText;
                if (_strCost.StartsWith("Variable"))
                {
                    decimal decMin = 0;
                    decimal decMax = decimal.MaxValue;
                    string strCost = _strCost.TrimStart("Variable", true).Trim("()".ToCharArray());
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
            }
            _decMarkup = decMarkup;

            objXmlMod.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMod.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objXmlMod["bonus"];
            _nodWirelessBonus = objXmlMod["wirelessbonus"];
            _blnWirelessOn = _nodWirelessBonus != null;

            if (GlobalOptions.Language != "en-us")
            {
                
                XmlNode objModNode = MyXmlNode;
                if (objModNode != null)
                {
                    objModNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objModNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");
                objModNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                    _strAltCategory = objModNode?.Attributes?["translate"]?.InnerText;
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
            objWriter.WriteStartElement("mod");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("slots", _strSlots);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("rating", _intRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("response", _intResponse.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("system", _intSystem.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("firewall", _intFirewall.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("signal", _intSignal.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("pilot", _intPilot.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("conditionmonitor", _intConditionMonitor.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("markup", _decMarkup.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString());
            objWriter.WriteElementString("installed", _blnInstalled.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteElementString("subsystems", _strSubsystems);
            objWriter.WriteElementString("weaponmountcategories", _strWeaponMountCategories);
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
            objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the VehicleMod from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Indicates whether a new item will be created as a copy of this one.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
            }
            else
            {
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
            objNode.TryGetStringFieldQuickly("slots", ref _strSlots);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
            objNode.TryGetInt32FieldQuickly("response", ref _intResponse);
            objNode.TryGetInt32FieldQuickly("system", ref _intSystem);
            objNode.TryGetInt32FieldQuickly("firewall", ref _intFirewall);
            objNode.TryGetInt32FieldQuickly("signal", ref _intSignal);
            objNode.TryGetInt32FieldQuickly("pilot", ref _intPilot);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetInt32FieldQuickly("conditionmonitor", ref _intConditionMonitor);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetDecFieldQuickly("markup", ref _decMarkup);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludeInVehicle);
            objNode.TryGetBoolFieldQuickly("installed", ref _blnInstalled);
            objNode.TryGetStringFieldQuickly("subsystems", ref _strSubsystems);
            // Legacy Shims
            if (Name.StartsWith("Gecko Tips (Bod"))
            {
                Name = "Gecko Tips";
                XmlNode objNewNode = MyXmlNode;
                if (objNewNode != null)
                {
                    objNewNode.TryGetStringFieldQuickly("cost", ref _strCost);
                    objNewNode.TryGetStringFieldQuickly("slots", ref _strSlots);
                }
            }
            if (Name.StartsWith("Gliding System (Bod"))
            {
                Name = "Gliding System";
                XmlNode objNewNode = MyXmlNode;
                if (objNewNode != null)
                {
                    objNewNode.TryGetStringFieldQuickly("cost", ref _strCost);
                    objNewNode.TryGetStringFieldQuickly("slots", ref _strSlots);
                    objNewNode.TryGetStringFieldQuickly("avail", ref _strAvail);
                }
            }

            if (objNode.InnerXml.Contains("<weapons>"))
            {
                XmlNodeList xmlNodeList = objNode.SelectNodes("weapons/weapon");
                if (xmlNodeList != null)
                    foreach (XmlNode nodChild in xmlNodeList)
                    {
                        var objWeapon = new Weapon(_objCharacter);
                        objWeapon.Load(nodChild, blnCopy);
                        _lstVehicleWeapons.Add(objWeapon);
                    }
            }
            if (objNode.InnerXml.Contains("<cyberwares>"))
            {
                XmlNodeList xmlNodeList = objNode.SelectNodes("cyberwares/cyberware");
                if (xmlNodeList != null)
                    foreach (XmlNode nodChild in xmlNodeList)
                    {
                        Cyberware objCyberware = new Cyberware(_objCharacter);
                        objCyberware.Load(nodChild, blnCopy);
                        _lstCyberware.Add(objCyberware);
                    }
            }

            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = _nodWirelessBonus != null;
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);

            if (GlobalOptions.Language != "en-us")
            {
                XmlNode objModNode = MyXmlNode;
                if (objModNode != null)
                {
                    objModNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objModNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");
                objModNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                    _strAltCategory = objModNode?.Attributes?["translate"]?.InnerText;
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("mod");
            objWriter.WriteElementString("name", DisplayNameShort);
            objWriter.WriteElementString("category", DisplayCategory);
            objWriter.WriteElementString("limit", _strLimit);
            objWriter.WriteElementString("slots", _strSlots);
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteElementString("avail", TotalAvail);
            objWriter.WriteElementString("cost", TotalCost.ToString());
            objWriter.WriteElementString("owncost", OwnCost.ToString());
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString());
            objWriter.WriteStartElement("weapons");
            foreach (Weapon objWeapon in _lstVehicleWeapons)
                objWeapon.Print(objWriter);
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("cyberwares");
            foreach (Cyberware objCyberware in _lstCyberware)
                objCyberware.Print(objWriter);
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapons.
        /// </summary>
        public List<Weapon> Weapons
        {
            get
            {
                return _lstVehicleWeapons;
            }
        }

        public List<Cyberware> Cyberware
        {
            get
            {
                return _lstCyberware;
            }
        }

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
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
        /// Limits the Weapon Selection form to specified categories.
        /// </summary>
        public string WeaponMountCategories
        {
            set
            {
                _strWeaponMountCategories = value;
            }
            get
            {
                return _strWeaponMountCategories; 
            }
        }

        /// <summary>
        /// Which Vehicle types the Mod is limited to.
        /// </summary>
        public string Limit
        {
            get
            {
                return _strLimit;
            }
            set
            {
                _strLimit = value;
            }
        }

        /// <summary>
        /// Number of Slots the Mod uses.
        /// </summary>
        public string Slots
        {
            get
            {
                return _strSlots;
            }
            set
            {
                _strSlots = value;
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
                if (_intRating == -1)
                {
                    _intRating = 0;
                }
                _intRating = value;
            }
        }

        /// <summary>
        /// Maximum Rating.
        /// </summary>
        public string MaxRating
        {
            get
            {
                return _strMaxRating;
            }
            set
            {
                _strMaxRating = value;
            }
        }

        /// <summary>
        /// Response.
        /// </summary>
        public int Response
        {
            get
            {
                return _intResponse;
            }
            set
            {
                _intResponse = value;
            }
        }

        /// <summary>
        /// System.
        /// </summary>
        public int System
        {
            get
            {
                return _intSystem;
            }
            set
            {
                _intSystem = value;
            }
        }

        /// <summary>
        /// Firewall.
        /// </summary>
        public int Firewall
        {
            get
            {
                return _intFirewall;
            }
            set
            {
                _intFirewall = value;
            }
        }

        /// <summary>
        /// Signal.
        /// </summary>
        public int Signal
        {
            get
            {
                return _intSignal;
            }
            set
            {
                _intSignal = value;
            }
        }

        /// <summary>
        /// Pilot.
        /// </summary>
        public int Pilot
        {
            get
            {
                return _intPilot;
            }
            set
            {
                _intPilot = value;
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

        /// <summary>
        /// Markup.
        /// </summary>
        public decimal Markup
        {
            get
            {
                return _decMarkup;
            }
            set
            {
                _decMarkup = value;
            }
        }

        /// <summary>
        /// Availability.
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
        /// Bonus node.
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
        /// Wireless Bonus node.
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
        /// Whether the vehicle mod's wireless is enabled
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
        /// Whether or not the Mod included with the Vehicle by default.
        /// </summary>
        public bool IncludedInVehicle
        {
            get
            {
                return _blnIncludeInVehicle;
            }
            set
            {
                _blnIncludeInVehicle = value;
            }
        }

        /// <summary>
        /// Whether or not this Mod is installed and contributing towards the Vehicle's stats.
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
        /// Whether or not the Vehicle Mod allows Cyberware Plugins.
        /// </summary>
        public bool AllowCyberware
        {
            get
            {
                return !string.IsNullOrEmpty(_strSubsystems);
            }
        }

        /// <summary>
        /// Allowed Cyberwarre Subsystems.
        /// </summary>
        public string Subsystems
        {
            get
            {
                return _strSubsystems;
            }
            set
            {
                _strSubsystems = value;
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
        /// Whether or not the Vehicle Mod's cost should be discounted by 10% through the Black Market Pipeline Quality.
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
        /// Whether or not the Vehicle Mod is a downgrade for drone attributes
        /// </summary>
        public bool Downgrade
        {
            get
            {
                return _blnDowngrade;
            }

        }

        /// <summary>
        /// Bonus/Penalty to the parent vehicle that this mod provides.
        /// </summary>
        public int ConditionMonitor
        {
            get { return _intConditionMonitor; }
        }

        /// <summary>
        /// Vehicle that the Mod is attached to. 
        /// </summary>
        public Vehicle Parent { internal get; set; }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Total Availablility of the VehicleMod.
        /// </summary>
        public string TotalAvail
        {
            get
            {
                // If the Avail contains "+", return the base string and don't try to calculate anything since we're looking at a child component.
                if (_strAvail.StartsWith("+"))
                    return _strAvail;

                string strCalculated = _strAvail;

                // Reordered to process fixed value strings
                if (strCalculated.StartsWith("FixedValues"))
                {
                    string[] strValues = strCalculated.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (strValues.Length >= _intRating)
                        strCalculated = strValues[_intRating - 1];
                }
                else if (strCalculated.StartsWith("Range"))
                {
                    // If the Availability code is based on the current Rating of the item, separate the Availability string into an array and find the first bracket that the Rating is lower than or equal to.
                    string[] strValues = strCalculated.Replace("MaxRating", MaxRating).Replace("Range", string.Empty).Trim("()".ToCharArray()).Split(',');
                    foreach (string strValue in strValues)
                    {
                        string strAvailCode = strValue.Split('[')[1].Trim("[]".ToCharArray());
                        int intMax = Convert.ToInt32(strValue.Split('[')[0]);
                        if (Rating > intMax) continue;
                        strCalculated = $"{Rating}{strAvailCode}";
                        break;
                    }
                }
                string strAvailText = string.Empty;
                if (strCalculated.EndsWith("F") || strCalculated.EndsWith("R"))
                {
                    strAvailText = strCalculated.Substring(strCalculated.Length - 1, 1);
                    // Translate the Avail string.
                    if (strAvailText == "F")
                        strAvailText = LanguageManager.GetString("String_AvailForbidden");
                    else if (strAvailText == "R")
                        strAvailText = LanguageManager.GetString("String_AvailRestricted");
                    strCalculated = strCalculated.Substring(0, strCalculated.Length - 1);
                }

                // If the availability is determined by the Rating, evaluate the expression.
                XmlDocument objXmlDocument = new XmlDocument();
                XPathNavigator nav = objXmlDocument.CreateNavigator();

                string strAvailExpr = strCalculated.Replace("Rating", _intRating.ToString());
                strAvailExpr = strAvailExpr.Replace("Vehicle Cost", Parent.OwnCost.ToString(CultureInfo.InvariantCulture));
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                strAvailExpr = strAvailExpr.Replace("Body", Parent.Body > 0 ? Parent.Body.ToString() : "0.5");
                strAvailExpr = strAvailExpr.Replace("Speed", Parent.Speed.ToString());
                strAvailExpr = strAvailExpr.Replace("Acceleration", Parent.Accel.ToString());
                strAvailExpr = strAvailExpr.Replace("Handling", Parent.Handling.ToString());
                string strReturn = string.Empty;
                try
                {
                    XPathExpression xprAvail = nav.Compile(strAvailExpr);
                    strReturn = Convert.ToInt32(nav.Evaluate(xprAvail)).ToString();
                }
                catch (XPathException)
                {
                    strReturn = strCalculated;
                }

                return strReturn + strAvailText;
            }
        }

        /// <summary>
        /// Caculated Capacity of the Vehicle Mod.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strReturn = "0";
                if (!string.IsNullOrEmpty(_strCapacity) && _strCapacity.Contains("/["))
                {
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    int intPos = _strCapacity.IndexOf("/[");
                    string strFirstHalf = _strCapacity.Substring(0, intPos);
                    string strSecondHalf = _strCapacity.Substring(intPos + 1, _strCapacity.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.Contains('['); ;
                    string strCapacity = strFirstHalf;

                    if (blnSquareBrackets && strCapacity.Length > 2)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

                    if (_strCapacity == "[*]")
                        strReturn = "*";
                    else
                    {
                        if (_strCapacity.StartsWith("FixedValues"))
                        {
                            string[] strValues = _strCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                            if (_intRating <= strValues.Length)
                                strReturn = strValues[_intRating - 1];
                            else
                                strReturn = "0";
                        }
                        else
                        {
                            try
                            {
                                strReturn = Convert.ToDecimal(nav.Evaluate(xprCapacity)).ToString("N2", GlobalOptions.CultureInfo);
                            }
                            catch (XPathException)
                            {
                                strReturn = "0";
                            }
                        }
                    }
                    if (blnSquareBrackets)
                        strReturn = "[" + strCapacity + "]";

                    if (strSecondHalf.Contains("Rating"))
                    {
                        strSecondHalf = strSecondHalf.Trim("[]".ToCharArray());
                        xprCapacity = nav.Compile(strSecondHalf.Replace("Rating", _intRating.ToString()));
                        strSecondHalf = "[" + Convert.ToDecimal(nav.Evaluate(xprCapacity)).ToString("N2", GlobalOptions.CultureInfo) + "]";
                    }

                    strReturn += "/" + strSecondHalf;
                }
                else if (_strCapacity.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = _strCapacity.Contains('[');
                    string strCapacity = _strCapacity;
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

                    strReturn = Convert.ToDecimal(nav.Evaluate(xprCapacity)).ToString("N2", GlobalOptions.CultureInfo);
                    if (blnSquareBrackets)
                        strReturn = "[" + strReturn + "]";
                }
                else
                {
                    if (_strCapacity.StartsWith("FixedValues"))
                    {
                        string[] strValues = _strCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                        if (strValues.Length >= _intRating)
                            strReturn = strValues[_intRating - 1];
                    }
                    else
                    {
                        // Just a straight Capacity, so return the value.
                        strReturn = _strCapacity;
                    }
                }
                if (string.IsNullOrEmpty(strReturn))
                    strReturn = "0";
                decimal decReturn;
                if (decimal.TryParse(strReturn, out decReturn))
                    return decReturn.ToString("N2", GlobalOptions.CultureInfo);
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
                    foreach (Cyberware objChildCyberware in _lstCyberware)
                    {
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains('['))
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
                    foreach (Cyberware objChildCyberware in _lstCyberware)
                    {
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains('['))
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
        public decimal TotalCost
        {
            get
            {
                return OwnCost + _lstVehicleWeapons.Sum(objWeapon => objWeapon.TotalCost) + _lstCyberware.Sum(objCyberware => objCyberware.TotalCost);
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
                XmlDocument objXmlDocument = new XmlDocument();
                XPathNavigator nav = objXmlDocument.CreateNavigator();

                string strCostExpression = _strCost;
                if (_strCost.StartsWith("FixedValues"))
                {
                    string[] strValues = _strCost.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (_intRating > 0)
                        strCostExpression = (strValues[Math.Min(_intRating, strValues.Length) - 1]);
                }
                string strCost = strCostExpression.Replace("Rating", _intRating.ToString());
                strCost = strCost.Replace("Vehicle Cost", Parent.OwnCost.ToString(CultureInfo.InvariantCulture));
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                strCost = strCost.Replace("Body", Parent.Body > 0 ? Parent.Body.ToString() : "0.5");
                strCost = strCost.Replace("Speed", Parent.Speed.ToString());
                strCost = strCost.Replace("Acceleration", Parent.Accel.ToString());
                strCost = strCost.Replace("Handling", Parent.Handling.ToString());
                XPathExpression xprCost = nav.Compile(strCost);
                decimal decReturn = Convert.ToDecimal(nav.Evaluate(xprCost)?.ToString(), GlobalOptions.InvariantCultureInfo);

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
                // If the slots is determined by the Rating, evaluate the expression.
                XmlDocument objXmlDocument = new XmlDocument();
                XPathNavigator nav = objXmlDocument.CreateNavigator();

                string strSlotsExpression = _strSlots;
                if (_strCost.StartsWith("FixedValues"))
                {
                    string[] strValues = _strCost.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (_intRating > 0)
                        strSlotsExpression = (strValues[Math.Min(_intRating, strValues.Length) - 1]);
                }
                string strSlots = strSlotsExpression.Replace("Rating", _intRating.ToString());
                strSlots = strSlots.Replace("Vehicle Cost", Parent.OwnCost.ToString(CultureInfo.InvariantCulture));
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                strSlots = strSlots.Replace("Body", Parent.Body > 0 ? Parent.Body.ToString() : "0.5");
                strSlots = strSlots.Replace("Speed", Parent.Speed.ToString());
                strSlots = strSlots.Replace("Acceleration", Parent.Accel.ToString());
                strSlots = strSlots.Replace("Handling", Parent.Handling.ToString());
                XPathExpression xprSlots = nav.Compile(strSlots);
                return Convert.ToInt32(nav.Evaluate(xprSlots)?.ToString());
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

                if (!string.IsNullOrEmpty(_strExtra))
                    strReturn += " (" + LanguageManager.TranslateExtra(_strExtra) + ")";
                if (_intRating > 0)
                    strReturn += " (" + LanguageManager.GetString("String_Rating") + " " + _intRating.ToString() + ")";
                return strReturn;
            }
        }

        /// <summary>
        /// Whether or not the Mod is allowed to accept Cyberware Modular Plugins.
        /// </summary>
        public bool AllowModularPlugins
        {
            get
            {
                return _lstCyberware.Any(objChild => objChild.AllowedSubsystems.Contains("Modular Plug-In"));
            }
        }

        public XmlNode MyXmlNode
        {
            get
            {
                return XmlManager.Load("vehicles.xml")?.SelectSingleNode("/chummer/mods/mod[name = \"" + Name + "\"]");
            }
        }
        #endregion
    }
}
