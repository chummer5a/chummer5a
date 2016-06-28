using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
	/// <summary>
	/// Vehicle Modification.
	/// </summary>
	public class VehicleMod
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strCategory = "";
		private string _strLimit = "";
		private string _strSlots = "0";
		private int _intRating = 0;
		private string _strMaxRating = "0";
		private string _strCost = "";
		private string _strAvail = "";
		private XmlNode _nodBonus;
		private string _strSource = "";
		private string _strPage = "";
		private bool _blnIncludeInVehicle = false;
		private bool _blnInstalled = true;
		private int _intResponse = 0;
		private int _intSystem = 0;
		private int _intFirewall = 0;
		private int _intSignal = 0;
		private int _intPilot = 0;
		private List<Weapon> _lstVehicleWeapons = new List<Weapon>();
		private string _strNotes = "";
		private string _strSubsystems = "";
		private List<Cyberware> _lstCyberware = new List<Cyberware>();
		private string _strAltName = "";
		private string _strAltCategory = "";
		private string _strAltPage = "";
		private string _strExtra = "";
		private string _strWeaponMountCategories = "";
		private bool _blnDiscountCost = false;

		// Variables used to calculate the Mod's cost from the Vehicle.
		private int _intVehicleCost = 0;
		private int _intBody = 0;
		private int _intSpeed = 0;
		private int _intAccel = 0;

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
		public void Create(XmlNode objXmlMod, TreeNode objNode, int intRating)
		{
			_strName = objXmlMod["name"].InnerText;
			_strCategory = objXmlMod["category"].InnerText;
			objXmlMod.TryGetField("limit", out _strLimit);
			objXmlMod.TryGetField("slots", out _strSlots);
			if (intRating != 0)
			{
				_intRating = Convert.ToInt32(intRating);
			}
			if (objXmlMod["rating"] != null)
				_strMaxRating = objXmlMod["rating"].InnerText;
			else
				_strMaxRating = "0";
			objXmlMod.TryGetField("response", out _intResponse);
			objXmlMod.TryGetField("system", out _intSystem);
			objXmlMod.TryGetField("firewall", out _intFirewall);
			objXmlMod.TryGetField("signal", out _intSignal);
			objXmlMod.TryGetField("pilot", out _intPilot);
			objXmlMod.TryGetField("weaponmountcategories", out _strWeaponMountCategories);
			// Add Subsytem information if applicable.
			if (objXmlMod.InnerXml.Contains("subsystems"))
			{
				string strSubsystem = "";
				foreach (XmlNode objXmlSubsystem in objXmlMod.SelectNodes("subsystems/subsystem"))
				{
					strSubsystem += objXmlSubsystem.InnerText + ",";
				}
				_strSubsystems = strSubsystem;
			}
			_strAvail = objXmlMod["avail"].InnerText;
			
			// Check for a Variable Cost.
			if (objXmlMod["cost"] != null)
			{
				if (objXmlMod["cost"].InnerText.StartsWith("Variable"))
				{
					int intMin = 0;
					int intMax = 0;
					string strCost = objXmlMod["cost"].InnerText.Replace("Variable(", string.Empty).Replace(")", string.Empty);
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
						_strCost = frmPickNumber.SelectedValue.ToString();
					}
				}
				else
					_strCost = objXmlMod["cost"].InnerText;
			}

			_strSource = objXmlMod["source"].InnerText;
			_strPage = objXmlMod["page"].InnerText;
			if (objXmlMod["bonus"] != null)
				_nodBonus = objXmlMod["bonus"];

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("vehicles.xml");
				XmlNode objModNode = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + _strName + "\"]");
				if (objModNode != null)
				{
					if (objModNode["translate"] != null)
						_strAltName = objModNode["translate"].InnerText;
					if (objModNode["altpage"] != null)
						_strAltPage = objModNode["altpage"].InnerText;
				}

				objModNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objModNode != null)
				{
					if (objModNode.Attributes["translate"] != null)
						_strAltCategory = objModNode.Attributes["translate"].InnerText;
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
			objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("cost", _strCost);
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
		public void Load(XmlNode objNode, bool blnCopy = false)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strCategory = objNode["category"].InnerText;
			_strLimit = objNode["limit"].InnerText;
			_strSlots = objNode["slots"].InnerText;
			_intRating = Convert.ToInt32(objNode["rating"].InnerText);
			_strMaxRating = objNode["maxrating"].InnerText;
			objNode.TryGetField("weaponmountcategories", out _strWeaponMountCategories);
			objNode.TryGetField("response", out _intResponse);
			objNode.TryGetField("system", out _intSystem);
			objNode.TryGetField("firewall", out _intFirewall);
			objNode.TryGetField("signal", out _intSignal);
			objNode.TryGetField("pilot", out _intPilot);
			objNode.TryGetField("page", out _strPage);
			_strAvail = objNode["avail"].InnerText;
			_strCost = objNode["cost"].InnerText;
			_strSource = objNode["source"].InnerText;
			_blnIncludeInVehicle = Convert.ToBoolean(objNode["included"].InnerText);
			objNode.TryGetField("installed", out _blnInstalled);
			objNode.TryGetField("subsystems", out _strSubsystems);

			if (objNode.InnerXml.Contains("<weapons>"))
			{
				XmlNodeList nodChildren = objNode.SelectNodes("weapons/weapon");
				foreach (XmlNode nodChild in nodChildren)
				{
					Weapon objWeapon = new Weapon(_objCharacter);
					objWeapon.Load(nodChild, blnCopy);
					_lstVehicleWeapons.Add(objWeapon);
				}
			}
			if (objNode.InnerXml.Contains("<cyberwares>"))
			{
				XmlNodeList nodChildren = objNode.SelectNodes("cyberwares/cyberware");
				foreach (XmlNode nodChild in nodChildren)
				{
					Cyberware objCyberware = new Cyberware(_objCharacter);
					objCyberware.Load(nodChild, blnCopy);
					_lstCyberware.Add(objCyberware);
				}
			}

			try
			{
				_nodBonus = objNode["bonus"];
			}
			catch
			{
			}
			objNode.TryGetField("notes", out _strNotes);
			objNode.TryGetField("discountedcost", out _blnDiscountCost);
			objNode.TryGetField("extra", out _strExtra);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("vehicles.xml");
				XmlNode objModNode = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + _strName + "\"]");
				if (objModNode != null)
				{
					if (objModNode["translate"] != null)
						_strAltName = objModNode["translate"].InnerText;
					if (objModNode["altpage"] != null)
						_strAltPage = objModNode["altpage"].InnerText;
				}

				objModNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objModNode != null)
				{
					if (objModNode.Attributes["translate"] != null)
						_strAltCategory = objModNode.Attributes["translate"].InnerText;
				}
			}

			if (blnCopy)
			{
				_guiID = Guid.NewGuid();
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
				string strReturn = _strCategory;
				if (_strAltCategory != string.Empty)
					strReturn = _strAltCategory;

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
				string strReturn = _strPage;
				if (_strAltPage != string.Empty)
					strReturn = _strAltPage;

				return strReturn;
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

		// Properties used to calculate the Mod's cost from the Vehicle.
		public int VehicleCost
		{
			get
			{
				return _intVehicleCost;
			}
			set
			{
				_intVehicleCost = value;
			}
		}

		public int Body
		{
			get
			{
				return _intBody;
			}
			set
			{
				_intBody = value;
			}
		}

		public int Speed
		{
			get
			{
				return _intSpeed;
			}
			set
			{
				_intSpeed = value;
			}
		}

		public int Accel
		{
			get
			{
				return _intAccel;
			}
			set
			{
				_intAccel = value;
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
				if (_strSubsystems == "")
					return false;
				else
					return true;
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

				string strCalculated = "";
				string strReturn = "";

				// Reordered to process fixed value strings
				if (_strAvail.StartsWith("FixedValues"))
				{
					string[] strValues = _strAvail.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					_strAvail = strValues[Convert.ToInt32(_intRating) - 1];
				}

				if (_strAvail.Contains("Rating"))
				{
					// If the availability is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strAvail = "";
					string strAvailExpr = _strAvail;

					if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
					{
						strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
						// Remove the trailing character if it is "F" or "R".
						strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
					}
					XPathExpression xprAvail = nav.Compile(strAvailExpr.Replace("Rating", _intRating.ToString()));
					strCalculated = Convert.ToInt32(nav.Evaluate(xprAvail)).ToString() + strAvail;
				}
				else
				{
					// Just a straight cost, so return the value.
					string strAvail = "";
					if (_strAvail.Contains("F") || _strAvail.Contains("R"))
					{
						strAvail = _strAvail.Substring(_strAvail.Length - 1, 1);
						strCalculated = Convert.ToInt32(_strAvail.Substring(0, _strAvail.Length - 1)) + strAvail;
					}
					else
						strCalculated = Convert.ToInt32(_strAvail).ToString();
				}

				int intAvail = 0;
				string strAvailText = "";
				if (strCalculated.Contains("F") || strCalculated.Contains("R"))
				{
					strAvailText = strCalculated.Substring(strCalculated.Length - 1);
					intAvail = Convert.ToInt32(strCalculated.Replace(strAvailText, string.Empty));
				}
				else
					intAvail = Convert.ToInt32(strCalculated);

				strReturn = intAvail.ToString() + strAvailText;

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
				int intReturn = 0;

				// If the cost is determined by the Rating, evaluate the expression.
				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();

				string strCost = "";
				strCost = _strCost;
				if (_strCost.StartsWith("FixedValues"))
				{
					string[] strValues = strCost.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					strCost = strValues[_intRating - 1];
				}
				strCost = strCost.Replace("Rating", _intRating.ToString());
				strCost = strCost.Replace("Vehicle Cost", _intVehicleCost.ToString());
				// If the Body is 0 (Microdrone), treat it as 2 for the purposes of determine Modification cost.
				if (_intBody > 0)
					strCost = strCost.Replace("Body", _intBody.ToString());
				else
					strCost = strCost.Replace("Body", "2");
				
				strCost = strCost.Replace("Speed", _intSpeed.ToString());
				strCost = strCost.Replace("Acceleration", _intAccel.ToString());
				XPathExpression xprCost = nav.Compile(strCost);
				intReturn = Convert.ToInt32(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo);

				if (DiscountCost)
					intReturn = Convert.ToInt32(Convert.ToDouble(intReturn, GlobalOptions.Instance.CultureInfo) * 0.9);

				// Retrieve the price of VehicleWeapons.
				foreach (Weapon objWeapon in _lstVehicleWeapons)
				{
					intReturn += objWeapon.TotalCost;
				}

				// Retrieve the price of Cyberware.
				foreach (Cyberware objCyberware in _lstCyberware)
					intReturn += objCyberware.TotalCost;

				return intReturn;
			}
		}

		/// <summary>
		/// The cost of just the Vehicle Mod itself.
		/// </summary>
		public int OwnCost
		{
			get
			{
				int intReturn = 0;

				// If the cost is determined by the Rating, evaluate the expression.
				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();

				string strCost = "";
				string strCostExpression = "";
				strCostExpression = _strCost;
				if (_strCost.StartsWith("FixedValues"))
				{
					string[] strValues = _strCost.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					strCostExpression = (strValues[Convert.ToInt32(_intRating) - 1]);
				}
				strCost = strCostExpression.Replace("Rating", _intRating.ToString());
				strCost = strCost.Replace("Vehicle Cost", _intVehicleCost.ToString());
				// If the Body is 0 (Microdrone), treat it as 2 for the purposes of determine Modification cost.
				if (_intBody > 0)
					strCost = strCost.Replace("Body", _intBody.ToString());
				else
					strCost = strCost.Replace("Body", "2");
				strCost = strCost.Replace("Speed", _intSpeed.ToString());
				strCost = strCost.Replace("Acceleration", _intAccel.ToString());
				XPathExpression xprCost = nav.Compile(strCost);
				intReturn = Convert.ToInt32(nav.Evaluate(xprCost).ToString());

				if (DiscountCost)
					intReturn = Convert.ToInt32(Convert.ToDouble(intReturn, GlobalOptions.Instance.CultureInfo) * 0.9);

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
					int intReturn = 0;
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					//return Convert.ToInt32(_strSlots.Replace("Rating", _intRating.ToString()));
					XPathExpression xprSlots = nav.Compile(_strSlots.Replace("Rating", _intRating.ToString()));
					intReturn = Convert.ToInt32(nav.Evaluate(xprSlots).ToString());
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
				string strReturn = _strName;
				if (_strAltName != string.Empty)
					strReturn = _strAltName;

				return strReturn;
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

				if (_strExtra != "")
					strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
				if (_intRating > 0)
					strReturn += " (" + LanguageManager.Instance.GetString("String_Rating") + " " + _intRating.ToString() + ")";
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
				bool blnReturn = false;
				foreach (Cyberware objChild in _lstCyberware)
				{
					if (objChild.Subsytems.Contains("Modular Plug-In"))
					{
						blnReturn = true;
						break;
					}
				}

				return blnReturn;
			}
		}
		#endregion
	}
}