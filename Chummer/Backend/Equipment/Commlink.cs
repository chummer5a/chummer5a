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
        private string _strAttack = string.Empty;
        private string _strSleaze = string.Empty;
        private string _strDataProcessing = string.Empty;
        private string _strFirewall = string.Empty;
        private int _intPrograms = 0;
        private string _strOverclocked = "None";
        private bool _blnCanSwapAttributes = false;

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
            IsActive = objGear.IsActive;
            HomeNode = objGear.HomeNode;
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
            objWriter.WriteElementString("canswapattributes", _blnCanSwapAttributes.ToString());
            objWriter.WriteElementString("active", IsActive.ToString());
            objWriter.WriteElementString("homenode", HomeNode.ToString());
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
            bool blnIsActive = false;
            if (objNode.TryGetBoolFieldQuickly("active", ref blnIsActive) && blnIsActive)
                IsActive = true;
            if (blnCopy)
            {
                HomeNode = false;
            }
            else
            {
                bool blnIsHomeNode = false;
                if (objNode.TryGetBoolFieldQuickly("homenode", ref blnIsHomeNode) && blnIsHomeNode)
                {
                    HomeNode = true;
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
        public override void PrintInner(XmlTextWriter objWriter, bool blnIsCommlink = true)
        {
            base.PrintInner(objWriter, true);

            objWriter.WriteElementString("attack", GetTotalMatrixAttribute("Attack").ToString());
            objWriter.WriteElementString("sleaze", GetTotalMatrixAttribute("Sleaze").ToString());
            objWriter.WriteElementString("dataprocessing", GetTotalMatrixAttribute("Data Processing").ToString());
            objWriter.WriteElementString("firewall", GetTotalMatrixAttribute("Firewall").ToString());
            objWriter.WriteElementString("devicerating", GetTotalMatrixAttribute("Device Rating").ToString());
            objWriter.WriteElementString("processorlimit", ProcessorLimit.ToString());
            objWriter.WriteElementString("active", IsActive.ToString());
            objWriter.WriteElementString("homenode", HomeNode.ToString());
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
                return CharacterObject.HomeNodeCommlink == this;
            }
            set
            {
                if (value)
                {
                    CharacterObject.HomeNodeCommlink = this;
                    CharacterObject.HomeNodeVehicle = null;
                }
                else if (CharacterObject.HomeNodeCommlink == this)
                    CharacterObject.HomeNodeCommlink = null;
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
        /// Whether or not this Commlink is active and counting towards the character's Matrix Initiative.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return CharacterObject.ActiveCommlink == this;
            }
            set
            {
                if (value)
                {
                    CharacterObject.ActiveCommlink = this;
                }
                else if (CharacterObject.ActiveCommlink == this)
                {
                    CharacterObject.ActiveCommlink = null;
                }
            }
        }
        #endregion

        #region Complex Properties
        /// <summary>
        /// Commlink's Processor Limit.
        /// </summary>
        public int ProcessorLimit
        {
            get
            {
                return GetTotalMatrixAttribute("Device Rating");
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
            int intBaseAttack = GetBaseMatrixAttribute("Attack");
            int intBaseSleaze = GetBaseMatrixAttribute("Sleaze");
            int intBaseDP = GetBaseMatrixAttribute("Data Processing");
            int intBaseFirewall = GetBaseMatrixAttribute("Firewall");
            int intBonusAttack = GetBonusMatrixAttribute("Attack");
            int intBonusSleaze = GetBonusMatrixAttribute("Sleaze");
            int intBonusDP = GetBonusMatrixAttribute("Data Processing");
            int intBonusFirewall = GetBonusMatrixAttribute("Firewall");

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
            int intBaseAttack = GetBaseMatrixAttribute("Attack");
            int intBaseSleaze = GetBaseMatrixAttribute("Sleaze");
            int intBaseDP = GetBaseMatrixAttribute("Data Processing");
            int intBaseFirewall = GetBaseMatrixAttribute("Firewall");
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
