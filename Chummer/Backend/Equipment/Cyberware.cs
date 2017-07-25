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
    public class Cyberware : INamedParentWithGuid<Cyberware>
    {
        private Guid _sourceID = new Guid();
        private Guid _guiID = new Guid();
        private string _strName = string.Empty;
        private string _strCategory = string.Empty;
        private string _strLimbSlot = string.Empty;
        private int _intLimbSlotCount = 1;
        private bool _blnInheritAttributes = false;
        private string _strESS = string.Empty;
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
        private Guid _guiWeaponID = new Guid();
        private Guid _guiVehicleID = new Guid();
        private Grade _objGrade = new Grade();
        private List<Cyberware> _objChildren = new List<Cyberware>();
        private List<Gear> _lstGear = new List<Gear>();
        private XmlNode _nodBonus;
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

        private readonly Character _objCharacter;

        #region Helper Methods
        /// <summary>
        /// Convert a string to a Grade.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public Grade ConvertToCyberwareGrade(string strValue, Improvement.ImprovementSource objSource)
        {
            if (objSource == Improvement.ImprovementSource.Bioware)
            {
                foreach (Grade objGrade in GlobalOptions.BiowareGrades)
                {
                    if (objGrade.Name == strValue)
                        return objGrade;
                }

                return GlobalOptions.BiowareGrades.GetGrade("Standard");
            }
            else
            {
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
        public void Create(XmlNode objXmlCyberware, Character objCharacter, Grade objGrade, Improvement.ImprovementSource objSource, int intRating, TreeNode objNode, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes, List<Vehicle> objVehicles, List<TreeNode> objVehicleNodes, bool blnCreateImprovements = true, bool blnCreateChildren = true, string strForced = "")
        {
            objXmlCyberware.TryGetStringFieldQuickly("name", ref _strName);
            objXmlCyberware.TryGetStringFieldQuickly("category", ref _strCategory);
            objXmlCyberware.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
            objXmlCyberware.TryGetInt32FieldQuickly("limbslotcount", ref _intLimbSlotCount);
            if (objXmlCyberware["inheritattributes"] != null)
                _blnInheritAttributes = true;
            _objGrade = objGrade;
            objXmlCyberware.TryGetStringFieldQuickly("ess", ref _strESS);
            objXmlCyberware.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objXmlCyberware.TryGetStringFieldQuickly("avail", ref _strAvail);
            objXmlCyberware.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlCyberware.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objXmlCyberware["bonus"];
            _nodAllowGear = objXmlCyberware["allowgear"];
            objXmlCyberware.TryGetField("id", Guid.TryParse, out _sourceID);

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

            if (GlobalOptions.Instance.Language != "en-us")
            {
                string strXmlFile = string.Empty;
                string strXPath = string.Empty;
                if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    strXmlFile = "bioware.xml";
                    strXPath = "/chummer/biowares/bioware";
                }
                else
                {
                    strXmlFile = "cyberware.xml";
                    strXPath = "/chummer/cyberwares/cyberware";
                }
                XmlDocument objXmlDocument = XmlManager.Instance.Load(strXmlFile);
                XmlNode objCyberwareNode = objXmlDocument.SelectSingleNode(strXPath + "[name = \"" + _strName + "\"]");
                if (objCyberwareNode != null)
                {
                    objCyberwareNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objCyberwareNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

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
                    int intMin = 0;
                    int intMax = 0;
                    char[] chrParentheses = { '(', ')' };
                    string strCost = objXmlCyberware["cost"].InnerText.Replace("Variable", string.Empty).Trim(chrParentheses);
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
                        frmSelectNumber frmPickNumber = new frmSelectNumber();
                        if (intMax == 0)
                            intMax = 1000000;
                        frmPickNumber.Minimum = intMin;
                        frmPickNumber.Maximum = intMax;
                        frmPickNumber.Description = LanguageManager.Instance.GetString("String_SelectVariableCost").Replace("{0}", DisplayNameShort);
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
                XmlDocument objXmlWeaponDocument = XmlManager.Instance.Load("weapons.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddWeapon in objXmlCyberware.SelectNodes("addweapon"))
                {
                    var objXmlWeapon = helpers.Guid.IsGuid(objXmlAddWeapon.InnerText)
                        ? objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[id = \"" + objXmlAddWeapon.InnerText + "\"]")
                        : objXmlWeaponDocument.SelectSingleNode("/chummer/weapons/weapon[name = \"" + objXmlAddWeapon.InnerText + "\"]");

                    TreeNode objGearWeaponNode = new TreeNode();
                    Weapon objGearWeapon = new Weapon(objCharacter);
                    objGearWeapon.Create(objXmlWeapon, objCharacter, objGearWeaponNode, null, null);
                    objGearWeaponNode.ForeColor = SystemColors.GrayText;
                    objWeaponNodes.Add(objGearWeaponNode);
                    objWeapons.Add(objGearWeapon);

                    _guiWeaponID = Guid.Parse(objGearWeapon.InternalId);
                }
            }

            // Add Drone Bodyparts if applicable.
            if (objXmlCyberware.InnerXml.Contains("<addvehicle>"))
            {
                XmlDocument objXmlVehicleDocument = XmlManager.Instance.Load("vehicles.xml");

                // More than one Weapon can be added, so loop through all occurrences.
                foreach (XmlNode objXmlAddVehicle in objXmlCyberware.SelectNodes("addvehicle"))
                {
                    var objXmlVehicle = helpers.Guid.IsGuid(objXmlAddVehicle.InnerText)
                        ? objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[id = \"" + objXmlAddVehicle.InnerText + "\"]")
                        : objXmlVehicleDocument.SelectSingleNode("/chummer/vehicles/vehicle[name = \"" + objXmlAddVehicle.InnerText + "\"]");

                    TreeNode objVehicleNode = new TreeNode();
                    Vehicle objVehicle = new Vehicle(_objCharacter);
                    objVehicle.Create(objXmlVehicle, objVehicleNode, null, null, null, null);
                    objVehicleNode.ForeColor = SystemColors.GrayText;
                    objVehicleNodes.Add(objVehicleNode);
                    objVehicles.Add(objVehicle);

                    _guiVehicleID = Guid.Parse(objVehicle.InternalId);
                }
            }

            // If the piece grants a bonus, pass the information to the Improvement Manager.
            if (objXmlCyberware["bonus"] != null && blnCreateImprovements)
            {
                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                if (!string.IsNullOrEmpty(strForced))
                    objImprovementManager.ForcedValue = strForced;

                if (!objImprovementManager.CreateImprovements(objSource, _guiID.ToString(), _nodBonus, false, _intRating, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(objImprovementManager.SelectedValue))
                    _strLocation = objImprovementManager.SelectedValue;
            }

            // Create the TreeNode for the new item.
            objNode.Text = DisplayName;
            objNode.Tag = _guiID.ToString();

            // If we've just added a new base item, see if there are any subsystems that should automatically be added.
            if (objXmlCyberware.InnerXml.Contains("subsystems") && blnCreateChildren)
            {
                XmlDocument objXmlDocument;
                if (objSource == Improvement.ImprovementSource.Bioware)
                    objXmlDocument = XmlManager.Instance.Load("bioware.xml");
                else
                    objXmlDocument = XmlManager.Instance.Load("cyberware.xml");

                XmlNodeList objXmlSubSystemNameList = objXmlCyberware.SelectNodes("subsystems/subsystem");
                XmlNode objXmlSubsystem;

                foreach (XmlNode objXmlSubsystemName in objXmlSubSystemNameList)
                {
                    if (objSource == Improvement.ImprovementSource.Bioware)
                        objXmlSubsystem = objXmlDocument.SelectSingleNode("/chummer/biowares/bioware[name = \"" + objXmlSubsystemName.InnerText + "\"]");
                    else
                        objXmlSubsystem = objXmlDocument.SelectSingleNode("/chummer/cyberwares/cyberware[name = \"" + objXmlSubsystemName.InnerText + "\"]");

                    Cyberware objSubsystem = new Cyberware(objCharacter);
                    TreeNode objSubsystemNode = new TreeNode();
                    objSubsystemNode.Text = objSubsystem.DisplayName;
                    objSubsystemNode.Tag = objSubsystem.InternalId;
                    objSubsystemNode.ForeColor = SystemColors.GrayText;
                    objSubsystemNode.ContextMenuStrip = objNode.ContextMenuStrip;
                    int intSubSystemRating = Convert.ToInt32(objXmlSubsystemName.Attributes?["rating"]?.InnerText);
                    objSubsystem.Create(objXmlSubsystem, _objCharacter, objGrade, objSource, intSubSystemRating, objSubsystemNode, objWeapons, objWeaponNodes, objVehicles, objVehicleNodes, blnCreateImprovements, blnCreateChildren, objXmlSubsystemName["forced"] != null ? objXmlSubsystemName["forced"].InnerText : string.Empty);

                    objSubsystem.Parent = this;
                    objSubsystem.Cost = "0";

                    _objChildren.Add(objSubsystem);

                    objNode.Nodes.Add(objSubsystemNode);
                    objNode.Expand();
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
            objWriter.WriteElementString("limbslotcount", _intLimbSlotCount.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("inheritattributes", _blnInheritAttributes.ToString());
            objWriter.WriteElementString("ess", _strESS);
            objWriter.WriteElementString("capacity", _strCapacity);
            objWriter.WriteElementString("avail", _strAvail);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("rating", _intRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("minrating", _intMinRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("maxrating", _intMaxRating.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("subsystems", _strAllowSubsystems);
            objWriter.WriteElementString("grade", _objGrade.Name);
            objWriter.WriteElementString("location", _strLocation);
            objWriter.WriteElementString("suite", _blnSuite.ToString());
            objWriter.WriteElementString("essdiscount", _intEssenceDiscount.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("forcegrade", _strForceGrade);
            objWriter.WriteElementString("matrixcmfilled", _intMatrixCMFilled.ToString(CultureInfo.InvariantCulture));
            objWriter.WriteElementString("vehiclemounted", _blnVehicleMounted.ToString());
            objWriter.WriteElementString("prototypetranshuman", _blnPrototypeTranshuman.ToString());
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
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
            objNode.TryGetInt32FieldQuickly("matrixcmfilled", ref _intMatrixCMFilled);
            objNode.TryGetStringFieldQuickly("limbslot", ref _strLimbSlot);
            objNode.TryGetInt32FieldQuickly("limbslotcount", ref _intLimbSlotCount);
            objNode.TryGetBoolFieldQuickly("inheritattributes", ref _blnInheritAttributes);
            objNode.TryGetStringFieldQuickly("ess", ref _strESS);
            objNode.TryGetStringFieldQuickly("capacity", ref _strCapacity);
            objNode.TryGetStringFieldQuickly("avail", ref _strAvail);
            objNode.TryGetStringFieldQuickly("cost", ref _strCost);
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);

            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetInt32FieldQuickly("minrating", ref _intMinRating);
            objNode.TryGetInt32FieldQuickly("maxrating", ref _intMaxRating);
            objNode.TryGetStringFieldQuickly("subsystems", ref _strAllowSubsystems);
            if (objNode["grade"] != null)
                _objGrade = ConvertToCyberwareGrade(objNode["grade"].InnerText, _objImprovementSource);
            objNode.TryGetStringFieldQuickly("location", ref _strLocation);
            objNode.TryGetBoolFieldQuickly("suite", ref _blnSuite);
            objNode.TryGetInt32FieldQuickly("essdiscount", ref _intEssenceDiscount);
            objNode.TryGetStringFieldQuickly("forcegrade", ref _strForceGrade);
            objNode.TryGetBoolFieldQuickly("vehiclemounted", ref _blnVehicleMounted);
            objNode.TryGetBoolFieldQuickly("prototypetranshuman", ref _blnPrototypeTranshuman);
            _nodBonus = objNode["bonus"];
            _nodAllowGear = objNode["allowgear"];
            if (objNode["improvementsource"] != null)
                _objImprovementSource = objImprovement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);
            if (objNode["weaponguid"] != null)
            {
                _guiWeaponID = Guid.Parse(objNode["weaponguid"].InnerText);
            }
            if (objNode["vehicleguid"] != null)
            {
                _guiVehicleID = Guid.Parse(objNode["vehicleguid"].InnerText);
            }

            if (GlobalOptions.Instance.Language != "en-us")
            {
                string strXmlFile = string.Empty;
                string strXPath = string.Empty;
                if (_objImprovementSource == Improvement.ImprovementSource.Bioware)
                {
                    strXmlFile = "bioware.xml";
                    strXPath = "/chummer/biowares/bioware";
                }
                else
                {
                    strXmlFile = "cyberware.xml";
                    strXPath = "/chummer/cyberwares/cyberware";
                }
                XmlDocument objXmlDocument = XmlManager.Instance.Load(strXmlFile);
                XmlNode objCyberwareNode = objXmlDocument.SelectSingleNode(strXPath + "[name = \"" + _strName + "\"]");
                if (objCyberwareNode != null)
                {
                    objCyberwareNode.TryGetStringFieldQuickly("translate", ref _strAltName);
                    objCyberwareNode.TryGetStringFieldQuickly("altpage", ref _strAltPage);
                }

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
                    switch (nodChild["category"]?.InnerText)
                    {
                        case "Commlinks":
                        case "Commlink Accessories":
                        case "Cyberdecks":
                        case "Rigger Command Consoles":
                            Commlink objCommlink = new Commlink(_objCharacter);
                            objCommlink.Load(nodChild, blnCopy);
                            _lstGear.Add(objCommlink);
                            break;
                        default:
                            Gear objGear = new Gear(_objCharacter);
                            objGear.Load(nodChild, blnCopy);
                            _lstGear.Add(objGear);
                            break;
                    }
                }
            }

            objNode.TryGetStringFieldQuickly("notes", ref _strNotes);
            objNode.TryGetBoolFieldQuickly("discountedcost", ref _blnDiscountCost);
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
                objWriter.WriteElementString("name", DisplayNameShort + " (" + _objCharacter.AGI.DisplayAbbrev + " " + TotalAgility + ", " + _objCharacter.STR.DisplayAbbrev + " " + TotalStrength + ", " + LanguageManager.Instance.GetString("String_LimitPhysicalShort") + " " + intLimit.ToString() + ")");
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
            objWriter.WriteElementString("grade", _objGrade.DisplayName);
            objWriter.WriteElementString("location", _strLocation);
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
                    strReturn += " (" + LanguageManager.Instance.GetString("String_Rating") + " " + _intRating.ToString() + ")";
                }

                if (!string.IsNullOrEmpty(_strLocation))
                {
                    LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
                    // Attempt to retrieve the CharacterAttribute name.
                    strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strLocation) + ")";
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
                return _intLimbSlotCount;
            }
            set
            {
                _intLimbSlotCount = value;
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
        /// The location of a Cyberlimb.
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
                if (_strAvail.StartsWith("+"))
                {
                    if (_strAvail.Contains("Rating") || _strAvail.Contains("MinRating"))
                    {
                        // If the availability is determined by the Rating, evaluate the expression.
                        XmlDocument objXmlDocument = new XmlDocument();
                        XPathNavigator nav = objXmlDocument.CreateNavigator();

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

                string strCalculated = string.Empty;
                string strReturn = string.Empty;

                // Second Hand Cyberware has a reduced Availability.
                int intAvailModifier = 0;

                // Apply the Grade's Avail modifier.
                intAvailModifier = Grade.Avail;

                if (_strAvail.Contains("Rating"))
                {
                    // If the availability is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    string strAvail = string.Empty;
                    string strAvailExpr = _strAvail;

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
                    if (_strAvail.StartsWith("FixedValues"))
                    {
                        string[] strValues = _strAvail.Replace("FixedValues(", string.Empty).Replace(")", string.Empty).Split(',');
                        string strAvail = strValues[Convert.ToInt32(_intRating) - 1];
                        if (strAvail.EndsWith("F") || strAvail.EndsWith("R"))
                        {
                            string strAvailSuffix = strAvail.Substring(strAvail.Length - 1, 1);
                            strAvail = strAvail.Substring(0, strAvail.Length - 1);
                            int intAvailFix = Convert.ToInt32(strAvail) + intAvailModifier;
                            strCalculated = intAvailFix.ToString() + strAvailSuffix;
                        }
                        else
                        {
                            int intAvailFix = Convert.ToInt32(strAvail) + intAvailModifier;
                            strCalculated = intAvailFix.ToString();
                        }
                    }
                    else
                    {
                        // Just a straight cost, so return the value.
                        if (_strAvail.Contains("F") || _strAvail.Contains("R"))
                        {
                            strCalculated = (Convert.ToInt32(_strAvail.Substring(0, _strAvail.Length - 1)) + intAvailModifier).ToString() + _strAvail.Substring(_strAvail.Length - 1, 1);
                        }
                        else
                            strCalculated = (Convert.ToInt32(_strAvail) + intAvailModifier).ToString();
                    }
                }

                int intAvail = 0;
                string strAvailText = string.Empty;
                if (strCalculated.Contains("F") || strCalculated.Contains("R"))
                {
                    strAvailText = strCalculated.Substring(strCalculated.Length - 1);
                    intAvail = Convert.ToInt32(strCalculated.Replace(strAvailText, string.Empty));
                }
                else
                    intAvail = Convert.ToInt32(strCalculated);

                // Run through the child items and increase the Avail by any Mod whose Avail contains "+".
                foreach (Cyberware objChild in _objChildren)
                {
                    if (objChild.Avail.Contains("+"))
                    {
                        string strChildAvail = objChild.Avail;
                        if (objChild.Avail.Contains("Rating") || objChild.Avail.Contains("MinRating"))
                        {
                            strChildAvail = strChildAvail.Replace("MinRating", objChild.MinRating.ToString());
                            strChildAvail = strChildAvail.Replace("Rating", objChild.Rating.ToString());
                            string strChildAvailText = string.Empty;
                            if (strChildAvail.Contains("R") || strChildAvail.Contains("F"))
                            {
                                strChildAvailText = strChildAvail.Substring(objChild.Avail.Length - 1);
                                strChildAvail = strChildAvail.Replace(strChildAvailText, string.Empty);
                            }

                            // If the availability is determined by the Rating, evaluate the expression.
                            XmlDocument objXmlDocument = new XmlDocument();
                            XPathNavigator nav = objXmlDocument.CreateNavigator();

                            string strChildAvailExpr = strChildAvail;

                            // Remove the "+" since the expression can't be evaluated if it starts with this.
                            XPathExpression xprAvail = nav.Compile(strChildAvailExpr.Replace("+", string.Empty));
                            strChildAvail = "+" + nav.Evaluate(xprAvail);
                            if (!string.IsNullOrEmpty(strChildAvailText))
                                strChildAvail += strChildAvailText;
                        }

                        if (strChildAvail.Contains("R") || strChildAvail.Contains("F"))
                        {
                            if (strAvailText != "F")
                                strAvailText = strChildAvail.Substring(objChild.Avail.Length - 1);
                            intAvail += Convert.ToInt32(strChildAvail.Replace("F", string.Empty).Replace("R", string.Empty));
                        }
                        else
                            intAvail += Convert.ToInt32(strChildAvail);
                    }
                }

                // Avail cannot go below 0. This typically happens when an item with Avail 0 is given the Second Hand category.
                if (intAvail < 0)
                    intAvail = 0;

                strReturn = intAvail.ToString() + strAvailText;

                // Translate the Avail string.
                strReturn = strReturn.Replace("R", LanguageManager.Instance.GetString("String_AvailRestricted"));
                strReturn = strReturn.Replace("F", LanguageManager.Instance.GetString("String_AvailForbidden"));

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
                string strReturn = "0";
                if (!string.IsNullOrEmpty(_strCapacity) && _strCapacity.Contains("/["))
                {
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    int intPos = _strCapacity.IndexOf("/[");
                    string strFirstHalf = _strCapacity.Substring(0, intPos);
                    string strSecondHalf = _strCapacity.Substring(intPos + 1, _strCapacity.Length - intPos - 1);
                    bool blnSquareBrackets = strFirstHalf.Contains('['); ;
                    string strCapacity = strFirstHalf;

                    if (blnSquareBrackets && strCapacity.Length > 2)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

                    if (_strCapacity == "[*]")
                        strReturn = "*";
                    else
                    {
                        if (_strCapacity.StartsWith("FixedValues"))
                        {
                            char[] chrParentheses = { '(', ')' };
                            string[] strValues = _strCapacity.Replace("FixedValues", string.Empty).Trim(chrParentheses).Split(',');
                            if (_intRating <= strValues.Length)
                                strReturn = strValues[_intRating - 1];
                            else
                                strReturn = "0";
                        }
                        else
                        {
                            try
                            {
                                strReturn = nav.Evaluate(xprCapacity).ToString();
                            }
                            catch (XPathException)
                            {
                                strReturn = "0";
                            }
                        }
                    }
                    if (blnSquareBrackets)
                        strReturn = "[" + strCapacity + "]";

                    if (strSecondHalf.Contains("Rating"))
                    {
                        strSecondHalf = strSecondHalf.Replace("[", string.Empty).Replace("]", string.Empty);
                        xprCapacity = nav.Compile(strSecondHalf.Replace("Rating", _intRating.ToString()));
                        strSecondHalf = "[" + nav.Evaluate(xprCapacity).ToString() + "]";
                    }

                    strReturn += "/" + strSecondHalf;
                }
                else if (_strCapacity.Contains("Rating"))
                {
                    // If the Capaicty is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    // XPathExpression cannot evaluate while there are square brackets, so remove them if necessary.
                    bool blnSquareBrackets = _strCapacity.Contains('[');
                    string strCapacity = _strCapacity;
                    if (blnSquareBrackets)
                        strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                    XPathExpression xprCapacity = nav.Compile(strCapacity.Replace("Rating", _intRating.ToString()));

                    strReturn = nav.Evaluate(xprCapacity).ToString();
                    if (blnSquareBrackets)
                        strReturn = "[" + strReturn + "]";
                }
                else
                {
                    if (_strCapacity.StartsWith("FixedValues"))
                    {
                        string[] strValues = _strCapacity.Replace("FixedValues", string.Empty).Trim("()".ToCharArray()).Split(',');
                        if (strValues.Length >= _intRating)
                            strReturn = strValues[_intRating - 1];
                    }
                    else
                    {
                        // Just a straight Capacity, so return the value.
                        strReturn = _strCapacity;
                    }
                }
                if (string.IsNullOrEmpty(strReturn))
                    strReturn = "0";
                return strReturn;
            }
        }

        /// <summary>
        /// Calculated Essence cost of the Cyberware.
        /// </summary>
        public decimal CalculatedESS(bool returnPrototype = true)
        {
            if (_blnPrototypeTranshuman && returnPrototype) return 0;
            if (SourceID == Guid.Parse("b57eadaa-7c3b-4b80-8d79-cbbd922c1196")) //Essence hole
            {
                return Convert.ToDecimal(Rating, GlobalOptions.InvariantCultureInfo) / 100m;
            }

            decimal decReturn = 0;

            if (_strESS.Contains("Rating"))
            {
                // If the cost is determined by the Rating, evaluate the expression.
                XmlDocument objXmlDocument = new XmlDocument();
                XPathNavigator nav = objXmlDocument.CreateNavigator();

                string strEss = _strESS.Replace("Rating", _intRating.ToString());

                XPathExpression xprEss = nav.Compile(strEss);
                decReturn = Convert.ToDecimal(nav.Evaluate(xprEss), GlobalOptions.InvariantCultureInfo);
            }
            else
            {
                if (_strESS.StartsWith("FixedValues"))
                {
                    string[] strValues = _strESS.Replace("FixedValues", string.Empty).Trim("()".ToCharArray()).Split(',');
                    decimal.TryParse(strValues[_intRating - 1], NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decReturn);
                }
                else
                {
                    // Just a straight cost, so return the value.
                    decimal.TryParse(_strESS, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out decReturn);
                }
            }

            // Factor in the Essence multiplier of the selected CyberwareGrade.
            decimal decESSMultiplier = Grade.Essence;

            if (_blnSuite)
                decESSMultiplier -= 0.1m;

            if (_intEssenceDiscount != 0)
            {
                decimal decDiscount = Convert.ToDecimal(_intEssenceDiscount, GlobalOptions.InvariantCultureInfo) * 0.01m;
                decESSMultiplier *= 1.0m - decDiscount;
            }

            ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);

            // Retrieve the Bioware or Cyberware ESS Cost Multiplier. Bioware Modifiers do not apply to Genetech.
            if (!_strCategory.StartsWith("Genetech") && !_strCategory.StartsWith("Genetic Infusions") &&
                !_strCategory.StartsWith("Genemods"))
            {
                decimal decMultiplier = 1;
                // Apply the character's Cyberware Essence cost multiplier if applicable.
                if (_objImprovementSource == Improvement.ImprovementSource.Cyberware && objImprovementManager.ValueOf(Improvement.ImprovementType.CyberwareEssCost) != 0)
                {
                    decMultiplier = _objCharacter.Improvements
                        .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.CyberwareEssCost && objImprovement.Enabled)
                        .Aggregate(decMultiplier, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                    decESSMultiplier -= 1.0m - decMultiplier;
                }

                // Apply the character's Bioware Essence cost multiplier if applicable.
                else if (_objImprovementSource == Improvement.ImprovementSource.Bioware && objImprovementManager.ValueOf(Improvement.ImprovementType.BiowareEssCost) != 0)
                {
                    decMultiplier = _objCharacter.Improvements
                        .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BiowareEssCost && objImprovement.Enabled)
                        .Aggregate(decMultiplier, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                    decESSMultiplier -= 1.0m - decMultiplier;
                }
            }

            // Apply the character's Basic Bioware Essence cost multiplier if applicable.
            if (_strCategory == "Basic" && _objImprovementSource == Improvement.ImprovementSource.Bioware && objImprovementManager.ValueOf(Improvement.ImprovementType.BasicBiowareEssCost) != 0)
            {
                decimal decBasicMultiplier = _objCharacter.Improvements
                    .Where(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.BasicBiowareEssCost && objImprovement.Enabled)
                    .Aggregate<Improvement, decimal>(1, (current, objImprovement) => current - (1m - Convert.ToDecimal(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100m));
                decESSMultiplier -= 1.0m - decBasicMultiplier;
            }
            decReturn = decReturn * decESSMultiplier;

            // Check if the character has Sensitive System.
            if (_objImprovementSource == Improvement.ImprovementSource.Cyberware && _objCharacter != null)
            {
                if (_objCharacter.Improvements.Any(objImprovement => objImprovement.ImproveType == Improvement.ImprovementType.SensitiveSystem && objImprovement.Enabled))
                {
                    decReturn *= 2.0m;
                }
            }

            if (_objCharacter != null)
                decReturn = Math.Round(decReturn, _objCharacter.Options.EssenceDecimals, MidpointRounding.AwayFromZero);
            if (SourceType == Improvement.ImprovementSource.Bioware)
            {
                decReturn += _objChildren.Sum(objChild => objChild.CalculatedESS());
            }
            return decReturn;
        }

        /// <summary>
        /// Total cost of the Cyberware and its plugins.
        /// </summary>
        public int TotalCost
        {
            get
            {
                int intCost = 0;
                int intReturn = 0;

                if (_strCost.Contains("Rating") || _strCost.Contains("MinRating"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    string strCost = string.Empty;
                    string strCostExpression = _strCost;

                    strCost = strCostExpression.Replace("MinRating", _intMinRating.ToString());
                    strCost = strCost.Replace("Rating", _intRating.ToString());
                    XPathExpression xprCost = nav.Compile(strCost);
                    intCost = Convert.ToInt32(nav.Evaluate(xprCost).ToString());
                }
                else if (_strCost.StartsWith("Parent Cost"))
                {
                    if (_objParent != null)
                    {
                        XmlDocument objXmlDocument = new XmlDocument();
                        XPathNavigator nav = objXmlDocument.CreateNavigator();

                        string strCostExpression = _strCost;
                        string strCost = "0";

                        strCost = strCostExpression.Replace("Parent Cost", _objParent.Cost);
                        if (strCost.Contains("Rating"))
                        {
                            strCost = strCost.Replace("Rating", _objParent.Rating.ToString());
                        }
                        XPathExpression xprCost = nav.Compile(strCost);
                        // This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                        double dblCost = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo));
                        intCost = Convert.ToInt32(dblCost);
                    }
                    else
                        intCost = 0;
                }
                else
                {
                    if (_strCost.StartsWith("FixedValues"))
                    {
                        char[] chrParentheses = { '(', ')' };
                        string[] strValues = _strCost.Replace("FixedValues", string.Empty).Trim(chrParentheses).Split(',');
                        if (_intRating <= strValues.Length)
                            intCost = Convert.ToInt32(strValues[_intRating - 1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                    {
                        // Just a straight cost, so return the value.
                        try
                        {
                            intCost = Convert.ToInt32(_strCost, GlobalOptions.InvariantCultureInfo);
                        }
                        catch (FormatException)
                        {
                            intCost = 0;
                        }
                    }
                }

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                intCost = Convert.ToInt32(Convert.ToDouble(intCost, GlobalOptions.InvariantCultureInfo) * Grade.Cost);

                intReturn = intCost;

                if (DiscountCost)
                    intReturn = intReturn * 9 / 10;

                // Add in the cost of all child components.
                foreach (Cyberware objChild in _objChildren)
                {
                    if (objChild.Capacity != "[*]")
                    {
                        // If the child cost starts with "*", multiply the item's base cost.
                        if (objChild.Cost.StartsWith("*"))
                        {
                            int intPluginCost = 0;
                            string strMultiplier = objChild.Cost;
                            strMultiplier = strMultiplier.Replace("*", string.Empty);
                            intPluginCost = Convert.ToInt32(intCost * (Convert.ToDouble(strMultiplier, GlobalOptions.InvariantCultureInfo) - 1));

                            if (objChild.DiscountCost)
                                intPluginCost = Convert.ToInt32(Convert.ToDouble(intPluginCost, GlobalOptions.InvariantCultureInfo) * 0.9);

                            intReturn += intPluginCost;
                        }
                        else
                            intReturn += objChild.TotalCostWithoutModifiers;
                    }
                }

                // Add in the cost of all Gear plugins.
                foreach (Gear objGear in _lstGear)
                {
                    intReturn += objGear.TotalCost;
                }

                // Retrieve the Genetech Cost Multiplier if available.
                double dblMultiplier = 1;
                ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
                if (objImprovementManager.ValueOf(Improvement.ImprovementType.GenetechCostMultiplier) != 0 && _objImprovementSource == Improvement.ImprovementSource.Bioware && _strCategory.StartsWith("Genetech"))
                {
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.GenetechCostMultiplier && objImprovement.Enabled)
                            dblMultiplier -= (1 - (Convert.ToDouble(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100));
                    }
                }

                // Retrieve the Transgenics Cost Multiplier if available.
                if (objImprovementManager.ValueOf(Improvement.ImprovementType.TransgenicsBiowareCost) != 0 && _objImprovementSource == Improvement.ImprovementSource.Bioware && _strCategory == "Genetech: Transgenics")
                {
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.TransgenicsBiowareCost && objImprovement.Enabled)
                            dblMultiplier -= (1 - (Convert.ToDouble(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100));
                    }
                }

                if (dblMultiplier == 0)
                    dblMultiplier = 1;

                double dblSuiteMultiplier = 1.0;
                if (_blnSuite)
                    dblSuiteMultiplier = 0.9;

                return Convert.ToInt32(Math.Round((Convert.ToDouble(intReturn, GlobalOptions.InvariantCultureInfo) * dblMultiplier * dblSuiteMultiplier), 2, MidpointRounding.AwayFromZero));
            }
        }

        /// <summary>
        /// Identical to TotalCost, but without the Improvement and Suite multpliers which would otherwise be doubled.
        /// </summary>
        private int TotalCostWithoutModifiers
        {
            get
            {
                int intCost = 0;
                int intReturn = 0;

                if (_strCost.Contains("Rating"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    string strCost = string.Empty;
                    string strCostExpression = _strCost;
                    strCost = strCostExpression.Replace("MinRating", _intMinRating.ToString());
                    strCost = strCost.Replace("Rating", _intRating.ToString());
                    XPathExpression xprCost = nav.Compile(strCost);
                    intCost = Convert.ToInt32(nav.Evaluate(xprCost).ToString());
                }
                else
                {
                    if (_strCost.StartsWith("FixedValues"))
                    {
                        string[] strValues = _strCost.Replace("FixedValues", string.Empty).Trim("()".ToCharArray()).Split(',');
                        if (_intRating <= strValues.Length)
                            intCost = Convert.ToInt32(strValues[_intRating - 1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                    {
                        // Just a straight cost, so return the value.
                        try
                        {
                            intCost = Convert.ToInt32(_strCost, GlobalOptions.InvariantCultureInfo);
                        }
                        catch (FormatException)
                        {
                            intCost = 0;
                        }
                    }
                }

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                intCost = Convert.ToInt32(Convert.ToDouble(intCost, GlobalOptions.InvariantCultureInfo) * Grade.Cost);

                intReturn = intCost;

                if (DiscountCost)
                    intReturn = intReturn * 9 / 10;

                // Add in the cost of all child components.
                foreach (Cyberware objChild in _objChildren)
                {
                    if (objChild.Capacity != "[*]")
                    {
                        // If the child cost starts with "*", multiply the item's base cost.
                        if (objChild.Cost.StartsWith("*"))
                        {
                            int intPluginCost = 0;
                            string strMultiplier = objChild.Cost;
                            strMultiplier = strMultiplier.Replace("*", string.Empty);
                            intPluginCost = Convert.ToInt32(intCost * (Convert.ToDouble(strMultiplier, GlobalOptions.InvariantCultureInfo) - 1));

                            if (objChild.DiscountCost)
                                intPluginCost = intPluginCost * 9 / 10;

                            intReturn += intPluginCost;
                        }
                        else
                            intReturn += objChild.TotalCostWithoutModifiers;
                    }
                }

                // Add in the cost of all Gear plugins.
                foreach (Gear objGear in _lstGear)
                {
                    intReturn += objGear.TotalCost;
                }

                const double dblMultiplier = 1;
                const double dblSuiteMultiplier = 1.0;

                return Convert.ToInt32(Math.Round((Convert.ToDouble(intReturn, GlobalOptions.InvariantCultureInfo) * dblMultiplier * dblSuiteMultiplier), 2, MidpointRounding.AwayFromZero));
            }
        }

        /// <summary>
        /// Cost of just the Cyberware itself.
        /// </summary>
        public int OwnCost
        {
            get
            {
                int intCost = 0;
                int intReturn = 0;

                if (_strCost.Contains("Rating"))
                {
                    // If the cost is determined by the Rating, evaluate the expression.
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    string strCost = string.Empty;
                    string strCostExpression = _strCost;
                    strCost = strCostExpression.Replace("MinRating", _intMinRating.ToString());
                    strCost = strCost.Replace("Rating", _intRating.ToString());
                    XPathExpression xprCost = nav.Compile(strCost);
                    intCost = Convert.ToInt32(nav.Evaluate(xprCost).ToString());
                }
                else if (_strCost.StartsWith("Parent Cost"))
                {
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();

                    string strCostExpression = _strCost;
                    string strCost = "0";

                    strCost = strCostExpression.Replace("Parent Cost", _objParent.Cost);
                    XPathExpression xprCost = nav.Compile(strCost);
                    // This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    double dblCost = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo));
                    intCost = Convert.ToInt32(dblCost);
                }
                else
                {
                    if (_strCost.StartsWith("FixedValues"))
                    {
                        char[] chrParentheses = { '(', ')' };
                        string[] strValues = _strCost.Replace("FixedValues", string.Empty).Trim(chrParentheses).Split(',');
                        if (_intRating <= strValues.Length)
                            intCost = Convert.ToInt32(strValues[_intRating - 1], GlobalOptions.InvariantCultureInfo);
                    }
                    else
                    {
                        // Just a straight cost, so return the value.
                        try
                        {
                            intCost = Convert.ToInt32(_strCost, GlobalOptions.InvariantCultureInfo);
                        }
                        catch (FormatException)
                        {
                            intCost = 0;
                        }
                    }
                }

                // Factor in the Cost multiplier of the selected CyberwareGrade.
                intCost = Convert.ToInt32(Convert.ToDouble(intCost, GlobalOptions.InvariantCultureInfo) * Grade.Cost);

                intReturn = intCost;

                if (DiscountCost)
                    intReturn = intReturn * 9 / 10;

                // Retrieve the Genetech Cost Multiplier if available.
                double dblMultiplier = 1;
                ImprovementManager objImprovementManager = new ImprovementManager(_objCharacter);
                if (objImprovementManager.ValueOf(Improvement.ImprovementType.GenetechCostMultiplier) != 0 && _objImprovementSource == Improvement.ImprovementSource.Bioware && _strCategory.StartsWith("Genetech"))
                {
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.GenetechCostMultiplier && objImprovement.Enabled)
                            dblMultiplier -= (1 - (Convert.ToDouble(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100));
                    }
                }

                // Retrieve the Transgenics Cost Multiplier if available.
                if (objImprovementManager.ValueOf(Improvement.ImprovementType.TransgenicsBiowareCost) != 0 && _objImprovementSource == Improvement.ImprovementSource.Bioware && _strCategory == "Genetech: Transgenics")
                {
                    foreach (Improvement objImprovement in _objCharacter.Improvements)
                    {
                        if (objImprovement.ImproveType == Improvement.ImprovementType.TransgenicsBiowareCost && objImprovement.Enabled)
                            dblMultiplier -= (1 - (Convert.ToDouble(objImprovement.Value, GlobalOptions.InvariantCultureInfo) / 100));
                    }
                }

                if (dblMultiplier == 0)
                    dblMultiplier = 1;

                double dblSuiteMultiplier = 1.0;
                if (_blnSuite)
                    dblSuiteMultiplier = 0.9;

                return Convert.ToInt32(Math.Round((Convert.ToDouble(intReturn, GlobalOptions.InvariantCultureInfo) * dblMultiplier * dblSuiteMultiplier), 2, MidpointRounding.AwayFromZero));
            }
        }

        /// <summary>
        /// The amount of Capacity remaining in the Cyberware.
        /// </summary>
        public int CapacityRemaining
        {
            get
            {
                int intCapacity = 0;
                if (_strCapacity.Contains("/["))
                {
                    // Get the Cyberware base Capacity.
                    string strBaseCapacity = CalculatedCapacity;
                    strBaseCapacity = strBaseCapacity.Substring(0, strBaseCapacity.IndexOf('/'));
                    intCapacity = Convert.ToInt32(strBaseCapacity);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Children)
                    {
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains("["))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        intCapacity -= Convert.ToInt32(strCapacity);
                    }

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Gear objChildGear in Gear)
                    {
                        string strCapacity = objChildGear.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains("["))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        intCapacity -= Convert.ToInt32(strCapacity);
                    }

                }
                else if (!_strCapacity.Contains("["))
                {
                    // Get the Cyberware base Capacity.
                    intCapacity = Convert.ToInt32(CalculatedCapacity);

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Cyberware objChildCyberware in Children)
                    {
                        string strCapacity = objChildCyberware.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains("["))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        intCapacity -= Convert.ToInt32(strCapacity);
                    }

                    // Run through its Children and deduct the Capacity costs.
                    foreach (Gear objChildGear in Gear)
                    {
                        string strCapacity = objChildGear.CalculatedCapacity;
                        if (strCapacity.Contains("/["))
                            strCapacity = strCapacity.Substring(strCapacity.IndexOf('[') + 1, strCapacity.IndexOf(']') - strCapacity.IndexOf('[') - 1);
                        else if (strCapacity.Contains("["))
                            strCapacity = strCapacity.Substring(1, strCapacity.Length - 2);
                        if (strCapacity == "*")
                            strCapacity = "0";
                        intCapacity -= Convert.ToInt32(strCapacity);
                    }
                }

                return intCapacity;
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
                if (!_blnVehicleMounted)
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

                if (!_blnVehicleMounted)
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
