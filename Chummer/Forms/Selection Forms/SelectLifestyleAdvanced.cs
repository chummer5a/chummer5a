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
using System.Threading;
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
        }

        private async void LifestyleQualitiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    return;
                case NotifyCollectionChangedAction.Reset:
                    await ResetLifestyleQualitiesTree().ConfigureAwait(false);
                    return;
                default:
                {
                    TreeNode nodPositiveQualityRoot
                        = await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectAdvancedLifestyle_PositiveQualities", false)).ConfigureAwait(false);
                    TreeNode nodNegativeQualityRoot
                        = await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectAdvancedLifestyle_NegativeQualities", false)).ConfigureAwait(false);
                    TreeNode nodEntertainmentsRoot
                        = await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectAdvancedLifestyle_Entertainments", false)).ConfigureAwait(false);
                    TreeNode nodFreeGridsRoot =
                        await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectAdvancedLifestyle_FreeMatrixGrids", false)).ConfigureAwait(false);

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            foreach (LifestyleQuality objQuality in e.NewItems)
                            {
                                await AddToTree(objQuality).ConfigureAwait(false);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (LifestyleQuality objQuality in e.OldItems)
                            {
                                await treLifestyleQualities.DoThreadSafeAsync(x =>
                                {
                                    TreeNode objNode = x.FindNodeByTag(objQuality);
                                    if (objNode != null)
                                    {
                                        TreeNode objParent = objNode.Parent;
                                        objNode.Remove();
                                        if (objParent.Level == 0 && objParent.Nodes.Count == 0)
                                            objParent.Remove();
                                    }
                                }).ConfigureAwait(false);
                            }

                            break;
                        }
                        case NotifyCollectionChangedAction.Replace:
                        {
                            List<TreeNode> lstOldParents = new List<TreeNode>(e.OldItems.Count);
                            foreach (LifestyleQuality objQuality in e.OldItems)
                            {
                                await treLifestyleQualities.DoThreadSafeAsync(x =>
                                {
                                    TreeNode objNode = x.FindNodeByTag(objQuality);
                                    if (objNode != null)
                                    {
                                        if (objNode.Parent != null)
                                            lstOldParents.Add(objNode.Parent);
                                        objNode.Remove();
                                    }
                                }).ConfigureAwait(false);
                            }

                            foreach (LifestyleQuality objQuality in e.NewItems)
                            {
                                await AddToTree(objQuality).ConfigureAwait(false);
                            }

                            if (lstOldParents.Count > 0)
                            {
                                await treLifestyleQualities.DoThreadSafeAsync(() =>
                                {
                                    foreach (TreeNode objOldParent in lstOldParents)
                                    {
                                        if (objOldParent.Level == 0 && objOldParent.Nodes.Count == 0)
                                            objOldParent.Remove();
                                    }
                                }).ConfigureAwait(false);
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
                        if (objQuality.IsFreeGrid)
                        {
                            if (nodFreeGridsRoot == null)
                            {
                                nodFreeGridsRoot = new TreeNode
                                {
                                    Tag = "Node_SelectAdvancedLifestyle_FreeMatrixGrids",
                                    Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_FreeMatrixGrids").ConfigureAwait(false)
                                };
                                // ReSharper disable once AssignNullToNotNullAttribute
                                await treLifestyleQualities.DoThreadSafeAsync(x =>
                                {
                                    x.Nodes.Insert(
                                        (nodPositiveQualityRoot == null ? 0 : 1)
                                        + (nodNegativeQualityRoot == null ? 0 : 1)
                                        // ReSharper disable once AssignNullToNotNullAttribute
                                        + (nodEntertainmentsRoot == null ? 0 : 1), nodFreeGridsRoot);
                                    nodFreeGridsRoot.Expand();
                                }).ConfigureAwait(false);
                            }
                            objParentNode = nodFreeGridsRoot;
                        }
                        else
                        {
                            switch (objQuality.Type)
                            {
                                case QualityType.Positive:
                                    if (nodPositiveQualityRoot == null)
                                    {
                                        nodPositiveQualityRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectAdvancedLifestyle_PositiveQualities",
                                            Text = await LanguageManager.GetStringAsync(
                                                "Node_SelectAdvancedLifestyle_PositiveQualities").ConfigureAwait(false)
                                        };
                                        // ReSharper disable once AssignNullToNotNullAttribute
                                        await treLifestyleQualities.DoThreadSafeAsync(x =>
                                        {
                                            x.Nodes.Insert(0,
                                                           nodPositiveQualityRoot);
                                            nodPositiveQualityRoot.Expand();
                                        }).ConfigureAwait(false);
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
                                                "Node_SelectAdvancedLifestyle_NegativeQualities").ConfigureAwait(false)
                                        };
                                        // ReSharper disable once AssignNullToNotNullAttribute
                                        await treLifestyleQualities.DoThreadSafeAsync(x =>
                                        {
                                            x.Nodes.Insert(nodPositiveQualityRoot == null ? 0 : 1,
                                                           nodNegativeQualityRoot);
                                            nodNegativeQualityRoot.Expand();
                                        }).ConfigureAwait(false);
                                    }

                                    objParentNode = nodNegativeQualityRoot;
                                    break;

                                default:
                                    if (nodEntertainmentsRoot == null)
                                    {
                                        nodEntertainmentsRoot = new TreeNode
                                        {
                                            Tag = "Node_SelectAdvancedLifestyle_Entertainments",
                                            Text = await LanguageManager.GetStringAsync(
                                                "Node_SelectAdvancedLifestyle_Entertainments").ConfigureAwait(false)
                                        };
                                        await treLifestyleQualities.DoThreadSafeAsync(x =>
                                        {
                                            x.Nodes.Insert(
                                                (nodPositiveQualityRoot == null ? 0 : 1)
                                                // ReSharper disable once AssignNullToNotNullAttribute
                                                + (nodNegativeQualityRoot == null ? 0 : 1), nodEntertainmentsRoot);
                                            nodEntertainmentsRoot.Expand();
                                        }).ConfigureAwait(false);
                                    }

                                    objParentNode = nodEntertainmentsRoot;
                                    break;
                            }
                        }

                        await treLifestyleQualities.DoThreadSafeAsync(x =>
                        {
                            if (objParentNode == null)
                                return;
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
                            x.SelectedNode = objNode;
                        }).ConfigureAwait(false);
                    }
                }
            }
        }

        private async ValueTask ResetLifestyleQualitiesTree(CancellationToken token = default)
        {
            TreeNode nodPositiveQualityRoot = null;
            TreeNode nodNegativeQualityRoot = null;
            TreeNode nodEntertainmentsRoot = null;
            TreeNode nodFreeGridsRoot = null;

            string strSelectedNode = await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag.ToString(), token: token).ConfigureAwait(false);

            await treLifestyleQualities.DoThreadSafeAsync(x => x.Nodes.Clear(), token: token).ConfigureAwait(false);

            foreach (LifestyleQuality objQuality in _objLifestyle.LifestyleQualities)
            {
                TreeNode objNode = objQuality.CreateTreeNode();
                if (objNode == null)
                    continue;
                if (objQuality.IsFreeGrid)
                {
                    if (nodFreeGridsRoot == null)
                    {
                        TreeNode objNewNode = new TreeNode
                        {
                            Tag = "Node_SelectAdvancedLifestyle_PositiveQualities",
                            Text = await LanguageManager
                                         .GetStringAsync("Node_SelectAdvancedLifestyle_PositiveQualities", token: token)
                                         .ConfigureAwait(false)
                        };
                        nodFreeGridsRoot = objNewNode;
                        int intOffset = (nodPositiveQualityRoot == null ? 0 : 1)
                                        + (nodNegativeQualityRoot == null ? 0 : 1)
                                        + (nodEntertainmentsRoot == null ? 0 : 1);
                        await treLifestyleQualities.DoThreadSafeAsync(x =>
                        {
                            x.Nodes.Insert(intOffset, objNewNode);
                            objNewNode.Expand();
                        }, token: token).ConfigureAwait(false);
                    }

                    TreeNode root = nodFreeGridsRoot;
                    await treLifestyleQualities.DoThreadSafeAsync(() => root.Nodes.Add(objNode), token: token).ConfigureAwait(false);
                }
                else
                {
                    switch (objQuality.Type)
                    {
                        case QualityType.Positive:
                        {
                            if (nodPositiveQualityRoot == null)
                            {
                                TreeNode objNewNode = new TreeNode
                                {
                                    Tag = "Node_SelectAdvancedLifestyle_PositiveQualities",
                                    Text = await LanguageManager.GetStringAsync(
                                                                    "Node_SelectAdvancedLifestyle_PositiveQualities",
                                                                    token: token)
                                                                .ConfigureAwait(false)
                                };
                                nodPositiveQualityRoot = objNewNode;
                                await treLifestyleQualities.DoThreadSafeAsync(x =>
                                {
                                    x.Nodes.Insert(0, objNewNode);
                                    objNewNode.Expand();
                                }, token: token).ConfigureAwait(false);
                            }

                            TreeNode root = nodPositiveQualityRoot;
                            await treLifestyleQualities.DoThreadSafeAsync(() => root.Nodes.Add(objNode), token: token)
                                                       .ConfigureAwait(false);
                            break;
                        }
                        case QualityType.Negative:
                        {
                            if (nodNegativeQualityRoot == null)
                            {
                                TreeNode objNewNode = new TreeNode
                                {
                                    Tag = "Node_SelectAdvancedLifestyle_NegativeQualities",
                                    Text = await LanguageManager.GetStringAsync(
                                                                    "Node_SelectAdvancedLifestyle_NegativeQualities",
                                                                    token: token)
                                                                .ConfigureAwait(false)
                                };
                                nodNegativeQualityRoot = objNewNode;
                                int intOffset = nodPositiveQualityRoot == null ? 0 : 1;
                                await treLifestyleQualities.DoThreadSafeAsync(x =>
                                {
                                    x.Nodes.Insert(intOffset, objNewNode);
                                    objNewNode.Expand();
                                }, token: token).ConfigureAwait(false);
                            }

                            TreeNode root = nodNegativeQualityRoot;
                            await treLifestyleQualities.DoThreadSafeAsync(() => root.Nodes.Add(objNode), token: token)
                                                       .ConfigureAwait(false);
                            break;
                        }
                        default:
                        {
                            if (nodEntertainmentsRoot == null)
                            {
                                TreeNode objNewNode = new TreeNode
                                {
                                    Tag = "Node_SelectAdvancedLifestyle_Entertainments",
                                    Text = await LanguageManager.GetStringAsync(
                                                                    "Node_SelectAdvancedLifestyle_Entertainments",
                                                                    token: token)
                                                                .ConfigureAwait(false)
                                };
                                nodEntertainmentsRoot = objNewNode;
                                int intOffset = (nodPositiveQualityRoot == null ? 0 : 1)
                                                + (nodNegativeQualityRoot == null ? 0 : 1);
                                await treLifestyleQualities.DoThreadSafeAsync(x =>
                                {
                                    x.Nodes.Insert(intOffset, objNewNode);
                                    objNewNode.Expand();
                                }, token: token).ConfigureAwait(false);
                            }

                            TreeNode root = nodEntertainmentsRoot;
                            await treLifestyleQualities
                                  .DoThreadSafeAsync(() => root.Nodes.Add(objNode), token: token)
                                  .ConfigureAwait(false);
                            break;
                        }
                    }
                }
            }

            await treLifestyleQualities.DoThreadSafeAsync(x => x.SortCustomAlphabetically(strSelectedNode), token: token).ConfigureAwait(false);
        }

        private async void SelectAdvancedLifestyle_Load(object sender, EventArgs e)
        {
            // Populate the Advanced Lifestyle ComboBoxes.
            // Lifestyles.
            using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool,
                                                           out List<ListItem> lstLifestyles))
            {
                using (XmlNodeList xmlLifestyleList
                       = _xmlDocument.SelectNodes("/chummer/lifestyles/lifestyle[" + await _objCharacter.Settings.BookXPathAsync().ConfigureAwait(false)
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
                                await _objCharacter.Settings.BookEnabledAsync(objXmlLifestyle["source"]?.InnerText).ConfigureAwait(false))
                            {
                                lstLifestyles.Add(new ListItem(strLifestyleName,
                                                               objXmlLifestyle["translate"]?.InnerText
                                                               ?? strLifestyleName));
                            }
                        }
                    }
                }

                await chkBonusLPRandomize.DoNegatableDataBindingAsync("Checked", _objLifestyle, nameof(Lifestyle.AllowBonusLP)).ConfigureAwait(false);
                await nudBonusLP.DoDataBindingAsync("Value", _objLifestyle, nameof(Lifestyle.BonusLP)).ConfigureAwait(false);
                await ResetLifestyleQualitiesTree().ConfigureAwait(false);
                await cboBaseLifestyle.PopulateWithListItemsAsync(lstLifestyles).ConfigureAwait(false);
            }

            await txtLifestyleName.DoDataBindingAsync("Text", _objLifestyle, nameof(Lifestyle.Name)).ConfigureAwait(false);
            await nudRoommates.DoDataBindingAsync("Value", _objLifestyle, nameof(Lifestyle.Roommates)).ConfigureAwait(false);
            await nudPercentage.DoDataBindingAsync("Value", _objLifestyle, nameof(Lifestyle.Percentage)).ConfigureAwait(false);
            await nudArea.DoDataBindingAsync("Value", _objLifestyle, nameof(Lifestyle.BindableArea)).ConfigureAwait(false);
            await nudComforts.DoDataBindingAsync("Value", _objLifestyle, nameof(Lifestyle.BindableComforts)).ConfigureAwait(false);
            await nudSecurity.DoDataBindingAsync("Value", _objLifestyle, nameof(Lifestyle.BindableSecurity)).ConfigureAwait(false);
            await nudArea.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objLifestyle,
                                                         nameof(Lifestyle.AreaDelta),
                                                         x => x.GetAreaDeltaAsync().AsTask())
                         .ConfigureAwait(false);
            await nudComforts.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objLifestyle,
                                                             nameof(Lifestyle.ComfortsDelta),
                                                             x => x.GetComfortsDeltaAsync().AsTask())
                             .ConfigureAwait(false);
            await nudSecurity.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objLifestyle,
                                                             nameof(Lifestyle.SecurityDelta),
                                                             x => x.GetSecurityDeltaAsync().AsTask())
                             .ConfigureAwait(false);
            await cboBaseLifestyle.DoDataBindingAsync("SelectedValue", _objLifestyle, nameof(Lifestyle.BaseLifestyle)).ConfigureAwait(false);
            await chkTrustFund.DoDataBindingAsync("Checked", _objLifestyle, nameof(Lifestyle.TrustFund)).ConfigureAwait(false);
            await chkTrustFund.DoOneWayDataBindingAsync("Enabled", _objLifestyle, nameof(Lifestyle.IsTrustFundEligible)).ConfigureAwait(false);
            await chkPrimaryTenant.DoDataBindingAsync("Checked", _objLifestyle, nameof(Lifestyle.PrimaryTenant)).ConfigureAwait(false);
            await lblCost.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objLifestyle,
                                                         nameof(Lifestyle.DisplayTotalMonthlyCost),
                                                         x => x.GetDisplayTotalMonthlyCostAsync().AsTask())
                         .ConfigureAwait(false);
            await lblArea.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objLifestyle,
                                                              nameof(Lifestyle.FormattedArea),
                                                              x => x.GetFormattedAreaAsync().AsTask())
                              .ConfigureAwait(false);
            await lblComforts.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objLifestyle,
                                                                 nameof(Lifestyle.FormattedComforts),
                                                                 x => x.GetFormattedComfortsAsync().AsTask())
                                 .ConfigureAwait(false);
            await lblSecurity.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y,
                                                             _objLifestyle,
                                                             nameof(Lifestyle.FormattedSecurity),
                                                             x => x.GetFormattedSecurityAsync().AsTask())
                             .ConfigureAwait(false);
            await lblAreaTotal.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objLifestyle,
                                                              nameof(Lifestyle.TotalArea),
                                                              x => x.GetTotalAreaAsync().AsTask())
                              .ConfigureAwait(false);
            await lblComfortTotal.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objLifestyle,
                                                                 nameof(Lifestyle.TotalComforts),
                                                                 x => x.GetTotalComfortsAsync().AsTask())
                                 .ConfigureAwait(false);
            await lblSecurityTotal.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objLifestyle,
                                                                  nameof(Lifestyle.TotalSecurity),
                                                                  x => x.GetTotalSecurityAsync().AsTask())
                                  .ConfigureAwait(false);
            await lblTotalLP.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objLifestyle,
                                                            nameof(Lifestyle.TotalLP),
                                                            x => x.GetTotalLPAsync().AsTask())
                            .ConfigureAwait(false);

            await cboBaseLifestyle.DoThreadSafeAsync(x =>
            {
                if (x.SelectedIndex == -1)
                    x.SelectedIndex = 0;
            }).ConfigureAwait(false);

            _objLifestyle.LifestyleQualities.CollectionChanged += LifestyleQualitiesOnCollectionChanged;

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
                                             ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                            lstCity.Add(new ListItem(strName, objXmlCity["translate"]?.InnerText ?? strName));
                        }
                    }
                }
                
                await cboCity.PopulateWithListItemsAsync(lstCity).ConfigureAwait(false);
                await cboCity.DoDataBindingAsync("SelectedValue", _objLifestyle, nameof(Lifestyle.City)).ConfigureAwait(false);
            }

            //Populate District and Borough ComboBox for the first time
            await RefreshDistrictList().ConfigureAwait(false);
            await RefreshBoroughList().ConfigureAwait(false);

            _blnSkipRefresh = false;
            await RefreshSelectedLifestyle().ConfigureAwait(false);
        }

        private async void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            await AcceptForm().ConfigureAwait(false);
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
            await AcceptForm().ConfigureAwait(false);
        }

        private async void cboBaseLifestyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await RefreshSelectedLifestyle().ConfigureAwait(false);
        }

        private async void cboCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await RefreshDistrictList().ConfigureAwait(false);
        }

        private async void cboDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnSkipRefresh)
                return;
            await RefreshBoroughList().ConfigureAwait(false);
            _objLifestyle.District = await cboDistrict.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false) ?? string.Empty;
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
            string strBaseLifestyle = await cboBaseLifestyle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false) ?? string.Empty;
            bool blnAddAgain;
            do
            {
                using (ThreadSafeForm<SelectLifestyleQuality> frmSelectLifestyleQuality =
                       await ThreadSafeForm<SelectLifestyleQuality>.GetAsync(() => new SelectLifestyleQuality(_objCharacter,
                                                                                 strBaseLifestyle, _objLifestyle.LifestyleQualities)).ConfigureAwait(false))
                {
                    // Don't do anything else if the form was canceled.
                    if (await frmSelectLifestyleQuality.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.Cancel)
                        return;
                    blnAddAgain = frmSelectLifestyleQuality.MyForm.AddAgain;

                    XmlNode objXmlQuality = _xmlDocument.SelectSingleNode("/chummer/qualities/quality[id = " + frmSelectLifestyleQuality.MyForm.SelectedQuality.CleanXPath() + ']');

                    LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);

                    objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                    //objNode.ContextMenuStrip = cmsQuality;
                    if (objQuality.InternalId.IsEmptyGuid())
                    {
                        objQuality.Remove(false);
                        objQuality.Dispose();
                        continue;
                    }
                    objQuality.Free = frmSelectLifestyleQuality.MyForm.FreeCost;

                    await _objLifestyle.LifestyleQualities.AddAsync(objQuality).ConfigureAwait(false);
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
            objQuality.Remove();
        }

        private void treLifestyleQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality)
            {
                tlpLifestyleQuality.Visible = true;
                chkQualityUseLPCost.Enabled = !objQuality.Free && objQuality.CanBeFreeByLifestyle;
                
                _blnSkipRefresh = true;
                try
                {
                    chkQualityUseLPCost.Checked = chkQualityUseLPCost.Enabled
                        ? objQuality.UseLPCost
                        : !objQuality.Free && !objQuality.CanBeFreeByLifestyle;
                }
                finally
                {
                    _blnSkipRefresh = false;
                }

                lblQualityLp.Text = objQuality.LP.ToString(GlobalSettings.CultureInfo);
                lblQualityCost.Text = objQuality.Cost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol");
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
            objQuality.UseLPCost = !chkQualityUseLPCost.Enabled || chkQualityUseLPCost.Checked;
            lblQualityLp.Text = objQuality.LP.ToString(GlobalSettings.CultureInfo);
        }

        private async void chkTravelerBonusLPRandomize_CheckedChanged(object sender, EventArgs e)
        {
            if (await chkBonusLPRandomize.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false))
            {
                int intRandom = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync().ConfigureAwait(false);
                await nudBonusLP.DoThreadSafeAsync(x =>
                {
                    x.Enabled = false;
                    x.Value = intRandom;
                }).ConfigureAwait(false);
            }
            else
            {
                await nudBonusLP.DoThreadSafeAsync(x => x.Enabled = true).ConfigureAwait(false);
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
        private async ValueTask AcceptForm(CancellationToken token = default)
        {
            string strLifestyleName = await txtLifestyleName.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strLifestyleName))
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectAdvancedLifestyle_LifestyleName", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_SelectAdvancedLifestyle_LifestyleName", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_objLifestyle.TotalLP < 0)
            {
                Program.ShowMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectAdvancedLifestyle_OverLPLimit", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_SelectAdvancedLifestyle_OverLPLimit", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strBaseLifestyle = await cboBaseLifestyle.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);
            XmlNode objXmlLifestyle = _xmlDocument.SelectSingleNode("/chummer/lifestyles/lifestyle[name = " + strBaseLifestyle.CleanXPath() + ']');
            if (objXmlLifestyle == null)
                return;
            _objLifestyle.Source = objXmlLifestyle["source"]?.InnerText;
            _objLifestyle.Page = objXmlLifestyle["page"]?.InnerText;
            _objLifestyle.Name = strLifestyleName;
            _objLifestyle.Cost = Convert.ToInt32(objXmlLifestyle["cost"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.Percentage = await nudPercentage.DoThreadSafeFuncAsync(x => x.Value, token).ConfigureAwait(false);
            _objLifestyle.BaseLifestyle = strBaseLifestyle;
            _objLifestyle.Area = await nudArea.DoThreadSafeFuncAsync(x => x.ValueAsInt, token).ConfigureAwait(false);
            _objLifestyle.Comforts = await nudComforts.DoThreadSafeFuncAsync(x => x.ValueAsInt, token).ConfigureAwait(false);
            _objLifestyle.Security = await nudSecurity.DoThreadSafeFuncAsync(x => x.ValueAsInt, token).ConfigureAwait(false);
            _objLifestyle.TrustFund = chkTrustFund.Checked;
            _objLifestyle.Roommates = _objLifestyle.TrustFund ? 0 : await nudRoommates.DoThreadSafeFuncAsync(x => x.ValueAsInt, token).ConfigureAwait(false);
            _objLifestyle.PrimaryTenant = await chkPrimaryTenant.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
            _objLifestyle.BonusLP = await nudBonusLP.DoThreadSafeFuncAsync(x => x.ValueAsInt, token).ConfigureAwait(false);

            // Get the starting Nuyen information.
            _objLifestyle.Dice = Convert.ToInt32(objXmlLifestyle["dice"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.Multiplier = Convert.ToDecimal(objXmlLifestyle["multiplier"]?.InnerText, GlobalSettings.InvariantCultureInfo);
            _objLifestyle.StyleType = StyleType;
            SelectedLifestyle = _objLifestyle;
            await this.DoThreadSafeAsync(x =>
            {
                x.DialogResult = DialogResult.OK;
                x.Close();
            }, token).ConfigureAwait(false);
        }

        private async ValueTask RefreshSelectedLifestyle(CancellationToken token = default)
        {
            if (_blnSkipRefresh)
                return;
            
            _objLifestyle.BaseLifestyle = await cboBaseLifestyle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? string.Empty;
            XPathNavigator xmlAspect = await _objLifestyle.GetNodeXPathAsync(token: token).ConfigureAwait(false);
            if (xmlAspect != null)
            {
                string strSource = (await xmlAspect.SelectSingleNodeAndCacheExpressionAsync("source", token).ConfigureAwait(false))?.Value ?? string.Empty;
                string strPage = (await xmlAspect.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token).ConfigureAwait(false))?.Value ?? (await xmlAspect.SelectSingleNodeAndCacheExpressionAsync("page", token).ConfigureAwait(false))?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strSource) && !string.IsNullOrEmpty(strPage))
                {
                    SourceString objSource = await SourceString.GetSourceStringAsync(strSource, strPage, GlobalSettings.Language,
                        GlobalSettings.CultureInfo, _objCharacter, token).ConfigureAwait(false);
                    await objSource.SetControlAsync(lblSource, token).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                }
                else
                {
                    await SourceString.Blank.SetControlAsync(lblSource, token).ConfigureAwait(false);
                    await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                }
            }
            else
            {
                await SourceString.Blank.SetControlAsync(lblSource, token).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
            }

            // Characters with the Trust Fund Quality can have the lifestyle discounted.
            if (_objLifestyle.IsTrustFundEligible)
            {
                await chkTrustFund.DoThreadSafeAsync(x =>
                {
                    x.Visible = true;
                    x.Checked = _objLifestyle.TrustFund;
                }, token).ConfigureAwait(false);
            }
            else
            {
                await chkTrustFund.DoThreadSafeAsync(x =>
                {
                    x.Checked = false;
                    x.Visible = false;
                }, token).ConfigureAwait(false);
            }

            if (_objLifestyle.AllowBonusLP)
            {
                await lblBonusLP.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                await nudBonusLP.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                await chkBonusLPRandomize.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);

                if (await chkBonusLPRandomize.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                {
                    int intValue = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync(token).ConfigureAwait(false);
                    await nudBonusLP.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = false;
                        _blnSkipRefresh = true;
                        x.Value = intValue;
                        _blnSkipRefresh = false;
                    }, token).ConfigureAwait(false);
                }
                else
                {
                    await nudBonusLP.DoThreadSafeAsync(x => x.Enabled = true, token: token).ConfigureAwait(false);
                }
            }
            else
            {
                await lblBonusLP.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await nudBonusLP.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await nudBonusLP.DoThreadSafeAsync(x => x.Value = 0, token).ConfigureAwait(false);
                await chkBonusLPRandomize.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
            }
        }

        private async void OpenSourceFromLabel(object sender, EventArgs e)
        {
            await CommonFunctions.OpenPdfFromControl(sender).ConfigureAwait(false);
        }

        /// <summary>
        /// Populates The District list after a City was selected
        /// </summary>
        private async ValueTask RefreshDistrictList(CancellationToken token = default)
        {
            string strSelectedCityRefresh = await cboCity.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? string.Empty;
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
                                             ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                            lstDistrict.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                        }
                    }
                }
                
                await cboDistrict.PopulateWithListItemsAsync(lstDistrict, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refreshes the BoroughList based on the selected District to generate a cascading dropdown menu
        /// </summary>
        private async ValueTask RefreshBoroughList(CancellationToken token = default)
        {
            string strSelectedCityRefresh = await cboCity.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? string.Empty;
            string strSelectedDistrictRefresh = await cboDistrict.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? string.Empty;
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
                                             ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                            lstBorough.Add(new ListItem(strName, objXmlDistrict["translate"]?.InnerText ?? strName));
                        }
                    }
                }
                
                await cboBorough.PopulateWithListItemsAsync(lstBorough, token: token).ConfigureAwait(false);
            }
        }

        #endregion Methods
    }
}
