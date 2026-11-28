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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using Microsoft.VisualStudio.Threading;

namespace Chummer
{
    public partial class SelectLifeModule : Form, IHasCharacterObject
    {
        public bool AddAgain { get; private set; }
        private readonly Character _objCharacter;
        private readonly int _intStage;
        private string _strDefaultStageName;
        private readonly XPathNavigator _xmlLifeModulesDocumentChummerNode;
        private string _strSelectedId;

        public Character CharacterObject => _objCharacter;

        private string _strWorkStage;

        private int _intUpdatingTree = 1;

        public SelectLifeModule(Character objCharacter, int intStage)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            this.UpdateParentForToolTipControls();
            _intStage = intStage;
            _xmlLifeModulesDocumentChummerNode
                = _objCharacter.LoadDataXPath("lifemodules.xml").SelectSingleNodeAndCacheExpression("/chummer");
        }

        private async void SelectLifeModule_Load(object _, EventArgs e)
        {
            string strSelectString = "stages/stage[@order = " + _intStage.ToString(GlobalSettings.InvariantCultureInfo).CleanXPath() + "]";

            XPathNavigator xmlStageNode = _xmlLifeModulesDocumentChummerNode.SelectSingleNodeAndCacheExpression(strSelectString);
            if (xmlStageNode != null)
            {
                _strWorkStage = _strDefaultStageName = xmlStageNode.Value;
            }
            else
            {
                _strWorkStage = _strDefaultStageName = string.Empty;
            }
            await BuildTree().ConfigureAwait(false);
            Interlocked.Decrement(ref _intUpdatingTree);
        }

        private async Task BuildTree(CancellationToken token = default)
        {
            string strFilter = string.Empty;
            string strSearch = await txtSearch.DoThreadSafeFuncAsync(x => x.Text, token).ConfigureAwait(false);
            string strSearchXPath = !string.IsNullOrEmpty(strSearch) ? CommonFunctions.GenerateSearchXPath(strSearch) : string.Empty;
            string strBooksXPath = await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false);
            using (new FetchSafelyFromObjectPool<StringBuilder>(Utils.StringBuilderPool, out StringBuilder sbdReturn))
            {
                sbdReturn.Append(strBooksXPath);

                if (!string.IsNullOrWhiteSpace(_strWorkStage))
                {
                    sbdReturn.Append(" and (stage = ", _strWorkStage.CleanXPath(), ')');
                }
                if (!string.IsNullOrEmpty(strSearchXPath))
                {
                    sbdReturn.Append(" and ((", strSearchXPath, ") or (versions/version[", strSearchXPath, "]))");
                }

                if (sbdReturn.Length > 0)
                    strFilter = string.Concat('[', sbdReturn.Append(']').ToString());
            }
            bool blnDoRefreshSelection;
            Interlocked.Increment(ref _intUpdatingTree);
            try
            {
                bool blnLimitList = await chkLimitList.DoThreadSafeFuncAsync(x => x.Checked, token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                string strOldSelectedId = await treModules.DoThreadSafeFuncAsync(x => x.SelectedNode?.Tag?.ToString(), token).ConfigureAwait(false);
                TreeNode objNewSelectedNode = null;
                List<TreeNode> lstTreeNodes = new List<TreeNode>(5);
                foreach (XPathNavigator xmlNode in _xmlLifeModulesDocumentChummerNode.Select("modules/module" + strFilter))
                {
                    token.ThrowIfCancellationRequested();
                    if (!blnLimitList || await xmlNode.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                    {
                        string strId = xmlNode.SelectSingleNodeAndCacheExpression("id", token)?.Value ?? Utils.GuidEmptyString;
                        TreeNode treNode = new TreeNode
                        {
                            Text = xmlNode.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                   ?? xmlNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty,
                            Tag = strId
                        };
                        if (!string.IsNullOrEmpty(strOldSelectedId) && objNewSelectedNode == null && string.Equals(strId, strOldSelectedId))
                            objNewSelectedNode = treNode;
                        XPathNavigator xmlVersionsNode = xmlNode.SelectSingleNodeAndCacheExpression("versions", token);
                        if (xmlVersionsNode != null)
                        {
                            string strInnerFilter;
                            if (string.IsNullOrEmpty(strBooksXPath))
                            {
                                if (string.IsNullOrEmpty(strSearchXPath))
                                {
                                    strInnerFilter = "version";
                                }
                                else
                                {
                                    strInnerFilter = "version[(" + strSearchXPath + ")]";
                                }
                            }
                            else if (string.IsNullOrEmpty(strSearchXPath))
                            {
                                strInnerFilter = "version[(" + strBooksXPath + ") or not(source)]";
                            }
                            else
                            {
                                strInnerFilter = "version[((" + strBooksXPath + ") or not(source)) and (" + strSearchXPath + ")]";
                            }

                            foreach (XPathNavigator xmlVersionNode in xmlVersionsNode.Select(strInnerFilter))
                            {
                                token.ThrowIfCancellationRequested();
                                if (!blnLimitList || await xmlVersionNode.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                                {
                                    string strInnerId = xmlVersionNode.SelectSingleNodeAndCacheExpression("id", token)?.Value ?? Utils.GuidEmptyString;
                                    TreeNode treVersionNode = new TreeNode
                                    {
                                        Text = xmlVersionNode.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                                            ?? xmlVersionNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty,
                                        Tag = strInnerId
                                    };
                                    token.ThrowIfCancellationRequested();
                                    if (!string.IsNullOrEmpty(strOldSelectedId) && objNewSelectedNode == null && string.Equals(strInnerId, strOldSelectedId))
                                        objNewSelectedNode = treVersionNode;
                                    treNode.Nodes.Add(treVersionNode);
                                }
                            }
                        }
                        token.ThrowIfCancellationRequested();
                        lstTreeNodes.Add(treNode);
                    }
                }
                token.ThrowIfCancellationRequested();
                TreeNode[] aobjNodes = lstTreeNodes.ToArray();
                blnDoRefreshSelection = await treModules.DoThreadSafeFuncAsync(x =>
                {
                    x.Nodes.Clear();
                    x.Nodes.AddRange(aobjNodes);
                    if (objNewSelectedNode != null)
                        x.SelectedNode = objNewSelectedNode;
                    return x.SelectedNode != null;
                }, token: token).ConfigureAwait(false);
            }
            finally
            {
                Interlocked.Decrement(ref _intUpdatingTree);
            }
            if (blnDoRefreshSelection)
                await RefreshSelectedNode(await treModules.DoThreadSafeFuncAsync(x => x.SelectedNode, token).ConfigureAwait(false), token);
        }

        private void cmdOK_Click(object _, EventArgs e)
        {
            AddAgain = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdOKAdd_Click(object _, EventArgs e)
        {
            AddAgain = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdCancel_Click(object _, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void treModules_AfterSelect(object sender, TreeViewEventArgs e)
        {
            await RefreshSelectedNode(e.Node);
        }

        private async Task RefreshSelectedNode(TreeNode objSelectedNode, CancellationToken token = default)
        {
            if (_intUpdatingTree > 0)
                return;
            _strSelectedId = objSelectedNode.Tag?.ToString() ?? string.Empty;
            bool blnSelectable = false;
            if (string.IsNullOrEmpty(_strSelectedId))
            {
                await lblBPLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await lblBP.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await lblStageLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await lblStage.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await lblSource.DoThreadSafeAsync(x => x.Visible = false, token).ConfigureAwait(false);
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdOKAdd.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
            }
            else
            {
                await lblBPLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                await lblStageLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);
                await lblSourceLabel.DoThreadSafeAsync(x => x.Visible = true, token).ConfigureAwait(false);

                if (objSelectedNode.GetNodeCount(false) != 0)
                {
                    //Select any node that have an id node equal to tag
                    //if it contains >selectable>True</selectable>, yes or </selectable>
                    //set button to selectable, otherwise not
                    XPathNavigator xmlModulePath = _xmlLifeModulesDocumentChummerNode.TryGetNodeByNameOrId("modules/module", _strSelectedId);
                    if (xmlModulePath != null)
                    {
                        blnSelectable
                            = xmlModulePath.SelectSingleNodeAndCacheExpression("selectable", token)?.Value != bool.FalseString;
                    }
                    else
                    {
                        xmlModulePath = _xmlLifeModulesDocumentChummerNode.TryGetNodeByNameOrId("modules/module/versions/version", _strSelectedId);
                        if (xmlModulePath != null)
                        {
                            blnSelectable
                                = xmlModulePath.SelectSingleNodeAndCacheExpression("selectable", token)?.Value != bool.FalseString;
                        }
                    }
                }
            }
            
            XmlNode xmlSelectedNodeInfo = Quality.GetNodeOverrideable(_strSelectedId, await _objCharacter.LoadDataAsync("lifemodules.xml", token: token).ConfigureAwait(false));

            if (xmlSelectedNodeInfo != null)
            {
                string strBP = xmlSelectedNodeInfo["karma"]?.InnerTextViaPool(token)
                               ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                string strSource = await (await SourceString.GetSourceStringAsync(xmlSelectedNodeInfo["source"]?.InnerTextViaPool(token),
                                                                            strPage: xmlSelectedNodeInfo["altpage"]?.InnerTextViaPool(token)
                                                                                ?? xmlSelectedNodeInfo["page"]?.InnerTextViaPool(token),
                                                                            objSettings: await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false),
                                                                            token: token).ConfigureAwait(false))
                                               .ToStringAsync(token).ConfigureAwait(false);
                string strStage = xmlSelectedNodeInfo["stage"]?.InnerTextViaPool(token)
                                  ?? await LanguageManager.GetStringAsync("String_Unknown", token: token).ConfigureAwait(false);
                await lblBP.DoThreadSafeAsync(x => x.Text = strBP, token).ConfigureAwait(false);
                await lblSource.DoThreadSafeAsync(x => x.Text = strSource, token).ConfigureAwait(false);
                await lblStage.DoThreadSafeAsync(x => x.Text = strStage, token).ConfigureAwait(false);
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = blnSelectable, token).ConfigureAwait(false);
                await cmdOKAdd.DoThreadSafeAsync(x => x.Enabled = blnSelectable, token).ConfigureAwait(false);
            }
            else
            {
                string strError = await LanguageManager.GetStringAsync("String_Error", token: token).ConfigureAwait(false);
                await lblBP.DoThreadSafeAsync(x => x.Text = strError, token).ConfigureAwait(false);
                await lblStage.DoThreadSafeAsync(x => x.Text = strError, token).ConfigureAwait(false);
                await lblSource.DoThreadSafeAsync(x => x.Text = strError, token).ConfigureAwait(false);
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
                await cmdOKAdd.DoThreadSafeAsync(x => x.Enabled = false, token).ConfigureAwait(false);
            }
        }

        public XmlNode SelectedNode => Quality.GetNodeOverrideable(_strSelectedId, _objCharacter.LoadData("lifemodules.xml"));

        private void treModules_DoubleClick(object sender, EventArgs e)
        {
            if (cmdOK.Enabled)
                cmdOK_Click(sender, e);
        }

        private async void chkLimitList_Click(object _, EventArgs e)
        {
            bool blnLimitListChecked = await chkLimitList.DoThreadSafeFuncAsync(x => x.Checked).ConfigureAwait(false);
            await lblStage.DoThreadSafeAsync(x => x.Visible = blnLimitListChecked).ConfigureAwait(false);
            await cboStage.DoThreadSafeAsync(x => x.Visible = !blnLimitListChecked).ConfigureAwait(false);

            if (blnLimitListChecked)
            {
                if (await cboStage.DoThreadSafeFuncAsync(x => x.DataSource == null).ConfigureAwait(false))
                {
                    using (new FetchSafelyFromSafeObjectPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstStages))
                    {
                        lstStages.Add(new ListItem("0", await LanguageManager.GetStringAsync("String_All").ConfigureAwait(false)));

                        foreach (XPathNavigator xnode in _xmlLifeModulesDocumentChummerNode.SelectAndCacheExpression("stages/stage"))
                        {
                            string strOrder = xnode.SelectSingleNodeAndCacheExpression("order")?.Value;
                            if (!string.IsNullOrEmpty(strOrder))
                            {
                                lstStages.Add(new ListItem(strOrder, xnode.Value));
                            }
                        }

                        //Sort based on integer value of key
                        lstStages.Sort((x, y) =>
                        {
                            if (int.TryParse(x.Value.ToString(), out int xint))
                            {
                                if (int.TryParse(y.Value.ToString(), out int yint))
                                {
                                    return xint - yint;
                                }

                                return 1;
                            }

                            if (int.TryParse(y.Value.ToString(), out int _))
                            {
                                return -1;
                            }

                            return 0;
                        });

                        await cboStage.PopulateWithListItemsAsync(lstStages).ConfigureAwait(false);
                    }
                }

                string strToFind = _intStage.ToString(GlobalSettings.InvariantCultureInfo);
                await cboStage.DoThreadSafeAsync(x =>
                {
                    ListItem selectedItem
                        = ((List<ListItem>) x.DataSource).Find(y => y.Value.ToString() == strToFind);
                    if (!string.IsNullOrEmpty(selectedItem.Name))
                        x.SelectedItem = selectedItem;
                }).ConfigureAwait(false);
            }
            else
            {
                if (Interlocked.Exchange(ref _strWorkStage, _strDefaultStageName) != _strDefaultStageName)
                    await BuildTree().ConfigureAwait(false);
            }
        }

        private async void cboStage_SelectionChangeCommitted(object _, EventArgs e)
        {
            string strSelected = await cboStage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false) ?? string.Empty;
            string strNewWorkStage = strSelected == "0"
                ? string.Empty
                : _xmlLifeModulesDocumentChummerNode
                    .SelectSingleNodeAndCacheExpression("stages/stage[@order = " + strSelected.CleanXPath() + "]")
                    ?.Value ?? string.Empty;
            if (Interlocked.Exchange(ref _strWorkStage, strNewWorkStage) != strNewWorkStage)
                await BuildTree().ConfigureAwait(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            await BuildTree().ConfigureAwait(false);
        }
    }
}
