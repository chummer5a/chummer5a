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
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Weapon Accessory.
    /// </summary>
    public class WeaponAccessory : IHasInternalId, IHasName, IHasXmlNode
    {
        private Guid _guiID = Guid.Empty;
        private readonly Character _objCharacter;
        private XmlNode _nodAllowGear;
        private List<Gear> _lstGear = new List<Gear>();
        private Weapon _objParent;
        private string _strName = string.Empty;
        private string _strMount = string.Empty;
        private string _strExtraMount = string.Empty;
        private string _strRC = string.Empty;
        private string _strDamage = string.Empty;
        private string _strDamageType = string.Empty;
        private string _strDamageReplace = string.Empty;
        private string _strFireMode = string.Empty;
        private string _strFireModeReplace = string.Empty;
        private string _strAPReplace = string.Empty;
        private string _strAP = string.Empty;
        private string _strConceal = string.Empty;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private string _strDicePool = string.Empty;
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
        private string _strExtra = string.Empty;
        private int _intRangeBonus = 0;
        private int _intSuppressive = 0;
        private int _intFullBurst = 0;
        private string _strAddMode = string.Empty;
        private string _strAmmoReplace = string.Empty;
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
        public void Create(XmlNode objXmlAccessory, Tuple<string, string> strMount, int intRating, bool blnSkipCost = false, bool blnCreateChildren = true, bool blnCreateImprovements = true)
        {
            if (objXmlAccessory.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            _strMount = strMount.Item1;
            _strExtraMount = strMount.Item2;
            _intRating = intRating;
            objXmlAccessory.TryGetStringFieldQuickly("avail", ref _strAvail);
            // Check for a Variable Cost.
            if (blnSkipCost)
                _strCost = "0";
            else
            {
                _strCost = objXmlAccessory["cost"]?.InnerText ?? "0";
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

            objXmlAccessory.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlAccessory.TryGetStringFieldQuickly("page", ref _strPage);
            _nodAllowGear = objXmlAccessory["allowgear"];
            if (!objXmlAccessory.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlAccessory.TryGetStringFieldQuickly("notes", ref _strNotes);
            objXmlAccessory.TryGetStringFieldQuickly("rc", ref _strRC);
            objXmlAccessory.TryGetBoolFieldQuickly("rcdeployable", ref _blnDeployable);
            objXmlAccessory.TryGetInt32FieldQuickly("rcgroup", ref _intRCGroup);
            objXmlAccessory.TryGetStringFieldQuickly("conceal", ref _strConceal);
            objXmlAccessory.TryGetInt32FieldQuickly("ammoslots", ref _intAmmoSlots);
            objXmlAccessory.TryGetStringFieldQuickly("ammoreplace", ref _strAmmoReplace);
            objXmlAccessory.TryGetInt32FieldQuickly("accuracy", ref _intAccuracy);
            objXmlAccessory.TryGetStringFieldQuickly("dicepool", ref _strDicePool);
            objXmlAccessory.TryGetStringFieldQuickly("damagetype", ref _strDamageType);
            objXmlAccessory.TryGetStringFieldQuickly("damage", ref _strDamage);
            objXmlAccessory.TryGetStringFieldQuickly("damagereplace", ref _strDamageReplace);
            objXmlAccessory.TryGetStringFieldQuickly("firemode", ref _strFireMode);
            objXmlAccessory.TryGetStringFieldQuickly("firemodereplace", ref _strFireModeReplace);
            objXmlAccessory.TryGetStringFieldQuickly("ap", ref _strAP);
            objXmlAccessory.TryGetStringFieldQuickly("apreplace", ref _strAPReplace);
            objXmlAccessory.TryGetStringFieldQuickly("addmode", ref _strAddMode);
            objXmlAccessory.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objXmlAccessory.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);
            objXmlAccessory.TryGetInt32FieldQuickly("rangebonus", ref _intRangeBonus);
            objXmlAccessory.TryGetStringFieldQuickly("extra", ref _strExtra);
            objXmlAccessory.TryGetInt32FieldQuickly("ammobonus", ref _intAmmoBonus);
            objXmlAccessory.TryGetInt32FieldQuickly("accessorycostmultiplier", ref _intAccessoryCostMultiplier);

            // Add any Gear that comes with the Weapon Accessory.
            if (objXmlAccessory["gears"] != null && blnCreateChildren)
            {
                XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");
                foreach (XmlNode objXmlAccessoryGear in objXmlAccessory.SelectNodes("gears/usegear"))
                {
                    XmlNode objXmlAccessoryGearName = objXmlAccessoryGear["name"];
                    XmlAttributeCollection objXmlAccessoryGearNameAttributes = objXmlAccessoryGearName.Attributes;
                    int intGearRating = 0;
                    decimal decGearQty = 1;
                    string strChildForceSource = objXmlAccessoryGear["source"]?.InnerText ?? string.Empty;
                    string strChildForcePage = objXmlAccessoryGear["page"]?.InnerText ?? string.Empty;
                    string strChildForceValue = objXmlAccessoryGearNameAttributes?["select"]?.InnerText ?? string.Empty;
                    bool blnStartCollapsed = objXmlAccessoryGearNameAttributes?["startcollapsed"]?.InnerText == bool.TrueString;
                    bool blnChildCreateChildren = objXmlAccessoryGearNameAttributes?["createchildren"]?.InnerText != bool.FalseString;
                    bool blnAddChildImprovements = objXmlAccessoryGearNameAttributes?["addimprovements"]?.InnerText == bool.FalseString ? false : blnCreateImprovements;
                    if (objXmlAccessoryGear["rating"] != null)
                        intGearRating = Convert.ToInt32(objXmlAccessoryGear["rating"].InnerText);
                    if (objXmlAccessoryGearNameAttributes?["qty"] != null)
                        decGearQty = Convert.ToDecimal(objXmlAccessoryGearNameAttributes["qty"].InnerText, GlobalOptions.InvariantCultureInfo);

                    XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlAccessoryGearName.InnerText + "\" and category = \"" + objXmlAccessoryGear["category"].InnerText + "\"]");
                    Gear objGear = new Gear(_objCharacter);
                    
                    List<Weapon> lstWeapons = new List<Weapon>();

                    objGear.Create(objXmlGear, intGearRating, lstWeapons, strChildForceValue, blnAddChildImprovements, blnChildCreateChildren);

                    objGear.Quantity = decGearQty;
                    objGear.Cost = "0";
                    objGear.MinRating = intGearRating;
                    objGear.MaxRating = intGearRating;
                    objGear.ParentID = InternalId;
                    if (!string.IsNullOrEmpty(strChildForceSource))
                        objGear.Source = strChildForceSource;
                    if (!string.IsNullOrEmpty(strChildForcePage))
                        objGear.Page = strChildForcePage;
                    _lstGear.Add(objGear);

                    // Change the Capacity of the child if necessary.
                    if (objXmlAccessoryGear["capacity"] != null)
                        objGear.Capacity = '[' + objXmlAccessoryGear["capacity"].InnerText + ']';
                }
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("accessory");
            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("mount", _strMount);
            objWriter.WriteElementString("extramount", _strExtraMount);
            objWriter.WriteElementString("rc", _strRC);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("rcgroup", _intRCGroup.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("rcdeployable", _blnDeployable.ToString());
            objWriter.WriteElementString("conceal", _strConceal);
            if (!string.IsNullOrEmpty(_strDicePool))
                objWriter.WriteElementString("dicepool", _strDicePool);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("included", _blnIncludedInWeapon.ToString());
            objWriter.WriteElementString("installed", _blnInstalled.ToString());
            if (_nodAllowGear != null)
                objWriter.WriteRaw(_nodAllowGear.OuterXml);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("accuracy", _intAccuracy.ToString(GlobalOptions.InvariantCultureInfo));
            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    objGear.Save(objWriter);
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
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString());
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
            if (blnCopy || !Guid.TryParse(objNode["guid"].InnerText, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objNode.TryGetStringFieldQuickly("mount", ref _strMount);
            objNode.TryGetStringFieldQuickly("extramount", ref _strExtraMount);
            objNode.TryGetStringFieldQuickly("rc", ref _strRC);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetInt32FieldQuickly("rcgroup", ref _intRCGroup);
            objNode.TryGetInt32FieldQuickly("accuracy", ref _intAccuracy);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("conceal", ref _strConceal);
            objNode.TryGetBoolFieldQuickly("rcdeployable", ref _blnDeployable);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludedInWeapon);
            objNode.TryGetBoolFieldQuickly("installed", ref _blnInstalled);
            _nodAllowGear = objNode["allowgear"];
            objNode.TryGetStringFieldQuickly("source", ref _strSource);

            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("dicepool", ref _strDicePool);

            objNode.TryGetInt32FieldQuickly("ammoslots", ref _intAmmoSlots);

            if (objNode.InnerXml.Contains("<gears>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("gears/gear");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Gear objGear = new Gear(_objCharacter);
                    objGear.Load(nodChild, blnCopy);
                    _lstGear.Add(objGear);
                }
            }
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);

            objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            objNode.TryGetStringFieldQuickly("damagetype", ref _strDamageType);
            objNode.TryGetStringFieldQuickly("damagereplace", ref _strDamageReplace);
            objNode.TryGetStringFieldQuickly("firemode", ref _strFireMode);
            objNode.TryGetStringFieldQuickly("firemodereplace", ref _strFireModeReplace);
            objNode.TryGetStringFieldQuickly("ap", ref _strAP);
            objNode.TryGetStringFieldQuickly("apreplace", ref _strAPReplace);
            objNode.TryGetInt32FieldQuickly("accessorycostmultiplier", ref _intAccessoryCostMultiplier);
            objNode.TryGetStringFieldQuickly("addmode", ref _strAddMode);
            objNode.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objNode.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);
            objNode.TryGetInt32FieldQuickly("rangebonus", ref _intRangeBonus);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetInt32FieldQuickly("ammobonus", ref _intAmmoBonus);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("accessory");
            objWriter.WriteElementString("name", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("mount", Mount);
            objWriter.WriteElementString("extramount", ExtraMount);
            objWriter.WriteElementString("rc", RC);
            objWriter.WriteElementString("conceal", TotalConcealability.ToString());
            objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("included", IncludedInWeapon.ToString());
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", Page(strLanguageToPrint));
            objWriter.WriteElementString("accuracy", Accuracy.ToString(objCulture));
            if (Gear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in Gear)
                {
                    objGear.Print(objWriter, objCulture, strLanguageToPrint);
                }
                objWriter.WriteEndElement();
            }
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
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
                return _guiID.ToString("D");
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
                if (_strName != value)
                {
                    _objCachedMyXmlNode = null;
                    _strName = value;
                }
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
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (!string.IsNullOrEmpty(Extra))
            {
                strReturn += " (" + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';
            }

            return strReturn;
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
        /// Additional mount slot used (if any).
        /// </summary>
        public string ExtraMount
        {
            get
            {
                return _strExtraMount;
            }
            set
            {
                _strExtraMount = value;
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
        public string Concealability
        {
            get
            {
                return _strConceal;
            }
            set
            {
                _strConceal = value;
            }
        }

        public int TotalConcealability
        {
            get
            {
                int intReturn = 0;

                string strConceal = Concealability;
                if (strConceal.Contains("Rating"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    strConceal = strConceal.Replace("Rating", Rating.ToString());
                    try
                    {
                        intReturn = Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(strConceal)));
                    }
                    catch (XPathException) { }
                    catch (OverflowException) { }
                    catch (InvalidCastException) { }
                }
                else if (!string.IsNullOrEmpty(strConceal))
                {
                    int.TryParse(strConceal, out intReturn);
                }
                return intReturn;
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
        public string Page(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
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
                if (strAvail.StartsWith("FixedValues("))
                {
                    string[] strValues = strAvail.TrimStart("FixedValues(", true).TrimEnd(')').Split(',');
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                strAvail = strAvail.TrimStart('+');

                strAvail = strAvail.Replace("Rating", Rating.ToString());

                try
                {
                    intAvail = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvail));
                }
                catch (XPathException)
                {
                }
            }

            if (blnCheckChildren)
            {
                // Run through gear children and increase the Avail by any Mod whose Avail starts with "+" or "-".
                foreach (Gear objChild in Gear)
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

            // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail);
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
        public IList<Gear> Gear
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
                return _blnDiscountCost && _objCharacter.BlackMarketDiscount;
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
        public decimal TotalCost
        {
            get
            {
                decimal decReturn = OwnCost;

                // Add in the cost of any Gear the Weapon Accessory has attached to it.
                foreach (Gear objGear in Gear)
                    decReturn += objGear.TotalCost;

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Weapon Accessory itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                string strCost = Cost
                    .CheapReplace("Weapon Cost", () => _objParent.OwnCost.ToString(GlobalOptions.InvariantCultureInfo))
                    .Replace("Rating", Rating.ToString());
                decimal decReturn = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCost).ToString(), GlobalOptions.InvariantCultureInfo) * _objParent.CostMultiplier;

                if (DiscountCost)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// Dice Pool modifier.
        /// </summary>
        public int DicePool
        {
            get
            {
                if (!string.IsNullOrEmpty(DicePoolString))
                    return Convert.ToInt32(DicePoolString);

                return 0;
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
                _objCachedMyXmlNode = XmlManager.Load("weapons.xml", strLanguage).SelectSingleNode("/chummer/accessories/accessory[name = \"" + Name + "\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Methods
        public TreeNode CreateTreeNode(ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear)
        {
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = DisplayName(GlobalOptions.Language),
                Tag = InternalId,
                ContextMenuStrip = cmsWeaponAccessory
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                objNode.ForeColor = Color.SaddleBrown;
            }
            else if (IncludedInWeapon)
            {
                objNode.ForeColor = SystemColors.GrayText;
            }
            objNode.ToolTipText = Notes.WordWrap(100);
            foreach (Gear objGear in Gear)
            {
                objNode.Nodes.Add(objGear.CreateTreeNode(cmsWeaponAccessoryGear));
                objNode.Expand();
            }

            return objNode;
        }
        #endregion
    }
}
