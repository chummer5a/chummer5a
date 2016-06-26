using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
	/// <summary>
	/// Weapon Accessory.
	/// </summary>
	public class WeaponAccessory
	{
		private Guid _guiID = new Guid();
		private readonly Character _objCharacter;
		private XmlNode _nodAllowGear;
		private List<Gear> _lstGear = new List<Gear>();
		private Weapon _objParent;
		private string _strName = "";
		private string _strMount = "";
		private string _strRC = "";
		private string _strDamage = "";
		private string _strDamageType = "";
		private string _strDamageReplace = "";
		private string _strFireMode = "";
		private string _strFireModeReplace = "";
		private string _strAPReplace = "";
		private string _strAP = "";
		private string _strConceal = "";
		private string _strAvail = "";
		private string _strCost = "";
		private string _strSource = "";
		private string _strPage = "";
		private string _strNotes = "";
		private string _strAltName = "";
		private string _strAltPage = "";
		private string _strDicePool = "";
		private int _intAccuracy = 0;
		private int _intRating = 0;
		private int _intRCGroup = 0;
		private int _intAmmoSlots = 0;
		private bool _blnDeployable = false;
		private bool _blnDiscountCost = false;
		private bool _blnBlackMarketDiscount = false;
		private bool _blnIncludedInWeapon = false;
		private bool _blnInstalled = true;
		private int _intAccessoryCostMultiplier = 1;
		private string _strExtra = "";
		private int _intRangeBonus = 0;
		private int _intSuppressive = 0;
		private int _intFullBurst = 0;
		private string _strAddMode = "";
		private string _strAmmoReplace = "";
		private int _intAmmoBonus = 0;

		#region Constructor, Create, Save, Load, and Print Methods
		public WeaponAccessory(Character objCharacter)
		{
			// Create the GUID for the new Weapon.
			_guiID = Guid.NewGuid();
			_objCharacter = objCharacter;
		}

		/// Create a Weapon Accessory from an XmlNode and return the TreeNodes for it.
		/// <param name="objXmlAccessory">XmlNode to create the object from.</param>
		/// <param name="objNode">TreeNode to populate a TreeView.</param>
		/// <param name="strMount">Mount slot that the Weapon Accessory will consume.</param>
		/// <param name="intRating">Rating of the Weapon Accessory.</param>
		public void Create(XmlNode objXmlAccessory, TreeNode objNode, string strMount, int intRating)
		{
			_strName = objXmlAccessory["name"].InnerText;
			_strMount = strMount;
			_intRating = intRating;
			_strAvail = objXmlAccessory["avail"].InnerText;
			_strCost = objXmlAccessory["cost"].InnerText;
			_strSource = objXmlAccessory["source"].InnerText;
			_strPage = objXmlAccessory["page"].InnerText;
			_nodAllowGear = objXmlAccessory["allowgear"];
			objXmlAccessory.TryGetField("rc", out _strRC, "");
			objXmlAccessory.TryGetField("rcdeployable", out _blnDeployable);
			objXmlAccessory.TryGetField("rcgroup", out _intRCGroup);
			objXmlAccessory.TryGetField("conceal", out _strConceal, "");
			objXmlAccessory.TryGetField("ammoslots", out _intAmmoSlots);
			objXmlAccessory.TryGetField("ammoreplace", out _strAmmoReplace, "");
			objXmlAccessory.TryGetField("accuracy", out _intAccuracy);
			objXmlAccessory.TryGetField("dicepool", out _strDicePool,"");
			objXmlAccessory.TryGetField("damagetype", out _strDamageType,"");
			objXmlAccessory.TryGetField("damage", out _strDamage, "");
			objXmlAccessory.TryGetField("damagereplace", out _strDamageReplace, "");
			objXmlAccessory.TryGetField("firemode", out _strFireMode, "");
			objXmlAccessory.TryGetField("firemodereplace", out _strFireModeReplace, "");
			objXmlAccessory.TryGetField("ap", out _strAP,"");
			objXmlAccessory.TryGetField("apreplace", out _strAPReplace,"");
			objXmlAccessory.TryGetField("addmode", out _strAddMode,"");
			objXmlAccessory.TryGetField("fullburst", out _intFullBurst);
			objXmlAccessory.TryGetField("suppressive", out _intSuppressive);
			objXmlAccessory.TryGetField("rangebonus", out _intRangeBonus);
			objXmlAccessory.TryGetField("extra", out _strExtra, "");
			objXmlAccessory.TryGetField("ammobonus", out _intAmmoBonus);
			objXmlAccessory.TryGetField("accessorycostmultiplier", out _intAccessoryCostMultiplier);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
				XmlNode objAccessoryNode = objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + _strName + "\"]");
				if (objAccessoryNode != null)
				{
					if (objAccessoryNode["translate"] != null)
						_strAltName = objAccessoryNode["translate"].InnerText;
					if (objAccessoryNode["altpage"] != null)
						_strAltPage = objAccessoryNode["altpage"].InnerText;
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
			objWriter.WriteStartElement("accessory");
			objWriter.WriteElementString("guid", _guiID.ToString());
			objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("mount", _strMount);
			objWriter.WriteElementString("rc", _strRC);
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("rcgroup", _intRCGroup.ToString());
			objWriter.WriteElementString("rcdeployable", _blnDeployable.ToString());
			objWriter.WriteElementString("conceal", _strConceal);
			if (_strDicePool != "")
				objWriter.WriteElementString("dicepool", _strDicePool);
			objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("cost", _strCost);
			objWriter.WriteElementString("included", _blnIncludedInWeapon.ToString());
			objWriter.WriteElementString("installed", _blnInstalled.ToString());
			if (_nodAllowGear != null)
				objWriter.WriteRaw(_nodAllowGear.OuterXml);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("accuracy", _intAccuracy.ToString());
			if (_lstGear.Count > 0)
			{
				objWriter.WriteStartElement("gears");
				foreach (Gear objGear in _lstGear)
				{
					// Use the Gear's SubClass if applicable.
					if (objGear.GetType() == typeof(Commlink))
					{
						Commlink objCommlink = new Commlink(_objCharacter);
						objCommlink = (Commlink)objGear;
						objCommlink.Save(objWriter);
					}
					else
					{
						objGear.Save(objWriter);
					}
				}
				objWriter.WriteEndElement();
			}
			objWriter.WriteElementString("ammoslots", _intAmmoSlots.ToString());
			objWriter.WriteElementString("damagetype", _strDamageType);
			objWriter.WriteElementString("damage", _strDamage);
			objWriter.WriteElementString("damagereplace", _strDamageReplace);
			objWriter.WriteElementString("firemode", _strFireMode);
			objWriter.WriteElementString("firemodereplace", _strFireModeReplace);
			objWriter.WriteElementString("ap", _strAP);
			objWriter.WriteElementString("apreplace", _strAPReplace);
			objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
			objWriter.WriteElementString("addmode", _strAddMode);
			objWriter.WriteElementString("fullburst", _intFullBurst.ToString());
			objWriter.WriteElementString("suppressive", _intSuppressive.ToString());
			objWriter.WriteElementString("rangebonus", _intRangeBonus.ToString());
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("ammobonus", _intAmmoBonus.ToString());
			objWriter.WriteEndElement();
			_objCharacter.SourceProcess(_strSource);
		}

		/// <summary>
		/// Load the CharacterAttribute from the XmlNode.
		/// </summary>
		/// <param name="objNode">XmlNode to load.</param>
		/// <param name="blnCopy">Whether another node is being copied.</param>
		public void Load(XmlNode objNode, bool blnCopy = false)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strMount = objNode["mount"].InnerText;
			_strRC = objNode["rc"].InnerText;
			objNode.TryGetField("rating", out _intRating,0);
			objNode.TryGetField("rcgroup", out _intRCGroup, 0);
			objNode.TryGetField("accuracy", out _intAccuracy, 0);
			objNode.TryGetField("rating", out _intRating, 0);
			objNode.TryGetField("rating", out _intRating, 0);
			objNode.TryGetField("conceal", out _strConceal, "0");
			objNode.TryGetField("rcdeployable", out _blnDeployable);
			_strAvail = objNode["avail"].InnerText;
			_strCost = objNode["cost"].InnerText;
			_blnIncludedInWeapon = Convert.ToBoolean(objNode["included"].InnerText);
			objNode.TryGetField("installed", out _blnInstalled, true);
			try
			{
				_nodAllowGear = objNode["allowgear"];
			}
			catch
			{
			}
			_strSource = objNode["source"].InnerText;

			objNode.TryGetField("page", out _strPage, "0");
			objNode.TryGetField("dicepool", out _strDicePool, "0");
			
			if (objNode.InnerXml.Contains("ammoslots"))
			{
				objNode.TryGetField("ammoslots", out _intAmmoSlots, 0);  //TODO: Might work if 0 -> 1
			}


			if (objNode.InnerXml.Contains("<gears>"))
			{
				XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
				foreach (XmlNode nodChild in nodChildren)
				{
					switch (nodChild["category"].InnerText)
					{
						case "Commlinks":
						case "Commlink Accessories":
						case "Cyberdecks":
						case "Rigger Command Consoles":
							Commlink objCommlink = new Commlink(_objCharacter);
							objCommlink.Load(nodChild, blnCopy);
							_lstGear.Add(objCommlink);
							break;
						default:
							Gear objGear = new Gear(_objCharacter);
							objGear.Load(nodChild, blnCopy);
							_lstGear.Add(objGear);
							break;
					}
				}
			}
			objNode.TryGetField("notes", out _strNotes, "");
			objNode.TryGetField("discountedcost", out _blnDiscountCost, false);

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("weapons.xml");
				XmlNode objAccessoryNode = objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + _strName + "\"]");
				if (objAccessoryNode != null)
				{
					if (objAccessoryNode["translate"] != null)
						_strAltName = objAccessoryNode["translate"].InnerText;
					if (objAccessoryNode["altpage"] != null)
						_strAltPage = objAccessoryNode["altpage"].InnerText;
				}
			}
			if (objNode["damage"] != null)
			{
				_strDamage = objNode["damage"].InnerText;
			}
			if (objNode["damagetype"] != null)
			{
				_strDamageType = objNode["damagetype"].InnerText;
			}
			if (objNode["damagereplace"] != null)
			{
				_strDamageReplace = objNode["damagereplace"].InnerText;
			}
			if (objNode["firemode"] != null)
			{
				_strFireMode = objNode["firemode"].InnerText;
			}
			if (objNode["firemodereplace"] != null)
			{
				_strFireModeReplace = objNode["firemodereplace"].InnerText;
			}

			if (objNode["ap"] != null)
			{
				_strAP = objNode["ap"].InnerText;
			}
			if (objNode["apreplace"] != null)
			{
				_strAPReplace = objNode["apreplace"].InnerText;
			}
			objNode.TryGetField("accessorycostmultiplier", out _intAccessoryCostMultiplier);
			objNode.TryGetField("addmode", out _strAddMode);
			objNode.TryGetField("fullburst", out _intFullBurst,0);
			objNode.TryGetField("suppressive", out _intSuppressive,0);
			objNode.TryGetField("rangebonus", out _intRangeBonus,0);
			objNode.TryGetField("extra", out _strExtra,"");
			objNode.TryGetField("ammobonus", out _intAmmoBonus,0);

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
			objWriter.WriteStartElement("accessory");
			objWriter.WriteElementString("name", DisplayName);
			objWriter.WriteElementString("mount", _strMount);
			objWriter.WriteElementString("rc", _strRC);
			objWriter.WriteElementString("conceal", _strConceal);
			objWriter.WriteElementString("avail", TotalAvail);
			objWriter.WriteElementString("cost", TotalCost.ToString());
			objWriter.WriteElementString("owncost", OwnCost.ToString());
			objWriter.WriteElementString("included", _blnIncludedInWeapon.ToString());
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			objWriter.WriteElementString("accuracy", _intAccuracy.ToString());
			if (_lstGear.Count > 0)
			{
				objWriter.WriteStartElement("gears");
				foreach (Gear objGear in _lstGear)
				{
					// Use the Gear's SubClass if applicable.
					if (objGear.GetType() == typeof(Commlink))
					{
						Commlink objCommlink = new Commlink(_objCharacter);
						objCommlink = (Commlink)objGear;
						objCommlink.Print(objWriter);
					}
					else
					{
						objGear.Print(objWriter);
					}
				}
				objWriter.WriteEndElement();
			}
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
		#endregion

		#region Properties
		/// <summary>
		/// Internal identifier which will be used to identify this Weapon.
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
		/// The accessory adds to the weapon's ammunition slots.
		/// </summary>
		public int AmmoSlots
		{
			get
			{
				return _intAmmoSlots;
			}
			set
			{
				_intAmmoSlots = value;
			}
		}
		/// <summary>
		/// The accessory adds to the weapon's damage value.
		/// </summary>
		public string Damage
		{
			get
			{
				return _strDamage;
			}
			set
			{
				_strDamage = value;
			}
		}
		/// <summary>
		/// The Accessory replaces the weapon's damage value.
		/// </summary>
		public string DamageReplacement
		{
			get
			{
				return _strDamageReplace;
			}
			set
			{
				_strDamageReplace = value;
			}
		}

		/// <summary>
		/// The Accessory changes the Damage Type.
		/// </summary>
		public string DamageType
		{
			get
			{
				return _strDamageType;
			}
			set
			{
				_strDamageType = value;
			}
		}

		/// <summary>
		/// The accessory adds to the weapon's Armor Penetration.
		/// </summary>
		public string AP
		{
			get
			{
				return _strAP;
			}
			set
			{
				_strAP = value;
			}
		}

		/// <summary>
		/// Whether the Accessory only grants a Recoil Bonus while deployed.
		/// </summary>
		public bool RCDeployable
		{
			get
			{
				return _blnDeployable;
			}
		}

		/// <summary>
		/// Accuracy.
		/// </summary>
		public int Accuracy
		{
			get
			{
				if (_blnInstalled)
					return _intAccuracy;
				else
					return 0;
			}
		}

		/// <summary>
		/// Concealability.
		/// </summary>
		public string APReplacement
		{
			get
			{
				return _strAPReplace;
			}
			set
			{
				_strAPReplace = value;
			}
		}

		/// <summary>
		/// The accessory adds a Fire Mode to the weapon.
		/// </summary>
		public string FireMode
		{
			get
			{
				return _strFireMode;
			}
			set
			{
				_strFireMode = value;
			}
		}

		/// <summary>
		/// The accessory replaces the weapon's Fire Modes.
		/// </summary>
		public string FireModeReplacement
		{
			get
			{
				return _strFireModeReplace;
			}
			set
			{
				_strFireModeReplace = value;
			}
		}

		/// <summary>
		/// The name of the object as it should appear on printouts (translated name only).
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
		/// The name of the object as it should be displayed in lists. Name (Extra).
		/// </summary>
		public string DisplayName
		{
			get
			{
				string strReturn = DisplayNameShort;

				return strReturn;
			}
		}

		/// <summary>
		/// Mount Used.
		/// </summary>
		public string Mount
		{
			get
			{
				return _strMount;
			}
			set
			{
				_strMount = value;
			}
		}

		/// <summary>
		/// Recoil.
		/// </summary>
		public string RC
		{
			get
			{
				return _strRC;
			}
			set
			{
				_strRC = value;
			}
		}

		/// <summary>
		/// Recoil Group.
		/// </summary>
		public int RCGroup
		{
			get
			{
				return _intRCGroup;
			}
		}

		/// <summary>
		/// Concealability.
		/// </summary>
		public int Concealability
		{
			get
			{
				int intReturn = 0;

				if (_strConceal.Contains("Rating"))
				{
					// If the cost is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strConceal = "";
					string strCostExpression = _strConceal;

					strConceal = strCostExpression.Replace("Rating", _intRating.ToString());
					XPathExpression xprCost = nav.Compile(strConceal);
					double dblConceal = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));
					intReturn = Convert.ToInt32(dblConceal);
				}
				else if (_strConceal != "")
				{
					intReturn = Convert.ToInt32(_strConceal);
				}
				return intReturn;
			}
			set
			{
				_strConceal = value.ToString();
			}
		}

		/// <summary>
		/// Concealability.
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
		/// Avail.
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
		/// Cost.
		/// </summary>
		public string Cost
		{
			get
			{
				// The Accessory has a cost of 0 if it is included in the base weapon configureation.
				if (_blnIncludedInWeapon)
					return "0";
				else
					return _strCost;
			}
			set
			{
				_strCost = value;
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
		/// Whether or not this Accessory is part of the base weapon configuration.
		/// </summary>
		public bool IncludedInWeapon
		{
			get
			{
				return _blnIncludedInWeapon;
			}
			set
			{
				_blnIncludedInWeapon = value;
			}
		}

		/// <summary>
		/// Whether or not this Accessory is installed and contributing towards the Weapon's stats.
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
		/// Total Availability.
		/// </summary>
		public string TotalAvail
		{
			get
			{
				// If the Avail contains "+", return the base string and don't try to calculate anything since we're looking at a child component.
				
				string strCalculated = "";
				string strReturn = "";

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
		/// AllowGear node from the XML file.
		/// </summary>
		public XmlNode AllowGear
		{
			get
			{
				return _nodAllowGear;
			}
			set
			{
				_nodAllowGear = value;
			}
		}

		/// <summary>
		/// A List of the Gear attached to the Cyberware.
		/// </summary>
		public List<Gear> Gear
		{
			get
			{
				return _lstGear;
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

		/// <summary>
		/// Parent Weapon.
		/// </summary>
		public Weapon Parent
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
		/// Total cost of the Weapon Accessory.
		/// </summary>
		public int TotalCost
		{
			get
			{
				int intReturn = 0;

				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();

				string strCost = "";
				string strCostExpression = _strCost;

				strCost = strCostExpression.Replace("Weapon Cost", _objParent.Cost.ToString());
				strCost = strCost.Replace("Rating", _intRating.ToString());
				XPathExpression xprCost = nav.Compile(strCost);
				intReturn = (Convert.ToInt32(nav.Evaluate(xprCost).ToString()) * _objParent.CostMultiplier);

				if (DiscountCost)
					intReturn = Convert.ToInt32(Convert.ToDouble(intReturn, GlobalOptions.Instance.CultureInfo) * 0.9);

				// Add in the cost of any Gear the Weapon Accessory has attached to it.
				foreach (Gear objGear in _lstGear)
					intReturn += objGear.TotalCost;

				return intReturn;
			}
		}

		/// <summary>
		/// The cost of just the Weapon Accessory itself.
		/// </summary>
		public int OwnCost
		{
			get
			{
				int intReturn = 0;

				if (_strCost.Contains("Rating") || _strCost.Contains("Weapon Cost"))
				{
					// If the cost is determined by the Rating, evaluate the expression.
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCost = "";
					string strCostExpression = _strCost;

					strCost = strCostExpression.Replace("Rating", _intRating.ToString());
					strCost = strCost.Replace("Weapon Cost", _objParent.Cost.ToString());
					XPathExpression xprCost = nav.Compile(strCost);
					double dblCost = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.Instance.CultureInfo));
					intReturn = Convert.ToInt32(dblCost);
				}
				else
					intReturn = Convert.ToInt32(_strCost) * _objParent.CostMultiplier;

				if (DiscountCost)
					intReturn = Convert.ToInt32(Convert.ToDouble(intReturn, GlobalOptions.Instance.CultureInfo) * 0.9);

				return intReturn;
			}
		}

		/// <summary>
		/// Dice Pool modifier.
		/// </summary>
		public int DicePool
		{
			get
			{
				int intReturn = 0;

				if (_strDicePool != string.Empty)
					intReturn = Convert.ToInt32(_strDicePool);

				return intReturn;
			}
		}

		private string DicePoolString
		{
			get
			{
				return _strDicePool;
			}
		}

		/// <summary>
		/// Adjust the Weapon's Ammo amount by the specified percent.
		/// </summary>
		public int AmmoBonus
		{
			get
			{
				return _intAmmoBonus;
			}
			set
			{
				_intAmmoBonus = value;
			}
		}

		/// <summary>
		/// Replace the Weapon's Ammo value with the Weapon Mod's value.
		/// </summary>
		public string AmmoReplace
		{
			get
			{
				return _strAmmoReplace;
			}
			set
			{
				_strAmmoReplace = value;
			}
		}

		/// <summary>
		/// Multiply the cost of other installed Accessories.
		/// </summary>
		public int AccessoryCostMultiplier
		{
			get
			{
				return _intAccessoryCostMultiplier;
			}
			set
			{
				_intAccessoryCostMultiplier = value;
			}
		}

		/// <summary>
		/// Additional Weapon Firing Mode.
		/// </summary>
		public string AddMode
		{
			get
			{
				return _strAddMode;
			}
			set
			{
				_strAddMode = value;
			}
		}

		/// <summary>
		/// Number of rounds consumed by Full Burst.
		/// </summary>
		public int FullBurst
		{
			get
			{
				return _intFullBurst;
			}
		}

		/// <summary>
		/// Number of rounds consumed by Suppressive Fire.
		/// </summary>
		public int Suppressive
		{
			get
			{
				return _intSuppressive;
			}
		}

		/// <summary>
		/// Range bonus granted by the Accessory.
		/// </summary>
		public int RangeBonus
		{
			get
			{
				return _intRangeBonus;
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
		/// Whether the Accessory is affected by Black Market Discounts.
		/// </summary>
		public bool BlackMarketDiscount
		{
			get
			{
				return _blnBlackMarketDiscount;
			}
			set
			{
				_blnBlackMarketDiscount = value;
			}
		}
		#endregion
	}
}