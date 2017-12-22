using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Equipment
{
    /// <summary>
    /// A piece of Cyberware.
    /// </summary>
    public class Cyberware : IHasChildren<Cyberware>, INamedItem, IItemWithGuid, IItemWithNode, IHasMatrixAttributes
    {
        private Guid _sourceID = Guid.Empty;
        private Guid _guiID = Guid.Empty;
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strLimbSlot = string.Empty;
        private string _strLimbSlotCount = "1";
        private bool _blnInheritAttributes = false;
        private string _strESS = string.Empty;
        private decimal _decExtraESSAdditiveMultiplier = 0.0m;
        private decimal _decExtraESSMultiplicativeMultiplier = 1.0m;
        private string _strCapacity = string.Empty;
        private string _strAvail = string.Empty;
        private string _strCost = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private int _intMatrixCMFilled = 0;
        private int _intRating = 0;
        private string _strMinRating = string.Empty;
        private string _strMaxRating = string.Empty;
        private string _strAllowSubsystems = string.Empty;
        private bool _blnSuite = false;
        private string _strLocation = string.Empty;
        private string _strExtra = string.Empty;
        private Guid _guiWeaponID = Guid.Empty;
        private Guid _guiVehicleID = Guid.Empty;
        private Grade _objGrade = new Grade();
        private BindingList<Cyberware> _objChildren = new BindingList<Cyberware>();
        private List<Gear> _lstGear = new List<Gear>();
        private XmlNode _nodBonus;
        private XmlNode _nodPairBonus;
        private XmlNode _nodWirelessBonus;
        private bool _blnWirelessOn = true;
        private XmlNode _nodAllowGear;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Cyberware;
        private string _strNotes = string.Empty;
        private int _intEssenceDiscount = 0;
        private string _strAltName = string.Empty;
        private string _strAltCategory = string.Empty;
        private string _strAltPage = string.Empty;
        private string _strForceGrade = string.Empty;
        private bool _blnDiscountCost = false;
        private Vehicle _objParentVehicle = null;
        private bool _blnPrototypeTranshuman;
        private Cyberware _objParent;
        private bool _blnAddToParentESS = false;
        private string _strParentID = string.Empty;
        private string _strHasModularMount = string.Empty;
        private string _strPlugsIntoModularMount = string.Empty;
        private string _strBlocksMounts = string.Empty;
        private string _strForced = string.Empty;

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
        private bool _blnCanSwapAttributes = false;

        private readonly Character _objCharacter;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a Grade.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static Grade ConvertToCyberwareGrade(string strValue, Improvement.ImprovementSource objSource, CharacterOptions objCharacterOptions)
        {
            IList<Grade> lstGrades = CommonFunctions.GetGradeList(objSource, objCharacterOptions);
            foreach (Grade objGrade in lstGrades)
            {
                if (objGrade.Name == strValue)
                    return objGrade;
            }

            return lstGrades.FirstOrDefault(x => x.Name == "Standard");
        }
        #endregion

        #region Constructor, Create, Save, Load, and Print Methods
        public Cyberware(Character objCharacter)
        {
            // Create the GUID for the new piece of Cyberware.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create a Cyberware from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlCyberware">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character object the Cyberware will be added to.</param>
        /// <param name="objGrade">Grade of the selected piece.</param>
        /// <param name="objSource">Source of the piece.</param>
        /// <param name="intRating">Selected Rating of the piece of Cyberware.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="objWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="objWeaponNodes">List of TreeNode to represent the Weapons added.</param>
        /// <param name="blnCreateImprovements">Whether or not Improvements should be created.</param>
        /// <param name="blnCreateChildren">Whether or not child items should be created.</param>
        /// <param name="strForced">Force a particular value to be selected by an Improvement prompts.</param>
        public void Create(XmlNode objXmlCyberware, Character objCharacter, Grade objGrade, Improvement.ImprovementSource objSource, int intRating, TreeNode objNode, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, List<Vehicle> objVehicles, List<TreeNode> objVehicleNodes, bool blnCreateImprovements = true, bool blnCreateChildren = true, string strForced = "", Cyberware objParent = null, Vehicle objParentVehicle = null)
        {
            Parent = objParent;
            _strForced = strForced;
            _objParentVehicle = objParentVehicle;
            objXmlCyberware.TryGetStringFieldQuickly("name", ref _strName);
            objXmlCyberware.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlCyberware.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
            objXmlCyberware.TryGetStringFieldQuickly("limbslotcount", ref _strLimbSlotCount);
            objXmlCyberware.TryGetStringFieldQuickly("notes", ref _strNotes);
            _blnInheritAttributes = objXmlCyberware["inheritattributes"] != null;
            _objGrade = objGrade;
            objXmlCyberware.TryGetStringFieldQuickly("ess", ref _strESS);
            objXmlCyberware.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objXmlCyberware.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlCyberware.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlCyberware.TryGetStringFieldQuickly("page", ref _strPage);
            _blnAddToParentESS = objXmlCyberware["addtoparentess"] != null;
            _nodBonus = objXmlCyberware["bonus"];
            _nodPairBonus = objXmlCyberware["pairbonus"];
            _nodWirelessBonus = objXmlCyberware["wirelessbonus"];
            _blnWirelessOn = _nodWirelessBonus != null;
            _nodAllowGear = objXmlCyberware["allowgear"];
            if (objXmlCyberware.TryGetField("id", Guid.TryParse, out _sourceID))
                _objCachedMyXmlNode = null;
            objXmlCyberware.TryGetStringFieldQuickly("mountsto", ref _strPlugsIntoModularMount);
            objXmlCyberware.TryGetStringFieldQuickly("modularmount", ref _strHasModularMount);
            objXmlCyberware.TryGetStringFieldQuickly("blocksmounts", ref _strBlocksMounts);

            _objImprovementSource = objSource;
            _objCachedMyXmlNode = null;
            objXmlCyberware.TryGetStringFieldQuickly("rating", ref _strMaxRating);
            objXmlCyberware.TryGetStringFieldQuickly("minrating", ref _strMinRating);

            _intRating = Math.Min(Math.Max(intRating, MinRating), MaxRating);

            objXmlCyberware.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objXmlCyberware.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
            {
                objXmlCyberware.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlCyberware.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlCyberware.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlCyberware.TryGetStringFieldQuickly("firewall", ref _strFirewall);
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
            objXmlCyberware.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            objXmlCyberware.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            objXmlCyberware.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            objXmlCyberware.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            objXmlCyberware.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            objXmlCyberware.TryGetStringFieldQuickly("programs", ref _strProgramLimit);

            objXmlCyberware.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);

            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
            {
                
                XmlNode objCyberwareNode = MyXmlNode;
                if (objCyberwareNode != null)
                {
                    objCyberwareNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objCyberwareNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                string strXmlFile = "cyberware.xml";
                if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    strXmlFile = "bioware.xml";
                }
                XmlDocument objXmlDocument = XmlManager.Load(strXmlFile);
                objCyberwareNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                _strAltCategory = objCyberwareNode?.Attributes?["translate"]?.InnerText;
            }

            // Add Subsytem information if applicable.
            if (objXmlCyberware.InnerXml.Contains("allowsubsystems"))
            {
                string strSubsystem = string.Empty;
                XmlNodeList lstSubSystems = objXmlCyberware.SelectNodes("allowsubsystems/category");
                for (int i = 0; i < lstSubSystems.Count; i++)
                {
                    strSubsystem += lstSubSystems[i].InnerText;
                    if (i != lstSubSystems.Count - 1)
                    {
                        strSubsystem += ",";
                    }
                }
                _strAllowSubsystems = strSubsystem;
            }

            // Check for a Variable Cost.
            if (objXmlCyberware["cost"] != null)
            {
                if (objXmlCyberware["cost"].InnerText.StartsWith("Variable"))
                {
                    decimal decMin = 0.0m;
                    decimal decMax = decimal.MaxValue;
                    char[] charParentheses = { '(', ')' };
                    string strCost = objXmlCyberware["cost"].InnerText.TrimStart("Variable", true).Trim(charParentheses);
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
                        frmPickNumber.Description = LanguageManager.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
                        frmPickNumber.AllowCancel = false;
                        frmPickNumber.ShowDialog();
                        _strCost = frmPickNumber.SelectedValue.ToString(GlobalOptions.InvariantCultureInfo);
                    }
                }
                else
                {
                    _strCost = objXmlCyberware["cost"].InnerText;
                }
            }

            // Add Cyberweapons if applicable.
            if (objXmlCyberware.InnerXml.Contains("<addweapon>"))
            {
                XmlDocument objXmlWeaponDocument = XmlManager.Load("weapons.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddWeapon in objXmlCyberware.SelectNodes("addweapon"))
                {
                    string strLoopID = objXmlAddWeapon.InnerText;
                    XmlNode objXmlWeapon = strLoopID.IsGuid()
                        ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + strLoopID + "\"]")
                        : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + strLoopID + "\"]");

                    List<TreeNode> lstGearWeaponNodes = new List<TreeNode>();
                    Weapon objGearWeapon = new Weapon(objCharacter);
                    objGearWeapon.ParentVehicle = ParentVehicle;
                    objGearWeapon.Create(objXmlWeapon, lstGearWeaponNodes, null, null, objWeapons);
                    objGearWeapon.ParentID = InternalId;
                    foreach (TreeNode objLoopNode in lstGearWeaponNodes)
                    {
                        objLoopNode.ForeColor = SystemColors.GrayText;
                        objWeaponNodes.Add(objLoopNode);
                    }
                    objWeapons.Add(objGearWeapon);

                    _guiWeaponID = Guid.Parse(objGearWeapon.InternalId);
                }
            }

            // Add Drone Bodyparts if applicable.
            if (objXmlCyberware.InnerXml.Contains("<addvehicle>"))
            {
                XmlDocument objXmlVehicleDocument = XmlManager.Load("vehicles.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddVehicle in objXmlCyberware.SelectNodes("addvehicle"))
                {
                    string strLoopID = objXmlAddVehicle.InnerText;
                    var objXmlVehicle = strLoopID.IsGuid()
                        ? objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[id = \"" + strLoopID + "\"]")
                        : objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + strLoopID + "\"]");

                    TreeNode objVehicleNode = new TreeNode();
                    Vehicle objVehicle = new Vehicle(_objCharacter);
                    objVehicle.Create(objXmlVehicle, objVehicleNode, null, null, null, null);
                    objVehicle.ParentID = InternalId;
                    objVehicleNode.ForeColor = SystemColors.GrayText;
                    objVehicleNodes.Add(objVehicleNode);
                    objVehicles.Add(objVehicle);

                    _guiVehicleID = Guid.Parse(objVehicle.InternalId);
                }
            }

            /*
             * This needs to be handled separately from usual bonus nodes because:
             * - Children must always inherit the side of their parent(s)
             * - In case of numerical limits, we must be able to apply them separately to each side
             * - Modular cyberlimbs need a constant side regardless of their equip status
             * - In cases where modular mounts might get blocked, we must force the 'ware to the unblocked side
             */
            if (objXmlCyberware["selectside"] != null)
            {
                string strParentSide = Parent?.Location;
                if (!string.IsNullOrEmpty(strParentSide))
                {
                    _strLocation = strParentSide;
                }
                else
                {
                    frmSelectSide frmPickSide = new frmSelectSide
                    {
                        Description = LanguageManager.GetString("Label_SelectSide").Replace("{0}", DisplayNameShort)
                    };
                    string strForcedSide = string.Empty;
                    if (_strForced == "Right" || _strForced == "Left")
                        strForcedSide = _strForced;
                    // TODO: Fix for modular mounts / banned mounts if someone has an amount of limbs different from the default amount
                    if (string.IsNullOrEmpty(strForcedSide) && ParentVehicle == null)
                    {
                        IList<Cyberware> lstCyberwareToCheck = Parent == null ? _objCharacter.Cyberware : Parent.Children;
                        if (!SelectionShared.RequirementsMet(objXmlCyberware, false, _objCharacter, null, null, null, string.Empty, string.Empty, string.Empty, "Left") ||
                            (!string.IsNullOrEmpty(BlocksMounts) && lstCyberwareToCheck.Any(x => !string.IsNullOrEmpty(x.HasModularMount) && x.Location == "Left" && BlocksMounts.Split(',').Contains(x.HasModularMount))) ||
                            (!string.IsNullOrEmpty(HasModularMount) && lstCyberwareToCheck.Any(x => !string.IsNullOrEmpty(x.BlocksMounts) && x.Location == "Left" && x.BlocksMounts.Split(',').Contains(HasModularMount))))
                            strForcedSide = "Right";
                        else if (!SelectionShared.RequirementsMet(objXmlCyberware, false, _objCharacter, null, null, null, string.Empty, string.Empty, string.Empty, "Right") ||
                            (!string.IsNullOrEmpty(BlocksMounts) && lstCyberwareToCheck.Any(x => !string.IsNullOrEmpty(x.HasModularMount) && x.Location == "Right" && BlocksMounts.Split(',').Contains(x.HasModularMount))) ||
                            (!string.IsNullOrEmpty(HasModularMount) && lstCyberwareToCheck.Any(x => !string.IsNullOrEmpty(x.BlocksMounts) && x.Location == "Right" && x.BlocksMounts.Split(',').Contains(HasModularMount))))
                            strForcedSide = "Left";
                    }
                    if (!string.IsNullOrEmpty(strForcedSide))
                        frmPickSide.ForceValue(strForcedSide);
                    else
                        frmPickSide.ShowDialog();

                    // Make sure the dialogue window was not canceled.
                    if (frmPickSide.DialogResult == DialogResult.Cancel)
                    {
                        _guiID = Guid.Empty;
                        return;
                    }

                    _strLocation = frmPickSide.SelectedSide;
                }
            }

            // If the piece grants a bonus, pass the information to the Improvement Manager.
            // Modular cyberlimbs only get their bonuses applied when they are equipped onto a limb, so we're skipping those here
            if (blnCreateImprovements && string.IsNullOrEmpty(_strPlugsIntoModularMount))
            {
                if (Bonus != null || WirelessBonus != null || PairBonus != null)
                {
                    if (!string.IsNullOrEmpty(_strForced) && _strForced != "Left" && _strForced != "Right")
                        ImprovementManager.ForcedValue = _strForced;

                    if (Bonus != null && !ImprovementManager.CreateImprovements(objCharacter, objSource, _guiID.ToString(), Bonus, false, Rating, DisplayNameShort))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (WirelessBonus != null && WirelessOn && !ImprovementManager.CreateImprovements(objCharacter, objSource, _guiID.ToString(), WirelessBonus, false, Rating, DisplayNameShort))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (PairBonus != null)
                    {
                        List<Cyberware> lstPairableCyberwares = objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Name == Name && x.Extra == Extra && x.IsModularCurrentlyEquipped).ToList();
                        int intCount = lstPairableCyberwares.Count;
                        if (!string.IsNullOrEmpty(Location))
                        {
                            intCount = Math.Min(lstPairableCyberwares.Count(x => x.Location == Location), lstPairableCyberwares.Count(x => x.Location != Location) - 1);
                        }
                        if (intCount > 0 && intCount % 2 == 1 && !ImprovementManager.CreateImprovements(objCharacter, objSource, _guiID.ToString(), PairBonus, false, Rating, DisplayNameShort))
                        {
                            _guiID = Guid.Empty;
                            return;
                        }
                    }
                }
            }

            // Create the TreeNode for the new item.
            objNode.Text = DisplayName;
            objNode.Tag = _guiID.ToString();

            // Retrieve the Bioware or Cyberware ESS Cost Multiplier. Bioware Modifiers do not apply to Genetech.
            if (MyXmlNode?["forcegrade"]?.InnerText != "None")
            {
                // Apply the character's Cyberware Essence cost multiplier if applicable.
                if (_objImprovementSource == Improvement.ImprovementSource.Cyberware)
                {
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.CyberwareEssCostNonRetroactive) != 0)
                    {
                        decimal decMultiplier = 1;
                        decMultiplier = _objCharacter.Improvements
                            .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.CyberwareEssCostNonRetroactive && objImprovement.Enabled)
                            .Aggregate(decMultiplier, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                        _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                    }
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive) != 0)
                    {
                        foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.CyberwareTotalEssMultiplierNonRetroactive))
                        {
                            _decExtraESSMultiplicativeMultiplier *= (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m);
                        }
                    }
                }

                // Apply the character's Bioware Essence cost multiplier if applicable.
                else if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.BiowareEssCostNonRetroactive) != 0)
                    {
                        decimal decMultiplier = 1;
                        decMultiplier = _objCharacter.Improvements
                            .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BiowareEssCostNonRetroactive && objImprovement.Enabled)
                            .Aggregate(decMultiplier, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                        _decExtraESSAdditiveMultiplier -= 1.0m - decMultiplier;
                    }
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive) != 0)
                    {
                        foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.BiowareTotalEssMultiplierNonRetroactive))
                        {
                            _decExtraESSMultiplicativeMultiplier *= (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m);
                        }
                    }
                }
            }

            if (blnCreateChildren)
                CreateChildren(objXmlCyberware, objNode, objGrade, objWeapons, objWeaponNodes, objVehicles, objVehicleNodes, blnCreateImprovements && string.IsNullOrEmpty(_strPlugsIntoModularMount));
        }

        private void CreateChildren(XmlNode objParentNode, TreeNode objParentTreeNode, Grade objGrade, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, List<Vehicle> objVehicles, List<TreeNode> objVehicleNodes, bool blnCreateImprovements = true)
        {
            // If we've just added a new base item, see if there are any subsystems that should automatically be added.
            if (objParentNode.InnerXml.Contains("<subsystems>"))
            {
                // Load Cyberware subsystems first
                XmlDocument objXmlDocument = XmlManager.Load("cyberware.xml");
                XmlNodeList objXmlSubSystemNameList = objParentNode.SelectNodes("subsystems/cyberware");

                foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                {
                    XmlNode objXmlSubsystem = objXmlDocument.SelectSingleNode("/chummer/cyberwares/cyberware[name = \"" + objXmlSubsystemNode["name"].InnerText + "\"]");

                    Cyberware objSubsystem = new Cyberware(_objCharacter);
                    TreeNode objSubsystemNode = new TreeNode
                    {
                        Text = objSubsystem.DisplayName,
                        Tag = objSubsystem.InternalId,
                        ForeColor = SystemColors.GrayText,
                        ContextMenuStrip = objParentTreeNode.ContextMenuStrip
                    };
                    int intSubSystemRating = Convert.ToInt32(objXmlSubsystemNode["rating"]?.InnerText);
                    objSubsystem.Create(objXmlSubsystem, _objCharacter, objGrade, Improvement.ImprovementSource.Cyberware, intSubSystemRating, objSubsystemNode, objWeapons, objWeaponNodes, objVehicles, objVehicleNodes, blnCreateImprovements, true, objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty, this);
                    objSubsystem.ParentID = InternalId;
                    objSubsystem.Cost = "0";
                    // If the <subsystem> tag itself contains extra children, add those, too
                    objSubsystem.CreateChildren(objXmlSubsystemNode, objSubsystemNode, objGrade, objWeapons, objWeaponNodes, objVehicles, objVehicleNodes, blnCreateImprovements);

                    _objChildren.Add(objSubsystem);

                    objParentTreeNode.Nodes.Add(objSubsystemNode);
                    objParentTreeNode.Expand();
                }

                // Load bioware subsystems next
                objXmlDocument = XmlManager.Load("bioware.xml");
                objXmlSubSystemNameList = objParentNode.SelectNodes("subsystems/bioware");

                foreach (XmlNode objXmlSubsystemNode in objXmlSubSystemNameList)
                {
                    XmlNode objXmlSubsystem = objXmlDocument.SelectSingleNode("/chummer/biowares/bioware[name = \"" + objXmlSubsystemNode["name"].InnerText + "\"]");

                    Cyberware objSubsystem = new Cyberware(_objCharacter);
                    TreeNode objSubsystemNode = new TreeNode
                    {
                        Text = objSubsystem.DisplayName,
                        Tag = objSubsystem.InternalId,
                        ForeColor = SystemColors.GrayText,
                        ContextMenuStrip = objParentTreeNode.ContextMenuStrip
                    };
                    int intSubSystemRating = Convert.ToInt32(objXmlSubsystemNode["rating"]?.InnerText);
                    objSubsystem.Create(objXmlSubsystem, _objCharacter, objGrade, Improvement.ImprovementSource.Bioware, intSubSystemRating, objSubsystemNode, objWeapons, objWeaponNodes, objVehicles, objVehicleNodes, blnCreateImprovements, true, objXmlSubsystemNode["forced"]?.InnerText ?? string.Empty, this);
                    objSubsystem.ParentID = InternalId;
                    objSubsystem.Cost = "0";
                    // If the <subsystem> tag itself contains extra children, add those, too
                    objSubsystem.CreateChildren(objXmlSubsystemNode, objSubsystemNode, objGrade, objWeapons, objWeaponNodes, objVehicles, objVehicleNodes, blnCreateImprovements);

                    _objChildren.Add(objSubsystem);

                    objParentTreeNode.Nodes.Add(objSubsystemNode);
                    objParentTreeNode.Expand();
                }
            }

            // Check to see if there are any child elements.
            if (objParentNode.InnerXml.Contains("<gears>"))
            {
                // Open the Gear XML file and locate the selected piece.
                XmlDocument objXmlGearDocument = XmlManager.Load("gear.xml");

                // Create Gear using whatever information we're given.
                foreach (XmlNode objXmlChild in objParentNode.SelectNodes("gears/usegear"))
                {
                    XmlNode objXmlGear = objXmlGearDocument.SelectSingleNode("/chummer/gears/gear[name = \"" + objXmlChild["name"].InnerText + "\" and category = \"" + objXmlChild["category"].InnerText + "\"]");
                    int intChildRating = 0;
                    decimal decChildQty = 1;
                    string strChildForceSource = string.Empty;
                    string strChildForcePage = string.Empty;
                    string strChildForceValue = string.Empty;
                    bool blnStartCollapsed = objXmlChild["name"].Attributes?["startcollapsed"]?.InnerText == "yes";
                    if (objXmlChild["rating"] != null)
                        intChildRating = Convert.ToInt32(objXmlChild["rating"].InnerText);
                    if (objXmlChild["name"].Attributes["qty"] != null)
                        decChildQty = Convert.ToDecimal(objXmlChild["name"].Attributes["qty"].InnerText, GlobalOptions.InvariantCultureInfo);
                    if (objXmlChild["name"].Attributes["select"] != null)
                        strChildForceValue = objXmlChild["name"].Attributes["select"].InnerText;
                    if (objXmlChild["source"] != null)
                        strChildForceSource = objXmlChild["source"].InnerText;
                    if (objXmlChild["page"] != null)
                        strChildForcePage = objXmlChild["page"].InnerText;
                    // Create the new piece of Gear.
                    TreeNode objChildNode = new TreeNode();
                    List<Weapon> objChildWeapons = new List<Weapon>();
                    List<TreeNode> objChildWeaponNodes = new List<TreeNode>();

                    Gear objChild = new Gear(_objCharacter);
                    objChild.Create(objXmlGear, objChildNode, intChildRating, objChildWeapons, objChildWeaponNodes, strChildForceValue, false, false, blnCreateImprovements);
                    objChild.Quantity = decChildQty;
                    objChildNode.Text = objChild.DisplayName;

                    objChild.Cost = "0";
                    objChild.ParentID = InternalId;
                    if (!string.IsNullOrEmpty(strChildForceSource))
                        objChild.Source = strChildForceSource;
                    if (!string.IsNullOrEmpty(strChildForcePage))
                        objChild.Page = strChildForcePage;
                    if (objXmlChild["capacity"] != null)
                        objChild.Capacity = "[" + objXmlChild["capacity"].InnerText + "]";
                    // Create any Weapons that came with this Gear.
                    foreach (Weapon objWeapon in objChildWeapons)
                        objWeapons.Add(objWeapon);

                    foreach (TreeNode objWeaponNode in objChildWeaponNodes)
                        objWeaponNodes.Add(objWeaponNode);

                    _lstGear.Add(objChild);

                    objChildNode.Text = objChild.DisplayName;
                    objChildNode.Tag = objChild.InternalId;
                    objParentTreeNode.Nodes.Add(objChildNode);
                    if (!blnStartCollapsed)
                        objParentTreeNode.Expand();
                }
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("cyberware");
            objWriter.WriteElementString("sourceid", _sourceID.ToString());
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("category", _strCategory);
            objWriter.WriteElementString("limbslot", _strLimbSlot);
            objWriter.WriteElementString("limbslotcount", _strLimbSlotCount);
            objWriter.WriteElementString("inheritattributes", _blnInheritAttributes.ToString());
            objWriter.WriteElementString("ess", _strESS);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("parentid", _strParentID);
            objWriter.WriteElementString("hasmodularmount", _strHasModularMount);
            objWriter.WriteElementString("plugsintomodularmount", _strPlugsIntoModularMount);
            objWriter.WriteElementString("blocksmounts", _strBlocksMounts);
            objWriter.WriteElementString("forced", _strForced);
            objWriter.WriteElementString("rating", _intRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("minrating", _strMinRating);
            objWriter.WriteElementString("maxrating", _strMaxRating);
            objWriter.WriteElementString("subsystems", _strAllowSubsystems);
            objWriter.WriteElementString("grade", _objGrade.Name);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("suite", _blnSuite.ToString());
            objWriter.WriteElementString("essdiscount", _intEssenceDiscount.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("extraessadditivemultiplier", _decExtraESSAdditiveMultiplier.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("extraessmultiplicativemultiplier", _decExtraESSMultiplicativeMultiplier.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("forcegrade", _strForceGrade);
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("prototypetranshuman", _blnPrototypeTranshuman.ToString());
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            if (_nodPairBonus != null)
                objWriter.WriteRaw(_nodPairBonus.OuterXml);
            else
                objWriter.WriteElementString("pairbonus", string.Empty);
            if (_nodWirelessBonus != null)
                objWriter.WriteRaw(_nodWirelessBonus.OuterXml);
            else
                objWriter.WriteElementString("wirelessbonus", string.Empty);
            if (_nodAllowGear != null)
                objWriter.WriteRaw(_nodAllowGear.OuterXml);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            if (_guiWeaponID != Guid.Empty)
                objWriter.WriteElementString("weaponguid", _guiWeaponID.ToString());
            if (_guiVehicleID != Guid.Empty)
                objWriter.WriteElementString("vehicleguid", _guiVehicleID.ToString());
            objWriter.WriteStartElement("children");
            foreach (Cyberware objChild in _objChildren)
            {
                objChild.Save(objWriter);
            }
            objWriter.WriteEndElement();
            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    objGear.Save(objWriter);
                }
                objWriter.WriteEndElement();
            }
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
            objWriter.WriteElementString("addtoparentess", AddToParentESS.ToString());

            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString());
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString());
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
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString());
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            if (objNode.TryGetField("sourceid", Guid.TryParse, out _sourceID))
                _objCachedMyXmlNode = null;
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
            }
            else
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            if (objNode["improvementsource"] != null)
            {
                _objImprovementSource = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
                _objCachedMyXmlNode = null;
            }
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
            objNode.TryGetStringFieldQuickly("limbslotcount", ref _strLimbSlotCount);
            objNode.TryGetBoolFieldQuickly("inheritattributes", ref _blnInheritAttributes);
            objNode.TryGetStringFieldQuickly("ess", ref _strESS);
            objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("parentid", ref _strParentID);
            if (!objNode.TryGetStringFieldQuickly("hasmodularmount", ref _strHasModularMount))
                _strHasModularMount = MyXmlNode?["hasmodularmount"]?.InnerText ?? string.Empty;
            if (!objNode.TryGetStringFieldQuickly("plugsintomodularmount", ref _strPlugsIntoModularMount))
                _strPlugsIntoModularMount = MyXmlNode?["plugsintomodularmount"]?.InnerText ?? string.Empty;
            if (!objNode.TryGetStringFieldQuickly("blocksmounts", ref _strBlocksMounts))
                _strBlocksMounts = MyXmlNode?["blocksmounts"]?. InnerText ?? string.Empty;
            objNode.TryGetStringFieldQuickly("forced", ref _strForced);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
            objNode.TryGetStringFieldQuickly("maxrating", ref _strMaxRating);
            // Legacy shim for old-form customized attribute
            if ((Name == "Customized Agility" || Name == "Customized Strength" || Name == "Cyberlimb Customization, Agility (2050)" || Name == "Cyberlimb Customization, Strength (2050)") &&
                int.TryParse(MaxRatingString, out int intDummy))
            {
                XmlNode objMyXmlNode = MyXmlNode;
                if (objMyXmlNode != null)
                {
                    objMyXmlNode.TryGetStringFieldQuickly("minrating", ref _strMinRating);
                    objMyXmlNode.TryGetStringFieldQuickly("rating", ref _strMaxRating);
                    objMyXmlNode.TryGetStringFieldQuickly("avail", ref _strAvail);
                    objMyXmlNode.TryGetStringFieldQuickly("cost", ref _strCost);
                }
            }
            objNode.TryGetStringFieldQuickly("subsystems", ref _strAllowSubsystems);
            if (objNode["grade"] != null)
                _objGrade = ConvertToCyberwareGrade(objNode["grade"].InnerText, _objImprovementSource, _objCharacter.Options);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            if (!objNode.TryGetStringFieldQuickly("extra", ref _strExtra) && _strLocation != "Left" && _strLocation != "Right")
            {
                _strExtra = _strLocation;
                _strLocation = string.Empty;
            }
            objNode.TryGetBoolFieldQuickly("suite", ref _blnSuite);
            objNode.TryGetInt32FieldQuickly("essdiscount", ref _intEssenceDiscount);
            objNode.TryGetDecFieldQuickly("extraessadditivemultiplier", ref _decExtraESSAdditiveMultiplier);
            objNode.TryGetDecFieldQuickly("extraessmultiplicativemultiplier", ref _decExtraESSMultiplicativeMultiplier);
            objNode.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);
            objNode.TryGetBoolFieldQuickly("prototypetranshuman", ref _blnPrototypeTranshuman);
            _nodBonus = objNode["bonus"];
            _nodPairBonus = objNode["pairbonus"];
            _nodWirelessBonus = objNode["wirelessbonus"];
            if (!objNode.TryGetBoolFieldQuickly("wirelesson", ref _blnWirelessOn))
            {
                _blnWirelessOn = _nodWirelessBonus != null;
            }
            _nodAllowGear = objNode["allowgear"];
            // Legacy Sweep
            if (_strForceGrade != "None" && (_strCategory.StartsWith("Genetech") || _strCategory.StartsWith("Genetic Infusions") || _strCategory.StartsWith("Genemods")))
            {
                _strForceGrade = MyXmlNode?["forcegrade"].InnerText;
                if (!string.IsNullOrEmpty(_strForceGrade))
                    _objGrade = ConvertToCyberwareGrade(_strForceGrade, _objImprovementSource, _objCharacter.Options);
            }
            if (objNode["weaponguid"] != null)
            {
                _guiWeaponID = Guid.Parse(objNode["weaponguid"].InnerText);
            }
            if (objNode["vehicleguid"] != null)
            {
                _guiVehicleID = Guid.Parse(objNode["vehicleguid"].InnerText);
            }

            if (GlobalOptions.Language != GlobalOptions.DefaultLanguage)
            {
                XmlNode objCyberwareNode = MyXmlNode;
                if (objCyberwareNode != null)
                {
                    objCyberwareNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objCyberwareNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

                string strXmlFile = "cyberware.xml";
                if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    strXmlFile = "bioware.xml";
                }
                XmlDocument objXmlDocument = XmlManager.Load(strXmlFile);
                objCyberwareNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + _strCategory + "\"]");
                _strAltCategory = objCyberwareNode?.Attributes?["translate"]?.InnerText;
            }

            if (objNode.InnerXml.Contains("<cyberware>"))
            {
                XmlNodeList nodChildren = objNode.SelectNodes("children/cyberware");
                foreach (XmlNode nodChild in nodChildren)
                {
                    Cyberware objChild = new Cyberware(_objCharacter);
                    objChild.Load(nodChild, blnCopy);
                    objChild.Parent = this;
                    _objChildren.Add(objChild);
                }
            }

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
            if (objNode["addtoparentess"] != null)
            {
                if (bool.TryParse(objNode["addtoparentess"].InnerText, out bool blnTmp))
                {
                    _blnAddToParentESS = blnTmp;
                }
            }
            else
                _blnAddToParentESS = MyXmlNode?["addtoparentess"] != null;

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
                MyXmlNode?.TryGetStringFieldQuickly("devicerating", ref _strDeviceRating);
            if (!objNode.TryGetStringFieldQuickly("programlimit", ref _strProgramLimit))
                MyXmlNode?.TryGetStringFieldQuickly("programs", ref _strProgramLimit);
            objNode.TryGetStringFieldQuickly("overclocked", ref _strOverclocked);
            if (!objNode.TryGetStringFieldQuickly("attack", ref _strAttack))
                MyXmlNode?.TryGetStringFieldQuickly("attack", ref _strAttack);
            if (!objNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze))
                MyXmlNode?.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
            if (!objNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing))
                MyXmlNode?.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("firewall", ref _strFirewall))
                MyXmlNode?.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            if (!objNode.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray))
                MyXmlNode?.TryGetStringFieldQuickly("attributearray", ref _strAttributeArray);
            if (!objNode.TryGetStringFieldQuickly("modattack", ref _strModAttack))
                MyXmlNode?.TryGetStringFieldQuickly("modattack", ref _strModAttack);
            if (!objNode.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze))
                MyXmlNode?.TryGetStringFieldQuickly("modsleaze", ref _strModSleaze);
            if (!objNode.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing))
                MyXmlNode?.TryGetStringFieldQuickly("moddataprocessing", ref _strModDataProcessing);
            if (!objNode.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall))
                MyXmlNode?.TryGetStringFieldQuickly("modfirewall", ref _strModFirewall);
            if (!objNode.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray))
                MyXmlNode?.TryGetStringFieldQuickly("modattributearray", ref _strModAttributeArray);

            this.RefreshMatrixAttributeArray();
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>obv
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture)
        {
            objWriter.WriteStartElement("cyberware");
            if (string.IsNullOrWhiteSpace(_strLimbSlot) && _strCategory != "Cyberlimb")
                objWriter.WriteElementString("name", DisplayNameShort);
            else
            {
                int intLimit = (TotalStrength * 2 + _objCharacter.BOD.TotalValue + _objCharacter.REA.TotalValue + 2) / 3;
                objWriter.WriteElementString("name", DisplayNameShort + " (" + _objCharacter.AGI.DisplayAbbrev + " " + TotalAgility.ToString() + ", " + _objCharacter.STR.DisplayAbbrev + " " + TotalStrength.ToString() + ", " + LanguageManager.GetString("String_LimitPhysicalShort") + " " + intLimit.ToString() + ")");
            }
            objWriter.WriteElementString("category", DisplayCategory);
            objWriter.WriteElementString("ess", CalculatedESS().ToString(objCulture));
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("avail", TotalAvail);
            objWriter.WriteElementString("cost", TotalCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            objWriter.WriteElementString("owncost", OwnCost.ToString(_objCharacter.Options.NuyenFormat, objCulture));
            if (_objCharacter.Options != null)
            {
                objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            }
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("rating", Rating.ToString(objCulture));
            objWriter.WriteElementString("minrating", MinRating.ToString(objCulture));
            objWriter.WriteElementString("maxrating", MaxRating.ToString(objCulture));
            objWriter.WriteElementString("allowsubsystems", _strAllowSubsystems);
            objWriter.WriteElementString("wirelesson", WirelessOn.ToString());
            objWriter.WriteElementString("grade", Grade.DisplayName);
            objWriter.WriteElementString("location", Location);
            objWriter.WriteElementString("extra", Extra);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    objGear.Print(objWriter, objCulture);
                }
                objWriter.WriteEndElement();
            }
            objWriter.WriteStartElement("children");
            foreach (Cyberware objChild in _objChildren)
            {
                objChild.Print(objWriter, objCulture);
            }
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("iscommlink", IsCommlink.ToString());
            objWriter.WriteElementString("active", this.IsActiveCommlink(_objCharacter).ToString());
            objWriter.WriteElementString("homenode", this.IsHomeNode(_objCharacter).ToString());
            objWriter.WriteElementString("attack", this.GetTotalMatrixAttribute("Attack").ToString(objCulture));
            objWriter.WriteElementString("sleaze", this.GetTotalMatrixAttribute("Sleaze").ToString(objCulture));
            objWriter.WriteElementString("dataprocessing", this.GetTotalMatrixAttribute("Data Processing").ToString(objCulture));
            objWriter.WriteElementString("firewall", this.GetTotalMatrixAttribute("Firewall").ToString(objCulture));
            objWriter.WriteElementString("devicerating", this.GetTotalMatrixAttribute("Device Rating").ToString(objCulture));
            objWriter.WriteElementString("programlimit", this.GetTotalMatrixAttribute("Program Limit").ToString(objCulture));
            objWriter.WriteEndElement();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this piece of Cyberware in the Improvement system.
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
        /// Guid of a Cyberware Drone/Vehicle.
        /// </summary>
        public string VehicleID
        {
            get
            {
                return _guiVehicleID.ToString();
            }
            set
            {
                _guiVehicleID = Guid.Parse(value);
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
        /// Bonus node from the XML file that only activates for each pair of 'ware.
        /// </summary>
        public XmlNode PairBonus
        {
            get
            {
                return _nodPairBonus;
            }
            set
            {
                _nodPairBonus = value;
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
        /// Whether the Cyberware's Wireless is enabled
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
        /// ImprovementSource Type.
        /// </summary>
        public Improvement.ImprovementSource SourceType
        {
            get
            {
                return _objImprovementSource;
            }
            set
            {
                if (_objImprovementSource != value)
                    _objCachedMyXmlNode = null;
                _objImprovementSource = value;
            }
        }

        /// <summary>
        /// Cyberware name.
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

        public Guid SourceID
        {
            get
            {
                return _sourceID;
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
                string strReturn = DisplayNameShort;

                if (Rating > 0 && _sourceID != Guid.Parse("b57eadaa-7c3b-4b80-8d79-cbbd922c1196"))
                {
                    strReturn += " (" + LanguageManager.GetString("String_Rating") + " " + Rating + ")";
                }

                if (!string.IsNullOrEmpty(_strExtra))
                {
                    LanguageManager.Load(GlobalOptions.Language, this);
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += " (" + LanguageManager.TranslateExtra(_strExtra) + ")";
                }

                if (!string.IsNullOrEmpty(_strLocation))
                {
                    string strSide = string.Empty;
                    LanguageManager.Load(GlobalOptions.Language, this);
                    if (_strLocation == "Left")
                        strSide = LanguageManager.GetString("String_Improvement_SideLeft");
                    else if (_strLocation == "Right")
                        strSide = LanguageManager.GetString("String_Improvement_SideRight");
                    if (!string.IsNullOrEmpty(strSide))
                        strReturn += " (" + strSide + ")";
                }
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
                if (!string.IsNullOrEmpty(_strAltCategory))
                    return _strAltCategory;

                return _strCategory;
            }
        }

        /// <summary>
        /// Cyberware category.
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
        /// The type of body "slot" a Cyberlimb occupies.
        /// </summary>
        public string LimbSlot
        {
            get
            {
                return _strLimbSlot;
            }
            set
            {
                _strLimbSlot = value;
            }
        }

        /// <summary>
        /// The amount of body "slots" a Cyberlimb occupies.
        /// </summary>
        public int LimbSlotCount
        {
            get
            {
                if (_strLimbSlotCount == "all")
                {
                    return _objCharacter.LimbCount(LimbSlot);
                }
                int.TryParse(_strLimbSlotCount, out int intReturn);
                return intReturn;
            }
            set
            {
                _strLimbSlotCount = value.ToString(GlobalOptions.InvariantCultureInfo);
            }
        }

        /// <summary>
        /// How many cyberlimbs does this cyberware have?
        /// </summary>
        public int CyberlimbCount
        {
            get
            {
                int intCount = 0;

                if (!string.IsNullOrEmpty(LimbSlot) && Name.Contains("Full"))
                {
                    intCount += LimbSlotCount;
                }

                foreach (Cyberware objCyberwareChild in Children)
                {
                    intCount += objCyberwareChild.CyberlimbCount;
                }

                return intCount;
            }
        }

        /// <summary>
        /// The location of a Cyberlimb (Left or Right).
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
        /// Original Forced Extra string associted with the 'ware.
        /// </summary>
        public string Forced
        {
            get
            {
                return _strForced;
            }
            set
            {
                _strForced = value;
            }
        }

        /// <summary>
        /// Extra string associted with the 'ware.
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
        /// Essence cost of the Cyberware.
        /// </summary>
        public string ESS
        {
            get
            {
                return _strESS;
            }
            set
            {
                _strESS = value;
            }
        }

        /// <summary>
        /// Cyberware capacity.
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

        /// <summary>
        /// The modular mount this cyberware contains. Returns string.Empty if it contains no mount.
        /// </summary>
        public string HasModularMount
        {
            get
            {
                return _strHasModularMount;
            }
            set
            {
                _strHasModularMount = value;
            }
        }

        /// <summary>
        /// What modular mount this cyberware plugs into. Returns string.Empty if it doesn't plug into a modular mount.
        /// </summary>
        public string PlugsIntoModularMount
        {
            get
            {
                return _strPlugsIntoModularMount;
            }
            set
            {
                _strPlugsIntoModularMount = value;
            }
        }

        /// <summary>
        /// Returns whether the 'ware is currently equipped (with improvements applied) or not.
        /// </summary>
        public bool IsModularCurrentlyEquipped
        {
            get
            {
                // Cyberware always equipped if it's not a modular one
                bool blnReturn = string.IsNullOrEmpty(PlugsIntoModularMount);
                if (blnReturn)
                    return true;
                Cyberware objCurrentParent = Parent;
                // If top-level parent is one that has a modular mount but also does not plug into another modular mount itself, then return true, otherwise return false
                while (objCurrentParent != null)
                {
                    if (!string.IsNullOrEmpty(objCurrentParent.HasModularMount))
                        blnReturn = true;
                    if (!string.IsNullOrEmpty(PlugsIntoModularMount))
                        blnReturn = false;
                    objCurrentParent = Parent.Parent;
                }
                return blnReturn;
            }
        }

        /// <summary>
        /// Equips a piece of modular cyberware, activating the improvements of it and its children. Call after attaching onto objCharacter.Cyberware or a parent
        /// </summary>
        public void ChangeModularEquip(bool blnEquip)
        {
            if (blnEquip)
            {
                // If the piece grants a bonus, pass the information to the Improvement Manager.
                if (Bonus != null || WirelessBonus != null || PairBonus != null)
                {
                    if (!string.IsNullOrEmpty(_strForced) && _strForced != "Right" && _strForced != "Left")
                        ImprovementManager.ForcedValue = _strForced;

                    if (Bonus != null)
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, Bonus, false, Rating, DisplayNameShort);
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (WirelessBonus != null && WirelessOn)
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, WirelessBonus, false, Rating, DisplayNameShort);
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (PairBonus != null)
                    {
                        List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Name == Name && x.Extra == Extra && x.IsModularCurrentlyEquipped).ToList();
                        int intCount = lstPairableCyberwares.Count;
                        if (!string.IsNullOrEmpty(Location))
                        {
                            intCount = Math.Min(lstPairableCyberwares.Count(x => x.Location == Location), lstPairableCyberwares.Count(x => x.Location != Location) - 1);
                        }
                        if (intCount >= 0 && intCount % 2 == 0)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, PairBonus, false, Rating, DisplayNameShort);
                        }
                    }
                }
            }
            else
            {
                ImprovementManager.RemoveImprovements(_objCharacter, SourceType, InternalId);
                if (PairBonus != null)
                {
                    List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Name == Name && x.Extra == Extra && x.IsModularCurrentlyEquipped).ToList();
                    int intCyberwaresCount = lstPairableCyberwares.Count - 1;
                    if (!string.IsNullOrEmpty(Location))
                    {
                        intCyberwaresCount = Math.Min(lstPairableCyberwares.Count(x => x.Location == Location) - 1, lstPairableCyberwares.Count(x => x.Location != Location));
                    }
                    foreach (Cyberware objLoopCyberware in lstPairableCyberwares.Where(x => x.InternalId != InternalId))
                    {
                        ImprovementManager.RemoveImprovements(_objCharacter, objLoopCyberware.SourceType, objLoopCyberware.InternalId);
                        if (objLoopCyberware.Bonus != null)
                            ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType, objLoopCyberware.InternalId, objLoopCyberware.Bonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort);
                        if (objLoopCyberware.WirelessOn && objLoopCyberware.WirelessBonus != null)
                            ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType, objLoopCyberware.InternalId, objLoopCyberware.WirelessBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort);
                        if (intCyberwaresCount > 0 && intCyberwaresCount % 2 == 0)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, objLoopCyberware.SourceType, objLoopCyberware.InternalId, objLoopCyberware.PairBonus, false, objLoopCyberware.Rating, objLoopCyberware.DisplayNameShort);
                        }
                        intCyberwaresCount -= 1;
                    }
                }
            }

            foreach (Gear objChildGear in Gear)
                CommonFunctions.ChangeGearEquippedStatus(_objCharacter, objChildGear, blnEquip);

            foreach (Cyberware objChild in Children)
                objChild.ChangeModularEquip(blnEquip);
        }

        /// <summary>
        /// Comma-separated list of mount locations with which this 'ware is mutually exclusive.
        /// </summary>
        public string BlocksMounts
        {
            get
            {
                return _strBlocksMounts;
            }
            set
            {
                _strBlocksMounts = value;
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
        /// Total Minimum Rating.
        /// </summary>
        public int MinRating
        {
            get
            {
                int intReturn = 0;
                string strRating = _strMinRating;

                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strRating) && !int.TryParse(strRating, out intReturn))
                {
                    strRating = strRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString());
                    strRating = strRating.CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString());
                    strRating = strRating.CheapReplace("MinimumSTR", () => (ParentVehicle != null ? ParentVehicle.TotalBody : 3).ToString());
                    strRating = strRating.CheapReplace("MinimumAGI", () => (ParentVehicle != null ? ParentVehicle.Pilot : 3).ToString());
                    try
                    {
                        intReturn = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strRating));
                    }
                    catch (XPathException)
                    {
                    }
                }

                return intReturn;
            }
        }

        /// <summary>
        /// String representing minimum rating before it would be computed.
        /// </summary>
        public string MinRatingString
        {
            get
            {
                return _strMinRating;
            }
            set
            {
                _strMinRating = value;
            }
        }

        /// <summary>
        /// Total Maximum Rating.
        /// </summary>
        public int MaxRating
        {
            get
            {
                int intReturn = 0;
                string strRating = _strMaxRating;

                // Not a simple integer, so we need to start mucking around with strings
                if (!string.IsNullOrEmpty(strRating) && !int.TryParse(strRating, out intReturn))
                {
                    strRating = strRating.CheapReplace("MaximumSTR", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.TotalBody * 2) : _objCharacter.STR.TotalMaximum).ToString());
                    strRating = strRating.CheapReplace("MaximumAGI", () => (ParentVehicle != null ? Math.Max(1, ParentVehicle.Pilot * 2) : _objCharacter.AGI.TotalMaximum).ToString());
                    strRating = strRating.CheapReplace("MinimumSTR", () => (ParentVehicle != null ? ParentVehicle.TotalBody : 3).ToString());
                    strRating = strRating.CheapReplace("MinimumAGI", () => (ParentVehicle != null ? ParentVehicle.Pilot : 3).ToString());
                    try
                    {
                        intReturn = Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strRating));
                    }
                    catch (XPathException)
                    {
                    }
                }

                return intReturn;
            }
        }

        /// <summary>
        /// String representing maximum rating before it would be computed.
        /// </summary>
        public string MaxRatingString
        {
            get
            {
                return _strMaxRating;
            }
            set
            {
                _strMaxRating = value;
            }
        }

        /// <summary>
        /// Grade level of the Cyberware.
        /// </summary>
        public Grade Grade
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_strForceGrade) && _strForceGrade != _objGrade.Name)
                {
                    return ConvertToCyberwareGrade(_strForceGrade, Improvement.ImprovementSource.Bioware, _objCharacter.Options);
                }
                return _objGrade;
            }
            set
            {
                _objGrade = value;
            }
        }

        /// <summary>
        /// The Categories of allowable Subsystems.
        /// </summary>
        public string AllowedSubsystems
        {
            get
            {
                return _strAllowSubsystems;
            }
            set
            {
                _strAllowSubsystems = value;
            }
        }

        /// <summary>
        /// Whether or not the piece of Cyberware is part of a Cyberware Suite.
        /// </summary>
        public bool Suite
        {
            get
            {
                return _blnSuite;
            }
            set
            {
                _blnSuite = value;
            }
        }

        /// <summary>
        /// Essence cost discount.
        /// </summary>
        public int ESSDiscount
        {
            get
            {
                return _intEssenceDiscount;
            }
            set
            {
                _intEssenceDiscount = value;
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (additively stacking, starts at 0).
        /// </summary>
        public decimal ExtraESSAdditiveMultiplier
        {
            get
            {
                return _decExtraESSAdditiveMultiplier;
            }
            set
            {
                _decExtraESSAdditiveMultiplier = value;
            }
        }

        /// <summary>
        /// Extra Essence cost multiplier (multiplicatively stacking, starts at 1).
        /// </summary>
        public decimal ExtraESSMultiplicativeMultiplier
        {
            get
            {
                return _decExtraESSMultiplicativeMultiplier;
            }
            set
            {
                _decExtraESSMultiplicativeMultiplier = value;
            }
        }

        /// <summary>
        /// Base Physical Boxes. 12 for vehicles, 6 for Drones.
        /// </summary>
        public int BaseMatrixBoxes
        {
            get
            {
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

        /// <summary>
        /// A List of child pieces of Cyberware.
        /// </summary>
        public IList<Cyberware> Children
        {
            get
            {
                return _objChildren;
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
        /// Whether or not the Cyberware's cost should be discounted by 10% through the Black Market Pipeline Quality.
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
        /// Whether or not the Cyberware's ESS cost increases that of its parent when added as a subsystem (usually no).
        /// </summary>
        public bool AddToParentESS
        {
            get
            {
                return _blnAddToParentESS;
            }
            set
            {
                _blnAddToParentESS = value;
            }
        }

        /// <summary>
        /// Parent Cyberware.
        /// </summary>
        public Cyberware Parent
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
        /// Topmost Parent Cyberware.
        /// </summary>
        public Cyberware TopMostParent
        {
            get
            {
                Cyberware objReturn = this;
                while (objReturn.Parent != null)
                    objReturn = objReturn.Parent;
                return objReturn;
            }
        }

        /// <summary>
        /// Vehicle to which this cyberware is attached (if any)
        /// </summary>
        public Vehicle ParentVehicle
        {
            get
            {
                return _objParentVehicle ?? Parent?.ParentVehicle;
            }
            set
            {
                _objParentVehicle = value;
            }
        }

        /// <summary>
        /// Grade that the Cyberware should be forced to use, if applicable.
        /// </summary>
        public string ForceGrade
        {
            get
            {
                return _strForceGrade;
            }
        }

        /// <summary>
        /// Is the bioware's cost affected by Prototype Transhuman?
        /// </summary>
        public bool PrototypeTranshuman
        {
            get
            {
                return _blnPrototypeTranshuman && _objCharacter.PrototypeTranshuman > 0;
            }
            set
            {
                _blnPrototypeTranshuman = value;
                foreach (Cyberware objCyberware in Children)
                    objCyberware.PrototypeTranshuman = value;
            }
        }

        private XmlNode _objCachedMyXmlNode = null;
        public XmlNode MyXmlNode
        {
            get
            {
                if (_objCachedMyXmlNode != null && !GlobalOptions.LiveCustomData)
                    return _objCachedMyXmlNode;
                XmlDocument objDoc = null;
                if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    objDoc = XmlManager.Load("bioware.xml");
                    if (objDoc != null)
                    {
                        _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/biowares/bioware[id = \"" + _sourceID.ToString() + "\" or id = \"" + _sourceID.ToString().ToUpperInvariant() + "\"]");
                        if (_objCachedMyXmlNode == null)
                        {
                            _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/biowares/bioware[name = \"" + Name + "\"]");
                            if (_objCachedMyXmlNode != null)
                            {
                                _objCachedMyXmlNode.TryGetField("id", Guid.TryParse, out _sourceID);
                            }
                        }
                    }
                }
                else
                {
                    objDoc = XmlManager.Load("cyberware.xml");
                    if (objDoc != null)
                    {
                        _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/cyberwares/cyberware[id = \"" + _sourceID.ToString() + "\" or id = \"" + _sourceID.ToString().ToUpperInvariant() + "\"]");
                        if (_objCachedMyXmlNode == null)
                        {
                            _objCachedMyXmlNode = objDoc.SelectSingleNode("/chummer/cyberwares/cyberware[name = \"" + Name + "\"]");
                            if (_objCachedMyXmlNode != null)
                            {
                                _objCachedMyXmlNode.TryGetField("id", Guid.TryParse, out _sourceID);
                            }
                        }
                    }
                }
                return _objCachedMyXmlNode;
            }
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Total Availablility of the Cyberware and its plugins.
        /// </summary>
        public string TotalAvail
        {
            get
            {
                // If the Avail starts with "+", return the base string and don't try to calculate anything since we're looking at a child component.
                if (_strAvail.StartsWith('+'))
                {
                    if (_strAvail.Contains("Rating") || _strAvail.Contains("MinRating"))
                    {
                        // If the availability is determined by the Rating, evaluate the expression.
                        string strAvail = string.Empty;
                        string strAvailExpr = _strAvail.Substring(1, _strAvail.Length - 1);

                        if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                        {
                            strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                            // Remove the trailing character if it is "F" or "R".
                            strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                        }
                        strAvailExpr = strAvailExpr.CheapReplace("MinRating", () => MinRating.ToString());
                        strAvailExpr = strAvailExpr.Replace("Rating", Rating.ToString());
                        return "+" + CommonFunctions.EvaluateInvariantXPath(strAvailExpr).ToString() + strAvail;
                    }
                    else
                        return _strAvail;
                }

                string strBaseAvail = _strAvail;
                if (strBaseAvail.StartsWith("FixedValues"))
                {
                    string[] strValues = strBaseAvail.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    strBaseAvail = strValues[Math.Min(Rating, strValues.Length) - 1];
                }
                bool blnCheckGearAvail = strBaseAvail.Contains(" or Gear");
                strBaseAvail = strBaseAvail.Replace(" or Gear", string.Empty);
                string strCalculated = string.Empty;
                string strReturn = string.Empty;

                // Second Hand Cyberware has a reduced Availability.
                // Apply the Grade's Avail modifier.
                int intAvailModifier = Grade.Avail;

                if (strBaseAvail.Contains("Rating"))
                {
                    // If the availability is determined by the Rating, evaluate the expression.
                    string strAvail = string.Empty;
                    string strAvailExpr = strBaseAvail;

                    if (strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "F" || strAvailExpr.Substring(strAvailExpr.Length - 1, 1) == "R")
                    {
                        strAvail = strAvailExpr.Substring(strAvailExpr.Length - 1, 1);
                        // Remove the trailing character if it is "F" or "R".
                        strAvailExpr = strAvailExpr.Substring(0, strAvailExpr.Length - 1);
                    }
                    strCalculated = (Convert.ToInt32(CommonFunctions.EvaluateInvariantXPath(strAvailExpr.Replace("Rating", Rating.ToString()))) + intAvailModifier).ToString() + strAvail;
                }
                else
                {
                    // Just a straight cost, so return the value.
                    if (strBaseAvail.EndsWith('F') || strBaseAvail.EndsWith('R'))
                    {
                        strCalculated = (Convert.ToInt32(strBaseAvail.Substring(0, strBaseAvail.Length - 1)) + intAvailModifier).ToString() + strBaseAvail.Substring(strBaseAvail.Length - 1, 1);
                    }
                    else
                        strCalculated = (Convert.ToInt32(strBaseAvail) + intAvailModifier).ToString();
                }

                int intAvail = 0;
                string strAvailText = string.Empty;
                if (strCalculated.EndsWith('F') || strCalculated.EndsWith('R'))
                {
                    strAvailText = strCalculated.Substring(strCalculated.Length - 1);
                    intAvail = Convert.ToInt32(strCalculated.Substring(0, strCalculated.Length - 1));
                }
                else
                    intAvail = Convert.ToInt32(strCalculated);

                // Run through the child items and increase the Avail by any Mod whose Avail contains "+".
                foreach (Cyberware objChild in _objChildren)
                {
                    if (objChild.Avail.Contains('+'))
                    {
                        string strChildAvail = objChild.Avail;
                        if (objChild.Avail.Contains("Rating") || objChild.Avail.Contains("MinRating"))
                        {
                            strChildAvail = strChildAvail.CheapReplace("MinRating", () => objChild.MinRating.ToString());
                            strChildAvail = strChildAvail.Replace("Rating", objChild.Rating.ToString());
                            string strChildAvailText = string.Empty;
                            if (strChildAvail.EndsWith('R') || strChildAvail.EndsWith('F'))
                            {
                                strChildAvailText = strChildAvail.Substring(objChild.Avail.Length - 1);
                                strChildAvail = strChildAvail.Substring(0, strChildAvail.Length - 1);
                            }

                            // If the availability is determined by the Rating, evaluate the expression.
                            string strChildAvailExpr = strChildAvail;

                            // Remove the "+" since the expression can't be evaluated if it starts with this.
                            strChildAvail = "+" + CommonFunctions.EvaluateInvariantXPath(strChildAvailExpr.TrimStart('+'));
                            if (!string.IsNullOrEmpty(strChildAvailText))
                                strChildAvail += strChildAvailText;
                        }

                        if (strChildAvail.EndsWith('R') || strChildAvail.EndsWith('F'))
                        {
                            if (strAvailText != "F")
                                strAvailText = strChildAvail.Substring(objChild.Avail.Length - 1);
                            intAvail += Convert.ToInt32(strChildAvail.Substring(0, strChildAvail.Length - 1));
                        }
                        else
                            intAvail += Convert.ToInt32(strChildAvail);
                    }
                }

                // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
                if (intAvail < 0)
                    intAvail = 0;

                if (blnCheckGearAvail)
                {
                    // Run through top-level Gears and take the maximum availability out of everything
                    int intLoopAvail = 0;
                    foreach (Gear objLoopGear in Gear)
                    {
                        string strLoopAvail = objLoopGear.TotalAvail(false, true);
                        if (strLoopAvail.EndsWith('R') || strLoopAvail.EndsWith('F'))
                        {
                            if (strAvailText != "F" && strLoopAvail.EndsWith('F'))
                                strAvailText = "F";
                            else
                                strAvailText = "R";
                            intLoopAvail = Convert.ToInt32(strLoopAvail.Substring(0, strLoopAvail.Length - 1));
                        }
                        else
                            intLoopAvail = Convert.ToInt32(strLoopAvail);
                        if (intAvail < intLoopAvail)
                            intAvail = intLoopAvail;
                    }
                }

                // Translate the Avail string.
                if (strAvailText == "R")
                    strAvailText = LanguageManager.GetString("String_AvailRestricted");
                else if (strAvailText == "F")
                    strAvailText = LanguageManager.GetString("String_AvailForbidden");

                strReturn = intAvail.ToString() + strAvailText;

                return strReturn;
            }
        }

        /// <summary>
        /// Caculated Capacity of the Cyberware.
        /// </summary>
        public string CalculatedCapacity
        {
            get
            {
                string strCapacity = _strCapacity;
                if (strCapacity.StartsWith("FixedValues"))
                {
                    string[] strValues = strCapacity.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    strCapacity = strValues[Math.Min(Rating, strValues.Length) - 1];
                }
                if (string.IsNullOrEmpty(strCapacity))
                    return "0";
                if (_strCapacity == "[*]")
                    return "*";
                string strReturn = "0";
                if (strCapacity.Contains("/["))
                {
                    int intPos = strCapacity.IndexOf("/[");
                    string strFirstHalf = strCapacity.Substring(0, intPos);
                    string strSecondHalf = strCapacity.Substring(intPos + 1, _strCapacity.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.Contains('['); ;

                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);
                    try
                    {
                        strReturn = ((double)CommonFunctions.EvaluateInvariantXPath(strFirstHalf.Replace("Rating", Rating.ToString()))).ToString("#,0.##", GlobalOptions.CultureInfo);
                    }
                    catch (XPathException)
                    {
                        strReturn = strFirstHalf;
                    }
                    catch (OverflowException) // Result is text and not a double
                    {
                        strReturn = strFirstHalf;
                    }
                    catch (InvalidCastException) // Result is text and not a double
                    {
                        strReturn = strFirstHalf;
                    }
                    if (blnSquareBrackets)
                        strReturn = "[" + strCapacity + "]";

                    strSecondHalf = strSecondHalf.Trim("[]".ToArray());
                    try
                    {
                        strSecondHalf = "[" + ((double)CommonFunctions.EvaluateInvariantXPath(strSecondHalf.Replace("Rating", Rating.ToString()))).ToString("#,0.##", GlobalOptions.CultureInfo) + "]";
                    }
                    catch (XPathException)
                    {
                        strSecondHalf = "[" + strSecondHalf + "]";
                    }
                    catch (OverflowException) // Result is text and not a double
                    {
                        strSecondHalf = "[" + strSecondHalf + "]";
                    }
                    catch (InvalidCastException) // Result is text and not a double
                    {
                        strSecondHalf = "[" + strSecondHalf + "]";
                    }

                    strReturn += "/" + strSecondHalf;
                }
                else if (strCapacity.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strCapacity.Contains('[');
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);

                    strReturn = ((double)CommonFunctions.EvaluateInvariantXPath(strCapacity.Replace("Rating", Rating.ToString()))).ToString("#,0.##", GlobalOptions.CultureInfo);
                    if (blnSquareBrackets)
                        strReturn = "[" + strReturn + "]";
                }
                else
                {
                    // Just a straight Capacity, so return the value.
                    strReturn = _strCapacity;
                }
                if (decimal.TryParse(strReturn, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decimal decReturn))
                    return decReturn.ToString("#,0.##", GlobalOptions.CultureInfo);
                return strReturn;
            }
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware.
        /// </summary>
        public decimal CalculatedESS(bool returnPrototype = true)
        {
            if (_blnPrototypeTranshuman && returnPrototype)
                return 0;
            if (SourceID == Guid.Parse("b57eadaa-7c3b-4b80-8d79-cbbd922c1196")) //Essence hole
            {
                return Convert.ToDecimal(Rating, GlobalOptions.InvariantCultureInfo) / 100m;
            }

            decimal decReturn = 0;

            string strESS = _strESS;
            if (strESS.StartsWith("FixedValues"))
            {
                string[] strValues = strESS.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                strESS = strValues[Math.Min(Rating, strValues.Length) - 1];
            }
            if (strESS.Contains("Rating"))
            {
                // If the cost is determined by the Rating, evaluate the expression.
                strESS = strESS.Replace("Rating", Rating.ToString());

                decReturn = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strESS), GlobalOptions.InvariantCultureInfo);
            }
            else
            {
                // Just a straight cost, so return the value.
                decimal.TryParse(strESS, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decReturn);
            }

            // Factor in the Essence multiplier of the selected CyberwareGrade.
            decimal decESSMultiplier = Grade.Essence + ExtraESSAdditiveMultiplier;
            decimal decTotalESSMultiplier = 1.0m * ExtraESSMultiplicativeMultiplier;

            if (_blnSuite)
                decESSMultiplier -= 0.1m;

            if (_intEssenceDiscount != 0)
            {
                decimal decDiscount = Convert.ToDecimal(_intEssenceDiscount, GlobalOptions.InvariantCultureInfo) * 0.01m;
                decTotalESSMultiplier *= 1.0m - decDiscount;
            }
            

            // Retrieve the Bioware or Cyberware ESS Cost Multiplier. Bioware Modifiers do not apply to Genetech.
            if (ForceGrade == "None")
            {
                decESSMultiplier = 1.0m;
                decTotalESSMultiplier = 1.0m;
            }
            else
            {
                decimal decMultiplier = 1;
                // Apply the character's Cyberware Essence cost multiplier if applicable.
                if (_objImprovementSource == Improvement.ImprovementSource.Cyberware)
                {
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.CyberwareEssCost) != 0)
                    {
                        decMultiplier = _objCharacter.Improvements
                            .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.CyberwareEssCost && objImprovement.Enabled)
                            .Aggregate(decMultiplier, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                        decESSMultiplier -= 1.0m - decMultiplier;
                    }
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.CyberwareTotalEssMultiplier) != 0)
                    {
                        foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.CyberwareTotalEssMultiplier))
                        {
                            decTotalESSMultiplier *= (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m);
                        }
                    }
                }

                // Apply the character's Bioware Essence cost multiplier if applicable.
                else if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.BiowareEssCost) != 0)
                    {
                        decMultiplier = _objCharacter.Improvements
                            .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BiowareEssCost && objImprovement.Enabled)
                            .Aggregate(decMultiplier, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                        decESSMultiplier -= 1.0m - decMultiplier;
                    }
                    if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.BiowareTotalEssMultiplier) != 0)
                    {
                        foreach (Improvement objImprovement in _objCharacter.Improvements.Where(x => x.Enabled && x.ImproveType == Improvement.ImprovementType.BiowareTotalEssMultiplier))
                        {
                            decTotalESSMultiplier *= (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m);
                        }
                    }
                }
                // Apply the character's Basic Bioware Essence cost multiplier if applicable.
                if (_strCategory == "Basic" && _objImprovementSource == Improvement.ImprovementSource.Bioware && ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.BasicBiowareEssCost) != 0)
                {
                    decimal decBasicMultiplier = _objCharacter.Improvements
                        .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BasicBiowareEssCost && objImprovement.Enabled)
                        .Aggregate<Improvement, decimal>(1, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                    decESSMultiplier -= 1.0m - decBasicMultiplier;
                }
            }
            decReturn = decReturn * decESSMultiplier * decTotalESSMultiplier;

            if (_objCharacter != null && !_objCharacter.Options.DontRoundEssenceInternally)
                decReturn = decimal.Round(decReturn, _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
            decReturn += _objChildren.Where(objChild => objChild.AddToParentESS).AsParallel().Sum(objChild => objChild.CalculatedESS());
            return decReturn;
        }

        public int GetBaseMatrixAttribute(string strAttributeName)
        {
            string strExpression = this.GetMatrixAttributeString(strAttributeName);
            if (string.IsNullOrEmpty(strExpression))
            {
                switch (strAttributeName)
                {
                    case "Device Rating":
                        return _objGrade.DeviceRating;
                    case "Program Limit":
                        if (IsCommlink)
                        {
                            strExpression = this.GetMatrixAttributeString("Device Rating");
                            if (string.IsNullOrEmpty(strExpression))
                                return _objGrade.DeviceRating;
                        }
                        else
                            return _objGrade.DeviceRating;
                        break;
                    case "Data Processing":
                    case "Firewall":
                        strExpression = this.GetMatrixAttributeString("Device Rating");
                        if (string.IsNullOrEmpty(strExpression))
                            return _objGrade.DeviceRating;
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
                    if (Children.Count + Gear.Count > 0 && strExpression.Contains("{Children " + strMatrixAttribute + "}"))
                    {
                        int intTotalChildrenValue = 0;
                        foreach (Cyberware objLoopCyberware in Children)
                        {
                            if (objLoopCyberware.IsModularCurrentlyEquipped)
                            {
                                intTotalChildrenValue += objLoopCyberware.GetBaseMatrixAttribute(strMatrixAttribute);
                            }
                        }
                        foreach (Gear objLoopGear in Gear)
                        {
                            if (objLoopGear.Equipped)
                            {
                                intTotalChildrenValue += objLoopGear.GetBaseMatrixAttribute(strMatrixAttribute);
                            }
                        }
                        objValue.Replace("{Children " + strMatrixAttribute + "}", intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    }
                }
                foreach (string strCharAttributeName in Attributes.AttributeSection.AttributeStrings)
                {
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "}", () => _objCharacter.GetAttribute(strCharAttributeName).TotalValue.ToString());
                    objValue.CheapReplace(strExpression, "{" + strCharAttributeName + "Base}", () => _objCharacter.GetAttribute(strCharAttributeName).TotalBase.ToString());
                }
                // This is first converted to a decimal and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                return Convert.ToInt32(Math.Ceiling((double)CommonFunctions.EvaluateInvariantXPath(objValue.ToString())));
            }
            int.TryParse(strExpression, out int intReturn);
            return intReturn;
        }

        public int GetBonusMatrixAttribute(string strAttributeName)
        {
            int intReturn = 0;

            if (Overclocked == strAttributeName)
            {
                intReturn += 1;
            }

            if (!strAttributeName.StartsWith("Mod "))
                strAttributeName = "Mod " + strAttributeName;

            foreach (Cyberware objLoopCyberware in Children)
            {
                if (objLoopCyberware.IsModularCurrentlyEquipped)
                {
                    intReturn += objLoopCyberware.GetTotalMatrixAttribute(strAttributeName);
                }
            }
            foreach (Gear objLoopGear in Gear)
            {
                if (objLoopGear.Equipped)
                {
                    intReturn += objLoopGear.GetTotalMatrixAttribute(strAttributeName);
                }
            }

            return intReturn;
        }

        /// <summary>
        /// Total cost of the just the Cyberware itself before we factor in any multipliers.
        /// </summary>
        public decimal OwnCostPreMultipliers
        {
            get
            {
                decimal decCost = 0;
                string strCostExpression = _strCost;

                if (strCostExpression.StartsWith("FixedValues"))
                {
                    string[] strValues = strCostExpression.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Rating > 0)
                        strCostExpression = strValues[Math.Min(Rating, strValues.Length) - 1].Trim("[]".ToArray());
                }

                string strParentCost = string.Empty;
                decimal decTotalParentGearCost = 0;
                if (_objParent != null)
                {
                    if (strCostExpression.Contains("Parent Cost"))
                        strParentCost = _objParent.Cost;
                    if (strCostExpression.Contains("Parent Gear Cost"))
                        foreach (Gear loopGear in _objParent.Gear)
                        {
                            decTotalParentGearCost += loopGear.CalculatedCost;
                        }
                }
                decimal decTotalGearCost = 0;
                if (Gear.Count > 0 && strCostExpression.Contains("Gear Cost"))
                {
                    foreach (Gear loopGear in Gear)
                    {
                        decTotalGearCost += loopGear.CalculatedCost;
                    }
                }
                decimal decTotalChildrenCost = 0;
                if (_objChildren.Count > 0 && strCostExpression.Contains("Children Cost"))
                {
                    foreach (Cyberware loopWare in _objChildren)
                    {
                        decTotalChildrenCost += loopWare.TotalCost;
                    }
                }

                if (string.IsNullOrEmpty(strCostExpression))
                    return 0;

                if (strCostExpression.Contains("Rating") || strCostExpression.Contains("MinRating") || strCostExpression.Contains("Parent Cost") || strCostExpression.Contains("Gear Cost") || strCostExpression.Contains("Children Cost"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    if (string.IsNullOrEmpty(strParentCost))
                        strParentCost = "0";
                    strCostExpression = strCostExpression.Replace("Parent Cost", strParentCost);
                    strCostExpression = strCostExpression.Replace("Parent Gear Cost", decTotalParentGearCost.ToString(GlobalOptions.InvariantCultureInfo));
                    strCostExpression = strCostExpression.Replace("Gear Cost", decTotalGearCost.ToString(GlobalOptions.InvariantCultureInfo));
                    strCostExpression = strCostExpression.Replace("Children Cost", decTotalChildrenCost.ToString(GlobalOptions.InvariantCultureInfo));
                    strCostExpression = strCostExpression.CheapReplace("MinRating", () => MinRating.ToString());
                    strCostExpression = strCostExpression.Replace("Rating", Rating.ToString());

                    decCost = Convert.ToDecimal(CommonFunctions.EvaluateInvariantXPath(strCostExpression).ToString(), GlobalOptions.InvariantCultureInfo);
                }
                else
                {
                    // Just a straight cost, so return the value.
                    try
                    {
                        decCost = Convert.ToDecimal(strCostExpression, GlobalOptions.InvariantCultureInfo);
                    }
                    catch (FormatException)
                    {
                        decCost = 0;
                    }
                }

                return decCost;
            }
        }

        /// <summary>
        /// Total cost of the Cyberware and its plugins.
        /// </summary>
        public decimal TotalCost
        {
            get
            {
                decimal decReturn = TotalCostWithoutModifiers;

                // Retrieve the Genetech Cost Multiplier if available.
                decimal decMultiplier = 1.0m;
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.GenetechCostMultiplier) != 0 && _objImprovementSource == Improvement.ImprovementSource.Bioware && _strCategory.StartsWith("Genetech"))
                {
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.GenetechCostMultiplier && objImprovement.Enabled)
                            decMultiplier -= (1 - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                    }
                }

                // Retrieve the Transgenics Cost Multiplier if available.
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.TransgenicsBiowareCost) != 0 && _objImprovementSource == Improvement.ImprovementSource.Bioware && _strCategory == "Genetech: Transgenics")
                {
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.TransgenicsBiowareCost && objImprovement.Enabled)
                            decMultiplier -= (1 - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                    }
                }

                if (decMultiplier == 0)
                    decMultiplier = 1;

                decimal decSuiteMultiplier = 1.0m;
                if (_blnSuite)
                    decSuiteMultiplier = 0.9m;

                return decReturn * decMultiplier * decSuiteMultiplier;
            }
        }

        /// <summary>
        /// Identical to TotalCost, but without the Improvement and Suite multpliers which would otherwise be doubled.
        /// </summary>
        private decimal TotalCostWithoutModifiers
        {
            get
            {
                decimal decCost = OwnCostPreMultipliers;
                decimal decReturn = decCost;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= Grade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Add in the cost of all child components.
                foreach (Cyberware objChild in _objChildren)
                {
                    if (objChild.Capacity != "[*]")
                    {
                        // If the child cost starts with "*", multiply the item's base cost.
                        if (objChild.Cost.StartsWith('*'))
                        {
                            decimal decPluginCost = 0;
                            string strMultiplier = objChild.Cost;
                            strMultiplier = strMultiplier.TrimStart('*');
                            decPluginCost = decCost * (Convert.ToDecimal(strMultiplier, GlobalOptions.InvariantCultureInfo) - 1);

                            if (objChild.DiscountCost)
                                decPluginCost *= 0.9m;

                            decReturn += decPluginCost;
                        }
                        else
                            decReturn += objChild.TotalCostWithoutModifiers;
                    }
                }

                // Add in the cost of all Gear plugins.
                foreach (Gear objGear in _lstGear)
                {
                    decReturn += objGear.TotalCost;
                }

                return decReturn;
            }
        }

        /// <summary>
        /// Cost of just the Cyberware itself.
        /// </summary>
        public decimal OwnCost
        {
            get
            {
                decimal decCost = OwnCostPreMultipliers;
                decimal decReturn = decCost;

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                decReturn *= Grade.Cost;

                if (DiscountCost)
                    decReturn *= 0.9m;

                // Retrieve the Genetech Cost Multiplier if available.
                decimal decMultiplier = 1.0m;
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.GenetechCostMultiplier) != 0 && _objImprovementSource == Improvement.ImprovementSource.Bioware && _strCategory.StartsWith("Genetech"))
                {
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.GenetechCostMultiplier && objImprovement.Enabled)
                            decMultiplier -= (1 - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                    }
                }

                // Retrieve the Transgenics Cost Multiplier if available.
                if (ImprovementManager.ValueOf(_objCharacter, Improvement.ImprovementType.TransgenicsBiowareCost) != 0 && _objImprovementSource == Improvement.ImprovementSource.Bioware && _strCategory == "Genetech: Transgenics")
                {
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.TransgenicsBiowareCost && objImprovement.Enabled)
                            decMultiplier -= (1 - (Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100.0m));
                    }
                }

                if (decMultiplier == 0)
                    decMultiplier = 1;

                decimal decSuiteMultiplier = 1.0m;
                if (_blnSuite)
                    decSuiteMultiplier = 0.9m;

                return decReturn * decMultiplier * decSuiteMultiplier;
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public decimal CapacityRemaining
        {
            get
            {
                decimal decCapacity = 0;
                if (_strCapacity.Contains("/["))
                {
                    // Get the Cyberware base Capacity.
                    string strBaseCapacity = CalculatedCapacity;
                    strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                    decCapacity = Convert.ToDecimal(strBaseCapacity, GlobalOptions.CultureInfo);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Children)
                    {
                        if (objChildCyberware.ParentID == InternalId)
                        {
                            continue;
                        }
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Gear objChildGear in Gear)
                    {
                        if (objChildGear.IncludedInParent)
                        {
                            continue;
                        }
                        string strCapacity = objChildGear.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }

                }
                else if (!_strCapacity.Contains('['))
                {
                    // Get the Cyberware base Capacity.
                    decCapacity = Convert.ToDecimal(CalculatedCapacity, GlobalOptions.CultureInfo);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Children)
                    {
                        if (objChildCyberware.ParentID == InternalId)
                        {
                            continue;
                        }
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Gear objChildGear in Gear)
                    {
                        if (objChildGear.IncludedInParent)
                        {
                            continue;
                        }
                        string strCapacity = objChildGear.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains('['))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        decCapacity -= Convert.ToDecimal(strCapacity, GlobalOptions.CultureInfo);
                    }
                }

                return decCapacity;
            }
        }

        /// <summary>
        /// Base Cyberlimb Strength (before modifiers and customization).
        /// </summary>
        public int BaseStrength
        {
            get
            {
                if (_strCategory != "Cyberlimb")
                    return 0;
                if (ParentVehicle != null)
                    return Math.Max(ParentVehicle.TotalBody, 0);
                // Base Strength for any limb is 3.
                return 3;
            }
        }

        /// <summary>
        /// Cyberlimb Strength.
        /// </summary>
        public int TotalStrength
        {
            get
            {
                if (_blnInheritAttributes)
                {
                    int intAverageAttribute = 0;
                    int intCyberlimbChildrenNumber = 0;
                    foreach (Cyberware objChild in _objChildren)
                    {
                        if (objChild.TotalStrength <= 0) continue;
                        intCyberlimbChildrenNumber += 1;
                        intAverageAttribute += objChild.TotalStrength;
                    }
                    if (intCyberlimbChildrenNumber == 0)
                        intCyberlimbChildrenNumber = 1;

                    return intAverageAttribute / intCyberlimbChildrenNumber;
                }

                if (_strCategory != "Cyberlimb")
                {
                    return 0;
                }

                int intAttribute = BaseStrength;
                int intBonus = 0;

                foreach (Cyberware objChild in _objChildren)
                {
                    // If the limb has Customized Strength, this is its new base value.
                    if (objChild.Name == "Customized Strength")
                        intAttribute = objChild.Rating;
                    // If the limb has Enhanced Strength, this adds to the limb's value.
                    if (objChild.Name == "Enhanced Strength")
                        intBonus = objChild.Rating;
                }
                if (ParentVehicle == null)
                {
                    return Math.Min(intAttribute + intBonus + _objCharacter.RedlinerBonus, _objCharacter.STR.TotalAugmentedMaximum);
                }
                else
                {
                    return Math.Min(intAttribute + intBonus, Math.Max(ParentVehicle.TotalBody * 2, 1));
                }
            }
        }

        /// <summary>
        /// Cyberlimb Body.
        /// </summary>
        public int TotalBody
        {
            get
            {
                if (_blnInheritAttributes)
                {
                    int intAverageAttribute = 0;
                    int intCyberlimbChildrenNumber = 0;
                    foreach (Cyberware objChild in _objChildren)
                    {
                        if (objChild.TotalBody <= 0) continue;
                        intCyberlimbChildrenNumber += 1;
                        intAverageAttribute += objChild.TotalBody;
                    }
                    if (intCyberlimbChildrenNumber == 0)
                        intCyberlimbChildrenNumber = 1;

                    return intAverageAttribute / intCyberlimbChildrenNumber;
                }

                if (_strCategory != "Cyberlimb")
                {
                    return 0;
                }

                // Base Strength for any limb is 3.
                int intAttribute = 3;
                int intBonus = 0;

                foreach (Cyberware objChild in _objChildren)
                {
                    // If the limb has Customized Body, this is its new base value.
                    if (objChild.Name == "Customized Body")
                        intAttribute = objChild.Rating;
                    // If the limb has Enhanced Body, this adds to the limb's value.
                    if (objChild.Name == "Enhanced Body")
                        intBonus = objChild.Rating;
                }

                return intAttribute + intBonus;
            }
        }

        /// <summary>
        /// Base Cyberlimb Agility (before modifiers and customization).
        /// </summary>
        public int BaseAgility
        {
            get
            {
                if (_strCategory != "Cyberlimb")
                    return 0;
                if (ParentVehicle != null)
                    return Math.Max(ParentVehicle.Pilot, 0);
                // Base Agility for any limb is 3.
                return 3;
            }
        }

        /// <summary>
        /// Cyberlimb Agility.
        /// </summary>
        public int TotalAgility
        {
            get
            {
                if (_blnInheritAttributes)
                {
                    int intAverageAttribute = 0;
                    int intCyberlimbChildrenNumber = 0;
                    foreach (Cyberware objChild in _objChildren)
                    {
                        if (objChild.TotalAgility <= 0) continue;
                        intCyberlimbChildrenNumber += 1;
                        intAverageAttribute += objChild.TotalAgility;
                    }
                    if (intCyberlimbChildrenNumber == 0)
                        intCyberlimbChildrenNumber = 1;

                    return intAverageAttribute / intCyberlimbChildrenNumber;
                }

                if (_strCategory != "Cyberlimb")
                {
                    return 0;
                }

                int intAttribute = BaseAgility;
                int intBonus = 0;

                foreach (Cyberware objChild in _objChildren)
                {
                    // If the limb has Customized Agility, this is its new base value.
                    if (objChild.Name == "Customized Agility")
                        intAttribute = objChild.Rating;
                    // If the limb has Enhanced Agility, this adds to the limb's value.
                    if (objChild.Name == "Enhanced Agility")
                        intBonus = objChild.Rating;
                }

                if (ParentVehicle == null)
                {
                    return Math.Min(intAttribute + intBonus + _objCharacter.RedlinerBonus, _objCharacter.AGI.TotalAugmentedMaximum);
                }
                else
                {
                    return Math.Min(intAttribute + intBonus, Math.Max(ParentVehicle.Pilot * 2, 1));
                }
            }
        }

        /// <summary>
        /// Whether or not the Cyberware is allowed to accept Modular Plugins.
        /// </summary>
        public bool AllowModularPlugins
        {
            get
            {
                foreach (Cyberware objChild in _objChildren)
                {
                    if (objChild.AllowedSubsystems.Contains("Modular Plug-In"))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsProgram => false;

        /// <summary>
        /// Device rating string for Cyberware. If it's empty, then GetBaseMatrixAttribute for Device Rating will fetch the grade's DR.
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
        /// Attack string (if one is explicitly specified for this 'ware).
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
        /// Sleaze string (if one is explicitly specified for this 'ware).
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
        /// Data Processing string (if one is explicitly specified for this 'ware).
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
        /// Firewall string (if one is explicitly specified for this 'ware).
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
        /// Empty for Cyberware.
        /// </summary>
        public string CanFormPersona { get => string.Empty; set { } }

        public bool IsCommlink => Gear.Any(x => x.CanFormPersona.Contains("Parent")) && this.GetTotalMatrixAttribute("Device Rating") > 0;

        /// <summary>
        /// 0 for Cyberware.
        /// </summary>
        public int BonusMatrixBoxes { get => 0; set { } }

        public int TotalBonusMatrixBoxes
        {
            get
            {
                int intBonusBoxes = 0;
                foreach (Gear objGear in Gear)
                {
                    if (objGear.Equipped)
                    {
                        intBonusBoxes += objGear.TotalBonusMatrixBoxes;
                    }
                }
                return intBonusBoxes;
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

        public IList<IHasMatrixAttributes> ChildrenWithMatrixAttributes
        {
            get
            {
                return IEnumerableExtensions.Both(Gear.Cast<IHasMatrixAttributes>(), Children.Cast<IHasMatrixAttributes>()).ToList();
            }
        }
        #endregion
    }
}
