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

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Vehicle Modification.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class WeaponMount : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanSell, ICanEquip, IHasSource, ICanSort, IHasStolenProperty
    {
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
        private string _strSourceId = string.Empty;
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
                Log.Warning(new object[] { "Missing id field for xmlnode", objXmlMod });
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
            if (!objXmlMod.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlMod.TryGetStringFieldQuickly("notes", ref _strNotes);
            // Check for a Variable Cost.
            objXmlMod.TryGetStringFieldQuickly("cost", ref _strCost);
            if (!string.IsNullOrEmpty(_strCost))
            {
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
            }
            _decMarkup = decMarkup;

            objXmlMod.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlMod.TryGetStringFieldQuickly("page", ref _strPage);
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
			objWriter.WriteStartElement("weaponmount");
		    objWriter.WriteElementString("sourceid", SourceIDString);
		    objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
			objWriter.WriteElementString("category", _strCategory);
			objWriter.WriteElementString("limit", _strLimit);
			objWriter.WriteElementString("slots", _intSlots.ToString());
			objWriter.WriteElementString("avail", _strAvail);
			objWriter.WriteElementString("cost", _strCost);
			objWriter.WriteElementString("markup", _decMarkup.ToString(GlobalOptions.InvariantCultureInfo));
			objWriter.WriteElementString("extra", _strExtra);
			objWriter.WriteElementString("source", _strSource);
			objWriter.WriteElementString("page", _strPage);
			objWriter.WriteElementString("included", _blnIncludeInVehicle.ToString());
			objWriter.WriteElementString("equuipped", _blnEquipped.ToString());
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
		/// <param name="objVehicle">Vehicle that the mod is attached to.</param>
		/// <param name="blnCopy">Indicates whether a new item will be created as a copy of this one.</param>
		public void Load(XmlNode objNode, Vehicle objVehicle, bool blnCopy = false)
		{
		    if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
		    {
		        _guiID = Guid.NewGuid();
		    }
		    objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (objNode["sourceid"] == null || !objNode.TryGetField("sourceid", Guid.TryParse, out _guiSourceID))
		    {
		        XmlNode node = GetNode(GlobalOptions.Language);
		        node?.TryGetField("id", Guid.TryParse, out _guiSourceID);
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
            if (xmlChildrenNode != null)
			{
                using (XmlNodeList xmlWeaponList = xmlChildrenNode.SelectNodes("weapon"))
                    if (xmlWeaponList != null)
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
            xmlChildrenNode = objNode["weaponmountoptions"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList xmlWeaponMountOptionList = xmlChildrenNode.SelectNodes("weaponmountoption"))
                    if (xmlWeaponMountOptionList != null)
                        foreach (XmlNode xmlWeaponMountOptionNode in xmlWeaponMountOptionList)
                        {
                            WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                            objWeaponMountOption.Load(xmlWeaponMountOptionNode);
                            WeaponMountOptions.Add(objWeaponMountOption);
                        }
            }
            xmlChildrenNode = objNode["mods"];
            if (xmlChildrenNode != null)
            {
                using (XmlNodeList xmlModList = xmlChildrenNode.SelectNodes("mod"))
                    if (xmlModList != null)
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
			objWriter.WriteElementString("slots", Slots.ToString());
			objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
			objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
			objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
			objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
		    objWriter.WriteElementString("page", Page(strLanguageToPrint));
		    objWriter.WriteElementString("location", _strLocation);
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
            XmlNode xmlDataNode = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{xmlNode["size"]?.InnerText}\" and category = \"Size\"]");
            if (xmlDataNode != null)
            {
                objMount.Create(xmlDataNode);

                xmlDataNode = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{xmlNode["flexibility"]?.InnerText}\" and category = \"Flexibility\"]");
                if (xmlDataNode != null)
                {
                    WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                    objWeaponMountOption.Create(xmlDataNode);
                    objMount.WeaponMountOptions.Add(objWeaponMountOption);
                }

                xmlDataNode = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{xmlNode["control"]?.InnerText}\" and category = \"Control\"]");
                if (xmlDataNode != null)
                {
                    WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                    objWeaponMountOption.Create(xmlDataNode);
                    objMount.WeaponMountOptions.Add(objWeaponMountOption);
                }

                xmlDataNode = xmlDoc.SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{xmlNode["visibility"]?.InnerText}\" and category = \"Visibility\"]");
                if (xmlDataNode != null)
                {
                    WeaponMountOption objWeaponMountOption = new WeaponMountOption(_objCharacter);
                    objWeaponMountOption.Create(xmlDataNode);
                    objMount.WeaponMountOptions.Add(objWeaponMountOption);
                }
                _strLocation = xmlNode["location"]?.InnerText ?? string.Empty;
                _strAllowedWeapons = xmlNode["allowedweapons"]?.InnerText ?? string.Empty;
                xmlDataNode = xmlNode["mods"];
                if (xmlDataNode == null) return;
                using (XmlNodeList xmlModList = xmlDataNode.SelectNodes("mod"))
                    if (xmlModList != null)
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
        #endregion

        #region Properties
        /// <summary>
        /// Weapons.
        /// </summary>
        public TaggedObservableCollection<Weapon> Weapons => _lstWeapons;

        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId => _guiID.ToString("D");
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
        public string SourceIDString => _guiSourceID.ToString("D");

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
        public IList<WeaponMountOption> WeaponMountOptions { get; } = new List<WeaponMountOption>();

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
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

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

            // Run through the Accessories and add in their availability.
            foreach (WeaponMountOption objWeaponMountOption in WeaponMountOptions)
            {
                AvailabilityValue objLoopAvailTuple = objWeaponMountOption.TotalAvailTuple();
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

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
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
                return cost + Weapons.Sum(w => w.TotalCost) + WeaponMountOptions.Sum(w => w.Cost) + Mods.Sum(m => m.TotalCost);
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
                    objCost.CheapReplace(strCost, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objCost.CheapReplace(strCost, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                objCost.CheapReplace(strCost, "Vehicle Cost", () => Parent?.OwnCost.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                // If the Body is 0 (Microdrone), treat it as 0.5 for the purposes of determine Modification cost.
                objCost.CheapReplace(strCost, "Body", () => Parent?.Body > 0 ? Parent.Body.ToString() : "0.5");
                objCost.CheapReplace(strCost, "Speed", () => Parent?.Speed.ToString() ?? "0");
                objCost.CheapReplace(strCost, "Acceleration", () => Parent?.Accel.ToString() ?? "0");
                objCost.CheapReplace(strCost, "Handling", () => Parent?.Handling.ToString() ?? "0");

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
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
		{
            StringBuilder strReturn = new StringBuilder(DisplayNameShort(strLanguage));
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
            if (WeaponMountOptions.Count > 0)
            {
                strReturn.Append(strSpaceCharacter + '(');
                bool blnCloseParantheses = false;
                foreach (WeaponMountOption objOption in WeaponMountOptions)
                {
                    if (objOption.Name != "None")
                    {
                        blnCloseParantheses = true;
                        strReturn.Append(objOption.DisplayName(strLanguage));
                        strReturn.Append(',' + strSpaceCharacter);
                    }
                }
                strReturn.Length -= 1 + strSpaceCharacter.Length;
                if (blnCloseParantheses)
                    strReturn.Append(')');
                if (!string.IsNullOrWhiteSpace(Location))
                {
                    strReturn.Append(strSpaceCharacter + '-' + strSpaceCharacter + Location);
                }
            }

            return strReturn.ToString();
        }

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = SourceID == Guid.Empty
                    ? XmlManager.Load("vehicles.xml", strLanguage)
                        .SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{Name}\"]")
                    : XmlManager.Load("vehicles.xml", strLanguage)
                        .SelectSingleNode($"/chummer/weaponmounts/weaponmount[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
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
                Text = DisplayName(GlobalOptions.Language),
                Tag = this,
                ContextMenuStrip = cmsVehicleWeaponMount,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
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
                    return Color.SaddleBrown;
                }
                if (IncludedInVehicle)
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }
        #endregion
        #endregion

        public bool Remove(Character characterObject, bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                if (!characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeaponMount",
                    GlobalOptions.Language)))
                    return false;
            }

            DeleteWeaponMount();
            return Parent.WeaponMounts.Remove(this);
        }

        public void Sell(Character characterObject, decimal percentage)
        {
            // Record the cost of the Armor with the ArmorMod.
            decimal decOriginal = Parent.TotalCost;

            // Create the Expense Log Entry for the sale.
            decimal decAmount = (decOriginal - Parent.TotalCost) * percentage;
            decAmount += DeleteWeaponMount() * percentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(characterObject);
            objExpense.Create(decAmount, LanguageManager.GetString("String_ExpenseSoldArmorMod", GlobalOptions.Language) + ' ' + DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
            characterObject.ExpenseEntries.AddWithSort(objExpense);
            characterObject.Nuyen += decAmount;

            Parent.WeaponMounts.Remove(this);
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }
    }

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
            if (_strCost.StartsWith("Variable("))
            {
                int intMin;
                int intMax = 0;
                string strCost = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                if (strCost.Contains('-'))
                {
                    string[] strValues = strCost.Split('-');
                    intMin = Convert.ToInt32(strValues[0]);
                    intMax = Convert.ToInt32(strValues[1]);
                }
                else
                    intMin = Convert.ToInt32(strCost.FastEscape('+'));

                if (intMin != 0 || intMax != 0)
                {
                    frmSelectNumber frmPickNumber = new frmSelectNumber(_objCharacter.Options.NuyenDecimals);
                    if (intMax == 0)
                        intMax = 1000000;
                    frmPickNumber.Minimum = intMin;
                    frmPickNumber.Maximum = intMax;
                    frmPickNumber.Description = string.Format(LanguageManager.GetString("String_SelectVariableCost", GlobalOptions.Language), DisplayName(GlobalOptions.Language));
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
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalID);
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
        public void Load(XmlNode objNode)
        {
            _objCachedMyXmlNode = null;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if (objNode["sourceid"] == null || !objNode.TryGetField("sourceid", Guid.TryParse, out _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetField("id", Guid.TryParse, out _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            objNode.TryGetInt32FieldQuickly("slots", ref _intSlots);
            objNode.TryGetStringFieldQuickly("weaponmountcategories", ref _strAllowedWeaponCategories);
            objNode.TryGetStringFieldQuickly("allowedweapons", ref _strAllowedWeapons);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
        }
        #endregion

        #region Properties

        public string InternalID => _guiID.ToString("D");

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
        public string SourceIDString => _guiSourceID.ToString("D");

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
                    objCost.CheapReplace(strCost, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objCost.CheapReplace(strCost, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;
            }
        }

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
        public string SourceId => _guiSourceID.ToString("D");

        public int StolenTotalCost { get; set; }

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
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess);
            }

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
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
                _objCachedMyXmlNode = SourceID == Guid.Empty
                    ? XmlManager.Load("vehicles.xml", strLanguage)
                        .SelectSingleNode($"/chummer/weaponmounts/weaponmount[name = \"{Name}\"]")
                    : XmlManager.Load("vehicles.xml", strLanguage)
                        .SelectSingleNode($"/chummer/weaponmounts/weaponmount[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion
    }
}
