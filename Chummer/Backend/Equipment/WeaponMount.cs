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
using System.Net;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
	/// <summary>
	/// Vehicle Modification.
	/// </summary>
	public class WeaponMount : IHasInternalId, IHasName, IHasXmlNode
    {
		private Guid _guiID;
		private decimal _decMarkup;
		private string _strAvail = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
		private bool _blnIncludeInVehicle;
		private bool _blnInstalled = true;
		private IList<Weapon> _lstWeapons = new List<Weapon>();
		private string _strNotes = string.Empty;
		private string _strExtra = string.Empty;
		private string _strWeaponMountCategories = string.Empty;
		private bool _blnDiscountCost;
		private string _strName = string.Empty;
		private string _strCategory = string.Empty;
		private string _strLimit = string.Empty;
		private int _intSlots = 0;
		private string _strCost = string.Empty;
        private string _strSourceId = string.Empty;

        private XmlNode _objCachedMyXmlNode = null;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private IList<VehicleMod> _lstMods = new List<VehicleMod>();

        private readonly Vehicle _vehicle;
	    private readonly Character _objCharacter;

		#region Constructor, Create, Save, Load, and Print Methods
		public WeaponMount(Character character, Vehicle vehicle)
		{
			// Create the GUID for the new VehicleMod.
			_guiID = Guid.NewGuid();
		    _objCharacter = character;
			_vehicle = vehicle;
        }

        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="objParent">Vehicle that the mod will be attached to.</param>
        /// <param name="decMarkup">Discount or markup that applies to the base cost of the mod.</param>
        public void Create(XmlNode objXmlMod, decimal decMarkup = 0)
        {
            if (objXmlMod == null) Utils.BreakIfDebug();
            objXmlMod.TryGetStringFieldQuickly("id", ref _strSourceId);
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetStringFieldQuickly("limit", ref _strLimit);
            objXmlMod.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objXmlMod.TryGetStringFieldQuickly("weaponcategories", ref _strWeaponMountCategories);
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);
            if (!objXmlMod.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMod.TryGetStringFieldQuickly("notes", ref _strNotes);
            // Check for a Variable Cost.
            _strCost = objXmlMod["cost"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(_strCost))
            {
                if (_strCost.StartsWith("Variable("))
                {
                    decimal decMin = 0;
                    decimal decMax = decimal.MaxValue;
                    string strCost = _strCost.TrimStart("Variable(", true).TrimEnd(')');
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
                }
            }
            _decMarkup = decMarkup;

            objXmlMod.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMod.TryGetStringFieldQuickly("page", ref _strPage);
        }

		/// <summary>
		/// Save the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Save(XmlTextWriter objWriter)
		{
			objWriter.WriteStartElement("weaponmount");
			objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("sourceid", _strSourceId);
            objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("category", _strCategory);
			objWriter.WriteElementString("limit", _strLimit);
			objWriter.WriteElementString("slots", _intSlots.ToString());
			objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("cost", _strCost);
			objWriter.WriteElementString("markup", _decMarkup.ToString(CultureInfo.InvariantCulture));
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString());
			objWriter.WriteElementString("installed", _blnInstalled.ToString());
			objWriter.WriteElementString("weaponmountcategories", _strWeaponMountCategories);
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
            objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString());
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
            if (!objNode.TryGetStringFieldQuickly("sourceid", ref _strSourceId))
            {
                _strSourceId = XmlManager.Load("vehicles.xml").SelectSingleNode("/chummer/weaponmounts/weaponmount[name = \"" + _strName + "\"]/id")?.InnerText ?? Guid.NewGuid().ToString("D");
            }
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
			objNode.TryGetStringFieldQuickly("limit", ref _strLimit);
			objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
			objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
			objNode.TryGetStringFieldQuickly("page", ref _strPage);
			objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
			objNode.TryGetStringFieldQuickly("cost", ref _strCost);
			objNode.TryGetDecFieldQuickly("markup", ref _decMarkup);
			objNode.TryGetStringFieldQuickly("source", ref _strSource);
			objNode.TryGetBoolFieldQuickly("included", ref _blnIncludeInVehicle);
			objNode.TryGetBoolFieldQuickly("installed", ref _blnInstalled);

            XmlNode xmlChildrenNode = objNode["weapons"];
            if (xmlChildrenNode != null)
			{
                foreach (XmlNode xmlWeaponNode in xmlChildrenNode.SelectNodes("weapon"))
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
            xmlChildrenNode = objNode["weaponmountoptions"];
            if (xmlChildrenNode != null)
            {
                foreach (XmlNode xmlWeaponMountOptionNode in xmlChildrenNode.SelectNodes("weaponmountoption"))
                {
                    WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                    objWeaponMountOption.Load(xmlWeaponMountOptionNode, Parent);
                    WeaponMountOptions.Add(objWeaponMountOption);
                }
            }
            xmlChildrenNode = objNode["mods"];
            if (xmlChildrenNode != null)
            {
		        foreach (XmlNode xmlModNode in xmlChildrenNode.SelectNodes("mod"))
		        {
                    VehicleMod objMod = new VehicleMod(_objCharacter)
                    {
                        Parent = Parent
                    };
                    objMod.Load(xmlModNode);
		            _lstMods.Add(objMod);
		        }
            }
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
			objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
			objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
        }

		/// <summary>
		/// Print the object's XML to the XmlWriter.
		/// </summary>
		/// <param name="objWriter">XmlTextWriter to write with.</param>
		public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
		{
			objWriter.WriteStartElement("mod");
			objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
			objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
			objWriter.WriteElementString("limit", Limit);
			objWriter.WriteElementString("slots", Slots.ToString());
			objWriter.WriteElementString("avail", TotalAvail(strLanguageToPrint));
			objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
			objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
			objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
			objWriter.WriteElementString("page", Page(strLanguageToPrint));
			objWriter.WriteElementString("included", IncludedInVehicle.ToString());
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
			if (_objCharacter.Options.PrintNotes)
				objWriter.WriteElementString("notes", Notes);
			objWriter.WriteEndElement();
		}
        /// <summary>
        /// Create a weapon mount using names instead of IDs, because user readability is important and untrustworthy. 
        /// </summary>
        /// <param name="xmlNode"></param>
        public void CreateByName(XmlNode xmlNode)
        {
            XmlDocument xmlDoc = XmlManager.Load("vehicles.xml");
            WeaponMount objMount = this;
            XmlNode xmlDataNode = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{xmlNode["size"].InnerText}\" and category = \"Size\"]");
            objMount.Create(xmlDataNode);

            WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
            xmlDataNode = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{xmlNode["flexibility"].InnerText}\" and category = \"Flexibility\"]");
            objWeaponMountOption.Create(xmlDataNode);
            objMount.WeaponMountOptions.Add(objWeaponMountOption);

            objWeaponMountOption = new WeaponMountOption(_objCharacter);
            xmlDataNode = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{xmlNode["control"].InnerText}\" and category = \"Control\"]");
            objWeaponMountOption.Create(xmlDataNode);
            objMount.WeaponMountOptions.Add(objWeaponMountOption);

            objWeaponMountOption = new WeaponMountOption(_objCharacter);
            xmlDataNode = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{xmlNode["visibility"].InnerText}\" and category = \"Visibility\"]");
            objWeaponMountOption.Create(xmlDataNode);
            objMount.WeaponMountOptions.Add(objWeaponMountOption);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapons.
        /// </summary>
        public IList<Weapon> Weapons
		{
			get
			{
				return _lstWeapons;
			}
		}

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString("D");
            }
        }

        /// <summary>
        /// Identifier of the WeaponMount's Size in the data files.
        /// </summary>
        public string SourceId
        {
            get
            {
                return _strSourceId;
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
        public int Slots
        {
            get
            {
                return _intSlots;
            }
            set
            {
                _intSlots = value;
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
        public string Page(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
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
                return _blnDiscountCost && _objCharacter.BlackMarketDiscount;
            }
            set
            {
                _blnDiscountCost = value;
            }
        }

        /// <summary>
        /// Vehicle that the Mod is attached to. 
        /// </summary>
        public Vehicle Parent => _vehicle;

        /// <summary>
        /// 
        /// </summary>
        public IList<WeaponMountOption> WeaponMountOptions { get; } = new List<WeaponMountOption>();
        #endregion

        #region Complex Properties

        /// <summary>
        /// The number of Slots the Mount consumes, including all child items.
        /// </summary>
        public int CalculatedSlots => Slots + WeaponMountOptions.Sum(w => w.Slots) + Mods.Sum(m => m.CalculatedSlots);

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(string strLanguage)
        {
            Tuple<int, string> objAvailPair = TotalAvailPair;
            string strAvail = objAvailPair.Item2;
            // Translate the Avail string.
            if (strAvail == "F")
                strAvail = LanguageManager.GetString("String_AvailForbidden", strLanguage);
            else if (strAvail == "R")
                strAvail = LanguageManager.GetString("String_AvailRestricted", strLanguage);

            return objAvailPair.Item1.ToString() + strAvail;
        }

        /// <summary>
        /// Total Availability as a pair: first item is availability magnitude, second is untranslated Restricted/Forbidden state (empty of neither).
        /// </summary>
        public Tuple<int, string> TotalAvailPair
        {
            get
            {
                if (_strAvail.Length == 0)
                    return new Tuple<int, string>(0, string.Empty);
                string strAvail = string.Empty;
                string strAvailExpr = _strAvail;
                int intAvail = 0;

                if (strAvailExpr.Substring(_strAvail.Length - 1, 1) == "F" || strAvailExpr.Substring(_strAvail.Length - 1, 1) == "R")
                {
                    strAvail = strAvailExpr.Substring(_strAvail.Length - 1, 1);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpr = strAvailExpr.Substring(0, _strAvail.Length - 1);
                }
                /*if (strAvailExpr.Contains("{Children Avail}"))
                {
                    int intMaxChildAvail = 0;
                    foreach (Weapon w in Weapons)
                    {
                        Tuple<int, string> objLoopAvail = w.TotalAvailPair;
                        if (objLoopAvail.Item1 > intMaxChildAvail)
                            intMaxChildAvail = objLoopAvail.Item1;
                        if (objLoopAvail.Item2.EndsWith('F'))
                            strAvail = "F";
                        else if (objLoopAvail.Item2.EndsWith('R') && strAvail != "F")
                            strAvail = "R";
                    }
                    strAvailExpr = strAvailExpr.Replace("{Children Avail}", intMaxChildAvail.ToString());
                }*/
                try
                {
                    intAvail = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpr));
                }
                catch (XPathException)
                {
                }

                // Run through the Accessories and add in their availability.
                foreach (WeaponMountOption wm in WeaponMountOptions)
                {
                    string strAccAvail = wm.Avail;
                    int intAccAvail = 0;

                    if (strAccAvail.StartsWith('+', '-'))
                    {
                        strAccAvail += wm.TotalAvail(GlobalOptions.DefaultLanguage);
                        if (strAccAvail.EndsWith('F'))
                        {
                            strAvail = "F";
                            strAccAvail = strAccAvail.Substring(0, strAccAvail.Length - 1);
                        }
                        else if (strAccAvail.EndsWith('R'))
                        {
                            if (strAvail != "F")
                                strAvail = "R";
                            strAccAvail = strAccAvail.Substring(0, strAccAvail.Length - 1);
                        }
                        intAccAvail = Convert.ToInt32(strAccAvail);
                        intAvail += intAccAvail;
                    }
                }
                return new Tuple<int, string>(intAvail, strAvail);
            }
        }

        /// <summary>
        /// Total cost of the WeaponMount.
        /// </summary>
        public decimal TotalCost
		{
			get
			{
			    if (IncludedInVehicle)
			    {
			        return 0;
			    }
				return OwnCost + Weapons.Sum(w => w.TotalCost) + WeaponMountOptions.Sum(w => w.Cost) + Mods.Sum(m => m.TotalCost);
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
                decimal decReturn = Convert.ToDecimal(_strCost, GlobalOptions.InvariantCultureInfo);

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

        public IList<VehicleMod> Mods
        {
            get => _lstMods;
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

            if (WeaponMountOptions.Count > 0)
            {
                strReturn += $" ({string.Join(", ", WeaponMountOptions.Select(wm => wm.DisplayName(strLanguage)).ToArray())})";
            }

            return strReturn;
        }

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = XmlManager.Load("vehicles.xml", strLanguage)?.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + _strSourceId + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a Weapon Mount to the TreeView
        /// </summary>
        /// <param name="objWeaponMount">WeaponMount that we're creating.</param>
        /// <param name="parentNode">Parent treenode to add to.</param>
        /// <param name="cmsVehicleWeapon">ContextMenuStrip for Vehicle Weapons</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip for Vehicle Weapon Accessories</param>
        /// <param name="cmsWeaponAccessoryGear">ContextMenuStrip for Vehicle Weapon Gear</param>
        /// <param name="cmsVehicleWeaponMount">ContextMenuStrip for Vehicle Weapon Mounts</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsVehicleWeaponMount, ContextMenuStrip cmsVehicleWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear, ContextMenuStrip cmsCyberware, ContextMenuStrip cmsCyberwareGear, ContextMenuStrip cmsVehicleMod)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = InternalId,
                ContextMenuStrip = cmsVehicleWeaponMount
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (IncludedInVehicle)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);
            // VehicleMods.
            foreach (VehicleMod objMod in Mods)
            {
                objNode.Nodes.Add(objMod.CreateTreeNode(cmsVehicleMod, cmsCyberware, cmsCyberwareGear, cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear));
                objNode.Expand();
            }
            foreach (Weapon objWeapon in Weapons)
            {
                objNode.Nodes.Add(objWeapon.CreateTreeNode(cmsVehicleWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear));
                objNode.Expand();
            }

            return objNode;
        }
        #endregion
    }

    public class WeaponMountOption : IHasName, IHasXmlNode
    {
        private readonly Character _objCharacter;
        private string _strAvail;
        private string _strName;
        private Guid _sourceID;
        private string _strCost;
        private string _strCategory;
        private int _intSlots;
        private string _strWeaponMountCategories;

        #region Constructor, Create, Save and Load Methods
        public WeaponMountOption(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a Weapon Mount Option from an XmlNode, returns true if creation was successful.
        /// </summary>
        /// <param name="id">String guid of the object.</param>
        public bool Create(XmlNode objXmlMod)
        {
            if (objXmlMod == null)
            {
                Utils.BreakIfDebug();
                return false;
            }
            objXmlMod.TryGetField("id", Guid.TryParse, out _sourceID);
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objXmlMod.TryGetStringFieldQuickly("weaponcategories", ref _strWeaponMountCategories);
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);

            // Check for a Variable Cost.
            // ReSharper disable once PossibleNullReferenceException
            _strCost = objXmlMod["cost"]?.InnerText ?? "0";
            if (_strCost.StartsWith("Variable("))
            {
                int intMin;
                int intMax = 0;
                string strCost = _strCost.Replace("Variable(", string.Empty).TrimEnd(')');
                if (strCost.Contains("-"))
                {
                    string[] strValues = strCost.Split('-');
                    intMin = Convert.ToInt32(strValues[0]);
                    intMax = Convert.ToInt32(strValues[1]);
                }
                else
                    intMin = Convert.ToInt32(strCost.FastEscape('+'));

                if (intMin != 0 || intMax != 0)
                {
                    string strNuyenFormat = _objCharacter.Options.NuyenFormat;
                    int intDecimalPlaces = strNuyenFormat.IndexOf('.');
                    if (intDecimalPlaces == -1)
                        intDecimalPlaces = 0;
                    else
                        intDecimalPlaces = strNuyenFormat.Length - intDecimalPlaces - 1;
                    frmSelectNumber frmPickNumber = new frmSelectNumber(intDecimalPlaces);
                    if (intMax == 0)
                        intMax = 1000000;
                    frmPickNumber.Minimum = intMin;
                    frmPickNumber.Maximum = intMax;
                    frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost", GlobalOptions.Language).Replace("{0}", DisplayName(GlobalOptions.Language));
                    frmPickNumber.AllowCancel = false;
                    frmPickNumber.ShowDialog();
                    _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                }
            }
            return true;
        }

        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        public string DisplayName(string strLanguage)
        {
            return DisplayNameShort(strLanguage);
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("weaponmountoption");
            objWriter.WriteElementString("id", _sourceID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("slots", _intSlots.ToString());
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Weapon Mount Option from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="objVehicle">Vehicle that the mod is attached to.</param>
        public void Load(XmlNode objNode, Vehicle objVehicle)
        {
            _objCachedMyXmlNode = null;
            Guid.TryParse(objNode["id"].InnerText, out _sourceID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The cost of just the WeaponMountOption itself.
        /// </summary>
        public decimal Cost => Convert.ToDecimal(_strCost, GlobalOptions.InvariantCultureInfo);

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

        /// <summary>
        /// Identifier of the WeaponMountOption in the data files.
        /// </summary>
        public string SourceId => _sourceID.ToString("D");
        #endregion

        #region Complex Properties
        /// <summary>
        /// Display text for the category of the weapon mount.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("vehicles.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
        }

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail(string strLanguage)
        {
            // If the Avail contains "+", return the base string and don't try to calculate anything since we're looking at a child component.

            string strCalculated = string.Empty;
            string strReturn = string.Empty;

            // Just a straight cost, so return the value.
            if (_strAvail.Contains("F") || _strAvail.Contains("R"))
            {
                strCalculated = Convert.ToInt32(_strAvail.Substring(0, _strAvail.Length - 1)).ToString() + _strAvail.Substring(_strAvail.Length - 1, 1);
            }
            else
                strCalculated = Convert.ToInt32(_strAvail).ToString();

            int intAvail = 0;
            string strAvailText = string.Empty;
            if (strCalculated.Contains("F") || strCalculated.Contains("R"))
            {
                strAvailText = strCalculated.Substring(strCalculated.Length - 1);
                intAvail = Convert.ToInt32(strCalculated.Substring(0, strCalculated.Length - 1));
            }
            else
                intAvail = Convert.ToInt32(strCalculated);

            // Translate the Avail string.
            if (strAvailText == "R")
                strAvailText = LanguageManager.GetString("String_AvailRestricted", strLanguage);
            else if (strAvailText == "F")
                strAvailText = LanguageManager.GetString("String_AvailForbidden", strLanguage);
            strReturn = intAvail.ToString() + strAvailText;

            return strReturn;
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
                _objCachedMyXmlNode = XmlManager.Load("vehicles.xml", strLanguage).SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + _sourceID.ToString("D") + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion
    }
}
