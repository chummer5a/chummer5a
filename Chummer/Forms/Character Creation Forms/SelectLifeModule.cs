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
        private Regex _rgxSearchRegex;

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

        private void SelectLifeModule_Load(object sender, EventArgs e)
        {
            string strSelectString = "chummer/stages/stage[@order = " + _intStage.ToString(GlobalSettings.InvariantCultureInfo).CleanXPath() + "]";

            XmlNode xmlStageNode = _xmlDocument.SelectSingleNode(strSelectString);
            if (xmlStageNode != null)
            {
                _strWorkStage = _strDefaultStageName = xmlStageNode.InnerText;

                BuildTree(GetSelectString());
            }
            else
            {
                _strWorkStage = _strDefaultStageName = "null";
            }
        }

        private void BuildTree(string stageString)
        {
            XmlNodeList matches = _xmlDocument.SelectNodes("chummer/modules/module" + stageString);
            treModules.Nodes.Clear();
            treModules.Nodes.AddRange(
                BuildList(matches));
        }

        private TreeNode[] BuildList(XmlNodeList xmlNodes)
        {
            List<TreeNode> lstTreeNodes = new List<TreeNode>(xmlNodes.Count);
            for (int i = 0; i < xmlNodes.Count; i++)
            {
                XmlNode xmlNode = xmlNodes[i];

                if (!chkLimitList.Checked || xmlNode.CreateNavigator().RequirementsMet(_objCharacter))
                {
                    TreeNode treNode = new TreeNode
                    {
                        Text = xmlNode["name"]?.InnerText ?? string.Empty
                    };
                    if (xmlNode["versions"] != null)
                    {
                        treNode.Nodes.AddRange(
                            BuildList(xmlNode.SelectNodes("versions/version[" + _objCharacter.Settings.BookXPath() + "or not(source)]")));
                    }

                    treNode.Tag = xmlNode["id"]?.InnerText;
                    if (_rgxSearchRegex != null)
                    {
                        if (_rgxSearchRegex.IsMatch(treNode.Text))
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
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            AddAgain = true;
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void treModules_AfterSelect(object sender, TreeViewEventArgs e)
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
            XmlNode xmlSelectedNodeInfo = Quality.GetNodeOverrideable(_strSelectedId, _objCharacter.LoadData("lifemodules.xml"));

            if (xmlSelectedNodeInfo != null)
            {
                cmdOK.Enabled = blnSelectAble;
                cmdOKAdd.Enabled = blnSelectAble;

                lblBP.Text = xmlSelectedNodeInfo["karma"]?.InnerText ?? string.Empty;
                lblSource.Text = xmlSelectedNodeInfo["source"]?.InnerText ?? string.Empty + LanguageManager.GetString("String_Space") + xmlSelectedNodeInfo["page"]?.InnerText;
                lblStage.Text = xmlSelectedNodeInfo["stage"]?.InnerText ?? string.Empty;
            }
            else
            {
                lblBP.Text = LanguageManager.GetString("String_Error");
                lblStage.Text = LanguageManager.GetString("String_Error");
                lblSource.Text = LanguageManager.GetString("String_Error");

                cmdOK.Enabled = false;
                cmdOKAdd.Enabled = false;
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

        private void chkLimitList_Click(object sender, EventArgs e)
        {
            cboStage.BeginUpdate();
            lblStage.Visible = chkLimitList.Checked;
            cboStage.Visible = !chkLimitList.Checked;

            if (cboStage.Visible)
            {
                if (cboStage.DataSource == null)
                {
                    using (new FetchSafelyFromPool<List<ListItem>>(Utils.ListItemListPool, out List<ListItem> lstStages))
                    {
                        lstStages.Add(new ListItem("0", LanguageManager.GetString("String_All")));

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

                        cboStage.PopulateWithListItems(lstStages);
                    }
                }

                string strToFind = _intStage.ToString(GlobalSettings.InvariantCultureInfo);
                ListItem selectedItem = ((List<ListItem>)cboStage.DataSource).Find(x => x.Value.ToString() == strToFind);
                if (!string.IsNullOrEmpty(selectedItem.Name))
                    cboStage.SelectedItem = selectedItem;
            }
            else
            {
                _strWorkStage = _strDefaultStageName;
                BuildTree(GetSelectString());
            }
            cboStage.EndUpdate();
        }

        private void cboStage_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string strSelected = (string)cboStage.SelectedValue;
            if (strSelected == "0")
            {
                _strWorkStage = string.Empty;
            }
            else
            {
                _strWorkStage = _xmlDocument.SelectSingleNode("chummer/stages/stage[@order = " + strSelected.CleanXPath() + "]")?.InnerText ?? string.Empty;
            }
            BuildTree(GetSelectString());
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                _rgxSearchRegex = null;
            }
            else
            {
                try
                {
                    _rgxSearchRegex = new Regex(txtSearch.Text, RegexOptions.IgnoreCase);
                }
                catch (ArgumentException)
                {
                    //No other way to check for a valid regex that i know of
                }
            }

            BuildTree(GetSelectString());
        }

        private string GetSelectString()
        {
            string strReturn = "[(" + _objCharacter.Settings.BookXPath();

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
            strReturn += ")]";

            return strReturn;
        }
    }
}
