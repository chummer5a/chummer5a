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
  using System.Collections.Specialized;
using System.Windows.Forms;
using System.Xml;
  using Chummer.Backend.Equipment;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class frmSelectLifestyleAdvanced : Form
    {
        private readonly Character _objCharacter;
        private readonly Lifestyle _objLifestyle;
        private readonly XmlDocument _xmlDocument;
        private bool _blnSkipRefresh = true;

        #region Control Events
        public frmSelectLifestyleAdvanced(Character objCharacter, Lifestyle objLifestyle)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            _objLifestyle = objLifestyle;
            // Load the Lifestyles information.
            _xmlDocument = _objCharacter.LoadData("lifestyles.xml");
        }

        private void frmSelectLifestyleAdvanced_FormClosing(object sender, FormClosingEventArgs e)
        {
            _objLifestyle.LifestyleQualities.CollectionChanged -= LifestyleQualitiesOnCollectionChanged;
            _objLifestyle.FreeGrids.CollectionChanged -= FreeGridsOnCollectionChanged;
            Dispose(true);
        }

        private void FreeGridsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                ResetLifestyleQualitiesTree();
                return;
            }
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Move)
                return;

            TreeNode nodFreeGridsRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_FreeMatrixGrids", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (LifestyleQuality objFreeGrid in notifyCollectionChangedEventArgs.NewItems)
                        {
                            TreeNode objNode = objFreeGrid.CreateTreeNode();
                            if (objNode == null)
                                return;
                            if (nodFreeGridsRoot == null)
                            {
                                nodFreeGridsRoot = new TreeNode
                                {
                                    Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                                    Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids")
                                };
                                treLifestyleQualities.Nodes.Add(nodFreeGridsRoot);
                                nodFreeGridsRoot.Expand();
                            }

                            TreeNodeCollection lstParentNodeChildren = nodFreeGridsRoot.Nodes;
                            int intNodesCount = lstParentNodeChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }

                            lstParentNodeChildren.Insert(intTargetIndex, objNode);
                            treLifestyleQualities.SelectedNode = objNode;
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (LifestyleQuality objFreeGrid in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treLifestyleQualities.FindNodeByTag(objFreeGrid);
                            if (objNode != null)
                            {
                                TreeNode objParent = objNode.Parent;
                                objNode.Remove();
                                if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                    objParent.Remove();
                            }
                        }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        List<TreeNode> lstOldParents = new List<TreeNode>();
                        foreach (LifestyleQuality objFreeGrid in notifyCollectionChangedEventArgs.OldItems)
                        {
                            TreeNode objNode = treLifestyleQualities.FindNodeByTag(objFreeGrid);
                            if (objNode != null)
                            {
                                if (objNode.Parent != null)
                                    lstOldParents.Add(objNode.Parent);
                                objNode.Remove();
                            }
                        }
                        foreach (LifestyleQuality objFreeGrid in notifyCollectionChangedEventArgs.NewItems)
                        {
                            TreeNode objNode = objFreeGrid.CreateTreeNode();
                            if (objNode == null)
                                return;
                            if (nodFreeGridsRoot == null)
                            {
                                nodFreeGridsRoot = new TreeNode
                                {
                                    Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                                    Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids")
                                };
                                treLifestyleQualities.Nodes.Add(nodFreeGridsRoot);
                                nodFreeGridsRoot.Expand();
                            }

                            TreeNodeCollection lstParentNodeChildren = nodFreeGridsRoot.Nodes;
                            int intNodesCount = lstParentNodeChildren.Count;
                            int intTargetIndex = 0;
                            for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                            {
                                if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                                {
                                    break;
                                }
                            }

                            lstParentNodeChildren.Insert(intTargetIndex, objNode);
                            treLifestyleQualities.SelectedNode = objNode;
                        }
                        foreach (TreeNode objOldParent in lstOldParents)
                        {
                            if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                objOldParent.Remove();
                        }
                        break;
                    }
            }
        }

        private void ResetLifestyleQualitiesTree()
        {
            TreeNode nodPositiveQualityRoot = null;
            TreeNode nodNegativeQualityRoot = null;
            TreeNode nodEntertainmentsRoot = null;
            TreeNode nodFreeGridsRoot = null;

            string strSelectedNode = treLifestyleQualities.SelectedNode?.Tag.ToString();

            treLifestyleQualities.Nodes.Clear();

            foreach (LifestyleQuality objQuality in _objLifestyle.LifestyleQualities)
            {
                TreeNode objNode = objQuality.CreateTreeNode();
                if (objNode == null)
                    continue;
                switch (objQuality.Type)
                {
                    case QualityType.Positive:
                        if (nodPositiveQualityRoot == null)
                        {
                            nodPositiveQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_PositiveQualities",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_PositiveQualities")
                            };
                            treLifestyleQualities.Nodes.Insert(0, nodPositiveQualityRoot);
                            nodPositiveQualityRoot.Expand();
                        }
                        nodPositiveQualityRoot.Nodes.Add(objNode);
                        break;
                    case QualityType.Negative:
                        if (nodNegativeQualityRoot == null)
                        {
                            nodNegativeQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_NegativeQualities",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_NegativeQualities")
                            };
                            treLifestyleQualities.Nodes.Insert(nodPositiveQualityRoot == null ? 0 : 1, nodNegativeQualityRoot);
                            nodNegativeQualityRoot.Expand();
                        }
                        nodNegativeQualityRoot.Nodes.Add(objNode);
                        break;
                    default:
                        if (nodEntertainmentsRoot == null)
                        {
                            nodEntertainmentsRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_Entertainments",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_Entertainments")
                            };
                            treLifestyleQualities.Nodes.Insert((nodPositiveQualityRoot == null ? 0 : 1) + (nodNegativeQualityRoot == null ? 0 : 1), nodEntertainmentsRoot);
                            nodEntertainmentsRoot.Expand();
                        }
                        nodEntertainmentsRoot.Nodes.Add(objNode);
                        break;
                }
            }

            foreach (LifestyleQuality objFreeGrid in _objLifestyle.FreeGrids)
            {
                TreeNode objNode = objFreeGrid.CreateTreeNode();
                if (objNode == null)
                    return;
                if (nodFreeGridsRoot == null)
                {
                    nodFreeGridsRoot = new TreeNode
                    {
                        Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                        Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids")
                    };
                    treLifestyleQualities.Nodes.Add(nodFreeGridsRoot);
                    nodFreeGridsRoot.Expand();
                }
                nodFreeGridsRoot.Nodes.Add(objNode);
            }

            treLifestyleQualities.SortCustomAlphabetically(strSelectedNode);
        }

        private void LifestyleQualitiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                ResetLifestyleQualitiesTree();
                return;
            }
            if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Move)
                return;

            TreeNode nodPositiveQualityRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_PositiveQualities", false);
            TreeNode nodNegativeQualityRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_NegativeQualities", false);
            TreeNode nodEntertainmentsRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_Entertainments", false);

            switch (notifyCollectionChangedEventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (LifestyleQuality objQuality in notifyCollectionChangedEventArgs.NewItems)
                    {
                        AddToTree(objQuality);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (LifestyleQuality objQuality in notifyCollectionChangedEventArgs.OldItems)
                    {
                        TreeNode objNode = treLifestyleQualities.FindNodeByTag(objQuality);
                        if (objNode != null)
                        {
                            TreeNode objParent = objNode.Parent;
                            objNode.Remove();
                            if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                objParent.Remove();
                        }
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    List<TreeNode> lstOldParents = new List<TreeNode>();
                    foreach (LifestyleQuality objQuality in notifyCollectionChangedEventArgs.OldItems)
                    {
                        TreeNode objNode = treLifestyleQualities.FindNodeByTag(objQuality);
                        if (objNode != null)
                        {
                            if (objNode.Parent != null)
                                lstOldParents.Add(objNode.Parent);
                            objNode.Remove();
                        }
                    }
                    foreach (LifestyleQuality objQuality in notifyCollectionChangedEventArgs.NewItems)
                    {
                        AddToTree(objQuality);
                    }
                    foreach (TreeNode objOldParent in lstOldParents)
                    {
                        if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                            objOldParent.Remove();
                    }
                    break;
                }
            }

            void AddToTree(LifestyleQuality objQuality)
            {
                TreeNode objNode = objQuality.CreateTreeNode();
                if (objNode == null)
                    return;
                TreeNode objParentNode;
                switch (objQuality.Type)
                {
                    case QualityType.Positive:
                        if (nodPositiveQualityRoot == null)
                        {
                            nodPositiveQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_PositiveQualities",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_PositiveQualities")
                            };
                            treLifestyleQualities.Nodes.Insert(0, nodPositiveQualityRoot);
                            nodPositiveQualityRoot.Expand();
                        }
                        objParentNode = nodPositiveQualityRoot;
                        break;
                    case QualityType.Negative:
                        if (nodNegativeQualityRoot == null)
                        {
                            nodNegativeQualityRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_NegativeQualities",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_NegativeQualities")
                            };
                            treLifestyleQualities.Nodes.Insert(nodPositiveQualityRoot == null ? 0 : 1, nodNegativeQualityRoot);
                            nodNegativeQualityRoot.Expand();
                        }
                        objParentNode = nodNegativeQualityRoot;
                        break;
                    default:
                        if (nodEntertainmentsRoot == null)
                        {
                            nodEntertainmentsRoot = new TreeNode
                            {
                                Tag = "Node_SelectAdvancedLifestyle_Entertainments",
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_Entertainments")
                            };
                            treLifestyleQualities.Nodes.Insert((nodPositiveQualityRoot == null ? 0 : 1) + (nodNegativeQualityRoot == null ? 0 : 1), nodEntertainmentsRoot);
                            nodEntertainmentsRoot.Expand();
                        }
                        objParentNode = nodEntertainmentsRoot;
                        break;
                }

                TreeNodeCollection lstParentNodeChildren = objParentNode.Nodes;
                int intNodesCount = lstParentNodeChildren.Count;
                int intTargetIndex = 0;
                for (; intTargetIndex < intNodesCount; ++intTargetIndex)
                {
                    if (CompareTreeNodes.CompareText(lstParentNodeChildren[intTargetIndex], objNode) >= 0)
                    {
                        break;
                    }
                }

                lstParentNodeChildren.Insert(intTargetIndex, objNode);
                treLifestyleQualities.SelectedNode = objNode;
            }
        }

        private void frmSelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            List<ListItem> lstLifestyles = new List<ListItem>();
            using (XmlNodeList xmlLifestyleList = _xmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + _objCharacter.Options.BookXPath() + "]"))
            {
                if (xmlLifestyleList?.Count > 0)
                {
                    foreach (XmlNode objXmlLifestyle in xmlLifestyleList)
                    {
                        string strLifestyleName = objXmlLifestyle["name"]?.InnerText;

                        if (!string.IsNullOrEmpty(strLifestyleName) &&
                            strLifestyleName != "ID ERROR. Re-add life style to fix" &&
                            (StyleType == LifestyleType.Advanced || objXmlLifestyle["slp"]?.InnerText == "remove") &&
                            !strLifestyleName.Contains("Hospitalized") &&
                            _objCharacter.Options.Books.Contains(objXmlLifestyle["source"]?.InnerText))
                        {
                            lstLifestyles.Add(new ListItem(strLifestyleName, objXmlLifestyle["translate"]?.InnerText ?? strLifestyleName));
                        }
                    }
                }
            }

            chkBonusLPRandomize.DoNegatableDatabinding("Checked",_objLifestyle, nameof(Lifestyle.AllowBonusLP));
            nudBonusLP.DoDatabinding("Value", _objLifestyle,nameof(Lifestyle.BonusLP));
            ResetLifestyleQualitiesTree();
            cboBaseLifestyle.BeginUpdate();
            cboBaseLifestyle.PopulateWithListItems(lstLifestyles);
            cboBaseLifestyle.SelectedValue = _objLifestyle.BaseLifestyle;
            if (cboBaseLifestyle.SelectedIndex == -1)
                cboBaseLifestyle.SelectedIndex = 0;
            cboBaseLifestyle.EndUpdate();
            txtLifestyleName.DoDatabinding("Text",_objLifestyle,nameof(Lifestyle.Name));
            nudRoommates.DoDatabinding("Value",_objLifestyle,nameof(Lifestyle.Roommates));
            nudPercentage.DoDatabinding("Value", _objLifestyle, nameof(Lifestyle.Percentage));
            nudArea.DoDatabinding("Value", _objLifestyle, nameof(Lifestyle.BindableArea));
            nudComforts.DoDatabinding("Value", _objLifestyle, nameof(Lifestyle.BindableComforts));
            nudSecurity.DoDatabinding("Value", _objLifestyle, nameof(Lifestyle.BindableSecurity));
            nudArea.DoOneWayDataBinding("Maximum", _objLifestyle, nameof(Lifestyle.AreaDelta));
            nudComforts.DoOneWayDataBinding("Maximum", _objLifestyle, nameof(Lifestyle.ComfortsDelta));
            nudSecurity.DoOneWayDataBinding("Maximum", _objLifestyle, nameof(Lifestyle.SecurityDelta));
            cboBaseLifestyle.DoDatabinding("SelectedValue",_objLifestyle,nameof(Lifestyle.BaseLifestyle));
            chkTrustFund.DoDatabinding("Checked", _objLifestyle, nameof(Lifestyle.TrustFund));
            chkTrustFund.DoOneWayDataBinding("Enabled",_objLifestyle,nameof(Lifestyle.IsTrustFundEligible));
            chkPrimaryTenant.DoDatabinding("Checked", _objLifestyle, nameof(Lifestyle.PrimaryTenant));
            lblCost.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.DisplayTotalMonthlyCost));
            lblArea.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.FormattedArea));
            lblComforts.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.FormattedComforts));
            lblSecurity.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.FormattedSecurity));
            lblAreaTotal.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.TotalArea));
            lblComfortTotal.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.TotalComforts));
            lblSecurityTotal.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.TotalSecurity));
            lblTotalLP.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.TotalLP));

            _objLifestyle.LifestyleQualities.CollectionChanged += LifestyleQualitiesOnCollectionChanged;
            _objLifestyle.FreeGrids.CollectionChanged += FreeGridsOnCollectionChanged;


            // Populate the City ComboBox
            List<ListItem> lstCity = new List<ListItem>();

            using (XmlNodeList xmlCityList = _xmlDocument.SelectNodes("/chummer/cities/city"))
            {
                if (xmlCityList?.Count > 0)
                {
                    foreach (XmlNode objXmlCity in xmlCityList)
                    {
                        string strName = objXmlCity["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
                        lstCity.Add(new ListItem(strName, objXmlCity["translate"]?.InnerText ?? strName));
                    }
                }
            }

            cboCity.BeginUpdate();
            cboCity.PopulateWithListItems(lstCity);
            cboCity.DoDatabinding("SelectedValue", _objLifestyle, nameof(Lifestyle.City));
            cboCity.EndUpdate();

            //Populate District and Borough ComboBox for the first time
            RefreshDistrictList();
            RefreshBoroughList();

            _blnSkipRefresh = false;
            RefreshSelectedLifestyle();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void chkTrustFund_Changed(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (chkTrustFund.Checked)
            {
                nudRoommates.Value = 0;
            }

            nudRoommates.Enabled = !chkTrustFund.Checked;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            AcceptForm();
        }

        private void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            RefreshSelectedLifestyle();
        }

        private void cboCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            RefreshDistrictList();
        }

        private void cboDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            RefreshBoroughList();
            _objLifestyle.District = cboDistrict.SelectedValue?.ToString() ?? string.Empty;
        }

        private void cboBorough_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            _objLifestyle.Borough = cboBorough.SelectedValue?.ToString() ?? string.Empty;
        }

        private void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (nudRoommates.Value == 0 && !chkPrimaryTenant.Checked)
            {
                chkPrimaryTenant.Checked = true;
            }
        }

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                using (frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter, cboBaseLifestyle.SelectedValue.ToString(), _objLifestyle.LifestyleQualities))
                {
                    frmSelectLifestyleQuality.ShowDialog(this);

                    // Don't do anything else if the form was canceled.
                    if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                        return;
                    blnAddAgain = frmSelectLifestyleQuality.AddAgain;

                    XmlNode objXmlQuality = _xmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + frmSelectLifestyleQuality.SelectedQuality.CleanXPath() + "]");

                    LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

                    objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                    objQuality.Free = frmSelectLifestyleQuality.FreeCost;
                    //objNode.ContextMenuStrip = cmsQuality;
                    if (objQuality.InternalId.IsEmptyGuid())
                        continue;

                    _objLifestyle.LifestyleQualities.Add(objQuality);
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treLifestyleQualities.SelectedNode == null || treLifestyleQualities.SelectedNode.Level == 0 || treLifestyleQualities.SelectedNode.Parent.Name == "nodFreeMatrixGrids")
                return;

            if (!(treLifestyleQualities.SelectedNode.Tag is LifestyleQuality objQuality)) return;
            if (objQuality.Name == "Not a Home" && cboBaseLifestyle.SelectedValue?.ToString() == "Bolt Hole")
            {
                return;
            }
            _objLifestyle.LifestyleQualities.Remove(objQuality);
        }

        private void treLifestyleQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality)
            {
                lblQualityLPLabel.Visible = true;
                lblQualityCostLabel.Visible = true;
                lblQualitySourceLabel.Visible = true;
                chkQualityContributesLP.Visible = true;
                chkQualityContributesLP.Enabled = !(objQuality.Free || objQuality.OriginSource == QualitySource.BuiltIn);

                _blnSkipRefresh = true;
                chkQualityContributesLP.Checked = objQuality.ContributesLP;
                _blnSkipRefresh = false;

                lblQualityLp.Text = objQuality.LP.ToString(GlobalOptions.CultureInfo);
                lblQualityCost.Text = objQuality.Cost.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
                objQuality.SetSourceDetail(lblQualitySource);
                cmdDeleteQuality.Enabled = !(objQuality.Free || objQuality.OriginSource == QualitySource.BuiltIn);
            }
            else
            {
                lblQualityLPLabel.Visible = false;
                lblQualityCostLabel.Visible = false;
                lblQualitySourceLabel.Visible = false;
                chkQualityContributesLP.Visible = false;
                lblQualityLp.Text = string.Empty;
                lblQualityCost.Text = string.Empty;
                lblQualitySource.Text = string.Empty;
                lblQualitySource.SetToolTip(null);
                cmdDeleteQuality.Enabled = false;
            }
        }

        private void chkQualityContributesLP_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            if (!(treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality))
                return;
            objQuality.ContributesLP = chkQualityContributesLP.Checked;
            lblQualityLp.Text = objQuality.LP.ToString(GlobalOptions.CultureInfo);
        }

        private void chkTravelerBonusLPRandomize_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBonusLPRandomize.Checked)
            {
                nudBonusLP.Enabled = false;
                nudBonusLP.Value = GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
            }
            else
            {
                nudBonusLP.Enabled = true;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain { get; private set; }

        /// <summary>
        /// Lifestyle that was created in the dialogue.
        /// </summary>
        public Lifestyle SelectedLifestyle { get; set; }

        /// <summary>
        /// Type of Lifestyle to create.
        /// </summary>
        public LifestyleType StyleType { get; set; } = LifestyleType.Advanced;

        #endregion

        #region Methods
        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            if (string.IsNullOrEmpty(txtLifestyleName.Text))
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_SelectAdvancedLifestyle_LifestyleName"), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_objLifestyle.TotalLP < 0)
            {
                Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_SelectAdvancedLifestyle_OverLPLimit"), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_OverLPLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
            XmlNode objXmlLifestyle = _xmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = " + strBaseLifestyle.CleanXPath() + "]");
            if (objXmlLifestyle == null)
                return;
            _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
            _objLifestyle.Name = txtLifestyleName.Text;
            _objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"]?.InnerText, GlobalOptions.InvariantCultureInfo);
            _objLifestyle.Percentage = nudPercentage.Value;
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            _objLifestyle.Area = nudArea.ValueAsInt;
            _objLifestyle.Comforts = nudComforts.ValueAsInt;
            _objLifestyle.Security = nudSecurity.ValueAsInt;
            _objLifestyle.TrustFund = chkTrustFund.Checked;
            _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : nudRoommates.ValueAsInt;
            _objLifestyle.PrimaryTenant = chkPrimaryTenant.Checked;
            _objLifestyle.BonusLP = nudBonusLP.ValueAsInt;

            // Get the starting Nuyen information.
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText, GlobalOptions.InvariantCultureInfo);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalOptions.InvariantCultureInfo);
            _objLifestyle.StyleType = StyleType;
            SelectedLifestyle = _objLifestyle;
            DialogResult = DialogResult.OK;
        }

        private void RefreshSelectedLifestyle()
        {
            if (_blnSkipRefresh)
                return;

            string strBaseLifestyle = cboBaseLifestyle.SelectedValue?.ToString() ?? string.Empty;
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            XmlNode xmlAspect = _objLifestyle.GetNode();
            if (xmlAspect != null)
            {
                string strSource = xmlAspect["source"]?.InnerText ?? string.Empty;
                string strPage = xmlAspect["altpage"]?.InnerText ?? xmlAspect["page"]?.InnerText ?? string.Empty;
                if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                {
                    string strSpace = LanguageManager.GetString("String_Space");
                    lblSource.Text = _objCharacter.LanguageBookShort(strSource) + strSpace + strPage;
                    lblSource.SetToolTip(_objCharacter.LanguageBookLong(strSource) + strSpace + LanguageManager.GetString("String_Page") + strSpace + strPage);
                }
                else
                {
                    lblSource.Text = string.Empty;
                    lblSource.SetToolTip(string.Empty);
                }
            }
            else
            {
                lblSource.Text = string.Empty;
                lblSource.SetToolTip(string.Empty);
            }

            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);

            // Characters with the Trust Fund Quality can have the lifestyle discounted.
            if (_objLifestyle.IsTrustFundEligible)
            {
                chkTrustFund.Visible = true;
                chkTrustFund.Checked = _objLifestyle.TrustFund;
            }
            else
            {
                chkTrustFund.Checked = false;
                chkTrustFund.Visible = false;
            }

            if (_objLifestyle.AllowBonusLP)
            {
                lblBonusLP.Visible = true;
                nudBonusLP.Visible = true;
                chkBonusLPRandomize.Visible = true;

                if (chkBonusLPRandomize.Checked)
                {
                    nudBonusLP.Enabled = false;
                    _blnSkipRefresh = true;
                    nudBonusLP.Value = GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
                    _blnSkipRefresh = false;
                }
                else
                {
                    nudBonusLP.Enabled = true;
                }
            }
            else
            {
                lblBonusLP.Visible = false;
                nudBonusLP.Visible = false;
                nudBonusLP.Value = 0;
                chkBonusLPRandomize.Visible = false;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        /// <summary>
        /// Populates The District list after a City was selected
        /// </summary>
        private void RefreshDistrictList()
        {
            string strSelectedCityRefresh = cboCity.SelectedValue?.ToString() ?? string.Empty;
            List<ListItem> lstDistrict = new List<ListItem>();
            using (XmlNodeList xmlDistrictList = _xmlDocument.SelectNodes("/chummer/cities/city[name = " + strSelectedCityRefresh.CleanXPath() + "]/district"))
            {
                if (xmlDistrictList?.Count > 0)
                {
                    foreach (XmlNode objXmlDistrict in xmlDistrictList)
                    {
                        string strName = objXmlDistrict["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
                        lstDistrict.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                    }
                }
            }

            cboDistrict.BeginUpdate();
            cboDistrict.PopulateWithListItems(lstDistrict);
            cboDistrict.EndUpdate();
        }

        /// <summary>
        /// Refreshes the BoroughList based on the selected District to generate a cascading dropdown menu
        /// </summary>
        private void RefreshBoroughList()
        {
            string strSelectedCityRefresh = cboCity.SelectedValue?.ToString() ?? string.Empty;
            string strSelectedDistrictRefresh = cboDistrict.SelectedValue?.ToString() ?? string.Empty;
            List<ListItem> lstBorough = new List<ListItem>();
            using (XmlNodeList xmlBoroughList = _xmlDocument.SelectNodes("/chummer/cities/city[name = " + strSelectedCityRefresh.CleanXPath() + "]/district[name = " + strSelectedDistrictRefresh.CleanXPath() + "]/borough"))
            {
                if (xmlBoroughList?.Count > 0)
                {
                    foreach (XmlNode objXmlDistrict in xmlBoroughList)
                    {
                        string strName = objXmlDistrict["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
                        lstBorough.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                    }
                }
            }

            cboBorough.BeginUpdate();
            cboBorough.PopulateWithListItems(lstBorough);
            cboBorough.EndUpdate();
        }
        #endregion
    }
}
