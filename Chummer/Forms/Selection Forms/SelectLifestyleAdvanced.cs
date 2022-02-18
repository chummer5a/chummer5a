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
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Chummer.Backend.Equipment;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class SelectLifestyleAdvanced : Form
    {
        private readonly Character _objCharacter;
        private readonly Lifestyle _objLifestyle;
        private readonly XmlDocument _xmlDocument;
        private bool _blnSkipRefresh = true;

        #region Control Events

        public SelectLifestyleAdvanced(Character objCharacter, Lifestyle objLifestyle)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            _objLifestyle = objLifestyle;
            // Load the Lifestyles information.
            _xmlDocument = _objCharacter.LoadData("lifestyles.xml");
        }

        private void SelectLifestyleAdvanced_FormClosing(object sender, FormClosingEventArgs e)
        {
            _objLifestyle.LifestyleQualities.CollectionChanged -= LifestyleQualitiesOnCollectionChanged;
            _objLifestyle.FreeGrids.CollectionChanged -= FreeGridsOnCollectionChanged;
        }

        private async void LifestyleQualitiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    return;
                case NotifyCollectionChangedAction.Reset:
                    await ResetLifestyleQualitiesTree();
                    return;
                default:
                {
                        TreeNode nodPositiveQualityRoot
                        = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_PositiveQualities", false);
                    TreeNode nodNegativeQualityRoot
                        = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_NegativeQualities", false);
                    TreeNode nodEntertainmentsRoot
                        = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_Entertainments", false);

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            foreach (LifestyleQuality objQuality in e.NewItems)
                            {
                                await AddToTree(objQuality);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (LifestyleQuality objQuality in e.OldItems)
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
                            List<TreeNode> lstOldParents = new List<TreeNode>(e.OldItems.Count);
                            foreach (LifestyleQuality objQuality in e.OldItems)
                            {
                                TreeNode objNode = treLifestyleQualities.FindNodeByTag(objQuality);
                                if (objNode != null)
                                {
                                    if (objNode.Parent != null)
                                        lstOldParents.Add(objNode.Parent);
                                    objNode.Remove();
                                }
                            }

                            foreach (LifestyleQuality objQuality in e.NewItems)
                            {
                                await AddToTree(objQuality);
                            }

                            foreach (TreeNode objOldParent in lstOldParents)
                            {
                                if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                    objOldParent.Remove();
                            }

                            break;
                        }
                    }

                    break;

                    async ValueTask AddToTree(LifestyleQuality objQuality)
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
                                        Text = await LanguageManager.GetStringAsync(
                                            "Node_SelectAdvancedLifestyle_PositiveQualities")
                                    };
                                    // ReSharper disable once AssignNullToNotNullAttribute
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
                                        Text = await LanguageManager.GetStringAsync(
                                            "Node_SelectAdvancedLifestyle_NegativeQualities")
                                    };
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    treLifestyleQualities.Nodes.Insert(nodPositiveQualityRoot == null ? 0 : 1,
                                                                       nodNegativeQualityRoot);
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
                                        Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_Entertainments")
                                    };
                                    treLifestyleQualities.Nodes.Insert(
                                        (nodPositiveQualityRoot == null ? 0 : 1)
                                        // ReSharper disable once AssignNullToNotNullAttribute
                                        + (nodNegativeQualityRoot == null ? 0 : 1), nodEntertainmentsRoot);
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
            }
        }

        private async void FreeGridsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    return;
                case NotifyCollectionChangedAction.Reset:
                    await ResetLifestyleQualitiesTree();
                    return;
                default:
                    TreeNode nodFreeGridsRoot = treLifestyleQualities.FindNode("Node_SelectAdvancedLifestyle_FreeMatrixGrids", false);
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (LifestyleQuality objFreeGrid in e.NewItems)
                                {
                                    TreeNode objNode = objFreeGrid.CreateTreeNode();
                                    if (objNode == null)
                                        return;
                                    if (nodFreeGridsRoot == null)
                                    {
                                        nodFreeGridsRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                                            Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_FreeMatrixGrids")
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
                            foreach (LifestyleQuality objFreeGrid in e.OldItems)
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
                                List<TreeNode> lstOldParents = new List<TreeNode>(e.OldItems.Count);
                                foreach (LifestyleQuality objFreeGrid in e.OldItems)
                                {
                                    TreeNode objNode = treLifestyleQualities.FindNodeByTag(objFreeGrid);
                                    if (objNode != null)
                                    {
                                        if (objNode.Parent != null)
                                            lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }
                                foreach (LifestyleQuality objFreeGrid in e.NewItems)
                                {
                                    TreeNode objNode = objFreeGrid.CreateTreeNode();
                                    if (objNode == null)
                                        return;
                                    if (nodFreeGridsRoot == null)
                                    {
                                        nodFreeGridsRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                                            Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_FreeMatrixGrids")
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

                    break;
            }
        }

        private async ValueTask ResetLifestyleQualitiesTree()
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
                                Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_PositiveQualities")
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
                                Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_NegativeQualities")
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
                                Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_Entertainments")
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
                        Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_FreeMatrixGrids")
                    };
                    treLifestyleQualities.Nodes.Add(nodFreeGridsRoot);
                    nodFreeGridsRoot.Expand();
                }
                nodFreeGridsRoot.Nodes.Add(objNode);
            }

            treLifestyleQualities.SortCustomAlphabetically(strSelectedNode);
        }

        private async void SelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstLifestyles))
            {
                using (XmlNodeList xmlLifestyleList
                       = _xmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + _objCharacter.Settings.BookXPath()
                                                  + ']'))
                {
                    if (xmlLifestyleList?.Count > 0)
                    {
                        foreach (XmlNode objXmlLifestyle in xmlLifestyleList)
                        {
                            string strLifestyleName = objXmlLifestyle["name"]?.InnerText;

                            if (!string.IsNullOrEmpty(strLifestyleName) &&
                                strLifestyleName != "ID ERROR. Re-add life style to fix" &&
                                (StyleType == LifestyleType.Advanced || objXmlLifestyle["slp"]?.InnerText == "remove")
                                &&
                                !strLifestyleName.Contains("Hospitalized") &&
                                _objCharacter.Settings.BookEnabled(objXmlLifestyle["source"]?.InnerText))
                            {
                                lstLifestyles.Add(new ListItem(strLifestyleName,
                                                               objXmlLifestyle["translate"]?.InnerText
                                                               ?? strLifestyleName));
                            }
                        }
                    }
                }

                chkBonusLPRandomize.DoNegatableDataBinding("Checked", _objLifestyle, nameof(Lifestyle.AllowBonusLP));
                nudBonusLP.DoDataBinding("Value", _objLifestyle, nameof(Lifestyle.BonusLP));
                await ResetLifestyleQualitiesTree();
                cboBaseLifestyle.BeginUpdate();
                cboBaseLifestyle.PopulateWithListItems(lstLifestyles);
                cboBaseLifestyle.EndUpdate();
            }

            txtLifestyleName.DoDataBinding("Text", _objLifestyle, nameof(Lifestyle.Name));
            nudRoommates.DoDataBinding("Value", _objLifestyle, nameof(Lifestyle.Roommates));
            nudPercentage.DoDataBinding("Value", _objLifestyle, nameof(Lifestyle.Percentage));
            nudArea.DoDataBinding("Value", _objLifestyle, nameof(Lifestyle.BindableArea));
            nudComforts.DoDataBinding("Value", _objLifestyle, nameof(Lifestyle.BindableComforts));
            nudSecurity.DoDataBinding("Value", _objLifestyle, nameof(Lifestyle.BindableSecurity));
            nudArea.DoOneWayDataBinding("Maximum", _objLifestyle, nameof(Lifestyle.AreaDelta));
            nudComforts.DoOneWayDataBinding("Maximum", _objLifestyle, nameof(Lifestyle.ComfortsDelta));
            nudSecurity.DoOneWayDataBinding("Maximum", _objLifestyle, nameof(Lifestyle.SecurityDelta));
            cboBaseLifestyle.DoDataBinding("SelectedValue", _objLifestyle, nameof(Lifestyle.BaseLifestyle));
            chkTrustFund.DoDataBinding("Checked", _objLifestyle, nameof(Lifestyle.TrustFund));
            chkTrustFund.DoOneWayDataBinding("Enabled", _objLifestyle, nameof(Lifestyle.IsTrustFundEligible));
            chkPrimaryTenant.DoDataBinding("Checked", _objLifestyle, nameof(Lifestyle.PrimaryTenant));
            lblCost.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.DisplayTotalMonthlyCost));
            lblArea.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.FormattedArea));
            lblComforts.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.FormattedComforts));
            lblSecurity.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.FormattedSecurity));
            lblAreaTotal.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.TotalArea));
            lblComfortTotal.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.TotalComforts));
            lblSecurityTotal.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.TotalSecurity));
            lblTotalLP.DoOneWayDataBinding("Text", _objLifestyle, nameof(Lifestyle.TotalLP));

            if (cboBaseLifestyle.SelectedIndex == -1)
                cboBaseLifestyle.SelectedIndex = 0;

            _objLifestyle.LifestyleQualities.CollectionChanged += LifestyleQualitiesOnCollectionChanged;
            _objLifestyle.FreeGrids.CollectionChanged += FreeGridsOnCollectionChanged;

            // Populate the City ComboBox
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstCity))
            {
                using (XmlNodeList xmlCityList = _xmlDocument.SelectNodes("/chummer/cities/city"))
                {
                    if (xmlCityList?.Count > 0)
                    {
                        foreach (XmlNode objXmlCity in xmlCityList)
                        {
                            string strName = objXmlCity["name"]?.InnerText
                                             ?? await LanguageManager.GetStringAsync("String_Unknown");
                            lstCity.Add(new ListItem(strName, objXmlCity["translate"]?.InnerText ?? strName));
                        }
                    }
                }

                cboCity.BeginUpdate();
                cboCity.PopulateWithListItems(lstCity);
                cboCity.DoDataBinding("SelectedValue", _objLifestyle, nameof(Lifestyle.City));
                cboCity.EndUpdate();
            }

            //Populate District and Borough ComboBox for the first time
            await RefreshDistrictList();
            await RefreshBoroughList();

            _blnSkipRefresh = false;
            await RefreshSelectedLifestyle();
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            await AcceptForm();
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

        private async void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            await AcceptForm();
        }

        private async void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await RefreshSelectedLifestyle();
        }

        private async void cboCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await RefreshDistrictList();
        }

        private async void cboDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await RefreshBoroughList();
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

        private async void cmdAddQuality_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                using (SelectLifestyleQuality frmSelectLifestyleQuality = new SelectLifestyleQuality(_objCharacter, cboBaseLifestyle.SelectedValue.ToString(), _objLifestyle.LifestyleQualities))
                {
                    await frmSelectLifestyleQuality.ShowDialogSafeAsync(this);

                    // Don't do anything else if the form was canceled.
                    if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                        return;
                    blnAddAgain = frmSelectLifestyleQuality.AddAgain;

                    XmlNode objXmlQuality = _xmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + frmSelectLifestyleQuality.SelectedQuality.CleanXPath() + ']');

                    LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

                    objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                    //objNode.ContextMenuStrip = cmsQuality;
                    if (objQuality.InternalId.IsEmptyGuid())
                    {
                        objQuality.Dispose();
                        continue;
                    }
                    objQuality.Free = frmSelectLifestyleQuality.FreeCost;

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

            if (!(treLifestyleQualities.SelectedNode.Tag is LifestyleQuality objQuality))
                return;
            if (objQuality.OriginSource == QualitySource.BuiltIn)
                return;
            _objLifestyle.LifestyleQualities.Remove(objQuality);
        }

        private void treLifestyleQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality)
            {
                tlpLifestyleQuality.Visible = true;
                chkQualityContributesLP.Enabled = !(objQuality.Free || objQuality.OriginSource == QualitySource.BuiltIn);

                _blnSkipRefresh = true;
                chkQualityContributesLP.Checked = objQuality.ContributesLP;
                _blnSkipRefresh = false;

                lblQualityLp.Text = objQuality.LP.ToString(GlobalSettings.CultureInfo);
                lblQualityCost.Text = objQuality.Cost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                objQuality.SetSourceDetail(lblQualitySource);
                cmdDeleteQuality.Enabled = objQuality.OriginSource != QualitySource.BuiltIn;
            }
            else
            {
                tlpLifestyleQuality.Visible = false;
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
            lblQualityLp.Text = objQuality.LP.ToString(GlobalSettings.CultureInfo);
        }

        private void chkTravelerBonusLPRandomize_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBonusLPRandomize.Checked)
            {
                nudBonusLP.Enabled = false;
                nudBonusLP.Value = GlobalSettings.RandomGenerator.NextD6ModuloBiasRemoved();
            }
            else
            {
                nudBonusLP.Enabled = true;
            }
        }

        #endregion Control Events

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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private async ValueTask AcceptForm()
        {
            if (string.IsNullOrEmpty(txtLifestyleName.Text))
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectAdvancedLifestyle_LifestyleName"), await LanguageManager.GetStringAsync("MessageTitle_SelectAdvancedLifestyle_LifestyleName"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_objLifestyle.TotalLP < 0)
            {
                Program.MainForm.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectAdvancedLifestyle_OverLPLimit"), await LanguageManager.GetStringAsync("MessageTitle_SelectAdvancedLifestyle_OverLPLimit"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
            XmlNode objXmlLifestyle = _xmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = " + strBaseLifestyle.CleanXPath() + ']');
            if (objXmlLifestyle == null)
                return;
            _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
            _objLifestyle.Name = txtLifestyleName.Text;
            _objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"]?.InnerText, GlobalSettings.InvariantCultureInfo);
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
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.StyleType = StyleType;
            SelectedLifestyle = _objLifestyle;
            DialogResult = DialogResult.OK;
        }

        private async ValueTask RefreshSelectedLifestyle()
        {
            if (_blnSkipRefresh)
                return;

            string strBaseLifestyle = cboBaseLifestyle.SelectedValue?.ToString() ?? string.Empty;
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            XPathNavigator xmlAspect = await _objLifestyle.GetNodeXPathAsync();
            if (xmlAspect != null)
            {
                string strSource = xmlAspect.SelectSingleNode("source")?.Value ?? string.Empty;
                string strPage = xmlAspect.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? xmlAspect.SelectSingleNode("page")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                {
                    SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language,
                        GlobalSettings.CultureInfo, _objCharacter);
                    lblSource.Text = objSource.ToString();
                    lblSource.SetToolTip(objSource.LanguageBookTooltip);
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
                    nudBonusLP.Value = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync();
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

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender);
        }

        /// <summary>
        /// Populates The District list after a City was selected
        /// </summary>
        private async ValueTask RefreshDistrictList()
        {
            string strSelectedCityRefresh = cboCity.SelectedValue?.ToString() ?? string.Empty;
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstDistrict))
            {
                using (XmlNodeList xmlDistrictList
                       = _xmlDocument.SelectNodes("/chummer/cities/city[name = " + strSelectedCityRefresh.CleanXPath()
                                                  + "]/district"))
                {
                    if (xmlDistrictList?.Count > 0)
                    {
                        foreach (XmlNode objXmlDistrict in xmlDistrictList)
                        {
                            string strName = objXmlDistrict["name"]?.InnerText
                                             ?? await LanguageManager.GetStringAsync("String_Unknown");
                            lstDistrict.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                        }
                    }
                }

                cboDistrict.BeginUpdate();
                cboDistrict.PopulateWithListItems(lstDistrict);
                cboDistrict.EndUpdate();
            }
        }

        /// <summary>
        /// Refreshes the BoroughList based on the selected District to generate a cascading dropdown menu
        /// </summary>
        private async ValueTask RefreshBoroughList()
        {
            string strSelectedCityRefresh = cboCity.SelectedValue?.ToString() ?? string.Empty;
            string strSelectedDistrictRefresh = cboDistrict.SelectedValue?.ToString() ?? string.Empty;
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstBorough))
            {
                using (XmlNodeList xmlBoroughList = _xmlDocument.SelectNodes(
                           "/chummer/cities/city[name = " + strSelectedCityRefresh.CleanXPath() + "]/district[name = "
                           + strSelectedDistrictRefresh.CleanXPath() + "]/borough"))
                {
                    if (xmlBoroughList?.Count > 0)
                    {
                        foreach (XmlNode objXmlDistrict in xmlBoroughList)
                        {
                            string strName = objXmlDistrict["name"]?.InnerText
                                             ?? await LanguageManager.GetStringAsync("String_Unknown");
                            lstBorough.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                        }
                    }
                }

                cboBorough.BeginUpdate();
                cboBorough.PopulateWithListItems(lstBorough);
                cboBorough.EndUpdate();
            }
        }

        #endregion Methods
    }
}
