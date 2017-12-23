using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// Standard Character Gear.
    /// </summary>
    public class Gear : IHasChildren<Gear>, IHasName, IHasInternalId, IHasXmlNode, IHasMatrixAttributes
    {
        private Guid _guiID;
        private string _SourceGuid;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private int _intMaxRating = 0;
        private int _intMinRating = 0;
        private int _intRating = 0;
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
        private bool _blnBonded = false;
        private bool _blnEquipped = true;
        private bool _blnWirelessOn = true;
        private XmlNode _nodBonus;
        private XmlNode _nodWirelessBonus;
        private XmlNode _nodWeaponBonus;
        private Guid _guiWeaponID = Guid.Empty;
        private List<Gear> _objChildren = new List<Gear>();
        private string _strNotes = string.Empty;
        private string _strLocation = string.Empty;
        private Character _objCharacter;
        private int _intChildCostMultiplier = 1;
        private int _intChildAvailModifier = 0;
        private Gear _objParent = null;
        private bool _blnDiscountCost = false;
        private string _strGearName = string.Empty;
        private string _strParentID = string.Empty;
        private int _intMatrixCMBonus = 0;
        private int _intMatrixCMFilled = 0;
        private string _strForcedValue = string.Empty;
        private bool _blnDisableQuantity = false;
        private bool _blnAllowRename = false;

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
        private bool _blnCanSwapAttributes = false;

        #region Constructor, Create, Save, Load, and Print Methods
        public Gear(Character objCharacter)
        {
            // Create the GUID for the new piece of Gear.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Gear from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlGear">XmlNode to create the object from.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="intRating">Selected Rating for the Gear.</param>
        /// <param name="objWeapons">List of Weapons that should be added to the character.</param>
        /// <param name="objWeaponNodes">List of TreeNodes to represent the added Weapons</param>
        /// <param name="strForceValue">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnAddImprovements">Whether or not Improvements should be added to the character.</param>
        /// <param name="blnCreateChildren">Whether or not child Gear should be created.</param>
        /// <param name="blnAerodynamic">Whether or not Weapons should be created as Aerodynamic.</param>
        public void Create(XmlNode objXmlGear, TreeNode objNode, int intRating, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, string strForceValue = "", bool blnAddImprovements = true, bool blnCreateChildren = true, bool blnAerodynamic = false)
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
            _blnWirelessOn = _nodWirelessBonus != null;
            objXmlGear.TryGetInt32FieldQuickly("rating", ref _intMaxRating);
            objXmlGear.TryGetInt32FieldQuickly("minrating", ref _intMinRating);
            objXmlGear.TryGetStringFieldQuickly("notes", ref _strNotes);
            _intRating = Math.Max(Math.Min(intRating, MaxRating), MinRating);
            objXmlGear.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            objXmlGear.TryGetInt32FieldQuickly("matrixcmbonus", ref _intMatrixCMBonus);
            objXmlGear.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlGear.TryGetStringFieldQuickly("page", ref _strPage);
            objXmlGear.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            objXmlGear.TryGetBoolFieldQuickly("disablequantity", ref _blnDisableQuantity);
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
                        _strName = frmPickText.SelectedValue;
                        _objCachedMyXmlNode = null;
                    }
                }
                else
                {
                    string strCustomName = LanguageManager.GetString(_strForcedValue, GlobalOptions.Language, false);
                    if (string.IsNullOrEmpty(strCustomName))
                        strCustomName = LanguageManager.TranslateExtra(_strForcedValue, GlobalOptions.Language);
                    _strName = strCustomName;
                    _objCachedMyXmlNode = null;
                }
            }
            // Check for a Variable Cost.
            if (!string.IsNullOrEmpty(_strCost))
            {
                if (_strCost.StartsWith("Variable") && string.IsNullOrEmpty(_strForcedValue))
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

            string strSource = _guiID.ToString();

            objNode.Text = DisplayName(GlobalOptions.Language);
            objNode.Tag = _guiID.ToString();

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
                objNode.Text += " (" + _strExtra + ")";
            }

            // Add Gear Weapons if applicable.
            if (objXmlGear.InnerXml.Contains("<addweapon>"))
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddWeapon in objXmlGear.SelectNodes("addweapon"))
                {
                    string strLoopID = objXmlAddWeapon.InnerText;
                    XmlNode objXmlWeapon = strLoopID.IsGuid()
                        ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                        : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");

                    List<TreeNode> lstGearWeaponNodes = new List<TreeNode>();
                    Weapon objGearWeapon = new Weapon(_objCharacter);
                    objGearWeapon.Create(objXmlWeapon, lstGearWeaponNodes, null, null, objWeapons, null, true, blnAddImprovements);
                    objGearWeapon.ParentID = InternalId;
                    foreach (TreeNode objLoopNode in lstGearWeaponNodes)
                    {
                        objLoopNode.ForeColor = SystemColors.GrayText;
                        if (blnAerodynamic)
                        {
                            objGearWeapon.Name += " (" + LanguageManager.GetString("Checkbox_Aerodynamic", GlobalOptions.Language) + ")";
                            objGearWeapon.Range = "Aerodynamic Grenades";
                            objLoopNode.Text = objGearWeapon.DisplayName(GlobalOptions.Language);
                            _strName += " (" + LanguageManager.GetString("Checkbox_Aerodynamic", GlobalOptions.Language) + ")";
                            objNode.Text = DisplayName(GlobalOptions.Language);
                        }
                        objWeaponNodes.Add(objLoopNode);
                    }
                    objWeapons.Add(objGearWeapon);

                    _guiWeaponID = Guid.Parse(objGearWeapon.InternalId);
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
                        objNode.Text += " (" + ImprovementManager.SelectedValue + ")";
                    }
                }
            }

            // Add the Copy Protection and Registration plugins to the Matrix program. This does not apply if Unwired is not enabled, Hacked is selected, or this is a Suite being added (individual programs will add it to themselves).
            if (blnCreateChildren)
            {
                // Check to see if there are any child elements.
                CreateChildren(objXmlDocument, objXmlGear, this, objNode, blnAddImprovements);
            }

            // If the item grants a Weapon bonus (Ammunition), just fill the WeaponBonus XmlNode.
            _nodWeaponBonus = objXmlGear["weaponbonus"];
            objNode.Text = DisplayName(GlobalOptions.Language);

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

        public void CreateChildren(XmlDocument objXmlGearDocument, XmlNode objXmlGear, Gear objParent, TreeNode objNode, bool blnAddImprovements)
        {
            XmlNode objGearsNode = objXmlGear["gears"];
            if (objGearsNode != null)
            {
                bool blnStartCollapsed = objGearsNode.Attributes?["startcollapsed"]?.InnerText == "yes";
                // Create Gear by looking up the name of the item we're provided with.
                if (objGearsNode.SelectNodes("usegear").Count > 0)
                {
                    foreach (XmlNode objXmlChild in objGearsNode.SelectNodes("usegear"))
                    {
                        CreateChild(objXmlGearDocument, objXmlChild, objParent, objNode, blnAddImprovements);
                    }
                }
                // Create Gear by choosing from pre-determined lists.
                if (objGearsNode.SelectNodes("choosegear").Count > 0)
                {
                    bool blnCancelledDialog = false;
                    List<XmlNode> lstChildrenToCreate = new List<XmlNode>();
                    foreach (XmlNode objXmlChooseGearNode in objGearsNode.SelectNodes("choosegear"))
                    {
                        // Each list is processed on its own and has usegear members
                        XmlNodeList objXmlNodeList = objXmlChooseGearNode.SelectNodes("usegear");

                        List<ListItem> lstGears = new List<ListItem>();
                        foreach (XmlNode objChoiceNode in objXmlNodeList)
                        {
                            XmlNode objXmlLoopGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objChoiceNode["name"].InnerText + "\" and category = \"" + objChoiceNode["category"].InnerText + "\"]");
                            if (objXmlLoopGear == null)
                                continue;
                            if (objXmlLoopGear["forbidden"]?["geardetails"] != null)
                            {
                                // Assumes topmost parent is an AND node
                                if (objXmlGear.ProcessFilterOperationNode(objXmlLoopGear["forbidden"]["geardetails"], false))
                                {
                                    continue;
                                }
                            }
                            if (objXmlLoopGear["required"]?["geardetails"] != null)
                            {
                                // Assumes topmost parent is an AND node
                                if (!objXmlGear.ProcessFilterOperationNode(objXmlLoopGear["required"]["geardetails"], false))
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
                            if (objXmlChooseGearNode["required"]?.InnerText == "yes")
                            {
                                blnCancelledDialog = true;
                                break;
                            }
                            else
                                continue;
                        }

                        string strChooseGearNodeName = objXmlChooseGearNode["name"]?.InnerText ?? string.Empty;
                        string strFriendlyName = LanguageManager.GetString(strChooseGearNodeName, GlobalOptions.Language, false);
                        if (string.IsNullOrEmpty(strFriendlyName))
                            strFriendlyName = LanguageManager.TranslateExtra(strChooseGearNodeName, GlobalOptions.Language);
                        frmSelectItem frmPickItem = new frmSelectItem
                        {
                            Description = LanguageManager.GetString("String_Improvement_SelectText", GlobalOptions.Language).Replace("{0}", strFriendlyName),
                            GeneralItems = lstGears
                        };

                        frmPickItem.ShowDialog();

                        // Make sure the dialogue window was not canceled.
                        if (frmPickItem.DialogResult == DialogResult.Cancel)
                        {
                            if (objXmlChooseGearNode["required"]?.InnerText == "yes")
                            {
                                blnCancelledDialog = true;
                                break;
                            }
                            else
                                continue;
                        }

                        XmlNode objXmlChosenGear = objXmlChooseGearNode.SelectSingleNode("usegear[name = \"" + frmPickItem.SelectedItem + "\"]");

                        if (objXmlChosenGear == null)
                        {
                            if (objXmlChooseGearNode["required"]?.InnerText == "yes")
                            {
                                blnCancelledDialog = true;
                                break;
                            }
                            else
                                continue;
                        }
                        else
                            lstChildrenToCreate.Add(objXmlChosenGear);
                    }
                    if (!blnCancelledDialog)
                    {
                        foreach (XmlNode objXmlChild in lstChildrenToCreate)
                        {
                            CreateChild(objXmlGearDocument, objXmlChild, objParent, objNode, blnAddImprovements);
                        }
                    }
                }

                if (!blnStartCollapsed && objNode.GetNodeCount(false) > 0)
                    objNode.Expand();
            }
        }

        protected void CreateChild(XmlDocument objXmlGearDocument, XmlNode objXmlChild, Gear objParent, TreeNode objNode, bool blnAddImprovements)
        {
            XmlNode objXmlChildName = objXmlChild["name"];
            XmlAttributeCollection objXmlChildNameAttributes = objXmlChildName.Attributes;
            XmlNode objXmlGearNode = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlChildName.InnerText + "\" and category = \"" + objXmlChild["category"].InnerText + "\"]");
            if (objXmlGearNode == null)
                return;
            int intChildRating = 0;
            decimal decChildQty = 1;
            string strChildForceSource = string.Empty;
            string strChildForcePage = string.Empty;
            string strChildForceValue = string.Empty;
            bool blnCreateChildren = objXmlChildNameAttributes["createchildren"]?.InnerText != "no";
            bool blnAddChildImprovements = blnAddImprovements;
            if (objXmlChildNameAttributes["addimprovements"]?.InnerText == "no")
                blnAddChildImprovements = false;
            if (objXmlChild["rating"] != null)
                intChildRating = Convert.ToInt32(objXmlChild["rating"].InnerText);
            if (objXmlChildNameAttributes["qty"] != null)
                decChildQty = Convert.ToDecimal(objXmlChildNameAttributes["qty"].InnerText, GlobalOptions.InvariantCultureInfo);
            if (objXmlChildNameAttributes["select"] != null)
                strChildForceValue = objXmlChildNameAttributes["select"].InnerText;
            if (objXmlChild["source"] != null)
                strChildForceSource = objXmlChild["source"].InnerText;
            if (objXmlChild["page"] != null)
                strChildForcePage = objXmlChild["page"].InnerText;

            Gear objChild = new Gear(_objCharacter);
            TreeNode objChildNode = new TreeNode();
            List<Weapon> lstChildWeapons = new List<Weapon>();
            List<TreeNode> lstChildWeaponNodes = new List<TreeNode>();
            objChild.Create(objXmlGearNode, objChildNode, intChildRating, lstChildWeapons, lstChildWeaponNodes, strChildForceValue, blnAddChildImprovements, blnCreateChildren);
            objChild.Quantity = decChildQty;
            objChild.Cost = "0";
            objChild.MinRating = intChildRating;
            objChild.MaxRating = intChildRating;
            objChild.Parent = objParent;
            objChild.ParentID = objParent.InternalId;
            if (!string.IsNullOrEmpty(strChildForceSource))
                objChild.Source = strChildForceSource;
            if (!string.IsNullOrEmpty(strChildForcePage))
                objChild.Page = strChildForcePage;
            objChildNode.ForeColor = SystemColors.GrayText;
            objChildNode.ContextMenuStrip = objNode.ContextMenuStrip;
            objParent.Children.Add(objChild);
            this.RefreshMatrixAttributeArray();

            // Change the Capacity of the child if necessary.
            if (objXmlChild["capacity"] != null)
                objChild.Capacity = "[" + objXmlChild["capacity"].InnerText + "]";

            objNode.Nodes.Add(objChildNode);

            CreateChildren(objXmlGearDocument, objXmlChild, objChild, objChildNode, blnAddChildImprovements);
        }

        /// <summary>
        /// Copy a piece of Gear.
        /// </summary>
        /// <param name="objGear">Gear object to copy.</param>
        /// <param name="objNode">TreeNode for the copied item.</param>
        /// <param name="objWeapons">List of Weapons created by the copied item.</param>
        /// <param name="objWeaponNodes">List of TreeNodes for the Weapons created by the copied item.</param>
        public void Copy(Gear objGear, TreeNode objNode, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes)
        {
            _objCachedMyXmlNode = objGear.GetNode();
            _SourceGuid = objGear._SourceGuid;
            _blnAllowRename = objGear.AllowRename;
            _strName = objGear.Name;
            _strCategory = objGear.Category;
            _intMaxRating = objGear.MaxRating;
            _intMinRating = objGear.MinRating;
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
            _blnDisableQuantity = objGear.DisableQuantity;
            _strExtra = objGear.Extra;
            _blnBonded = objGear.Bonded;
            _blnEquipped = objGear.Equipped;
            _blnWirelessOn = objGear.WirelessOn;
            _nodBonus = objGear.Bonus;
            _nodWirelessBonus = objGear.WirelessBonus;
            _nodWeaponBonus = objGear.WeaponBonus;
            _guiWeaponID = Guid.Parse(objGear.WeaponID);
            _strNotes = objGear.Notes;
            _strLocation = objGear.Location;
            _intChildAvailModifier = objGear.ChildAvailModifier;
            _intChildCostMultiplier = objGear.ChildCostMultiplier;
            _strGearName = objGear.GearName;
            _strForcedValue = objGear._strForcedValue;

            objNode.Text = DisplayName(GlobalOptions.Language);
            objNode.Tag = _guiID.ToString();

            foreach (Gear objGearChild in objGear.Children)
            {
                TreeNode objChildNode = new TreeNode();
                Gear objChild = new Gear(_objCharacter);
                objChild.Copy(objGearChild, objChildNode, objWeapons, objWeaponNodes);
                _objChildren.Add(objChild);

                objNode.Nodes.Add(objChildNode);
                objNode.Expand();
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

            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("id", _SourceGuid);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("minrating", _intMinRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("rating", Rating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("qty", _decQty.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("avail", _strAvail);
            if (_decCostFor > 1)
                objWriter.WriteElementString("costfor", _decCostFor.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("bonded", _blnBonded.ToString());
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString());
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
            objWriter.WriteElementString("disablequantity", _blnDisableQuantity.ToString());
            objWriter.WriteElementString("devicerating", _strDeviceRating);
            objWriter.WriteElementString("gearname", _strGearName);
            objWriter.WriteElementString("forcedvalue", _strForcedValue);
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("matrixcmbonus", _intMatrixCMBonus.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("allowrename", _blnAllowRename.ToString());
            if (_intChildCostMultiplier != 1)
                objWriter.WriteElementString("childcostmultiplier", _intChildCostMultiplier.ToString(CultureInfo.InvariantCulture));
            if (_intChildAvailModifier != 0)
                objWriter.WriteElementString("childavailmodifier", _intChildAvailModifier.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteStartElement("children");
            foreach (Gear objGear in _objChildren)
            {
                objGear.Save(objWriter);
            }
            objWriter.WriteEndElement();
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", DiscountCost.ToString());

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

            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Gear from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            _guiID = Guid.Parse(objNode["guid"].InnerText);
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
            objNode.TryGetInt32FieldQuickly("minrating", ref _intMinRating);
            objNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
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
                _blnWirelessOn = _nodWirelessBonus != null;
            _nodWeaponBonus = objNode["weaponbonus"];
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            bool blnNeedCommlinkLegacyShim = !objNode.TryGetStringFieldQuickly("canformpersona", ref _strCanFormPersona);
            objNode.TryGetBoolFieldQuickly("disablequantity", ref _blnDisableQuantity);
            if (!objNode.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating))
                GetNode()?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            string strWeaponID = string.Empty;
            if (objNode.TryGetStringFieldQuickly("weaponguid", ref strWeaponID))
            {
                _guiWeaponID = Guid.Parse(strWeaponID);
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
                    _strParentID = Guid.NewGuid().ToString();
                }
            }

            if (objNode.InnerXml.Contains("<gear>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("children/gear");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Gear objGear = new Gear(_objCharacter);
                    objGear.Load(nodChild, blnCopy);
                    objGear.Parent = this;
                    _objChildren.Add(objGear);
                }
            }

            // Legacy Shim
            if (_intMaxRating != 0 && _strName.Contains("Certified Credstick"))
            {
                XmlNode objNuyenNode = XmlManager.Load("gear.xml")?.SelectSingleNode("/chummer/gears/gear[contains(name, \"Nuyen\") and category = \"Currency\"]");
                if (objNuyenNode != null)
                {
                    if (Rating > 0)
                    {
                        Gear objNuyenGear = new Gear(_objCharacter);
                        objNuyenGear.Create(objNuyenNode, new TreeNode(), 0, new List<Weapon>(), new List<TreeNode>());
                        objNuyenGear.Parent = this;
                        objNuyenGear.Quantity = Rating;
                        _objChildren.Add(objNuyenGear);
                    }
                    GetNode()?.TryGetInt32FieldQuickly("rating", ref _intMaxRating);
                    GetNode()?.TryGetInt32FieldQuickly("minrating", ref _intMinRating);
                    Rating = Math.Max(Math.Min(0, _intMaxRating), _intMinRating);
                    GetNode()?.TryGetStringFieldQuickly("capacity", ref _strCapacity);
                }
            }

            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);

            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);

            // Convert old qi foci to the new bonus. In order to force the user to update their powers, unequip the focus and remove all improvements.
            if (_strName == "Qi Focus")
            {
                Version.TryParse("5.193.5", out Version test);
                if (test != null)
                {
                    int intResult = _objCharacter.LastSavedVersion.CompareTo(test);
                    //Check for typo in Corrupter quality and correct it
                    if (intResult == -1)
                    {
                        XmlDocument objXmlDocument = XmlManager.Load("gear.xml");
                        XmlNode gear = objXmlDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + _strName + "\"]");
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

            this.RefreshMatrixAttributeArray();

            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
                _strLocation = string.Empty;
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            objWriter.WriteStartElement("gear");

            if ((_strCategory == "Foci" || _strCategory == "Metamagic Foci") && _blnBonded)
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint) + " (" + LanguageManager.GetString("Label_BondedFoci", strLanguageToPrint) + ")");
            else
                objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));

            objWriter.WriteElementString("name_english", _strName);
            objWriter.WriteElementString("category", DisplayCategory(strLanguageToPrint));
            objWriter.WriteElementString("category_english", _strCategory);
            objWriter.WriteElementString("iscommlink", IsCommlink.ToString());
            objWriter.WriteElementString("ispersona", (Name == "Living Persona").ToString());
            //objWriter.WriteElementString("isnexus", (_strCategory == "Nexus").ToString());
            objWriter.WriteElementString("isammo", (_strCategory == "Ammunition").ToString());
            objWriter.WriteElementString("isprogram", IsProgram.ToString());
            objWriter.WriteElementString("isos", System.Boolean.FalseString);
            if (_strName == "Fake SIN")
                objWriter.WriteElementString("issin", System.Boolean.TrueString);
            else
                objWriter.WriteElementString("issin", System.Boolean.FalseString);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("armorcapacity", _strArmorCapacity);
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(objCulture));
            objWriter.WriteElementString("rating", _intRating.ToString(objCulture));
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(objCulture));
            objWriter.WriteElementString("conditionmonitor", MatrixCM.ToString(objCulture));
            objWriter.WriteElementString("qty", _decQty.ToString(Name.StartsWith("Nuyen") ? _objCharacter.Options.NuyenFormat : Category == "Currency" ? "#,0.00" : "#,0.##", objCulture));
            objWriter.WriteElementString("avail", TotalAvail(GlobalOptions.CultureInfo, strLanguageToPrint, true));
            objWriter.WriteElementString("avail_english", TotalAvail(GlobalOptions.CultureInfo, GlobalOptions.DefaultLanguage, true));
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("extra", LanguageManager.TranslateExtra(_strExtra, strLanguageToPrint));
            objWriter.WriteElementString("bonded", _blnBonded.ToString());
            objWriter.WriteElementString("equipped", _blnEquipped.ToString());
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("gearname", _strGearName);
            objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteStartElement("children");
            foreach (Gear objGear in _objChildren)
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
                objWriter.WriteElementString("notes", _strNotes);

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
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }
        public string SourceID
        {
            get
            {
                return _SourceGuid;
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
        /// Wireless bonus node from the XML file.
        /// </summary>
        public XmlNode WirelessBonus
        {
            get
            {
                return _nodWirelessBonus;
            }
            set
            {
                _nodWirelessBonus = value;
            }
        }

        /// <summary>
        /// WeaponBonus node from the XML file.
        /// </summary>
        public XmlNode WeaponBonus
        {
            get
            {
                return _nodWeaponBonus;
            }
            set
            {
                _nodWeaponBonus = value;
            }
        }

        /// <summary>
        /// Character to which the gear is assigned.
        /// </summary>
        public Character CharacterObject
        {
            get
            {
                return _objCharacter;
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
                    _objCachedMyXmlNode = null;
                _strName = value;
            }
        }

        /// <summary>
        /// A custom name for the Gear assigned by the player.
        /// </summary>
        public string GearName
        {
            get
            {
                return _strGearName;
            }
            set
            {
                _strGearName = value;
            }
        }

        /// <summary>
        /// Translated Category.
        /// </summary>
        public string DisplayCategory(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return Category;

            return XmlManager.Load("gear.xml", strLanguage)?.SelectSingleNode("/chummer/categories/category[. = \"" + Category + "\"]")?.Attributes?["translate"]?.InnerText ?? Category;
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
            get
            {
                return _strCapacity;
            }
            set
            {
                _strCapacity = value;
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
        /// Minimum Rating.
        /// </summary>
        public int MinRating
        {
            get
            {
                return _intMinRating;
            }
            set
            {
                _intMinRating = value;
            }
        }

        /// <summary>
        /// Maximum Rating.
        /// </summary>
        public int MaxRating
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
        /// Rating.
        /// </summary>
        public int Rating
        {
            get => Math.Max(Math.Min(_intRating, MaxRating), MinRating);
            set => _intRating = Math.Max(Math.Min(value, MaxRating), MinRating);
        }

        /// <summary>
        /// Quantity.
        /// </summary>
        public decimal Quantity
        {
            get
            {
                return _decQty;
            }
            set
            {
                _decQty = value;
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
        /// Use for ammo. The number of rounds that the nuyen amount buys.
        /// </summary>
        public decimal CostFor
        {
            get
            {
                return _decCostFor;
            }
            set
            {
                _decCostFor = value;
            }
        }

        /// <summary>
        /// Cost.
        /// </summary>
        public string Cost
        {
            get
            {
                if (_strCost.StartsWith("FixedValues"))
                {
                    string[] strValues = _strCost.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    string strCost = "0";
                    if (Rating > 0)
                        strCost = strValues[Math.Min(Rating, strValues.Length) - 1].Trim("[]".ToCharArray());
                    else
                        strCost = strValues[0].Trim("[]".ToCharArray());
                    return strCost;
                }
                else if (_strCost.StartsWith("Parent Cost"))
                {
                    string strCostExpression = _strCost;
                    string strCost = "0";

                    if (_objParent == null)
                    {
                        return strCost;
                    }
                    else
                    {
                        strCost = strCostExpression.Replace("Weapon Cost", _objParent.Cost);
                    }
                    return strCost;
                }
                else
                    return _strCost;
            }
            set
            {
                _strCost = value;
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
                _strExtra = LanguageManager.ReverseTranslateExtra(value, GlobalOptions.Language);
            }
        }

        /// <summary>
        /// Whether or not the Foci is bonded.
        /// </summary>
        public bool Bonded
        {
            get
            {
                return _blnBonded;
            }
            set
            {
                _blnBonded = value;
            }
        }

        /// <summary>
        /// Whether or not the Gear is equipped.
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
        /// Whether or not the Gear's wireless bonus is enabled.
        /// </summary>
        public bool WirelessOn
        {
            get
            {
                return _blnWirelessOn;
            }
            set
            {
                _blnWirelessOn = value;
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
                return _strPage;
            }
            set
            {
                _strPage = value;
            }
        }

        public string DisplayPage(string strLanguage)
        {
            if (strLanguage == GlobalOptions.DefaultLanguage)
                return _strPage;

            return GetNode(strLanguage)?["altpage"]?.InnerText ?? _strPage;
        }

        /// <summary>
        /// String to determine if gear can form persona or grants persona forming to its parent.
        /// </summary>
        public string CanFormPersona
        {
            get
            {
                return _strCanFormPersona;
            }
            set
            {
                _strCanFormPersona = value;
            }
        }

        public bool IsCommlink
        {
            get
            {
                return _strCanFormPersona.Contains("Self") || Children.Any(x => x.CanFormPersona.Contains("Parent"));
            }
        }

        /// <summary>
        /// Whether to disable the ability to get more of a particular gear.
        /// </summary>
        public bool DisableQuantity
        {
            get
            {
                return _blnDisableQuantity;
            }
            set
            {
                _blnDisableQuantity = value;
            }
        }

        /// <summary>
        /// A List of child pieces of Gear.
        /// </summary>
        public IList<Gear> Children
        {
            get
            {
                return _objChildren;
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
        /// Device Rating string.
        /// </summary>
        public string DeviceRating
        {
            get
            {
                return _strDeviceRating;
            }
            set
            {
                _strDeviceRating = value;
            }
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
                        if (IsCommlink)
                            return 2;
                        else
                            return 0;
                    case "Program Limit":
                        if (IsCommlink)
                        {
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                return 2;
                        }
                        else
                            return 0;
                        break;
                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            return 0;
                        break;
                    case "Attack":
                    case "Sleaze":
                    default:
                        return 0;
                }
            }

            if (strExpression.StartsWith("FixedValues"))
            {
                string[] strValues = strExpression.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                if (Rating > 0)
                    strExpression = strValues[Math.Min(Rating, strValues.Length) - 1].Trim("[]".ToCharArray());
            }
            if (strExpression.Contains('{') || strExpression.Contains('+') || strExpression.Contains('-') || strExpression.Contains('*') || strExpression.Contains("div"))
            {
                StringBuilder objValue = new StringBuilder(strExpression);
                objValue.Replace("{Rating}", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                foreach (string strMatrixAttribute in MatrixAttributes.MatrixAttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{Gear " + strMatrixAttribute + "}", () => (Parent?.GetBaseMatrixAttribute(strMatrixAttribute) ?? 0).ToString(GlobalOptions.InvariantCultureInfo));
                    objValue.CheapReplace(strExpression, "{Parent " + strMatrixAttribute + "}", () => (Parent?.GetMatrixAttributeString(strMatrixAttribute) ?? "0"));
                    if (Children.Count > 0 && strExpression.Contains("{Children " + strMatrixAttribute + "}"))
                    {
                        int intTotalChildrenValue = 0;
                        foreach (Gear loopGear in Children)
                        {
                            if (loopGear.Equipped)
                            {
                                intTotalChildrenValue += loopGear.GetBaseMatrixAttribute(strMatrixAttribute);
                            }
                        }
                        objValue.Replace("{Children " + strMatrixAttribute + "}", intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    }
                }
                foreach (string strCharAttributeName in Attributes.AttributeSection.AttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "}", () => CharacterObject.GetAttribute(strCharAttributeName).TotalValue.ToString());
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "Base}", () => CharacterObject.GetAttribute(strCharAttributeName).TotalBase.ToString());
                }
                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                return Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(objValue.ToString())));
            }
            int.TryParse(strExpression, out int intReturn);
            return intReturn;
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
        public string Location
        {
            get
            {
                return _strLocation;
            }
            set
            {
                _strLocation = value;
            }
        }

        /// <summary>
        /// Whether or not the Gear qualifies as a Program in the printout XML.
        /// </summary>
        public bool IsProgram
        {
            get
            {
                if (_strCategory == "ARE Programs" || _strCategory.StartsWith("Autosofts") || _strCategory == "Data Software" || _strCategory == "Malware" || _strCategory == "Matrix Programs" || _strCategory == "Tactical AR Software" || _strCategory == "Telematics Infrastructure Software" || _strCategory == "Sensor Software")
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Whether or not the Gear has the Ergonomic Program Option.
        /// </summary>
        public bool IsErgonomic
        {
            get
            {
                foreach (Gear objPlugin in _objChildren)
                {
                    if (objPlugin.Name == "Ergonomic")
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Cost multiplier for Children attached to this Gear.
        /// </summary>
        public int ChildCostMultiplier
        {
            get
            {
                return _intChildCostMultiplier;
            }
            set
            {
                _intChildCostMultiplier = value;
            }
        }

        /// <summary>
        /// Avail modifier for Children attached to this Gear.
        /// </summary>
        public int ChildAvailModifier
        {
            get
            {
                return _intChildAvailModifier;
            }
            set
            {
                _intChildAvailModifier = value;
            }
        }

        /// <summary>
        /// Parent Gear.
        /// </summary>
        public Gear Parent
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
        /// Whether or not the Gear's cost should be discounted by 10% through the Black Market Pipeline Quality.
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
        /// Attack.
        /// </summary>
        public string Attack
        {
            get
            {
                return _strAttack;
            }
            set
            {
                _strAttack = value;
            }
        }

        /// <summary>
        /// Sleaze.
        /// </summary>
        public string Sleaze
        {
            get
            {
                return _strSleaze;
            }
            set
            {
                _strSleaze = value;
            }
        }

        /// <summary>
        /// Data Processing.
        /// </summary>
        public string DataProcessing
        {
            get
            {
                return _strDataProcessing;
            }
            set
            {
                _strDataProcessing = value;
            }
        }

        /// <summary>
        /// Firewall.
        /// </summary>
        public string Firewall
        {
            get
            {
                return _strFirewall;
            }
            set
            {
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
                return _strModAttack;
            }
            set
            {
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
                return _strModSleaze;
            }
            set
            {
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
                return _strModDataProcessing;
            }
            set
            {
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
                return _strModFirewall;
            }
            set
            {
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
                return _strAttributeArray;
            }
            set
            {
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
                return _strModAttributeArray;
            }
            set
            {
                _strModAttributeArray = value;
            }
        }

        public IList<IHasMatrixAttributes> ChildrenWithMatrixAttributes
        {
            get
            {
                return Children.Cast<IHasMatrixAttributes>().ToList();
            }
        }

        /// <summary>
        /// Commlink's Limit for how many Programs they can run.
        /// </summary>
        public string ProgramLimit
        {
            get
            {
                return _strProgramLimit;
            }
            set
            {
                _strProgramLimit = value;
            }
        }

        /// <summary>
        /// ASDF attribute boosted by Overclocker.
        /// </summary>
        public string Overclocked
        {
            get
            {
                return _strOverclocked;
            }
            set
            {
                _strOverclocked = value;
            }
        }

        /// <summary>
        /// Returns true if this is a cyberdeck whose attributes we could swap around.
        /// </summary>
        public bool CanSwapAttributes
        {
            get
            {
                return _blnCanSwapAttributes;
            }
            set
            {
                _blnCanSwapAttributes = value;
            }
        }

        /// <summary>
        /// Whether or not the Gear is included in its parent item when purchased (currently applies to Armor only).
        /// </summary>
        public bool IncludedInParent
        {
            get
            {
                return !string.IsNullOrEmpty(_strParentID);
            }
        }

        /// <summary>
        /// ID of the object that added this cyberware (if any).
        /// </summary>
        public string ParentID
        {
            get
            {
                return _strParentID;
            }
            set
            {
                _strParentID = value;
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
                XmlDocument objDoc = XmlManager.Load("gear.xml", strLanguage);
                if (objDoc != null)
                {
                    _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/gears/gear[(id = \"" + _SourceGuid + "\") or (name = \"" + Name + "\" and category = \"" + Category + "\")]");
                    if (_objCachedMyXmlNode == null)
                    {
                        _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/gears/gear[(name = \"" + Name + "\")]");
                        if (_objCachedMyXmlNode == null)
                        {
                            _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/gears/gear[contains(name, \"" + Name + "\")]");
                        }
                    }
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
        public string TotalAvail(CultureInfo objCulture, string strLanguage, bool blnCalculateAdditions = false)
        {
            if (string.IsNullOrEmpty(_strAvail))
                _strAvail = "0";

            bool blnIncludePlus = false;

            // If the Avail contains "+", return the base string and don't try to calculate anything since we're looking at a child component.
            if (_strAvail.StartsWith('+'))
            {
                if (!blnCalculateAdditions)
                    return _strAvail;
                blnIncludePlus = true;
            }

            string strCalculated;

            string strAvailExpression = _strAvail;
            
            if (strAvailExpression.Contains("Rating"))
            {
                // If the availability is determined by the Rating, evaluate the expression.
                string strAvail = string.Empty;
                if (blnIncludePlus)
                    strAvailExpression = strAvailExpression.Substring(1, strAvailExpression.Length - 1);

                if (strAvailExpression.Substring(strAvailExpression.Length - 1, 1) == "F" || strAvailExpression.Substring(strAvailExpression.Length - 1, 1) == "R")
                {
                    strAvail = strAvailExpression.Substring(strAvailExpression.Length - 1, 1);
                    // Remove the trailing character if it is "F" or "R".
                    strAvailExpression = strAvailExpression.Substring(0, strAvailExpression.Length - 1);
                }
                strCalculated = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpression.Replace("Rating", Rating.ToString(CultureInfo.InvariantCulture)))).ToString() + strAvail;
            }
            else
            {
                // Just a straight cost, so return the value.
                strCalculated = strAvailExpression.EndsWith('F') || strAvailExpression.EndsWith('R')
                    ? Convert.ToInt32(strAvailExpression.Substring(0, strAvailExpression.Length - 1)).ToString() + strAvailExpression.Substring(strAvailExpression.Length - 1, 1)
                    : Convert.ToInt32(strAvailExpression).ToString();
            }

            int intAvail;
            string strAvailText = string.Empty;
            if (strCalculated.EndsWith('F') || strCalculated.EndsWith('R'))
            {
                strAvailText = strCalculated.Substring(strCalculated.Length - 1);
                intAvail = Convert.ToInt32(strCalculated.Substring(0, strCalculated.Length - 1));
            }
            else
                intAvail = Convert.ToInt32(strCalculated);

            // Run through the child items and increase the Avail by any Mod whose Avail contains "+".
            foreach (Gear objChild in _objChildren)
            {
                if (objChild.Avail.StartsWith('+'))
                {
                    string strAvail = objChild.Avail.Replace("Rating", objChild.Rating.ToString());
                    strAvail = strAvail.Substring(1).Trim();
                    if (strAvail.EndsWith('R') || strAvail.EndsWith('F'))
                    {
                        if (strAvailText != "F")
                            strAvailText = strAvail.Substring(strAvail.Length - 1);
                        intAvail += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvail.Substring(0, strAvail.Length - 1)));
                    }
                    else
                    {
                        intAvail += Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvail));
                    }
                }
            }

            // Translate the Avail string.
            if (strLanguage != GlobalOptions.DefaultLanguage)
            {
                if (strAvailText == "F")
                    strAvailText = LanguageManager.GetString("String_AvailForbidden", strLanguage);
                else if (strAvailText == "R")
                    strAvailText = LanguageManager.GetString("String_AvailRestricted", strLanguage);
            }

            // Add any Avail modifier that comes from its Parent.
            if (_objParent != null)
                intAvail += _objParent.ChildAvailModifier;

            string strReturn = intAvail.ToString(objCulture) + strAvailText;

            if (blnIncludePlus)
                strReturn = "+" + strReturn;

            return strReturn;
        }

        /// <summary>
        /// Caculated Capacity of the Gear.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strReturn = _strCapacity;
                string strSecondHalf = string.Empty;
                if (strReturn.Contains("/["))
                {
                    int intPos = strReturn.IndexOf("/[");
                    string strFirstHalf = strReturn.Substring(0, intPos);
                    strSecondHalf = strReturn.Substring(intPos + 1, strReturn.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.Contains('[');
                    string strCapacity = strFirstHalf;
                    if (blnSquareBrackets && strCapacity.Length > 2)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                    if (_strArmorCapacity == "[*]")
                        strReturn = "*";
                    else if (_strArmorCapacity.StartsWith("FixedValues"))
                    {
                        string[] strValues = _strArmorCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                        strReturn = strValues[Math.Min(Rating, strValues.Length) - 1];
                    }
                    else
                        strReturn = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString()))).ToString("#,0.##", GlobalOptions.CultureInfo);
                }
                if (strReturn.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strReturn.Contains('[');
                    string strCapacity = strReturn;
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                    // This has resulted in a non-whole number, so round it (minimum of 1).
                    double dblNumber = (double)CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString()));
                    if (dblNumber < 1)
                        dblNumber = 1;
                    strReturn = dblNumber.ToString("#,0.##", GlobalOptions.CultureInfo);

                    if (blnSquareBrackets)
                        strReturn = "[" + strReturn + "]";
                }
                else if (string.IsNullOrEmpty(strReturn))
                    return "0";
                if (!string.IsNullOrEmpty(strSecondHalf))
                    strReturn += "/" + strSecondHalf;
                if (decimal.TryParse(strReturn, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                    return decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);
                // Just a straight Capacity, so return the value.
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
                if (_strArmorCapacity.Contains("/["))
                {
                    int intPos = _strArmorCapacity.IndexOf("/[");
                    string strFirstHalf = _strArmorCapacity.Substring(0, intPos);
                    string strSecondHalf = _strArmorCapacity.Substring(intPos + 1, _strArmorCapacity.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.Contains('[');
                    string strCapacity = strFirstHalf;
                    if (blnSquareBrackets && strCapacity.Length > 2)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                    string strReturn;
                    if (_strArmorCapacity == "[*]")
                        strReturn = "*";
                    else if (_strArmorCapacity.StartsWith("FixedValues"))
                    {
                        string[] strValues = _strArmorCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                        strReturn = strValues[Math.Min(Rating, strValues.Length) - 1];
                    }
                    else
                        strReturn = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString()))).ToString("#,0.##", GlobalOptions.CultureInfo);
                    if (blnSquareBrackets)
                        strReturn = "[" + strCapacity + "]";
                    strReturn += "/" + strSecondHalf;
                    return strReturn;
                }
                else if (_strArmorCapacity.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = _strArmorCapacity.Contains('[');
                    string strCapacity = _strArmorCapacity;
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                    string strReturn = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString()))).ToString("#,0.##", GlobalOptions.CultureInfo);
                    if (blnSquareBrackets)
                        strReturn = "[" + strReturn + "]";

                    return strReturn;
                }
                // Just a straight Capacity, so return the value.
                else if (string.IsNullOrEmpty(_strArmorCapacity))
                    return "0";
                else
                {
                    if (decimal.TryParse(_strArmorCapacity, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                        return decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);
                    return _strArmorCapacity;
                }
            }
        }

        /// <summary>
        /// Total cost of the just the Gear itself before we factor in any multipliers.
        /// </summary>
        public decimal OwnCostPreMultipliers
        {
            get
            {
                string strCostExpression = _strCost;

                if (strCostExpression.StartsWith("FixedValues"))
                {
                    string[] strValues = strCostExpression.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Rating > 0)
                        strCostExpression = strValues[Math.Min(Rating, strValues.Length) - 1].Trim("[]".ToCharArray());
                }

                decimal decGearCost = 0;
                string strParentCost = string.Empty;
                if (_objParent != null)
                {
                    if (strCostExpression.Contains("Gear Cost"))
                        decGearCost = _objParent.CalculatedCost;
                    if (strCostExpression.Contains("Parent Cost"))
                        strParentCost = _objParent.Cost;
                }
                decimal decTotalChildrenCost = 0;
                if (_objChildren.Count > 0 && strCostExpression.Contains("Children Cost"))
                {
                    object decTotalChildrenCostLock = new object();
                    Parallel.ForEach(_objChildren, loopGear =>
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
                objCost.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                objCost.Replace("Parent Cost", string.IsNullOrEmpty(strParentCost) ? "0" : strParentCost);

                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                decimal decReturn = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(objCost.ToString()), GlobalOptions.InvariantCultureInfo);
                return decReturn;
            }
        }

        /// <summary>
        /// Total cost of the just the Gear itself.
        /// </summary>
        public decimal CalculatedCost
        {
            get
            {
                // The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
                return (OwnCostPreMultipliers * Quantity) / CostFor;
            }
        }

        /// <summary>
        /// Total cost of the Gear and its accessories.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decReturn = OwnCostPreMultipliers;

                if (DiscountCost)
                    decReturn *= 0.9m;

                decimal decPlugin = 0;
                if (_objChildren.Count > 0)
                {
                    // Add in the cost of all child components.
                    object decPluginLock = new object();
                    Parallel.ForEach(_objChildren, objChild =>
                    {
                        decimal decLoop = objChild.TotalCost;
                        lock (decPluginLock)
                            decPlugin += decLoop;
                    });
                }

                // The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
                int intParentMultiplier = 1;
                if (_objParent != null)
                    intParentMultiplier = _objParent.ChildCostMultiplier;

                decReturn = (decReturn * _decQty * intParentMultiplier) / CostFor;
                // Add in the cost of the plugins separate since their value is not based on the Cost For number (it is always cost x qty).
                decReturn += decPlugin * _decQty;

                return decReturn;
            }
        }

        /// <summary>
        /// The cost of just the Gear itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                decimal decReturn = OwnCostPreMultipliers;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // The number is divided at the end for ammo purposes. This is done since the cost is per "costfor" but is being multiplied by the actual number of rounds.
                int intParentMultiplier = 1;
                if (_objParent != null)
                    intParentMultiplier = _objParent.ChildCostMultiplier;

                decReturn = (decReturn * intParentMultiplier) / CostFor;

                return decReturn;
            }
        }

        /// <summary>
        /// The Gear's Capacity cost if used as a plugin.
        /// </summary>
        public decimal PluginCapacity
        {
            get
            {
                string strCapacity = CalculatedCapacity;
                if (strCapacity.Contains("/["))
                {
                    // If this is a multiple-capacity item, use only the second half.
                    int intPos = strCapacity.IndexOf("/[");
                    strCapacity = strCapacity.Substring(intPos + 1);
                }

                // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                if (strCapacity.Contains('['))
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                else
                    strCapacity = "0";
                return Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
            }
        }

        /// <summary>
        /// The Gear's Capacity cost if used as an Armor plugin.
        /// </summary>
        public int PluginArmorCapacity
        {
            get
            {
                string strCapacity = CalculatedArmorCapacity;
                if (strCapacity.Contains("/["))
                {
                    // If this is a multiple-capacity item, use only the second half.
                    int intPos = strCapacity.IndexOf("/[");
                    strCapacity = strCapacity.Substring(intPos + 1);
                }

                // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                if (strCapacity.Contains('['))
                    strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                else
                    strCapacity = "0";
                return Convert.ToInt32(strCapacity);
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
                if (!strMyCapacity.Contains('[') || strMyCapacity.Contains("/["))
                {
                    // Get the Gear base Capacity.
                    if (strMyCapacity.Contains("/["))
                    {
                        // If this is a multiple-capacity item, use only the first half.
                        int intPos = strMyCapacity.IndexOf("/[");
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
                            string strCapacity = objChildGear.CalculatedCapacity;
                            if (strCapacity.Contains("/["))
                            {
                                // If this is a multiple-capacity item, use only the second half.
                                int intPos = strCapacity.IndexOf("/[");
                                strCapacity = strCapacity.Substring(intPos + 1);
                            }

                            // Only items that contain square brackets should consume Capacity. Everything else is treated as [0].
                            if (strCapacity.Contains('['))
                                strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                            else
                                strCapacity = "0";
                            decimal decLoop = (Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo) * objChildGear.Quantity);
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

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Qty Name (Rating) (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            if (_decQty != 1.0m || Category == "Currency")
                strReturn = _decQty.ToString(Name.StartsWith("Nuyen") ? _objCharacter.Options.NuyenFormat : Category == "Currency" ? "#,0.00" : "#,0.##", GlobalOptions.CultureInfo) + " " + strReturn;
            if (Rating > 0)
                strReturn += " (" + LanguageManager.GetString("String_Rating", strLanguage) + " " + Rating + ")";
            if (!string.IsNullOrEmpty(_strExtra))
                strReturn += " (" + LanguageManager.TranslateExtra(_strExtra, strLanguage) + ")";

            if (!string.IsNullOrEmpty(_strGearName))
            {
                strReturn += " (\"" + _strGearName + "\")";
            }

            return strReturn;
        }

        /// <summary>
        /// Weapon Bonus Damage.
        /// </summary>
        public string WeaponBonusDamage(string strLanguage)
        {
            if (_nodWeaponBonus == null)
                return string.Empty;
            else
            {
                string strReturn = "0";
                // Use the damagereplace value if applicable.
                if (_nodWeaponBonus["damagereplace"] != null)
                    strReturn = _nodWeaponBonus["damagereplace"].InnerText;
                else
                {
                    // Use the damage bonus if available, otherwise use 0.
                    if (_nodWeaponBonus["damage"] != null)
                        strReturn = _nodWeaponBonus["damage"].InnerText;

                    // Attach the type if applicable.
                    if (_nodWeaponBonus["damagetype"] != null)
                        strReturn += _nodWeaponBonus["damagetype"].InnerText;

                    // If this does not start with "-", add a "+" to the string.
                    if (!strReturn.StartsWith('-'))
                        strReturn = "+" + strReturn;
                }

                // Translate the Avail string.
                if (strLanguage != GlobalOptions.DefaultLanguage)
                {
                    strReturn = strReturn.CheapReplace("P", () => LanguageManager.GetString("String_DamagePhysical", strLanguage));
                    strReturn = strReturn.CheapReplace("S", () => LanguageManager.GetString("String_DamageStun", strLanguage));
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
                    string strReturn = "0";
                    // Use the apreplace value if applicable.
                    if (_nodWeaponBonus["apreplace"] != null)
                        strReturn = _nodWeaponBonus["apreplace"].InnerText;
                    // Use the ap bonus if available, otherwise use 0.
                    else if (_nodWeaponBonus["ap"] != null)
                    {
                        strReturn = _nodWeaponBonus["ap"].InnerText;

                        // If this does not start with "-", add a "+" to the string.
                        if (!strReturn.StartsWith('-'))
                            strReturn = "+" + strReturn;
                    }

                    return strReturn;
                }
            }
        }

        /// <summary>
        /// Weapon Bonus Range.
        /// </summary>
        public int WeaponBonusRange
        {
            get
            {
                if (_nodWeaponBonus?["rangebonus"] != null)
                    return Convert.ToInt32(_nodWeaponBonus["rangebonus"].InnerText);
                else
                    return 0;
            }
        }


        /// <summary>
        /// Base Matrix Boxes.
        /// </summary>
        public int BaseMatrixBoxes
        {
            get
            {
                return 8;
            }
        }

        /// <summary>
        /// Bonus Matrix Boxes.
        /// </summary>
        public int BonusMatrixBoxes
        {
            get
            {
                return _intMatrixCMBonus;
            }
            set
            {
                _intMatrixCMBonus = value;
            }
        }

        /// <summary>
        /// Total Bonus Matrix Boxes (including all children).
        /// </summary>
        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intReturn = BonusMatrixBoxes;
                foreach (Gear objGear in Children)
                {
                    if (objGear.Equipped)
                    {
                        intReturn += objGear.TotalBonusMatrixBoxes;
                    }
                }
                return intReturn;
            }
        }

        /// <summary>
        /// Matrix Condition Monitor boxes.
        /// </summary>
        public int MatrixCM
        {
            get
            {
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
                return _intMatrixCMFilled;
            }
            set
            {
                _intMatrixCMFilled = value;
            }
        }
        #endregion

        #region Methods
        public bool IsIdenticalToOtherGear(Gear objOtherGear)
        {
            if (Name == objOtherGear.Name && Category == objOtherGear.Category && Rating == objOtherGear.Rating && Extra == objOtherGear.Extra && GearName == objOtherGear.GearName && Notes == objOtherGear.Notes)
            {
                if (Children.DeepMatch(objOtherGear.Children, x => x.Children, (x, y) => x.IsIdenticalToOtherGear(y)))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}
