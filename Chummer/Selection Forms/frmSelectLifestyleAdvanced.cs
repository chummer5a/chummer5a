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
 ﻿using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmSelectLifestyleAdvanced : Form
    {
        private bool _blnAddAgain;
        private readonly Lifestyle _objLifestyle;
        private Lifestyle _objSourceLifestyle;
        private readonly Character _objCharacter;

        private readonly XmlDocument _xmlDocument;

        private bool _blnSkipRefresh = true;

        #region Control Events
        public frmSelectLifestyleAdvanced(Character objCharacter)
        {
            InitializeComponent();
            LanguageManager.TranslateWinForm(GlobalOptions.Language, this);
            _objCharacter = objCharacter;
            _objLifestyle = new Lifestyle(objCharacter);
            // Load the Lifestyles information.
            _xmlDocument = XmlManager.Load("lifestyles.xml");
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
                                    Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids", GlobalOptions.Language)
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
                                    Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids", GlobalOptions.Language)
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
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_PositiveQualities", GlobalOptions.Language)
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
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_NegativeQualities", GlobalOptions.Language)
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
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_Entertainments", GlobalOptions.Language)
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
                        Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_FreeMatrixGrids", GlobalOptions.Language)
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
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_PositiveQualities", GlobalOptions.Language)
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
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_NegativeQualities", GlobalOptions.Language)
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
                                Text = LanguageManager.GetString("Node_SelectAdvancedLifestyle_Entertainments", GlobalOptions.Language)
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
                if (xmlLifestyleList?.Count > 0)
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
            // Populate the Qualities list.
            if (_objSourceLifestyle != null)
            {
                foreach (LifestyleQuality objQuality in _objSourceLifestyle.LifestyleQualities)
                {
                    _objLifestyle.LifestyleQualities.Add(objQuality);
                }
                foreach (LifestyleQuality objQuality in _objSourceLifestyle.FreeGrids)
                {
                    _objLifestyle.FreeGrids.Add(objQuality);
                }

                if (_objSourceLifestyle.AllowBonusLP)
                {
                    chkBonusLPRandomize.Checked = false;
                    nudBonusLP.Value = _objSourceLifestyle.BonusLP;
                }

                _objLifestyle.BaseLifestyle = _objSourceLifestyle.BaseLifestyle;
            }
            ResetLifestyleQualitiesTree();
            cboBaseLifestyle.BeginUpdate();
            cboBaseLifestyle.ValueMember = "Value";
            cboBaseLifestyle.DisplayMember = "Name";
            cboBaseLifestyle.DataSource = lstLifestyles;

            cboBaseLifestyle.SelectedValue = _objLifestyle.BaseLifestyle;

            if (cboBaseLifestyle.SelectedIndex == -1)
                cboBaseLifestyle.SelectedIndex = 0;

            if (_objSourceLifestyle != null)
            {
                int intMinComfort = 0;
                int intMaxComfort = 0;
                int intMinArea = 0;
                int intMaxArea = 0;
                int intMinSec = 0;
                int intMaxSec = 0;
                string strBaseLifestyle = _objSourceLifestyle.BaseLifestyle;

                // Calculate the limits of the 3 aspects.
                // Comforts.
                XmlNode xmlNode = _xmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + strBaseLifestyle + "\"]");
                xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinComfort);
                xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxComfort);
                if (intMaxComfort < intMinComfort)
                    intMaxComfort = intMinComfort;
                // Area.
                xmlNode = _xmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + strBaseLifestyle + "\"]");
                xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinArea);
                xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxArea);
                if (intMaxArea < intMinArea)
                    intMaxArea = intMinArea;
                // Security.
                xmlNode = _xmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + strBaseLifestyle + "\"]");
                xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinSec);
                xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxSec);
                if (intMaxSec < intMinSec)
                    intMaxSec = intMinSec;

                // Calculate the cost of Positive Qualities.
                foreach (LifestyleQuality objQuality in _objLifestyle.LifestyleQualities)
                {
                    intMaxArea += objQuality.AreaMaximum;
                    intMaxComfort += objQuality.ComfortMaximum;
                    intMaxSec += objQuality.SecurityMaximum;
                    intMinArea += objQuality.Area;
                    intMinComfort += objQuality.Comfort;
                    intMinSec += objQuality.Security;
                }
                _blnSkipRefresh = true;

                nudComforts.Maximum = Math.Max(intMaxComfort - intMinComfort, 0);
                nudArea.Maximum = Math.Max(intMaxArea - intMinArea, 0);
                nudSecurity.Maximum = Math.Max(intMaxSec - intMinSec, 0);

                txtLifestyleName.Text = _objSourceLifestyle.Name;
                nudRoommates.Value = _objSourceLifestyle.Roommates;
                nudPercentage.Value = _objSourceLifestyle.Percentage;
                nudArea.Value = _objSourceLifestyle.Area;
                nudComforts.Value = _objSourceLifestyle.Comforts;
                nudSecurity.Value = _objSourceLifestyle.Security;
                cboBaseLifestyle.SelectedValue = _objSourceLifestyle.BaseLifestyle;
                chkTrustFund.Checked = _objSourceLifestyle.TrustFund;
                chkPrimaryTenant.Checked = _objSourceLifestyle.PrimaryTenant;
            }

            cboBaseLifestyle.EndUpdate();

            _objLifestyle.LifestyleQualities.CollectionChanged += LifestyleQualitiesOnCollectionChanged;
            _objLifestyle.FreeGrids.CollectionChanged += FreeGridsOnCollectionChanged;

            _blnSkipRefresh = false;
            RefreshSelectedLifestyle();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
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

            CalculateValues();
        }

        private void chkPrimaryTenant_CheckedChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            CalculateValues();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            RefreshSelectedLifestyle();
        }

        private void nudBonusLP_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            CalculateValues();
        }

        private void nudPercentage_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            CalculateValues();
        }

        private void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;

            if (nudRoommates.Value == 0 && !chkPrimaryTenant.Checked)
            {
                chkPrimaryTenant.Checked = true;
            }
            CalculateValues();
        }

        private void cmdAddQuality_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;
            do
            {
                frmSelectLifestyleQuality frmSelectLifestyleQuality = new frmSelectLifestyleQuality(_objCharacter, cboBaseLifestyle.SelectedValue.ToString(), _objLifestyle.LifestyleQualities);
                frmSelectLifestyleQuality.ShowDialog(this);

                // Don't do anything else if the form was canceled.
                if (frmSelectLifestyleQuality.DialogResult == DialogResult.Cancel)
                {
                    frmSelectLifestyleQuality.Close();
                    return;
                }
                blnAddAgain = frmSelectLifestyleQuality.AddAgain;

                XmlNode objXmlQuality = _xmlDocument.SelectSingleNode("/chummer/qualities/quality[id = \"" + frmSelectLifestyleQuality.SelectedQuality + "\"]");

                LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

                objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                objQuality.Free = frmSelectLifestyleQuality.FreeCost;
                frmSelectLifestyleQuality.Close();
                //objNode.ContextMenuStrip = cmsQuality;
                if (objQuality.InternalId.IsEmptyGuid())
                    continue;
                
                _objLifestyle.LifestyleQualities.Add(objQuality);
                CalculateValues();
            }
            while (blnAddAgain);
        }

        private void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            if (treLifestyleQualities.SelectedNode == null || treLifestyleQualities.SelectedNode.Level == 0 || treLifestyleQualities.SelectedNode.Parent.Name == "nodFreeMatrixGrids")
                return;

            if (treLifestyleQualities.SelectedNode.Tag is LifestyleQuality objQuality)
            {
                if (objQuality.Name == "Not a Home" && cboBaseLifestyle.SelectedValue?.ToString() == "Bolt Hole")
                {
                    return;
                }
                _objLifestyle.LifestyleQualities.Remove(objQuality);
                CalculateValues();
            }
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
                objQuality.SetSourceDetail(lblSource);
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

            if (treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality)
            {
                objQuality.ContributesLP = chkQualityContributesLP.Checked;
                lblQualityLp.Text = objQuality.LP.ToString();
                CalculateValues();
            }
        }

        private void chkTravelerBonusLPRandomize_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBonusLPRandomize.Checked)
            {
                nudBonusLP.Enabled = false;
                nudBonusLP.Value = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
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
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Lifestyle that was created in the dialogue.
        /// </summary>
        public Lifestyle SelectedLifestyle => _objLifestyle;

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
                MessageBox.Show(LanguageManager.GetString("Message_SelectAdvancedLifestyle_LifestyleName", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_LifestyleName", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (Convert.ToInt32(lblTotalLP.Text) < 0)
            {
                MessageBox.Show(LanguageManager.GetString("Message_SelectAdvancedLifestyle_OverLPLimit", GlobalOptions.Language), LanguageManager.GetString("MessageTitle_SelectAdvancedLifestyle_OverLPLimit", GlobalOptions.Language), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strBaseLifestyle = cboBaseLifestyle.SelectedValue.ToString();
            XmlNode objXmlLifestyle = _xmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = \"" + strBaseLifestyle + "\"]");
            if (objXmlLifestyle == null)
                return;
            _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
            _objLifestyle.Name = txtLifestyleName.Text;
            _objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"]?.InnerText);
            _objLifestyle.Percentage = nudPercentage.Value;
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            _objLifestyle.Area = decimal.ToInt32(nudArea.Value);
            _objLifestyle.Comforts = decimal.ToInt32(nudComforts.Value);
            _objLifestyle.Security = decimal.ToInt32(nudSecurity.Value);
            _objLifestyle.TrustFund = chkTrustFund.Checked;
            _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : decimal.ToInt32(nudRoommates.Value);
            _objLifestyle.PrimaryTenant = chkPrimaryTenant.Checked;
            _objLifestyle.BonusLP = decimal.ToInt32(nudBonusLP.Value);

            // Get the starting Nuyen information.
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalOptions.InvariantCultureInfo);
            _objLifestyle.StyleType = StyleType;

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
                    string strSpaceCharacter = LanguageManager.GetString("String_Space", GlobalOptions.Language);
                    lblSource.Text = CommonFunctions.LanguageBookShort(strSource, GlobalOptions.Language) + strSpaceCharacter + strPage;
                    lblSource.SetToolTip(CommonFunctions.LanguageBookLong(strSource, GlobalOptions.Language) + strSpaceCharacter + LanguageManager.GetString("String_Page", GlobalOptions.Language) + strSpaceCharacter + strPage);
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
                    nudBonusLP.Value = 1 + GlobalOptions.RandomGenerator.NextD6ModuloBiasRemoved();
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

            CalculateValues();
        }

        /// <summary>
        /// Calculate the LP value for the selected items.
        /// </summary>
        private void CalculateValues()
        {
            int intLP = 0;
            decimal decBaseNuyen = 0;
            decimal decNuyen = 0;
            int intMultiplier = 0;
            int intMultiplierBaseOnly = 0;
            decimal decExtraCostAssets = 0;
            decimal decExtraCostServicesOutings = 0;
            decimal decExtraCostContracts = 0;
            int intMinComfort = 0;
            int intMaxComfort = 0;
            int intMinArea = 0;
            int intMaxArea = 0;
            int intMinSec = 0;
            int intMaxSec = 0;
            string strBaseLifestyle = cboBaseLifestyle.SelectedValue?.ToString() ?? string.Empty;

            // Calculate the limits of the 3 aspects.
            // Comforts.
            XmlNode xmlNode = _xmlDocument.SelectSingleNode("/chummer/comforts/comfort[name = \"" + strBaseLifestyle + "\"]");
            xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinComfort);
            xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxComfort);
            if (intMaxComfort < intMinComfort)
                intMaxComfort = intMinComfort;
            // Area.
            xmlNode = _xmlDocument.SelectSingleNode("/chummer/neighborhoods/neighborhood[name = \"" + strBaseLifestyle + "\"]");
            xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinArea);
            xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxArea);
            if (intMaxArea < intMinArea)
                intMaxArea = intMinArea;
            // Security.
            xmlNode = _xmlDocument.SelectSingleNode("/chummer/securities/security[name = \"" + strBaseLifestyle + "\"]");
            xmlNode.TryGetInt32FieldQuickly("minimum", ref intMinSec);
            xmlNode.TryGetInt32FieldQuickly("limit", ref intMaxSec);
            if (intMaxSec < intMinSec)
                intMaxSec = intMinSec;

            // Calculate the cost of Positive Qualities.
            foreach (LifestyleQuality objQuality in _objLifestyle.LifestyleQualities)
            {
                intLP -= objQuality.LP;
                intMultiplier += objQuality.Multiplier;
                intMultiplierBaseOnly += objQuality.BaseMultiplier;
                intMaxArea += objQuality.AreaMaximum;
                intMaxComfort += objQuality.ComfortMaximum;
                intMaxSec += objQuality.SecurityMaximum;
                intMinArea += objQuality.Area;
                intMinComfort += objQuality.Comfort;
                intMinSec += objQuality.Security;

                decimal decCost = objQuality.Cost;
                // Calculate the cost of Entertainments.
                if (decCost != 0 && (objQuality.Type == QualityType.Entertainment || objQuality.Type == QualityType.Contracts))
                {
                    if (objQuality.Type == QualityType.Contracts)
                    {
                        decExtraCostContracts += decCost;
                    }
                    else if (objQuality.Category == "Entertainment - Outing" || objQuality.Category == "Entertainment - Service")
                    {
                        decExtraCostServicesOutings += decCost;
                    }
                    else
                    {
                        decExtraCostAssets += decCost;
                    }
                }
                else
                    decBaseNuyen += decCost;
            }
            _blnSkipRefresh = true;

            nudComforts.Maximum = Math.Max(intMaxComfort - intMinComfort, 0);
            nudArea.Maximum = Math.Max(intMaxArea - intMinArea, 0);
            nudSecurity.Maximum = Math.Max(intMaxSec - intMinSec, 0);
            int intComfortsValue = decimal.ToInt32(nudComforts.Value);
            int intAreaValue = decimal.ToInt32(nudArea.Value);
            int intSecurityValue = decimal.ToInt32(nudSecurity.Value);
            int intRoommatesValue = decimal.ToInt32(nudRoommates.Value);

            _blnSkipRefresh = false;
            //set the Labels for current/maximum
            Label_SelectAdvancedLifestyle_Base_Comforts.Text = string.Format(LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Comforts", GlobalOptions.Language),
                (nudComforts.Value + intMinComfort).ToString(GlobalOptions.CultureInfo),
                (nudComforts.Maximum + intMinComfort).ToString(GlobalOptions.CultureInfo));
            Label_SelectAdvancedLifestyle_Base_Security.Text = string.Format(LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Security", GlobalOptions.Language),
                (nudSecurity.Value + intMinSec).ToString(GlobalOptions.CultureInfo),
                (nudSecurity.Maximum + intMinSec).ToString(GlobalOptions.CultureInfo));
            Label_SelectAdvancedLifestyle_Base_Neighborhood.Text = string.Format(LanguageManager.GetString("Label_SelectAdvancedLifestyle_Base_Neighborhood", GlobalOptions.Language),
                (nudArea.Value + intMinArea).ToString(GlobalOptions.CultureInfo),
                (nudArea.Maximum + intMinArea).ToString(GlobalOptions.CultureInfo));

            //calculate the total LP
            xmlNode = _objLifestyle.GetNode();
            int intBaseLP = Convert.ToInt32(xmlNode?["lp"]?.InnerText);
            intLP += intBaseLP;
            intLP -= intComfortsValue;
            intLP -= intAreaValue;
            intLP -= intSecurityValue;
            intLP += intRoommatesValue;
            intLP += decimal.ToInt32(nudBonusLP.Value);

            intLP = Math.Min(intLP, intBaseLP * 2);

            if (strBaseLifestyle == "Street")
            {
                decNuyen += intComfortsValue * 50;
                decNuyen += intAreaValue * 50;
                decNuyen += intSecurityValue * 50;
            }
            else
            {
                intMultiplier += 10 * (intComfortsValue + intAreaValue + intSecurityValue);
            }

            if (!chkTrustFund.Checked)
            {
                // Determine the base Nuyen cost.
                string strCost = xmlNode?["cost"]?.InnerText;
                if (!string.IsNullOrEmpty(strCost))
                    decBaseNuyen += Convert.ToDecimal(strCost, GlobalOptions.InvariantCultureInfo);
                decBaseNuyen += decBaseNuyen * ((intMultiplier + intMultiplierBaseOnly) / 100.0m);
                decNuyen += decBaseNuyen;
            }
            decNuyen += decExtraCostAssets + (decExtraCostAssets * (intMultiplier / 100.0m));
            decNuyen *= nudPercentage.Value / 100.0m;
            if (!chkPrimaryTenant.Checked)
            {
                decNuyen /= intRoommatesValue + 1.0m;
            }
            decNuyen += decExtraCostServicesOutings + (decExtraCostServicesOutings * (intMultiplier / 100.0m));
            decNuyen += decExtraCostContracts;
            lblTotalLP.Text = intLP.ToString();
            lblCost.Text = decNuyen.ToString(_objCharacter.Options.NuyenFormat, GlobalOptions.CultureInfo) + '¥';
            lblTotalLPLabel.Visible = !string.IsNullOrEmpty(lblTotalLP.Text);
            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
        }

        /// <summary>
        /// Lifestyle to update when editing.
        /// </summary>
        /// <param name="objLifestyle">Lifestyle to edit.</param>
        public void SetLifestyle(Lifestyle objLifestyle)
        {
            _objSourceLifestyle = objLifestyle;
            StyleType = objLifestyle.StyleType;
        }
        
        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPDFFromControl(sender, e);
        }
        #endregion
    }
}
