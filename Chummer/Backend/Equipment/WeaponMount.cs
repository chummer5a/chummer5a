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
	public class WeaponMount : INamedItemWithGuid
	{
		private Guid _guiID;
		private decimal _decMarkup;
		private string _strAvail = string.Empty;
		private string _strSource = string.Empty;
		private string _strPage = string.Empty;
		private bool _blnIncludeInVehicle;
		private bool _blnInstalled = true;
		private List<Weapon> _weapons = new List<Weapon>();
		private string _strNotes = string.Empty;
		private string _strAltName = string.Empty;
		private string _strAltCategory = string.Empty;
		private string _strAltPage = string.Empty;
		private string _strExtra = string.Empty;
		private string _strWeaponMountCategories = string.Empty;
		private bool _blnDiscountCost;
		private string _strName = string.Empty;
		private string _strCategory = string.Empty;
		private string _strLimit = string.Empty;
		private int _intSlots = 0;
		private string _strCost = string.Empty;

		private readonly Vehicle _vehicle;
	    private readonly Character _character;

		#region Constructor, Create, Save, Load, and Print Methods
		public WeaponMount(Character character, Vehicle vehicle)
		{
			// Create the GUID for the new VehicleMod.
			_guiID = Guid.NewGuid();
		    _character = character;
			_vehicle = vehicle;
        }

        /// Create a Vehicle Modification from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlMod">XmlNode to create the object from.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="objParent">Vehicle that the mod will be attached to.</param>
        /// <param name="decMarkup">Discount or markup that applies to the base cost of the mod.</param>
        public void Create(XmlNode objXmlMod, TreeNode objNode, Vehicle objParent, decimal decMarkup = 0)
        {
            Parent = objParent ?? throw new ArgumentNullException(nameof(objParent));
            if (objXmlMod == null) Utils.BreakIfDebug();
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetStringFieldQuickly("limit", ref _strLimit);
            objXmlMod.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objXmlMod.TryGetStringFieldQuickly("weaponcategories", ref _strWeaponMountCategories);
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlMod.TryGetStringFieldQuickly("notes", ref _strNotes);
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
                        string strNuyenFormat = _character.Options.NuyenFormat;
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

            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
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
			objWriter.WriteStartElement("weaponmount");
			objWriter.WriteElementString("guid", _guiID.ToString());
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
            foreach (Weapon w in _weapons)
            {
                w.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weaponmountoptions");
            foreach (WeaponMountOption w in WeaponMountOptions)
            {
                w.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
			objWriter.WriteEndElement();
			_character.SourceProcess(_strSource);
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
			objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
			objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
			objNode.TryGetStringFieldQuickly("page", ref _strPage);
			objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
			objNode.TryGetStringFieldQuickly("cost", ref _strCost);
			objNode.TryGetDecFieldQuickly("markup", ref _decMarkup);
			objNode.TryGetStringFieldQuickly("source", ref _strSource);
			objNode.TryGetBoolFieldQuickly("included", ref _blnIncludeInVehicle);
			objNode.TryGetBoolFieldQuickly("installed", ref _blnInstalled);
			if (objNode["weapons"] != null)
			{
                foreach (XmlNode n in objNode.SelectNodes("weapons/weapon"))
                {
                    Weapon w = new Weapon(_character);
                    w.Load(n, blnCopy);
                    _weapons.Add(w);
                }
            }
            if (objNode["weaponmountoptions"] != null)
            {
                foreach (XmlNode n in objNode.SelectNodes("weaponmountoptions/weaponmountoption"))
                {
                    WeaponMountOption w = new WeaponMountOption(_character);
                    w.Load(n, _vehicle);
                    WeaponMountOptions.Add(w);
                }
            }
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
			objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
			objNode.TryGetStringFieldQuickly("extra", ref _strExtra);

            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
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
		public void Print(XmlTextWriter objWriter, CultureInfo objCulture)
		{
			objWriter.WriteStartElement("mod");
			objWriter.WriteElementString("name", DisplayNameShort);
			objWriter.WriteElementString("category", DisplayCategory);
			objWriter.WriteElementString("limit", _strLimit);
			objWriter.WriteElementString("slots", _intSlots.ToString());
			objWriter.WriteElementString("avail", TotalAvail);
			objWriter.WriteElementString("cost", TotalCost.ToString());
			objWriter.WriteElementString("owncost", OwnCost.ToString());
			objWriter.WriteElementString("source", _character.Options.LanguageBookShort(_strSource));
			objWriter.WriteElementString("page", Page);
			objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString());
            objWriter.WriteStartElement("weapons");
            foreach (Weapon w in _weapons)
            {
                w.Print(objWriter, objCulture);
            }
            objWriter.WriteEndElement();
			if (_character.Options.PrintNotes)
				objWriter.WriteElementString("notes", _strNotes);
			objWriter.WriteEndElement();
		}
        /// <summary>
        /// Create a weapon mount using names instead of IDs, because user readability is important and untrustworthy. 
        /// </summary>
        /// <param name="n"></param>
        internal void CreateByName(XmlNode n, TreeNode t)
        {
            XmlDocument doc = XmlManager.Load("vehicles.xml");
            TreeNode tree = new TreeNode();
            WeaponMount mount = this;
            XmlNode node = doc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{n["size"].InnerText}\" and category = \"Size\"]");
            mount.Create(node, tree, _vehicle);
            WeaponMountOption option = new WeaponMountOption(_character);
            node = doc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{n["flexibility"].InnerText}\" and category = \"Flexibility\"]");
            option.Create(node["id"].InnerText, mount.WeaponMountOptions);
            option = new WeaponMountOption(_character);
            node = doc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{n["control"].InnerText}\" and category = \"Control\"]");
            option.Create(node["id"].InnerText, mount.WeaponMountOptions);
            option = new WeaponMountOption(_character);
            node = doc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{n["visibility"].InnerText}\" and category = \"Visibility\"]");
            option.Create(node["id"].InnerText, mount.WeaponMountOptions);
            t.Text = DisplayName;
            t.Tag = _guiID.ToString();
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
				return _weapons;
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
                return _blnDiscountCost;
            }
            set
            {
                _blnDiscountCost = value;
            }
        }

		/// <summary>
		/// Vehicle that the Mod is attached to. 
		/// </summary>
		public Vehicle Parent { internal get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<WeaponMountOption> WeaponMountOptions { get; set; } = new List<WeaponMountOption>();
        #endregion

        #region Complex Properties

        /// <summary>
        /// The number of Slots the Mount consumes, including all child items.
        /// </summary>
        public int CalculatedSlots => Slots + WeaponMountOptions.Sum(w => w.Slots);

        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail
        {
            get
            {
                Tuple<int, string> objAvailPair = TotalAvailPair;
                string strAvail = objAvailPair.Item2;
                // Translate the Avail string.
                if (strAvail == "F")
                    strAvail = LanguageManager.GetString("String_AvailForbidden");
                else if (strAvail == "R")
                    strAvail = LanguageManager.GetString("String_AvailRestricted");

                return objAvailPair.Item1.ToString() + strAvail;
            }
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

                    if (strAccAvail.StartsWith('+') || strAccAvail.StartsWith('-'))
                    {
                        strAccAvail += wm.TotalAvail;
                        if (strAccAvail.EndsWith('F'))
                            strAvail = "F";
                        if (strAccAvail.EndsWith('F') || strAccAvail.EndsWith('R'))
                            strAccAvail = strAccAvail.Substring(0, strAccAvail.Length - 1);
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
				return OwnCost + Weapons.Sum(w => w.TotalCost) + WeaponMountOptions.Sum(w => w.Cost);
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
                string strReturn = $"{DisplayNameShort}";
                if (WeaponMountOptions.Count > 0)
                {
                    strReturn += $" ({string.Join(", ", WeaponMountOptions.Select(wm => wm.DisplayName).ToArray())})";
                }
                
                return strReturn;
			}
        }
        public XmlNode MyXmlNode
        {
            get
            {
                return XmlManager.Load("vehicles.xml")?.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + _guiID.ToString() + "\"]");
            }
        }

	    #endregion
    }

    public class WeaponMountOption
    {
        /// <summary>
        /// Category of the weapon mount.
        /// </summary>
        public string Category;

        private readonly Character _objCharacter;
        private string _strAvail;
        private string _strName;
        private Guid _sourceID;
        private string _strCost;
        private string _strCategory;
        private int _intSlots;
        private string _strWeaponMountCategories;
        private string _strAltName;
        private string _strAltPage;
        private string _strAltCategory;

        #region Constructor, Create, Save and Load Methods
        public WeaponMountOption(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a Weapon Mount Option from an XmlNode.
        /// </summary>
        /// <param name="id">String guid of the object.</param>
        /// <param name="list">List to add the object to. Called inside the Create method in case the mount itself is null.</param>
        public void Create(string id, List<WeaponMountOption> list)
        {
            XmlDocument xmlDoc = XmlManager.Load("vehicles.xml");
            XmlNode objXmlMod = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[id = \"{id}\"]");
            if (objXmlMod == null) Utils.BreakIfDebug();
            Guid.TryParse(id, out _sourceID);
            objXmlMod.TryGetStringFieldQuickly("name", ref _strName);
            objXmlMod.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlMod.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objXmlMod.TryGetStringFieldQuickly("weaponcategories", ref _strWeaponMountCategories);
            objXmlMod.TryGetStringFieldQuickly("avail", ref _strAvail);

            // Check for a Variable Cost.
            // ReSharper disable once PossibleNullReferenceException
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
                        frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost").Replace("{0}", DisplayName);
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _strCost = frmPickNumber.SelectedValue.ToString();
                    }
                }
                else
                    _strCost = objXmlMod["cost"].InnerText;
            }
            list.Add(this);
            if (GlobalOptions.Language == "en-us") return;
            XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");
            XmlNode objModNode = objXmlDocument.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + _sourceID.ToString() + "\"]");
            if (objModNode != null)
            {
                objModNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                objModNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
            }

            objModNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
            _strAltCategory = objModNode?.Attributes?["translate"]?.InnerText;
        }

        public string DisplayName
        {
            get
            {
                return _strAltName ?? _strName;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("weaponmountoption");
            objWriter.WriteElementString("id", _sourceID.ToString());
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
            Guid.TryParse(objNode["id"].InnerText, out _sourceID);
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strWeaponMountCategories);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);

            if (GlobalOptions.Language == "en-us") return;
            XmlDocument objXmlDocument = XmlManager.Load("vehicles.xml");
            XmlNode objModNode = objXmlDocument.SelectSingleNode($"/chummer/weaponmounts/weaponmount[id = \"{_sourceID}\"]");
            if (objModNode != null)
            {
                objModNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                objModNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
            }

            objModNode = objXmlDocument.SelectSingleNode($"/chummer/categories/category[. = \"{_strCategory}\"]");
            _strAltCategory = objModNode?.Attributes?["translate"]?.InnerText;
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
        #endregion

        #region Complex Properties
        /// <summary>
        /// Total Availability.
        /// </summary>
        public string TotalAvail
        {
            get
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
                    strAvailText = LanguageManager.GetString("String_AvailRestricted");
                else if (strAvailText == "F")
                    strAvailText = LanguageManager.GetString("String_AvailForbidden");
                strReturn = intAvail.ToString() + strAvailText;

                return strReturn;
            }
        }
        #endregion
    }
}
