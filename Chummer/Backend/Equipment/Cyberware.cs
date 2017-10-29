using Chummer.Backend.Extensions;
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
    /// A piece of Cyberware.
    /// </summary>
    public class Cyberware : INamedParentWithGuidAndNode<Cyberware>
    {
        private Guid _sourceID = new Guid();
        private Guid _guiID = new Guid();
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
        private int _intMinRating = 0;
        private int _intMaxRating = 0;
        private string _strAllowSubsystems = string.Empty;
        private bool _blnSuite = false;
        private string _strLocation = string.Empty;
        private string _strExtra = string.Empty;
        private Guid _guiWeaponID = new Guid();
        private Guid _guiVehicleID = new Guid();
        private Grade _objGrade = new Grade();
        private List<Cyberware> _objChildren = new List<Cyberware>();
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
        private bool _blnVehicleMounted = false;
        private bool _blnPrototypeTranshuman;
        private Cyberware _objParent;
        private bool _blnAddToParentESS = false;
        private string _strParentID = string.Empty;
        private string _strHasModularMount = string.Empty;
        private string _strPlugsIntoModularMount = string.Empty;
        private string _strBlocksMounts = string.Empty;
        private string _strForced = string.Empty;

        private readonly Character _objCharacter;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a Grade.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static Grade ConvertToCyberwareGrade(string strValue, Improvement.ImprovementSource objSource, CharacterOptions objCharacterOptions)
        {
            if (objSource == Improvement.ImprovementSource.Bioware)
            {
                GlobalOptions.BiowareGrades.LoadList(Improvement.ImprovementSource.Bioware, objCharacterOptions);
                foreach (Grade objGrade in GlobalOptions.BiowareGrades)
                {
                    if (objGrade.Name == strValue)
                        return objGrade;
                }

                return GlobalOptions.BiowareGrades.GetGrade("Standard");
            }
            else
            {
                GlobalOptions.CyberwareGrades.LoadList(Improvement.ImprovementSource.Cyberware, objCharacterOptions);
                foreach (Grade objGrade in GlobalOptions.CyberwareGrades)
                {
                    if (objGrade.Name == strValue)
                        return objGrade;
                }

                return GlobalOptions.CyberwareGrades.GetGrade("Standard");
            }
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
        public void Create(XmlNode objXmlCyberware, Character objCharacter, Grade objGrade, Improvement.ImprovementSource objSource, int intRating, TreeNode objNode, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, List<Vehicle> objVehicles, List<TreeNode> objVehicleNodes, bool blnCreateImprovements = true, bool blnCreateChildren = true, string strForced = "", Cyberware objParent = null, bool blnVehicleMounted = false)
        {
            Parent = objParent;
            _strForced = strForced;
            _blnVehicleMounted = blnVehicleMounted;
            objXmlCyberware.TryGetStringFieldQuickly("name", ref _strName);
            objXmlCyberware.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlCyberware.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
            objXmlCyberware.TryGetStringFieldQuickly("limbslotcount", ref _strLimbSlotCount);
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
            objXmlCyberware.TryGetField("id", Guid.TryParse, out _sourceID);
            objXmlCyberware.TryGetStringFieldQuickly("mountsto", ref _strPlugsIntoModularMount);
            objXmlCyberware.TryGetStringFieldQuickly("modularmount", ref _strHasModularMount);
            objXmlCyberware.TryGetStringFieldQuickly("blocksmounts", ref _strBlocksMounts);

            _objImprovementSource = objSource;
            if (objXmlCyberware["rating"] != null)
            {
                if (objXmlCyberware["rating"].InnerText == "MaximumSTR")
                {
                    _intMaxRating = _objCharacter.STR.TotalMaximum;
                }
                else if (objXmlCyberware["rating"].InnerText == "MaximumAGI")
                {
                    _intMaxRating = _objCharacter.AGI.TotalMaximum;
                }
                else
                {
                    int.TryParse(objXmlCyberware["rating"].InnerText, out _intMaxRating);
                }
            }

            if (objXmlCyberware["minrating"] != null)
            {
                if (objXmlCyberware["minrating"].InnerText == "MinimumSTR")
                {
                    _intMinRating = 3;
                }
                else if (objXmlCyberware["minrating"].InnerText == "MinimumAGI")
                {
                    _intMinRating = 3;
                }
                else
                {
                    try
                    {
                        _intMinRating = Convert.ToInt32(objXmlCyberware["minrating"].InnerText);
                    }
                    catch (FormatException)
                    {
                        if (_intMaxRating > 0)
                            _intMinRating = 1;
                    }
                }
            }

            _intRating = Math.Min(Math.Max(intRating, _intMinRating), _intMaxRating);

            objXmlCyberware.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);

            if (GlobalOptions.Language != "en-us")
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
                        frmSelectNumber frmPickNumber = new frmSelectNumber();
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
                    var objXmlWeapon = helpers.Guid.IsGuid(objXmlAddWeapon.InnerText)
                        ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + objXmlAddWeapon.InnerText + "\"]")
                        : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlAddWeapon.InnerText + "\"]");

                    TreeNode objGearWeaponNode = new TreeNode();
                    Weapon objGearWeapon = new Weapon(objCharacter);
                    objGearWeapon.Create(objXmlWeapon, objGearWeaponNode, null, null);
                    objGearWeapon.ParentID = InternalId;
                    objGearWeaponNode.ForeColor = SystemColors.GrayText;
                    objWeaponNodes.Add(objGearWeaponNode);
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
                    var objXmlVehicle = helpers.Guid.IsGuid(objXmlAddVehicle.InnerText)
                        ? objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[id = \"" + objXmlAddVehicle.InnerText + "\"]")
                        : objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + objXmlAddVehicle.InnerText + "\"]");

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
                    frmSelectSide frmPickSide = new frmSelectSide();
                    frmPickSide.Description = LanguageManager.GetString("Label_SelectSide").Replace("{0}", DisplayNameShort);
                    string strForcedSide = string.Empty;
                    if (_strForced == "Right" || _strForced == "Left")
                        strForcedSide = _strForced;
                    // TODO: Fix for modular mounts / banned mounts if someone has an amount of limbs different from the default amount
                    if (string.IsNullOrEmpty(strForcedSide) && !VehicleMounted)
                    {
                        List<Cyberware> lstCyberwareToCheck = Parent == null ? _objCharacter.Cyberware : Parent.Children;
                        if (!Shared_Methods.SelectionShared.RequirementsMet(objXmlCyberware, false, _objCharacter, null, null, null, string.Empty, string.Empty, string.Empty, "Left") ||
                            (!string.IsNullOrEmpty(BlocksMounts) && lstCyberwareToCheck.Any(x => !string.IsNullOrEmpty(x.HasModularMount) && x.Location == "Left" && BlocksMounts.Split(',').Contains(x.HasModularMount))) ||
                            (!string.IsNullOrEmpty(HasModularMount) && lstCyberwareToCheck.Any(x => !string.IsNullOrEmpty(x.BlocksMounts) && x.Location == "Left" && x.BlocksMounts.Split(',').Contains(HasModularMount))))
                            strForcedSide = "Right";
                        else if (!Shared_Methods.SelectionShared.RequirementsMet(objXmlCyberware, false, _objCharacter, null, null, null, string.Empty, string.Empty, string.Empty, "Right") ||
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

                    if (Bonus != null && !ImprovementManager.CreateImprovements(objCharacter, objSource, _guiID.ToString(), Bonus, false, _intRating, DisplayNameShort))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (WirelessBonus != null && WirelessOn && !ImprovementManager.CreateImprovements(objCharacter, objSource, _guiID.ToString(), WirelessBonus, false, _intRating, DisplayNameShort))
                    {
                        _guiID = Guid.Empty;
                        return;
                    }
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (PairBonus != null)
                    {
                        List<Cyberware> lstPairableCyberwares = objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Name == Name && x.Extra == Extra && x.IsModularCurrentlyEquipped).ToList();
                        bool blnAdd = lstPairableCyberwares.Count % 2 == 1;
                        if (!string.IsNullOrEmpty(Location))
                        {
                            blnAdd = Math.Min(lstPairableCyberwares.Count(x => x.Location == Location), lstPairableCyberwares.Count(x => x.Location != Location) - 1) % 2 == 1;
                        }
                        if (blnAdd && !ImprovementManager.CreateImprovements(objCharacter, objSource, _guiID.ToString(), PairBonus, false, _intRating, DisplayNameShort))
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
                    TreeNode objSubsystemNode = new TreeNode();
                    objSubsystemNode.Text = objSubsystem.DisplayName;
                    objSubsystemNode.Tag = objSubsystem.InternalId;
                    objSubsystemNode.ForeColor = SystemColors.GrayText;
                    objSubsystemNode.ContextMenuStrip = objParentTreeNode.ContextMenuStrip;
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
                    TreeNode objSubsystemNode = new TreeNode();
                    objSubsystemNode.Text = objSubsystem.DisplayName;
                    objSubsystemNode.Tag = objSubsystem.InternalId;
                    objSubsystemNode.ForeColor = SystemColors.GrayText;
                    objSubsystemNode.ContextMenuStrip = objParentTreeNode.ContextMenuStrip;
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
                    Gear objChild = null;
                    // Create the new piece of Gear.
                    TreeNode objChildNode = new TreeNode();
                    List<Weapon> objChildWeapons = new List<Weapon>();
                    List<TreeNode> objChildWeaponNodes = new List<TreeNode>();
                    if (!string.IsNullOrEmpty(objXmlChild["devicerating"]?.InnerText))
                    {
                        Commlink objCommlink = new Commlink(_objCharacter);
                        objCommlink.Create(objXmlGear, objChildNode, intChildRating, blnCreateImprovements, true, strChildForceValue);
                        objCommlink.Quantity = decChildQty;
                        objChildNode.Text = objCommlink.DisplayName;

                        objChild = objCommlink;
                    }
                    else
                    {
                        Gear objGear = new Gear(_objCharacter);
                        objGear.Create(objXmlGear, objChildNode, intChildRating, objChildWeapons, objChildWeaponNodes, strChildForceValue, false, false, blnCreateImprovements);
                        objGear.Quantity = decChildQty;
                        objChildNode.Text = objGear.DisplayName;

                        objChild = objGear;
                    }
                    objChild.Cost = "0";
                    objChild.IncludedInParent = true;
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
            objWriter.WriteElementString("minrating", _intMinRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(CultureInfo.InvariantCulture));
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
            objWriter.WriteElementString("vehiclemounted", _blnVehicleMounted.ToString());
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
                    // Use the Gear's SubClass if applicable.
                    if (objGear.GetType() == typeof(Commlink))
                    {
                        Commlink objCommlink = new Commlink(_objCharacter);
                        objCommlink = (Commlink)objGear;
                        objCommlink.Save(objWriter);
                    }
                    else
                    {
                        objGear.Save(objWriter);
                    }
                }
                objWriter.WriteEndElement();
            }
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteElementString("discountedcost", DiscountCost.ToString());
            objWriter.WriteElementString("addtoparentess", AddToParentESS.ToString());
            objWriter.WriteEndElement();
            _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode, bool blnCopy = false)
        {
            Improvement objImprovement = new Improvement();

            objNode.TryGetField("sourceid", Guid.TryParse, out _sourceID);
            if (blnCopy)
            {
                _guiID = Guid.NewGuid();
            }
            else
                objNode.TryGetField("guid", Guid.TryParse, out _guiID);

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            objNode.TryGetStringFieldQuickly("category", ref _strCategory);
            if (objNode["improvementsource"] != null)
                _objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
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
            objNode.TryGetInt32FieldQuickly("minrating", ref _intMinRating);
            objNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
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
            objNode.TryGetBoolFieldQuickly("vehiclemounted", ref _blnVehicleMounted);
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

            if (GlobalOptions.Language != "en-us")
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
                    if (nodChild["iscommlink"]?.InnerText == System.Boolean.TrueString || (nodChild["category"].InnerText == "Commlinks" ||
                        nodChild["category"].InnerText == "Commlink Accessories" || nodChild["category"].InnerText == "Cyberdecks" || nodChild["category"].InnerText == "Rigger Command Consoles"))
                    {
                        Gear objCommlink = new Commlink(_objCharacter);
                        objCommlink.Load(nodChild, blnCopy);
                        _lstGear.Add(objCommlink);
                    }
                    else
                    {
                        Gear objGear = new Gear(_objCharacter);
                        objGear.Load(nodChild, blnCopy);
                        _lstGear.Add(objGear);
                    }
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
            if (objNode["addtoparentess"] != null)
            {
                bool blnTmp;
                if (bool.TryParse(objNode["addtoparentess"].InnerText, out blnTmp))
                {
                    _blnAddToParentESS = blnTmp;
                }
            }
            else
                _blnAddToParentESS = MyXmlNode?["addtoparentess"] != null;
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>obv
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
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
            objWriter.WriteElementString("ess", CalculatedESS().ToString(GlobalOptions.CultureInfo));
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("avail", TotalAvail);
            objWriter.WriteElementString("cost", TotalCost.ToString());
            objWriter.WriteElementString("owncost", OwnCost.ToString());
            if (_objCharacter.Options != null)
            {
                objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
            }
            objWriter.WriteElementString("page", Page);
            objWriter.WriteElementString("rating", _intRating.ToString());
            objWriter.WriteElementString("minrating", _intMinRating.ToString());
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString());
            objWriter.WriteElementString("allowsubsystems", _strAllowSubsystems);
            objWriter.WriteElementString("wirelesson", _blnWirelessOn.ToString());
            objWriter.WriteElementString("grade", _objGrade.DisplayName);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            if (_lstGear.Count > 0)
            {
                objWriter.WriteStartElement("gears");
                foreach (Gear objGear in _lstGear)
                {
                    // Use the Gear's SubClass if applicable.
                    if (objGear.GetType() == typeof(Commlink))
                    {
                        Commlink objCommlink = objGear as Commlink;
                        objCommlink?.Print(objWriter);
                    }
                    else
                    {
                        objGear.Print(objWriter);
                    }
                }
                objWriter.WriteEndElement();
            }
            objWriter.WriteStartElement("children");
            foreach (Cyberware objChild in _objChildren)
            {
                objChild.Print(objWriter);
            }
            objWriter.WriteEndElement();
            if (_objCharacter.Options.PrintNotes)
                objWriter.WriteElementString("notes", _strNotes);
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

                if (_intRating > 0 && _sourceID != Guid.Parse("b57eadaa-7c3b-4b80-8d79-cbbd922c1196"))
                {
                    strReturn += " (" + LanguageManager.GetString("String_Rating") + " " + _intRating.ToString() + ")";
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
                int intReturn = 0;
                int.TryParse(_strLimbSlotCount, out intReturn);
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
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, Bonus, false, _intRating, DisplayNameShort);
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (WirelessBonus != null && WirelessOn)
                        ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, WirelessBonus, false, _intRating, DisplayNameShort);
                    if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue) && string.IsNullOrEmpty(_strExtra))
                        _strExtra = ImprovementManager.SelectedValue;

                    if (PairBonus != null)
                    {
                        List<Cyberware> lstPairableCyberwares = _objCharacter.Cyberware.DeepWhere(x => x.Children, x => x.Name == Name && x.Extra == Extra && x.IsModularCurrentlyEquipped).ToList();
                        bool blnAdd = lstPairableCyberwares.Count % 2 == 0;
                        if (!string.IsNullOrEmpty(Location))
                        {
                            blnAdd = Math.Min(lstPairableCyberwares.Count(x => x.Location == Location), lstPairableCyberwares.Count(x => x.Location != Location)) % 2 == 0;
                        }
                        if (blnAdd)
                        {
                            ImprovementManager.CreateImprovements(_objCharacter, SourceType, InternalId, PairBonus, false, _intRating, DisplayNameShort);
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
        /// Grade level of the Cyberware.
        /// </summary>
        public Grade Grade
        {
            get
            {
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
                int intGrade = 0;

                switch (_objGrade.Name)
                {
                    case "Standard":
                    case "Standard (Burnout's Way)":
                    case "Used":
                    case "Omegaware":
                        intGrade = 2;
                        break;
                    case "Alphaware":
                        intGrade = 3;
                        break;
                    case "Betaware":
                        intGrade = 4;
                        break;
                    case "Deltaware":
                        intGrade = 5;
                        break;
                    case "Gammaware":
                        intGrade = 6;
                        break;
                }
                return BaseMatrixBoxes + (intGrade + 1) / 2;
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
        public List<Cyberware> Children
        {
            get
            {
                return _objChildren;
            }
        }

        /// <summary>
        /// A List of the Gear attached to the Cyberware.
        /// </summary>
        public List<Gear> Gear
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
        /// Grade that the Cyberware should be forced to use, if applicable.
        /// </summary>
        public string ForceGrade
        {
            get
            {
                return _strForceGrade;
            }
        }

        public bool VehicleMounted
        {
            get
            {
                return _blnVehicleMounted;
            }
            set
            {
                _blnVehicleMounted = value;
            }
        }

        /// <summary>
        /// Is the bioware's cost affected by Prototype Transhuman?
        /// </summary>
        public bool PrototypeTranshuman
        {
            get { return _blnPrototypeTranshuman; }
            set { _blnPrototypeTranshuman = value; }
        }

        public XmlNode MyXmlNode
        {
            get
            {
                XmlNode objReturn = null;
                XmlDocument objDoc = null;
                if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    objDoc = XmlManager.Load("bioware.xml");
                    if (objDoc != null)
                    {
                        objReturn = objDoc.SelectSingleNode("/chummer/biowares/bioware[id = \"" + _sourceID.ToString() + "\" or id = \"" + _sourceID.ToString().ToUpperInvariant() + "\"]");
                        if (objReturn == null)
                        {
                            objReturn = objDoc.SelectSingleNode("/chummer/biowares/bioware[name = \"" + Name + "\"]");
                            if (objReturn != null)
                            {
                                objReturn.TryGetField("id", Guid.TryParse, out _sourceID);
                            }
                        }
                    }
                }
                else
                {
                    objDoc = XmlManager.Load("cyberware.xml");
                    if (objDoc != null)
                    {
                        objReturn = objDoc.SelectSingleNode("/chummer/cyberwares/cyberware[id = \"" + _sourceID.ToString() + "\" or id = \"" + _sourceID.ToString().ToUpperInvariant() + "\"]");
                        if (objReturn == null)
                        {
                            objReturn = objDoc.SelectSingleNode("/chummer/cyberwares/cyberware[name = \"" + Name + "\"]");
                            if (objReturn != null)
                            {
                                objReturn.TryGetField("id", Guid.TryParse, out _sourceID);
                            }
                        }
                    }
                }
                return objReturn;
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
                XmlDocument objXmlDocument = new XmlDocument();
                XPathNavigator nav = objXmlDocument.CreateNavigator();
                // If the Avail starts with "+", return the base string and don't try to calculate anything since we're looking at a child component.
                if (_strAvail.StartsWith("+"))
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
                        strAvailExpr = strAvailExpr.Replace("MinRating", _intMinRating.ToString());
                        strAvailExpr = strAvailExpr.Replace("Rating", _intRating.ToString());
                        XPathExpression xprAvail = nav.Compile(strAvailExpr);
                        return "+" + nav.Evaluate(xprAvail).ToString() + strAvail;
                    }
                    else
                        return _strAvail;
                }

                string strBaseAvail = _strAvail;
                if (strBaseAvail.StartsWith("FixedValues"))
                {
                    string[] strValues = strBaseAvail.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    strBaseAvail = strValues[Math.Min(_intRating, strValues.Length) - 1];
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
                    XPathExpression xprAvail = nav.Compile(strAvailExpr.Replace("Rating", _intRating.ToString()));
                    strCalculated = (Convert.ToInt32(nav.Evaluate(xprAvail)) + intAvailModifier).ToString() + strAvail;
                }
                else
                {
                    // Just a straight cost, so return the value.
                    if (strBaseAvail.EndsWith("F") || strBaseAvail.EndsWith("R"))
                    {
                        strCalculated = (Convert.ToInt32(strBaseAvail.Substring(0, strBaseAvail.Length - 1)) + intAvailModifier).ToString() + strBaseAvail.Substring(strBaseAvail.Length - 1, 1);
                    }
                    else
                        strCalculated = (Convert.ToInt32(strBaseAvail) + intAvailModifier).ToString();
                }

                int intAvail = 0;
                string strAvailText = string.Empty;
                if (strCalculated.EndsWith("F") || strCalculated.EndsWith("R"))
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
                            strChildAvail = strChildAvail.Replace("MinRating", objChild.MinRating.ToString());
                            strChildAvail = strChildAvail.Replace("Rating", objChild.Rating.ToString());
                            string strChildAvailText = string.Empty;
                            if (strChildAvail.EndsWith("R") || strChildAvail.EndsWith("F"))
                            {
                                strChildAvailText = strChildAvail.Substring(objChild.Avail.Length - 1);
                                strChildAvail = strChildAvail.Substring(0, strChildAvail.Length - 1);
                            }

                            // If the availability is determined by the Rating, evaluate the expression.
                            string strChildAvailExpr = strChildAvail;

                            // Remove the "+" since the expression can't be evaluated if it starts with this.
                            XPathExpression xprAvail = nav.Compile(strChildAvailExpr.FastEscape('+'));
                            strChildAvail = "+" + nav.Evaluate(xprAvail);
                            if (!string.IsNullOrEmpty(strChildAvailText))
                                strChildAvail += strChildAvailText;
                        }

                        if (strChildAvail.EndsWith("R") || strChildAvail.EndsWith("F"))
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
                        if (strLoopAvail.EndsWith("R") || strLoopAvail.EndsWith("F"))
                        {
                            if (strAvailText != "F" && strLoopAvail.EndsWith("F"))
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
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    int intPos = strCapacity.IndexOf("/[");
                    string strFirstHalf = strCapacity.Substring(0, intPos);
                    string strSecondHalf = strCapacity.Substring(intPos + 1, _strCapacity.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.Contains('['); ;

                    if (blnSquareBrackets && strFirstHalf.Length > 2)
                        strFirstHalf = strFirstHalf.Substring(1, strFirstHalf.Length - 2);
                    XPathExpression xprCapacity = nav.Compile(strFirstHalf.Replace("Rating", _intRating.ToString()));

                    try
                    {
                        strReturn = Convert.ToDecimal(nav.Evaluate(xprCapacity)).ToString("N2", GlobalOptions.CultureInfo);
                    }
                    catch (XPathException)
                    {
                        strReturn = "0";
                    }
                    if (blnSquareBrackets)
                        strReturn = "[" + strCapacity + "]";

                    strSecondHalf = strSecondHalf.Trim("[]".ToArray());
                    xprCapacity = nav.Compile(strSecondHalf.Replace("Rating", _intRating.ToString()));
                    strSecondHalf = "[" + Convert.ToDecimal(nav.Evaluate(xprCapacity)).ToString("N2", GlobalOptions.CultureInfo) + "]";

                    strReturn += "/" + strSecondHalf;
                }
                else if (strCapacity.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = strCapacity.Contains('[');
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

                    strReturn = Convert.ToDecimal(nav.Evaluate(xprCapacity)).ToString("N2", GlobalOptions.CultureInfo);
                    if (blnSquareBrackets)
                        strReturn = "[" + strReturn + "]";
                }
                else
                {
                    // Just a straight Capacity, so return the value.
                    strReturn = _strCapacity;
                }
                decimal decReturn;
                if (decimal.TryParse(strReturn, out decReturn))
                    return decReturn.ToString("N2", GlobalOptions.CultureInfo);
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
                strESS = strValues[Math.Min(_intRating, strValues.Length) - 1];
            }
            if (strESS.Contains("Rating"))
            {
                // If the cost is determined by the Rating, evaluate the expression.
                XmlDocument objXmlDocument = new XmlDocument();
                XPathNavigator nav = objXmlDocument.CreateNavigator();

                strESS = strESS.Replace("Rating", _intRating.ToString());

                XPathExpression xprEss = nav.Compile(strESS);
                decReturn = Convert.ToDecimal(nav.Evaluate(strESS), GlobalOptions.InvariantCultureInfo);
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
            if (_strForceGrade == "None")
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

            if (_objCharacter != null)
                decReturn = Math.Round(decReturn, _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
            decReturn += _objChildren.Where(objChild => objChild.AddToParentESS).Sum(objChild => objChild.CalculatedESS());
            return decReturn;
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
                    if (_intRating > 0)
                        strCostExpression = strValues[Math.Min(_intRating, strValues.Length) - 1].Trim("[]".ToArray());
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
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    if (string.IsNullOrEmpty(strParentCost))
                        strParentCost = "0";
                    strCostExpression = strCostExpression.Replace("Parent Cost", strParentCost);
                    strCostExpression = strCostExpression.Replace("Parent Gear Cost", decTotalParentGearCost.ToString(GlobalOptions.InvariantCultureInfo));
                    strCostExpression = strCostExpression.Replace("Gear Cost", decTotalGearCost.ToString(GlobalOptions.InvariantCultureInfo));
                    strCostExpression = strCostExpression.Replace("Children Cost", decTotalChildrenCost.ToString(GlobalOptions.InvariantCultureInfo));
                    strCostExpression = strCostExpression.Replace("MinRating", _intMinRating.ToString());
                    strCostExpression = strCostExpression.Replace("Rating", _intRating.ToString());
                    XPathExpression xprCost = nav.Compile(strCostExpression);
                    decCost = Convert.ToDecimal(nav.Evaluate(xprCost).ToString(), GlobalOptions.InvariantCultureInfo);
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
                        if (objChild.Cost.StartsWith("*"))
                        {
                            decimal decPluginCost = 0;
                            string strMultiplier = objChild.Cost;
                            strMultiplier = strMultiplier.FastEscape('*');
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

                // Base Strength for any limb is 3.
                int intAttribute = 3;
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
                if (!VehicleMounted)
                {
                    return Math.Min(intAttribute + intBonus + _objCharacter.RedlinerBonus, _objCharacter.STR.TotalAugmentedMaximum);
                }
                Vehicle objParentVehicle = CommonFunctions.FindByIdWithNameCheck(_objParent.InternalId, _objCharacter.Vehicles);
                return Math.Min(intAttribute + intBonus, objParentVehicle.TotalBody * 2);
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

                // Base Strength for any limb is 3.
                int intAttribute = 3;
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

                if (!VehicleMounted)
                {
                    return Math.Min(intAttribute + intBonus + _objCharacter.RedlinerBonus, _objCharacter.AGI.TotalAugmentedMaximum);
                }
                Vehicle objParentVehicle = CommonFunctions.FindByIdWithNameCheck(_objParent.InternalId, _objCharacter.Vehicles);
                return Math.Min(intAttribute + intBonus, objParentVehicle.Pilot*2);
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
        #endregion
    }
}
