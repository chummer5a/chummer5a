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
using Chummer.Backend.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Chummer.Annotations;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Standard Character Gear.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.InvariantCultureInfo, GlobalOptions.DefaultLanguage)}")]
    public class Gear : IHasChildrenAndCost<Gear>, IHasName, IHasInternalId, IHasXmlNode, IHasMatrixAttributes, IHasNotes, ICanSell, IHasLocation, ICanEquip, IHasSource, IHasRating, INotifyMultiplePropertyChanged, ICanSort
    {
        private Guid _guiID;
        private string _SourceGuid;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strMaxRating = string.Empty;
        private string _strMinRating = string.Empty;
        private int _intRating;
        private decimal _decQty = 1.0m;
        private string _strCapacity = string.Empty;
        private string _strArmorCapacity = string.Empty;
        private string _strAvail = string.Empty;
        private decimal _decCostFor = 1.0m;
        private string _strDeviceRating = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strExtra = string.Empty;
        private string _strCanFormPersona = string.Empty;
        private bool _blnBonded;
        private bool _blnEquipped = true;
        private bool _blnWirelessOn = false;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private XmlNode _nodWeaponBonus;
        private Guid _guiWeaponID = Guid.Empty;
        private readonly TaggedObservableCollection<Gear> _lstChildren = new TaggedObservableCollection<Gear>();
        private string _strNotes = string.Empty;
        private Location _objLocation;
        private readonly Character _objCharacter;
        private int _intChildCostMultiplier = 1;
        private int _intChildAvailModifier;
        private bool _blnDiscountCost;
        private string _strGearName = string.Empty;
        private string _strParentID = string.Empty;
        private int _intMatrixCMBonus;
        private int _intMatrixCMFilled;
        private string _strForcedValue = string.Empty;
        private bool _blnAllowRename;
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
        private int _intSortOrder;

        #region Constructor, Create, Save, Load, and Print Methods
        public Gear(Character objCharacter)
        {
            // Create the GUID for the new piece of Gear.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;

            _lstChildren.CollectionChanged += ChildrenOnCollectionChanged;
        }

        private void ChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Gear objNewItem in e.NewItems)
                        objNewItem.Parent = this;
                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Gear objOldItem in e.OldItems)
                        objOldItem.Parent = null;
                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (Gear objOldItem in e.OldItems)
                        objOldItem.Parent = null;
                    foreach (Gear objNewItem in e.NewItems)
                        objNewItem.Parent = this;
                    this.RefreshMatrixAttributeArray();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.RefreshMatrixAttributeArray();
                    break;
            }
        }

        /// Create a Gear from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlGear">XmlNode to create the object from.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="lstWeapons">List of Weapons that should be added to the character.</param>
        /// <param name="strForceValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnAddImprovements">Whether or not Improvements should be added to the character.</param>
        /// <param name="blnCreateChildren">Whether or not child Gear should be created.</param>
        public void Create(XmlNode objXmlGear, int intRating, IList<Weapon> lstWeapons, string strForceValue = "", bool blnAddImprovements = true, bool blnCreateChildren = true)
        {
            if (objXmlGear == null)
                return;
            _strForcedValue = strForceValue;
            XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
            if (objXmlGear.TryGetStringFieldQuickly("id", ref _SourceGuid))
                _objCachedMyXmlNode = null;
            if (objXmlGear.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (objXmlGear.TryGetStringFieldQuickly("category", ref _strCategory))
                _objCachedMyXmlNode = null;
            objXmlGear.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlGear.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objXmlGear.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objXmlGear.TryGetDecFieldQuickly("costfor", ref _decCostFor);
            _decQty = _decCostFor;
            objXmlGear.TryGetStringFieldQuickly("cost", ref _strCost);
            _nodBonus = objXmlGear["bonus"];
            _nodWirelessBonus = objXmlGear["wirelessbonus"];
            _blnWirelessOn = false;
			objXmlGear.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            if (_strMaxRating == "0")
                _strMaxRating = string.Empty;
            objXmlGear.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            if (!objXmlGear.TryGetStringFieldQuickly("altnotes", ref _strNotes))
                objXmlGear.TryGetStringFieldQuickly("notes", ref _strNotes);
            _intRating = Math.Max(Math.Min(intRating, MaxRatingValue), MinRatingValue);
            objXmlGear.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            objXmlGear.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
            objXmlGear.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlGear.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlGear.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            objXmlGear.TryGetInt32FieldQuickly("childcostmultiplier", ref _intChildCostMultiplier);
            objXmlGear.TryGetInt32FieldQuickly("childavailmodifier", ref _intChildAvailModifier);
            objXmlGear.TryGetBoolFieldQuickly("allowrename", ref _blnAllowRename);
            
            // Check for a Custom name
            if (_strName == "Custom Item")
            {
                if (string.IsNullOrEmpty(_strForcedValue))
                {
                    frmSelectText frmPickText = new frmSelectText
                    {
                        PreventXPathErrors = true,
                        Description = LanguageManager.GetString("String_CustomItem_SelectText", GlobalOptions.Language)
                    };
                    frmPickText.ShowDialog();

                    // Make sure the dialogue window was not canceled.
                    if (frmPickText.DialogResult != DialogResult.Cancel)
                    {
                        string strCustomName = LanguageManager.GetString(frmPickText.SelectedValue, GlobalOptions.DefaultLanguage, false);
                        if (string.IsNullOrEmpty(strCustomName))
                            strCustomName = LanguageManager.ReverseTranslateExtra(frmPickText.SelectedValue, GlobalOptions.Language);
                        _strName = strCustomName;
                        _objCachedMyXmlNode = null;
                    }
                }
                else
                {
                    string strCustomName = LanguageManager.GetString(_strForcedValue, GlobalOptions.DefaultLanguage, false);
                    if (string.IsNullOrEmpty(strCustomName))
                        strCustomName = _strForcedValue;
                    _strName = strCustomName;
                    _objCachedMyXmlNode = null;
                }
            }
            // Check for a Variable Cost.
            if (!string.IsNullOrEmpty(_strCost))
            {
                if (_strCost.StartsWith("Variable(") && string.IsNullOrEmpty(_strForcedValue))
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

            string strSource = _guiID.ToString("D");

            // If the Gear is Ammunition, ask the user to select a Weapon Category for it to be limited to.
            if (_strCategory == "Ammunition" && (_strName.StartsWith("Ammo:") || _strName.StartsWith("Arrow:") || _strName.StartsWith("Bolt:")))
            {
                frmSelectWeaponCategory frmPickWeaponCategory = new frmSelectWeaponCategory
                {
                    Description = LanguageManager.GetString("String_SelectWeaponCategoryAmmo", GlobalOptions.Language)
                };
                if (!string.IsNullOrEmpty(_strForcedValue) && !_strForcedValue.Equals(_strName))
                    frmPickWeaponCategory.OnlyCategory = _strForcedValue;

                //should really go in a data file
                if (_strName.StartsWith("Ammo:"))
                {
                    if (_strName.StartsWith("Ammo: Assault Cannon") || _strName.StartsWith("Ammo: Gauss"))
                    {
                        frmPickWeaponCategory.WeaponType = "cannon";
                    }
                    else if (_strName.StartsWith("Ammo: Taser Dart"))
                    {
                        frmPickWeaponCategory.WeaponType = "taser";
                    }
                    else if(_strName.StartsWith("Ammo: Fuel Canister"))
                    {
                        frmPickWeaponCategory.WeaponType = "flame";
                    }
                    else if (_strName.StartsWith("Ammo: Injection Dart"))
                    {
                        frmPickWeaponCategory.WeaponType = "exotic";
                    }
                    else if (_strName.StartsWith("Ammo: DMSO Rounds"))
                    {
                        frmPickWeaponCategory.WeaponType = "exotic";
                    }
                    else if (_strName.StartsWith("Ammo: Peak-Discharge"))
                    {
                        frmPickWeaponCategory.WeaponType = "laser";
                    }
                    else
                    {
                        frmPickWeaponCategory.WeaponType = "gun";
                    }
                }
                else if (_strName.StartsWith("Arrow:"))
                {
                    frmPickWeaponCategory.WeaponType = "bow";
                }
                else if (_strName.StartsWith("Bolt:"))
                {
                    frmPickWeaponCategory.WeaponType = "crossbow";
                }
                frmPickWeaponCategory.ShowDialog();

                _strExtra = frmPickWeaponCategory.SelectedCategory;
            }

            // Add Gear Weapons if applicable.
            using (XmlNodeList xmlWeaponList = objXmlGear.SelectNodes("addweapon"))
            {
                if (xmlWeaponList != null)
                {
                    XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                    // More than one Weapon can be added, so loop through all occurrences.
                    foreach (XmlNode objXmlAddWeapon in xmlWeaponList)
                    {
                        string strLoopID = objXmlAddWeapon.InnerText;
                        XmlNode objXmlWeapon = strLoopID.IsGuid()
                            ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = " + strLoopID.CleanXPath() + "]")
                            : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = " + strLoopID.CleanXPath() + "]");

                        if (objXmlWeapon != null)
                        {
                            Weapon objGearWeapon = new Weapon(_objCharacter);
                            objGearWeapon.Create(objXmlWeapon, lstWeapons, true, blnAddImprovements, !blnAddImprovements);
                            objGearWeapon.ParentID = InternalId;
                            objGearWeapon.Cost = "0";
                            lstWeapons.Add(objGearWeapon);

                            Guid.TryParse(objGearWeapon.InternalId, out _guiWeaponID);
                        }
                    }
                }
            }

            // If the item grants a bonus, pass the information to the Improvement Manager.
            if (Bonus != null && blnAddImprovements)
            {
                // Do not apply the Improvements if this is a Focus, unless we're speicifically creating a Weapon Focus. This is to avoid creating the Foci's Improvements twice (once when it's first added
                // to the character which is incorrect, and once when the Focus is actually Bonded).
                bool blnApply = !((_strCategory == "Foci" || _strCategory == "Metamagic Foci") && !_nodBonus.InnerXml.Contains("selectweapon"));

                if (blnApply)
                {
                    ImprovementManager.ForcedValue = _strForcedValue;
                    if (!ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear, strSource, Bonus, false, intRating, DisplayNameShort(GlobalOptions.Language)))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    {
                        _strExtra = ImprovementManager.SelectedValue;
                    }
                }
            }

            // Add the Copy Protection and Registration plugins to the Matrix program. This does not apply if Unwired is not enabled, Hacked is selected, or this is a Suite being added (individual programs will add it to themselves).
            if (blnCreateChildren)
            {
                // Check to see if there are any child elements.
                CreateChildren(objXmlDocument, objXmlGear, blnAddImprovements);
            }

            // If the item grants a Weapon bonus (Ammunition), just fill the WeaponBonus XmlNode.
            _nodWeaponBonus = objXmlGear["weaponbonus"];

            if (!objXmlGear.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlGear.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlGear.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlGear.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlGear.TryGetStringFieldQuickly("firewall", ref _strFirewall);
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
            objXmlGear.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlGear.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlGear.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlGear.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlGear.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlGear.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
        }

        private SourceString _objCachedSourceDetail;
        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == null)
                {
                    string strSource = Source;
                    string strPage = DisplayPage(GlobalOptions.Language);
                    if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                    {
                        _objCachedSourceDetail = new SourceString(strSource, strPage, GlobalOptions.Language);
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                    }
                }

                return _objCachedSourceDetail;
            }
        }

        public void CreateChildren(XmlDocument xmlGearDocument, XmlNode xmlParentGearNode, bool blnAddImprovements)
        {
            XmlNode objGearsNode = xmlParentGearNode["gears"];
            if (objGearsNode != null)
            {
                // Create Gear by looking up the name of the item we're provided with.
                using (XmlNodeList xmlUseGearList = objGearsNode.SelectNodes("usegear"))
                    if (xmlUseGearList != null && xmlUseGearList.Count > 0)
                    {
                        foreach (XmlNode objXmlChild in xmlUseGearList)
                        {
                            CreateChild(xmlGearDocument, objXmlChild, blnAddImprovements);
                        }
                    }
                // Create Gear by choosing from pre-determined lists.
                using (XmlNodeList xmlChooseGearList = objGearsNode.SelectNodes("choosegear"))
                    if (xmlChooseGearList != null && xmlChooseGearList.Count > 0)
                    {
                        bool blnCancelledDialog = false;
                        List<XmlNode> lstChildrenToCreate = new List<XmlNode>();
                        foreach (XmlNode objXmlChooseGearNode in xmlChooseGearList)
                        {
                            // Each list is processed on its own and has usegear members
                            XmlNodeList objXmlNodeList = objXmlChooseGearNode.SelectNodes("usegear");
                            if (objXmlNodeList == null)
                                continue;
                            List<ListItem> lstGears = new List<ListItem>();
                            foreach (XmlNode objChoiceNode in objXmlNodeList)
                            {
                                XmlNode objXmlLoopGear = xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = " + objChoiceNode["name"]?.InnerText.CleanXPath() + " and category = " + objChoiceNode["category"]?.InnerText.CleanXPath() + "]");
                                if (objXmlLoopGear == null)
                                    continue;
                                XmlNode xmlTestNode = objXmlLoopGear.SelectSingleNode("forbidden/geardetails");
                                if (xmlTestNode != null)
                                {
                                    // Assumes topmost parent is an AND node
                                    if (xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        continue;
                                    }
                                }
                                xmlTestNode = objXmlLoopGear.SelectSingleNode("required/geardetails");
                                if (xmlTestNode != null)
                                {
                                    // Assumes topmost parent is an AND node
                                    if (!xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                    {
                                        continue;
                                    }
                                }

                                string strName = objChoiceNode["name"]?.InnerText ?? string.Empty;
                                string strDisplayName = LanguageManager.GetString(strName, GlobalOptions.Language, false);
                                if (string.IsNullOrEmpty(strDisplayName))
                                    strDisplayName = LanguageManager.TranslateExtra(strName, GlobalOptions.Language);
                                lstGears.Add(new ListItem(strName, strDisplayName));
                            }

                            if (lstGears.Count <= 0)
                            {
                                if (objXmlChooseGearNode["required"]?.InnerText == bool.TrueString)
                                {
                                    blnCancelledDialog = true;
                                    break;
                                }
                                continue;
                            }

                            string strChooseGearNodeName = objXmlChooseGearNode["name"]?.InnerText ?? string.Empty;
                            string strFriendlyName = LanguageManager.GetString(strChooseGearNodeName, GlobalOptions.Language, false);
                            if (string.IsNullOrEmpty(strFriendlyName))
                                strFriendlyName = LanguageManager.TranslateExtra(strChooseGearNodeName, GlobalOptions.Language);
                            frmSelectItem frmPickItem = new frmSelectItem
                            {
                                Description = string.Format(LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language), strFriendlyName),
                                GeneralItems = lstGears
                            };

                            frmPickItem.ShowDialog();

                            // Make sure the dialogue window was not canceled.
                            if (frmPickItem.DialogResult == DialogResult.Cancel)
                            {
                                if (objXmlChooseGearNode["required"]?.InnerText == bool.TrueString)
                                {
                                    blnCancelledDialog = true;
                                    break;
                                }
                                continue;
                            }

                            XmlNode objXmlChosenGear = objXmlChooseGearNode.SelectSingleNode("usegear[name = " + frmPickItem.SelectedItem.CleanXPath() + "]");

                            if (objXmlChosenGear == null)
                            {
                                if (objXmlChooseGearNode["required"]?.InnerText == bool.TrueString)
                                {
                                    blnCancelledDialog = true;
                                    break;
                                }
                                continue;
                            }
                            lstChildrenToCreate.Add(objXmlChosenGear);
                        }
                        if (!blnCancelledDialog)
                        {
                            foreach (XmlNode objXmlChild in lstChildrenToCreate)
                            {
                                CreateChild(xmlGearDocument, objXmlChild, blnAddImprovements);
                            }
                        }
                    }
            }
        }

        protected void CreateChild(XmlDocument xmlGearDocument, XmlNode xmlChildNode, bool blnAddImprovements)
        {
            XmlNode xmlChildName = xmlChildNode["name"];
            XmlAttributeCollection xmlChildNameAttributes = xmlChildName?.Attributes;
            XmlNode xmlChildDataNode = xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = " + xmlChildName?.InnerText.CleanXPath() + " and category = " + xmlChildNode["category"]?.InnerText.CleanXPath() + "]");
            if (xmlChildDataNode == null)
                return;
            int intChildRating = Convert.ToInt32(xmlChildNode["rating"]?.InnerText);
            decimal decChildQty = 1;
            string strChildForceSource = xmlChildNode["source"]?.InnerText ?? string.Empty;
            string strChildForcePage = xmlChildNode["page"]?.InnerText ?? string.Empty;
            string strChildForceValue = xmlChildNameAttributes?["select"]?.InnerText ?? string.Empty;
            bool blnCreateChildren = xmlChildNameAttributes?["createchildren"]?.InnerText != bool.FalseString;
            bool blnAddChildImprovements = xmlChildNameAttributes?["addimprovements"]?.InnerText != bool.FalseString && blnAddImprovements;
            if (xmlChildNameAttributes?["qty"] != null)
                decChildQty = Convert.ToDecimal(xmlChildNameAttributes["qty"].InnerText, GlobalOptions.InvariantCultureInfo);

            Gear objChild = new Gear(_objCharacter);
            List<Weapon> lstChildWeapons = new List<Weapon>();
            objChild.Create(xmlChildDataNode, intChildRating, lstChildWeapons, strChildForceValue, blnAddChildImprovements, blnCreateChildren);
            objChild.Quantity = decChildQty;
            objChild.Cost = "0";
            objChild.ParentID = InternalId;
            if (!string.IsNullOrEmpty(strChildForceSource))
                objChild.Source = strChildForceSource;
            if (!string.IsNullOrEmpty(strChildForcePage))
                objChild.Page = strChildForcePage;
            Children.Add(objChild);
            this.RefreshMatrixAttributeArray();

            // Change the Capacity of the child if necessary.
            if (xmlChildNode["capacity"] != null)
                objChild.Capacity = '[' + xmlChildNode["capacity"].InnerText + ']';

            objChild.CreateChildren(xmlGearDocument, xmlChildNode, blnAddChildImprovements);
        }

        /// <summary>
        /// Create a gear from an XmlNode attached to another object type.
        /// </summary>
        /// <param name="xmlGearsDocument">XmlDocument containing information about all possible gear items.</param>
        /// <param name="xmlGearNode">XmlNode containing information about the child gear that needs to be created.</param>
        /// <param name="lstWeapons">List of weapons that this (and other children) gear creates.</param>
        /// <param name="blnAddImprovements">Whether to create improvements for the gear or not (for Selection Windows, set to False).</param>
        /// <returns></returns>
        public bool CreateFromNode(XmlDocument xmlGearsDocument, XmlNode xmlGearNode, IList<Weapon> lstWeapons, bool blnAddImprovements = true)
        {
            XmlNode xmlGearDataNode;
            List<Gear> lstChildGears = new List<Gear>();
            XmlAttributeCollection lstGearAttributes = xmlGearNode.Attributes;
            int intRating = Convert.ToInt32(lstGearAttributes?["rating"]?.InnerText);
            string strMaxRating = lstGearAttributes?["maxrating"]?.InnerText ?? string.Empty;
            decimal decQty = Convert.ToDecimal(lstGearAttributes?["qty"]?.InnerText ?? "1", GlobalOptions.InvariantCultureInfo);
            string strForceValue = lstGearAttributes?["select"]?.InnerText ?? string.Empty;
            if (xmlGearNode["name"] != null)
            {
                xmlGearDataNode = xmlGearsDocument.SelectSingleNode("/chummer/gears/gear[name = " + xmlGearNode["name"].InnerText.CleanXPath() + "]");
                XmlNodeList xmlInnerGears = xmlGearNode.SelectNodes("gears/gear");
                if (xmlInnerGears?.Count > 0)
                {
                    foreach (XmlNode xmlChildGearNode in xmlInnerGears)
                    {
                        Gear objChildGear = new Gear(_objCharacter);
                        if (objChildGear.CreateFromNode(xmlGearsDocument, xmlChildGearNode, lstWeapons, blnAddImprovements))
                        {
                            objChildGear.ParentID = InternalId;
                            objChildGear.Parent = this;
                            lstChildGears.Add(objChildGear);
                        }
                        else
                            Utils.BreakIfDebug();
                    }
                }
            }
            else
            {
                xmlGearDataNode = xmlGearsDocument.SelectSingleNode("/chummer/gears/gear[name = " + xmlGearNode.InnerText.CleanXPath() + "]");
            }

            if (xmlGearDataNode != null)
            {
                Create(xmlGearDataNode, intRating, lstWeapons, strForceValue, blnAddImprovements);

                string strOldCapacity = Capacity;
                int intSlashIndex = strOldCapacity?.IndexOf("/[") ?? -1;
                if (intSlashIndex == -1)
                    Capacity = "[0]";
                else
                    Capacity = (strOldCapacity?.Substring(0, intSlashIndex) ?? "0") + "/[0]";
                strOldCapacity = ArmorCapacity;
                intSlashIndex = strOldCapacity?.IndexOf("/[") ?? -1;
                if (intSlashIndex == -1)
                    ArmorCapacity = "[0]";
                else
                    ArmorCapacity = (strOldCapacity?.Substring(0, intSlashIndex) ?? "0") + "/[0]";
                Cost = "0";
                Quantity = decQty;
                if (!string.IsNullOrEmpty(strMaxRating))
                    MaxRating = strMaxRating;

                foreach (Gear objGearChild in lstChildGears)
                {
                    objGearChild.ParentID = InternalId;
                    Children.Add(objGearChild);
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Copy a piece of Gear.
        /// </summary>
        /// <param name="objGear">Gear object to copy.</param>
        public void Copy(Gear objGear)
        {
            _objCachedMyXmlNode = objGear.GetNode();
            _SourceGuid = objGear._SourceGuid;
            _blnAllowRename = objGear.AllowRename;
            _strName = objGear.Name;
            _strCategory = objGear.Category;
            _strMaxRating = objGear.MaxRating;
            _strMinRating = objGear.MinRating;
            Rating = objGear.Rating;
            _decQty = objGear.Quantity;
            _strCapacity = objGear.Capacity;
            _strArmorCapacity = objGear.ArmorCapacity;
            _strAvail = objGear.Avail;
            _decCostFor = objGear.CostFor;
            _strDeviceRating = objGear.DeviceRating;
            _strCost = objGear.Cost;
            _strSource = objGear.Source;
            _strPage = objGear.Page;
            _strCanFormPersona = objGear.CanFormPersona;
            _strExtra = objGear.Extra;
            _blnBonded = objGear.Bonded;
            _blnEquipped = objGear.Equipped;
            _blnWirelessOn = objGear.WirelessOn;
            _nodBonus = objGear.Bonus;
            _nodWirelessBonus = objGear.WirelessBonus;
            _nodWeaponBonus = objGear.WeaponBonus;
            Guid.TryParse(objGear.WeaponID, out _guiWeaponID);
            _strNotes = objGear.Notes;
            _objLocation = objGear.Location;
            _intChildAvailModifier = objGear.ChildAvailModifier;
            _intChildCostMultiplier = objGear.ChildCostMultiplier;
            _strGearName = objGear.GearName;
            _strForcedValue = objGear._strForcedValue;

            foreach (Gear objGearChild in objGear.Children)
            {
                Gear objChild = new Gear(_objCharacter);
                objChild.Copy(objGearChild);
                _lstChildren.Add(objChild);
            }

            _strOverclocked = objGear.Overclocked;
            _strAttack = objGear.Attack;
            _strSleaze = objGear.Sleaze;
            _strDataProcessing = objGear.DataProcessing;
            _strFirewall = objGear.Firewall;
            _strAttributeArray = objGear.AttributeArray;
            _strModAttack = objGear.ModAttack;
            _strModSleaze = objGear.ModSleaze;
            _strModDataProcessing = objGear.ModDataProcessing;
            _strModFirewall = objGear.ModFirewall;
            _strModAttributeArray = objGear.ModAttributeArray;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("gear");

            objWriter.WriteElementString("guid", _guiID.ToString("D"));
            objWriter.WriteElementString("id", _SourceGuid);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("minrating", _strMinRating);
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("qty", _decQty.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("avail", _strAvail);
            if (_decCostFor > 1)
                objWriter.WriteElementString("costfor", _decCostFor.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("bonded", _blnBonded.ToString());
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString("D"));
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw("<wirelessbonus>" + _nodWirelessBonus.InnerXml + "</wirelessbonus>");
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            if (_nodWeaponBonus != null)
                objWriter.WriteRaw("<weaponbonus>" + _nodWeaponBonus.InnerXml + "</weaponbonus>");
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("canformpersona", _strCanFormPersona);
            objWriter.WriteElementString("devicerating", _strDeviceRating);
            objWriter.WriteElementString("gearname", _strGearName);
            objWriter.WriteElementString("forcedvalue", _strForcedValue);
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("matrixcmbonus", _intMatrixCMBonus.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("allowrename", _blnAllowRename.ToString());
            if (_intChildCostMultiplier != 1)
                objWriter.WriteElementString("childcostmultiplier", _intChildCostMultiplier.ToString(GlobalOptions.InvariantCultureInfo));
            if (_intChildAvailModifier != 0)
                objWriter.WriteElementString("childavailmodifier", _intChildAvailModifier.ToString(GlobalOptions.InvariantCultureInfo));
            objWriter.WriteStartElement("children");
            foreach (Gear objGear in _lstChildren)
            {
                objGear.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("location", Location?.InternalId ?? string.Empty);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", _blnDiscountCost.ToString());

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
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString());
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString());
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString());
            objWriter.WriteElementString("sortorder", _intSortOrder.ToString());

            objWriter.WriteEndElement();

            if (!IncludedInParent)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Gear from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        /// <param name="blnCopy">Whether or not we are loading a copy of an existing gear.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            objNode.TryGetField("guid", Guid.TryParse, out _guiID);
            if (objNode.TryGetStringFieldQuickly("id", ref _SourceGuid))
                _objCachedMyXmlNode = null;
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (objNode.TryGetStringFieldQuickly("category", ref _strCategory))
                _objCachedMyXmlNode = null;
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
            objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objNode.TryGetStringFieldQuickly("armorcapacity", ref _strArmorCapacity);
            objNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            if (_strMaxRating == "0")
                _strMaxRating = string.Empty;
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetDecFieldQuickly("qty", ref _decQty);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            // Legacy shim
            if (string.IsNullOrEmpty(_strAvail) && (objNode["avail3"] != null || objNode["avail6"] != null || objNode["avail10"] != null))
            {
                GetNode()?.TryGetStringFieldQuickly("avail", ref _strAvail);
            }
            objNode.TryGetDecFieldQuickly("costfor", ref _decCostFor);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            // Legacy shim
            if (string.IsNullOrEmpty(_strCost) && (objNode["cost3"] != null || objNode["cost6"] != null || objNode["cost10"] != null))
            {
                GetNode()?.TryGetStringFieldQuickly("cost", ref _strCost);
            }
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            if (_strExtra == "Hold-Outs")
                _strExtra = "Holdouts";
            objNode.TryGetBoolFieldQuickly("bonded", ref _blnBonded);
            objNode.TryGetBoolFieldQuickly("equipped", ref _blnEquipped);
            _nodBonus = objNode["bonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
                _blnWirelessOn = false;
            _nodWeaponBonus = objNode["weaponbonus"];
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            bool blnNeedCommlinkLegacyShim = !objNode.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            if (!objNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating))
                GetNode()?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            string strWeaponID = string.Empty;
            if (objNode.TryGetStringFieldQuickly("weaponguid", ref strWeaponID))
            {
                Guid.TryParse(strWeaponID, out _guiWeaponID);
            }
            objNode.TryGetInt32FieldQuickly("childcostmultiplier", ref _intChildCostMultiplier);
            objNode.TryGetInt32FieldQuickly("childavailmodifier", ref _intChildAvailModifier);

            objNode.TryGetStringFieldQuickly("gearname", ref _strGearName);
            objNode.TryGetStringFieldQuickly("forcedvalue", ref _strForcedValue);
            objNode.TryGetBoolFieldQuickly("allowrename", ref _blnAllowRename);
            if (!objNode.TryGetStringFieldQuickly("parentid", ref _strParentID))
            {
                // Legacy Shim
                bool blnIncludedInParent = false;
                if (objNode.TryGetBoolFieldQuickly("includedinparent", ref blnIncludedInParent) && blnIncludedInParent)
                {
                    // ParentIDs were only added when improvements were added that could allow for the adding of gear by something that would not become the gear's parent...
                    // ... so all we care about is that this string is not empty and does not match the internal IDs of any sources for adding gear via improvements.
                    _strParentID = Guid.NewGuid().ToString("D");
                }
            }

            using (XmlNodeList nodChildren = objNode.SelectNodes("children/gear"))
                if (nodChildren != null)
                    foreach (XmlNode nodChild in nodChildren)
                    {
                        Gear objGear = new Gear(_objCharacter);
                        objGear.Load(nodChild, blnCopy);
                        objGear.Parent = this;
                        _lstChildren.Add(objGear);
                    }

            // Legacy Shim
            if (!string.IsNullOrEmpty(_strMaxRating) && _strName.Contains("Certified Credstick"))
            {
                XmlNode objNuyenNode = XmlManager.Load("gear.xml").SelectSingleNode("/chummer/gears/gear[contains(name, \"Nuyen\") and category = \"Currency\"]");
                if (objNuyenNode != null)
                {
                    if (Rating > 0)
                    {
                        Gear objNuyenGear = new Gear(_objCharacter);
                        objNuyenGear.Create(objNuyenNode, 0, new List<Weapon>());
                        objNuyenGear.Quantity = Rating;
                        _lstChildren.Add(objNuyenGear);
                    }
                    GetNode()?.TryGetStringFieldQuickly("rating", ref _strMaxRating);
                    if (_strMaxRating == "0")
                        _strMaxRating = string.Empty;
                    GetNode()?.TryGetStringFieldQuickly("minrating", ref _strMinRating);
                    Rating = 0;
                    GetNode()?.TryGetStringFieldQuickly("capacity", ref _strCapacity);
                }
            }

            string strLocation = objNode["location"]?.InnerText;
            if (!string.IsNullOrEmpty(strLocation))
            {
                if (Guid.TryParse(strLocation, out Guid temp))
                {
                    // Location is an object. Look for it based on the InternalId. Requires that locations have been loaded already!
                    _objLocation =
                        CharacterObject.GearLocations.FirstOrDefault(location =>
                            location.InternalId == temp.ToString());
                }
                else
                {
                    //Legacy. Location is a string. 
                    _objLocation =
                        CharacterObject.GearLocations.FirstOrDefault(location =>
                            location.Name == strLocation);
                }
                _objLocation?.Children.Add(this);
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            objNode.TryGetInt32FieldQuickly("sortorder", ref _intSortOrder);

            // Convert old qi foci to the new bonus. In order to force the user to update their powers, unequip the focus and remove all improvements.
            if (_strName == "Qi Focus")
            {
                Version.TryParse("5.193.5", out Version test);
                if (test != null)
                {
                    int intResult = _objCharacter.LastSavedVersion.CompareTo(test);
                    if (intResult == -1)
                    {
                        XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
                        XmlNode gear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = " + _strName.CleanXPath() + "]");
                        if (gear != null)
                        {
                            Equipped = false;
                            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Gear, InternalId);
                            Bonus = gear["bonus"];
                            WirelessBonus = gear["wirelessbonus"];
                        }
                    }
                }
            }

            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
                _objLocation = null;

                if (Bonus != null || WirelessBonus != null)
                {
                    bool blnAddImprovement = true;
                    // If this is a Focus which is not bonded, don't do anything.
                    if (Category != "Stacked Focus")
                    {
                        if (Category.EndsWith("Foci"))
                            blnAddImprovement = Bonded;

                        if (blnAddImprovement)
                        {
                            if (!string.IsNullOrEmpty(Extra))
                                ImprovementManager.ForcedValue = Extra;
                            if (Bonus != null)
                            {
                                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear, InternalId, Bonus, false, Rating, DisplayNameShort(GlobalOptions.Language));
                                Extra = ImprovementManager.SelectedValue;
                            }
                            if (WirelessOn && WirelessBonus != null)
                            {
                                ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear, InternalId, WirelessBonus, false, Rating, DisplayNameShort(GlobalOptions.Language));
                            }
                        }
                    }
                    else
                    {
                        // Stacked Foci need to be handled a little differently.
                        foreach (StackedFocus objStack in _objCharacter.StackedFoci)
                        {
                            if (objStack.GearId == InternalId && objStack.Bonded)
                            {
                                foreach (Gear objFociGear in objStack.Gear)
                                {
                                    if (!string.IsNullOrEmpty(objFociGear.Extra))
                                        ImprovementManager.ForcedValue = objFociGear.Extra;
                                    if (objFociGear.Bonus != null)
                                    {
                                        ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating,
                                            objFociGear.DisplayNameShort(GlobalOptions.Language));
                                        objFociGear.Extra = ImprovementManager.SelectedValue;
                                    }
                                    if (objFociGear.WirelessOn && objFociGear.WirelessBonus != null)
                                    {
                                        ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.WirelessBonus, false, Rating,
                                            objFociGear.DisplayNameShort(GlobalOptions.Language));
                                    }
                                }
                            }
                        }
                    }
                }

                if (!Equipped)
                    ChangeEquippedStatus(false);
            }
            else if (!Equipped && (Bonus != null || WirelessBonus != null) && !_objCharacter.Improvements.Any(x => x.ImproveSource == Improvement.ImprovementSource.Gear && x.SourceName == InternalId))
            {
                bool blnAddImprovement = true;
                // If this is a Focus which is not bonded, don't do anything.
                if (Category != "Stacked Focus")
                {
                    if (Category.EndsWith("Foci"))
                        blnAddImprovement = Bonded;

                    if (blnAddImprovement)
                    {
                        if (!string.IsNullOrEmpty(Extra))
                            ImprovementManager.ForcedValue = Extra;
                        if (Bonus != null)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear, InternalId, Bonus, false, Rating, DisplayNameShort(GlobalOptions.Language));
                            Extra = ImprovementManager.SelectedValue;
                        }
                        if (WirelessOn && WirelessBonus != null)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.Gear, InternalId, WirelessBonus, false, Rating, DisplayNameShort(GlobalOptions.Language));
                        }
                    }
                }
                else
                {
                    // Stacked Foci need to be handled a little differently.
                    foreach (StackedFocus objStack in _objCharacter.StackedFoci)
                    {
                        if (objStack.GearId == InternalId && objStack.Bonded)
                        {
                            foreach (Gear objFociGear in objStack.Gear)
                            {
                                if (!string.IsNullOrEmpty(objFociGear.Extra))
                                    ImprovementManager.ForcedValue = objFociGear.Extra;
                                if (objFociGear.Bonus != null)
                                {
                                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.Bonus, false, objFociGear.Rating,
                                        objFociGear.DisplayNameShort(GlobalOptions.Language));
                                    objFociGear.Extra = ImprovementManager.SelectedValue;
                                }
                                if (objFociGear.WirelessOn && objFociGear.WirelessBonus != null)
                                {
                                    ImprovementManager.CreateImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId, objFociGear.WirelessBonus, false, Rating,
                                        objFociGear.DisplayNameShort(GlobalOptions.Language));
                                }
                            }
                        }
                    }
                }
                ChangeEquippedStatus(false);
            }
            
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
            if (!objNode.TryGetBoolFieldQuickly("canswapattributes", ref _blnCanSwapAttributes))
            {
                // Legacy shim
                if (Category == "Cyberdecks")
                {
                    _blnCanSwapAttributes = (Name != "MCT Trainee" && Name != "C-K Analyst" && Name != "Aztechnology Emissary" &&
                        Name != "Yak Killer" && Name != "Ring of Light Special" && Name != "Ares Echo Unlimited");
                }
            }

            if (blnNeedCommlinkLegacyShim)
            {
                if (_strDeviceRating == "0")
                {
                    _strModAttack = _strAttack;
                    _strModSleaze = _strSleaze;
                    _strModDataProcessing = _strDataProcessing;
                    _strModFirewall = _strFirewall;
                    if (GetNode() != null)
                    {
                        _strAttack = string.Empty;
                        GetNode().TryGetStringFieldQuickly("attack", ref _strAttack);
                        _strSleaze = string.Empty;
                        GetNode().TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                        _strDataProcessing = string.Empty;
                        GetNode().TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                        _strFirewall = string.Empty;
                        GetNode().TryGetStringFieldQuickly("firewall", ref _strFirewall);
                    }
                }
                GetNode()?.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
                bool blnIsCommlinkLegacy = false;
                objNode.TryGetBoolFieldQuickly("iscommlink", ref blnIsCommlinkLegacy);
                // This is Commlink Functionality, which originally had Persona Firmware that would now make the Commlink Functionality item count as a commlink
                if (blnIsCommlinkLegacy != IsCommlink)
                {
                    for (int i = Children.Count - 1; i >= 0; --i)
                    {
                        Gear objLoopChild = Children[i];
                        if (objLoopChild.ParentID == InternalId && objLoopChild.CanFormPersona == "Parent")
                            Children.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("gear");

            if ((Category == "Foci" || Category == "Metamagic Foci") && Bonded)
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint) + LanguageManager.GetString("String_Space", strLanguageToPrint) + '(' + LanguageManager.GetString("Label_BondedFoci", strLanguageToPrint) + ')');
            else
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));

            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", Category);
            objWriter.WriteElementString("iscommlink", IsCommlink.ToString());
            objWriter.WriteElementString("ispersona", (Name == "Living Persona").ToString());
            //objWriter.WriteElementString("isnexus", (Category == "Nexus").ToString());
            objWriter.WriteElementString("isammo", (Category == "Ammunition").ToString());
            objWriter.WriteElementString("isprogram", IsProgram.ToString());
            objWriter.WriteElementString("isos", bool.FalseString);
            objWriter.WriteElementString("issin", (Name == "Fake SIN" || Name == "Credstick, Fake (2050)" || Name == "Fake SIN").ToString());
            objWriter.WriteElementString("capacity", Capacity);
            objWriter.WriteElementString("armorcapacity", ArmorCapacity);
            objWriter.WriteElementString("maxrating", MaxRating.ToString(objCulture));
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("matrixcmfilled", MatrixCMFilled.ToString(objCulture));
            objWriter.WriteElementString("conditionmonitor", MatrixCM.ToString(objCulture));
            objWriter.WriteElementString("qty", Quantity.ToString(Name.StartsWith("Nuyen") ? _objCharacter.Options.NuyenFormat : Category == "Currency" ? "#,0.00" : "#,0.##", objCulture));
            objWriter.WriteElementString("avail", TotalAvail(GlobalOptions.CultureInfo, strLanguageToPrint));
            objWriter.WriteElementString("avail_english", TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.DefaultLanguage));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(Extra, strLanguageToPrint));
            objWriter.WriteElementString("bonded", Bonded.ToString());
            objWriter.WriteElementString("equipped", Equipped.ToString());
            objWriter.WriteElementString("wirelesson", WirelessOn.ToString());
            objWriter.WriteElementString("location", Location?.DisplayName(GlobalOptions.Language));
            objWriter.WriteElementString("gearname", GearName);
            objWriter.WriteElementString("source", CommonFunctions.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteStartElement("children");
            foreach (Gear objGear in Children)
            {
                objGear.Print(objWriter, objCulture, strLanguageToPrint);
            }
            objWriter.WriteEndElement();
            if (_nodWeaponBonus != null)
            {
                objWriter.WriteElementString("weaponbonusdamage", WeaponBonusDamage(strLanguageToPrint));
                objWriter.WriteElementString("weaponbonusdamage_english", WeaponBonusDamage(GlobalOptions.DefaultLanguage));
                objWriter.WriteElementString("weaponbonusap", WeaponBonusAP);
            }
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", Notes);

            objWriter.WriteElementString("attack", this.GetTotalMatrixAttribute("Attack").ToString(objCulture));
            objWriter.WriteElementString("sleaze", this.GetTotalMatrixAttribute("Sleaze").ToString(objCulture));
            objWriter.WriteElementString("dataprocessing", this.GetTotalMatrixAttribute("Data Processing").ToString(objCulture));
            objWriter.WriteElementString("firewall", this.GetTotalMatrixAttribute("Firewall").ToString(objCulture));
            objWriter.WriteElementString("devicerating", this.GetTotalMatrixAttribute("Device Rating").ToString(objCulture));
            objWriter.WriteElementString("programlimit", this.GetTotalMatrixAttribute("Program Limit").ToString(objCulture));
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString());
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString());
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this piece of Gear in the Character.
        /// </summary>
        public string InternalId => _guiID.ToString("D");

        public string SourceID => _SourceGuid;

        /// <summary>
        /// Guid of a Cyberware Weapon.
        /// </summary>
        public string WeaponID
        {
            get => _guiWeaponID.ToString("D");
            set
            {
                if (Guid.TryParse(value, out Guid guiTemp))
                    _guiWeaponID = guiTemp;
            }
        }

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// Wireless bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get => _nodWirelessBonus;
            set => _nodWirelessBonus = value;
        }

        /// <summary>
        /// WeaponBonus node from the XML file.
        /// </summary>
        public XmlNode WeaponBonus
        {
            get => _nodWeaponBonus;
            set => _nodWeaponBonus = value;
        }

        /// <summary>
        /// Character to which the gear is assigned.
        /// </summary>
        public Character CharacterObject => _objCharacter;

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    _objCachedMyXmlNode = null;
                    _strName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// A custom name for the Gear assigned by the player.
        /// </summary>
        public string GearName
        {
            get => _strGearName;
            set
            {
                if (_strGearName != value)
                {
                    _strGearName = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("gear.xml", strLanguage).SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]/@translate")?.InnerText ?? Category;
        }

        /// <summary>
        /// Category.
        /// </summary>
        public string Category
        {
            get => _strCategory;
            set
            {
                if (_strCategory != value)
                    _objCachedMyXmlNode = null;
                _strCategory = value;
            }
        }

        /// <summary>
        /// Gear capacity.
        /// </summary>
        public string Capacity
        {
            get => _strCapacity;
            set => _strCapacity = value;
        }

        /// <summary>
        /// Armor capacity.
        /// </summary>
        public string ArmorCapacity
        {
            get => _strArmorCapacity;
            set => _strArmorCapacity = value;
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
            if (strExpression.StartsWith("FixedValues("))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            if (strExpression.IndexOfAny('{', '+', '-', '*', ',') != -1 || strExpression.Contains("div"))
            {
                StringBuilder objValue = new StringBuilder(strExpression);
                objValue.Replace("{Rating}", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                objValue.CheapReplace(strExpression, "{Parent Rating}",
                    () => (Parent as IHasRating)?.Rating.ToString(GlobalOptions.InvariantCultureInfo) ??
                          int.MaxValue.ToString(GlobalOptions.InvariantCultureInfo));
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    objValue.CheapReplace(strExpression, '{' + strCharAttributeName + '}',
                        () => CharacterObject.GetAttribute(strCharAttributeName).TotalValue.ToString());
                    objValue.CheapReplace(strExpression, '{' + strCharAttributeName + "Base}",
                        () => CharacterObject.GetAttribute(strCharAttributeName).TotalBase.ToString());
                }

                foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + '}',
                        () => ((Parent as IHasMatrixAttributes)?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0).ToString(
                            GlobalOptions.InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + '}',
                        () => (Parent as IHasMatrixAttributes).GetMatrixAttributeString(strMatrixAttribute) ?? "0");
                    if (Children.Count <= 0 || !strExpression.Contains("{Children " + strMatrixAttribute + '}'))
                        continue;
                    int intTotalChildrenValue = Children.Where(g => g.Equipped)
                        .Sum(loopGear => loopGear.GetBaseMatrixAttribute(strMatrixAttribute));

                    objValue.Replace("{Children " + strMatrixAttribute + '}',
                        intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                }

                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objValue.ToString(), out bool blnIsSuccess);
                return blnIsSuccess ? Convert.ToInt32(Math.Ceiling((double) objProcess)) : 0;
            }

            int.TryParse(strExpression, out int intReturn);

            return intReturn;
        }

        /// <summary>
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set
            {
                int intNewValue = Math.Max(Math.Min(value, MaxRatingValue), MinRatingValue);
                if (_intRating != intNewValue)
                {
                    _intRating = intNewValue;
                    if (Children.Count > 0)
                    {
                        foreach (Gear objChild in Children.Where(x => x.MaxRating.Contains("Parent") || x.MinRating.Contains("Parent")))
                        {
                            // This will update a child's rating if it would become out of bounds due to its parent's rating changing
                            objChild.Rating = objChild.Rating;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Quantity.
        /// </summary>
        public decimal Quantity
        {
            get => _decQty;
            set
            {
                if (_decQty != value)
                {
                    _decQty = value;
                    OnPropertyChanged();
                }
            }
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
        /// Use for ammo. The number of rounds that the nuyen amount buys.
        /// </summary>
        public decimal CostFor
        {
            get => _decCostFor;
            set => _decCostFor = value;
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
        /// Value that was selected during an ImprovementManager dialogue.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set
            {
                string strNewValue = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
                if (_strExtra != strNewValue)
                {
                    _strExtra = strNewValue;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether or not the Foci is bonded.
        /// </summary>
        public bool Bonded
        {
            get => _blnBonded;
            set => _blnBonded = value;
        }

        /// <summary>
        /// Whether or not the Gear is equipped.
        /// </summary>
        public bool Equipped
        {
            get => _blnEquipped;
            set => _blnEquipped = value;
        }

        /// <summary>
        /// Whether or not the Gear's wireless bonus is enabled.
        /// </summary>
        public bool WirelessOn
        {
            get => _blnWirelessOn;
            set => _blnWirelessOn = value;
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

        /// <summary>
        /// String to determine if gear can form persona or grants persona forming to its parent.
        /// </summary>
        public string CanFormPersona
        {
            get => _strCanFormPersona;
            set => _strCanFormPersona = value;
        }

        public bool IsCommlink
        {
            get
            {
                return _strCanFormPersona.Contains("Self") || Children.Any(x => x.CanFormPersona.Contains("Parent"));
            }
        }
        
        /// <summary>
        /// A List of child pieces of Gear.
        /// </summary>
        public TaggedObservableCollection<Gear> Children => _lstChildren;

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set
            {
                if (_strNotes != value)
                {
                    _strNotes = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Device Rating string.
        /// </summary>
        public string DeviceRating
        {
            get => _strDeviceRating;
            set => _strDeviceRating = value;
        }

        /// <summary>
        /// Allow Renaming the Gear in Create Mode
        /// </summary>
        public bool AllowRename => _blnAllowRename;

        /// <summary>
        /// Get the base value of a Matrix attribute of this gear (without children or Overclocker)
        /// </summary>
        /// <param name="strAttributeName">Matrix attribute name.</param>
        /// <returns></returns>
        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExpression = IsCommlink ? "2" : "0";
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
                            strExpression = "0";
                        break;
                    default:
                        strExpression = "0";
                        break;
                }
            }

            if (strExpression.StartsWith("FixedValues("))
            {
                string[] strValues = strExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                strExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
            }

            if (Name == "Living Persona")
            {
                string strExtraExpression = string.Empty;
                switch (strAttributeName)
                {
                    case "Device Rating":
                        strExtraExpression = string.Concat(CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LivingPersonaDeviceRating && x.Enabled).Select(x => x.ImprovedName));
                        break;
                    case "Program Limit":
                        strExtraExpression = string.Concat(CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LivingPersonaProgramLimit && x.Enabled).Select(x => x.ImprovedName));
                        break;
                    case "Attack":
                        strExtraExpression = string.Concat(CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LivingPersonaAttack && x.Enabled).Select(x => x.ImprovedName));
                        break;
                    case "Sleaze":
                        strExtraExpression = string.Concat(CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LivingPersonaSleaze && x.Enabled).Select(x => x.ImprovedName));
                        break;
                    case "Data Processing":
                        strExtraExpression = string.Concat(CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LivingPersonaDataProcessing && x.Enabled).Select(x => x.ImprovedName));
                        break;
                    case "Firewall":
                        strExtraExpression = string.Concat(CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LivingPersonaFirewall && x.Enabled).Select(x => x.ImprovedName));
                        break;
                }
                if (!string.IsNullOrEmpty(strExtraExpression))
                    strExpression += strExtraExpression;
            }
            return string.IsNullOrEmpty(strExpression) ? 0 : ProcessRatingString(strExpression);
        }

        /// <summary>
        /// Get the bonus value of a Matrix attribute of this gear from children and Overclocker
        /// </summary>
        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                intReturn += 1;
            }

            if (!strAttributeName.StartsWith("Mod "))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Gear loopGear in Children)
            {
                if (loopGear.Equipped)
                {
                    intReturn += loopGear.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        /// <summary>
        /// Location.
        /// </summary>
        public Location Location
        {
            get => _objLocation;
            set => _objLocation = value;
        }

        /// <summary>
        /// Whether or not the Gear qualifies as a Program in the printout XML.
        /// </summary>
        public bool IsProgram => Category == "ARE Programs" ||
                                 Category.StartsWith("Autosofts") ||
                                 Category == "Data Software" ||
                                 Category == "Malware" ||
                                 Category == "Matrix Programs" ||
                                 Category == "Tactical AR Software" ||
                                 Category == "Telematics Infrastructure Software" ||
                                 Category == "Sensor Software";
        
        /// <summary>
        /// Cost multiplier for Children attached to this Gear.
        /// </summary>
        public int ChildCostMultiplier
        {
            get => _intChildCostMultiplier;
            set => _intChildCostMultiplier = value;
        }

        /// <summary>
        /// Avail modifier for Children attached to this Gear.
        /// </summary>
        public int ChildAvailModifier
        {
            get => _intChildAvailModifier;
            set => _intChildAvailModifier = value;
        }

        private object _objParent;

        /// <summary>
        /// Parent Gear.
        /// </summary>
        public object Parent
        {
            get => _objParent;
            set
            {
                if (_objParent != value)
                {
                    _objParent = value;
                    Rating = Math.Max(MinRatingValue, Math.Min(MaxRatingValue, Rating));
                }
            }
        }

        /// <summary>
        /// Whether or not the Gear's cost should be discounted by 10% through the Black Market Pipeline Quality.
        /// </summary>
        public bool DiscountCost
        {
            get => _blnDiscountCost && _objCharacter.BlackMarketDiscount;
            set => _blnDiscountCost = value;
        }

        /// <summary>
        /// Attack.
        /// </summary>
        public string Attack
        {
            get => _strAttack;
            set => _strAttack = value;
        }

        /// <summary>
        /// Sleaze.
        /// </summary>
        public string Sleaze
        {
            get => _strSleaze;
            set => _strSleaze = value;
        }

        /// <summary>
        /// Data Processing.
        /// </summary>
        public string DataProcessing
        {
            get => _strDataProcessing;
            set => _strDataProcessing = value;
        }

        /// <summary>
        /// Firewall.
        /// </summary>
        public string Firewall
        {
            get => _strFirewall;
            set => _strFirewall = value;
        }

        /// <summary>
        /// Modify Parent's Attack by this.
        /// </summary>
        public string ModAttack
        {
            get => _strModAttack;
            set => _strModAttack = value;
        }

        /// <summary>
        /// Modify Parent's Sleaze by this.
        /// </summary>
        public string ModSleaze
        {
            get => _strModSleaze;
            set => _strModSleaze = value;
        }

        /// <summary>
        /// Modify Parent's Data Processing by this.
        /// </summary>
        public string ModDataProcessing
        {
            get => _strModDataProcessing;
            set => _strModDataProcessing = value;
        }

        /// <summary>
        /// Modify Parent's Firewall by this.
        /// </summary>
        public string ModFirewall
        {
            get => _strModFirewall;
            set => _strModFirewall = value;
        }

        /// <summary>
        /// Cyberdeck's Attribute Array string.
        /// </summary>
        public string AttributeArray
        {
            get => _strAttributeArray;
            set => _strAttributeArray = value;
        }

        /// <summary>
        /// Modify Parent's Attribute Array by this.
        /// </summary>
        public string ModAttributeArray
        {
            get => _strModAttributeArray;
            set => _strModAttributeArray = value;
        }

        public IList<IHasMatrixAttributes> ChildrenWithMatrixAttributes => Children.Cast<IHasMatrixAttributes>().ToList();

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get => _strProgramLimit;
            set => _strProgramLimit = value;
        }

        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        public string Overclocked
        {
            get
            {
                if (!CharacterObject.Overclocker)
                    return string.Empty;
                return _strOverclocked;
            }
            set => _strOverclocked = value;
        }

        /// <summary>
        /// Returns true if this is a cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get => _blnCanSwapAttributes;
            set => _blnCanSwapAttributes = value;
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
        /// Whether or not the Gear is included in its parent item when purchased (currently applies to Armor only).
        /// </summary>
        public bool IncludedInParent => !string.IsNullOrEmpty(ParentID);

        /// <summary>
        /// ID of the object that added this cyberware (if any).
        /// </summary>
        public string ParentID
        {
            get => _strParentID;
            set
            {
                if (_strParentID != value)
                {
                    _strParentID = value;
                    OnPropertyChanged();
                }
            }
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                XmlDocument objDoc = XmlManager.Load("gear.xml", strLanguage);
                string strNameWithQuotes = Name.CleanXPath();
                _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/gears/gear[(id = \"" + _SourceGuid + "\") or (name = " + strNameWithQuotes + " and category = \"" + Category + "\")]");
                if (_objCachedMyXmlNode == null)
                {
                    _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/gears/gear[(name = " + strNameWithQuotes + ")]") ??
                                          objDoc.SelectSingleNode("/chummer/gears/gear[contains(name, " + strNameWithQuotes + ")]");
                    _objCachedMyXmlNode?.TryGetStringFieldQuickly("id", ref _SourceGuid);
                }
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Total Availablility of the Gear and its accessories.
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
                    string[] strValues = strAvail.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strAvail = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }

                chrLastAvailChar = strAvail[strAvail.Length - 1];
                if (chrLastAvailChar == 'F' || chrLastAvailChar == 'R')
                {
                    strAvail = strAvail.Substring(0, strAvail.Length - 1);
                }

                blnModifyParentAvail = strAvail.StartsWith('+', '-');
                StringBuilder objAvail = new StringBuilder(strAvail.TrimStart('+'));
                objAvail.CheapReplace(strAvail, "MinRating", () => MinRatingValue.ToString(GlobalOptions.InvariantCultureInfo));
                objAvail.CheapReplace(strAvail, "Parent Rating", () => (Parent as IHasRating)?.Rating.ToString(GlobalOptions.InvariantCultureInfo));
                objAvail.Replace("Rating", Rating.ToString());

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objAvail.CheapReplace(strAvail, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                object objProcess = CommonFunctions.EvaluateInvariantXPath(objAvail.ToString(), out bool blnIsSuccess);
                if (blnIsSuccess)
                    intAvail += Convert.ToInt32(objProcess);
            }

            if (blnCheckChildren)
            {
                // Run through the child items and increase the Avail by any Mod whose Avail contains "+".
                foreach (Gear objChild in Children)
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
        /// Caculated Capacity of the Gear.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strReturn = _strCapacity;
                if (string.IsNullOrEmpty(strReturn))
                    return (0.0m).ToString("#,0.##", GlobalOptions.CultureInfo);
                if (strReturn.StartsWith("FixedValues("))
                {
                    string[] strValues = strReturn.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strReturn = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                }
                int intPos = strReturn.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    string strFirstHalf = strReturn.Substring(0, intPos);
                    string strSecondHalf = strReturn.Substring(intPos + 1, strReturn.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.StartsWith('[');
                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                    if (strFirstHalf == "[*]")
                        strReturn = "*";
                    else
                    {
                        if (strFirstHalf.StartsWith("FixedValues("))
                        {
                            string[] strValues = strFirstHalf.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                            strFirstHalf = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                        }
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strFirstHalf.Replace("Rating", Rating.ToString()), out bool blnIsSuccess);
                        strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strFirstHalf;
                    }

                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                    if (!string.IsNullOrEmpty(strSecondHalf))
                        strReturn += '/' + strSecondHalf;
                }
                else if (strReturn.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strReturn.StartsWith('[');
                    if (blnSquareBrackets)
                        strReturn = strReturn.Substring(1, strReturn.Length - 2);

                    // This has resulted in a non-whole number, so round it (minimum of 1).
                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strReturn
                        .CheapReplace("Parent Rating", () => (Parent as IHasRating)?.Rating.ToString(GlobalOptions.InvariantCultureInfo))
                        .Replace("Rating", Rating.ToString()), out bool blnIsSuccess);
                    double dblNumber = blnIsSuccess ? (double)objProcess : 1;
                    if (dblNumber < 1)
                        dblNumber = 1;
                    strReturn = dblNumber.ToString("#,0.##", GlobalOptions.CultureInfo);

                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                }
                else if (decimal.TryParse(strReturn, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                {
                    // Just a straight Capacity, so return the value.
                    strReturn = decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Caculated Capacity of the Gear when attached to Armor.
        /// </summary>
        public string CalculatedArmorCapacity
        {
            get
            {
                string strReturn = ArmorCapacity;
                if (string.IsNullOrEmpty(strReturn))
                    return 0.ToString(GlobalOptions.CultureInfo);
                int intPos = strReturn.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    string strFirstHalf = strReturn.Substring(0, intPos);
                    string strSecondHalf = strReturn.Substring(intPos + 1, strReturn.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.StartsWith('[');
                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);

                    if (strFirstHalf == "[*]")
                        strReturn = "*";
                    else
                    {
                        if (strFirstHalf.StartsWith("FixedValues("))
                        {
                            string[] strValues = strFirstHalf.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                            strFirstHalf = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)];
                        }
                        object objProcess = CommonFunctions.EvaluateInvariantXPath(strFirstHalf.Replace("Rating", Rating.ToString()), out bool blnIsSuccess);
                        strReturn = blnIsSuccess ? ((double)objProcess).ToString("#,0.##", GlobalOptions.CultureInfo) : strFirstHalf;
                    }
                    
                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                    strReturn += '/' + strSecondHalf;
                }
                else if (strReturn.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strReturn.StartsWith('[');
                    if (blnSquareBrackets)
                        strReturn = strReturn.Substring(1, strReturn.Length - 2);

                    object objProcess = CommonFunctions.EvaluateInvariantXPath(strReturn.Replace("Rating", Rating.ToString()), out bool blnIsSuccess);
                    if (blnIsSuccess)
                        strReturn = ((double) objProcess).ToString("#,0.##", GlobalOptions.CultureInfo);
                    if (blnSquareBrackets)
                        strReturn = '[' + strReturn + ']';
                }
                else if (decimal.TryParse(strReturn, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                {
                    // Just a straight Capacity, so return the value.
                    strReturn = decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Total cost of the just the Gear itself before we factor in any multipliers.
        /// </summary>
        public decimal OwnCostPreMultipliers
        {
            get
            {
                string strCostExpression = Cost;

                if (strCostExpression.StartsWith("FixedValues("))
                {
                    string[] strValues = strCostExpression.TrimStartOnce("FixedValues(", true).TrimEndOnce(')').Split(',');
                    strCostExpression = strValues[Math.Max(Math.Min(Rating, strValues.Length) - 1, 0)].Trim('[', ']');
                }

                decimal decGearCost = 0;
                decimal decParentCost = 0;
                if (Parent != null)
                {
                    if (strCostExpression.Contains("Gear Cost"))
                        decGearCost = ((Gear) Parent).CalculatedCost;
                    if (strCostExpression.Contains("Parent Cost"))
                        decParentCost = ((Gear)Parent).OwnCostPreMultipliers;
                }
                decimal decTotalChildrenCost = 0;
                if (Children.Count > 0 && strCostExpression.Contains("Children Cost"))
                {
                    object decTotalChildrenCostLock = new object();
                    Parallel.ForEach(Children, loopGear =>
                    {
                        decimal decLoop = loopGear.CalculatedCost;
                        lock (decTotalChildrenCostLock)
                            decTotalChildrenCost += decLoop;
                    });
                }

                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                StringBuilder objCost = new StringBuilder(strCostExpression.TrimStart('+'));
                objCost.Replace("Gear Cost", decGearCost.ToString(GlobalOptions.InvariantCultureInfo));
                objCost.Replace("Children Cost", decTotalChildrenCost.ToString(GlobalOptions.InvariantCultureInfo));
                objCost.Replace("Parent Rating", (Parent as IHasRating)?.Rating.ToString(GlobalOptions.InvariantCultureInfo) ?? "0");
                objCost.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                objCost.Replace("Parent Cost", decParentCost.ToString(GlobalOptions.InvariantCultureInfo));

                foreach (CharacterAttrib objLoopAttribute in _objCharacter.AttributeSection.AttributeList.Concat(_objCharacter.AttributeSection.SpecialAttributeList))
                {
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev, () => objLoopAttribute.TotalValue.ToString());
                    objCost.CheapReplace(strCostExpression, objLoopAttribute.Abbrev + "Base", () => objLoopAttribute.TotalBase.ToString());
                }

                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                object objProcess = CommonFunctions.EvaluateInvariantXPath(objCost.ToString(), out bool blnIsSuccess);
                decimal decReturn = blnIsSuccess ? Convert.ToDecimal(objProcess, GlobalOptions.InvariantCultureInfo) : 0;

                if (DiscountCost)
                    decReturn *= 0.9m;

                return decReturn;
            }
        }

        /// <summary>
        /// Total cost of the just the Gear itself.
        /// </summary>
        public decimal CalculatedCost => (OwnCostPreMultipliers * Quantity) / CostFor;

        /// <summary>
        /// Total cost of the Gear and its accessories.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decReturn = OwnCostPreMultipliers;

                decimal decPlugin = 0;
                if (Children.Count > 0)
                {
                    // Add in the cost of all child components.
                    object decPluginLock = new object();
                    Parallel.ForEach(Children, objChild =>
                    {
                        decimal decLoop = objChild.TotalCost;
                        lock (decPluginLock)
                            decPlugin += decLoop;
                    });
                }

                // The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
                int intParentMultiplier = (Parent as IHasChildrenAndCost<Gear>)?.ChildCostMultiplier ?? 1;

                decReturn = (decReturn * Quantity * intParentMultiplier) / CostFor;
                // Add in the cost of the plugins separate since their value is not based on the Cost For number (it is always cost x qty).
                decReturn += decPlugin * Quantity;

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Gear itself.
        /// </summary>
        public decimal OwnCost => (OwnCostPreMultipliers * (Parent as IHasChildrenAndCost<Gear>)?.ChildCostMultiplier ?? 1) / CostFor;

        /// <summary>
        /// The Gear's Capacity cost if used as a plugin.
        /// </summary>
        public decimal PluginCapacity
        {
            get
            {
                string strCapacity = CalculatedCapacity;
                // If this is a multiple-capacity item, use only the second half.
                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    strCapacity = strCapacity.Substring(intPos + 1);
                }

                // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                strCapacity = strCapacity.StartsWith('[') ? strCapacity.Substring(1, strCapacity.Length - 2) : "0";
                if (strCapacity == "*")
                    return 0;
                return Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// The Gear's Capacity cost if used as an Armor plugin.
        /// </summary>
        public decimal PluginArmorCapacity
        {
            get
            {
                string strCapacity = CalculatedArmorCapacity;
                // If this is a multiple-capacity item, use only the second half.
                int intPos = strCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1)
                {
                    strCapacity = strCapacity.Substring(intPos + 1);
                }

                // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                strCapacity = strCapacity.StartsWith('[') ? strCapacity.Substring(1, strCapacity.Length - 2) : "0";
                if (strCapacity == "*")
                    return 0;
                return Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Gear.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                decimal decCapacity = 0;
                string strMyCapacity = CalculatedCapacity;
                int intPos = strMyCapacity.IndexOf("/[", StringComparison.Ordinal);
                if (intPos != -1 || !strMyCapacity.Contains('['))
                {
                    // Get the Gear base Capacity.
                    if (intPos != -1)
                    {
                        // If this is a multiple-capacity item, use only the first half.
                        strMyCapacity = strMyCapacity.Substring(0, intPos);
                        decCapacity = Convert.ToDecimal(strMyCapacity, GlobalOptions.CultureInfo);
                    }
                    else
                        decCapacity = Convert.ToDecimal(strMyCapacity, GlobalOptions.CultureInfo);

                    if (Children.Count > 0)
                    {
                        object decCapacityLock = new object();
                        // Run through its Children and deduct the Capacity costs.
                        Parallel.ForEach(Children, objChildGear =>
                        {
                            decimal decLoop = objChildGear.PluginCapacity * objChildGear.Quantity;
                            lock (decCapacityLock)
                                decCapacity -= decLoop;
                        });
                    }
                }

                return decCapacity;
            }
        }

        /// <summary>
        /// The name of the object as it should appear on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Name;

            XmlNode xmlGearDataNode = GetNode(strLanguage);
            if (xmlGearDataNode?["name"]?.InnerText == "Custom Item")
            {
                return LanguageManager.TranslateExtra(Name, strLanguage);
            }

            return xmlGearDataNode?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(CultureInfo objCulture, string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);
            string strSpaceCharacter = LanguageManager.GetString("String_Space", strLanguage);
            if (Quantity != 1.0m || Category == "Currency")
                strReturn = Quantity.ToString(Name.StartsWith("Nuyen") ? _objCharacter.Options.NuyenFormat : Category == "Currency" ? "#,0.00" : "#,0.##", objCulture) + strSpaceCharacter + strReturn;
            if (Rating > 0)
                strReturn += strSpaceCharacter + '(' + LanguageManager.GetString("String_Rating", strLanguage) + strSpaceCharacter + Rating.ToString(objCulture) + ')';
            if (!string.IsNullOrEmpty(Extra))
                strReturn += strSpaceCharacter + '(' + LanguageManager.TranslateExtra(Extra, strLanguage) + ')';

            if (!string.IsNullOrEmpty(GearName))
            {
                strReturn += strSpaceCharacter + "(\"" + GearName + "\")";
            }

            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);

        /// <summary>
        /// Weapon Bonus Damage.
        /// </summary>
        public string WeaponBonusDamage(string strLanguage)
        {
            if (_nodWeaponBonus == null)
                return string.Empty;
            else
            {
                string strReturn = _nodWeaponBonus["damagereplace"]?.InnerText ?? "0";
                // Use the damagereplace value if applicable.
                if (strReturn == "0")
                {
                    // Use the damage bonus if available, otherwise use 0.
                    strReturn = _nodWeaponBonus["damage"]?.InnerText ?? "0";

                    // Attach the type if applicable.
                    strReturn += _nodWeaponBonus["damagetype"]?.InnerText ?? string.Empty;

                    // If this does not start with "-", add a "+" to the string.
                    if (!strReturn.StartsWith('-'))
                        strReturn = '+' + strReturn;
                }

                // Translate the Avail string.
                if (strLanguage != GlobalOptions.DefaultLanguage)
                {
                    strReturn = strReturn.CheapReplace("P", () => LanguageManager.GetString("String_DamagePhysical", strLanguage))
                        .CheapReplace("S", () => LanguageManager.GetString("String_DamageStun", strLanguage));
                }

                return strReturn;
            }
        }

        /// <summary>
        /// Weapon Bonus AP.
        /// </summary>
        public string WeaponBonusAP
        {
            get
            {
                if (_nodWeaponBonus == null)
                    return string.Empty;
                else
                {
                    // Use the apreplace value if applicable.
                    // Use the ap bonus if available, otherwise use 0.
                    string strReturn = _nodWeaponBonus["apreplace"]?.InnerText ?? _nodWeaponBonus["ap"]?.InnerText ?? "0";

                    // If this does not start with "-", add a "+" to the string.
                    if (!strReturn.StartsWith('-'))
                        strReturn = '+' + strReturn;

                    return strReturn;
                }
            }
        }

        /// <summary>
        /// Weapon Bonus Range.
        /// </summary>
        public int WeaponBonusRange => Convert.ToInt32(_nodWeaponBonus?["rangebonus"]?.InnerText);


        /// <summary>
        /// Base Matrix Boxes.
        /// </summary>
        public int BaseMatrixBoxes => 8;

        /// <summary>
        /// Bonus Matrix Boxes.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get => _intMatrixCMBonus;
            set => _intMatrixCMBonus = value;
        }

        /// <summary>
        /// Total Bonus Matrix Boxes (including all children).
        /// </summary>
        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intReturn = BonusMatrixBoxes;
                if (Name == "Living Persona")
                {
                    string strExpression = string.Concat(CharacterObject.Improvements.Where(x => x.ImproveType == Improvement.ImprovementType.LivingPersonaMatrixCM && x.Enabled).Select(x => x.ImprovedName));
                    intReturn += string.IsNullOrEmpty(strExpression) ? 0 : ProcessRatingString(strExpression);
                }
                intReturn += Children.Where(g => g.Equipped)
                    .Sum(loopGear => loopGear.TotalBonusMatrixBoxes);
                return intReturn;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM => BaseMatrixBoxes + (this.GetTotalMatrixAttribute("Device Rating") + 1) / 2 + TotalBonusMatrixBoxes;

        /// <summary>
        /// Matrix Condition Monitor boxes filled.
        /// </summary>
        public int MatrixCMFilled
        {
            get => _intMatrixCMFilled;
            set => _intMatrixCMFilled = value;
        }
        #endregion

        #region Methods
        public bool IsIdenticalToOtherGear(Gear objOtherGear)
        {
            if (Name == objOtherGear.Name &&
                Category == objOtherGear.Category &&
                Rating == objOtherGear.Rating &&
                Extra == objOtherGear.Extra &&
                GearName == objOtherGear.GearName &&
                Notes == objOtherGear.Notes)
            {
                if (Children.DeepMatch(objOtherGear.Children, x => x.Children, (x, y) => x.Quantity == y.Quantity && x.IsIdenticalToOtherGear(y)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Change the Equipped status of a piece of Gear and all of its children.
        /// </summary>
        /// <param name="blnEquipped">Whether or not the Gear should be marked as Equipped.</param>
        public void ChangeEquippedStatus(bool blnEquipped)
        {
            if (blnEquipped)
            {
                // Add any Improvements from the Gear.
                if (Category != "Stacked Focus")
                {
                    if (!Category.EndsWith("Foci") || Bonded)
                    {
                        ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Gear && x.SourceName == InternalId).ToList());
                    }
                }
                else
                {
                    // Stacked Foci need to be handled a little differently.
                    foreach (StackedFocus objStack in _objCharacter.StackedFoci)
                    {
                        if (objStack.GearId == InternalId && objStack.Bonded)
                        {
                            ImprovementManager.EnableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.StackedFocus && x.SourceName == objStack.InternalId).ToList());
                        }
                    }
                }
            }
            else
            {
                // Remove any Improvements from the Gear.
                if (Category != "Stacked Focus")
                    ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.Gear && x.SourceName == InternalId).ToList());
                else
                {
                    // Stacked Foci need to be handled a little differetnly.
                    foreach (StackedFocus objStack in _objCharacter.StackedFoci)
                    {
                        if (objStack.GearId == InternalId)
                        {
                            ImprovementManager.DisableImprovements(_objCharacter, _objCharacter.Improvements.Where(x => x.ImproveSource == Improvement.ImprovementSource.StackedFocus && x.SourceName == objStack.InternalId).ToList());
                        }
                    }
                }
            }

            if (Children.Count > 0)
                foreach (Gear objGear in Children)
                    objGear.ChangeEquippedStatus(blnEquipped);
        }

        /// <summary>
        /// Recursive method to delete a piece of Gear and its Improvements from the character. Returns total extra cost removed unrelated to children.
        /// </summary>
        public decimal DeleteGear()
        {
            decimal decReturn = 0;
            // Remove any children the Gear may have.
            foreach (Gear objChild in Children)
                decReturn += objChild.DeleteGear();

            // Remove the Gear Weapon created by the Gear if applicable.
            if (!WeaponID.IsEmptyGuid())
            {
                List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>> lstWeaponsToDelete = new List<Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>>();
                foreach (Weapon objWeapon in _objCharacter.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                {
                    lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, null, null, null));
                }
                foreach (Vehicle objVehicle in _objCharacter.Vehicles)
                {
                    foreach (Weapon objWeapon in objVehicle.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                    {
                        lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, null, null));
                    }

                    foreach (VehicleMod objMod in objVehicle.Mods)
                    {
                        foreach (Weapon objWeapon in objMod.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                        {
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, objMod, null));
                        }
                    }

                    foreach (WeaponMount objMount in objVehicle.WeaponMounts)
                    {
                        foreach (Weapon objWeapon in objMount.Weapons.DeepWhere(x => x.Children, x => x.ParentID == InternalId))
                        {
                            lstWeaponsToDelete.Add(new Tuple<Weapon, Vehicle, VehicleMod, WeaponMount>(objWeapon, objVehicle, null, objMount));
                        }
                    }
                }
                // We need this list separate because weapons to remove can contain gear that add more weapons in need of removing
                foreach (Tuple<Weapon, Vehicle, VehicleMod, WeaponMount> objLoopTuple in lstWeaponsToDelete)
                {
                    Weapon objWeapon = objLoopTuple.Item1;
                    decReturn += objWeapon.TotalCost + objWeapon.DeleteWeapon();
                    if (objWeapon.Parent != null)
                        objWeapon.Parent.Children.Remove(objWeapon);
                    else if (objLoopTuple.Item4 != null)
                        objLoopTuple.Item4.Weapons.Remove(objWeapon);
                    else if (objLoopTuple.Item3 != null)
                        objLoopTuple.Item3.Weapons.Remove(objWeapon);
                    else if (objLoopTuple.Item2 != null)
                        objLoopTuple.Item2.Weapons.Remove(objWeapon);
                    else
                        _objCharacter.Weapons.Remove(objWeapon);
                }
            }

            decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.Gear, InternalId);

            // If a Focus is being removed, make sure the actual Focus is being removed from the character as well.
            if (Category == "Foci" || Category == "Metamagic Foci")
            {
                HashSet<Focus> lstRemoveFoci = new HashSet<Focus>();
                foreach (Focus objFocus in _objCharacter.Foci)
                {
                    if (objFocus.GearObject == this)
                        lstRemoveFoci.Add(objFocus);
                }
                foreach (Focus objFocus in lstRemoveFoci)
                {
                    /*
                    foreach (Power objPower in objCharacter.Powers)
                    {
                        if (objPower.BonusSource == objFocus.GearId)
                        {
                            //objPower.FreeLevels -= (objFocus.Rating / 4);
                        }
                    }
                    */
                    _objCharacter.Foci.Remove(objFocus);
                }
            }
            // If a Stacked Focus is being removed, make sure the Stacked Foci and its bonuses are being removed.
            else if (Category == "Stacked Focus")
            {
                StackedFocus objStack = _objCharacter.StackedFoci.FirstOrDefault(x => x.GearId == InternalId);
                if (objStack != null)
                {
                    decReturn += ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.StackedFocus, objStack.InternalId);
                    _objCharacter.StackedFoci.Remove(objStack);
                }
            }

            this.SetActiveCommlink(_objCharacter, false);
            return decReturn;
        }

        public void ReaddImprovements(TreeView treGears, StringBuilder strOutdatedItems, ICollection<string> lstInternalIdFilter, Improvement.ImprovementSource eSource = Improvement.ImprovementSource.Gear, bool blnStackEquipped = true)
        {
            // We're only re-apply improvements a list of items, not all of them
            if (lstInternalIdFilter == null || lstInternalIdFilter.Contains(InternalId))
            {
                XmlNode objNode = GetNode();
                if (objNode != null)
                {
                    if (Category == "Stacked Focus")
                    {
                        StackedFocus objStack = _objCharacter.StackedFoci.FirstOrDefault(x => x.GearId == InternalId);
                        if (objStack != null)
                        {
                            foreach (Gear objFociGear in objStack.Gear)
                            {
                                objFociGear.ReaddImprovements(treGears, strOutdatedItems, lstInternalIdFilter, Improvement.ImprovementSource.StackedFocus, blnStackEquipped);
                            }
                        }
                    }
                    Bonus = objNode["bonus"];
                    WirelessBonus = objNode["wirelessbonus"];
                    if (blnStackEquipped && Equipped)
                    {
                        if (Bonus != null)
                        {
                            ImprovementManager.ForcedValue = Extra;
                            ImprovementManager.CreateImprovements(_objCharacter, eSource, InternalId, Bonus, false, Rating, DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                Extra = ImprovementManager.SelectedValue;
                                TreeNode objGearNode = treGears.FindNode(InternalId);
                                if (objGearNode != null)
                                    objGearNode.Text = DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                            }
                        }
                        if (WirelessOn && WirelessBonus != null)
                        {
                            ImprovementManager.ForcedValue = Extra;
                            ImprovementManager.CreateImprovements(_objCharacter, eSource, InternalId, WirelessBonus, false, Rating, DisplayNameShort(GlobalOptions.Language));
                            if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                            {
                                Extra = ImprovementManager.SelectedValue;
                                TreeNode objGearNode = treGears.FindNode(InternalId);
                                if (objGearNode != null)
                                    objGearNode.Text = DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language);
                            }
                        }
                    }

                }
                else
                {
                    strOutdatedItems.AppendLine(DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language));
                }
            }
            foreach (Gear objChild in Children)
                objChild.ReaddImprovements(treGears, strOutdatedItems, lstInternalIdFilter, eSource, blnStackEquipped);
        }

        #region UI Methods
        /// <summary>
        /// Collection of TreeNodes to update when a relevant property is changed
        /// </summary>
        public ICollection<TreeNode> LinkedTreeNodes { get; } = new HashSet<TreeNode>();

        /// <summary>
        /// Build up the Tree for the current piece of Gear and all of its children.
        /// </summary>
        /// <param name="cmsGear">ContextMenuStrip for the Gear to use.</param>
        public TreeNode CreateTreeNode(ContextMenuStrip cmsGear)
        {
            if (!string.IsNullOrEmpty(ParentID) && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayName,
                Tag = this,
                ContextMenuStrip = cmsGear,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap(100)
            };

            BuildChildrenGearTree(objNode, cmsGear);

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
                if (!string.IsNullOrEmpty(ParentID))
                {
                    return SystemColors.GrayText;
                }

                return SystemColors.WindowText;
            }
        }

        /// <summary>
        /// Build up the Tree for the current piece of Gear's children.
        /// </summary>
        /// <param name="objParentNode">Parent node to which to append children gear.</param>
        /// <param name="cmsGear">ContextMenuStrip for the Gear's children to use to use.</param>
        public void BuildChildrenGearTree(TreeNode objParentNode, ContextMenuStrip cmsGear)
        {
            bool blnExpandNode = false;
            foreach (Gear objChild in Children)
            {
                TreeNode objChildNode = objChild.CreateTreeNode(cmsGear);
                if (objChildNode != null)
                {
                    objParentNode.Nodes.Add(objChildNode);
                    if (objChild.ParentID != InternalId || (GetNode()?.SelectSingleNode("gears/@startcollapsed")?.InnerText != bool.TrueString))
                        blnExpandNode = true;
                }
            }
            if (blnExpandNode)
                objParentNode.Expand();
        }

        public void SetupChildrenGearsCollectionChanged(bool blnAdd, TreeView treGear, ContextMenuStrip cmsGear = null)
        {
            if (blnAdd)
            {
                Children.AddTaggedCollectionChanged(treGear, (x, y) => this.RefreshChildrenGears(treGear, cmsGear, null, y));
                foreach (Gear objChild in Children)
                {
                    objChild.SetupChildrenGearsCollectionChanged(true, treGear, cmsGear);
                }
            }
            else
            {
                Children.RemoveTaggedCollectionChanged(treGear);
                foreach (Gear objChild in Children)
                {
                    objChild.SetupChildrenGearsCollectionChanged(false, treGear);
                }
            }
        }

        /// <summary>
        /// Refreshes a single focus' rating (for changing ratings in create mode)
        /// </summary>
        /// <param name="treFoci">TreeView of foci.</param>
        /// <param name="intNewRating">New rating that the focus is supposed to have.</param>
        /// <returns>True if the new rating complies by focus limits or the gear is not bonded, false otherwise</returns>
        public bool RefreshSingleFocusRating(TreeView treFoci, int intNewRating)
        {
            if (Bonded)
            {
                int intMaxFocusTotal = _objCharacter.MAG.TotalValue * 5;
                if (_objCharacter.Options.MysAdeptSecondMAGAttribute && _objCharacter.IsMysticAdept)
                    intMaxFocusTotal = Math.Min(intMaxFocusTotal, _objCharacter.MAGAdept.TotalValue * 5);

                int intFociTotal = _objCharacter.Foci.Where(x => x.GearObject != this).Sum(x => x.Rating);

                if (intFociTotal + intNewRating > intMaxFocusTotal && !_objCharacter.IgnoreRules)
                {
                    MessageBox.Show(LanguageManager.GetString("Message_FocusMaximumForce", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_FocusMaximum", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
            }

            Rating = intNewRating;

            switch (Category)
            {
                case "Foci":
                case "Metamagic Foci":
                    {
                        TreeNode nodFocus = treFoci.FindNodeByTag(this);
                        if (nodFocus != null)
                        {
                            nodFocus.Text = DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language).Replace(LanguageManager.GetString("String_Rating", GlobalOptions.Language), LanguageManager.GetString("String_Force", GlobalOptions.Language));
                        }
                    }
                    break;
                case "Stacked Focus":
                    {
                        for (int i = _objCharacter.StackedFoci.Count - 1; i >= 0; --i)
                        {
                            if (i < _objCharacter.StackedFoci.Count)
                            {
                                StackedFocus objStack = _objCharacter.StackedFoci[i];
                                if (objStack.GearId == InternalId)
                                {
                                    TreeNode nodFocus = treFoci.FindNode(objStack.InternalId);
                                    if (nodFocus != null)
                                    {
                                        nodFocus.Text = DisplayName(GlobalOptions.CultureInfo, GlobalOptions.Language).Replace(LanguageManager.GetString("String_Rating", GlobalOptions.Language), LanguageManager.GetString("String_Force", GlobalOptions.Language));
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    break;
            }

            return true;
        }
        #endregion

        #region Hero Lab Importing Methods
        public bool ImportHeroLabGear(XmlNode xmlGearImportNode, XmlNode xmlParentGearNode, IList<Weapon> lstWeapons)
        {
            if (xmlGearImportNode == null)
                return false;
            string strOriginalName = xmlGearImportNode.Attributes?["name"]?.InnerText ?? string.Empty;
            if (!string.IsNullOrEmpty(strOriginalName))
            {
                XmlDocument xmlGearDocument = XmlManager.Load("gear.xml");
                string strForceValue = string.Empty;
                XmlNode xmlGearDataNode = null;
                using (XmlNodeList xmlGearDataList = xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, \"" + strOriginalName + "\")]"))
                {
                    if (xmlGearDataList?.Count > 0)
                    {
                        foreach (XmlNode xmlLoopNode in xmlGearDataList)
                        {
                            XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                            if (xmlTestNode != null)
                            {
                                // Assumes topmost parent is an AND node
                                if (xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    continue;
                                }
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                            if (xmlTestNode != null)
                            {
                                // Assumes topmost parent is an AND node
                                if (!xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    continue;
                                }
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/geardetails");
                            if (xmlTestNode != null)
                            {
                                // Assumes topmost parent is an AND node
                                if (xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    continue;
                                }
                            }

                            xmlTestNode = xmlLoopNode.SelectSingleNode("required/geardetails");
                            if (xmlTestNode != null)
                            {
                                // Assumes topmost parent is an AND node
                                if (!xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                {
                                    continue;
                                }
                            }

                            xmlGearDataNode = xmlLoopNode;
                            break;
                        }
                    }
                }

                if (xmlGearDataNode == null)
                {
                    string[] astrOriginalNameSplit = strOriginalName.Split(':');
                    if (astrOriginalNameSplit.Length > 1)
                    {
                        string strName = astrOriginalNameSplit[0].Trim();
                        using (XmlNodeList xmlGearDataList = xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, \"" + strName + "\")]"))
                        {
                            if (xmlGearDataList?.Count > 0)
                            {
                                foreach (XmlNode xmlLoopNode in xmlGearDataList)
                                {
                                    XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                    if (xmlTestNode != null)
                                    {
                                        // Assumes topmost parent is an AND node
                                        if (xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            continue;
                                        }
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                    if (xmlTestNode != null)
                                    {
                                        // Assumes topmost parent is an AND node
                                        if (!xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            continue;
                                        }
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/geardetails");
                                    if (xmlTestNode != null)
                                    {
                                        // Assumes topmost parent is an AND node
                                        if (xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            continue;
                                        }
                                    }

                                    xmlTestNode = xmlLoopNode.SelectSingleNode("required/geardetails");
                                    if (xmlTestNode != null)
                                    {
                                        // Assumes topmost parent is an AND node
                                        if (!xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                        {
                                            continue;
                                        }
                                    }

                                    xmlGearDataNode = xmlLoopNode;
                                    break;
                                }
                            }
                        }

                        if (xmlGearDataNode != null)
                            strForceValue = astrOriginalNameSplit[1].Trim();
                    }
                    if (xmlGearDataNode == null)
                    {
                        astrOriginalNameSplit = strOriginalName.Split(',');
                        if (astrOriginalNameSplit.Length > 1)
                        {
                            string strName = astrOriginalNameSplit[0].Trim();
                            using (XmlNodeList xmlGearDataList = xmlGearDocument.SelectNodes("/chummer/gears/gear[contains(name, \"" + strName + "\")]"))
                            {
                                if (xmlGearDataList?.Count > 0)
                                {
                                    foreach (XmlNode xmlLoopNode in xmlGearDataList)
                                    {
                                        XmlNode xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/parentdetails");
                                        if (xmlTestNode != null)
                                        {
                                            // Assumes topmost parent is an AND node
                                            if (xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                            {
                                                continue;
                                            }
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNode("required/parentdetails");
                                        if (xmlTestNode != null)
                                        {
                                            // Assumes topmost parent is an AND node
                                            if (!xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                            {
                                                continue;
                                            }
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNode("forbidden/geardetails");
                                        if (xmlTestNode != null)
                                        {
                                            // Assumes topmost parent is an AND node
                                            if (xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                            {
                                                continue;
                                            }
                                        }

                                        xmlTestNode = xmlLoopNode.SelectSingleNode("required/geardetails");
                                        if (xmlTestNode != null)
                                        {
                                            // Assumes topmost parent is an AND node
                                            if (!xmlParentGearNode.ProcessFilterOperationNode(xmlTestNode, false))
                                            {
                                                continue;
                                            }
                                        }

                                        xmlGearDataNode = xmlLoopNode;
                                        break;
                                    }
                                }
                            }

                            if (xmlGearDataNode != null)
                                strForceValue = astrOriginalNameSplit[1].Trim();
                        }
                    }
                }
                if (xmlGearDataNode != null)
                {
                    Create(xmlGearDataNode, Convert.ToInt32(xmlGearImportNode.Attributes?["rating"]?.InnerText), lstWeapons, strForceValue);
                }
                else
                {
                    XmlNode xmlCustomGearDataNode = xmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = 'Custom Item']");
                    if (xmlCustomGearDataNode != null)
                    {
                        Create(xmlCustomGearDataNode, Convert.ToInt32(xmlGearImportNode.Attributes?["rating"]?.InnerText), lstWeapons, strOriginalName);
                        Cost = xmlGearImportNode.SelectSingleNode("gearcost/@value")?.InnerText;
                    }
                    else
                        return false;
                }

                if (InternalId.IsEmptyGuid())
                    return false;

                Quantity = Convert.ToDecimal(xmlGearImportNode.Attributes?["quantity"]?.InnerText ?? "1", GlobalOptions.InvariantCultureInfo);
                Notes = xmlGearImportNode["description"]?.InnerText;

                ProcessHeroLabGearPlugins(xmlGearImportNode, lstWeapons);

                return true;
            }
            return false;
        }

        public void ProcessHeroLabGearPlugins(XmlNode xmlGearImportNode, IList<Weapon> lstWeapons)
        {
            if (xmlGearImportNode == null)
                return;
            foreach (string strPluginNodeName in Character.HeroLabPluginNodeNames)
            {
                using (XmlNodeList xmlPluginsList = xmlGearImportNode.SelectNodes(strPluginNodeName + "/item[@useradded != \"no\"]"))
                {
                    if (xmlPluginsList?.Count > 0)
                    {
                        foreach (XmlNode xmlPluginToAdd in xmlPluginsList)
                        {
                            Gear objPlugin = new Gear(_objCharacter);
                            if (objPlugin.ImportHeroLabGear(xmlPluginToAdd, GetNode(), lstWeapons))
                            {
                                objPlugin.Parent = this;
                                Children.Add(objPlugin);
                            }
                        }
                    }
                }

                using (XmlNodeList xmlPluginsList = xmlGearImportNode.SelectNodes(strPluginNodeName + "/item[@useradded = \"no\"]"))
                {
                    if (xmlPluginsList?.Count > 0)
                    {
                        foreach (XmlNode xmlPluginToAdd in xmlPluginsList)
                        {
                            string strName = xmlPluginToAdd.Attributes?["name"]?.InnerText ?? string.Empty;
                            if (!string.IsNullOrEmpty(strName))
                            {
                                Gear objPlugin = Children.FirstOrDefault(x => x.IncludedInParent && (x.Name.Contains(strName) || strName.Contains(x.Name)));
                                if (objPlugin != null)
                                {
                                    objPlugin.Quantity = Convert.ToDecimal(xmlPluginToAdd.Attributes?["quantity"]?.InnerText ?? "1", GlobalOptions.InvariantCultureInfo);
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

        #region static
        //A tree of dependencies. Once some of the properties are changed,
        //anything they depend on, also needs to raise OnChanged
        //This tree keeps track of dependencies
        private static readonly DependancyGraph<string> GearDependancyGraph =
            new DependancyGraph<string>(
                new DependancyGraphNode<string>(nameof(CurrentDisplayName),
                    new DependancyGraphNode<string>(nameof(DisplayName),
                        new DependancyGraphNode<string>(nameof(DisplayNameShort),
                            new DependancyGraphNode<string>(nameof(Name))
                        ),
                        new DependancyGraphNode<string>(nameof(Quantity)),
                        new DependancyGraphNode<string>(nameof(Rating)),
                        new DependancyGraphNode<string>(nameof(Extra)),
                        new DependancyGraphNode<string>(nameof(GearName))
                    )
                ),
                new DependancyGraphNode<string>(nameof(PreferredColor),
                    new DependancyGraphNode<string>(nameof(Notes)),
                    new DependancyGraphNode<string>(nameof(ParentID))
                )
            );
        #endregion

        /// <summary>
        /// Recursive method to add a Gear's Improvements to a character when moving them from a Vehicle.
        /// </summary>
        /// <param name="objGear">Gear to create Improvements for.
        /// </param>
        public void AddGearImprovements()
        {
            string strForce = string.Empty;
            if (Bonus != null || (WirelessOn && WirelessBonus != null))
            {
                if (!string.IsNullOrEmpty(Extra))
                    strForce = Extra;
                ImprovementManager.ForcedValue = strForce;
                if (Bonus != null)
                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, InternalId, Bonus, true, Rating, DisplayNameShort(GlobalOptions.Language));
                if (WirelessOn && WirelessBonus != null)
                    ImprovementManager.CreateImprovements(CharacterObject, Improvement.ImprovementSource.Gear, InternalId, WirelessBonus, true, Rating, DisplayNameShort(GlobalOptions.Language));
            }
            foreach (Gear objChild in Children)
                objChild.AddGearImprovements();
        }

        public bool Remove(Character characterObject, bool blnConfirmDelete = true)
        {
            if (blnConfirmDelete)
            {
                if (!characterObject.ConfirmDelete(LanguageManager.GetString("Message_DeleteGear",
                    GlobalOptions.Language)))
                    return false;
            }

            switch (Parent)
            {
                case IHasGear objHasChildren:
                    DeleteGear();
                    objHasChildren.Gear.Remove(this);
                    break;
                case IHasChildren<Gear> objHasChildren:
                    DeleteGear();
                    objHasChildren.Children.Remove(this);
                    break;
                default:
                    DeleteGear();
                    characterObject.Gear.Remove(this);
                    break;
            }

            return true;
        }

        public void Sell(Character characterObject, decimal percentage)
        {
            decimal decOriginal = 0;
            decimal decNewCost = 0;
            if (CharacterObject.Gear.Any(gear => gear == this))
            {
                decOriginal = TotalCost;
                CharacterObject.Gear.Remove(this);
            }
            else if (Parent != null && Parent is IHasChildrenAndCost<Gear> parentObject)
            {
                decOriginal = parentObject.TotalCost;
                parentObject.Children.Remove(this);
                decNewCost = parentObject.TotalCost;
            }

            // Create the Expense Log Entry for the sale.
            decimal decAmount = (decOriginal - decNewCost) * percentage;
            decAmount += DeleteGear() * percentage;
            ExpenseLogEntry objExpense = new ExpenseLogEntry(CharacterObject);
            string strEntry = LanguageManager.GetString("String_ExpenseSoldCyberwareGear", GlobalOptions.Language);
            objExpense.Create(decAmount, strEntry + ' ' + DisplayNameShort(GlobalOptions.Language), ExpenseType.Nuyen,
                DateTime.Now);
            CharacterObject.ExpenseEntries.AddWithSort(objExpense);
            CharacterObject.Nuyen += decAmount;
        }
        
        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail?.Language != GlobalOptions.Language)
                _objCachedSourceDetail = null;
            SourceDetail.SetControl(sourceControl);
        }

        [NotifyPropertyChangedInvocator]
        public void OnPropertyChanged([CallerMemberName] string strPropertyName = null)
        {
            OnMultiplePropertyChanged(strPropertyName);
        }

        public void OnMultiplePropertyChanged(params string[] lstPropertyNames)
        {
            ICollection<string> lstNamesOfChangedProperties = null;
            foreach (string strPropertyName in lstPropertyNames)
            {
                if (lstNamesOfChangedProperties == null)
                    lstNamesOfChangedProperties = GearDependancyGraph.GetWithAllDependants(strPropertyName);
                else
                {
                    foreach (string strLoopChangedProperty in GearDependancyGraph.GetWithAllDependants(strPropertyName))
                        lstNamesOfChangedProperties.Add(strLoopChangedProperty);
                }
            }

            if ((lstNamesOfChangedProperties?.Count > 0) != true)
                return;

            foreach (string strPropertyToChange in lstNamesOfChangedProperties)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(strPropertyToChange));
            }
        }
    }
}
