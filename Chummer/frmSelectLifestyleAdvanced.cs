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
            _objLifestyle.Cost = CalculateValues(false);
            _objLifestyle.Roommates = Convert.ToInt32(nudRoommates.Value);
            _objLifestyle.Percentage = Convert.ToInt32(nudPercentage.Value);
            _objLifestyle.LifestyleQualities.Clear();

            // Get the starting Nuyen information.
            XmlNode objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + cboBaseLifestyle.SelectedValue + "\"]");
            _objLifestyle.Dice = Convert.ToInt32(objXmlAspect["dice"].InnerText);
            _objLifestyle.Multiplier = Convert.ToInt32(objXmlAspect["multiplier"].InnerText);
            _objLifestyle.StyleType = _objType;

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
        private int CalculateValues(bool blnIncludePercentage = true)
        {
            if (_blnSkipRefresh)
                return 0;

            int intLP = 0;
            int intNuyen = 0;

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
                intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
            }

            // Calculate the cost of Negative Qualities.
            foreach (TreeNode objNode in treLifestyleQualities.Nodes[1].Nodes)
            {
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objNode.Text.ToString() + "\" and category = \"Negative\"]");
                intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
            }

            // Calculate the cost of Entertainments.
            foreach (TreeNode objNode in treLifestyleQualities.Nodes[2].Nodes)
            {
                objXmlAspect = _objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + objNode.Text.ToString() + "\"]");
                intLP -= Convert.ToInt32(objXmlAspect["lp"].InnerText);
                string[] strLifestyleEntertainments = objXmlAspect["lp"].InnerText.Split(',');
                bool blnLifestyleEntertainmentFree = false;
                foreach (string strLifestyle in strLifestyleEntertainments)
                {
                    if (strLifestyle != objXmlLifestyle.ToString())
                    {
                        blnLifestyleEntertainmentFree = false;
                    }
                    else
                        blnLifestyleEntertainmentFree = true;

                    if (blnLifestyleEntertainmentFree == false)
                    {
                        if (objXmlAspect["cost"] != null)
                        {
                            intNuyen += Convert.ToInt32(objXmlAspect["cost"].InnerText);
                        }
                    }
                }
            }

            if (blnIncludePercentage)
            {
                intNuyen = Convert.ToInt32(Convert.ToDouble(intNuyen, GlobalOptions.Instance.CultureInfo) * (1.0 + Convert.ToDouble(nudRoommates.Value / 10, GlobalOptions.Instance.CultureInfo)));
                intNuyen = Convert.ToInt32(Convert.ToDouble(intNuyen, GlobalOptions.Instance.CultureInfo) * Convert.ToDouble(nudPercentage.Value / 100, GlobalOptions.Instance.CultureInfo));
            }
            intNuyen += Convert.ToInt32(objXmlLifestyle["cost"].InnerText);
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

            XmlDocument objXmlDocument = XmlManager.Instance.Load("lifestyles.xml");
            XmlNode objXmlQuality = objXmlDocument.SelectSingleNode("/chummer/qualities/quality[name = \"" + frmSelectLifestyleQuality.SelectedQuality + "\"]");

            TreeNode objNode = new TreeNode();
            List<Weapon> objWeapons = new List<Weapon>();
            List<TreeNode> objWeaponNodes = new List<TreeNode>();
            LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

            //objQuality.Create(objXmlQuality, _objCharacter, QualitySource.Selected, objNode, objWeapons, objWeaponNodes);
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
                if (objQuality.Type == LifestyleQualityType.Positive)
                {
                    treLifestyleQualities.Nodes[0].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[0].Expand();
                }
                else if (objQuality.Type == LifestyleQualityType.Negative)
                {
                    treLifestyleQualities.Nodes[1].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[1].Expand();
                }
                else
                {
                    treLifestyleQualities.Nodes[2].Nodes.Add(objNode);
                    treLifestyleQualities.Nodes[2].Expand();
                }
                _objCharacter.LifestyleQualities.Add(objQuality);

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
}