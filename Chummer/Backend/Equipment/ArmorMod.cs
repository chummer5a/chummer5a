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
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
	/// <summary>
	/// A piece of Armor Modification.
	/// </summary>
	public class ArmorMod
	{
		private Guid _guiID = new Guid();
		private string _strName = "";
		private string _strCategory = "";
		private string _strArmorCapacity = "[0]";
		private int _intA = 0;
		private int _intMaxRating = 0;
		private int _intRating = 0;
		private string _strAvail = "";
		private string _strCost = "";
		private string _strSource = "";
		private string _strPage = "";
		private bool _blnIncludedInArmor = false;
		private bool _blnEquipped = true;
		private string _strExtra = "";
		private Guid _guiWeaponID = new Guid();
		private XmlNode _nodBonus;
		private readonly Character _objCharacter;
		private string _strNotes = "";
		private string _strAltName = "";
		private string _strAltCategory = "";
		private string _strAltPage = "";
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
		public void Create(XmlNode objXmlArmorNode, TreeNode objNode, int intRating, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, bool blnSkipCost = false)
		{
			_strName = objXmlArmorNode["name"].InnerText;
			_strCategory = objXmlArmorNode["category"].InnerText;
			_strArmorCapacity = objXmlArmorNode["armorcapacity"].InnerText;
			_intA = Convert.ToInt32(objXmlArmorNode["armor"].InnerText);
			_intRating = intRating;
			_intMaxRating = Convert.ToInt32(objXmlArmorNode["maxrating"].InnerText);
			_strAvail = objXmlArmorNode["avail"].InnerText;
			_strCost = objXmlArmorNode["cost"].InnerText;
			_strSource = objXmlArmorNode["source"].InnerText;
			_strPage = objXmlArmorNode["page"].InnerText;
			_nodBonus = objXmlArmorNode["bonus"];

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("armor.xml");
				XmlNode objArmorNode = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + _strName + "\"]");
				if (objArmorNode != null)
				{
					if (objArmorNode["translate"] != null)
						_strAltName = objArmorNode["translate"].InnerText;
					if (objArmorNode["altpage"] != null)
						_strAltPage = objArmorNode["altpage"].InnerText;
				}

				objArmorNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objArmorNode != null)
				{
					if (objArmorNode.Attributes["translate"] != null)
						_strAltCategory = objArmorNode.Attributes["translate"].InnerText;
				}
			}

			if (objXmlArmorNode["bonus"] != null && !blnSkipCost)
			{
				ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
				if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.ArmorMod, _guiID.ToString(), objXmlArmorNode["bonus"], false, intRating, DisplayNameShort))
				{
					_guiID = Guid.Empty;
					return;
				}
				if (objImprovementManager.SelectedValue != "")
				{
					_strExtra = objImprovementManager.SelectedValue;
					objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
				}
			}

			// Add Weapons if applicable.
			if (objXmlArmorNode.InnerXml.Contains("<addweapon>"))
			{
				XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");

				// More than one Weapon can be added, so loop through all occurrences.
				foreach (XmlNode objXmlAddWeapon in objXmlArmorNode.SelectNodes("addweapon"))
				{
					XmlNode objXmlWeapon = objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlAddWeapon.InnerText + "\" and starts-with(category, \"Cyberware\")]");

					TreeNode objGearWeaponNode = new TreeNode();
					Weapon objGearWeapon = new Weapon(_objCharacter);
					objGearWeapon.Create(objXmlWeapon, _objCharacter, objGearWeaponNode, null, null);
					objGearWeaponNode.ForeColor = SystemColors.GrayText;
					objWeaponNodes.Add(objGearWeaponNode);
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
			objWriter.WriteElementString("armor", _intA.ToString());
			objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
			objWriter.WriteElementString("maxrating", _intMaxRating.ToString());
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("cost", _strCost);
			if (_nodBonus != null)
				objWriter.WriteRaw(_nodBonus.OuterXml);
			else
				objWriter.WriteElementString("bonus", "");
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
		public void Load(XmlNode objNode, bool blnCopy = false)
		{
			_guiID = Guid.Parse(objNode["guid"].InnerText);
			_strName = objNode["name"].InnerText;
			_strCategory = objNode["category"].InnerText;
			_intA = Convert.ToInt32(objNode["armor"].InnerText);
			try
			{
				_strArmorCapacity = objNode["armorcapacity"].InnerText;
			}
			catch
			{
			}
			_intMaxRating = Convert.ToInt32(objNode["maxrating"].InnerText);
			_intRating = Convert.ToInt32(objNode["rating"].InnerText);
			_strAvail = objNode["avail"].InnerText;
			_strCost = objNode["cost"].InnerText;
			try
			{
				_nodBonus = objNode["bonus"];
			}
			catch
			{
			}
			_strSource = objNode["source"].InnerText;
			try
			{
				_strPage = objNode["page"].InnerText;
			}
			catch
			{
			}
			try
			{
				_blnIncludedInArmor = Convert.ToBoolean(objNode["included"].InnerText);
			}
			catch
			{
			}
			try
			{
				_blnEquipped = Convert.ToBoolean(objNode["equipped"].InnerText);
			}
			catch
			{
			}
			try
			{
				_strExtra = objNode["extra"].InnerText;
			}
			catch
			{
			}
			try
			{
				_guiWeaponID = Guid.Parse(objNode["weaponguid"].InnerText);
			}
			catch
			{
			}
			try
			{
				_strNotes = objNode["notes"].InnerText;
			}
			catch
			{
			}

			try
			{
				_blnDiscountCost = Convert.ToBoolean(objNode["discountedcost"].InnerText);
			}
			catch
			{
			}

			if (GlobalOptions.Instance.Language != "en-us")
			{
				XmlDocument objXmlDocument = XmlManager.Instance.Load("armor.xml");
				XmlNode objArmorNode = objXmlDocument.SelectSingleNode("/chummer/mods/mod[name = \"" + _strName + "\"]");
				if (objArmorNode != null)
				{
					if (objArmorNode["translate"] != null)
						_strAltName = objArmorNode["translate"].InnerText;
					if (objArmorNode["altpage"] != null)
						_strAltPage = objArmorNode["altpage"].InnerText;
				}

				objArmorNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
				if (objArmorNode != null)
				{
					if (objArmorNode.Attributes["translate"] != null)
						_strAltCategory = objArmorNode.Attributes["translate"].InnerText;
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
			objWriter.WriteStartElement("armormod");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("name_english", _strName);
			objWriter.WriteElementString("category", DisplayCategory);
			objWriter.WriteElementString("category_english", _strCategory);
			objWriter.WriteElementString("armor", _intA.ToString());
			objWriter.WriteElementString("maxrating", _intMaxRating.ToString());
			objWriter.WriteElementString("rating", _intRating.ToString());
			objWriter.WriteElementString("avail", TotalAvail);
			objWriter.WriteElementString("cost", TotalCost.ToString());
			objWriter.WriteElementString("owncost", OwnCost.ToString());
			objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			objWriter.WriteElementString("included", _blnIncludedInArmor.ToString());
			objWriter.WriteElementString("equipped", _blnEquipped.ToString());
			objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
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

				if (_intRating > 0)
					strReturn += " (" + LanguageManager.Instance.GetString("String_Rating") + " " + _intRating.ToString() + ")";
				if (_strExtra != "")
					strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
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
				string strReturn = _strCategory;
				if (_strAltCategory != string.Empty)
					strReturn = _strAltCategory;

				return strReturn;
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
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCost = "";
					string strCostExpression = _strCost;

					strCost = strCostExpression.Replace("Rating", _intRating.ToString());
					XPathExpression xprCost = nav.Compile(strCost);
					return nav.Evaluate(xprCost).ToString();
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
				if (_strAvail.Contains("+"))
					return _strAvail;

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
		/// Caculated Capacity of the Armor Mod.
		/// </summary>
		public string CalculatedCapacity
		{
			get
			{
				XmlDocument objXmlDocument = new XmlDocument();
				XPathNavigator nav = objXmlDocument.CreateNavigator();
				if (_strArmorCapacity == "")
					return "0";
				string strCapacity = _strArmorCapacity;
				strCapacity = strCapacity.Replace("Capacity", this._objParent.ArmorCapacity);
				strCapacity = strCapacity.Replace("Rating", _intRating.ToString());
				if (strCapacity.StartsWith("FixedValues"))
				{
					string[] strValues = strCapacity.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					strCapacity = strValues[Convert.ToInt32(_intRating) - 1];
				}
				bool blnSquareBrackets = strCapacity.Contains('[');
				if (blnSquareBrackets)
					strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
				XPathExpression xprCapacity = nav.Compile(strCapacity);

				decimal decCapacity = Convert.ToDecimal(nav.Evaluate(xprCapacity));
				string strReturn = "";

				//Rounding is always 'up'. For items that generate capacity, this means making it a larger negative number. 
				if (decCapacity > 0)
				{
					strReturn = Math.Ceiling(decCapacity).ToString();
				}
				else
				{
					strReturn = Math.Floor(decCapacity).ToString();
				}
				if (blnSquareBrackets)
					strReturn = "[" + strReturn + "]";

				return strReturn;
			}
		}

		/// <summary>
		/// Total cost of the Armor Mod.
		/// </summary>
		public int TotalCost
		{
			get
			{
				int intReturn = 0;

				if (_strCost.Contains("Armor Cost"))
				{
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCostExpr = _strCost.Replace("Armor Cost", _objParent.Cost.ToString());
					XPathExpression xprCost = nav.Compile(strCostExpr);
					intReturn = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost).ToString(), GlobalOptions.Instance.CultureInfo)));
				}
				else if (_strCost.Contains("Rating"))
				{
					XmlDocument objXmlDocument = new XmlDocument();
					XPathNavigator nav = objXmlDocument.CreateNavigator();

					string strCostExpr = _strCost.Replace("Rating", _intRating.ToString());
					XPathExpression xprCost = nav.Compile(strCostExpr);
					intReturn = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost).ToString(), GlobalOptions.Instance.CultureInfo)));
				}
				else if (_strCost.StartsWith("FixedValues"))
				{
					string[] strValues = _strCost.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
					intReturn = Convert.ToInt32(strValues[Convert.ToInt32(_intRating) - 1]);
				}
				else
					intReturn = Convert.ToInt32(_strCost);

				if (DiscountCost)
					intReturn = Convert.ToInt32(Convert.ToDouble(intReturn, GlobalOptions.Instance.CultureInfo) * 0.9);

				return intReturn;
			}
		}

		/// <summary>
		/// Cost for just the Armor Mod.
		/// </summary>
		public int OwnCost
		{
			get
			{
				return TotalCost;
			}
		}
		#endregion
	}
}