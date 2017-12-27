using System;
using System.Collections.Generic;
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
		private IList<Weapon> _weapons = new List<Weapon>();
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
        private IList<VehicleMod> _mods = new List<VehicleMod>();

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
            objXmlMod.TryGetStringFieldQuickly("id", ref _strSourceId);
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

            objNode.Text = DisplayName(GlobalOptions.Language);
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
            objWriter.WriteElementString("sourceid", _strSourceId.ToString());
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
            foreach (var w in _weapons)
            {
                w.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("weaponmountoptions");
            foreach (var w in WeaponMountOptions)
            {
                w.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteStartElement("mods");
		    foreach (var m in Mods)
		    {
		        m.Save(objWriter);
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
            if (!objNode.TryGetStringFieldQuickly("sourceid", ref _strSourceId))
            {
                _strSourceId = XmlManager.Load("vehicles.xml")?.SelectSingleNode("/chummer/weaponmounts/weaponmount[name = \"" + _strName + "\"]")?["id"]?.InnerText ?? Guid.NewGuid().ToString();
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
			if (objNode["weapons"] != null)
			{
                foreach (XmlNode n in objNode.SelectNodes("weapons/weapon"))
                {
                    Weapon w = new Weapon(_character);
                    w.Load(n, blnCopy);
                    _weapons.Add(w);
                    w.ParentMount = this;
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
		    if (objNode["mods"] != null)
		    {
		        foreach (XmlNode n in objNode.SelectNodes("mods/mod"))
		        {
		            var m = new VehicleMod(_character);
		            m.Load(n);
		            Mods.Add(m);
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
			objWriter.WriteElementString("limit", _strLimit);
			objWriter.WriteElementString("slots", _intSlots.ToString());
			objWriter.WriteElementString("avail", TotalAvail(strLanguageToPrint));
			objWriter.WriteElementString("cost", TotalCost.ToString(_character.Options.NuyenFormat, objCulture));
			objWriter.WriteElementString("owncost", OwnCost.ToString(_character.Options.NuyenFormat, objCulture));
			objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
			objWriter.WriteElementString("page", Page(strLanguageToPrint));
			objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString());
            objWriter.WriteStartElement("weapons");
		    foreach (var w in _weapons)
		    {
		        w.Print(objWriter, objCulture, strLanguageToPrint);
            }
		    foreach (var m in _mods)
		    {
		        m.Print(objWriter, objCulture, strLanguageToPrint);
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
            t.Text = DisplayName(GlobalOptions.Language);
            t.Tag = _guiID.ToString();
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
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("vehicles.xml", strLanguage)?.SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]")?.Attributes?["translate"]?.InnerText ?? Category;
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

                    if (strAccAvail.StartsWith('+') || strAccAvail.StartsWith('-'))
                    {
                        strAccAvail += wm.TotalAvail(GlobalOptions.DefaultLanguage);
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
            get => _mods;
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
        /// Create a Weapon Mount Option from an XmlNode.
        /// </summary>
        /// <param name="id">String guid of the object.</param>
        /// <param name="list">List to add the object to. Called inside the Create method in case the mount itself is null.</param>
        public void Create(string id, ICollection<WeaponMountOption> list)
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
                        frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost", GlobalOptions.Language).Replace("{0}", DisplayName(GlobalOptions.Language));
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                    }
                }
                else
                    _strCost = objXmlMod["cost"].InnerText;
            }
            list.Add(this);
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
        #endregion

        #region Complex Properties
        /// <summary>
        /// Display text for the category of the weapon mount.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("vehicles.xml", strLanguage)?.SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]")?.Attributes?["translate"]?.InnerText ?? Category;
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
                _objCachedMyXmlNode = XmlManager.Load("vehicles.xml", strLanguage)?.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = \"" + _sourceID.ToString() + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion
    }
}
