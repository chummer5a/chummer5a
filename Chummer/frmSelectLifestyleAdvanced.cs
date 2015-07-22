﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    public partial class frmSelectLifestyleAdvanced : Form
    {
        private bool _blnAddAgain = false;
        private Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;
        private LifestyleType _objType = LifestyleType.Advanced;

        private XmlDocument _objXmlDocument = new XmlDocument();

        private bool _blnSkipRefresh = false;

        #region Control Events
        public frmSelectLifestyleAdvanced(Lifestyle objLifestyle, Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = objLifestyle;
            MoveControls();
        }

        private void frmSelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            _blnSkipRefresh = true;

            foreach (Label objLabel in this.Controls.OfType<Label>())
            {
                if (objLabel.Text.StartsWith("["))
                    objLabel.Text = "";
            }

            // Load the Lifestyles information.
            _objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            List<ListItem> lstLifestyles = new List<ListItem>();
            foreach (XmlNode objXmlLifestyle in _objXmlDocument.SelectNodes("/chummer/lifestyles/lifestyle"))
            {
                bool blnAdd = true;
                if (_objType != LifestyleType.Advanced && objXmlLifestyle["slp"] != null)
                {
                    if (objXmlLifestyle["slp"].InnerText == "remove")
                        blnAdd = false;
                }

                if (blnAdd)
                {
                    ListItem objItem = new ListItem();
                    objItem.Value = objXmlLifestyle["name"].InnerText;
                    if (objXmlLifestyle["translate"] != null)
                        objItem.Name = objXmlLifestyle["translate"].InnerText;
                    else
                        objItem.Name = objXmlLifestyle["name"].InnerText;
                    lstLifestyles.Add(objItem);
                }
            }
            cboBaseLifestyle.ValueMember = "Value";
            cboBaseLifestyle.DisplayMember = "Name";
            cboBaseLifestyle.DataSource = lstLifestyles;

            cboBaseLifestyle.SelectedValue = _objLifestyle.BaseLifestyle;

            if (cboBaseLifestyle.SelectedIndex == -1)
            { cboBaseLifestyle.SelectedIndex = 0; }

            if (_objSourceLifestyle != null)
            {
                txtLifestyleName.Text = _objSourceLifestyle.Name;
                nudRoommates.Value = _objSourceLifestyle.Roommates;
                nudPercentage.Value = _objSourceLifestyle.Percentage;
                lblSource.Text = _objSourceLifestyle.Source;

            }

            // Safehouses have a cost per week instead of cost per month.
            if (_objType == LifestyleType.Safehouse)
                lblCostLabel.Text = LanguageManager.Instance.GetString("Label_SelectLifestyle_CostPerWeek");

            _blnSkipRefresh = false;
            CalculateValues();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            if (txtLifestyleName.Text == "")
            {
                MessageBox.Show(LanguageManager.Instance.GetString("Message_SelectAdvancedLifestyle_LifestyleName"), LanguageManager.Instance.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            cmdOK_Click(sender, e);
        }

        private void trePositiveQualities_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CalculateValues();
        }

        private void treNegativeQualities_AfterCheck(object sender, TreeViewEventArgs e)
        {
            CalculateValues();
        }

        private void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalculateValues();
        }

        private void nudPercentage_ValueChanged(object sender, EventArgs e)
        {
            CalculateValues();
        }

        private void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            CalculateValues();
        }

        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain
        {
            get
            {
                return _blnAddAgain;
            }
        }

        /// <summary>
        /// Lifestyle that was created in the dialogue.
        /// </summary>
        public Lifestyle SelectedLifestyle
        {
            get
            {
                return _objLifestyle;
            }
        }

        /// <summary>
        /// Type of Lifestyle to create.
        /// </summary>
        public LifestyleType StyleType
        {
            get
            {
                return _objType;
            }
            set
            {
                _objType = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            _objLifestyle.Source = "RF";
            _objLifestyle.Page = "154";
            _objLifestyle.Name = txtLifestyleName.Text;
            _objLifestyle.Cost = CalculateValues();
            _objLifestyle.Roommates = Convert.ToInt32(nudRoommates.Value);
            _objLifestyle.Percentage = Convert.ToInt32(nudPercentage.Value);
            _objLifestyle.BaseLifestyle = cboBaseLifestyle.Text;
			
            //_objLifestyle.LifestyleQualities.Clear();

            // Get the starting Nuyen information.
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            _objLifestyle.Dice = Convert.ToInt32(objXmlAspect["dice"].InnerText);
            _objLifestyle.Multiplier = Convert.ToInt32(objXmlAspect["multiplier"].InnerText);
            _objLifestyle.StyleType = _objType;

	        Guid source;
	        if (objXmlAspect.TryGetField("id", Guid.TryParse, out source))
	        {
				{
					Log.Warning(new object[] { "Missing id field for lifestyle xmlnode", objXmlAspect });

					if (System.Diagnostics.Debugger.IsAttached)
						System.Diagnostics.Debugger.Break();
				}
				_objLifestyle.SourceID = source;
	        }

            foreach (TreeNode objNode in treLifestyleQualities.Nodes[0].Nodes)
            {
                    _objLifestyle.LifestyleQualities.Add(objNode.Text.ToString());
            }
            foreach (TreeNode objNode in treLifestyleQualities.Nodes[1].Nodes)
            {
                _objLifestyle.LifestyleQualities.Add(objNode.Text.ToString());
            }
            foreach (TreeNode objNode in treLifestyleQualities.Nodes[2].Nodes)
            {
                _objLifestyle.LifestyleQualities.Add(objNode.Text.ToString());
            }
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private int CalculateValues()
        {
            if (_blnSkipRefresh)
                return 0;

            int intLP = 0;
            int intNuyen = 0;
            int intMultiplier = 0;

            // Calculate the limits of the 3 aspects.
            // Comforts LP.
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            nudComforts.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudComforts.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            if (nudComforts.Value > nudComforts.Maximum)
            {
                nudComforts.Value = nudComforts.Maximum;
            }
            nudComfortsEntertainment.Maximum = Convert.ToInt32(Convert.ToInt32(objXmlAspect["limit"].InnerText) - nudComforts.Value);
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Comforts").Replace("{0}", nudComforts.Value.ToString()).Replace("{1}", objXmlAspect["limit"].InnerText);

            // Area.
            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            nudArea.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudArea.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            if (nudArea.Value > nudArea.Maximum)
            {
                nudArea.Value = nudArea.Maximum;
            }
            nudAreaEntertainment.Maximum = Convert.ToInt32(Convert.ToInt32(objXmlAspect["limit"].InnerText) - nudArea.Value);
            Label_SelectAdvancedLifestyle_Base_Area.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Area").Replace("{0}", nudArea.Value.ToString()).Replace("{1}", objXmlAspect["limit"].InnerText);

            // Security.
            objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            nudSecurity.Minimum = Convert.ToInt32(objXmlAspect["minimum"].InnerText);
            nudSecurity.Maximum = Convert.ToInt32(objXmlAspect["limit"].InnerText);
            if (nudSecurity.Value > nudSecurity.Maximum)
            {
                nudSecurity.Value = nudSecurity.Maximum;
            }
            nudSecurityEntertainment.Maximum = Convert.ToInt32(Convert.ToInt32(objXmlAspect["limit"].InnerText) - nudSecurity.Value);
            Label_SelectAdvancedLifestyle_Base_Securities.Text = LanguageManager.Instance.GetString("Label_SelectAdvancedLifestyle_Base_Security").Replace("{0}", nudSecurity.Value.ToString()).Replace("{1}", objXmlAspect["limit"].InnerText);

            intLP = (Convert.ToInt32(nudComforts.Maximum) - Convert.ToInt32(nudComforts.Value));
            intLP += (Convert.ToInt32(nudArea.Maximum) - Convert.ToInt32(nudArea.Value));
            intLP += (Convert.ToInt32(nudSecurity.Maximum) - Convert.ToInt32(nudSecurity.Value));
            intLP += Convert.ToInt32(nudComfortsEntertainment.Value);
            intLP += Convert.ToInt32(nudSecurityEntertainment.Value);
            intLP += Convert.ToInt32(nudAreaEntertainment.Value);
            intLP += Convert.ToInt32(nudRoommates.Value);


            // Determine the base Nuyen cost.
            XmlNode objXmlLifestyle = _objXmlDocument.SelectSingleNode("chummer/lifestyles/lifestyle[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");

            // Calculate the cost of Positive Qualities.
            foreach (TreeNode objNode in treLifestyleQualities.Nodes[0].Nodes)
            {
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objNode.Text.ToString() + "\" and category = \"Positive\"]");
                if (objXmlAspect != null)
                {
                    intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
                    if (objXmlAspect["multiplier"] != null)
                    {
                        intMultiplier += Convert.ToInt32(objXmlAspect["multiplier"].InnerText); ;
                    }
                }
            }

            // Calculate the cost of Negative Qualities.
            foreach (TreeNode objNode in treLifestyleQualities.Nodes[1].Nodes)
            {
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objNode.Text.ToString() + "\" and category = \"Negative\"]");
                if (objXmlAspect != null)
                {
                    intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
                    if (objXmlAspect["neighborhood"] != null)
                    {
                        nudArea.Maximum += Convert.ToInt32(objXmlAspect["neighborhood"].InnerText);
                    }

                    if (objXmlAspect["comforts"] != null)
                    {
                        nudComforts.Maximum += Convert.ToInt32(objXmlAspect["comforts"].InnerText); ;
                    }
                     if (objXmlAspect["multiplier"] != null)
                    {
                        intMultiplier += Convert.ToInt32(objXmlAspect["multiplier"].InnerText); ;
                    }
                     if (objXmlAspect["cost"] != null)
                     {
                         intNuyen += Convert.ToInt32(objXmlAspect["cost"].InnerText);
                     }
                    
                }

            }

            // Calculate the cost of Entertainments.
            foreach (TreeNode objNode in treLifestyleQualities.Nodes[2].Nodes)
            {
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objNode.Text.ToString() + "\"]");
                string[] strLifestyleEntertainments = objXmlAspect["allowed"].InnerText.Split(',');
                int intLifestyleEntertainmentFree = 0;
                if (objXmlAspect != null)
                {
                    intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
                    if (objXmlAspect["multiplier"] != null)
                    {
                        intMultiplier += Convert.ToInt32(objXmlAspect["multiplier"].InnerText); ;
                    }
                    if (objXmlAspect["cost"] != null)
                    {
                        intNuyen += Convert.ToInt32(objXmlAspect["cost"].InnerText);
                    }
                }
                
                foreach (string strLifestyle in strLifestyleEntertainments)
                {
                    if (strLifestyle == cboBaseLifestyle.SelectedValue.ToString())
                    {
                        intLifestyleEntertainmentFree += 1;
                    }
                }

                if (intLifestyleEntertainmentFree == 0)
                {
                    if (objXmlAspect["cost"] != null)
                    {
                        intNuyen += Convert.ToInt32(objXmlAspect["cost"].InnerText);
                    }
                }


            }

            intMultiplier += Convert.ToInt32(nudRoommates.Value * 10);
            intNuyen += Convert.ToInt32(objXmlLifestyle["cost"].InnerText);
            intMultiplier = (intNuyen * intMultiplier) / 100;
            intNuyen += intMultiplier;
            intNuyen = Convert.ToInt32(Convert.ToDouble(intNuyen, GlobalOptions.Instance.CultureInfo) * Convert.ToDouble(nudPercentage.Value / 100, GlobalOptions.Instance.CultureInfo));
            lblTotalLP.Text = intLP.ToString();
            lblCost.Text = String.Format("{0:###,###,##0¥}", intNuyen);

            return intNuyen;
        }

        /// <summary>
        /// Lifestyle to update when editing.
        /// </summary>
        /// <param name="objLifestyle">Lifestyle to edit.</param>
        public void SetLifestyle(Lifestyle objLifestyle)
        {
            _objSourceLifestyle = objLifestyle;
            _objType = objLifestyle.StyleType;
        }

        /// <summary>
        /// Sort the contents of a TreeView alphabetically.
        /// </summary>
        /// <param name="treTree">TreeView to sort.</param>
        private void SortTree(TreeView treTree)
        {
            List<TreeNode> lstNodes = new List<TreeNode>();
            foreach (TreeNode objNode in treTree.Nodes)
                lstNodes.Add(objNode);
            treTree.Nodes.Clear();
            try
            {
                SortByName objSort = new SortByName();
                lstNodes.Sort(objSort.Compare);
            }
            catch
            {
            }
            foreach (TreeNode objNode in lstNodes)
                treTree.Nodes.Add(objNode);
        }

        private void MoveControls()
        {
            //int intLeft = 0;
            //intLeft = Math.Max(lblLifestyleNameLabel.Left + lblLifestyleNameLabel.Width, Label_SelectAdvancedLifestyle_Upgrade_Comforts.Left + Label_SelectAdvancedLifestyle_Upgrade_Comforts.Width);
            //intLeft = Math.Max(intLeft, lblNeighborhood.Left + lblNeighborhood.Width);
            //intLeft = Math.Max(intLeft, lblSecurity.Left + lblSecurity.Width);

            //txtLifestyleName.Left = intLeft + 6;
            //cboBaseLifestyle.Left = intLeft + 6;
            //cboBaseLifestyle.Left = intLeft + 6;
            //cboBaseLifestyle.Left = intLeft + 6;
        }
        #endregion

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter);
            frmSelectLifestyleQuality.ShowDialog(this);

                        // Don't do anything else if the form was canceled.
            if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                return;

            XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
            XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmSelectLifestyleQuality.SelectedQuality + "\"]");

            TreeNode objNode = new TreeNode();
            List<Weapon> objWeapons = new List<Weapon>();
            List<TreeNode> objWeaponNodes = new List<TreeNode>();
            LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

            try { objQuality.Create(objXmlQuality, _objCharacter, QualitySource.Selected, objNode); }
            catch { }
            //objNode.ContextMenuStrip = cmsQuality;
            if (objQuality.InternalId == Guid.Empty.ToString())
                return;

            if (frmSelectLifestyleQuality.FreeCost)
                objQuality.LP = 0;

            // If the item being checked would cause the limit of 25 BP spent on Positive Qualities to be exceed, do not let it be checked and display a message.
            string strAmount = "";
            strAmount = "25 " + LanguageManager.Instance.GetString("String_Karma");

            // Make sure that adding the Quality would not cause the character to exceed their BP limits.
            bool blnAddItem = true;

            if (blnAddItem)
            {
                // Add the Quality to the appropriate parent node.
                if (objQuality.Type == QualityType.Positive)
                {
                    treLifestyleQualities.Nodes[0].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[0].Expand();
                }
                else if (objQuality.Type == QualityType.Negative)
                {
                    treLifestyleQualities.Nodes[1].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[1].Expand();
                }
                else
                {
                    treLifestyleQualities.Nodes[2].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[2].Expand();
                }
                //_objLifestyle.LifestyleQualities.Add(objQuality);

                CalculateValues();

                if (frmSelectLifestyleQuality.AddAgain)
                    cmdAddQuality_Click(sender, e);
            }
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treLifestyleQualities.SelectedNode.Level == 0)
                return;
            else
            {
                treLifestyleQualities.SelectedNode.Remove();
                CalculateValues();
            }
        }
    }
    public class LifestyleQuality
    {
        private Guid _guiID = new Guid();
        private string _strName = "";
        private string _strCost = "";
        private string _strExtra = "";
        private string _strSource = "";
        private string _strPage = "";
        private string _strNotes = "";
        private bool _blnContributeToLimit = true;
        private bool _blnPrint = true;
        private int _intLP = 0;
        private int _intCost = 0;
        private QualityType _objLifestyleQualityType = QualityType.Positive;
        private QualitySource _objLifestyleQualitySource = QualitySource.Selected;
        private XmlNode _nodBonus;
        private readonly Character _objCharacter;
        private string _strAltName = "";
        private string _strAltPage = "";

        #region Helper Methods
        /// <summary>
        /// Convert a string to a LifestyleQualityType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public QualityType ConvertToLifestyleQualityType(string strValue)
        {
            switch (strValue)
            {
                case "Negative":
                    return QualityType.Negative;
                case "Entertainment - Asset":
                    return QualityType.Entertainment;
                case "Entertainment - Service":
                    return QualityType.Entertainment;
                case "Entertainment - Outing":
                    return QualityType.Entertainment;
                default:
                    return QualityType.Positive;
            }
        }

        /// <summary>
        /// Convert a string to a LifestyleQualitySource.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public QualitySource ConvertToLifestyleQualitySource(string strValue)
        {
            switch (strValue)
            {
                case "Metatype":
                    return QualitySource.Metatype;
                case "MetatypeRemovable":
                    return QualitySource.MetatypeRemovable;
                default:
                    return QualitySource.Selected;
            }
        }
        #endregion

        #region Constructor, Create, Save, Load, and Print Methods
        public LifestyleQuality(Character objCharacter)
        {
            // Create the GUID for the new LifestyleQuality.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a LifestyleQuality from an XmlNode and return the TreeNodes for it.
        /// </summary>
        /// <param name="objXmlLifestyleQuality">XmlNode to create the object from.</param>
        /// <param name="objCharacter">Character object the LifestyleQuality will be added to.</param>
        /// <param name="objLifestyleQualitySource">Source of the LifestyleQuality.</param>
        /// <param name="objNode">TreeNode to populate a TreeView.</param>
        /// <param name="objWeapons">List of Weapons that should be added to the Character.</param>
        /// <param name="objWeaponNodes">List of TreeNodes to represent the Weapons added.</param>
        /// <param name="strForceValue">Force a value to be selected for the LifestyleQuality.</param>
        public void Create(XmlNode objXmlLifestyleQuality, Character objCharacter, QualitySource objLifestyleQualitySource, TreeNode objNode)
        {
            _strName = objXmlLifestyleQuality["name"].InnerText;
            _intLP = Convert.ToInt32(objXmlLifestyleQuality["lp"].InnerText);
            try
            {
                _intCost = Convert.ToInt32(objXmlLifestyleQuality["cost"].InnerText);
            }
            catch 
            {}
            _objLifestyleQualityType = ConvertToLifestyleQualityType(objXmlLifestyleQuality["category"].InnerText);
            _objLifestyleQualitySource = objLifestyleQualitySource;
            if (objXmlLifestyleQuality["print"] != null)
            {
                if (objXmlLifestyleQuality["print"].InnerText == "no")
                    _blnPrint = false;
            }
            if (objXmlLifestyleQuality["contributetolimit"] != null)
            {
                if (objXmlLifestyleQuality["contributetolimit"].InnerText == "no")
                    _blnContributeToLimit = false;
            }
            _strSource = objXmlLifestyleQuality["source"].InnerText;
            _strPage = objXmlLifestyleQuality["page"].InnerText;
            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
                XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/LifestyleQuality[name = \"" + _strName + "\"]");
                if (objLifestyleQualityNode != null)
                {
                    if (objLifestyleQualityNode["translate"] != null)
                        _strAltName = objLifestyleQualityNode["translate"].InnerText;
                    if (objLifestyleQualityNode["altpage"] != null)
                        _strAltPage = objLifestyleQualityNode["altpage"].InnerText;
                }
            }

            // If the item grants a bonus, pass the information to the Improvement Manager.
            if (objXmlLifestyleQuality.InnerXml.Contains("<bonus>"))
            {
                ImprovementManager objImprovementManager = new ImprovementManager(objCharacter);
                if (!objImprovementManager.CreateImprovements(Improvement.ImprovementSource.Quality, _guiID.ToString(), objXmlLifestyleQuality["bonus"], false, 1, DisplayNameShort))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (objImprovementManager.SelectedValue != "")
                {
                    _strExtra = objImprovementManager.SelectedValue;
                    objNode.Text += " (" + objImprovementManager.SelectedValue + ")";
                }
            }
            objNode.Text = DisplayName;
            objNode.Tag = InternalId;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            objWriter.WriteStartElement("LifestyleQuality");
            objWriter.WriteElementString("guid", _guiID.ToString());
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("cost", _strCost);
            objWriter.WriteElementString("lp", _intLP.ToString());
            objWriter.WriteElementString("contributetolimit", _blnContributeToLimit.ToString());
            objWriter.WriteElementString("print", _blnPrint.ToString());
            objWriter.WriteElementString("LifestyleQualitytype", _objLifestyleQualityType.ToString());
            objWriter.WriteElementString("LifestyleQualitysource", _objLifestyleQualitySource.ToString());
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            if (_nodBonus != null)
                objWriter.WriteRaw("<bonus>" + _nodBonus.InnerXml + "</bonus>");
            else
                objWriter.WriteElementString("bonus", "");
            objWriter.WriteElementString("notes", _strNotes);
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Attribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            _guiID = Guid.Parse(objNode["guid"].InnerText);
            _strName = objNode["name"].InnerText;
            _strExtra = objNode["extra"].InnerText;
            _intLP = Convert.ToInt32(objNode["lp"].InnerText);
            _intCost = Convert.ToInt32(objNode["cost"].InnerText);
            _blnContributeToLimit = Convert.ToBoolean(objNode["contributetolimit"].InnerText);
            _blnPrint = Convert.ToBoolean(objNode["print"].InnerText);
            _objLifestyleQualityType = ConvertToLifestyleQualityType(objNode["LifestyleQualitytype"].InnerText);
            _objLifestyleQualitySource = ConvertToLifestyleQualitySource(objNode["LifestyleQualitysource"].InnerText);
            _strSource = objNode["source"].InnerText;
            _strPage = objNode["page"].InnerText;
            _nodBonus = objNode["bonus"];
            try
            {
                _strNotes = objNode["notes"].InnerText;
            }
            catch
            {
            }

            if (GlobalOptions.Instance.Language != "en-us")
            {
                XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
                XmlNode objLifestyleQualityNode = objXmlDocument.SelectSingleNode("/chummer/qualities/LifestyleQuality[name = \"" + _strName + "\"]");
                if (objLifestyleQualityNode != null)
                {
                    if (objLifestyleQualityNode["translate"] != null)
                        _strAltName = objLifestyleQualityNode["translate"].InnerText;
                    if (objLifestyleQualityNode["altpage"] != null)
                        _strAltPage = objLifestyleQualityNode["altpage"].InnerText;
                }
            }
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Print(XmlTextWriter objWriter)
        {
            if (_blnPrint)
            {
                objWriter.WriteStartElement("LifestyleQuality");
                objWriter.WriteElementString("name", DisplayNameShort);
                objWriter.WriteElementString("extra", LanguageManager.Instance.TranslateExtra(_strExtra));
                objWriter.WriteElementString("lp", _intLP.ToString());
                objWriter.WriteElementString("cost", _intCost.ToString());
                string strLifestyleQualityType = _objLifestyleQualityType.ToString();
                if (GlobalOptions.Instance.Language != "en-us")
                {
                    XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");

                    XmlNode objNode = objXmlDocument.SelectSingleNode("/chummer/categories/category[. = \"" + strLifestyleQualityType + "\"]");
                    if (objNode != null)
                    {
                        if (objNode.Attributes["translate"] != null)
                            strLifestyleQualityType = objNode.Attributes["translate"].InnerText;
                    }
                }
                objWriter.WriteElementString("LifestyleQualitytype", strLifestyleQualityType);
                objWriter.WriteElementString("LifestyleQualitytype_english", _objLifestyleQualityType.ToString());
                objWriter.WriteElementString("LifestyleQualitysource", _objLifestyleQualitySource.ToString());
                objWriter.WriteElementString("source", _objCharacter.Options.LanguageBookShort(_strSource));
                objWriter.WriteElementString("page", Page);
                if (_objCharacter.Options.PrintNotes)
                    objWriter.WriteElementString("notes", _strNotes);
                objWriter.WriteEndElement();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Internal identifier which will be used to identify this LifestyleQuality in the Improvement system.
        /// </summary>
        public string InternalId
        {
            get
            {
                return _guiID.ToString();
            }
        }

        /// <summary>
        /// LifestyleQuality's name.
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

        /// <summary>
        /// Extra information that should be applied to the name, like a linked Attribute.
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
        /// Page Number.
        /// </summary>
        public string Page
        {
            get
            {
                string strReturn = _strPage;
                if (_strAltPage != string.Empty)
                    strReturn = _strAltPage;

                return strReturn;
            }
            set
            {
                _strPage = value;
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
        /// LifestyleQuality Type.
        /// </summary>
        public QualityType Type
        {
            get
            {
                return _objLifestyleQualityType;
            }
            set
            {
                _objLifestyleQualityType = value;
            }
        }

        /// <summary>
        /// Source of the LifestyleQuality.
        /// </summary>
        public QualitySource OriginSource
        {
            get
            {
                return _objLifestyleQualitySource;
            }
            set
            {
                _objLifestyleQualitySource = value;
            }
        }

        /// <summary>
        /// Number of Build Points the LifestyleQuality costs.
        /// </summary>
        public int LP
        {
            get
            {
                return _intLP;
            }
            set
            {
                _intLP = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort
        {
            get
            {
                string strReturn = _strName;
                if (_strAltName != string.Empty)
                    strReturn = _strAltName;

                return strReturn;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName
        {
            get
            {
                string strReturn = DisplayNameShort;

                if (_strExtra != "")
                {
                    LanguageManager.Instance.Load(GlobalOptions.Instance.Language, this);
                    // Attempt to retrieve the Attribute name.
                    try
                    {
                        if (LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") != "")
                            strReturn += " (" + LanguageManager.Instance.GetString("String_Attribute" + _strExtra + "Short") + ")";
                        else
                            strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
                    }
                    catch
                    {
                        strReturn += " (" + LanguageManager.Instance.TranslateExtra(_strExtra) + ")";
                    }
                }
                return strReturn;
            }
        }

        /// <summary>
        /// Whether or not the LifestyleQuality appears on the printouts.
        /// </summary>
        public bool AllowPrint
        {
            get
            {
                return _blnPrint;
            }
            set
            {
                _blnPrint = value;
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
        #endregion
    }
}