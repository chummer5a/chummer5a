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
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Skills;
using System.Drawing;
using Chummer.Backend.Attributes;
using System.Text;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using NLog;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A Weapon.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", null)]
    [DebuggerDisplay("{DisplayName(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage)}")]
    public class Weapon : IHasChildren<Weapon>, IHasName, IHasInternalId, IHasXmlNode, IHasMatrixAttributes, IHasNotes, ICanSell, IHasCustomName, IHasLocation, ICanEquip, IHasSource, ICanSort, IHasWirelessBonus, IHasStolenProperty, ICanPaste, IHasRating
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private Guid _guiSourceID = Guid.Empty;
        private Guid _guiID;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strType = string.Empty;
        private int _intReach;
        private int _intAmmoSlots = 1;
        private string _strDamage = string.Empty;
        private string _strAP = "0";
        private string _strMode = string.Empty;
        private string _strRC = string.Empty;
        private string _strAmmo = string.Empty;
        private string _strAmmoCategory = string.Empty;
        private string _strAmmoName = string.Empty;
        private int _intConceal;
        private List<Clip> _lstAmmo = new List<Clip>();
        //private int _intAmmoRemaining = 0;
        //private int _intAmmoRemaining2 = 0;
        //private int _intAmmoRemaining3 = 0;
        //private int _intAmmoRemaining4 = 0;
        //private Guid _guiAmmoLoaded = Guid.Empty;
        //private Guid _guiAmmoLoaded2 = Guid.Empty;
        //private Guid _guiAmmoLoaded3 = Guid.Empty;
        //private Guid _guiAmmoLoaded4 = Guid.Empty;
        private int _intActiveAmmoSlot = 1;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strRange = string.Empty;
        private string _strAlternateRange = string.Empty;
        private decimal _decRangeMultiplier = 1;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strWeaponName = string.Empty;
        private int _intSingleShot = 1;
        private int _intShortBurst = 3;
        private int _intLongBurst = 6;
        private int _intFullBurst = 10;
        private int _intSuppressive = 20;
        private readonly TaggedObservableCollection<WeaponAccessory> _lstAccessories = new TaggedObservableCollection<WeaponAccessory>();
        private readonly TaggedObservableCollection<Weapon> _lstUnderbarrel = new TaggedObservableCollection<Weapon>();
        private Vehicle _objMountedVehicle;
        private WeaponMount _objWeaponMount;
        private string _strNotes = string.Empty;
        private string _strUseSkill = string.Empty;
        private string _strUseSkillSpec = string.Empty;
        private Location _objLocation;
        private string _strSpec = string.Empty;
        private string _strSpec2 = string.Empty;
        private bool _blnIncludedInWeapon;
        private bool _blnEquipped = true;
        private bool _blnDiscountCost;
        private bool _blnRequireAmmo = true;
        private string _strAccuracy = string.Empty;
        private string _strRCTip = string.Empty;
        private string _strWeaponSlots = string.Empty;
        private string _strDoubledCostWeaponSlots = string.Empty;
        private bool _blnCyberware;
        private string _strParentID = string.Empty;
        private bool _blnAllowAccessory = true;
        private Weapon _objParent;
        private string _strSizeCategory;
        private int _intRating;
        private string _strMinRating;
        private string _strMaxRating;
        private string _strRatingLabel = "String_Rating";

        private XmlNode _nodWirelessBonus;
        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;
        private FiringMode _eFiringMode;

        private string _strDeviceRating = string.Empty;
        private string _strAttack = string.Empty;
        private string _strSleaze = string.Empty;
        private string _strDataProcessing = string.Empty;
        private string _strFirewall = string.Empty;
        private string _strAttributeArray = string.Empty;
        private string _strModAttack = string.Empty;
        private string _strModSleaze = string.Empty;
        private string _strModDataProcessing = string.Empty;
        private string _strModFirewall = string.Empty;
        private string _strModAttributeArray = string.Empty;
        private string _strProgramLimit = string.Empty;
        private string _strOverclocked = "None";
        private bool _blnCanSwapAttributes;
        private bool _blnWirelessOn;
        private int _intMatrixCMFilled;
        private int _intSortOrder;
        private bool _blnStolen;
        private readonly Character _objCharacter;
        private string _strMount;
        private string _strExtraMount;

        private bool _blnAllowSingleShot  = true;
        private bool _blnAllowShortBurst  = true;
        private bool _blnAllowLongBurst   = true;
        private bool _blnAllowFullBurst   = true;
        private bool _blnAllowSuppressive = true;

        #region Constructor, Create, Save, Load, and Print Methods
        public Weapon(Character objCharacter)
        {
            // Create the GUID for the new Weapon.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _lstUnderbarrel.CollectionChanged += ChildrenOnCollectionChanged;
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Weapon objNewItem in e.NewItems)
                        objNewItem.Parent = this;
                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Weapon objOldItem in e.OldItems)
                        objOldItem.Parent = null;
                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Weapon objOldItem in e.OldItems)
                        objOldItem.Parent = null;
                    foreach (Weapon objNewItem in e.NewItems)
                        objNewItem.Parent = this;
                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.RefreshMatrixAttributeArray();
                    break;
            }
        }

        /// Create a Weapon from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlWeapon">XmlNode to create the object from.</param>
        /// <param name="lstWeapons">List of child Weapons to generate.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        /// <param name="blnCreateImprovements">Whether or not bonuses should be created.</param>
        /// <param name="blnSkipCost">Whether or not forms asking to determine variable costs should be displayed.</param>
        /// <param name="intRating">Rating of the weapon</param>
        public void Create(XmlNode objXmlWeapon, IList<Weapon> lstWeapons, bool blnCreateChildren = true, bool blnCreateImprovements = true, bool blnSkipCost = false, int intRating = 0)
        {
            if (!objXmlWeapon.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for weapon xmlnode", objXmlWeapon });
                Utils.BreakIfDebug();
            }
            else
                _objCachedMyXmlNode = null;
            objXmlWeapon.TryGetStringFieldQuickly("name", ref _strName);
            objXmlWeapon.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlWeapon.TryGetStringFieldQuickly("type", ref _strType);
            objXmlWeapon.TryGetInt32FieldQuickly("reach", ref _intReach);
            objXmlWeapon.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
            objXmlWeapon.TryGetStringFieldQuickly("damage", ref _strDamage);
            objXmlWeapon.TryGetStringFieldQuickly("ap", ref _strAP);
            objXmlWeapon.TryGetStringFieldQuickly("mode", ref _strMode);
            objXmlWeapon.TryGetStringFieldQuickly("ammo", ref _strAmmo);
            objXmlWeapon.TryGetStringFieldQuickly("mount", ref _strMount);
            objXmlWeapon.TryGetStringFieldQuickly("extramount", ref _strExtraMount);
            objXmlWeapon.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objXmlWeapon.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            if (_strMaxRating == "0")
                _strMaxRating = string.Empty;
            objXmlWeapon.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            if (!objXmlWeapon.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlWeapon.TryGetStringFieldQuickly("notes", ref _strNotes);

            if (string.IsNullOrEmpty(Notes))
            {
                string strEnglishNameOnPage = Name;
                string strNameOnPage = string.Empty;
                // make sure we have something and not just an empty tag
                if (objXmlWeapon.TryGetStringFieldQuickly("nameonpage", ref strNameOnPage) &&
                    !string.IsNullOrEmpty(strNameOnPage))
                    strEnglishNameOnPage = strNameOnPage;

                string strGearNotes = CommonFunctions.GetTextFromPDF(Source + ' ' + Page, strEnglishNameOnPage, _objCharacter);

                if (string.IsNullOrEmpty(strGearNotes) && GlobalOptions.Language != GlobalOptions.DefaultLanguage)
                {
                    string strTranslatedNameOnPage = CurrentDisplayName;

                    // don't check again it is not translated
                    if (strTranslatedNameOnPage != _strName)
                    {
                        // if we found <altnameonpage>, and is not empty and not the same as english we must use that instead
                        if (objXmlWeapon.TryGetStringFieldQuickly("altnameonpage", ref strNameOnPage)
                            && !string.IsNullOrEmpty(strNameOnPage) && strNameOnPage != strEnglishNameOnPage)
                            strTranslatedNameOnPage = strNameOnPage;

                        Notes = CommonFunctions.GetTextFromPDF(Source + ' ' + DisplayPage(GlobalOptions.Language),
                            strTranslatedNameOnPage, _objCharacter);
                    }
                }
                else
                    Notes = strGearNotes;
            }

            _intRating = Math.Max(Math.Min(intRating, MaxRatingValue), MinRatingValue);
            if (objXmlWeapon["accessorymounts"] != null)
            {
                XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("accessorymounts/mount");
                if (objXmlMountList?.Count > 0)
                {
                    StringBuilder strMounts = new StringBuilder();
                    foreach (XmlNode objXmlMount in objXmlMountList)
                    {
                        strMounts.Append(objXmlMount.InnerText);
                        strMounts.Append('/');
                    }
                    if (strMounts.Length > 0)
                        strMounts.Length -= 1;
                    _strWeaponSlots = strMounts.ToString();
                }
            }
            if (objXmlWeapon["doubledcostaccessorymounts"] != null)
            {
                XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("doubledcostaccessorymounts/mount");
                if (objXmlMountList?.Count > 0)
                {
                    StringBuilder strMounts = new StringBuilder();
                    foreach (XmlNode objXmlMount in objXmlMountList)
                    {
                        strMounts.Append(objXmlMount.InnerText);
                        strMounts.Append('/');
                    }
                    if (strMounts.Length > 0)
                        strMounts.Length -= 1;
                    _strDoubledCostWeaponSlots = strMounts.ToString();
                }
            }
            if (!objXmlWeapon.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlWeapon.TryGetStringFieldQuickly("notes", ref _strNotes);
            _nodWirelessBonus = objXmlWeapon["wirelessbonus"];
            objXmlWeapon.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn);
            objXmlWeapon.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            objXmlWeapon.TryGetStringFieldQuickly("ammoname", ref _strAmmoName);
            if (!objXmlWeapon.TryGetInt32FieldQuickly("ammoslots", ref _intAmmoSlots))
                _intAmmoSlots = 1;
            objXmlWeapon.TryGetStringFieldQuickly("rc", ref _strRC);
            objXmlWeapon.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objXmlWeapon.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlWeapon.TryGetStringFieldQuickly("cost", ref _strCost);
            objXmlWeapon.TryGetBoolFieldQuickly("stolen", ref _blnStolen);

            // Check for a Variable Cost.
            if (!blnSkipCost && _strCost.StartsWith("Variable(", StringComparison.Ordinal))
            {
                string strFirstHalf = _strCost.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                string strSecondHalf = string.Empty;
                int intHyphenIndex = strFirstHalf.IndexOf('-');
                if (intHyphenIndex != -1)
                {
                    if (intHyphenIndex + 1 < strFirstHalf.Length)
                        strSecondHalf = strFirstHalf.Substring(intHyphenIndex + 1);
                    strFirstHalf = strFirstHalf.Substring(0, intHyphenIndex);
                }

                if (blnCreateImprovements)
                {
                    decimal decMin;
                    decimal decMax = decimal.MaxValue;
                    if (intHyphenIndex != -1)
                    {
                        decMin = Convert.ToDecimal(strFirstHalf, GlobalOptions.InvariantCultureInfo);
                        decMax = Convert.ToDecimal(strSecondHalf, GlobalOptions.InvariantCultureInfo);
                    }
                    else
                        decMin = Convert.ToDecimal(strFirstHalf.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                    if (decMin != decimal.MinValue || decMax != decimal.MaxValue)
                    {
                        if (decMax > 1000000)
                            decMax = 1000000;
                        using (frmSelectNumber frmPickNumber = new frmSelectNumber(_objCharacter.Options.NuyenDecimals)
                        {
                            Minimum = decMin,
                            Maximum = decMax,
                            Description = string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("String_SelectVariableCost"), DisplayNameShort(GlobalOptions.Language)),
                            AllowCancel = false
                        })
                        {
                            frmPickNumber.ShowDialog();
                            _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                        }
                    }
                    else
                        _strCost = strFirstHalf;
                }
                else
                    _strCost = strFirstHalf;
            }
            objXmlWeapon.TryGetStringFieldQuickly("sizecategory", ref _strSizeCategory);
            objXmlWeapon.TryGetBoolFieldQuickly("cyberware", ref _blnCyberware);
            objXmlWeapon.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlWeapon.TryGetStringFieldQuickly("page", ref _strPage);

            XmlDocument objXmlDocument = _objCharacter.LoadData("weapons.xml");

            // Populate the Range if it differs from the Weapon's Category.
            XmlNode objRangeNode = objXmlWeapon["range"];
            if (objRangeNode != null)
            {
                _strRange = objRangeNode.InnerText;
                string strMultiply = objRangeNode.Attributes?["multiply"]?.InnerText;
                if (!string.IsNullOrEmpty(strMultiply))
                    _decRangeMultiplier = Convert.ToDecimal(strMultiply, GlobalOptions.InvariantCultureInfo);
            }
            objXmlWeapon.TryGetStringFieldQuickly("alternaterange", ref _strAlternateRange);

            objXmlWeapon.TryGetInt32FieldQuickly("singleshot",      ref _intSingleShot);
            objXmlWeapon.TryGetInt32FieldQuickly("shortburst",      ref _intShortBurst);
            objXmlWeapon.TryGetInt32FieldQuickly("longburst",       ref _intLongBurst);
            objXmlWeapon.TryGetInt32FieldQuickly("fullburst",       ref _intFullBurst);
            objXmlWeapon.TryGetInt32FieldQuickly("suppressive",     ref _intSuppressive);
            objXmlWeapon.TryGetBoolFieldQuickly("allowfullburst",   ref _blnAllowFullBurst);
            objXmlWeapon.TryGetBoolFieldQuickly("allowlongburst",   ref _blnAllowLongBurst);
            objXmlWeapon.TryGetBoolFieldQuickly("allowshortburst",  ref _blnAllowShortBurst);
            objXmlWeapon.TryGetBoolFieldQuickly("allowsingleshot",  ref _blnAllowSingleShot);
            objXmlWeapon.TryGetBoolFieldQuickly("allowsuppressive", ref _blnAllowSuppressive);

            objXmlWeapon.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
            objXmlWeapon.TryGetStringFieldQuickly("useskillspec", ref _strUseSkillSpec);
            objXmlWeapon.TryGetBoolFieldQuickly("requireammo", ref _blnRequireAmmo);
            objXmlWeapon.TryGetStringFieldQuickly("spec", ref _strSpec);
            objXmlWeapon.TryGetStringFieldQuickly("spec2", ref _strSpec2);
            objXmlWeapon.TryGetBoolFieldQuickly("allowaccessory", ref _blnAllowAccessory);

            // If the Weapon comes with an Underbarrel Weapon, add it.
            if (blnCreateChildren)
            {
                XmlNodeList xmlUnderbarrelsList = objXmlWeapon["underbarrels"]?.ChildNodes;
                if (xmlUnderbarrelsList?.Count > 0)
                {
                    foreach (XmlNode objXmlUnderbarrel in xmlUnderbarrelsList)
                    {
                        Weapon objUnderbarrelWeapon = new Weapon(_objCharacter);
                        XmlNode objXmlWeaponNode = objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlUnderbarrel.InnerText + "\"]");
                        objUnderbarrelWeapon.Create(objXmlWeaponNode, lstWeapons, true, blnCreateImprovements, blnSkipCost);
                        if (!AllowAccessory)
                            objUnderbarrelWeapon.AllowAccessory = false;
                        objUnderbarrelWeapon.ParentID = InternalId;
                        objUnderbarrelWeapon.Cost = "0";
                        objUnderbarrelWeapon.IncludedInWeapon = true;
                        _lstUnderbarrel.Add(objUnderbarrelWeapon);
                    }
                }
            }

            //#1544 Ammunition not loading or available.
            if (_strUseSkill == "Throwing Weapons"
                && _strAmmo != "1")
            {
                _strAmmo = "1";
            }

            objXmlWeapon.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objXmlWeapon.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlWeapon.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlWeapon.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlWeapon.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlWeapon.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            }
            else
            {
                _blnCanSwapAttributes = true;
                string[] strArray = _strAttributeArray.Split(',');
                _strAttack = strArray[0];
                _strSleaze = strArray[1];
                _strDataProcessing = strArray[2];
                _strFirewall = strArray[3];
            }
            objXmlWeapon.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlWeapon.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlWeapon.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlWeapon.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlWeapon.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlWeapon.TryGetStringFieldQuickly("programs", ref _strProgramLimit);

            // If there are any Accessories that come with the Weapon, add them.
            if (blnCreateChildren)
            {
                XmlNodeList objXmlAccessoryList = objXmlWeapon.SelectNodes("accessories/accessory");
                if (objXmlAccessoryList?.Count > 0)
                {
                    foreach (XmlNode objXmlWeaponAccessory in objXmlAccessoryList)
                    {
                        XmlNode objXmlAccessory = objXmlDocument.SelectSingleNode("/chummer/accessories/accessory[name = \"" + objXmlWeaponAccessory["name"].InnerText + "\"]");
                        WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
                        int intAccessoryRating = 0;
                        if (objXmlWeaponAccessory["rating"] != null)
                        {
                            intAccessoryRating = Convert.ToInt32(objXmlWeaponAccessory["rating"].InnerText, GlobalOptions.InvariantCultureInfo);
                        }

                        if (objXmlWeaponAccessory.InnerXml.Contains("mount"))
                        {
                            objAccessory.Create(objXmlAccessory,
                                objXmlWeaponAccessory.InnerXml.Contains("<extramount>")
                                    ? new Tuple<string, string>(objXmlAccessory["mount"].InnerText, objXmlAccessory["extramount"].InnerText)
                                    : new Tuple<string, string>(objXmlAccessory["mount"].InnerText, "None"), intAccessoryRating, false, blnCreateChildren, blnCreateImprovements);
                        }
                        else
                        {
                            objAccessory.Create(objXmlAccessory, new Tuple<string, string>("Internal", "None"), intAccessoryRating, false, blnCreateChildren, blnCreateImprovements);
                        }

                        // Add any extra Gear that comes with the Weapon Accessory.
                        XmlNode xmlGearsNode = objXmlWeaponAccessory["gears"];
                        if (xmlGearsNode != null)
                        {
                            XmlDocument objXmlGearDocument = _objCharacter.LoadData("gear.xml");
                            foreach (XmlNode objXmlAccessoryGear in xmlGearsNode.SelectNodes("usegear"))
                            {
                                XmlNode objXmlAccessoryGearName = objXmlAccessoryGear["name"];
                                XmlAttributeCollection objXmlAccessoryGearNameAttributes = objXmlAccessoryGearName.Attributes;
                                int intGearRating = 0;
                                decimal decGearQty = 1;
                                string strChildForceSource = objXmlAccessoryGear["source"]?.InnerText ?? string.Empty;
                                string strChildForcePage = objXmlAccessoryGear["page"]?.InnerText ?? string.Empty;
                                string strChildForceValue = objXmlAccessoryGearNameAttributes?["select"]?.InnerText ?? string.Empty;
                                bool blnChildCreateChildren = objXmlAccessoryGearNameAttributes?["createchildren"]?.InnerText != bool.FalseString;
                                bool blnAddChildImprovements = objXmlAccessoryGearNameAttributes?["addimprovements"]?.InnerText != bool.FalseString;
                                if (objXmlAccessoryGear["rating"] != null)
                                    intGearRating = Convert.ToInt32(objXmlAccessoryGear["rating"].InnerText, GlobalOptions.InvariantCultureInfo);
                                if (objXmlAccessoryGearNameAttributes?["qty"] != null)
                                    decGearQty = Convert.ToDecimal(objXmlAccessoryGearNameAttributes["qty"].InnerText, GlobalOptions.InvariantCultureInfo);

                                XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = " + objXmlAccessoryGearName.InnerText.CleanXPath() + " and category = " +
                                                                                         objXmlAccessoryGear["category"].InnerText.CleanXPath() + "]");
                                Gear objGear = new Gear(_objCharacter);

                                objGear.Create(objXmlGear, intGearRating, lstWeapons, strChildForceValue, blnAddChildImprovements, blnChildCreateChildren);

                                objGear.Quantity = decGearQty;
                                objGear.Cost = "0";
                                objGear.ParentID = InternalId;

                                if (!string.IsNullOrEmpty(strChildForceSource))
                                    objGear.Source = strChildForceSource;
                                if (!string.IsNullOrEmpty(strChildForcePage))
                                    objGear.Page = strChildForcePage;
                                objAccessory.Gear.Add(objGear);

                                // Change the Capacity of the child if necessary.
                                if (objXmlAccessoryGear["capacity"] != null)
                                    objGear.Capacity = '[' + objXmlAccessoryGear["capacity"].InnerText + ']';
                            }
                        }

                        objAccessory.IncludedInWeapon = true;
                        objAccessory.Parent = this;
                        _lstAccessories.Add(objAccessory);
                    }
                }
            }

            // Add Subweapons (not underbarrels) if applicable.
            if (lstWeapons == null)
                return;
            // More than one Weapon can be added, so loop through all occurrences.
            foreach (XmlNode objXmlAddWeapon in objXmlWeapon.SelectNodes("addweapon"))
            {
                string strLoopID = objXmlAddWeapon.InnerText;
                XmlNode objXmlSubWeapon = strLoopID.IsGuid()
                    ? objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                    : objXmlDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");

                Weapon objSubWeapon = new Weapon(_objCharacter)
                {
                    ParentVehicle = ParentVehicle
                };
                int intAddWeaponRating = 0;
                if (objXmlAddWeapon.Attributes["rating"]?.InnerText != null)
                {
                    intAddWeaponRating = Convert.ToInt32(objXmlAddWeapon.Attributes["rating"]?.InnerText
                        .CheapReplace("{Rating}", () => Rating.ToString(GlobalOptions.InvariantCultureInfo)), GlobalOptions.InvariantCultureInfo);
                }
                objSubWeapon.Create(objXmlSubWeapon, lstWeapons, blnCreateChildren, blnCreateImprovements, blnSkipCost, intAddWeaponRating);
                objSubWeapon.ParentID = InternalId;
                objSubWeapon.Cost = "0";
                lstWeapons.Add(objSubWeapon);
            }
            foreach (Weapon objLoopWeapon in lstWeapons)
                objLoopWeapon.ParentVehicle = ParentVehicle;

            ToggleWirelessBonuses(WirelessOn);
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail => _objCachedSourceDetail = _objCachedSourceDetail
                                                                     ?? new SourceString(Source, DisplayPage(GlobalOptions.Language), GlobalOptions.Language, _objCharacter);

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("type", _strType);
            objWriter.WriteElementString("spec", _strSpec);
            objWriter.WriteElementString("spec2", _strSpec2);
            objWriter.WriteElementString("reach", _intReach.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("damage", _strDamage);
            objWriter.WriteElementString("ap", _strAP);
            objWriter.WriteElementString("mode", _strMode);
            objWriter.WriteElementString("rc", _strRC);
            objWriter.WriteElementString("ammo", _strAmmo);
            objWriter.WriteElementString("cyberware", _blnCyberware.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("ammocategory", _strAmmoCategory);
            objWriter.WriteElementString("ammoname", _strAmmoName);
            objWriter.WriteElementString("ammoslots", _intAmmoSlots.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("sizecategory", _strSizeCategory);
            objWriter.WriteElementString("firingmode",_eFiringMode.ToString());
            objWriter.WriteElementString("minrating", _strMinRating);
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteStartElement("clips");
            foreach (Clip clip in _lstAmmo)
            {
                if (string.IsNullOrWhiteSpace(clip.AmmoName))
                {
                    clip.AmmoName = GetAmmoName(clip.Guid, GlobalOptions.DefaultLanguage);
                }
                clip.Save(objWriter);
            }
            objWriter.WriteEndElement();

            objWriter.WriteElementString("conceal", _intConceal.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("useskill", _strUseSkill);
            objWriter.WriteElementString("useskillspec", _strUseSkillSpec);
            objWriter.WriteElementString("range", _strRange);
            objWriter.WriteElementString("alternaterange", _strAlternateRange);
            objWriter.WriteElementString("rangemultiply", _decRangeMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("singleshot", _intSingleShot.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("shortburst", _intShortBurst.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("longburst", _intLongBurst.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("fullburst", _intFullBurst.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("suppressive", _intSuppressive.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("allowsingleshot", _blnAllowSingleShot.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("allowshortburst", _blnAllowShortBurst.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("allowlongburst", _blnAllowLongBurst.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("allowfullburst", _blnAllowFullBurst.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("allowsuppressive", _blnAllowSuppressive.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("allowaccessory", _blnAllowAccessory.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("weaponname", _strWeaponName);
            objWriter.WriteElementString("included", _blnIncludedInWeapon.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("equipped", _blnEquipped.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("requireammo", _blnRequireAmmo.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("accuracy", _strAccuracy);
            objWriter.WriteElementString("mount", _strMount);
            objWriter.WriteElementString("stolen", _blnStolen.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("extramount", _strExtraMount);
            if (_lstAccessories.Count > 0)
            {
                objWriter.WriteStartElement("accessories");
                foreach (WeaponAccessory objAccessory in _lstAccessories)
                    objAccessory.Save(objWriter);
                objWriter.WriteEndElement();
            }
            if (_lstUnderbarrel.Count > 0)
            {
                foreach (Weapon objUnderbarrel in _lstUnderbarrel)
                {
                    objWriter.WriteStartElement("underbarrel");
                    objUnderbarrel.Save(objWriter);
                    objWriter.WriteEndElement();
                }
            }
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("weaponslots", _strWeaponSlots);
            objWriter.WriteElementString("doubledcostweaponslots", _strDoubledCostWeaponSlots);

            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("devicerating", _strDeviceRating);
            objWriter.WriteElementString("programlimit", _strProgramLimit);
            objWriter.WriteElementString("overclocked", _strOverclocked);
            objWriter.WriteElementString("attack", _strAttack);
            objWriter.WriteElementString("sleaze", _strSleaze);
            objWriter.WriteElementString("dataprocessing", _strDataProcessing);
            objWriter.WriteElementString("firewall", _strFirewall);
            objWriter.WriteElementString("attributearray", _strAttributeArray);
            objWriter.WriteElementString("modattack", _strModAttack);
            objWriter.WriteElementString("modsleaze", _strModSleaze);
            objWriter.WriteElementString("moddataprocessing", _strModDataProcessing);
            objWriter.WriteElementString("modfirewall", _strModFirewall);
            objWriter.WriteElementString("modattributearray", _strModAttributeArray);
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(GlobalOptions.InvariantCultureInfo));
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteEndElement();

            if (!IncludedInWeapon)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Are we loading a copy of an existing weapon?</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (blnCopy || !objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            objNode.TryGetStringFieldQuickly("name", ref _strName);
            if(!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            if (blnCopy)
            {
                _lstAmmo = new List<Clip>();
                _intActiveAmmoSlot = 1;
            }
            else
            {
                _lstAmmo.Clear();
                if (objNode["clips"] != null)
                {
                    XmlNode clipNode = objNode["clips"];

                    foreach (XmlNode node in clipNode.ChildNodes)
                    {
                        Clip LoopClip = Clip.Load(node);
                        if (string.IsNullOrWhiteSpace(LoopClip.AmmoName))
                        {
                            LoopClip.AmmoName = GetAmmoName(LoopClip.Guid, GlobalOptions.DefaultLanguage);
                        }
                        _lstAmmo.Add(LoopClip);
                    }
                }
                else //Load old clips
                {
                    foreach (string s in new[] { string.Empty, "2", "3", "4" })
                    {
                        int ammo = 0;
                        if (objNode.TryGetInt32FieldQuickly("ammoremaining" + s, ref ammo) &&
                            objNode.TryGetField("ammoloaded" + s, Guid.TryParse, out Guid guid) &&
                            ammo > 0 && guid != Guid.Empty)
                        {
                            _lstAmmo.Add(new Clip(guid, ammo));
                        }
                    }
                }
            }

            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            if (_strCategory == "Hold-Outs")
                _strCategory = "Holdouts";
            else if (_strCategory == "Cyberware Hold-Outs")
                _strCategory = "Cyberware Holdouts";
            objNode.TryGetStringFieldQuickly("type", ref _strType);
            objNode.TryGetStringFieldQuickly("spec", ref _strSpec);
            objNode.TryGetStringFieldQuickly("spec2", ref _strSpec2);
            objNode.TryGetInt32FieldQuickly("reach", ref _intReach);
            objNode.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
            objNode.TryGetStringFieldQuickly("damage", ref _strDamage);
            objNode.TryGetStringFieldQuickly("ratinglabel", ref _strRatingLabel);
            objNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            if (objNode["firingmode"] != null)
                _eFiringMode = ConvertToFiringMode(objNode["firingmode"].InnerText);
            // Legacy shim
            if (Name.Contains("Osmium Mace (STR"))
            {
                XmlNode objNewOsmiumMaceNode = _objCharacter.LoadData("weapons.xml").SelectSingleNode("/chummer/weapons/weapon[name = \"Osmium Mace\"]");
                if (objNewOsmiumMaceNode != null)
                {
                    objNewOsmiumMaceNode.TryGetStringFieldQuickly("name", ref _strName);
                    objNewOsmiumMaceNode.TryGetField("id", Guid.TryParse, out _guiSourceID);
                    _objCachedMyXmlNode = objNewOsmiumMaceNode;
                    objNewOsmiumMaceNode.TryGetStringFieldQuickly("accuracy", ref _strAccuracy);
                    objNewOsmiumMaceNode.TryGetStringFieldQuickly("damage", ref _strDamage);
                }
            }
            objNode.TryGetStringFieldQuickly("ap", ref _strAP);
            objNode.TryGetStringFieldQuickly("mode", ref _strMode);
            objNode.TryGetStringFieldQuickly("rc", ref _strRC);
            objNode.TryGetStringFieldQuickly("ammo", ref _strAmmo);
            objNode.TryGetBoolFieldQuickly("cyberware", ref _blnCyberware);
            objNode.TryGetStringFieldQuickly("ammocategory", ref _strAmmoCategory);
            objNode.TryGetStringFieldQuickly("ammoname", ref _strAmmoName);
            if (!objNode.TryGetInt32FieldQuickly("ammoslots", ref _intAmmoSlots))
                _intAmmoSlots = 1;
            objNode.TryGetStringFieldQuickly("sizecategory", ref _strSizeCategory);
            objNode.TryGetInt32FieldQuickly("conceal", ref _intConceal);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);
            objNode.TryGetInt32FieldQuickly("singleshot", ref _intSingleShot);
            objNode.TryGetInt32FieldQuickly("shortburst", ref _intShortBurst);
            objNode.TryGetInt32FieldQuickly("longburst", ref _intLongBurst);
            objNode.TryGetInt32FieldQuickly("fullburst", ref _intFullBurst);
            objNode.TryGetInt32FieldQuickly("suppressive", ref _intSuppressive);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("parentid", ref _strParentID);
            if (!objNode.TryGetBoolFieldQuickly("allowaccessory", ref _blnAllowAccessory))
                _blnAllowAccessory = GetNode()?["allowaccessory"]?.InnerText != bool.FalseString;
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("weaponname", ref _strWeaponName);
            objNode.TryGetBoolFieldQuickly("stolen", ref _blnStolen);
            objNode.TryGetStringFieldQuickly("range", ref _strRange);
            objNode.TryGetStringFieldQuickly("mount", ref _strMount);
            objNode.TryGetStringFieldQuickly("extramount", ref _strExtraMount);
            if (_strRange == "Hold-Outs")
            {
                _strRange = "Holdouts";
            }
            if (!objNode.TryGetStringFieldQuickly("alternaterange", ref _strAlternateRange))
            {
                string strAlternateRange = GetNode()?["alternaterange"]?.InnerText;
                if (!string.IsNullOrEmpty(strAlternateRange))
                {
                    _strAlternateRange = strAlternateRange;
                }
            }
            objNode.TryGetStringFieldQuickly("useskill", ref _strUseSkill);
            objNode.TryGetStringFieldQuickly("useskillspec", ref _strUseSkillSpec);
            objNode.TryGetDecFieldQuickly("rangemultiply", ref _decRangeMultiplier);
            objNode.TryGetBoolFieldQuickly("included", ref _blnIncludedInWeapon);
            if (Name == "Unarmed Attack")
                _blnIncludedInWeapon = true; // Unarmed Attack can never be removed
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            if (!_blnEquipped)
            {
                objNode.TryGetBoolFieldQuickly("installed", ref _blnEquipped);
            }
            objNode.TryGetBoolFieldQuickly("requireammo", ref _blnRequireAmmo);

            _nodWirelessBonus = objNode["wirelessbonus"];
            objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn);

            //#1544 Ammunition not loading or available.
            if (_strUseSkill == "Throwing Weapons"
                && _strAmmo != "1")
            {
                _strAmmo = "1";
            }

            XmlNode xmlAccessoriesNode = objNode["accessories"];
            if (xmlAccessoriesNode != null)
            {
                using (XmlNodeList nodChildren = xmlAccessoriesNode.SelectNodes("accessory"))
                {
                    if (nodChildren != null)
                    {
                        foreach (XmlNode nodChild in nodChildren)
                        {
                            WeaponAccessory objAccessory = new WeaponAccessory(_objCharacter);
                            objAccessory.Load(nodChild, blnCopy);
                            objAccessory.Parent = this;
                            _lstAccessories.Add(objAccessory);
                        }
                    }
                }
            }

            XmlNode xmlUnderbarrelNode = objNode["underbarrel"];
            if (xmlUnderbarrelNode != null)
            {
                using (XmlNodeList nodChildren = xmlUnderbarrelNode.SelectNodes("weapon"))
                {
                    if (nodChildren != null)
                    {
                        foreach (XmlNode nodWeapon in nodChildren)
                        {
                            Weapon objUnderbarrel = new Weapon(_objCharacter)
                            {
                                ParentVehicle = ParentVehicle
                            };
                            objUnderbarrel.Load(nodWeapon, blnCopy);
                            _lstUnderbarrel.Add(objUnderbarrel);
                        }
                    }
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            string strLocation = objNode["location"]?.InnerText;
            if (!string.IsNullOrEmpty(strLocation))
            {
                if (Guid.TryParse(strLocation, out Guid temp))
                {
                    // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                    _objLocation =
                        _objCharacter.WeaponLocations.FirstOrDefault(location =>
                            location.InternalId == temp.ToString());
                }
                else
                {
                    //Legacy. Location is a string.
                    _objLocation =
                        _objCharacter.WeaponLocations.FirstOrDefault(location =>
                            location.Name == strLocation);
                }
            }
            _objLocation?.Children.Add(this);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            if (!objNode.TryGetStringFieldQuickly("weaponslots", ref _strWeaponSlots))
            {
                XmlNode objXmlWeapon = GetNode();
                if (objXmlWeapon?["accessorymounts"] != null)
                {
                    XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("accessorymounts/mount");
                    if (objXmlMountList?.Count > 0)
                    {
                        StringBuilder strMounts = new StringBuilder();
                        foreach (XmlNode objXmlMount in objXmlMountList)
                        {
                            strMounts.Append(objXmlMount.InnerText);
                            strMounts.Append('/');
                        }
                        if (strMounts.Length > 0)
                            strMounts.Length -= 1;
                        _strWeaponSlots = strMounts.ToString();
                    }
                }
            }
            if (!objNode.TryGetStringFieldQuickly("doubledcostweaponslots", ref _strDoubledCostWeaponSlots))
            {
                XmlNode objXmlWeapon = GetNode();
                if (objXmlWeapon?["doubledcostaccessorymounts"] != null)
                {
                    XmlNodeList objXmlMountList = objXmlWeapon.SelectNodes("doubledcostaccessorymounts/mount");
                    if (objXmlMountList?.Count > 0)
                    {
                        StringBuilder strMounts = new StringBuilder();
                        foreach (XmlNode objXmlMount in objXmlMountList)
                        {
                            strMounts.Append(objXmlMount.InnerText);
                            strMounts.Append('/');
                        }
                        if (strMounts.Length > 0)
                            strMounts.Length -= 1;
                        _strDoubledCostWeaponSlots = strMounts.ToString();
                    }
                }
            }

            bool blnIsActive = false;
            if (objNode.TryGetBoolFieldQuickly("active", ref blnIsActive) && blnIsActive)
                this.SetActiveCommlink(_objCharacter, true);
            if (blnCopy)
            {
                this.SetHomeNode(_objCharacter, false);
            }
            else
            {
                bool blnIsHomeNode = false;
                if (objNode.TryGetBoolFieldQuickly("homenode", ref blnIsHomeNode) && blnIsHomeNode)
                {
                    this.SetHomeNode(_objCharacter, true);
                }
            }

            if (!objNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating))
                GetNode()?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objNode.TryGetStringFieldQuickly("programlimit", ref _strProgramLimit))
                GetNode()?.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
            objNode.TryGetStringFieldQuickly("overclocked", ref _strOverclocked);
            if (!objNode.TryGetStringFieldQuickly("attack", ref _strAttack))
                GetNode()?.TryGetStringFieldQuickly("attack", ref _strAttack);
            if (!objNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze))
                GetNode()?.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
            if (!objNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing))
                GetNode()?.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("firewall", ref _strFirewall))
                GetNode()?.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            if (!objNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
                GetNode()?.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray);
            if (!objNode.TryGetStringFieldQuickly("modattack", ref _strModAttack))
                GetNode()?.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            if (!objNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze))
                GetNode()?.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            if (!objNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing))
                GetNode()?.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall))
                GetNode()?.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            if (!objNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray))
                GetNode()?.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            // Find the piece of Gear that created this item if applicable
            List<Gear> lstGearToSearch = new List<Gear>(_objCharacter.Gear);
            foreach (Cyberware objCyberware in _objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
            {
                lstGearToSearch.AddRange(objCyberware.Gear);
            }
            foreach (Weapon objWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
            {
                foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                {
                    lstGearToSearch.AddRange(objAccessory.Gear);
                }
            }
            foreach (Armor objArmor in _objCharacter.Armor)
            {
                lstGearToSearch.AddRange(objArmor.Gear);
            }
            foreach (Vehicle objVehicle in _objCharacter.Vehicles)
            {
                lstGearToSearch.AddRange(objVehicle.Gear);
                foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                {
                    foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                    {
                        lstGearToSearch.AddRange(objAccessory.Gear);
                    }
                }
                foreach (VehicleMod objVehicleMod in objVehicle.Mods.Where(x => x.Cyberware.Count > 0 || x.Weapons.Count > 0))
                {
                    foreach (Cyberware objCyberware in objVehicleMod.Cyberware.DeepWhere(x => x.Children, x => x.Gear.Count > 0))
                    {
                        lstGearToSearch.AddRange(objCyberware.Gear);
                    }
                    foreach (Weapon objWeapon in objVehicleMod.Weapons.DeepWhere(x => x.Children, x => x.WeaponAccessories.Any(y => y.Gear.Count > 0)))
                    {
                        foreach (WeaponAccessory objAccessory in objWeapon.WeaponAccessories)
                        {
                            lstGearToSearch.AddRange(objAccessory.Gear);
                        }
                    }
                }
            }
            Gear objGear = lstGearToSearch.DeepFindById(ParentID);

            objWriter.WriteStartElement("weapon");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("type", WeaponType);
            objWriter.WriteElementString("reach", TotalReach.ToString(objCulture));
            objWriter.WriteElementString("accuracy", GetAccuracy(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("damage", CalculatedDamage(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("damage_english", CalculatedDamage(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage));
            objWriter.WriteElementString("rawdamage", Damage);
            objWriter.WriteElementString("ap", TotalAP(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("mode", CalculatedMode(strLanguageToPrint));
            objWriter.WriteElementString("rc", TotalRC(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("ammo", CalculatedAmmo(objCulture, strLanguageToPrint));
            objWriter.WriteElementString("ammo_english", CalculatedAmmo(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage));
            objWriter.WriteElementString("conceal", CalculatedConcealability(objCulture));
            if (objGear != null)
            {
                objWriter.WriteElementString("avail", objGear.TotalAvail(objCulture, strLanguageToPrint));
                objWriter.WriteElementString("cost", objGear.TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
                objWriter.WriteElementString("owncost", objGear.OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            }
            else
            {
                objWriter.WriteElementString("avail", TotalAvail(objCulture, strLanguageToPrint));
                objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
                objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            }
            objWriter.WriteElementString("source", _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("weaponname", CustomName);
            objWriter.WriteElementString("location", Location?.DisplayName());
            if (_lstAccessories.Count > 0)
            {
                objWriter.WriteStartElement("accessories");
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                    objAccessory.Print(objWriter, objCulture, strLanguageToPrint);
                objWriter.WriteEndElement();
            }

            IDictionary<string, string> dictionaryRanges = GetRangeStrings(objCulture);
            // <ranges>
            objWriter.WriteStartElement("ranges");
            objWriter.WriteElementString("name", DisplayRange(strLanguageToPrint));
            objWriter.WriteElementString("short", dictionaryRanges["short"]);
            objWriter.WriteElementString("medium", dictionaryRanges["medium"]);
            objWriter.WriteElementString("long", dictionaryRanges["long"]);
            objWriter.WriteElementString("extreme", dictionaryRanges["extreme"]);
            // </ranges>
            objWriter.WriteEndElement();

            // <alternateranges>
            objWriter.WriteStartElement("alternateranges");
            objWriter.WriteElementString("name", DisplayAlternateRange(strLanguageToPrint));
            objWriter.WriteElementString("short", dictionaryRanges["alternateshort"]);
            objWriter.WriteElementString("medium", dictionaryRanges["alternatemedium"]);
            objWriter.WriteElementString("long", dictionaryRanges["alternatelong"]);
            objWriter.WriteElementString("extreme", dictionaryRanges["alternateextreme"]);
            // </alternateranges>
            objWriter.WriteEndElement();

            foreach (Weapon objUnderbarrel in Children)
            {
                objWriter.WriteStartElement("underbarrel");
                objUnderbarrel.Print(objWriter, objCulture, strLanguageToPrint);
                objWriter.WriteEndElement();
            }

            // Currently loaded Ammo.
            Guid guiAmmo = GetClip(_intActiveAmmoSlot).Guid;
            objWriter.WriteElementString("availableammo", GetAvailableAmmo.ToString(objCulture));
            objWriter.WriteElementString("currentammo", GetAmmoName(guiAmmo, strLanguageToPrint));
            objWriter.WriteStartElement("clips");
            foreach (Clip objClip in _lstAmmo)
            {
                objClip.AmmoName = GetAmmoName(objClip.Guid, strLanguageToPrint);
                objClip.Save(objWriter);
            }
            objWriter.WriteEndElement();

            //Don't seem to be used
            //objWriter.WriteElementString("ammoslot1", GetAmmoName(_guiAmmoLoaded));
            //objWriter.WriteElementString("ammoslot2", GetAmmoName(_guiAmmoLoaded2));
            //objWriter.WriteElementString("ammoslot3", GetAmmoName(_guiAmmoLoaded3));
            //objWriter.WriteElementString("ammoslot4", GetAmmoName(_guiAmmoLoaded4));

            objWriter.WriteElementString("dicepool", DicePool.ToString(objCulture));
            objWriter.WriteElementString("skill", Skill?.Name);

            objWriter.WriteElementString("wirelesson", WirelessOn.ToString(GlobalOptions.InvariantCultureInfo));
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);

            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Get the name of Ammo from the character or Vehicle.
        /// </summary>
        /// <param name="guiAmmo">InternalId of the Ammo to find.</param>
        /// <param name="strLanguage">Language in which to display ammo name.</param>
        public string GetAmmoName(Guid guiAmmo, string strLanguage)
        {
            if (guiAmmo == Guid.Empty)
                return string.Empty;
            string strAmmoGuid = guiAmmo.ToString("D", GlobalOptions.InvariantCultureInfo);
            Gear objAmmo = ParentVehicle != null
                ? _objCharacter.Vehicles.FindVehicleGear(strAmmoGuid)
                : _objCharacter.Gear.DeepFindById(strAmmoGuid);

            return objAmmo?.DisplayNameShort(strLanguage) ?? string.Empty;
        }

        /// <summary>
        /// Get the amount of available Ammo from the character or Vehicle.
        /// </summary>
        private decimal GetAvailableAmmo
        {
            get
            {
                if (!RequireAmmo)
                {
                    return 0;
                }
                HashSet<string> setAmmoPrefixStringSet = new HashSet<string>(AmmoPrefixStrings);
                IList<Gear> lstGear = ParentVehicle == null ? _objCharacter.Gear : ParentVehicle.Gear;
                return lstGear.DeepWhere(x => x.Children, x =>
                    x.Quantity > 0 && (x.Category == "Ammunition" && x.Extra == AmmoCategory ||
                                       !string.IsNullOrWhiteSpace(AmmoName) && x.Name == AmmoName ||
                                       string.IsNullOrEmpty(x.Extra) && setAmmoPrefixStringSet.Any(y => x.Name.StartsWith(y, StringComparison.Ordinal)) ||
                                       UseSkill == "Throwing Weapons" && Name == x.Name)).Sum(x => x.Quantity);
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Weapon Accessories.
        /// </summary>
        public TaggedObservableCollection<WeaponAccessory> WeaponAccessories => _lstAccessories;

        /// <summary>
        /// Underbarrel Weapon.
        /// </summary>
        public TaggedObservableCollection<Weapon> UnderbarrelWeapons => _lstUnderbarrel;

        /// <summary>
        /// Children as Underbarrel Weapon.
        /// </summary>
        public TaggedObservableCollection<Weapon> Children => UnderbarrelWeapons;

        /// <summary>
        /// Internal identifier which will be used to identify this Weapon.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Display name.
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            if (Rating > 0)
            {
                strReturn += strSpace + '(' +
                             LanguageManager.GetString(RatingLabel, strLanguage) + strSpace +
                             Rating.ToString(objCulture) + ')';
            }

            if (!string.IsNullOrEmpty(_strWeaponName))
            {
                strReturn += strSpace + "(\"" + _strWeaponName + "\")";
            }

            return strReturn;
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set => _strName = value;
        }

        /// <summary>
        /// A custom name for the Weapon assigned by the player.
        /// </summary>
        public string CustomName
        {
            get => _strWeaponName;
            set => _strWeaponName = value;
        }

        public int Rating
        {
            get => _intRating;
            set => _intRating = value;
        }

        public string RatingLabel
        {
            get => _strRatingLabel;
            set => _strRatingLabel = value;
        }

        /// <summary>
        /// Minimum Rating (string form).
        /// </summary>
        public string MinRating
        {
            get => _strMinRating;
            set => _strMinRating = value;
        }

        /// <summary>
        /// Maximum Rating (string form).
        /// </summary>
        public string MaxRating
        {
            get => _strMaxRating;
            set => _strMaxRating = value;
        }

        /// <summary>
        /// Minimum Rating (value form).
        /// </summary>
        public int MinRatingValue
        {
            get
            {
                string strExpression = MinRating;
                return string.IsNullOrEmpty(strExpression) ? 0 : ProcessRatingString(strExpression);
            }
            set => MinRating = value.ToString(GlobalOptions.InvariantCultureInfo);
        }

        /// <summary>
        /// Maximum Rating (string form).
        /// </summary>
        public int MaxRatingValue
        {
            get
            {
                string strExpression = MaxRating;
                return string.IsNullOrEmpty(strExpression) ? int.MaxValue : ProcessRatingString(strExpression);
            }
            set => MaxRating = value.ToString(GlobalOptions.InvariantCultureInfo);
        }

        /// <summary>
        /// Processes a string into an int based on logical processing.
        /// </summary>
        /// <param name="strExpression"></param>
        /// <returns></returns>
        private int ProcessRatingString(string strExpression)
        {
            if (strExpression.StartsWith("FixedValues(", StringComparison.Ordinal))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                StringBuilder objValue = new StringBuilder(strExpression);
                objValue.Replace("{Rating}", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "}",
                        () => _objCharacter.GetAttribute(strCharAttributeName).TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "Base}",
                        () => _objCharacter.GetAttribute(strCharAttributeName).TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }

                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objValue.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToInt32(Math.Ceiling((double)objProcess)) : 0;
            }

            int.TryParse(strExpression, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intReturn);

            return intReturn;
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            // If Categories are actually the name of object types, so pull them from the language file.
            if (Category == "Gear")
            {
                return LanguageManager.GetString("String_SelectPACKSKit_Gear", strLanguage);
            }

            if (Category == "Cyberware")
            {
                return LanguageManager.GetString("String_SelectPACKSKit_Cyberware", strLanguage);
            }
            if (Category == "Bioware")
            {
                return LanguageManager.GetString("String_SelectPACKSKit_Bioware", strLanguage);
            }

            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return _objCharacter.LoadData("weapons.xml", strLanguage)
                       .SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText
                   ?? Category;
        }

        /// <summary>
        /// Translated Ammo Category.
        /// </summary>
        public string DisplayAmmoCategory(string strLanguage)
        {
            if (!string.IsNullOrWhiteSpace(AmmoName))
                return DisplayAmmoName(strLanguage);
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return AmmoCategory;

            return _objCharacter.LoadData("weapons.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + AmmoCategory + "\"]/@translate")?.InnerText ?? AmmoCategory;
        }

        /// <summary>
        /// Translated Ammo Category.
        /// </summary>
        public string DisplayAmmoName(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return AmmoName;

            return _objCharacter.LoadData("gear.xml", strLanguage).SelectSingleNode("/chummer/gears/gear[name = " + AmmoName.CleanXPath() + "]/@translate")?.InnerText ?? AmmoName;
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
        /// Effective size of the weapon for mounting purposes.
        /// </summary>
        public string SizeCategory => string.IsNullOrWhiteSpace(_strSizeCategory) ? Category : _strSizeCategory;

        /// <summary>
        /// Type of Weapon (either Melee or Ranged).
        /// </summary>
        public string WeaponType
        {
            get => _strType;
            set => _strType = value;
        }

        /// <summary>
        /// Is this weapon cyberware?
        /// </summary>
        public bool Cyberware => _blnCyberware;

        /// <summary>
        /// Reach.
        /// </summary>
        public int Reach
        {
            get => _intReach;
            set => _intReach = value;
        }

        /// <summary>
        /// Accuracy.
        /// </summary>
        public string Accuracy
        {
            get => _strAccuracy;
            set => _strAccuracy = value;
        }

        /// <summary>
        /// Damage.
        /// </summary>
        public string Damage
        {
            get => _strDamage;
            set => _strDamage = value;
        }

        /// <summary>
        /// Armor Penetration.
        /// </summary>
        public string AP
        {
            get => _strAP;
            set => _strAP = value;
        }

        /// <summary>
        /// Firing Mode.
        /// </summary>
        public string Mode
        {
            get => _strMode;
            set => _strMode = value;
        }

        /// <summary>
        /// Recoil.
        /// </summary>
        public string RC
        {
            get => _strRC;
            set => _strRC = value;
        }

        /// <summary>
        /// Ammo.
        /// </summary>
        public string Ammo
        {
            get => _strAmmo;
            set => _strAmmo = value;
        }

        /// <summary>
        /// Category of Ammo the Weapon uses.
        /// </summary>
        public string AmmoCategory
        {
            get
            {
                if (!string.IsNullOrEmpty(_strAmmoCategory))
                    return _strAmmoCategory;

                return Category;
            }
        }

        /// <summary>
        /// Category of Ammo the Weapon uses.
        /// </summary>
        public string AmmoName => _strAmmoName;

        /// <summary>
        /// What names can gear begin with to count as ammunition for this weapon
        /// </summary>
        public IEnumerable<string> AmmoPrefixStrings
        {
            get
            {
                if (Spec == "Flare Launcher" && Name == "Micro Flare Launcher")
                    yield return "Micro Flares";
                else if (Name.Contains("Net Gun XL"))
                    yield return "XL Net Gun";
                else if (Name.Contains("Net Gun"))
                    yield return "Net Gun";
                else if (Name == "Pepper Punch Pen")
                    yield return "Pepper Punch";
                else if (Name == "Ares S-III Super Squirt")
                    yield return "Ammo: DMSO Rounds";
                else
                {
                    switch (AmmoCategory)
                    {
                        case "Grenade Launchers":
                            yield return "Minigrenade:";
                            break;
                        case "Missile Launchers":
                            yield return "Missile:";
                            yield return "Rocket:";
                            break;
                        case "Mortar Launchers":
                            yield return "Mortar Round:";
                            break;
                        case "Bows":
                            yield return "Arrow:";
                            break;
                        case "Crossbows":
                            if (Name.Contains("Harpoon"))
                            {
                                yield return "Harpoon";
                                yield return "Bolt:";
                            }
                            else
                                yield return "Bolt:";
                            break;
                        case "Flamethrowers":
                            yield return "Ammo: Fuel";
                            break;
                        case "Gear":
                        {
                            string strGearName = Name;
                            if (!string.IsNullOrEmpty(ParentID))
                            {
                                Gear objParent = _objCharacter.Gear.DeepFindById(ParentID) ??
                                                 _objCharacter.Vehicles.FindVehicleGear(ParentID) ??
                                                 _objCharacter.Weapons.FindWeaponGear(ParentID) ??
                                                 _objCharacter.Armor.FindArmorGear(ParentID) ??
                                                 _objCharacter.Cyberware.FindCyberwareGear(ParentID);
                                if (objParent != null)
                                    strGearName = objParent.Name;
                            }

                            yield return strGearName;
                            break;
                        }
                        default:
                            yield return "Ammo:";
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// The number of rounds remaining in the Weapon.
        /// </summary>
        public int AmmoRemaining
        {
            get => GetClip(_intActiveAmmoSlot).Ammo;
            set => GetClip(_intActiveAmmoSlot).Ammo = value;
        }

        /// <summary>
        /// The total number of rounds that the weapon can load.
        /// </summary>
        private static string AmmoCapacity(string ammoString)
        {
            // Determine which loading methods are available to the Weapon.
            if (ammoString.IndexOfAny('x', '+') != -1 || ammoString.Contains("Special"))
            {
                string strWeaponAmmo = ammoString.ToLowerInvariant();
                // Get rid of external source, special, or belt, and + energy.
                strWeaponAmmo = strWeaponAmmo.Replace("external source", "100")
                    .Replace("special", "100")
                    .FastEscapeOnceFromEnd(" + energy");

                // Assuming base text of 10(ml)x2
                // matches [2x]10(ml) or [10x]2(ml)
                foreach (Match m in Regex.Matches(strWeaponAmmo, "^[0-9]*[0-9]*x"))
                {
                    strWeaponAmmo = strWeaponAmmo.TrimStartOnce(m.Value);
                }

                // Matches 2(ml[)x10] (But does not capture the ')') or 10(ml)[x2]
                foreach (Match m in Regex.Matches(strWeaponAmmo, "(?<=\\))(x[0-9]*[0-9]*$)*"))
                {
                    strWeaponAmmo = strWeaponAmmo.TrimEndOnce(m.Value);
                }

                int intPos = strWeaponAmmo.IndexOf('(');
                if (intPos != -1)
                    strWeaponAmmo = strWeaponAmmo.Substring(0, intPos);
                return strWeaponAmmo;
            }
            else
            {
                // Nothing weird in the ammo string, so just use the number given.
                string strAmmo = ammoString;
                int intPos = strAmmo.IndexOf('(');
                if (intPos != -1)
                    strAmmo = strAmmo.Substring(0, intPos);
                return strAmmo;
            }
        }

        /// <summary>
        /// The type of Ammunition loaded in the Weapon.
        /// </summary>
        public string AmmoLoaded
        {
            get => GetClip(_intActiveAmmoSlot).Guid.ToString("D", GlobalOptions.InvariantCultureInfo);
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    GetClip(_intActiveAmmoSlot).Guid = guiTemp;
            }
        }

        /// <summary>
        /// Active Ammo slot number.
        /// </summary>
        public int ActiveAmmoSlot
        {
            get => _intActiveAmmoSlot;
            set => _intActiveAmmoSlot = value;
        }

        /// <summary>
        /// Number of Ammo slots the Weapon has.
        /// </summary>
        public int AmmoSlots
        {
            get
            {
                return _intAmmoSlots + WeaponAccessories.Sum(objAccessory => objAccessory.AmmoSlots);
            }
        }

        /// <summary>
        /// Concealability.
        /// </summary>
        public int Concealability
        {
            get => _intConceal;
            set => _intConceal = value;
        }

        /// <summary>
        /// Avail.
        /// </summary>
        public string Avail
        {
            get => _strAvail;
            set => _strAvail = value;
        }

        /// <summary>
        /// Cost.
        /// </summary>
        public string Cost
        {
            get => _strCost;
            set => _strCost = value;
        }

        public string DisplayCost(out decimal decItemCost, decimal decMarkup = 0.0m)
        {
            string strReturn = Cost;
            if (strReturn.StartsWith("Variable(", StringComparison.Ordinal))
            {
                strReturn = strReturn.TrimStartOnce("Variable(", true).TrimEndOnce(')');
                decimal decMin;
                decimal decMax = decimal.MaxValue;
                if (strReturn.Contains('-'))
                {
                    string[] strValues = strReturn.Split('-');
                    decMin = Convert.ToDecimal(strValues[0], GlobalOptions.InvariantCultureInfo);
                    decMax = Convert.ToDecimal(strValues[1], GlobalOptions.InvariantCultureInfo);
                }
                else
                    decMin = Convert.ToDecimal(strReturn.FastEscape('+'), GlobalOptions.InvariantCultureInfo);

                if (decMax == decimal.MaxValue)
                    strReturn = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + "¥+";
                else
                    strReturn = decMin.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + " - " + decMax.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';

                decItemCost = decMin;
                return strReturn;
            }

            decimal decTotalCost = Convert.ToDecimal(strReturn, GlobalOptions.InvariantCultureInfo);

            decTotalCost *= 1.0m + decMarkup;

            if (DiscountCost)
                decTotalCost *= 0.9m;

            decItemCost = decTotalCost;

            return decTotalCost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
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
        public string Page
        {
            get => _strPage;
            set => _strPage = value;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        public Weapon Parent
        {
            get => _objParent;
            set
            {
                if (_objParent != value)
                {
                    _objParent = value;
                    // Includes ParentVehicle setter
                    ParentMount = value?.ParentMount;
                }
            }
        }

        /// <summary>
        /// ID of the object that added this weapon (if any).
        /// </summary>
        public string ParentID
        {
            get => _strParentID;
            set => _strParentID = value;
        }

        /// <summary>
        /// Whether the object allows accessories.
        /// </summary>
        public bool AllowAccessory
        {
            get => _blnAllowAccessory;
            set
            {
                if (value)
                    _blnAllowAccessory = true;
                else if (_blnAllowAccessory)
                {
                    _blnAllowAccessory = false;
                    foreach (Weapon objChild in UnderbarrelWeapons)
                        objChild.AllowAccessory = false;
                }
            }
        }

        /// <summary>
        /// Location.
        /// </summary>
        public Location Location
        {
            get; set;
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
        /// Vehicle to which the weapon is mounted (if none, returns null)
        /// </summary>
        public Vehicle ParentVehicle
        {
            get => _objMountedVehicle;
            set
            {
                if (_objMountedVehicle != value)
                {
                    _objMountedVehicle = value;
                    foreach (WeaponAccessory objAccessory in WeaponAccessories)
                    {
                        foreach (Gear objGear in objAccessory.Gear)
                        {
                            if (value != null)
                                objGear.ChangeEquippedStatus(false);
                            else if (Equipped && objGear.Equipped)
                                objGear.ChangeEquippedStatus(true);
                        }
                    }
                }

                if (value == null)
                {
                    _objWeaponMount = null;
                }

                foreach (Weapon objChild in Children)
                    objChild.ParentVehicle = value;
            }
        }

        /// <summary>
        /// WeaponMount to which the weapon is mounted (if none, returns null)
        /// </summary>
        public WeaponMount ParentMount
        {
            get => _objWeaponMount;
            set
            {
                if (_objWeaponMount != value)
                {
                    _objWeaponMount = value;
                    ParentVehicle = value?.Parent;
                }
                foreach (Weapon objChild in Children)
                    objChild.ParentMount = value;
            }
        }

        /// <summary>
        /// Whether or not the Underbarrel Weapon is part of the parent Weapon by default.
        /// </summary>
        public bool IncludedInWeapon
        {
            get => _blnIncludedInWeapon;
            set => _blnIncludedInWeapon = value;
        }

        /// <summary>
        /// Whether or not the Underbarrel Weapon is installed.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set => _blnEquipped = value;
        }

        /// <summary>
        /// Active Skill that should be used with this Weapon instead of the default one.
        /// </summary>
        public string UseSkill
        {
            get => _strUseSkill;
            set => _strUseSkill = value;
        }

        /// <summary>
        /// Active Skill Specialization that should be used with this Weapon instead of the default one.
        /// </summary>
        public string UseSkillSpec
        {
            get => string.IsNullOrWhiteSpace(_strUseSkillSpec) ? _strName : _strUseSkillSpec;
            set => _strUseSkillSpec = value;
        }

        /// <summary>
        /// Whether or not the Armor's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost && _objCharacter.BlackMarketDiscount;
            set => _blnDiscountCost = value;
        }

        /// <summary>
        /// Whether or not the Weapon requires Ammo to be reloaded.
        /// </summary>
        public bool RequireAmmo
        {
            get => _blnRequireAmmo;
            set => _blnRequireAmmo = value;
        }

        /// <summary>
        /// The Active Skill Specialization that this Weapon uses, in addition to any others it would normally use.
        /// </summary>
        public string Spec => _strSpec;

        /// <summary>
        /// The second Active Skill Specialization that this Weapon uses, in addition to any others it would normally use.
        /// </summary>
        public string Spec2 => _strSpec2;

        /// <summary>
        /// Used by our sorting algorithm to remember which order the user moves things to
        /// </summary>
        public int SortOrder
        {
            get => _intSortOrder;
            set => _intSortOrder = value;
        }



        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = _objCharacter.LoadData("weapons.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? $"/chummer/weapons/weapon[name = \"{Name}\"]"
                        : $"/chummer/weapons/weapon[id = \"{SourceIDString}\" or id = \"{SourceIDString.ToUpperInvariant()}\"]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        /// <summary>
        /// Wireless Bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set => _nodWirelessBonus = value;
        }

        /// <summary>
        /// Whether or not the Weapon's wireless bonus is enabled
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set
            {
                if (value == _blnWirelessOn)
                    return;
                ToggleWirelessBonuses(value);
                _blnWirelessOn = value;
            }
        }

        public bool Stolen
        {
            get => _blnStolen;
            set => _blnStolen = value;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications in the program's current language.
        /// </summary>
        public string DisplayConcealability => CalculatedConcealability(GlobalOptions.CultureInfo);

        /// <summary>
        /// Weapon's total Concealability including all Accessories and Modifications.
        /// </summary>
        public string CalculatedConcealability(CultureInfo objCulture)
        {
            int intReturn = Concealability;

            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                if (objAccessory.Equipped)
                    intReturn += objAccessory.TotalConcealability;
            }

            /* Commented out because there's no reference to this in RAW
            // Add +4 for each Underbarrel Weapon installed.
            if (_lstUnderbarrel.Count > 0)
            {
                foreach (Weapon objUnderbarrelWeapon in _lstUnderbarrel)
                {
                    if (objUnderbarrelWeapon.Installed)
                        intReturn += 4;
                }
            }
            */

            // Factor in the character's Concealability modifiers.
            intReturn += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Concealability);

            string strReturn;
            if (intReturn >= 0)
                strReturn = '+' + intReturn.ToString(objCulture);
            else
                strReturn = intReturn.ToString(objCulture);

            return strReturn;
        }

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition in the program's current language.
        /// </summary>
        public string DisplayDamage => CalculatedDamage(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Weapon's Damage including all Accessories, Modifications, Attributes, and Ammunition.
        /// </summary>
        public string CalculatedDamage(CultureInfo objCulture, string strLanguage)
        {
            // If the cost is determined by the Rating, evaluate the expression.
            string strDamage = Damage;
            string strDamageType = string.Empty;
            string strDamageExtra = string.Empty;

            int intUseSTR = 0;
            int intUseAGI = 0;
            if (strDamage.Contains("STR") || strDamage.Contains("AGI"))
            {
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        intUseAGI = ParentVehicle.Pilot;
                        if (!string.IsNullOrEmpty(ParentID))
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID, out VehicleMod objVehicleMod);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                int intSTR = objAttributeSource.TotalStrength;
                                int intAGI = objAttributeSource.TotalStrength;
                                while (objAttributeSource != null)
                                {
                                    if (intSTR != 0 || intAGI != 0)
                                        break;
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null) continue;
                                    intSTR = objAttributeSource.TotalStrength;
                                    intAGI = objAttributeSource.TotalStrength;
                                }

                                intUseSTR = intSTR;
                                intUseAGI = intAGI;

                                if (intUseSTR == 0)
                                    intUseSTR = objVehicleMod.TotalStrength;
                                if (intUseAGI == 0)
                                    intUseAGI = objVehicleMod.TotalAgility;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intAGI = objAttributeSource.TotalStrength;
                            }

                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                        if (intUseAGI == 0)
                            intUseAGI = _objCharacter.AGI.TotalValue;
                    }
                }
                else if (ParentVehicle == null)
                {
                    intUseSTR = _objCharacter.STR.TotalValue;
                    intUseAGI = _objCharacter.AGI.TotalValue;
                }

                if (Category == "Throwing Weapons" || UseSkill == "Throwing Weapons")
                    intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);
            }

            foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList)
            {
                if (objLoopAttribute.Abbrev == "STR")
                    strDamage = strDamage.Replace("STR", intUseSTR.ToString(GlobalOptions.InvariantCultureInfo));
                else if (objLoopAttribute.Abbrev == "AGI")
                    strDamage = strDamage.Replace("AGI", intUseAGI.ToString(GlobalOptions.InvariantCultureInfo));
                else
                    strDamage = strDamage.CheapReplace(objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
            }
            foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.SpecialAttributeList)
            {
                if (objLoopAttribute.Abbrev == "STR")
                    strDamage = strDamage.Replace("STR", intUseSTR.ToString(GlobalOptions.InvariantCultureInfo));
                else if (objLoopAttribute.Abbrev == "AGI")
                    strDamage = strDamage.Replace("AGI", intUseAGI.ToString(GlobalOptions.InvariantCultureInfo));
                else
                    strDamage = strDamage.CheapReplace(objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
            }

            strDamage.CheapReplace("{Rating}", () => Rating.ToString(GlobalOptions.InvariantCultureInfo));
            // Evaluate the min expression if there is one.
            int intStart = strDamage.IndexOf("min(", StringComparison.Ordinal);
            if (intStart != -1)
            {
                int intEnd = strDamage.IndexOf(')', intStart);
                string strMin = strDamage.Substring(intStart, intEnd - intStart + 1);

                string[] strValue = strMin.TrimStartOnce("min(", true).TrimEndOnce(')').Split(',');
                int intMinValue = Convert.ToInt32(strValue[0], GlobalOptions.InvariantCultureInfo);
                for (int i = 1; i < strValue.Length; ++i)
                {
                    intMinValue = Math.Min(intMinValue, Convert.ToInt32(strValue[i], GlobalOptions.InvariantCultureInfo));
                }

                strDamage = strDamage.Replace(strMin, intMinValue.ToString(GlobalOptions.InvariantCultureInfo));
            }

            // Place the Damage Type (P or S) into a string and remove it from the expression.
            if (strDamage.Contains("P or S"))
            {
                strDamageType = "P or S";
                strDamage = strDamage.FastEscape("P or S");
            }
            else if (strDamage.Contains('P'))
            {
                strDamageType = "P";
                strDamage = strDamage.FastEscape('P');
            }
            else if (strDamage.Contains('S'))
            {
                strDamageType = "S";
                strDamage = strDamage.FastEscape('S');
            }
            else if (strDamage.Contains("(M)"))
            {
                strDamageType = "M";
                strDamage = strDamage.FastEscape("(M)");
            }
            // Place any extra text like (e) and (f) in a string and remove it from the expression.
            if (strDamage.Contains("(e)"))
            {
                strDamageExtra = "(e)";
                strDamage = strDamage.FastEscape("(e)");
            }
            else if (strDamage.Contains("(f)"))
            {
                strDamageExtra = "(f)";
                strDamage = strDamage.FastEscape("(f)");
            }

            // Look for splash damage info.
            if (strDamage.Contains("/m)") || strDamage.Contains(" Radius)"))
            {
                int intPos = strDamage.IndexOf('(');
                string strSplash = strDamage.Substring(intPos, strDamage.IndexOf(')') - intPos + 1);
                strDamageExtra += ' ' + strSplash;
                strDamage = strDamage.FastEscape(strSplash).Trim();
            }

            // Include WeaponCategoryDV Improvements.
            int intImprove = 0;
            if (_objCharacter != null)
            {
                string strCategory = Category;
                if (strCategory == "Unarmed")
                {
                    strCategory = "Unarmed Combat";
                }

                string strUseSkill = Skill?.Name;

                string strExoticMelee = "Exotic Melee Weapon (" + UseSkillSpec + ')';
                string strExoticRanged = "Exotic Ranged Weapon (" + UseSkillSpec + ')';
                intImprove += _objCharacter.Improvements.Where(objImprovement =>
                        objImprovement.ImproveType == Improvement.ImprovementType.WeaponCategoryDV &&
                        objImprovement.Enabled && (objImprovement.ImprovedName == strCategory
                                                   || objImprovement.ImprovedName == strUseSkill
                                                   || (Skill?.IsExoticSkill == true
                                                       && (objImprovement.ImprovedName == strExoticMelee
                                                           || objImprovement.ImprovedName == strExoticRanged))
                                                   || "Cyberware " + objImprovement.ImprovedName == strCategory))
                    .Sum(objImprovement => objImprovement.Value);
            }

            // If this is the Unarmed Attack Weapon and the character has the UnarmedDVPhysical Improvement, change the type to Physical.
            // This should also add any UnarmedDV bonus which only applies to Unarmed Combat, not Unarmed Weapons.
            if (Name == "Unarmed Attack")
            {
                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (strDamageType == "S" && objImprovement.ImproveType == Improvement.ImprovementType.UnarmedDVPhysical && objImprovement.Enabled)
                        strDamageType = "P";
                    if (objImprovement.ImproveType == Improvement.ImprovementType.UnarmedDV && objImprovement.Enabled)
                        intImprove += objImprovement.Value;
                }
            }

            // This should also add any UnarmedDV bonus to Unarmed physical weapons if the option is enabled.
            else if (Skill?.Name == "Unarmed Combat" && _objCharacter.Options.UnarmedImprovementsApplyToWeapons)
            {
                intImprove += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedDV);
            }
            bool blnDamageReplaced = false;

            // Add in the DV bonus from any Weapon Mods.
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                if (objAccessory.Equipped)
                {
                    if (!string.IsNullOrEmpty(objAccessory.DamageType))
                    {
                        strDamageType = string.Empty;
                        strDamageExtra = objAccessory.DamageType;
                    }
                    // Adjust the Weapon's Damage.
                    if (!string.IsNullOrEmpty(objAccessory.Damage))
                    {
                        strDamage += " + " + objAccessory.Damage;
                    }
                    if (!string.IsNullOrEmpty(objAccessory.DamageReplacement))
                    {
                        blnDamageReplaced = true;
                        strDamage = objAccessory.DamageReplacement;
                    }
                }
            }
            if (intImprove != 0)
                strDamage += " + " + intImprove.ToString(GlobalOptions.InvariantCultureInfo);

            int intBonus = _objCharacter.Options.MoreLethalGameplay ? 2 : 0;

            // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
            if (!string.IsNullOrEmpty(AmmoLoaded))
            {
                // Look for Ammo on the character.
                Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded) ?? _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);
                if (objGear != null)
                {
                    if (objGear.WeaponBonus != null)
                    {
                        // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                        if (!(objGear.WeaponBonus.InnerXml.Contains("(f)") && Damage.Contains("(f)")))
                        {
                            if (objGear.WeaponBonus["damagetype"] != null)
                            {
                                strDamageType = string.Empty;
                                strDamageExtra = objGear.WeaponBonus["damagetype"].InnerText;
                            }
                            // Adjust the Weapon's Damage.
                            if (objGear.WeaponBonus["damage"] != null)
                                strDamage += " + " + objGear.WeaponBonus["damage"].InnerText;
                            if (objGear.WeaponBonus["damagereplace"] != null)
                            {
                                blnDamageReplaced = true;
                                strDamage = objGear.WeaponBonus["damagereplace"].InnerText;
                            }
                        }
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in objGear.Children)
                    {
                        if (objChild.WeaponBonus != null)
                        {
                            // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                            if (!(objChild.WeaponBonus.InnerXml.Contains("(f)") && Damage.Contains("(f)")))
                            {
                                if (objChild.WeaponBonus["damagetype"] != null)
                                {
                                    strDamageType = string.Empty;
                                    strDamageExtra = objChild.WeaponBonus["damagetype"].InnerText;
                                }
                                // Adjust the Weapon's Damage.
                                if (objChild.WeaponBonus["damage"] != null)
                                    strDamage += " + " + objChild.WeaponBonus["damage"].InnerText;
                                if (objChild.WeaponBonus["damagereplace"] != null)
                                {
                                    blnDamageReplaced = true;
                                    strDamage = objChild.WeaponBonus["damagereplace"].InnerText;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            string strReturn;
            if (!blnDamageReplaced)
            {
                if (string.IsNullOrEmpty(strDamage))
                    strReturn = strDamageType + strDamageExtra;
                else
                {
                    // Replace the division sign with "div" since we're using XPath.
                    strDamage = strDamage.Replace("/", " div ");
                    try
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strDamage, out bool blnIsSuccess);
                        if (blnIsSuccess)
                        {
                            int intDamage = Convert.ToInt32(Math.Ceiling((double) objProcess));
                            intDamage += intBonus;
                            if (Name == "Unarmed Attack (Smashing Blow)")
                                intDamage *= 2;
                            strDamage = intDamage.ToString(objCulture);
                            strReturn = strDamage + strDamageType + strDamageExtra;
                        }
                        else
                        {
                            strReturn = "NaN";
                        }
                    }
                    catch (OverflowException)
                    {
                        strReturn = "NaN";
                    } // Result is text and not a double
                    catch (InvalidCastException)
                    {
                        strReturn = "NaN";
                    } // Result is text and not a double
                }
            }
            else
            {
                string strOriginalDamage = strDamage;
                // Place the Damage Type (P or S) into a string and remove it from the expression.
                if (strDamage.Contains("P or S"))
                {
                    strDamageType = "P or S";
                    strDamage = strDamage.FastEscape("P or S");
                }
                else if (strDamage.Contains('P'))
                {
                    strDamageType = "P";
                    strDamage = strDamage.FastEscape('P');
                }
                else if (strDamage.Contains('S'))
                {
                    strDamageType = "S";
                    strDamage = strDamage.FastEscape('S');
                }
                else if (strDamage.Contains("(M)"))
                {
                    strDamageType = "M";
                    strDamage = strDamage.FastEscape("(M)");
                }
                // Place any extra text like (e) and (f) in a string and remove it from the expression.
                if (strDamage.Contains("(e)"))
                {
                    strDamageExtra = "(e)";
                    strDamage = strDamage.FastEscape("(e)");
                }
                else if (strDamage.Contains("(f)"))
                {
                    strDamageExtra = "(f)";
                    strDamage = strDamage.FastEscape("(f)");
                }

                if (string.IsNullOrEmpty(strDamage))
                    strReturn = strDamageType + strDamageExtra;
                else
                {
                    // Replace the division sign with "div" since we're using XPath.
                    strDamage = strDamage.Replace("/", " div ");
                    try
                    {
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strDamage, out bool blnIsSuccess);
                        if (blnIsSuccess)
                        {
                            int intDamage = Convert.ToInt32(Math.Ceiling((double)objProcess));
                            intDamage += intBonus;
                            if (Name == "Unarmed Attack (Smashing Blow)")
                                intDamage *= 2;
                            strDamage = intDamage.ToString(objCulture);
                            strReturn = strDamage + strDamageType + strDamageExtra;
                        }
                        else
                        {
                            strReturn = strOriginalDamage;
                        }
                    }
                    catch (OverflowException)
                    {
                        strReturn = strOriginalDamage;
                    } // Result is text and not a double
                    catch (InvalidCastException)
                    {
                        strReturn = strOriginalDamage;
                    } // Result is text and not a double
                }
            }

            // If the string couldn't be parsed (resulting in NaN which will happen if it is a special string like "Grenade", "Chemical", etc.), return the Weapon's Damage string.
            if (strReturn.StartsWith("NaN", StringComparison.Ordinal))
                strReturn = Damage;

            // Translate the Damage Code.
            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                strReturn = strReturn.CheapReplace("Special", () => LanguageManager.GetString("String_DamageSpecial", strLanguage))
                    .CheapReplace("P or S", () => LanguageManager.GetString("String_DamagePOrS", strLanguage))
                    .CheapReplace("Chemical", () => LanguageManager.GetString("String_DamageChemical", strLanguage))
                    .CheapReplace("(e)", () => LanguageManager.GetString("String_DamageElectric", strLanguage))
                    .CheapReplace("(f)", () => LanguageManager.GetString("String_DamageFlechette", strLanguage))
                    .CheapReplace("Grenade", () => LanguageManager.GetString("String_DamageGrenade", strLanguage))
                    .CheapReplace("Missile", () => LanguageManager.GetString("String_DamageMissile", strLanguage))
                    .CheapReplace("Mortar", () => LanguageManager.GetString("String_DamageMortar", strLanguage))
                    .CheapReplace("Rocket", () => LanguageManager.GetString("String_DamageRocket", strLanguage))
                    .CheapReplace("Radius", () => LanguageManager.GetString("String_DamageRadius", strLanguage))
                    .CheapReplace("As Drug/Toxin", () => LanguageManager.GetString("String_DamageAsDrugToxin", strLanguage))
                    .CheapReplace("as round", () => LanguageManager.GetString("String_DamageAsRound", strLanguage))
                    .CheapReplace("/m", () => '/' + LanguageManager.GetString("String_DamageMeter", strLanguage))
                    .CheapReplace("(M)", () => LanguageManager.GetString("String_DamageMatrix", strLanguage));
                strReturn = strReturn
                    .CheapReplace("0S", () => '0' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("1S", () => '1' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("2S", () => '2' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("3S", () => '3' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("4S", () => '4' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("5S", () => '5' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("6S", () => '6' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("7S", () => '7' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("8S", () => '8' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("9S", () => '9' + LanguageManager.GetString("String_DamageStun", strLanguage))
                    .CheapReplace("0P", () => '0' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("1P", () => '1' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("2P", () => '2' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("3P", () => '3' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("4P", () => '4' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("5P", () => '5' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("6P", () => '6' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("7P", () => '7' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("8P", () => '8' + LanguageManager.GetString("String_DamagePhysical", strLanguage))
                    .CheapReplace("9P", () => '9' + LanguageManager.GetString("String_DamagePhysical", strLanguage));
            }

            return strReturn;
        }

        /// <summary>
        /// Calculated Ammo capacity in the program's current language.
        /// </summary>
        public string DisplayAmmo => CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Calculated Ammo capacity.
        /// </summary>
        public string CalculatedAmmo(CultureInfo objCulture, string strLanguage)
        {
            string[] strAmmos = Ammo.Split(' ');
            string strReturn = string.Empty;
            int intAmmoBonus = 0;

            int intExtendedMax = 0;
            if (WeaponAccessories.Count != 0)
            {
                if (WeaponAccessories.Any(x => x.Name.Contains("Extended Clip")))
                    intExtendedMax = WeaponAccessories.Where(x => x.Name.Contains("Extended Clip")).Max(x => x.Rating);

                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped)
                    {
                        // Replace the Ammo value.
                        if (!string.IsNullOrEmpty(objAccessory.AmmoReplace))
                        {
                            strAmmos = new[] {objAccessory.AmmoReplace};
                            break;
                        }

                        intAmmoBonus += objAccessory.AmmoBonus;
                    }
                }
            }

            decimal decAmmoBonusPercent = 1.0m;
            if (ParentMount != null)
            {
                foreach (VehicleMod objMod in ParentMount.Mods)
                {
                    if (!string.IsNullOrEmpty(objMod.AmmoReplace))
                    {
                        strAmmos = new[] { objMod.AmmoReplace };
                        break;
                    }
                    intAmmoBonus += objMod.AmmoBonus;
                    if (objMod.AmmoBonusPercent != 0)
                    {
                        decAmmoBonusPercent *= objMod.AmmoBonusPercent / 100.0m;
                    }
                }
            }
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            foreach (string strAmmo in strAmmos)
            {
                string strThisAmmo = strAmmo;
                int intPos = strThisAmmo.IndexOf('(');
                if (intPos != -1)
                {
                    string strPrepend = string.Empty;
                    strThisAmmo = strThisAmmo.Substring(0, intPos);
                    intPos = strThisAmmo.IndexOf('x');
                    if (intPos != -1)
                    {
                        strPrepend = strThisAmmo.Substring(0, intPos + 1);
                        strThisAmmo = strThisAmmo.Substring(intPos + 1, strThisAmmo.Length - (intPos + 1));
                    }
                    if (WeaponAccessories.Count != 0)
                    {
                        foreach (WeaponAccessory objAccessory in WeaponAccessories)
                        {
                            if (objAccessory.Equipped)
                            {
                                string strModifyAmmoCapacity = objAccessory.ModifyAmmoCapacity;
                                if (!string.IsNullOrEmpty(strModifyAmmoCapacity))
                                {
                                    strThisAmmo = '(' + strThisAmmo + strModifyAmmoCapacity + ')';
                                    int AddParenthesesCount = strModifyAmmoCapacity.Count(x => x == ')') - strModifyAmmoCapacity.Count(x => x == '(');
                                    for (int i = 0; i < AddParenthesesCount; ++i)
                                        strThisAmmo = '(' + strThisAmmo;
                                    for (int i = 0; i < -AddParenthesesCount; ++i)
                                        strThisAmmo += ')';
                                }
                            }
                        }
                    }
                    strThisAmmo = strThisAmmo.CheapReplace("Weapon", () => AmmoCapacity(Ammo));
                    // Replace the division sign with "div" since we're using XPath.
                    strThisAmmo = strThisAmmo.Replace("/", " div ");
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strThisAmmo, out bool blnIsSuccess);
                    if (blnIsSuccess)
                    {
                        int intAmmo = Convert.ToInt32(Math.Ceiling((double)objProcess));

                        intAmmo += (intAmmo * intAmmoBonus + 99) / 100;

                        if (intExtendedMax > 0 && strAmmo.Contains("(c)"))
                        {
                            //Multiply by 2-4 and divide by 2 to get 1, 1.5 or 2 times original result
                            intAmmo = intAmmo * (2 + intExtendedMax) / 2;
                        }

                        if (decAmmoBonusPercent != 1.0m)
                        {
                            intAmmo = decimal.ToInt32(decimal.Ceiling(intAmmo * decAmmoBonusPercent));
                        }

                        strThisAmmo = intAmmo.ToString(objCulture) + strAmmo.Substring(strAmmo.IndexOf('('), strAmmo.Length - strAmmo.IndexOf('('));
                    }

                    if (!string.IsNullOrEmpty(strPrepend))
                        strThisAmmo = strPrepend + strThisAmmo;
                }
                strReturn += strThisAmmo + strSpace;
            }
            strReturn = strReturn.Trim();

            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                // Translate the Ammo string.
                strReturn = strReturn.CheapReplace(" or ", () => strSpace + LanguageManager.GetString("String_Or", strLanguage) + strSpace)
                    .CheapReplace(" belt", () => LanguageManager.GetString("String_AmmoBelt", strLanguage))
                    .CheapReplace(" Energy", () => LanguageManager.GetString("String_AmmoEnergy", strLanguage))
                    .CheapReplace(" external source", () => LanguageManager.GetString("String_AmmoExternalSource", strLanguage))
                    .CheapReplace(" Special", () => LanguageManager.GetString("String_AmmoSpecial", strLanguage))
                    .CheapReplace("(b)", () => '(' + LanguageManager.GetString("String_AmmoBreakAction", strLanguage) + ')')
                    .CheapReplace("(belt)", () => '(' + LanguageManager.GetString("String_AmmoBelt", strLanguage) + ')')
                    .CheapReplace("(box)", () => '(' + LanguageManager.GetString("String_AmmoBox", strLanguage) + ')')
                    .CheapReplace("(c)", () => '(' + LanguageManager.GetString("String_AmmoClip", strLanguage) + ')')
                    .CheapReplace("(cy)", () => '(' + LanguageManager.GetString("String_AmmoCylinder", strLanguage) + ')')
                    .CheapReplace("(d)", () => '(' + LanguageManager.GetString("String_AmmoDrum", strLanguage) + ')')
                    .CheapReplace("(m)", () => '(' + LanguageManager.GetString("String_AmmoMagazine", strLanguage) + ')')
                    .CheapReplace("(ml)", () => '(' + LanguageManager.GetString("String_AmmoMuzzleLoad", strLanguage) + ')');
            }

            return strReturn;
        }

        public bool AllowSingleShot =>
            WeaponType == "Melee" && Ammo != "0" || _blnAllowSingleShot && (AllowMode(LanguageManager.GetString("String_ModeSingleShot")) ||
                                                                            AllowMode(LanguageManager.GetString("String_ModeSemiAutomatic")));

        public bool AllowShortBurst => _blnAllowShortBurst && (
                                           AllowMode(LanguageManager.GetString("String_ModeBurstFire")) ||
                                           AllowMode(LanguageManager.GetString("String_ModeSemiAutomatic")) ||
                                           AllowMode(LanguageManager.GetString("String_ModeFullAutomatic")));

        public bool AllowLongBurst => _blnAllowLongBurst && (
                                          AllowMode(LanguageManager.GetString("String_ModeBurstFire")) ||
                                          AllowMode(LanguageManager.GetString("String_ModeFullAutomatic")));

        public bool AllowFullBurst =>
            _blnAllowFullBurst && AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));

        public bool AllowSuppressive =>
            _blnAllowSuppressive && AllowMode(LanguageManager.GetString("String_ModeFullAutomatic"));

        /// <summary>
        /// The Weapon's Firing Mode including Modifications in the program's current language.
        /// </summary>
        public string DisplayMode => CalculatedMode(GlobalOptions.Language);

        /// <summary>
        /// The Weapon's Firing Mode including Modifications.
        /// </summary>
        public string CalculatedMode(string strLanguage)
        {
            string[] strModes = _strMode.Split('/');
            // Move the contents of the array to a list so it's easier to work with.
            List<string> lstModes = strModes.ToList();

            // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
            if (!string.IsNullOrEmpty(AmmoLoaded))
            {
                // Look for Ammo on the character.
                Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded) ?? _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);
                if (objGear != null)
                {
                    if (objGear.WeaponBonus != null)
                    {
                        string strFireMode = objGear.WeaponBonus["firemode"]?.InnerText;
                        if (!string.IsNullOrEmpty(strFireMode))
                        {
                            if (strFireMode.Contains('/'))
                            {
                                strModes = strFireMode.Split('/');

                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(strFireMode);
                            }
                        }
                        strFireMode = objGear.WeaponBonus["modereplace"]?.InnerText;
                        if (!string.IsNullOrEmpty(strFireMode))
                        {
                            lstModes.Clear();
                            if (strFireMode.Contains('/'))
                            {
                                strModes = strFireMode.Split('/');
                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(strFireMode);
                            }
                        }
                    }

                    // Do the same for any plugins.
                    foreach (Gear objChild in objGear.Children.GetAllDescendants(x => x.Children))
                    {
                        if (objChild.WeaponBonus == null) continue;
                        string strFireMode = objChild.WeaponBonus["firemode"]?.InnerText;
                        if (!string.IsNullOrEmpty(strFireMode))
                        {
                            if (strFireMode.Contains('/'))
                            {
                                strModes = strFireMode.Split('/');

                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(strFireMode);
                            }
                        }
                        strFireMode = objChild.WeaponBonus["modereplace"]?.InnerText;
                        if (!string.IsNullOrEmpty(strFireMode))
                        {
                            lstModes.Clear();
                            if (strFireMode.Contains('/'))
                            {
                                strModes = strFireMode.Split('/');
                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(strFireMode);
                            }
                            break;
                        }
                    }

                    // Do the same for any accessories/modifications.
                    foreach (WeaponAccessory objAccessory in WeaponAccessories)
                    {
                        if (!objAccessory.Equipped) continue;
                        if (!string.IsNullOrEmpty(objAccessory.FireMode))
                        {
                            if (objAccessory.FireMode.Contains('/'))
                            {
                                strModes = objAccessory.FireMode.Split('/');

                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(objAccessory.FireMode);
                            }
                        }

                        if (!string.IsNullOrEmpty(objAccessory.FireModeReplacement))
                        {
                            lstModes.Clear();
                            if (objAccessory.FireModeReplacement.Contains('/'))
                            {
                                strModes = objAccessory.FireModeReplacement.Split('/');

                                // Move the contents of the array to a list so it's easier to work with.
                                foreach (string strMode in strModes)
                                    lstModes.Add(strMode);
                            }
                            else
                            {
                                lstModes.Add(objAccessory.FireModeReplacement);
                            }

                            break;
                        }
                    }
                }
            }

            lstModes.AddRange(from objAccessory in WeaponAccessories where objAccessory.Equipped && !string.IsNullOrEmpty(objAccessory.AddMode) select objAccessory.AddMode);

            string strReturn = string.Empty;
            if (lstModes.Contains("SS"))
                strReturn += LanguageManager.GetString("String_ModeSingleShot", strLanguage) + "/";
            if (lstModes.Contains("SA"))
                strReturn += LanguageManager.GetString("String_ModeSemiAutomatic", strLanguage) + "/";
            if (lstModes.Contains("BF"))
                strReturn += LanguageManager.GetString("String_ModeBurstFire", strLanguage) + "/";
            if (lstModes.Contains("FA"))
                strReturn += LanguageManager.GetString("String_ModeFullAutomatic", strLanguage) + "/";
            if (lstModes.Contains("Special"))
                strReturn += LanguageManager.GetString("String_ModeSpecial", strLanguage) + "/";

            // Remove the trailing "/".
            if (!string.IsNullOrEmpty(strReturn))
                strReturn = strReturn.Substring(0, strReturn.Length - 1);

            return strReturn;
        }

        /// <summary>
        /// Determine if the Weapon is capable of firing in a particular mode.
        /// </summary>
        /// <param name="strFindMode">Firing mode to find.</param>
        public bool AllowMode(string strFindMode)
        {
            string[] strModes = CalculatedMode(GlobalOptions.Language).Split('/');
            return strModes.Any(strMode => strMode == strFindMode);
        }

        /// <summary>
        /// Weapon Cost to use when working with Total Cost price modifiers for Weapon Mods.
        /// </summary>
        public decimal MultipliableCost(WeaponAccessory objExcludeAccessory)
        {
            decimal decReturn = OwnCost;

            // Run through the list of Weapon Mods.
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                if (objExcludeAccessory != objAccessory && objAccessory.Equipped && !objAccessory.IncludedInWeapon)
                {
                    decReturn += objAccessory.TotalCost;
                }
            }

            return decReturn;
        }

        public string AccessoryMounts
        {
            get
            {
                string[] astrPossibleMounts = ModificationSlots.Split('/');

                if (astrPossibleMounts.Length <= 0)
                    return string.Empty;

                StringBuilder strMounts = new StringBuilder();
                foreach (string strMount in astrPossibleMounts)
                {
                    if (WeaponAccessories.All(objAccessory => !objAccessory.Equipped || objAccessory.Mount != strMount && objAccessory.ExtraMount != strMount) && UnderbarrelWeapons.All(weapon => !weapon.Equipped || weapon.Mount != strMount && weapon.ExtraMount != strMount))
                    {
                        strMounts.Append(strMount);
                        strMounts.Append('/');
                    }
                }

                strMounts.Append("Internal/None");

                return strMounts.ToString();
            }
        }

        /// <summary>
        /// The Weapon's total cost including Accessories and Modifications.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decReturn = OwnCost;

                // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
                decReturn += WeaponAccessories.AsParallel().Sum(objAccessory => objAccessory.TotalCost);

                // Include the cost of any Underbarrel Weapon.
                if (Children.Count > 0)
                {
                    decReturn += Children.AsParallel().Sum(objUnderbarrel => objUnderbarrel.TotalCost);
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Weapon itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                // If this is a Cyberware or Gear Weapon, remove the Weapon Cost from this since it has already been paid for through the parent item (but is needed to calculate Mod price).
                if (Cyberware || Category == "Gear")
                    return 0;
                string strCostExpression = Cost;

                StringBuilder objCost = new StringBuilder(strCostExpression.TrimStart('+'));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }
                objCost.CheapReplace("{Rating}", () => Rating.ToString(GlobalOptions.InvariantCultureInfo));

                // Replace the division sign with "div" since we're using XPath.
                objCost.Replace("/", " div ");
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                decimal decReturn = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;

                if (DiscountCost)
                    decReturn *= 0.9m;

                if (!string.IsNullOrEmpty(Parent?.DoubledCostModificationSlots))
                {
                    string[] astrParentDoubledCostModificationSlots = Parent.DoubledCostModificationSlots.Split('/');
                    if (astrParentDoubledCostModificationSlots.Contains(Mount) || astrParentDoubledCostModificationSlots.Contains(ExtraMount))
                    {
                        decReturn *= 2;
                    }
                }

                return decReturn;
            }
        }

        /// <summary>
        /// The Weapon's total AP including Ammunition in the program's current language.
        /// </summary>
        public string DisplayTotalAP => TotalAP(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// The Weapon's total AP including Ammunition.
        /// </summary>
        public string TotalAP(CultureInfo objCulture, string strLanguage)
        {
            string strAP = AP;

            int bonusAP = 0;
            // Check if the Weapon has Ammunition loaded and look for any Damage bonus/replacement.
            if (!string.IsNullOrEmpty(AmmoLoaded))
            {
                // Look for Ammo on the character.
                Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded) ?? _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);
                if (objGear?.WeaponBonus != null)
                {
                    // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                    if (!(objGear.WeaponBonus.InnerXml.Contains("(f)") && Damage.Contains("(f)")))
                    {
                        // Armor-Piercing Flechettes (and any other that might come along that does not explicitly add +5 AP) should instead reduce
                        // the AP for Flechette-only Weapons which have the standard Flechette +5 AP built into their stats.
                        if (Damage.Contains("(f)") && objGear.Name.Contains("Flechette"))
                        {
                            bonusAP -= 5;
                        }
                        else
                        {
                            // Change the Weapon's Damage Type.
                            string strAPReplace = objGear.WeaponBonus["apreplace"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPReplace))
                                strAP = strAPReplace;
                            // Adjust the Weapon's Damage.
                            string strAPAdd = objGear.WeaponBonus["ap"]?.InnerText;
                            if (!string.IsNullOrEmpty(strAPAdd))
                                bonusAP += Convert.ToInt32(strAPAdd, GlobalOptions.InvariantCultureInfo);
                        }
                    }
                }

                if (_objCharacter != null)
                {
                    // Add any UnarmedAP bonus for the Unarmed Attack item.
                    if (Name == "Unarmed Attack" || Skill?.Name == "Unarmed Combat" &&
                        _objCharacter.Options.UnarmedImprovementsApplyToWeapons)
                    {
                        bonusAP += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.UnarmedAP);
                    }
                }
            }

            foreach (WeaponAccessory objAccessory in WeaponAccessories.Where(objAccessory => objAccessory.Equipped))
            {
                // Change the Weapon's Damage Type. (flechette rounds cannot affect weapons that have flechette included in their damage)
                if (!(objAccessory.DamageType.Contains("(f)") && Damage.Contains("(f)")))
                {
                    // Armor-Piercing Flechettes (and any other that might come along that does not explicitly add +5 AP) should instead reduce
                    // the AP for Flechette-only Weapons which have the standard Flechette +5 AP built into their stats.
                    if (Damage.Contains("(f)") && objAccessory.Name.Contains("Flechette"))
                    {
                        bonusAP -= 5;
                    }
                    else
                    {
                        // Change the Weapon's AP value.
                        if (!string.IsNullOrEmpty(objAccessory.APReplacement))
                            strAP = objAccessory.APReplacement;
                        // Adjust the Weapon's AP value.
                        if (!string.IsNullOrEmpty(objAccessory.AP))
                            bonusAP += Convert.ToInt32(objAccessory.AP, GlobalOptions.InvariantCultureInfo);
                    }
                }
            }

            if (strAP == "-")
                strAP = "0";

            StringBuilder objAP = new StringBuilder(strAP);
            objAP.CheapReplace("{Rating}", () => Rating.ToString(GlobalOptions.InvariantCultureInfo));
            int intUseSTR = 0;
            int intUseAGI = 0;
            int intUseSTRBase = 0;
            int intUseAGIBase = 0;
            if (strAP.Contains("{STR") || strAP.Contains("{AGI"))
            {
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        intUseSTRBase = intUseSTR;
                        intUseAGI = ParentVehicle.Pilot;
                        intUseAGIBase = intUseAGI;
                        if (!string.IsNullOrEmpty(ParentID))
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID, out VehicleMod objVehicleMod);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                int intSTR = objAttributeSource.TotalStrength;
                                int intSTRBase = objAttributeSource.BaseStrength;
                                int intAGI = objAttributeSource.TotalAgility;
                                int intAGIBase = objAttributeSource.BaseAgility;
                                while (objAttributeSource != null)
                                {
                                    if (intSTR != 0 || intAGI != 0 || intSTRBase != 0 || intAGIBase != 0)
                                        break;
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null) continue;
                                    intSTR = objAttributeSource.TotalStrength;
                                    intSTRBase = objAttributeSource.BaseStrength;
                                    intAGI = objAttributeSource.TotalAgility;
                                    intAGIBase = objAttributeSource.BaseAgility;
                                }

                                intUseSTR = intSTR;
                                intUseAGI = intAGI;
                                intUseSTRBase = intSTRBase;
                                intUseAGIBase = intAGIBase;

                                if (intUseSTR == 0)
                                    intUseSTR = objVehicleMod.TotalStrength;
                                if (intUseAGI == 0)
                                    intUseAGI = objVehicleMod.TotalAgility;
                                if (intUseSTRBase == 0)
                                    intUseSTRBase = ParentVehicle.TotalBody;
                                if (intUseAGIBase == 0)
                                    intUseAGIBase = ParentVehicle.Pilot;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intSTRBase = objAttributeSource.BaseStrength;
                            int intAGI = objAttributeSource.TotalAgility;
                            int intAGIBase = objAttributeSource.BaseAgility;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0 || intSTRBase != 0 || intAGIBase != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intSTRBase = objAttributeSource.BaseStrength;
                                intAGI = objAttributeSource.TotalAgility;
                                intAGIBase = objAttributeSource.BaseAgility;
                            }

                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                            intUseSTRBase = intSTRBase;
                            intUseAGIBase = intAGIBase;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                        if (intUseAGI == 0)
                            intUseAGI = _objCharacter.AGI.TotalValue;
                        if (intUseSTRBase == 0)
                            intUseSTRBase = _objCharacter.STR.TotalBase;
                        if (intUseAGIBase == 0)
                            intUseAGIBase = _objCharacter.AGI.TotalBase;
                    }
                }
                else if (ParentVehicle == null)
                {
                    intUseSTR = _objCharacter.STR.TotalValue;
                    intUseAGI = _objCharacter.AGI.TotalValue;
                }

                if (Category == "Throwing Weapons" || UseSkill == "Throwing Weapons")
                    intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);
            }

            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                CharacterAttrib objLoopAttribute = _objCharacter.GetAttribute(strAttribute);
                if (strAttribute == "STR")
                {
                    objAP.Replace("{" + strAttribute + "}", intUseSTR.ToString(GlobalOptions.InvariantCultureInfo));
                    objAP.Replace("{" + strAttribute + "Base}", intUseSTRBase.ToString(GlobalOptions.InvariantCultureInfo));
                }
                else if (strAttribute == "AGI")
                {
                    objAP.Replace("{" + strAttribute + "}", intUseAGI.ToString(GlobalOptions.InvariantCultureInfo));
                    objAP.Replace("{" + strAttribute + "Base}", intUseAGIBase.ToString(GlobalOptions.InvariantCultureInfo));
                }
                else
                {
                    objAP.CheapReplace(strAP, "{" + strAttribute + "}", () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objAP.CheapReplace(strAP, "{" + strAttribute + "Base}", () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }
            }

            int intAP;
            try
            {
                // Replace the division sign with "div" since we're using XPath.
                objAP.Replace("/", " div ");
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAP.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAP = Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo);
                else
                    return strLanguage == GlobalOptions.DefaultLanguage ? strAP : strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage));
            }
            catch (FormatException)
            {
                // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                return strLanguage == GlobalOptions.DefaultLanguage ? strAP : strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage));
            }
            catch (OverflowException)
            {
                // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                return strLanguage == GlobalOptions.DefaultLanguage ? strAP : strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage));
            }
            catch (InvalidCastException)
            {
                // If AP is not numeric (for example "-half"), do do anything and just return the weapon's AP.
                return strLanguage == GlobalOptions.DefaultLanguage ? strAP : strAP.CheapReplace("-half", () => LanguageManager.GetString("String_APHalf", strLanguage));
            }
            intAP += bonusAP;
            if (intAP == 0)
                return "-";
            if (intAP > 0)
                return '+' + intAP.ToString(objCulture);
            return intAP.ToString(objCulture);
        }

        public string DisplayTotalRC => TotalRC(GlobalOptions.CultureInfo, GlobalOptions.Language, true);

        /// <summary>
        /// The Weapon's total RC including Accessories and Modifications.
        /// </summary>
        public string TotalRC(CultureInfo objCulture, string strLanguage, bool blnRefreshRCToolTip = false)
        {
            string strSpace = LanguageManager.GetString("String_Space", strLanguage);
            string strRCBase = "0";
            string strRCFull;
            string strRC = RC;

            List<Tuple<string, int>> lstRCGroups = new List<Tuple<string, int>>(5);
            List<Tuple<string, int>> lstRCDeployGroups = new List<Tuple<string, int>>(5);
            strRC = strRC.CheapReplace("{Rating}", () => Rating.ToString(GlobalOptions.InvariantCultureInfo));
            int intPos = strRC.IndexOf('(');
            if (intPos != -1)
            {
                if (intPos == 0)
                {
                    // The string contains only RC from pieces that can be removed - "(x)" only.
                    strRCFull = strRC;
                }
                // The string contains a mix of both fixed and removable RC. "x(y)".
                else
                {
                    strRCBase = strRC.Substring(0, intPos);
                    strRCFull = strRC.Substring(intPos, strRC.Length - intPos);
                }
            }
            else
            {
                // The string contains only RC from fixed pieces - "x" only.
                strRCBase = strRC;
                strRCFull = strRC;
            }

            string strRCTip = "1" + strSpace;
            if (blnRefreshRCToolTip && strRCBase != "0")
            {
                strRCTip += '+' + strSpace + LanguageManager.GetString("Label_Base", strLanguage) + '(' + strRCBase + ')';
            }

            int.TryParse(strRCBase, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intRCBase);
            int.TryParse(strRCFull.Trim('(', ')'), NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intRCFull);

            // Check if the Weapon has Ammunition loaded and look for any Recoil bonus.
            if (!string.IsNullOrEmpty(AmmoLoaded) && AmmoLoaded != "00000000-0000-0000-0000-000000000000")
            {
                Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded) ?? _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);

                // Change the Weapon's Damage Type.
                string strRCBonus = objGear?.WeaponBonus?["rc"]?.InnerText;
                if (!string.IsNullOrEmpty(strRCBonus) && int.TryParse(strRCBonus, out int intLoopRCBonus))
                {
                    intRCBase += intLoopRCBonus;
                    intRCFull += intLoopRCBonus;

                    if (blnRefreshRCToolTip)
                        strRCTip += strSpace + '+' + strSpace + objGear.DisplayName(objCulture, strLanguage) + strSpace + '(' + strRCBonus + ')';
                }
            }

            // Now that we know the Weapon's RC values, run through all of the Accessories and add theirs to the mix.
            // Only add in the values for items that do not come with the weapon.
            foreach (WeaponAccessory objAccessory in WeaponAccessories.Where(objAccessory => !string.IsNullOrEmpty(objAccessory.RC) && objAccessory.Equipped))
            {
                if (_objCharacter.Options.RestrictRecoil && objAccessory.RCGroup != 0)
                {
                    int intItemRC = Convert.ToInt32(objAccessory.RC, GlobalOptions.InvariantCultureInfo);
                    List<Tuple<string, int>> lstLoopRCGroup = lstRCGroups;
                    if (objAccessory.RCDeployable)
                    {
                        lstLoopRCGroup = lstRCDeployGroups;
                    }
                    while (lstLoopRCGroup.Count < objAccessory.RCGroup)
                    {
                        lstLoopRCGroup.Add(new Tuple<string, int>(string.Empty, 0));
                    }
                    if (lstLoopRCGroup[objAccessory.RCGroup - 1].Item2 < intItemRC)
                    {
                        lstLoopRCGroup[objAccessory.RCGroup - 1] = new Tuple<string, int>(objAccessory.DisplayName(strLanguage), intItemRC);
                    }
                    if (objAccessory.RCDeployable)
                    {
                        lstRCDeployGroups = lstLoopRCGroup;
                    }
                    else
                    {
                        lstRCGroups = lstLoopRCGroup;
                    }
                }
                else if (!string.IsNullOrEmpty(objAccessory.RC) && int.TryParse(objAccessory.RC, out int intLoopRCBonus))
                {
                    intRCFull += intLoopRCBonus;
                    if (!objAccessory.RCDeployable)
                    {
                        intRCBase += intLoopRCBonus;
                    }
                    if (blnRefreshRCToolTip)
                        strRCTip += strSpace + '+' + strSpace + objAccessory.DisplayName(strLanguage) + strSpace + '(' + objAccessory.RC + ')';
                }
            }

            foreach (Tuple<string, int> objRCGroup in lstRCGroups)
            {
                if (!string.IsNullOrEmpty(objRCGroup.Item1))
                {
                    // Add in the Recoil Group bonuses.
                    intRCBase += objRCGroup.Item2;
                    intRCFull += objRCGroup.Item2;
                    if (blnRefreshRCToolTip)
                        strRCTip += strSpace + '+' + strSpace + objRCGroup.Item1 + strSpace + '(' + objRCGroup.Item2.ToString(objCulture) + ')';
                }
            }

            foreach (Tuple<string, int> objRCGroup in lstRCDeployGroups)
            {
                if (!string.IsNullOrEmpty(objRCGroup.Item1))
                {
                    // Add in the Recoil Group bonuses.
                    intRCFull += objRCGroup.Item2;
                    if (blnRefreshRCToolTip)
                        strRCTip += strSpace + '+' + strSpace
                                    + string.Format(objCulture, LanguageManager.GetString("Tip_RecoilAccessories", strLanguage), objRCGroup.Item1, objRCGroup.Item2.ToString(objCulture));
                }
            }

            int intUseSTR = 0;
            if (Cyberware)
            {
                if (ParentVehicle != null)
                {
                    intUseSTR = ParentVehicle.TotalBody;
                    if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID, out VehicleMod objVehicleMod);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intAGI = objAttributeSource.TotalStrength;
                            }

                            intUseSTR = intSTR;

                            if (intUseSTR == 0)
                                intUseSTR = objVehicleMod.TotalStrength;
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(ParentID))
                {
                    // Look to see if this is attached to a Cyberlimb and use its STR instead.
                    Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                    if (objWeaponParent != null)
                    {
                        Cyberware objAttributeSource = objWeaponParent;
                        int intSTR = objAttributeSource.TotalStrength;
                        int intAGI = objAttributeSource.TotalStrength;
                        while (objAttributeSource != null)
                        {
                            if (intSTR != 0 || intAGI != 0)
                                break;
                            objAttributeSource = objAttributeSource.Parent;
                            if (objAttributeSource == null) continue;
                            intSTR = objAttributeSource.TotalStrength;
                        }

                        intUseSTR = intSTR;
                    }
                    if (intUseSTR == 0)
                        intUseSTR = _objCharacter.STR.TotalValue;
                }
            }
            else if (ParentVehicle == null)
            {
                intUseSTR = _objCharacter.STR.TotalValue;
            }

            if (Category == "Throwing Weapons" || UseSkill == "Throwing Weapons")
                intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);

            int intStrRC = (intUseSTR + 2) / 3;

            intRCBase += intStrRC + 1;
            intRCFull += intStrRC + 1;
            if (blnRefreshRCToolTip)
                strRCTip += strSpace + '+' + strSpace + _objCharacter.STR.GetDisplayAbbrev(strLanguage) + strSpace
                    + '[' + intUseSTR.ToString(objCulture) + strSpace + '/' + strSpace + 3.ToString(objCulture)
                    + strSpace + '=' + strSpace + intStrRC.ToString(objCulture) + ']';
            // If the full RC is not higher than the base, only the base value is shown.
            strRC = intRCBase.ToString(objCulture);
            if (intRCFull > intRCBase)
            {
                strRC += strSpace + '(' + intRCFull.ToString(objCulture) + ')';
            }

            if (blnRefreshRCToolTip)
                _strRCTip = strRCTip;

            return strRC;
        }

        /// <summary>
        /// The tooltip showing the sources of RC bonuses
        /// </summary>
        public string RCToolTip => _strRCTip;

        /// <summary>
        /// The full Reach of the Weapons including the Character's Reach.
        /// </summary>
        public int TotalReach
        {
            get
            {
                int intReach = Reach;
                if (WeaponType == "Melee")
                {
                    // Run through the Character's Improvements and add any Reach Improvements.
                    intReach += _objCharacter.Improvements
                        .Where(objImprovement =>
                            objImprovement.ImproveType == Improvement.ImprovementType.Reach &&
                            (objImprovement.ImprovedName == Name || string.IsNullOrEmpty(objImprovement.ImprovedName)) &&
                            objImprovement.Enabled)
                        .Sum(objImprovement => objImprovement.Value);
                }

                if (Name == "Unarmed Attack" || Skill?.Name == "Unarmed Combat" &&
                    _objCharacter.Options.UnarmedImprovementsApplyToWeapons)
                {
                    intReach += _objCharacter.Improvements
                        .Where(objImprovement =>
                            objImprovement.ImproveType == Improvement.ImprovementType.UnarmedReach &&
                            objImprovement.Enabled).Sum(objImprovement => objImprovement.Value);
                }

                return intReach;
            }
        }

        /// <summary>
        /// The full Accuracy of the Weapon including modifiers from accessories.
        /// </summary>
        public int TotalAccuracy
        {
            get
            {
                string strAccuracy = Accuracy;
                StringBuilder objAccuracy = new StringBuilder(strAccuracy);
                int intAccuracy = 0;
                objAccuracy.CheapReplace("{Rating}", () => Rating.ToString(GlobalOptions.InvariantCultureInfo));
                int intUseSTR = 0;
                int intUseAGI = 0;
                int intUseSTRBase = 0;
                int intUseAGIBase = 0;
                if (strAccuracy.Contains("{STR") || strAccuracy.Contains("{AGI"))
                {
                    if (Cyberware)
                    {
                        if (ParentVehicle != null)
                        {
                            intUseSTR = ParentVehicle.TotalBody;
                            intUseSTRBase = intUseSTR;
                            intUseAGI = ParentVehicle.Pilot;
                            intUseAGIBase = intUseAGI;
                            if (!string.IsNullOrEmpty(ParentID))
                            {
                                // Look to see if this is attached to a Cyberlimb and use its STR instead.
                                Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID, out VehicleMod objVehicleMod);
                                if (objWeaponParent != null)
                                {
                                    Cyberware objAttributeSource = objWeaponParent;
                                    int intSTR = objAttributeSource.TotalStrength;
                                    int intSTRBase = objAttributeSource.BaseStrength;
                                    int intAGI = objAttributeSource.TotalAgility;
                                    int intAGIBase = objAttributeSource.BaseAgility;
                                    while (objAttributeSource != null)
                                    {
                                        if (intSTR != 0 || intAGI != 0 || intSTRBase != 0 || intAGIBase != 0)
                                            break;
                                        objAttributeSource = objAttributeSource.Parent;
                                        if (objAttributeSource == null) continue;
                                        intSTR = objAttributeSource.TotalStrength;
                                        intSTRBase = objAttributeSource.BaseStrength;
                                        intAGI = objAttributeSource.TotalAgility;
                                        intAGIBase = objAttributeSource.BaseAgility;
                                    }

                                    intUseSTR = intSTR;
                                    intUseAGI = intAGI;
                                    intUseSTRBase = intSTRBase;
                                    intUseAGIBase = intAGIBase;

                                    if (intUseSTR == 0)
                                        intUseSTR = objVehicleMod.TotalStrength;
                                    if (intUseAGI == 0)
                                        intUseAGI = objVehicleMod.TotalAgility;
                                    if (intUseSTRBase == 0)
                                        intUseSTRBase = ParentVehicle.TotalBody;
                                    if (intUseAGIBase == 0)
                                        intUseAGIBase = ParentVehicle.Pilot;
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(ParentID))
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                int intSTR = objAttributeSource.TotalStrength;
                                int intSTRBase = objAttributeSource.BaseStrength;
                                int intAGI = objAttributeSource.TotalAgility;
                                int intAGIBase = objAttributeSource.BaseAgility;
                                while (objAttributeSource != null)
                                {
                                    if (intSTR != 0 || intAGI != 0 || intSTRBase != 0 || intAGIBase != 0)
                                        break;
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null) continue;
                                    intSTR = objAttributeSource.TotalStrength;
                                    intSTRBase = objAttributeSource.BaseStrength;
                                    intAGI = objAttributeSource.TotalAgility;
                                    intAGIBase = objAttributeSource.BaseAgility;
                                }

                                intUseSTR = intSTR;
                                intUseAGI = intAGI;
                                intUseSTRBase = intSTRBase;
                                intUseAGIBase = intAGIBase;
                            }
                            if (intUseSTR == 0)
                                intUseSTR = _objCharacter.STR.TotalValue;
                            if (intUseAGI == 0)
                                intUseAGI = _objCharacter.AGI.TotalValue;
                            if (intUseSTRBase == 0)
                                intUseSTRBase = _objCharacter.STR.TotalBase;
                            if (intUseAGIBase == 0)
                                intUseAGIBase = _objCharacter.AGI.TotalBase;
                        }
                    }
                    else if (ParentVehicle == null)
                    {
                        intUseSTR = _objCharacter.STR.TotalValue;
                        intUseAGI = _objCharacter.AGI.TotalValue;
                    }

                    if (Category == "Throwing Weapons" || UseSkill == "Throwing Weapons")
                        intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);
                }

                Func<string> funcPhysicalLimitString = () => _objCharacter.LimitPhysical.ToString(GlobalOptions.InvariantCultureInfo);
                if (ParentVehicle != null)
                {
                    funcPhysicalLimitString = () =>
                    {
                        string strHandling = ParentVehicle.TotalHandling;
                        int intSlashIndex = strHandling.IndexOf('/');
                        if (intSlashIndex != -1)
                            strHandling = strHandling.Substring(0, intSlashIndex);
                        return strHandling;
                    };
                }
                objAccuracy.CheapReplace(strAccuracy, "Physical", funcPhysicalLimitString);
                objAccuracy.CheapReplace(strAccuracy, "Missile", funcPhysicalLimitString);
                foreach (string strAttribute in AttributeSection.AttributeStrings)
                {
                    CharacterAttrib objLoopAttribute = _objCharacter.GetAttribute(strAttribute);
                    if (strAttribute == "STR")
                    {
                        objAccuracy.Replace("{" + strAttribute + "}", intUseSTR.ToString(GlobalOptions.InvariantCultureInfo));
                        objAccuracy.Replace("{" + strAttribute + "Base}", intUseSTRBase.ToString(GlobalOptions.InvariantCultureInfo));
                    }
                    else if (strAttribute == "AGI")
                    {
                        objAccuracy.Replace("{" + strAttribute + "}", intUseAGI.ToString(GlobalOptions.InvariantCultureInfo));
                        objAccuracy.Replace("{" + strAttribute + "Base}", intUseAGIBase.ToString(GlobalOptions.InvariantCultureInfo));
                    }
                    else
                    {
                        objAccuracy.CheapReplace(strAccuracy, "{" + strAttribute + "}", () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                        objAccuracy.CheapReplace(strAccuracy, "{" + strAttribute + "Base}", () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                    }
                }

                // Replace the division sign with "div" since we're using XPath.
                objAccuracy.Replace("/", " div ");
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAccuracy.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAccuracy = Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo);

                int intBonusAccuracyFromAccessories = 0;
                int intBonusAccuracyFromNonStackingAccessories = 0;
                foreach (WeaponAccessory objWeaponAccessory in WeaponAccessories)
                {
                    if (objWeaponAccessory.Equipped)
                    {
                        int intLoopAccuracy = objWeaponAccessory.Accuracy;
                        if (intLoopAccuracy != 0)
                        {
                            if (!objWeaponAccessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && !objWeaponAccessory.Name.Contains("Sight"))
                                intBonusAccuracyFromAccessories += intLoopAccuracy;
                            else if (intLoopAccuracy > intBonusAccuracyFromNonStackingAccessories)
                                intBonusAccuracyFromNonStackingAccessories = intLoopAccuracy;
                        }
                    }
                }

                intAccuracy += intBonusAccuracyFromAccessories + intBonusAccuracyFromNonStackingAccessories;

                string strNameUpper = Name.ToUpperInvariant();

                foreach (Improvement objImprovement in _objCharacter.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.WeaponAccuracy && objImprovement.Enabled)
                    {
                        string strImprovedName = objImprovement.ImprovedName;
                        if (string.IsNullOrEmpty(strImprovedName) || strImprovedName == Name
                            || strImprovedName.StartsWith("[contains]", StringComparison.Ordinal)
                            && strNameUpper.Contains(strImprovedName.TrimStartOnce("[contains]", true).ToUpperInvariant()))
                        {
                            intAccuracy += objImprovement.Value;
                        }
                    }
                }

                string strSkill = UseSkill;
                string strSpec = Spec;
                // Use the Skill defined by the Weapon if one is present.
                if (string.IsNullOrEmpty(strSkill))
                {
                    // Exotic Skills require a matching Specialization.
                    switch (Category)
                    {
                        case "Bows":
                        case "Crossbows":
                            strSkill = "Archery";
                            break;
                        case "Assault Rifles":
                        case "Carbines":
                        case "Machine Pistols":
                        case "Submachine Guns":
                            strSkill = "Automatics";
                            break;
                        case "Blades":
                            strSkill = "Blades";
                            break;
                        case "Clubs":
                        case "Improvised Weapons":
                            strSkill = "Clubs";
                            break;
                        case "Exotic Melee Weapons":
                            strSkill = "Exotic Melee Weapon";
                            if (string.IsNullOrEmpty(strSpec))
                                strSpec = UseSkillSpec;
                            break;
                        case "Exotic Ranged Weapons":
                        case "Special Weapons":
                            strSkill = "Exotic Ranged Weapon";
                            if (string.IsNullOrEmpty(strSpec))
                                strSpec = UseSkillSpec;
                            break;
                        case "Flamethrowers":
                            strSkill = "Exotic Ranged Weapon";
                            strSpec = "Flamethrowers";
                            break;
                        case "Laser Weapons":
                            strSkill = "Exotic Ranged Weapon";
                            strSpec = "Laser Weapons";
                            break;
                        case "Assault Cannons":
                        case "Grenade Launchers":
                        case "Missile Launchers":
                        case "Light Machine Guns":
                        case "Medium Machine Guns":
                        case "Heavy Machine Guns":
                            strSkill = "Heavy Weapons";
                            break;
                        case "Shotguns":
                        case "Sniper Rifles":
                        case "Sporting Rifles":
                            strSkill = "Longarms";
                            break;
                        case "Throwing Weapons":
                            strSkill = "Throwing Weapons";
                            break;
                        case "Unarmed":
                            strSkill = "Unarmed Combat";
                            break;
                        default:
                            strSkill = "Pistols";
                            break;
                    }
                }

                if (strSkill.StartsWith("Exotic", StringComparison.Ordinal))
                {
                    strSkill += LanguageManager.GetString("String_Space")
                                + '(' + (!string.IsNullOrEmpty(strSpec) ? strSpec : UseSkillSpec) + ')';
                }
                intAccuracy += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponSkillAccuracy, false, strSkill);

                return intAccuracy;
            }
        }

        /// <summary>
        /// Displays the base and Total Accuracy of the weapon in the same format as it appears in rulebooks in the program's current language.
        /// </summary>
        public string DisplayAccuracy => GetAccuracy(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Displays the base and Total Accuracy of the weapon in the same format as it appears in rulebooks.
        /// TODO: Databindable?
        /// </summary>
        public string GetAccuracy(CultureInfo objCulture, string strLanguage)
        {
            int intTotalAccuracy = TotalAccuracy;
            if (int.TryParse(Accuracy, out int intAccuracy) && intAccuracy != intTotalAccuracy)
                return intAccuracy.ToString(objCulture) + LanguageManager.GetString("String_Space", strLanguage) + '(' + intTotalAccuracy.ToString(objCulture) + ')';
            return intTotalAccuracy.ToString(objCulture);
        }
        /// <summary>
        /// The slots the weapon has for modifications.
        /// </summary>
        public string ModificationSlots => _strWeaponSlots;

        /// <summary>
        /// What modification slots have their costs doubled.
        /// </summary>
        public string DoubledCostModificationSlots => _strDoubledCostWeaponSlots;

        public string Range
        {
            get => _strRange;
            set => _strRange = value;
        }

        public string AlternateRange
        {
            get => _strAlternateRange;
            set => _strAlternateRange = value;
        }

        public string CurrentDisplayRange => DisplayRange(GlobalOptions.Language);

        /// <summary>
        /// The string for the Weapon's Range category
        /// </summary>
        public string DisplayRange(string strLanguage)
        {
            string strRange = Range;
            if (string.IsNullOrWhiteSpace(strRange))
                strRange = Category;
            if (!string.IsNullOrWhiteSpace(strRange) && strLanguage != GlobalOptions.DefaultLanguage)
            {
                XmlDocument objXmlDocument = _objCharacter.LoadData("ranges.xml",  strLanguage);
                XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[name = \"" + strRange + "\"]");
                XmlNode xmlTranslateNode = objXmlCategoryNode?["translate"];
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.InnerText;
                }
                else
                {
                    objXmlDocument = _objCharacter.LoadData("weapons.xml", strLanguage);
                    objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strRange + "\"]");
                    xmlTranslateNode = objXmlCategoryNode?.Attributes?["translate"];
                    if (xmlTranslateNode != null)
                        strRange = xmlTranslateNode.InnerText;
                }
            }
            return strRange;
        }

        public string CurrentDisplayAlternateRange => DisplayAlternateRange(GlobalOptions.Language);

        /// <summary>
        /// The string for the Weapon's Range category (setter is English-only).
        /// </summary>
        public string DisplayAlternateRange(string strLanguage)
        {
            string strRange = AlternateRange.Trim();
            if (!string.IsNullOrEmpty(strRange) && strLanguage != GlobalOptions.DefaultLanguage)
            {
                XmlDocument objXmlDocument = _objCharacter.LoadData("ranges.xml", strLanguage);
                XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[name = \"" + strRange + "\"]");
                XmlNode xmlTranslateNode = objXmlCategoryNode?["translate"];
                if (xmlTranslateNode != null)
                {
                    strRange = xmlTranslateNode.InnerText;
                }
                else
                {
                    objXmlDocument = _objCharacter.LoadData("weapons.xml", strLanguage);
                    objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strRange + "\"]");
                    xmlTranslateNode = objXmlCategoryNode?.Attributes?["translate"];
                    if (xmlTranslateNode != null)
                        strRange = xmlTranslateNode.InnerText;
                }
            }
            return strRange;
        }

        /// <summary>
        /// Evalulate and return the requested Range for the Weapon.
        /// </summary>
        /// <param name="strFindRange">Range node to use.</param>
        /// <param name="blnUseAlternateRange">Use alternate range instead of the weapon's main range.</param>
        private int GetRange(string strFindRange, bool blnUseAlternateRange)
        {
            string strRangeCategory = Category;
            if (blnUseAlternateRange)
            {
                strRangeCategory = AlternateRange;
                if (string.IsNullOrWhiteSpace(strRangeCategory))
                    return -1;
            }
            else if (!string.IsNullOrEmpty(Range))
                strRangeCategory = Range;


            XmlDocument objXmlDocument = _objCharacter.LoadData("ranges.xml");
            XmlNode objXmlCategoryNode = objXmlDocument.SelectSingleNode("/chummer/ranges/range[name = \"" + strRangeCategory + "\"]");
            if (objXmlCategoryNode?[strFindRange] == null)
            {
                return -1;
            }
            string strRange = objXmlCategoryNode[strFindRange].InnerText;
            StringBuilder objRange = new StringBuilder(strRange);

            int intUseSTR = 0;
            int intUseAGI = 0;
            if (strRange.Contains("STR") || strRange.Contains("AGI"))
            {
                if (Cyberware)
                {
                    if (ParentVehicle != null)
                    {
                        intUseSTR = ParentVehicle.TotalBody;
                        intUseAGI = ParentVehicle.Pilot;
                        if (!string.IsNullOrEmpty(ParentID))
                        {
                            // Look to see if this is attached to a Cyberlimb and use its STR instead.
                            Cyberware objWeaponParent = _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID, out VehicleMod objVehicleMod);
                            if (objWeaponParent != null)
                            {
                                Cyberware objAttributeSource = objWeaponParent;
                                int intSTR = objAttributeSource.TotalStrength;
                                int intAGI = objAttributeSource.TotalStrength;
                                while (objAttributeSource != null)
                                {
                                    if (intSTR != 0 || intAGI != 0)
                                        break;
                                    objAttributeSource = objAttributeSource.Parent;
                                    if (objAttributeSource == null) continue;
                                    intSTR = objAttributeSource.TotalStrength;
                                    intAGI = objAttributeSource.TotalStrength;
                                }

                                intUseSTR = intSTR;
                                intUseAGI = intAGI;

                                if (intUseSTR == 0)
                                    intUseSTR = objVehicleMod.TotalStrength;
                                if (intUseAGI == 0)
                                    intUseAGI = objVehicleMod.TotalAgility;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(ParentID))
                    {
                        // Look to see if this is attached to a Cyberlimb and use its STR instead.
                        Cyberware objWeaponParent = _objCharacter.Cyberware.DeepFirstOrDefault(x => x.Children, x => x.InternalId == ParentID);
                        if (objWeaponParent != null)
                        {
                            Cyberware objAttributeSource = objWeaponParent;
                            int intSTR = objAttributeSource.TotalStrength;
                            int intAGI = objAttributeSource.TotalStrength;
                            while (objAttributeSource != null)
                            {
                                if (intSTR != 0 || intAGI != 0)
                                    break;
                                objAttributeSource = objAttributeSource.Parent;
                                if (objAttributeSource == null) continue;
                                intSTR = objAttributeSource.TotalStrength;
                                intAGI = objAttributeSource.TotalStrength;
                            }

                            intUseSTR = intSTR;
                            intUseAGI = intAGI;
                        }
                        if (intUseSTR == 0)
                            intUseSTR = _objCharacter.STR.TotalValue;
                        if (intUseAGI == 0)
                            intUseAGI = _objCharacter.AGI.TotalValue;
                    }
                }
                else if (ParentVehicle == null)
                {
                    intUseSTR = _objCharacter.STR.TotalValue;
                    intUseAGI = _objCharacter.AGI.TotalValue;
                }

                if (Category == "Throwing Weapons" || UseSkill == "Throwing Weapons")
                {
                    intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowSTR);
                    intUseSTR += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowRangeSTR);
                }
            }

            foreach (string strAttribute in AttributeSection.AttributeStrings)
            {
                CharacterAttrib objLoopAttribute = _objCharacter.GetAttribute(strAttribute);
                if (strAttribute == "STR")
                {
                    objRange.Replace("STR", intUseSTR.ToString(GlobalOptions.InvariantCultureInfo));
                }
                else if (strAttribute == "AGI")
                {
                    objRange.Replace("AGI", intUseAGI.ToString(GlobalOptions.InvariantCultureInfo));
                }
                else
                {
                    objRange.CheapReplace(strRange, strAttribute, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                }
            }

            if (Category == "Throwing Weapons" || UseSkill == "Throwing Weapons")
                objRange.Append(" + " + ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.ThrowRange).ToString(GlobalOptions.InvariantCultureInfo));

            // Replace the division sign with "div" since we're using XPath.
            objRange.Replace("/", " div ");

            object objProcess = CommonFunctions.EvaluateInvariantXPath(objRange.ToString(), out bool blnIsSuccess);

            return blnIsSuccess ? decimal.ToInt32(decimal.Ceiling(Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) * _decRangeMultiplier)) : -1;
        }

        /// <summary>
        /// Weapon's total Range bonus from Accessories.
        /// </summary>
        public int RangeBonus
        {
            get
            {
                int intRangeBonus = 0;

                // Weapon Mods.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                    if (objAccessory.Equipped)
                        intRangeBonus += objAccessory.RangeBonus;

                // Check if the Weapon has Ammunition loaded and look for any Range bonus.
                if (!string.IsNullOrEmpty(AmmoLoaded))
                {
                    Gear objGear = _objCharacter.Gear.DeepFindById(AmmoLoaded) ?? _objCharacter.Vehicles.FindVehicleGear(AmmoLoaded);

                    if (objGear?.WeaponBonus != null)
                    {
                        intRangeBonus += objGear.WeaponBonusRange;
                    }
                }

                return intRangeBonus;
            }
        }

        public string RangeModifier(string strRange)
        {
            if (string.IsNullOrEmpty(strRange))
                return string.Empty;
            int i = Convert.ToInt32(_objCharacter.LoadData("ranges.xml")
                .SelectSingleNode($"chummer/modifiers/{strRange.ToLowerInvariant()}")
                ?.InnerText, GlobalOptions.InvariantCultureInfo);
            i += WeaponAccessories.Sum(wa => wa.RangeModifier);
            i = Math.Min(0, i);
            return string.Format(GlobalOptions.InvariantCultureInfo, LanguageManager.GetString("Label_Range" + strRange), i);
        }

        /// <summary>
        /// Dictionary where keys are range categories (short, medium, long, extreme, alternateshort, etc.), values are strings depicting range values for the category.
        /// </summary>
        public IDictionary<string, string> GetRangeStrings(CultureInfo objCulture)
        {
            int intRangeModifier = RangeBonus + 100;
            int intMin = GetRange("min", false);
            int intShort = GetRange("short", false);
            int intMedium = GetRange("medium", false);
            int intLong = GetRange("long", false);
            int intExtreme = GetRange("extreme", false);
            int intAlternateMin = GetRange("min", true);
            int intAlternateShort = GetRange("short", true);
            int intAlternateMedium = GetRange("medium", true);
            int intAlternateLong = GetRange("long", true);
            int intAlternateExtreme = GetRange("extreme", true);
            if (intMin > 0)
                intMin = (intMin * intRangeModifier + 99) / 100;
            if (intShort > 0)
                intShort = (intShort * intRangeModifier + 99) / 100;
            if (intMedium > 0)
                intMedium = (intMedium * intRangeModifier + 99) / 100;
            if (intLong > 0)
                intLong = (intLong * intRangeModifier + 99) / 100;
            if (intExtreme > 0)
                intExtreme = (intExtreme * intRangeModifier + 99) / 100;
            if (intAlternateMin > 0)
                intAlternateMin = (intAlternateMin * intRangeModifier + 99) / 100;
            if (intAlternateShort > 0)
                intAlternateShort = (intAlternateShort * intRangeModifier + 99) / 100;
            if (intAlternateMedium > 0)
                intAlternateMedium = (intAlternateMedium * intRangeModifier + 99) / 100;
            if (intAlternateLong > 0)
                intAlternateLong = (intAlternateLong * intRangeModifier + 99) / 100;
            if (intAlternateExtreme > 0)
                intAlternateExtreme = (intAlternateExtreme * intRangeModifier + 99) / 100;

            Dictionary<string, string> retDictionary = new Dictionary<string, string>(8)
                {
                    { "short", intMin < 0 || intShort < 0 ? string.Empty : intMin.ToString(objCulture) + '-' + intShort.ToString(objCulture) },
                    { "medium", intShort < 0 || intMedium < 0 ? string.Empty : (intShort + 1).ToString(objCulture) + '-' + intMedium.ToString(objCulture) },
                    { "long", intMedium < 0 || intLong < 0 ? string.Empty : (intMedium + 1).ToString(objCulture) + '-' + intLong.ToString(objCulture) },
                    { "extreme", intLong < 0 || intExtreme < 0 ? string.Empty : (intLong + 1).ToString(objCulture) + '-' + intExtreme.ToString(objCulture) },
                    { "alternateshort", intAlternateMin < 0 || intAlternateShort < 0 ? string.Empty : intAlternateMin.ToString(objCulture) + '-' + intAlternateShort.ToString(objCulture) },
                    { "alternatemedium", intAlternateShort < 0 || intAlternateMedium < 0 ? string.Empty : (intAlternateShort + 1).ToString(objCulture) + '-' + intAlternateMedium.ToString(objCulture) },
                    { "alternatelong", intAlternateMedium < 0 || intAlternateLong < 0 ? string.Empty : (intAlternateMedium + 1).ToString(objCulture) + '-' + intAlternateLong.ToString(objCulture) },
                    { "alternateextreme", intAlternateLong < 0 || intAlternateExtreme < 0 ? string.Empty : (intAlternateLong + 1).ToString(objCulture) + '-' + intAlternateExtreme.ToString(objCulture) }
                };

            return retDictionary;
        }

        /// <summary>
        /// Number of rounds consumed by Single Shot.
        /// </summary>
        public int SingleShot
        {
            get
            {
                int intReturn = _intSingleShot;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.SingleShot > intReturn)
                        intReturn = objAccessory.SingleShot;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Number of rounds consumed by Short Burst.
        /// </summary>
        public int ShortBurst
        {
            get
            {
                int intReturn = _intShortBurst;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.ShortBurst > intReturn)
                        intReturn = objAccessory.ShortBurst;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Number of rounds consumed by Long Burst.
        /// </summary>
        public int LongBurst
        {
            get
            {
                int intReturn = _intLongBurst;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.LongBurst > intReturn)
                        intReturn = objAccessory.LongBurst;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Number of rounds consumed by Full Burst.
        /// </summary>
        public int FullBurst
        {
            get
            {
                int intReturn = _intFullBurst;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.FullBurst > intReturn)
                        intReturn = objAccessory.FullBurst;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Number of rounds consumed by Suppressive Fire.
        /// </summary>
        public int Suppressive
        {
            get
            {
                int intReturn = _intSuppressive;

                // Check to see if any of the Mods replace this value.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.Suppressive > intReturn)
                        intReturn = objAccessory.Suppressive;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Total Accessory Cost multiplier for the Weapon.
        /// </summary>
        public int AccessoryMultiplier
        {
            get
            {
                int intReturn = 0;
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.AccessoryCostMultiplier != 1)
                        intReturn += objAccessory.AccessoryCostMultiplier;
                }

                if (intReturn == 0)
                    intReturn = 1;

                return intReturn;
            }
        }

        /// <summary>
        /// The Dice Pool size for the Active Skill required to use the Weapon.
        /// </summary>
        public int DicePool
        {
            get
            {
                int intDicePool = 0;
                int intDicePoolModifier = WeaponAccessories.Where(a => a.Equipped).Sum(a => a.DicePool);
                switch (FireMode)
                {
                    case FiringMode.DogBrain:
                    {
                            intDicePool = ParentVehicle.Pilot;
                            Gear objAutosoft = ParentVehicle.Gear.DeepFirstOrDefault(x => x.Children, x => x.Name == "[Weapon] Targeting Autosoft" && (x.Extra == Name || x.Extra == CurrentDisplayName));

                            intDicePool += objAutosoft?.Rating ?? -1;

                            if (WeaponAccessories.Any(accessory => accessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && accessory.WirelessOn))
                            {
                                if (ParentVehicle.Gear.DeepAny(x => x.Children, x => x.Name == "Smartsoft"))
                                {
                                    ++intDicePoolModifier;
                                }
                            }
                            break;
                        }
                    case FiringMode.GunneryCommandDevice:
                    case FiringMode.RemoteOperated:
                        {
                            intDicePool = _objCharacter.SkillsSection.GetActiveSkill("Gunnery").PoolOtherAttribute(_objCharacter.LOG.TotalValue, "LOG");

                            if (WeaponAccessories.Any(accessory => accessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && accessory.WirelessOn))
                            {
                                intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Smartlink);
                            }

                            intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice, false, Category);
                            break;
                        }
                    case FiringMode.ManualOperation:
                        {
                            intDicePool = _objCharacter.SkillsSection.GetActiveSkill("Gunnery").Pool;

                            if (WeaponAccessories.Any(accessory => accessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && accessory.WirelessOn))
                            {
                                intDicePoolModifier +=
                                    ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Smartlink);
                            }

                            intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice, false, Category);
                            break;
                        }
                    case FiringMode.Skill:
                        {
                            Skill objSkill = Skill;
                            if (objSkill != null)
                            {
                                intDicePool = objSkill.Pool;

                                if (WeaponAccessories.Any(accessory => accessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && accessory.WirelessOn))
                                {
                                    intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Smartlink);
                                }

                                intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponCategoryDice, false, Category);
                                intDicePoolModifier += ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.WeaponSpecificDice, false, InternalId);

                                // If the character has a Specialization, include it in the Dice Pool string.
                                if (objSkill.Specializations.Count > 0 && !objSkill.IsExoticSkill)
                                {
                                    SkillSpecialization objSpec =
                                        objSkill.GetSpecialization(DisplayNameShort(GlobalOptions.Language)) ??
                                        objSkill.GetSpecialization(Name) ??
                                        objSkill.GetSpecialization(DisplayCategory(GlobalOptions.DefaultLanguage)) ??
                                        objSkill.GetSpecialization(Category);

                                    if (objSpec == null && !string.IsNullOrWhiteSpace(objSkill.Specialization))
                                    {
                                        objSpec = objSkill.GetSpecialization(Spec) ?? objSkill.GetSpecialization(Spec2);
                                    }

                                    if (objSpec != null)
                                    {
                                        intDicePoolModifier += objSpec.SpecializationBonus;
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(FireMode));
                }

                if (FireMode == FiringMode.GunneryCommandDevice || FireMode == FiringMode.RemoteOperated ||
                    FireMode == FiringMode.ManualOperation)
                {
                    Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill("Gunnery");
                    if (objSkill?.Specializations.Count > 0 && RelevantSpecialization != "None")
                    {
                        intDicePool += objSkill.GetSpecializationBonus(RelevantSpecialization);
                    }
                }

                string strWeaponBonusPool = ParentVehicle != null
                    ? ParentVehicle.Gear.DeepFindById(AmmoLoaded)?.WeaponBonus?["pool"]?.InnerText
                    : _objCharacter.Gear.DeepFindById(AmmoLoaded)?.WeaponBonus?["pool"]?.InnerText;

                if (!string.IsNullOrEmpty(strWeaponBonusPool))
                    intDicePoolModifier += Convert.ToInt32(strWeaponBonusPool, GlobalOptions.InvariantCultureInfo);

                return intDicePool + intDicePoolModifier;
            }
        }

        private string _strRelevantSpec = string.Empty;
        internal string RelevantSpecialization
        {
            get
            {
                if (!string.IsNullOrEmpty(_strRelevantSpec))
                {
                    return _strRelevantSpec;
                }

                string strGunnerySpec = GetNode()?.SelectSingleNode("category/@gunneryspec")?.InnerText;
                if (string.IsNullOrEmpty(strGunnerySpec))
                {
                    strGunnerySpec = _objCharacter.LoadData("weapons.xml")
                        .SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@gunneryspec")?.InnerText
                                     ?? "None";
                }

                _strRelevantSpec = strGunnerySpec;
                return _strRelevantSpec;
            }
        }

        private Skill Skill
        {
            get
            {
                string strCategory = Category;
                string strSpec = string.Empty;

                // If this is a Special Weapon, use the Range to determine the required Active Skill (if present).
                if (strCategory == "Special Weapons" && !string.IsNullOrEmpty(Range))
                    strCategory = Range;

                // Exotic Skills require a matching Specialization.
                string strSkill = GetSkillName(strCategory, ref strSpec);

                // Use the Skill defined by the Weapon if one is present.
                if (!string.IsNullOrEmpty(UseSkill))
                {
                    strSkill = UseSkill;
                    strSpec = string.Empty;

                    if (UseSkill.Contains("Exotic"))
                        strSpec = UseSkillSpec;
                }

                // Locate the Active Skill to be used.
                Skill objSkill = null;
                foreach (Skill objCharacterSkill in _objCharacter.SkillsSection.Skills)
                {
                    if (objCharacterSkill.Name != strSkill)
                        continue;
                    if (string.IsNullOrEmpty(strSpec) || objCharacterSkill.HasSpecialization(strSpec))
                    {
                        objSkill = objCharacterSkill;
                        break;
                    }
                    //If the weapon doesn't have a Spec2 or it doesn't match, move along. Mostly affects exotics.
                    if (string.IsNullOrEmpty(Spec2) || !objCharacterSkill.HasSpecialization(Spec2))
                        continue;
                    objSkill = objCharacterSkill;
                    break;
                }
                return objSkill;
            }
        }

        private string GetSkillName(string strCategory, ref string strSpec)
        {
            string strSkill;
            switch (strCategory)
            {
                case "Bows":
                case "Crossbows":
                    strSkill = "Archery";
                    break;
                case "Assault Rifles":
                case "Carbines":
                case "Machine Pistols":
                case "Submachine Guns":
                    strSkill = "Automatics";
                    break;
                case "Blades":
                    strSkill = "Blades";
                    break;
                case "Clubs":
                case "Improvised Weapons":
                    strSkill = "Clubs";
                    break;
                case "Exotic Melee Weapons":
                    strSkill = "Exotic Melee Weapon";
                    strSpec = UseSkillSpec;
                    break;
                case "Exotic Ranged Weapons":
                case "Special Weapons":
                    strSkill = "Exotic Ranged Weapon";
                    strSpec = UseSkillSpec;
                    break;
                case "Flamethrowers":
                    strSkill = "Exotic Ranged Weapon";
                    strSpec = "Flamethrowers";
                    break;
                case "Laser Weapons":
                    strSkill = "Exotic Ranged Weapon";
                    strSpec = "Laser Weapons";
                    break;
                case "Assault Cannons":
                case "Grenade Launchers":
                case "Missile Launchers":
                case "Light Machine Guns":
                case "Medium Machine Guns":
                case "Heavy Machine Guns":
                    strSkill = "Heavy Weapons";
                    break;
                case "Shotguns":
                case "Sniper Rifles":
                case "Sporting Rifles":
                    strSkill = "Longarms";
                    break;
                case "Throwing Weapons":
                    strSkill = "Throwing Weapons";
                    break;
                case "Unarmed":
                    strSkill = "Unarmed Combat";
                    break;
                default:
                    strSkill = "Pistols";
                    break;
            }
            return strSkill;
        }

        /// <summary>
        /// Tooltip information for the Dice Pool.
        /// </summary>
        public string DicePoolTooltip
        {
            get
            {
                StringBuilder sbdReturn = new StringBuilder();
                string strSpace = LanguageManager.GetString("String_Space");
                switch (FireMode)
                {
                    case FiringMode.DogBrain:
                    {
                            sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{1}{0}({2})",
                                strSpace, LanguageManager.GetString("String_Pilot"), ParentVehicle.Pilot);
                            Gear objAutosoft = ParentVehicle.Gear.DeepFirstOrDefault(x => x.Children,
                                x => x.Name == "[Weapon] Targeting Autosoft" &&
                                     (x.Extra == Name || x.Extra == CurrentDisplayName));

                            if (objAutosoft != null)
                            {
                                sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                                    objAutosoft.CurrentDisplayName, strSpace, objAutosoft.Rating);
                            }
                            else
                            {
                                sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                                    LanguageManager.GetString("Tip_Skill_Defaulting"), strSpace, -1);
                            }
                            break;
                        }
                    case FiringMode.RemoteOperated:
                        //TODO: how/why are these different?
                    case FiringMode.GunneryCommandDevice:
                        {
                            Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill("Gunnery");
                            sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{1}[{2}]{0}({3})",
                                strSpace, objSkill.CurrentDisplayName, _objCharacter.LOG.DisplayAbbrev, objSkill.PoolOtherAttribute(_objCharacter.LOG.TotalValue, "LOG"));
                            if (objSkill.Specializations.Count > 0 && RelevantSpecialization != "None")
                            {
                                SkillSpecialization spec = objSkill.GetSpecialization(RelevantSpecialization);
                                if (spec != null)
                                {
                                    int intSpecBonus = spec.SpecializationBonus;
                                    if (intSpecBonus != 0)
                                        sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                                            strSpace, spec.CurrentDisplayName, intSpecBonus);
                                }
                            }
                            break;
                        }
                    case FiringMode.ManualOperation:
                        {
                            Skill objSkill = _objCharacter.SkillsSection.GetActiveSkill("Gunnery");
                            sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{1}[{2}]{0}({3})",
                                strSpace, objSkill.CurrentDisplayName, objSkill.AttributeObject.DisplayAbbrev, objSkill.Pool);
                            if (objSkill.Specializations.Count > 0 && RelevantSpecialization != "None")
                            {
                                SkillSpecialization spec = objSkill.GetSpecialization(RelevantSpecialization);
                                if (spec != null)
                                {
                                    int intSpecBonus = spec.SpecializationBonus;
                                    if (intSpecBonus != 0)
                                        sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                                            strSpace, spec.CurrentDisplayName, intSpecBonus);
                                }
                            }
                            break;
                        }
                    case FiringMode.Skill:
                        {
                            Skill objSkill = Skill;
                            if (objSkill != null)
                            {
                                sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}[{1}]{2}({3})",
                                    objSkill.CurrentDisplayName, objSkill.AttributeObject.DisplayAbbrev, strSpace, objSkill.Pool);
                                // If the character has a Specialization, include it in the Dice Pool string.
                                if (objSkill.Specializations.Count > 0 && !objSkill.IsExoticSkill)
                                {
                                    SkillSpecialization spec =
                                        objSkill.GetSpecialization(DisplayNameShort(GlobalOptions.Language)) ??
                                        objSkill.GetSpecialization(Name) ??
                                        objSkill.GetSpecialization(DisplayCategory(GlobalOptions.DefaultLanguage)) ??
                                        objSkill.GetSpecialization(Category);

                                    if (spec == null && !string.IsNullOrWhiteSpace(objSkill.Specialization))
                                    {
                                        spec = objSkill.GetSpecialization(Spec) ?? objSkill.GetSpecialization(Spec2);
                                    }
                                    if (spec != null)
                                    {
                                        int intSpecBonus = spec.SpecializationBonus;
                                        if (intSpecBonus != 0)
                                            sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                                                strSpace, spec.CurrentDisplayName, intSpecBonus);
                                    }
                                }
                            }

                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(FireMode));
                }

                foreach (WeaponAccessory wa in WeaponAccessories.Where(a => a.Equipped && a.DicePool != 0))
                {
                    sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                        strSpace, wa.CurrentDisplayName, wa.DicePool);
                }

                Gear objLoadedAmmo = ParentVehicle != null
                    ? ParentVehicle.Gear.DeepFindById(AmmoLoaded)
                    : _objCharacter.Gear.DeepFindById(AmmoLoaded);
                if (!string.IsNullOrEmpty(objLoadedAmmo?.WeaponBonus?["pool"]?.InnerText))
                {
                    sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                        strSpace, objLoadedAmmo.CurrentDisplayNameShort, objLoadedAmmo.WeaponBonus?["pool"]?.InnerText);
                }
                if (ParentVehicle == null)
                {
                    if (WeaponAccessories.Any(accessory => accessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && accessory.WirelessOn))
                    {
                        sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                            strSpace, LanguageManager.GetString("Tip_Skill_Smartlink"), ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.Smartlink));
                    }

                    foreach (Improvement objImprovement in _objCharacter.Improvements
                        .Where(objImprovement => objImprovement.Enabled && (objImprovement.ImproveType == Improvement.ImprovementType.WeaponCategoryDice &&
                                                 objImprovement.ImprovedName == Category || objImprovement.ImproveType == Improvement.ImprovementType.WeaponSpecificDice &&
                                                 objImprovement.ImprovedName == InternalId)  && string.IsNullOrEmpty(objImprovement.Condition)))
                    {
                        sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                            strSpace, _objCharacter.GetObjectName(objImprovement), objImprovement.Value);
                    }
                }
                else if (WeaponAccessories.Any(accessory => accessory.Name.StartsWith("Smartgun", StringComparison.Ordinal) && accessory.WirelessOn))
                {
                    if (ParentVehicle.Gear.DeepAny(x => x.Children, x => x.Name == "Smartsoft"))
                    {
                        sbdReturn.AppendFormat(GlobalOptions.CultureInfo, "{0}+{0}{1}{0}({2})",
                            strSpace, LanguageManager.GetString("Tip_Skill_Smartlink"), 1);
                    }
                }

                return sbdReturn.ToString();
            }
        }

        /// <summary>
        /// Total Availability in the program's current language.
        /// </summary>
        public string DisplayTotalAvail => TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.Language);

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
            bool blnCheckUnderbarrels = blnCheckChildren;
            if (strAvail.Length > 0)
            {
                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));
                objAvail.CheapReplace("{Rating}", () => Rating.ToString(GlobalOptions.InvariantCultureInfo));

                if (blnCheckUnderbarrels && strAvail.Contains("{Children Avail}"))
                {
                    blnCheckUnderbarrels = false;
                    int intMaxChildAvail = 0;
                    foreach (Weapon objUnderbarrel in UnderbarrelWeapons)
                    {
                        if (objUnderbarrel.ParentID != InternalId)
                        {
                            AvailabilityValue objLoopAvail = objUnderbarrel.TotalAvailTuple();
                            if (!objLoopAvail.AddToParent)
                                intAvail += objLoopAvail.Value;
                            else if (objLoopAvail.Value > intMaxChildAvail)
                                intMaxChildAvail = objLoopAvail.Value;
                            if (objLoopAvail.Suffix == 'F')
                                chrLastAvailChar = 'F';
                            else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                                chrLastAvailChar = 'R';
                        }
                    }
                    objAvail.Replace("{Children Avail}", intMaxChildAvail.ToString(GlobalOptions.InvariantCultureInfo));
                }

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }

                // Replace the division sign with "div" since we're using XPath.
                objAvail.Replace("/", " div ");
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess, GlobalOptions.InvariantCultureInfo);
            }

            if (blnCheckUnderbarrels)
            {
                foreach (Weapon objUnderbarrel in UnderbarrelWeapons)
                {
                    if (objUnderbarrel.ParentID != InternalId)
                    {
                        AvailabilityValue objLoopAvail = objUnderbarrel.TotalAvailTuple();
                        if (objLoopAvail.AddToParent)
                            intAvail += objLoopAvail.Value;
                        if (objLoopAvail.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

            if (blnCheckChildren)
            {
                // Run through the Accessories and add in their availability.
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (!objAccessory.IncludedInWeapon && objAccessory.Equipped)
                    {
                        AvailabilityValue objLoopAvail = objAccessory.TotalAvailTuple();
                        if (objLoopAvail.AddToParent)
                            intAvail += objLoopAvail.Value;
                        if (objLoopAvail.Suffix == 'F')
                            chrLastAvailChar = 'F';
                        else if (chrLastAvailChar != 'F' && objLoopAvail.Suffix == 'R')
                            chrLastAvailChar = 'R';
                    }
                }
            }

            if (intAvail < 0)
                intAvail = 0;

            return new AvailabilityValue(intAvail, chrLastAvailChar, blnModifyParentAvail, IncludedInWeapon);
        }

        // Run through the Weapon Mods and see if anything changes the cost multiplier (Vintage mod).
        public int CostMultiplier
        {
            get
            {
                int intReturn = 1;
                foreach (WeaponAccessory objAccessory in WeaponAccessories)
                {
                    if (objAccessory.Equipped && objAccessory.AccessoryCostMultiplier > 1)
                        intReturn = objAccessory.AccessoryCostMultiplier;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Mount slot that is used when mounting this weapon to another weapon.
        /// </summary>
        public string Mount => _strMount;

        /// <summary>
        /// Additional Mount slot that is used when mounting this weapon to another weapon.
        /// </summary>
        public string ExtraMount => _strExtraMount;

        /// <summary>
        /// Method used to fire the Weapon. If not vehicle mounted, always returns Skill.
        /// </summary>
        public FiringMode FireMode
        {
            get => ParentVehicle == null ? FiringMode.Skill : _eFiringMode;
            set => _eFiringMode = value;
        }

        public bool IsProgram
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.IsProgram;
                return false;
            }
        }

        /// <summary>
        /// Device rating string for Cyberware. If it's empty, then GetBaseMatrixAttribute for Device Rating will fetch the grade's DR.
        /// </summary>
        public string DeviceRating
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.DeviceRating;
                return _strDeviceRating;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.DeviceRating = value;
                else
                    _strDeviceRating = value;
            }
        }

        /// <summary>
        /// Attack string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Attack
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.Attack;
                return _strAttack;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.Attack = value;
                else
                    _strAttack = value;
            }
        }

        /// <summary>
        /// Sleaze string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Sleaze
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.Sleaze;
                return _strSleaze;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.Sleaze = value;
                else
                    _strSleaze = value;
            }
        }

        /// <summary>
        /// Data Processing string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string DataProcessing
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.DataProcessing;
                return _strDataProcessing;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.DataProcessing = value;
                else
                    _strDataProcessing = value;
            }
        }

        /// <summary>
        /// Firewall string (if one is explicitly specified for this 'ware).
        /// </summary>
        public string Firewall
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.Firewall;
                return _strFirewall;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.Firewall = value;
                else
                    _strFirewall = value;
            }
        }

        /// <summary>
        /// Modify Parent's Attack by this.
        /// </summary>
        public string ModAttack
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.ModAttack;
                return _strModAttack;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModAttack = value;
                else
                    _strModAttack = value;
            }
        }

        /// <summary>
        /// Modify Parent's Sleaze by this.
        /// </summary>
        public string ModSleaze
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.ModSleaze;
                return _strModSleaze;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModSleaze = value;
                else
                    _strModSleaze = value;
            }
        }

        /// <summary>
        /// Modify Parent's Data Processing by this.
        /// </summary>
        public string ModDataProcessing
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.ModDataProcessing;
                return _strModDataProcessing;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModDataProcessing = value;
                else
                    _strModDataProcessing = value;
            }
        }

        /// <summary>
        /// Modify Parent's Firewall by this.
        /// </summary>
        public string ModFirewall
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.ModFirewall;
                return _strModFirewall;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModFirewall = value;
                else
                    _strModFirewall = value;
            }
        }

        /// <summary>
        /// Cyberdeck's Attribute Array string.
        /// </summary>
        public string AttributeArray
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.AttributeArray;
                return _strAttributeArray;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.AttributeArray = value;
                else
                    _strAttributeArray = value;
            }
        }

        /// <summary>
        /// Modify Parent's Attribute Array by this.
        /// </summary>
        public string ModAttributeArray
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.ModAttributeArray;
                return _strModAttributeArray;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ModAttributeArray = value;
                else
                    _strModAttributeArray = value;
            }
        }

        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        public string Overclocked
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.Overclocked;
                return _strOverclocked;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.Overclocked = value;
                else
                    _strOverclocked = value;
            }
        }

        /// <summary>
        /// Empty for Weapons.
        /// </summary>
        public string CanFormPersona
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.CanFormPersona;
                return string.Empty;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.CanFormPersona = value;
            }
        }

        public bool IsCommlink
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.IsCommlink;
                return false;
            }
        }

        /// <summary>
        /// 0 for Cyberware.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.BonusMatrixBoxes;
                return 0;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.BonusMatrixBoxes = value;
            }
        }

        public int TotalBonusMatrixBoxes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.TotalBonusMatrixBoxes;
                return 0;
            }
        }

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.ProgramLimit;
                return _strProgramLimit;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.ProgramLimit = value;
                else
                    _strProgramLimit = value;
            }
        }

        /// <summary>
        /// Returns true if this is a cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.CanSwapAttributes;
                return _blnCanSwapAttributes;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.CanSwapAttributes = value;
                else
                    _blnCanSwapAttributes = value;
            }
        }

        public IList<IHasMatrixAttributes> ChildrenWithMatrixAttributes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.ChildrenWithMatrixAttributes;
                return Children.Cast<IHasMatrixAttributes>().ToList();
            }
        }

        /// <summary>
        /// Base Physical Boxes. 12 for vehicles, 6 for Drones.
        /// </summary>
        public int BaseMatrixBoxes
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.BaseMatrixBoxes;
                int baseMatrixBoxes = 8;
                return baseMatrixBoxes;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.MatrixCM;
                return BaseMatrixBoxes + (this.GetTotalMatrixAttribute("Device Rating") + 1) / 2 + TotalBonusMatrixBoxes;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes filled.
        /// </summary>
        public int MatrixCMFilled
        {
            get
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    return objThis.MatrixCMFilled;
                return _intMatrixCMFilled;
            }
            set
            {
                IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
                if (objThis != null)
                    objThis.MatrixCMFilled = value;
                else
                    _intMatrixCMFilled = value;
            }
        }

        public enum FiringMode
        {
            Skill,
            GunneryCommandDevice,
            RemoteOperated,
            DogBrain,
            ManualOperation,
            NumFiringModes // 🡐 This one should always be the last defined enum
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Convert a string to a Firing Mode.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static FiringMode ConvertToFiringMode(string strValue)
        {
            switch (strValue)
            {
                case "DogBrain":
                    return FiringMode.DogBrain;
                case "GunneryCommandDevice":
                    return FiringMode.GunneryCommandDevice;
                case "RemoteOperated":
                    return FiringMode.RemoteOperated;
                case "ManualOperation":
                    return FiringMode.ManualOperation;
                default:
                    return FiringMode.Skill;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Toggle the Wireless Bonus for this armor.
        /// </summary>
        /// <param name="enable"></param>
        public void ToggleWirelessBonuses(bool enable)
        {
            if (enable)
            {
                if (WirelessBonus?.Attributes?.Count > 0)
                {
                    if (WirelessBonus.Attributes["mode"].InnerText == "replace")
                    {
                        ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId).ToList());
                    }
                }
                if (WirelessBonus?.InnerText != null)
                {
                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.ArmorMod,
                        _guiID.ToString("D", GlobalOptions.InvariantCultureInfo) + "Wireless", WirelessBonus, 1, DisplayNameShort(GlobalOptions.Language));
                }
            }
            else
            {
                if (WirelessBonus?.Attributes?.Count > 0)
                {
                    if (WirelessBonus.Attributes?["mode"].InnerText == "replace")
                    {
                        ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId).ToList());
                    }
                }
                ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.ArmorMod && x.SourceName == InternalId + "Wireless").ToList());
            }
        }

        /// <summary>
        /// Recursive method to delete a piece of 'ware and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteWeapon()
        {
            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Weapon objChild in Children)
                decReturn += objChild.DeleteWeapon();

            foreach (WeaponAccessory objLoopAccessory in WeaponAccessories)
                decReturn += objLoopAccessory.DeleteWeaponAccessory();

            List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>>();
            foreach (Weapon objDeleteWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
            {
                lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objDeleteWeapon, null, null, null));
            }
            foreach (Vehicle objVehicle in _objCharacter.Vehicles)
            {
                foreach (Weapon objDeleteWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                {
                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objDeleteWeapon, objVehicle, null, null));
                }

                foreach (VehicleMod objMod in objVehicle.Mods)
                {
                    foreach (Weapon objDeleteWeapon in objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                    {
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objDeleteWeapon, objVehicle, objMod, null));
                    }
                }

                foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                {
                    foreach (Weapon objDeleteWeapon in objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                    {
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objDeleteWeapon, objVehicle, null, objMount));
                    }
                }
            }
            // We need this list separate because weapons to remove can contain gear that add more weapons in need of removing
            foreach (Tuple<Weapon, Vehicle, VehicleMod, WeaponMount> objLoopTuple in lstWeaponsToDelete)
            {
                Weapon objDeleteWeapon = objLoopTuple.Item1;
                decReturn += objDeleteWeapon.TotalCost + objDeleteWeapon.DeleteWeapon();
                if (objDeleteWeapon.Parent != null)
                    objDeleteWeapon.Parent.Children.Remove(objDeleteWeapon);
                else if (objLoopTuple.Item4 != null)
                    objLoopTuple.Item4.Weapons.Remove(objDeleteWeapon);
                else if (objLoopTuple.Item3 != null)
                    objLoopTuple.Item3.Weapons.Remove(objDeleteWeapon);
                else if (objLoopTuple.Item2 != null)
                    objLoopTuple.Item2.Weapons.Remove(objDeleteWeapon);
                else
                    _objCharacter.Weapons.Remove(objDeleteWeapon);
            }

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Weapon, InternalId + "Wireless");

            return decReturn;
        }


        /// <summary>
        /// Checks a nominated piece of gear for Availability requirements.
        /// </summary>
        /// <param name="blnRestrictedGearUsed">Whether Restricted Gear is already being used.</param>
        /// <param name="intRestrictedCount">Amount of gear that is currently over the availability limit.</param>
        /// <param name="strAvailItems">String used to list names of gear that are currently over the availability limit.</param>
        /// <param name="strRestrictedItem">Item that is being used for Restricted Gear.</param>
        /// <param name="blnOutRestrictedGearUsed">Whether Restricted Gear is already being used (tracked across gear children).</param>
        /// <param name="intOutRestrictedCount">Amount of gear that is currently over the availability limit (tracked across gear children).</param>
        /// <param name="strOutAvailItems">String used to list names of gear that are currently over the availability limit (tracked across gear children).</param>
        /// <param name="strOutRestrictedItem">Item that is being used for Restricted Gear (tracked across gear children).</param>
        public void CheckRestrictedGear(bool blnRestrictedGearUsed, int intRestrictedCount, string strAvailItems, string strRestrictedItem, out bool blnOutRestrictedGearUsed, out int intOutRestrictedCount, out string strOutAvailItems, out string strOutRestrictedItem)
        {
            if (!IncludedInWeapon)
            {
                AvailabilityValue objTotalAvail = TotalAvailTuple();
                if (!objTotalAvail.AddToParent)
                {
                    int intAvailInt = objTotalAvail.Value;
                    if (intAvailInt > _objCharacter.MaximumAvailability)
                    {
                        if (intAvailInt <= _objCharacter.RestrictedGear && !blnRestrictedGearUsed)
                        {
                            blnRestrictedGearUsed = true;
                            strRestrictedItem = Parent == null
                                ? CurrentDisplayName
                                : CurrentDisplayName + LanguageManager.GetString("String_Space") + '(' + Parent.CurrentDisplayName + ')';
                        }
                        else
                        {
                            intRestrictedCount++;
                            strAvailItems += Environment.NewLine + "\t\t" + DisplayNameShort(GlobalOptions.Language);
                        }
                    }
                }
            }

            foreach (WeaponAccessory objChild in WeaponAccessories)
            {
                objChild.CheckRestrictedGear(blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
            }

            foreach (Weapon objChild in UnderbarrelWeapons)
            {
                objChild.CheckRestrictedGear(blnRestrictedGearUsed, intRestrictedCount, strAvailItems, strRestrictedItem, out blnRestrictedGearUsed, out intRestrictedCount, out strAvailItems, out strRestrictedItem);
            }
            strOutAvailItems = strAvailItems;
            intOutRestrictedCount = intRestrictedCount;
            blnOutRestrictedGearUsed = blnRestrictedGearUsed;
            strOutRestrictedItem = strRestrictedItem;
        }

        public void Reload(ICollection<Gear> lstGears, TreeView treGearView)
        {
            List<Gear> lstAmmo = new List<Gear>();
            List<string> lstCount = new List<string>();
            bool blnExternalSource = false;
            Gear objExternalSource = new Gear(_objCharacter)
            {
                Name = "External Source"
            };

            string ammoString = CalculatedAmmo(GlobalOptions.CultureInfo, GlobalOptions.DefaultLanguage);
            // Determine which loading methods are available to the Weapon.
            if (ammoString.IndexOfAny('x', '+') != -1 || ammoString.Contains(" or ") || ammoString.Contains("Special"))
            {
                string strWeaponAmmo = ammoString.ToLowerInvariant();
                if (strWeaponAmmo.Contains("external source"))
                    blnExternalSource = true;
                // Get rid of external source, special, or belt, and + energy.
                strWeaponAmmo = strWeaponAmmo.Replace("external source", "100")
                    .Replace("special", "100")
                    .FastEscapeOnceFromEnd(" + energy")
                    .Replace(" or belt", " or 250(belt)");

                string[] strAmmos = strWeaponAmmo.Split(new[] { " or " }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string strAmmo in strAmmos)
                {
                    lstCount.Add(AmmoCapacity(strAmmo));
                }
            }
            else
            {
                // Nothing weird in the ammo string, so just use the number given.
                string strAmmo = ammoString;
                int intPos = strAmmo.IndexOf('(');
                if (intPos != -1)
                    strAmmo = strAmmo.Substring(0, intPos);
                lstCount.Add(strAmmo);
            }

            if (!RequireAmmo)
            {
                // If the Weapon does not require Ammo, just use External Source.
                lstAmmo.Add(objExternalSource);
            }
            else
            {
                // Find all of the Ammo for the current Weapon that the character is carrying.
                HashSet<string> setAmmoPrefixStringSet = new HashSet<string>(AmmoPrefixStrings);
                // This is a standard Weapon, so consume traditional Ammunition.
                lstAmmo.AddRange(lstGears.DeepWhere(x => x.Children, x =>
                    x.Quantity > 0 && (x.Category == "Ammunition" && x.Extra == AmmoCategory ||
                                       !string.IsNullOrWhiteSpace(AmmoName) && x.Name == AmmoName ||
                                       string.IsNullOrEmpty(x.Extra) && setAmmoPrefixStringSet.Any(y => x.Name.StartsWith(y, StringComparison.Ordinal)) ||
                                       UseSkill == "Throwing Weapons" && Name == x.Name)));

                // If the Weapon is allowed to use an External Source, put in an External Source item.
                if (blnExternalSource)
                {
                    lstAmmo.Add(objExternalSource);
                }

                // Make sure the character has some form of Ammunition for this Weapon.
                if (lstAmmo.Count == 0)
                {
                    Program.MainForm.ShowMessageBox(string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_OutOfAmmoType"), DisplayAmmoCategory(GlobalOptions.Language)),
                        LanguageManager.GetString("MessageTitle_OutOfAmmo"), MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }
            }

            // Show the Ammunition Selection window.
            using (frmReload frmReloadWeapon = new frmReload
            {
                Ammo = lstAmmo,
                Count = lstCount
            })
            {
                frmReloadWeapon.ShowDialog();

                if (frmReloadWeapon.DialogResult == DialogResult.Cancel)
                    return;

                // Return any unspent rounds to the Ammo.
                if (AmmoRemaining > 0)
                {
                    foreach (Gear objAmmo in lstGears)
                    {
                        if (objAmmo.InternalId == AmmoLoaded)
                        {
                            objAmmo.Quantity += AmmoRemaining;

                            // Refresh the Gear tree.
                            TreeNode objNode = treGearView.FindNode(objAmmo.InternalId);
                            if (objNode != null)
                            {
                                objNode.Text = objAmmo.CurrentDisplayName;
                            }

                            break;
                        }

                        foreach (Gear objChild in objAmmo.Children.GetAllDescendants(x => x.Children))
                        {
                            if (objChild.InternalId == AmmoLoaded)
                            {
                                // If this is a plugin for a Spare Clip, move any extra rounds to the character instead of messing with the Clip amount.
                                if (objChild.Parent is Gear parent &&
                                    (parent.Name.StartsWith("Spare Clip", StringComparison.Ordinal) || parent.Name.StartsWith("Speed Loader", StringComparison.Ordinal)))
                                {
                                    Gear objNewGear = new Gear(_objCharacter);
                                    objNewGear.Copy(objChild);
                                    objNewGear.Quantity = AmmoRemaining;
                                    lstGears.Add(objNewGear);

                                    goto EndLoop;
                                }

                                objChild.Quantity += AmmoRemaining;

                                // Refresh the Gear tree.
                                TreeNode objNode = treGearView.FindNode(objChild.InternalId);
                                if (objNode != null)
                                {
                                    objNode.Text = objAmmo.CurrentDisplayName;
                                }

                                break;
                            }
                        }
                    }

                    EndLoop: ;
                }

                Gear objSelectedAmmo;
                decimal decQty = frmReloadWeapon.SelectedCount;
                // If an External Source is not being used, consume ammo.
                if (frmReloadWeapon.SelectedAmmo != objExternalSource.InternalId)
                {
                    objSelectedAmmo = lstGears.DeepFindById(frmReloadWeapon.SelectedAmmo);

                    if (objSelectedAmmo.Quantity == decQty && objSelectedAmmo.Parent != null)
                    {
                        // If the Ammo is coming from a Spare Clip, reduce the container quantity instead of the plugin quantity.
                        if (objSelectedAmmo.Parent is Gear objParent &&
                            (objParent.Name.StartsWith("Spare Clip", StringComparison.Ordinal) || objParent.Name.StartsWith("Speed Loader", StringComparison.Ordinal)))
                        {
                            if (objParent.Quantity > 0)
                                objParent.Quantity -= 1;
                            TreeNode objNode = treGearView.FindNode(objParent.InternalId);
                            objNode.Text = objParent.CurrentDisplayName;
                        }
                    }
                    else
                    {
                        // Deduct the ammo qty from the ammo. If there isn't enough remaining, use whatever is left.
                        if (objSelectedAmmo.Quantity > decQty)
                            objSelectedAmmo.Quantity -= decQty;
                        else
                        {
                            decQty = objSelectedAmmo.Quantity;
                            objSelectedAmmo.Quantity = 0;
                        }
                    }

                    // Refresh the Gear tree.
                    TreeNode objSelectedNode = treGearView.FindNode(objSelectedAmmo.InternalId);
                    if (objSelectedNode != null)
                        objSelectedNode.Text = objSelectedAmmo.CurrentDisplayName;
                }
                else
                {
                    objSelectedAmmo = objExternalSource;
                }

                AmmoRemaining = decimal.ToInt32(decQty);
                AmmoLoaded = objSelectedAmmo.InternalId;
            }
        }

        #region UI Methods
        /// <summary>
        /// Add a Weapon to the TreeView.
        /// </summary>
        /// <param name="cmsWeapon">ContextMenuStrip for the Weapon Node.</param>
        /// <param name="cmsWeaponAccessory">ContextMenuStrip for Vehicle Accessory Nodes.</param>
        /// <param name="cmsWeaponAccessoryGear">ContextMenuStrip for Vehicle Weapon Accessory Gear Nodes.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsWeapon, ContextMenuStrip cmsWeaponAccessory, ContextMenuStrip cmsWeaponAccessoryGear)
        {
            if ((Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID)) && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsWeapon,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            TreeNodeCollection lstChildNodes = objNode.Nodes;
            // Add Underbarrel Weapons.
            foreach (Weapon objUnderbarrelWeapon in UnderbarrelWeapons)
            {
                TreeNode objLoopNode = objUnderbarrelWeapon.CreateTreeNode(cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                if (objLoopNode != null)
                    lstChildNodes.Add(objLoopNode);
            }
            // Add attached Weapon Accessories.
            foreach (WeaponAccessory objAccessory in WeaponAccessories)
            {
                TreeNode objLoopNode = objAccessory.CreateTreeNode(cmsWeaponAccessory, cmsWeaponAccessoryGear);
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
                if (Cyberware || Category == "Gear" || Category.StartsWith("Quality", StringComparison.Ordinal) || !string.IsNullOrEmpty(ParentID))
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }

        public void SetupChildrenWeaponsCollectionChanged(bool blnAdd, TreeView treWeapons, ContextMenuStrip cmsWeapon = null, ContextMenuStrip cmsWeaponAccessory = null, ContextMenuStrip cmsWeaponAccessoryGear = null)
        {
            if (blnAdd)
            {
                UnderbarrelWeapons.AddTaggedCollectionChanged(treWeapons, (x, y) => this.RefreshChildrenWeapons(treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear, null, y));
                WeaponAccessories.AddTaggedCollectionChanged(treWeapons, (x, y) => this.RefreshWeaponAccessories(treWeapons, cmsWeaponAccessory, cmsWeaponAccessoryGear, () => UnderbarrelWeapons.Count, y));
                foreach (Weapon objChild in UnderbarrelWeapons)
                {
                    objChild.SetupChildrenWeaponsCollectionChanged(true, treWeapons, cmsWeapon, cmsWeaponAccessory, cmsWeaponAccessoryGear);
                }

                foreach (WeaponAccessory objChild in WeaponAccessories)
                {
                    objChild.Gear.AddTaggedCollectionChanged(treWeapons, (x, y) => objChild.RefreshChildrenGears(treWeapons, cmsWeaponAccessoryGear, null, y));
                    foreach (Gear objGear in objChild.Gear)
                        objGear.SetupChildrenGearsCollectionChanged(true, treWeapons, cmsWeaponAccessoryGear);
                }
            }
            else
            {
                UnderbarrelWeapons.RemoveTaggedCollectionChanged(treWeapons);
                WeaponAccessories.RemoveTaggedCollectionChanged(treWeapons);
                foreach (Weapon objChild in UnderbarrelWeapons)
                {
                    objChild.SetupChildrenWeaponsCollectionChanged(false, treWeapons);
                }
                foreach (WeaponAccessory objChild in WeaponAccessories)
                {
                    objChild.Gear.RemoveTaggedCollectionChanged(treWeapons);
                    foreach (Gear objGear in objChild.Gear)
                        objGear.SetupChildrenGearsCollectionChanged(false, treWeapons);
                }
            }
        }
        #endregion

        #region Hero Lab Importing
        public bool ImportHeroLabWeapon(XmlNode xmlWeaponImportNode, IList<Weapon> lstWeapons)
        {
            if (xmlWeaponImportNode == null)
                return false;
            string strOriginalName = xmlWeaponImportNode.Attributes?["name"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(strOriginalName))
            {
                XmlDocument xmlWeaponDocument = _objCharacter.LoadData("weapons.xml");
                XmlNode xmlWeaponDataNode = xmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strOriginalName + "\"]");
                if (xmlWeaponDataNode == null)
                {
                    string[] astrOriginalNameSplit = strOriginalName.Split(':');
                    if (astrOriginalNameSplit.Length > 1)
                    {
                        string strName = astrOriginalNameSplit[0].Trim();
                        xmlWeaponDataNode = xmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strName + "\"]");
                    }
                    if (xmlWeaponDataNode == null)
                    {
                        astrOriginalNameSplit = strOriginalName.Split(',');
                        if (astrOriginalNameSplit.Length > 1)
                        {
                            string strName = astrOriginalNameSplit[0].Trim();
                            xmlWeaponDataNode = xmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strName + "\"]");
                        }
                    }
                }
                if (xmlWeaponDataNode != null)
                {
                    Create(xmlWeaponDataNode, lstWeapons, true, true, true);
                    if (Cost.Contains("Variable"))
                    {
                        Cost = xmlWeaponImportNode.SelectSingleNode("gearcost/@value")?.InnerText;
                    }
                    Notes = xmlWeaponImportNode["description"]?.InnerText;

                    ProcessHeroLabWeaponPlugins(xmlWeaponImportNode, lstWeapons);

                    return true;
                }
            }
            return false;
        }

        public void ProcessHeroLabWeaponPlugins(XmlNode xmlWeaponImportNode, IList<Weapon> lstWeapons)
        {
            if (xmlWeaponImportNode == null)
                return;
            XmlNode xmlWeaponDataNode = GetNode();
            foreach (string strName in Character.HeroLabPluginNodeNames)
            {
                foreach (XmlNode xmlWeaponAccessoryToImport in xmlWeaponImportNode.SelectNodes(strName + "/item[@useradded != \"no\"]"))
                {
                    Weapon objUnderbarrel = new Weapon(_objCharacter);
                    if (objUnderbarrel.ImportHeroLabWeapon(xmlWeaponAccessoryToImport, lstWeapons))
                    {
                        objUnderbarrel.Parent = this;
                        UnderbarrelWeapons.Add(objUnderbarrel);
                    }
                    else
                    {
                        string strWeaponAccessoryName = xmlWeaponImportNode.Attributes["name"]?.InnerText;
                        if (!string.IsNullOrEmpty(strWeaponAccessoryName))
                        {
                            XmlDocument xmlWeaponDocument = _objCharacter.LoadData("weapons.xml");
                            XmlNode xmlWeaponAccessoryData = null;
                            foreach (XmlNode xmlLoopNode in xmlWeaponDocument.SelectNodes("chummer/accessories/accessory[contains(name, \"" + strWeaponAccessoryName + "\")]"))
                            {
                                XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/weapondetails");
                                if (xmlTestNode != null)
                                {
                                    // Assumes topmost parent is an AND node
                                    if (xmlWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        continue;
                                    }
                                }
                                xmlTestNode = xmlLoopNode.SelectSingleNode("required/weapondetails");
                                if (xmlTestNode != null)
                                {
                                    // Assumes topmost parent is an AND node
                                    if (!xmlWeaponDataNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        continue;
                                    }
                                }

                                xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/oneof");
                                if (xmlTestNode != null)
                                {
                                    XmlNodeList objXmlForbiddenList = xmlTestNode.SelectNodes("accessory");
                                    //Add to set for O(N log M) runtime instead of O(N * M)

                                    HashSet<string> objForbiddenAccessory = new HashSet<string>();
                                    foreach (XmlNode node in objXmlForbiddenList)
                                    {
                                        objForbiddenAccessory.Add(node.InnerText);
                                    }

                                    if (WeaponAccessories.Any(objAccessory => objForbiddenAccessory.Contains(objAccessory.Name)))
                                    {
                                        continue;
                                    }
                                }

                                xmlTestNode = xmlLoopNode.SelectSingleNode("required/oneof");
                                if (xmlTestNode != null)
                                {
                                    XmlNodeList objXmlRequiredList = xmlTestNode.SelectNodes("accessory");
                                    //Add to set for O(N log M) runtime instead of O(N * M)

                                    HashSet<string> objRequiredAccessory = new HashSet<string>();
                                    foreach (XmlNode node in objXmlRequiredList)
                                    {
                                        objRequiredAccessory.Add(node.InnerText);
                                    }

                                    if (!WeaponAccessories.Any(objAccessory => objRequiredAccessory.Contains(objAccessory.Name)))
                                    {
                                        continue;
                                    }
                                }
                                xmlWeaponAccessoryData = xmlLoopNode;
                                break;
                            }
                            if (xmlWeaponAccessoryData != null)
                            {
                                WeaponAccessory objWeaponAccessory = new WeaponAccessory(_objCharacter);
                                string strMainMount = xmlWeaponAccessoryData["mount"]?.InnerText.Split('/').FirstOrDefault() ?? string.Empty;
                                string strExtraMount = xmlWeaponAccessoryData["extramount"]?.InnerText.Split('/').FirstOrDefault(x => x != strMainMount) ?? string.Empty;

                                objWeaponAccessory.Create(xmlWeaponAccessoryData, new Tuple<string, string>(strMainMount, strExtraMount), Convert.ToInt32(xmlWeaponAccessoryToImport.Attributes["rating"]?.InnerText, GlobalOptions.InvariantCultureInfo));
                                objWeaponAccessory.Notes = xmlWeaponAccessoryToImport["description"]?.InnerText;
                                objWeaponAccessory.Parent = this;
                                WeaponAccessories.Add(objWeaponAccessory);

                                foreach (string strPluginName in Character.HeroLabPluginNodeNames)
                                {
                                    foreach (XmlNode xmlPluginToAdd in xmlWeaponAccessoryToImport.SelectNodes(strPluginName + "/item[@useradded != \"no\"]"))
                                    {
                                        Gear objPlugin = new Gear(_objCharacter);
                                        if (objPlugin.ImportHeroLabGear(xmlPluginToAdd, xmlWeaponAccessoryData, lstWeapons))
                                            objWeaponAccessory.Gear.Add(objPlugin);
                                    }
                                    foreach (XmlNode xmlPluginToAdd in xmlWeaponAccessoryToImport.SelectNodes(strPluginName + "/item[@useradded = \"no\"]"))
                                    {
                                        string strGearName = xmlPluginToAdd.Attributes["name"]?.InnerText;
                                        if (!string.IsNullOrEmpty(strGearName))
                                        {
                                            Gear objPlugin = objWeaponAccessory.Gear.FirstOrDefault(x => x.IncludedInParent && !string.IsNullOrEmpty(x.Name) && (x.Name.Contains(strGearName) || strGearName.Contains(x.Name)));
                                            if (objPlugin != null)
                                            {
                                                objPlugin.Quantity = Convert.ToDecimal(xmlPluginToAdd.Attributes["quantity"]?.InnerText ?? "1", GlobalOptions.InvariantCultureInfo);
                                                objPlugin.Notes = xmlPluginToAdd["description"]?.InnerText;
                                                objPlugin.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Gear objPlugin = new Gear(_objCharacter);
                                if (objPlugin.ImportHeroLabGear(xmlWeaponAccessoryToImport, null, lstWeapons))
                                    _objCharacter.Gear.Add(objPlugin);
                            }
                        }
                    }
                }
                foreach (XmlNode xmlPluginToAdd in xmlWeaponImportNode.SelectNodes(strName + "/item[@useradded = \"no\"]"))
                {
                    string strPluginName = xmlWeaponImportNode.Attributes["name"]?.InnerText;
                    if (!string.IsNullOrEmpty(strPluginName))
                    {
                        Weapon objUnderbarrel = UnderbarrelWeapons.FirstOrDefault(x => x.IncludedInWeapon && (x.Name.Contains(strPluginName) || strPluginName.Contains(x.Name)));
                        if (objUnderbarrel != null)
                        {
                            objUnderbarrel.Notes = xmlPluginToAdd["description"]?.InnerText;
                            objUnderbarrel.ProcessHeroLabWeaponPlugins(xmlPluginToAdd, lstWeapons);
                        }
                        else
                        {
                            WeaponAccessory objWeaponAccessory = WeaponAccessories.FirstOrDefault(x => x.IncludedInWeapon && (x.Name.Contains(strPluginName) || strPluginName.Contains(x.Name)));
                            if (objWeaponAccessory != null)
                            {
                                objWeaponAccessory.Notes = xmlPluginToAdd["description"]?.InnerText;

                                foreach (string strPluginNodeName in Character.HeroLabPluginNodeNames)
                                {
                                    foreach (XmlNode xmlSubPluginToAdd in xmlPluginToAdd.SelectNodes(strPluginNodeName + "/item[@useradded != \"no\"]"))
                                    {
                                        Gear objPlugin = new Gear(_objCharacter);
                                        if (objPlugin.ImportHeroLabGear(xmlSubPluginToAdd, objWeaponAccessory.GetNode(), lstWeapons))
                                            objWeaponAccessory.Gear.Add(objPlugin);
                                    }
                                    foreach (XmlNode xmlSubPluginToAdd in xmlPluginToAdd.SelectNodes(strPluginNodeName + "/item[@useradded = \"no\"]"))
                                    {
                                        string strGearName = xmlSubPluginToAdd.Attributes["name"]?.InnerText;
                                        if (!string.IsNullOrEmpty(strGearName))
                                        {
                                            Gear objPlugin = objWeaponAccessory.Gear.FirstOrDefault(x => x.IncludedInParent && !string.IsNullOrEmpty(x.Name) && (x.Name.Contains(strGearName) || strGearName.Contains(x.Name)));
                                            if (objPlugin != null)
                                            {
                                                objPlugin.Quantity = Convert.ToDecimal(xmlSubPluginToAdd.Attributes["quantity"]?.InnerText ?? "1", GlobalOptions.InvariantCultureInfo);
                                                objPlugin.Notes = xmlSubPluginToAdd["description"]?.InnerText;
                                                objPlugin.ProcessHeroLabGearPlugins(xmlSubPluginToAdd, lstWeapons);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(strName))
                            {
                                Gear objPlugin = objWeaponAccessory.Gear.FirstOrDefault(x => x.IncludedInParent && !string.IsNullOrEmpty(x.Name) && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                if (objPlugin != null)
                                {
                                    objPlugin.Quantity = Convert.ToDecimal(xmlPluginToAdd.Attributes["quantity"]?.InnerText ?? "1", GlobalOptions.InvariantCultureInfo);
                                    objPlugin.Notes = xmlPluginToAdd["description"]?.InnerText;
                                    objPlugin.ProcessHeroLabGearPlugins(xmlPluginToAdd, lstWeapons);
                                }
                            }
                        }
                    }
                }
            }
            this.RefreshMatrixAttributeArray();
        }
        #endregion
        #endregion

        private Clip GetClip(int clip)
        {
            //1 indexed due legacy
            clip--;

            for (int i = _lstAmmo.Count; i <= clip; i++)
            {
                _lstAmmo.Add(new Clip(Guid.Empty, 0));
            }


            return _lstAmmo[clip];
        }

        IHasMatrixAttributes GetMatrixAttributesOverride
        {
            get
            {
                IHasMatrixAttributes objReturn = null;
                if (!string.IsNullOrEmpty(ParentID))
                {
                    objReturn = (_objCharacter.Gear.DeepFindById(ParentID) ??
                                 _objCharacter.Vehicles.FindVehicleGear(ParentID) ??
                                 _objCharacter.Weapons.FindWeaponGear(ParentID) ??
                                 _objCharacter.Armor.FindArmorGear(ParentID) ??
                                 _objCharacter.Cyberware.FindCyberwareGear(ParentID)) ??
                                ((_objCharacter.Cyberware.DeepFindById(ParentID) ??
                                  _objCharacter.Vehicles.FindVehicleCyberware(x => x.InternalId == ParentID)) ??
                                 ((_objCharacter.Weapons.DeepFindById(ParentID) ??
                                   _objCharacter.Vehicles.FindVehicleWeapon(ParentID)) ?? (IHasMatrixAttributes) _objCharacter.Vehicles.FirstOrDefault(x => x.InternalId == ParentID)));
                }
                return objReturn;
            }
        }

        public decimal StolenTotalCost
        {
            get
            {
                decimal decReturn = 0;
                if (Stolen)
                    decReturn += OwnCost;

                // Run through the Accessories and add in their cost. If the cost is "Weapon Cost", the Weapon's base cost is added in again.
                decReturn += WeaponAccessories.AsParallel().Sum(objAccessory => objAccessory.StolenTotalCost);

                // Include the cost of any Underbarrel Weapon.
                decReturn += Children.AsParallel().Sum(objUnderbarrel => objUnderbarrel.StolenTotalCost);

                return decReturn;
            }
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
            if (objThis != null)
                return objThis.GetBaseMatrixAttribute(strAttributeName);
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExpression = "2";
                        break;
                    case "Program Limit":
                        if (IsCommlink)
                        {
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                strExpression = "2";
                        }
                        else
                            strExpression = "0";
                        break;
                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            strExpression = "2";
                        break;
                    default:
                        strExpression = "0";
                        break;
                }
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                StringBuilder objValue = new StringBuilder(strExpression);
                foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + "}", () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0).ToString(GlobalOptions.InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + "}", () => Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0");
                    if (Children.Count > 0 && strExpression.Contains("{Children " + strMatrixAttribute + "}"))
                    {
                        int intTotalChildrenValue = 0;
                        foreach (Weapon objLoopWeapon in Children)
                        {
                            if (objLoopWeapon.Equipped)
                            {
                                intTotalChildrenValue += objLoopWeapon.GetBaseMatrixAttribute(strMatrixAttribute);
                            }
                        }
                        objValue.Replace("{Children " + strMatrixAttribute + "}", intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    }
                }
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "}", () => _objCharacter.GetAttribute(strCharAttributeName).TotalValue.ToString(GlobalOptions.InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "Base}", () => _objCharacter.GetAttribute(strCharAttributeName).TotalBase.ToString(GlobalOptions.InvariantCultureInfo));
                }
                // Replace the division sign with "div" since we're using XPath.
                objValue.Replace("/", " div ");
                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objValue.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToInt32(Math.Ceiling((double)objProcess)) : 0;
            }
            int.TryParse(strExpression, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intReturn);
            return intReturn;
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            if (string.IsNullOrEmpty(strAttributeName))
                return 0;
            IHasMatrixAttributes objThis = GetMatrixAttributesOverride;
            if (objThis != null)
                return objThis.GetBonusMatrixAttribute(strAttributeName);
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                intReturn += 1;
            }

            if (!strAttributeName.StartsWith("Mod ", StringComparison.Ordinal))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Weapon objLoopWeapon in Children)
            {
                if (objLoopWeapon.Equipped && objLoopWeapon.ParentID != InternalId)
                {
                    intReturn += objLoopWeapon.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        private sealed class Clip
        {
            internal Guid Guid { get; set; }
            internal int Ammo { get; set; }
            public string AmmoName { get; internal set; }

            internal static Clip Load(XmlNode node)
            {
                if (node != null)
                {
                    string strId = node["id"]?.InnerText;
                    string strCount = node["count"]?.InnerText;
                    if (!string.IsNullOrEmpty(strId) && !string.IsNullOrEmpty(strCount) && Guid.TryParse(strId, out Guid guiClipId) && int.TryParse(strCount, out int intCount))
                    {
                        return new Clip(guiClipId, intCount);
                    }
                }
                return null;
            }

            internal void Save(XmlTextWriter writer)
            {
                if (Guid != Guid.Empty || Ammo != 0) //Don't save empty clips, we are recreating them anyway. Save those kb
                {
                    writer.WriteStartElement("clip");
                    writer.WriteElementString("name", AmmoName);
                    writer.WriteElementString("id", Guid.ToString("D", GlobalOptions.InvariantCultureInfo));
                    writer.WriteElementString("count", Ammo.ToString(GlobalOptions.InvariantCultureInfo));
                    writer.WriteEndElement();
                }
            }

            internal Clip(Guid guid, int ammo)
            {
                Guid = guid;
                Ammo = ammo;
            }
        }

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (!CanBeRemoved)
                return false;
            if (blnConfirmDelete)
            {
                if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteWeapon")))
                    return false;
            }

            DeleteWeapon();
            if (_objCharacter.Weapons.Any(weapon => weapon == this))
            {
                return _objCharacter.Weapons.Remove(this);
            }
            if (Parent != null)
                return Parent.Children.Remove(this);
            return ParentMount?.Weapons.Remove(this) ?? ParentVehicle.Weapons.Remove(this);
            //else if (objWeapon.parent != null)
            //    objWeaponMount.Weapons.Remove(objWeapon);
            // This bit here should never be reached, but I'm adding it for future-proofing in case we want people to be able to remove weapons attached directly to vehicles
        }

        /// <summary>
        /// Check whether the weapon is not removable due to various restrictions.
        /// </summary>
        /// <returns></returns>
        private bool CanBeRemoved
        {
            get
            {
                // Cyberweapons cannot be removed through here and must be done by removing the piece of Cyberware.
                if (Cyberware)
                {
                    Program.MainForm.ShowMessageBox(
                        LanguageManager.GetString("Message_CannotRemoveCyberweapon"),
                        LanguageManager.GetString("MessageTitle_CannotRemoveCyberweapon"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                // Qualities cannot be removed through here and must be done by removing the piece of Cyberware.
                if (Category.StartsWith("Quality", StringComparison.Ordinal))
                {
                    Program.MainForm.ShowMessageBox(
                        LanguageManager.GetString("Message_CannotRemoveQualityWeapon"),
                        LanguageManager.GetString("MessageTitle_CannotRemoveQualityWeapon"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                if (Category == "Gear")
                {
                    string message = LanguageManager.GetString(
                        ParentVehicle != null ? "Message_CannotRemoveGearWeaponVehicle" : "Message_CannotRemoveGearWeapon",
                        GlobalOptions.Language);
                    Program.MainForm.ShowMessageBox(message,
                        LanguageManager.GetString("MessageTitle_CannotRemoveGearWeapon"),
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }

                return true;
            }
        }

        public void Sell(decimal percentage)
        {
            decimal decAmount = TotalCost * percentage;
            string expense = ParentVehicle != null ? "String_ExpenseSoldVehicleWeapon" : "String_ExpenseSoldWeapon";
            if (!Remove()) return;

            // Create the Expense Log Entry for the sale.
            ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
            objExpense.Create(decAmount, LanguageManager.GetString(expense) + ' ' + DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen, DateTime.Now);
            _objCharacter.ExpenseEntries.AddWithSort(objExpense);
            _objCharacter.Nuyen += decAmount;
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }

        public bool AllowPasteXml
        {
            get
            {
                switch (GlobalOptions.ClipboardContentType)
                {
                    case ClipboardContentType.WeaponAccessory:
                        XPathNavigator checkNode = GlobalOptions.Clipboard.SelectSingleNode("/character/weaponaccessories/accessory")?.CreateNavigator();
                        if (CheckAccessoryRequirements(checkNode)) return true;
                        break;
                    default:
                        return false;
                }

                return false;
            }
        }

        public bool AllowPasteObject(object input)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks whether a given WeaponAccessory is allowed to be added to this weapon.
        /// </summary>
        public bool CheckAccessoryRequirements(XPathNavigator objXmlAccessory)
        {
            if (objXmlAccessory == null) return false;
            List<string> lstMounts = AccessoryMounts.Split('/').ToList();
            XPathNavigator xmlMountNode = objXmlAccessory.SelectSingleNode("mount");
            if (lstMounts.Count == 0 || xmlMountNode != null && xmlMountNode.Value.Split('/').All(strItem =>
                !string.IsNullOrEmpty(strItem) && lstMounts.All(strAllowedMount =>
                    strAllowedMount != strItem)))
            {
                return false;
            }
            xmlMountNode = objXmlAccessory.SelectSingleNode("extramount");
            if (lstMounts.Count == 0 || xmlMountNode != null && xmlMountNode.Value.Split('/').All(strItem =>
                !string.IsNullOrEmpty(strItem) && lstMounts.All(strAllowedMount =>
                    strAllowedMount != strItem)))
            {
                return false;
            }

            if (!objXmlAccessory.RequirementsMet(_objCharacter, this, string.Empty, string.Empty)) return false;

            XPathNavigator xmlTestNode = objXmlAccessory.SelectSingleNode("forbidden/weapondetails");
            if (xmlTestNode != null)
            {
                // Assumes topmost parent is an AND node
                if (GetNode().CreateNavigator().ProcessFilterOperationNode(xmlTestNode, false))
                {
                    return false;
                }
            }

            xmlTestNode = objXmlAccessory.SelectSingleNode("required/weapondetails");
            if (xmlTestNode != null)
            {
                // Assumes topmost parent is an AND node
                if (!GetNode().CreateNavigator().ProcessFilterOperationNode(xmlTestNode, false))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
