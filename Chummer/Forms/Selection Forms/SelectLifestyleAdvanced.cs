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
using Timer = System.Windows.Forms.Timer;

namespace Chummer
{
    // ReSharper disable once InconsistentNaming
    public partial class SelectLifestyleAdvanced : Form
    {
        private readonly Character _objCharacter;
        private readonly Lifestyle _objLifestyle;
        private readonly XmlDocument _xmlDocument;
        private int _intSkipRefresh = 1;

        private int _intUpdatingCity = 1;
        private int _intUpdatingDistrict = 1;
        private int _intUpdatingBorough = 1;
        private readonly Timer _tmrCityChangeTimer;
        private readonly Timer _tmrDistrictChangeTimer;
        private readonly Timer _tmrBoroughChangeTimer;

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
            _tmrCityChangeTimer = new Timer { Interval = 1000 };
            _tmrDistrictChangeTimer = new Timer { Interval = 1000 };
            _tmrBoroughChangeTimer = new Timer { Interval = 1000 };
            Disposed += (o, args) =>
            {
                _tmrCityChangeTimer?.Dispose();
                _tmrDistrictChangeTimer?.Dispose();
                _tmrBoroughChangeTimer?.Dispose();
            };
            _tmrCityChangeTimer.Tick += CityChangeTimer_Tick;
            _tmrDistrictChangeTimer.Tick += DistrictChangeTimer_Tick;
            _tmrBoroughChangeTimer.Tick += BoroughChangeTimer_Tick;
        }

        private async void SelectLifestyleAdvanced_FormClosing(object sender, FormClosingEventArgs e)
        {
            await _objLifestyle.LifestyleQualities.RemoveCollectionChangedAsync(LifestyleQualitiesOnCollectionChanged).ConfigureAwait(false);
        }

        private async Task LifestyleQualitiesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    return;
                case NotifyCollectionChangedAction.Reset:
                    await ResetLifestyleQualitiesTree(token).ConfigureAwait(false);
                    return;
                default:
                {
                    TreeNode nodPositiveQualityRoot
                        = await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectAdvancedLifestyle_PositiveQualities", false), token: token).ConfigureAwait(false);
                    TreeNode nodNegativeQualityRoot
                        = await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectAdvancedLifestyle_NegativeQualities", false), token: token).ConfigureAwait(false);
                    TreeNode nodEntertainmentsRoot
                        = await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectAdvancedLifestyle_Entertainments", false), token: token).ConfigureAwait(false);
                    TreeNode nodFreeGridsRoot =
                        await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.FindNode("Node_SelectAdvancedLifestyle_FreeMatrixGrids", false), token: token).ConfigureAwait(false);

                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                        {
                            foreach (LifestyleQuality objQuality in e.NewItems)
                            {
                                token.ThrowIfCancellationRequested();
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
                                }, token: token).ConfigureAwait(false);
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
                                }, token: token).ConfigureAwait(false);
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
                                }, token: token).ConfigureAwait(false);
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
                                    Text = await LanguageManager.GetStringAsync("Node_SelectAdvancedLifestyle_FreeMatrixGrids", token: token).ConfigureAwait(false)
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
                                }, token: token).ConfigureAwait(false);
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
                                                "Node_SelectAdvancedLifestyle_PositiveQualities", token: token).ConfigureAwait(false)
                                        };
                                        // ReSharper disable once AssignNullToNotNullAttribute
                                        await treLifestyleQualities.DoThreadSafeAsync(x =>
                                        {
                                            x.Nodes.Insert(0,
                                                           nodPositiveQualityRoot);
                                            nodPositiveQualityRoot.Expand();
                                        }, token: token).ConfigureAwait(false);
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
                                                "Node_SelectAdvancedLifestyle_NegativeQualities", token: token).ConfigureAwait(false)
                                        };
                                        // ReSharper disable once AssignNullToNotNullAttribute
                                        await treLifestyleQualities.DoThreadSafeAsync(x =>
                                        {
                                            x.Nodes.Insert(nodPositiveQualityRoot == null ? 0 : 1,
                                                           nodNegativeQualityRoot);
                                            nodNegativeQualityRoot.Expand();
                                        }, token: token).ConfigureAwait(false);
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
                                                "Node_SelectAdvancedLifestyle_Entertainments", token: token).ConfigureAwait(false)
                                        };
                                        await treLifestyleQualities.DoThreadSafeAsync(x =>
                                        {
                                            x.Nodes.Insert(
                                                (nodPositiveQualityRoot == null ? 0 : 1)
                                                // ReSharper disable once AssignNullToNotNullAttribute
                                                + (nodNegativeQualityRoot == null ? 0 : 1), nodEntertainmentsRoot);
                                            nodEntertainmentsRoot.Expand();
                                        }, token: token).ConfigureAwait(false);
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
                        }, token: token).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task ResetLifestyleQualitiesTree(CancellationToken token = default)
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
                                                         x => x.GetAreaDeltaAsync())
                         .ConfigureAwait(false);
            await nudComforts.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objLifestyle,
                                                             nameof(Lifestyle.ComfortsDelta),
                                                             x => x.GetComfortsDeltaAsync())
                             .ConfigureAwait(false);
            await nudSecurity.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Maximum = y, _objLifestyle,
                                                             nameof(Lifestyle.SecurityDelta),
                                                             x => x.GetSecurityDeltaAsync())
                             .ConfigureAwait(false);
            await cboBaseLifestyle.DoDataBindingAsync("SelectedValue", _objLifestyle, nameof(Lifestyle.BaseLifestyle)).ConfigureAwait(false);
            await chkTrustFund.DoDataBindingAsync("Checked", _objLifestyle, nameof(Lifestyle.TrustFund)).ConfigureAwait(false);
            await chkTrustFund.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Enabled = y, _objLifestyle,
                    nameof(Lifestyle.IsTrustFundEligible), x => x.GetIsTrustFundEligibleAsync())
                .ConfigureAwait(false);
            await chkPrimaryTenant.DoDataBindingAsync("Checked", _objLifestyle, nameof(Lifestyle.PrimaryTenant)).ConfigureAwait(false);
            await lblCost.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objLifestyle,
                                                         nameof(Lifestyle.DisplayTotalMonthlyCost),
                                                         x => x.GetDisplayTotalMonthlyCostAsync())
                         .ConfigureAwait(false);
            await lblArea.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objLifestyle,
                                                              nameof(Lifestyle.FormattedArea),
                                                              x => x.GetFormattedAreaAsync())
                              .ConfigureAwait(false);
            await lblComforts.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y, _objLifestyle,
                                                                 nameof(Lifestyle.FormattedComforts),
                                                                 x => x.GetFormattedComfortsAsync())
                                 .ConfigureAwait(false);
            await lblSecurity.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y,
                                                             _objLifestyle,
                                                             nameof(Lifestyle.FormattedSecurity),
                                                             x => x.GetFormattedSecurityAsync())
                             .ConfigureAwait(false);
            await lblAreaTotal.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objLifestyle,
                                                              nameof(Lifestyle.TotalArea),
                                                              x => x.GetTotalAreaAsync())
                              .ConfigureAwait(false);
            await lblComfortTotal.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objLifestyle,
                                                                 nameof(Lifestyle.TotalComforts),
                                                                 x => x.GetTotalComfortsAsync())
                                 .ConfigureAwait(false);
            await lblSecurityTotal.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objLifestyle,
                                                                  nameof(Lifestyle.TotalSecurity),
                                                                  x => x.GetTotalSecurityAsync())
                                  .ConfigureAwait(false);
            await lblTotalLP.RegisterOneWayAsyncDataBindingAsync((x, y) => x.Text = y.ToString(GlobalSettings.CultureInfo), _objLifestyle,
                                                            nameof(Lifestyle.TotalLP),
                                                            x => x.GetTotalLPAsync())
                            .ConfigureAwait(false);

            await cboBaseLifestyle.DoThreadSafeAsync(x =>
            {
                if (x.SelectedIndex == -1)
                    x.SelectedIndex = 0;
            }).ConfigureAwait(false);

            await _objLifestyle.LifestyleQualities.AddCollectionChangedAsync(LifestyleQualitiesOnCollectionChanged).ConfigureAwait(false);

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

                lstCity.Sort();
                await cboCity.PopulateWithListItemsAsync(lstCity).ConfigureAwait(false);
                await cboCity.DoDataBindingAsync("SelectedValue", _objLifestyle, nameof(Lifestyle.City)).ConfigureAwait(false);
            }

            //Populate District and Borough ComboBox for the first time
            await RefreshDistrictList().ConfigureAwait(false);
            await cboDistrict.DoDataBindingAsync("SelectedValue", _objLifestyle, nameof(Lifestyle.District)).ConfigureAwait(false);
            await RefreshBoroughList().ConfigureAwait(false);
            await cboBorough.DoDataBindingAsync("SelectedValue", _objLifestyle, nameof(Lifestyle.Borough)).ConfigureAwait(false);

            Interlocked.Decrement(ref _intSkipRefresh);
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
            if (_intSkipRefresh > 0)
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
            if (_intSkipRefresh > 0)
                return;
            await RefreshSelectedLifestyle().ConfigureAwait(false);
        }

        private void nudRoommates_ValueChanged(object sender, EventArgs e)
        {
            if (_intSkipRefresh > 0)
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
                using (ThreadSafeForm<SelectLifestyleQuality> frmSelectLifestyleQuality =
                       await ThreadSafeForm<SelectLifestyleQuality>.GetAsync(() => new SelectLifestyleQuality(_objCharacter, _objLifestyle)).ConfigureAwait(false))
                {
                    // Don't do anything else if the form was canceled.
                    if (await frmSelectLifestyleQuality.ShowDialogSafeAsync(this).ConfigureAwait(false) == DialogResult.Cancel)
                        return;
                    blnAddAgain = frmSelectLifestyleQuality.MyForm.AddAgain;

                    XmlNode objXmlQuality = _xmlDocument.TryGetNodeByNameOrId("/chummer/qualities/quality", frmSelectLifestyleQuality.MyForm.SelectedQuality);

                    LifestyleQuality objQuality = new LifestyleQuality(_objCharacter);
                    try
                    {
                        objQuality.Create(objXmlQuality, _objLifestyle, _objCharacter, QualitySource.Selected);
                        //objNode.ContextMenuStrip = cmsQuality;
                        if (objQuality.InternalId.IsEmptyGuid())
                        {
                            await objQuality.RemoveAsync(false).ConfigureAwait(false);
                            continue;
                        }

                        objQuality.Free = frmSelectLifestyleQuality.MyForm.FreeCost;

                        await _objLifestyle.LifestyleQualities.AddAsync(objQuality).ConfigureAwait(false);
                    }
                    catch
                    {
                        try
                        {
                            await objQuality.RemoveAsync(false).ConfigureAwait(false);
                        }
                        catch
                        {
                            await objQuality.DisposeAsync().ConfigureAwait(false);
                            // Swallow removal exceptions here because we already want to throw an exception
                        }

                        throw;
                    }
                }
            }
            while (blnAddAgain);
        }

        private async void cmdDeleteQuality_Click(object sender, EventArgs e)
        {
            // Locate the selected Quality.
            TreeNode objNode = await treLifestyleQualities.DoThreadSafeFuncAsync(x => x.SelectedNode).ConfigureAwait(false);
            if (objNode == null || objNode.Level == 0 || objNode.Parent.Name == "nodFreeMatrixGrids")
                return;
            if (!(objNode.Tag is LifestyleQuality objQuality) || objQuality.OriginSource == QualitySource.BuiltIn)
                return;
            await objQuality.RemoveAsync().ConfigureAwait(false);
        }

        private void treLifestyleQualities_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality)
            {
                tlpLifestyleQuality.Visible = true;
                chkQualityUseLPCost.Enabled = !objQuality.Free && objQuality.CanBeFreeByLifestyle;

                Interlocked.Increment(ref _intSkipRefresh);
                try
                {
                    chkQualityUseLPCost.Checked = chkQualityUseLPCost.Enabled
                        ? objQuality.UseLPCost
                        : !objQuality.Free && !objQuality.CanBeFreeByLifestyle;
                }
                finally
                {
                    Interlocked.Decrement(ref _intSkipRefresh);
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
            if (_intSkipRefresh > 0)
                return;
            if (!(treLifestyleQualities.SelectedNode?.Tag is LifestyleQuality objQuality))
                return;
            objQuality.UseLPCost = !chkQualityUseLPCost.Enabled || chkQualityUseLPCost.Checked;
            lblQualityCost.Text = objQuality.Cost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + LanguageManager.GetString("String_NuyenSymbol");
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

        // Hacky solutions to data binding causing cursor to reset whenever the user is typing something in: have text changes start a timer, and have a 1s delay in the timer update fire the text update
        private void cboCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tmrCityChangeTimer == null)
                return;
            if (_tmrCityChangeTimer.Enabled)
                _tmrCityChangeTimer.Stop();
            if (_intSkipRefresh > 0 || _intUpdatingCity > 0)
                return;
            _tmrCityChangeTimer.Start();
        }

        private void cboDistrict_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tmrDistrictChangeTimer == null)
                return;
            if (_tmrDistrictChangeTimer.Enabled)
                _tmrDistrictChangeTimer.Stop();
            if (_intSkipRefresh > 0 || _intUpdatingDistrict > 0)
                return;
            _tmrDistrictChangeTimer.Start();
        }

        private void cboBorough_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_tmrBoroughChangeTimer == null)
                return;
            if (_tmrBoroughChangeTimer.Enabled)
                _tmrBoroughChangeTimer.Stop();
            if (_intSkipRefresh > 0 || _intUpdatingBorough > 0)
                return;
            _tmrBoroughChangeTimer.Start();
        }

        private async void CityChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrCityChangeTimer.Stop();
            Interlocked.Increment(ref _intUpdatingCity);
            try
            {
                string strText = await cboCity.DoThreadSafeFuncAsync(x => x.Text)
                                              .ConfigureAwait(false);
                await _objLifestyle.SetCityAsync(strText).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingCity);
            }
            await RefreshDistrictList().ConfigureAwait(false);
        }

        private async void DistrictChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrDistrictChangeTimer.Stop();
            Interlocked.Increment(ref _intUpdatingDistrict);
            try
            {
                string strText = await cboDistrict.DoThreadSafeFuncAsync(x => x.Text)
                                              .ConfigureAwait(false);
                await _objLifestyle.SetDistrictAsync(strText).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingDistrict);
            }
            await RefreshBoroughList().ConfigureAwait(false);
        }

        private async void BoroughChangeTimer_Tick(object sender, EventArgs e)
        {
            _tmrBoroughChangeTimer.Stop();
            Interlocked.Increment(ref _intUpdatingBorough);
            try
            {
                string strText = await cboBorough.DoThreadSafeFuncAsync(x => x.Text)
                                              .ConfigureAwait(false);
                await _objLifestyle.SetBoroughAsync(strText).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingBorough);
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
        private async Task AcceptForm(CancellationToken token = default)
        {
            string strLifestyleName = await txtLifestyleName.DoThreadSafeFuncAsync(x => x.Text, token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strLifestyleName))
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectAdvancedLifestyle_LifestyleName", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_SelectAdvancedLifestyle_LifestyleName", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_objLifestyle.TotalLP < 0)
            {
                Program.ShowScrollableMessageBox(this, await LanguageManager.GetStringAsync("Message_SelectAdvancedLifestyle_OverLPLimit", token: token).ConfigureAwait(false), await LanguageManager.GetStringAsync("MessageTitle_SelectAdvancedLifestyle_OverLPLimit", token: token).ConfigureAwait(false), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string strBaseLifestyle = await cboBaseLifestyle.DoThreadSafeFuncAsync(x => x.SelectedValue.ToString(), token: token).ConfigureAwait(false);
            XmlNode objXmlLifestyle = _xmlDocument.TryGetNodeByNameOrId("/chummer/lifestyles/lifestyle", strBaseLifestyle);
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

        private async Task RefreshSelectedLifestyle(CancellationToken token = default)
        {
            if (_intSkipRefresh > 0)
                return;

            _objLifestyle.BaseLifestyle = await cboBaseLifestyle.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString(), token: token).ConfigureAwait(false) ?? string.Empty;
            XPathNavigator xmlAspect = await _objLifestyle.GetNodeXPathAsync(token: token).ConfigureAwait(false);
            if (xmlAspect != null)
            {
                string strSource = xmlAspect.SelectSingleNodeAndCacheExpression("source", token)?.Value ?? string.Empty;
                string strPage = xmlAspect.SelectSingleNodeAndCacheExpression("altpage", token: token)?.Value ?? xmlAspect.SelectSingleNodeAndCacheExpression("page", token)?.Value ?? string.Empty;
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
                string strBonusLP = await LanguageManager.GetStringAsync("Label_BonusLP", token: token).ConfigureAwait(false);
                string strBaseLifestyleName
                    = await cboBaseLifestyle.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(strBaseLifestyleName))
                    strBonusLP = string.Format(GlobalSettings.CultureInfo, strBonusLP, string.Empty);
                else
                {
                    string strSpace = await LanguageManager.GetStringAsync("String_Space", token: token)
                                                           .ConfigureAwait(false);
                    strBonusLP = string.Format(GlobalSettings.CultureInfo, strBonusLP, strBaseLifestyleName + strSpace);
                }
                await lblBonusLP.DoThreadSafeAsync(x =>
                {
                    x.Text = strBonusLP;
                    x.Visible = true;
                }, token).ConfigureAwait(false);
                await nudBonusLP.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                await chkBonusLPRandomize.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);

                if (await chkBonusLPRandomize.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false))
                {
                    int intValue = await GlobalSettings.RandomGenerator.NextD6ModuloBiasRemovedAsync(token).ConfigureAwait(false);
                    await nudBonusLP.DoThreadSafeAsync(x =>
                    {
                        x.Enabled = false;
                        Interlocked.Increment(ref _intSkipRefresh);
                        try
                        {
                            x.Value = intValue;
                        }
                        finally
                        {
                            Interlocked.Decrement(ref _intSkipRefresh);
                        }
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
        private async Task RefreshDistrictList(CancellationToken token = default)
        {
            string strSelectedCityRefresh = await cboCity.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString() ?? x.SelectedText, token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedCityRefresh))
                strSelectedCityRefresh = string.Empty;
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

                lstDistrict.Sort();
                await cboDistrict.PopulateWithListItemsAsync(lstDistrict, token: token).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Refreshes the BoroughList based on the selected District to generate a cascading dropdown menu
        /// </summary>
        private async Task RefreshBoroughList(CancellationToken token = default)
        {
            string strSelectedCityRefresh = await cboCity.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString() ?? x.SelectedText, token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedCityRefresh))
                strSelectedCityRefresh = string.Empty;
            string strSelectedDistrictRefresh = await cboDistrict.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString() ?? x.SelectedText, token: token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(strSelectedDistrictRefresh))
                strSelectedDistrictRefresh = string.Empty;
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

                lstBorough.Sort();
                await cboBorough.PopulateWithListItemsAsync(lstBorough, token: token).ConfigureAwait(false);
            }
        }

        #endregion Methods
    }
}
