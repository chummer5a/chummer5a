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
using System.Text.RegularExpressions;
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
        private Regex _rgxSearchExpression;

        public Character CharacterObject => _objCharacter;

        private string _strWorkStage;

        public SelectLifeModule(Character objCharacter, int intStage)
        {
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _intStage = intStage;
            _xmlLifeModulesDocumentChummerNode
                = _objCharacter.LoadDataXPath("lifemodules.xml").SelectSingleNode("/chummer");
        }

        private async void SelectLifeModule_Load(object sender, EventArgs e)
        {
            string strSelectString = "stages/stage[@order = " + _intStage.ToString(GlobalSettings.InvariantCultureInfo).CleanXPath() + ']';

            XPathNavigator xmlStageNode = _xmlLifeModulesDocumentChummerNode.SelectSingleNodeAndCacheExpression(strSelectString);
            if (xmlStageNode != null)
            {
                _strWorkStage = _strDefaultStageName = xmlStageNode.Value;

                await BuildTree(await GetSelectString().ConfigureAwait(false)).ConfigureAwait(false);
            }
            else
            {
                _strWorkStage = _strDefaultStageName = "null";
            }
        }

        private async Task BuildTree(string stageString, CancellationToken token = default)
        {
            TreeNode[] aobjNodes = await BuildList(_xmlLifeModulesDocumentChummerNode.Select("modules/module" + stageString), token).ConfigureAwait(false);
            await treModules.DoThreadSafeAsync(x =>
            {
                x.Nodes.Clear();
                x.Nodes.AddRange(aobjNodes);
            }, token: token).ConfigureAwait(false);
        }

        private async Task<TreeNode[]> BuildList(XPathNodeIterator lstXmlNodes, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            List<TreeNode> lstTreeNodes = new List<TreeNode>(5);
            bool blnLimitList = await chkLimitList.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            AsyncLazy<string> strBookPath = new AsyncLazy<string>(async () => await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false), Utils.JoinableTaskFactory);
            foreach (XPathNavigator xmlNode in lstXmlNodes)
            {
                token.ThrowIfCancellationRequested();
                if (!blnLimitList || await xmlNode.RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                {
                    TreeNode treNode = new TreeNode
                    {
                        Text = xmlNode.SelectSingleNodeAndCacheExpression("translate", token)?.Value
                               ?? xmlNode.SelectSingleNodeAndCacheExpression("name", token)?.Value ?? string.Empty
                    };
                    XPathNavigator xmlVersionsNode = xmlNode
                                                           .SelectSingleNodeAndCacheExpression("versions", token);
                    if (xmlVersionsNode != null)
                    {
                        TreeNode[] aobjNodes = await BuildList(
                            xmlVersionsNode.Select(
                                "version[(" + await strBookPath.GetValueAsync(token).ConfigureAwait(false)
                                            + ") or not(source)]"), token).ConfigureAwait(false);
                        treNode.Nodes.AddRange(aobjNodes);
                    }

                    treNode.Tag = xmlNode.SelectSingleNodeAndCacheExpression("id", token)?.Value;
                    token.ThrowIfCancellationRequested();
                    if (_rgxSearchExpression != null)
                    {
                        if (_rgxSearchExpression.IsMatch(treNode.Text) || treNode.Nodes.Count != 0)
                        {
                            lstTreeNodes.Add(treNode);
                        }
                    }
                    else
                    {
                        lstTreeNodes.Add(treNode);
                    }
                }
            }
            token.ThrowIfCancellationRequested();

            return lstTreeNodes.ToArray();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            AddAgain = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void treModules_AfterSelect(object sender, TreeViewEventArgs e)
        {
            bool blnSelectable = true;
            if (e.Node.Nodes.Count != 0)
            {
                //Select any node that have an id node equal to tag
                //if it contains >selectable>True</selectable>, yes or </selectable>
                //set button to selectable, otherwise not
                XPathNavigator xmlModulePath = _xmlLifeModulesDocumentChummerNode.TryGetNodeByNameOrId("//*", e.Node.Tag.ToString());
                if (xmlModulePath != null)
                {
                    blnSelectable
                        = xmlModulePath.SelectSingleNodeAndCacheExpression("selectable")?.Value != bool.FalseString;
                }
            }

            _strSelectedId = (string)e.Node.Tag;
            XmlNode xmlSelectedNodeInfo = Quality.GetNodeOverrideable(_strSelectedId, await _objCharacter.LoadDataAsync("lifemodules.xml").ConfigureAwait(false));

            if (xmlSelectedNodeInfo != null)
            {
                string strBP = xmlSelectedNodeInfo["karma"]?.InnerText
                               ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                string strSource = await (await SourceString.GetSourceStringAsync(xmlSelectedNodeInfo["source"]?.InnerText,
                                                                            strPage: xmlSelectedNodeInfo["altpage"]?.InnerText
                                                                            ?? xmlSelectedNodeInfo["page"]?.InnerText,
                                                                            objSettings: await _objCharacter.GetSettingsAsync().ConfigureAwait(false)).ConfigureAwait(false))
                                               .ToStringAsync().ConfigureAwait(false);
                string strStage = xmlSelectedNodeInfo["stage"]?.InnerText
                                  ?? await LanguageManager.GetStringAsync("String_Unknown").ConfigureAwait(false);
                await lblBP.DoThreadSafeAsync(x => x.Text = strBP).ConfigureAwait(false);
                await lblSource.DoThreadSafeAsync(x => x.Text = strSource).ConfigureAwait(false);
                await lblStage.DoThreadSafeAsync(x => x.Text = strStage).ConfigureAwait(false);
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = blnSelectable).ConfigureAwait(false);
                await cmdOKAdd.DoThreadSafeAsync(x => x.Enabled = blnSelectable).ConfigureAwait(false);
            }
            else
            {
                string strError = await LanguageManager.GetStringAsync("String_Error").ConfigureAwait(false);
                await lblBP.DoThreadSafeAsync(x => x.Text = strError).ConfigureAwait(false);
                await lblStage.DoThreadSafeAsync(x => x.Text = strError).ConfigureAwait(false);
                await lblSource.DoThreadSafeAsync(x => x.Text = strError).ConfigureAwait(false);
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false);
                await cmdOKAdd.DoThreadSafeAsync(x => x.Enabled = false).ConfigureAwait(false);
            }
        }

        public XmlNode SelectedNode => Quality.GetNodeOverrideable(_strSelectedId, _objCharacter.LoadData("lifemodules.xml"));

        private void treModules_DoubleClick(object sender, EventArgs e)
        {
            if (cmdOK.Enabled)
            {
                AddAgain = false;
                cmdOK_Click(sender, e);
            }
        }

        private async void chkLimitList_Click(object sender, EventArgs e)
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

                        foreach (XPathNavigator xnode in _xmlLifeModulesDocumentChummerNode.Select("stages/stage"))
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
                _strWorkStage = _strDefaultStageName;
                await BuildTree(await GetSelectString().ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        private async void cboStage_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string strSelected = await cboStage.DoThreadSafeFuncAsync(x => x.SelectedValue?.ToString()).ConfigureAwait(false) ?? string.Empty;
            _strWorkStage = strSelected == "0"
                ? string.Empty
                : _xmlLifeModulesDocumentChummerNode
                    .SelectSingleNodeAndCacheExpression("stages/stage[@order = " + strSelected.CleanXPath() + ']')
                    ?.Value ?? string.Empty;
            await BuildTree(await GetSelectString().ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string strText = await txtSearch.DoThreadSafeFuncAsync(x => x.Text).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(strText))
            {
                _rgxSearchExpression = null;
            }
            else
            {
                try
                {
                    bool _ = Regex.IsMatch("Test for properly formatted Regular Expression pattern.",
                                           strText);
                }
                catch (ArgumentException)
                {
                    // Test to make sure RegEx pattern is properly formatted
                    return;
                }
                _rgxSearchExpression = new Regex(strText, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            }

            await BuildTree(await GetSelectString().ConfigureAwait(false)).ConfigureAwait(false);
        }

        private async Task<string> GetSelectString(CancellationToken token = default)
        {
            string strReturn = "[(" + await (await _objCharacter.GetSettingsAsync(token).ConfigureAwait(false)).BookXPathAsync(token: token).ConfigureAwait(false);

            //chummer/modules/module//name[contains(., "C")]/..["" = string.Empty]
            // /chummer/modules/module//name[contains(., "can")]/..[id]

            //if (!string.IsNullOrWhiteSpace(_strSearch))
            //{
            //    strReturn = string.Format("//name[contains(., \"{0}\")]..[", _strSearch);
            //    before = true;
            //}
            if (!string.IsNullOrWhiteSpace(_strWorkStage))
            {
                strReturn += ") and (stage = " + _strWorkStage.CleanXPath();
            }

            return strReturn + ")]";
        }
    }
}
