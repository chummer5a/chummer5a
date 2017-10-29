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
    /// Commlink Device.
    /// </summary>
    public class Commlink : Gear
    {
        private bool _blnIsLivingPersona = false;
        private bool _blnActiveCommlink = false;
        private string _strAttack = string.Empty;
        private string _strSleaze = string.Empty;
        private string _strDataProcessing = string.Empty;
        private string _strFirewall = string.Empty;
        private int _intPrograms = 0;
        private string _strOverclocked = "None";
        private bool _blnCanSwapAttributes = false;
        private bool _blnHomeNode = false;

        #region Constructor, Create, Save, Load, and Print Methods
        public Commlink(Character objCharacter) : base(objCharacter)
        {
        }

        /// Create a Commlink from an XmlNode and return the TreeNodes for it.
        /// <param name="objXmlGear">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character the Gear is being added to.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="intRating">Gear Rating.</param>
        /// <param name="blnAddImprovements">Whether or not Improvements should be added to the character.</param>
        /// <param name="blnCreateChildren">Whether or not child Gear should be created.</param>
        public void Create(XmlNode objXmlGear, TreeNode objNode, int intRating, bool blnAddImprovements = true, bool blnCreateChildren = true, string strForceValue = "")
        {
            base.Create(objXmlGear, objNode, intRating, new List<Weapon>(), new List<TreeNode>(), strForceValue, false, false, blnAddImprovements, blnCreateChildren);

            if (string.IsNullOrEmpty(objXmlGear["attributearray"]?.InnerText))
            {
                objXmlGear.TryGetStringFieldQuickly("attack", ref _strAttack);
                objXmlGear.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
                objXmlGear.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
                objXmlGear.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            }
            else
            {
                _blnCanSwapAttributes = true;
                string[] strArray = objXmlGear["attributearray"].InnerText.Split(',');
                _strAttack = strArray[0];
                _strSleaze = strArray[1];
                _strDataProcessing = strArray[2];
                _strFirewall = strArray[3];
            }
            objXmlGear.TryGetInt32FieldQuickly("programs", ref _intPrograms);
        }

        /// <summary>
        /// Copy a piece of Gear.
        /// </summary>
        /// <param name="objGear">Gear object to copy.</param>
        /// <param name="objNode">TreeNode created by copying the item.</param>
        /// <param name="objWeapons">List of Weapons created by copying the item.</param>
        /// <param name="objWeaponNodes">List of Weapon TreeNodes created by copying the item.</param>
        public void Copy(Commlink objGear, TreeNode objNode, List<Weapon> objWeapons, List<TreeNode> objWeaponNodes)
        {
            base.Copy(objGear, objNode, objWeapons, objWeaponNodes);
            _strOverclocked = objGear.Overclocked;
            _strAttack = objGear.Attack;
            _strDataProcessing = objGear.DataProcessing;
            _strFirewall = objGear.Firewall;
            _strSleaze = objGear.Sleaze;
            _blnIsLivingPersona = objGear.IsLivingPersona;
            _blnActiveCommlink = objGear.IsActive;
            _blnHomeNode = objGear.HomeNode;
        }

        /// <summary>
        /// Core code to Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public override void SaveInner(XmlTextWriter objWriter)
        {
            base.SaveInner(objWriter);
            objWriter.WriteElementString("iscommlink", System.Boolean.TrueString);
            objWriter.WriteElementString("overclocked", _strOverclocked);
            objWriter.WriteElementString("attack", _strAttack);
            objWriter.WriteElementString("sleaze", _strSleaze);
            objWriter.WriteElementString("dataprocessing", _strDataProcessing);
            objWriter.WriteElementString("firewall", _strFirewall);
            objWriter.WriteElementString("livingpersona", _blnIsLivingPersona.ToString());
            objWriter.WriteElementString("active", _blnActiveCommlink.ToString());
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString());
            objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
        }

        /// <summary>
        /// Load the Gear from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public override void Load(XmlNode objNode, bool blnCopy = false)
        {
            base.Load(objNode, blnCopy);
            objNode.TryGetStringFieldQuickly("overclocked", ref _strOverclocked);
            objNode.TryGetStringFieldQuickly("attack", ref _strAttack);
            objNode.TryGetStringFieldQuickly("sleaze", ref _strSleaze);
            objNode.TryGetStringFieldQuickly("dataprocessing", ref _strDataProcessing);
            objNode.TryGetStringFieldQuickly("firewall", ref _strFirewall);
            objNode.TryGetBoolFieldQuickly("livingpersona", ref _blnIsLivingPersona);
            objNode.TryGetBoolFieldQuickly("active", ref _blnActiveCommlink);
            if (blnCopy)
            {
                _blnHomeNode = false;
            }
            else
            {
                objNode.TryGetBoolFieldQuickly("homenode", ref _blnHomeNode);
                if (_blnHomeNode)
                {
                    CharacterObject.HomeNodeCommlink = this;
                    CharacterObject.HomeNodeVehicle = null;
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
            if (CanSwapAttributes)
            {
                RefreshCyberdeckArray();
            }
        }

        /// <summary>
        /// Core code to Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public override void PrintInner(XmlTextWriter objWriter, bool blnIsCommlink = true, bool blnIsPersona = false)
        {
            base.PrintInner(objWriter, true, IsLivingPersona);

            objWriter.WriteElementString("attack", TotalAttack.ToString());
            objWriter.WriteElementString("sleaze", TotalSleaze.ToString());
            objWriter.WriteElementString("dataprocessing", TotalDataProcessing.ToString());
            objWriter.WriteElementString("firewall", TotalFirewall.ToString());
            objWriter.WriteElementString("devicerating", TotalDeviceRating.ToString());
            objWriter.WriteElementString("processorlimit", ProcessorLimit.ToString());
            objWriter.WriteElementString("active", _blnActiveCommlink.ToString());
            objWriter.WriteElementString("homenode", _blnHomeNode.ToString());
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not an item is an A.I.'s Home Node.
        /// </summary>
        public bool HomeNode
        {
            get
            {
                return _blnHomeNode;
            }
            set
            {
                _blnHomeNode = value;
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
        /// Whether or not this Commlink is a Living Persona. This should only be set by the character when printing.
        /// </summary>
        public bool IsLivingPersona
        {
            get
            {
                return _blnIsLivingPersona;
            }
            set
            {
                _blnIsLivingPersona = value;
            }
        }

        /// <summary>
        /// Whether or not this Commlink is active and counting towards the character's Matrix Initiative.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return _blnActiveCommlink;
            }
            set
            {
                _blnActiveCommlink = value;
            }
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Get the total DP of this gear without counting children or Overclocker
        /// </summary>
        public int BaseDataProcessing
        {
            get
            {
                if (string.IsNullOrEmpty(_strDataProcessing))
                    return 0;
                int intReturn = 0;

                string strExpression = _strDataProcessing;

                if (strExpression.StartsWith("FixedValues"))
                {
                    string[] strValues = strExpression.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Rating > 0)
                        strExpression = strValues[Math.Min(Rating, strValues.Length) - 1].Trim("[]".ToCharArray());
                }

                int intGearValue = 0;
                string strParentValue = string.Empty;
                Commlink objParent = Parent as Commlink;
                if (objParent != null)
                {
                    if (strExpression.Contains("Gear Data Processing"))
                        intGearValue = objParent.BaseDataProcessing;
                    if (strExpression.Contains("Parent Data Processing"))
                        strParentValue = objParent.DataProcessing;
                }
                int intTotalChildrenValue = 0;
                if (Children.Count > 0 && strExpression.Contains("Children Data Processing"))
                {
                    foreach (Gear loopGear in Children)
                    {
                        Commlink objLoopCommlink = loopGear as Commlink;
                        if (objLoopCommlink != null && loopGear.Equipped)
                            intTotalChildrenValue += objLoopCommlink.BaseDataProcessing;
                    }
                }

                if (intGearValue != 0 || intTotalChildrenValue != 0 || !string.IsNullOrEmpty(strParentValue) || strExpression.Contains('+') || strExpression.Contains("Rating"))
                {
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    string strValue = strExpression.Replace("Gear Data Processing", intGearValue.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Children Data Processing", intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Parent Data Processing", string.IsNullOrEmpty(strParentValue) ? "0" : strParentValue);
                    XPathExpression xprCost = nav.Compile(strValue);
                    // This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    double dblValue = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprCost), GlobalOptions.InvariantCultureInfo));
                    intReturn = Convert.ToInt32(dblValue);
                }
                else
                {
                    // Just a straight cost, so return the value.
                    intReturn = Convert.ToInt32(strExpression);
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Get the total Attack of this gear without counting children or Overclocker
        /// </summary>
        public int BaseAttack
        {
            get
            {
                if (string.IsNullOrEmpty(_strAttack))
                    return 0;
                int intReturn = 0;

                string strExpression = _strAttack;

                if (strExpression.StartsWith("FixedValues"))
                {
                    string[] strValues = strExpression.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Rating > 0)
                        strExpression = strValues[Math.Min(Rating, strValues.Length) - 1].Trim("[]".ToCharArray());
                }

                int intGearValue = 0;
                string strParentValue = string.Empty;
                Commlink objParent = Parent as Commlink;
                if (objParent != null)
                {
                    if (strExpression.Contains("Gear Attack"))
                        intGearValue = objParent.BaseAttack;
                    if (strExpression.Contains("Parent Attack"))
                        strParentValue = objParent.Attack;
                }
                int intTotalChildrenValue = 0;
                if (Children.Count > 0 && strExpression.Contains("Children Attack"))
                {
                    foreach (Gear loopGear in Children)
                    {
                        Commlink objLoopCommlink = loopGear as Commlink;
                        if (objLoopCommlink != null && loopGear.Equipped)
                            intTotalChildrenValue += objLoopCommlink.BaseAttack;
                    }
                }

                if (intGearValue != 0 || intTotalChildrenValue != 0 || !string.IsNullOrEmpty(strParentValue) || strExpression.Contains('+') || strExpression.Contains("Rating"))
                {
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    string strValue = strExpression.Replace("Gear Attack", intGearValue.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Children Attack", intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Parent Attack", string.IsNullOrEmpty(strParentValue) ? "0" : strParentValue);
                    XPathExpression xprValue = nav.Compile(strValue);
                    // This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    double dblValue = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprValue), GlobalOptions.InvariantCultureInfo));
                    intReturn = Convert.ToInt32(dblValue);
                }
                else
                {
                    // Just a straight cost, so return the value.
                    intReturn = Convert.ToInt32(strExpression);
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Get the total Sleaze of this gear without counting children or Overclocker
        /// </summary>
        public int BaseSleaze
        {
            get
            {
                if (string.IsNullOrEmpty(_strSleaze))
                    return 0;
                int intReturn = 0;

                string strExpression = _strSleaze;

                if (strExpression.StartsWith("FixedValues"))
                {
                    string[] strValues = strExpression.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Rating > 0)
                        strExpression = strValues[Math.Min(Rating, strValues.Length) - 1].Trim("[]".ToCharArray());
                }

                int intGearValue = 0;
                string strParentValue = string.Empty;
                Commlink objParent = Parent as Commlink;
                if (objParent != null)
                {
                    if (strExpression.Contains("Gear Sleaze"))
                        intGearValue = objParent.BaseSleaze;
                    if (strExpression.Contains("Parent Sleaze"))
                        strParentValue = objParent.Sleaze;
                }
                int intTotalChildrenValue = 0;
                if (Children.Count > 0 && strExpression.Contains("Children Sleaze"))
                {
                    foreach (Gear loopGear in Children)
                    {
                        Commlink objLoopCommlink = loopGear as Commlink;
                        if (objLoopCommlink != null && loopGear.Equipped)
                            intTotalChildrenValue += objLoopCommlink.BaseSleaze;
                    }
                }

                if (intGearValue != 0 || intTotalChildrenValue != 0 || !string.IsNullOrEmpty(strParentValue) || strExpression.Contains('+') || strExpression.Contains("Rating"))
                {
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    string strValue = strExpression.Replace("Gear Sleaze", intGearValue.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Children Sleaze", intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Parent Sleaze", string.IsNullOrEmpty(strParentValue) ? "0" : strParentValue);
                    XPathExpression xprValue = nav.Compile(strValue);
                    // This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    double dblValue = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprValue), GlobalOptions.InvariantCultureInfo));
                    intReturn = Convert.ToInt32(dblValue);
                }
                else
                {
                    // Just a straight cost, so return the value.
                    intReturn = Convert.ToInt32(strExpression);
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Get the total Firewall of this gear without counting children or Overclocker
        /// </summary>
        public int BaseFirewall
        {
            get
            {
                if (string.IsNullOrEmpty(_strFirewall))
                    return 0;
                int intReturn = 0;

                string strExpression = _strFirewall;

                if (strExpression.StartsWith("FixedValues"))
                {
                    string[] strValues = strExpression.TrimStart("FixedValues", true).Trim("()".ToCharArray()).Split(',');
                    if (Rating > 0)
                        strExpression = strValues[Math.Min(Rating, strValues.Length) - 1].Trim("[]".ToCharArray());
                }

                int intGearValue = 0;
                string strParentValue = string.Empty;
                Commlink objParent = Parent as Commlink;
                if (objParent != null)
                {
                    if (strExpression.Contains("Gear Firewall"))
                        intGearValue = objParent.BaseFirewall;
                    if (strExpression.Contains("Parent Firewall"))
                        strParentValue = objParent.Firewall;
                }
                int intTotalChildrenValue = 0;
                if (Children.Count > 0 && strExpression.Contains("Children Firewall"))
                {
                    foreach (Gear loopGear in Children)
                    {
                        Commlink objLoopCommlink = loopGear as Commlink;
                        if (objLoopCommlink != null && loopGear.Equipped)
                            intTotalChildrenValue += objLoopCommlink.BaseFirewall;
                    }
                }

                if (intGearValue != 0 || intTotalChildrenValue != 0 || !string.IsNullOrEmpty(strParentValue) || strExpression.Contains('+') || strExpression.Contains("Rating"))
                {
                    XmlDocument objXmlDocument = new XmlDocument();
                    XPathNavigator nav = objXmlDocument.CreateNavigator();
                    string strValue = strExpression.Replace("Gear Firewall", intGearValue.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Children Firewall", intTotalChildrenValue.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Rating", Rating.ToString(GlobalOptions.InvariantCultureInfo));
                    strValue = strValue.Replace("Parent Firewall", string.IsNullOrEmpty(strParentValue) ? "0" : strParentValue);
                    XPathExpression xprValue = nav.Compile(strValue);
                    // This is first converted to a double and rounded up since some items have a multiplier that is not a whole number, such as 2.5.
                    double dblValue = Math.Ceiling(Convert.ToDouble(nav.Evaluate(xprValue), GlobalOptions.InvariantCultureInfo));
                    intReturn = Convert.ToInt32(dblValue);
                }
                else
                {
                    // Just a straight cost, so return the value.
                    intReturn = Convert.ToInt32(strExpression);
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Get the bonus DP of this gear from children and Overclocker
        /// </summary>
        public int BonusDataProcessing
        {
            get
            {
                int intReturn = 0;

                foreach (Gear loopGear in Children)
                {
                    if (loopGear.GetType() == typeof(Commlink) && loopGear.Equipped)
                        intReturn += (loopGear as Commlink).TotalDataProcessing;
                }

                if (CharacterObject.Overclocker && Overclocked == "DataProc")
                {
                    intReturn += 1;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Get the bonus Attack of this gear from children and Overclocker
        /// </summary>
        public int BonusAttack
        {
            get
            {
                int intReturn = 0;

                foreach (Gear loopGear in Children)
                {
                    if (loopGear.GetType() == typeof(Commlink) && loopGear.Equipped)
                        intReturn += (loopGear as Commlink).TotalAttack;
                }

                if (CharacterObject.Overclocker && Overclocked == "Attack")
                {
                    intReturn += 1;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Get the bonus Sleaze of this gear from children and Overclocker
        /// </summary>
        public int BonusSleaze
        {
            get
            {
                int intReturn = 0;

                foreach (Gear loopGear in Children)
                {
                    if (loopGear.GetType() == typeof(Commlink) && loopGear.Equipped)
                        intReturn += (loopGear as Commlink).TotalSleaze;
                }

                if (CharacterObject.Overclocker && Overclocked == "Sleaze")
                {
                    intReturn += 1;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Get the bonus Firewall of this gear from children and Overclocker
        /// </summary>
        public int BonusFirewall
        {
            get
            {
                int intReturn = 0;

                foreach (Gear loopGear in Children)
                {
                    if (loopGear.GetType() == typeof(Commlink) && loopGear.Equipped)
                        intReturn += (loopGear as Commlink).TotalFirewall;
                }

                if (CharacterObject.Overclocker && Overclocked == "Firewall")
                {
                    intReturn += 1;
                }

                return intReturn;
            }
        }

        /// <summary>
        /// Get the total DP of this gear after children and Overclocker
        /// </summary>
        public int TotalDataProcessing
        {
            get
            {
                return BaseDataProcessing + BonusDataProcessing;
            }
        }

        /// <summary>
        /// Get the total Attack of this gear after children and Overclocker
        /// </summary>
        public int TotalAttack
        {
            get
            {
                return BaseAttack + BonusAttack;
            }
        }

        /// <summary>
        /// Get the total Sleaze of this gear after children and Overclocker
        /// </summary>
        public int TotalSleaze
        {
            get
            {
                return BaseSleaze + BonusSleaze;
            }
        }

        /// <summary>
        /// Get the total Firewall of this gear after children and Overclocker
        /// </summary>
        public int TotalFirewall
        {
            get
            {
                return BaseFirewall + BonusFirewall;
            }
        }

        /// <summary>
        /// Commlink's Processor Limit.
        /// </summary>
        public int ProcessorLimit
        {
            get
            {
                return TotalDeviceRating;
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
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Refreshes a set of ComboBoxes corresponding to Matrix attributes
        /// </summary>
        public void RefreshCommlinkCBOs(ComboBox cboAttack, ComboBox cboSleaze, ComboBox cboDP, ComboBox cboFirewall)
        {
            int intBaseAttack = BaseAttack;
            int intBaseSleaze = BaseSleaze;
            int intBaseDP = BaseDataProcessing;
            int intBaseFirewall = BaseFirewall;
            int intBonusAttack = BonusAttack;
            int intBonusSleaze = BonusSleaze;
            int intBonusDP = BonusDataProcessing;
            int intBonusFirewall = BonusFirewall;

            cboAttack.BeginUpdate();
            cboSleaze.BeginUpdate();
            cboDP.BeginUpdate();
            cboFirewall.BeginUpdate();

            cboAttack.Enabled = false;
            cboAttack.BindingContext = new BindingContext();
            cboAttack.ValueMember = "Value";
            cboAttack.DisplayMember = "Name";
            cboAttack.DataSource = new List<string>() { (intBaseAttack + intBonusAttack).ToString(), (intBaseSleaze + intBonusAttack).ToString(), (intBaseDP + intBonusAttack).ToString(), (intBaseFirewall + intBonusAttack).ToString() };
            cboAttack.SelectedIndex = 0;
            cboAttack.Visible = true;
            cboAttack.Enabled = CanSwapAttributes;

            cboSleaze.Enabled = false;
            cboSleaze.BindingContext = new BindingContext();
            cboSleaze.ValueMember = "Value";
            cboSleaze.DisplayMember = "Name";
            cboSleaze.DataSource = new List<string>() { (intBaseAttack + intBonusSleaze).ToString(), (intBaseSleaze + intBonusSleaze).ToString(), (intBaseDP + intBonusSleaze).ToString(), (intBaseFirewall + intBonusSleaze).ToString() };
            cboSleaze.SelectedIndex = 1;
            cboSleaze.Visible = true;
            cboSleaze.Enabled = CanSwapAttributes;

            cboDP.Enabled = false;
            cboDP.BindingContext = new BindingContext();
            cboDP.ValueMember = "Value";
            cboDP.DisplayMember = "Name";
            cboDP.DataSource = new List<string>() { (intBaseAttack + intBonusDP).ToString(), (intBaseSleaze + intBonusDP).ToString(), (intBaseDP + intBonusDP).ToString(), (intBaseFirewall + intBonusDP).ToString() };
            cboDP.SelectedIndex = 2;
            cboDP.Visible = true;
            cboDP.Enabled = CanSwapAttributes;

            cboFirewall.Enabled = false;
            cboFirewall.BindingContext = new BindingContext();
            cboFirewall.ValueMember = "Value";
            cboFirewall.DisplayMember = "Name";
            cboFirewall.DataSource = new List<string>() { (intBaseAttack + intBonusFirewall).ToString(), (intBaseSleaze + intBonusFirewall).ToString(), (intBaseDP + intBonusFirewall).ToString(), (intBaseFirewall + intBonusFirewall).ToString() };
            cboFirewall.SelectedIndex = 3;
            cboFirewall.Visible = true;
            cboFirewall.Enabled = CanSwapAttributes;

            cboAttack.EndUpdate();
            cboSleaze.EndUpdate();
            cboDP.EndUpdate();
            cboFirewall.EndUpdate();
        }

        public void RefreshCyberdeckArray()
        {
            if (!CanSwapAttributes)
                return;
            int intBaseAttack = BaseAttack;
            int intBaseSleaze = BaseSleaze;
            int intBaseDP = BaseDataProcessing;
            int intBaseFirewall = BaseFirewall;
            List<int> lstStatsArray = new List<int>(4);
            lstStatsArray.Add(intBaseAttack);
            lstStatsArray.Add(intBaseSleaze);
            lstStatsArray.Add(intBaseDP);
            lstStatsArray.Add(intBaseFirewall);
            lstStatsArray.Sort();
            lstStatsArray.Reverse();

            string[] strCyberdeckArray = MyXmlNode["attributearray"].InnerText.Split(',');
            foreach (Gear objChild in Children)
            {
                XmlNode objLoopNode = objChild.MyXmlNode;
                string strLoopArrayText = objLoopNode["modattributearray"]?.InnerText;
                if (!string.IsNullOrEmpty(strLoopArrayText))
                {
                    string[] strLoopArray = strLoopArrayText.Split(',');
                    for (int i = 0; i < 4; ++i)
                    {
                        strCyberdeckArray[i] += "+(" + strLoopArray[i] + ")";
                    }
                }
            }
            for(int i = 0; i < 4; ++i)
            {
                if (intBaseAttack == lstStatsArray[i])
                {
                    _strAttack = strCyberdeckArray[i];
                    lstStatsArray[i] = int.MinValue;
                    break;
                }
            }
            for (int i = 0; i < 4; ++i)
            {
                if (intBaseSleaze == lstStatsArray[i])
                {
                    _strSleaze = strCyberdeckArray[i];
                    lstStatsArray[i] = int.MinValue;
                    break;
                }
            }
            for (int i = 0; i < 4; ++i)
            {
                if (intBaseDP == lstStatsArray[i])
                {
                    _strDataProcessing = strCyberdeckArray[i];
                    lstStatsArray[i] = int.MinValue;
                    break;
                }
            }
            for (int i = 0; i < 4; ++i)
            {
                if (intBaseFirewall == lstStatsArray[i])
                {
                    _strFirewall = strCyberdeckArray[i];
                    break;
                }
            }
        }
        #endregion
    }
}
