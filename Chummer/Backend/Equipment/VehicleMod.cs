using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
	/// <summary>
	/// Vehicle Modification.
	/// </summary>
	public class VehicleMod : INamedItemWithGuid
    {
		private Guid _guiID;
		private string _strName = string.Empty;
		private string _strCategory = string.Empty;
		private string _strLimit = string.Empty;
		private string _strSlots = "0";
		private int _intRating;
		private string _strMaxRating = "0";
		private string _strCost = string.Empty;
        private int _intMarkup;
		private string _strAvail = string.Empty;
		private XmlNode _nodBonus;
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
        public void Create(XmlNode objXmlMod, TreeNode objNode, int intRating, Vehicle objParent, int intMarkup = 0)
		{
		    if (objParent == null) throw new ArgumentNullException(nameof(objParent));
		    Parent = objParent;
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetStringFieldQuickly("limit", ref _strLimit);
			objXmlMod.TryGetStringFieldQuickly("slots", ref _strSlots);
		    _intRating = intRating;
            if (objXmlMod["downgrade"] != null)
		    {
		        _blnDowngrade = true;
		    }

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
				if (objXmlMod["cost"].InnerText.StartsWith("Variable"))
				{
					int intMin;
					var intMax = 0;
                    char[] chrParentheses = { '(', ')' };
					string strCost = objXmlMod["cost"].InnerText.Replace("Variable", string.Empty).Trim(chrParentheses);
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
						var frmPickNumber = new frmSelectNumber();
						if (intMax == 0)
							intMax = 1000000;
						frmPickNumber.Minimum = intMin;
						frmPickNumber.Maximum = intMax;
						frmPickNumber.Description = LanguageManager.Instance.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
						frmPickNumber.AllowCancel = false;
						frmPickNumber.ShowDialog();
						_strCost = frmPickNumber.SelectedValue.ToString();
					}
				}
				else
					_strCost = objXmlMod["cost"].InnerText;
			}
            _intMarkup = intMarkup;

            objXmlMod.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMod.TryGetStringFieldQuickly("page", ref _strPage);
		    _nodBonus = objXmlMod["bonus"];

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("vehicles.xml");
				XmlNode objModNode = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + _strName + "\"]");
				if (objModNode != null)
				{
                    objModNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objModNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
				}

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
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("maxrating", _strMaxRating);
			objWriter.WriteElementString("response", _intResponse.ToString());
			objWriter.WriteElementString("system", _intSystem.ToString());
			objWriter.WriteElementString("firewall", _intFirewall.ToString());
			objWriter.WriteElementString("signal", _intSignal.ToString());
			objWriter.WriteElementString("pilot", _intPilot.ToString());
            objWriter.WriteElementString("conditionmonitor", _intConditionMonitor.ToString());
            objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("markup", _intMarkup.ToString());
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString());
			objWriter.WriteElementString("installed", _blnInstalled.ToString());
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
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

        /// <summary>
        /// Load the VehicleMod from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="objVehicle">Vehicle that the mod is attached to.</param>
        /// <param name="blnCopy">Indicates whether a new item will be created as a copy of this one.</param>
        public void Load(XmlNode objNode, Vehicle objVehicle, bool blnCopy = false)
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
            objNode.TryGetInt32FieldQuickly("markup", ref _intMarkup);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludeInVehicle);
			objNode.TryGetBoolFieldQuickly("installed", ref _blnInstalled);
			objNode.TryGetStringFieldQuickly("subsystems", ref _strSubsystems);

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
			objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
			objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
			objNode.TryGetStringFieldQuickly("extra", ref _strExtra);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("vehicles.xml");
				XmlNode objModNode = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + _strName + "\"]");
				if (objModNode != null)
				{
                    objModNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objModNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
				}

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
		public int Markup
        {
            get
            {
                return _intMarkup;
            }
            set
            {
                _intMarkup = value;
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

				string strCalculated;

			    // Reordered to process fixed value strings
				if (_strAvail.StartsWith("FixedValues"))
				{
					string[] strValues = _strAvail.Replace("FixedValues", string.Empty).Trim("()".ToCharArray()).Split(',');
                    if (strValues.Length >= _intRating)
					    _strAvail = strValues[_intRating - 1];
				}

				if (_strAvail.Contains("Rating"))
				{
					// If the availability is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strAvail = string.Empty;
					string strAvailExpr = _strAvail;

					if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
					{
						strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
						// Remove the trailing character if it is "F" or "R".
						strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
					}
					XPathExpression xprAvail = nav.Compile(strAvailExpr.Replace("Rating", _intRating.ToString()));
					strCalculated = Convert.ToInt32(nav.Evaluate(xprAvail)) + strAvail;
				}
				else
				{
					// Just a straight cost, so return the value.
					if (_strAvail.Contains("F") || _strAvail.Contains("R"))
					{
						strCalculated = Convert.ToInt32(_strAvail.Substring(0, _strAvail.Length - 1)) + _strAvail.Substring(_strAvail.Length - 1, 1);
					}
					else
						strCalculated = Convert.ToInt32(_strAvail).ToString();
				}

				int intAvail;
				string strAvailText = string.Empty;
				if (strCalculated.Contains("F") || strCalculated.Contains("R"))
				{
					strAvailText = strCalculated.Substring(strCalculated.Length - 1);
					intAvail = Convert.ToInt32(strCalculated.Replace(strAvailText, string.Empty));
				}
				else
					intAvail = Convert.ToInt32(strCalculated);

				string strReturn = intAvail + strAvailText;

				// Translate the Avail string.
				strReturn = strReturn.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted"));
				strReturn = strReturn.Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

				return strReturn;
			}
		}

		/// <summary>
		/// Total cost of the VehicleMod.
		/// </summary>
		public int TotalCost
		{
			get
			{


			    return OwnCost + _lstVehicleWeapons.Sum(objWeapon => objWeapon.TotalCost) + _lstCyberware.Sum(objCyberware => objCyberware.TotalCost);
			}
		}

		/// <summary>
		/// The cost of just the Vehicle Mod itself.
		/// </summary>
		public int OwnCost
		{
			get
			{
			    // If the cost is determined by the Rating, evaluate the expression.
				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();

			    string strCostExpression = _strCost;
				if (_strCost.StartsWith("FixedValues"))
				{
					string[] strValues = _strCost.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					strCostExpression = (strValues[Convert.ToInt32(_intRating) - 1]);
				}
				string strCost = strCostExpression.Replace("Rating", _intRating.ToString());
				strCost = strCost.Replace("Vehicle Cost", Parent.OwnCost.ToString());
				// If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
			    strCost = strCost.Replace("Body", Parent.Body > 0 ? Parent.Body.ToString() : "0.5");
			    strCost = strCost.Replace("Speed", Parent.Speed.ToString());
				strCost = strCost.Replace("Acceleration", Parent.Accel.ToString());
				XPathExpression xprCost = nav.Compile(strCost);
				int intReturn = Convert.ToInt32(nav.Evaluate(xprCost)?.ToString());

				if (DiscountCost)
					intReturn = intReturn * 9 / 10;

                // Apply a markup if applicable.
                if (_intMarkup != 0)
                {
                    double dblCost = Convert.ToDouble(intReturn, GlobalOptions.InvariantCultureInfo);
                    dblCost *= 1 + (Convert.ToDouble(_intMarkup, GlobalOptions.InvariantCultureInfo) / 100.0);
                    intReturn = Convert.ToInt32(dblCost);
                }

                return intReturn;
			}
		}

		/// <summary>
		/// The number of Slots the Mod consumes.
		/// </summary>
		public int CalculatedSlots
		{
			get
			{
				if (_strSlots.StartsWith("FixedValues"))
				{
					string[] strValues = _strSlots.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					return Convert.ToInt32(strValues[Convert.ToInt32(_intRating) - 1]);
				}
				else
				{
					// If the slots is determined by the Rating, evaluate the expression.
				    XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					//return Convert.ToInt32(_strSlots.Replace("Rating", _intRating.ToString()));
					XPathExpression xprSlots = nav.Compile(_strSlots.Replace("Rating", _intRating.ToString()));
					int intReturn = Convert.ToInt32(nav.Evaluate(xprSlots)?.ToString());
					return intReturn;
				}
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
					strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
				if (_intRating > 0)
					strReturn += " (" + LanguageManager.Instance.GetString("String_Rating") + " " + _intRating + ")";
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
		#endregion
	}
}