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

namespace Chummer
{
    public partial class SelectLifeModule : Form
    {
        public bool AddAgain { get; private set; }
        private readonly Character _objCharacter;
        private readonly int _intStage;
        private string _strDefaultStageName;
        private readonly XmlDocument _xmlDocument;
        private string _strSelectedId;
        private Regex _rgxSearchExpression;

        private string _strWorkStage;

        public SelectLifeModule(Character objCharacter, int intStage)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter;
            _intStage = intStage;
            _xmlDocument = _objCharacter.LoadData("lifemodules.xml");
        }

        private async void SelectLifeModule_Load(object sender, EventArgs e)
        {
            string strSelectString = "chummer/stages/stage[@order = " + _intStage.ToString(GlobalSettings.InvariantCultureInfo).CleanXPath() + ']';

            XmlNode xmlStageNode = _xmlDocument.SelectSingleNode(strSelectString);
            if (xmlStageNode != null)
            {
                _strWorkStage = _strDefaultStageName = xmlStageNode.InnerText;

                await BuildTree(await GetSelectString().ConfigureAwait(false)).ConfigureAwait(false);
            }
            else
            {
                _strWorkStage = _strDefaultStageName = "null";
            }
        }

        private async ValueTask BuildTree(string stageString, CancellationToken token = default)
        {
            XmlNodeList matches = _xmlDocument.SelectNodes("chummer/modules/module" + stageString);
            TreeNode[] aobjNodes = await BuildList(matches, token).ConfigureAwait(false);
            await treModules.DoThreadSafeAsync(x =>
            {
                x.Nodes.Clear();
                x.Nodes.AddRange(aobjNodes);
            }, token: token).ConfigureAwait(false);
        }

        private async ValueTask<TreeNode[]> BuildList(XmlNodeList xmlNodes, CancellationToken token = default)
        {
            List<TreeNode> lstTreeNodes = new List<TreeNode>(xmlNodes.Count);
            bool blnLimitList = await chkLimitList.DoThreadSafeFuncAsync(x => x.Checked, token: token).ConfigureAwait(false);
            for (int i = 0; i < xmlNodes.Count; i++)
            {
                XmlNode xmlNode = xmlNodes[i];

                if (!blnLimitList || await xmlNode.CreateNavigator().RequirementsMetAsync(_objCharacter, token: token).ConfigureAwait(false))
                {
                    TreeNode treNode = new TreeNode
                    {
                        Text = xmlNode["name"]?.InnerText ?? string.Empty
                    };
                    if (xmlNode["versions"] != null)
                    {
                        TreeNode[] aobjNodes = await BuildList(
                            xmlNode.SelectNodes("versions/version["
                                                + await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false)
                                                + "or not(source)]"), token).ConfigureAwait(false);
                        treNode.Nodes.AddRange(aobjNodes);
                    }

                    treNode.Tag = xmlNode["id"]?.InnerText;
                    if (_rgxSearchExpression != null)
                    {
                        if (_rgxSearchExpression.IsMatch(treNode.Text))
                        {
                            lstTreeNodes.Add(treNode);
                        }
                        else if (treNode.Nodes.Count != 0)
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
            bool blnSelectAble;
            if (e.Node.Nodes.Count == 0)
            {
                blnSelectAble = true;
            }
            else
            {
                //Select any node that have an id node equal to tag
                string selectString = "//*[id = " + e.Node.Tag.ToString().CleanXPath() + "]/selectable";
                XmlNode node = _xmlDocument.SelectSingleNode(selectString);
                //if it contains >selectable>True</selectable>, yes or </selectable>
                //set button to selectable, otherwise not
                blnSelectAble = (node != null && (node.InnerText == bool.TrueString || node.OuterXml.EndsWith("/>", StringComparison.Ordinal)));
            }

            _strSelectedId = (string)e.Node.Tag;
            XmlNode xmlSelectedNodeInfo = Quality.GetNodeOverrideable(_strSelectedId, await _objCharacter.LoadDataAsync("lifemodules.xml").ConfigureAwait(false));

            if (xmlSelectedNodeInfo != null)
            {
                await cmdOK.DoThreadSafeAsync(x => x.Enabled = blnSelectAble).ConfigureAwait(false);
                await cmdOKAdd.DoThreadSafeAsync(x => x.Enabled = blnSelectAble).ConfigureAwait(false);

                await lblBP.DoThreadSafeAsync(x => x.Text = xmlSelectedNodeInfo["karma"]?.InnerText ?? string.Empty).ConfigureAwait(false);
                string strSpace = await LanguageManager.GetStringAsync("String_Space").ConfigureAwait(false);
                await lblSource.DoThreadSafeAsync(x => x.Text = xmlSelectedNodeInfo["source"]?.InnerText
                                                                ?? string.Empty + strSpace
                                                                + xmlSelectedNodeInfo["page"]?.InnerText).ConfigureAwait(false);
                await lblStage.DoThreadSafeAsync(x => x.Text = xmlSelectedNodeInfo["stage"]?.InnerText ?? string.Empty).ConfigureAwait(false);
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
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstStages))
                    {
                        lstStages.Add(new ListItem("0", await LanguageManager.GetStringAsync("String_All").ConfigureAwait(false)));

                        using (XmlNodeList xmlNodes = _xmlDocument.SelectNodes("/chummer/stages/stage"))
                        {
                            if (xmlNodes?.Count > 0)
                            {
                                foreach (XmlNode xnode in xmlNodes)
                                {
                                    string strOrder = xnode.Attributes?["order"]?.Value;
                                    if (!string.IsNullOrEmpty(strOrder))
                                    {
                                        lstStages.Add(new ListItem(strOrder, xnode.InnerText));
                                    }
                                }
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
            if (strSelected == "0")
            {
                _strWorkStage = string.Empty;
            }
            else
            {
                _strWorkStage = _xmlDocument.SelectSingleNode("chummer/stages/stage[@order = " + strSelected.CleanXPath() + ']')?.InnerText ?? string.Empty;
            }
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

        private async ValueTask<string> GetSelectString(CancellationToken token = default)
        {
            string strReturn = "[(" + await _objCharacter.Settings.BookXPathAsync(token: token).ConfigureAwait(false);

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
